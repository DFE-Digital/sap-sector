using FluentAssertions;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Tests.Features.Ks4CoreSubjects;

public class GetFilteredSchoolKs4CoreSubjectTests
{
    [Fact]
    public void Execute_NormalizesUnknownValuesToEnglishLanguageGrade4()
    {
        var sut = new GetFilteredSchoolKs4CoreSubject();
        var response = CreateResponse();

        var result = sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest(response, "unknown-subject", "99"));

        result.Subject.Should().Be(SchoolKs4CoreSubject.EnglishLanguage);
        result.Grade.Should().Be(SchoolKs4CoreSubjectGradeFilter.Grade4);
        result.Selection.Should().BeSameAs(response.Grade4AndAbove.EnglishLanguage);
    }

    [Fact]
    public void Execute_SelectsRequestedSubjectAndGrade()
    {
        var sut = new GetFilteredSchoolKs4CoreSubject();
        var response = CreateResponse();

        var result = sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest(response, "combined-science-double-award", "7"));

        result.Subject.Should().Be(SchoolKs4CoreSubject.CombinedScienceDoubleAward);
        result.Grade.Should().Be(SchoolKs4CoreSubjectGradeFilter.Grade7);
        result.Selection.Should().BeSameAs(response.Grade7AndAbove.CombinedScienceDoubleAward);
        result.Subject.ToSubjectValue().Should().Be("combined-science-double-award");
        result.Grade.ToFilterValue().Should().Be("7");
    }

    private static GetSchoolKs4CoreSubjectsResponse CreateResponse()
    {
        var englishLanguage4 = CreateSelection(41m);
        var englishLanguage5 = CreateSelection(51m);
        var englishLanguage7 = CreateSelection(71m);
        var combinedScience4 = CreateSelection(44m);
        var combinedScience5 = CreateSelection(55m);
        var combinedScience7 = CreateSelection(77m);

        return new(
            CreateSchoolDetails(),
            2,
            new SchoolKs4CoreSubjectsGradeSelections(
                englishLanguage4,
                CreateSelection(42m),
                CreateSelection(43m),
                CreateSelection(44m),
                CreateSelection(45m),
                CreateSelection(46m),
                combinedScience4),
            new SchoolKs4CoreSubjectsGradeSelections(
                englishLanguage5,
                CreateSelection(52m),
                CreateSelection(53m),
                CreateSelection(54m),
                CreateSelection(55m),
                CreateSelection(56m),
                combinedScience5),
            new SchoolKs4CoreSubjectsGradeSelections(
                englishLanguage7,
                CreateSelection(72m),
                CreateSelection(73m),
                CreateSelection(74m),
                CreateSelection(75m),
                CreateSelection(76m),
                combinedScience7));
    }

    private static SchoolDetails CreateSchoolDetails() =>
        new()
        {
            Urn = "100",
            Name = "Test school",
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

    private static SchoolKs4CoreSubjectSelection CreateSelection(decimal schoolValue) =>
        new(
            new SchoolKs4ComparisonAverage(schoolValue, schoolValue + 1, schoolValue + 2, schoolValue + 3),
            Array.Empty<Ks4TopPerformer>(),
            new SchoolKs4ComparisonYearByYear(
                new Ks4HeadlineMeasureSeries(schoolValue, schoolValue, schoolValue),
                new Ks4HeadlineMeasureSeries(schoolValue, schoolValue, schoolValue),
                new Ks4HeadlineMeasureSeries(schoolValue, schoolValue, schoolValue),
                new Ks4HeadlineMeasureSeries(schoolValue, schoolValue, schoolValue)));
}
