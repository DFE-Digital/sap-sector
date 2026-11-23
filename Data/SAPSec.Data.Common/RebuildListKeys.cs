using System.Text.RegularExpressions;

namespace SAPSec.Data.Common;

/// <summary>
/// Resolves the entries in a raw tables rebuild list to the logical dataset keys of the files being loaded.
/// </summary>
public static class RebuildListKeys
{
    public const string DateToken = "YYYYmmDD";

    /// <summary>
    /// Replaces each entry containing <see cref="DateToken"/> (e.g. <c>edubasealldataYYYYmmDD</c>) with the
    /// dated keys of the available files that match it (e.g. <c>edubasealldata20261124</c>).
    /// Raw table names are built from the dated key, so the pattern itself never matches a table.
    /// Other entries are returned unchanged.
    /// </summary>
    /// <param name="listKeys">Entries from the rebuild list.</param>
    /// <param name="availableLogicalKeys">Logical keys of the files being loaded (file names without extension or <c>manual_</c> prefix).</param>
    /// <param name="log">Optional logger for what each pattern resolved to.</param>
    public static HashSet<string> ExpandDatedKeys(
        IEnumerable<string> listKeys,
        IEnumerable<string> availableLogicalKeys,
        Action<string>? log = null)
    {
        var available = availableLogicalKeys.ToList();
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in listKeys)
        {
            if (!key.Contains(DateToken, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(key);
                continue;
            }

            var pattern = BuildPattern(key);
            var matches = available.Where(k => pattern.IsMatch(k)).ToList();

            if (matches.Count == 0)
            {
                log?.Invoke($"Rebuild list entry '{key}' matched no downloaded file.");
                continue;
            }

            foreach (var match in matches)
            {
                result.Add(match);
            }

            log?.Invoke($"Rebuild list entry '{key}' resolved to: {string.Join(", ", matches)}");
        }

        return result;
    }

    /// <summary>The logical dataset key for a source file: its name without extension or <c>manual_</c> prefix.</summary>
    public static string LogicalKeyForFile(string filePath)
    {
        var fileKey = Path.GetFileNameWithoutExtension(filePath);
        return fileKey.StartsWith("manual_", StringComparison.OrdinalIgnoreCase)
            ? fileKey["manual_".Length..]
            : fileKey;
    }

    private static Regex BuildPattern(string key)
    {
        var index = key.IndexOf(DateToken, StringComparison.OrdinalIgnoreCase);
        var prefix = Regex.Escape(key[..index]);
        var suffix = Regex.Escape(key[(index + DateToken.Length)..]);

        return new Regex($"^{prefix}[0-9]{{8}}{suffix}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
