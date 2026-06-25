using SAPSec.Data.Dto;

namespace SAPSec.Core.Interfaces.Repositories;

public interface IEstablishmentEmailRepository
{
    Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn);
}
