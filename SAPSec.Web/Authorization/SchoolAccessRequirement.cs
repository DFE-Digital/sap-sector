using Microsoft.AspNetCore.Authorization;

namespace SAPSec.Web.Authorization;

/// <summary>
/// Restricts trust and local authority users to schools associated with their organisation.
/// Applies to the school identified by the "urn" route value.
/// </summary>
public class SchoolAccessRequirement : IAuthorizationRequirement
{
    public const string UrnRouteValueName = "urn";
}
