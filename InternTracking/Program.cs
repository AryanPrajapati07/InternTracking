using BaselineTypeDiscovery;
using DinkToPdf;
using DinkToPdf.Contracts;
using InternTracking.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

IronPdf.License.LicenseKey = "IRONSUITE.ARYAN22.EXCELSIOR.GMAIL.COM.6945-287E510A5A-B5GXCA6OBESTZA-LNO4ODK76XTM-7C6OHA466XIJ-M3QPGDAMR66S-SGQ5HVGNZG2N-TB3NWOQBUGUB-EK4CC7-TAFDRUCFSRWPUA-DEPLOYMENT.TRIAL-BS27OC.TRIAL.EXPIRES.12.JUL.2025";


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();

builder.Services.AddControllers();

builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

var context = new CustomAssemblyLoadContext();


builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});


var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


