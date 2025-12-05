using SAPSec.Core.Model;

namespace SAPSec.Core.Interfaces.Services;

public interface IDsiClient
{
    Task<UserInfo?> GetUserAsync(string userId);
    Task<UserInfo?> GetUserByEmailAsync(string email);
    Task<Organisation?> GetOrganisationAsync(string organisationId);
    Task<string> GenerateBearerToken();
}