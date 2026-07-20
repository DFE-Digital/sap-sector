namespace SAPSec.Data.Common;

public static class Project
{
    public static string FindProjectDirectoryDownwards(string projectFileName)
    {
        var startDir = Directory.GetCurrentDirectory();

        // Fast path: if we're already in the project directory.
        var direct = Path.Combine(startDir, projectFileName);
        if (File.Exists(direct))
            return startDir;

        // Primary search (works in GitHub Actions)
        var matches = Directory.GetFiles(startDir, projectFileName, SearchOption.AllDirectories);

        if (matches.Length == 0)
        {
            // ---------- LOCAL FALLBACK ----------
            // If running from bin/Debug/... or bin/Release/..., walk up until we exit /bin
            var dir = new DirectoryInfo(startDir);
            while (dir != null && dir.Name.Equals("bin", StringComparison.OrdinalIgnoreCase) == false)
            {
                dir = dir.Parent;
            }

            // If we found /bin, move one level up (project root candidate)
            if (dir?.Parent != null)
            {
                var projectRootCandidate = dir.Parent.FullName;

                var fallbackMatches = Directory.GetFiles(
                    projectRootCandidate,
                    projectFileName,
                    SearchOption.AllDirectories);

                if (fallbackMatches.Length > 0)
                {
                    return GetPreferredMatch(fallbackMatches, projectFileName);
                }
            }

            throw new DirectoryNotFoundException(
                $"Could not find {projectFileName} under {startDir} (including bin fallback)"
            );
        }

        return GetPreferredMatch(matches, projectFileName);
    }

    private static string GetPreferredMatch(string[] matches, string projectFileName)
    {
        var preferred = matches
            .FirstOrDefault(p =>
                string.Equals(
                    new DirectoryInfo(Path.GetDirectoryName(p)!).Name,
                    Path.GetFileNameWithoutExtension(projectFileName),
                    StringComparison.OrdinalIgnoreCase))
            ?? matches[0];

        return Path.GetDirectoryName(preferred)!;
    }
}
