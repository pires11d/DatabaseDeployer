using CommandLine;

namespace DatabaseDeployer.Cli
{
    public class CommandLineOptions
    {
        [Option('c', "connection", Required = true, HelpText = "SQL Server connection string")]
        public string ConnectionString { get; set; } = string.Empty;

        // New unified deployment folder option
        [Option('f', "deployment-folder", Required = false, HelpText = "Path to unified deployment folder (contains updates/ and scripts/ subfolders)")]
        public string? DeploymentFolderPath { get; set; }

        [Option('m', "mode", Default = "upgrade", HelpText = "Deployment mode: 'upgrade' or 'downgrade'")]
        public string Mode { get; set; } = "upgrade";

        // Legacy individual path options (for backward compatibility)
        [Option('d', "dacpac", Required = false, HelpText = "Path to the DACPAC file to deploy")]
        public string? DacpacPath { get; set; }

        [Option("pre-deploy", Required = false, HelpText = "Path to pre-deployment scripts directory")]
        public string? PreDeployScriptsPath { get; set; }

        [Option("post-deploy", Required = false, HelpText = "Path to post-deployment scripts directory")]
        public string? PostDeployScriptsPath { get; set; }

        [Option("block-data-loss", Default = true, HelpText = "Block deployment if potential data loss is detected")]
        public bool BlockOnPossibleDataLoss { get; set; }

        [Option("drop-objects", Default = false, HelpText = "Drop objects in target that don't exist in source")]
        public bool DropObjectsNotInSource { get; set; }

        [Option("ignore-permissions", Default = true, HelpText = "Ignore permissions during deployment")]
        public bool IgnorePermissions { get; set; }

        [Option("ignore-roles", Default = true, HelpText = "Ignore role membership during deployment")]
        public bool IgnoreRoleMembership { get; set; }

        [Option("ignore-user-settings", Default = true, HelpText = "Ignore user settings objects")]
        public bool IgnoreUserSettingsObjects { get; set; }

        [Option("backup", Default = false, HelpText = "Backup database before deploying changes")]
        public bool BackupDatabaseBeforeChanges { get; set; }

        [Option("generate-script", Default = false, HelpText = "Generate deployment script without executing")]
        public bool GenerateDeploymentScript { get; set; }

        [Option("script-path", Required = false, HelpText = "Path where deployment script will be saved")]
        public string? DeploymentScriptPath { get; set; }

        [Option("timeout", Default = 300, HelpText = "Command timeout in seconds")]
        public int CommandTimeout { get; set; }

        [Option("continue-on-error", Default = false, HelpText = "Continue executing scripts even if one fails")]
        public bool ContinueOnError { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Enable verbose logging")]
        public bool Verbose { get; set; }
    }
}
