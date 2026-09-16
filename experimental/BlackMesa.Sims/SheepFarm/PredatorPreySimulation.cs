using BlackMesa.V2;

namespace BlackMesa.Sims;

public sealed class PredatorPreySimulation : Simulation
{
    public PredatorPreyModel Model { get; }

    public PredatorPreySimulation(int? seed = null)
    {
        Model = new PredatorPreyModel(seed);

        // Model = new PredatorPreyModel
        // {
        //     Seed = seed ?? Environment.TickCount
        // };
    }

    protected override void Initialize()
    {
        for (var i = 0; i < 100; i++)
            Model.Add(new Sheep(Model));

        for (var i = 0; i < 20; i++)
            Model.Add(new Wolf(Model));
    }

    protected override void Step()
    {
        Model.Step();
    }
}