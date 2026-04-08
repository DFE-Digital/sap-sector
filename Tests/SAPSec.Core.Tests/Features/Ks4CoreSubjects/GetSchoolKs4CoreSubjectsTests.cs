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
        SchoolKs4CoreSubjectSelection.ParseSubject("biology").Should().Be(SchoolKs4CoreSubject.Biology);
        SchoolKs4CoreSubjectSelection.ParseSubject("chemistry").Should().Be(SchoolKs4CoreSubject.Chemistry);
        SchoolKs4CoreSubjectSelection.ParseSubject("physics").Should().Be(SchoolKs4CoreSubject.Physics);
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
            .ReturnsAsync(CreateMeasures("100", "52", "51", "50", "42", "41", "40", "18", "17", "16", "54", "53", "52", "44", "43", "42", "20", "19", "18", "74", "73", "72", "64", "63", "62", "40", "39", "38", "56", "55", "54", "46", "45", "44", "22", "21", "20", "48", "47", "46", "38", "37", "36", "14", "13", "12", "56", "55", "54", "46", "45", "44", "22", "21", "20", "48", "47", "46", "38", "37", "36", "14", "13", "12"));

        similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
            .ReturnsAsync(["200", "300"]);

        repositoryMock
            .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[]
            {
                CreateMeasures("200", "62", "61", "60", "52", "51", "50", "28", "27", "26", "64", "63", "62", "54", "53", "52", "30", "29", "28", "84", "83", "82", "74", "73", "72", "50", "49", "48", "66", "65", "64", "56", "55", "54", "32", "31", "30", "58", "57", "56", "48", "47", "46", "24", "23", "22", "66", "65", "64", "56", "55", "54", "32", "31", "30", "58", "57", "56", "48", "47", "46", "24", "23", "22"),
                CreateMeasures("300", "58", "57", "56", "48", "47", "46", "24", "23", "22", "60", "59", "58", "50", "49", "48", "26", "25", "24", "80", "79", "78", "70", "69", "68", "46", "45", "44", "62", "61", "60", "52", "51", "50", "28", "27", "26", "54", "53", "52", "44", "43", "42", "20", "19", "18", "62", "61", "60", "52", "51", "50", "28", "27", "26", "54", "53", "52", "44", "43", "42", "20", "19", "18")
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
        var physics = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.Physics, SchoolKs4CoreSubjectGradeFilter.Grade4);
        var maths = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.Maths, SchoolKs4CoreSubjectGradeFilter.Grade4);
        var combinedScience = SchoolKs4CoreSubjectSelection.From(result, SchoolKs4CoreSubject.CombinedScienceDoubleAward, SchoolKs4CoreSubjectGradeFilter.Grade4);

        result.SimilarSchoolsCount.Should().Be(2);
        englishLanguage.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(51m, 59m, 60m, 61m));
        englishLanguage.TopPerformers.Select(x => x.Name).Should().ContainInOrder("Alpha school", "Beta school");
        englishLiterature.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(53m, 61m, 62m, 63m));
        englishLiterature.YearByYear.SimilarSchools.Should().Be(new Ks4HeadlineMeasureSeries(62m, 61m, 60m));
        physics.ThreeYearAverage.Should().BeEquivalentTo(new SchoolKs4ComparisonAverage(47m, 55m, 77m, 78m));
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

    private static Ks4PerformanceData CreateMeasures(
        string urn,
        string? lang4Current, string? lang4Previous, string? lang4Previous2,
        string? lang5Current, string? lang5Previous, string? lang5Previous2,
        string? lang7Current, string? lang7Previous, string? lang7Previous2,
        string? lit4Current, string? lit4Previous, string? lit4Previous2,
        string? lit5Current, string? lit5Previous, string? lit5Previous2,
        string? lit7Current, string? lit7Previous, string? lit7Previous2,
        string? bio4Current, string? bio4Previous, string? bio4Previous2,
        string? bio5Current, string? bio5Previous, string? bio5Previous2,
        string? bio7Current, string? bio7Previous, string? bio7Previous2,
        string? chem4Current, string? chem4Previous, string? chem4Previous2,
        string? chem5Current, string? chem5Previous, string? chem5Previous2,
        string? chem7Current, string? chem7Previous, string? chem7Previous2,
        string? physics4Current, string? physics4Previous, string? physics4Previous2,
        string? physics5Current, string? physics5Previous, string? physics5Previous2,
        string? physics7Current, string? physics7Previous, string? physics7Previous2,
        string? maths4Current, string? maths4Previous, string? maths4Previous2,
        string? maths5Current, string? maths5Previous, string? maths5Previous2,
        string? maths7Current, string? maths7Previous, string? maths7Previous2,
        string? comb4Current, string? comb4Previous, string? comb4Previous2,
        string? comb5Current, string? comb5Previous, string? comb5Previous2,
        string? comb7Current, string? comb7Previous, string? comb7Previous2) =>
        new(
            urn,
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
                Bio49_Sum_Est_Current_Pct = bio4Current ?? string.Empty,
                Bio49_Sum_Est_Previous_Pct = bio4Previous ?? string.Empty,
                Bio49_Sum_Est_Previous2_Pct = bio4Previous2 ?? string.Empty,
                Bio59_Sum_Est_Current_Pct = bio5Current ?? string.Empty,
                Bio59_Sum_Est_Previous_Pct = bio5Previous ?? string.Empty,
                Bio59_Sum_Est_Previous2_Pct = bio5Previous2 ?? string.Empty,
                Bio79_Sum_Est_Current_Pct = bio7Current ?? string.Empty,
                Bio79_Sum_Est_Previous_Pct = bio7Previous ?? string.Empty,
                Bio79_Sum_Est_Previous2_Pct = bio7Previous2 ?? string.Empty,
                Chem49_Sum_Est_Current_Pct = chem4Current ?? string.Empty,
                Chem49_Sum_Est_Previous_Pct = chem4Previous ?? string.Empty,
                Chem49_Sum_Est_Previous2_Pct = chem4Previous2 ?? string.Empty,
                Chem59_Sum_Est_Current_Pct = chem5Current ?? string.Empty,
                Chem59_Sum_Est_Previous_Pct = chem5Previous ?? string.Empty,
                Chem59_Sum_Est_Previous2_Pct = chem5Previous2 ?? string.Empty,
                Chem79_Sum_Est_Current_Pct = chem7Current ?? string.Empty,
                Chem79_Sum_Est_Previous_Pct = chem7Previous ?? string.Empty,
                Chem79_Sum_Est_Previous2_Pct = chem7Previous2 ?? string.Empty,
                Physics49_Sum_Est_Current_Pct = physics4Current ?? string.Empty,
                Physics49_Sum_Est_Previous_Pct = physics4Previous ?? string.Empty,
                Physics49_Sum_Est_Previous2_Pct = physics4Previous2 ?? string.Empty,
                Physics59_Sum_Est_Current_Pct = physics5Current ?? string.Empty,
                Physics59_Sum_Est_Previous_Pct = physics5Previous ?? string.Empty,
                Physics59_Sum_Est_Previous2_Pct = physics5Previous2 ?? string.Empty,
                Physics79_Sum_Est_Current_Pct = physics7Current ?? string.Empty,
                Physics79_Sum_Est_Previous_Pct = physics7Previous ?? string.Empty,
                Physics79_Sum_Est_Previous2_Pct = physics7Previous2 ?? string.Empty,
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
                Bio49_Tot_LA_Current_Pct = "83", Bio49_Tot_LA_Previous_Pct = "82", Bio49_Tot_LA_Previous2_Pct = "81",
                Bio59_Tot_LA_Current_Pct = "73", Bio59_Tot_LA_Previous_Pct = "72", Bio59_Tot_LA_Previous2_Pct = "71",
                Bio79_Tot_LA_Current_Pct = "49", Bio79_Tot_LA_Previous_Pct = "48", Bio79_Tot_LA_Previous2_Pct = "47",
                Chem49_Tot_LA_Current_Pct = "79", Chem49_Tot_LA_Previous_Pct = "78", Chem49_Tot_LA_Previous2_Pct = "77",
                Chem59_Tot_LA_Current_Pct = "69", Chem59_Tot_LA_Previous_Pct = "68", Chem59_Tot_LA_Previous2_Pct = "67",
                Chem79_Tot_LA_Current_Pct = "45", Chem79_Tot_LA_Previous_Pct = "44", Chem79_Tot_LA_Previous2_Pct = "43",
                Physics49_Tot_LA_Current_Pct = "78", Physics49_Tot_LA_Previous_Pct = "77", Physics49_Tot_LA_Previous2_Pct = "76",
                Physics59_Tot_LA_Current_Pct = "68", Physics59_Tot_LA_Previous_Pct = "67", Physics59_Tot_LA_Previous2_Pct = "66",
                Physics79_Tot_LA_Current_Pct = "44", Physics79_Tot_LA_Previous_Pct = "43", Physics79_Tot_LA_Previous2_Pct = "42",
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
                Bio49_Tot_Eng_Current_Pct = "84", Bio49_Tot_Eng_Previous_Pct = "83", Bio49_Tot_Eng_Previous2_Pct = "82",
                Bio59_Tot_Eng_Current_Pct = "74", Bio59_Tot_Eng_Previous_Pct = "73", Bio59_Tot_Eng_Previous2_Pct = "72",
                Bio79_Tot_Eng_Current_Pct = "50", Bio79_Tot_Eng_Previous_Pct = "49", Bio79_Tot_Eng_Previous2_Pct = "48",
                Chem49_Tot_Eng_Current_Pct = "80", Chem49_Tot_Eng_Previous_Pct = "79", Chem49_Tot_Eng_Previous2_Pct = "78",
                Chem59_Tot_Eng_Current_Pct = "70", Chem59_Tot_Eng_Previous_Pct = "69", Chem59_Tot_Eng_Previous2_Pct = "68",
                Chem79_Tot_Eng_Current_Pct = "46", Chem79_Tot_Eng_Previous_Pct = "45", Chem79_Tot_Eng_Previous2_Pct = "44",
                Physics49_Tot_Eng_Current_Pct = "79", Physics49_Tot_Eng_Previous_Pct = "78", Physics49_Tot_Eng_Previous2_Pct = "77",
                Physics59_Tot_Eng_Current_Pct = "69", Physics59_Tot_Eng_Previous_Pct = "68", Physics59_Tot_Eng_Previous2_Pct = "67",
                Physics79_Tot_Eng_Current_Pct = "45", Physics79_Tot_Eng_Previous_Pct = "44", Physics79_Tot_Eng_Previous2_Pct = "43",
                Maths49_Tot_Eng_Current_Pct = "66", Maths49_Tot_Eng_Previous_Pct = "65", Maths49_Tot_Eng_Previous2_Pct = "64",
                Maths59_Tot_Eng_Current_Pct = "56", Maths59_Tot_Eng_Previous_Pct = "55", Maths59_Tot_Eng_Previous2_Pct = "54",
                Maths79_Tot_Eng_Current_Pct = "32", Maths79_Tot_Eng_Previous_Pct = "31", Maths79_Tot_Eng_Previous2_Pct = "30",
                CombSci49_Tot_Eng_Current_Pct = "58", CombSci49_Tot_Eng_Previous_Pct = "57", CombSci49_Tot_Eng_Previous2_Pct = "56",
                CombSci59_Tot_Eng_Current_Pct = "48", CombSci59_Tot_Eng_Previous_Pct = "47", CombSci59_Tot_Eng_Previous2_Pct = "46",
                CombSci79_Tot_Eng_Current_Pct = "24", CombSci79_Tot_Eng_Previous_Pct = "23", CombSci79_Tot_Eng_Previous2_Pct = "22"
            });
}
