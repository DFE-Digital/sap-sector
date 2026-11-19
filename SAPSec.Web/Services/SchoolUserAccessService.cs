using System.Security.Claims;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Features.SchoolSearch.Extensions;
using SAPSec.Core.Time;
using SAPSec.Data.Repositories;

namespace SAPSec.Web.Services;

public class SchoolUserAccessService(
    IUserService userService,
    IFeatureFlagService featureFlagService,
    IEstablishmentRepository establishmentRepository,
    ICurrentTimeProvider timeProvider)
{
    public async Task<bool> CanAccessAsync(ClaimsPrincipal user)
    {
        if (await featureFlagService.IsEnabledAsync(Flags.EnableFindASchool))
        {
            return true;
        }

        var organisation = await userService.GetCurrentOrganisationAsync(user);
        if (organisation is not { IsEstablishment: true })
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(organisation.Urn))
        {
            return false;
        }

        var establishment = await establishmentRepository.GetEstablishmentAsync(organisation.Urn);
        return establishment.CanSearch(
            await featureFlagService.IsEnabledAsync(Flags.EnablePrimarySchools),
            await featureFlagService.IsEnabledAsync(Flags.EnableAllThroughSchools),
            timeProvider);
    }
}
