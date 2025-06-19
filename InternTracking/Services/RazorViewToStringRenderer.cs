namespace InternTracking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class RazorViewToStringRenderer : IRazorViewToStringRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IActionContextAccessor _actionContextAccessor;

    public RazorViewToStringRenderer(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider,
        IActionContextAccessor actionContextAccessor)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _actionContextAccessor = actionContextAccessor;
    }

    // Fix for CS0111: Removed duplicate method definition
    public async Task<string> RenderViewToStringAsync(string viewName, object model, Dictionary<string, object>? viewData = null) 
    {
        var actionContext = _actionContextAccessor.ActionContext;

        var viewResult = _viewEngine.FindView(actionContext, viewName, false);
        if (viewResult.View == null)
            throw new ArgumentNullException($"{viewName} does not match any available view.");

        using var sw = new StringWriter();

        var vdd = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        // Set custom ViewData items (like QR code)
        if (viewData != null)
        {
            foreach (var kvp in viewData)
            {
                vdd[kvp.Key] = kvp.Value;
            }
        }

        var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            vdd,
            tempData,
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}
