using BlackMesa.V2;
using CodeMechanic.Types;

namespace BlackMesa.Sims;

public sealed class Wolf(PredatorPreyModel model)
    : Agent<PredatorPreyModel>(model)
{
    public int Energy { get; private set; } = model.Parameters.StartingWolfEnergy;

    public bool IsHungry =>
        Energy < Model.Parameters.WolfReproductionThreshold;

    public bool CanReproduce =>
        Energy >= Model.Parameters.WolfReproductionThreshold;

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

        // Console.WriteLine(
        //     $"Wolf {GetHashCode()} hunted sheep {prey?.GetHashCode()}");

        if (prey is null)
            return;

        prey.Die();

        Energy += Model.Parameters.WolfHuntEnergy;
    }

    private void Reproduce()
    {
        Energy -= Model.Parameters.WolfReproductionCost;

        Model.Add(new Wolf(Model));
    }

    public void Die()
    {
        Energy = 0;
        Model.Remove(this);
    }
}