using System.Collections.Immutable;
using Ardalis.SmartEnum;

namespace BlackMesa;

public sealed class BatchRunner
{
    public IReadOnlyList<Ensemble> Sweep(IEnumerable<int> seeds, int ticks)
    {
        return seeds.Select(seed =>
        {
            var model = new CircuitModel(seed, north: Strategy.Cyclic, red: Strategy.Greedy);
            model.Run(ticks);
            var ensemble = Ensemble.From(model.Collector); // final capital, delivered, …
            return ensemble;
        }).ToArray();
    }
}

public sealed class Strategy : SmartEnum<Strategy>
{
    public static readonly Strategy Cyclic = new Strategy(nameof(Cyclic), 1);
    public static readonly Strategy Greedy = new Strategy(nameof(Greedy), 2);

    private Strategy(string name, int value) : base(name, value)
    {
    }
}

public class CircuitModel
{
    public CircuitModel(int seed, object north, object red)
    {
        throw new NotImplementedException();
    }

    public DataCollector Collector { get; set; }

    public void Run(int ticks)
    {
        throw new NotImplementedException();
    }
}

public class Ensemble
{
    public static Ensemble From(DataCollector modelCollector)
    {
        throw new NotImplementedException();
    }
}