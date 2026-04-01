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

public class GetSchoolKs4CoreSubjectsTests
{
    [Fact]
    public void ParseFilters_NormalizeSupportedValues()
    {
        SchoolKs4CoreSubjectSelection.ParseGradeFilter("4").Should().Be(SchoolKs4CoreSubjectGradeFilter.Grade4);
        SchoolKs4CoreSubjectSelection.ParseGradeFilter("5").Should().Be(SchoolKs4CoreSubjectGradeFilter.Grade5);
        SchoolKs4CoreSubjectSelection.ParseGradeFilter("7").Should().Be(SchoolKs4CoreSubjectGradeFilter.Grade7);
        SchoolKs4CoreSubjectSelection.ParseSubject("english-language").Should().Be(SchoolKs4CoreSubject.EnglishLanguage);
        SchoolKs4CoreSubjectSelection.ParseSubject("english-literature").Should().Be(SchoolKs4CoreSubject.EnglishLiterature);
        SchoolKs4CoreSubjectSelection.ParseSubject("combined-science-double-award").Should().Be(SchoolKs4CoreSubject.CombinedScienceDoubleAward);
    }

    [Fact]
    public async Task Execute_BuildsEnglishLanguageAndEnglishLiteratureComparisons()
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
            .ReturnsAsync(CreateMeasures("52", "51", "50", "42", "41", "40", "18", "17", "16", "54", "53", "52", "44", "43", "42", "20", "19", "18", "56", "55", "54", "46", "45", "44", "22", "21", "20", "48", "47", "46", "38", "37", "36", "14", "13", "12"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300"]);

        repositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                new Ks4HeadlineMeasuresByUrn("200", CreateMeasures("62", "61", "60", "52", "51", "50", "28", "27", "26", "64", "63", "62", "54", "53", "52", "30", "29", "28", "66", "65", "64", "56", "55", "54", "32", "31", "30", "58", "57", "56", "48", "47", "46", "24", "23", "22")),
                new Ks4HeadlineMeasuresByUrn("300", CreateMeasures("58", "57", "56", "48", "47", "46", "24", "23", "22", "60", "59", "58", "50", "49", "48", "26", "25", "24", "62", "61", "60", "52", "51", "50", "28", "27", "26", "54", "53", "52", "44", "43", "42", "20", "19", "18"))
            });

        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                new Establishment { URN = "200", EstablishmentName = "Alpha school" },
                new Establishment { URN = "300", EstablishmentName = "Beta school" }
            });

        var sut = new GetSchoolKs4CoreSubjects(
            repositoryMock.Object,
            schoolDetailsServiceMock.Object,
            establishmentRepositoryMock.Object,
            similarSchoolsRepositoryMock.Object);

        var result = await sut.Execute(new GetSchoolKs4CoreSubjectsRequest("100"));
        var englishLanguage = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.EnglishLanguage, SchoolKs4CoreSubjectGradeFilter.Grade4);
        var englishLiterature = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.EnglishLiterature, SchoolKs4CoreSubjectGradeFilter.Grade4);
        var maths = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.Maths, SchoolKs4CoreSubjectGradeFilter.Grade4);
        var combinedScience = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade4);

        result.SimilarSchoolsCount.Should().Be(2);
        englishLanguage.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(51m, 59m, 60m, 61m));
        englishLanguage.TopPerformers.Select(x => x.Name).Should().ContainInOrder("Alpha school", "Beta school");
        englishLiterature.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(53m, 61m, 62m, 63m));
        englishLiterature.YearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(62m, 61m, 60m));
        maths.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(55m, 63m, 64m, 65m));
        combinedScience.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(47m, 55m, 56m, 57m));
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
        string? lang4Current, string? lang4Previous, string? lang4Previous2,
        string? lang5Current, string? lang5Previous, string? lang5Previous2,
        string? lang7Current, string? lang7Previous, string? lang7Previous2,
        string? lit4Current, string? lit4Previous, string? lit4Previous2,
        string? lit5Current, string? lit5Previous, string? lit5Previous2,
        string? lit7Current, string? lit7Previous, string? lit7Previous2,
        string? maths4Current, string? maths4Previous, string? maths4Previous2,
        string? maths5Current, string? maths5Previous, string? maths5Previous2,
        string? maths7Current, string? maths7Previous, string? maths7Previous2,
        string? comb4Current, string? comb4Previous, string? comb4Previous2,
        string? comb5Current, string? comb5Previous, string? comb5Previous2,
        string? comb7Current, string? comb7Previous, string? comb7Previous2) =>
        new(
            new EstablishmentPerformance
            {
                EngLang49_Sum_Est_Current_Pct = lang4Current ?? string.Empty,
                EngLang49_Sum_Est_Previous_Pct = lang4Previous ?? string.Empty,
                EngLang49_Sum_Est_Previous2_Pct = lang4Previous2 ?? string.Empty,
                EngLang59_Sum_Est_Current_Pct = lang5Current ?? string.Empty,
                EngLang59_Sum_Est_Previous_Pct = lang5Previous ?? string.Empty,
                EngLang59_Sum_Est_Previous2_Pct = lang5Previous2 ?? string.Empty,
                EngLang79_Sum_Est_Current_Pct = lang7Current ?? string.Empty,
                EngLang79_Sum_Est_Previous_Pct = lang7Previous ?? string.Empty,
                EngLang79_Sum_Est_Previous2_Pct = lang7Previous2 ?? string.Empty,
                EngLit49_Sum_Est_Current_Pct = lit4Current ?? string.Empty,
                EngLit49_Sum_Est_Previous_Pct = lit4Previous ?? string.Empty,
                EngLit49_Sum_Est_Previous2_Pct = lit4Previous2 ?? string.Empty,
                EngLit59_Sum_Est_Current_Pct = lit5Current ?? string.Empty,
                EngLit59_Sum_Est_Previous_Pct = lit5Previous ?? string.Empty,
                EngLit59_Sum_Est_Previous2_Pct = lit5Previous2 ?? string.Empty,
                EngLit79_Sum_Est_Current_Pct = lit7Current ?? string.Empty,
                EngLit79_Sum_Est_Previous_Pct = lit7Previous ?? string.Empty,
                EngLit79_Sum_Est_Previous2_Pct = lit7Previous2 ?? string.Empty,
                Maths49_Sum_Est_Current_Pct = maths4Current ?? string.Empty,
                Maths49_Sum_Est_Previous_Pct = maths4Previous ?? string.Empty,
                Maths49_Sum_Est_Previous2_Pct = maths4Previous2 ?? string.Empty,
                Maths59_Sum_Est_Current_Pct = maths5Current ?? string.Empty,
                Maths59_Sum_Est_Previous_Pct = maths5Previous ?? string.Empty,
                Maths59_Sum_Est_Previous2_Pct = maths5Previous2 ?? string.Empty,
                Maths79_Sum_Est_Current_Pct = maths7Current ?? string.Empty,
                Maths79_Sum_Est_Previous_Pct = maths7Previous ?? string.Empty,
                Maths79_Sum_Est_Previous2_Pct = maths7Previous2 ?? string.Empty,
                CombSci49_Sum_Est_Current_Pct = comb4Current ?? string.Empty,
                CombSci49_Sum_Est_Previous_Pct = comb4Previous ?? string.Empty,
                CombSci49_Sum_Est_Previous2_Pct = comb4Previous2 ?? string.Empty,
                CombSci59_Sum_Est_Current_Pct = comb5Current ?? string.Empty,
                CombSci59_Sum_Est_Previous_Pct = comb5Previous ?? string.Empty,
                CombSci59_Sum_Est_Previous2_Pct = comb5Previous2 ?? string.Empty,
                CombSci79_Sum_Est_Current_Pct = comb7Current ?? string.Empty,
                CombSci79_Sum_Est_Previous_Pct = comb7Previous ?? string.Empty,
                CombSci79_Sum_Est_Previous2_Pct = comb7Previous2 ?? string.Empty
            },
            new LAPerformance
            {
                EngLang49_Tot_LA_Current_Pct = "61", EngLang49_Tot_LA_Previous_Pct = "60", EngLang49_Tot_LA_Previous2_Pct = "59",
                EngLang59_Tot_LA_Current_Pct = "51", EngLang59_Tot_LA_Previous_Pct = "50", EngLang59_Tot_LA_Previous2_Pct = "49",
                EngLang79_Tot_LA_Current_Pct = "27", EngLang79_Tot_LA_Previous_Pct = "26", EngLang79_Tot_LA_Previous2_Pct = "25",
                EngLit49_Tot_LA_Current_Pct = "63", EngLit49_Tot_LA_Previous_Pct = "62", EngLit49_Tot_LA_Previous2_Pct = "61",
                EngLit59_Tot_LA_Current_Pct = "53", EngLit59_Tot_LA_Previous_Pct = "52", EngLit59_Tot_LA_Previous2_Pct = "51",
                EngLit79_Tot_LA_Current_Pct = "29", EngLit79_Tot_LA_Previous_Pct = "28", EngLit79_Tot_LA_Previous2_Pct = "27",
                Maths49_Tot_LA_Current_Pct = "65", Maths49_Tot_LA_Previous_Pct = "64", Maths49_Tot_LA_Previous2_Pct = "63",
                Maths59_Tot_LA_Current_Pct = "55", Maths59_Tot_LA_Previous_Pct = "54", Maths59_Tot_LA_Previous2_Pct = "53",
                Maths79_Tot_LA_Current_Pct = "31", Maths79_Tot_LA_Previous_Pct = "30", Maths79_Tot_LA_Previous2_Pct = "29",
                CombSci49_Tot_LA_Current_Pct = "57", CombSci49_Tot_LA_Previous_Pct = "56", CombSci49_Tot_LA_Previous2_Pct = "55",
                CombSci59_Tot_LA_Current_Pct = "47", CombSci59_Tot_LA_Previous_Pct = "46", CombSci59_Tot_LA_Previous2_Pct = "45",
                CombSci79_Tot_LA_Current_Pct = "23", CombSci79_Tot_LA_Previous_Pct = "22", CombSci79_Tot_LA_Previous2_Pct = "21"
            },
            new EnglandPerformance
            {
                EngLang49_Tot_Eng_Current_Pct = "62", EngLang49_Tot_Eng_Previous_Pct = "61", EngLang49_Tot_Eng_Previous2_Pct = "60",
                EngLang59_Tot_Eng_Current_Pct = "52", EngLang59_Tot_Eng_Previous_Pct = "51", EngLang59_Tot_Eng_Previous2_Pct = "50",
                EngLang79_Tot_Eng_Current_Pct = "28", EngLang79_Tot_Eng_Previous_Pct = "27", EngLang79_Tot_Eng_Previous2_Pct = "26",
                EngLit49_Tot_Eng_Current_Pct = "64", EngLit49_Tot_Eng_Previous_Pct = "63", EngLit49_Tot_Eng_Previous2_Pct = "62",
                EngLit59_Tot_Eng_Current_Pct = "54", EngLit59_Tot_Eng_Previous_Pct = "53", EngLit59_Tot_Eng_Previous2_Pct = "52",
                EngLit79_Tot_Eng_Current_Pct = "30", EngLit79_Tot_Eng_Previous_Pct = "29", EngLit79_Tot_Eng_Previous2_Pct = "28",
                Maths49_Tot_Eng_Current_Pct = "66", Maths49_Tot_Eng_Previous_Pct = "65", Maths49_Tot_Eng_Previous2_Pct = "64",
                Maths59_Tot_Eng_Current_Pct = "56", Maths59_Tot_Eng_Previous_Pct = "55", Maths59_Tot_Eng_Previous2_Pct = "54",
                Maths79_Tot_Eng_Current_Pct = "32", Maths79_Tot_Eng_Previous_Pct = "31", Maths79_Tot_Eng_Previous2_Pct = "30",
                CombSci49_Tot_Eng_Current_Pct = "58", CombSci49_Tot_Eng_Previous_Pct = "57", CombSci49_Tot_Eng_Previous2_Pct = "56",
                CombSci59_Tot_Eng_Current_Pct = "48", CombSci59_Tot_Eng_Previous_Pct = "47", CombSci59_Tot_Eng_Previous2_Pct = "46",
                CombSci79_Tot_Eng_Current_Pct = "24", CombSci79_Tot_Eng_Previous_Pct = "23", CombSci79_Tot_Eng_Previous2_Pct = "22"
            },
            null,
            null,
            null);
}
