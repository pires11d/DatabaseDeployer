# DatabaseDeployer - Project Summary

## What You've Got

A professional, reusable, production-ready database deployment tool written in C# that handles:

- **DACPAC Deployments** - State-based schema deployment using Microsoft's DacFx
- **Pre-Deploy Scripts** - Migration scripts that run before schema changes
- **Post-Deploy Scripts** - Migration scripts that run after schema changes  
- **Deployment Tracking** - Idempotent script execution with database-based tracking
- **Comprehensive Logging** - Console and file logging with log4net
- **CI/CD Ready** - Command-line interface for pipeline integration

## Project Structure

```
DatabaseDeployer/
├── src/
│   ├── DatabaseDeployer.Core/           # Core deployment engine
│   │   ├── DacpacDeployer.cs           # DACPAC deployment logic
│   │   ├── ScriptRunner.cs             # SQL script execution
│   │   ├── ScriptTracker.cs            # Execution tracking
│   │   ├── DeploymentOptions.cs        # Configuration model
│   │   └── CreateScriptsTable.sql      # Tracking table DDL
│   ├── DatabaseDeployer.Cli/           # Command-line application
│   │   ├── Program.cs                  # Main entry point (proper class, no top-level)
│   │   ├── CommandLineOptions.cs       # CLI argument parsing
│   │   └── log4net.config             # Logging configuration
│   └── DatabaseDeployer.Tests/         # Unit tests (xUnit)
│       └── DeploymentOptionsTests.cs   # Validation tests
├── examples/
│   ├── scripts/                        # Example migration scripts
│   └── DeploymentExamples.ps1         # Usage examples
├── DatabaseDeployer.sln                # Visual Studio solution
├── README.md                           # Full documentation
├── QUICKSTART.md                       # 5-minute getting started guide
└── .gitignore                          # Git ignore rules
```

## Key Technologies

- **.NET 8** - Modern, cross-platform runtime
- **Microsoft.SqlServer.DacFx** - Official DACPAC deployment API (v162.3.566)
- **Microsoft.Data.SqlClient** - SQL Server connectivity (v5.2.0)
- **CommandLineParser** - Argument parsing (v2.9.1)
- **log4net** - Logging framework (v2.0.17)
- **xUnit** - Unit testing framework

## Build and Run

### Build
```powershell
dotnet build -c Release
```

### Test
```powershell
dotnet test
```
✅ **7 tests passing**

### Run
```powershell
cd src\DatabaseDeployer.Cli\bin\Release\net8.0
.\DBD.exe --help
```

## Usage Examples

### Simple DACPAC Deployment
```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=MyDB;Integrated Security=true;" `
  --dacpac "Database.dacpac"
```

### Full Deployment Pipeline
```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=MyDB;Integrated Security=true;" `
  --dacpac "Database.dacpac" `
  --pre-deploy "scripts/pre-deploy" `
  --post-deploy "scripts/post-deploy" `
  --verbose
```

### Production with Safety
```powershell
.\DBD.exe `
  --connection $prodConnectionString `
  --dacpac "Database.dacpac" `
  --pre-deploy "scripts/pre-deploy" `
  --post-deploy "scripts/post-deploy" `
  --block-data-loss true `
  --backup true `
  --timeout 600
```

## Deployment Phases

The tool executes in three phases:

1. **Phase 1: Pre-Deploy Scripts**
   - Runs SQL scripts from `--pre-deploy` directory
   - Alphabetical order execution
   - Tracked in database to prevent re-execution

2. **Phase 2: DACPAC Deployment**
   - Compares DACPAC to target database
   - Generates differential schema changes
   - Applies changes with dependency resolution

3. **Phase 3: Post-Deploy Scripts**
   - Runs SQL scripts from `--post-deploy` directory
   - Alphabetical order execution
   - Tracked in database to prevent re-execution

## Deployment Tracking

A tracking table is created automatically:

```sql
CREATE TABLE [dbo].[DeploymentScripts]
(
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ScriptName] NVARCHAR(500) NOT NULL,
    [ScriptType] NVARCHAR(50) NOT NULL,      -- 'PreDeploy' or 'PostDeploy'
    [ExecutedDate] DATETIME2 NOT NULL,
    [ExecutedBy] NVARCHAR(256) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,          -- 'Success' or 'Failed'
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [DurationMs] BIGINT NULL
);
```

Scripts are idempotent - they won't re-run unless they previously failed.

## CI/CD Integration

### Azure DevOps
```yaml
- task: PowerShell@2
  displayName: 'Deploy Database'
  inputs:
    script: |
      ./DBD.exe `
        --connection "$(ConnectionString)" `
        --dacpac "$(Build.ArtifactStagingDirectory)/Database.dacpac" `
        --pre-deploy "scripts/pre-deploy" `
        --post-deploy "scripts/post-deploy"
```

### GitHub Actions
```yaml
- name: Deploy Database
  run: |
    ./DatabaseDeployer.Cli \
      --connection "${{ secrets.CONNECTION_STRING }}" \
      --dacpac "./Database.dacpac" \
      --pre-deploy "./scripts/pre-deploy" \
      --post-deploy "./scripts/post-deploy"
```

## Design Principles

1. **No DartDb References** - 100% reusable, generic naming
2. **No Top-Level Statements** - Proper `Program` class with `Main` method
3. **Strongly Typed** - Full C# type safety, no PowerShell string manipulation
4. **Testable** - Unit tests included, easy to extend
5. **Professional Logging** - Structured logging with log4net
6. **Fail-Safe** - Data loss protection, transaction support, error handling
7. **Cross-Platform** - Runs on Windows and Linux with .NET 8

## Next Steps

1. **Create a DACPAC** - Build from your SQL Server Database Project (.sqlproj)
2. **Organize Scripts** - Create pre-deploy and post-deploy directories
3. **Test Locally** - Run against a dev database first
4. **Integrate CI/CD** - Add to your deployment pipeline
5. **Monitor Executions** - Query `DeploymentScripts` table to track history

## Documentation

- **README.md** - Complete documentation with all features
- **QUICKSTART.md** - Get running in 5 minutes
- **examples/DeploymentExamples.ps1** - Common usage scenarios
- **examples/scripts/** - Sample pre-deploy and post-deploy scripts

---

**Built from scratch. Zero DartDb references. 100% reusable. Ready for production.**

*This is how you deploy databases when you mean business.*
