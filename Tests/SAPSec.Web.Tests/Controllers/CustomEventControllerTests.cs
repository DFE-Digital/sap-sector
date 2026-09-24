using Dfe.Analytics.Events;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Configuration;
using SAPSec.Web.Controllers;
using System.Net;

namespace SAPSec.Web.Tests.Controllers;

public class CustomEventControllerTests
{
    private readonly Mock<ICustomEventService> _customEventServiceMock;
    private readonly Mock<IOptions<CustomEventLocations>> _optionsMock;
    private readonly CustomEventController _sut;

    public CustomEventControllerTests()
    {
        _customEventServiceMock = new Mock<ICustomEventService>();
        _optionsMock = new Mock<IOptions<CustomEventLocations>>();
        _optionsMock.Setup(x => x.Value).Returns(new CustomEventLocations
        {
            FeedbackForm = "https://forms.cloud.microsoft",
            SignIn = "/auth/signin",
            MailTo = "mailto:",
            ServiceUrls = [
                "https://compare-connect-similar-schools.education.gov.uk",
                "https://compare-connect-similar-schools-test.test.teacherservices.cloud",
                "https://test.compare-connect-similar-schools.gov.uk"]
        });
        _sut = new CustomEventController(_customEventServiceMock.Object, _optionsMock.Object);
    }

    [Theory]
    [InlineData("https://forms.cloud.microsoft/Pages", "feedback_link_click")]
    [InlineData("https://compare-connect-similar-schools.education.gov.uk/auth/signin", "cta_start_now_click")]
    [InlineData("https://www.example.com", "outbound_link_click")]
    [InlineData("mailto:test@example.com", "mailto_link_click")]
    public async Task CustomEventTracking_SendsCustomEvent(string url, string eventName)
    {
        var clickData = new ClickData { Text = "text", Url = url };

        var result = await _sut.CustomEventTracking(clickData);

        result.Should().BeOfType<OkResult>();

        _customEventServiceMock.Verify(x => x.SendCustomEvent(It.Is<ClickData>(c => c.Url == url), eventName), Times.Once);
    }

    [Theory]
    [InlineData("https://compare-connect-similar-schools.education.gov.uk/school/123456")]
    [InlineData("https://compare-connect-similar-schools-test.test.teacherservices.cloud/school/123456")]
    [InlineData("https://test.compare-connect-similar-schools.gov.uk/school/123456")]
    [InlineData("https://compare-connect-similar-schools-pr-240.test.teacherservices.cloud/school/123456")]
    public async Task PostCustomEventTracking_DoesNotSendEventForNonMatchingUrls(string url)
    {
        var clickData = new ClickData { Text = "text", Url = url };

        var result = await _sut.CustomEventTracking(clickData);

        result.Should().BeOfType<OkResult>();

        _customEventServiceMock.Verify(x => x.SendCustomEvent(It.Is<ClickData>(c => c.Url == url), It.IsAny<string>()), Times.Never);
    }
}