using Microsoft.AspNetCore.Authorization;

namespace SAPSec.Web.Authorization;

/// <summary>
/// A school user may only reach resources for their own school. Pages for a school carry its URN in the
/// "urn" route value and are checked against the user's school. Anything without one (the "Find a school"
/// search, or a check made without a request such as the navigation) covers every school, so school users
/// can never satisfy it.
/// </summary>
public class SchoolAccessRequirement : IAuthorizationRequirement
{
    public const string UrnRouteValueName = "urn";
}
