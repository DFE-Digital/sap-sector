using SAPSec.Core.Model;

namespace SAPSec.Core.Interfaces.Services
{
    public interface IEstablishmentService
    {
        Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync();
        Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns);
        Task<Establishment> GetEstablishmentAsync(string urn);
        Task<Establishment> GetEstablishmentByAnyNumberAsync(string number);
    }
}
