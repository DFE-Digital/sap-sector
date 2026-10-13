using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Services;

namespace SAPSec.Web.Tests.Deprecated.Filters;

public class RequireSchoolPhaseFilterTests
{
    private readonly Mock<IRequestSchoolAccessor> _requestSchoolAccessorMock = new();
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock = new();

    public RequireSchoolPhaseFilterTests()
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnablePrimarySchools))
            .ReturnsAsync(true);
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnableAllThroughSchools))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task SecondaryFilter_WithPrimarySchoolIndex_RedirectsToPrimaryPath()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.PrimaryId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "School",
            action: "Index",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be(Routes.PrimarySchool("123456").Overview);
    }

    [Fact]
    public async Task PrimaryFilter_WithSecondarySchoolIndex_RedirectsToSecondaryPath()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.SecondaryId);
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
            .Which.Url.Should().Be(Routes.SecondarySchool("123456").Overview);
    }

    [Fact]
    public async Task SecondaryFilter_WithPrimarySchoolOnKs4Route_PreservesPathOnPrimaryRedirect()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.PrimaryId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "School",
            action: "Ks4HeadlineMeasures",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"{Routes.PrimarySchool("123456").Overview}/ks4-headline-measures");
    }

    [Fact]
    public async Task PrimaryFilter_WithSecondarySchoolAndPathBase_PreservesPathBaseInRedirect()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.SecondaryId);
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
            .Which.Url.Should().Be("/app" + Routes.SecondarySchool("123456").Overview);
    }

    [Fact]
    public async Task PrimaryFilter_WithPrimarySchoolAndPrimaryFeatureDisabled_ReturnsNotFound()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.PrimaryId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnablePrimarySchools))
            .ReturnsAsync(false);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Primary,
            controller: "School",
            action: "Index",
            area: "Primary",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task PrimaryFilter_WithAllThroughSchoolAndAllThroughFeatureDisabled_ReturnsNotFound()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.AllThroughId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnableAllThroughSchools))
            .ReturnsAsync(false);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Primary,
            controller: "School",
            action: "Index",
            area: "Primary",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task PrimaryFilter_WithAllThroughSchoolAndOnlyAllThroughFeatureEnabled_RedirectsToAllThroughPath()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.AllThroughId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnablePrimarySchools))
            .ReturnsAsync(false);
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnableAllThroughSchools))
            .ReturnsAsync(true);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Primary,
            controller: "School",
            action: "Index",
            area: "Primary",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be(Routes.AllThroughSchool("123456").Overview);
    }

    [Fact]
    public async Task AllThroughFilter_WithAllThroughSchoolAndAllThroughFeatureEnabled_AllowsExecution()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.AllThroughId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnableAllThroughSchools))
            .ReturnsAsync(true);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.AllThrough,
            controller: "School",
            action: "Index",
            area: "AllThrough",
            routeValues: [("urn", "123456")]);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AllThroughFilter_WithAllThroughSchoolAndAllThroughFeatureDisabled_ReturnsNotFound()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.AllThroughId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(Flags.EnableAllThroughSchools))
            .ReturnsAsync(false);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.AllThrough,
            controller: "School",
            action: "Index",
            area: "AllThrough",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SecondaryFilter_WithUnsupportedSchoolPhase_ReturnsNotFound()
    {
        var school = CreateSchool("123456", "Special");
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
        var school = CreateSchool("123456", PhaseOfEducationId.PrimaryId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "School",
            action: "Attendance",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be(Routes.PrimarySchool("123456").Attendance);
    }

    [Fact]
    public async Task SecondaryFilter_WithPrimarySchoolOnDataRoute_PreservesPathOnPrimaryRedirect()
    {
        var school = CreateSchool("123456", PhaseOfEducationId.PrimaryId);
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(school);

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "School",
            action: "AttendanceData",
            routeValues: [("urn", "123456")]);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"{Routes.PrimarySchool("123456").Overview}/attendance-data");
    }

    [Fact]
    public async Task SecondaryFilter_WithTwoSecondaryRouteValues_AllowsExecution()
    {
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "123456"))
            .ReturnsAsync(CreateSchool("123456", PhaseOfEducationId.SecondaryId));
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "654321"))
            .ReturnsAsync(CreateSchool("654321", PhaseOfEducationId.SecondaryId));

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
            .ReturnsAsync(CreateSchool("123456", PhaseOfEducationId.SecondaryId));
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "654321"))
            .ReturnsAsync(CreateSchool("654321", PhaseOfEducationId.PrimaryId));

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.Secondary,
            controller: "SimilarSchoolsComparison",
            action: "Similarity",
            routeValues: [("urn", "123456"), ("similarSchoolUrn", "654321")]);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task PrimaryComparisonParticipantFilter_WithAllThroughPrimaryComparisonUrlAndSecondaryComparator_ReturnsNotFound()
    {
        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext?>(), "137157"))
            .ReturnsAsync(CreateSchool("137157", PhaseOfEducationId.SecondaryId));

        var result = await ExecuteFilterAsync(
            ExpectedSchoolPhase.PrimaryComparisonParticipant,
            controller: "Comparison",
            action: "PrimarySimilarity",
            routeValues: [("urn", "134314"), ("comparatorSchoolUrn", "137157")],
            area: "AllThrough",
            routeParameterNames: ["comparatorSchoolUrn"]);

        result.Should().BeOfType<NotFoundResult>();
    }

    private async Task<IActionResult?> ExecuteFilterAsync(
        ExpectedSchoolPhase expectedSchoolPhase,
        string controller,
        string action,
        (string Key, string Value)[] routeValues,
        string? area = null,
        string? pathBase = null,
        string[]? routeParameterNames = null)
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
            _featureFlagServiceMock.Object,
            expectedSchoolPhase,
            routeParameterNames ?? routeValues.Select(x => x.Key).ToArray());

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
                "Ks2" => Routes.PrimarySchool(urn).KS2,
                "Attendance" => Routes.PrimarySchool(urn).Attendance,
                "SchoolDetails" => Routes.PrimarySchool(urn).SchoolDetails,
                "WhatIsASimilarSchool" => Routes.PrimarySchool(urn).WhatIsASimilarSchool,
                "ViewSimilarSchools" => Routes.PrimarySchool(urn).ViewSimilarSchools,
                _ => Routes.PrimarySchool(urn).Overview
            };
        }

        if (string.Equals(area, "AllThrough", StringComparison.OrdinalIgnoreCase))
        {
            var comparatorSchoolUrn = routeValues.FirstOrDefault(x => x.Key == "comparatorSchoolUrn").Value;

            return action switch
            {
                "PrimarySimilarity" => Routes.AllThroughSchool(urn).PrimaryComparison(comparatorSchoolUrn).Similarity,
                "SecondarySimilarity" => Routes.AllThroughSchool(urn).SecondaryComparison(comparatorSchoolUrn).Similarity,
                _ => Routes.AllThroughSchool(urn).Overview
            };
        }

        return action switch
        {
            "Attendance" => Routes.SecondarySchool(urn).Attendance,
            "AttendanceData" => Routes.SecondarySchool(urn).AttendanceData,
            "SchoolDetails" => Routes.SecondarySchool(urn).SchoolDetails,
            "WhatIsASimilarSchool" => Routes.SecondarySchool(urn).WhatIsASimilarSchool,
            "ViewSimilarSchools" => Routes.SecondarySchool(urn).ViewSimilarSchools,
            "Ks4HeadlineMeasures" => Routes.SecondarySchool(urn).KS4HeadlineMeasures,
            _ => Routes.SecondarySchool(urn).Overview
        };
    }

    private static LinkedSchool CreateSchool(string urn, string phaseOfEducationId) => new(
        urn,
        "Test School",
        new("123", "Test LA"),
        new("1 Test Street"),
        EstablishmentStatusId.OpenId,
        phaseOfEducationId,
        null,
        false,
        [],
        []);
}
