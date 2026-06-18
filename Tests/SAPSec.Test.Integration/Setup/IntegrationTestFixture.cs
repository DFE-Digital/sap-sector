using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io.Network;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SAPSec.Data.Store;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Test.Common.InMemoryStore;
using System.Net;

namespace SAPSec.Test.Integration.Setup;

public class IntegrationTestFixture : IAsyncLifetime
{
    private static readonly TimeSpan SearchIndexTimeout = TimeSpan.FromSeconds(3);

    private IntegrationTestsWebApplicationFactory _factory = null!;
    private IBrowsingContext _browsingContext = null!;

    public HttpClient Client { get; private set; } = null!;

    public HttpClient NonRedirectingClient { get; private set; } = null!;

    public InMemoryEstablishmentStore EstablishmentStore => (InMemoryEstablishmentStore)_factory.Services.GetRequiredService<IEstablishmentStore>();
    public StartupIndexBuilder IndexBuilder => _factory.Services.GetServices<IHostedService>()
        .OfType<StartupIndexBuilder>()
        .Single();

    public Task InitializeAsync()
    {
        _factory = new IntegrationTestsWebApplicationFactory();

        if (_factory.Server == null) throw new InvalidOperationException("Test Server not started");

        // Client that follows redirects
        Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = _factory.ClientOptions.BaseAddress,
            AllowAutoRedirect = true
        });

        // Client that doesn't follow redirects
        NonRedirectingClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = _factory.ClientOptions.BaseAddress,
            AllowAutoRedirect = false
        });

        Console.WriteLine($"Fixture initialized with base URL: {_factory.ClientOptions.BaseAddress}");

        var requester = new HttpClientRequester(Client);
        var config = Configuration.Default.With(requester).WithDefaultLoader();
        _browsingContext = BrowsingContext.New(config);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        NonRedirectingClient?.Dispose();
        await _factory.DisposeAsync();
    }

    public async Task RebuildSearchIndex()
    {
        IndexBuilder.IndexBuiltSuccessfully = false;
        var startTime = DateTime.UtcNow.TimeOfDay;
        await IndexBuilder.StartAsync(CancellationToken.None);
        while (!IndexBuilder.IndexBuiltSuccessfully)
        {
            if (DateTime.UtcNow.TimeOfDay - startTime > SearchIndexTimeout)
            {
                throw new TimeoutException("Timed out waiting for search index to be rebuilt.");
            }

            await Task.Delay(100);
        }
    }

    public async Task<IDocument> RequestPageAsync(string path, params HttpStatusCode[] expectedStatusCodes)
    {
        if (!expectedStatusCodes.Any())
        {
            expectedStatusCodes = [HttpStatusCode.OK];
        }

        var document = await _browsingContext.OpenAsync($"https://127.0.0.1:0{path}");
        document.StatusCode.Should().BeOneOf(expectedStatusCodes);

        return document;
    }
}