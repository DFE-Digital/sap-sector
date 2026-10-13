using SAPSec.Core.Authentication;

namespace SAPSec.Test.Common.Authentication;

/// <summary>
/// The roles held by the signed-in test user. By default the test user is a DfE user, so tests
/// can use find-a-school search; call <see cref="Set"/> with no roles to act as a non-DfE user.
/// </summary>
public class TestUserRoles
{
    private static readonly string[] DefaultRoles = [UserRoles.DfeUser];

    public IReadOnlySet<string> Roles { get; private set; } = new HashSet<string>(DefaultRoles);

    public void Set(params string[] roles) => Roles = new HashSet<string>(roles);

    public void Reset() => Roles = new HashSet<string>(DefaultRoles);
}
