using Android.App;
using Android.OS;
using Android.Widget;
using System;
using Mobile.Services;
using Android.Content;

namespace Mobile.Activities
{
    [Activity(Label = "Quiz App", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private EditText _emailEditText;
        private EditText _passwordEditText;
        private Button _loginButton;
        private TextView _registerLinkTextView;
        private ApiService _apiService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            // Initialize UI elements
            _emailEditText = FindViewById<EditText>(Resource.Id.emailEditText);
            _passwordEditText = FindViewById<EditText>(Resource.Id.passwordEditText);
            _loginButton = FindViewById<Button>(Resource.Id.loginButton);
            _registerLinkTextView = FindViewById<TextView>(Resource.Id.registerLinkTextView);

            // Initialize service with context
            _apiService = new ApiService(this);

            // Check if token exists and skip login if it does
            string token = TokenManager.GetToken(this);
            if (!string.IsNullOrEmpty(token))
            {
                Toast.MakeText(this, "Welcome back!", ToastLength.Short).Show();
                NavigateToDashboard();
                return;
            }

            // Set up event handlers
            _loginButton.Click += OnLoginButtonClick;
            _registerLinkTextView.Click += OnRegisterLinkClick;
        }

        private async void OnLoginButtonClick(object sender, EventArgs e)
        {
            try
            {
                var email = _emailEditText.Text;
                var password = _passwordEditText.Text;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    Toast.MakeText(this, "Please enter both email and password", ToastLength.Short).Show();
                    return;
                }

                _loginButton.Enabled = false;
                _loginButton.Text = "Logging in...";

                // Create a progress dialog
                var progressDialog = new Android.App.ProgressDialog(this);
                progressDialog.SetMessage("Logging in...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    var authResponse = await _apiService.LoginAsync(email, password);

                    progressDialog.Dismiss();
                    Toast.MakeText(this, $"Welcome {authResponse.User.UserName}!", ToastLength.Short).Show();

                    // Navigate to dashboard
                    NavigateToDashboard();
                }
                finally
                {
                    // Ensure dialog is dismissed
                    if (progressDialog != null && progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Exception: {ex}");
                Toast.MakeText(this, $"Login failed: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                _loginButton.Enabled = true;
                _loginButton.Text = "Login";
            }
        }

        private void NavigateToDashboard()
        {
            // Check if a username is saved
            string username = TokenManager.GetUsername(this);

            // Navigate to dashboard or quiz list
            Intent intent;
            if (!string.IsNullOrEmpty(username))
            {
                // Go to dashboard if we have user info
                intent = new Intent(this, typeof(DashboardActivity));
                intent.PutExtra("UserName", username);
            }
            else
            {
                // Otherwise just go to quiz list
                intent = new Intent(this, typeof(QuizListActivity));
            }

            StartActivity(intent);
            Finish(); // Close login activity
        }

        private void OnRegisterLinkClick(object sender, EventArgs e)
        {
            // Navigate to registration screen
            var intent = new Intent(this, typeof(RegisterActivity));
            StartActivity(intent);
        }
    }
}