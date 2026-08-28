using System.Text;
using System.Text.RegularExpressions;

namespace range.Pages.Shared.Components;

public static class StringExtensions
{
    public static RegexOptions gmix = RegexOptions.Compiled |
                                      RegexOptions.Multiline |
                                      RegexOptions.IgnoreCase |
                                      RegexOptions.IgnorePatternWhitespace;

    public static string Rollup(this IEnumerable<string> lines_of_code)
    {
        return lines_of_code.Aggregate(
                new StringBuilder(),
                (sb, next) =>
                {
                    sb.AppendLine(next);
                    return sb;
                })
            .ToString();
    }

    /// <summary>
    /// Takes a dictionary full of Regex patterns (or words) and swaps those values with whatever you set as the .Value.
    ///
    /// <usage>
    /// So, for example, a dictionary like this:
    ///
    /// var replacements = new Dictionary<..>{ { "\d+", "hello there!"}, {"Order", "66"}  }
    ///
    /// ... and a text string like this:
    ///
    /// string text = "Order was valued at $100.00";
    /// var altered_text = text.ReplaceAll(replacements);
    ///
    /// Should look something like:
    ///
    /// `66 was valued at $hello there!.hello there!`
    ///
    /// This can be used to do quick (but not comprehensive) replacements to format things like:
    /// * Random Unicode chars you don't want
    /// * Extra spaces
    /// * Other garbage like CLRF
    ///
    /// It does have a flaw in that the more you replace things, the less reliable it can be, especially if your replacements replace OTHER replacements.  So, tread lightly...
    /// </usage>
    /// </summary>
    public static string[] ReplaceAll(
        this string[] lines,
        Dictionary<string, string> replacementMap,
        RegexOptions options = RegexOptions.None
    )
    {
        if (options == RegexOptions.None)
            options = gmix;

        Dictionary<string, string> map = replacementMap.Aggregate(
            new Dictionary<string, string>(),
            (modified, next) =>
            {
                // Sometimes in JSON \ have to be represented in unicode.  This reverts it.
                string fixedKey = next.Key
                    .Replace("%5C", @"\")
                    .Replace(@"\\", @"\");

                string fixedValue =
                    Regex.Replace(
                        next.Value,
                        @"\""",
                        "'"
                    );

                modified.Add(fixedKey, fixedValue);
                return modified;
            }
        );

        List<string> results = new List<string>();

        foreach (string line in lines)
        {
            string modified = line;
            foreach (KeyValuePair<string, string> replacement in map)
            {
                modified = Regex.Replace(
                    modified,
                    replacement.Key,
                    replacement.Value,
                    options
                );
            }

            results.Add(modified);
        }

        return results.ToArray();
    }
}