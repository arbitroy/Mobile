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
                Console.WriteLine("AdminEditUser: OnCreate started");

                // Set our view from the layout resource
                SetContentView(Resource.Layout.activity_admin_edit_user);
                Console.WriteLine("AdminEditUser: SetContentView completed");

                // Get user ID from intent
                _userId = Intent.GetStringExtra("UserId");
                if (string.IsNullOrEmpty(_userId))
                {
                    Console.WriteLine("AdminEditUser: Invalid user ID");
                    Toast.MakeText(this, "Invalid user ID", ToastLength.Short).Show();
                    Finish();
                    return;
                }

                Console.WriteLine($"AdminEditUser: Edit user ID: {_userId}");

                // Initialize UI elements
                InitializeUIElements();

                // Determine if editing current user
                string currentUserId = TokenManager.GetUserId(this);
                _isCurrentUser = _userId == currentUserId;
                Console.WriteLine($"AdminEditUser: Is current user: {_isCurrentUser}, Current user ID: {currentUserId}");

                // Set up event handlers
                SetupEventHandlers();

                // Load user details
                LoadUserAsync();

                Console.WriteLine("AdminEditUser: OnCreate completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in OnCreate: {ex.Message}");
                Console.WriteLine($"AdminEditUser: Stack trace: {ex.StackTrace}");
                Toast.MakeText(this, "Error initializing edit user page: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private void InitializeUIElements()
        {
            Console.WriteLine("AdminEditUser: InitializeUIElements started");

            try
            {
                _emailTextView = FindViewById<TextView>(Resource.Id.emailTextView);
                _usernameEditText = FindViewById<EditText>(Resource.Id.usernameEditText);
                _adminRoleCheckBox = FindViewById<CheckBox>(Resource.Id.adminRoleCheckBox);
                _saveUserButton = FindViewById<Button>(Resource.Id.saveUserButton);
                _resetPasswordButton = FindViewById<Button>(Resource.Id.resetPasswordButton);
                _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
                _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
                _noteTextView = FindViewById<TextView>(Resource.Id.noteTextView);

                Console.WriteLine("AdminEditUser: UI elements initialized");

                // Verify UI elements were found
                if (_emailTextView == null) Console.WriteLine("AdminEditUser: emailTextView is null");
                if (_usernameEditText == null) Console.WriteLine("AdminEditUser: usernameEditText is null");
                if (_adminRoleCheckBox == null) Console.WriteLine("AdminEditUser: adminRoleCheckBox is null");
                if (_saveUserButton == null) Console.WriteLine("AdminEditUser: saveUserButton is null");
                if (_resetPasswordButton == null) Console.WriteLine("AdminEditUser: resetPasswordButton is null");
                if (_cancelButton == null) Console.WriteLine("AdminEditUser: cancelButton is null");
                if (_loadingProgressBar == null) Console.WriteLine("AdminEditUser: loadingProgressBar is null");
                if (_noteTextView == null) Console.WriteLine("AdminEditUser: noteTextView is null");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in InitializeUIElements: {ex.Message}");
                throw;
            }
        }

        private void SetupEventHandlers()
        {
            Console.WriteLine("AdminEditUser: SetupEventHandlers started");

            try
            {
                if (_saveUserButton != null)
                    _saveUserButton.Click += OnSaveUserButtonClick;

                if (_resetPasswordButton != null)
                    _resetPasswordButton.Click += OnResetPasswordButtonClick;

                if (_cancelButton != null)
                    _cancelButton.Click += OnCancelButtonClick;

                Console.WriteLine("AdminEditUser: Event handlers set up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in SetupEventHandlers: {ex.Message}");
                throw;
            }
        }

        protected override void OnRolesLoaded()
        {
            try
            {
                Console.WriteLine("AdminEditUser: OnRolesLoaded called");

                // Ensure user has admin access
                if (!IsAdmin)
                {
                    Console.WriteLine("AdminEditUser: User does not have admin role");
                    Toast.MakeText(this, "You need administrator privileges to access this page", ToastLength.Long).Show();
                    Finish();
                }
                else
                {
                    Console.WriteLine("AdminEditUser: User has admin role confirmed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in OnRolesLoaded: {ex.Message}");
            }
        }

        private async void LoadUserAsync()
        {
            try
            {
                Console.WriteLine("AdminEditUser: LoadUserAsync started");

                // Show loading indicator
                if (_loadingProgressBar != null)
                {
                    _loadingProgressBar.Visibility = Android.Views.ViewStates.Visible;
                }

                // Get user details from API
                Console.WriteLine($"AdminEditUser: Fetching user details for ID: {_userId}");
                _userProfile = await ApiService.GetUserByIdAsync(_userId);
                Console.WriteLine($"AdminEditUser: User details fetched. Email: {_userProfile?.Email}, Username: {_userProfile?.UserName}");

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

                    Console.WriteLine("AdminEditUser: Disabled admin checkbox for current user");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"AdminEditUser: UnauthorizedAccessException in LoadUserAsync: {ex.Message}");
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in LoadUserAsync: {ex.Message}");
                Console.WriteLine($"AdminEditUser: Stack trace: {ex.StackTrace}");
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

                Console.WriteLine("AdminEditUser: LoadUserAsync completed");
            }
        }

        private void UpdateUIWithUserProfile()
        {
            try
            {
                Console.WriteLine("AdminEditUser: UpdateUIWithUserProfile started");

                if (_userProfile == null)
                {
                    Console.WriteLine("AdminEditUser: UserProfile is null");
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
                    Console.WriteLine($"AdminEditUser: Is admin role checked: {isAdmin}");
                }

                Console.WriteLine("AdminEditUser: UI updated with user profile data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in UpdateUIWithUserProfile: {ex.Message}");
                Toast.MakeText(this, $"Error updating UI: {ex.Message}", ToastLength.Short).Show();
            }
        }

        private async void OnSaveUserButtonClick(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("AdminEditUser: OnSaveUserButtonClick started");

                // Get input values
                string username = _usernameEditText?.Text?.Trim();
                bool isAdmin = _adminRoleCheckBox?.Checked ?? false;

                Console.WriteLine($"AdminEditUser: Saving user - Username: {username}, IsAdmin: {isAdmin}");

                // Validate inputs
                if (string.IsNullOrEmpty(username))
                {
                    Toast.MakeText(this, "Username is required", ToastLength.Short).Show();
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

                Console.WriteLine($"AdminEditUser: Calling API to update user {_userId}");

                // Call API to update user
                await ApiService.UpdateUserAsync(_userId, updateUserRequest);

                // If editing current user, update stored username
                if (_isCurrentUser)
                {
                    TokenManager.SaveUsername(this, username);
                    Console.WriteLine($"AdminEditUser: Updated token username to {username}");
                }

                Toast.MakeText(this, "User updated successfully", ToastLength.Short).Show();
                Console.WriteLine("AdminEditUser: User updated successfully");

                Finish(); // Return to previous activity
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("AdminEditUser: UnauthorizedAccessException caught");
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in OnSaveUserButtonClick: {ex.Message}");
                Console.WriteLine($"AdminEditUser: Stack trace: {ex.StackTrace}");
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

                Console.WriteLine("AdminEditUser: OnSaveUserButtonClick completed");
            }
        }

        private void OnResetPasswordButtonClick(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("AdminEditUser: OnResetPasswordButtonClick started");

                // Create a dialog for resetting password
                var dialogView = Android.Views.LayoutInflater.From(this).Inflate(Resource.Layout.dialog_reset_password, null);
                var passwordEditText = dialogView.FindViewById<EditText>(Resource.Id.passwordEditText);
                var confirmPasswordEditText = dialogView.FindViewById<EditText>(Resource.Id.confirmPasswordEditText);

                // Verify dialog views were found
                if (passwordEditText == null) Console.WriteLine("AdminEditUser: Dialog passwordEditText is null");
                if (confirmPasswordEditText == null) Console.WriteLine("AdminEditUser: Dialog confirmPasswordEditText is null");

                var alertDialog = new AlertDialog.Builder(this);
                alertDialog.SetTitle("Reset Password");
                alertDialog.SetView(dialogView);
                alertDialog.SetPositiveButton("Reset", async (senderAlert, args) =>
                {
                    // Get input values
                    string password = passwordEditText?.Text;
                    string confirmPassword = confirmPasswordEditText?.Text;

                    Console.WriteLine("AdminEditUser: Reset password dialog OK clicked");

                    if (ProcessPasswordReset(password, confirmPassword))
                    {
                        await ResetUserPasswordAsync(password);
                    }
                });
                alertDialog.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    Console.WriteLine("AdminEditUser: Reset password dialog cancelled");
                });

                Console.WriteLine("AdminEditUser: Showing reset password dialog");
                alertDialog.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in OnResetPasswordButtonClick: {ex.Message}");
                Console.WriteLine($"AdminEditUser: Stack trace: {ex.StackTrace}");
                Toast.MakeText(this, $"Error showing password reset dialog: {ex.Message}", ToastLength.Short).Show();
            }
        }

        private bool ProcessPasswordReset(string password, string confirmPassword)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    Toast.MakeText(this, "Please enter both password fields", ToastLength.Short).Show();
                    return false;
                }

                // Check if passwords match
                if (password != confirmPassword)
                {
                    Toast.MakeText(this, "Passwords do not match", ToastLength.Short).Show();
                    return false;
                }

                // Validate password requirements
                if (!IsValidPassword(password))
                {
                    Toast.MakeText(this, "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character", ToastLength.Long).Show();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in ProcessPasswordReset: {ex.Message}");
                return false;
            }
        }

        private async Task ResetUserPasswordAsync(string password)
        {
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

                    Console.WriteLine($"AdminEditUser: Resetting password for user {_userId}");

                    // Call API to reset password
                    await ApiService.ResetUserPasswordAsync(resetPasswordRequest);

                    Toast.MakeText(this, "Password reset successfully", ToastLength.Short).Show();
                    Console.WriteLine("AdminEditUser: Password reset successfully");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"AdminEditUser: UnauthorizedAccessException during password reset: {ex.Message}");
                    HandleAuthError();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AdminEditUser: Exception during password reset: {ex.Message}");
                    Console.WriteLine($"AdminEditUser: Stack trace: {ex.StackTrace}");
                    Toast.MakeText(this, $"Failed to reset password: {ex.Message}", ToastLength.Long).Show();
                }
                finally
                {
                    if (progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in ResetUserPasswordAsync: {ex.Message}");
                Toast.MakeText(this, $"Error during password reset: {ex.Message}", ToastLength.Short).Show();
            }
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("AdminEditUser: OnCancelButtonClick - finishing activity");
                Finish(); // Return to previous activity
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminEditUser: Exception in OnCancelButtonClick: {ex.Message}");
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
                Console.WriteLine($"AdminEditUser: Exception in IsValidPassword: {ex.Message}");
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
                Console.WriteLine($"AdminEditUser: Exception in HandleAuthError: {ex.Message}");
            }
        }
    }
}