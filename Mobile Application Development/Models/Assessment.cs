using SQLite;

namespace Mobile_Application_Development.Models
{
    public enum AssessmentType
    {
        Performance = 0,
        Objective = 1
    }

    [Table("Assessments")]
    public class Assessment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int CourseId { get; set; }

        public AssessmentType Type { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        
        public bool StartAlertEnabled { get; set; }
        public bool EndAlertEnabled { get; set; }
    }
}
