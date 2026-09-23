using BlackMesa.V2;

namespace BlackMesa.Sims;

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

    public Farm(int? seed = null) //: base(seed)
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