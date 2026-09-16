using BlackMesa.V2;
using CodeMechanic.Types;

namespace BlackMesa.Sims;

public sealed class Wolf(PredatorPreyModel model)
    : Agent<PredatorPreyModel>(model)
{
    public const int StartingEnergy = 30;

    public const int HuntEnergy = 15;

    public const int ReproductionThreshold = 45;

    public const int ReproductionCost = 20;

    public int Energy { get; private set; } = StartingEnergy;

    public bool IsHungry =>
        Energy < 15;

    public bool CanReproduce =>
        Energy >= ReproductionThreshold;

    public bool IsDead => Energy <= 0;

    public bool Alive => !IsDead;

    public override void Step()
    {
        if (!Alive)
            return;

        Energy--;

        Hunt();

        if (CanReproduce)
            Reproduce();

        if (Energy <= 0)
            Die();
    }

    private void Hunt()
    {
        var prey = Model.Sheep.TakeFirstRandom(Model.Random);

        Console.WriteLine(
            $"Wolf {GetHashCode()} hunted sheep {prey?.GetHashCode()}");

        if (prey is null)
            return;

        prey.Die();

        Energy += HuntEnergy;
    }

    private void Reproduce()
    {
        Energy -= ReproductionCost;

        Model.Add(new Wolf(Model));
    }

    public void Die()
    {
        Energy = 0;
        Model.Remove(this);
    }
}