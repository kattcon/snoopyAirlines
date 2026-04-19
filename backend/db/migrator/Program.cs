using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

const string SetupMigrationId = "V0_migration-setup.sql";

var options = ParseArguments(args);

if (!Directory.Exists(options.MigrationsDirectory))
{
    throw new DirectoryNotFoundException(
        $"Migrations directory was not found: {options.MigrationsDirectory}");
}

var migrationFiles = Directory
    .EnumerateFiles(options.MigrationsDirectory, "*.sql")
    .Where(migrationPath => !IsSetupMigration(migrationPath))
    .OrderBy(GetMigrationVersion)
    .ThenBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
    .ToArray();

Console.WriteLine($"Migrations: {options.MigrationsDirectory}");
Console.WriteLine($"Database: {DescribeDatabase(options.ConnectionString)}");

await using var connection = new SqlConnection(options.ConnectionString);
await connection.OpenAsync();

var schemaMigrationExists = await SchemaMigrationTableExistsAsync(connection);
if (!schemaMigrationExists)
{
    Console.WriteLine("Migration table was not found. Applying built-in V0 setup migration.");
    await ApplySetupMigrationAsync(connection);
}

var appliedMigrations = await LoadAppliedMigrationsAsync(connection);

foreach (var migrationFile in migrationFiles)
{
    var migrationId = Path.GetFileName(migrationFile);

    if (appliedMigrations.Contains(migrationId))
    {
        Console.WriteLine($"Skipping {migrationId}");
        continue;
    }

    Console.WriteLine($"Applying {migrationId}");
    await ApplyMigrationAsync(connection, migrationFile, migrationId);
    appliedMigrations.Add(migrationId);
}

Console.WriteLine("Database is up to date.");

static MigratorOptions ParseArguments(string[] args)
{
    string? migrationsDirectory = null;
    string? connectionString = null;

    for (var index = 0; index < args.Length; index++)
    {
        var argument = args[index];

        if (argument.Equals("--migrations-directory", StringComparison.OrdinalIgnoreCase)
            || argument.Equals("--migrations-dir", StringComparison.OrdinalIgnoreCase)
            || argument.Equals("-m", StringComparison.OrdinalIgnoreCase))
        {
            if (index + 1 >= args.Length)
            {
                throw new InvalidOperationException("Missing value for --migrations-directory.");
            }

            migrationsDirectory = args[++index];
            continue;
        }

        if (argument.StartsWith("--migrations-directory=", StringComparison.OrdinalIgnoreCase))
        {
            migrationsDirectory = argument["--migrations-directory=".Length..];
            continue;
        }

        if (argument.StartsWith("--migrations-dir=", StringComparison.OrdinalIgnoreCase))
        {
            migrationsDirectory = argument["--migrations-dir=".Length..];
            continue;
        }

        if (argument.StartsWith("-m=", StringComparison.OrdinalIgnoreCase))
        {
            migrationsDirectory = argument["-m=".Length..];
            continue;
        }

        if (argument.Equals("--connection-string", StringComparison.OrdinalIgnoreCase)
            || argument.Equals("-c", StringComparison.OrdinalIgnoreCase))
        {
            if (index + 1 >= args.Length)
            {
                throw new InvalidOperationException("Missing value for --connection-string.");
            }

            connectionString = args[++index];
            continue;
        }

        if (argument.StartsWith("--connection-string=", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = argument["--connection-string=".Length..];
            continue;
        }

        if (argument.StartsWith("-c=", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = argument["-c=".Length..];
            continue;
        }

        throw new InvalidOperationException($"Unknown argument: {argument}");
    }

    if (string.IsNullOrWhiteSpace(migrationsDirectory) || string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Usage: dotnet run --project backend/db/migration/migrator/SnoopyAirlines.DbMigrator.csproj -- --migrations-directory <path> --connection-string <connection-string>");
    }

    return new MigratorOptions(
        Path.GetFullPath(migrationsDirectory),
        connectionString);
}

static async Task<bool> SchemaMigrationTableExistsAsync(SqlConnection connection)
{
    const string sql = """
        SELECT CASE
            WHEN OBJECT_ID(N'dbo.schema_migration', N'U') IS NULL THEN 0
            ELSE 1
        END;
        """;

    await using var command = new SqlCommand(sql, connection);
    var result = await command.ExecuteScalarAsync();

    return Convert.ToInt32(result) == 1;
}

static async Task<HashSet<string>> LoadAppliedMigrationsAsync(SqlConnection connection)
{
    const string sql = "SELECT id FROM dbo.schema_migration;";

    await using var command = new SqlCommand(sql, connection);
    await using var reader = await command.ExecuteReaderAsync();

    var migrations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    while (await reader.ReadAsync())
    {
        migrations.Add(reader.GetString(0));
    }

    return migrations;
}

static async Task ApplySetupMigrationAsync(SqlConnection connection)
{
    const string createTableSql = """
        CREATE TABLE dbo.schema_migration (
            id NVARCHAR(255) NOT NULL PRIMARY KEY,
            applied_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
        );
        """;

    await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

    try
    {
        await using var createTableCommand = new SqlCommand(createTableSql, connection, transaction);
        await createTableCommand.ExecuteNonQueryAsync();

        await using var recordCommand = new SqlCommand(
            "INSERT INTO dbo.schema_migration (id) VALUES (@id);",
            connection,
            transaction);
        recordCommand.Parameters.AddWithValue("@id", SetupMigrationId);
        await recordCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

static async Task ApplyMigrationAsync(SqlConnection connection, string migrationFile, string migrationId)
{
    var script = await File.ReadAllTextAsync(migrationFile);
    var batches = SplitSqlBatches(script);

    await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

    try
    {
        foreach (var batch in batches)
        {
            await using var command = new SqlCommand(batch, connection, transaction);
            await command.ExecuteNonQueryAsync();
        }

        await using var recordCommand = new SqlCommand(
            "INSERT INTO dbo.schema_migration (id) VALUES (@id);",
            connection,
            transaction);
        recordCommand.Parameters.AddWithValue("@id", migrationId);
        await recordCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

static IReadOnlyCollection<string> SplitSqlBatches(string script)
{
    var batches = new List<string>();
    var currentBatch = new List<string>();

    foreach (var line in script.ReplaceLineEndings("\n").Split('\n'))
    {
        if (Regex.IsMatch(line, @"^\s*GO\s*;?\s*$", RegexOptions.IgnoreCase))
        {
            AddBatchIfNotEmpty();
            continue;
        }

        currentBatch.Add(line);
    }

    AddBatchIfNotEmpty();
    return batches;

    void AddBatchIfNotEmpty()
    {
        var batch = string.Join(Environment.NewLine, currentBatch).Trim();
        if (batch.Length > 0)
        {
            batches.Add(batch);
        }

        currentBatch.Clear();
    }
}

static int GetMigrationVersion(string migrationPath)
{
    var migrationName = Path.GetFileNameWithoutExtension(migrationPath);
    var match = Regex.Match(migrationName, @"^V(?<version>\d+)_", RegexOptions.IgnoreCase);

    return match.Success
        ? int.Parse(match.Groups["version"].Value)
        : int.MaxValue;
}

static bool IsSetupMigration(string migrationPath)
{
    return Path.GetFileName(migrationPath)
        .Equals(SetupMigrationId, StringComparison.OrdinalIgnoreCase);
}

static string DescribeDatabase(string connectionString)
{
    var builder = new SqlConnectionStringBuilder(connectionString);
    return $"{builder.DataSource}/{builder.InitialCatalog}";
}

internal sealed record MigratorOptions(string MigrationsDirectory, string ConnectionString);
