namespace BlackMesa.Range.Lab01;

public sealed class SeededRng
{
    uint _s;
    readonly int _seed;

    // public SeededRng(int seed) => _s = (uint)seed;
    public SeededRng(int seed)
    {
        _seed = seed;
        _s = (uint)seed;
    }

    public SeededRng Fork(int salt) =>
        new(unchecked((int)((uint)_seed ^ (uint)((salt + 1) * unchecked((int)0x9E3779B9)))));

    public double Next()
    {
        _s += 0x6D2B79F5;
        var t = (uint)((int)(_s ^ (_s >> 15)) * (int)(1 | _s));
        t = (t + (uint)((int)(t ^ (t >> 7)) * (int)(61 | t))) ^ t;
        return (_s = t ^ (t >> 14)) / 4294967296.0;
    }

    public double Normal(double mean, double std)
    {
        var u = Math.Max(1e-12, Next());
        var v = Next();
        return mean + std * Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
    }
}