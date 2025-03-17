using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Text.RegularExpressions;
using Mobile.Services;
using Mobile.Models;
using Android.Content;
using Android.Graphics;

namespace Mobile.Activities
{
    [Activity(Label = "Register")]
    public class RegisterActivity : Activity
    {
        private EditText _emailEditText;
        private EditText _passwordEditText;
        private EditText _confirmPasswordEditText;
        private Button _registerButton;
        private TextView _loginLinkTextView;
        private ApiService _apiService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "register" layout resource
            SetContentView(Resource.Layout.activity_register);

            // Initialize UI elements
            _emailEditText = FindViewById<EditText>(Resource.Id.emailEditText);
            _passwordEditText = FindViewById<EditText>(Resource.Id.passwordEditText);
            _confirmPasswordEditText = FindViewById<EditText>(Resource.Id.confirmPasswordEditText);
            _registerButton = FindViewById<Button>(Resource.Id.registerButton);
            _loginLinkTextView = FindViewById<TextView>(Resource.Id.loginLinkTextView);

            // Initialize service with context
            _apiService = new ApiService(this);

            // Set up event handlers
            _registerButton.Click += OnRegisterButtonClick;
            _loginLinkTextView.Click += OnLoginLinkClick;
        }

        private async void OnRegisterButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get input values
                var email = _emailEditText.Text.Trim();
                var password = _passwordEditText.Text;
                var confirmPassword = _confirmPasswordEditText.Text;

                // Validate inputs
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    Toast.MakeText(this, "All fields are required", ToastLength.Short).Show();
                    return;
                }

                // Validate email format
                if (!IsValidEmail(email))
                {
                    Toast.MakeText(this, "Please enter a valid email address", ToastLength.Short).Show();
                    return;
                }

                // Validate password requirements (min 8 chars, uppercase, lowercase, number, special char)
                if (!IsValidPassword(password))
                {
                    Toast.MakeText(this, "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character", ToastLength.Long).Show();
                    return;
                }

                // Check if passwords match
                if (password != confirmPassword)
                {
                    Toast.MakeText(this, "Passwords do not match", ToastLength.Short).Show();
                    return;
                }

                // Disable register button and show progress
                _registerButton.Enabled = false;
                _registerButton.Text = "Registering...";

                // Show progress dialog
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Creating your account...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    // Create registration request
                    var registerRequest = new RegisterRequest
                    {
                        Email = email,
                        Password = password,
                        ConfirmPassword = confirmPassword
                    };

                    // Call API to register user
                    var authResponse = await _apiService.RegisterAsync(registerRequest);

                    // Show success message
                    Toast.MakeText(this, "Registration successful!", ToastLength.Short).Show();

                    // Navigate to dashboard
                    var intent = new Intent(this, typeof(DashboardActivity));
                    intent.PutExtra("UserName", authResponse.User.UserName);
                    StartActivity(intent);
                    Finish(); // Close registration activity
                }
                finally
                {
                    // Ensure dialog is dismissed
                    if (progressDialog != null && progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }

                    // Re-enable register button
                    _registerButton.Enabled = true;
                    _registerButton.Text = "Register";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration Exception: {ex}");

                // Extract and show meaningful error message
                var errorMessage = GetFriendlyErrorMessage(ex.Message);
                Toast.MakeText(this, errorMessage, ToastLength.Long).Show();
            }
        }

        private void OnLoginLinkClick(object sender, EventArgs e)
        {
            // Navigate back to login screen
            StartActivity(new Intent(this, typeof(MainActivity)));
            Finish(); // Close registration activity
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

        private string GetFriendlyErrorMessage(string originalMessage)
        {
            // Check for common error patterns and return user-friendly messages
            if (originalMessage.Contains("email already exists") ||
                originalMessage.Contains("already taken") ||
                originalMessage.Contains("already registered"))
            {
                return "This email is already registered. Please use a different email or try to log in.";
            }

            if (originalMessage.Contains("password") &&
                (originalMessage.Contains("requirements") || originalMessage.Contains("complexity")))
            {
                return "Your password doesn't meet the requirements. It must contain at least 8 characters including uppercase, lowercase, number, and special character.";
            }

            if (originalMessage.Contains("network") ||
                originalMessage.Contains("connection") ||
                originalMessage.Contains("timeout"))
            {
                return "Network error. Please check your internet connection and try again.";
            }

            // Default message if we can't classify the error
            return "Registration failed: " + originalMessage;
        }
    }
}