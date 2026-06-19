using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.domain;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public class AirplaneRepository : IAirplaneRepository
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
                INSERTED.id AS Id,
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

        public async Task<IReadOnlyCollection<Airplane>> GetAirplanesAsync(CancellationToken cancellationToken)
        {
            const string sql = """
                Select 
                id AS Id,
                model AS Model, 
                tourist_rows AS TouristRows, 
                tourist_columns AS TouristColumns, 
                firstclass_rows AS FirstclassRows,
                firstclass_columns AS FirstclassColumns, 
                max_weight AS MaxWeight
                FROM Airplane
                WHERE is_deleted = 0;

                """;
            await using var connection = new SqlConnection(_connectionString);
            var airplanes = await connection.QueryAsync<Airplane>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));
            return airplanes.ToList().AsReadOnly();
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

        public async Task<Airplane?> GetAirplaneByModelAsync(string model, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT 
                    id AS Id,
                    model AS Model,
                    tourist_rows AS TouristRows,
                    tourist_columns AS TouristColumns,
                    firstclass_rows AS FirstclassRows,
                    firstclass_columns AS FirstclassColumns,
                    max_weight AS MaxWeight
                
                FROM Airplane
                WHERE model = @Model;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Airplane>(
                new CommandDefinition(sql, new { Model = model }, cancellationToken: cancellationToken));
        }

        public async Task<Airplane?> GetAirplaneByIdAsync(int airplaneId, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    id AS Id,
                    model AS Model,
                    tourist_rows AS TouristRows,
                    tourist_columns AS TouristColumns,
                    firstclass_rows AS FirstclassRows,
                    firstclass_columns AS FirstclassColumns,
                    max_weight AS MaxWeight,
                    is_deleted AS IsDeleted
                FROM airplane
                WHERE id = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Airplane>(
                new CommandDefinition(sql, new { Id = airplaneId }, cancellationToken: cancellationToken));
        }

        public async Task UpdateAirplaneCapacitiesAsync(int airplaneId, AirplaneUpdateIntake intake, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);

            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "sp_update_airplane_capacities",
                    new
                    {
                        Id = airplaneId,
                        TouristRows = intake.TouristRows,
                        TouristColumns = intake.TouristColumns,
                        FirstclassRows = intake.FirstClassRows,
                        FirstclassColumns = intake.FirstClassColumns,
                        MaxWeight = intake.MaxWeight
                    },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
            }
            catch (SqlException exception)
            {
                if (exception.Message.Contains("No se encontró"))
                    throw new KeyNotFoundException(exception.Message, exception);

                throw new InvalidOperationException(exception.Message, exception);
            }
        }

        public async Task<bool> AirplaneHasPurchasesAsync(int airplaneId, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM dbo.PurchaseOrderRoute por
                INNER JOIN dbo.[route] r ON r.id = por.RouteId
                WHERE r.airplane_id = @AirplaneId;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { AirplaneId = airplaneId }, cancellationToken: cancellationToken));

            return count > 0;
        }

        public async Task SoftDeleteAirplaneAsync(int airplaneId, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        "UPDATE dbo.[route] SET is_deleted = 1 WHERE airplane_id = @Id;",
                        new { Id = airplaneId }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        "UPDATE dbo.airplane SET is_deleted = 1 WHERE id = @Id;",
                        new { Id = airplaneId }, transaction, cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task HardDeleteAirplaneAsync(int airplaneId, CancellationToken cancellationToken)
        {
            const string sql = "DELETE FROM dbo.airplane WHERE id = @Id;";

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                new CommandDefinition(sql, new { Id = airplaneId }, cancellationToken: cancellationToken));
        }
    }
}
