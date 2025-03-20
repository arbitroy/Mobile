using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    // Updated comprehensive user edit model
    public class UpdateUserFullRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("setNewPassword")]
        public bool SetNewPassword { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }

    // Response for bulk user deletion
    public class BulkDeleteResultDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("successCount")]
        public int SuccessCount { get; set; }

        [JsonProperty("errorCount")]
        public int ErrorCount { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; } = new List<string>();
    }

    // Response for admin password reset
    public class PasswordResetResponseDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("tempPassword")]
        public string TempPassword { get; set; }
    }

    // User report data class for CSV generation
    public class UserReportData
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string LastLoginTime { get; set; }
        public int QuizAttempts { get; set; }
        public double AverageScore { get; set; }
        public string IsEmailConfirmed { get; set; }
    }
}