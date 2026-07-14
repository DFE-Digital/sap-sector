using SAPSec.Core.Data;
using SAPSec.Data.Dto;

namespace SAPSec.Core.School.Details;

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

        return DataWithAvailability.NotAvailable<bool>();
    }
}