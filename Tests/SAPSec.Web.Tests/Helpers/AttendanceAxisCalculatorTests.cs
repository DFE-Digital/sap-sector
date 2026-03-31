using FluentAssertions;
using SAPSec.Web.Helpers;

namespace SAPSec.Web.Tests.Helpers;

public class AttendanceAxisCalculatorTests
{
    [Fact]
    public void CalculateMax_OverallAbsenceWithinDefaultRange_ReturnsDefaultMax()
    {
        var result = AttendanceAxisCalculator.CalculateMax([5.1m, 4.8m, 4.7m], AttendanceAxisCalculator.OverallAbsence);

        result.Should().Be(10m);
    }

    [Fact]
    public void CalculateMax_OverallAbsenceAboveDefaultRange_AddsHeadroomAndRoundsToStep()
    {
        var result = AttendanceAxisCalculator.CalculateMax([12.35m, 9.2m, 8.4m], AttendanceAxisCalculator.OverallAbsence);

        result.Should().Be(14m);
    }

    [Fact]
    public void CalculateMax_PersistentAbsenceAboveDefaultRange_AddsHeadroomAndRoundsToStep()
    {
        var result = AttendanceAxisCalculator.CalculateMax([31m, 29m, 27m], AttendanceAxisCalculator.PersistentAbsence);

        result.Should().Be(35m);
    }

    [Fact]
    public void ForAbsenceType_ReturnsPersistentSettings_WhenRequested()
    {
        var result = AttendanceAxisCalculator.ForAbsenceType(isPersistentAbsence: true);

        result.Should().Be(AttendanceAxisCalculator.PersistentAbsence);
    }
}
