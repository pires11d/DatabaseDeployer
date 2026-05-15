# Interactive Mode - Quick Reference

## How to Launch

**Windows:**
- Double-click `DBD.exe` in File Explorer
- OR run from PowerShell: `.\DBD.exe`

**Command Line:**
```powershell
cd src\DatabaseDeployer.Cli\bin\Release\net8.0
.\DBD.exe
```

## What Happens

1. **Help Menu Displays** - Full documentation of all available options
2. **Prompt Appears** - `Enter your command (or 'exit' to quit):`
3. **You Type Your Command** - Enter deployment parameters
4. **Deployment Executes** - Tool runs with your parameters
5. **Wait for Keypress** - Press any key to close the window

## Example Commands to Enter

### Basic DACPAC Deployment
```
--connection "Server=localhost;Database=TestDB;Integrated Security=true;" --dacpac "C:\path\to\Database.dacpac"
```

### Full Deployment with Scripts
```
--connection "Server=localhost;Database=TestDB;Integrated Security=true;" --dacpac "Database.dacpac" --pre-deploy "scripts\pre-deploy" --post-deploy "scripts\post-deploy" --verbose
```

### Generate Script Only
```
--connection "Server=localhost;Database=TestDB;Integrated Security=true;" --dacpac "Database.dacpac" --generate-script --script-path "review.sql"
```

### With Authentication
```
--connection "Server=myserver;Database=MyDB;User Id=deploy;Password=MyP@ssw0rd;" --dacpac "Database.dacpac"
```

## Exit Without Deploying

Type: `exit`

## Tips

- **Copy/Paste Works** - Right-click in console to paste long connection strings
- **Use Quotes** - Wrap paths with spaces in double quotes: `"C:\My Path\file.dacpac"`
- **Relative Paths OK** - If your dacpac is in the same folder: `--dacpac "Database.dacpac"`
- **No Double-Dash Prefix** - When entering interactively, DON'T include the exe name
  - ✅ CORRECT: `--connection "..." --dacpac "..."`
  - ❌ WRONG: `DBD.exe --connection "..." --dacpac "..."`

## Why This Is Useful

Perfect for:
- **Developers** testing deployments locally
- **Quick deployments** without writing batch files
- **Windows users** who prefer GUI-like interaction
- **Learning** the tool's options without memorizing commands

## Still Works from Command Line

If you pass arguments when launching, it skips interactive mode:

```powershell
# This runs immediately, no interactive prompt
.\DBD.exe --connection "..." --dacpac "..."
```

## CI/CD Pipelines

Use command-line mode in pipelines (pass arguments directly). Interactive mode only activates when NO arguments are provided.

---

**Double-click friendly. Professional. Ready for anyone to use.**
