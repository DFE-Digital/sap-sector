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
        var establishment = new Establishment { URN = "123456" };
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
}
