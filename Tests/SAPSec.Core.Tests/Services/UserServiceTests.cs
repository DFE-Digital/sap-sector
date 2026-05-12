using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Services;

namespace SAPSec.Core.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetUserFromClaimsAsync_WhenOrganisationClaimsMissing_FetchesOrganisationsFromDsi()
    {
        var organisationsFromDsi = new List<Organisation>
        {
            new() { Id = "org-1", Name = "Test organisation" }
        };

        var dsiClient = new Mock<IDsiClient>();
        dsiClient.Setup(x => x.GetUserAsync("user-123"))
            .ReturnsAsync(new UserInfo
            {
                Sub = "user-123",
                Organisations = organisationsFromDsi
            });

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var logger = new Mock<ILogger<UserService>>();

        var service = new UserService(
            dsiClient.Object,
            httpContextAccessor.Object,
            logger.Object);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim(ClaimTypes.Name, "Test User")
        ], "TestAuth"));

        var result = await service.GetUserFromClaimsAsync(principal);

        result.Should().NotBeNull();
        result!.Sub.Should().Be("user-123");
        result.Email.Should().Be("user@example.com");
        result.Organisations.Should().BeEquivalentTo(organisationsFromDsi);
        dsiClient.Verify(x => x.GetUserAsync("user-123"), Times.Once);
    }

    [Fact]
    public async Task GetUserFromClaimsAsync_WhenOrganisationClaimsExist_DoesNotFetchOrganisationsFromDsi()
    {
        var dsiClient = new Mock<IDsiClient>();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var logger = new Mock<ILogger<UserService>>();

        var service = new UserService(
            dsiClient.Object,
            httpContextAccessor.Object,
            logger.Object);

        var organisationClaimJson = """
            [
              {
                "id": "org-1",
                "name": "Claim organisation"
              }
            ]
            """;

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim("organisation", organisationClaimJson)
        ], "TestAuth"));

        var result = await service.GetUserFromClaimsAsync(principal);

        result.Should().NotBeNull();
        result!.Organisations.Should().ContainSingle();
        result.Organisations[0].Id.Should().Be("org-1");
        dsiClient.Verify(x => x.GetUserAsync(It.IsAny<string>()), Times.Never);
    }
}
