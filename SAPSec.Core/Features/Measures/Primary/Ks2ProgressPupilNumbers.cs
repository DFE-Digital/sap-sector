namespace SAPSec.Core.Features.Measures.Primary;

/// <summary>
/// Values for the latest year and the two years before it.
/// </summary>
public record PupilNumbersYears<T>(T? Previous2, T? Previous, T? Current) where T : struct;

/// <summary>
/// The 'Number of pupils' figures for one school, for the selected subject.
/// <see cref="ProportionIncluded"/> is null when it is not published for the pupil characteristic.
/// </summary>
public record SchoolPupilNumbers(
    string Urn,
    PupilNumbersYears<int> EligiblePupils,
    PupilNumbersYears<decimal>? ProportionIncluded);
