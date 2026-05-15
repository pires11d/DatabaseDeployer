# DatabaseDeployer

A professional-grade SQL Server database deployment tool built in C# that orchestrates DACPAC deployments with pre-deploy and post-deploy migration scripts.

## The Philosophy

Forget running loose SQL scripts and hoping they execute in the right order. DatabaseDeployer uses Microsoft's battle-tested DACPAC technology for declarative, state-based database deployments, combined with migration scripts for data transformations. This is the industrial-strength approach that serious database DevOps teams use.

## What It Does

DatabaseDeployer executes database deployments in three precise phases:

1. **Pre-Deployment Scripts** - Backup operations, pre-migration data prep, anything that needs to run before schema changes
2. **DACPAC Deployment** - Smart, differential schema deployment using Microsoft.SqlServer.DacFx
3. **Post-Deployment Scripts** - Data migrations, reference data updates, post-deployment cleanup

All script executions are tracked in a `DeploymentScripts` table, ensuring idempotent deployments. Scripts that have already been executed successfully are automatically skipped.

## Features

- **Interactive Mode** - Double-click the exe to get help and enter commands interactively
- **State-Based DACPAC Deployment** - Define your database schema declaratively, let the tool generate the diff
- **Script Orchestration** - Pre-deploy and post-deploy scripts run in alphabetical order
- **Execution Tracking** - Scripts are tracked and won't re-run unless they fail
- **Data Loss Protection** - Configurable blocking when schema changes might destroy data
- **Deployment Script Generation** - Preview the SQL before it executes
- **Comprehensive Logging** - Console and file logging with log4net
- **CI/CD Ready** - Command-line interface perfect for Azure DevOps, GitHub Actions, etc.
- **Cross-Platform** - Built on .NET 8, runs on Windows and Linux

## Installation

### Build from Source

```powershell
git clone <your-repo-url>
cd DatabaseDeployer
dotnet build -c Release
```

The compiled executable will be in `src/DatabaseDeployer.Cli/bin/Release/net8.0/`

### Package as a Tool (Optional)

```powershell
dotnet pack src/DatabaseDeployer.Cli/DatabaseDeployer.Cli.csproj -c Release
dotnet tool install --global --add-source ./src/DatabaseDeployer.Cli/bin/Release DatabaseDeployer.Cli
```

## Usage

### Interactive Mode (Double-Click Friendly!)

Simply run the executable without arguments (or double-click it in Windows Explorer):

```powershell
DBD.exe
```

You'll see:
1. Full help menu with all options and examples
2. A prompt asking you to enter your command
3. Type your deployment command (or 'exit' to quit)
4. The tool executes your command
5. Press any key to close

**Example Interactive Session:**
```
================================================================================
                     DatabaseDeployer - Interactive Mode
================================================================================

USAGE:
  DBD.exe [OPTIONS]

OPTIONS:
  -c, --connection <string>    Required. SQL Server connection string
  ...

Enter your command (or 'exit' to quit): --connection "Server=localhost;Database=MyDB;Integrated Security=true;" --dacpac "Database.dacpac"

[Deployment executes...]

Press any key to exit...
```

### Command-Line Mode

### Basic Deployment with DACPAC Only

```powershell
DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;Integrated Security=true;" `
  --dacpac "path/to/MyDatabase.dacpac"
```

### Scripts-Only Deployment (No DACPAC)

Run migration scripts without deploying a DACPAC:

```powershell
DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;Integrated Security=true;" `
  --post-deploy "path/to/scripts/post-deploy"
```

Or with both pre and post-deploy scripts:

```powershell
DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;Integrated Security=true;" `
  --pre-deploy "path/to/scripts/pre-deploy" `
  --post-deploy "path/to/scripts/post-deploy"
```

### Full Deployment with Pre/Post Scripts

```powershell
DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;Integrated Security=true;" `
  --dacpac "path/to/MyDatabase.dacpac" `
  --pre-deploy "path/to/scripts/pre-deploy" `
  --post-deploy "path/to/scripts/post-deploy"
```

### Generate Deployment Script Without Executing

```powershell
DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;Integrated Security=true;" `
  --dacpac "path/to/MyDatabase.dacpac" `
  --generate-script `
  --script-path "output/deploy.sql"
```

### Advanced Options

```powershell
DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;User Id=deploy;Password=***;" `
  --dacpac "MyDatabase.dacpac" `
  --pre-deploy "scripts/pre-deploy" `
  --post-deploy "scripts/post-deploy" `
  --block-data-loss false `
  --drop-objects true `
  --backup true `
  --timeout 600 `
  --verbose
```

## Command-Line Options

| Option | Required | Default | Description |
|--------|----------|---------|-------------|
| `-c, --connection` | Yes | - | SQL Server connection string |
| `-d, --dacpac` | No | - | Path to DACPAC file |
| `--pre-deploy` | No | - | Path to pre-deployment scripts directory |
| `--post-deploy` | No | - | Path to post-deployment scripts directory |
| `--block-data-loss` | No | true | Block deployment if data loss detected |
| `--drop-objects` | No | false | Drop objects in target not in source |
| `--ignore-permissions` | No | true | Ignore permissions during deployment |
| `--ignore-roles` | No | true | Ignore role membership |
| `--ignore-user-settings` | No | true | Ignore user settings objects |
| `--backup` | No | false | Backup database before changes |
| `--generate-script` | No | false | Generate deployment script only |
| `--script-path` | No | - | Output path for generated script |
| `--timeout` | No | 300 | Command timeout in seconds |
| `--continue-on-error` | No | false | Continue on script errors |
| `-v, --verbose` | No | false | Enable verbose logging |

