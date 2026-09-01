using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public sealed class RazorPartialRenderer : IRazorPartialRenderer
{
    private readonly IRazorViewEngine _views;
    private readonly ITempDataProvider _tempData;

    public RazorPartialRenderer(IRazorViewEngine views, ITempDataProvider tempData)
    {
        _views = views;
        _tempData = tempData;
    }

    public async Task<string> RenderAsync(HttpContext http, string viewPath, object? model)
    {
        var actionContext = new ActionContext(http, http.GetRouteData(), new ActionDescriptor());
        var result = _views.GetView(executingFilePath: null, viewPath, isMainPage: false);
        if (!result.Success)
            throw new InvalidOperationException($"Partial not found: {viewPath}");

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        await using var writer = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            result.View,
            viewData,
            new TempDataDictionary(http, _tempData),
            writer,
            new HtmlHelperOptions());

        await result.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}

public interface IRazorPartialRenderer
{
    Task<string> RenderAsync(HttpContext http, string viewPath, object? model);
}