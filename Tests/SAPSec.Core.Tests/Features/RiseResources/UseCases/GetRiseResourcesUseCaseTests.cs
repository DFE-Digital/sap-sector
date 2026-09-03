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
            Entry("Primary only", "Curriculum", PhaseOfEducationValues.Primary),
            Entry("Secondary only", "Curriculum", PhaseOfEducationValues.Secondary),
            Entry("All phases", "Curriculum", PhaseOfEducationValues.Primary, PhaseOfEducationValues.Secondary, "All through"));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        Titles(result).Should().BeEquivalentTo("Secondary only", "All phases");
    }

    [Fact]
    public async Task Execute_ForPrimarySchool_ReturnsOnlyResourcesTaggedForPrimary()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.Primary()));
        _riseResourcesRepo.SetupResources(
            Entry("Primary only", "Curriculum", PhaseOfEducationValues.Primary),
            Entry("Secondary only", "Curriculum", PhaseOfEducationValues.Secondary));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        Titles(result).Should().BeEquivalentTo("Primary only");
    }

    [Fact]
    public async Task Execute_ForAllThroughSchool_ReturnsResourcesTaggedPrimaryOrSecondaryOrAllThrough()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.AllThrough()));
        _riseResourcesRepo.SetupResources(
            Entry("Primary only", "Curriculum", PhaseOfEducationValues.Primary),
            Entry("Secondary only", "Curriculum", PhaseOfEducationValues.Secondary),
            Entry("All through only", "Curriculum", "All through"),
            Entry("Untagged", "Curriculum", "16 plus"));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        Titles(result).Should().BeEquivalentTo("Primary only", "Secondary only", "All through only");
    }

    [Fact]
    public async Task Execute_OrdersCategoriesByResourceCategoriesConfiguration_AndAttachesDescriptions()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.Secondary()));
        _riseResourcesRepo.SetupCategories(
            Category("Wider school", "About the wider school."),
            Category("Performance and attendance", "About performance."));
        _riseResourcesRepo.SetupResources(
            // File order lists a "Performance and attendance" resource first, but config order wins.
            Entry("Attendance guidance", "Performance and attendance", PhaseOfEducationValues.Secondary),
            Entry("Leadership guidance", "Wider school", PhaseOfEducationValues.Secondary),
            Entry("Pastoral guidance", "Pupil characteristics", PhaseOfEducationValues.Secondary));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        // Listed categories in resourceCategories order, then the unlisted one.
        result.Categories.Select(c => c.Name)
            .Should().Equal("Wider school", "Performance and attendance", "Pupil characteristics");
        result.Categories.Single(c => c.Name == "Wider school").Description.Should().Be("About the wider school.");
        result.Categories.Single(c => c.Name == "Pupil characteristics").Description.Should().BeNull();
    }

    [Fact]
    public async Task Execute_MapsAllResourceFields()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.Secondary()));
        _riseResourcesRepo.SetupResources(new Data.Dto.RiseResources.RiseResourceEntry
        {
            ResourceTitle = "Improving attendance",
            ResourceDescription = "Guidance for schools",
            ResourceUrl = "https://example.gov.uk/attendance",
            SchoolPhases = [PhaseOfEducationValues.Secondary],
            Category = "Performance and attendance",
            SubCategory = "Attendance",
            MappingMeasures = "Overall absence rate; Persistent absence rate"
        });

        var resource = Resources(await _sut.Execute(new GetRiseResourcesRequest("123456"))).Single();

        resource.Title.Should().Be("Improving attendance");
        resource.Description.Should().Be("Guidance for schools");
        resource.Url.Should().Be("https://example.gov.uk/attendance");
        resource.SubCategory.Should().Be("Attendance");
        resource.MappingMeasures.Should().Be("Overall absence rate; Persistent absence rate");
    }

    private static IEnumerable<Core.Features.RiseResources.RiseResource> Resources(GetRiseResourcesResponse response) =>
        response.Categories.SelectMany(category => category.Resources);

    private static IEnumerable<string> Titles(GetRiseResourcesResponse response) =>
        Resources(response).Select(resource => resource.Title);

    private static Data.Dto.RiseResources.RiseResourceEntry Entry(string title, string category, params string[] phases) =>
        new()
        {
            ResourceTitle = title,
            Category = category,
            SchoolPhases = phases
        };

    private static RiseResourceCategoryEntry Category(string name, string description) =>
        new() { Category = name, CategoryDescription = description };
}
