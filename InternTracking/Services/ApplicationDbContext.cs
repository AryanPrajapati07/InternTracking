using Microsoft.EntityFrameworkCore;
using InternTracking.Models; 

namespace InternTracking.Services
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Intern> Interns { get; set; }
        public DbSet<Timesheet> Timesheets { get; set; }

    }
}
