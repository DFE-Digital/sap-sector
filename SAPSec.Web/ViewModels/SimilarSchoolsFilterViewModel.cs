namespace SAPSec.Web.ViewModels;

public class SimilarSchoolsFilterViewModel
{
    // Location
    public string? Distance { get; set; }
    public List<string> SelectedRegions { get; set; } = new();
    public List<string> SelectedUrbanOrRural { get; set; } = new();

    // School characteristics
    public List<string> SelectedPhaseOfEducation { get; set; } = new();
    public string? SchoolCapacityInUse { get; set; }
    public string? NurseryProvision { get; set; }
    public string? SixthForm { get; set; }
    public List<string> SelectedAdmissionsPolicy { get; set; } = new();
    public List<string> SelectedGovernanceStructure { get; set; } = new();
    public List<string> SelectedResourcedProvisionType { get; set; } = new();
    public List<string> SelectedGenderOfEntry { get; set; } = new();

    // Attendance
    public string? OverallAbsenceRate { get; set; }
    public string? PersistentAbsenceRate { get; set; }

    public bool HasActiveFilters =>
        !string.IsNullOrEmpty(Distance) ||
        SelectedRegions.Any() ||
        SelectedUrbanOrRural.Any() ||
        SelectedPhaseOfEducation.Any() ||
        !string.IsNullOrEmpty(SchoolCapacityInUse) ||
        !string.IsNullOrEmpty(NurseryProvision) ||
        !string.IsNullOrEmpty(SixthForm) ||
        SelectedAdmissionsPolicy.Any() ||
        SelectedGovernanceStructure.Any() ||
        SelectedResourcedProvisionType.Any() ||
        SelectedGenderOfEntry.Any() ||
        !string.IsNullOrEmpty(OverallAbsenceRate) ||
        !string.IsNullOrEmpty(PersistentAbsenceRate);
}

public static class FilterOptions
{
    public static readonly List<string> Distances = new()
    {
        "Within 5 miles",
        "Within 10 miles",
        "Within 25 miles",
        "Within 50 miles",
        "Within 100 miles"
    };

    public static readonly List<string> Regions = new()
    {
        "East Midlands",
        "East of England",
        "London",
        "North East",
        "North West",
        "South East",
        "South West",
        "West Midlands",
        "Yorkshire and The Humber"
    };

    public static readonly List<string> UrbanOrRural = new()
    {
        "Urban",
        "Rural"
    };

    public static readonly List<string> PhasesOfEducation = new()
    {
        "Primary",
        "Secondary",
        "All-through"
    };

    public static readonly List<string> SchoolCapacityInUse = new()
    {
        "Under 50%",
        "50% to 75%",
        "75% to 100%",
        "Over 100%"
    };

    public static readonly List<string> NurseryProvision = new()
    {
        "Yes",
        "No"
    };

    public static readonly List<string> SixthForm = new()
    {
        "Yes",
        "No"
    };

    public static readonly List<string> AdmissionsPolicies = new()
    {
        "Comprehensive",
        "Selective",
        "Non-selective",
        "Not applicable"
    };

    public static readonly List<string> GovernanceStructures = new()
    {
        "Academy",
        "Community school",
        "Foundation school",
        "Free school",
        "Voluntary aided school",
        "Voluntary controlled school"
    };

    public static readonly List<string> ResourcedProvisionTypes = new()
    {
        "None",
        "Resourced provision",
        "SEN unit"
    };

    public static readonly List<string> GendersOfEntry = new()
    {
        "Mixed",
        "Boys",
        "Girls"
    };

    public static readonly List<string> AbsenceRates = new()
    {
        "Below 3%",
        "3% to 5%",
        "5% to 7%",
        "Above 7%"
    };

    public static readonly List<string> SortOptions = new()
    {
        "Attainment 8",
        "School name",
        "Distance",
        "Overall absence rate",
        "Persistent absence rate"
    };
} 