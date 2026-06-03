using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model.Generated;
using SAPSec.Infrastructure.Json;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class PrimaryJsonEstablishmentRepositoryTests
{
    private readonly Mock<IJsonFileFactory> _jsonFileFactory = new();
    private readonly Mock<IJsonFile<Establishment>> _establishments = new();
    private readonly Mock<IJsonFile<EstablishmentEmail>> _emails = new();
    private readonly Mock<ILogger<PrimaryJsonEstablishmentRepository>> _logger = new();

    private PrimaryJsonEstablishmentRepository CreateSut()
    {
        _jsonFileFactory
            .Setup(x => x.Create<Establishment>(JsonDataSource.PrimarySchools))
            .Returns(_establishments.Object);
        _jsonFileFactory
            .Setup(x => x.Create<EstablishmentEmail>(JsonDataSource.PrimarySchools))
            .Returns(_emails.Object);

        return new PrimaryJsonEstablishmentRepository(_jsonFileFactory.Object, _logger.Object);
    }

    [Fact]
    public async Task GetEstablishmentAsync_ReturnsPrimaryEstablishment()
    {
        _establishments
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync(
            [
                new Establishment { URN = "100001", EstablishmentName = "Primary A" },
                new Establishment { URN = "100002", EstablishmentName = "Primary B" }
            ]);

        var sut = CreateSut();

        var result = await sut.GetEstablishmentAsync("100002");

        Assert.NotNull(result);
        Assert.Equal("100002", result.URN);
        Assert.Equal("Primary B", result.EstablishmentName);
    }

    [Fact]
    public async Task GetEstablishmentEmailAsync_ReturnsPrimaryEmail()
    {
        _emails
            .Setup(x => x.ReadAllAsync())
            .ReturnsAsync(
            [
                new EstablishmentEmail { URN = "100002", MainEmail = "school@example.com" }
            ]);

        var sut = CreateSut();

        var result = await sut.GetEstablishmentEmailAsync("100002");

        Assert.NotNull(result);
        Assert.Equal("school@example.com", result.MainEmail);
    }
}
