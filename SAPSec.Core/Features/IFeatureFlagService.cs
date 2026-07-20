namespace SAPSec.Core.Features;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName);
}
