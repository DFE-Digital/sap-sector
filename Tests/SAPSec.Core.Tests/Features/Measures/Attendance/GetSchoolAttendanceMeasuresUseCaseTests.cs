using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.Measures.Attendance;

public partial class GetSchoolAttendanceMeasuresUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryAbsenceRepository _absenceRepo;
    private readonly GetSchoolAttendanceMeasuresUseCase _sut;

    public GetSchoolAttendanceMeasuresUseCaseTests()
    {
        _establishmentRepo = new();
        _absenceRepo = new(_establishmentRepo);
        _sut = new GetSchoolAttendanceMeasuresUseCase(
            _establishmentRepo,
            _absenceRepo);
    }

    [Theory]
    [InlineData(MeasurePhase.Primary)]
    [InlineData(MeasurePhase.Secondary)]
    public async Task WhenCurrentSchoolDoesNotExist_ThrowsNotFoundException(MeasurePhase phase)
    {
        var act = async () => await _sut.Execute(Request(phase, "999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Theory]
    [InlineData(MeasurePhase.Primary)]
    [InlineData(MeasurePhase.Secondary)]
    public async Task School_ShouldContainCurrentSchoolInfo(MeasurePhase phase)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x
                .Primary()
                .WithAddress("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG")
                .InLA("001", "Test LA")));

        var response = await _sut.Execute(Request(phase, "100001"));

        response.School.Urn.Should().Be("100001");
        response.School.Name.Should().Be("Test School");
        response.School.Address.Should().Be(
            new Address("1 Test Street", "Testingbury", "Test Place", "Test Town", "TE57 1NG"));
        response.School.LocalAuthority.Should().Be(
            new LocalAuthority("001", "Test LA"));
    }

    [Theory]
    [InlineData(MeasurePhase.Primary)]
    [InlineData(MeasurePhase.Secondary)]
    public async Task Absence_ShouldContainExpectedMeasureSeries(MeasurePhase phase)
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x.Primary()));

        var response = await _sut.Execute(Request(phase, "100001"));

        response.School.Name.Should().Be("Test School");
        var seriesTypes = response.Absence.Series.Select(s => s.SeriesType);

        seriesTypes.Should().BeEquivalentTo([
            MeasureSeriesType.CurrentSchool,
                MeasureSeriesType.LASchoolsAverage,
                MeasureSeriesType.EnglandSchoolsAverage
        ]);
    }

    private GetSchoolAttendanceMeasuresRequest Request(
        MeasurePhase phase,
        string urn,
        Dictionary<string, string>? filterBy = null) =>
            new(phase, urn, filterBy ?? []);
}