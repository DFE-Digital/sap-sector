using SAPSec.Core.Constants;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has resourced provision.
/// Single Responsibility: Only handles resourced provision logic.
/// </summary>
public sealed class ResourcedProvisionRule : IBusinessRule<string>
{
    public DataWithAvailability<string> Evaluate(Establishment establishment)
    {
        return GetResourcedProvision(establishment.ResourcedProvisionName);
    }

    public DataWithAvailability<string> Evaluate(SimilarSchool similarSchool)
    {
        throw new NotImplementedException();
    }

    public DataWithAvailability<string> GetResourcedProvision(string resourcedProvisionName)
    {
        if (ResourcedProvisionValues.NoResourcedProvision(resourcedProvisionName))
        {
            return DataWithAvailability.Available("Does not have a resourced provision");
        }
        if (ResourcedProvisionValues.HasResourcedProvision(resourcedProvisionName))
        {
            return DataWithAvailability.Available("Has a resourced provision");
        }
        return DataWithAvailability.Available("No data available");
    }
}