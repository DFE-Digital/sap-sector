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

    public EstablishmentBuilder Primary()
    {
        PhaseOfEducationId = PhaseOfEducationValues.PrimaryId;
        PhaseOfEducationName = PhaseOfEducationValues.Primary;

        return this;
    }

    public EstablishmentBuilder Open()
    {
        EstablishmentStatusId = EstablishmentStatusValues.OpenId;
        EstablishmentStatusName = EstablishmentStatusValues.Open;

        return this;
    }

    public EstablishmentBuilder InLA(string laId)
    {
        LAId = laId;

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
            LAId = LAId
        };
}
