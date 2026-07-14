using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Filters;

public sealed class RequireFeatureFlagAttribute : TypeFilterAttribute
{
    public RequireFeatureFlagAttribute(string expectedFeatureFlag)
        : base(typeof(RequireFeatureFlagFilter))
    {
        Arguments =
        [
            expectedFeatureFlag
        ];
    }
}
