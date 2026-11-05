using System.Globalization;
using System.Net;
using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Data.Dto;
using SAPSec.Test.Common.Authentication;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;

namespace SAPSec.Test.Integration.Tests;

[Collection("InMemoryRepositoryIntegrationTestsCollection")]
public class SchoolUserEligibilityIntegrationTests(InMemoryRepositoryIntegrationTestFixture fixture) : IAsyncLifetime
{
    private const string Urn = "123456";

    public Task InitializeAsync()
    {
        fixture.TestUser.CategoryId = OrganisationCategoryIds.Establishment;
        fixture.TestUser.Name = TestUserOrganisation.NonDfeOrganisationName;
        fixture.TestUser.Urn = Urn;
        fixture.FeatureFlagService.Override(Flags.EnableFindASchool, false);
        fixture.FeatureFlagService.Override(Flags.EnablePrimarySchools, true);
        fixture.FeatureFlagService.Override(Flags.EnableAllThroughSchools, true);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        fixture.TestUser.Reset();
        fixture.FeatureFlagService.ClearOverrides(Flags.EnableFindASchool);
        fixture.FeatureFlagService.ClearOverrides(Flags.EnablePrimarySchools);
        fixture.FeatureFlagService.ClearOverrides(Flags.EnableAllThroughSchools);
        fixture.ClearDownRepositories();
        return Task.CompletedTask;
    }

    public static IEnumerable<object[]> EligibilityCases()
    {
        foreach (var phase in new[] { "Primary", "Secondary", "All-through", "Nursery", "Special", "Not applicable", "Middle deemed secondary" })
        foreach (var status in new[] { "1", "2", "3", "4" })
        {
            var supported = phase is "Primary" or "Secondary" or "All-through";
            yield return [phase, status, 1, supported && status != "4"];
            if (status == "2")
            {
                yield return [phase, status, 0, supported];
                yield return [phase, status, -1, false];
            }
        }
    }

    [Theory]
    [MemberData(nameof(EligibilityCases))]
    public async Task LoginAndDirectAccess_UseSearchEligibility(string phase, string status, int daysFromCutoff, bool eligible)
    {
        fixture.TestUser.Phase = phase;
        var now = DateTime.UtcNow;
        var cutoff = new DateTime(now.Year, 9, 12);
        if (now < cutoff) cutoff = cutoff.AddYears(-1);

        fixture.EstablishmentRepository.SetupEstablishments(new Establishment
        {
            URN = Urn,
            EstablishmentName = "Test School",
            PhaseOfEducationName = phase,
            EstablishmentStatusId = status,
            CloseDate = cutoff.AddYears(-3).AddDays(daysFromCutoff).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)
        });

        var login = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);
        login.StatusCode.Should().Be(eligible ? HttpStatusCode.Redirect : HttpStatusCode.NotFound);

        if (eligible)
        {
            login.Headers.Location!.OriginalString.Should().Be(Routes.School(Urn, phase));
            var overview = await fixture.Client.GetAsync(Routes.UserHome);
            overview.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        else
        {
            foreach (var path in new[] { Routes.SecondarySchool(Urn).Overview, Routes.PrimarySchool(Urn).SchoolDetails, Routes.AllThroughSchool(Urn).Overview })
            {
                var direct = await fixture.NonRedirectingClient.GetAsync(path);
                direct.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }

    [Fact]
    public async Task RestrictionsDisabled_ProposedSchoolStillUsesExistingLoginRouting()
    {
        fixture.FeatureFlagService.Override(Flags.EnableFindASchool, true);
        fixture.EstablishmentRepository.SetupEstablishments(new Establishment
        {
            URN = Urn, PhaseOfEducationName = "Primary", EstablishmentStatusId = "4"
        });

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }
}
