using BlackMesa.V2;

namespace BlackMesa.Sims;

public sealed class PredatorPreyModel : Model
{
    public PredatorPreyModel(int? seed = null)
        : base(seed)
    {
        SheepScheduler = new RandomScheduler<Sheep>(Random);
        WolfScheduler = new RandomScheduler<Wolf>(Random);
    }

    public override void Step()
    {
        // Tick++;

        base.Step(); // Do I need this over Tick?

        // todo: inject a printfn, like you do with the Dump method. - Nick.
        Console.WriteLine(
            $"T={Tick} Sheep={Sheep.Count} Wolves={Wolves.Count}");

        SheepScheduler.Step(sheep => sheep.Step());
        WolfScheduler.Step(wolf => wolf.Step());
    }

    public List<Sheep> Sheep { get; } = [];

    public List<Wolf> Wolves { get; } = [];

    public RandomScheduler<Sheep> SheepScheduler { get; }

    public RandomScheduler<Wolf> WolfScheduler { get; }

    public int Population =>
        Sheep.Count + Wolves.Count;

    public int PreyCount =>
        Sheep.Count;

    public int PredatorCount =>
        Wolves.Count;

    public double PredatorRatio =>
        Population == 0
            ? 0
            : (double)PredatorCount / Population;

    public double PreyRatio =>
        Population == 0
            ? 0
            : (double)PreyCount / Population;

    public void Add(Sheep sheep)
    {
        Sheep.Add(sheep);
        SheepScheduler.Add(sheep);
    }

    public void Add(Wolf wolf)
    {
        Wolves.Add(wolf);
        WolfScheduler.Add(wolf);
    }

    public void Remove(Sheep sheep)
    {
        Sheep.Remove(sheep);
        SheepScheduler.Remove(sheep);
    }

    public void Remove(Wolf wolf)
    {
        Wolves.Remove(wolf);
        WolfScheduler.Remove(wolf);
    }

    // protected override void TickEnvironment()
    // {
    //     throw new NotImplementedException();
    // }
}