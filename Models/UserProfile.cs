using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    public class UserProfile
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime LastLoginTime { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int QuizzesTaken { get; set; }
        public double AverageScore { get; set; }
    }

    public class ProfileUpdateRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }  // Add this property
    }

    public class PasswordChangeRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    // Enhanced profile update model
    public class UpdateProfileRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("updatePassword")]
        public bool UpdatePassword { get; set; }

        [JsonProperty("currentPassword")]
        public string CurrentPassword { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }

        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class QuizAttemptDetailDto : QuizAttemptSummary
    {
        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; } // in minutes
    }
}