using SAPSec.Core.Authentication;

namespace SAPSec.Web.Authorization;

/// <summary>
/// Decides whether an organisation belongs to internal (DfE) staff, who may view every school.
/// </summary>
public interface IInternalStaffChecker
{
    bool IsInternalStaff(Organisation organisation);
}
