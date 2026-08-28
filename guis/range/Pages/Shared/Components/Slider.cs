using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace range.Pages.Shared.Components;

[HtmlTargetElement("slider")]
public class Slider : TagHelper
{
    public uint Step { get; set; } = default(uint);
    public int Min { get; set; } = 0;
    public int Max { get; set; } = 1;

    public int? Padding { get; set; }

    public int? Margin { get; set; }

    private readonly HtmlEncoder _encoder;

    public Slider(HtmlEncoder encoder)
    {
        _encoder = encoder;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (Padding.HasValue)
            output.AddClass($"p-{Padding}", _encoder);

        if (Margin.HasValue)
            output.AddClass($"p-{Margin}", _encoder);

        var content = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(content);
    }
}