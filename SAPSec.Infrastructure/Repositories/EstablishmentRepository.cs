using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model;

namespace SAPSec.Infrastructure.Repositories
{
    public class EstablishmentRepository : IEstablishmentRepository
    {
        private readonly ILogger<EstablishmentRepository> _logger;
        private readonly NpgsqlDataSource _dataSource;

        public EstablishmentRepository(
            ILogger<EstablishmentRepository> logger,
            NpgsqlDataSource dataSource)
        {
            _logger = logger;
            _dataSource = dataSource;
        }

        public IEnumerable<Establishment> GetAllEstablishments()
        {
            using var conn = _dataSource.OpenConnection();
            
            const string sql = """
                SELECT *
                FROM public.v_establishment;
            """;

            return conn.Query<Establishment>(sql).ToList();
        }

        public Establishment GetEstablishment(string urn)
        {
            if (string.IsNullOrWhiteSpace(urn))
                return new Establishment();

            using var conn = _dataSource.OpenConnection();
            
            const string sql = """
                SELECT *
                FROM public.v_establishment
               WHERE "URN" = @urn
                LIMIT 1;
            """;

            return conn.QuerySingleOrDefault<Establishment>(sql, new { urn }) ?? new Establishment();
        }

        public Establishment GetEstablishmentByAnyNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return new Establishment();

            using var conn = _dataSource.OpenConnection();
            
            const string sql = """
                SELECT *
                FROM public.v_establishment
               WHERE "URN" = @number
                      OR "UKPRN" = @number
                      OR "DfENumberSearchable" = @number
                LIMIT 1;
            """;

            return conn.QuerySingleOrDefault<Establishment>(sql, new { number }) ?? new Establishment();
        }
    }
}
