using SAPSec.Core.Constants;
using SAPSec.Core.Features.SimilarSchools;
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
    public DataWithAvailability<string> Evaluate(Establishment establishment)
    {
        return GetSenUnit(establishment.ResourcedProvisionName);
    }

    private DataWithAvailability<string> GetSenUnit(string resourcedProvisionName)
    {
        if (SenUnitValues.NoSenUnit(resourcedProvisionName))
        {
            return DataWithAvailability.Available("Does not have a SEN unit");
        }

        if (SenUnitValues.HasSenUnit(resourcedProvisionName))
        {
            return DataWithAvailability.Available("Has a SEN unit");
        }

        return DataWithAvailability.Available("No data available");
    }

    public DataWithAvailability<string> Evaluate(SimilarSchool similarSchool)
    {
        throw new NotImplementedException();
    }

}