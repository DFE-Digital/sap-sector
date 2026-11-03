using SAPSec.Core.Authentication;

namespace SAPSec.Test.Common.Authentication;

/// <summary>
/// The organisation category of the signed-in test user. Defaults to a non-school category so existing
/// journeys that rely on "Find a school" keep working; school-user tests switch it to
/// <see cref="OrganisationCategoryIds.Establishment"/>.
/// </summary>
public class TestUserOrganisation
{
    public const string DefaultCategoryId = OrganisationCategoryIds.LocalAuthority;

    public const string DefaultUrn = "123456";

    public const string DefaultPhase = "Primary";

    public string CategoryId { get; set; } = DefaultCategoryId;

    public string Urn { get; set; } = DefaultUrn;

    public string Phase { get; set; } = DefaultPhase;

    public void Reset()
    {
        CategoryId = DefaultCategoryId;
        Urn = DefaultUrn;
        Phase = DefaultPhase;
    }
}
