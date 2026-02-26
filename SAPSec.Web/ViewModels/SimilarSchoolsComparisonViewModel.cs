using System.Globalization;
using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class SimilarSchoolsComparisonViewModel
{
    public required string Urn { get; set; }
    public required string SimilarSchoolUrn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolName { get; set; }

    // ----------------------------
    // School Details
    // ----------------------------
    public double? CurrentSchoolLatitude { get; set; }
    public double? CurrentSchoolLongitude { get; set; }
    public double? SimilarSchoolLatitude { get; set; }
    public double? SimilarSchoolLongitude { get; set; }
    public double? Distance { get; set; }
    public string DistanceDisplay =>
        Distance is double d
            ? d.ToString("0.0", CultureInfo.InvariantCulture)
            : string.Empty;
    public SchoolDetails? SimilarSchoolDetails { get; set; }

    // ----------------------------
    // Similarity (9 characteristics table)
    // ----------------------------
    public IReadOnlyList<CharacteristicRow> CharacteristicsRows { get; set; }
        = Array.Empty<CharacteristicRow>();

    public sealed class CharacteristicRow
    {
        public required string Characteristic { get; init; }
        public required string CurrentSchoolValue { get; init; }
        public required string SimilarSchoolValue { get; init; }
        public bool IsNumeric { get; init; }
    }
}