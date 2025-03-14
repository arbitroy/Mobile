using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    public class QuizSubmission
    {
        [JsonProperty("quizId")]
        public int QuizId { get; set; }

        [JsonProperty("answers")]
        public Dictionary<int, int> Answers { get; set; } = new Dictionary<int, int>();
    }
}