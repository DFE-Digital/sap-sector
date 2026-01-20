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
        var provision = establishment.ResourcedProvision;

        // Empty or explicitly no provision
        if (ResourcedProvisionValues.IsNoProvision(provision))
        {
            return DataAvailability.Available(false);
        }

        // Check for SEN unit
        if (ResourcedProvisionValues.HasSenUnit(provision))
        {
            return DataAvailability.Available(true);
        }

        // Has some provision data but no SEN unit mentioned
        return DataAvailability.Available(false);
    }
}