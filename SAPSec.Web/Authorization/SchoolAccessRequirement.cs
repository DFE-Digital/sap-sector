using Microsoft.AspNetCore.Authorization;

namespace SAPSec.Web.Authorization;

/// <summary>
/// A school user may only reach resources for their own school. Pages for a school carry its URN in the
/// "urn" route value and are checked against the user's school; a check without a URN is denied for school
/// users. Access to the "Find a school" search is a separate policy (<see cref="FindASchoolRequirement"/>).
/// </summary>
public class SchoolAccessRequirement : IAuthorizationRequirement
{
    public const string UrnRouteValueName = "urn";
}
