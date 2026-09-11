using BlackMesa.Tests.XUnitSupport;
using BlackMesa.V2;
using CodeMechanic.Types;
using NSpecifications;
using Xunit.Abstractions;

namespace BlackMesa.Tests;

public class PredatorPreyTests : XUnitBaseTest
{
    public PredatorPreyTests(ITestOutputHelper output) : base(output, debug: false)
    {
    }

    [Fact]
    public void BasicSim()
    {
        var simulation = new PredatorPreySimulation(seed: 42);

        simulation.Run(10);

        logger.Information($"Ticks:      {simulation.Model.Tick}");
        logger.Information($"Sheep:      {simulation.Model.PreyCount}");
        logger.Information($"Wolves:     {simulation.Model.PredatorCount}");
        logger.Information($"Population: {simulation.Model.Population}");
        logger.Information($"Predator:   {simulation.Model.PredatorRatio:P2}");


        // simulation.Run(10_000);
        //
        // logger.Information(simulation.Model.Population);
        // logger.Information(simulation.Model.PredatorRatio);

        // todo: uncommnet and test the following...

        // var result = simulation.RunMany(
        //     iterations: 10_000,
        //     seed: 42);

        // var distribution = result
        //     .Select(x => x.Model.SurvivalRate)
        //     .ToArray();
    }
}

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

public sealed class Wolf(PredatorPreyModel model)
    : Agent<PredatorPreyModel>(model)
{
    public const int StartingEnergy = 30;

    public const int HuntEnergy = 15;

    public const int ReproductionThreshold = 45;

    public const int ReproductionCost = 20;

    public int Energy { get; private set; } = StartingEnergy;

    public bool IsHungry =>
        Energy < 15;

    public bool CanReproduce =>
        Energy >= ReproductionThreshold;

    public bool IsDead => Energy <= 0;

    public bool Alive => !IsDead;

    public override void Step()
    {
        if (!Alive)
            return;

        Energy--;

        Hunt();

        if (CanReproduce)
            Reproduce();

        if (Energy <= 0)
            Die();
    }

    private void Hunt()
    {
        var prey = Model.Sheep
            .TakeFirstRandom();

        Console.WriteLine(
            $"Wolf {GetHashCode()} hunted sheep {prey?.GetHashCode()}");

        if (prey is null)
            return;

        prey.Die();

        Energy += HuntEnergy;
    }

    private void Reproduce()
    {
        Energy -= ReproductionCost;

        Model.Add(new Wolf(Model));
    }

    public void Die()
    {
        Energy = 0;
        Model.Remove(this);
    }
}

public sealed class Farm : Model
{
    public List<Sheep> Sheep { get; } = [];
    public List<Wolf> Wolves { get; } = [];

    public int Population => Sheep.Count + Wolves.Count;
    public int Predators => Wolves.Count;

    public double PredatorRatio =>
        Population == 0
            ? 0
            : (double)Predators / Population;

    public Farm(int? seed = null) : base(seed)
    {
    }

    // protected override void TickEnvironment()
    // {
    // }

    public void Remove(Sheep sheep)
    {
        throw new NotImplementedException();
    }
}

public sealed class Sheep(PredatorPreyModel model) : Agent<PredatorPreyModel>(model)
// public sealed class Sheep(Farm model) : Agent<Farm>(model)
{
    // private void Die()
    // {
    //     Model.Sheep.Remove(this);
    // }

    public bool IsHealthy => Energy >= 50;
    public bool IsDead => Energy <= 0;

    public ASpec<Sheep>.And IsVulnerable => SheepSpecs.Hungry &
                                            !SheepSpecs.Dead;


    //

    public const int StartingEnergy = 20;

    public const int GrassEnergy = 4;

    public const int ReproductionThreshold = 30;

    public const int ReproductionCost = 15;

    public int Energy { get; private set; } = StartingEnergy;

    public bool IsHungry =>
        Energy < 10;

    public bool CanReproduce =>
        Energy >= ReproductionThreshold;

    public override void Step()
    {
        if (!Alive)
            return;

        Energy--;

        Eat();

        if (CanReproduce)
            Reproduce();

        if (Energy <= 0)
            Die();
    }

    public bool Alive => !IsDead;

    private void Eat()
    {
        Energy += GrassEnergy;
    }

    private void Reproduce()
    {
        Energy -= ReproductionCost;

        Model.Add(new Sheep(Model));
    }

    public void Die()
    {
        Kill();
        Model.Remove(this);
    }

    private void Kill()
    {
        Energy = 0;
    }

    public void LoseEnergy(int amount)
    {
        Energy -= amount;

        if (Energy <= 0)
            Die();
    }
}

public static class SheepSpecs
{
    public static Spec<Sheep> Hungry =>
        new(x => x.IsHungry);

    public static Spec<Sheep> Healthy =>
        new(x => x.IsHealthy);

    public static Spec<Sheep> Dead =>
        new(x => x.IsDead);
}

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