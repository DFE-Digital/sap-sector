namespace SAPSec.Web.ViewModels;

/// <summary>
/// Represents a similar school result, combining data from:
/// - v_similar_schools_secondary_groups (urn, neighbour_urn, dist, rank)
/// - v_similar_schools_secondary_values (ks2_rp, ks2_mp, pp_perc, etc.)
/// - v_establishment (establishment_name, street, town, county, postcode, etc.)
/// </summary>
public class SimilarSchoolViewModel
{
    // From v_similar_schools_secondary_groups
    public int Urn { get; set; }
    public int NeighbourUrn { get; set; }
    public double Dist { get; set; }
    public int Rank { get; set; }

    // From v_similar_schools_secondary_values (for the neighbour)
    public double? Ks2Rp { get; set; }
    public double? Ks2Mp { get; set; }
    public double? PpPerc { get; set; }
    public double? PercentEal { get; set; }
    public int? Polar4QuintilePupils { get; set; }
    public double? PStability { get; set; }
    public double? IdaciPupils { get; set; }
    public double? PercentSchSupport { get; set; }
    public int? NumberOfPupils { get; set; }
    public double? PercentStatementOrEhp { get; set; }
    public double? Att8Scr { get; set; }

    // From v_establishment (for the neighbour)
    public string EstablishmentName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string Postcode { get; set; } = string.Empty;
    public string? PhaseOfEducation { get; set; }
    public string? TypeOfEstablishment { get; set; }
    public string? Region { get; set; }
    public string? UrbanOrRural { get; set; }
    public string? AdmissionsPolicy { get; set; }
    public string? Gender { get; set; }
    public bool? HasSixthForm { get; set; }
    public bool? HasNurseryProvision { get; set; }
    public string? ResourcedProvisionType { get; set; }
    public int? SchoolCapacity { get; set; }
    public double? OverallAbsenceRate { get; set; }
    public double? PersistentAbsenceRate { get; set; }

    public string FullAddress
    {
        get
        {
            var parts = new[] { Street, Town, County, Postcode }
                .Where(p => !string.IsNullOrWhiteSpace(p));
            return string.Join(", ", parts);
        }
    }
}