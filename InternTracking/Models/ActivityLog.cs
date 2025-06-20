namespace InternTracking.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string Action { get; set; }            // e.g., "Deleted Intern"
        public string AdminName { get; set; }         // e.g., "Aryan"
        public DateTime Timestamp { get; set; }       // e.g., DateTime.Now
        public string Details { get; set; }           // e.g., "Deleted Intern Ravi"
    }

}
