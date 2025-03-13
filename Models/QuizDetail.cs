namespace Mobile.Models
{
    public class QuizDetail : Quiz
    {
        public System.Collections.Generic.List<Question> Questions { get; set; } = new System.Collections.Generic.List<Question>();
    }

    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public System.Collections.Generic.List<Option> Options { get; set; } = new System.Collections.Generic.List<Option>();
    }

    public class Option
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}