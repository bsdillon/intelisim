using System.Collections.Concurrent;
using BlackMesa.Sims;
using CodeMechanic.Diagnostics;
using CodeMechanic.Shargs;
using CodeMechanic.Types;
using JsonFlatFileDataStore;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using Westwind.AspNetCore.Markdown.Utilities;

namespace range;

public class SheepFarm(Logger logger, ArgsMap arguments, DataStore farm_db) : RazorHatPage(logger, arguments)
{
    // TODO: make this DI injected, according to the docs: https://github.com/ttu/json-flatfile-datastore
    // private readonly DataStore farm_db = new("farm_db.json");
    public PredatorPreySimulation FarmSim { get; set; } = new(42);
    public PredatorPreyParameters FarmParams { get; set; } = new PredatorPreyParameters();
    public int Trials { get; set; } = 1;

    public IActionResult OnGet()
    {
        if (debug) FarmSim.Dump(printFn: printFn);
        if (debug) FarmParams.Dump(printFn: printFn);
        // farm_db = new JsonFlatFileDataStore.DataStore("farm_db.json");
        return Page();
    }

    public IActionResult OnGetReset()
    {
        logger.Information($"{nameof(OnGetReset)}");
        FarmSim = new PredatorPreySimulation(seed: 0);
        FarmSim.Dump("new");
        return Content("Resetti");
    }

    public async Task<IActionResult> OnGetPlay()
    {
        try
        {
            var sims_collection = farm_db.GetCollection<SimulationRun>("simulations");

            var simulations = Enumerable.Range(0, Trials)
                .Aggregate(new Dictionary<int, PredatorPreySimulation>(), (map, i) =>
                {
                    map.TryAdd(i, new PredatorPreySimulation(i));
                    return map;
                });


            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 20
            };

            // var results = new ConcurrentBag<SimulationResult>();
            var results = new ConcurrentBag<SimulationRun>();

            await Parallel.ForEachAsync(simulations, options, async (kvp, ct) =>
            {
                int seed = kvp.Key;
                var simulation = kvp.Value;

                simulation.Run(seed);

                results.Add(new SimulationRun(
                    seed,
                    FarmParams,
                    simulation.Snapshots
                ));

                await Task.CompletedTask;
            });

            await sims_collection.InsertManyAsync(results);
            
            // await Parallel.ForEachAsync(simulations, options, async (kvp, ct) =>
            // {
            //     int seed = kvp.Key;
            //     var simulation = kvp.Value;
            //
            //     simulation.Run(seed);
            //
            //     logger.Information($"Ticks:      {simulation.Model.Tick}");
            //     logger.Information($"Sheep:      {simulation.Model.PreyCount}");
            //     logger.Information($"Wolves:     {simulation.Model.PredatorCount}");
            //     logger.Information($"Population: {simulation.Model.Population}");
            //     logger.Information($"Predator:   {simulation.Model.PredatorRatio:P2}");
            //
            //     results.Add(new SimulationResult(
            //         seed,
            //         simulation.Model.Tick,
            //         simulation.Model.PreyCount,
            //         simulation.Model.PredatorCount,
            //         simulation.Model.Population,
            //         simulation.Model.PredatorRatio
            //     ));
            //
            //     await Task.CompletedTask;
            // });
            //
            // await sims_collection.InsertManyAsync(results);
        }
        catch (Exception ex)
        {
            logger.Information(ex.ToString());
            if (debug) throw;
        }

        return Partial("_SimulationComplete", FarmSim);
    }

    public IActionResult OnGetStep()
    {
        return Content("Stepped!");
    }
}