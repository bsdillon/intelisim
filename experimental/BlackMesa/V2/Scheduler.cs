namespace BlackMesa.V2;

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