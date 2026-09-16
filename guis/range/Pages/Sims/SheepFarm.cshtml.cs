using BlackMesa.Sims;
using CodeMechanic.Diagnostics;
using CodeMechanic.Shargs;
using CodeMechanic.Types;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using Westwind.AspNetCore.Markdown.Utilities;

namespace range;

public class SheepFarm(Logger logger, ArgsMap arguments) : RazorHatPage(logger, arguments)
{
    public PredatorPreySimulation FarmSim { get; set; } = new(42);
    public PredatorPreyParameters FarmParams { get; set; } = new PredatorPreyParameters();

    public void OnGet()
    {
        FarmSim.Dump(printFn: printFn);
        FarmParams.Dump(printFn: printFn);
    }

    public IActionResult OnGetReset()
    {
        logger.Information($"{nameof(OnGetReset)}");
        FarmSim = new PredatorPreySimulation(seed: 0);
        FarmSim.Dump("new");
        return Content("Resetti");
    }

    public IActionResult OnGetPlay()
    {
        var simulation = new PredatorPreySimulation(seed: 420);

        simulation.Run(10);

        logger.Information($"Ticks:      {simulation.Model.Tick}");
        logger.Information($"Sheep:      {simulation.Model.PreyCount}");
        logger.Information($"Wolves:     {simulation.Model.PredatorCount}");
        logger.Information($"Population: {simulation.Model.Population}");
        logger.Information($"Predator:   {simulation.Model.PredatorRatio:P2}");


        return Content("Played!");
    }

    public IActionResult OnGetStep()
    {
        return Content("Stepped!");
    }
}