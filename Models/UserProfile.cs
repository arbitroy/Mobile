using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    public class UserProfile
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("lastLoginTime")]
        public DateTime LastLoginTime { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [JsonProperty("quizzesTaken")]
        public int QuizzesTaken { get; set; }

        [JsonProperty("averageScore")]
        public double AverageScore { get; set; }
    }

    public class ProfileUpdateRequest
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }
    }

    public class PasswordChangeRequest
    {
        [JsonProperty("currentPassword")]
        public string CurrentPassword { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }

        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}