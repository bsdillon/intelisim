using System.Collections.Concurrent;
using System.Collections.Immutable;
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

    public async Task<IActionResult> OnGet()
    {
        if (debug) FarmSim.Dump(printFn: printFn);
        if (debug) FarmParams.Dump(printFn: printFn);

        // farm_db = new JsonFlatFileDataStore.DataStore("farm_db.json");
        await RemoveUnasignedSimulations();
        return Page();
    }

    public IActionResult OnGetReset()
    {
        logger.Information($"{nameof(OnGetReset)}");
        UseStoredParameters();
        FarmSim.Dump("new");
        return Content("Resetti");
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

            await RemoveUnasignedSimulations();
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

    private async Task<bool> RemoveUnasignedParameters()
    {
        var unassigned_parameters = FindAnyUnassignedParameters();

        unassigned_parameters.Dump(nameof(unassigned_parameters), printFn: logger.Information);

        if (unassigned_parameters.Length == 0)
            return false;

        var parameters_doc = _farmDb.GetCollection<SheepFarmParameterSet>(ParametersCollectionName);

        // DeleteOneAsync(object) matches the document id, not the entity.
        return await parameters_doc.DeleteManyAsync(p => p.Id.ToGuid() == Guid.Empty);
    }

    private ImmutableArray<SheepFarmParameterSet> FindAnyUnassignedParameters()
    {
        return _farmDb.GetCollection<SheepFarmParameterSet>(ParametersCollectionName)
            .AsQueryable()
            .Where(p => p.Id.ToGuid() == Guid.Empty)
            .ToImmutableArray();
    }

    private async Task<bool> RemoveUnasignedSimulations()
    {
        var unassigned_simulations = FindAnyUnassignedSimulations();

        unassigned_simulations
            .Select(run => run.Parameters.Id)
            .Dump(nameof(unassigned_simulations), printFn: logger.Information);

        if (unassigned_simulations.Length == 0)
            return false;

        var simulations_doc = _farmDb.GetCollection<SimulationRun>(SimCollectionName);

        // Simulation runs have no document id. The empty guid is parameters.id.
        return await simulations_doc.DeleteManyAsync(run => run.Parameters.Id == Guid.Empty);
    }

    private ImmutableArray<SimulationRun> FindAnyUnassignedSimulations()
    {
        return _farmDb.GetCollection<SimulationRun>(SimCollectionName)
            .AsQueryable()
            .Where(run => run.Parameters.Id == Guid.Empty)
            .ToImmutableArray();
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
        var parameters_doc = _farmDb.GetCollection<SheepFarmParameterSet>(ParametersCollectionName);

        if (parameters_doc.Count > 0)
            return parameters_doc.AsQueryable().First().ToParameters();

        var unassignedParameters = FindAnyUnassignedParameters();

        if (unassignedParameters.Length > 0)
            Task.Run(async () => { await RemoveUnasignedParameters(); });

        var seeded = PredatorPreyParameters.SheepFarmDefaults();
        parameters_doc.InsertOne(SheepFarmParameterSet.From(seeded));
        return seeded;
    }
}

// Todo: add these methods to CodeMechanic.Types library
public static class TypeExtensions
{
    public static Guid ToGuid(this string text, Guid? fallback = null)
    {
        if (!fallback.HasValue) fallback = Guid.Empty;
        var ret = Guid.TryParse(text, out var id) ? id : fallback.Value;
        return ret;
    }
}