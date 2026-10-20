using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Authentication;
using SAPSec.Web.Authorization;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Deprecated.Authorization;

public class TrustOrLocalAuthorityUserHandlerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<ILogger<TrustOrLocalAuthorityUserHandler>> _logger = new();
    private readonly TrustOrLocalAuthorityUserRequirement _requirement = new();
    private readonly ClaimsPrincipal _principal = new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, "user-123")],
        "TestAuth"));

    public TrustOrLocalAuthorityUserHandlerTests()
    {
        _userService.Setup(x => x.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust)]
    [InlineData(OrganisationCategoryIds.SingleAcademyTrust)]
    [InlineData(OrganisationCategoryIds.LocalAuthority)]
    public async Task HandleAsync_WhenOrganisationIsTrustOrLocalAuthority_Succeeds(string categoryId)
    {
        SetupCurrentOrganisation(categoryId);

        var context = await HandleAsync();

        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.Establishment)]
    [InlineData("011")]
    [InlineData("")]
    public async Task HandleAsync_WhenOrganisationIsNotTrustOrLocalAuthority_DoesNotSucceed(string categoryId)
    {
        SetupCurrentOrganisation(categoryId);

        var context = await HandleAsync();

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenOrganisationHasNoCategory_DoesNotSucceed()
    {
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new Organisation { Id = "org-1" });

        var context = await HandleAsync();

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentOrganisationIsNull_DoesNotSucceed()
    {
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((Organisation?)null);

        var context = await HandleAsync();

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_DoesNotSucceed()
    {
        _userService.Setup(x => x.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        var context = await HandleAsync();

        context.HasSucceeded.Should().BeFalse();
        _userService.Verify(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);
    }

    private void SetupCurrentOrganisation(string categoryId)
    {
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new Organisation
            {
                Id = "org-1",
                Category = new Category { Id = categoryId }
            });
    }

    private async Task<AuthorizationHandlerContext> HandleAsync()
    {
        var context = new AuthorizationHandlerContext([_requirement], _principal, null);
        var handler = new TrustOrLocalAuthorityUserHandler(_userService.Object, _logger.Object);

        await handler.HandleAsync(context);

        return context;
    }
}
