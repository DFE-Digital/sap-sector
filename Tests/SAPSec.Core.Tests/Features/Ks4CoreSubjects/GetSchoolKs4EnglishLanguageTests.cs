using FluentAssertions;
using Moq;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.Ks4CoreSubjects;

public class GetSchoolKs4EnglishLanguageTests
{
    [Fact]
    public void ParseFilter_NormalizesSupportedValues()
    {
        SchoolKs4EnglishLanguageSelection.ParseFilter("4")
            .Should().Be(SchoolKs4EnglishLanguageGradeFilter.Grade4);
        SchoolKs4EnglishLanguageSelection.ParseFilter("5")
            .Should().Be(SchoolKs4EnglishLanguageGradeFilter.Grade5);
        SchoolKs4EnglishLanguageSelection.ParseFilter("7")
            .Should().Be(SchoolKs4EnglishLanguageGradeFilter.Grade7);
        SchoolKs4EnglishLanguageSelection.ParseFilter("unexpected")
            .Should().Be(SchoolKs4EnglishLanguageGradeFilter.Grade4);
    }

    [Fact]
    public async Task Execute_BuildsEnglishLanguageComparisonsAndTopPerformers()
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
            .ReturnsAsync(CreateMeasures("52", "51", "50", "42", "41", "40", "18", "17", "16"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300"]);

        repositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                new Ks4HeadlineMeasuresByUrn("200", CreateMeasures("62", "61", "60", "52", "51", "50", "28", "27", "26")),
                new Ks4HeadlineMeasuresByUrn("300", CreateMeasures("58", "57", "56", "48", "47", "46", "24", "23", "22"))
            });

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                new Establishment { URN = "200", EstablishmentName = "Alpha school" },
                new Establishment { URN = "300", EstablishmentName = "Beta school" }
            });

        var sut = new GetSchoolKs4EnglishLanguage(
            repositoryMock.Object,
            schoolDetailsServiceMock.Object,
            establishmentRepositoryMock.Object,
            similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetSchoolKs4EnglishLanguageRequest("100"));
        var grade4 = SchoolKs4EnglishLanguageSelection.From(result, SchoolKs4EnglishLanguageGradeFilter.Grade4);
        var grade5 = SchoolKs4EnglishLanguageSelection.From(result, SchoolKs4EnglishLanguageGradeFilter.Grade5);
        var grade7 = SchoolKs4EnglishLanguageSelection.From(result, SchoolKs4EnglishLanguageGradeFilter.Grade7);

        result.SimilarSchoolsCount.Should().Be(2);
        grade4.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(51m, 59m, 60m, 61m));
        grade4.YearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(60m, 59m, 58m));
        grade4.TopPerformers.Select(x => x.Name).Should().ContainInOrder("Alpha school", "Beta school");

        grade5.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(41m, 49m, 50m, 51m));
        grade7.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(17m, 25m, 26m, 27m));
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
        string? grade4Current,
        string? grade4Previous,
        string? grade4Previous2,
        string? grade5Current,
        string? grade5Previous,
        string? grade5Previous2,
        string? grade7Current,
        string? grade7Previous,
        string? grade7Previous2) =>
        new(
            new EstablishmentPerformance
            {
                EngLang49_Sum_Est_Current_Pct = grade4Current ?? string.Empty,
                EngLang49_Sum_Est_Previous_Pct = grade4Previous ?? string.Empty,
                EngLang49_Sum_Est_Previous2_Pct = grade4Previous2 ?? string.Empty,
                EngLang59_Sum_Est_Current_Pct = grade5Current ?? string.Empty,
                EngLang59_Sum_Est_Previous_Pct = grade5Previous ?? string.Empty,
                EngLang59_Sum_Est_Previous2_Pct = grade5Previous2 ?? string.Empty,
                EngLang79_Sum_Est_Current_Pct = grade7Current ?? string.Empty,
                EngLang79_Sum_Est_Previous_Pct = grade7Previous ?? string.Empty,
                EngLang79_Sum_Est_Previous2_Pct = grade7Previous2 ?? string.Empty
            },
            new LAPerformance
            {
                EngLang49_Tot_LA_Current_Pct = "61",
                EngLang49_Tot_LA_Previous_Pct = "60",
                EngLang49_Tot_LA_Previous2_Pct = "59",
                EngLang59_Tot_LA_Current_Pct = "51",
                EngLang59_Tot_LA_Previous_Pct = "50",
                EngLang59_Tot_LA_Previous2_Pct = "49",
                EngLang79_Tot_LA_Current_Pct = "27",
                EngLang79_Tot_LA_Previous_Pct = "26",
                EngLang79_Tot_LA_Previous2_Pct = "25"
            },
            new EnglandPerformance
            {
                EngLang49_Tot_Eng_Current_Pct = "62",
                EngLang49_Tot_Eng_Previous_Pct = "61",
                EngLang49_Tot_Eng_Previous2_Pct = "60",
                EngLang59_Tot_Eng_Current_Pct = "52",
                EngLang59_Tot_Eng_Previous_Pct = "51",
                EngLang59_Tot_Eng_Previous2_Pct = "50",
                EngLang79_Tot_Eng_Current_Pct = "28",
                EngLang79_Tot_Eng_Previous_Pct = "27",
                EngLang79_Tot_Eng_Previous2_Pct = "26"
            },
            null,
            null,
            null);
}
