using FluentAssertions;
using Moq;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Tests.Features.Ks4CoreSubjects;

public class GetFilteredSchoolKs4CoreSubjectTests
{
    [Fact]
    public async Task Execute_WithUnsupportedSubject_ThrowsArgumentOutOfRangeException()
    {
        var context = new TestContext();

        var act = () => context.Sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest("100", "unknown-subject", "4"));

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("subject");
    }

    [Fact]
    public async Task Execute_WithUnsupportedGrade_ThrowsArgumentOutOfRangeException()
    {
        var context = new TestContext();

        var act = () => context.Sut.Execute(new GetFilteredSchoolKs4CoreSubjectRequest("100", "english-language", "99"));

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
        private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
        private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepositoryMock = new();

        public TestContext()
        {
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

        public GetFilteredSchoolKs4CoreSubject Sut => new(
            _repositoryMock.Object,
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object);

        public void SetupCurrentSchoolData(Action<EstablishmentPerformance>? establishment = null)
        {
            var establishmentPerformance = new EstablishmentPerformance();
            establishment?.Invoke(establishmentPerformance);

            _repositoryMock
                .Setup(x => x.GetByUrnAsync("100"))
                .ReturnsAsync(new Ks4PerformanceData("100", establishmentPerformance, new LAPerformance(), new EnglandPerformance()));
        }
    }
}
