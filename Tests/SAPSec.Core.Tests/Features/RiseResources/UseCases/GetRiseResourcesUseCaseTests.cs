using SAPSec.Core.Features.RiseResources;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.RiseResources.UseCases;

public class GetRiseResourcesUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly GetRiseResourcesUseCase _sut;

    public GetRiseResourcesUseCaseTests()
    {
        _establishmentRepo = new();
        _sut = new GetRiseResourcesUseCase(_establishmentRepo);
    }

    [Fact]
    public async Task Execute_WhenSchoolMissing_ThrowsNotFoundException()
    {
        var act = async () => await _sut.Execute(new GetRiseResourcesRequest("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task Execute_WhenSchoolExists_ReturnsResponseWithSchoolInfoAndNoResources()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("123456", "Test School", x => x.Secondary()));

        var result = await _sut.Execute(new GetRiseResourcesRequest("123456"));

        result.Urn.Should().Be("123456");
        result.SchoolName.Should().Be("Test School");
    }
}
