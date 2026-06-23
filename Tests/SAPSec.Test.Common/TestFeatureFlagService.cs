using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Test.Common;

public class TestFeatureFlagService : IFeatureFlagService
{
    private Dictionary<string, bool> _featureState = [];

    public void SetFeatureEnabled(string featureName, bool enabled)
    {
        _featureState[featureName] = enabled;
    }

    public Task<bool> IsEnabledAsync(string featureName)
        => Task.FromResult(_featureState.TryGetValue(featureName, out bool enabled) && enabled);
}
