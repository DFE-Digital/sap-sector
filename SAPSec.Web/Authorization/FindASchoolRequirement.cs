using Microsoft.AspNetCore.Authorization;

namespace SAPSec.Web.Authorization;

/// <summary>
/// Whether the user can use the "Find a school" search, which covers every school. While the
/// EnableFindASchool flag is on everyone can; otherwise only DfE staff (by email domain) can.
/// </summary>
public class FindASchoolRequirement : IAuthorizationRequirement { }
