using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Text.RegularExpressions;
using Mobile.Services;
using Android.Content;

namespace Mobile.Activities
{
    [Activity(Label = "Forgot Password")]
    public class ForgotPasswordActivity : Activity
    {
        private EditText _emailEditText;
        private Button _resetButton;
        private TextView _loginLinkTextView;
        private ProgressBar _loadingProgressBar;
        private ApiService _apiService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_forgot_password);

            // Initialize UI elements
            _emailEditText = FindViewById<EditText>(Resource.Id.emailEditText);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);
            _loginLinkTextView = FindViewById<TextView>(Resource.Id.loginLinkTextView);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);

            // Initialize API service
            _apiService = new ApiService(this);

            // Set up event handlers
            _resetButton.Click += OnResetButtonClick;
            _loginLinkTextView.Click += OnLoginLinkClick;
        }

        private async void OnResetButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get email
                string email = _emailEditText.Text?.Trim();

                // Validate email
                if (string.IsNullOrEmpty(email))
                {
                    Toast.MakeText(this, "Please enter your email address", ToastLength.Short).Show();
                    return;
                }

                if (!IsValidEmail(email))
                {
                    Toast.MakeText(this, "Please enter a valid email address", ToastLength.Short).Show();
                    return;
                }

                // Show loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                _resetButton.Enabled = false;

                // Send password reset request
                bool success = await _apiService.ForgotPasswordAsync(email);

                if (success)
                {
                    // Show success message and navigate to reset password screen
                    Toast.MakeText(this, "Reset instructions have been sent to your email", ToastLength.Long).Show();

                    var intent = new Intent(this, typeof(ResetPasswordActivity));
                    intent.PutExtra("Email", email);
                    StartActivity(intent);
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, "Password reset request failed", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
                _resetButton.Enabled = true;
            }
        }

        private void OnLoginLinkClick(object sender, EventArgs e)
        {
            // Navigate back to login
            StartActivity(new Intent(this, typeof(MainActivity)));
            Finish();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}