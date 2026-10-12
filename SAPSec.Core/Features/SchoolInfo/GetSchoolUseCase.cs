using SAPSec.Core.Exceptions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolInfo;

public class GetSchoolUseCase(
    IEstablishmentRepository establishmentRepository)
    : IUseCase<GetSchoolRequest, GetSchoolResponse>
{
    public async Task<GetSchoolResponse> Execute(GetSchoolRequest request)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(request.Urn);

        if (establishment is null)
        {
            throw new NotFoundException($"School with URN {request.Urn} was not found");
        }

        return new(School.Create(establishment));
    }
}

public record GetSchoolRequest(string Urn);

public record GetSchoolResponse(School School);