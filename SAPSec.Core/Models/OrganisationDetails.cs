namespace SAPSec.Core.Models;

public class OrganisationDetails
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Urn { get; set; }
    public string? Ukprn { get; set; }
    public string? EstablishmentNumber { get; set; }
    public OrganisationCategory? Category { get; set; }
    public OrganisationType? Type { get; set; }
}

public class OrganisationCategory
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public class OrganisationType
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}