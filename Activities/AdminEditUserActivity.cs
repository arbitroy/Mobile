using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Mobile.Models;
using Mobile.Services;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mobile.Activities
{
    [Activity(Label = "Edit User")]
    public class AdminEditUserActivity : BaseAuthenticatedActivity
    {
        private TextView _emailTextView;
        private EditText _usernameEditText;
        private CheckBox _adminRoleCheckBox;
        private Button _saveUserButton;
        private Button _resetPasswordButton;
        private Button _cancelButton;
        private ProgressBar _loadingProgressBar;

        private string _userId;
        private UserProfile _userProfile;
        private bool _isCurrentUser;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_admin_edit_user);

            // Get user ID from intent
            _userId = Intent.GetStringExtra("UserId");
            if (string.IsNullOrEmpty(_userId))
            {
                Toast.MakeText(this, "Invalid user ID", ToastLength.Short).Show();
                Finish();
                return;
            }

            // Initialize UI elements
            _emailTextView = FindViewById<TextView>(Resource.Id.emailTextView);
            _usernameEditText = FindViewById<EditText>(Resource.Id.usernameEditText);
            _adminRoleCheckBox = FindViewById<CheckBox>(Resource.Id.adminRoleCheckBox);
            _saveUserButton = FindViewById<Button>(Resource.Id.saveUserButton);
            _resetPasswordButton = FindViewById<Button>(Resource.Id.resetPasswordButton);
            _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);

            // Determine if editing current user
            string currentUserId = TokenManager.GetUserId(this);
            _isCurrentUser = _userId == currentUserId;

            // Set up event handlers
            _saveUserButton.Click += OnSaveUserButtonClick;
            _resetPasswordButton.Click += OnResetPasswordButtonClick;
            _cancelButton.Click += OnCancelButtonClick;

            // Load user details
            LoadUserAsync();
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

        private async void LoadUserAsync()
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;

                // Get user details from API
                _userProfile = await ApiService.GetUserByIdAsync(_userId);

                // Update UI with user details
                _emailTextView.Text = _userProfile.Email;
                _usernameEditText.Text = _userProfile.UserName;
                _adminRoleCheckBox.Checked = _userProfile.Roles.Contains("Administrator");

                // If editing current user, disable admin role checkbox (prevent removing own admin rights)
                if (_isCurrentUser)
                {
                    _adminRoleCheckBox.Enabled = false;
                    _adminRoleCheckBox.Alpha = 0.5f;

                    // Add note
                    var note = FindViewById<TextView>(Resource.Id.noteTextView);
                    if (note != null)
                    {
                        note.Text = "Note: You cannot modify your own admin status.";
                        note.Visibility = Android.Views.ViewStates.Visible;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load user details: {ex.Message}", ToastLength.Long).Show();
                Finish();
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private async void OnSaveUserButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get input values
                string username = _usernameEditText.Text?.Trim();
                bool isAdmin = _adminRoleCheckBox.Checked;

                // Validate inputs
                if (string.IsNullOrEmpty(username))
                {
                    Toast.MakeText(this, "Username is required", ToastLength.Short).Show();
                    return;
                }

                // Show loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                _saveUserButton.Enabled = false;

                // Create user roles list
                List<string> roles = new List<string> { "User" };
                if (isAdmin)
                {
                    roles.Add("Administrator");
                }

                // Create update request
                var updateUserRequest = new AdminUpdateUserRequest
                {
                    UserName = username,
                    Roles = roles
                };

                // Call API to update user
                await ApiService.UpdateUserAsync(_userId, updateUserRequest);

                // If editing current user, update stored username
                if (_isCurrentUser)
                {
                    TokenManager.SaveUsername(this, username);
                }

                Toast.MakeText(this, "User updated successfully", ToastLength.Short).Show();
                Finish(); // Return to previous activity
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to update user: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
                _saveUserButton.Enabled = true;
            }
        }

        private void OnResetPasswordButtonClick(object sender, EventArgs e)
        {
            // Create a dialog for resetting password
            var dialogView = Android.Views.LayoutInflater.From(this).Inflate(Resource.Layout.dialog_reset_password, null);
            var passwordEditText = dialogView.FindViewById<EditText>(Resource.Id.passwordEditText);
            var confirmPasswordEditText = dialogView.FindViewById<EditText>(Resource.Id.confirmPasswordEditText);

            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle("Reset Password");
            alertDialog.SetView(dialogView);
            alertDialog.SetPositiveButton("Reset", async (senderAlert, args) =>
            {
                // Get input values
                string password = passwordEditText.Text;
                string confirmPassword = confirmPasswordEditText.Text;

                // Validate inputs
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    Toast.MakeText(this, "Please enter both password fields", ToastLength.Short).Show();
                    return;
                }

                // Check if passwords match
                if (password != confirmPassword)
                {
                    Toast.MakeText(this, "Passwords do not match", ToastLength.Short).Show();
                    return;
                }

                // Validate password requirements
                if (!IsValidPassword(password))
                {
                    Toast.MakeText(this, "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character", ToastLength.Long).Show();
                    return;
                }

                try
                {
                    // Show loading dialog
                    var progressDialog = new ProgressDialog(this);
                    progressDialog.SetMessage("Resetting password...");
                    progressDialog.SetCancelable(false);
                    progressDialog.Show();

                    try
                    {
                        // Create reset password request
                        var resetPasswordRequest = new AdminResetPasswordRequest
                        {
                            UserId = _userId,
                            NewPassword = password
                        };

                        // Call API to reset password
                        await ApiService.ResetUserPasswordAsync(resetPasswordRequest);

                        Toast.MakeText(this, "Password reset successfully", ToastLength.Short).Show();
                    }
                    finally
                    {
                        if (progressDialog.IsShowing)
                        {
                            progressDialog.Dismiss();
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    HandleAuthError();
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, $"Failed to reset password: {ex.Message}", ToastLength.Long).Show();
                }
            });
            alertDialog.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                // Do nothing
            });
            alertDialog.Show();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            Finish(); // Return to previous activity
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