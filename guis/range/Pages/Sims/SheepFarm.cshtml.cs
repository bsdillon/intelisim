using BlackMesa.Sims;
using CodeMechanic.Diagnostics;
using CodeMechanic.Shargs;
using CodeMechanic.Types;
using JsonFlatFileDataStore;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using Westwind.AspNetCore.Markdown.Utilities;

namespace range;

public class SheepFarm(Logger logger, ArgsMap arguments) : RazorHatPage(logger, arguments)
{
    private DataStore farm_db;
    public PredatorPreySimulation FarmSim { get; set; } = new(42);
    public PredatorPreyParameters FarmParams { get; set; } = new PredatorPreyParameters();

    public IActionResult OnGet()
    {
        FarmSim.Dump(printFn: printFn);
        FarmParams.Dump(printFn: printFn);
        farm_db = new JsonFlatFileDataStore.DataStore("farm_db.json");
        return Page();
    }

    public IActionResult OnGetReset()
    {
        logger.Information($"{nameof(OnGetReset)}");
        FarmSim = new PredatorPreySimulation(seed: 0);
        FarmSim.Dump("new");
        return Content("Resetti");
    }

    public async Task<IActionResult> OnGetPlay()
    {
        try
        {
            var simulation = new PredatorPreySimulation(seed: 420);
            var sims = farm_db.GetCollection<PredatorPreySimulation>("simulations");

            simulation.Run(50);

            logger.Information($"Ticks:      {simulation.Model.Tick}");
            logger.Information($"Sheep:      {simulation.Model.PreyCount}");
            logger.Information($"Wolves:     {simulation.Model.PredatorCount}");
            logger.Information($"Population: {simulation.Model.Population}");
            logger.Information($"Predator:   {simulation.Model.PredatorRatio:P2}");

            simulation.Dump(printFn: printFn);
            await sims.InsertOneAsync(simulation);
        }
        catch (Exception ex)
        {
            logger.Information(ex.ToString());
            // throw;
        }


        return Partial("_SimulationComplete", FarmSim);
    }

    public IActionResult OnGetStep()
    {
        return Content("Stepped!");
    }
}