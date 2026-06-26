using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Domain.Intake;

namespace SnoopyAirlines.Repositories
{
    public class UserRepository : IUserRepository
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
            if (string.IsNullOrWhiteSpace(pendingUser.RegistrationKeyHash))
            {
                throw new ArgumentException(
                    "Pending user registration key hash is required.",
                    nameof(pendingUser));
            }

            const string sql = """
                IF EXISTS (SELECT 1 FROM dbo.pending_user WHERE email = @Email)
                BEGIN
                    UPDATE dbo.pending_user
                    SET
                        identification_number = @IdentificationNumber,
                        first_name = @FirstName,
                        last_name_one = @LastNameOne,
                        last_name_two = @LastNameTwo,
                        type = @TypeCode,
                        registration_key_hash = @RegistrationKeyHash
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
                        END AS Type,
                        INSERTED.registration_key_hash AS RegistrationKeyHash,
                        INSERTED.created_at AS CreatedAt
                    WHERE email = @Email;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.pending_user (
                        identification_number,
                        email,
                        first_name,
                        last_name_one,
                        last_name_two,
                        type,
                        registration_key_hash
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
                        END AS Type,
                        INSERTED.registration_key_hash AS RegistrationKeyHash,
                        INSERTED.created_at AS CreatedAt
                    VALUES (
                        @IdentificationNumber,
                        @Email,
                        @FirstName,
                        @LastNameOne,
                        @LastNameTwo,
                        @TypeCode,
                        @RegistrationKeyHash
                    );
                END
                """;

            var parameters = new
            {
                pendingUser.IdentificationNumber,
                pendingUser.Email,
                pendingUser.FirstName,
                pendingUser.LastNameOne,
                pendingUser.LastNameTwo,
                TypeCode = MapRoleToDatabaseCode(pendingUser.Type),
                pendingUser.RegistrationKeyHash
            };

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<PendingUser>(
                new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        }

        public async Task<UserView> SaveAsync(
            User user,
            CancellationToken cancellationToken)
        {
            const string sql = """
                IF EXISTS (SELECT 1 FROM dbo.[user] WHERE email = @Email)
                BEGIN
                    UPDATE dbo.[user]
                    SET
                        identification_number = @IdentificationNumber,
                        first_name = @FirstName,
                        last_name_one = @LastNameOne,
                        last_name_two = @LastNameTwo,
                        type = @TypeCode,
                        password_hash = CASE
                            WHEN @PasswordHash IS NULL OR @PasswordHash = '' THEN password_hash
                            ELSE @PasswordHash
                        END,
                        password_salt = CASE
                            WHEN @PasswordSalt IS NULL OR @PasswordSalt = '' THEN password_salt
                            ELSE @PasswordSalt
                        END
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
                        END AS Type,
                        CAST(0 AS BIT) AS Pending
                    WHERE email = @Email;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.[user] (
                        identification_number,
                        email,
                        first_name,
                        last_name_one,
                        last_name_two,
                        type,
                        password_hash,
                        password_salt
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
                        END AS Type,
                        CAST(0 AS BIT) AS Pending
                    VALUES (
                        @IdentificationNumber,
                        @Email,
                        @FirstName,
                        @LastNameOne,
                        @LastNameTwo,
                        @TypeCode,
                        @PasswordHash,
                        @PasswordSalt
                    );
                END
                """;

            var parameters = new
            {
                user.IdentificationNumber,
                user.Email,
                user.FirstName,
                user.LastNameOne,
                user.LastNameTwo,
                TypeCode = MapRoleToDatabaseCode(user.Type),
                user.PasswordHash,
                user.PasswordSalt
            };

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<UserView>(
                new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT CASE
                    WHEN EXISTS (SELECT 1 FROM dbo.[user] WHERE email = @Email) THEN CAST(1 AS BIT)
                    ELSE CAST(0 AS BIT)
                END;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<bool>(
                new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));
        }

