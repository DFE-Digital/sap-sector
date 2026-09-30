namespace SAPSec.Data.Common.Tests;

internal static class Repository
{
    public static string PathTo(params string[] parts) => Path.Combine([Root(), .. parts]);

    private static string Root()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SAPSec.sln")))
                return dir.FullName;
        }

        throw new DirectoryNotFoundException("Could not find SAPSec.sln above the test output directory.");
    }
}
