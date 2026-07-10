using Moq;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.KS2.Performance;
using SAPSec.Data.Repositories;
using SAPSec.Infrastructure.Json;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class JsonPrimaryKs2RepositoryTests
{
    private readonly Mock<IEstablishmentRepository> _establishmentRepository = new();
    private readonly Mock<IJsonFileFactory> _jsonFileFactory = new();
    private readonly Mock<IJsonFile<EstablishmentPerformance>> _establishmentPerformanceFile = new();
    private readonly Mock<IJsonFile<EstablishmentSubjectEntries>> _establishmentSubjectEntriesFile = new();
    private readonly Mock<IJsonFile<LAPerformance>> _localAuthorityPerformanceFile = new();
    private readonly Mock<IJsonFile<LASubjectEntries>> _localAuthoritySubjectEntriesFile = new();
    private readonly Mock<IJsonFile<EnglandPerformance>> _englandPerformanceFile = new();

    private readonly JsonPrimaryKs2Repository _sut;

    public JsonPrimaryKs2RepositoryTests()
    {
        _jsonFileFactory
            .Setup(x => x.Create<EstablishmentPerformance>(JsonDataSource.PrimarySchools))
            .Returns(_establishmentPerformanceFile.Object);
        _jsonFileFactory
            .Setup(x => x.Create<EstablishmentSubjectEntries>(JsonDataSource.PrimarySchools))
            .Returns(_establishmentSubjectEntriesFile.Object);
        _jsonFileFactory
            .Setup(x => x.Create<LAPerformance>(JsonDataSource.PrimarySchools))
            .Returns(_localAuthorityPerformanceFile.Object);
        _jsonFileFactory
            .Setup(x => x.Create<LASubjectEntries>(JsonDataSource.PrimarySchools))
            .Returns(_localAuthoritySubjectEntriesFile.Object);
        _jsonFileFactory
            .Setup(x => x.Create<EnglandPerformance>(JsonDataSource.PrimarySchools))
            .Returns(_englandPerformanceFile.Object);

        _sut = new JsonPrimaryKs2Repository(_establishmentRepository.Object, _jsonFileFactory.Object);
    }

    [Fact]
    public async Task GetByUrnAsync_JoinsPrimaryKs2DataAcrossDifferentKeyFormats()
    {
        _establishmentRepository
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "100590", LAId = "925" }
            ]);
        _establishmentPerformanceFile
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync([
                new EstablishmentPerformance { Id = "100590", RwmExpected_Tot_Cohort_Est_Current_Num = "68.26" }
            ]);
        _establishmentSubjectEntriesFile
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync([
                new EstablishmentSubjectEntries { school_urn = "100590", subject = "Reading", grade = "EXS" }
            ]);
        _localAuthorityPerformanceFile
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync([
                new LAPerformance { Id = "925", RwmExpected_Tot_Cohort_LA_Current_Num = "65.83" }
            ]);
        _localAuthoritySubjectEntriesFile
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync([
                new LASubjectEntries { new_la_code = "E00000925", subject = "Reading", grade = "EXS" }
            ]);
        _englandPerformanceFile
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync([
                new EnglandPerformance { Id = "Local authority" },
                new EnglandPerformance { Id = "National", RwmExpected_Tot_Cohort_Eng_Current_Num = "74.00" }
            ]);

        var result = await _sut.GetByUrnAsync("100590");

        Assert.NotNull(result);
        Assert.Equal("100590", result.URN);
        Assert.Equal("68.26", result.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Current_Num);
        Assert.Single(result.EstablishmentSubjectEntries);
        Assert.Equal("925", result.LocalAuthorityPerformance?.Id);
        Assert.Single(result.LocalAuthoritySubjectEntries);
        Assert.Equal("National", result.EnglandPerformance?.Id);
    }

    [Fact]
    public async Task GetByUrnAsync_ReturnsEmptyCollections_WhenSubjectEntryMatchesAreMissing()
    {
        _establishmentRepository
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([
                new Establishment { URN = "100590", LAId = "925" }
            ]);
        _establishmentPerformanceFile
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync([
                new EstablishmentPerformance { Id = "100590" }
            ]);
        _establishmentSubjectEntriesFile.Setup(x => x.ReadAllAsync()).ReturnsAsync([]);
        _localAuthorityPerformanceFile
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync([
                new LAPerformance { Id = "925" }
            ]);
        _localAuthoritySubjectEntriesFile.Setup(x => x.ReadAllAsync()).ReturnsAsync([]);
        _englandPerformanceFile
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync([
                new EnglandPerformance { Id = "National" }
            ]);

        var result = await _sut.GetByUrnAsync("100590");

        Assert.NotNull(result);
        Assert.Empty(result.EstablishmentSubjectEntries);
        Assert.Empty(result.LocalAuthoritySubjectEntries);
    }
}
