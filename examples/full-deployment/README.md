# Full Deployment Mode - Example

This example demonstrates the **unified deployment folder structure** with upgrade and downgrade capabilities.

## Folder Structure

```
full-deployment/
├── updates/
│   └── YourDatabase.dacpac          (Optional: Place DACPAC files here)
└── scripts/
    ├── pre-deploy/
    │   ├── up/
    │   │   └── 001_CreateTables.sql
    │   └── down/
    │       └── 001_CreateTables.sql
    └── post-deploy/
        ├── up/
        │   └── 002_SeedData.sql
        └── down/
            └── 002_SeedData.sql
```

## Usage

### Upgrade Deployment

Executes in order: **Pre-deploy (up) → DACPAC → Post-deploy (up)**

```powershell
DBD.exe --connection "Server=localhost;Database=MyDB;Integrated Security=true;" `
        --deployment-folder "C:\Deploy\full-deployment" `
        --mode upgrade
```

### Downgrade Deployment

Executes in **REVERSE** order: **Post-deploy (down) REVERSED → Pre-deploy (down) REVERSED**

```powershell
DBD.exe --connection "Server=localhost;Database=MyDB;Integrated Security=true;" `
        --deployment-folder "C:\Deploy\full-deployment" `
        --mode downgrade
```

## How It Works

1. **Auto-Detection**: Tool automatically finds:
   - DACPAC files in `updates/` folder
   - Pre-deploy scripts in `scripts/pre-deploy/`
   - Post-deploy scripts in `scripts/post-deploy/`

2. **Upgrade Mode**:
   - Runs `up/` scripts in alphabetical order
   - Registers both upgrade and downgrade scripts in database
   - Marks scripts as executed

3. **Downgrade Mode**:
   - Runs `down/` scripts in REVERSE alphabetical order
   - Only processes previously executed scripts
   - Marks scripts as NOT executed (resets flag)

4. **Script Pairing**:
   - `001_CreateTables.sql` in both `up/` and `down/` folders
   - Same filename = paired upgrade/downgrade operations
   - Downgrade script is OPTIONAL

## Important Notes

- **DACPAC Support**: DACPAC files are deployed in BOTH upgrade AND downgrade modes
- **Auto-Detection**: Tool automatically detects if branch is behind database and switches to downgrade mode
  - If `scripts in folder < scripts in database` → AUTO DOWNGRADE
  - Otherwise → Use specified mode (default: upgrade)
- **File Naming**: Use numeric prefixes (001, 002, etc.) for execution order
- **Idempotency**: Write downgrade scripts carefully to handle data safely
- **Testing**: ALWAYS test downgrade scripts in non-production first

## Auto-Detection Logic

The tool intelligently detects whether to upgrade or downgrade:

```
Branch has 2 scripts, Database has 5 executed scripts
→ AUTO DOWNGRADE (branch is behind)

Branch has 5 scripts, Database has 3 executed scripts  
→ UPGRADE (branch is ahead)

Branch has 5 scripts, Database has 5 executed scripts
→ UPGRADE (default mode, unless --mode downgrade specified)
```

This means **you can deploy any version** to any environment safely - the tool figures out the direction automatically!
