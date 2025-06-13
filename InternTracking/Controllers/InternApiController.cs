using InternTracking.Models;
using InternTracking.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternTracking.Controllers
{
    public class InternApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InternApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public JsonResult GetAll() 
        { 
            var interns = _context.Interns
                .Select(i => new 
                { 
                    i.Id, 
                    i.Name, 
                    i.Email,
                    i.Department,
                    i.JoinDate
                })
                .ToList();
            return Json(interns);
        }

        [HttpGet]
        public JsonResult GetById(int id)
        {
            var interns = _context.Interns
                .Where(i => i.Id == id)
                .Select(i => new { i.Id, i.Name, i.Email, i.Department ,i.JoinDate })
                .FirstOrDefault();

            if(interns == null)
                return Json(new {status = 404,message = "Intern Not Found"});

            return Json(interns);
        }

        [HttpPost]
        public JsonResult Create([FromBody] Intern intern)
        {
            _context.Interns.Add(intern);
            _context.SaveChanges();
            return Json(new { status = 200, message = "Intern created", intern });
        }

        [HttpPut]
        public JsonResult Update([FromBody] Intern updated)
        {
            var intern = _context.Interns.Find(updated.Id);
            if (intern == null)
                return Json(new { status = 404, message = "Intern Not Found" });

            intern.Name = updated.Name;
            intern.Email = updated.Email;
            intern.Department = updated.Department;
            intern.JoinDate = updated.JoinDate;
            _context.SaveChanges();
            return Json(new {status=200,message="Intern Updated"});
        }

        [HttpDelete]
        public JsonResult Delete(int id)
        {
            var intern = _context.Interns.Find(id);
            if (intern == null) return Json(new { status = 404, message = "Intern Not Found" });

            _context.Interns.Remove(intern);
            _context.SaveChanges();
            return Json(new { status = 200, message = "Intern Deleted" });
        } 
       
    }
}
