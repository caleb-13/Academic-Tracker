using SQLite;

namespace Mobile_Application_Development.Models
{
    public enum CourseStatus
    {
        PlanToTake = 0,
        InProgress = 1,
        Completed = 2,
        Dropped = 3
    }

    [Table("Courses")]
    public class Course
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        
        [Indexed]
        public int TermId { get; set; }

     
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

       
        public CourseStatus Status { get; set; } = CourseStatus.PlanToTake;
        public string InstructorName { get; set; } = string.Empty;
        public string InstructorPhone { get; set; } = string.Empty;
        public string InstructorEmail { get; set; } = string.Empty;

      
        public string Notes { get; set; } = string.Empty;
        public bool StartAlertEnabled { get; set; }
        public bool EndAlertEnabled { get; set; }
    }
}
