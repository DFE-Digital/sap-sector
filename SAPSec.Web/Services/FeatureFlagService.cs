using Microsoft.FeatureManagement;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Services;

public class FeatureFlagService(IFeatureManagerSnapshot featureManager) : IFeatureFlagService
{
    private readonly IFeatureManagerSnapshot _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));

    public Task<bool> IsEnabledAsync(string featureName)
    {
        return _featureManager.IsEnabledAsync(featureName);
    }
}
