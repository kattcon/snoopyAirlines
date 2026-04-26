using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<UserView>> GetAllAsync(CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    combined.id AS Id,
                    combined.identification_number AS IdentificationNumber,
                    combined.email AS Email,
                    combined.first_name AS FirstName,
                    combined.last_name_one AS LastNameOne,
                    combined.last_name_two AS LastNameTwo,
                    CASE combined.type
                        WHEN 'AD' THEN CAST(0 AS INT)
                        WHEN 'OP' THEN CAST(1 AS INT)
                    END AS Type,
                    combined.pending AS Pending
                FROM (
                    SELECT
                        id,
                        identification_number,
                        email,
                        first_name,
                        last_name_one,
                        last_name_two,
                        type,
                        CAST(0 AS BIT) AS pending
                    FROM dbo.[user]

                    UNION ALL

                    SELECT
                        id,
                        identification_number,
                        email,
                        first_name,
                        last_name_one,
                        last_name_two,
                        type,
                        CAST(1 AS BIT) AS pending
                    FROM pending_user
                ) AS combined
                ORDER BY
                    combined.pending DESC,
                    combined.first_name,
                    combined.last_name_one,
                    combined.last_name_two,
                    combined.email;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var users = await connection.QueryAsync<UserView>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return users.ToList();
        }

        public async Task<PendingUser> SaveAsync(
            PendingUser pendingUser,
            CancellationToken cancellationToken)
        {
            const string sql = """
                IF @Id > 0 AND EXISTS (SELECT 1 FROM pending_user WHERE id = @Id)
                BEGIN
                    UPDATE pending_user
                    SET
                        identification_number = @IdentificationNumber,
                        email = @Email,
                        first_name = @FirstName,
                        last_name_one = @LastNameOne,
                        last_name_two = @LastNameTwo,
                        type = @TypeCode
                    OUTPUT
                        INSERTED.id AS Id,
                        INSERTED.identification_number AS IdentificationNumber,
                        INSERTED.email AS Email,
                        INSERTED.first_name AS FirstName,
                        INSERTED.last_name_one AS LastNameOne,
                        INSERTED.last_name_two AS LastNameTwo,
                        CASE INSERTED.type
                            WHEN 'AD' THEN CAST(0 AS INT)
                            WHEN 'OP' THEN CAST(1 AS INT)
                        END AS Type
                    WHERE id = @Id;
                END
                ELSE
                BEGIN
                    INSERT INTO pending_user (
                        identification_number,
                        email,
                        first_name,
                        last_name_one,
                        last_name_two,
                        type
                    )
                    OUTPUT
                        INSERTED.id AS Id,
                        INSERTED.identification_number AS IdentificationNumber,
                        INSERTED.email AS Email,
                        INSERTED.first_name AS FirstName,
                        INSERTED.last_name_one AS LastNameOne,
                        INSERTED.last_name_two AS LastNameTwo,
                        CASE INSERTED.type
                            WHEN 'AD' THEN CAST(0 AS INT)
                            WHEN 'OP' THEN CAST(1 AS INT)
                        END AS Type
                    VALUES (
                        @IdentificationNumber,
                        @Email,
                        @FirstName,
                        @LastNameOne,
                        @LastNameTwo,
                        @TypeCode
                    );
                END
                """;

            var parameters = new
            {
                pendingUser.Id,
                pendingUser.IdentificationNumber,
                pendingUser.Email,
                pendingUser.FirstName,
                pendingUser.LastNameOne,
                pendingUser.LastNameTwo,
                TypeCode = MapRoleToDatabaseCode(pendingUser.Type)
            };

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<PendingUser>(
                new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        }

        private static string MapRoleToDatabaseCode(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "AD",
                UserRole.Operator => "OP",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unsupported user role.")
            };
        }

        public async Task<User?> GetByCredentialsAsync(string email, string password, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    id as Id,
                    identification_number as IdentificationNumber,
                    email as Email,
                    first_name as FirstName,
                    last_name_one as LastNameOne,
                    last_name_two as LastNameTwo,
                    CASE type
                        WHEN 'AD' THEN CAST(0 AS INT)
                        WHEN 'OP' THEN CAST(1 AS INT)
                    END AS Type,
                    password_hash as PasswordHash
                    password_salt as PasswordSalt
                FROM dbo.[user]
                WHERE email = @Email;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<User>(
                new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));    
        }

    }
}
