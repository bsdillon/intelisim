using CodeMechanic.Diagnostics;
using CodeMechanic.FileSystem;
using CodeMechanic.Logging;
using CodeMechanic.Types;
using Serilog;
using Xunit.Abstractions;
using Log = Serilog.Log;

namespace BlackMesa.Tests.XUnitSupport;

public class XUnitBaseTest
{
    protected readonly ILogger logger;
    protected readonly string ProjectRoot; // ← always available

    public XUnitBaseTest(ITestOutputHelper output, bool debug = false)
    {
        // Force the working directory once
        ProjectRoot = ProjectPaths.Root;
        Directory.SetCurrentDirectory(ProjectRoot);

        var tool = new ToolSettings(name: "blackmesa.tests");

        if (debug) tool.Dump(nameof(tool));

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            $"{tool.dotfolder}/{tool.name}.log");

        logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output) // shows in test output + terminal
            .WriteTo.AnsiConsole()
            .WriteTo.File(path, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .WriteTo.Console() // also nice in `dotnet test`
            .CreateLogger()
            .ForContext<XUnitBaseTest>();

        if (debug)
        {
            logger.Information("logFilePath:>> " + path);
            logger.Information($"{nameof(ProjectRoot)} set to :>> {ProjectRoot}");
        }
    }
}

public static class ProjectPaths
{
    /// <summary>
    /// Absolute path to the test project root (where the .csproj lives).
    /// Cached forever after first access.
    /// </summary>
    public static string Root { get; } = ResolveRoot();

    private static string ResolveRoot()
    {
        // todo: make grepper do this
        var marker = FindUpwards(".projectroot");
        if (marker != null)
            return Path.GetDirectoryName(marker)!;

        // 2. Fallback: look for the .csproj (short, safe walk)
        var csproj = FindUpwards("*.csproj");
        if (csproj != null)
            return Path.GetDirectoryName(csproj)!;

        // 3. Last resort
        return AppContext.BaseDirectory;
    }

    private static string? FindUpwards(string pattern, int limit = 8)
    {
        if (limit < 0) throw new ArgumentOutOfRangeException(nameof(limit));

        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        // Only walk a reasonable number of levels (prevents infinite loops)
        for (int i = 0; i < limit && dir != null; i++)
        {
            var match = dir.GetFiles(pattern).FirstOrDefault()
                        ?? dir.GetDirectories(pattern).FirstOrDefault() as FileSystemInfo;

            if (match != null)
                return match.FullName;

            dir = dir.Parent;
        }

        return null;
    }
}

/// <summary>
/// Settings defined in the dotfolder of a given tool.
/// </summary>
public record ToolSettings
{
    private readonly string tools_folder = "~/.dotnet/tools".AsUnixPath();
    public string name { get; init; }

    public string settings_filename { get; init; } // "settings.json"

    public string dotfolder =>
        name.IsEmpty() ? throw new ArgumentNullException(nameof(name)) : Path.Combine(tools_folder, $".{name}");

    public string tool_settings_path => name.IsEmpty()
        ? throw new ArgumentNullException(nameof(dotfolder))
        : Path.Combine(dotfolder, settings_filename);

    // Primary constructor with optional parameters (your favorite!)
    public ToolSettings(string name, string settingsFilename = "settings.json")
    {
        this.name = name;
        settings_filename = settingsFilename;
    }

    // Parameterless constructor for maximum convenience
    public ToolSettings()
        : this("fubar", settingsFilename: "settings.json")
    {
    }

    public static ToolSettings Create(
        string name, string settingsFilename = "settings.json")
    {
        return new ToolSettings(
            name, settingsFilename
        );
    }
}

// One-time global setup (can even live in a static constructor or ModuleInitializer)

public class GlobalSerilog
{
    static GlobalSerilog()
    {
        var tool = new ToolSettings(name: nameof(Tests));
        tool.Dump(nameof(tool));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(tool.tool_settings_path)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}

// Optional: shared fixture if you want more control later
public class SerilogFixture
{
    public SerilogFixture()
    {
        // anything expensive you only want once
    }
}

[CollectionDefinition("Serilog")]
public class SerilogCollection : ICollectionFixture<SerilogFixture>
{
}