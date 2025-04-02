using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Mobile.Models;
using Mobile.Services;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Android.Views;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Android.Support.V4.Content;
using AndroidX.Core.Content;

namespace Mobile.Activities
{
    [Activity(Label = "Manage Users")]
    public class AdminUserListActivity : BaseAuthenticatedActivity
    {
        private RecyclerView _userRecyclerView;
        private ProgressBar _loadingProgressBar;
        private TextView _emptyTextView;
        private Button _createUserButton;
        private SearchView _searchView;
        private List<UserProfile> _users;
        private AdminUserAdapter _adapter;
        private Button _downloadReportButton;
        private Button _bulkDeleteButton;
        private Button _selectAllButton;
        private Button _cancelSelectionButton;

        // Flag to indicate if we're in bulk selection mode
        private bool _isInSelectionMode = false;

        // Set of selected user IDs for bulk operations
        private HashSet<string> _selectedUserIds = new HashSet<string>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_admin_user_list);

            // Initialize UI elements
            _userRecyclerView = FindViewById<RecyclerView>(Resource.Id.userRecyclerView);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
            _emptyTextView = FindViewById<TextView>(Resource.Id.emptyTextView);
            _createUserButton = FindViewById<Button>(Resource.Id.createUserButton);
            _searchView = FindViewById<SearchView>(Resource.Id.searchView);
            _downloadReportButton = FindViewById<Button>(Resource.Id.downloadReportButton);
            _bulkDeleteButton = FindViewById<Button>(Resource.Id.bulkDeleteButton);
            _selectAllButton = FindViewById<Button>(Resource.Id.selectAllButton);
            _cancelSelectionButton = FindViewById<Button>(Resource.Id.cancelSelectionButton);

            // Set up RecyclerView
            _userRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            // Set up event handlers
            _createUserButton.Click += OnCreateUserButtonClick;
            _searchView.QueryTextChange += OnSearchQueryTextChange;

            if (_downloadReportButton != null)
            {
                _downloadReportButton.Click += OnDownloadReportButtonClick;
            }

            if (_bulkDeleteButton != null)
            {
                _bulkDeleteButton.Click += OnBulkDeleteButtonClick;
            }

            if (_selectAllButton != null)
            {
                _selectAllButton.Click += OnSelectAllButtonClick;
            }

            if (_cancelSelectionButton != null)
            {
                _cancelSelectionButton.Click += OnCancelSelectionButtonClick;
            }

            // Initialize selection mode UI state
            UpdateSelectionModeUI(false);

            // Load users
            LoadUsersAsync();
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

        protected override void OnResume()
        {
            base.OnResume();
            // Reload users when returning to this activity
            LoadUsersAsync();
        }

        private async void LoadUsersAsync()
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = ViewStates.Visible;
                _emptyTextView.Visibility = ViewStates.Gone;
                _userRecyclerView.Visibility = ViewStates.Gone;

                // Get users from API
                _users = await ApiService.GetUsersAsync();

