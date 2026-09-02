using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolDetails.School;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SchoolDetails;

public class GetSchoolDetailsUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly Mock<ILogger<SchoolDetailsService>> _loggerMock = new();
    private readonly GetSchoolDetailsUseCase _sut;

    public GetSchoolDetailsUseCaseTests()
    {
        _sut = new GetSchoolDetailsUseCase(
            new SchoolDetailsService(_establishmentRepo, _loggerMock.Object));
    }

    [Fact]
    public async Task WhenSchoolUrnDoesNotExist_ThrowsNotFoundException()
    {
        var act = async () => await _sut.Execute(new("100001"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*100001*");
    }

    [Fact]
    public async Task SchoolDetails_ContainsSchoolDetailsForSimilarSchool()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Test School", x => x
                .WithTelephone("01234 567890")
                .WithWebsite("https://similar-school.example.com")));

        var response = await _sut.Execute(new("100001"));

        response.SchoolDetails.Urn.Should().Be("100001");
        response.SchoolDetails.Name.Should().Be("Test School");
        response.SchoolDetails.Telephone.Value.Should().Be("01234 567890");
        response.SchoolDetails.Website.Value.Should().Be("https://similar-school.example.com");
    }
}
