using SAPSec.Core.Constants;
using SAPSec.Core.Features.RiseResources;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.RiseResources.UseCases;

public class GetRiseResourcesUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryRiseResourcesRepository _riseResourcesRepo;
    private readonly GetRiseResourcesUseCase _sut;

    public GetRiseResourcesUseCaseTests()
    {
        _establishmentRepo = new();
        _riseResourcesRepo = new();
        _sut = new GetRiseResourcesUseCase(_establishmentRepo, _riseResourcesRepo);
    }

    [Fact]
    public async Task Execute_WhenSchoolMissing_ThrowsNotFoundException()
    {
        var act = async () => await _sut.Execute(new GetRiseResourcesRequest("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task Execute_WhenSchoolExists_ReturnsSchoolInfo()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.Secondary()));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        result.Urn.Should().Be("123456");
        result.SchoolName.Should().Be("Test School");
    }

    [Fact]
    public async Task Execute_ForSecondarySchool_ReturnsOnlyResourcesTaggedForSecondary()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.Secondary()));
        _riseResourcesRepo.SetupResources(
            Entry("Primary only", PhaseOfEducationValues.Primary),
            Entry("Secondary only", PhaseOfEducationValues.Secondary),
            Entry("All phases", PhaseOfEducationValues.Primary, PhaseOfEducationValues.Secondary, "All through"));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        result.Resources.Select(r => r.Title)
            .Should().BeEquivalentTo("Secondary only", "All phases");
    }

    [Fact]
    public async Task Execute_ForPrimarySchool_ReturnsOnlyResourcesTaggedForPrimary()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.Primary()));
        _riseResourcesRepo.SetupResources(
            Entry("Primary only", PhaseOfEducationValues.Primary),
            Entry("Secondary only", PhaseOfEducationValues.Secondary));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        result.Resources.Select(r => r.Title).Should().BeEquivalentTo("Primary only");
    }

    [Fact]
    public async Task Execute_ForAllThroughSchool_ReturnsResourcesTaggedPrimaryOrSecondaryOrAllThrough()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.AllThrough()));
        _riseResourcesRepo.SetupResources(
            Entry("Primary only", PhaseOfEducationValues.Primary),
            Entry("Secondary only", PhaseOfEducationValues.Secondary),
            Entry("All through only", "All through"),
            Entry("Untagged", "16 plus"));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        result.Resources.Select(r => r.Title)
            .Should().BeEquivalentTo("Primary only", "Secondary only", "All through only");
    }

    [Fact]
    public async Task Execute_MapsAllResourceFields()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.Secondary()));
        _riseResourcesRepo.SetupResources(new RiseResourceEntry
        {
            ResourceTitle = "Improving attendance",
            ResourceDescription = "Guidance for schools",
            ResourceUrl = "https://example.gov.uk/attendance",
            SchoolPhases = [PhaseOfEducationValues.Secondary],
            Category = "Attendance",
            SubCategory = "Whole-school approaches",
            MappingMeasures = ["Overall absence rate; Persistent absence rate"]
        });

        var resource = (await _sut.Execute(new GetRiseResourcesRequest("123456"))).Resources.Single();

        resource.Title.Should().Be("Improving attendance");
        resource.Description.Should().Be("Guidance for schools");
        resource.Url.Should().Be("https://example.gov.uk/attendance");
        resource.Category.Should().Be("Attendance");
        resource.SubCategory.Should().Be("Whole-school approaches");
        resource.MappingMeasures.Should().BeEquivalentTo("Overall absence rate; Persistent absence rate");
    }

    private static RiseResourceEntry Entry(string title, params string[] phases) =>
        new()
        {
            ResourceTitle = title,
            SchoolPhases = phases
        };
}
