# Quick Start Guide

## Get Up and Running in 5 Minutes

### Step 1: Build the Tool

```powershell
cd c:\Git\DatabaseDeployer
dotnet build -c Release
```

### Step 2: Navigate to the Output Directory

```powershell
cd src\DatabaseDeployer.Cli\bin\Release\net8.0
```

### Step 3: Run Your First Deployment

#### Option A: Deploy a DACPAC

```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=TestDB;Integrated Security=true;" `
  --dacpac "C:\path\to\your\Database.dacpac" `
  --verbose
```

#### Option B: Run Migration Scripts Only

```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=TestDB;Integrated Security=true;" `
  --post-deploy "C:\path\to\scripts\post-deploy"
```

#### Option C: Full Deployment (DACPAC + Scripts)

```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=TestDB;Integrated Security=true;" `
  --dacpac "C:\path\to\Database.dacpac" `
  --pre-deploy "C:\path\to\scripts\pre-deploy" `
  --post-deploy "C:\path\to\scripts\post-deploy" `
  --verbose
```

### Step 4: Check the Results

- **Console Output**: See real-time deployment progress
- **Log File**: Check `logs/DatabaseDeployer.log` for detailed logs
- **Database**: Query `[dbo].[DeploymentScripts]` to see execution history

```sql
SELECT * FROM [dbo].[DeploymentScripts] ORDER BY ExecutedDate DESC;
```

## Common Scenarios

### Preview Deployment Without Executing

```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=TestDB;Integrated Security=true;" `
  --dacpac "Database.dacpac" `
  --generate-script `
  --script-path "review-this-before-prod.sql"
```

Review the generated SQL file, then deploy if it looks good.

### Production Deployment with Safety Checks

```powershell
.\DBD.exe `
  --connection "Server=prodserver;Database=ProdDB;User Id=deploy;Password=***;" `
  --dacpac "Database.dacpac" `
  --pre-deploy "scripts\pre-deploy" `
  --post-deploy "scripts\post-deploy" `
  --block-data-loss true `
  --backup true `
  --timeout 600 `
  --verbose
```

### Development Environment (Aggressive)

```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=DevDB;Integrated Security=true;" `
  --dacpac "Database.dacpac" `
  --block-data-loss false `
  --drop-objects true
```

## Creating a DACPAC

### From SQL Server Database Project

1. Open your `.sqlproj` in Visual Studio
2. Right-click project → **Build**
3. Find the `.dacpac` in `bin\Debug` or `bin\Release`

### From Existing Database

```powershell
sqlpackage /Action:Extract `
  /SourceConnectionString:"Server=localhost;Database=MyDB;Integrated Security=true;" `
  /TargetFile:"MyDatabase.dacpac"
```

## Script Organization

Create this structure:

```
your-project/
├── database/
│   ├── MyDatabase.sqlproj      # SQL Server Database Project
│   └── scripts/
│       ├── pre-deploy/
│       │   ├── 001_DisableTriggers.sql
│       │   └── 002_BackupData.sql
│       └── post-deploy/
│           ├── 001_MigrateData.sql
│           └── 002_UpdateReferenceData.sql
```

Scripts run in **alphabetical order** - use numeric prefixes to control execution.

## Troubleshooting

### "Cannot connect to database"
- Check your connection string
- Verify network access to SQL Server
- Test with SQL Server Management Studio first

### "DACPAC file not found"
- Use absolute paths: `C:\full\path\to\file.dacpac`
- Or relative from where you're running the command

### "Script already executed, skipping"
- This is normal - DatabaseDeployer tracks executed scripts
- To re-run a script, delete it from `[dbo].[DeploymentScripts]` table

### "Deployment blocked due to possible data loss"
- Review what's changing (use `--generate-script`)
- If safe, use `--block-data-loss false`
- Or modify your DACPAC to preserve data

## Next Steps

- Read the full [README.md](../README.md) for detailed documentation
- Check [DeploymentExamples.ps1](./DeploymentExamples.ps1) for more scenarios
- Integrate into your CI/CD pipeline (see README for Azure DevOps/GitHub Actions examples)

---

**You're ready to deploy databases like a professional. No more script chaos.**
