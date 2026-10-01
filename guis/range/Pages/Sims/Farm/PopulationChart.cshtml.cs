using System.Text.Json;
using CodeMechanic.Razorhat;
using JsonFlatFileDataStore;
using Microsoft.AspNetCore.Mvc;

namespace range.Pages.Sims.Farm;

public sealed class PopulationChart : RazorhatIsland
{
    private readonly DataStore _farmDb;
    public string Json { get; private set; } = "[]";

    public PopulationChart(DataStore farmDb)
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

        var series = FarmRuns.LatestSnapshots(_farmDb)
            .Select(snapshot => new { tick = snapshot.Tick, sheep = snapshot.Sheep, wolves = snapshot.Wolves });
        Json = JsonSerializer.Serialize(series);
        return Page();
    }
}
