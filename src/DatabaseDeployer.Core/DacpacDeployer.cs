using Microsoft.SqlServer.Dac;
using System.Diagnostics;

namespace DatabaseDeployer.Core
{
    public class DacpacDeployer
    {
        private readonly DeploymentOptions _options;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(DacpacDeployer));

        public DacpacDeployer(DeploymentOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<bool> DeployAsync()
        {
            if (string.IsNullOrWhiteSpace(_options.DacpacPath))
            {
                _log.Info("No DACPAC path specified, skipping DACPAC deployment");
                return false;
            }

            if (!File.Exists(_options.DacpacPath))
            {
                throw new FileNotFoundException($"DACPAC file not found: {_options.DacpacPath}");
            }

            _log.Info($"Starting DACPAC deployment: {_options.DacpacPath}");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var dacServices = new DacServices(_options.ConnectionString);

                dacServices.Message += (sender, args) =>
                {
                    if (_options.Verbose)
                    {
                        _log.Info($"[DacFx] {args.Message}");
                    }
                };

                dacServices.ProgressChanged += (sender, args) =>
                {
                    if (_options.Verbose)
                    {
                        _log.Debug($"Progress: {args.Status} - {args.Message}");
                    }
                };

                using var dacPackage = DacPackage.Load(_options.DacpacPath);

                var deployOptions = new DacDeployOptions
                {
                    BlockOnPossibleDataLoss = _options.BlockOnPossibleDataLoss,
                    DropObjectsNotInSource = _options.DropObjectsNotInSource,
                    IgnorePermissions = _options.IgnorePermissions,
                    IgnoreRoleMembership = _options.IgnoreRoleMembership,
                    IgnoreUserSettingsObjects = _options.IgnoreUserSettingsObjects,
                    BackupDatabaseBeforeChanges = _options.BackupDatabaseBeforeChanges,
                    CommandTimeout = _options.CommandTimeout,
                    GenerateSmartDefaults = true,
                    CreateNewDatabase = false
                };

                var databaseName = GetDatabaseNameFromConnectionString(_options.ConnectionString);

                if (_options.GenerateDeploymentScript && !string.IsNullOrWhiteSpace(_options.DeploymentScriptPath))
                {
                    _log.Info($"Generating deployment script to: {_options.DeploymentScriptPath}");
                    var script = dacServices.GenerateDeployScript(dacPackage, databaseName, deployOptions);
                    
                    var scriptDirectory = Path.GetDirectoryName(_options.DeploymentScriptPath);
                    if (!string.IsNullOrEmpty(scriptDirectory) && !Directory.Exists(scriptDirectory))
                    {
                        Directory.CreateDirectory(scriptDirectory);
                    }

                    await File.WriteAllTextAsync(_options.DeploymentScriptPath, script);
                    _log.Info("Deployment script generated successfully");
                }

                await Task.Run(() => dacServices.Deploy(dacPackage, databaseName, upgradeExisting: true, options: deployOptions));

                stopwatch.Stop();
                _log.Info($"DACPAC deployment completed successfully in {stopwatch.ElapsedMilliseconds}ms");
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _log.Error($"DACPAC deployment failed after {stopwatch.ElapsedMilliseconds}ms", ex);
                throw new InvalidOperationException("DACPAC deployment failed", ex);
            }
        }

        private static string GetDatabaseNameFromConnectionString(string connectionString)
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }
    }
}
