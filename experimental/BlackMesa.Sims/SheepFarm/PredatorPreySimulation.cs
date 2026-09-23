using BlackMesa.V2;

namespace BlackMesa.Sims;

public sealed class PredatorPreySimulation : Simulation
{
    public PredatorPreyModel Model { get; }
    public int Seed { get; init; }

    public List<SimulationSnapshot> Snapshots { get; } = [];
    public PredatorPreyParameters Parameters { get; }

    public PredatorPreySimulation(
        PredatorPreyParameters parameters,
        int seed)
    {
        Parameters = parameters;
        Seed = seed;
        Model = new PredatorPreyModel(seed);
    }
    
    //
    // public PredatorPreySimulation(int? seed = null)
    // {
    //     Model = new PredatorPreyModel(seed);
    //     this.Seed = seed ?? 0;
    //
    //     // Model = new PredatorPreyModel
    //     // {
    //     //     Seed = seed ?? Environment.TickCount
    //     // };
    // }

    protected override void Initialize()
    {
        for (var i = 0; i < Parameters.Sheep; i++)
            Model.Add(new Sheep(Model));

        for (var i = 0; i < Parameters.Wolves; i++)
            Model.Add(new Wolf(Model));
    }
    
    // protected override void Initialize()
    // {
    //     for (var i = 0; i < 100; i++)
    //         Model.Add(new Sheep(Model));
    //
    //     for (var i = 0; i < 20; i++)
    //         Model.Add(new Wolf(Model));
    // }

    protected override void Step()
    {
        Model.Step();
        Snapshots.Add(new SimulationSnapshot(
            Model.Tick,
            Model.PreyCount,
            Model.PredatorCount,
            Model.Population,
            Model.PredatorRatio
        ));
    }
}

// public sealed record SimulationResult(
//     int Seed,
//     int Ticks,
//     int Sheep,
//     int Wolves,
//     int Population,
//     double PredatorRatio
// );

public sealed record SimulationRun(
    int Seed,
    PredatorPreyParameters Parameters,
    IReadOnlyList<SimulationSnapshot> Snapshots
);

public sealed record SimulationSnapshot(
    int Tick,
    int Sheep,
    int Wolves,
    int Population,
    double PredatorRatio
);