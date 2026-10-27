using SAPSec.Core.Authentication;

namespace SAPSec.Web.Authorization;

/// <summary>
/// PLACEHOLDER: there is no agreed rule for identifying internal staff yet.
/// Replace this implementation (or its registration in Program.cs) once the rules are defined,
/// e.g. a DSI role, an organisation id / UKPRN, or configuration.
/// </summary>
public class DepartmentForEducationInternalStaffChecker : IInternalStaffChecker
{
    private const string DepartmentForEducationName = "Department for Education";

    public bool IsInternalStaff(Organisation organisation) =>
        string.Equals(organisation.Name?.Trim(), DepartmentForEducationName, StringComparison.OrdinalIgnoreCase);
}
