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
            try
            {
                base.OnCreate(savedInstanceState);
                Console.WriteLine("AdminCreateUser: OnCreate started");

                // Set our view from the layout resource
                SetContentView(Resource.Layout.activity_admin_create_user);
                Console.WriteLine("AdminCreateUser: SetContentView completed");

                // Initialize UI elements
                InitializeUIElements();

                // Set up event handlers
                SetupEventHandlers();

                Console.WriteLine("AdminCreateUser: OnCreate completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminCreateUser: Exception in OnCreate: {ex.Message}");
                Console.WriteLine($"AdminCreateUser: Stack trace: {ex.StackTrace}");
                Toast.MakeText(this, "Error initializing create user page: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private void InitializeUIElements()
        {
            Console.WriteLine("AdminCreateUser: InitializeUIElements started");

            try
            {
                _emailEditText = FindViewById<EditText>(Resource.Id.emailEditText);
                _passwordEditText = FindViewById<EditText>(Resource.Id.passwordEditText);
                _confirmPasswordEditText = FindViewById<EditText>(Resource.Id.confirmPasswordEditText);
                _usernameEditText = FindViewById<EditText>(Resource.Id.usernameEditText);
                _adminRoleCheckBox = FindViewById<CheckBox>(Resource.Id.adminRoleCheckBox);
                _createUserButton = FindViewById<Button>(Resource.Id.createUserButton);
                _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
                _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);

                // Verify UI elements were found
                if (_emailEditText == null) Console.WriteLine("AdminCreateUser: emailEditText is null");
                if (_passwordEditText == null) Console.WriteLine("AdminCreateUser: passwordEditText is null");
                if (_confirmPasswordEditText == null) Console.WriteLine("AdminCreateUser: confirmPasswordEditText is null");
                if (_usernameEditText == null) Console.WriteLine("AdminCreateUser: usernameEditText is null");
                if (_adminRoleCheckBox == null) Console.WriteLine("AdminCreateUser: adminRoleCheckBox is null");
                if (_createUserButton == null) Console.WriteLine("AdminCreateUser: createUserButton is null");
                if (_cancelButton == null) Console.WriteLine("AdminCreateUser: cancelButton is null");
                if (_loadingProgressBar == null) Console.WriteLine("AdminCreateUser: loadingProgressBar is null");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminCreateUser: Exception in InitializeUIElements: {ex.Message}");
                throw;
            }
        }

        private void SetupEventHandlers()
        {
            Console.WriteLine("AdminCreateUser: SetupEventHandlers started");

            try
            {
                if (_createUserButton != null)
                    _createUserButton.Click += OnCreateUserButtonClick;

                if (_cancelButton != null)
                    _cancelButton.Click += OnCancelButtonClick;

                Console.WriteLine("AdminCreateUser: Event handlers set up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminCreateUser: Exception in SetupEventHandlers: {ex.Message}");
                throw;
            }
        }

        protected override void OnRolesLoaded()
        {
            try
            {
                Console.WriteLine("AdminCreateUser: OnRolesLoaded called");

                // Ensure user has admin access
                if (!IsAdmin)
                {
                    Console.WriteLine("AdminCreateUser: User does not have admin role");
                    Toast.MakeText(this, "You need administrator privileges to access this page", ToastLength.Long).Show();
                    Finish();
                }
                else
                {
                    Console.WriteLine("AdminCreateUser: User has admin role confirmed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminCreateUser: Exception in OnRolesLoaded: {ex.Message}");
            }
        }

        private async void OnCreateUserButtonClick(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("AdminCreateUser: OnCreateUserButtonClick started");

                // Get input values
                string email = _emailEditText?.Text?.Trim();
                string password = _passwordEditText?.Text;
                string confirmPassword = _confirmPasswordEditText?.Text;
                string username = _usernameEditText?.Text?.Trim();
                bool isAdmin = _adminRoleCheckBox?.Checked ?? false;

                Console.WriteLine($"AdminCreateUser: Email: {email}, Username: {username}, IsAdmin: {isAdmin}");

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
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                }

                if (_createUserButton != null)
                {
                    _createUserButton.Enabled = false;
                }

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

                Console.WriteLine("AdminCreateUser: Calling ApiService.CreateUserAsync");

                // Call API to create user
                await ApiService.CreateUserAsync(createUserRequest);

                Toast.MakeText(this, "User created successfully", ToastLength.Short).Show();
                Console.WriteLine("AdminCreateUser: User created successfully");

                Finish(); // Return to previous activity
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("AdminCreateUser: UnauthorizedAccessException caught");
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminCreateUser: Exception in OnCreateUserButtonClick: {ex.Message}");
                Console.WriteLine($"AdminCreateUser: Stack trace: {ex.StackTrace}");
                Toast.MakeText(this, $"Failed to create user: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
                }

                if (_createUserButton != null)
                {
                    _createUserButton.Enabled = true;
                }

                Console.WriteLine("AdminCreateUser: OnCreateUserButtonClick completed");
            }
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("AdminCreateUser: OnCancelButtonClick - finishing activity");
                Finish(); // Return to previous activity
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminCreateUser: Exception in OnCancelButtonClick: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminCreateUser: Exception in IsValidEmail: {ex.Message}");
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"AdminCreateUser: Exception in IsValidPassword: {ex.Message}");
                return false;
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
                Console.WriteLine($"AdminCreateUser: Exception in HandleAuthError: {ex.Message}");
            }
        }
    }
}