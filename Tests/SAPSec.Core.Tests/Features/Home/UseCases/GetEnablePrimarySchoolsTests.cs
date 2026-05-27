using FluentAssertions;
using Moq;
using SAPSec.Core.Features.Home;
using SAPSec.Core.Features.Home.UseCases;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Core.Tests.Features.Home.UseCases;

public class GetEnablePrimarySchoolsTests
{
    [Fact]
    public async Task Execute_ReturnsEnablePrimarySchoolsState()
    {
        var featureFlagService = new Mock<IFeatureFlagService>();
        featureFlagService
            .Setup(x => x.IsEnabledAsync(FeatureFlags.EnablePrimarySchools))
            .ReturnsAsync(true);

        var sut = new GetEnablePrimarySchools(featureFlagService.Object);

        var result = await sut.Execute();

        result.EnablePrimarySchools.Should().BeTrue();
    }
}
