using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has resourced provision.
/// Single Responsibility: Only handles resourced provision logic.
/// </summary>
public sealed class ResourcedProvisionRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        var provision = establishment.ResourcedProvision;

        // Empty or explicitly no provision
        if (ResourcedProvisionValues.IsNoProvision(provision))
        {
            return DataWithAvailability.Available(false);
        }

        // Check for resourced provision
        if (ResourcedProvisionValues.HasResourcedProvision(provision))
        {
            return DataWithAvailability.Available(true);
        }

        return DataWithAvailability.Available(false);
    }
}