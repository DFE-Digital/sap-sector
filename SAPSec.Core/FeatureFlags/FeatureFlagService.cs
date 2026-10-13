using Microsoft.FeatureManagement;

namespace SAPSec.Core.FeatureFlags;

public class FeatureFlagService(IFeatureManager featureManager) : IFeatureFlagService
{
    private readonly IFeatureManager _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));

    public Task<bool> IsEnabledAsync(string featureName)
    {
        return _featureManager.IsEnabledAsync(featureName);
    }
}
