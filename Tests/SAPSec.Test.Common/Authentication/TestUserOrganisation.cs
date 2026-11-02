using SAPSec.Core.Authentication;

namespace SAPSec.Test.Common.Authentication;

/// <summary>
/// The organisation the test user is signed in as. Defaults to a DfE organisation in a non-school category so
/// existing journeys that rely on "Find a school" keep working; school-user tests switch it to
/// <see cref="OrganisationCategoryIds.Establishment"/> with a non-DfE organisation name.
/// </summary>
public class TestUserOrganisation
{
    public const string DefaultCategoryId = OrganisationCategoryIds.LocalAuthority;

    public const string DefaultUrn = "123456";

    public const string DefaultPhase = "Primary";

    public const string DfeOrganisationName = "Department for Education";

    public const string NonDfeOrganisationName = "Test Organisation";

    public const string DefaultName = DfeOrganisationName;

    public string CategoryId { get; set; } = DefaultCategoryId;

    public string Urn { get; set; } = DefaultUrn;

    public string Phase { get; set; } = DefaultPhase;

    public string Name { get; set; } = DefaultName;

    public void Reset()
    {
        CategoryId = DefaultCategoryId;
        Urn = DefaultUrn;
        Phase = DefaultPhase;
        Name = DefaultName;
    }
}
