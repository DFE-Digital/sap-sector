namespace SAPSec.Core.Model.DsiUser;

public class DsiUser
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<DsiOrganisation> Organisations { get; set; } = new();

    public string FullName => $"{GivenName} {FamilyName}";
}

public class DsiOrganisation
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DsiCategory? Category { get; set; }
    public string? Urn { get; set; }
    public string? Uid { get; set; }
    public string? Ukprn { get; set; }
    public string? EstablishmentNumber { get; set; }
    public DsiStatus? Status { get; set; }
    public DateTime? ClosedOn { get; set; }
    public string? Address { get; set; }
    public string? Telephone { get; set; }
    public int? StatutoryLowAge { get; set; }
    public int? StatutoryHighAge { get; set; }
    public string? LegacyId { get; set; }
    public string? CompanyRegistrationNumber { get; set; }
    public List<DsiService> Services { get; set; } = new();
}

public class DsiCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class DsiStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DsiService
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<DsiRole> Roles { get; set; } = new();
}

public class DsiRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int NumericId { get; set; }
    public int Status { get; set; }
}

public class DsiUserInfo
{
    public string UserId { get; set; } = string.Empty;
    public int UserStatus { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public List<DsiOrganisation> Organisations { get; set; } = new();
}