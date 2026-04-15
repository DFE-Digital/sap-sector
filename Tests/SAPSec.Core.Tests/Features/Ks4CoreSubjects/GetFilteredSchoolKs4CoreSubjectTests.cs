using FluentAssertions;
using Moq;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.Ks4CoreSubjects;

public class GetFilteredSchoolKs4CoreSubjectTests
{
    [Fact]
    public async Task Execute_NormalizesUnknownValuesToEnglishLanguageGrade4()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(establishment: data =>
        {
            data.EngLang49_Sum_Est_Current_Pct = "52";
            data.EngLang49_Sum_Est_Previous_Pct = "51";
            data.EngLang49_Sum_Est_Previous2_Pct = "50";
        });

        var result = await context.Sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest("100", "unknown-subject", "99"));

        result.Subject.Should().Be(SchoolKs4CoreSubject.EnglishLanguage);
        result.Grade.Should().Be(SchoolKs4CoreSubjectGradeFilter.Grade4);
        result.Selection.ThreeYearAverage.SchoolValue.Should().Be(51m);
    }

    [Fact]
    public async Task Execute_SelectsRequestedSubjectAndGrade()
    {
        var context = new TestContext();
        context.SetupCurrentSchoolData(establishment: data =>
        {
            data.CombSci79_Sum_Est_Current_Pct = "78";
            data.CombSci79_Sum_Est_Previous_Pct = "77";
            data.CombSci79_Sum_Est_Previous2_Pct = "76";
        });

        var result = await context.Sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest("100", "combined-science-double-award", "7"));

        result.Subject.Should().Be(SchoolKs4CoreSubject.CombinedScienceDoubleAward);
        result.Grade.Should().Be(SchoolKs4CoreSubjectGradeFilter.Grade7);
        result.Selection.ThreeYearAverage.SchoolValue.Should().Be(77m);
        result.Subject.ToSubjectValue().Should().Be("combined-science-double-award");
        result.Grade.ToFilterValue().Should().Be("7");
    }

    private sealed class TestContext
    {
        private readonly Mock<IKs4PerformanceRepository> _repositoryMock = new();
        private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock = new();
        private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
        private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepositoryMock = new();

        public TestContext()
        {
            _schoolDetailsServiceMock
                .Setup(x => x.GetByUrnAsync("100"))
                .ReturnsAsync(CreateSchoolDetails("100", "Current school"));
            _similarSchoolsRepositoryMock
                .Setup(x => x.GetSimilarSchoolUrnsAsync("100"))
                .ReturnsAsync(Array.Empty<string>());
            _repositoryMock
                .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(Array.Empty<Ks4PerformanceData>());
            _establishmentRepositoryMock
                .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(Array.Empty<Establishment>());
        }

        public GetFilteredSchoolKs4CoreSubject Sut => new(new GetSchoolKs4CoreSubjects(
            _repositoryMock.Object,
            _schoolDetailsServiceMock.Object,
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object));

        public void SetupCurrentSchoolData(Action<EstablishmentPerformance>? establishment = null)
        {
            var establishmentPerformance = new EstablishmentPerformance();
            establishment?.Invoke(establishmentPerformance);

            _repositoryMock
                .Setup(x => x.GetByUrnAsync("100"))
                .ReturnsAsync(new Ks4PerformanceData("100", establishmentPerformance, new LAPerformance(), new EnglandPerformance()));
        }
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
}
