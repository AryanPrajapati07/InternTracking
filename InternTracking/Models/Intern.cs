namespace InternTracking.Models
{
    public class Intern
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string Department { get; set; } 
        public DateTime JoinDate { get; set; }
    } 
}
