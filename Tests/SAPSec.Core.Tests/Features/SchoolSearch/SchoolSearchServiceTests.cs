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
    public async Task SearchByNumberAsync_WithNonPrimaryOrSecondarySchool_ReturnsNull()
    {
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456", PhaseOfEducationName = "Nursery" });

        var result = await _sut.SearchByNumberAsync("123456");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_FiltersOutNonPrimaryOrSecondarySchools()
    {
        _indexReaderMock
            .Setup(x => x.SearchAsync("test", It.IsAny<int>()))
            .ReturnsAsync([
                (100001, "Test Primary"),
                (100002, "Test Nursery"),
                (100003, "Test Secondary")
            ]);

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "100001", EstablishmentName = "Test Primary", PhaseOfEducationName = "Primary" },
                new Establishment { URN = "100002", EstablishmentName = "Test Nursery", PhaseOfEducationName = "Nursery" },
                new Establishment { URN = "100003", EstablishmentName = "Test Secondary", PhaseOfEducationName = "Secondary" }
            ]);

        var result = await _sut.SearchAsync("test");

        result.Select(x => x.URN).Should().BeEquivalentTo(["100001", "100003"]);
    }

    [Fact]
    public async Task SuggestAsync_FiltersOutNonPrimaryOrSecondarySchools()
    {
        _indexReaderMock
            .Setup(x => x.SearchAsync("test", It.IsAny<int>()))
            .ReturnsAsync([
                (100001, "Test Primary"),
                (100002, "Test Nursery"),
                (100003, "Test Secondary")
            ]);

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "100001", EstablishmentName = "Test Primary", PhaseOfEducationName = "Primary" },
                new Establishment { URN = "100002", EstablishmentName = "Test Nursery", PhaseOfEducationName = "Nursery" },
                new Establishment { URN = "100003", EstablishmentName = "Test Secondary", PhaseOfEducationName = "Secondary" }
            ]);

        var result = await _sut.SuggestAsync("test");

        result.Select(x => x.URN).Should().BeEquivalentTo(["100001", "100003"]);
    }
}
