using Android.Content;
using System;

namespace Mobile.Services
{
    public static class TokenManager
    {
        private const string PREFERENCES_NAME = "QuizAppPrefs";
        private const string TOKEN_KEY = "auth_token";
        private const string USERNAME_KEY = "username";
        private const string USER_ID_KEY = "user_id";

        public static void SaveToken(Context context, string token, string username = null, string userId = null)
        {
            if (context == null)
            {
                Console.WriteLine("Cannot save token: Context is null");
                return;
            }

            var prefs = context.GetSharedPreferences(PREFERENCES_NAME, FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.PutString(TOKEN_KEY, token);

            if (!string.IsNullOrEmpty(username))
                editor.PutString(USERNAME_KEY, username);

            if (!string.IsNullOrEmpty(userId))
                editor.PutString(USER_ID_KEY, userId);

            editor.Apply();

            Console.WriteLine("Token saved to SharedPreferences");
        }

        public static string GetToken(Context context)
        {
            if (context == null)
            {
                Console.WriteLine("Cannot get token: Context is null");
                return null;
            }

            var prefs = context.GetSharedPreferences(PREFERENCES_NAME, FileCreationMode.Private);
            string token = prefs.GetString(TOKEN_KEY, null);

            if (string.IsNullOrEmpty(token))
                Console.WriteLine("No token found in SharedPreferences");
            else
                Console.WriteLine("Token retrieved from SharedPreferences");

            return token;
        }

        public static string GetUsername(Context context)
        {
            if (context == null) return null;

            var prefs = context.GetSharedPreferences(PREFERENCES_NAME, FileCreationMode.Private);
            return prefs.GetString(USERNAME_KEY, null);
        }

        public static string GetUserId(Context context)
        {
            if (context == null) return null;

            var prefs = context.GetSharedPreferences(PREFERENCES_NAME, FileCreationMode.Private);
            return prefs.GetString(USER_ID_KEY, null);
        }

        public static void ClearToken(Context context)
        {
            if (context == null) return;

            var prefs = context.GetSharedPreferences(PREFERENCES_NAME, FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.Remove(TOKEN_KEY);
            editor.Remove(USERNAME_KEY);
            editor.Remove(USER_ID_KEY);
            editor.Apply();

            Console.WriteLine("Token cleared from SharedPreferences");
        }
    }
}