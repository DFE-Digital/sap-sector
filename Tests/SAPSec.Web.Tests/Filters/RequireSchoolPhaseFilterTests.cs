using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Filters;

namespace SAPSec.Web.Tests.Filters;

public class RequireSchoolPhaseFilterTests
{
    private readonly Mock<IRequestSchoolAccessor> _requestSchoolAccessorMock = new();

    [Fact]
    public async Task SecondaryFilter_WithPrimarySchoolIndex_ReturnsNotFound()
    {
        var school = CreateSchoolDetails("123456", "Primary");
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "School",
            action: "Index",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task PrimaryFilter_WithSecondarySchoolIndex_ReturnsNotFound()
    {
        var school = CreateSchoolDetails("123456", "Secondary");
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Primary,
            controller: "School",
            action: "Index",
            area: "Primary",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SecondaryFilter_WithPrimarySchoolOnKs4Route_ReturnsNotFound()
    {
        var school = CreateSchoolDetails("123456", "Primary");
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "School",
            action: "Ks4HeadlineMeasures",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SecondaryFilter_WithTwoSecondaryRouteValues_AllowsExecution()
    {
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(CreateSchoolDetails("123456", "Secondary"));
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "654321"))
            .ReturnsAsync(CreateSchoolDetails("654321", "Secondary"));

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "SimilarSchoolsComparison",
            action: "Similarity",
            routeValues: [("urn", "123456"), ("similarSchoolUrn", "654321")]);

        result.Should().BeNull();
    }

    private async Task<IActionResult?> ExecuteFilterAsync(
        ExpectedSchoolPhase expectedSchoolPhase,
        string controller,
        string action,
        (string Key, string Value)[] routeValues,
        string? area = null)
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        routeData.Values["controller"] = controller;
        routeData.Values["action"] = action;
        if (!string.IsNullOrWhiteSpace(area))
        {
            routeData.Values["area"] = area;
        }

        foreach (var (key, value) in routeValues)
        {
            routeData.Values[key] = value;
        }

        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var actionArguments = new Dictionary<string, object?>();

        var context = new ActionExecutingContext(actionContext, filters, actionArguments, controller: new object());
        var filter = new RequireSchoolPhaseFilter(
            _requestSchoolAccessorMock.Object,
            expectedSchoolPhase,
            routeValues.Select(x => x.Key).ToArray());

        await filter.OnActionExecutionAsync(
            context,
            () => Task.FromResult(new ActionExecutedContext(actionContext, filters, new object())));

        return context.Result;
    }

    private static SchoolDetails CreateSchoolDetails(string urn, string phaseOfEducation) =>
        new()
        {
            Name = "Test School",
            Urn = urn,
            DfENumber = DataWithAvailability.Available("123/4567"),
            Ukprn = DataWithAvailability.Available("10012345"),
            Address = DataWithAvailability.Available("1 Test Street"),
            LocalAuthorityName = DataWithAvailability.Available("Test LA"),
            LocalAuthorityCode = DataWithAvailability.Available("123"),
            Region = DataWithAvailability.Available("Test Region"),
            UrbanRuralDescription = DataWithAvailability.Available("Urban"),
            AgeRangeLow = DataWithAvailability.Available(4),
            AgeRangeHigh = DataWithAvailability.Available(18),
            GenderOfEntry = DataWithAvailability.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability.Available(phaseOfEducation),
            SchoolType = DataWithAvailability.Available("Community school"),
            AdmissionsPolicy = DataWithAvailability.Available("Not applicable"),
            ReligiousCharacter = DataWithAvailability.Available("None"),
            GovernanceStructure = DataWithAvailability.NotAvailable<GovernanceType>(),
            AcademyTrustName = DataWithAvailability.NotAvailable<string>(),
            AcademyTrustId = DataWithAvailability.NotAvailable<string>(),
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(false),
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),
            HeadteacherName = DataWithAvailability.Available("Jane Smith"),
            Website = DataWithAvailability.NotAvailable<string>(),
            Telephone = DataWithAvailability.NotAvailable<string>(),
            Email = DataWithAvailability.NotAvailable<string>()
        };
}
