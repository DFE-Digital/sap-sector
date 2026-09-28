using Microsoft.Extensions.Configuration;
using SAPData.Models;
using SAPSec.Data.Common.Catalogue.Validation;

namespace SAPData;

// Checks that stop the run before anything is loaded.
internal partial class Program
{
    // Checks the files about to be loaded against the data map and the establishment view. Runs before the ETL step
    // (and the maintenance page), so a schema change in a new file stops the run and the live views keep their data.
    private static void CheckSourceFiles(IReadOnlyList<DataMapRow> rows, GenerateRawTables rawTables, IConfiguration configuration)
    {
        var issues = SourceFileCheck.Run(rows, rawTables.TableMappings, rawTables.SourceFilesByTable);
        if (issues.Count == 0)
        {
            Console.WriteLine("Source files checked: every column and filter value the data map and establishment view use is present.");
            return;
        }

        foreach (var issue in issues)
            Console.Error.WriteLine(issue);

        var mode = configuration["SourceFileCheck"] ?? Environment.GetEnvironmentVariable("SOURCE_FILE_CHECK");
        if (string.Equals(mode?.Trim(), "warn", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"WARNING: {issues.Count} source file issue(s) found; continuing because SOURCE_FILE_CHECK=warn. Affected values may be blank.");
            return;
        }

        throw new InvalidOperationException(
            $"The downloaded source files don't match the data map ({issues.Count} issue(s), listed above). Nothing has been loaded. " +
            "Update the catalogue for the new files (docs/operational/003-new-data-year.md), or for [establishment] issues " +
            "the columns in GenerateViews.GenerateEstablishmentDimensionView, " +
            "or set SOURCE_FILE_CHECK=warn to load anyway.");
    }

    // Fails before any SQL is generated, so a data map mistake never reaches the ETL step.
    private static void ValidateCatalogue(IReadOnlyList<DataMapRow> rows)
    {
        var issues = CatalogueValidator.Validate(rows);
        if (issues.Count == 0)
        {
            Console.WriteLine($"Catalogue validated: {rows.Count} rows, no issues.");
            return;
        }

        foreach (var issue in issues)
            Console.Error.WriteLine(issue);

        throw new InvalidOperationException($"Data map catalogue has {issues.Count} validation issue(s); see the log above.");
    }
}
