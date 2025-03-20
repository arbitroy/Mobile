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
    public class ResetPasswordActivity : Activity
    {
        private TextView _emailTextView;
        private EditText _codeEditText;
        private EditText _passwordEditText;
        private EditText _confirmPasswordEditText;
        private Button _resetButton;
        private ProgressBar _loadingProgressBar;
        private ApiService _apiService;
        private string _email;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_reset_password);

            // Get email from intent
            _email = Intent.GetStringExtra("Email") ?? string.Empty;

            // Initialize UI elements
            _emailTextView = FindViewById<TextView>(Resource.Id.emailTextView);
            _codeEditText = FindViewById<EditText>(Resource.Id.codeEditText);
            _passwordEditText = FindViewById<EditText>(Resource.Id.passwordEditText);
            _confirmPasswordEditText = FindViewById<EditText>(Resource.Id.confirmPasswordEditText);
            _resetButton = FindViewById<Button>(Resource.Id.resetButton);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);

            // Set email text
            _emailTextView.Text = _email;

            // Initialize API service
            _apiService = new ApiService(this);

            // Set up event handlers
            _resetButton.Click += OnResetButtonClick;
        }

        private async void OnResetButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get input values
                string code = _codeEditText.Text?.Trim();
                string password = _passwordEditText.Text;
                string confirmPassword = _confirmPasswordEditText.Text;

                // Validate inputs
                if (string.IsNullOrEmpty(code))
                {
                    Toast.MakeText(this, "Please enter the reset code", ToastLength.Short).Show();
                    return;
                }

                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    Toast.MakeText(this, "Please enter your new password", ToastLength.Short).Show();
                    return;
                }

                if (password != confirmPassword)
                {
                    Toast.MakeText(this, "Passwords do not match", ToastLength.Short).Show();
                    return;
                }

                if (!IsValidPassword(password))
                {
                    Toast.MakeText(this, "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character", ToastLength.Long).Show();
                    return;
                }

                // Show loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                _resetButton.Enabled = false;

                // Reset password
                bool success = await _apiService.ResetPasswordAsync(_email, code, password, confirmPassword);

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
                    Toast.MakeText(this, "Password reset failed", ToastLength.Long).Show();
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