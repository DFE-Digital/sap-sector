using Dfe.Analytics.Events;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NSubstitute;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;

namespace SAPSec.Web.Tests.Controllers;

public class CustomEventControllerTests
{
    private readonly Mock<ICustomEventService> _customEventServiceMock;
    private readonly CustomEventController _sut;

    public CustomEventControllerTests()
    {
        _customEventServiceMock = new Mock<ICustomEventService>();
        _sut = new CustomEventController(_customEventServiceMock.Object);
    }

    [Theory]
    [InlineData("https://forms.cloud.microsoft", "feedback_link_click")]
    [InlineData("https://get-school-improvement-insights.education.gov.uk//auth/signin", "cta_start_now_click")]
    [InlineData("https://www.example.com", "outbound_link_click")]
    [InlineData("https://get-school-improvement-insights.education.gov.uk/school/136994/route#location", "inbound_link_clik")]
    [InlineData("https://get-school-improvement-insights.education.gov.uk/school/136994", "overview_page")]

    public async Task CustomEventTracking_SendsCustomEvent(string url, string eventName)
    {
        var clickData = new ClickData { Text = "text", Url = url };

        var result = await _sut.CustomEventTracking(clickData);

        _customEventServiceMock.Verify(x => x.SendCustomEvent(It.Is<ClickData>(c => c.Url == url), eventName), Times.Once);
    }
}