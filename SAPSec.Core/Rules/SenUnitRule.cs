using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has SEN unit based on ResourcedProvision field.
/// Single Responsibility: Only handles SEN unit logic.
/// </summary>
public sealed class SenUnitRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        var provision = establishment.ResourcedProvisionId;

        // Empty or explicitly no provision
        if (ResourcedProvisionValues.IsNoProvision(provision))
        {
            return DataWithAvailability.Available(false);
        }

        // Check for SEN unit
        if (ResourcedProvisionValues.HasSenUnit(provision))
        {
            return DataWithAvailability.Available(true);
        }

        // Has some provision data but no SEN unit mentioned
        return DataWithAvailability.Available(false);
    }
}