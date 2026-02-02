using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Xunit;

namespace SAPSec.Infrastructure.Tests.Repositories
{
    public sealed class PostgresFixture : IAsyncLifetime
    {
        public NpgsqlDataSource DataSource { get; private set; } = default!;

        private readonly string _testDbName;
        private readonly string _adminConnectionString;

        public PostgresFixture()
        {
            var baseCs = Environment.GetEnvironmentVariable("TEST_DB")
                         ?? throw new InvalidOperationException("TEST_DB env var not set.");

            _testDbName = Environment.GetEnvironmentVariable("TEST_DB_NAME") ?? "sapsec_test";

            _adminConnectionString = new NpgsqlConnectionStringBuilder(baseCs)
            {
                Database = "postgres"
            }.ConnectionString;
        }

        public async Task InitializeAsync()
        {
            await EnsureDatabaseExistsAsync(_testDbName);

            var testConnectionString = new NpgsqlConnectionStringBuilder(_adminConnectionString)
            {
                Database = _testDbName
            }.ConnectionString;

            DataSource = NpgsqlDataSource.Create(testConnectionString);

            await EnsureSchemaAsync();
            await ResetAsync();
        }

        public async Task DisposeAsync()
        {
            if (DataSource is not null)
                await DataSource.DisposeAsync();
        }

        public async Task ResetAsync()
        {
            await using var conn = await DataSource.OpenConnectionAsync();

            const string sql = """
                TRUNCATE TABLE public.establishment;
                REFRESH MATERIALIZED VIEW public.v_establishment;
            """;

            await conn.ExecuteAsync(sql);
        }

        public async Task RefreshAsync()
        {
            await using var conn = await DataSource.OpenConnectionAsync();
            await conn.ExecuteAsync("""REFRESH MATERIALIZED VIEW public.v_establishment;""");
        }

        private async Task EnsureDatabaseExistsAsync(string dbName)
        {
            await using var adminConn = new NpgsqlConnection(_adminConnectionString);
            await adminConn.OpenAsync();

            var exists = await adminConn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM pg_database WHERE datname = @name;",
                new { name = dbName });

            if (exists > 0) return;

            var safeDbName = "\"" + dbName.Replace("\"", "\"\"") + "\"";
            await adminConn.ExecuteAsync($"CREATE DATABASE {safeDbName};");
        }

        private async Task EnsureSchemaAsync()
        {
            await using var conn = await DataSource.OpenConnectionAsync();

            const string sql = """
                DO $$
                DECLARE
                  k "char";
                BEGIN
                  SELECT c.relkind
                    INTO k
                  FROM pg_class c
                  JOIN pg_namespace n ON n.oid = c.relnamespace
                  WHERE n.nspname = 'public'
                    AND c.relname = 'v_establishment';

                  IF k = 'm' THEN
                    EXECUTE 'DROP MATERIALIZED VIEW public.v_establishment';
                  ELSIF k = 'v' THEN
                    EXECUTE 'DROP VIEW public.v_establishment';
                  ELSIF k = 'r' THEN
                    EXECUTE 'DROP TABLE public.v_establishment';
                  END IF;
                END $$;

                DROP TABLE IF EXISTS public.establishment;

                CREATE TABLE public.establishment (
                  "URN" TEXT PRIMARY KEY,
                  "UKPRN" TEXT NULL,
                  "DfENumberSearchable" TEXT NULL,
                  "EstablishmentName" TEXT NULL
                );

                CREATE MATERIALIZED VIEW public.v_establishment AS
                SELECT
                  "URN",
                  "UKPRN",
                  "DfENumberSearchable",
                  "EstablishmentName"
                FROM public.establishment;

                CREATE INDEX IF NOT EXISTS ix_v_establishment_urn
                  ON public.v_establishment ("URN");

                REFRESH MATERIALIZED VIEW public.v_establishment;
            """;

            await conn.ExecuteAsync(sql);
        }
    }
}
