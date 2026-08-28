namespace range.Pages.Shared.Components;

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("link-button")]
public class LinkButtonTagHelper : TagHelper
{
    private readonly HtmlEncoder _encoder;

    public LinkButtonTagHelper(HtmlEncoder encoder)
    {
        _encoder = encoder;
    }

    public int? Padding { get; set; }
    public string? Rounded { get; set; }
    public string? Shadow { get; set; }

    public string href { get; set; } = "#";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        string[] classname_parts =
            "inline-flex items-center justify-center px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                .Split(" ");

        foreach (var part in classname_parts)
        {
            output.AddClass(part, _encoder);
        }

        if (Padding.HasValue)
            output.AddClass($"p-{Padding}", _encoder);

        if (!string.IsNullOrWhiteSpace(Rounded))
            output.AddClass($"rounded-{Rounded}", _encoder);

        if (!string.IsNullOrWhiteSpace(Shadow))
            output.AddClass($"shadow-{Shadow}", _encoder);

        if (!string.IsNullOrWhiteSpace(href))
        {
            // Console.WriteLine($"{nameof(href)} :>> {href}");
            output.Attributes.SetAttribute("href", href);
        }

        var content = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(content);
    }
}