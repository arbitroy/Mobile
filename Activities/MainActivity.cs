using Android.App;
using Android.OS;
using Android.Widget;
using System;
using Mobile.Services;

namespace Mobile.Activities
{
    [Activity(Label = "Quiz App", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private EditText _emailEditText;
        private EditText _passwordEditText;
        private Button _loginButton;
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

            // Initialize service
            _apiService = new ApiService();

            // Set up event handlers
            _loginButton.Click += OnLoginButtonClick;
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

                // Initialize API service with context
                _apiService = new ApiService(this);

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

                    // Navigate to quiz list activity
                    var intent = new Android.Content.Intent(this, typeof(QuizListActivity));
                    StartActivity(intent);
                    Finish(); // Close login activity
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
    }
}