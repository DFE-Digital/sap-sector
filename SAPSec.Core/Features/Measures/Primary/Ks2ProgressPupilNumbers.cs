namespace SAPSec.Core.Features.Measures.Primary;

/// <summary>
/// The KS2 progress measure subjects. Eligible pupils are the same across all three;
/// the proportion of pupils included differs by subject.
/// </summary>
public enum Ks2ProgressSubject
{
    Reading,
    Writing,
    Maths
}

/// <summary>
/// Values for the latest year and the two years before it.
/// </summary>
public record PupilNumbersYears<T>(T? Previous2, T? Previous, T? Current) where T : struct;

/// <summary>
/// The 'Number of pupils' figures for one school, for the selected subject and pupil characteristic.
/// <see cref="ProportionIncluded"/> is null when it is not published for the pupil characteristic.
/// </summary>
public record SchoolPupilNumbers(
    string Urn,
    PupilNumbersYears<int> EligiblePupils,
    PupilNumbersYears<decimal>? ProportionIncluded);

public interface IKs2ProgressPupilNumbersProvider
{
    /// <summary>
    /// The pupil characteristic code that has published 'proportion of pupils included' figures.
    /// EES only publish the proportion for all pupils.
    /// </summary>
    const string AllPupils = "tot";

    Task<IReadOnlyDictionary<string, SchoolPupilNumbers>> GetAsync(
        IEnumerable<string> urns,
        Ks2ProgressSubject subject,
        string pupilCharacteristic);
}

/// <summary>
/// Placeholder figures used until the KS2 progress measures are switched on in the data map.
/// Values are stable for a URN so pages render the same on every request.
/// Only used behind <see cref="FeatureFlags.Flags.EnableKs2ProgressNumberOfPupils"/>.
/// </summary>
public class PlaceholderKs2ProgressPupilNumbersProvider : IKs2ProgressPupilNumbersProvider
{
    private static readonly IReadOnlyDictionary<string, decimal> CharacteristicShare =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            [IKs2ProgressPupilNumbersProvider.AllPupils] = 1m,
            ["boy"] = 0.52m,
            ["grl"] = 0.48m,
            ["dis"] = 0.27m,
            ["ndi"] = 0.73m,
            ["eal"] = 0.18m,
            ["nmo"] = 0.9m,
            ["hpa"] = 0.3m,
            ["mpa"] = 0.45m,
            ["lpa"] = 0.25m
        };

    public Task<IReadOnlyDictionary<string, SchoolPupilNumbers>> GetAsync(
        IEnumerable<string> urns,
        Ks2ProgressSubject subject,
        string pupilCharacteristic)
    {
        var share = CharacteristicShare.TryGetValue(pupilCharacteristic, out var s) ? s : 1m;
        var isAllPupils = string.Equals(pupilCharacteristic, IKs2ProgressPupilNumbersProvider.AllPupils, StringComparison.OrdinalIgnoreCase);

        IReadOnlyDictionary<string, SchoolPupilNumbers> result = urns
            .Distinct(StringComparer.Ordinal)
            .ToDictionary(
                urn => urn,
                urn => new SchoolPupilNumbers(
                    urn,
                    new PupilNumbersYears<int>(
                        Eligible(urn, 0, share),
                        Eligible(urn, 1, share),
                        Eligible(urn, 2, share)),
                    isAllPupils
                        ? new PupilNumbersYears<decimal>(
                            Proportion(urn, 0, subject),
                            Proportion(urn, 1, subject),
                            Proportion(urn, 2, subject))
                        : null),
                StringComparer.Ordinal);

        return Task.FromResult(result);
    }

    private static int Eligible(string urn, int yearIndex, decimal share) =>
        (int)Math.Round((40 + Seed(urn, yearIndex) % 35) * share, MidpointRounding.AwayFromZero);

    private static decimal Proportion(string urn, int yearIndex, Ks2ProgressSubject subject) =>
        90m + Seed(urn, yearIndex * 3 + (int)subject) % 100 / 10m;

    // Deterministic across processes, unlike string.GetHashCode().
    private static int Seed(string urn, int salt)
    {
        var hash = 17 + salt * 7919;
        foreach (var c in urn)
        {
            hash = (hash * 31 + c * 104729) % 99991;
        }

        return hash;
    }
}
