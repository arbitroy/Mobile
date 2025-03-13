namespace Mobile.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TimeLimit { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int QuestionCount { get; set; }
    }
}