using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolInfo;

public record LocalAuthority(string Id, string Name)
{
    public static LocalAuthority FromEstablishment(Establishment establishment) =>
         new(establishment.LAId, establishment.LAName);
}
