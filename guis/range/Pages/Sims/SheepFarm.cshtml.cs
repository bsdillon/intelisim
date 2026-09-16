using BlackMesa.Sims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Westwind.AspNetCore.Markdown.Utilities;

namespace range;

public class SheepFarm : PageModel
{
    public PredatorPreySimulation FarmSim { get; init; }

    public void OnGetReset()
    {
    }

    public void OnGetPlay()
    {
    }

    public void OnGetStep()
    {
    }
}