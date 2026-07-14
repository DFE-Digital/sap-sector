using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.School.Info;

public class GetSchoolInfoUseCase(
    IEstablishmentRepository establishmentRepository)
    : IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse>
{
    public async Task<GetSchoolInfoResponse> Execute(GetSchoolInfoRequest request)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(request.Urn);

        if (establishment is null)
        {
            throw new NotFoundException($"School with URN {request.Urn} was not found");
        }

        return new(SchoolInfo.FromEstablishment(establishment));
    }
}

public record GetSchoolInfoRequest(string Urn);

public record GetSchoolInfoResponse(SchoolInfo School);
