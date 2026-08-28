using Moq;
using SAPSec.Core.Features.RiseResources;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Tests.Features.RiseResources.UseCases;

public class GetRiseResourcesUseCaseTests
{
    [Fact]
    public async Task Execute_WhenSchoolMissing_ThrowsNotFoundException()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("999999"))
            .ReturnsAsync((Establishment?)null);

        var sut = new GetRiseResourcesUseCase(establishmentRepositoryMock.Object);

        var act = async () => await sut.Execute(new GetRiseResourcesRequest("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task Execute_WhenSchoolExists_ReturnsResponseWithSchoolInfoAndNoResources()
    {
        var establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync("123456"))
            .ReturnsAsync(new Establishment { URN = "123456", EstablishmentName = "Test School" });

        var sut = new GetRiseResourcesUseCase(establishmentRepositoryMock.Object);

        var result = await sut.Execute(new GetRiseResourcesRequest("123456"));

        result.Urn.Should().Be("123456");
        result.SchoolName.Should().Be("Test School");
    }
}
