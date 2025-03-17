using Android.App;
using Android.OS;
using Android.Widget;
using System;
using Mobile.Services;
using Mobile.Models;
using Android.Content;
using System.Linq;
using System.Threading.Tasks;

namespace Mobile.Activities
{
    [Activity(Label = "Admin Dashboard")]
    public class AdminDashboardActivity : BaseAuthenticatedActivity
    {
        private TextView _adminUsernameTextView;
        private TextView _userCountTextView;
        private TextView _quizCountTextView;
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
            _manageQuizzesButton = FindViewById<Button>(Resource.Id.manageQuizzesButton);
            _createQuizButton = FindViewById<Button>(Resource.Id.createQuizButton);
            _manageUsersButton = FindViewById<Button>(Resource.Id.manageUsersButton);
            _createUserButton = FindViewById<Button>(Resource.Id.createUserButton);
            _viewMyProfileButton = FindViewById<Button>(Resource.Id.viewMyProfileButton);
            _logoutButton = FindViewById<Button>(Resource.Id.logoutButton);

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
            }
        }

        private void OnManageQuizzesButtonClick(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Manage Quizzes feature is not yet implemented", ToastLength.Short).Show();
            // TODO: Implement AdminQuizListActivity
        }

        private void OnCreateQuizButtonClick(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Create Quiz feature is not yet implemented", ToastLength.Short).Show();
            // TODO: Implement AdminCreateQuizActivity
        }

        private void OnManageUsersButtonClick(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Manage Users feature is not yet implemented", ToastLength.Short).Show();
            // TODO: Implement AdminUserListActivity
        }

        private void OnCreateUserButtonClick(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Create User feature is not yet implemented", ToastLength.Short).Show();
            // TODO: Implement AdminCreateUserActivity
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
}