using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace range.Pages;

[OutputCache(Duration = 100),
 ResponseCache(
     Duration = 100,
     Location = ResponseCacheLocation.Any,
     NoStore = false)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}