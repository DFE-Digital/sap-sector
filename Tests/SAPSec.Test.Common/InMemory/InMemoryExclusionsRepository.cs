using SAPSec.Data.Dto.Exclusions;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemoryExclusionsRepository(IEstablishmentRepository establishmentRepository) : IExclusionsRepository
{
    private List<EstablishmentExclusions> _establishment = new();

    public InMemoryExclusionsRepository SetupEstablishmentExclusions(params EstablishmentExclusions[] establishment)
    {
        _establishment = establishment.ToList();

        return this;
    }

    public InMemoryExclusionsRepository ClearDown()
    {
        _establishment = [];

        return this;
    }

    public async Task<ExclusionsData?> GetByUrnAsync(string urn)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn);

        return establishment is null
            ? null
            : new ExclusionsData(urn, _establishment.FirstOrDefault(x => x.Id == urn));
    }

    public async Task<IReadOnlyCollection<ExclusionsData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);

        return establishments.Select(e => new ExclusionsData(
                e.URN,
                _establishment.FirstOrDefault(x => x.Id == e.URN)))
            .ToList();
    }
}
