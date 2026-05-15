# Scripts-Only Deployment Example

This demonstrates how to use DatabaseDeployer to run ONLY migration scripts without deploying a DACPAC.

## Use Cases

**When to use scripts-only deployment:**
- Running data migrations on an existing database
- Updating reference/lookup tables
- Executing maintenance scripts
- Post-deployment data transformations
- Seeding data into an existing schema

## Examples

### Post-Deploy Scripts Only

Perfect for data migrations after someone else has deployed the schema:

```powershell
cd src\DatabaseDeployer.Cli\bin\Release\net8.0

.\DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;Integrated Security=true;" `
  --post-deploy "C:\MyProject\scripts\data-migrations"
```

### Pre-Deploy Scripts Only

For backup or archival operations:

```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;Integrated Security=true;" `
  --pre-deploy "C:\MyProject\scripts\backups"
```

### Both Pre and Post Scripts

Run preparation scripts, then data migrations:

```powershell
.\DBD.exe `
  --connection "Server=localhost;Database=MyDatabase;Integrated Security=true;" `
  --pre-deploy "C:\MyProject\scripts\pre-deploy" `
  --post-deploy "C:\MyProject\scripts\post-deploy" `
  --verbose
```

### Interactive Mode

Double-click the exe and enter:

```
--connection "Server=localhost;Database=MyDB;Integrated Security=true;" --post-deploy "scripts/data-migrations"
```

## Script Organization

```
your-project/
└── scripts/
    ├── pre-deploy/
    │   ├── 001_ArchiveOldData.sql
    │   └── 002_DisableTriggers.sql
    └── post-deploy/
        ├── 001_MigrateCustomerData.sql
        ├── 002_UpdateReferenceTables.sql
        └── 003_EnableTriggers.sql
```

## Execution Tracking

Just like DACPAC deployments, scripts are tracked in `DeploymentScripts` table:

```sql
SELECT * FROM [dbo].[DeploymentScripts] 
WHERE ScriptType IN ('PreDeploy', 'PostDeploy')
ORDER BY ExecutedDate DESC;
```

Scripts that have been successfully executed won't re-run automatically.

## Benefits

- **No DACPAC needed** - Deploy scripts independently of schema changes
- **Idempotent** - Scripts are tracked and won't re-execute
- **Flexible** - Mix and match pre/post scripts as needed
- **CI/CD Ready** - Perfect for separating schema from data deployments
- **Safe** - Same error handling and logging as DACPAC deployments

## Common Patterns

### Pattern 1: Schema First, Data Second

```powershell
# Step 1: Deploy schema (with DACPAC)
.\DBD.exe --connection $conn --dacpac "MyDb.dacpac"

# Step 2: Deploy data (scripts only)
.\DBD.exe --connection $conn --post-deploy "scripts/data"
```

### Pattern 2: Environment-Specific Data

```powershell
# Dev environment - seed test data
.\DBD.exe --connection $devConn --post-deploy "scripts/dev-data"

# Prod environment - migrate production data
.\DBD.exe --connection $prodConn --post-deploy "scripts/prod-migrations"
```

### Pattern 3: Maintenance Scripts

```powershell
# Monthly maintenance
.\DBD.exe --connection $conn --post-deploy "scripts/monthly-maintenance"
```

## Validation

The tool requires at least ONE of:
- DACPAC path
- Pre-deploy scripts path
- Post-deploy scripts path

So you can't run it with ONLY a connection string - you must provide something to deploy.

---

**Scripts-only deployments give you the flexibility to separate schema changes from data operations. Professional database DevOps at its finest.**
