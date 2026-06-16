using Moq;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SchoolSearch.UseCases;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.SchoolSearch;

public class SchoolSearchServiceTests
{
    private const string EnablePrimarySchoolsFeature = "EnablePrimarySchools";
    private readonly Mock<ISchoolSearchIndexReader> _indexReaderMock = new();
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock = new();
    private readonly SchoolSearchService _sut;

    public SchoolSearchServiceTests()
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(EnablePrimarySchoolsFeature))
            .ReturnsAsync(false);

        _sut = new SchoolSearchService(
            _indexReaderMock.Object,
            _establishmentRepositoryMock.Object,
            _featureFlagServiceMock.Object,
            new DetermineSchoolSearchEligibility());
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
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailAsync("123456"))
            .ReturnsAsync(new EstablishmentEmail { URN = "123456", EstablishmentStatusId = "1" });

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
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailAsync("123456"))
            .ReturnsAsync(new EstablishmentEmail { URN = "123456", EstablishmentStatusId = "1" });

        var result = await _sut.SearchByNumberAsync("123456");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchByNumberAsync_WithNonPrimaryOrSecondarySchool_ReturnsNull()
    {
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456", PhaseOfEducationName = "Nursery" });
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailAsync("123456"))
            .ReturnsAsync(new EstablishmentEmail { URN = "123456", EstablishmentStatusId = "1" });

        var result = await _sut.SearchByNumberAsync("123456");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchByNumberAsync_WithPrimarySchoolAndFeatureEnabled_ReturnsSchool()
    {
        var establishment = new Establishment { URN = "123456", PhaseOfEducationName = "Primary" };
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(EnablePrimarySchoolsFeature))
            .ReturnsAsync(true);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("123456"))
            .ReturnsAsync(establishment);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailAsync("123456"))
            .ReturnsAsync(new EstablishmentEmail { URN = "123456", EstablishmentStatusId = "1" });

        var result = await _sut.SearchByNumberAsync("123456");

        result.Should().Be(establishment);
    }

    [Fact]
    public async Task SearchByNumberAsync_WithSecondarySchoolAndMissingEmail_ReturnsSchool()
    {
        var establishment = new Establishment { URN = "123456", PhaseOfEducationId = "4", PhaseOfEducationName = "Secondary" };
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("123456"))
            .ReturnsAsync(establishment);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailAsync("123456"))
            .ReturnsAsync((EstablishmentEmail?)null);

        var result = await _sut.SearchByNumberAsync("123456");

        result.Should().Be(establishment);
    }

    [Theory]
    [InlineData("2")]
    [InlineData("4")]
    public async Task SearchByNumberAsync_WithExcludedStatus_ReturnsNull(string statusId)
    {
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentByAnyNumberAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456", PhaseOfEducationName = "Secondary" });
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailAsync("123456"))
            .ReturnsAsync(new EstablishmentEmail { URN = "123456", EstablishmentStatusId = statusId });

        var result = await _sut.SearchByNumberAsync("123456");

        result.Should().BeNull();
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
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new EstablishmentEmail { URN = "1", EstablishmentStatusId = "1" },
                new EstablishmentEmail { URN = "2", EstablishmentStatusId = "1" }
            ]);

        var results = await _sut.SearchAsync("school");

        results.Should().ContainSingle();
        results[0].URN.Should().Be("2");
    }

    [Fact]
    public async Task SearchAsync_IncludesAllThroughSchools_WhenFeatureEnabled()
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(EnablePrimarySchoolsFeature))
            .ReturnsAsync(true);
        _indexReaderMock
            .Setup(x => x.SearchAsync("school", It.IsAny<int>()))
            .ReturnsAsync([(1, "All-through School"), (2, "Secondary School")]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "1", EstablishmentName = "All-through School", PhaseOfEducationName = "All-through" },
                new Establishment { URN = "2", EstablishmentName = "Secondary School", PhaseOfEducationName = "Secondary" }
            ]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new EstablishmentEmail { URN = "1", EstablishmentStatusId = "1" },
                new EstablishmentEmail { URN = "2", EstablishmentStatusId = "1" }
            ]);

        var results = await _sut.SearchAsync("school");

        results.Should().HaveCount(2);
        results.Select(x => x.URN).Should().Contain(["1", "2"]);
    }

    [Fact]
    public async Task SuggestAsync_FiltersOutPrimarySchools_WhenFeatureDisabled()
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
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new EstablishmentEmail { URN = "1", EstablishmentStatusId = "1" },
                new EstablishmentEmail { URN = "2", EstablishmentStatusId = "1" }
            ]);

        var results = await _sut.SuggestAsync("school");

        results.Should().ContainSingle();
        results[0].URN.Should().Be("2");
    }

    [Fact]
    public async Task SuggestAsync_ExcludesSchools_WithExcludedStatus()
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(EnablePrimarySchoolsFeature))
            .ReturnsAsync(true);
        _indexReaderMock
            .Setup(x => x.SearchAsync("school", It.IsAny<int>()))
            .ReturnsAsync([(1, "Primary School")]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "1", EstablishmentName = "Primary School", PhaseOfEducationId = "2", PhaseOfEducationName = "Primary" }
            ]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new EstablishmentEmail { URN = "1", EstablishmentStatusId = "2" }
            ]);

        var results = await _sut.SuggestAsync("school");

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_IncludesSecondarySchools_WhenStatusRecordMissing()
    {
        _indexReaderMock
            .Setup(x => x.SearchAsync("school", It.IsAny<int>()))
            .ReturnsAsync([(2, "Secondary School")]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "2", EstablishmentName = "Secondary School", PhaseOfEducationId = "4", PhaseOfEducationName = "Secondary" }
            ]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);

        var results = await _sut.SearchAsync("school");

        results.Should().ContainSingle();
        results[0].URN.Should().Be("2");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("3")]
    public async Task SearchAsync_IncludesSchools_WithIncludedStatusIds(string statusId)
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(EnablePrimarySchoolsFeature))
            .ReturnsAsync(true);
        _indexReaderMock
            .Setup(x => x.SearchAsync("school", It.IsAny<int>()))
            .ReturnsAsync([(1, "Primary School")]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "1", EstablishmentName = "Primary School", PhaseOfEducationName = "Primary", PhaseOfEducationId = "2" }
            ]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new EstablishmentEmail { URN = "1", EstablishmentStatusId = statusId }
            ]);

        var results = await _sut.SearchAsync("school");

        results.Should().ContainSingle();
    }

    [Theory]
    [InlineData("2")]
    [InlineData("4")]
    public async Task SearchAsync_ExcludesSchools_WithExcludedStatusIds(string statusId)
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(EnablePrimarySchoolsFeature))
            .ReturnsAsync(true);
        _indexReaderMock
            .Setup(x => x.SearchAsync("school", It.IsAny<int>()))
            .ReturnsAsync([(1, "Primary School")]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "1", EstablishmentName = "Primary School", PhaseOfEducationName = "Primary", PhaseOfEducationId = "2" }
            ]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new EstablishmentEmail { URN = "1", EstablishmentStatusId = statusId }
            ]);

        var results = await _sut.SearchAsync("school");

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData("0", "Not applicable")]
    [InlineData("1", "Nursery")]
    [InlineData("3", "Middle deemed primary")]
    [InlineData("5", "Middle deemed secondary")]
    [InlineData("6", "16 plus")]
    public async Task SearchAsync_ExcludesSchools_WithUnsupportedPhaseIds(string phaseId, string phaseName)
    {
        _featureFlagServiceMock
            .Setup(x => x.IsEnabledAsync(EnablePrimarySchoolsFeature))
            .ReturnsAsync(true);
        _indexReaderMock
            .Setup(x => x.SearchAsync("school", It.IsAny<int>()))
            .ReturnsAsync([(1, "Excluded School")]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "1", EstablishmentName = "Excluded School", PhaseOfEducationName = phaseName, PhaseOfEducationId = phaseId }
            ]);
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentEmailsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new EstablishmentEmail { URN = "1", EstablishmentStatusId = "1" }
            ]);

        var results = await _sut.SearchAsync("school");

        results.Should().BeEmpty();
    }
}
