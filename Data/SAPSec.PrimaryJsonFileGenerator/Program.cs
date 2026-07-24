using CsvHelper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SAPData.Models;
using SAPSec.Data.Common;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SAPSec.PrimaryJsonFileGenerator;

internal class Program
{
    private enum Scope
    {
        Establishment,
        England,
        LA
    }

    private sealed record FileSpec(
        Scope Range,
        string Type,
        string ModelName);

    private static readonly FileSpec[] Files =
    {
        new(Scope.Establishment, "KS2_Performance", "EstablishmentPerformance"),
        new(Scope.England, "KS2_Performance", "EnglandPerformance"),
        new(Scope.LA, "KS2_Performance", "LAPerformance")
    };

    static void Main(string[] args)
    {
        // In CI the working directory is often the repo root.
        // Find SAPData.csproj anywhere under the current directory and use its folder.
        string baseDir = Project.FindProjectDirectoryDownwards("SAPSec.PrimaryJsonFileGenerator.csproj");

        IConfiguration _configuration = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var settings = _configuration.Get<JsonGeneratorSettings>()
            ?? new JsonGeneratorSettings();

        string dataMapDir = Path.Combine(baseDir, "DataMap");
        string dataMapCsv = Path.Combine(dataMapDir, "datamap.csv");

        string infrastructureDir = Path.Combine(Directory.GetParent(Directory.GetParent(baseDir)!.FullName)!.FullName, "SAPSec.Infrastructure");
        string jsonDir = Path.Combine(infrastructureDir, "Data", "Files");
        string primaryJsonDir = Path.Combine(jsonDir, "PrimarySchools");
        string generatedJsonDir = Path.Combine(jsonDir, "Generated");
        string csDir = Path.Combine(baseDir, "..\\SAPSec.Data\\Dto\\KS2\\Performance");

        Directory.CreateDirectory(jsonDir);
        Directory.CreateDirectory(primaryJsonDir);
        Directory.CreateDirectory(generatedJsonDir);

        // -------------------------------------------------
        // 1. Load DataMap
        // -------------------------------------------------
        List<DataMapRow> rows;
        using (var reader = new StreamReader(dataMapCsv))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<DataMapMapping>();
            rows = csv.GetRecords<DataMapRow>().ToList();
        }

        Console.WriteLine($"Loaded {rows.Count} DataMap rows");

        var establishmentsFile = Path.Combine(generatedJsonDir, "Establishment.json");
        var establishments = JsonConvert.DeserializeObject<List<Establishment>>(File.ReadAllText(establishmentsFile)) ?? [];

        Console.WriteLine($"Loaded {establishments.Count} Establishments");

        var similarSchoolsFile = Path.Combine(generatedJsonDir, "SimilarSchoolsPrimaryGroupsEntry.json");
        var similarSchoolsGroupEntries = JsonConvert.DeserializeObject<List<SimilarSchoolsPrimaryGroupsEntry>>(File.ReadAllText(similarSchoolsFile)) ?? [];
        var similarSchoolsGroups = similarSchoolsGroupEntries.GroupBy(e => e.URN);
        var similarSchoolsUrns = similarSchoolsGroupEntries.Select(g => g.URN).Concat(similarSchoolsGroupEntries.Select(g => g.NeighbourURN)).Distinct().ToList();

        Console.WriteLine($"Loaded {similarSchoolsGroups.Count()} primary similar schools groups");

        var primarySchoolUrns = establishments
            .Where(e => e.PhaseOfEducationId is "2")
            .Where(e => similarSchoolsUrns.Contains(e.URN))
            .Select(e => e.URN)
            .ToArray();

        var laIds = establishments
            .Where(e => e.PhaseOfEducationId is "2")
            .Where(e => similarSchoolsUrns.Contains(e.URN))
            .Select(e => e.LAId)
            .Distinct()
            .ToArray();

        Console.WriteLine($"Loaded {primarySchoolUrns.Length} primary schools");

