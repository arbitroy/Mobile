using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Mobile.Models;
using Mobile.Services;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mobile.Activities
{
    [Activity(Label = "Edit User")]
    public class AdminEditUserActivity : BaseAuthenticatedActivity
    {
        private TextView _emailTextView;
        private EditText _usernameEditText;
        private CheckBox _adminRoleCheckBox;
        private CheckBox _setNewPasswordCheckBox;
        private LinearLayout _passwordLayout;
        private EditText _newPasswordEditText;
        private Button _saveUserButton;
        private Button _resetPasswordButton;
        private Button _cancelButton;
        private ProgressBar _loadingProgressBar;
        private TextView _noteTextView;

        private string _userId;
        private UserProfile _userProfile;
        private bool _isCurrentUser;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
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
                InitializeUIElements();

                // Determine if editing current user
                string currentUserId = TokenManager.GetUserId(this);
                _isCurrentUser = _userId == currentUserId;

                // Set up event handlers
                SetupEventHandlers();

                // Load user details
                LoadUserAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in OnCreate: {ex.Message}");
                Toast.MakeText(this, "Error initializing edit user page: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private void InitializeUIElements()
        {
            _emailTextView = FindViewById<TextView>(Resource.Id.emailTextView);
            _usernameEditText = FindViewById<EditText>(Resource.Id.usernameEditText);
            _adminRoleCheckBox = FindViewById<CheckBox>(Resource.Id.adminRoleCheckBox);
            _setNewPasswordCheckBox = FindViewById<CheckBox>(Resource.Id.setNewPasswordCheckBox);
            _passwordLayout = FindViewById<LinearLayout>(Resource.Id.passwordLayout);
            _newPasswordEditText = FindViewById<EditText>(Resource.Id.newPasswordEditText);
            _saveUserButton = FindViewById<Button>(Resource.Id.saveUserButton);
            _resetPasswordButton = FindViewById<Button>(Resource.Id.resetPasswordButton);
            _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
            _noteTextView = FindViewById<TextView>(Resource.Id.noteTextView);

            // Initially hide password field
            if (_passwordLayout != null)
            {
                _passwordLayout.Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private void SetupEventHandlers()
        {
            if (_saveUserButton != null)
                _saveUserButton.Click += OnSaveUserButtonClick;

            if (_resetPasswordButton != null)
                _resetPasswordButton.Click += OnResetPasswordButtonClick;

            if (_cancelButton != null)
                _cancelButton.Click += OnCancelButtonClick;

            if (_setNewPasswordCheckBox != null)
                _setNewPasswordCheckBox.CheckedChange += OnSetNewPasswordCheckedChange;
        }

        private void OnSetNewPasswordCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (_passwordLayout != null)
            {
                _passwordLayout.Visibility = e.IsChecked ? Android.Views.ViewStates.Visible : Android.Views.ViewStates.Gone;
            }
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
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                }

                // Get user details from API
                _userProfile = await ApiService.GetUserByIdAsync(_userId);

                // Update UI with user details
                UpdateUIWithUserProfile();

                // If editing current user, disable admin role checkbox (prevent removing own admin rights)
                if (_isCurrentUser && _adminRoleCheckBox != null)
                {
                    _adminRoleCheckBox.Enabled = false;
                    _adminRoleCheckBox.Alpha = 0.5f;

                    // Add note
                    if (_noteTextView != null)
                    {
                        _noteTextView.Text = "Note: You cannot modify your own admin status.";
                        _noteTextView.Visibility = Android.Views.ViewStates.Visible;
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
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
                }
            }
        }

        private void UpdateUIWithUserProfile()
        {
            if (_userProfile == null)
            {
                Toast.MakeText(this, "Failed to load user profile data", ToastLength.Short).Show();
                return;
            }

            if (_emailTextView != null)
            {
                _emailTextView.Text = _userProfile.Email ?? "No email";
            }

            if (_usernameEditText != null)
            {
                _usernameEditText.Text = _userProfile.UserName ?? "";
            }

            if (_adminRoleCheckBox != null)
            {
                bool isAdmin = _userProfile.Roles?.Contains("Administrator") ?? false;
                _adminRoleCheckBox.Checked = isAdmin;
            }
        }

        private async void OnSaveUserButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get input values
                string username = _usernameEditText?.Text?.Trim();
                bool isAdmin = _adminRoleCheckBox?.Checked ?? false;
                bool setNewPassword = _setNewPasswordCheckBox?.Checked ?? false;
                string newPassword = _newPasswordEditText?.Text;

                // Validate inputs
                if (string.IsNullOrEmpty(username))
                {
                    Toast.MakeText(this, "Username is required", ToastLength.Short).Show();
                    return;
                }

                if (setNewPassword && string.IsNullOrEmpty(newPassword))
                {
                    Toast.MakeText(this, "New password is required", ToastLength.Short).Show();
                    return;
                }

                if (setNewPassword && !IsValidPassword(newPassword))
                {
                    Toast.MakeText(this, "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character", ToastLength.Long).Show();
                    return;
                }

                // Show loading indicator
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                }

                if (_saveUserButton != null)
                {
                    _saveUserButton.Enabled = false;
                }

                // Create update request
                var updateRequest = new UpdateUserFullRequest
                {
                    Id = _userId,
                    UserName = username,
                    Email = _userProfile.Email, // Keep the same email
                    IsAdmin = isAdmin,
                    SetNewPassword = setNewPassword,
                    NewPassword = setNewPassword ? newPassword : null
                };

                // Call API to update user
                await ApiService.UpdateUserFullAsync(_userId, updateRequest);

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
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
                }

                if (_saveUserButton != null)
                {
                    _saveUserButton.Enabled = true;
                }
            }
        }

        private async void OnResetPasswordButtonClick(object sender, EventArgs e)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Reset Password");
            alert.SetMessage("This will generate a new temporary password for the user. Are you sure you want to continue?");
            alert.SetPositiveButton("Reset", async (senderAlert, args) => {
                await ResetPasswordAsync();
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args) => {
                // Do nothing
            });
            alert.Show();
        }

        private async Task ResetPasswordAsync()
        {
            try
            {
                // Show loading indicator
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                }

                if (_resetPasswordButton != null)
                {
                    _resetPasswordButton.Enabled = false;
                }

                // Call API to reset password
                await ApiService.ResetUserPasswordAsync(_userId);

                Toast.MakeText(this, "Password has been reset successfully", ToastLength.Long).Show();
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to reset password: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Gone;
                }

                if (_resetPasswordButton != null)
                {
                    _resetPasswordButton.Enabled = true;
                }
            }
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