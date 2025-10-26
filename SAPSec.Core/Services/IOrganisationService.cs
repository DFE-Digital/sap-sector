namespace SAPSec.Core.Services;

using SAPSec.Core.Models;

public interface IOrganisationService
{
    OrganisationDetails? GetUserOrganisation();
}