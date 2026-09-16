using CodeMechanic.Shargs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog.Core;

namespace range;

public class RazorHatPage : PageModel
{
    protected readonly Logger logger;
    protected readonly ArgsMap arguments;
    protected readonly Action<string> printFn;

    public RazorHatPage(Logger logger, ArgsMap arguments)
    {
        this.logger = logger;
        this.arguments = arguments;
        this.printFn = logger.Information;
    }
}