using Dapper;
using Microsoft.Extensions.Logging.Abstractions;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Repositories;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class PostgresEstablishmentRepositoryTests : IClassFixture<PostgresFixture>
{
     private readonly PostgresFixture _fx;

        public PostgresEstablishmentRepositoryTests(PostgresFixture fx)
        {
            _fx = fx;
        }

        [Fact(Skip = "Temporarily disabled due to Postgres Not Setup in CI")]
        public async Task GetAllEstablishments_ReturnsAllItemsFromDatabase()
        {
            await _fx.ResetAsync();
            await SeedAsync(
                new Establishment { URN = "1", EstablishmentName = "One" },
                new Establishment { URN = "2", EstablishmentName = "Two" });

            var sut = CreateSut();

            var result = sut.GetAllEstablishments().ToList();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, e => e.URN == "1");
            Assert.Contains(result, e => e.URN == "2");
        }

        [Fact(Skip = "Temporarily disabled due to Postgres Not Setup in CI")]
        public async Task GetAllEstablishments_ReturnsEmptyWhenDatabaseHasNoRows()
        {
            await _fx.ResetAsync();

            var sut = CreateSut();

            var result = sut.GetAllEstablishments().ToList();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact(Skip = "Temporarily disabled due to Postgres Not Setup in CI")]
        public async Task GetEstablishment_ReturnsCorrectItemWhenUrnExists()
        {
            await _fx.ResetAsync();
            await SeedAsync(new Establishment { URN = "123", EstablishmentName = "Found" });

            var sut = CreateSut();

            var result = sut.GetEstablishment("123");

            Assert.NotNull(result);
            Assert.Equal("123", result.URN);
            Assert.Equal("Found", result.EstablishmentName);
        }

        [Fact(Skip = "Temporarily disabled due to Postgres Not Setup in CI")]
        public async Task GetEstablishment_ReturnsNewEstablishmentWhenUrnDoesNotExist()
        {
            await _fx.ResetAsync();
            await SeedAsync(new Establishment { URN = "111", EstablishmentName = "Something" });

            var sut = CreateSut();

            var result = sut.GetEstablishment("999");

            Assert.NotNull(result);

            
            Assert.True(string.IsNullOrEmpty(result.URN));
            Assert.True(string.IsNullOrEmpty(result.EstablishmentName));
        }
        
        [Theory(Skip = "Temporarily disabled due to Postgres Not Setup in CI")]
        [InlineData("URN-1")]
        public async Task GetEstablishmentByAnyNumber_ReturnsMatch(string input)
        {
            await _fx.ResetAsync();

            await SeedRawAsync(
                urn: "URN-1",
                ukprn: 2,
                dfe: "DFE-99",
                name: "AnyNumber Match");

            var sut = CreateSut();

            var result = sut.GetEstablishmentByAnyNumber(input);

            Assert.NotNull(result);
            Assert.Equal("URN-1", result.URN);
            Assert.Equal("AnyNumber Match", result.EstablishmentName);
        }

        
        private PostgresEstablishmentRepository CreateSut()
        {
            return new PostgresEstablishmentRepository(
                NullLogger<PostgresEstablishmentRepository>.Instance,
                _fx.DataSource);
        }

       
        private async Task SeedAsync(params Establishment[] rows)
        {
            await using var conn = await _fx.DataSource.OpenConnectionAsync();

            const string sql = """
                INSERT INTO public.establishment("URN","EstablishmentName")
                VALUES (@URN, @EstablishmentName);

                REFRESH MATERIALIZED VIEW public.v_establishment;
            """;

            await conn.ExecuteAsync(sql, rows);
            await _fx.RefreshAsync();
        }

        
        private async Task SeedRawAsync(string urn, int ukprn, string dfe, string name)
        {
            await using var conn = await _fx.DataSource.OpenConnectionAsync();

            const string sql = """
                INSERT INTO public.establishment("URN","UKPRN","DfENumberSearchable","EstablishmentName")
                VALUES (@urn,@ukprn,@dfe,@name);

                REFRESH MATERIALIZED VIEW public.v_establishment;
            """;

            await conn.ExecuteAsync(sql, new { urn, ukprn, dfe, name });
        }
}