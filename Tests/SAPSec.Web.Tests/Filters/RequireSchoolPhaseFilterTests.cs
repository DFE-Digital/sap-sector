using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using SAPSec.Core.Model;
using SAPSec.Web.Filters;
using SAPSec.Web.Services;

namespace SAPSec.Web.Tests.Filters;

public class RequireSchoolPhaseFilterTests
{
    private readonly Mock<IRequestSchoolAccessor> _requestSchoolAccessorMock = new();

    [Fact]
    public async Task SecondaryFilter_WithPrimarySchoolIndex_RedirectsToPrimaryPath()
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

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/school/primary/123456");
    }

    [Fact]
    public async Task PrimaryFilter_WithSecondarySchoolIndex_RedirectsToSecondaryPath()
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

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/school/123456");
    }

    [Fact]
    public async Task SecondaryFilter_WithPrimarySchoolOnKs4Route_RedirectsToPrimaryOverview()
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

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/school/primary/123456");
    }

    [Fact]
    public async Task PrimaryFilter_WithSecondarySchoolAndPathBase_PreservesPathBaseInRedirect()
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
            routeValues: [("urn", "123456")],
            pathBase: "/app");

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/app/school/123456");
    }

    [Fact]
    public async Task SecondaryFilter_WithUnsupportedSchoolPhase_ReturnsNotFound()
    {
        var school = CreateSchoolDetails("123456", "Special");
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
    public async Task SecondaryFilter_WithPrimarySchoolOnAttendanceRoute_RedirectsToPrimaryAttendance()
    {
        var school = CreateSchoolDetails("123456", "Primary");
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "School",
            action: "Attendance",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/school/primary/123456/attendance");
    }

    [Fact]
    public async Task SecondaryFilter_WithPrimarySchoolOnDataRoute_RedirectsToPrimaryOverview()
    {
        var school = CreateSchoolDetails("123456", "Primary");
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "School",
            action: "AttendanceData",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/school/primary/123456");
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

    [Fact]
    public async Task SecondaryFilter_WithTwoRouteValuesAndPrimaryMismatch_ReturnsNotFound()
    {
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(CreateSchoolDetails("123456", "Secondary"));
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "654321"))
            .ReturnsAsync(CreateSchoolDetails("654321", "Primary"));

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "SimilarSchoolsComparison",
            action: "Similarity",
            routeValues: [("urn", "123456"), ("similarSchoolUrn", "654321")]);

        result.Should().BeOfType<NotFoundResult>();
    }

    private async Task<IActionResult?> ExecuteFilterAsync(
        ExpectedSchoolPhase expectedSchoolPhase,
        string controller,
        string action,
        (string Key, string Value)[] routeValues,
        string? area = null,
        string? pathBase = null)
    {
        var httpContext = new DefaultHttpContext();
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            httpContext.Request.PathBase = new PathString(pathBase);
        }
        httpContext.Request.Path = BuildRequestPath(area, routeValues, action);

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

    private static PathString BuildRequestPath(string? area, (string Key, string Value)[] routeValues, string action)
    {
        var urn = routeValues.FirstOrDefault(x => x.Key == "urn").Value;

        if (string.Equals(area, "Primary", StringComparison.OrdinalIgnoreCase))
        {
            return action switch
            {
                "Ks2" => $"/school/primary/{urn}/ks2",
                "Attendance" => $"/school/primary/{urn}/attendance",
                "SchoolDetails" => $"/school/primary/{urn}/school-details",
                "WhatIsASimilarSchool" => $"/school/primary/{urn}/what-is-a-similar-school",
                "ViewSimilarSchools" => $"/school/primary/{urn}/view-similar-schools",
                _ => $"/school/primary/{urn}"
            };
        }

        return action switch
        {
            "Attendance" => $"/school/{urn}/attendance",
            "AttendanceData" => $"/school/{urn}/attendance-data",
            "SchoolDetails" => $"/school/{urn}/school-details",
            "WhatIsASimilarSchool" => $"/school/{urn}/what-is-a-similar-school",
            "ViewSimilarSchools" => $"/school/{urn}/view-similar-schools",
            "Ks4HeadlineMeasures" => $"/school/{urn}/ks4-headline-measures",
            _ => $"/school/{urn}"
        };
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
