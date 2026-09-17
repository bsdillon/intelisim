using CodeMechanic.Razorhat;
using Microsoft.AspNetCore.Mvc;

namespace range.Pages.Sims.Farm;

public sealed class FarmStats : RazorhatIsland
{
    public int Ticks { get; private set; }
    public int Sheep { get; private set; }
    public int Wolves { get; private set; }

    public IActionResult OnGet(string json)
    {
        // TODO:
        // Deserialize JSON / load persisted SimulationRun.
        // For now, fake it.

        Ticks = 10;
        Sheep = 0;
        Wolves = 87;

        return Page();
    }
}