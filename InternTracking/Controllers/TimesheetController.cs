using InternTracking.Models;
using InternTracking.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InternTracking.Controllers
{
    public class TimesheetController : Controller
    {
        private readonly ApplicationDbContext context;

        public TimesheetController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult AddIntern()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddIntern(Intern intern)
        {
            context.Interns.Add(intern);
            context.SaveChanges();
            return RedirectToAction("AddIntern");
        }

        public IActionResult InternDetails()
        {
           var intern = context.Interns.ToList();
            return View(intern);
        }


        public IActionResult Submit()
        {
            ViewBag.Interns = new SelectList(context.Interns.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Submit(Timesheet timesheet)
        {
            timesheet.Status = "Pending";
            context.Timesheets.Add(timesheet);
            context.SaveChanges();
            return RedirectToAction("MyEntries");
        }

        public IActionResult MyEntries()
        {
            var all = context.Timesheets.Include(t => t.Intern).ToList();
            
            return View(all);
        }

        // For Admin/Supervisor to review
        public IActionResult AllSubmissions()
        {
            var all = context.Timesheets.Include(t => t.Intern).ToList();
            return View(all);
        }

        public IActionResult Approve(int id)
        {
            var t = context.Timesheets.Find(id);
            t.Status = "Approved";
            context.SaveChanges();
            return RedirectToAction("AllSubmissions");
        }

        public IActionResult Reject(int id)
        {
            var t = context.Timesheets.Find(id);
            t.Status = "Rejected";
            context.SaveChanges();
            return RedirectToAction("AllSubmissions");
        }


    }
}
