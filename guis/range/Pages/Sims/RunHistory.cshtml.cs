using System.Text.RegularExpressions;
using CodeMechanic.Diagnostics;
using CodeMechanic.FileSystem;
using CodeMechanic.RegularExpressions;
using CodeMechanic.Shargs;
using CodeMechanic.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog.Core;
using static System.IO.File;

// using File = System.IO.File;

namespace range;

public class RunHistory : PageModel
{
    private readonly Logger logger;
    private readonly ArgsMap arguments;

    public RunHistory(Logger logger, ArgsMap args)
    {
        this.logger = logger;
        this.arguments = args;
    }

    private Grepper blackmesa_log_search = new Grepper()
    {
        RootPath = "~/.dotnet/tools/.blackmesa.tests/".AsUnixPath(),
        DirectoryPattern = DirectoryPatterns.StandardDirectoryBlacklist(),
        FileSearchMask = "*log",
        Recursive = true
    };

    private static string[] logfiles = [];

    public string[] Logs => logfiles;

    public string Search { get; set; } = "foo";

    public async Task<IActionResult> OnPostSearch(string search)
    {
        Search = search;
        logger.Information($"{nameof(OnPostSearch)}{nameof(search)} :>> {search}");
        var logs = SearchLogs(search);
        return Partial("_SerilogRecords", logs);
    }

    public List<SerilogRecord> SearchLogs(string search = "")
    {
        try
        {
            blackmesa_log_search.Dump("looking for logs", printFn: logger.Information);

            logfiles = blackmesa_log_search.GetFileNames().ToArray();
            logfiles.Dump(printFn: logger.Information);

            var serilog_records = logfiles
                .Select(f => ReadAllText(f)
                    .Extract<SerilogRecord>(SerilogPatterns.Basic()))
                .Flatten()
                .If(search.NotEmpty(),
                    list => list.Where(x => x.content.Contains(search, StringComparison.OrdinalIgnoreCase))
                )
                .ToList();

            serilog_records.Dump("existing logs");

            return serilog_records;
        }
        catch (Exception e)
        {
            logger.Error(e?.ToString());
            // throw;
            return new List<SerilogRecord>();
        }
    }
}

public record struct SerilogRecord()
{
    public string raw_date { get; set; } = string.Empty;
    public string log_type { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
}

public static partial class SerilogPatterns
{
    [GeneratedRegex(
        @"(?<Serilog>
    (?<raw_date>[\d-:\s\.]+?)\s
    (?<log_type>\[(INF|WRN)\])\s
    (?<content>.*)
   )", // TODO: create saved a regex 101 link for this regex
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
        matchTimeoutMilliseconds: 500)]
    public static partial Regex Basic(); // https://regex101.com/r/?????
}

public static partial class DirectoryPatterns
{
    [GeneratedRegex(
        @"^((?!

.*  # ignore anything preceding the blacklisted folders

 (node_modules|(wwwroot\/lib\b)|\/obj\b|\/bin\b) # excluded this list of folders

 [^.] ## exclude actual extensions from match

).)*$",
        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
        matchTimeoutMilliseconds: 500)]
    public static partial Regex StandardDirectoryBlacklist(); // https://regex101.com/r/GJtfLX/1
}