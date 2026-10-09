using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SAPSec.Core.Authentication;
using SAPSec.Web.Authorization;
using System.Security.Claims;

namespace SAPSec.Test.Integration.Tests.Authorization;

public class DsiAuthorizationHandlerTests
{
    private static readonly DsiAuthorizationRequirement Requirement = new();

    [Fact]
    public async Task EstablishmentUser_OwnUrn_Succeeds()
    {
        var context = await HandleAsync(Establishment("123456"), requestedUrn: "123456");

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task EstablishmentUser_OwnUrnWithWhitespace_Succeeds()
    {
        var context = await HandleAsync(Establishment("123456"), requestedUrn: " 123456 ");

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task EstablishmentUser_DifferentUrn_Fails()
    {
        var context = await HandleAsync(Establishment("123456"), requestedUrn: "999999");

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task EstablishmentUser_WithNullUrn_ViewingAnyUrn_Fails()
    {
        var context = await HandleAsync(Establishment(null), requestedUrn: "123456");

        context.HasFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("Local Authority")]
    [InlineData("Multi-Academy Trust")]
    public async Task NonEstablishmentUser_ViewingUrn_Fails(string category)
    {
        var org = Establishment("123456");
        org.Category = new Category { Id = "002", Name = category };

        var context = await HandleAsync(org, requestedUrn: "123456");

        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task User_WithNoOrganisation_Fails()
    {
        var context = await HandleAsync(organisation: null, requestedUrn: "123456");

        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task NonEstablishmentUser_OnRouteWithoutUrn_Succeeds()
    {
        var org = Establishment("123456");
        org.Category = new Category { Id = "002", Name = "Local Authority" };

        var context = await HandleAsync(org, requestedUrn: null);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task EstablishmentUser_ComparatorUrnIsNotChecked()
    {
        // Only the "urn" route value is checked; comparatorSchoolUrn is intentionally open.
        var context = await HandleAsync(
            Establishment("123456"),
            requestedUrn: "123456",
            extraRouteValue: ("comparatorSchoolUrn", "999999"));

        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("Department for Education")]
    [InlineData("  department for education  ")]
    public async Task InternalStaff_CanOpenAnyUrn(string organisationName)
    {
        var org = new Organisation
        {
            Id = "dfe-1",
            Name = organisationName,
            Category = new Category { Id = "003", Name = "Other Stakeholders" }
        };

        var context = await HandleAsync(org, requestedUrn: "999999");

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task OrganisationWithSimilarName_IsNotTreatedAsInternalStaff()
    {
        var org = new Organisation
        {
            Id = "other-1",
            Name = "Department for Education Scotland",
            Category = new Category { Id = "003", Name = "Other Stakeholders" }
        };

        var context = await HandleAsync(org, requestedUrn: "999999");

        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task UnauthenticatedUser_DoesNothing()
    {
        var context = await HandleAsync(Establishment("123456"), requestedUrn: "123456", authenticated: false);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeFalse();
    }

    private static Organisation Establishment(string? urn) => new()
    {
        Id = "org-123",
        Name = "Test School",
        Urn = urn,
        Category = new Category { Id = "001", Name = "Establishment" }
    };

    private static async Task<AuthorizationHandlerContext> HandleAsync(
        Organisation? organisation,
        string? requestedUrn,
        (string Key, string Value)? extraRouteValue = null,
        bool authenticated = true)
    {
        var httpContext = new DefaultHttpContext();
        if (requestedUrn is not null)
        {
            httpContext.Request.RouteValues["urn"] = requestedUrn;
        }

        if (extraRouteValue is { } extra)
        {
            httpContext.Request.RouteValues[extra.Key] = extra.Value;
        }

        var principal = new ClaimsPrincipal(authenticated
            ? new ClaimsIdentity([new Claim("sub", "user-1")], "TestScheme")
            : new ClaimsIdentity());

        var handler = new DsiAuthorizationHandler(
            new StubUserService(organisation),
            new DepartmentForEducationInternalStaffChecker(),
            NullLogger<DsiAuthorizationHandler>.Instance);

        var context = new AuthorizationHandlerContext([Requirement], principal, httpContext);
        await handler.HandleAsync(context);
        return context;
    }

    private sealed class StubUserService(Organisation? organisation) : IUserService
    {
        public Task<User?> GetUserFromClaimsAsync(ClaimsPrincipal principal) =>
            Task.FromResult<User?>(new User { Sub = "user-1" });

        public Task<Organisation?> GetCurrentOrganisationAsync(ClaimsPrincipal principal) =>
            Task.FromResult(organisation);

        public Task<bool> SetCurrentOrganisationAsync(ClaimsPrincipal principal, string organisationId) =>
            Task.FromResult(true);

        public string? GetUserId(ClaimsPrincipal principal) => "user-1";
        public string? GetUserEmail(ClaimsPrincipal principal) => null;
        public bool IsAuthenticated(ClaimsPrincipal principal) => principal.Identity?.IsAuthenticated == true;
        public bool HasRole(ClaimsPrincipal principal, string role) => false;
    }
}
