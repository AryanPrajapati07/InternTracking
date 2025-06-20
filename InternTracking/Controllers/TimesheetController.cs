using ClosedXML.Excel;
using com.sun.xml.@internal.bind.v2.runtime;
using DinkToPdf;
using DinkToPdf.Contracts;
using InternTracking.Helpers;
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
using QRCoder;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO.Compression;
using DocumentFormat.OpenXml.Spreadsheet;
using java.util.zip;

namespace InternTracking.Controllers
{
    public class TimesheetController : Controller
    {
        private const string StaticAdminName = "Aryan Prajapati";

        private readonly ApplicationDbContext context;
        private readonly IConverter converter;
        private readonly IRazorViewToStringRenderer _viewRenderService;

        public TimesheetController(ApplicationDbContext context, IConverter converter, IRazorViewToStringRenderer _viewRenderedService)
        {
            this.context = context;
            this.converter = converter;
            this._viewRenderService = _viewRenderedService;

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

        //Edit Intern
        [HttpGet]
        public IActionResult EditIntern(int id)
        {
            var intern = context.Interns.Find(id);
            if (intern == null) return NotFound();
            return View(intern);
        }
        [HttpPost]
        public IActionResult EditIntern(Intern intern)
        {
            if (ModelState.IsValid)
            {
                context.Interns.Update(intern);
                context.SaveChanges();
                return RedirectToAction("InternDetails");
            }
            return View(intern);
        }

        //Delete Intern
        [HttpGet]
        public IActionResult DeleteIntern(int id)
        {
            var intern = context.Interns.Find(id);
            if (intern == null) return NotFound();
            return View(intern);
        }
        [HttpPost, ActionName("DeleteIntern")]
        public IActionResult DeleteInternConfirmed(int id)
        {
            var intern = context.Interns.Find(id);
            if (intern == null) return NotFound();
            context.Interns.Remove(intern);
            context.SaveChanges();

            LogActivity("Deleted Intern", $"Deleted Intern {intern.Name} (ID: {intern.Id})");
            TempData["Success"] = "Intern deleted successfully!";
            return RedirectToAction("InternDetails");
        }

        //View of Intern Data
        public IActionResult InternDetails(string searchTerm, int page = 1)
        {
            int pageSize = 5;
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

        //Create Dashboard for Interns
        public IActionResult Dashboard()
        {
            var totalInterns = context.Interns.Count();
            var thismonth = DateTime.Now.Month;
            var thisyear = DateTime.Now.Year;

            var newInternsThisMonth = context.Interns
                .Where(i => i.JoinDate.Month == thismonth && i.JoinDate.Year == thisyear)
                .Count();

            var internsByDepartment = context.Interns
                .GroupBy(i => i.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.TotalInterns = totalInterns;
            ViewBag.NewInterns = newInternsThisMonth;
            ViewBag.DeptChartLabels = internsByDepartment.Select(d => d.Department).ToList();
            ViewBag.DeptChartData = internsByDepartment.Select(d => d.Count).ToList();


            return View();
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

        // Export to Excel Intern Details
        public IActionResult ExportToExcel(string searchTerm = "")
        {
            var query = context.Interns.AsQueryable();
            if (!String.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(searchTerm) || i.Email.ToLower().Contains(searchTerm));
            }
            var interns = query.Select(i => new
            {
                i.Id,
                i.Name,
                i.Email,
                i.Department,
                i.JoinDate
            }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Interns");
                worksheet.Cell(1, 1).Value = "Id";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Department";
                worksheet.Cell(1, 5).Value = "Join Date";

                int row = 2;
                foreach (var intern in interns)
                {
                    worksheet.Cell(row, 1).Value = intern.Id;
                    worksheet.Cell(row, 2).Value = intern.Name;
                    worksheet.Cell(row, 3).Value = intern.Email;
                    worksheet.Cell(row, 4).Value = intern.Department;
                    worksheet.Cell(row, 5).Value = intern.JoinDate.ToString("yyyy-MM-dd");
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Interns_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        //send PDF by email
        public async Task<IActionResult> SendPdfEmail()
        {

            var interns = context.Interns.ToList();
            var html = await RenderViewToStringAsync("InternDetails", interns);
            var Renderer = new IronPdf.ChromePdfRenderer();
            var pdf = Renderer.RenderHtmlAsPdf(html);
            byte[] pdfBytes = pdf.BinaryData;

            string toEmail = "aryan22.excelsior@gmail.com";
            string subject = "Intern Report PDF";
            string body = "Attached is the latest intern report.";
            string fileName = "InternReport.pdf";

            try
            {
                EmailService.SendEmailWithAttachmentFromStream(toEmail, subject, body, pdfBytes, fileName);
                TempData["Success"] = $"PDF report sent to {toEmail}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("InternDetails");
        }

        //Certificate Generation
        public async Task<IActionResult> GenerateCertificate(int Internid)
        {
            var intern = context.Interns.FirstOrDefault(i => i.Id == Internid);
            if (intern == null)
            {
                TempData["Error"] = "Intern not found.";
                return RedirectToAction("InternDetails");
            }

            string certUrl = $"{Request.Scheme}://{Request.Host}/Timesheet/Verify?id={Internid}";
            //string certUrl = $"{Request.Scheme}://Google.com";
            string qrBase64 = QrHelper.GenerateQrCodeBase64(certUrl);

            var viewData = new Dictionary<string, object>
            {
                { "QrCode", qrBase64 }
            };

            var html = await _viewRenderService.RenderViewToStringAsync("CertificateTemplate", intern, viewData);


            var Renderer = new IronPdf.HtmlToPdf();
            var pdf = Renderer.RenderHtmlAsPdf(html);
            byte[] pdfBytes = pdf.BinaryData;

            try
            {
                EmailService.SendEmailWithAttachmentFromStream(
                    intern.Email,
                    "Your Internship Certificate",
                    "Dear " + intern.Name + ",\n\nPlease find your certificate attached.",
                    pdfBytes,
                    "InternshipCertificate.pdf"
                );

                LogActivity("Sent Certificate", $"Certificate sent to {intern.Name} ({intern.Email})");


                TempData["Success"] = "Certificate sent successfully to your Email!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to send certificate. " + ex.Message;
            }
            return File(pdfBytes, "application/pdf", $"{intern.Name}_Certificate.pdf");
            //return RedirectToAction("InternDetails");

        }

        // QR code verify
        public IActionResult Verify(int id)
        {
            var intern = context.Interns.FirstOrDefault(i => i.Id == id);
            if (intern == null) return View("NotFound");

            return View("Verify", intern);
        }

        public IActionResult PreviewCertificate(int internId)
        {
            var intern = context.Interns.FirstOrDefault(i => i.Id == internId);
            if (intern == null)
                return NotFound();

            string certUrl = $"{Request.Scheme}://{Request.Host}/Timesheet/Verify?id={internId}";
            string qrBase64 = QrHelper.GenerateQrCodeBase64(certUrl);

            ViewBag.QrCode = qrBase64;

            return View("CertificateTemplate", intern);
        }

        // download all certificates as ZIP file
        public async Task<IActionResult> DownloadAllCertificatesAsZip()
        {
            var interns = context.Interns.ToList();
            if (!interns.Any())
            {
                TempData["Error"] = "No interns Found";
                return RedirectToAction("InternDetails");
            }
            var renderer = new IronPdf.HtmlToPdf();
            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var intern in interns)
                {
                    string certUrl = $"{Request.Scheme}://{Request.Host}/Timesheet/Verify?id={intern.Id}";
                    string qrBase64 = QrHelper.GenerateQrCodeBase64(certUrl);

                    var viewData = new Dictionary<string, object>
                    {
                        { "QrCode", qrBase64 }
                    };

                    var html = await _viewRenderService.RenderViewToStringAsync("CertificateTemplate", intern, viewData);

                    var pdf = renderer.RenderHtmlAsPdf(html);
                    byte[] pdfBytes = pdf.BinaryData;

                    var entry = archive.CreateEntry($"{intern.Name}_Certificate.pdf");
                    using var entryStream = entry.Open();
                    entryStream.Write(pdfBytes, 0, pdfBytes.Length);
                }
            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return File(zipStream.ToArray(), "application/zip", "All_Intern_Certificates.zip");
        }


        //track Log Activity
        private void LogActivity(string action, string details)
        {
            var log = new ActivityLog
            {
                Action = action,
                AdminName = StaticAdminName,
                Timestamp = DateTime.Now,
                Details = details
            };
            context.ActivityLogs.Add(log);
            context.SaveChanges();
        }


    }
}
