namespace SAPSec.Core.Interfaces.Services;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName);
}
