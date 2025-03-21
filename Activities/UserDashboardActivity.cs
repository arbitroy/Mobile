using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mobile.Models;
using Mobile.Services;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using System.Linq;
using Android.Views;

namespace Mobile.Activities
{
    [Activity(Label = "Dashboard")]
    public class UserDashboardActivity : BaseAuthenticatedActivity
    {
        private TextView _usernameTextView;
        private TextView _totalQuizzesTextView;
        private TextView _averageScoreTextView;
        private TextView _bestScoreTextView;
        private RecyclerView _recentAttemptsRecyclerView;
        private RecyclerView _recommendedQuizzesRecyclerView;
        private Button _browseQuizzesButton;
        private Button _historyButton;
        private Button _profileButton;
        private Button _adminDashboardButton;
        private ProgressBar _loadingProgressBar;
        private TextView _emptyAttemptsTextView;
        private TextView _emptyRecommendedTextView;

        protected override void OnRolesLoaded()
        {
            // Show/hide admin dashboard button based on role
            RunOnUiThread(() => {
                if (_adminDashboardButton != null)
                {
                    _adminDashboardButton.Visibility = IsAdmin ?
                        ViewStates.Visible :
                        ViewStates.Gone;
                }
            });
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_user_dashboard);

            // Initialize UI elements
            _usernameTextView = FindViewById<TextView>(Resource.Id.userNameTextView);
            _totalQuizzesTextView = FindViewById<TextView>(Resource.Id.totalQuizzesTextView);
            _averageScoreTextView = FindViewById<TextView>(Resource.Id.averageScoreTextView);
            _bestScoreTextView = FindViewById<TextView>(Resource.Id.bestScoreTextView);
            _recentAttemptsRecyclerView = FindViewById<RecyclerView>(Resource.Id.recentAttemptsRecyclerView);
            _recommendedQuizzesRecyclerView = FindViewById<RecyclerView>(Resource.Id.recommendedQuizzesRecyclerView);
            _browseQuizzesButton = FindViewById<Button>(Resource.Id.browseQuizzesButton);
            _historyButton = FindViewById<Button>(Resource.Id.historyButton);
            _profileButton = FindViewById<Button>(Resource.Id.profileButton);
            _adminDashboardButton = FindViewById<Button>(Resource.Id.adminDashboardButton);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
            _emptyAttemptsTextView = FindViewById<TextView>(Resource.Id.emptyAttemptsTextView);
            _emptyRecommendedTextView = FindViewById<TextView>(Resource.Id.emptyRecommendedTextView);

