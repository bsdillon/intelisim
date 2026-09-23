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

public class SheepFarm : RazorHatPage
{
    private readonly DataStore _farmDb;
    public int Trials { get; set; } = 1;
    public int Seed { get; set; } = 42;

    public PredatorPreyParameters FarmParams { get; set; } = new()
    {
        Sheep = 100,
        Wolves = 20,
        StartingSheepEnergy = 10,
        StartingWolfEnergy = 10,
        GrassEnergy = 5,
        WolfHuntEnergy = 5,
        SheepReproductionThreshold = 20,
        WolfReproductionThreshold = 20,
        SheepReproductionCost = 10,
        WolfReproductionCost = 10
    };

    public PredatorPreySimulation FarmSim { get; set; }


    public SheepFarm(Logger logger, ArgsMap arguments, DataStore farmDb) : base(logger, arguments)
    {
        _farmDb = farmDb;
        FarmSim = new PredatorPreySimulation(FarmParams);
    }

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
        FarmSim = new PredatorPreySimulation(FarmParams);
        FarmSim.Dump("new");
        return Content("Resetti");
    }

    public async Task<IActionResult> OnGetPlay()
    {
        try
        {
            var sims_collection = _farmDb.GetCollection<SimulationRun>("simulations");

            // var simulations = Enumerable.Range(0, Trials)
            //     .Aggregate(new Dictionary<int, PredatorPreySimulation>(), (map, i) =>
            //     {
            //         map.TryAdd(i, new PredatorPreySimulation(FarmParams, i));
            //         return map;
            //     });

            var simulations = Enumerable.Range(0, Trials)
                .Select(seed => new PredatorPreySimulation(FarmParams))
                .ToArray();


            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 20
            };

            // var results = new ConcurrentBag<SimulationResult>();
            var results = new ConcurrentBag<SimulationRun>();

            await Parallel.ForEachAsync(simulations, options, async (simulation, ct) =>
            {
                simulation.Run(Seed, ticks: 120);

                var run = new SimulationRun(
                    simulation.Seed,
                    simulation.Parameters,
                    simulation.Snapshots);

                results.Add(run);

                logger.Information(
                    "Seed={Seed}, Snapshots={Snapshots}",
                    run.Seed,
                    run.Snapshots.Count);

                foreach (var simulationSnapshot in run.Snapshots)
                {
                    logger.Information($"Ticks:      {simulationSnapshot.Tick}");
                    logger.Information($"Sheep:      {simulationSnapshot.Sheep}");
                    logger.Information($"Wolves:     {simulationSnapshot.Wolves}");
                    logger.Information($"Population: {simulationSnapshot.Population}");
                    logger.Information($"Predator:   {simulationSnapshot.PredatorRatio:P2}");
                }

                await Task.CompletedTask;
            });

            int total = results.Count;
            logger.Information($"Total sims completed: {total}");
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