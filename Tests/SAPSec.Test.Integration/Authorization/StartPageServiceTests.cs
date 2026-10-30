using FluentAssertions;
using SAPSec.Core.FeatureFlags;
using SAPSec.Web.Constants;
using SAPSec.Web.Services;

namespace SAPSec.Test.Integration.Authorization;

public class StartPageServiceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenFindASchoolIsEnabled_StartsAtFindASchool_WhetherSignedInOrNot(bool signedIn)
    {
        var service = new StartPageService(new FixedFeatureFlagService(enabled: true));

        var url = await service.GetStartUrlAsync(signedIn);

        url.Should().Be(Routes.FindASchool());
    }

    [Fact]
    public async Task WhenFindASchoolIsRestricted_SignedInUsersStartAtUserHome()
    {
        var service = new StartPageService(new FixedFeatureFlagService(enabled: false));

        var url = await service.GetStartUrlAsync(signedIn: true);

        url.Should().Be(Routes.UserHome);
    }

    [Fact]
    public async Task WhenFindASchoolIsRestricted_UsersWhoAreNotSignedInStartAtHome()
    {
        var service = new StartPageService(new FixedFeatureFlagService(enabled: false));

        var url = await service.GetStartUrlAsync(signedIn: false);

        url.Should().Be(Routes.Home);
    }

    private sealed class FixedFeatureFlagService(bool enabled) : IFeatureFlagService
    {
        public Task<bool> IsEnabledAsync(string featureName) => Task.FromResult(enabled);
    }
}
