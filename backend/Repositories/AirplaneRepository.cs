using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.domain;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public class AirplaneRepository
    {

        private readonly string _connectionString;

        public AirplaneRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        }

        public async Task<Airplane> CreateAirplaneAsync(Airplane airplane, CancellationToken cancellationToken)
        {
           const string sql = """
            INSERT INTO airplane (model, tourist_rows, tourist_columns, firstclass_rows, firstclass_columns, max_weight)
            OUTPUT
                INSERTED.model AS Model,
                INSERTED.tourist_rows AS TouristRows,
                INSERTED.tourist_columns AS TouristColumns,
                INSERTED.firstclass_rows AS FirstclassRows,
                INSERTED.firstclass_columns AS FirstclassColumns,
                INSERTED.max_weight AS MaxWeight
            VALUES (@Model, @TouristRows, @TouristColumns, @FirstclassRows, @FirstclassColumns, @MaxWeight);
            """;
            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<Airplane>(
                new CommandDefinition(sql, airplane, cancellationToken: cancellationToken));

        }

        public Task<IReadOnlyCollection<Airplane>> GetAirplanesAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Airplanes retrieved");
            return Task.FromResult<IReadOnlyCollection<Airplane>>(Array.Empty<Airplane>()); //  cambiar por consulta
        }

        public async Task<bool> ExistsByModelAsync(string model, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM Airplane
                WHERE model = @Model;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { Model = model }, cancellationToken: cancellationToken)) > 0;

        }
    }
}