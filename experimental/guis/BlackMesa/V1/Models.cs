namespace BlackMesa;

public sealed class SeededRng
{
    public SeededRng(int i)
    {
        throw new NotImplementedException();
    }

    // One stream per seed. Fork(salt) for haul i, ship j, weather, etc.
    // RANGE Lab 01: same seed ⇒ same histogram. Whitepaper: n seeds = scenario identity.
    public SeededRng Fork(int i)
    {
        throw new NotImplementedException();
    }

    public double Normal(double lossMean, double p1)
    {
        throw new NotImplementedException();
    }
}

public abstract class Agent
{
    public required string Id { get; init; }
    public abstract void Step(World world, SeededRng rng);

    public virtual void Sense(World w)
    {
    }

    public virtual void Decide(World w, SeededRng rng)
    {
    }

    public virtual void Act(World w, SeededRng rng)
    {
    }
}

public record struct World
{
}

public interface ISpace
{
    // RANGE: six ports + edges. Later: grid, continuous, graph.
    IReadOnlyList<PortId> Neighbors(PortId id);
    double Distance(PortId a, PortId b);
}

public record struct PortId
{
}

public interface IScheduler
{
    IEnumerable<Agent> Order(IReadOnlyList<Agent> agents, SeededRng rng);
}

public abstract class DataCollector
{
    public abstract void Collect(World world); // one row per tick

    public IReadOnlyList<Snapshot> Series { get; }
}

public record struct Snapshot
{
}

public abstract class Model
{
    public int T { get; protected set; }
    public required int Seed { get; init; }
    protected SeededRng Rng { get; }
    protected ISpace Space { get; }
    protected IScheduler Scheduler { get; }
    protected DataCollector Collector { get; }
    protected List<Agent> Agents { get; }

    public void Step()
    {
        T++;
        TickEnvironment(); // prices, holdings, contracts
        foreach (var a in Scheduler.Order(Agents, Rng))
            a.Step(this.World, Rng); // trade + choose + sail
        Collector.Collect(World);
    }

    public World World { get; set; }

    public void Run(int ticks)
    {
        for (var i = 0; i < ticks; i++) Step();
    }

    protected abstract void TickEnvironment();
}