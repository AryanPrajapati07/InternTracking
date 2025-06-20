using Microsoft.AspNetCore.Mvc;
using InternTracking.Models;
using InternTracking.Services;

namespace InternTracking.Controllers
{
    public class ActivityLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var logs = _context.ActivityLogs.OrderByDescending(l => l.Timestamp).ToList();
            return View(logs);
        }
    }
}


