using BlackMesa.V2;
using NSpecifications;

namespace BlackMesa.Sims;

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

    public const int ReproductionThreshold = 10;

    public const int ReproductionCost = 10;

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