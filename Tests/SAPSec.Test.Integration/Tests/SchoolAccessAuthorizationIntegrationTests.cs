using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests;

/// <summary>
/// Requests the real school page routes as different kinds of user to check that trust and
/// local authority users can only view schools associated with their organisation.
/// </summary>
public class SchoolAccessAuthorizationIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string UsersTrustUid = "5001";
    private const string OtherTrustUid = "5002";
    private const string UsersLaCode = "201";
    private const string OtherLaCode = "202";
    private const string UsersDioceseCode = "CE01";
    private const string OtherDioceseCode = "RC02";

    private const string PrimaryUrn = "100001";
    private const string SecondaryUrn = "200001";
    private const string AllThroughUrn = "300001";

    public static TheoryData<string> SchoolPages =>
    [
        Routes.PrimarySchool(PrimaryUrn).Overview,
        Routes.PrimarySchool(PrimaryUrn).KS2,
        Routes.PrimarySchool(PrimaryUrn).Attendance,
        Routes.PrimarySchool(PrimaryUrn).ViewSimilarSchools,
        Routes.PrimarySchool(PrimaryUrn).SchoolDetails,
        Routes.PrimarySchool(PrimaryUrn).WhatIsASimilarSchool,
        Routes.PrimarySchool(PrimaryUrn).Comparison("100002").Similarity,
        Routes.PrimarySchool(PrimaryUrn).Comparison("100002").Ks2,
        Routes.PrimarySchool(PrimaryUrn).Comparison("100002").Attendance,
        Routes.PrimarySchool(PrimaryUrn).Comparison("100002").SchoolDetails,

        Routes.SecondarySchool(SecondaryUrn).Overview,
        Routes.SecondarySchool(SecondaryUrn).KS4HeadlineMeasures,
        Routes.SecondarySchool(SecondaryUrn).KS4CoreSubjects,
        Routes.SecondarySchool(SecondaryUrn).Attendance,
        Routes.SecondarySchool(SecondaryUrn).ViewSimilarSchools,
        Routes.SecondarySchool(SecondaryUrn).SchoolDetails,
        Routes.SecondarySchool(SecondaryUrn).WhatIsASimilarSchool,
        Routes.SecondarySchool(SecondaryUrn).Comparison("200002").Similarity,
        Routes.SecondarySchool(SecondaryUrn).Comparison("200002").KS4HeadlineMeasures,
        Routes.SecondarySchool(SecondaryUrn).Comparison("200002").KS4CoreSubjects,
        Routes.SecondarySchool(SecondaryUrn).Comparison("200002").Attendance,
        Routes.SecondarySchool(SecondaryUrn).Comparison("200002").SchoolDetails,

        Routes.AllThroughSchool(AllThroughUrn).Overview,
        Routes.AllThroughSchool(AllThroughUrn).KS2,
        Routes.AllThroughSchool(AllThroughUrn).KS4HeadlineMeasures,
        Routes.AllThroughSchool(AllThroughUrn).KS4CoreSubjects,
        Routes.AllThroughSchool(AllThroughUrn).Attendance,
        Routes.AllThroughSchool(AllThroughUrn).ViewSimilarSchools,
        Routes.AllThroughSchool(AllThroughUrn).SchoolDetails,
        Routes.AllThroughSchool(AllThroughUrn).WhatIsASimilarSchool,
        Routes.AllThroughSchool(AllThroughUrn).PrimaryComparison("300002").Similarity,
        Routes.AllThroughSchool(AllThroughUrn).SecondaryComparison("300002").Similarity,
    ];

    public override Task InitializeAsync()
    {
        Fixture.FeatureFlagService.Override(Flags.EnableAllThroughSchools, true);

        // The schools being viewed belong to the user's trust and LA; the comparator
        // schools belong to another trust and LA and must remain comparable.
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(PrimaryUrn, "Primary School", x => x.Open().Primary().WithTrust(UsersTrustUid).InLA(UsersLaCode).WithDiocese(UsersDioceseCode)),
            Build.Establishment("100002", "Primary Comparator 1", x => x.Open().Primary().WithTrust(OtherTrustUid).InLA(OtherLaCode).WithDiocese(OtherDioceseCode)),
            Build.Establishment("100003", "Primary Comparator 2", x => x.Open().Primary().WithTrust(OtherTrustUid).InLA(OtherLaCode).WithDiocese(OtherDioceseCode)),
            Build.Establishment(SecondaryUrn, "Secondary School", x => x.Open().Secondary().WithTrust(UsersTrustUid).InLA(UsersLaCode).WithDiocese(UsersDioceseCode)),
            Build.Establishment("200002", "Secondary Comparator 1", x => x.Open().Secondary().WithTrust(OtherTrustUid).InLA(OtherLaCode).WithDiocese(OtherDioceseCode)),
            Build.Establishment("200003", "Secondary Comparator 2", x => x.Open().Secondary().WithTrust(OtherTrustUid).InLA(OtherLaCode).WithDiocese(OtherDioceseCode)),
            Build.Establishment(AllThroughUrn, "All-through School", x => x.Open().AllThrough().WithTrust(UsersTrustUid).InLA(UsersLaCode).WithDiocese(UsersDioceseCode)),
            Build.Establishment("300002", "All-through Comparator", x => x.Open().AllThrough().WithTrust(OtherTrustUid).InLA(OtherLaCode).WithDiocese(OtherDioceseCode)));

        Fixture.SimilarSchoolsPrimaryRepository
            .SetupGroups([
                .. Build.PrimaryGroup(PrimaryUrn, ["100002", "100003"]),
                .. Build.PrimaryGroup(AllThroughUrn, ["300002"])])
            .SetupValues(Build.PrimaryValues([PrimaryUrn, "100002", "100003", AllThroughUrn, "300002"]));

        Fixture.SimilarSchoolsSecondaryRepository
            .SetupGroups([
                .. Build.SecondaryGroup(SecondaryUrn, ["200002", "200003"]),
                .. Build.SecondaryGroup(AllThroughUrn, ["300002"])])
            .SetupValues(Build.SecondaryValues([SecondaryUrn, "200002", "200003", AllThroughUrn, "300002"]));

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        Fixture.TestUserOrganisation.Clear();
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableAllThroughSchools);

        return base.DisposeAsync();
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenEstablishmentUserViewsTheirOwnSchool_IsAllowed(string path)
    {
        Fixture.TestUserOrganisation.Set(
            Organisation(OrganisationCategoryIds.Establishment, urn: ViewedSchoolUrn(path)));

        await AssertStatusCode(path, HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenEstablishmentUserViewsAnotherSchool_IsForbidden(string path)
    {
        Fixture.TestUserOrganisation.Set(
            Organisation(OrganisationCategoryIds.Establishment, urn: "999999"));

        await AssertStatusCode(path, HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenInternalStaff_IsAllowed(string path)
    {
        Fixture.TestUserOrganisation.Set(new Organisation
        {
            Id = "dfe-org",
            Name = "Department for Education",
            Category = new Category { Id = "003" }
        });

        await AssertStatusCode(path, HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenTrustUserAndSchoolInTheirTrust_IsAllowed(string path)
    {
        Fixture.TestUserOrganisation.Set(Organisation(OrganisationCategoryIds.MultiAcademyTrust, uid: UsersTrustUid));

        await AssertStatusCode(path, HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenTrustUserAndSchoolNotInTheirTrust_IsForbidden(string path)
    {
        Fixture.TestUserOrganisation.Set(Organisation(OrganisationCategoryIds.MultiAcademyTrust, uid: OtherTrustUid));

        await AssertStatusCode(path, HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenLocalAuthorityUserAndSchoolInTheirLocalAuthority_IsAllowed(string path)
    {
        Fixture.TestUserOrganisation.Set(Organisation(OrganisationCategoryIds.LocalAuthority, establishmentNumber: UsersLaCode));

        await AssertStatusCode(path, HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenLocalAuthorityUserAndSchoolNotInTheirLocalAuthority_IsForbidden(string path)
    {
        Fixture.TestUserOrganisation.Set(Organisation(OrganisationCategoryIds.LocalAuthority, establishmentNumber: OtherLaCode));

        await AssertStatusCode(path, HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenDioceseUserAndSchoolInTheirDiocese_IsAllowed(string path)
    {
        Fixture.TestUserOrganisation.Set(Diocese(uid: UsersDioceseCode));

        await AssertStatusCode(path, HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(SchoolPages))]
    public async Task SchoolPage_WhenDioceseUserAndSchoolNotInTheirDiocese_IsForbidden(string path)
    {
        Fixture.TestUserOrganisation.Set(Diocese(uid: OtherDioceseCode));

        await AssertStatusCode(path, HttpStatusCode.Forbidden);
    }

    // PLACEHOLDER: recognised by category name until the DSI category id for a Diocese is confirmed.
    private static Organisation Diocese(string uid) =>
        new()
        {
            Id = "test-diocese",
            Name = "Test Diocese",
            Category = new Category { Id = "TBC", Name = OrganisationCategoryIds.DioceseCategoryName },
            Uid = uid
        };

    private async Task AssertStatusCode(string path, HttpStatusCode expectedStatusCode)
    {
        var response = await Fixture.NonRedirectingClient.GetAsync(path);

        response.StatusCode.Should().Be(expectedStatusCode, because: $"of the response to {path}");
    }

    // The school being viewed is the first URN in the path; any later URN is the comparator school.
    private static string ViewedSchoolUrn(string path) =>
        System.Text.RegularExpressions.Regex.Match(path, @"/school/[a-z-]+/(\d+)").Groups[1].Value;

    private static Organisation Organisation(
        string categoryId,
        string? uid = null,
        string? establishmentNumber = null,
        string? urn = null) =>
        new()
        {
            Id = "test-org",
            Name = "Test Organisation",
            Category = new Category { Id = categoryId },
            Uid = uid,
            EstablishmentNumber = establishmentNumber,
            Urn = urn
        };
}
