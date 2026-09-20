using SAPSec.Core.Features.Availability;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Business rule: Determines if school has resourced provision.
/// Single Responsibility: Only handles resourced provision logic.
/// </summary>
public sealed class ResourcedProvisionRule : IBusinessRule<bool>
{
    public DataWithAvailability<bool> Evaluate(Establishment establishment)
    {
        var resourcedProvisionName = establishment.ResourcedProvisionName;

        if (ResourcedProvisionValues.NoResourcedProvision(resourcedProvisionName))
        {
            return DataWithAvailability.Available(false);
        }
        if (ResourcedProvisionValues.HasResourcedProvision(resourcedProvisionName))
        {
            return DataWithAvailability.Available(true);
        }

        return DataWithAvailability.NotAvailable<bool>();
    }
}