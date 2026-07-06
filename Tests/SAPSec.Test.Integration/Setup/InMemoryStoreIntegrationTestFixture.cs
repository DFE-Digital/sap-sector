using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Store;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Test.Integration.Setup;

public class InMemoryStoreIntegrationTestFixture : IntegrationTestFixture
{
    public InMemoryEstablishmentStore EstablishmentStore =>
        (InMemoryEstablishmentStore)_factory.Services.GetRequiredService<IEstablishmentStore>();

    public InMemorySimilarSchoolsSecondaryStore SimilarSchoolsSecondaryStore =>
        (InMemorySimilarSchoolsSecondaryStore)_factory.Services.GetRequiredService<ISimilarSchoolsSecondaryStore>();

    public InMemorySimilarSchoolsPrimaryStore SimilarSchoolsPrimaryStore =>
        (InMemorySimilarSchoolsPrimaryStore)_factory.Services.GetRequiredService<ISimilarSchoolsPrimaryStore>();

    public InMemoryKs2PerformanceStore Ks2PerformanceStore =>
        (InMemoryKs2PerformanceStore)_factory.Services.GetRequiredService<IKs2PerformanceStore>();

    public InMemoryKs4PerformanceStore Ks4PerformanceStore =>
        (InMemoryKs4PerformanceStore)_factory.Services.GetRequiredService<IKs4PerformanceStore>();

    public InMemoryKs4DestinationsStore Ks4DestinationsStore =>
        (InMemoryKs4DestinationsStore)_factory.Services.GetRequiredService<IKs4DestinationsStore>();

    public InMemoryAbsenceStore AbsenceStore =>
        (InMemoryAbsenceStore)_factory.Services.GetRequiredService<IAbsenceStore>();

    protected override IntegrationTestsWebApplicationFactory CreateWebApplicationFactory() =>
        new InMemoryStoreIntegrationTestsWebApplicationFactory();

    public override async Task DisposeAsync()
    {
        EstablishmentStore.ClearDown();
        SimilarSchoolsSecondaryStore.ClearDown();
        SimilarSchoolsPrimaryStore.ClearDown();
        Ks2PerformanceStore.ClearDown();
        Ks4PerformanceStore.ClearDown();
        Ks4DestinationsStore.ClearDown();
        AbsenceStore.ClearDown();

        await base.DisposeAsync();
    }
}
