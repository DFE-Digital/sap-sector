using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SapSec.SimilarSchoolsJsonGenerator.Models;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Repositories;
using SAPSec.Infrastructure.Repositories.Generic;
using System.Globalization;
using static Lucene.Net.Util.Fst.Util;

namespace SapSec.SimilarSchoolsJsonGenerator;

public class Program
{
    public static void Main(string[] args)
    {
        string baseDir = FindProjectDirectoryDownwards("SapSec.SimilarSchoolsJsonGenerator.csproj");

        string sourceFilesDir = Path.Combine(baseDir, "SourceFiles");
        string outputFilesDir = Path.Combine(baseDir, "OutputFiles");

        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var logger1 = factory.CreateLogger<Establishment>();
        var logger2 = factory.CreateLogger<JSONRepository<Establishment>>();

        var establishmentRepository = new EstablishmentRepository(new JSONRepository<Establishment>(logger2), logger1);
        var establishments = establishmentRepository.GetAllEstablishments();
        var allUrns = establishments.Select(e => e.URN).ToList();
        Dictionary<string, string> urnLookup = new();

        List<(string, Action<string>)> files = [
            ("*_off_sen_primary_*_grps.csv", ConvertGroupsFile<SimilarSchoolsPrimaryGroupsRow, SimilarSchoolsPrimaryGroupsRowMapping>),
            ("*_off_sen_primary_*_vals.csv", ConvertValuesFile<SimilarSchoolsPrimaryValuesRow, SimilarSchoolsPrimaryValuesRowMapping>),
            ("*_off_sen_secondary_*_grps.csv", ConvertGroupsFile<SimilarSchoolsSecondaryGroupsRow, SimilarSchoolsSecondaryGroupsRowMapping>),
            ("*_off_sen_secondary_*_vals.csv", ConvertValuesFile<SimilarSchoolsSecondaryValuesRow, SimilarSchoolsSecondaryValuesRowMapping>)
        ];

        foreach (var (filePattern, convert) in files)
        {
            var matchingFiles = Directory.GetFiles(sourceFilesDir, filePattern);

            if (matchingFiles.Length == 0)
            {
                Console.WriteLine($"No matching files for pattern: {filePattern}");
                continue;
            }

            var matchingFile = matchingFiles.First();
            if (matchingFiles.Length > 1)
            {
                Console.WriteLine($"Pattern {filePattern} has multiple matches, choosing {matchingFile}");
            }

            var filePath = Path.Combine(sourceFilesDir, matchingFile);
            convert(filePath);
        }

        void ConvertValuesFile<TRow, TMapping>(string csvFile)
            where TRow : ISimilarSchoolsRow
            where TMapping : ClassMap<TRow>
        {
            List<TRow> results = new();

            foreach (var row in ReadCsvFile<TRow, TMapping>(csvFile))
            {
                if (!allUrns.Contains(row.Urn))
                {
                    continue;
                }

                results.Add(row);
            }

            WriteJsonFile(results);
        }

        void ConvertGroupsFile<TRow, TMapping>(string csvFile)
            where TRow : ISimilarSchoolsGroupsRow
            where TMapping : ClassMap<TRow>
        {
            List<TRow> results = new();
            string? currentUrn = null;
            List<string> neighbours = new();

            foreach (var row in ReadCsvFile<TRow, TMapping>(csvFile))
            {
                if (!allUrns.Contains(row.Urn))
                {
                    continue;
                }

                if (currentUrn != row.Urn)
                {
                    currentUrn = row.Urn;
                    neighbours = new();
                }

                if (!allUrns.Contains(row.NeighbourUrn))
                {
                    if (!urnLookup.ContainsKey(row.NeighbourUrn))
                    {
                        urnLookup[row.NeighbourUrn] = allUrns.Except([currentUrn, .. neighbours]).First();
                    }

                    row.NeighbourUrn = urnLookup[row.NeighbourUrn];
                    neighbours.Add(row.NeighbourUrn);
                }

                results.Add(row);
            }

            WriteJsonFile(results);
        }

        IEnumerable<TRow> ReadCsvFile<TRow, TMapping>(string csvFile)
            where TRow : ISimilarSchoolsRow
            where TMapping : ClassMap<TRow>
        {
            Console.WriteLine($"Converting: {csvFile}");

            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TMapping>();
                var rows = csv.GetRecords<TRow>();

                foreach (var row in rows)
                {
                    if (!allUrns.Contains(row.Urn))
                    {
                        continue;
                    }

                    yield return row;
                }
            }
        }

        void WriteJsonFile<TRow>(List<TRow> results)
        {
            var outputFilePath = Path.Combine(outputFilesDir, $"{typeof(TRow).Name}.json");
            Console.WriteLine($"Writing: {outputFilePath}");
            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(results));
        }
    }

    private static string FindProjectDirectoryDownwards(string projectFileName)
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
                    var preferredFallback = fallbackMatches
                        .FirstOrDefault(p =>
                            string.Equals(
                                new DirectoryInfo(Path.GetDirectoryName(p)!).Name,
                                "SapSec.SimilarSchoolsJsonGenerator",
                                StringComparison.OrdinalIgnoreCase))
                        ?? fallbackMatches[0];

                    return Path.GetDirectoryName(preferredFallback)!;
                }
            }

            throw new DirectoryNotFoundException(
                $"Could not find {projectFileName} under {startDir} (including bin fallback)"
            );
        }

        // Prefer SAPData project if multiple found
        var preferred = matches
            .FirstOrDefault(p =>
                string.Equals(
                    new DirectoryInfo(Path.GetDirectoryName(p)!).Name,
                    "SapSec.SimilarSchoolsJsonGenerator",
                    StringComparison.OrdinalIgnoreCase))
            ?? matches[0];

        return Path.GetDirectoryName(preferred)!;
    }
}
