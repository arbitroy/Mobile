using System;
using System.Collections.Generic;

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
}