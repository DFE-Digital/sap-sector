using Moq;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.SchoolSearch;

public class SchoolSearchServiceTests
{
    private readonly Mock<ISchoolSearchIndexReader> _indexReaderMock = new();
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
    private readonly SchoolSearchService _sut;

    public SchoolSearchServiceTests()
    {
        _sut = new SchoolSearchService(_indexReaderMock.Object, _establishmentRepositoryMock.Object);
    }

    [Theory]
    [InlineData("123456", "123456")]
    [InlineData("10000001", "10000001")]
    [InlineData("123/456", "123456")]
    [InlineData(@"123\456", "123456")]
    [InlineData(" 123456 ", "123456")]
    public async Task SearchByNumberAsync_WithSupportedNumberFormat_SearchesRepository(string input, string expectedNumber)
    {
        var establishment = new Establishment { URN = "123456", PhaseOfEducationName = "Secondary" };
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync(expectedNumber))
            .ReturnsAsync(establishment);

        var result = await _sut.SearchByNumberAsync(input);

        result.Should().Be(establishment);
        _establishmentRepositoryMock.Verify(x => x.GetEstablishmentByAnyNumberAsync(expectedNumber), Times.Once);
    }

    [Fact]
    public async Task SearchByNumberAsync_WithSchoolName_DoesNotSearchRepository()
    {
        var result = await _sut.SearchByNumberAsync("Test school");

        result.Should().BeNull();
        _establishmentRepositoryMock.Verify(x => x.GetEstablishmentByAnyNumberAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchByNumberAsync_WithPrimarySchoolAndFeatureDisabled_ReturnsNull()
    {
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456", PhaseOfEducationName = "Primary" });

        var result = await _sut.SearchByNumberAsync("123456");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchByNumberAsync_WithPrimarySchoolAndFeatureEnabled_ReturnsSchool()
    {
        var establishment = new Establishment { URN = "123456", PhaseOfEducationName = "Primary" };
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("123456"))
            .ReturnsAsync(establishment);

        var result = await _sut.SearchByNumberAsync("123456", true);

        result.Should().Be(establishment);
    }

    [Fact]
    public async Task SearchAsync_FiltersOutPrimarySchools_WhenFeatureDisabled()
    {
        _indexReaderMock
            .Setup(x => x.SearchAsync("school", It.IsAny<int>()))
            .ReturnsAsync([(1, "Primary School"), (2, "Secondary School")]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "1", EstablishmentName = "Primary School", PhaseOfEducationName = "Primary" },
                new Establishment { URN = "2", EstablishmentName = "Secondary School", PhaseOfEducationName = "Secondary" }
            ]);

        var results = await _sut.SearchAsync("school");

        results.Should().ContainSingle();
        results[0].URN.Should().Be("2");
    }

    [Fact]
    public async Task SearchAsync_IncludesAllThroughSchools_WhenFeatureEnabled()
    {
        _indexReaderMock
            .Setup(x => x.SearchAsync("school", It.IsAny<int>()))
            .ReturnsAsync([(1, "All-through School"), (2, "Secondary School")]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "1", EstablishmentName = "All-through School", PhaseOfEducationName = "All-through" },
                new Establishment { URN = "2", EstablishmentName = "Secondary School", PhaseOfEducationName = "Secondary" }
            ]);

        var results = await _sut.SearchAsync("school", true);

        results.Should().HaveCount(2);
        results.Select(x => x.URN).Should().Contain(["1", "2"]);
    }
}