            // Set up RecyclerViews
            _recentAttemptsRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recommendedQuizzesRecyclerView.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));

            // Set username from preferences if available
            string username = TokenManager.GetUsername(this) ?? "Quiz User";
            _usernameTextView.Text = username;

            // Load dashboard data
            LoadDashboardAsync();

            // Set up event handlers
            _browseQuizzesButton.Click += OnBrowseQuizzesButtonClick;
            _historyButton.Click += OnHistoryButtonClick;
            _profileButton.Click += OnProfileButtonClick;
            _adminDashboardButton.Click += OnAdminDashboardButtonClick;
        }

        private async void LoadDashboardAsync()
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = ViewStates.Visible;

                try
                {
                    // Get dashboard data from API
                    var dashboard = await ApiService.GetUserDashboardAsync();

                    // Update UI with dashboard data
                    _totalQuizzesTextView.Text = dashboard.TotalQuizzesTaken.ToString();
                    _averageScoreTextView.Text = $"{dashboard.AverageScore:F1}%";
                    _bestScoreTextView.Text = $"{dashboard.BestScore:F0}%";

                    // Set up recent attempts RecyclerView
                    if (dashboard.RecentAttempts.Count > 0)
                    {
                        var attemptsAdapter = new RecentAttemptsAdapter(this, dashboard.RecentAttempts);
                        _recentAttemptsRecyclerView.SetAdapter(attemptsAdapter);

                        // Hide empty message
                        _emptyAttemptsTextView.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        // Show empty message
                        _emptyAttemptsTextView.Visibility = ViewStates.Visible;
                    }

                    // Set up recommended quizzes RecyclerView
                    if (dashboard.RecommendedQuizzes.Count > 0)
                    {
                        var quizzesAdapter = new RecommendedQuizzesAdapter(this, dashboard.RecommendedQuizzes);
                        _recommendedQuizzesRecyclerView.SetAdapter(quizzesAdapter);

                        // Hide empty message
                        _emptyRecommendedTextView.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        // Show empty message
                        _emptyRecommendedTextView.Visibility = ViewStates.Visible;
                    }
                }
                finally
                {
                    // Hide loading indicator
                    _loadingProgressBar.Visibility = ViewStates.Gone;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // BaseAuthenticatedActivity will handle this
                Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                // For other errors, just show a message but stay on the page
                Toast.MakeText(this, $"Error loading dashboard: {ex.Message}", ToastLength.Long).Show();

                // Initialize with empty values
                _totalQuizzesTextView.Text = "0";
                _averageScoreTextView.Text = "0%";
                _bestScoreTextView.Text = "0%";
                _emptyAttemptsTextView.Visibility = ViewStates.Visible;
                _emptyRecommendedTextView.Visibility = ViewStates.Visible;
            }
        }

        private void OnBrowseQuizzesButtonClick(object sender, EventArgs e)
        {
            // Navigate to quiz list activity
            var intent = new Intent(this, typeof(QuizListActivity));
            StartActivity(intent);
        }

        private void OnHistoryButtonClick(object sender, EventArgs e)
        {
            // Navigate to quiz history activity
            var intent = new Intent(this, typeof(QuizHistoryActivity));
            StartActivity(intent);
        }

        private void OnProfileButtonClick(object sender, EventArgs e)
        {
            // Navigate to user profile activity
            var intent = new Intent(this, typeof(UserProfileActivity));
            StartActivity(intent);
        }

        private void OnAdminDashboardButtonClick(object sender, EventArgs e)
        {
            // Navigate to admin dashboard
            var intent = new Intent(this, typeof(AdminDashboardActivity));
            StartActivity(intent);
        }

        // RecyclerView adapter for recent quiz attempts
        public class RecentAttemptsAdapter : RecyclerView.Adapter
        {
            private readonly Activity _activity;
            private readonly List<QuizAttemptSummary> _attempts;

            public RecentAttemptsAdapter(Activity activity, List<QuizAttemptSummary> attempts)
            {
                _activity = activity;
                _attempts = attempts;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_attempt_item, parent, false);
                return new AttemptViewHolder(itemView);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                AttemptViewHolder viewHolder = holder as AttemptViewHolder;
                QuizAttemptSummary attempt = _attempts[position];

                // Set quiz title
                viewHolder.QuizTitleTextView.Text = attempt.QuizTitle;

                // Set date
                viewHolder.DateTextView.Text = attempt.EndTime.ToString("MMM dd, yyyy HH:mm");

                // Set score
                viewHolder.ScoreTextView.Text = $"{attempt.Score}%";

                // Set score color based on value
                if (attempt.Score >= 80)
                {
                    viewHolder.ScoreTextView.SetTextColor(Android.Graphics.Color.ParseColor("#4CAF50")); // Green
                }
                else if (attempt.Score >= 60)
                {
                    viewHolder.ScoreTextView.SetTextColor(Android.Graphics.Color.ParseColor("#FF9800")); // Orange
                }
                else
                {
                    viewHolder.ScoreTextView.SetTextColor(Android.Graphics.Color.ParseColor("#F44336")); // Red
                }

                // Set up event handler for item click
                viewHolder.ItemView.Click += (sender, e) => {
                    // Navigate to quiz result activity
                    var intent = new Intent(_activity, typeof(QuizResultActivity));
                    intent.PutExtra("AttemptId", attempt.AttemptId);
                    _activity.StartActivity(intent);
                };
            }

            public override int ItemCount => _attempts.Count;

            // ViewHolder for attempt items
            public class AttemptViewHolder : RecyclerView.ViewHolder
            {
                public TextView QuizTitleTextView { get; }
                public TextView DateTextView { get; }
                public TextView ScoreTextView { get; }

                public AttemptViewHolder(View itemView) : base(itemView)
                {
                    QuizTitleTextView = itemView.FindViewById<TextView>(Resource.Id.quizTitleTextView);
                    DateTextView = itemView.FindViewById<TextView>(Resource.Id.dateTextView);
                    ScoreTextView = itemView.FindViewById<TextView>(Resource.Id.scoreTextView);
                }
            }
        }

        // RecyclerView adapter for recommended quizzes
        public class RecommendedQuizzesAdapter : RecyclerView.Adapter
        {
            private readonly Activity _activity;
            private readonly List<Quiz> _quizzes;

            public RecommendedQuizzesAdapter(Activity activity, List<Quiz> quizzes)
            {
                _activity = activity;
                _quizzes = quizzes;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_recommended_quiz_item, parent, false);
                return new QuizViewHolder(itemView);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                QuizViewHolder viewHolder = holder as QuizViewHolder;
                Quiz quiz = _quizzes[position];

                // Set quiz title
                viewHolder.TitleTextView.Text = quiz.Title;

                // Set quiz description (truncate if too long)
                string description = quiz.Description;
                if (description.Length > 100)
                {
                    description = description.Substring(0, 97) + "...";
                }
                viewHolder.DescriptionTextView.Text = description;

                // Set question count
                viewHolder.QuestionCountTextView.Text = $"{quiz.QuestionCount} questions";

                // Set time limit
                viewHolder.TimeLimitTextView.Text = $"{quiz.TimeLimit} min";

                // Set up event handler for the "View Quiz" button
                viewHolder.StartQuizButton.Click += (sender, e) => {
                    // Navigate to quiz detail activity
                    var intent = new Intent(_activity, typeof(QuizDetailActivity));
                    intent.PutExtra("QuizId", quiz.Id);
                    intent.PutExtra("QuizTitle", quiz.Title);
                    _activity.StartActivity(intent);
                };
            }

            public override int ItemCount => _quizzes.Count;

            // ViewHolder for quiz items
            public class QuizViewHolder : RecyclerView.ViewHolder
            {
                public TextView TitleTextView { get; }
                public TextView DescriptionTextView { get; }
                public TextView QuestionCountTextView { get; }
                public TextView TimeLimitTextView { get; }
                public Button StartQuizButton { get; }

                public QuizViewHolder(View itemView) : base(itemView)
                {
                    TitleTextView = itemView.FindViewById<TextView>(Resource.Id.titleTextView);
                    DescriptionTextView = itemView.FindViewById<TextView>(Resource.Id.descriptionTextView);
                    QuestionCountTextView = itemView.FindViewById<TextView>(Resource.Id.questionCountTextView);
                    TimeLimitTextView = itemView.FindViewById<TextView>(Resource.Id.timeLimitTextView);
                    StartQuizButton = itemView.FindViewById<Button>(Resource.Id.startQuizButton);
                }
            }
        }
    }
}