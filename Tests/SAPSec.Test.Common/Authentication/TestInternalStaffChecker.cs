using SAPSec.Core.Authentication;
using SAPSec.Web.Authorization;

namespace SAPSec.Test.Common.Authentication;

/// <summary>
/// Treats the default mock test user as internal staff, so the many page tests that visit arbitrary
/// URNs are not restricted. Once a test sets an organisation through <see cref="TestUserOrganisation"/>
/// the real internal staff rule applies, so access rules can be tested for that organisation.
/// </summary>
public class TestInternalStaffChecker(TestUserOrganisation testUserOrganisation) : IInternalStaffChecker
{
    private readonly DepartmentForEducationInternalStaffChecker _realChecker = new();

    public bool IsInternalStaff(Organisation organisation) =>
        testUserOrganisation.Override is null || _realChecker.IsInternalStaff(organisation);
}
