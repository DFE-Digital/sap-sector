using SAPSec.Core.Model;
using SAPSec.Web.Helpers;

namespace SAPSec.Web.ViewModels;

public class Ks4HeadlineMeasuresPageViewModel
{
    public required SchoolDetails SchoolDetails { get; set; }
    public required decimal? SchoolAttainment8ThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityAttainment8ThreeYearAverage { get; set; }
    public required decimal? EnglandAttainment8ThreeYearAverage { get; set; }

    public string SchoolAttainment8Display => DisplayValue(SchoolAttainment8ThreeYearAverage);
    public string LocalAuthorityAttainment8Display => DisplayValue(LocalAuthorityAttainment8ThreeYearAverage);
    public string EnglandAttainment8Display => DisplayValue(EnglandAttainment8ThreeYearAverage);

    public string SchoolLabel => SchoolDetails.Name.Display();
    public string LocalAuthorityLabel => $"{SchoolDetails.LocalAuthorityName.Display()} local authority";
    public string EnglandLabel => "England";

    public decimal SchoolWidthPercent => WidthPercent(SchoolAttainment8ThreeYearAverage);
    public decimal LocalAuthorityWidthPercent => WidthPercent(LocalAuthorityAttainment8ThreeYearAverage);
    public decimal EnglandWidthPercent => WidthPercent(EnglandAttainment8ThreeYearAverage);

    private decimal MaxValue => new[]
        {
            SchoolAttainment8ThreeYearAverage ?? 0m,
            LocalAuthorityAttainment8ThreeYearAverage ?? 0m,
            EnglandAttainment8ThreeYearAverage ?? 0m
        }
        .Max();

    private decimal WidthPercent(decimal? value)
    {
        if (!value.HasValue || MaxValue <= 0)
        {
            return 0m;
        }

        return Math.Round((value.Value / MaxValue) * 100m, 1, MidpointRounding.AwayFromZero);
    }

    private static string DisplayValue(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0") : "No available data";
}
