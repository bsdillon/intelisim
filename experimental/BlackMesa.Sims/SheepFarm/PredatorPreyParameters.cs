namespace BlackMesa.Sims;

public record PredatorPreyParameters(
    int Sheep,
    int Wolves,
    int StartingSheepEnergy,
    int StartingWolfEnergy,
    int GrassEnergy,
    int WolfHuntEnergy,
    int SheepReproductionThreshold,
    int WolfReproductionThreshold,
    int SheepReproductionCost,
    int WolfReproductionCost);