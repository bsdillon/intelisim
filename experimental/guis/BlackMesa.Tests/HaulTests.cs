using BlackMesa;

namespace BlackMesa.Tests;

public class HaulTests
{
    [Fact]
    public void Seed_42_fork_1_never_moves()
    {
        var r = new Haul().Run(new SeededRng(42).Fork(1));
        Assert.Equal(13.86, r.Time, 2);
        Assert.Equal(115.73, r.Profit, 2);
        Assert.False(r.Late);
    }

    [Fact]
    public void Seed_42_n_400_deadline_on()
    {
        var xs = new Haul().Ensemble(400);
        Assert.Equal(44.65, xs.Average(x => x.Profit), 2);
        Assert.Equal(0.44, xs.Count(x => x.Late) / 400.0, 2);
    }
}