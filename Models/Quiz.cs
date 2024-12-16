namespace importer_app.Models
{
    public class Quiz
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswer { get; set; } // Chỉ số đáp án đúng (0 = A, 1 = B, ...)
        public string QuestionType { get; set; } = "Multiple Choice";
        public int Duration { get; set; } = 100; // Thời gian mặc định
    }
}
