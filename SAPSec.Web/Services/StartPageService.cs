using SAPSec.Core.FeatureFlags;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Services;

public class StartPageService(IFeatureFlagService featureFlagService) : IStartPageService
{
    public async Task<string> GetStartUrlAsync(bool signedIn = true)
    {
        if (await featureFlagService.IsEnabledAsync(Flags.EnableFindASchool))
        {
            return Routes.FindASchool();
        }

        return signedIn ? Routes.UserHome : Routes.Home;
    }
}
