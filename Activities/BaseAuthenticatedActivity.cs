using Android.App;
using Android.OS;
using Android.Content;
using Mobile.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Widget;

namespace Mobile.Activities
{
    public abstract class BaseAuthenticatedActivity : Activity
    {
        protected ApiService ApiService;
        protected bool IsAdmin = false;
        protected List<string> UserRoles = new List<string>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Initialize shared services
            ApiService = new ApiService(this);

            // Validate authentication
            if (string.IsNullOrEmpty(TokenManager.GetToken(this)))
            {
                // Not authenticated, redirect to login
                RedirectToLogin();
                return;
            }

            // Check user roles asynchronously
            CheckUserRolesAsync();
        }

        protected async void CheckUserRolesAsync()
        {
            try
            {
                var userProfile = await ApiService.GetUserProfileAsync();
                UserRoles = userProfile.Roles;
                IsAdmin = UserRoles.Contains("Administrator");

                // Hook for derived classes to handle role checking
                OnRolesLoaded();
            }
            catch (UnauthorizedAccessException)
            {
                // Invalid or expired token
                TokenManager.ClearToken(this);
                RedirectToLogin();
            }
            catch (Exception ex)
            {
                // Don't fail completely on role check error
                Console.WriteLine($"Error checking user roles: {ex.Message}");
                Toast.MakeText(this, "Could not verify user permissions", ToastLength.Short).Show();
            }
        }

        // Hook for derived classes to override for role-specific behavior
        protected virtual void OnRolesLoaded()
        {
            // Default implementation does nothing
        }

        protected void RedirectToLogin()
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
            StartActivity(intent);
            Finish();
        }

        protected void RedirectToAdminDashboard()
        {
            // This will be implemented once we create the admin dashboard
            // For now, just show a message
            Toast.MakeText(this, "Admin dashboard is not yet implemented", ToastLength.Short).Show();
        }

        protected bool EnsureAdminAccess()
        {
            if (!IsAdmin)
            {
                Toast.MakeText(this, "You need administrator privileges to access this feature", ToastLength.Long).Show();
                return false;
            }
            return true;
        }
    }
}