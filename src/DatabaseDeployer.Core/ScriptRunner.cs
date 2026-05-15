using Microsoft.Data.SqlClient;

namespace DatabaseDeployer.Core
{
    public class ScriptRunner
    {
        private readonly DeploymentOptions _options;
        private readonly ScriptTracker _tracker;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ScriptRunner));

        public ScriptRunner(DeploymentOptions options, ScriptTracker tracker)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        }

        public async Task<int> RunScriptsAsync(string scriptsPath, bool isPreDeploy, bool trackExecution = true)
        {
            if (string.IsNullOrWhiteSpace(scriptsPath) || !Directory.Exists(scriptsPath))
            {
                _log.Info($"No {(isPreDeploy ? "pre-deploy" : "post-deploy")} scripts directory found at: {scriptsPath}");
                return 0;
            }

            // Check if this is a structured folder (has 'up' and 'down' subfolders)
            var upPath = Path.Combine(scriptsPath, "up");
            var downPath = Path.Combine(scriptsPath, "down");
            var hasStructuredFolders = Directory.Exists(upPath) || Directory.Exists(downPath);

            if (hasStructuredFolders)
            {
                return await RunStructuredScriptsAsync(scriptsPath, upPath, downPath, isPreDeploy, trackExecution);
            }
            else
            {
                // Legacy mode: scripts directly in the folder
                return await RunLegacyScriptsAsync(scriptsPath, isPreDeploy, trackExecution);
            }
        }

        private async Task<int> RunStructuredScriptsAsync(string scriptsPath, string upPath, string downPath, bool isPreDeploy, bool trackExecution)
        {
            var isUpgrade = _options.Mode == DeploymentMode.Upgrade;
            var activePath = isUpgrade ? upPath : downPath;
            var modeName = isUpgrade ? "upgrade" : "downgrade";
            var phaseType = isPreDeploy ? "pre-deploy" : "post-deploy";

            if (!Directory.Exists(activePath))
            {
                _log.Info($"No {phaseType} {modeName} scripts directory found at: {activePath}");
                return 0;
            }

            var sqlFiles = Directory.GetFiles(activePath, "*.sql", SearchOption.AllDirectories)
                                    .OrderBy(f => f)
                                    .ToArray();

            // CRITICAL: Reverse order for downgrade
            if (!isUpgrade)
            {
                Array.Reverse(sqlFiles);
                _log.Info($"Executing {phaseType} {modeName} scripts in REVERSE order");
            }

            if (sqlFiles.Length == 0)
            {
                _log.Info($"No {phaseType} {modeName} scripts found in: {activePath}");
                return 0;
            }

            _log.Info($"Found {sqlFiles.Length} {phaseType} {modeName} script(s) to process");

            var executedCount = 0;

            foreach (var sqlFile in sqlFiles)
            {
                var filename = Path.GetFileName(sqlFile);
                var scriptIdentifier = $"{(isPreDeploy ? "PRE" : "POST")}:{filename}";

                // Read upgrade script
                var upgradeScriptPath = Path.Combine(upPath, filename);
                string? upgradeScript = null;
                if (File.Exists(upgradeScriptPath))
                {
                    upgradeScript = await File.ReadAllTextAsync(upgradeScriptPath);
                }

                // Read downgrade script (optional)
                var downgradeScriptPath = Path.Combine(downPath, filename);
                string? downgradeScript = null;
                if (File.Exists(downgradeScriptPath))
                {
                    downgradeScript = await File.ReadAllTextAsync(downgradeScriptPath);
                }

                // Determine which script to execute
                var scriptToExecute = isUpgrade ? upgradeScript : downgradeScript;

                if (scriptToExecute == null)
                {
                    _log.Warn($"No {modeName} script found for: {filename}, skipping");
                    continue;
                }

                if (trackExecution)
                {
                    var alreadyExecuted = isUpgrade
                        ? await _tracker.HasScriptBeenExecutedAsync(scriptIdentifier)
                        : !await _tracker.HasScriptBeenExecutedAsync(scriptIdentifier); // For downgrade, skip if NOT executed

                    if (alreadyExecuted)
                    {
                        _log.Info($"Skipping {(isUpgrade ? "already executed" : "not previously executed")} script: {filename}");
                        continue;
                    }
                }

                _log.Info($"Executing {phaseType} {modeName} script: {filename}");

                try
                {
                    if (trackExecution && isUpgrade)
                    {
                        // Register both upgrade and downgrade scripts together
                        await _tracker.RegisterScriptAsync(scriptIdentifier, upgradeScript!, downgradeScript, isPreDeploy);
                    }

                    await ExecuteSqlScriptAsync(scriptToExecute);

                    if (trackExecution)
                    {
                        if (isUpgrade)
                        {
                            await _tracker.MarkScriptExecutedAsync(scriptIdentifier);
                        }
                        else
                        {
                            // For downgrade, mark as NOT executed (reset the flag)
                            await _tracker.MarkScriptNotExecutedAsync(scriptIdentifier);
                        }
                    }

                    executedCount++;
                    _log.Info($"Successfully executed: {filename}");
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to execute script: {filename}", ex);

                    if (!_options.ContinueOnError)
                    {
                        throw new InvalidOperationException($"Script execution failed: {filename}", ex);
                    }
                }
            }

            _log.Info($"Completed {phaseType} {modeName} scripts: {executedCount} executed, {sqlFiles.Length - executedCount} skipped");
            return executedCount;
        }

        private async Task<int> RunLegacyScriptsAsync(string scriptsPath, bool isPreDeploy, bool trackExecution)
        {
            var sqlFiles = Directory.GetFiles(scriptsPath, "*.sql", SearchOption.AllDirectories)
                                    .OrderBy(f => f)
                                    .ToArray();

            if (sqlFiles.Length == 0)
            {
                _log.Info($"No {(isPreDeploy ? "pre-deploy" : "post-deploy")} scripts found in: {scriptsPath}");
                return 0;
            }

            _log.Info($"Found {sqlFiles.Length} {(isPreDeploy ? "pre-deploy" : "post-deploy")} script(s) to process");

            var executedCount = 0;

            foreach (var sqlFile in sqlFiles)
            {
                var filename = Path.GetFileName(sqlFile);
                var scriptIdentifier = $"{(isPreDeploy ? "PRE" : "POST")}:{filename}";

                if (trackExecution && await _tracker.HasScriptBeenExecutedAsync(scriptIdentifier))
                {
                    _log.Info($"Skipping already executed script: {filename}");
                    continue;
                }

                _log.Info($"Executing {(isPreDeploy ? "pre-deploy" : "post-deploy")} script: {filename}");

                try
                {
                    var sqlContent = await File.ReadAllTextAsync(sqlFile);

                    if (trackExecution)
                    {
                        await _tracker.RegisterScriptAsync(scriptIdentifier, sqlContent, downgradeScript: null, isPreDeploy);
                    }

                    await ExecuteSqlScriptAsync(sqlContent);

                    if (trackExecution)
                    {
                        await _tracker.MarkScriptExecutedAsync(scriptIdentifier);
                    }

                    executedCount++;
                    _log.Info($"Successfully executed: {filename}");
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to execute script: {filename}", ex);

                    if (!_options.ContinueOnError)
                    {
                        throw new InvalidOperationException($"Script execution failed: {filename}", ex);
                    }
                }
            }

            _log.Info($"Completed {(isPreDeploy ? "pre-deploy" : "post-deploy")} scripts: {executedCount} executed, {sqlFiles.Length - executedCount} skipped");
            return executedCount;
        }

        private async Task ExecuteSqlScriptAsync(string sqlScript)
        {
            await using var connection = new SqlConnection(_options.ConnectionString);
            await connection.OpenAsync();

            var batches = SplitSqlIntoBatches(sqlScript);

            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                await using var command = new SqlCommand(batch, connection)
                {
                    CommandTimeout = _options.CommandTimeout
                };

                await command.ExecuteNonQueryAsync();
            }
        }

        private static string[] SplitSqlIntoBatches(string sqlScript)
        {
            return sqlScript.Split(new[] { "\r\nGO\r\n", "\nGO\n", "\r\nGO ", "\nGO " }, 
                                   StringSplitOptions.RemoveEmptyEntries);
        }

        public int CountUpgradeScripts(string? preDeployPath, string? postDeployPath)
        {
            var count = 0;

            if (!string.IsNullOrWhiteSpace(preDeployPath))
            {
                var preUpPath = Path.Combine(preDeployPath, "up");
                if (Directory.Exists(preUpPath))
                {
                    count += Directory.GetFiles(preUpPath, "*.sql", SearchOption.AllDirectories).Length;
                }
                else if (Directory.Exists(preDeployPath))
                {
                    // Legacy mode - count scripts directly in folder
                    count += Directory.GetFiles(preDeployPath, "*.sql", SearchOption.AllDirectories).Length;
                }
            }

            if (!string.IsNullOrWhiteSpace(postDeployPath))
            {
                var postUpPath = Path.Combine(postDeployPath, "up");
                if (Directory.Exists(postUpPath))
                {
                    count += Directory.GetFiles(postUpPath, "*.sql", SearchOption.AllDirectories).Length;
                }
                else if (Directory.Exists(postDeployPath))
                {
                    // Legacy mode - count scripts directly in folder
                    count += Directory.GetFiles(postDeployPath, "*.sql", SearchOption.AllDirectories).Length;
                }
            }

            return count;
        }
    }
}
