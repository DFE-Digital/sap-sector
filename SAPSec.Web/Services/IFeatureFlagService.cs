namespace SAPSec.Web.Services;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName);
}
