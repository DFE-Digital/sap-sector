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
    private readonly Mock<IOptions<CustomEventPatterns>> _optionsMock;
    private readonly CustomEventController _sut;

    public CustomEventControllerTests()
    {
        _customEventServiceMock = new Mock<ICustomEventService>();
        _optionsMock = new Mock<IOptions<CustomEventPatterns>>();
        _optionsMock.Setup(x => x.Value).Returns(new CustomEventPatterns
        {
            FeedbackForm = "^https://forms.cloud.microsoft.+$",
            SignIn = "^.*/auth/signin.*$",
            MailTo = "^mailto:.*$",
            ServiceUrls = "^https://get-school-improvement-insights.education.gov.uk.*$|https://get-school-improvement-insights-test.test.teacherservices.cloud.*$"
        });
        _sut = new CustomEventController(_customEventServiceMock.Object, _optionsMock.Object);
    }

    [Theory]
    [InlineData("https://forms.cloud.microsoft/Pages", "feedback_link_click")]
    [InlineData("https://get-school-improvement-insights.education.gov.uk/auth/signin", "cta_start_now_click")]
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
    [InlineData("https://get-school-improvement-insights.education.gov.uk/school/123456")]
    [InlineData("https://get-school-improvement-insights-test.test.teacherservices.cloud/school/123456")]
    public async Task PostCustomEventTracking_DoesNotSendEventForNonMatchingUrls(string url)
    {
        var clickData = new ClickData { Text = "text", Url = url };

        var result = await _sut.CustomEventTracking(clickData);

        result.Should().BeOfType<OkResult>();

        _customEventServiceMock.Verify(x => x.SendCustomEvent(It.Is<ClickData>(c => c.Url == url), It.IsAny<string>()), Times.Never);
    }
}