using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has nursery classes
/// Single Responsibility: Only handles nursery classes logic.
/// </summary>
public sealed class NurseryProvisionRule : IBusinessRule<string>
{
    public static readonly string HasNurseryClasses = "Has nursery classes";
    public static readonly string NoNurseryClasses = "Does not have nursery classes";

    public DataWithAvailability<string> Evaluate(Establishment establishment)
    {
        var nurseryProvisionName = establishment.NurseryProvisionName;

        if (NurseryProvisionValues.HasNurseryClasses(nurseryProvisionName))
        {
            return DataWithAvailability.Available(HasNurseryClasses);
        }
        if (NurseryProvisionValues.NoNurseryClasses(nurseryProvisionName))
        {
            return DataWithAvailability.Available(NoNurseryClasses);
        }
        return DataWithAvailability.NotAvailable<string>();
    }
}