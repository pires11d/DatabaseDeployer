using CommandLine;
using DatabaseDeployer.Core;
using log4net;
using log4net.Config;
using System.Reflection;

namespace DatabaseDeployer.Cli
{
    internal class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        static async Task<int> Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    return await InteractiveModeAsync();
                }

                var parser = new Parser(settings =>
                {
                    settings.HelpWriter = null;
                });

                var parseResult = parser.ParseArguments<CommandLineOptions>(args);

                return await parseResult.MapResult(
                    async options => await RunDeploymentAsync(options),
                    errors => Task.FromResult(HandleParseErrors(errors, args, parseResult))
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                return 1;
            }
        }

        private static async Task<int> InteractiveModeAsync()
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("                     DatabaseDeployer - Interactive Mode");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            
            ShowHelp();
            
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.Write("Enter your command (or 'exit' to quit): ");
            
            var input = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting...");
                return 0;
            }

            var inputArgs = ParseCommandLine(input);
            
            var parser = new Parser(settings =>
            {
                settings.HelpWriter = null;
            });

            var parseResult = parser.ParseArguments<CommandLineOptions>(inputArgs);

            var exitCode = await parseResult.MapResult(
                async options => await RunDeploymentAsync(options),
                errors => Task.FromResult(HandleParseErrors(errors, inputArgs, parseResult))
            );

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            
            return exitCode;
        }

        private static string[] ParseCommandLine(string commandLine)
        {
            var args = new List<string>();
            var currentArg = new System.Text.StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < commandLine.Length; i++)
            {
                char c = commandLine[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ' ' && !inQuotes)
                {
                    if (currentArg.Length > 0)
                    {
                        args.Add(currentArg.ToString());
                        currentArg.Clear();
                    }
                }
                else
                {
                    currentArg.Append(c);
                }
            }

            if (currentArg.Length > 0)
            {
                args.Add(currentArg.ToString());
            }

            return args.ToArray();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("  DBD.exe [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("REQUIRED:");
            Console.WriteLine("  -c, --connection <string>          SQL Server connection string");
            Console.WriteLine("  AND at least one of:");
            Console.WriteLine("    -d, --dacpac <path>              Path to the DACPAC file to deploy");
            Console.WriteLine("    --pre-deploy <path>              Path to pre-deployment scripts directory");
            Console.WriteLine("    --post-deploy <path>             Path to post-deployment scripts directory");
            Console.WriteLine();
            Console.WriteLine("OPTIONAL:");
            Console.WriteLine("  --block-data-loss <true|false>     Block deployment if data loss detected (default: true)");
            Console.WriteLine("  --drop-objects <true|false>        Drop objects not in source (default: false)");
            Console.WriteLine("  --ignore-permissions <true|false>  Ignore permissions (default: true)");
            Console.WriteLine("  --ignore-roles <true|false>        Ignore role membership (default: true)");
            Console.WriteLine("  --backup <true|false>              Backup database before changes (default: false)");
            Console.WriteLine("  --generate-script                  Generate deployment script without executing");
            Console.WriteLine("  --script-path <path>               Path where deployment script will be saved");
            Console.WriteLine("  --timeout <seconds>                Command timeout in seconds (default: 300)");
            Console.WriteLine("  --continue-on-error                Continue executing scripts even if one fails");
            Console.WriteLine("  -v, --verbose                      Enable verbose logging");
            Console.WriteLine("  --help                             Display this help screen");
            Console.WriteLine("  --version                          Display version information");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("  1. Deploy DACPAC only:");
            Console.WriteLine("     --connection \"Server=localhost;Database=MyDB;Integrated Security=true;\" --dacpac \"Database.dacpac\"");
            Console.WriteLine();
            Console.WriteLine("  2. Run scripts only (no DACPAC):");
            Console.WriteLine("     --connection \"Server=localhost;Database=MyDB;Integrated Security=true;\" --post-deploy \"scripts/post-deploy\"");
            Console.WriteLine();
            Console.WriteLine("  3. Full deployment with DACPAC and scripts:");
            Console.WriteLine("     --connection \"Server=localhost;Database=MyDB;Integrated Security=true;\" --dacpac \"Database.dacpac\" --pre-deploy \"scripts/pre-deploy\" --post-deploy \"scripts/post-deploy\"");
            Console.WriteLine();
            Console.WriteLine("  4. Generate script without deploying:");
            Console.WriteLine("     --connection \"Server=localhost;Database=MyDB;Integrated Security=true;\" --dacpac \"Database.dacpac\" --generate-script --script-path \"deploy.sql\"");
        }

        private static void ConfigureLogging()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            var configFile = new FileInfo("log4net.config");
            
            if (configFile.Exists)
            {
                XmlConfigurator.Configure(logRepository, configFile);
            }
            else
            {
                BasicConfigurator.Configure(logRepository);
            }
        }

        private static async Task<int> RunDeploymentAsync(CommandLineOptions cmdOptions)
        {
            ConfigureLogging();
            
            _log.Info("DatabaseDeployer CLI - Starting");
            _log.Info($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            try
            {
                var deploymentOptions = MapToDeploymentOptions(cmdOptions);
                
                _log.Info("Validating deployment options...");
                deploymentOptions.Validate();

                _log.Info("Deployment Configuration:");
                _log.Info($"  Mode: {deploymentOptions.Mode}");
                _log.Info($"  DACPAC: {deploymentOptions.DacpacPath ?? "None"}");
                _log.Info($"  Pre-Deploy Scripts: {deploymentOptions.PreDeployScriptsPath ?? "None"}");
                _log.Info($"  Post-Deploy Scripts: {deploymentOptions.PostDeployScriptsPath ?? "None"}");
                _log.Info($"  Block Data Loss: {deploymentOptions.BlockOnPossibleDataLoss}");
                _log.Info($"  Drop Objects Not In Source: {deploymentOptions.DropObjectsNotInSource}");
                _log.Info($"  Command Timeout: {deploymentOptions.CommandTimeout}s");

                var tracker = new ScriptTracker(deploymentOptions.ConnectionString);
                await tracker.EnsureTrackingTableExistsAsync();

                var scriptRunner = new ScriptRunner(deploymentOptions, tracker);
                var dacpacDeployer = new DacpacDeployer(deploymentOptions);

                // AUTO-DETECT MODE: Compare scripts in folder vs scripts in database
                var scriptsInFolder = scriptRunner.CountUpgradeScripts(
                    deploymentOptions.PreDeployScriptsPath, 
                    deploymentOptions.PostDeployScriptsPath);
                var scriptsInDatabase = await tracker.GetExecutedScriptCountAsync();

                var originalMode = deploymentOptions.Mode;
                
                if (scriptsInFolder < scriptsInDatabase)
                {
                    // Branch is behind database - automatic downgrade required
                    deploymentOptions.Mode = DeploymentMode.Downgrade;
                    _log.Warn("============================================");
                    _log.Warn("AUTO-DETECTION: Branch appears outdated!");
                    _log.Warn($"Scripts in folder: {scriptsInFolder}");
                    _log.Warn($"Scripts executed in database: {scriptsInDatabase}");
                    _log.Warn($"Automatically switching to DOWNGRADE mode");
                    _log.Warn("============================================");
                }
                else if (originalMode == DeploymentMode.Downgrade && scriptsInFolder >= scriptsInDatabase)
                {
                    // User requested downgrade but folder has equal/more scripts
                    _log.Info("============================================");
                    _log.Info("AUTO-DETECTION: Branch is current or ahead");
                    _log.Info($"Scripts in folder: {scriptsInFolder}");
                    _log.Info($"Scripts executed in database: {scriptsInDatabase}");
                    _log.Info($"Honoring user-specified DOWNGRADE mode");
                    _log.Info("============================================");
                }
                else
                {
                    _log.Info("============================================");
                    _log.Info($"Mode: {deploymentOptions.Mode}");
                    _log.Info($"Scripts in folder: {scriptsInFolder}");
                    _log.Info($"Scripts executed in database: {scriptsInDatabase}");
                    _log.Info("============================================");
                }

                // Execute in different order based on mode
                if (deploymentOptions.Mode == DeploymentMode.Upgrade)
                {
                    await ExecuteUpgradeAsync(deploymentOptions, scriptRunner, dacpacDeployer);
                }
                else
                {
                    await ExecuteDowngradeAsync(deploymentOptions, scriptRunner, dacpacDeployer);
                }

                _log.Info("============================================");
                _log.Info($"{deploymentOptions.Mode.ToString().ToUpper()} COMPLETED SUCCESSFULLY");
                _log.Info("============================================");

                return 0;
            }
            catch (Exception ex)
            {
                _log.Error("============================================");
                _log.Error("DEPLOYMENT FAILED");
                _log.Error("============================================");
                _log.Error("Error details:", ex);
                return 1;
            }
        }

        private static DeploymentOptions MapToDeploymentOptions(CommandLineOptions cmdOptions)
        {
            // Parse deployment mode
            var mode = DeploymentMode.Upgrade;
            if (!string.IsNullOrWhiteSpace(cmdOptions.Mode))
            {
                if (cmdOptions.Mode.Equals("downgrade", StringComparison.OrdinalIgnoreCase))
                {
                    mode = DeploymentMode.Downgrade;
                }
                else if (!cmdOptions.Mode.Equals("upgrade", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Invalid mode '{cmdOptions.Mode}'. Valid values are 'upgrade' or 'downgrade'.");
                }
            }

            var options = new DeploymentOptions
            {
                ConnectionString = cmdOptions.ConnectionString,
                Mode = mode,
                DeploymentFolderPath = cmdOptions.DeploymentFolderPath,
                DacpacPath = cmdOptions.DacpacPath,
                PreDeployScriptsPath = cmdOptions.PreDeployScriptsPath,
                PostDeployScriptsPath = cmdOptions.PostDeployScriptsPath,
                BlockOnPossibleDataLoss = cmdOptions.BlockOnPossibleDataLoss,
                DropObjectsNotInSource = cmdOptions.DropObjectsNotInSource,
                IgnorePermissions = cmdOptions.IgnorePermissions,
                IgnoreRoleMembership = cmdOptions.IgnoreRoleMembership,
                IgnoreUserSettingsObjects = cmdOptions.IgnoreUserSettingsObjects,
                BackupDatabaseBeforeChanges = cmdOptions.BackupDatabaseBeforeChanges,
                GenerateDeploymentScript = cmdOptions.GenerateDeploymentScript,
                DeploymentScriptPath = cmdOptions.DeploymentScriptPath,
                CommandTimeout = cmdOptions.CommandTimeout,
                ContinueOnError = cmdOptions.ContinueOnError,
                Verbose = cmdOptions.Verbose
            };

            // If using unified deployment folder, auto-detect paths
            if (!string.IsNullOrWhiteSpace(options.DeploymentFolderPath))
            {
                var updatesPath = Path.Combine(options.DeploymentFolderPath, "updates");
                var scriptsPath = Path.Combine(options.DeploymentFolderPath, "scripts");

                // Auto-detect DACPAC in updates folder (if not explicitly specified)
                if (string.IsNullOrWhiteSpace(options.DacpacPath) && Directory.Exists(updatesPath))
                {
                    var dacpacFiles = Directory.GetFiles(updatesPath, "*.dacpac");
                    if (dacpacFiles.Length > 0)
                    {
                        // Use the first (or only) DACPAC found
                        options.DacpacPath = dacpacFiles.OrderByDescending(f => f).First();
                        _log.Info($"Auto-detected DACPAC: {Path.GetFileName(options.DacpacPath)}");
                    }
                }

                // Auto-set script paths
                if (Directory.Exists(scriptsPath))
                {
                    var preDeployPath = Path.Combine(scriptsPath, "pre-deploy");
                    var postDeployPath = Path.Combine(scriptsPath, "post-deploy");

                    if (string.IsNullOrWhiteSpace(options.PreDeployScriptsPath) && Directory.Exists(preDeployPath))
                    {
                        options.PreDeployScriptsPath = preDeployPath;
                    }

                    if (string.IsNullOrWhiteSpace(options.PostDeployScriptsPath) && Directory.Exists(postDeployPath))
                    {
                        options.PostDeployScriptsPath = postDeployPath;
                    }
                }
            }

            return options;
        }

        private static async Task ExecuteUpgradeAsync(DeploymentOptions deploymentOptions, ScriptRunner scriptRunner, DacpacDeployer dacpacDeployer)
        {
            _log.Info("============================================");
            _log.Info("PHASE 1: Pre-Deployment Scripts (UPGRADE)");
            _log.Info("============================================");

            if (!string.IsNullOrWhiteSpace(deploymentOptions.PreDeployScriptsPath))
            {
                var preDeployCount = await scriptRunner.RunScriptsAsync(
                    deploymentOptions.PreDeployScriptsPath,
                    isPreDeploy: true
                );
                _log.Info($"Pre-deployment phase completed: {preDeployCount} script(s) executed");
            }
            else
            {
                _log.Info("No pre-deployment scripts configured");
            }

            _log.Info("============================================");
            _log.Info("PHASE 2: DACPAC Deployment");
            _log.Info("============================================");

            var dacpacDeployed = await dacpacDeployer.DeployAsync();

            if (dacpacDeployed)
            {
                _log.Info("DACPAC deployment phase completed successfully");
            }
            else
            {
                _log.Info("No DACPAC deployment performed");
            }

            _log.Info("============================================");
            _log.Info("PHASE 3: Post-Deployment Scripts (UPGRADE)");
            _log.Info("============================================");

            if (!string.IsNullOrWhiteSpace(deploymentOptions.PostDeployScriptsPath))
            {
                var postDeployCount = await scriptRunner.RunScriptsAsync(
                    deploymentOptions.PostDeployScriptsPath,
                    isPreDeploy: false
                );
                _log.Info($"Post-deployment phase completed: {postDeployCount} script(s) executed");
            }
            else
            {
                _log.Info("No post-deployment scripts configured");
            }
        }

        private static async Task ExecuteDowngradeAsync(DeploymentOptions deploymentOptions, ScriptRunner scriptRunner, DacpacDeployer dacpacDeployer)
        {
            _log.Info("============================================");
            _log.Info("PHASE 1: Post-Deployment Scripts (DOWNGRADE - REVERSE ORDER)");
            _log.Info("============================================");

            if (!string.IsNullOrWhiteSpace(deploymentOptions.PostDeployScriptsPath))
            {
                var postDeployCount = await scriptRunner.RunScriptsAsync(
                    deploymentOptions.PostDeployScriptsPath,
                    isPreDeploy: false
                );
                _log.Info($"Post-deployment downgrade phase completed: {postDeployCount} script(s) executed");
            }
            else
            {
                _log.Info("No post-deployment scripts configured");
            }

            _log.Info("============================================");
            _log.Info("PHASE 2: DACPAC Deployment (DOWNGRADE)");
            _log.Info("============================================");

            var dacpacDeployed = await dacpacDeployer.DeployAsync();

            if (dacpacDeployed)
            {
                _log.Info("DACPAC downgrade deployment completed successfully");
            }
            else
            {
                _log.Info("No DACPAC deployment performed");
            }

            _log.Info("============================================");
            _log.Info("PHASE 3: Pre-Deployment Scripts (DOWNGRADE - REVERSE ORDER)");
            _log.Info("============================================");

            if (!string.IsNullOrWhiteSpace(deploymentOptions.PreDeployScriptsPath))
            {
                var preDeployCount = await scriptRunner.RunScriptsAsync(
                    deploymentOptions.PreDeployScriptsPath,
                    isPreDeploy: true
                );
                _log.Info($"Pre-deployment downgrade phase completed: {preDeployCount} script(s) executed");
            }
            else
            {
                _log.Info("No pre-deployment scripts configured");
            }
        }

        private static int HandleParseErrors(IEnumerable<Error> errors, string[] args, ParserResult<CommandLineOptions> parseResult)
        {
            var errorList = errors.ToList();
            
            if (errorList.Any(e => e is HelpRequestedError || e is VersionRequestedError))
            {
                var helpText = CommandLine.Text.HelpText.AutoBuild(parseResult);
                Console.WriteLine(helpText);
                return 0;
            }

            var customHelpText = CommandLine.Text.HelpText.AutoBuild(parseResult, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = "DatabaseDeployer.Cli 1.0.0";
                h.Copyright = "Copyright (C) 2026 DatabaseDeployer.Cli";
                return h;
            });

            Console.WriteLine(customHelpText);
            Console.WriteLine();
            
            if (errorList.Any(e => e is MissingRequiredOptionError))
            {
                Console.WriteLine("ERROR: Missing required options.");
            }
            else
            {
                Console.WriteLine("ERROR: Invalid command line arguments.");
            }
            
            Console.WriteLine();
            Console.WriteLine("Example usage:");
            Console.WriteLine("  DBD.exe --connection \"Server=localhost;Database=MyDB;Integrated Security=true;\" --dacpac \"Database.dacpac\"");
            Console.WriteLine();

            return 1;
        }
    }
}