                if (_users.Count == 0)
                {
                    // Show empty message
                    _emptyTextView.Visibility = ViewStates.Visible;
                    _userRecyclerView.Visibility = ViewStates.Gone;
                }
                else
                {
                    // Setup and show RecyclerView
                    _adapter = new AdminUserAdapter(this, _users);
                    _userRecyclerView.SetAdapter(_adapter);

                    _emptyTextView.Visibility = ViewStates.Gone;
                    _userRecyclerView.Visibility = ViewStates.Visible;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle authentication errors
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load users: {ex.Message}", ToastLength.Long).Show();
                _emptyTextView.Text = "Error loading users. Please try again.";
                _emptyTextView.Visibility = ViewStates.Visible;
                _userRecyclerView.Visibility = ViewStates.Gone;
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = ViewStates.Gone;
            }
        }

        // Toggle selection mode
        private void EnterSelectionMode()
        {
            _isInSelectionMode = true;
            _selectedUserIds.Clear();
            UpdateSelectionModeUI(true);
            _adapter?.NotifyDataSetChanged();
        }

        private void ExitSelectionMode()
        {
            _isInSelectionMode = false;
            _selectedUserIds.Clear();
            UpdateSelectionModeUI(false);
            _adapter?.NotifyDataSetChanged();
        }

        // Update UI based on selection mode
        private void UpdateSelectionModeUI(bool selectionMode)
        {
            if (_bulkDeleteButton != null)
            {
                _bulkDeleteButton.Visibility = selectionMode ? ViewStates.Visible : ViewStates.Gone;
            }

            if (_selectAllButton != null)
            {
                _selectAllButton.Visibility = selectionMode ? ViewStates.Visible : ViewStates.Gone;
            }

            if (_cancelSelectionButton != null)
            {
                _cancelSelectionButton.Visibility = selectionMode ? ViewStates.Visible : ViewStates.Gone;
            }

            if (_createUserButton != null)
            {
                _createUserButton.Visibility = selectionMode ? ViewStates.Gone : ViewStates.Visible;
            }

            // Update action bar title
            if (selectionMode)
            {
                Title = $"Select Users ({_selectedUserIds.Count})";
            }
            else
            {
                Title = "Manage Users";
            }
        }

        // Toggle user selection and update UI
        public void ToggleUserSelection(string userId)
        {
            if (_selectedUserIds.Contains(userId))
            {
                _selectedUserIds.Remove(userId);
            }
            else
            {
                _selectedUserIds.Add(userId);
            }

            // Update title to show selected count
            Title = $"Select Users ({_selectedUserIds.Count})";

            // Update bulk delete button state
            if (_bulkDeleteButton != null)
            {
                _bulkDeleteButton.Enabled = _selectedUserIds.Count > 0;
                _bulkDeleteButton.Alpha = _selectedUserIds.Count > 0 ? 1.0f : 0.5f;
            }
        }

        private void OnCreateUserButtonClick(object sender, EventArgs e)
        {
            // Navigate to create user activity
            var intent = new Intent(this, typeof(AdminCreateUserActivity));
            StartActivity(intent);
        }

        private void OnSearchQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            // Filter users by name or email
            if (_adapter != null)
            {
                _adapter.Filter(e.NewText);
            }
        }

        private void OnSelectAllButtonClick(object sender, EventArgs e)
        {
            // Reset selection
            _selectedUserIds.Clear();

            // Get all valid user IDs (non-admin, non-self)
            string currentUserId = TokenManager.GetUserId(this);

            foreach (var user in _adapter.GetFilteredUsers())
            {
                bool isAdmin = user.Roles.Contains("Administrator");
                bool isSelf = user.Id == currentUserId;

                // Skip admins and self
                if (!isAdmin && !isSelf)
                {
                    _selectedUserIds.Add(user.Id);
                }
            }

            // Update UI
            Title = $"Select Users ({_selectedUserIds.Count})";

            // Update bulk delete button state
            if (_bulkDeleteButton != null)
            {
                _bulkDeleteButton.Enabled = _selectedUserIds.Count > 0;
                _bulkDeleteButton.Alpha = _selectedUserIds.Count > 0 ? 1.0f : 0.5f;
            }

            _adapter?.NotifyDataSetChanged();
        }

        private void OnCancelSelectionButtonClick(object sender, EventArgs e)
        {
            // Exit selection mode
            ExitSelectionMode();
        }

        private async void OnBulkDeleteButtonClick(object sender, EventArgs e)
        {
            if (_selectedUserIds.Count == 0)
            {
                Toast.MakeText(this, "No users selected", ToastLength.Short).Show();
                return;
            }

            // Confirm deletion
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Bulk Delete Users");
            builder.SetMessage($"Are you sure you want to delete {_selectedUserIds.Count} users? This action cannot be undone.");
            builder.SetPositiveButton("Delete", async (dialog, which) =>
            {
                await ExecuteBulkDeleteAsync();
            });
            builder.SetNegativeButton("Cancel", (dialog, which) => { });
            builder.Show();
        }

        private async Task ExecuteBulkDeleteAsync()
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = ViewStates.Visible;
                _bulkDeleteButton.Enabled = false;

                // Call API to bulk delete users
                var result = await ApiService.BulkDeleteUsersAsync(_selectedUserIds.ToList());

                // Show result
                string message = $"Successfully deleted {result.SuccessCount} users";
                if (result.ErrorCount > 0)
                {
                    message += $", {result.ErrorCount} failed";
                }
                Toast.MakeText(this, message, ToastLength.Long).Show();

                // Exit selection mode
                ExitSelectionMode();

                // Reload users list
                await Task.Delay(500);  // Short delay to ensure backend syncs
                LoadUsersAsync();
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to delete users: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = ViewStates.Gone;
                if (_bulkDeleteButton != null)
                {
                    _bulkDeleteButton.Enabled = true;
                }
            }
        }

        private async void OnDownloadReportButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = ViewStates.Visible;
                _downloadReportButton.Enabled = false;

                // Download report
                byte[] reportData = await ApiService.DownloadUserReportAsync();

                // Create filename with timestamp
                string fileName = $"user-report-{DateTime.Now:yyyyMMdd-HHmmss}.csv";

                // Save to file in Downloads folder
                string filePath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryDownloads).AbsolutePath, fileName);

                File.WriteAllBytes(filePath, reportData);

                // Create a File object for the saved report
                Java.IO.File reportFile = new Java.IO.File(filePath);

                // Create a content URI using FileProvider to securely share the file
                string authority = Application.Context.PackageName + ".fileprovider";
                Android.Net.Uri fileUri = null;

                try
                {
                    fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(this, authority, reportFile);
                }
                catch (Java.Lang.IllegalArgumentException ex)
                {
                    Console.WriteLine($"Error getting URI for file: {ex.Message}");
                    // Fallback to direct file URI - less secure but might work in some cases
                    fileUri = Android.Net.Uri.FromFile(reportFile);
                }

                if (fileUri != null)
                {
                    // First try to find apps that can handle CSV files
                    Intent viewIntent = new Intent(Intent.ActionView);
                    viewIntent.SetDataAndType(fileUri, "text/csv");
                    viewIntent.SetFlags(ActivityFlags.GrantReadUriPermission);

                    // Check if there's an app that can handle CSV files
                    if (viewIntent.ResolveActivity(PackageManager) != null)
                    {
                        // Start the activity to let the user choose which app to open the CSV with
                        StartActivity(Intent.CreateChooser(viewIntent, "Open CSV with..."));
                    }
                    else
                    {
                        // No CSV handler found, try generic text viewing
                        viewIntent.SetDataAndType(fileUri, "text/plain");

                        if (viewIntent.ResolveActivity(PackageManager) != null)
                        {
                            StartActivity(Intent.CreateChooser(viewIntent, "Open Report with..."));
                        }
                        else
                        {
                            // No suitable app found, show in our own viewer
                            ShowReportInAppViewer(reportData, fileName);
                        }
                    }
                }
                else
                {
                    // If URI creation failed, show in our own viewer
                    ShowReportInAppViewer(reportData, fileName);
                }

                Toast.MakeText(this, $"Report downloaded to Downloads/{fileName}", ToastLength.Long).Show();
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading report: {ex}");
                Toast.MakeText(this, $"Failed to download report: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = ViewStates.Gone;
                _downloadReportButton.Enabled = true;
            }
        }

        private void ShowReportInAppViewer(byte[] reportData, string fileName)
        {
            // Create an intent to show the report in our own viewer activity
            Intent viewerIntent = new Intent(this, typeof(ReportViewerActivity));

            // Save the data to a temporary file
            string tempPath = Path.Combine(CacheDir.AbsolutePath, fileName);
            File.WriteAllBytes(tempPath, reportData);

            // Pass the file path to the viewer activity
            viewerIntent.PutExtra("FilePath", tempPath);
            viewerIntent.PutExtra("FileName", fileName);

            StartActivity(viewerIntent);
        }

        private async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                // Show loading dialog
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Deleting user...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    // Call API to delete user
                    await ApiService.DeleteUserAsync(userId);
                    return true;
                }
                finally
                {
                    // Hide loading dialog
                    if (progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
                return false;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to delete user: {ex.Message}", ToastLength.Long).Show();
                return false;
            }
        }

        private void HandleAuthError()
        {
            TokenManager.ClearToken(this);
            Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
            RedirectToLogin();
        }

        // Handle long press on user list item
        public void OnUserItemLongClick()
        {
            if (!_isInSelectionMode)
            {
                EnterSelectionMode();
            }
        }

        // RecyclerView adapter for users
        private class AdminUserAdapter : RecyclerView.Adapter
        {
            private readonly AdminUserListActivity _activity;
            private readonly List<UserProfile> _allUsers;
            private List<UserProfile> _filteredUsers;

            public AdminUserAdapter(AdminUserListActivity activity, List<UserProfile> users)
            {
                _activity = activity;
                _allUsers = users;
                _filteredUsers = new List<UserProfile>(users);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_admin_user_list_item, parent, false);
                return new UserViewHolder(itemView);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                UserViewHolder viewHolder = holder as UserViewHolder;
                UserProfile user = _filteredUsers[position];

                // Set user details
                viewHolder.UsernameTextView.Text = user.UserName;
                viewHolder.EmailTextView.Text = user.Email;
                viewHolder.RolesTextView.Text = string.Join(", ", user.Roles);
                viewHolder.QuizCountTextView.Text = $"{user.QuizzesTaken} quizzes taken";

                // Determine if current user
                string currentUserId = TokenManager.GetUserId(_activity);
                bool isCurrentUser = user.Id == currentUserId;

                // Set selection checkbox state
                if (viewHolder.SelectCheckBox != null)
                {
                    viewHolder.SelectCheckBox.Visibility = _activity._isInSelectionMode ? ViewStates.Visible : ViewStates.Gone;
                    viewHolder.SelectCheckBox.Checked = _activity._selectedUserIds.Contains(user.Id);

                    // Disable checkbox for admins and self
                    viewHolder.SelectCheckBox.Enabled = !(user.Roles.Contains("Administrator") || isCurrentUser);
                }

                // Handle deletion restrictions for current user and admins
                bool isAdmin = user.Roles.Contains("Administrator");
                viewHolder.DeleteButton.Enabled = !isCurrentUser && !isAdmin;
                viewHolder.DeleteButton.Visibility = _activity._isInSelectionMode ? ViewStates.Gone : ViewStates.Visible;

                // Set button visibility based on selection mode
                viewHolder.EditButton.Visibility = _activity._isInSelectionMode ? ViewStates.Gone : ViewStates.Visible;

                if (!viewHolder.DeleteButton.Enabled)
                {
                    viewHolder.DeleteButton.Alpha = 0.5f;
                }
                else
                {
                    viewHolder.DeleteButton.Alpha = 1.0f;
                }

                // Set up event handlers for buttons
                viewHolder.EditButton.Click += (sender, e) =>
                {
                    // Navigate to edit user activity
                    var intent = new Intent(_activity, typeof(AdminEditUserActivity));
                    intent.PutExtra("UserId", user.Id);
                    _activity.StartActivity(intent);
                };

                viewHolder.DeleteButton.Click += async (sender, e) =>
                {
                    if (isCurrentUser)
                    {
                        Toast.MakeText(_activity, "You cannot delete your own account", ToastLength.Short).Show();
                        return;
                    }

                    if (isAdmin)
                    {
                        Toast.MakeText(_activity, "Admin users cannot be deleted from the mobile app", ToastLength.Short).Show();
                        return;
                    }

                    // Show confirmation dialog
                    var alertDialog = new AlertDialog.Builder(_activity);
                    alertDialog.SetTitle("Delete User");
                    alertDialog.SetMessage($"Are you sure you want to delete user '{user.UserName}'? This action cannot be undone.");
                    alertDialog.SetPositiveButton("Delete", async (senderAlert, args) =>
                    {
                        // Delete user
                        bool success = await _activity.DeleteUserAsync(user.Id);
                        if (success)
                        {
                            // Remove from lists and update UI
                            int adapterPosition = viewHolder.AdapterPosition;
                            if (adapterPosition != RecyclerView.NoPosition)
                            {
                                UserProfile userToRemove = _filteredUsers[adapterPosition];
                                _filteredUsers.RemoveAt(adapterPosition);
                                _allUsers.Remove(userToRemove);
                                NotifyItemRemoved(adapterPosition);

                                // Show empty message if no more users
                                if (_filteredUsers.Count == 0)
                                {
                                    _activity._emptyTextView.Visibility = ViewStates.Visible;
                                    _activity._userRecyclerView.Visibility = ViewStates.Gone;
                                }

                                Toast.MakeText(_activity, "User deleted successfully", ToastLength.Short).Show();
                            }
                        }
                    });
                    alertDialog.SetNegativeButton("Cancel", (senderAlert, args) =>
                    {
                        // Do nothing
                    });
                    alertDialog.Show();
                };

                // Set up click and long click handlers for the item view
                viewHolder.ItemView.Click += (sender, e) =>
                {
                    if (_activity._isInSelectionMode)
                    {
                        // Toggle selection if user is not admin or self
                        bool isAdmin = user.Roles.Contains("Administrator");
                        bool isCurrentUser = user.Id == currentUserId;

                        if (!isAdmin && !isCurrentUser)
                        {
                            _activity.ToggleUserSelection(user.Id);
                            NotifyItemChanged(position);
                        }
                        else if (isAdmin)
                        {
                            Toast.MakeText(_activity, "Admin users cannot be selected for bulk operations", ToastLength.Short).Show();
                        }
                        else if (isCurrentUser)
                        {
                            Toast.MakeText(_activity, "You cannot select your own account", ToastLength.Short).Show();
                        }
                    }
                };

                viewHolder.ItemView.LongClick += (sender, e) =>
                {
                    _activity.OnUserItemLongClick();

                    // If the user is not admin or self, select it on entering selection mode
                    bool isAdmin = user.Roles.Contains("Administrator");
                    bool isCurrentUser = user.Id == currentUserId;

                    if (!isAdmin && !isCurrentUser)
                    {
                        _activity.ToggleUserSelection(user.Id);
                        NotifyItemChanged(position);
                    }
                };

                // Set up checkbox click handler
                if (viewHolder.SelectCheckBox != null)
                {
                    viewHolder.SelectCheckBox.Click += (sender, e) =>
                    {
                        _activity.ToggleUserSelection(user.Id);
                        // No need to call NotifyItemChanged here as the checkbox state is already updated
                    };
                }
            }

            public override int ItemCount => _filteredUsers.Count;

            // Return the filtered users list for Select All feature
            public List<UserProfile> GetFilteredUsers()
            {
                return _filteredUsers;
            }

            public void Filter(string query)
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    // If query is empty, show all users
                    _filteredUsers = new List<UserProfile>(_allUsers);
                }
                else
                {
                    // Filter users by username or email containing the query (case-insensitive)
                    string lowerQuery = query.ToLower();
                    _filteredUsers = _allUsers
                        .Where(u => u.UserName.ToLower().Contains(lowerQuery) ||
                                   u.Email.ToLower().Contains(lowerQuery))
                        .ToList();
                }

                // Update UI based on filter results
                if (_filteredUsers.Count == 0 && _allUsers.Count > 0)
                {
                    _activity._emptyTextView.Text = "No users match your search.";
                    _activity._emptyTextView.Visibility = ViewStates.Visible;
                }
                else
                {
                    _activity._emptyTextView.Visibility = ViewStates.Gone;
                }

                NotifyDataSetChanged();
            }

            // ViewHolder for user items
            private class UserViewHolder : RecyclerView.ViewHolder
            {
                public TextView UsernameTextView { get; }
                public TextView EmailTextView { get; }
                public TextView RolesTextView { get; }
                public TextView QuizCountTextView { get; }
                public Button EditButton { get; }
                public Button DeleteButton { get; }
                public CheckBox SelectCheckBox { get; }

                public UserViewHolder(View itemView) : base(itemView)
                {
                    UsernameTextView = itemView.FindViewById<TextView>(Resource.Id.usernameTextView);
                    EmailTextView = itemView.FindViewById<TextView>(Resource.Id.emailTextView);
                    RolesTextView = itemView.FindViewById<TextView>(Resource.Id.rolesTextView);
                    QuizCountTextView = itemView.FindViewById<TextView>(Resource.Id.quizCountTextView);
                    EditButton = itemView.FindViewById<Button>(Resource.Id.editButton);
                    DeleteButton = itemView.FindViewById<Button>(Resource.Id.deleteButton);
                    SelectCheckBox = itemView.FindViewById<CheckBox>(Resource.Id.selectCheckBox);
                }
            }
        }
    }
}