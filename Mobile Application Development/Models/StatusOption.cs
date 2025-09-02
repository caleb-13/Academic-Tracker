using SQLite;

namespace Mobile_Application_Development.Models
{
    public class StatusOption
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        [Indexed] public string Category { get; set; } = "CourseStatus";
        public string Code { get; set; } = "";                          
        public string Label { get; set; } = "";                         
        public int Order { get; set; }                                   
    }
}