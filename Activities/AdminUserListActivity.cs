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

            // Set up RecyclerView
            _userRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            // Set up event handlers
            _createUserButton.Click += OnCreateUserButtonClick;
            _searchView.QueryTextChange += OnSearchQueryTextChange;

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

                // Handle deletion restrictions for current user and admins
                bool isAdmin = user.Roles.Contains("Administrator");
                viewHolder.DeleteButton.Enabled = !isCurrentUser && !isAdmin;

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
            }

            public override int ItemCount => _filteredUsers.Count;

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

                public UserViewHolder(View itemView) : base(itemView)
                {
                    UsernameTextView = itemView.FindViewById<TextView>(Resource.Id.usernameTextView);
                    EmailTextView = itemView.FindViewById<TextView>(Resource.Id.emailTextView);
                    RolesTextView = itemView.FindViewById<TextView>(Resource.Id.rolesTextView);
                    QuizCountTextView = itemView.FindViewById<TextView>(Resource.Id.quizCountTextView);
                    EditButton = itemView.FindViewById<Button>(Resource.Id.editButton);
                    DeleteButton = itemView.FindViewById<Button>(Resource.Id.deleteButton);
                }
            }
        }
    }
}