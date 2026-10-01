using BlackMesa.Sims;
using JsonFlatFileDataStore;

namespace range.Pages.Sims.Farm;

public static class FarmRuns
{
    public const string CollectionName = "simulations";

    public static IReadOnlyList<SimulationSnapshot> LatestSnapshots(DataStore store)
    {
        var runs = store.GetCollection<SimulationRun>(CollectionName).AsQueryable().ToList();
        if (runs.Count == 0)
            return [];

        return runs[^1].Snapshots;
    }
}