        foreach (var file in Files)
        {
            var includedRows = rows
                .Where(r => r.Range == file.Range.ToString())
                .Where(r => r.Type == file.Type)
                .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
                .Where(r => !IsIgnored(r))
                .ToArray();

            var ignoredRows = rows
                .Where(r => r.Range == file.Range.ToString())
                .Where(r => r.Type == file.Type)
                .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
                .Where(IsIgnored)
                .ToArray();

            if (ignoredRows.Length > 0)
            {
                Console.WriteLine($"Ignoring {ignoredRows.Length} DataMap rows for {file.Type}");
                foreach (var row in ignoredRows)
                    Console.WriteLine($"Ignored mapping: {row.Ref} ({row.PropertyName})");
            }

            if (includedRows.Length == 0)
            {
                continue;
            }

            string[] ids = file.Range switch
            {
                Scope.Establishment => primarySchoolUrns,
                Scope.LA => laIds,
                _ => ["National"]
            };

            var json = GenerateJsonFile(includedRows, ids, settings);
            File.WriteAllText(
                Path.Combine(primaryJsonDir, $"{file.ModelName}.json"),
                json,
                new UTF8Encoding(false));

            var cs = GenerateCsFile(file.ModelName, includedRows);
            File.WriteAllText(
                Path.Combine(csDir, $"{file.ModelName}.cs"),
                cs,
                new UTF8Encoding(false));
        }
    }

    private static bool IsIgnored(DataMapRow r)
    {
        return string.Equals(
            r.IgnoreMapping?.Trim(),
            "Y",
            StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateJsonFile(DataMapRow[] rows, string[] ids, JsonGeneratorSettings settings)
    {
        var rnd = new Random();
        var json = new StringBuilder();

        json.AppendLine("[");
        foreach (var (urn, i) in ids.Select((r, i) => (r, i)))
        {
            json.AppendLine("  {");
            json.AppendLine($"    \"Id\": \"{urn}\",");

            var propertySpecsApplyingToThisUrn = settings.Properties
                .Where(p => !p.Urns.Any() || p.Urns.Contains(urn))
                .ToList();

            foreach (var (row, j) in rows.Select((r, j) => (r, j)))
            {
                double? value;

                var propertySpecsApplyingToThisProperty = propertySpecsApplyingToThisUrn
                    .Where(p => !p.PropertyNamePatterns.Any() || p.PropertyNamePatterns.Any(pattern => Regex.IsMatch(row.PropertyName, pattern)))
                    .ToList();

                var empty = SelectBestPropertySpec(propertySpecsApplyingToThisProperty, urn, row.PropertyName)
                    ?.Empty ?? false;

                if (empty)
                {
                    value = null;
                }
                else
                {
                    var propertySpec = SelectBestPropertySpec(
                        propertySpecsApplyingToThisProperty.Where(p => p.MinValue is not null || p.MaxValue is not null),
                        urn,
                        row.PropertyName);

                    var minValue = propertySpec?.MinValue ?? 0.0;
                    var maxValue = propertySpec?.MaxValue ?? 100.0;

                    value = Math.Round(minValue + rnd.NextDouble() * (maxValue - minValue), 2);
                }

                json.AppendLine($"    \"{row.PropertyName}\": \"{value}\"{(j < rows.Length - 1 ? "," : "")}");
            }

            json.AppendLine($"  }}{(i < ids.Length - 1 ? "," : "")}");
        }
        json.AppendLine("]");

        return json.ToString();
    }

    private static PropertyDataSpec? SelectBestPropertySpec(IEnumerable<PropertyDataSpec> matchingSpecs, string urn, string propertyName)
    {
        return matchingSpecs
            .Select((spec, index) => new { spec, index })
            .OrderByDescending(x => x.spec.PropertyNamePatterns.Any(pattern => Regex.IsMatch(propertyName, pattern)))
            .ThenByDescending(x => x.spec.Urns.Contains(urn))
            .ThenBy(x => x.index)
            .Select(x => x.spec)
            .FirstOrDefault();
    }

    private static string GenerateCsFile(string modelName, DataMapRow[] rows)
    {
        var cs = new StringBuilder();

        cs.AppendLine("// This file is automatically generated by SAPSec.PrimaryJsonFileGenerator.");
        cs.AppendLine("// Please do not manually edit this file or your changes will be lost when the file is regenerated.");
        cs.AppendLine();
        cs.AppendLine("using System.Diagnostics.CodeAnalysis;");
        cs.AppendLine();
        cs.AppendLine($"namespace SAPSec.Data.Dto.KS2.Performance;");
        cs.AppendLine();
        cs.AppendLine("[ExcludeFromCodeCoverage]");
        cs.AppendLine($"public class {modelName}");
        cs.AppendLine("{");
        cs.AppendLine($"    public string Id {{ get; set; }} = string.Empty;");

        foreach (var (row, i) in rows.Select((r, i) => (r, i)))
        {
            cs.AppendLine($"    public string {row.PropertyName} {{ get; set; }} = string.Empty;");
        }

        cs.AppendLine("}");

        return cs.ToString();
    }
}
