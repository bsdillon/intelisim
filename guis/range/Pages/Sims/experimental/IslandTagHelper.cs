using CodeMechanic.Types;
// using Drip.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace range
{
    /// <summary>
    /// This class is based on Drip.UI's implementation.
    /// TODO: Any improvements should be copied and committed to the Drip.UI library.
    /// Chat: https://chatgpt.com/c/6a9cb5ad-b850-83ea-8d22-fd619455bbb9
    /// </summary>
    [HtmlTargetElement("island")]
    public class IslandTagHelper : TagHelper
    {
        [HtmlAttributeName("url")] public string Url { get; set; } = string.Empty;

        [HtmlAttributeName("event")] public range.IslandEvents Event { get; set; } = range.IslandEvents.Intersect;

        [HtmlAttributeName("render-for")] public ModelExpression? RenderFor { get; set; }

        public string Page { get; set; } = string.Empty;
        public string? Handler { get; set; }

        public string Method { get; set; } = "get";
        public string Target { get; set; } = string.Empty;

        public IslandSwaps Swap { get; set; } = IslandSwaps.InnerHTML;

        public bool debug { get; set; } = false;

        public override async Task ProcessAsync(
            TagHelperContext context,
            TagHelperOutput output)
        {
            if (debug)
            {
                Console.WriteLine(
                    $"[IslandTagHelper] → <{context.TagName}> " +
                    $"Url='{Url}' Page='{Page}' Event={Event} " +
                    $"RenderFor='{RenderFor?.ModelExplorer.ModelType?.Name}'");
                // $"RenderFor='{RenderFor?.Name}'");
            }

            // No endpoint = plain div.
            if (Url.IsEmpty() && Page.IsEmpty())
            {
                output.TagName = "div";
                output.TagMode = TagMode.StartTagAndEndTag;

                var emptyContent = await output.GetChildContentAsync();
                output.Content.SetHtmlContent(emptyContent);

                return;
            }

            output.TagName = "div";

            // ---------------------------------------------------------
            // Build URL
            // ---------------------------------------------------------

            if (Url.IsEmpty() && Page.NotEmpty())
            {
                Url = Page;

                if (Handler.NotEmpty())
                    Url += $"?handler={Uri.EscapeDataString(Handler!)}";
            }

            // ---------------------------------------------------------
            // Add automatic render key
            // ---------------------------------------------------------

            if (RenderFor != null)
            {
                var modelType = RenderFor.ModelExplorer.ModelType;

                if (modelType != null)
                {
                    var renderKey = modelType.Name;

                    Url = AppendQueryString(
                        Url,
                        "render",
                        renderKey);
                }
            }

            // ---------------------------------------------------------
            // HTMX configuration
            // ---------------------------------------------------------

            var @event = Event switch
            {
                IslandEvents.Revealed => "revealed",
                IslandEvents.Click => "click",
                IslandEvents.Load => "load",
                IslandEvents.Intersect => "intersect once",
                _ => "load"
            };

            var swap = Swap switch
            {
                IslandSwaps.OuterHTML => "outerHTML",
                IslandSwaps.InnerHTML => "innerHTML",
                _ => "outerHTML"
            };

            var verb = Method.ToLowerInvariant();

            if (Url.NotEmpty())
            {
                output.Attributes.SetAttribute($"hx-{verb}", Url);
                output.Attributes.SetAttribute("hx-trigger", @event);
                output.Attributes.SetAttribute("hx-swap", swap);

                if (Target.NotEmpty())
                    output.Attributes.SetAttribute("hx-target", Target);
            }

            var childContent = await output.GetChildContentAsync();
            output.Content.SetHtmlContent(childContent);
            output.TagMode = TagMode.StartTagAndEndTag;

            if (debug)
                Console.WriteLine($"[IslandTagHelper] → hx-{verb}='{Url}'");
        }

        private static string AppendQueryString(
            string url,
            string key,
            string value)
        {
            var separator = url.Contains('?') ? '&' : '?';

            return $"{url}{separator}" +
                   $"{Uri.EscapeDataString(key)}=" +
                   $"{Uri.EscapeDataString(value)}";
        }
    }
//
// public static class PartialViewExtensions
// {

// }

// [HtmlTargetElement("island")]
// public class IslandTagHelper : TagHelper
// {
//     [HtmlAttributeName("url")] public string Url { get; set; } = string.Empty;
//     [HtmlAttributeName("event")] public IslandEvents Event { get; set; } = IslandEvents.Intersect;
//
//     [HtmlAttributeName("render-for")] public ModelExpression? RenderFor { get; set; }
//
//     public string Page { get; set; } = string.Empty;
//     public string? Handler { get; set; } = string.Empty;
//
//     public string Method { get; set; } = "get";
//     public string Target { get; set; } = string.Empty;
//
//     public IslandSwaps Swap { get; set; } = IslandSwaps.InnerHTML;
//
//     public bool debug { get; set; } = false;
//
//     public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
//     {
//         if (debug)
//             Console.WriteLine($"[IslandTagHelper] → <{context.TagName}> | Url='{Url}' | Page='{Page}' | Event={Event}");
//
//         // ─────────────────────────────────────────────────────────────
//         // CRITICAL GUARD — stops the infinite reload loop instantly
//         // If we have no URL and no Page, this is a short tag (<badge />)
//         // that fell through to the base class → just render it as plain HTML.
//         // ─────────────────────────────────────────────────────────────
//         if (Url.IsEmpty() && Page.IsEmpty())
//         {
//             if (debug)
//                 Console.WriteLine(
//                     $"[IslandTagHelper] → EMPTY URL fallback: rendering <{context.TagName}> as plain div");
//             output.TagName = "div";
//             output.TagMode = TagMode.StartTagAndEndTag;
//             var emptyContent = await output.GetChildContentAsync();
//             output.Content.SetHtmlContent(emptyContent);
//             return;
//         }
//
//         output.TagName = "div";
//
//         if (Url.IsEmpty() && Page.NotEmpty())
//         {
//             Url = Page;
//             if (Handler.NotEmpty())
//                 Url += $"?handler={Handler}";
//         }
//
//         var @event = Event switch
//         {
//             IslandEvents.Revealed => "revealed",
//             IslandEvents.Intersect => "intersect once",
//             _ => "load"
//         };
//
//         var swap = Swap switch
//         {
//             IslandSwaps.OuterHTML => "outerHTML",
//             IslandSwaps.InnerHTML => "innerHTML",
//             _ => "outerHTML"
//         };
//
//         var verb = Method.ToLowerInvariant();
//
//         // Only wire htmx when we actually have a URL
//         if (Url.NotEmpty())
//         {
//             output.Attributes.SetAttribute($"hx-{verb}", Url);
//             output.Attributes.SetAttribute("hx-trigger", @event);
//             output.Attributes.SetAttribute("hx-swap", swap);
//
//             if (Target.NotEmpty())
//                 output.Attributes.SetAttribute("hx-target", Target);
//         }
//
//         var childContent = await output.GetChildContentAsync();
//         output.Content.SetHtmlContent(childContent);
//         output.TagMode = TagMode.StartTagAndEndTag;
//     }
// }

    public static class PartialViewExtensions
    {
        /// <summary>
        /// Usage:
        ///     return this.PartialFor(state);
        ///
        /// Convention:
        ///     Die -> _Die.cshtml
        /// </summary>
        public static PartialViewResult PartialFor<T>(
            this PageModel page,
            T model)
        {
            var viewName = $"_{typeof(T).Name}";

            return page.Partial(viewName, model);
        }
    }

    public static class IslandRenderer
    {
        public static string ViewName(Type modelType)
            => $"_{modelType.Name}";

        public static string RenderKey(Type modelType)
            => modelType.Name;
    }

    public enum IslandEvents
    {
        Load,
        Revealed,
        Intersect,
        Click,
    }

    public enum IslandSwaps
    {
        OuterHTML,
        InnerHTML,
    }
}