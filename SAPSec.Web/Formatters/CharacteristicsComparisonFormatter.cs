using System.Globalization;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Formatters;

public interface ICharacteristicsComparisonFormatter
{
    IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow> BuildRows(
        SimilarSchoolsSecondaryValues current,
        SimilarSchoolsSecondaryValues similar);
}

public sealed class CharacteristicsComparisonFormatter : ICharacteristicsComparisonFormatter
{
    public IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow> BuildRows(
        SimilarSchoolsSecondaryValues current,
        SimilarSchoolsSecondaryValues similar)
    {
        return new List<SimilarSchoolsComparisonViewModel.CharacteristicRow>(9)
        {
            new()
            {
                Characteristic = "Average KS2 reading and maths score",
                CurrentSchoolValue = Ks2Int(current),
                SimilarSchoolValue = Ks2Int(similar),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Total number of pupils",
                CurrentSchoolValue = IntN0(current.NumberOfPupils),
                SimilarSchoolValue = IntN0(similar.NumberOfPupils),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Pupil stability rate",
                CurrentSchoolValue = Percent1dp(current.PStability),
                SimilarSchoolValue = Percent1dp(similar.PStability),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Eligibility for pupil premium",
                CurrentSchoolValue = Percent1dp(current.PpPerc),
                SimilarSchoolValue = Percent1dp(similar.PpPerc),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Average IDACI score",
                CurrentSchoolValue = Dec3dp(current.IdaciPupils),
                SimilarSchoolValue = Dec3dp(similar.IdaciPupils),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Average POLAR4 quintile",
                CurrentSchoolValue = PolarText(current.Polar4QuintilePupils),
                SimilarSchoolValue = PolarText(similar.Polar4QuintilePupils),
                IsNumeric = false
            },
            new()
            {
                Characteristic = "Percentage of pupils with an EHC plan",
                CurrentSchoolValue = Percent1dp(current.PercentStatementOrEhp),
                SimilarSchoolValue = Percent1dp(similar.PercentStatementOrEhp),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Percentage of pupils with SEN support",
                CurrentSchoolValue = Percent1dp(current.PercentSchSupport),
                SimilarSchoolValue = Percent1dp(similar.PercentSchSupport),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Percentage of pupils with EAL",
                CurrentSchoolValue = Percent1dp(current.PercentEal),
                SimilarSchoolValue = Percent1dp(similar.PercentEal),
                IsNumeric = true
            },
        }.AsReadOnly();
    }

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
