using Microsoft.AspNetCore.Mvc.RazorPages;

namespace range.Pages.Sims.Farm;

using CodeMechanic.Razorhat;
using Microsoft.AspNetCore.Mvc;

public sealed class SimulationHistogram : RazorhatIsland
{
    public string Json { get; private set; } = "{}";

    public IActionResult OnGet(string json)
    {
        // TODO:
        // Load observations from SimulationRun(s).
        Json = json;

        return Page();
    }
}