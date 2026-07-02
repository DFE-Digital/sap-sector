using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Rules;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Rules;

/// <summary>
/// Business rule: Determines if school has resourced provision.
/// Single Responsibility: Only handles resourced provision logic.
/// </summary>
public sealed class ResourcedProvisionRule : IBusinessRule<string>
{
    public static readonly string NoResourcedProvisionText = "Does not have a resourced provision";
    public static readonly string HasResourcedProvisionText = "Has a resourced provision";
    public static readonly string NoDataAvailableText = "No data available";

    public DataWithAvailability<string> Evaluate(Establishment establishment)
    {
        var resourcedProvisionName = establishment.ResourcedProvisionName;

        if (ResourcedProvisionValues.NoResourcedProvision(resourcedProvisionName))
        {
            return DataWithAvailability.Available(NoResourcedProvisionText);
        }
        if (ResourcedProvisionValues.HasResourcedProvision(resourcedProvisionName))
        {
            return DataWithAvailability.Available(HasResourcedProvisionText);
        }
        return DataWithAvailability.Available(NoDataAvailableText);
    }
}