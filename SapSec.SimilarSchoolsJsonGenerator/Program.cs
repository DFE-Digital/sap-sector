using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SapSec.SimilarSchoolsJsonGenerator.Models;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Json;
using System.Globalization;

namespace SapSec.SimilarSchoolsJsonGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        string baseDir = FindProjectDirectoryDownwards("SapSec.SimilarSchoolsJsonGenerator");

        string sourceFilesDir = Path.Combine(baseDir, "SourceFiles");
        string outputFilesDir = Path.Combine(baseDir, "OutputFiles");

        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var logger1 = factory.CreateLogger<Establishment>();
        var logger2 = factory.CreateLogger<JsonFile<Establishment>>();

        var establishmentRepository = new JsonEstablishmentRepository(new JsonFile<Establishment>(logger2), logger1);
        var establishments = await establishmentRepository.GetAllEstablishmentsAsync();
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
                // Skip rows for schools not in our data set
                if (!allUrns.Contains(row.URN))
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
                // Skip rows for schools not in our data set
                if (!allUrns.Contains(row.URN))
                {
                    continue;
                }

                // For every new school reset the set of neighbour URNs
                // (assumes CSV file rows are grouped by URN)
                if (currentUrn != row.URN)
                {
                    currentUrn = row.URN;
                    neighbours = new();
                }

                // If neighbour URN is not in our data set, map it to a URN that is in our dataset
                if (!allUrns.Contains(row.NeighbourURN))
                {
                    // If we've already mapped this URN, use that, otherwise pick the next unmapped one
                    // that isn't already in our neighbour set
                    if (!urnLookup.ContainsKey(row.NeighbourURN))
                    {
                        urnLookup[row.NeighbourURN] = allUrns.Except([currentUrn, .. neighbours]).First();
                    }

                    row.NeighbourURN = urnLookup[row.NeighbourURN];
                }

                // Update neighbours
                neighbours.Add(row.NeighbourURN);

                results.Add(row);
            }

            WriteJsonFile(results);
        }

        IEnumerable<TRow> ReadCsvFile<TRow, TMapping>(string csvFile)
            where TRow : ISimilarSchoolsRow
            where TMapping : ClassMap<TRow>
        {
            Console.WriteLine($"Reading: {csvFile}");

            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TMapping>();

                foreach (var row in csv.GetRecords<TRow>())
                {
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

    private static string FindProjectDirectoryDownwards(string projectName)
    {
        var projectFileName = $"{projectName}.csproj";
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
                                projectName,
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
                    projectName,
                    StringComparison.OrdinalIgnoreCase))
            ?? matches[0];

        return Path.GetDirectoryName(preferred)!;
    }
}