## Project Structure

```
DatabaseDeployer/
├── src/
│   ├── DatabaseDeployer.Core/        # Core deployment logic
│   │   ├── DacpacDeployer.cs        # DACPAC deployment orchestration
│   │   ├── ScriptRunner.cs          # SQL script execution engine
│   │   ├── ScriptTracker.cs         # Script execution tracking
│   │   └── DeploymentOptions.cs     # Configuration model
│   ├── DatabaseDeployer.Cli/        # Command-line interface
│   │   ├── Program.cs               # Entry point
│   │   ├── CommandLineOptions.cs    # CLI argument parsing
│   │   └── log4net.config          # Logging configuration
│   └── DatabaseDeployer.Tests/      # Unit tests
└── examples/
    └── scripts/
        ├── pre-deploy/
        │   └── 001_CreateBackup.sql
        └── post-deploy/
            └── 002_MigrateData.sql
```

## Script Organization Best Practices

### Naming Convention

Use numeric prefixes to control execution order:

```
scripts/
├── pre-deploy/
│   ├── 001_DisableTriggers.sql
│   ├── 002_BackupCriticalData.sql
│   └── 003_ArchiveOldRecords.sql
└── post-deploy/
    ├── 001_MigrateCustomerData.sql
    ├── 002_UpdateReferenceTables.sql
    └── 003_EnableTriggers.sql
```

### Script Types

**Pre-Deploy Scripts** - Use for:
- Database backups
- Data archival
- Disabling triggers/constraints
- Pre-migration data transformations

**Post-Deploy Scripts** - Use for:
- Data migrations
- Reference data updates
- Re-enabling triggers/constraints
- Post-deployment validations

## Integration with CI/CD

### Azure DevOps Pipeline Example

```yaml
- task: PowerShell@2
  displayName: 'Deploy Database'
  inputs:
    targetType: 'inline'
    script: |
      ./DBD.exe `
        --connection "$(ConnectionString)" `
        --dacpac "$(Build.ArtifactStagingDirectory)/Database.dacpac" `
        --pre-deploy "$(Build.SourcesDirectory)/database/scripts/pre-deploy" `
        --post-deploy "$(Build.SourcesDirectory)/database/scripts/post-deploy" `
        --verbose
```

### GitHub Actions Example

```yaml
- name: Deploy Database
  run: |
    ./DatabaseDeployer.Cli \
      --connection "${{ secrets.CONNECTION_STRING }}" \
      --dacpac "./artifacts/Database.dacpac" \
      --pre-deploy "./database/scripts/pre-deploy" \
      --post-deploy "./database/scripts/post-deploy"
```

## How DACPAC Deployment Works

Instead of manually scripting `ALTER TABLE` and `CREATE INDEX` statements, you:

1. Define your database schema in a SQL Server Database Project (.sqlproj)
2. Build the project to generate a `.dacpac` file
3. DatabaseDeployer compares the dacpac to the target database
4. It generates and executes only the necessary changes
5. Dependencies are automatically resolved (tables before views, etc.)

This is **declarative deployment** - you describe what the database *should look like*, not how to get there.

## Deployment Tracking

DatabaseDeployer creates a `DeploymentScripts` table to track script executions:

```sql
CREATE TABLE [dbo].[DeploymentScripts]
(
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ScriptName] NVARCHAR(500) NOT NULL,
    [ScriptType] NVARCHAR(50) NOT NULL,      -- 'PreDeploy' or 'PostDeploy'
    [ExecutedDate] DATETIME2 NOT NULL,
    [ExecutedBy] NVARCHAR(256) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,          -- 'Success' or 'Failed'
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [DurationMs] BIGINT NULL
);
```

Scripts that have been successfully executed are automatically skipped on subsequent deployments.

## Building a DACPAC

You can create DACPACs using:

1. **Visual Studio SQL Server Database Project** - Right-click project > Build
2. **MSBuild** - `msbuild MyDatabase.sqlproj /p:Configuration=Release`
3. **SqlPackage** - Extract from existing database: `sqlpackage /Action:Extract /SourceConnectionString:"..." /TargetFile:Database.dacpac`

## Error Handling

- By default, deployment stops on first error
- Use `--continue-on-error` to execute remaining scripts after failures
- All errors are logged to console and log file
- Failed scripts are recorded in the tracking table

## Logging

Logs are written to:
- **Console** - Real-time deployment progress
- **File** - `logs/DatabaseDeployer.log` with rolling file appender

Configure logging in `log4net.config`

## License

[Apache License 2.0](LICENSE) - Use it, abuse it, make it your own.

## Contributing

This is a reusable tool. Fork it, extend it, make it better. No bureaucracy, just code.

---

**Built with anger and caffeine by developers who are tired of brittle database deployment scripts.**
