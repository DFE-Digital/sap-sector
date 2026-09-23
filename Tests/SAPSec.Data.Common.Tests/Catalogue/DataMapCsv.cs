using System.Globalization;
using CsvHelper;
using SAPData.Models;

namespace SAPSec.Data.Common.Tests.Catalogue;

/// <summary>Loads SAPData/DataMap/datamap.csv from the repository for golden tests.</summary>
internal static class DataMapCsv
{
    private static readonly Lazy<IReadOnlyList<DataMapRow>> Rows = new(Load);

    public static IReadOnlyList<DataMapRow> ForType(string type) => Rows.Value.Where(r => r.Type == type).ToList();

    private static IReadOnlyList<DataMapRow> Load()
    {
        using var reader = new StreamReader(Path.Combine(RepositoryRoot(), "SAPData", "DataMap", "datamap.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<DataMapMapping>();
        return csv.GetRecords<DataMapRow>().ToList();
    }

    private static string RepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SAPSec.sln")))
                return dir.FullName;
        }

        throw new DirectoryNotFoundException("Could not find SAPSec.sln above the test output directory.");
    }
}
