using BlackMesa.Tests.XUnitSupport;
using BlackMesa.V2;
using CodeMechanic.Types;
using NSpecifications;
using Xunit;
using Xunit.Abstractions;

namespace BlackMesa.Tests;

public class PredatorPreyTests : XUnitBaseTest
{
    public PredatorPreyTests(ITestOutputHelper output) : base(output, debug: false)
    {
    }

    [Fact]
    public void BasicSim()
    {
        var simulation = new PredatorPreySimulation(seed: 420);

        simulation.Run(10);

        logger.Information($"Ticks:      {simulation.Model.Tick}");
        logger.Information($"Sheep:      {simulation.Model.PreyCount}");
        logger.Information($"Wolves:     {simulation.Model.PredatorCount}");
        logger.Information($"Population: {simulation.Model.Population}");
        logger.Information($"Predator:   {simulation.Model.PredatorRatio:P2}");


        // simulation.Run(10_000);
        //
        // logger.Information(simulation.Model.Population);
        // logger.Information(simulation.Model.PredatorRatio);

        // todo: uncommnet and test the following...

        // var result = simulation.RunMany(
        //     iterations: 10_000,
        //     seed: 42);

        // var distribution = result
        //     .Select(x => x.Model.SurvivalRate)
        //     .ToArray();
    }
}