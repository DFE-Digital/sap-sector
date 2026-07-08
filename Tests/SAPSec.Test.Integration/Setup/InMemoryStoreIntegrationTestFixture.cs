using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Repositories;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Test.Integration.Setup;

public class InMemoryRepositoryIntegrationTestFixture : IntegrationTestFixture
{
    public InMemoryEstablishmentRepository EstablishmentRepository =>
        (InMemoryEstablishmentRepository)_factory.Services.GetRequiredService<IEstablishmentRepository>();

    public InMemorySimilarSchoolsSecondaryRepository SimilarSchoolsSecondaryRepository =>
        (InMemorySimilarSchoolsSecondaryRepository)_factory.Services.GetRequiredService<ISimilarSchoolsSecondaryRepository>();

    public InMemorySimilarSchoolsPrimaryRepository SimilarSchoolsPrimaryRepository =>
        (InMemorySimilarSchoolsPrimaryRepository)_factory.Services.GetRequiredService<ISimilarSchoolsPrimaryRepository>();

    public InMemoryKs2PerformanceRepository Ks2PerformanceRepository =>
        (InMemoryKs2PerformanceRepository)_factory.Services.GetRequiredService<IKs2PerformanceRepository>();

    public InMemoryKs4PerformanceRepository Ks4PerformanceRepository =>
        (InMemoryKs4PerformanceRepository)_factory.Services.GetRequiredService<IKs4PerformanceRepository>();

    public InMemoryKs4DestinationsRepository Ks4DestinationsRepository =>
        (InMemoryKs4DestinationsRepository)_factory.Services.GetRequiredService<IKs4DestinationsRepository>();

    public InMemoryAbsenceRepository AbsenceRepository =>
        (InMemoryAbsenceRepository)_factory.Services.GetRequiredService<IAbsenceRepository>();

    protected override IntegrationTestsWebApplicationFactory CreateWebApplicationFactory() =>
        new InMemoryRepositoryIntegrationTestsWebApplicationFactory();

    public override async Task DisposeAsync()
    {
        EstablishmentRepository.ClearDown();
        SimilarSchoolsSecondaryRepository.ClearDown();
        SimilarSchoolsPrimaryRepository.ClearDown();
        Ks2PerformanceRepository.ClearDown();
        Ks4PerformanceRepository.ClearDown();
        Ks4DestinationsRepository.ClearDown();
        AbsenceRepository.ClearDown();

        await base.DisposeAsync();
    }
}
