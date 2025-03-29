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
    [Activity(Label = "My Profile")]
    public class UserProfileActivity : BaseAuthenticatedActivity
    {
        private TextView _emailTextView;
        private EditText _usernameEditText;
        private Button _saveProfileButton;
        private EditText _currentPasswordEditText;
        private EditText _newPasswordEditText;
        private EditText _confirmNewPasswordEditText;
        private Button _changePasswordButton;
        private Button _logoutButton;
        private TextView _roleInfoTextView;

        private UserProfile _userProfile;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "user_profile" layout resource
            SetContentView(Resource.Layout.activity_user_profile);

            // Initialize UI elements
            _emailTextView = FindViewById<TextView>(Resource.Id.emailTextView);
            _usernameEditText = FindViewById<EditText>(Resource.Id.usernameEditText);
            _saveProfileButton = FindViewById<Button>(Resource.Id.saveProfileButton);
            _currentPasswordEditText = FindViewById<EditText>(Resource.Id.currentPasswordEditText);
            _newPasswordEditText = FindViewById<EditText>(Resource.Id.newPasswordEditText);
            _confirmNewPasswordEditText = FindViewById<EditText>(Resource.Id.confirmNewPasswordEditText);
            _changePasswordButton = FindViewById<Button>(Resource.Id.changePasswordButton);
            _logoutButton = FindViewById<Button>(Resource.Id.logoutButton);
            _roleInfoTextView = FindViewById<TextView>(Resource.Id.roleInfoTextView);

            // Set up event handlers
            _saveProfileButton.Click += OnSaveProfileButtonClick;
            _changePasswordButton.Click += OnChangePasswordButtonClick;
            _logoutButton.Click += OnLogoutButtonClick;

            // Load user profile data
            LoadUserProfileAsync();
        }

        private async void LoadUserProfileAsync()
        {
            try
            {
                // Show loading indicator
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Loading profile...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    // Get user profile from API
                    _userProfile = await ApiService.GetUserProfileAsync();

                    // Update UI with user profile data
                    _emailTextView.Text = _userProfile.Email;
                    _usernameEditText.Text = _userProfile.UserName;

                    // Update role info text
                    _roleInfoTextView.Text = IsAdmin ?
                        "Account Type: Administrator" :
                        "Account Type: Standard User";

                    // Make the role text more visible for admins
                    if (IsAdmin)
                    {
                        _roleInfoTextView.SetTextColor(Android.Graphics.Color.ParseColor("#4361ee"));
                        _roleInfoTextView.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                    }
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
                TokenManager.ClearToken(this);
                Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
                RedirectToLogin();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load profile: {ex.Message}", ToastLength.Long).Show();
                Finish();
            }
        }

        private async void OnSaveProfileButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get updated username
                string updatedUsername = _usernameEditText.Text?.Trim();

                // Validate username
                if (string.IsNullOrEmpty(updatedUsername))
                {
                    Toast.MakeText(this, "Username cannot be empty", ToastLength.Short).Show();
                    return;
                }

                // Show loading indicator
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Saving profile...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    // Create profile update request
                    var updateRequest = new ProfileUpdateRequest
                    {
                        UserName = updatedUsername,
                        Email = _userProfile.Email  // Include the current email
                    };

                    // Update profile via API
                    var updatedProfile = await ApiService.UpdateUserProfileAsync(updateRequest);

                    // Update local user profile data
                    _userProfile = updatedProfile;

                    // Update token storage with new username
                    TokenManager.SaveUsername(this, updatedProfile.UserName);

                    Toast.MakeText(this, "Profile updated successfully", ToastLength.Short).Show();
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
                TokenManager.ClearToken(this);
                Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
                RedirectToLogin();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to update profile: {ex.Message}", ToastLength.Long).Show();
            }
        }

        private async void OnChangePasswordButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get password values
                string currentPassword = _currentPasswordEditText.Text;
                string newPassword = _newPasswordEditText.Text;
                string confirmNewPassword = _confirmNewPasswordEditText.Text;

                // Validate passwords
                if (string.IsNullOrEmpty(currentPassword) ||
                    string.IsNullOrEmpty(newPassword) ||
                    string.IsNullOrEmpty(confirmNewPassword))
                {
                    Toast.MakeText(this, "All password fields are required", ToastLength.Short).Show();
                    return;
                }

                // Check if passwords match
                if (newPassword != confirmNewPassword)
                {
                    Toast.MakeText(this, "New passwords do not match", ToastLength.Short).Show();
                    return;
                }

                // Validate password requirements
                if (!IsValidPassword(newPassword))
                {
                    Toast.MakeText(this, "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character", ToastLength.Long).Show();
                    return;
                }

                // Show loading indicator
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Changing password...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    // Create password change request
                    var passwordChangeRequest = new PasswordChangeRequest
                    {
                        CurrentPassword = currentPassword,
                        NewPassword = newPassword,
                        ConfirmPassword = confirmNewPassword
                    };

                    // Change password via API
                    await ApiService.ChangePasswordAsync(passwordChangeRequest);

                    // Clear password fields
                    _currentPasswordEditText.Text = string.Empty;
                    _newPasswordEditText.Text = string.Empty;
                    _confirmNewPasswordEditText.Text = string.Empty;

                    Toast.MakeText(this, "Password changed successfully", ToastLength.Short).Show();
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
                TokenManager.ClearToken(this);
                Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
                RedirectToLogin();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to change password: {ex.Message}", ToastLength.Long).Show();
            }
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

        protected override void OnRolesLoaded()
        {
            // Update role info in UI when roles are loaded or changed
            RunOnUiThread(() => {
                _roleInfoTextView.Text = IsAdmin ?
                    "Account Type: Administrator" :
                    "Account Type: Standard User";

                if (IsAdmin)
                {
                    _roleInfoTextView.SetTextColor(Android.Graphics.Color.ParseColor("#4361ee"));
                    _roleInfoTextView.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                }
            });
        }

        private bool IsValidPassword(string password)
        {
            // Check minimum length
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            // Check for uppercase letter
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for lowercase letter
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for digit
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
                return false;

            // Check for special character
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return false;

            return true;
        }
    }
}