using Microsoft.AspNetCore.Authorization;

namespace SAPSec.Web.Authorization;

/// <summary>
/// Whether the "Home" breadcrumb navigation is shown. Its Home link leads to "Find a school", so it is
/// hidden from users who must not reach it. Unlike <see cref="SchoolAccessRequirement"/> it also applies to
/// people who are not signed in (the cookies and terms pages can be read before sign-in).
/// </summary>
public class BreadcrumbNavigationRequirement : IAuthorizationRequirement { }
