using DatabaseDeployer.Core;
using Xunit;

namespace DatabaseDeployer.Tests
{
    public class DeploymentOptionsTests
    {
        [Fact]
        public void Validate_ThrowsException_WhenConnectionStringIsEmpty()
        {
            var options = new DeploymentOptions
            {
                ConnectionString = string.Empty
            };

            var exception = Assert.Throws<ArgumentException>(() => options.Validate());
            Assert.Contains("ConnectionString", exception.Message);
        }

        [Fact]
        public void Validate_ThrowsException_WhenDacpacPathDoesNotExist()
        {
            var options = new DeploymentOptions
            {
                ConnectionString = "Server=localhost;Database=Test;",
                DacpacPath = "C:\\NonExistent\\Path\\Database.dacpac"
            };

            Assert.Throws<FileNotFoundException>(() => options.Validate());
        }

        [Fact]
        public void Validate_ThrowsException_WhenPreDeployScriptsPathDoesNotExist()
        {
            var options = new DeploymentOptions
            {
                ConnectionString = "Server=localhost;Database=Test;",
                PreDeployScriptsPath = "C:\\NonExistent\\PreDeploy"
            };

            Assert.Throws<DirectoryNotFoundException>(() => options.Validate());
        }

        [Fact]
        public void Validate_ThrowsException_WhenPostDeployScriptsPathDoesNotExist()
        {
            var options = new DeploymentOptions
            {
                ConnectionString = "Server=localhost;Database=Test;",
                PostDeployScriptsPath = "C:\\NonExistent\\PostDeploy"
            };

            Assert.Throws<DirectoryNotFoundException>(() => options.Validate());
        }

        [Fact]
        public void Validate_ThrowsException_WhenGenerateScriptTrueButNoScriptPath()
        {
            var options = new DeploymentOptions
            {
                ConnectionString = "Server=localhost;Database=Test;",
                PostDeployScriptsPath = "C:\\", // Need at least one deployment option
                GenerateDeploymentScript = true,
                DeploymentScriptPath = null
            };

            var exception = Assert.Throws<ArgumentException>(() => options.Validate());
            Assert.Contains("DeploymentScriptPath", exception.Message);
        }

        [Fact]
        public void Validate_Succeeds_WithMinimalValidOptions()
        {
            var options = new DeploymentOptions
            {
                ConnectionString = "Server=localhost;Database=Test;Integrated Security=true;",
                PostDeployScriptsPath = "C:\\" // Use a path that exists
            };

            var exception = Record.Exception(() => options.Validate());
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_ThrowsException_WhenNoDeploymentOptionsProvided()
        {
            var options = new DeploymentOptions
            {
                ConnectionString = "Server=localhost;Database=Test;Integrated Security=true;"
                // No DacpacPath, PreDeployScriptsPath, or PostDeployScriptsPath
            };

            var exception = Assert.Throws<ArgumentException>(() => options.Validate());
            Assert.Contains("At least one deployment option is required", exception.Message);
        }

        [Fact]
        public void DefaultValues_AreSetCorrectly()
        {
            var options = new DeploymentOptions();

            Assert.True(options.BlockOnPossibleDataLoss);
            Assert.False(options.DropObjectsNotInSource);
            Assert.True(options.IgnorePermissions);
            Assert.True(options.IgnoreRoleMembership);
            Assert.True(options.IgnoreUserSettingsObjects);
            Assert.False(options.BackupDatabaseBeforeChanges);
            Assert.False(options.GenerateDeploymentScript);
            Assert.Equal(300, options.CommandTimeout);
            Assert.False(options.ContinueOnError);
            Assert.False(options.Verbose);
        }
    }
}
