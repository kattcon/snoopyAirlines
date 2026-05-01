using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.External.Domain;

namespace SnoopyAirlines.External.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<ApiUser?> GetByApiKey(
            string apiKey,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    id AS Id,
                    api_key AS ApiKey,
                    name AS Name,
                    role AS Role
                FROM api_user
                WHERE api_key = @ApiKey;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<ApiUser>(
                new CommandDefinition(sql, new { ApiKey = apiKey }, cancellationToken: cancellationToken));
        }

        public async Task<ApiUser> Save(
            ApiUser user,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                IF @Id > 0 AND EXISTS (SELECT 1 FROM api_user WHERE id = @Id)
                BEGIN
                    UPDATE api_user
                    SET
                        api_key = @ApiKey,
                        name = @Name,
                        role = @Role
                    OUTPUT
                        INSERTED.id AS Id,
                        INSERTED.api_key AS ApiKey,
                        INSERTED.name AS Name,
                        INSERTED.role AS Role
                    WHERE id = @Id;
                END
                ELSE
                BEGIN
                    INSERT INTO api_user (
                        api_key,
                        name,
                        role
                    )
                    OUTPUT
                        INSERTED.id AS Id,
                        INSERTED.api_key AS ApiKey,
                        INSERTED.name AS Name,
                        INSERTED.role AS Role
                    VALUES (
                        @ApiKey,
                        @Name,
                        @Role
                    );
                END
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<ApiUser>(
                new CommandDefinition(sql, user, cancellationToken: cancellationToken));
        }
    }
}
