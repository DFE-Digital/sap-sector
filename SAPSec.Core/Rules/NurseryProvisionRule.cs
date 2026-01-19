using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has nursery provision based on phase of education.
/// Single Responsibility: Only handles nursery provision logic.
/// </summary>
public sealed class NurseryProvisionRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        var phase = establishment.PhaseOfEducationName;

        if (string.IsNullOrWhiteSpace(phase))
        {
            return DataWithAvailability<bool>.NotAvailable();
        }

        // Nursery phase schools have nursery provision
        if (PhaseOfEducationValues.IndicatesNursery(phase))
        {
            return DataWithAvailability<bool>.Available(true);
        }

        // All-through schools - we can't determine without more data
        if (PhaseOfEducationValues.IsIndeterminate(phase))
        {
            return DataWithAvailability<bool>.NotAvailable();
        }

        // Secondary, post-16, and primary schools don't have nursery
        if (PhaseOfEducationValues.IndicatesNoNursery(phase))
        {
            return DataWithAvailability<bool>.Available(false);
        }

        return DataWithAvailability<bool>.NotAvailable();
    }
}