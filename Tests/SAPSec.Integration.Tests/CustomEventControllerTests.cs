using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class CustomEventControllerTests(WebApplicationSetupFixture fixture)
{
    #region POST /custom-event-tracking Tests

    [Fact]
    public async Task PostCustomEventTracking_ReturnsOK()
    {
        var payload = new { Url = "https://forms.cloud.microsoft/Pages", Text = "Text" };

        var response = await fixture.Client.PostAsJsonAsync("/custom-event-tracking", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    #endregion
}
