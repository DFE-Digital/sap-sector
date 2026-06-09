namespace SAPSec.Core.Features.SchoolDetails;
public class GetSchoolDetailsUseCase(
    ISchoolDetailsService schoolDetailsService)
    : IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse>
{
    public async Task<GetSchoolDetailsResponse> Execute(GetSchoolDetailsRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);

        return new(schoolDetails);

    }
}

public record GetSchoolDetailsRequest(string Urn);

public record GetSchoolDetailsResponse(SchoolDetails SchoolDetails);