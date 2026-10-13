using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SchoolSearch.Extensions;
using SAPSec.Core.Geography;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.AssociatedSchools;

public class AssociatedSchoolsService(
    IEstablishmentRepository establishmentRepository,
    IFeatureFlagService featureFlagService) : IAssociatedSchoolsService
{
    public async Task<IReadOnlyList<SchoolSearchResult>> GetAssociatedSchoolsAsync(Organisation organisation)
    {
        var establishments = await GetEstablishmentsAsync(organisation);
        if (establishments.Count == 0)
        {
            return [];
        }

        var primarySchoolsEnabled = await featureFlagService.IsEnabledAsync(Flags.EnablePrimarySchools);
        var allThroughSchoolsEnabled = await featureFlagService.IsEnabledAsync(Flags.EnableAllThroughSchools);

        // Apply the same rules as search so users only see schools the service can show.
        return establishments
            .Where(e => e.CanSearch(primarySchoolsEnabled, allThroughSchoolsEnabled))
            .Select(e => SchoolSearchResult.FromNameAndEstablishment(
                e.EstablishmentName,
                e,
                BNGCoordinates.TryParse(e.Easting, e.Northing, out var coords)
                    ? CoordinateConverter.Convert(coords)
                    : null))
            .OrderBy(r => r.EstablishmentName)
            .ToList();
    }

    private async Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(Organisation organisation)
    {
        if (!string.IsNullOrWhiteSpace(organisation.TrustId))
        {
            return await establishmentRepository.GetEstablishmentsByTrustIdAsync(organisation.TrustId);
        }

        if (!string.IsNullOrWhiteSpace(organisation.LocalAuthorityCode))
        {
            return await establishmentRepository.GetEstablishmentsByLocalAuthorityIdAsync(organisation.LocalAuthorityCode);
        }

        return [];
    }
}
