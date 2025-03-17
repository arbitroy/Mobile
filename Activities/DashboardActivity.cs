using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Threading.Tasks;
using Mobile.Models;
using Mobile.Services;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using Android.Views;

namespace Mobile.Activities
{
    [Activity(Label = "Dashboard")]
    public class DashboardActivity : Activity
    {
        private TextView _userNameTextView;
        private TextView _quizzesTakenTextView;
        private TextView _averageScoreTextView;
        private TextView _bestScoreTextView;
        private RecyclerView _recentAttemptsRecyclerView;
        private Button _browseQuizzesButton;
        private Button _profileButton;
        private ApiService _apiService;
        private ProgressBar _loadingProgressBar;
        private TextView _emptyAttemptsTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_dashboard);

            // Initialize UI elements
            _userNameTextView = FindViewById<TextView>(Resource.Id.userNameTextView);
            _quizzesTakenTextView = FindViewById<TextView>(Resource.Id.quizzesTakenTextView);
            _averageScoreTextView = FindViewById<TextView>(Resource.Id.averageScoreTextView);
            _bestScoreTextView = FindViewById<TextView>(Resource.Id.bestScoreTextView);
            _recentAttemptsRecyclerView = FindViewById<RecyclerView>(Resource.Id.recentAttemptsRecyclerView);
            _browseQuizzesButton = FindViewById<Button>(Resource.Id.browseQuizzesButton);
            _profileButton = FindViewById<Button>(Resource.Id.profileButton);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
            _emptyAttemptsTextView = FindViewById<TextView>(Resource.Id.emptyAttemptsTextView);

            // Set up RecyclerView
            _recentAttemptsRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            // Initialize service with context to access token
            _apiService = new ApiService(this);

            // Set username from preferences if available
            string username = TokenManager.GetUsername(this) ?? Intent.GetStringExtra("UserName") ?? "Quiz App User";
            _userNameTextView.Text = username;

            // Load user stats
            LoadUserStatsAsync();

            // Set up event handlers
            _browseQuizzesButton.Click += OnBrowseQuizzesButtonClick;
        }

        private async void LoadUserStatsAsync()
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = ViewStates.Visible;

                // Get user stats from API
                var userStats = await _apiService.GetUserStatsAsync();

                // Update UI with user stats
                _quizzesTakenTextView.Text = userStats.TotalQuizzesTaken.ToString();
                _averageScoreTextView.Text = $"{userStats.AverageScore:F1}%";
                _bestScoreTextView.Text = $"{userStats.BestScore:F0}%";

                // Set up recent attempts RecyclerView
                if (userStats.RecentAttempts.Count > 0)
                {
                    var adapter = new RecentAttemptsAdapter(this, userStats.RecentAttempts);
                    _recentAttemptsRecyclerView.SetAdapter(adapter);

                    // Hide empty message
                    _emptyAttemptsTextView.Visibility = ViewStates.Gone;
                }
                else
                {
                    // Show empty message
                    _emptyAttemptsTextView.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception ex)
            {
                // Check if this is an authentication error
                if (ex.Message.Contains("must be logged in") || ex.Message.Contains("Unauthorized"))
                {
                    // Clear invalid token and redirect to login
                    TokenManager.ClearToken(this);
                    Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();

                    var intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    Finish();
                    return;
                }

                // For other errors, just show a message but stay on the page
                Toast.MakeText(this, $"Error loading stats: {ex.Message}", ToastLength.Long).Show();

                // Initialize with empty values
                _quizzesTakenTextView.Text = "0";
                _averageScoreTextView.Text = "0%";
                _bestScoreTextView.Text = "0%";
                _emptyAttemptsTextView.Visibility = ViewStates.Visible;
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = ViewStates.Gone;
            }
        }

        private void OnBrowseQuizzesButtonClick(object sender, EventArgs e)
        {
            // Navigate to quiz list activity
            var intent = new Intent(this, typeof(QuizListActivity));
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
    }
}