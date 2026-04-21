using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;
using SAPSec.Core.Services;

namespace SAPSec.Core.Tests.Features.Ks4CoreSubjects;

public class GetFilteredSchoolKs4CoreSubjectTests
{
    [Fact]
    public async Task Execute_WithUnsupportedSubject_ThrowsArgumentOutOfRangeException()
    {
        var context = new TestContext();

        var act = () => context.Sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest("100001", "unknown-subject", "4"));

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("subject");
    }

    [Fact]
    public async Task Execute_WithUnsupportedGrade_ThrowsArgumentOutOfRangeException()
    {
        var context = new TestContext();

        var act = () => context.Sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest("100001", "english-language", "99"));

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("grade");
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

        var result = await context.Sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest("100001", "combined-science-double-award", "7"));

        result.Subject.Should().Be(SchoolKs4CoreSubject.CombinedScienceDoubleAward);
        result.Grade.Should().Be(SchoolKs4CoreSubjectGradeFilter.Grade7);
        result.Selection.ThreeYearAverage.SchoolValue.Should().Be(77m);
        result.Subject.ToSubjectValue().Should().Be("combined-science-double-award");
        result.Grade.ToFilterValue().Should().Be("7");
    }

    private sealed class TestContext
    {
        private readonly Mock<IKs4PerformanceRepository> _repositoryMock = new();
        private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
        private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepositoryMock = new();

        public TestContext()
        {
            _similarSchoolsRepositoryMock
                .Setup(x => x.GetSimilarSchoolsGroupAsync("100001"))
                .ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryGroupsEntry>());
            var school = CreateSchool("100001", "Current school");
            _repositoryMock
                .Setup(x => x.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(Array.Empty<Ks4PerformanceData>());
            _establishmentRepositoryMock
                .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new[] { school });
            _establishmentRepositoryMock
                .Setup(x => x.GetEstablishmentAsync("100001"))
                .ReturnsAsync(school);
        }

        public GetFilteredSchoolKs4CoreSubject Sut => new(
            _repositoryMock.Object,
            new SchoolDetailsService(_establishmentRepositoryMock.Object, new Mock<ILogger<SchoolDetailsService>>().Object),
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object);

        public void SetupCurrentSchoolData(Action<EstablishmentPerformance>? establishment = null)
        {
            var establishmentPerformance = new EstablishmentPerformance();
            establishment?.Invoke(establishmentPerformance);

            _repositoryMock
                .Setup(x => x.GetByUrnAsync("100001"))
                .ReturnsAsync(new Ks4PerformanceData("100001", establishmentPerformance, new LAPerformance(), new EnglandPerformance()));
        }
    }

    private static Establishment CreateSchool(string urn, string name, BNGCoordinates? coordinates = null)
    {
        return new Establishment
        {
            URN = urn,
            EstablishmentName = name,
            Street = "123 Test Street",
            Town = "Sheffield",
            Postcode = "S1 1AA",
            Locality = "",
            Address3 = "",
            TotalCapacity = 1200,
            TotalPupils = 1000,
            NurseryProvisionName = "No",
            LAId = "373",
            LAName = "Sheffield",
            RegionId = "R",
            RegionName = "Yorkshire and the Humber",
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban",
            PhaseOfEducationId = "P",
            PhaseOfEducationName = "Secondary",
            OfficialSixthFormId = "1",
            OfficialSixthFormName = "Has sixth form",
            AdmissionsPolicyId = "1",
            AdmissionsPolicyName = "Comprehensive",
            GenderId = "M",
            GenderName = "Mixed",
            ResourcedProvisionId = "0",
            ResourcedProvisionName = "No",
            TypeOfEstablishmentId = "27",
            TypeOfEstablishmentName = "Academy",
            EstablishmentTypeGroupId = "10",
            EstablishmentTypeGroupName = "Academies",
            TrustSchoolFlagId = "0",
            TrustSchoolFlagName = "No",
            Easting = coordinates?.Easting,
            Northing = coordinates?.Northing,
        };
    }
}
