using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io.Network;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.FeatureFlags;
using System.Net;

namespace SAPSec.Test.Integration.Setup;

public abstract class IntegrationTestFixture : IAsyncLifetime
{

    protected IntegrationTestsWebApplicationFactory? _factory;

    private IBrowsingContext? _browsingContext = null;

    public HttpClient Client { get; private set; } = null!;

    public HttpClient NonRedirectingClient { get; private set; } = null!;

    public MockFeatureFlagService FeatureFlagService =>
        (MockFeatureFlagService)_factory!.Services.GetRequiredService<IFeatureFlagService>();

    public Task InitializeAsync()
    {
        _factory = CreateWebApplicationFactory();

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

    public virtual async Task DisposeAsync()
    {
        Client?.Dispose();
        NonRedirectingClient?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        _browsingContext?.Dispose();
    }

    public async Task<IDocument> RequestPageAsync(string path, params HttpStatusCode[] expectedStatusCodes)
    {
        if (!expectedStatusCodes.Any())
        {
            expectedStatusCodes = [HttpStatusCode.OK];
        }

        var document = await _browsingContext!.OpenAsync($"https://127.0.0.1:0{path}");

        document.StatusCode.Should().BeOneOf(expectedStatusCodes);

        return document;
    }

    protected abstract IntegrationTestsWebApplicationFactory CreateWebApplicationFactory();
}
