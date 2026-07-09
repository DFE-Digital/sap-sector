using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Test.Common.FeatureFlags;

public class MockFeatureFlagService(IFeatureFlagService realImplementation) : IFeatureFlagService
{
    private Dictionary<string, bool> _overrides = new();

    public void Override(string featureName, bool enabled) =>
        _overrides[featureName] = enabled;

    public void ClearOverrides(string featureName) =>
        _overrides.Remove(featureName);

    public async Task<bool> IsEnabledAsync(string featureName)
    {
        if (_overrides.TryGetValue(featureName, out var enabled))
        {
            return enabled;
        }

        return await realImplementation.IsEnabledAsync(featureName);
    }
}
