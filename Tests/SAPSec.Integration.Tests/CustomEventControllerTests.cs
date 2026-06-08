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
    public async Task PostCustomEventTracking_ReturnRedirect()
    {
        var payload = new { Url = "https://forms.cloud.microsoft", Text = "Text" };
    

//var json = System.Text.Json.JsonSerializer.Serialize(payload);
  //      var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Ensure the request/response headers are set to JSON
        //  fixture.Client.DefaultRequestHeaders.Accept.Clear();
        // fixture.Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        //var response = await fixture.Client.PostAsync("/custom-event-tracking", content);

        var response = await fixture.Client.PostAsJsonAsync("/custom-event-tracking", payload);


        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/custom-event-tracking");
    }
    #endregion
}
