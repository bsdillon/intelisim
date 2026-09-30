namespace BlackMesa.Sims;

// JsonFlatFileDataStore collections require a class. This is the Parameters document.
public sealed class SheepFarmParameterSet
{
    // String, not Guid: the datastore writes the key back as a string and cannot set a Guid property.
    public string Id { get; set; } = "";
    public int Sheep { get; set; }
    public int Wolves { get; set; }
    public int StartingSheepEnergy { get; set; }
    public int StartingWolfEnergy { get; set; }
    public int GrassEnergy { get; set; }
    public int WolfHuntEnergy { get; set; }
    public int SheepReproductionThreshold { get; set; }
    public int WolfReproductionThreshold { get; set; }
    public int SheepReproductionCost { get; set; }
    public int WolfReproductionCost { get; set; }

    public PredatorPreyParameters ToParameters() => new(
        Id: Guid.TryParse(Id, out var id) ? id : Guid.Empty,
        Sheep: Sheep,
        Wolves: Wolves,
        StartingSheepEnergy: StartingSheepEnergy,
        StartingWolfEnergy: StartingWolfEnergy,
        GrassEnergy: GrassEnergy,
        WolfHuntEnergy: WolfHuntEnergy,
        SheepReproductionThreshold: SheepReproductionThreshold,
        WolfReproductionThreshold: WolfReproductionThreshold,
        SheepReproductionCost: SheepReproductionCost,
        WolfReproductionCost: WolfReproductionCost);

    public static SheepFarmParameterSet From(PredatorPreyParameters parameters) => new()
    {
        Id = parameters.Id.ToString(),
        Sheep = parameters.Sheep,
        Wolves = parameters.Wolves,
        StartingSheepEnergy = parameters.StartingSheepEnergy,
        StartingWolfEnergy = parameters.StartingWolfEnergy,
        GrassEnergy = parameters.GrassEnergy,
        WolfHuntEnergy = parameters.WolfHuntEnergy,
        SheepReproductionThreshold = parameters.SheepReproductionThreshold,
        WolfReproductionThreshold = parameters.WolfReproductionThreshold,
        SheepReproductionCost = parameters.SheepReproductionCost,
        WolfReproductionCost = parameters.WolfReproductionCost
    };
}
