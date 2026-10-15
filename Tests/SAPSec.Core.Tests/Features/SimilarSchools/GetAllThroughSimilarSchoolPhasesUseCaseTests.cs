using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools;

public class GetAllThroughSimilarSchoolPhasesUseCaseTests
{
    private readonly InMemorySimilarSchoolsPrimaryRepository _primaryRepository = new();
    private readonly InMemorySimilarSchoolsSecondaryRepository _secondaryRepository = new();

    private GetAllThroughSimilarSchoolPhasesUseCase CreateSut() =>
        new(_primaryRepository, _secondaryRepository);

    [Fact]
    public async Task Execute_WhenPrimaryAndSecondaryGroupsExist_ReturnsBothPhases()
    {
        _primaryRepository.SetupGroups(Build.PrimaryGroup("100001", ["100002"]));
        _secondaryRepository.SetupGroups(Build.SecondaryGroup("100001", ["200002"]));

        var response = await CreateSut().Execute(new("100001"));

        response.HasPrimary.Should().BeTrue();
        response.HasSecondary.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_WhenOnlyPrimaryGroupExists_ReturnsPrimaryOnly()
    {
        _primaryRepository.SetupGroups(Build.PrimaryGroup("100001", ["100002"]));

        var response = await CreateSut().Execute(new("100001"));

        response.HasPrimary.Should().BeTrue();
        response.HasSecondary.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WhenOnlySecondaryGroupExists_ReturnsSecondaryOnly()
    {
        _secondaryRepository.SetupGroups(Build.SecondaryGroup("100001", ["200002"]));

        var response = await CreateSut().Execute(new("100001"));

        response.HasPrimary.Should().BeFalse();
        response.HasSecondary.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_WhenNoGroupsExist_ReturnsNoPhases()
    {
        var response = await CreateSut().Execute(new("100001"));

        response.HasPrimary.Should().BeFalse();
        response.HasSecondary.Should().BeFalse();
    }
}
