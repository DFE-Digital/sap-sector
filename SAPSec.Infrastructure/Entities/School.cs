namespace SAPSec.Infrastructure.Entities;

public class School(
    int urn,
    int ukprn,
    int laCode,
    int establishmentNumber,
    string establishmentName)

// public class School(
//     int urn,
//     int ukprn,
//     int laCode,
//     int establishmentNumber,
//     string establishmentName,
//     string street,
//     string locality,
//     string address3,
//     string town,
//     string county,
//     string postcode)
{
    public string EstablishmentName { get; } = establishmentName;
    public int Urn { get; } = urn;
    public int Ukprn { get; } = ukprn;

    private int LaCode { get; } = laCode;

    private int EstablishmentNumber { get; } = establishmentNumber;

    public string DfENumber => $"{LaCode}/{EstablishmentNumber}";

    public int SearchAbleDfENumber => int.Parse($"{LaCode}{EstablishmentNumber}");

    // private string Street { get; } = street;
    // private string Locality { get; } = locality;
    // private string Address3 { get; } = address3;
    // private string Town { get; } = town;
    // private string County { get; } = county;
    // private string Postcode { get; } = postcode;
    //
    // public string EstablishmentNameSchoolId => $"{EstablishmentName}-{Urn}-{Ukprn}-{DfENumber}";
    // public string EstablishmentNameSchoolIdAddress => $"{EstablishmentNameSchoolId}-{Street}-{Locality}-{Address3}-{Town}-{County}-{Postcode}";
}