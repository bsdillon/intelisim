using NSpecifications;

namespace BlackMesa.V2;

public abstract class Simulation
{
    public int Tick { get; private set; }

    public void Run(int ticks)
    {
        Initialize();

        for (Tick = 0; Tick < ticks; Tick++)
        {
            BeforeStep();
            Step();
            AfterStep();
        }

        Complete();
    }

    protected virtual void Initialize()
    {
    }

    protected abstract void Step();

    protected virtual void BeforeStep()
    {
    }

    protected virtual void AfterStep()
    {
    }

    protected virtual void Complete()
    {
    }
}

public abstract class Model(int? seed = null)
{
    public Random Random { get; } =
        seed.HasValue
            ? new Random(seed.Value)
            : Random.Shared;

    public int Tick { get; protected internal set; }

    public virtual void Step()
    {
        Tick++;
    }
}

public abstract class Agent<TModel>(TModel model)
{
    public TModel Model { get; } = model;

    public virtual void Step()
    {
    }
}

public class Scheduler<TAgent>
{
    protected readonly List<TAgent> Agents = [];

    public IReadOnlyList<TAgent> Items => Agents;

    public virtual void Add(TAgent agent)
        => Agents.Add(agent);

    public virtual void Remove(TAgent agent)
        => Agents.Remove(agent);

    public virtual void Step(Action<TAgent> action)
    {
        foreach (var agent in Agents.ToArray())
            action(agent);
    }

    // private readonly List<TAgent> _agents = [];
    //
    // public IReadOnlyList<TAgent> Agents => _agents;
    // public IReadOnlyList<TAgent> Items => Agents;
    //
    //
    // // public void Add(TAgent agent)
    // //     => _agents.Add(agent);
    //
    // public void Remove(TAgent agent)
    //     => _agents.Remove(agent);
    //
    // public virtual void Step(Action<TAgent> action)
    // {
    //     foreach (var agent in _agents)
    //         action(agent);
    // }
    //
    //
    // public virtual void Add(TAgent agent)
    //     => _agents.Add(agent);
    //
    // public virtual void Remove(TAgent agent)
    //     => Agents.Remove(agent);
    //
    // public virtual void Step(Action<TAgent> action)
    // {
    //     foreach (var agent in Agents.ToArray())
    //         action(agent);
    // }
}
