using SAPSec.Core.FeatureFlags;

namespace SAPSec.Test.Common.FeatureFlags;

public class MockFeatureFlagService() : IFeatureFlagService
{
    private IFeatureFlagService? _realImplementation;

    public MockFeatureFlagService(IFeatureFlagService realImplementation)
        : this()
    {
        _realImplementation = realImplementation;
    }

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

        if (_realImplementation is null)
        {
            return false;
        }

        return await _realImplementation.IsEnabledAsync(featureName);
    }
}
