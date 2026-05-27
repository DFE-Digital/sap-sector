using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Core.Features.Home.UseCases;

public class GetEnablePrimarySchools(IFeatureFlagService featureFlagService)
{
    public async Task<GetEnablePrimarySchoolsResponse> Execute()
    {
        return new GetEnablePrimarySchoolsResponse(
            await featureFlagService.IsEnabledAsync(FeatureFlags.EnablePrimarySchools));
    }
}

public record GetEnablePrimarySchoolsResponse(bool EnablePrimarySchools);
