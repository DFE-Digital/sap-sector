using SAPSec.Core.Constants;
using SAPSec.Data.Dto;

namespace SAPSec.Test.Common.Builders;

public class EstablishmentBuilder(string urn, string name)
{
    string PhaseOfEducationId = string.Empty;
    string PhaseOfEducationName = string.Empty;
    string EstablishmentStatusId = string.Empty;
    string EstablishmentStatusName = string.Empty;
    string LAId = string.Empty;
    string LAName = string.Empty;
    string Street = string.Empty;
    string Locality = string.Empty;
    string Address3 = string.Empty;
    string Town = string.Empty;
    string Postcode = string.Empty;

    public EstablishmentBuilder Primary()
    {
        PhaseOfEducationId = PhaseOfEducationValues.PrimaryId;
        PhaseOfEducationName = PhaseOfEducationValues.Primary;

        return this;
    }

    public EstablishmentBuilder Secondary()
    {
        PhaseOfEducationId = PhaseOfEducationValues.SecondaryId;
        PhaseOfEducationName = PhaseOfEducationValues.Secondary;

        return this;
    }

    public EstablishmentBuilder AllThrough()
    {
        PhaseOfEducationId = PhaseOfEducationValues.AllThroughId;
        PhaseOfEducationName = PhaseOfEducationValues.AllThrough;

        return this;
    }

    public EstablishmentBuilder Open()
    {
        EstablishmentStatusId = EstablishmentStatusValues.OpenId;
        EstablishmentStatusName = EstablishmentStatusValues.Open;

        return this;
    }

    public EstablishmentBuilder InLA(string laId, string? laName = null)
    {
        LAId = laId;
        LAName = laName ?? string.Empty;

        return this;
    }

    public EstablishmentBuilder WithAddress(
        string street,
        string locality,
        string address3,
        string town,
        string postcode)
    {
        Street = street;
        Locality = locality;
        Address3 = address3;
        Town = town;
        Postcode = postcode;

        return this;
    }

    public Establishment Build() =>
        new Establishment()
        {
            URN = urn,
            EstablishmentName = name,
            PhaseOfEducationId = PhaseOfEducationId,
            PhaseOfEducationName = PhaseOfEducationName,
            EstablishmentStatusId = EstablishmentStatusId,
            EstablishmentStatusName = EstablishmentStatusName,
            LAId = LAId,
            LAName = LAName,
            Street = Street,
            Locality = Locality,
            Address3 = Address3,
            Town = Town,
            Postcode = Postcode
        };
}
