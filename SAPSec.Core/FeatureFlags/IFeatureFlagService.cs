namespace SAPSec.Core.FeatureFlags;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName);
}
