using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has SEN unit based on TypeOfResourcedProvision field.
/// Single Responsibility: Only handles SEN unit logic.
/// </summary>
public sealed class SenUnitRule : IBusinessRule<string>
{
    public static readonly string NoSenUnitText = "Does not have a SEN unit";
    public static readonly string HasSenUnitText = "Has a SEN unit";
    public static readonly string NoDataAvailableText = "No data available";

    public DataWithAvailability<string> Evaluate(Establishment establishment)
    {
        var resourcedProvisionName = establishment.ResourcedProvisionName;

        if (SenUnitValues.NoSenUnit(resourcedProvisionName))
        {
            return DataWithAvailability.Available(NoSenUnitText);
        }

        if (SenUnitValues.HasSenUnit(resourcedProvisionName))
        {
            return DataWithAvailability.Available(HasSenUnitText);
        }

        return DataWithAvailability.Available(NoDataAvailableText);
    }
}