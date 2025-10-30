using SAPSec.Core.Model.DsiUser;

namespace SAPSec.Core.Interfaces.Services.IDsiApiService;

public interface IDsiApiService
{
    Task<DsiUserInfo?> GetUserAsync(string userId);
    Task<DsiUserInfo?> GetUserByEmailAsync(string email);
    Task<DsiOrganisation?> GetOrganisationAsync(string organisationId);
    Task<string> GenerateBearerToken();
}