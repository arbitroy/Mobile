using Android.App;
using Android.OS;
using Android.Widget;
using System;
using Mobile.Services;
using Mobile.Models;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using System.Linq;

namespace Mobile.Activities
{
    [Activity(Label = "Admin Dashboard")]
    public class AdminDashboardActivity : BaseAuthenticatedActivity
    {
        private TextView _adminUsernameTextView;
        private TextView _userCountTextView;
        private TextView _quizCountTextView;
        private TextView _attemptCountTextView;
        private TextView _avgScoreTextView;
        private RecyclerView _recentActivityRecyclerView;
        private RecyclerView _popularQuizzesRecyclerView;
        private Button _manageQuizzesButton;
        private Button _createQuizButton;
        private Button _manageUsersButton;
        private Button _createUserButton;
        private Button _viewMyProfileButton;
        private Button _logoutButton;

        // Stats
        private AdminStats _adminStats;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "admin_dashboard" layout resource
            SetContentView(Resource.Layout.activity_admin_dashboard);

            // Initialize UI elements
            _adminUsernameTextView = FindViewById<TextView>(Resource.Id.adminUsernameTextView);
            _userCountTextView = FindViewById<TextView>(Resource.Id.userCountTextView);
            _quizCountTextView = FindViewById<TextView>(Resource.Id.quizCountTextView);
            _attemptCountTextView = FindViewById<TextView>(Resource.Id.attemptCountTextView);
            _avgScoreTextView = FindViewById<TextView>(Resource.Id.avgScoreTextView);
            _recentActivityRecyclerView = FindViewById<RecyclerView>(Resource.Id.recentActivityRecyclerView);
            _popularQuizzesRecyclerView = FindViewById<RecyclerView>(Resource.Id.popularQuizzesRecyclerView);
            _manageQuizzesButton = FindViewById<Button>(Resource.Id.manageQuizzesButton);
            _createQuizButton = FindViewById<Button>(Resource.Id.createQuizButton);
            _manageUsersButton = FindViewById<Button>(Resource.Id.manageUsersButton);
            _createUserButton = FindViewById<Button>(Resource.Id.createUserButton);
            _viewMyProfileButton = FindViewById<Button>(Resource.Id.viewMyProfileButton);
            _logoutButton = FindViewById<Button>(Resource.Id.logoutButton);

            // Set up RecyclerViews
            _recentActivityRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _popularQuizzesRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            // Set admin username
            string username = TokenManager.GetUsername(this) ?? "Admin";
            _adminUsernameTextView.Text = username;

            // Set up event handlers
            _manageQuizzesButton.Click += OnManageQuizzesButtonClick;
            _createQuizButton.Click += OnCreateQuizButtonClick;
            _manageUsersButton.Click += OnManageUsersButtonClick;
            _createUserButton.Click += OnCreateUserButtonClick;
            _viewMyProfileButton.Click += OnViewMyProfileButtonClick;
            _logoutButton.Click += OnLogoutButtonClick;

            // Load admin stats
            LoadAdminStatsAsync();
        }

        protected override void OnRolesLoaded()
        {
            // Ensure user has admin access
            if (!IsAdmin)
            {
                Toast.MakeText(this, "You need administrator privileges to access this dashboard", ToastLength.Long).Show();

                // Redirect to user dashboard
                var intent = new Intent(this, typeof(DashboardActivity));
                StartActivity(intent);
                Finish();
            }
        }

        private async void LoadAdminStatsAsync()
        {
            try
            {
                // Show loading indicator
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Loading statistics...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    // Get admin stats from API
                    _adminStats = await ApiService.GetAdminStatsAsync();

                    // Update UI with stats
                    _userCountTextView.Text = _adminStats.UserCount.ToString();
                    _quizCountTextView.Text = _adminStats.QuizCount.ToString();
                    _attemptCountTextView.Text = _adminStats.AttemptCount.ToString();
                    _avgScoreTextView.Text = $"{_adminStats.AverageScore:F1}%";

                    // Set up recent activity RecyclerView
                    if (_adminStats.RecentAttempts.Count > 0)
                    {
                        var recentActivityAdapter = new RecentActivityAdapter(this, _adminStats.RecentAttempts);
                        _recentActivityRecyclerView.SetAdapter(recentActivityAdapter);
                    }

                    // Set up popular quizzes RecyclerView
                    if (_adminStats.PopularQuizzes.Count > 0)
                    {
                        var popularQuizzesAdapter = new PopularQuizzesAdapter(this, _adminStats.PopularQuizzes);
                        _popularQuizzesRecyclerView.SetAdapter(popularQuizzesAdapter);
                    }
                }
                finally
                {
                    // Hide loading indicator
                    if (progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle authentication errors
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load statistics: {ex.Message}", ToastLength.Long).Show();

                // Set default values for stats
                _userCountTextView.Text = "-";
                _quizCountTextView.Text = "-";
                _attemptCountTextView.Text = "-";
                _avgScoreTextView.Text = "-";
            }
        }

        private void OnManageQuizzesButtonClick(object sender, EventArgs e)
        {
            // Navigate to AdminQuizListActivity
            var intent = new Intent(this, typeof(AdminQuizListActivity));
            StartActivity(intent);
        }

        private void OnCreateQuizButtonClick(object sender, EventArgs e)
        {
            // Navigate to AdminCreateQuizActivity
            var intent = new Intent(this, typeof(AdminCreateQuizActivity));
            StartActivity(intent);
        }

        private void OnManageUsersButtonClick(object sender, EventArgs e)
        {
            // Navigate to AdminUserListActivity
            var intent = new Intent(this, typeof(AdminUserListActivity));
            StartActivity(intent);
        }

        private void OnCreateUserButtonClick(object sender, EventArgs e)
        {
            // Navigate to AdminCreateUserActivity
            var intent = new Intent(this, typeof(AdminCreateUserActivity));
            StartActivity(intent);
        }

        private void OnViewMyProfileButtonClick(object sender, EventArgs e)
        {
            // Navigate to user profile activity
            var intent = new Intent(this, typeof(UserProfileActivity));
            StartActivity(intent);
        }

        private void OnLogoutButtonClick(object sender, EventArgs e)
        {
            // Show confirmation dialog
            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle("Logout");
            alertDialog.SetMessage("Are you sure you want to logout?");
            alertDialog.SetPositiveButton("Yes", (senderAlert, args) => {
                // Clear token and redirect to login
                TokenManager.ClearToken(this);
                Toast.MakeText(this, "You have been logged out", ToastLength.Short).Show();
                RedirectToLogin();
            });
            alertDialog.SetNegativeButton("No", (senderAlert, args) => {
                // Do nothing
            });
            alertDialog.Show();
        }

        private void HandleAuthError()
        {
            TokenManager.ClearToken(this);
            Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
            RedirectToLogin();
        }
    }

