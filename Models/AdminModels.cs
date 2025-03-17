using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    // Admin Quiz Models
    public class QuizAdmin
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("timeLimit")]
        public int TimeLimit { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("questionCount")]
        public int QuestionCount => Questions?.Count ?? 0;

        [JsonProperty("questions")]
        public List<QuestionAdmin> Questions { get; set; } = new List<QuestionAdmin>();
    }

    public class QuestionAdmin
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("options")]
        public List<OptionAdmin> Options { get; set; } = new List<OptionAdmin>();
    }

    public class OptionAdmin
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("isCorrect")]
        public bool IsCorrect { get; set; }
    }

    // Admin User Models
    public class AdminCreateUserRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class AdminUpdateUserRequest
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class AdminResetPasswordRequest
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }
}