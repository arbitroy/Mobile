using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    public class UserStats
    {
        public int TotalQuizzesTaken { get; set; }
        public double AverageScore { get; set; }
        public double BestScore { get; set; }
        public List<QuizAttemptSummary> RecentAttempts { get; set; } = new List<QuizAttemptSummary>();
    }

    public class QuizAttemptSummary
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int Score { get; set; }
        public DateTime EndTime { get; set; }
    }

    // New models for extended functionality
    public class UserDashboardDto
    {
        [JsonProperty("totalQuizzesTaken")]
        public int TotalQuizzesTaken { get; set; }

        [JsonProperty("averageScore")]
        public double AverageScore { get; set; }

        [JsonProperty("bestScore")]
        public double BestScore { get; set; }

        [JsonProperty("recentAttempts")]
        public List<QuizAttemptSummary> RecentAttempts { get; set; } = new List<QuizAttemptSummary>();

        [JsonProperty("recommendedQuizzes")]
        public List<Quiz> RecommendedQuizzes { get; set; } = new List<Quiz>();
    }

    public class CategoryStatsDto
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("averageScore")]
        public double AverageScore { get; set; }
    }
}