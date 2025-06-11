namespace InternTracking.Models
{
    public class Timesheet
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string TaskDescription { get; set; }
        public int HoursWorked { get; set; }

        public string Status { get; set; } = "Pending";  // Pending, Approved, Rejected

        public int InternId { get; set; }
        public Intern Intern { get; set; }
    }

}
