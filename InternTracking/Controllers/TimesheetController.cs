using DinkToPdf;
using DinkToPdf.Contracts;
using InternTracking.Models;
using InternTracking.Services;
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;

namespace InternTracking.Controllers
{
    public class TimesheetController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IConverter converter;
       

        public TimesheetController(ApplicationDbContext context,IConverter converter)
        {
            this.context = context;
            this.converter = converter;
           
        }

        //Add Intern Data
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

        //View of Intern Data
        public IActionResult InternDetails(string searchTerm, int page = 1)
        {
            int pageSize = 2;
            var internsQuery = context.Interns.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                internsQuery = internsQuery.Where(i =>
                    i.Name.ToLower().Contains(searchTerm) ||
                    i.Email.ToLower().Contains(searchTerm));
            }

            int totalRecords = internsQuery.Count();

            var interns = internsQuery
                            .OrderBy(i => i.Id)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return View(interns);
        }


        // Submit timesheets by interns
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

        //View all entries by interns
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

        
        // Export to PDF Intern Details
        public async Task<IActionResult> ExportInternDetailsToPdf()
        {
            var interns = context.Interns.ToList();
            string htmlContent = await RenderViewToStringAsync("InternDetails", interns);
            var Renderer = new HtmlToPdf();
            var pdf = Renderer.RenderHtmlAsPdf(htmlContent);
            return File(pdf.BinaryData, "application/pdf", "InternDetails.pdf");
        }

        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var viewEngine = HttpContext.RequestServices.GetService(typeof(IRazorViewEngine)) as IRazorViewEngine;
            var tempDataProvider = HttpContext.RequestServices.GetService(typeof(ITempDataProvider)) as ITempDataProvider;
            var serviceProvider = HttpContext.RequestServices.GetService(typeof(IServiceProvider)) as IServiceProvider;

            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);

            using var sw = new StringWriter();
            var viewResult = viewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any available view.");
            }

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(HttpContext, tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }



    }
}
