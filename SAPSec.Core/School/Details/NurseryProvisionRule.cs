using SAPSec.Core.Data;
using SAPSec.Data.Dto;

namespace SAPSec.Core.School.Details;

/// <summary>
/// Business rule: Determines if school has nursery classes
/// Single Responsibility: Only handles nursery classes logic.
/// </summary>
public sealed class NurseryProvisionRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        var nurseryProvisionName = establishment.NurseryProvisionName;

        if (NurseryProvisionValues.HasNurseryClasses(nurseryProvisionName))
        {
            return DataWithAvailability.Available(true);
        }
        if (NurseryProvisionValues.NoNurseryClasses(nurseryProvisionName))
        {
            return DataWithAvailability.Available(false);
        }

        return DataWithAvailability.NotAvailable<bool>();
    }
}