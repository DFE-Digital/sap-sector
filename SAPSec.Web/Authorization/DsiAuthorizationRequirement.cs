using Microsoft.AspNetCore.Authorization;

namespace SAPSec.Web.Authorization;

public class DsiAuthorizationRequirement : IAuthorizationRequirement
{
    public const string PolicyName = "DsiAuthorizationPolicy";
}
