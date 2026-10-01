using System.Text.Json;
using BlackMesa.Sims;
using CodeMechanic.Razorhat;
using JsonFlatFileDataStore;
using Microsoft.AspNetCore.Mvc;

namespace range.Pages.Sims.Farm;

public sealed class SimulationHistogram : RazorhatIsland
{
    private readonly DataStore _farmDb;
    public string Json { get; private set; } = "[]";

    public SimulationHistogram(DataStore farmDb)
    {
        _farmDb = farmDb;
    }

    public IActionResult OnGet(string json)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            Json = json;
            return Page();
        }

        var samples = FarmRuns.LatestSnapshots(_farmDb).Select(snapshot => snapshot.Population);
        Json = JsonSerializer.Serialize(samples);
        return Page();
    }
}
