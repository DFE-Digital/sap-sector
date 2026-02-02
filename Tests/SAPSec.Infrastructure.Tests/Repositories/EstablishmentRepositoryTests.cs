using Dapper;
using Microsoft.Extensions.Logging.Abstractions;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Repositories;

namespace SAPSec.Infrastructure.Tests.Repositories
{

    public class EstablishmentRepositoryTests : IClassFixture<PostgresFixture>
    {
        private readonly PostgresFixture _fx;

        public EstablishmentRepositoryTests(PostgresFixture fx)
        {
            _fx = fx;
        }

        [Fact]
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

        [Fact]
        public async Task GetAllEstablishments_ReturnsEmptyWhenDatabaseHasNoRows()
        {
            await _fx.ResetAsync();

            var sut = CreateSut();

            var result = sut.GetAllEstablishments().ToList();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
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

        [Fact]
        public async Task GetEstablishment_ReturnsNewEstablishmentWhenUrnDoesNotExist()
        {
            await _fx.ResetAsync();
            await SeedAsync(new Establishment { URN = "111", EstablishmentName = "Something" });

            var sut = CreateSut();

            var result = sut.GetEstablishment("999");

            Assert.NotNull(result);

            // Matches your repo behavior: returns new Establishment() when not found
            Assert.True(string.IsNullOrEmpty(result.URN));
            Assert.True(string.IsNullOrEmpty(result.EstablishmentName));
        }

        [Theory]
        [InlineData("URN-1")]
        [InlineData("UKPRN-1")]
        [InlineData("DFE-1")]
        public async Task GetEstablishmentByAnyNumber_ReturnsMatch(string input)
        {
            await _fx.ResetAsync();
            await SeedRawAsync(
                urn: "URN-1",
                ukprn: "UKPRN-1",
                dfe: "DFE-1",
                name: "AnyNumber Match");

            var sut = CreateSut();

            var result = sut.GetEstablishmentByAnyNumber(input);

            Assert.NotNull(result);
            Assert.Equal("URN-1", result.URN);
            Assert.Equal("AnyNumber Match", result.EstablishmentName);
        }

        private EstablishmentRepository CreateSut()
        {
            return new EstablishmentRepository(
                NullLogger<EstablishmentRepository>.Instance,
                _fx.DataSource);
        }

        /// <summary>
        /// Seeds the base table and refreshes the materialized view so repository reads see the new rows.
        /// </summary>
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

        /// <summary>
        /// Seeds all columns used by GetEstablishmentByAnyNumber and refreshes the MV.
        /// </summary>
        private async Task SeedRawAsync(string urn, string ukprn, string dfe, string name)
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
}
