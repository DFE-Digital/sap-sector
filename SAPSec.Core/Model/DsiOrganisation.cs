using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SAPSec.Core.Model;

public class DsiOrganisation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("LegalName")]
    public string? LegalName { get; set; }

    [JsonPropertyName("category")]
    public DsiCategory? Category { get; set; }

    [JsonPropertyName("type")]
    public DsiType? Type { get; set; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("upin")]
    public string? Upin { get; set; }

    [JsonPropertyName("ukprn")]
    public string? Ukprn { get; set; }

    [JsonPropertyName("establishmentNumber")]
    public string? EstablishmentNumber { get; set; }

    [JsonPropertyName("status")]
    public DsiStatus? Status { get; set; }

    [JsonPropertyName("closedOn")]
    public DateTime? ClosedOn { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("region")]
    public DsiRegion? Region { get; set; }

    [JsonPropertyName("localAuthority")]
    public DsiLocalAuthority? LocalAuthority { get; set; }

    [JsonPropertyName("phaseOfEducation")]
    public DsiPhaseOfEducation? PhaseOfEducation { get; set; }

    [JsonPropertyName("statutoryLowAge")]
    public int? StatutoryLowAge { get; set; }

    [JsonPropertyName("statutoryHighAge")]
    public int? StatutoryHighAge { get; set; }

    [JsonPropertyName("legacyId")]
    public string? LegacyId { get; set; }

    [JsonPropertyName("companyRegistrationNumber")]
    public string? CompanyRegistrationNumber { get; set; }

    [JsonPropertyName("SourceSystem")]
    public string? SourceSystem { get; set; }

    [JsonPropertyName("providerTypeName")]
    public string? ProviderTypeName { get; set; }

    [JsonPropertyName("ProviderTypeCode")]
    public int? ProviderTypeCode { get; set; }

    [JsonPropertyName("GIASProviderType")]
    public string? GiasProviderType { get; set; }

    [JsonPropertyName("PIMSProviderType")]
    public string? PimsProviderType { get; set; }

    [JsonPropertyName("PIMSProviderTypeCode")]
    public int? PimsProviderTypeCode { get; set; }

    [JsonPropertyName("PIMSStatusName")]
    public string? PimsStatusName { get; set; }

    [JsonPropertyName("pimsStatus")]
    public string? PimsStatus { get; set; }

    [JsonPropertyName("GIASStatusName")]
    public string? GiasStatusName { get; set; }

    [JsonPropertyName("GIASStatus")]
    public int? GiasStatus { get; set; }

    [JsonPropertyName("MasterProviderStatusName")]
    public string? MasterProviderStatusName { get; set; }

    [JsonPropertyName("MasterProviderStatusCode")]
    public int? MasterProviderStatusCode { get; set; }

    [JsonPropertyName("OpenedOn")]
    public DateTime? OpenedOn { get; set; }

    [JsonPropertyName("DistrictAdministrativeName")]
    public string? DistrictAdministrativeName { get; set; }

    [JsonPropertyName("DistrictAdministrativeCode")]
    public string? DistrictAdministrativeCode { get; set; }

    [JsonPropertyName("IsOnAPAR")]
    public string? IsOnApar { get; set; }

    public List<DsiService> Services { get; set; } = new();
}