    // RecyclerView adapter for recent activity
    public class RecentActivityAdapter : RecyclerView.Adapter
    {
        private readonly Activity _activity;
        private readonly List<RecentAttempt> _attempts;

        public RecentActivityAdapter(Activity activity, List<RecentAttempt> attempts)
        {
            _activity = activity;
            _attempts = attempts;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_admin_activity_item, parent, false);
            return new ActivityViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ActivityViewHolder viewHolder = holder as ActivityViewHolder;
            RecentAttempt attempt = _attempts[position];

            // Set activity user
            viewHolder.UserNameTextView.Text = attempt.UserName;

            // Set quiz title
            viewHolder.QuizTitleTextView.Text = attempt.QuizTitle;

            // Set date
            viewHolder.DateTextView.Text = attempt.EndTime.ToString("MMM dd, HH:mm");

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

        // ViewHolder for activity items
        public class ActivityViewHolder : RecyclerView.ViewHolder
        {
            public TextView UserNameTextView { get; }
            public TextView QuizTitleTextView { get; }
            public TextView DateTextView { get; }
            public TextView ScoreTextView { get; }

            public ActivityViewHolder(View itemView) : base(itemView)
            {
                UserNameTextView = itemView.FindViewById<TextView>(Resource.Id.userNameTextView);
                QuizTitleTextView = itemView.FindViewById<TextView>(Resource.Id.quizTitleTextView);
                DateTextView = itemView.FindViewById<TextView>(Resource.Id.dateTextView);
                ScoreTextView = itemView.FindViewById<TextView>(Resource.Id.scoreTextView);
            }
        }
    }

    // RecyclerView adapter for popular quizzes
    public class PopularQuizzesAdapter : RecyclerView.Adapter
    {
        private readonly Activity _activity;
        private readonly List<PopularQuiz> _quizzes;

        public PopularQuizzesAdapter(Activity activity, List<PopularQuiz> quizzes)
        {
            _activity = activity;
            _quizzes = quizzes;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_admin_quiz_item, parent, false);
            return new QuizViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            QuizViewHolder viewHolder = holder as QuizViewHolder;
            PopularQuiz quiz = _quizzes[position];

            // Set quiz title
            viewHolder.QuizTitleTextView.Text = quiz.Title;

            // Set attempt count
            viewHolder.AttemptCountTextView.Text = $"{quiz.AttemptCount} attempts";

            // Set average score
            viewHolder.AverageScoreTextView.Text = $"{quiz.AverageScore:F1}%";

            // Set up event handler for item click
            viewHolder.ItemView.Click += (sender, e) => {
                // Navigate to admin quiz edit activity
                var intent = new Intent(_activity, typeof(AdminEditQuizActivity));
                intent.PutExtra("QuizId", quiz.Id);
                _activity.StartActivity(intent);
            };
        }

        public override int ItemCount => _quizzes.Count;

        // ViewHolder for quiz items
        public class QuizViewHolder : RecyclerView.ViewHolder
        {
            public TextView QuizTitleTextView { get; }
            public TextView AttemptCountTextView { get; }
            public TextView AverageScoreTextView { get; }

            public QuizViewHolder(View itemView) : base(itemView)
            {
                QuizTitleTextView = itemView.FindViewById<TextView>(Resource.Id.quizTitleTextView);
                AttemptCountTextView = itemView.FindViewById<TextView>(Resource.Id.attemptCountTextView);
                AverageScoreTextView = itemView.FindViewById<TextView>(Resource.Id.averageScoreTextView);
            }
        }
    }
}