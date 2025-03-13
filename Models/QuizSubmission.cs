using System;
using System.Collections.Generic;

namespace Mobile.Models
{
    public class QuizSubmission
    {
        public int QuizId { get; set; }
        public Dictionary<int, int> Answers { get; set; } = new Dictionary<int, int>();
    }
}