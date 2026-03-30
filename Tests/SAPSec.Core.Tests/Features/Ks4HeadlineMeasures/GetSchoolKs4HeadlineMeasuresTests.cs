using FluentAssertions;
using Moq;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.Ks4HeadlineMeasures;

public class GetSchoolKs4HeadlineMeasuresTests
{
    [Fact]
    public async Task Execute_UsesBatchRepositoryCallForSimilarSchoolsAndBuildsComparisonData()
    {
        var repositoryMock = new Mock<IKs4PerformanceRepository>();
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateSchoolDetails("100", "Current school"));

        repositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateMeasures("45.0", "46.0", "47.0", "66.0", "67.0", "68.0", "90", "91", "92"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300", "400"]);

        repositoryMock
            .Setup(x => x.GetByUrnsAsync(It.Is<IEnumerable<string>>(urns => urns.SequenceEqual(new[] { "200", "300", "400" }))))
            .ReturnsAsync(new Dictionary<string, Ks4HeadlineMeasuresData?>
            {
                ["200"] = CreateMeasures("40.0", "41.0", "42.0", "60.0", "61.0", "62.0", "84", "85", "86"),
                ["300"] = CreateMeasures("50.0", "51.0", "52.0", "70.0", "71.0", "72.0", "94", "95", "96"),
                ["400"] = CreateMeasures(null, null, null, null, null, null, null, null, null)
            });

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.Is<IEnumerable<string>>(urns => urns.SequenceEqual(new[] { "200", "300", "400" }))))
            .ReturnsAsync(new[]
            {
                new Establishment { URN = "200", EstablishmentName = "Alpha school" },
                new Establishment { URN = "300", EstablishmentName = "Beta school" },
                new Establishment { URN = "400", EstablishmentName = "Gamma school" }
            });

        var sut = new GetSchoolKs4HeadlineMeasures(
            repositoryMock.Object,
            schoolDetailsServiceMock.Object,
            establishmentRepositoryMock.Object,
            similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetSchoolKs4HeadlineMeasuresRequest("100"));

        result.SimilarSchoolsCount.Should().Be(3);
        result.Attainment8ThreeYearAverage.SimilarSchoolsValue.Should().Be(46.0m);
        result.EngMaths49ThreeYearAverage.SimilarSchoolsValue.Should().Be(66.0m);
        result.DestinationsThreeYearAverage.SimilarSchoolsValue.Should().Be(90.0m);
        result.Attainment8TopPerformers.Select(x => x.Name).Should().ContainInOrder("Beta school", "Alpha school");

        repositoryMock.Verify(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task Execute_IgnoresSimilarSchoolsWithoutEstablishmentDetails()
    {
        var repositoryMock = new Mock<IKs4PerformanceRepository>();
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateSchoolDetails("100", "Current school"));

        repositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateMeasures("45.0", "46.0", "47.0", "66.0", "67.0", "68.0", "90", "91", "92"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300"]);

        repositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Ks4HeadlineMeasuresData?>
            {
                ["200"] = CreateMeasures("40.0", "41.0", "42.0", "60.0", "61.0", "62.0", "84", "85", "86"),
                ["300"] = CreateMeasures("50.0", "51.0", "52.0", "70.0", "71.0", "72.0", "94", "95", "96")
            });

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                new Establishment { URN = "200", EstablishmentName = "Alpha school" }
            });

        var sut = new GetSchoolKs4HeadlineMeasures(
            repositoryMock.Object,
            schoolDetailsServiceMock.Object,
            establishmentRepositoryMock.Object,
            similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetSchoolKs4HeadlineMeasuresRequest("100"));

        result.SimilarSchoolsCount.Should().Be(1);
        result.Attainment8TopPerformers.Should().ContainSingle();
        result.Attainment8TopPerformers[0].Name.Should().Be("Alpha school");
    }

    private static SchoolDetails CreateSchoolDetails(string urn, string name) =>
        new()
        {
            Urn = urn,
            Name = name,
            DfENumber = DataWithAvailability.Available("001/1234"),
            Ukprn = DataWithAvailability.Available("10000000"),
            Address = DataWithAvailability.Available("Test Address"),
            LocalAuthorityName = DataWithAvailability.Available("Test LA"),
            LocalAuthorityCode = DataWithAvailability.Available("001"),
            Region = DataWithAvailability.Available("Test Region"),
            UrbanRuralDescription = DataWithAvailability.Available("Urban"),
            AgeRangeLow = DataWithAvailability.Available(11),
            AgeRangeHigh = DataWithAvailability.Available(16),
            GenderOfEntry = DataWithAvailability.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability.Available("Secondary"),
            SchoolType = DataWithAvailability.Available("Academy"),
            AdmissionsPolicy = DataWithAvailability.Available("Not selective"),
            ReligiousCharacter = DataWithAvailability.Available("None"),
            GovernanceStructure = DataWithAvailability.Available(GovernanceType.MultiAcademyTrust),
            AcademyTrustName = DataWithAvailability.Available("Test Trust"),
            AcademyTrustId = DataWithAvailability.Available("5000"),
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(false),
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),
            HeadteacherName = DataWithAvailability.Available("Head Teacher"),
            Website = DataWithAvailability.Available("https://example.test"),
            Telephone = DataWithAvailability.Available("0123456789"),
            Email = DataWithAvailability.NotAvailable<string>()
        };

    private static Ks4HeadlineMeasuresData CreateMeasures(
        string? attainmentCurrent,
        string? attainmentPrevious,
        string? attainmentPrevious2,
        string? engMathsCurrent,
        string? engMathsPrevious,
        string? engMathsPrevious2,
        string? destinationsCurrent,
        string? destinationsPrevious,
        string? destinationsPrevious2) =>
        new(
            new EstablishmentPerformance
            {
                Attainment8_Tot_Est_Current_Num = attainmentCurrent ?? string.Empty,
                Attainment8_Tot_Est_Previous_Num = attainmentPrevious ?? string.Empty,
                Attainment8_Tot_Est_Previous2_Num = attainmentPrevious2 ?? string.Empty,
                EngMaths49_Tot_Est_Current_Pct = engMathsCurrent ?? string.Empty,
                EngMaths49_Tot_Est_Previous_Pct = engMathsPrevious ?? string.Empty,
                EngMaths49_Tot_Est_Previous2_Pct = engMathsPrevious2 ?? string.Empty,
                EngMaths59_Tot_Est_Current_Pct = engMathsCurrent ?? string.Empty,
                EngMaths59_Tot_Est_Previous_Pct = engMathsPrevious ?? string.Empty,
                EngMaths59_Tot_Est_Previous2_Pct = engMathsPrevious2 ?? string.Empty
            },
            null,
            null,
            new EstablishmentDestinations
            {
                AllDest_Tot_Est_Current_Pct = destinationsCurrent ?? string.Empty,
                AllDest_Tot_Est_Previous_Pct = destinationsPrevious ?? string.Empty,
                AllDest_Tot_Est_Previous2_Pct = destinationsPrevious2 ?? string.Empty,
                Education_Tot_Est_Current_Pct = destinationsCurrent ?? string.Empty,
                Education_Tot_Est_Previous_Pct = destinationsPrevious ?? string.Empty,
                Education_Tot_Est_Previous2_Pct = destinationsPrevious2 ?? string.Empty,
                Employment_Tot_Est_Current_Pct = destinationsCurrent ?? string.Empty,
                Employment_Tot_Est_Previous_Pct = destinationsPrevious ?? string.Empty,
                Employment_Tot_Est_Previous2_Pct = destinationsPrevious2 ?? string.Empty
            },
            null,
            null);
}
