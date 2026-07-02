using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has resourced provision.
/// Single Responsibility: Only handles resourced provision logic.
/// </summary>
public sealed class ResourcedProvisionRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        var resourcedProvisionName = establishment.ResourcedProvisionName;

        if (ResourcedProvisionValues.NoResourcedProvision(resourcedProvisionName))
        {
            return DataWithAvailability.Available(false);
        }
        if (ResourcedProvisionValues.HasResourcedProvision(resourcedProvisionName))
        {
            return DataWithAvailability.Available(true);
        }
        return DataWithAvailability.Available(false);
    }
}