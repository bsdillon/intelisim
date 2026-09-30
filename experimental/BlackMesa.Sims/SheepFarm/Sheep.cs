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

    public bool IsHealthy => Energy >= Model.Parameters.SheepReproductionThreshold;
    public bool IsDead => Energy <= 0;

    public ASpec<Sheep>.And IsVulnerable => SheepSpecs.Hungry &
                                            !SheepSpecs.Dead;


    //

    public int Energy { get; private set; } = model.Parameters.StartingSheepEnergy;

    public bool IsHungry =>
        Energy < Model.Parameters.SheepReproductionThreshold;

    public bool CanReproduce =>
        Energy >= Model.Parameters.SheepReproductionThreshold;

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
        Energy += Model.Parameters.GrassEnergy;
    }

    private void Reproduce()
    {
        Energy -= Model.Parameters.SheepReproductionCost;

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