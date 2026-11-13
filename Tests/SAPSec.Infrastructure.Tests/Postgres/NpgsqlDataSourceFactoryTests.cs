using Microsoft.Extensions.Configuration;
using Npgsql;
using SAPSec.Infrastructure.Postgres;

namespace SAPSec.Infrastructure.Tests.Postgres;

public class NpgsqlDataSourceFactoryTests
{
    [Theory]
    [InlineData(null, 100)]
    [InlineData("300", 300)]
    public void Create_AppliesConfiguredPoolSize_AndReusesDataSource(string? poolSize, int expected)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgresConnectionString"] = "Host=localhost;Database=pool_test;Maximum Pool Size=50",
            ["Database:MaxPoolSize"] = poolSize
        }).Build();
        var factory = new NpgsqlDataSourceFactory(configuration);

        using var source = factory.Create();

        new NpgsqlConnectionStringBuilder(source.ConnectionString).MaxPoolSize.Should().Be(expected);
        factory.Create().Should().BeSameAs(source);
    }

    [Theory]
    [InlineData(null, 15)]
    [InlineData("60", 60)]
    public void Create_OverridesConnectionTimeoutOnlyWhenConfigured(string? timeout, int expected)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgresConnectionString"] = "Host=localhost;Database=pool_test;Timeout=15",
            ["Database:ConnectionTimeoutSeconds"] = timeout
        }).Build();

        using var source = new NpgsqlDataSourceFactory(configuration).Create();

        new NpgsqlConnectionStringBuilder(source.ConnectionString).Timeout.Should().Be(expected);
    }
}
