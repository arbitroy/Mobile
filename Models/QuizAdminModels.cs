using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mobile.Models
{
    // Admin Quiz Models for quiz management
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
}