        public async Task<bool> ExistsPendingByEmailAsync(string email, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT CASE
                    WHEN EXISTS (SELECT 1 FROM dbo.pending_user WHERE email = @Email) THEN CAST(1 AS BIT)
                    ELSE CAST(0 AS BIT)
                END;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<bool>(
                new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));
        }

        public async Task<PendingUser?> GetPendingByRegistrationKeyHashAsync(
            string registrationKeyHash,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    id AS Id,
                    identification_number AS IdentificationNumber,
                    email AS Email,
                    first_name AS FirstName,
                    last_name_one AS LastNameOne,
                    last_name_two AS LastNameTwo,
                    CASE type
                        WHEN 'AD' THEN CAST(0 AS INT)
                        WHEN 'OP' THEN CAST(1 AS INT)
                    END AS Type,
                    registration_key_hash AS RegistrationKeyHash,
                    created_at AS CreatedAt
                FROM dbo.pending_user
                WHERE registration_key_hash = @RegistrationKeyHash;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<PendingUser>(
                new CommandDefinition(
                    sql,
                    new { RegistrationKeyHash = registrationKeyHash },
                    cancellationToken: cancellationToken));
        }

        public async Task<UserView> SaveUserAndDeletePendingAsync(
            User user,
            CancellationToken cancellationToken)
        {
            const string insertUserSql = """
                INSERT INTO dbo.[user] (
                    identification_number,
                    email,
                    first_name,
                    last_name_one,
                    last_name_two,
                    type,
                    password_hash,
                    password_salt
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
                    END AS Type,
                    CAST(0 AS BIT) AS Pending
                VALUES (
                    @IdentificationNumber,
                    @Email,
                    @FirstName,
                    @LastNameOne,
                    @LastNameTwo,
                    @TypeCode,
                    @PasswordHash,
                    @PasswordSalt
                );
                """;

            const string deletePendingSql = """
                DELETE FROM dbo.pending_user
                WHERE email = @Email;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var savedUser = await connection.QuerySingleAsync<UserView>(
                    new CommandDefinition(
                        insertUserSql,
                        new
                        {
                            user.IdentificationNumber,
                            user.Email,
                            user.FirstName,
                            user.LastNameOne,
                            user.LastNameTwo,
                            TypeCode = MapRoleToDatabaseCode(user.Type),
                            user.PasswordHash,
                            user.PasswordSalt
                        },
                        transaction,
                        cancellationToken: cancellationToken));

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        deletePendingSql,
                        new { user.Email },
                        transaction,
                        cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);

                return savedUser;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
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
                    [password_hash] as PasswordHash,
                    [password_salt] as PasswordSalt
                FROM dbo.[user]
                WHERE email = @Email;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<User>(
                new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));    
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    id AS Id,
                    identification_number AS IdentificationNumber,
                    email AS Email,
                    first_name AS FirstName,
                    last_name_one AS LastNameOne,
                    last_name_two AS LastNameTwo,
                    CASE type
                        WHEN 'AD' THEN CAST(0 AS INT)
                        WHEN 'OP' THEN CAST(1 AS INT)
                    END AS Type,
                    password_hash AS PasswordHash,
                    password_salt AS PasswordSalt
                FROM dbo.[user]
                WHERE id = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<User>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        }

        public async Task<UserView> UpdateAsync(int id, UserUpdateIntake intake, CancellationToken cancellationToken)
        {
            const string sql = """
                UPDATE dbo.[user]
                SET
                    first_name = @FirstName,
                    last_name_one = @LastNameOne,
                    last_name_two = @LastNameTwo
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
                    END AS Type,
                    CAST(0 AS BIT) AS Pending
                WHERE id = @Id;
                """;

            var parameters = new
            {
                Id = id,
                intake.FirstName,
                intake.LastNameOne,
                intake.LastNameTwo
            };

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<UserView>(
                new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        }

        public async Task UpdatePasswordAsync(int id, string passwordHash, string passwordSalt, CancellationToken cancellationToken)
        {
            const string sql = """
                UPDATE dbo.[user]
                SET
                    password_hash = @PasswordHash,
                    password_salt = @PasswordSalt
                WHERE id = @Id;
                """;

            var parameters = new { Id = id, PasswordHash = passwordHash, PasswordSalt = passwordSalt };

            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        }
        public async Task<UserView?> AdminUpdateAsync(int id, AdminUserUpdateIntake intake, CancellationToken cancellationToken)
        {
            const string sql = """
                UPDATE dbo.[user]
                SET
                    first_name = @FirstName,
                    last_name_one = @LastNameOne,
                    last_name_two = @LastNameTwo,
                    identification_number = @IdentificationNumber,
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
                    END AS Type,
                    CAST(0 AS BIT) AS Pending
                WHERE id = @Id;
                """;

            var typeCode = intake.Type.ToUpper() switch
            {
                "AD" => "AD",
                "OP" => "OP",
                _ => throw new ArgumentException($"Invalid user type: {intake.Type}.", nameof(intake))
            };

            var parameters = new
            {
                Id = id,
                intake.FirstName,
                intake.LastNameOne,
                intake.LastNameTwo,
                intake.IdentificationNumber,
                TypeCode = typeCode
            };

            await using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<UserView>(
                new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        }

        public async Task DeleteUserAsync(int actorUserId, int targetUserId, CancellationToken cancellationToken)
        {
            const string sql = """
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;

                IF @ActorUserId = @TargetUserId
                BEGIN
                    THROW 50000, 'You cannot delete your own user.', 1;
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM dbo.[user] WITH (UPDLOCK, HOLDLOCK)
                    WHERE id = @ActorUserId
                      AND type = 'AD'
                )
                BEGIN
                    THROW 50000, 'Only admin users can delete users.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.[user] WITH (UPDLOCK, HOLDLOCK)
                    WHERE id = @TargetUserId
                      AND email = 'admin@snoopyairlines.com'
                )
                BEGIN
                    THROW 50000, 'The initial admin user cannot be deleted.', 1;
                END;

                DELETE FROM dbo.[user]
                WHERE id = @TargetUserId;

                IF @@ROWCOUNT = 0
                BEGIN
                    THROW 50000, 'User not found.', 1;
                END;

                COMMIT TRANSACTION;
                """;

            await using var connection = new SqlConnection(_connectionString);

            try
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        sql,
                        new
                        {
                            ActorUserId = actorUserId,
                            TargetUserId = targetUserId
                        },
                        cancellationToken: cancellationToken));
            }
            catch (SqlException exception) when (exception.Number == 50000)
            {
                throw new InvalidOperationException(exception.Message, exception);
            }
        }
    }
}
