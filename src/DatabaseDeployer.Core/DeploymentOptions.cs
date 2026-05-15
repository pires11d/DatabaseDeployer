namespace DatabaseDeployer.Core
{
    public enum DeploymentMode
    {
        Upgrade,
        Downgrade
    }

    public class DeploymentOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        
        // Legacy individual paths (backward compatibility)
        public string? DacpacPath { get; set; }
        public string? PreDeployScriptsPath { get; set; }
        public string? PostDeployScriptsPath { get; set; }
        
        // New unified deployment folder structure
        public string? DeploymentFolderPath { get; set; }
        public DeploymentMode Mode { get; set; } = DeploymentMode.Upgrade;
        
        public bool BlockOnPossibleDataLoss { get; set; } = true;
        public bool DropObjectsNotInSource { get; set; } = false;
        public bool IgnorePermissions { get; set; } = true;
        public bool IgnoreRoleMembership { get; set; } = true;
        public bool IgnoreUserSettingsObjects { get; set; } = true;
        public bool BackupDatabaseBeforeChanges { get; set; } = false;
        public bool GenerateDeploymentScript { get; set; } = false;
        public string? DeploymentScriptPath { get; set; }
        public int CommandTimeout { get; set; } = 300;
        public bool ContinueOnError { get; set; } = false;
        public bool Verbose { get; set; } = false;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ArgumentException("ConnectionString is required", nameof(ConnectionString));
            }

            var hasDeploymentFolder = !string.IsNullOrWhiteSpace(DeploymentFolderPath);
            var hasDacpac = !string.IsNullOrWhiteSpace(DacpacPath);
            var hasPreDeploy = !string.IsNullOrWhiteSpace(PreDeployScriptsPath);
            var hasPostDeploy = !string.IsNullOrWhiteSpace(PostDeployScriptsPath);

            if (!hasDeploymentFolder && !hasDacpac && !hasPreDeploy && !hasPostDeploy)
            {
                throw new ArgumentException("At least one deployment option is required: deployment folder, DACPAC, pre-deploy scripts, or post-deploy scripts");
            }

            if (hasDeploymentFolder && !Directory.Exists(DeploymentFolderPath))
            {
                throw new DirectoryNotFoundException($"Deployment folder not found: {DeploymentFolderPath}");
            }

            if (!string.IsNullOrWhiteSpace(DacpacPath) && !File.Exists(DacpacPath))
            {
                throw new FileNotFoundException($"DACPAC file not found: {DacpacPath}");
            }

            if (!string.IsNullOrWhiteSpace(PreDeployScriptsPath) && !Directory.Exists(PreDeployScriptsPath))
            {
                throw new DirectoryNotFoundException($"Pre-deploy scripts directory not found: {PreDeployScriptsPath}");
            }

            if (!string.IsNullOrWhiteSpace(PostDeployScriptsPath) && !Directory.Exists(PostDeployScriptsPath))
            {
                throw new DirectoryNotFoundException($"Post-deploy scripts directory not found: {PostDeployScriptsPath}");
            }

            if (GenerateDeploymentScript && string.IsNullOrWhiteSpace(DeploymentScriptPath))
            {
                throw new ArgumentException("DeploymentScriptPath is required when GenerateDeploymentScript is true", nameof(DeploymentScriptPath));
            }
        }
    }
}
