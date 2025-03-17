using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Mobile.Models;
using Mobile.Services;
using System.Text.RegularExpressions;
using System.Linq;

namespace Mobile.Activities
{
    [Activity(Label = "Create User")]
    public class AdminCreateUserActivity : BaseAuthenticatedActivity
    {
        private EditText _emailEditText;
        private EditText _passwordEditText;
        private EditText _confirmPasswordEditText;
        private EditText _usernameEditText;
        private CheckBox _adminRoleCheckBox;
        private Button _createUserButton;
        private Button _cancelButton;
        private ProgressBar _loadingProgressBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_admin_create_user);

            // Initialize UI elements
            _emailEditText = FindViewById<EditText>(Resource.Id.emailEditText);
            _passwordEditText = FindViewById<EditText>(Resource.Id.passwordEditText);
            _confirmPasswordEditText = FindViewById<EditText>(Resource.Id.confirmPasswordEditText);
            _usernameEditText = FindViewById<EditText>(Resource.Id.usernameEditText);
            _adminRoleCheckBox = FindViewById<CheckBox>(Resource.Id.adminRoleCheckBox);
            _createUserButton = FindViewById<Button>(Resource.Id.createUserButton);
            _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);

            // Set up event handlers
            _createUserButton.Click += OnCreateUserButtonClick;
            _cancelButton.Click += OnCancelButtonClick;
        }

        protected override void OnRolesLoaded()
        {
            // Ensure user has admin access
            if (!IsAdmin)
            {
                Toast.MakeText(this, "You need administrator privileges to access this page", ToastLength.Long).Show();
                Finish();
            }
        }

        private async void OnCreateUserButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get input values
                string email = _emailEditText.Text?.Trim();
                string password = _passwordEditText.Text;
                string confirmPassword = _confirmPasswordEditText.Text;
                string username = _usernameEditText.Text?.Trim();
                bool isAdmin = _adminRoleCheckBox.Checked;

                // Validate inputs
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(username))
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

                // Validate password requirements
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

                // Show loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                _createUserButton.Enabled = false;

                // Create user roles list
                List<string> roles = new List<string> { "User" };
                if (isAdmin)
                {
                    roles.Add("Administrator");
                }

                // Create user request
                var createUserRequest = new AdminCreateUserRequest
                {
                    Email = email,
                    Password = password,
                    UserName = username,
                    Roles = roles
                };

                // Call API to create user
                await ApiService.CreateUserAsync(createUserRequest);

                Toast.MakeText(this, "User created successfully", ToastLength.Short).Show();
                Finish(); // Return to previous activity
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to create user: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
                _createUserButton.Enabled = true;
            }
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            Finish(); // Return to previous activity
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

        private void HandleAuthError()
        {
            TokenManager.ClearToken(this);
            Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
            RedirectToLogin();
        }
    }
}