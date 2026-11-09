using SAPSec.Core.Authentication;

namespace SAPSec.Test.Common.Authentication;

/// <summary>
/// The organisation the test user is signed in as, and their email address. Defaults to a DfE email in a
/// non-school category so existing journeys that rely on "Find a school" keep working; school-user tests switch it
/// to <see cref="OrganisationCategoryIds.Establishment"/> with a non-DfE email.
/// </summary>
public class TestUserOrganisation
{
    public const string DefaultCategoryId = OrganisationCategoryIds.LocalAuthority;

    public const string DefaultUrn = "123456";

    public const string DefaultPhase = "Primary";

    public const string DefaultName = "Test Organisation";

    public const string DfeEmailDomain = "education.gov.uk";

    public const string DfeEmail = "test.user@" + DfeEmailDomain;

    public const string NonDfeEmail = "test.user@example.com";

    public const string DefaultEmail = DfeEmail;

    public string CategoryId { get; set; } = DefaultCategoryId;

    public string Urn { get; set; } = DefaultUrn;

    public string Phase { get; set; } = DefaultPhase;

    public string Name { get; set; } = DefaultName;

    public string Email { get; set; } = DefaultEmail;

    public void Reset()
    {
        CategoryId = DefaultCategoryId;
        Urn = DefaultUrn;
        Phase = DefaultPhase;
        Name = DefaultName;
        Email = DefaultEmail;
    }
}
