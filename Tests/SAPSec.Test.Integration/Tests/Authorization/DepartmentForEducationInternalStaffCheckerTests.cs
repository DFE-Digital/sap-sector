using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Web.Authorization;

namespace SAPSec.Test.Integration.Tests.Authorization;

public class DepartmentForEducationInternalStaffCheckerTests
{
    private readonly DepartmentForEducationInternalStaffChecker _checker = new();

    [Theory]
    [InlineData("Department for Education")]
    [InlineData("DEPARTMENT FOR EDUCATION")]
    [InlineData("  Department for Education  ")]
    public void IsInternalStaff_MatchingName_ReturnsTrue(string name)
    {
        _checker.IsInternalStaff(new Organisation { Name = name }).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Department for Education Scotland")]
    [InlineData("Wetherby High School")]
    public void IsInternalStaff_OtherName_ReturnsFalse(string name)
    {
        _checker.IsInternalStaff(new Organisation { Name = name }).Should().BeFalse();
    }
}
