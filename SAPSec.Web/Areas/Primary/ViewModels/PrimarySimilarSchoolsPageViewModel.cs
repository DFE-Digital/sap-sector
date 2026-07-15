using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using System.Globalization;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public record PrimarySimilarSchoolsPageViewModel(
    SchoolInfoViewModel CurrentSchool,
    string CurrentSchoolLocalAuthorityName,
    PrimarySimilarSchoolsCharacteristicsViewModel CurrentSchoolCharacteristics,
    IReadOnlyCollection<PrimarySimilarSchoolsRowViewModel> SimilarSchools)
{
    public static PrimarySimilarSchoolsPageViewModel FromResponse(
        FindPrimarySimilarSchoolsResponse response) =>
        new(
            new SchoolInfoViewModel(
                response.CurrentSchool.Urn,
                response.CurrentSchool.Name,
                string.Empty),
            response.CurrentSchool.LocalAuthorityName,
            PrimarySimilarSchoolsCharacteristicsViewModel.FromResponse(response.CurrentSchool.Characteristics),
            response.SimilarSchools
                .Select(row => new PrimarySimilarSchoolsRowViewModel(
                    row.Urn,
                    row.Name,
                    row.LocalAuthorityName,
                    row.Rank,
                    row.Distance,
                    Routes.PrimarySchool(response.CurrentSchool.Urn).SimilarSchoolComparison(row.Urn),
                    PrimarySimilarSchoolsCharacteristicsViewModel.FromResponse(row.Characteristics)))
                .ToList()
                .AsReadOnly());
}

public record PrimarySimilarSchoolsRowViewModel(
    string Urn,
    string Name,
    string LocalAuthorityName,
    string Rank,
    string Distance,
    string ComparisonUrl,
    PrimarySimilarSchoolsCharacteristicsViewModel Characteristics);

public record PrimarySimilarSchoolsCharacteristicsViewModel(
    string ReadMatAverage,
    string Ks1PriorRwmAverage,
    string PupilPremiumEligibilityPercentage,
    string PupilsWithEalPercentage,
    string Polar4Quintile,
    string PupilStabilityRate,
    string AverageIdaciScore,
    string PupilsWithSenSupportPercentage,
    string PupilCount,
    string PupilsWithEhcPlanPercentage)
{
    public static PrimarySimilarSchoolsCharacteristicsViewModel FromResponse(
        PrimarySimilarSchoolCharacteristics characteristics) =>
        new(
            DisplayDecimal(characteristics.ReadMatAverage),
            DisplayDecimal(characteristics.Ks1PriorRwmAverage),
            DisplayPercent(characteristics.PupilPremiumEligibilityPercentage),
            DisplayPercent(characteristics.PupilsWithEalPercentage),
            DisplayDecimal(characteristics.Polar4Quintile),
            DisplayPercent(characteristics.PupilStabilityRate),
            DisplayDecimal(characteristics.AverageIdaciScore, "0.###"),
            DisplayPercent(characteristics.PupilsWithSenSupportPercentage),
            DisplayDecimal(characteristics.PupilCount, "0"),
            DisplayPercent(characteristics.PupilsWithEhcPlanPercentage));

    private static string DisplayDecimal(decimal value, string format = "0.#") =>
        value.ToString(format, CultureInfo.InvariantCulture);

    private static string DisplayPercent(decimal value) =>
        value.ToString("0.#", CultureInfo.InvariantCulture) + "%";
}
