using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Interfaces.Repositories;

public interface IEstablishmentEmailRepository
{
    Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn);
}
