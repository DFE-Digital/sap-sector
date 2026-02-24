using System.Globalization;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetCharacteristicsComparison(
    ISimilarSchoolsSecondaryRepository repository)
{
    public async Task<GetCharacteristicsComparisonResponse> Execute(GetCharacteristicsComparisonRequest request)
    {
        var urns = new[] { request.CurrentSchoolUrn, request.SimilarSchoolUrn };

        var values = await repository.GetSecondaryValuesByUrnsAsync(urns);

        if (!values.TryGetValue(request.CurrentSchoolUrn, out var current))
            throw new InvalidOperationException($"No characteristics found for URN {request.CurrentSchoolUrn}");

        if (!values.TryGetValue(request.SimilarSchoolUrn, out var similar))
            throw new InvalidOperationException($"No characteristics found for URN {request.SimilarSchoolUrn}");

        // AC: return all 9 characteristics in this exact order + backend formatting
        var rows = new List<GetCharacteristicsComparisonRow>(9)
        {
            Row("Average KS2 reading and maths score", Ks2Int(current), Ks2Int(similar), true),
            Row("Total number of pupils", IntN0(current.NumberOfPupils), IntN0(similar.NumberOfPupils), true),
            Row("Pupil stability rate", Percent1dp(current.PStability), Percent1dp(similar.PStability), true),
            Row("Eligibility for pupil premium", Percent1dp(current.PpPerc), Percent1dp(similar.PpPerc), true),
            Row("Average IDACI score", Dec3dp(current.IdaciPupils), Dec3dp(similar.IdaciPupils), true),
            Row("Average POLAR4 quintile", PolarText(current.Polar4QuintilePupils), PolarText(similar.Polar4QuintilePupils), false),
            Row("Percentage of pupils with an EHC plan", Percent1dp(current.PercentStatementOrEhp), Percent1dp(similar.PercentStatementOrEhp), true),
            Row("Percentage of pupils with SEN support", Percent1dp(current.PercentSchSupport), Percent1dp(similar.PercentSchSupport), true),
            Row("Percentage of pupils with EAL", Percent1dp(current.PercentEal), Percent1dp(similar.PercentEal), true),
        };

        return new(rows.AsReadOnly());
    }

    private static GetCharacteristicsComparisonRow Row(string c, string a, string b, bool isNumeric) =>
        new(c, a, b, isNumeric);

    // Formatting rules
    private static string Ks2Int(SimilarSchoolsSecondaryValues s)
    {
        var avg = (s.Ks2Rp + s.Ks2Mp) / 2m;
        return Convert.ToInt32(Math.Round(avg, MidpointRounding.AwayFromZero))
            .ToString(CultureInfo.InvariantCulture);
    }

    private static string IntN0(int v) =>
        v.ToString("N0", CultureInfo.GetCultureInfo("en-GB"));

    private static string Percent1dp(decimal v) =>
        $"{v.ToString("0.0", CultureInfo.InvariantCulture)}%";

    private static string Dec3dp(decimal v) =>
        v.ToString("0.000", CultureInfo.InvariantCulture);

    private static string PolarText(int q) =>
        $"Quintile {q}";
}

public record GetCharacteristicsComparisonRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn);

public record GetCharacteristicsComparisonResponse(
    IReadOnlyList<GetCharacteristicsComparisonRow> Rows);

public record GetCharacteristicsComparisonRow(
    string Characteristic,
    string CurrentSchoolValue,
    string SimilarSchoolValue,
    bool IsNumeric);