using SAPSec.Core.Model;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public class MeasureViewModel
{
    public required string Key { get; set; }
    public required string HtmlPrefix { get; set; }
    public required string Title { get; set; }
    public required SchoolDetails SchoolDetails { get; set; }
    public required SubjectSection Subject { get; set; }
    public required IEnumerable<GradeOptionViewModel> GradeOptions { get; set; }

    public string SchoolLabel => SchoolDetails.Name;
    public string SimilarSchoolsLabel => "Similar schools average";
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";
}

public enum NumberDisplayType
{
    Number,
    Percentage
}

public class Measure2ViewModel
{
    public required string HtmlPrefix { get; set; }
    public required SchoolDetails SchoolDetails { get; set; }
    public required SubjectSection Subject { get; set; }
    public required NumberDisplayType NumberDisplayType { get; set; }

    public string SchoolLabel => SchoolDetails.Name;
    public string SimilarSchoolsLabel => "Similar schools average";
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";

    public string DisplayNumber(decimal? value) =>
        this.NumberDisplayType == NumberDisplayType.Number
            ? DisplayValue(value)
            : DisplayWholePercent(value);

    private static string DisplayValue(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) : "No available data";

    private static string DisplayWholeValue(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture)
            : "No available data";

    private static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";

    private static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";

}

public record GradeOptionViewModel(string Key, string Name);
