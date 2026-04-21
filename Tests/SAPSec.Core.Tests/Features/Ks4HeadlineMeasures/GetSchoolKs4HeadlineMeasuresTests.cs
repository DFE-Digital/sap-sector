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
    public void ParseDestinationFilter_NormalizesSupportedValues()
    {
        SchoolKs4DestinationsSelection.ParseFilter("education")
            .Should().Be(SchoolKs4DestinationFilter.Education);
        SchoolKs4DestinationsSelection.ParseFilter("employment")
            .Should().Be(SchoolKs4DestinationFilter.Employment);
        SchoolKs4DestinationsSelection.ParseFilter("unexpected")
            .Should().Be(SchoolKs4DestinationFilter.All);
        SchoolKs4DestinationsSelection.ParseFilter(null)
            .Should().Be(SchoolKs4DestinationFilter.All);
    }

    [Fact]
    public void ParseGradeFilter_NormalizesSupportedValues()
    {
        SchoolKs4EngMathsSelection.ParseFilter("4")
            .Should().Be(SchoolKs4GradeFilter.Grade4);
        SchoolKs4EngMathsSelection.ParseFilter("5")
            .Should().Be(SchoolKs4GradeFilter.Grade5);
        SchoolKs4EngMathsSelection.ParseFilter("unexpected")
            .Should().Be(SchoolKs4GradeFilter.Grade4);
        SchoolKs4EngMathsSelection.ParseFilter(null)
            .Should().Be(SchoolKs4GradeFilter.Grade4);
    }

    [Fact]
    public void GetEngMaths_ReturnsSelectedGradeSlice()
    {
        var grade4Average = new SchoolKs4ComparisonAverage(1m, 2m, 3m, 4m);
        var grade5Average = new SchoolKs4ComparisonAverage(5m, 6m, 7m, 8m);
        var grade4TopPerformers = new[] { new Ks4TopPerformer(1, "100", "Grade 4 school", 1m) };
        var grade5TopPerformers = new[] { new Ks4TopPerformer(1, "200", "Grade 5 school", 2m) };
        var grade4YearByYear = new SchoolKs4ComparisonYearByYear(
            new Ks4HeadlineMeasureSeries(1m, 2m, 3m),
            new Ks4HeadlineMeasureSeries(4m, 5m, 6m),
            new Ks4HeadlineMeasureSeries(7m, 8m, 9m),
            new Ks4HeadlineMeasureSeries(10m, 11m, 12m));
        var grade5YearByYear = new SchoolKs4ComparisonYearByYear(
            new Ks4HeadlineMeasureSeries(13m, 14m, 15m),
            new Ks4HeadlineMeasureSeries(16m, 17m, 18m),
            new Ks4HeadlineMeasureSeries(19m, 20m, 21m),
            new Ks4HeadlineMeasureSeries(22m, 23m, 24m));

        var response = new GetSchoolKs4HeadlineMeasuresResponse(
            CreateSchoolDetails("100", "Current school"),
            3,
            new SchoolKs4ComparisonAverage(null, null, null, null),
            Array.Empty<Ks4TopPerformer>(),
            new SchoolKs4ComparisonYearByYear(new(null, null, null), new(null, null, null), new(null, null, null), new(null, null, null)),
            grade4Average,
            grade4TopPerformers,
            grade4YearByYear,
            grade5Average,
            grade5TopPerformers,
            grade5YearByYear,
            new SchoolKs4ComparisonAverage(null, null, null, null),
            Array.Empty<Ks4TopPerformer>(),
            new SchoolKs4ComparisonYearByYear(new(null, null, null), new(null, null, null), new(null, null, null), new(null, null, null)),
            new SchoolKs4ComparisonAverage(null, null, null, null),
            Array.Empty<Ks4TopPerformer>(),
            new SchoolKs4ComparisonYearByYear(new(null, null, null), new(null, null, null), new(null, null, null), new(null, null, null)),
            new SchoolKs4ComparisonAverage(null, null, null, null),
            Array.Empty<Ks4TopPerformer>(),
            new SchoolKs4ComparisonYearByYear(new(null, null, null), new(null, null, null), new(null, null, null), new(null, null, null)));

        SchoolKs4EngMathsSelection.From(response, SchoolKs4GradeFilter.Grade4).Should().BeEquivalentTo(
            new SchoolKs4EngMathsSelection(grade4Average, grade4TopPerformers, grade4YearByYear));
        SchoolKs4EngMathsSelection.From(response, SchoolKs4GradeFilter.Grade5).Should().BeEquivalentTo(
            new SchoolKs4EngMathsSelection(grade5Average, grade5TopPerformers, grade5YearByYear));
    }

    [Fact]
    public void GetDestinations_ReturnsSelectedDestinationSlice()
    {
        var allAverage = new SchoolKs4ComparisonAverage(1m, 2m, 3m, 4m);
        var educationAverage = new SchoolKs4ComparisonAverage(5m, 6m, 7m, 8m);
        var employmentAverage = new SchoolKs4ComparisonAverage(9m, 10m, 11m, 12m);
        var allTopPerformers = new[] { new Ks4TopPerformer(1, "100", "All school", 1m) };
        var educationTopPerformers = new[] { new Ks4TopPerformer(1, "200", "Education school", 2m) };
        var employmentTopPerformers = new[] { new Ks4TopPerformer(1, "300", "Employment school", 3m) };
        var allYearByYear = new SchoolKs4ComparisonYearByYear(
            new Ks4HeadlineMeasureSeries(1m, 2m, 3m),
            new Ks4HeadlineMeasureSeries(4m, 5m, 6m),
            new Ks4HeadlineMeasureSeries(7m, 8m, 9m),
            new Ks4HeadlineMeasureSeries(10m, 11m, 12m));
        var educationYearByYear = new SchoolKs4ComparisonYearByYear(
            new Ks4HeadlineMeasureSeries(13m, 14m, 15m),
            new Ks4HeadlineMeasureSeries(16m, 17m, 18m),
            new Ks4HeadlineMeasureSeries(19m, 20m, 21m),
            new Ks4HeadlineMeasureSeries(22m, 23m, 24m));
        var employmentYearByYear = new SchoolKs4ComparisonYearByYear(
            new Ks4HeadlineMeasureSeries(25m, 26m, 27m),
            new Ks4HeadlineMeasureSeries(28m, 29m, 30m),
            new Ks4HeadlineMeasureSeries(31m, 32m, 33m),
            new Ks4HeadlineMeasureSeries(34m, 35m, 36m));

        var response = new GetSchoolKs4HeadlineMeasuresResponse(
            CreateSchoolDetails("100", "Current school"),
            3,
            new SchoolKs4ComparisonAverage(null, null, null, null),
            Array.Empty<Ks4TopPerformer>(),
            new SchoolKs4ComparisonYearByYear(new(null, null, null), new(null, null, null), new(null, null, null), new(null, null, null)),
            new SchoolKs4ComparisonAverage(null, null, null, null),
            Array.Empty<Ks4TopPerformer>(),
            new SchoolKs4ComparisonYearByYear(new(null, null, null), new(null, null, null), new(null, null, null), new(null, null, null)),
            new SchoolKs4ComparisonAverage(null, null, null, null),
            Array.Empty<Ks4TopPerformer>(),
            new SchoolKs4ComparisonYearByYear(new(null, null, null), new(null, null, null), new(null, null, null), new(null, null, null)),
            allAverage,
            allTopPerformers,
            allYearByYear,
            educationAverage,
            educationTopPerformers,
            educationYearByYear,
            employmentAverage,
            employmentTopPerformers,
            employmentYearByYear);

        SchoolKs4DestinationsSelection.From(response, SchoolKs4DestinationFilter.All).Should().BeEquivalentTo(
            new SchoolKs4DestinationsSelection(allAverage, allTopPerformers, allYearByYear));
        SchoolKs4DestinationsSelection.From(response, SchoolKs4DestinationFilter.Education).Should().BeEquivalentTo(
            new SchoolKs4DestinationsSelection(educationAverage, educationTopPerformers, educationYearByYear));
        SchoolKs4DestinationsSelection.From(response, SchoolKs4DestinationFilter.Employment).Should().BeEquivalentTo(
            new SchoolKs4DestinationsSelection(employmentAverage, employmentTopPerformers, employmentYearByYear));
    }

    [Fact]
    public async Task Execute_UsesBatchRepositoryCallForSimilarSchoolsAndBuildsComparisonData()
    {
        var performanceRepositoryMock = new Mock<IKs4PerformanceRepository>();
        var destinationsRepositoryMock = new Mock<IKs4DestinationsRepository>();
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateSchoolDetails("100", "Current school"));

        performanceRepositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateMeasures("100", "45.0", "46.0", "47.0", "66.0", "67.0", "68.0"));

        destinationsRepositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateDestinations("100", "90", "91", "92"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300", "400"]);

        performanceRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.Is<IEnumerable<string>>(urns => urns.SequenceEqual(new[] { "200", "300", "400" }))))
            .ReturnsAsync(new[]
            {
                CreateMeasures("200", "40.0", "41.0", "42.0", "60.0", "61.0", "62.0"),
                CreateMeasures("300", "50.0", "51.0", "52.0", "70.0", "71.0", "72.0"),
                CreateMeasures("400", null, null, null, null, null, null)
            });

        destinationsRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.Is<IEnumerable<string>>(urns => urns.SequenceEqual(new[] { "200", "300", "400" }))))
            .ReturnsAsync(new[]
            {
                CreateDestinations("200", "84", "85", "86"),
                CreateDestinations("300", "94", "95", "96"),
                CreateDestinations("400", null, null, null)
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
            performanceRepositoryMock.Object,
            destinationsRepositoryMock.Object,
            schoolDetailsServiceMock.Object,
            establishmentRepositoryMock.Object,
            similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetSchoolKs4HeadlineMeasuresRequest("100"));

        result.SimilarSchoolsCount.Should().Be(3);
        result.Attainment8ThreeYearAverage.SimilarSchoolsValue.Should().Be(46.0m);
        result.EngMaths49ThreeYearAverage.SimilarSchoolsValue.Should().Be(66.0m);
        result.DestinationsThreeYearAverage.SimilarSchoolsValue.Should().Be(90.0m);
        result.Attainment8TopPerformers.Select(x => x.Name).Should().ContainInOrder("Beta school", "Alpha school");

        performanceRepositoryMock.Verify(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        destinationsRepositoryMock.Verify(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task Execute_IgnoresSimilarSchoolsWithoutEstablishmentDetails()
    {
        var performanceRepositoryMock = new Mock<IKs4PerformanceRepository>();
        var destinationsRepositoryMock = new Mock<IKs4DestinationsRepository>();
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateSchoolDetails("100", "Current school"));

        performanceRepositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateMeasures("100", "45.0", "46.0", "47.0", "66.0", "67.0", "68.0"));

        destinationsRepositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateDestinations("100", "90", "91", "92"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300"]);

        performanceRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                CreateMeasures("200", "40.0", "41.0", "42.0", "60.0", "61.0", "62.0"),
                CreateMeasures("300", "50.0", "51.0", "52.0", "70.0", "71.0", "72.0")
            });

        destinationsRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                CreateDestinations("200", "84", "85", "86"),
                CreateDestinations("300", "94", "95", "96")
            });

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                new Establishment { URN = "200", EstablishmentName = "Alpha school" }
            });

        var sut = new GetSchoolKs4HeadlineMeasures(
            performanceRepositoryMock.Object,
            destinationsRepositoryMock.Object,
            schoolDetailsServiceMock.Object,
            establishmentRepositoryMock.Object,
            similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetSchoolKs4HeadlineMeasuresRequest("100"));

        result.SimilarSchoolsCount.Should().Be(1);
        result.Attainment8TopPerformers.Select(x => (x.Rank, x.Name, x.IsCurrentSchool)).Should().ContainInOrder(
            (1, "Current school", true),
            (2, "Alpha school", false));
    }

    [Fact]
    public async Task Execute_WhenSimilarSchoolSourceDataContainsNullsNonNumericValuesAndMarkers_TreatsThemAsMissing()
    {
        var performanceRepositoryMock = new Mock<IKs4PerformanceRepository>();
        var destinationsRepositoryMock = new Mock<IKs4DestinationsRepository>();
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateSchoolDetails("100", "Current school"));

        performanceRepositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateMeasures("100", "45.0", "46.0", "47.0", "66.0", "67.0", "68.0"));

        destinationsRepositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateDestinations("100", "90", "91", "92"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300", "400"]);

        performanceRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                CreateMeasures("200", "40.0", null, "z", "60.0", "x", "c"),
                CreateMeasures("300", "n/a", "50.0", "60.0", "bad", "71.0", "72.0"),
                CreateMeasures("400", null, "x", "c", null, "z", "n/a")
            });

        destinationsRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                CreateDestinations("200", "84", "", "z"),
                CreateDestinations("300", "c", "95", "96"),
                CreateDestinations("400", null, "x", "c")
            });

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                new Establishment { URN = "200", EstablishmentName = "Alpha school" },
                new Establishment { URN = "300", EstablishmentName = "Beta school" },
                new Establishment { URN = "400", EstablishmentName = "Gamma school" }
            });

        var sut = new GetSchoolKs4HeadlineMeasures(
            performanceRepositoryMock.Object,
            destinationsRepositoryMock.Object,
            schoolDetailsServiceMock.Object,
            establishmentRepositoryMock.Object,
            similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetSchoolKs4HeadlineMeasuresRequest("100"));

        result.Attainment8ThreeYearAverage.SimilarSchoolsValue.Should().Be(47.5m);
        result.Attainment8YearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(40.0m, 50.0m, 60.0m));
        result.EngMaths49ThreeYearAverage.SimilarSchoolsValue.Should().Be(65.8m);
        result.EngMaths49YearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(60.0m, 71.0m, 72.0m));
        result.DestinationsThreeYearAverage.SimilarSchoolsValue.Should().Be(89.8m);
        result.DestinationsYearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(84m, 95m, 96m));
        result.Attainment8TopPerformers.Select(x => x.Name).Should().ContainInOrder("Beta school", "Alpha school");
    }

    [Fact]
    public async Task Execute_WhenAllSimilarSchoolSourceDataIsUnavailable_ReturnsNullComparisonValuesAndCurrentSchoolTopPerformers()
    {
        var performanceRepositoryMock = new Mock<IKs4PerformanceRepository>();
        var destinationsRepositoryMock = new Mock<IKs4DestinationsRepository>();
        var schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        var similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();

        schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateSchoolDetails("100", "Current school"));

        performanceRepositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateMeasures("100", "45.0", "46.0", "47.0", "66.0", "67.0", "68.0"));

        destinationsRepositoryMock
            .Setup(x => x.GetByUrnAsync("100"))
            .ReturnsAsync(CreateDestinations("100", "90", "91", "92"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300"]);

        performanceRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                CreateMeasures("200", null, "x", "c", null, "z", "n/a"),
                CreateMeasures("300", "", "s", "u", "", "bad", "t")
            });

        destinationsRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                CreateDestinations("200", null, "x", "c"),
                CreateDestinations("300", "", "w", "q")
            });

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                new Establishment { URN = "200", EstablishmentName = "Alpha school" },
                new Establishment { URN = "300", EstablishmentName = "Beta school" }
            });

        var sut = new GetSchoolKs4HeadlineMeasures(
            performanceRepositoryMock.Object,
            destinationsRepositoryMock.Object,
            schoolDetailsServiceMock.Object,
            establishmentRepositoryMock.Object,
            similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetSchoolKs4HeadlineMeasuresRequest("100"));

        result.Attainment8ThreeYearAverage.SimilarSchoolsValue.Should().BeNull();
        result.Attainment8YearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.EngMaths49ThreeYearAverage.SimilarSchoolsValue.Should().BeNull();
        result.EngMaths49YearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.DestinationsThreeYearAverage.SimilarSchoolsValue.Should().BeNull();
        result.DestinationsYearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(null, null, null));
        result.Attainment8TopPerformers.Should().ContainSingle(x => x.Name == "Current school" && x.IsCurrentSchool);
        result.EngMaths49TopPerformers.Should().ContainSingle(x => x.Name == "Current school" && x.IsCurrentSchool);
        result.DestinationsTopPerformers.Should().ContainSingle(x => x.Name == "Current school" && x.IsCurrentSchool);
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

    private static Ks4PerformanceData CreateMeasures(
        string urn,
        string? attainmentCurrent,
        string? attainmentPrevious,
        string? attainmentPrevious2,
        string? engMathsCurrent,
        string? engMathsPrevious,
        string? engMathsPrevious2) =>
        new(
            urn,
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
            null);

    private static Ks4DestinationsData CreateDestinations(
        string urn,
        string? destinationsCurrent,
        string? destinationsPrevious,
        string? destinationsPrevious2) =>
        new(
            urn,
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
