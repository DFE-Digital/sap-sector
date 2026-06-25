using SAPSec.Data.Dto;

namespace SAPSec.Data.Repositories;

public interface IEstablishmentEmailRepository
{
    Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn);
}
