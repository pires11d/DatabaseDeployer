using Microsoft.Data.SqlClient;
using System.Reflection;

namespace DatabaseDeployer.Core
{
    public class ScriptTracker
    {
        private readonly string _connectionString;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ScriptTracker));

        public ScriptTracker(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task EnsureTrackingTableExistsAsync()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "DatabaseDeployer.Core.CreateScriptsTable.sql";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    throw new InvalidOperationException($"Embedded resource '{resourceName}' not found");
                }

                using var reader = new StreamReader(stream);
                var createTableSql = await reader.ReadToEndAsync();

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(createTableSql, connection)
                {
                    CommandTimeout = 60
                };

                await command.ExecuteNonQueryAsync();
                _log.Info("Script tracking table verified/created");
            }
            catch (Exception ex)
            {
                _log.Error("Failed to ensure tracking table exists", ex);
                throw;
            }
        }

        public async Task<bool> HasScriptBeenExecutedAsync(string filename)
        {
            try
            {
                const string checkSql = @"
                    SELECT COUNT(1) 
                    FROM dbo.Scripts 
                    WHERE Filename = UPPER(@Filename) 
                    AND Executed = 1";

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(checkSql, connection);
                command.Parameters.AddWithValue("@Filename", filename.ToUpper());

                var result = await command.ExecuteScalarAsync();
                var count = result != null ? Convert.ToInt32(result) : 0;
                return count > 0;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to check if script has been executed: {filename}", ex);
                throw;
            }
        }

        public async Task RegisterScriptAsync(string filename, string upgradeScript, string? downgradeScript, bool isPreDeploy)
        {
            try
            {
                const string insertSql = @"
                    IF NOT EXISTS (SELECT 1 FROM dbo.Scripts WHERE Filename = UPPER(@Filename))
                    BEGIN
                        INSERT INTO dbo.Scripts (Id, Filename, UpgradeScript, DowngradeScript, PreDeploy, Executed)
                        VALUES (NEWID(), UPPER(@Filename), @UpgradeScript, @DowngradeScript, @PreDeploy, 0)
                    END";

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(insertSql, connection);
                command.Parameters.AddWithValue("@Filename", filename.ToUpper());
                command.Parameters.AddWithValue("@UpgradeScript", upgradeScript);
                command.Parameters.AddWithValue("@DowngradeScript", (object?)downgradeScript ?? DBNull.Value);
                command.Parameters.AddWithValue("@PreDeploy", isPreDeploy);

                await command.ExecuteNonQueryAsync();
                _log.Debug($"Registered script: {filename}");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to register script: {filename}", ex);
                throw;
            }
        }

        public async Task MarkScriptExecutedAsync(string filename)
        {
            try
            {
                const string updateSql = @"
                    UPDATE dbo.Scripts 
                    SET Executed = 1, ExecutedDate = GETUTCDATE()
                    WHERE Filename = UPPER(@Filename)";

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(updateSql, connection);
                command.Parameters.AddWithValue("@Filename", filename.ToUpper());

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    _log.Debug($"Marked script as executed: {filename}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to mark script as executed: {filename}", ex);
                throw;
            }
        }

        public async Task MarkScriptNotExecutedAsync(string filename)
        {
            try
            {
                const string updateSql = @"
                    UPDATE dbo.Scripts 
                    SET Executed = 0, ExecutedDate = NULL
                    WHERE Filename = UPPER(@Filename)";

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(updateSql, connection);
                command.Parameters.AddWithValue("@Filename", filename.ToUpper());

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    _log.Debug($"Marked script as NOT executed (downgrade): {filename}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to mark script as not executed: {filename}", ex);
                throw;
            }
        }

        public async Task<List<ScriptRecord>> GetPendingScriptsAsync(bool preDeployOnly = false)
        {
            try
            {
                var sql = preDeployOnly
                    ? "SELECT * FROM dbo.Scripts WHERE Executed = 0 AND PreDeploy = 1 ORDER BY Filename"
                    : "SELECT * FROM dbo.Scripts WHERE Executed = 0 ORDER BY PreDeploy DESC, Filename";

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync();

                var scripts = new List<ScriptRecord>();

                while (await reader.ReadAsync())
                {
                    scripts.Add(new ScriptRecord
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Filename = reader.GetString(reader.GetOrdinal("Filename")),
                        UpgradeScript = reader.GetString(reader.GetOrdinal("UpgradeScript")),
                        DowngradeScript = reader.IsDBNull(reader.GetOrdinal("DowngradeScript"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("DowngradeScript")),
                        PreDeploy = reader.GetBoolean(reader.GetOrdinal("PreDeploy")),
                        Executed = reader.GetBoolean(reader.GetOrdinal("Executed"))
                    });
                }

                return scripts;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to get pending scripts", ex);
                throw;
            }
        }

        public async Task<int> GetExecutedScriptCountAsync()
        {
            try
            {
                const string countSql = "SELECT COUNT(*) FROM dbo.Scripts WHERE Executed = 1";

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(countSql, connection);
                var result = await command.ExecuteScalarAsync();
                
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to get executed script count", ex);
                throw;
            }
        }
    }

    public record ScriptRecord
    {
        public Guid Id { get; init; }
        public required string Filename { get; init; }
        public required string UpgradeScript { get; init; }
        public string? DowngradeScript { get; init; }
        public bool PreDeploy { get; init; }
        public bool Executed { get; init; }
    }
}
