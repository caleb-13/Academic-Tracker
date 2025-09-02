namespace Mobile_Application_Development.Models
{
    public enum AssessmentType { Performance, Objective }

    public class Assessment
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public AssessmentType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
