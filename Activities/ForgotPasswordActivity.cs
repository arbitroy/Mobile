using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Text.RegularExpressions;
using Mobile.Services;
using Mobile.Models;
using Android.Content;

namespace Mobile.Activities
{
    [Activity(Label = "Reset Password")]
    public class ForgotPasswordActivity : Activity
    {
        private EditText _emailEditText;
        private EditText _newPasswordEditText;
        private EditText _confirmPasswordEditText;
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
            _newPasswordEditText = FindViewById<EditText>(Resource.Id.newPasswordEditText);
            _confirmPasswordEditText = FindViewById<EditText>(Resource.Id.confirmPasswordEditText);
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
                // Get input values
                string email = _emailEditText.Text?.Trim();
                string newPassword = _newPasswordEditText.Text;
                string confirmPassword = _confirmPasswordEditText.Text;

                // Validate inputs
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

                if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    Toast.MakeText(this, "Please enter your new password", ToastLength.Short).Show();
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    Toast.MakeText(this, "Passwords do not match", ToastLength.Short).Show();
                    return;
                }

                if (!IsValidPassword(newPassword))
                {
                    Toast.MakeText(this, "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character", ToastLength.Long).Show();
                    return;
                }

                // Show loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                _resetButton.Enabled = false;

                try
                {
                    // Call API to reset password directly
                    bool success = await _apiService.DirectResetPasswordAsync(email, newPassword);

                    if (success)
                    {
                        // Show success message and navigate to login
                        Toast.MakeText(this, "Password has been reset successfully. You can now log in with your new password.", ToastLength.Long).Show();

                        var intent = new Intent(this, typeof(MainActivity));
                        StartActivity(intent);
                        Finish();
                    }
                    else
                    {
                        Toast.MakeText(this, "Failed to reset password. Please check your email and try again.", ToastLength.Long).Show();
                    }
                }
                finally
                {
                    // Hide loading indicator
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
                    _resetButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error: {ex.Message}", ToastLength.Long).Show();
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

        private bool IsValidPassword(string password)
        {
            // Check minimum length
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            // Check for uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for digit
            if (!Regex.IsMatch(password, @"[0-9]"))
                return false;

            // Check for special character
            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return false;

            return true;
        }
    }
}