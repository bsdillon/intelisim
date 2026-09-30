using System.Collections.Concurrent;
using BlackMesa.Sims;
using CodeMechanic.Diagnostics;
using CodeMechanic.Shargs;
using JsonFlatFileDataStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Serilog.Core;

namespace range;

public class SheepFarm : RazorHatPage
{
    private const string ParametersCollectionName = "Parameters";
    private const string SimCollectionName = "simulations";

    private readonly DataStore _farmDb;
    public int Trials { get; set; } = 1;
    public int Seed { get; set; } = 42;
    public int MaxTicks { get; set; } = 120;

    private static CancellationTokenSource _simulationCts = new();

    [BindNever] public PredatorPreyParameters FarmParams { get; set; }

    public PredatorPreySimulation FarmSim { get; set; } = null!;

    public SheepFarm(Logger logger, ArgsMap arguments, DataStore farmDb) : base(logger, arguments)
    {
        _farmDb = farmDb;
        UseStoredParameters();
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
        UseStoredParameters();
        FarmSim.Dump("new");
        return Content("Resetti");
    }

    private void UseStoredParameters()
    {
        FarmParams = LoadParameters();
        FarmSim = new PredatorPreySimulation(FarmParams);
        logger.Information(
            "Loaded SheepFarm parameters from {Collection}: sheep={Sheep}, wolves={Wolves}, grass={Grass}, hunt={Hunt}",
            ParametersCollectionName,
            FarmParams.Sheep,
            FarmParams.Wolves,
            FarmParams.GrassEnergy,
            FarmParams.WolfHuntEnergy);
    }

    private PredatorPreyParameters LoadParameters()
    {
        var parameters = _farmDb.GetCollection<SheepFarmParameterSet>(ParametersCollectionName);

        if (parameters.Count > 0)
            return parameters.AsQueryable().First().ToParameters();

        var seeded = PredatorPreyParameters.SheepFarmDefaults();
        parameters.InsertOne(SheepFarmParameterSet.From(seeded));
        return seeded;
    }

    public async Task<IActionResult> OnGetPlay()
    {
        try
        {
            UseStoredParameters();

            var sims_collection = _farmDb.GetCollection<SimulationRun>(SimCollectionName);
            var cts = new CancellationTokenSource();

            var old = Interlocked.Exchange(ref _simulationCts, cts);
            old.Cancel();
            old.Dispose();


            var simulations = Enumerable.Range(0, Trials)
                .Select(seed => new PredatorPreySimulation(FarmParams))
                .ToArray();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 20,
                CancellationToken = cts.Token
            };

            var results = new ConcurrentBag<SimulationRun>();

            await Parallel.ForEachAsync(simulations, options, async (simulation, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                simulation.Run(Seed, ticks: MaxTicks);

                var run = new SimulationRun(
                    simulation.Seed,
                    simulation.Parameters,
                    simulation.Snapshots);

                results.Add(run);

                await Task.CompletedTask;
            });

            logger.Information("Total sims completed: {Total}", results.Count);

            await sims_collection.InsertManyAsync(results);
        }
        catch (OperationCanceledException)
        {
            logger.Information("Simulation cancelled.");
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

    public IActionResult OnGetStop()
    {
        _simulationCts.Cancel();
        return Content("Stopped!");
    }
}