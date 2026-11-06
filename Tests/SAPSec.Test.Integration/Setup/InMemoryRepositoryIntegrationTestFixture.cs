using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Caching;
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

    public InMemoryExclusionsRepository ExclusionsRepository =>
        (InMemoryExclusionsRepository)_factory.Services.GetRequiredService<IExclusionsRepository>();

    public InMemoryRiseResourcesRepository RiseResourcesRepository =>
        (InMemoryRiseResourcesRepository)_factory.Services.GetRequiredService<IRiseResourcesRepository>();

    public ICache Cache => _factory.Services.GetRequiredService<ICache>();

    protected override IntegrationTestsWebApplicationFactory CreateWebApplicationFactory() =>
        new InMemoryRepositoryIntegrationTestsWebApplicationFactory();

    public override async Task ClearDown()
    {
        EstablishmentRepository.ClearDown();
        SimilarSchoolsSecondaryRepository.ClearDown();
        SimilarSchoolsPrimaryRepository.ClearDown();
        Ks2PerformanceRepository.ClearDown();
        Ks4PerformanceRepository.ClearDown();
        Ks4DestinationsRepository.ClearDown();
        AbsenceRepository.ClearDown();
        ExclusionsRepository.ClearDown();
        RiseResourcesRepository.ClearDown();
        await Cache.Clear();
    }
}
