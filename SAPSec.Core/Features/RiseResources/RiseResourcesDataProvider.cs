using SAPSec.Core.Constants;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.RiseResources;

internal class RiseResourcesDataProvider(
    IEstablishmentRepository establishmentRepository,
    IRiseResourcesRepository riseResourcesRepository)
{
    private static readonly string[] AllThroughSchoolPhases =
    [
        PhaseOfEducationValues.Primary,
        PhaseOfEducationValues.Secondary,
        PhaseOfEducationValues.AllThrough
    ];

    public async Task<RiseResourcesSourceData> GetRiseResourcesData(string urn)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn)
            ?? throw new NotFoundException($"School with URN {urn} was not found");

        var document = await riseResourcesRepository.GetAsync();

        var applicableResources = document.ResourceEntries
            .Where(entry => AppliesToPhase(entry.SchoolPhases, establishment.PhaseOfEducationName))
            .ToList();

        var configuredCategories = document.ResourceCategories
            .GroupBy(category => category.Category, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var categories = applicableResources
            .GroupBy(entry => entry.Category ?? string.Empty)
            .Select(group => BuildCategory(group.Key, group, configuredCategories))
            .ToList();

        return new RiseResourcesSourceData(establishment, categories);
    }

    private static RiseResourceCategory BuildCategory(
        string name,
        IEnumerable<RiseResourceEntry> entries,
        IReadOnlyDictionary<string, RiseResourceCategoryEntry> configuredCategories)
    {
        configuredCategories.TryGetValue(name, out var configured);

        // Sub-category order follows first appearance in resourceEntries.
        var resources = entries
            .GroupBy(entry => entry.SubCategory ?? string.Empty)
            .SelectMany(group => group)
            .Select(Map)
            .ToList();

        return new RiseResourceCategory(name, NullIfBlank(configured?.CategoryDescription), resources);
    }

    private static RiseResource Map(RiseResourceEntry entry) =>
        new(
            Title: entry.ResourceTitle,
            Description: NullIfBlank(entry.ResourceDescription),
            Url: NullIfBlank(entry.ResourceUrl),
            SubCategory: NullIfBlank(entry.SubCategory),
            MappingMeasures: NullIfBlank(entry.MappingMeasures));

    private static bool AppliesToPhase(IReadOnlyList<string> resourcePhases, string schoolPhase)
    {
        var tagged = resourcePhases.Select(NormalisePhase).ToHashSet();

        IEnumerable<string> wanted = IsAllThrough(schoolPhase)
            ? AllThroughSchoolPhases.Select(NormalisePhase)
            : [NormalisePhase(schoolPhase)];

        return wanted.Any(tagged.Contains);
    }

    private static bool IsAllThrough(string schoolPhase) =>
        NormalisePhase(schoolPhase) == NormalisePhase(PhaseOfEducationValues.AllThrough);

    // "All-through" (constant) vs "All through" (content) — match on either.
    private static string NormalisePhase(string phase) =>
        phase.Trim().Replace("-", " ").ToLowerInvariant();

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}

internal record RiseResourcesSourceData(
    Establishment Establishment,
    IReadOnlyList<RiseResourceCategory> Categories);
