using SAPSec.Core.Constants;
using SAPSec.Data.Dto;
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

        var descriptionByCategory = document.ResourceCategories
            .GroupBy(category => category.Category, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().CategoryDescription, StringComparer.Ordinal);

        var configuredOrderByCategory = document.ResourceCategories
            .Select((category, index) => (category.Category, index))
            .GroupBy(x => x.Category, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().index, StringComparer.Ordinal);

        var categories = applicableResources
            .GroupBy(entry => entry.Category ?? string.Empty)
            // Listed categories first, in resourceCategories order; unlisted keep first-appearance order (stable sort).
            .OrderBy(group => configuredOrderByCategory.TryGetValue(group.Key, out var order) ? order : int.MaxValue)
            .Select(group => new RiseResourceCategory(
                Name: group.Key,
                Description: descriptionByCategory.TryGetValue(group.Key, out var description)
                    ? NullIfBlank(description)
                    : null,
                Resources: [.. group.Select(Map)]))
            .ToList();

        return new RiseResourcesSourceData(establishment, categories);
    }

    private static RiseResource Map(Data.Dto.RiseResources.RiseResourceEntry entry) =>
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
