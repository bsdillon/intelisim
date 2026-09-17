using CodeMechanic.Razorhat;
using Microsoft.AspNetCore.Mvc;

namespace range.Pages.Sims.Farm;

public sealed class PopulationChart : RazorhatIsland
{
    public string Json { get; private set; } = "{}";

    public IActionResult OnGet(string json)
    {
        // TODO:
        // Replace this with loading the SimulationRun.
        Json = json;

        return Page();
    }
}