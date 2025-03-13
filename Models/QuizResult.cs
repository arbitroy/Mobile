using System;
using System.Collections.Generic;

namespace Mobile.Models
{

    public class QuizResult
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int Score { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<QuestionResult> Questions { get; set; } = new List<QuestionResult>();
    }

    public class QuestionResult
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int SelectedOptionId { get; set; }
        public string SelectedOptionText { get; set; }
        public int CorrectOptionId { get; set; }
        public string CorrectOptionText { get; set; }
        public bool IsCorrect { get; set; }
    }
}