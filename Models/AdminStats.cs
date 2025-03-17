using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    public class AdminStats
    {
        [JsonProperty("userCount")]
        public int UserCount { get; set; }

        [JsonProperty("quizCount")]
        public int QuizCount { get; set; }

        [JsonProperty("attemptCount")]
        public int AttemptCount { get; set; }

        [JsonProperty("averageScore")]
        public double AverageScore { get; set; }

        [JsonProperty("recentAttempts")]
        public List<RecentAttempt> RecentAttempts { get; set; } = new List<RecentAttempt>();

        [JsonProperty("popularQuizzes")]
        public List<PopularQuiz> PopularQuizzes { get; set; } = new List<PopularQuiz>();
    }

    public class RecentAttempt
    {
        [JsonProperty("attemptId")]
        public int AttemptId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("quizId")]
        public int QuizId { get; set; }

        [JsonProperty("quizTitle")]
        public string QuizTitle { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }
    }

    public class PopularQuiz
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("attemptCount")]
        public int AttemptCount { get; set; }

        [JsonProperty("averageScore")]
        public double AverageScore { get; set; }
    }
}