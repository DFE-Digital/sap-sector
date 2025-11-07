using SAPSec.Core.Model;

namespace SAPSec.Core.Interfaces.Services;

public interface IDsiApiService
{
    Task<DsiUserInfo?> GetUserAsync(string userId);
    Task<DsiUserInfo?> GetUserByEmailAsync(string email);
    Task<DsiOrganisation?> GetOrganisationAsync(string organisationId);
    Task<string> GenerateBearerToken();
}