using BlackMesa.Sims;
using BlackMesa.Tests.XUnitSupport;
using Xunit;
using Xunit.Abstractions;

namespace BlackMesa.Tests;

public class PredatorPreyTests : XUnitBaseTest
{
    public PredatorPreyTests(ITestOutputHelper output) : base(output, debug: false)
    {
    }

    [Fact]
    public void ParameterSetRoundTrips()
    {
        var defaults = PredatorPreyParameters.SheepFarmDefaults();
        var again = SheepFarmParameterSet.From(defaults).ToParameters();

        Assert.Equal(defaults, again);
    }

    [Fact]
    public void AgentsStartFromParameters()
    {
        var farm_params = PredatorPreyParameters.SheepFarmDefaults();
        var simulation = new PredatorPreySimulation(farm_params);

        simulation.Run(42, ticks: 0);

        Assert.Equal(farm_params.Sheep, simulation.Model.PreyCount);
        Assert.Equal(farm_params.Wolves, simulation.Model.PredatorCount);
        Assert.Equal(farm_params.StartingSheepEnergy, simulation.Model.Sheep[0].Energy);
        Assert.Equal(farm_params.StartingWolfEnergy, simulation.Model.Wolves[0].Energy);
        Assert.Equal(42, simulation.Seed);
    }

    [Fact]
    public void BasicSim()
    {
        var farm_params = PredatorPreyParameters.SheepFarmDefaults();
        var simulation = new PredatorPreySimulation(parameters: farm_params);

        simulation.Run(42, 50);

        logger.Information($"Ticks:      {simulation.Model.Tick}");
        logger.Information($"Sheep:      {simulation.Model.PreyCount}");
        logger.Information($"Wolves:     {simulation.Model.PredatorCount}");
        logger.Information($"Population: {simulation.Model.Population}");
        logger.Information($"Predator:   {simulation.Model.PredatorRatio:P2}");

        var peak = simulation.Snapshots.Max(snapshot => snapshot.Population);
        logger.Information($"Peak:       {peak}");

        Assert.Equal(50, simulation.Snapshots.Count);
        Assert.Equal(42, simulation.Seed);
        Assert.Contains(simulation.Snapshots, snapshot => snapshot.Tick == 16);
        Assert.Contains(simulation.Snapshots, snapshot => snapshot.Tick == 17);
        Assert.All(simulation.Snapshots, snapshot => Assert.True(snapshot.Population < 1_000));

        // todo: uncomment and test the following...

        // var result = simulation.RunMany(
        //     iterations: 10_000,
        //     seed: 42);

        // var distribution = result
        //     .Select(x => x.Model.SurvivalRate)
        //     .ToArray();
    }
}