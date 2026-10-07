using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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

    private static readonly TimeSpan TickDelay = TimeSpan.FromMilliseconds(80);
    private static readonly JsonSerializerOptions ChartJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly record struct FarmPopulationPoint(int Tick, int Sheep, int Wolves);

    private readonly DataStore _farmDb;
    private readonly IRazorPartialRenderer _razor;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    public int Trials { get; set; } = 1;
    public int Seed { get; set; } = 42;
    public int MaxTicks { get; set; } = 120;

    private static CancellationTokenSource _simulationCts = new();

    [BindNever] public PredatorPreyParameters FarmParams { get; set; }

    public PredatorPreySimulation FarmSim { get; set; } = null!;

    public SimulationSnapshot ShownSnapshot { get; private set; } = new(0, 0, 0, 0, 0);

    public SheepFarm(Logger logger, ArgsMap arguments, DataStore farmDb, IRazorPartialRenderer razor) : base(logger, arguments)
    {
        _farmDb = farmDb;
        _razor = razor;
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
            var cts = new CancellationTokenSource();
            var old = Interlocked.Exchange(ref _simulationCts, cts);
            old.Cancel();
            old.Dispose();

            await RunAndStoreAsync(cts.Token);
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

    public async Task<IActionResult> OnGetWs()
    {
        logger.Information($"{nameof(OnGetWs)}");
        var is_not_a_ws_request = !HttpContext.WebSockets.IsWebSocketRequest;
        logger.Information($"{nameof(is_not_a_ws_request)} :>> {is_not_a_ws_request}");

        if (is_not_a_ws_request)
            return StatusCode(StatusCodes.Status400BadRequest);

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        logger.Information("Websocket established.");

        try
        {
            await RunSocketAsync(socket, HttpContext.RequestAborted);
        }
        catch (WebSocketException exception)
        {
            logger.Information($"{nameof(exception)} :>> {exception}");
        }
        catch (OperationCanceledException exception)
        {
            logger.Information($"{nameof(exception)} :>> {exception}");
        }

        logger.Information("WS call completed.");
        return new EmptyResult();
    }

    private async Task RunSocketAsync(WebSocket socket, CancellationToken httpCt)
    {
        var buffer = new byte[8 * 1024];

        while (socket.State == WebSocketState.Open && !httpCt.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(buffer, httpCt);
            if (result.MessageType == WebSocketMessageType.Close) break;

            var cmd = ReadCmd(buffer, result.Count);
            logger.Information($"{nameof(cmd)} :>> {cmd}");

            switch (cmd)
            {
                case "play":
                case "start":
                    var playCts = CancellationTokenSource.CreateLinkedTokenSource(httpCt);
                    var old = Interlocked.Exchange(ref _simulationCts, playCts);
                    old.Cancel();
                    old.Dispose();
                    _ = StreamAsync(socket, playCts.Token);
                    break;
                case "stop":
                    _simulationCts.Cancel();
                    await SendPartial(socket, "_Status", "Stopped");
                    break;
            }
        }
    }

    private async Task StreamAsync(WebSocket socket, CancellationToken ct)
    {
        try
        {
            await SendPartial(socket, "_Status", "Running…");
            var snapshots = await RunAndStoreAsync(ct);
            logger.Information("Streaming {Ticks} ticks", snapshots.Count);

            var populations = new List<int>(snapshots.Count);
            var series = new List<FarmPopulationPoint>(snapshots.Count);
            for (var i = 0; i < snapshots.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                populations.Add(snapshots[i].Population);
                series.Add(new FarmPopulationPoint(snapshots[i].Tick, snapshots[i].Sheep, snapshots[i].Wolves));
                await SendHtml(socket, await RenderTickMessage(snapshots[i], populations, series));
                if (i < snapshots.Count - 1)
                    await Task.Delay(TickDelay, ct);
            }

            if (snapshots.Count == 0)
            {
                await SendPartial(socket, "_Status", "No ticks.");
                return;
            }

            var last = snapshots[^1];
            await SendPartial(socket, "_Status",
                $"Done at tick {last.Tick}: {last.Sheep} sheep, {last.Wolves} wolves, population {last.Population}");
        }
        catch (OperationCanceledException)
        {
            logger.Information("Tick stream cancelled.");
        }
        catch (ObjectDisposedException)
        {
            logger.Information("Tick stream cancelled.");
        }
        catch (Exception ex)
        {
            logger.Information(ex.ToString());
            await SendPartial(socket, "_Status", "Simulation failed.");
            if (debug) throw;
        }
    }

    private async Task<string> RenderTickMessage(
        SimulationSnapshot snapshot,
        IReadOnlyList<int> populations,
        IReadOnlyList<FarmPopulationPoint> series)
    {
        var tick = await _razor.RenderAsync(HttpContext, "/Pages/Sims/Farm/_FarmTick.cshtml", snapshot);
        var status = await _razor.RenderAsync(HttpContext, "/Pages/Sims/_Status.cshtml",
            $"Tick {snapshot.Tick}: {snapshot.Sheep} sheep, {snapshot.Wolves} wolves, population {snapshot.Population}");
        var histogram = await _razor.RenderAsync(HttpContext, "/Pages/Sims/Farm/_FarmHistogram.cshtml",
            JsonSerializer.Serialize(populations));
        var population = await _razor.RenderAsync(HttpContext, "/Pages/Sims/Farm/_FarmPopulation.cshtml",
            JsonSerializer.Serialize(series, ChartJson));
        return tick + status + histogram + population;
    }

    private async Task<IReadOnlyList<SimulationSnapshot>> RunAndStoreAsync(CancellationToken ct)
    {
        UseStoredParameters();

        var sims_collection = _farmDb.GetCollection<SimulationRun>(SimCollectionName);
        var simulations = Enumerable.Range(0, Trials)
            .Select(seed => new PredatorPreySimulation(FarmParams))
            .ToArray();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 20,
            CancellationToken = ct
        };

        var results = new ConcurrentBag<SimulationRun>();

        await Parallel.ForEachAsync(simulations, options, async (simulation, token) =>
        {
            token.ThrowIfCancellationRequested();

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

        return simulations.SelectMany(simulation => simulation.Snapshots).ToArray();
    }

    private async Task SendPartial(WebSocket socket, string name, object? model)
    {
        logger.Information($"sending partial '{name}'");
        var html = await _razor.RenderAsync(HttpContext, $"/Pages/Sims/{name}.cshtml", model);
        await SendHtml(socket, html);
    }

    private async Task SendHtml(WebSocket socket, string html)
    {
        if (socket.State != WebSocketState.Open)
            return;

        var bytes = Encoding.UTF8.GetBytes(html);
        await _sendLock.WaitAsync();
        try
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private static string ReadCmd(byte[] buffer, int count)
    {
        using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(buffer, 0, count));
        return doc.RootElement.TryGetProperty("cmd", out var c) ? c.GetString() ?? "" : "";
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
        var population = FarmParams.Sheep + FarmParams.Wolves;
        ShownSnapshot = new SimulationSnapshot(
            0,
            FarmParams.Sheep,
            FarmParams.Wolves,
            population,
            population == 0 ? 0 : (double)FarmParams.Wolves / population);
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