using SAPSec.Data.Dto;

namespace SAPSec.Data.Store;

public interface IEstablishmentEmailStore
{
    Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn);
}
