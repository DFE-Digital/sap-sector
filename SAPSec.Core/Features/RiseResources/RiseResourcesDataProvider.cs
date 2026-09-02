using SAPSec.Core.Constants;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.RiseResources;

internal class RiseResourcesDataProvider(
    IEstablishmentRepository establishmentRepository,
    IRiseResourcesRepository riseResourcesRepository)
{
    /// <summary>
    /// Phases a resource may be tagged with that an all-through school should see.
    /// </summary>
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

        var entries = await riseResourcesRepository.GetAllAsync();

        var resources = entries
            .Where(entry => AppliesToPhase(entry.SchoolPhases, establishment.PhaseOfEducationName))
            .Select(Map)
            .ToList();

        return new RiseResourcesSourceData(establishment, resources);
    }

    private static RiseResource Map(RiseResourceEntry entry) =>
        new(
            Title: entry.ResourceTitle,
            Description: NullIfBlank(entry.ResourceDescription),
            Url: NullIfBlank(entry.ResourceUrl),
            Category: NullIfBlank(entry.Category),
            SubCategory: NullIfBlank(entry.SubCategory),
            MappingMeasures: entry.MappingMeasures);

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

    private static string? NullIfBlank(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}

internal record RiseResourcesSourceData(
    Establishment Establishment,
    IReadOnlyList<RiseResource> Resources);
