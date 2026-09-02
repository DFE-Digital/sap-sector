using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests.Secondary;

public class SchoolSchoolDetailsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    [Fact]
    public async Task NonExistentUrn_ReturnsNotFound()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()));

        await Fixture.RequestPageAsync(Routes.SecondarySchool("999999").SchoolDetails, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CaptionAndHeading_DisplaysSchoolName()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Test School 1", x => x.Open().Secondary()));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool("100001").SchoolDetails, HttpStatusCode.OK);

        var heading = page.ElementShouldExist("h1.govuk-heading-xl");
        heading.TrimmedTextContent().Should().Be("School details");
        var caption = page.ElementShouldExist("span.govuk-caption-xl");
        caption.TrimmedTextContent().Should().Be("Test School 1");
    }
}
