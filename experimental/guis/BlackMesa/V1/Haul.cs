namespace BlackMesa;

public readonly record struct HaulParams(
    int Seed,
    int Trials,
    double Distance,
    double Load,
    double Price,
    double SpeedSpend,
    double Security,
    double WeatherSigma,
    double LossMean,
    double Deadline,
    bool UseDeadline);

public readonly record struct Haul(
    double Distance = 12,
    double Load = 40,
    double Price = 8,
    double SpeedSpend = 0.35,
    double Security = 0.45,
    double WeatherSigma = 0.35,
    double LossMean = 0.22,
    double Deadline = 14,
    bool UseDeadline = true,
    int Seed = 42)
{
    public double Kt => 1.15 - SpeedSpend * 0.7;

    public HaulResult Run(SeededRng rng)
    {
        var foo = rng.Normal(0.2, WeatherSigma);
        var w = Math.Max(0, foo);
        var loss = Math.Clamp(rng.Normal(LossMean, 0.12), 0, 0.95);
        var t = Kt * Distance * (1 + w);
        var late = UseDeadline && t > Deadline;
        var q = late ? 0 : Load * (1 - loss * (1 - Security));
        var cost = Distance * (1 + Kt + Security) * (Load / 10);
        return new(t, q, cost, q * Price - cost, late, w, loss);
    }

    public HaulResult[] Ensemble(int n)
    {
        var root = new SeededRng(Seed);

        // because structs cannot access 'this', I've added this hack:
        // todo: look into whether assigning a temporary var to 'this' affects anything adversely, sim-wise:
        var haul = this;
        return Enumerable.Range(1, n).Select(i => haul.Run(root.Fork(i))).ToArray();
    }
}

public readonly record struct HaulResult(
    double Time,
    double Delivered,
    double Cost,
    double Profit,
    bool Late,
    double Weather,
    double Loss)
{
    public override string ToString() =>
        $"t={Time:0.00} q={Delivered:0.00} p={Profit:0.00} late={Late} w={Weather:0.00} loss={Loss:0.00}";
}

//
// public static class Haul
// {
//     public static HaulResult Run(HaulParams p, SeededRng stream) { /* RANGE kernel */ }
//
//     public static Ensemble RunMany(HaulParams p)
//     {
//         var root = new SeededRng(p.Seed);
//         var xs = Enumerable.Range(1, p.Trials)
//             .Select(i => Run(p, root.Fork(i)))
//             .ToArray();
//         return Ensemble.From(xs); // mean, σ, p05, p50, p95, P(p>0), P(late)
//     }
// }