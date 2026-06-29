using FluentAssertions;
using SAPSec.Test.Integration.Setup;
using System.Net;
using System.Net.Http.Json;

namespace SAPSec.Test.Integration;

[Collection("IntegrationTestsCollection")]
public class CustomEventControllerTests(IntegrationTestFixture fixture)
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
