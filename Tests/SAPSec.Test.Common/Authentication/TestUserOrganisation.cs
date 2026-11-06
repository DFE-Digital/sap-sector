using SAPSec.Core.Authentication;

namespace SAPSec.Test.Common.Authentication;

/// <summary>
/// Lets tests override the signed-in test user's current organisation (e.g. to act as a trust or local authority user).
/// </summary>
public class TestUserOrganisation
{
    public Organisation? Override { get; private set; }

    public void Set(Organisation organisation) => Override = organisation;

    public void Clear() => Override = null;
}
