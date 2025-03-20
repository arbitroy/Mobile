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
            try
            {
                base.OnCreate(savedInstanceState);
                Console.WriteLine("AdminDashboard: OnCreate started");

                // Set our view from the "admin_dashboard" layout resource
                SetContentView(Resource.Layout.activity_admin_dashboard);
                Console.WriteLine("AdminDashboard: SetContentView completed");

                // Initialize UI elements with null checking
                InitializeUIElements();

                // Set admin username
                string username = TokenManager.GetUsername(this) ?? "Admin";
                if (_adminUsernameTextView != null)
                {
                    _adminUsernameTextView.Text = username;
                }
                else
                {
                    Console.WriteLine("AdminDashboard: adminUsernameTextView is null");
                }

                // Set up event handlers
                SetupEventHandlers();

                // Load admin stats
                LoadAdminStatsAsync();
                Console.WriteLine("AdminDashboard: OnCreate completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in OnCreate: {ex.Message}");
                Console.WriteLine($"AdminDashboard: Stack trace: {ex.StackTrace}");
                Toast.MakeText(this, "Error initializing dashboard: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private void InitializeUIElements()
        {
            Console.WriteLine("AdminDashboard: InitializeUIElements started");

            try
            {
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
                if (_recentActivityRecyclerView != null)
                {
                    _recentActivityRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
                }
                else
                {
                    Console.WriteLine("AdminDashboard: recentActivityRecyclerView is null");
                }

                if (_popularQuizzesRecyclerView != null)
                {
                    _popularQuizzesRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
                }
                else
                {
                    Console.WriteLine("AdminDashboard: popularQuizzesRecyclerView is null");
                }

                Console.WriteLine("AdminDashboard: UI elements initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception during UI initialization: {ex.Message}");
                throw; // Re-throw to be caught by outer try-catch
            }
        }

        private void SetupEventHandlers()
        {
            Console.WriteLine("AdminDashboard: Setting up event handlers");

            try
            {
                if (_manageQuizzesButton != null)
                    _manageQuizzesButton.Click += OnManageQuizzesButtonClick;

                if (_createQuizButton != null)
                    _createQuizButton.Click += OnCreateQuizButtonClick;

                if (_manageUsersButton != null)
                    _manageUsersButton.Click += OnManageUsersButtonClick;

                if (_createUserButton != null)
                    _createUserButton.Click += OnCreateUserButtonClick;

                if (_viewMyProfileButton != null)
                    _viewMyProfileButton.Click += OnViewMyProfileButtonClick;

                if (_logoutButton != null)
                    _logoutButton.Click += OnLogoutButtonClick;

                Console.WriteLine("AdminDashboard: Event handlers setup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception setting up event handlers: {ex.Message}");
                throw; // Re-throw to be caught by outer try-catch
            }
        }

        protected override void OnRolesLoaded()
        {
            try
            {
                Console.WriteLine("AdminDashboard: OnRolesLoaded started");

                // Ensure user has admin access
                if (!IsAdmin)
                {
                    Console.WriteLine("AdminDashboard: User does not have admin role");
                    Toast.MakeText(this, "You need administrator privileges to access this dashboard", ToastLength.Long).Show();

                    // Redirect to user dashboard
                    var intent = new Intent(this, typeof(DashboardActivity));
                    StartActivity(intent);
                    Finish();
                }
                else
                {
                    Console.WriteLine("AdminDashboard: User has admin role confirmed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in OnRolesLoaded: {ex.Message}");
            }
        }

        private async void LoadAdminStatsAsync()
        {
            try
            {
                Console.WriteLine("AdminDashboard: LoadAdminStatsAsync started");

                // Show loading indicator
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Loading statistics...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();
                Console.WriteLine("AdminDashboard: Progress dialog shown");

                try
                {
                    // Get admin stats from API
                    Console.WriteLine("AdminDashboard: Calling GetAdminStatsAsync");
                    _adminStats = await ApiService.GetAdminStatsAsync();
                    Console.WriteLine("AdminDashboard: GetAdminStatsAsync completed successfully");

                    // Update UI with stats
                    UpdateDashboardStats();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AdminDashboard: Inner exception in LoadAdminStatsAsync: {ex.Message}");
                    throw; // Re-throw to be handled by outer catch
                }
                finally
                {
                    // Hide loading indicator
                    if (progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                        Console.WriteLine("AdminDashboard: Progress dialog dismissed");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"AdminDashboard: Unauthorized access: {ex.Message}");
                // Handle authentication errors
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in LoadAdminStatsAsync: {ex.Message}");
                Console.WriteLine($"AdminDashboard: Stack trace: {ex.StackTrace}");

                Toast.MakeText(this, $"Failed to load statistics: {ex.Message}", ToastLength.Long).Show();

                // Set default values for stats
                SafeSetTextView(_userCountTextView, "-");
                SafeSetTextView(_quizCountTextView, "-");
                SafeSetTextView(_attemptCountTextView, "-");
                SafeSetTextView(_avgScoreTextView, "-");
            }
        }

        private void UpdateDashboardStats()
        {
            Console.WriteLine("AdminDashboard: UpdateDashboardStats started");

            try
            {
                if (_adminStats != null)
                {
                    Console.WriteLine("AdminDashboard: Admin stats object is not null");

                    // Update UI with stats
                    SafeSetTextView(_userCountTextView, _adminStats.UserCount.ToString());
                    SafeSetTextView(_quizCountTextView, _adminStats.QuizCount.ToString());
                    SafeSetTextView(_attemptCountTextView, _adminStats.AttemptCount.ToString());
                    SafeSetTextView(_avgScoreTextView, $"{_adminStats.AverageScore:F1}%");

                    // Set up recent activity RecyclerView
                    if (_recentActivityRecyclerView != null && _adminStats.RecentAttempts != null && _adminStats.RecentAttempts.Count > 0)
                    {
                        Console.WriteLine($"AdminDashboard: Setting up recent activity with {_adminStats.RecentAttempts.Count} items");
                        var recentActivityAdapter = new RecentActivityAdapter(this, _adminStats.RecentAttempts);
                        _recentActivityRecyclerView.SetAdapter(recentActivityAdapter);
                    }
                    else
                    {
                        Console.WriteLine("AdminDashboard: No recent activities to display or RecyclerView is null");
                    }

                    // Set up popular quizzes RecyclerView
                    if (_popularQuizzesRecyclerView != null && _adminStats.PopularQuizzes != null && _adminStats.PopularQuizzes.Count > 0)
                    {
                        Console.WriteLine($"AdminDashboard: Setting up popular quizzes with {_adminStats.PopularQuizzes.Count} items");
                        var popularQuizzesAdapter = new PopularQuizzesAdapter(this, _adminStats.PopularQuizzes);
                        _popularQuizzesRecyclerView.SetAdapter(popularQuizzesAdapter);
                    }
                    else
                    {
                        Console.WriteLine("AdminDashboard: No popular quizzes to display or RecyclerView is null");
                    }
                }
                else
                {
                    Console.WriteLine("AdminDashboard: Admin stats object is null");

                    // Set default values for stats
                    SafeSetTextView(_userCountTextView, "-");
                    SafeSetTextView(_quizCountTextView, "-");
                    SafeSetTextView(_attemptCountTextView, "-");
                    SafeSetTextView(_avgScoreTextView, "-");
                }

                Console.WriteLine("AdminDashboard: UpdateDashboardStats completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in UpdateDashboardStats: {ex.Message}");
                Console.WriteLine($"AdminDashboard: Stack trace: {ex.StackTrace}");
            }
        }

        private void SafeSetTextView(TextView textView, string value)
        {
            if (textView != null)
            {
                textView.Text = value;
            }
            else
            {
                Console.WriteLine($"AdminDashboard: Attempted to set text on null TextView. Value: {value}");
            }
        }

        private void OnManageQuizzesButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Navigate to AdminQuizListActivity
                var intent = new Intent(this, typeof(AdminQuizListActivity));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in OnManageQuizzesButtonClick: {ex.Message}");
                Toast.MakeText(this, "Error navigating to Quiz List: " + ex.Message, ToastLength.Short).Show();
            }
        }

        private void OnCreateQuizButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Navigate to AdminCreateQuizActivity
                var intent = new Intent(this, typeof(AdminCreateQuizActivity));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in OnCreateQuizButtonClick: {ex.Message}");
                Toast.MakeText(this, "Error navigating to Create Quiz: " + ex.Message, ToastLength.Short).Show();
            }
        }

        private void OnManageUsersButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Navigate to AdminUserListActivity
                var intent = new Intent(this, typeof(AdminUserListActivity));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in OnManageUsersButtonClick: {ex.Message}");
                Toast.MakeText(this, "Error navigating to User List: " + ex.Message, ToastLength.Short).Show();
            }
        }

        private void OnCreateUserButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Navigate to AdminCreateUserActivity
                var intent = new Intent(this, typeof(AdminCreateUserActivity));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in OnCreateUserButtonClick: {ex.Message}");
                Toast.MakeText(this, "Error navigating to Create User: " + ex.Message, ToastLength.Short).Show();
            }
        }

        private void OnViewMyProfileButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Navigate to user profile activity
                var intent = new Intent(this, typeof(UserProfileActivity));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in OnViewMyProfileButtonClick: {ex.Message}");
                Toast.MakeText(this, "Error navigating to Profile: " + ex.Message, ToastLength.Short).Show();
            }
        }

        private void OnLogoutButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Show confirmation dialog
                var alertDialog = new AlertDialog.Builder(this);
                alertDialog.SetTitle("Logout");
                alertDialog.SetMessage("Are you sure you want to logout?");
                alertDialog.SetPositiveButton("Yes", (senderAlert, args) => {
                    try
                    {
                        // Clear token and redirect to login
                        TokenManager.ClearToken(this);
                        Toast.MakeText(this, "You have been logged out", ToastLength.Short).Show();
                        RedirectToLogin();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"AdminDashboard: Exception during logout: {ex.Message}");
                        Toast.MakeText(this, "Error during logout: " + ex.Message, ToastLength.Short).Show();
                    }
                });
                alertDialog.SetNegativeButton("No", (senderAlert, args) => {
                    // Do nothing
                });
                alertDialog.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in OnLogoutButtonClick: {ex.Message}");
                Toast.MakeText(this, "Error showing logout dialog: " + ex.Message, ToastLength.Short).Show();
            }
        }

        private void HandleAuthError()
        {
            try
            {
                TokenManager.ClearToken(this);
                Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
                RedirectToLogin();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminDashboard: Exception in HandleAuthError: {ex.Message}");
            }
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
            _attempts = attempts ?? new List<RecentAttempt>(); // Ensure it's never null
            Console.WriteLine($"RecentActivityAdapter: Created with {_attempts.Count} items");
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                Console.WriteLine("RecentActivityAdapter: OnCreateViewHolder called");
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_admin_activity_item, parent, false);
                return new ActivityViewHolder(itemView);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RecentActivityAdapter: Exception in OnCreateViewHolder: {ex.Message}");
                // Create a fallback ViewHolder using our own ActivityViewHolder
                View fallbackView = new TextView(_activity) { Text = "Error loading item" };
                return new ActivityViewHolder(fallbackView);
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            try
            {
                Console.WriteLine($"RecentActivityAdapter: OnBindViewHolder called for position {position}");

                if (!(holder is ActivityViewHolder viewHolder))
                {
                    Console.WriteLine("RecentActivityAdapter: ViewHolder is not ActivityViewHolder");
                    return;
                }

                if (position >= _attempts.Count)
                {
                    Console.WriteLine($"RecentActivityAdapter: Position {position} out of range (count: {_attempts.Count})");
                    return;
                }

                RecentAttempt attempt = _attempts[position];

                if (attempt == null)
                {
                    Console.WriteLine("RecentActivityAdapter: Attempt at position is null");
                    return;
                }

                // Set activity user
                if (viewHolder.UserNameTextView != null)
                {
                    viewHolder.UserNameTextView.Text = attempt.UserName ?? "Unknown User";
                }

                // Set quiz title
                if (viewHolder.QuizTitleTextView != null)
                {
                    viewHolder.QuizTitleTextView.Text = attempt.QuizTitle ?? "Unknown Quiz";
                }

                // Set date
                if (viewHolder.DateTextView != null)
                {
                    viewHolder.DateTextView.Text = attempt.EndTime.ToString("MMM dd, HH:mm");
                }

                // Set score
                if (viewHolder.ScoreTextView != null)
                {
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
                }

                // Set up event handler for item click
                if (viewHolder.ItemView != null)
                {
                    viewHolder.ItemView.Click += (sender, e) => {
                        try
                        {
                            // Navigate to quiz result activity
                            var intent = new Intent(_activity, typeof(QuizResultActivity));
                            intent.PutExtra("AttemptId", attempt.AttemptId);
                            _activity.StartActivity(intent);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"RecentActivityAdapter: Exception in item click handler: {ex.Message}");
                            Toast.MakeText(_activity, "Error viewing result: " + ex.Message, ToastLength.Short).Show();
                        }
                    };
                }

                Console.WriteLine($"RecentActivityAdapter: Successfully bound item at position {position}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RecentActivityAdapter: Exception in OnBindViewHolder: {ex.Message}");
                Console.WriteLine($"RecentActivityAdapter: Stack trace: {ex.StackTrace}");
            }
        }

        public override int ItemCount
        {
            get
            {
                int count = _attempts?.Count ?? 0;
                Console.WriteLine($"RecentActivityAdapter: ItemCount = {count}");
                return count;
            }
        }

        // ViewHolder for activity items
        public class ActivityViewHolder : RecyclerView.ViewHolder
        {
            public TextView UserNameTextView { get; }
            public TextView QuizTitleTextView { get; }
            public TextView DateTextView { get; }
            public TextView ScoreTextView { get; }

            public ActivityViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    UserNameTextView = itemView.FindViewById<TextView>(Resource.Id.userNameTextView);
                    QuizTitleTextView = itemView.FindViewById<TextView>(Resource.Id.quizTitleTextView);
                    DateTextView = itemView.FindViewById<TextView>(Resource.Id.dateTextView);
                    ScoreTextView = itemView.FindViewById<TextView>(Resource.Id.scoreTextView);

                    if (UserNameTextView == null) Console.WriteLine("ActivityViewHolder: userNameTextView is null");
                    if (QuizTitleTextView == null) Console.WriteLine("ActivityViewHolder: quizTitleTextView is null");
                    if (DateTextView == null) Console.WriteLine("ActivityViewHolder: dateTextView is null");
                    if (ScoreTextView == null) Console.WriteLine("ActivityViewHolder: scoreTextView is null");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ActivityViewHolder: Exception during initialization: {ex.Message}");
                }
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
            _quizzes = quizzes ?? new List<PopularQuiz>(); // Ensure it's never null
            Console.WriteLine($"PopularQuizzesAdapter: Created with {_quizzes.Count} items");
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                Console.WriteLine("PopularQuizzesAdapter: OnCreateViewHolder called");
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_admin_quiz_item, parent, false);
                return new QuizViewHolder(itemView);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PopularQuizzesAdapter: Exception in OnCreateViewHolder: {ex.Message}");
                // Create a fallback ViewHolder using our own QuizViewHolder
                View fallbackView = new TextView(_activity) { Text = "Error loading item" };
                return new QuizViewHolder(fallbackView);
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            try
            {
                Console.WriteLine($"PopularQuizzesAdapter: OnBindViewHolder called for position {position}");

                if (!(holder is QuizViewHolder viewHolder))
                {
                    Console.WriteLine("PopularQuizzesAdapter: ViewHolder is not QuizViewHolder");
                    return;
                }

                if (position >= _quizzes.Count)
                {
                    Console.WriteLine($"PopularQuizzesAdapter: Position {position} out of range (count: {_quizzes.Count})");
                    return;
                }

                PopularQuiz quiz = _quizzes[position];

                if (quiz == null)
                {
                    Console.WriteLine("PopularQuizzesAdapter: Quiz at position is null");
                    return;
                }

                // Set quiz title
                if (viewHolder.QuizTitleTextView != null)
                {
                    viewHolder.QuizTitleTextView.Text = quiz.Title ?? "Unknown Quiz";
                }

                // Set attempt count
                if (viewHolder.AttemptCountTextView != null)
                {
                    viewHolder.AttemptCountTextView.Text = $"{quiz.AttemptCount} attempts";
                }

                // Set average score
                if (viewHolder.AverageScoreTextView != null)
                {
                    viewHolder.AverageScoreTextView.Text = $"{quiz.AverageScore:F1}%";
                }

                // Set up event handler for item click
                if (viewHolder.ItemView != null)
                {
                    viewHolder.ItemView.Click += (sender, e) => {
                        try
                        {
                            // Navigate to admin quiz edit activity
                            var intent = new Intent(_activity, typeof(AdminEditQuizActivity));
                            intent.PutExtra("QuizId", quiz.Id);
                            _activity.StartActivity(intent);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"PopularQuizzesAdapter: Exception in item click handler: {ex.Message}");
                            Toast.MakeText(_activity, "Error editing quiz: " + ex.Message, ToastLength.Short).Show();
                        }
                    };
                }

                Console.WriteLine($"PopularQuizzesAdapter: Successfully bound item at position {position}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PopularQuizzesAdapter: Exception in OnBindViewHolder: {ex.Message}");
                Console.WriteLine($"PopularQuizzesAdapter: Stack trace: {ex.StackTrace}");
            }
        }

        public override int ItemCount
        {
            get
            {
                int count = _quizzes?.Count ?? 0;
                Console.WriteLine($"PopularQuizzesAdapter: ItemCount = {count}");
                return count;
            }
        }

        // ViewHolder for quiz items
        public class QuizViewHolder : RecyclerView.ViewHolder
        {
            public TextView QuizTitleTextView { get; }
            public TextView AttemptCountTextView { get; }
            public TextView AverageScoreTextView { get; }

            public QuizViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    QuizTitleTextView = itemView.FindViewById<TextView>(Resource.Id.quizTitleTextView);
                    AttemptCountTextView = itemView.FindViewById<TextView>(Resource.Id.attemptCountTextView);
                    AverageScoreTextView = itemView.FindViewById<TextView>(Resource.Id.averageScoreTextView);

                    if (QuizTitleTextView == null) Console.WriteLine("QuizViewHolder: quizTitleTextView is null");
                    if (AttemptCountTextView == null) Console.WriteLine("QuizViewHolder: attemptCountTextView is null");
                    if (AverageScoreTextView == null) Console.WriteLine("QuizViewHolder: averageScoreTextView is null");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"QuizViewHolder: Exception during initialization: {ex.Message}");
                }
            }
        }
    }
}