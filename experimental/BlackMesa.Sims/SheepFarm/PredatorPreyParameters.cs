namespace BlackMesa.Sims;

public readonly record struct PredatorPreyParameters
{
    public Guid Id { get; init; }
    public int Sheep { get; init; }
    public int Wolves { get; init; }
    public int StartingSheepEnergy { get; init; }
    public int StartingWolfEnergy { get; init; }
    public int GrassEnergy { get; init; }
    public int WolfHuntEnergy { get; init; }
    public int SheepReproductionThreshold { get; init; }
    public int WolfReproductionThreshold { get; init; }
    public int SheepReproductionCost { get; init; }
    public int WolfReproductionCost { get; init; }

    // public bool IsEmpty => PetCount == 0 && string.IsNullOrEmpty(Name);

    // Primary constructor with optional parameters (your favorite!)
    public PredatorPreyParameters(
        Guid Id = default,
        int Sheep = 42,
        int Wolves = 42,
        int StartingSheepEnergy = 30,
        int StartingWolfEnergy = 30,
        int GrassEnergy = 50,
        int WolfHuntEnergy = 32,
        int SheepReproductionThreshold = 45,
        int WolfReproductionThreshold = 45,
        int SheepReproductionCost = 8,
        int WolfReproductionCost = 10
    )
    {
        this.Id = Id;
        this.Sheep = Sheep;
        this.Wolves = Wolves;
        this.StartingSheepEnergy = StartingSheepEnergy;
        this.StartingWolfEnergy = StartingWolfEnergy;
        this.GrassEnergy = GrassEnergy;
        this.WolfHuntEnergy = WolfHuntEnergy;

        this.SheepReproductionThreshold = SheepReproductionThreshold;
        this.WolfReproductionThreshold = WolfReproductionThreshold;
        this.SheepReproductionCost = SheepReproductionCost;
        this.WolfReproductionCost = WolfReproductionCost;
    }

    // Parameterless constructor for maximum convenience
    // public PredatorPreyParameters()
    //     : this( )
    // {
    // }


    // Optional: Constructor that forces named arguments (prevents ordering mistakes)
    public static PredatorPreyParameters Create(
        Guid Id = default,
        int Sheep = 42,
        int Wolves = 42,
        int StartingSheepEnergy = 30,
        int StartingWolfEnergy = 30,
        int GrassEnergy = 50,
        int WolfHuntEnergy = 32,
        int SheepReproductionThreshold = 45,
        int WolfReproductionThreshold = 45,
        int SheepReproductionCost = 8,
        int WolfReproductionCost = 10)
    {
        return new PredatorPreyParameters(
            Id: Id,
            Sheep: Sheep,
            Wolves: Wolves,
            StartingSheepEnergy: StartingSheepEnergy,
            StartingWolfEnergy: StartingWolfEnergy,
            GrassEnergy: GrassEnergy,
            WolfHuntEnergy: WolfHuntEnergy,
            SheepReproductionThreshold: SheepReproductionThreshold,
            WolfReproductionThreshold: WolfReproductionThreshold,
            SheepReproductionCost: SheepReproductionCost,
            WolfReproductionCost: WolfReproductionCost
        );
    }

    // Deconstruct method (great for tuples)
    public void Deconstruct(out Guid id, out int sheep, out int wolves, out int startingSheepEnergy,
        out int startingWolfEnergy, out int grassEnergy, out int wolfHuntEnergy, out int sheepReproductionThreshold,
        out int wolfReproductionThreshold, out int sheepReproductionCost, out int wolfReproductionCost)
    {
        id = Id;
        sheep = Sheep;
        wolves = Wolves;
        startingSheepEnergy = StartingSheepEnergy;
        startingWolfEnergy = StartingWolfEnergy;
        grassEnergy = GrassEnergy;
        wolfHuntEnergy = WolfHuntEnergy;
        sheepReproductionThreshold = SheepReproductionThreshold;
        wolfReproductionThreshold = WolfReproductionThreshold;
        sheepReproductionCost = SheepReproductionCost;
        wolfReproductionCost = WolfReproductionCost;
    }
}


//
// public record PredatorPreyParameters(
//     int Sheep,
//     int Wolves,
//     int StartingSheepEnergy,
//     int StartingWolfEnergy,
//     int GrassEnergy,
//     int WolfHuntEnergy,
//     int SheepReproductionThreshold,
//     int WolfReproductionThreshold,
//     int SheepReproductionCost,
//     int WolfReproductionCost);