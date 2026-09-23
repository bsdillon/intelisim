using CodeMechanic.Shargs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog.Core;

namespace range;

public class RazorHatPage : PageModel
{
    protected readonly Logger logger;
    protected readonly ArgsMap arguments;
    protected readonly Action<string> printFn;

    // Convenience fields for different modes of operation:
    // Usage:  `dotnet run web --debug # runs this as a website and turns on debugging (logs, etc.)`
    //          `dotnet run --debug # runs in CLI mode, with debug`
    protected readonly bool debug;
    protected readonly bool webmode;

    public RazorHatPage(Logger logger, ArgsMap arguments)
    {
        this.logger = logger;
        this.arguments = arguments;
        this.printFn = logger.Information;
        this.debug = arguments.HasFlag("--debug");
        this.webmode = arguments.HasCommand("web");
    }
}