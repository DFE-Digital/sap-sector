using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Repositories;
using SAPSec.Test.Common.Authentication;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web;
using SAPSec.Web.Authorization;
using SAPSec.Web.Constants;
using System.Net;

namespace SAPSec.Test.Integration.Tests.Authorization;

/// <summary>
/// Runs the real <see cref="DsiAuthorizationHandler"/>. The mock signed-in user belongs to
/// establishment URN 123456 (see MockDsiUserService).
/// </summary>
public class SchoolUrnAccessIntegrationTests : IAsyncLifetime
{
    private const string OwnUrn = "123456";
    private const string OtherUrn = "100001";

    private InMemoryRepositoryIntegrationTestsWebApplicationFactory _baseFactory = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _baseFactory = new InMemoryRepositoryIntegrationTestsWebApplicationFactory();
        _factory = _baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services
                         .Where(d => d.ServiceType == typeof(IAuthorizationHandler)
                                     && d.ImplementationType == typeof(AllowAllUrnsDsiAuthorizationHandler))
                         .ToList())
            {
                services.Remove(descriptor);
            }

            services.AddScoped<IAuthorizationHandler, DsiAuthorizationHandler>();
        }));

        ((InMemoryEstablishmentRepository)_factory.Services.GetRequiredService<IEstablishmentRepository>())
            .SetupEstablishments(
                Build.Establishment(OwnUrn, "Own School", x => x.Open().Primary()),
                Build.Establishment(OtherUrn, "Other School", x => x.Open().Primary()));

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _baseFactory.DisposeAsync();
    }

    [Fact]
    public async Task OwnSchool_ReturnsOk()
    {
        var response = await _client.GetAsync(Routes.PrimarySchool(OwnUrn).Overview);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AnotherSchool_ReturnsForbidden()
    {
        var response = await _client.GetAsync(Routes.PrimarySchool(OtherUrn).Overview);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task FindASchool_StillAccessible()
    {
        var response = await _client.GetAsync("/find-a-school");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
