using System.Text.Encodings.Web;
using CodeMechanic.Types;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace range.Pages.Shared.Components;

[HtmlTargetElement("range-navbar")]
public class Navbar : TagHelper
{
    private readonly HtmlEncoder _encoder;

    public string[] Links { get; set; } = default(string[]);

    public Navbar(HtmlEncoder encoder)
    {
        _encoder = encoder;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        Console.WriteLine("Hello from RANGE navbar");
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;
        // var content = await output.GetChildContentAsync();

        string link_elements = (Links.IsNullOrEmpty()
            ? ""
            : Links?
                .Select(href => $"<li><a href={href}>Homepage</a></li>")
                .Rollup()) ?? "";

        string html_template = """
                               <div class="navbar bg-base-100 shadow-sm">
                                   <div class="flex-none">
                                     <div class="dropdown">
                                       <div tabindex="0" role="button" class="btn btn-ghost btn-circle">
                                         <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"> <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h7" /> </svg>
                                       </div>
                                       <ul
                                         tabindex="-1"
                                         class="menu menu-sm dropdown-content bg-base-100 rounded-box z-1 mt-3 w-52 p-2 shadow">
                                         $links$
                                       </ul>
                                     </div>
                                   </div>
                                   <div class="flex-1">
                                       <a class="btn btn-ghost text-xl">RANGE</a>
                                   </div>
                                   <div class="flex-none">
                                       <button class="btn btn-square btn-ghost">
                                           <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block h-5 w-5 stroke-current"> <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 12h.01M12 12h.01M19 12h.01M6 12a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0z"></path> </svg>
                                       </button>
                                   </div>
                               </div>
                               """;

        string html = html_template.AsArray().ReplaceAll(new Dictionary<string, string>()
        {
            [@"\$links\$"] = link_elements
        }).FlattenText();

        output.Content.SetHtmlContent(html);
    }
}