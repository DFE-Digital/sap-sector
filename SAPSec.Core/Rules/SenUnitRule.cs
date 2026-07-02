using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has SEN unit based on TypeOfResourcedProvision field.
/// Single Responsibility: Only handles SEN unit logic.
/// </summary>
public sealed class SenUnitRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        var resourcedProvisionName = establishment.ResourcedProvisionName;

        if (SenUnitValues.NoSenUnit(resourcedProvisionName))
        {
            return DataWithAvailability.Available(false);
        }

        if (SenUnitValues.HasSenUnit(resourcedProvisionName))
        {
            return DataWithAvailability.Available(true);
        }

        return DataWithAvailability.Available(false);
    }
}