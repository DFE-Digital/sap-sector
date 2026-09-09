namespace SAPSec.Web.ViewModels;

public class SimilarSchoolViewModel
{
    public required string Urn { get; init; }
    public required string Name { get; init; }
    public required string LocalAuthorityName { get; init; }
    public required string FullAddress { get; init; }
    public required string? Latitude { get; init; }
    public required string? Longitude { get; init; }
    public required string SortMetricName { get; init; }
    public required string SortMetricDisplayValue { get; init; }
    public required string ComparisonUrl { get; init; }
}

///// <summary>
///// Represents a similar school result, combining data from:
///// - v_similar_schools_secondary_groups (urn, neighbour_urn, dist, rank)
///// - v_similar_schools_secondary_values (ks2_rp, ks2_mp, pp_perc, etc.)
///// - v_establishment (establishment_name, street, town, county, postcode, etc.)
///// </summary>
//public class SimilarSchoolViewModel : ISimilarSchoolRowViewModel
//{
//    public string UrnRaw { get; init; } = string.Empty;
//    public string LocalAuthorityName { get; init; } = string.Empty;
//    public string ComparisonUrl { get; init; } = string.Empty;

//    // From v_similar_schools_secondary_groups
//    public int Urn { get; init; }
//    public int NeighbourUrn { get; init; }
//    public double Dist { get; init; }
//    public int Rank { get; init; }

//    // From v_similar_schools_secondary_values (for the neighbour)
//    //public double? Ks2Rp { get; init; }
//    //public double? Ks2Mp { get; init; }
//    //public double? PpPerc { get; init; }
//    //public double? PercentEal { get; init; }
//    //public int? Polar4QuintilePupils { get; init; }
//    //public double? PStability { get; init; }
//    //public double? IdaciPupils { get; init; }
//    //public double? PercentSchSupport { get; init; }
//    //public int? NumberOfPupils { get; init; }
//    //public double? PercentStatementOrEhp { get; init; }

//    // From v_establishment (for the neighbour)
//    public string EstablishmentName { get; init; } = string.Empty;
//    public string Street { get; init; } = string.Empty;
//    public string Town { get; init; } = string.Empty;
//    public string County { get; init; } = string.Empty;
//    public string Postcode { get; init; } = string.Empty;
//    public string? PhaseOfEducation { get; init; }
//    public string? TypeOfEstablishment { get; init; }
//    public string? Region { get; init; }
//    public string? UrbanOrRural { get; init; }
//    public string? AdmissionsPolicy { get; init; }
//    public string? Gender { get; init; }
//    public bool? HasSixthForm { get; init; }
//    public bool? HasNurseryProvision { get; init; }
//    public string? ResourcedProvisionType { get; init; }
//    public int? SchoolCapacity { get; init; }
//    public double? OverallAbsenceRate { get; init; }
//    public double? PersistentAbsenceRate { get; init; }

//    public string? Latitude { get; init; }
//    public string? Longitude { get; init; }
//    public string SortMetricName { get; init; } = "Attainment 8";
//    public string SortMetricDisplayValue { get; init; } = "N/A";

//    public string FullAddress
//    {
//        get
//        {
//            var parts = new[] { Street, Town, County, Postcode }
//                .Where(p => !string.IsNullOrWhiteSpace(p));
//            return string.Join(", ", parts);
//        }
//    }

//    string ISimilarSchoolRowViewModel.Urn => UrnRaw;
//    string ISimilarSchoolRowViewModel.Name => EstablishmentName;
//    string ISimilarSchoolRowViewModel.LocalAuthorityName => LocalAuthorityName;
//    string ISimilarSchoolRowViewModel.ComparisonUrl => ComparisonUrl;
//    string ISimilarSchoolRowViewModel.FullAddress => FullAddress;
//    string? ISimilarSchoolRowViewModel.Latitude => Latitude;
//    string? ISimilarSchoolRowViewModel.Longitude => Longitude;
//    string ISimilarSchoolRowViewModel.SortMetricName => SortMetricName;
//    string ISimilarSchoolRowViewModel.SortMetricDisplayValue => SortMetricDisplayValue;
//}
