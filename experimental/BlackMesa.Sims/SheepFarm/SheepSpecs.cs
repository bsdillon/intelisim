using NSpecifications;

namespace BlackMesa.Sims;

public static class SheepSpecs
{
    public static Spec<Sheep> Hungry =>
        new(x => x.IsHungry);

    public static Spec<Sheep> Healthy =>
        new(x => x.IsHealthy);

    public static Spec<Sheep> Dead =>
        new(x => x.IsDead);
}