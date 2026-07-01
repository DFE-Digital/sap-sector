using SAPSec.Core.Constants;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has nursery classes
/// Single Responsibility: Only handles nursery classes logic.
/// </summary>
public sealed class NurseryProvisionRule : IBusinessRule<NurseryProvision>
{
    public DataWithAvailability<NurseryProvision> Evaluate(Establishment establishment)
    {
        return GetNurseryProvision(establishment.NurseryProvisionName);
    }

    public DataWithAvailability<NurseryProvision> Evaluate(SimilarSchool similarSchool)
    {
        return GetNurseryProvision(similarSchool.NurseryProvisionName);
    }

    private DataWithAvailability<NurseryProvision> GetNurseryProvision(string nurseryProvisionName)
    {
        if (PhaseOfEducationValues.IndicatesNursery(nurseryProvisionName))
        {
            return DataWithAvailability.Available(new NurseryProvision("H", "Has nursery classes"));
        }
        if (PhaseOfEducationValues.IndicatesNoNursery(nurseryProvisionName))
        {
            return DataWithAvailability.Available(new NurseryProvision("N", "Does not have nursery classes"));
        }
        return DataWithAvailability.NotAvailable<NurseryProvision>();
    }
}