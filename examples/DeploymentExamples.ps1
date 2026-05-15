# DatabaseDeployer - Example Deployment Script
# This demonstrates how to use the DatabaseDeployer CLI for various deployment scenarios

# Configuration
$ConnectionString = "Server=localhost;Database=MyDatabase;Integrated Security=true;"
$DacpacPath = "path\to\MyDatabase.dacpac"
$PreDeployScripts = ".\examples\scripts\pre-deploy"
$PostDeployScripts = ".\examples\scripts\post-deploy"

Write-Host "DatabaseDeployer - Example Deployment Scenarios" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Example 1: DACPAC Only Deployment
Write-Host "Example 1: Deploy DACPAC only (no migration scripts)" -ForegroundColor Yellow
Write-Host "Command:" -ForegroundColor Gray
Write-Host "  .\DBD.exe --connection `"$ConnectionString`" --dacpac `"$DacpacPath`"" -ForegroundColor Gray
Write-Host ""

# Example 2: Full Deployment with Pre/Post Scripts
Write-Host "Example 2: Full deployment with pre-deploy and post-deploy scripts" -ForegroundColor Yellow
Write-Host "Command:" -ForegroundColor Gray
Write-Host "  .\DBD.exe \" -ForegroundColor Gray
Write-Host "    --connection `"$ConnectionString`" \" -ForegroundColor Gray
Write-Host "    --dacpac `"$DacpacPath`" \" -ForegroundColor Gray
Write-Host "    --pre-deploy `"$PreDeployScripts`" \" -ForegroundColor Gray
Write-Host "    --post-deploy `"$PostDeployScripts`" \" -ForegroundColor Gray
Write-Host "    --verbose" -ForegroundColor Gray
Write-Host ""

# Example 3: Generate Script Without Deploying
Write-Host "Example 3: Generate deployment script for review (no execution)" -ForegroundColor Yellow
Write-Host "Command:" -ForegroundColor Gray
Write-Host "  .\DBD.exe \" -ForegroundColor Gray
Write-Host "    --connection `"$ConnectionString`" \" -ForegroundColor Gray
Write-Host "    --dacpac `"$DacpacPath`" \" -ForegroundColor Gray
Write-Host "    --generate-script \" -ForegroundColor Gray
Write-Host "    --script-path `".\output\deployment.sql`"" -ForegroundColor Gray
Write-Host ""

# Example 4: Production Deployment with Safety Options
Write-Host "Example 4: Production deployment with all safety features enabled" -ForegroundColor Yellow
Write-Host "Command:" -ForegroundColor Gray
Write-Host "  .\DBD.exe \" -ForegroundColor Gray
Write-Host "    --connection `"$ConnectionString`" \" -ForegroundColor Gray
Write-Host "    --dacpac `"$DacpacPath`" \" -ForegroundColor Gray
Write-Host "    --pre-deploy `"$PreDeployScripts`" \" -ForegroundColor Gray
Write-Host "    --post-deploy `"$PostDeployScripts`" \" -ForegroundColor Gray
Write-Host "    --block-data-loss true \" -ForegroundColor Gray
Write-Host "    --backup true \" -ForegroundColor Gray
Write-Host "    --timeout 600 \" -ForegroundColor Gray
Write-Host "    --verbose" -ForegroundColor Gray
Write-Host ""

# Example 5: Aggressive Deployment (Dev/Test environments)
Write-Host "Example 5: Aggressive deployment for dev/test (allows data loss, drops objects)" -ForegroundColor Yellow
Write-Host "Command:" -ForegroundColor Gray
Write-Host "  .\DBD.exe \" -ForegroundColor Gray
Write-Host "    --connection `"$ConnectionString`" \" -ForegroundColor Gray
Write-Host "    --dacpac `"$DacpacPath`" \" -ForegroundColor Gray
Write-Host "    --block-data-loss false \" -ForegroundColor Gray
Write-Host "    --drop-objects true \" -ForegroundColor Gray
Write-Host "    --continue-on-error" -ForegroundColor Gray
Write-Host ""

# Example 6: Scripts Only (No DACPAC)
Write-Host "Example 6: Run migration scripts only (no DACPAC deployment)" -ForegroundColor Yellow
Write-Host "Command:" -ForegroundColor Gray
Write-Host "  .\DBD.exe \" -ForegroundColor Gray
Write-Host "    --connection `"$ConnectionString`" \" -ForegroundColor Gray
Write-Host "    --post-deploy `"$PostDeployScripts`"" -ForegroundColor Gray
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run any of these examples:" -ForegroundColor Green
Write-Host "1. Build the solution: dotnet build -c Release" -ForegroundColor White
Write-Host "2. Navigate to: src\DatabaseDeployer.Cli\bin\Release\net8.0\" -ForegroundColor White
Write-Host "3. Update the variables at the top of this script with your actual paths" -ForegroundColor White
Write-Host "4. Run the desired command from the examples above" -ForegroundColor White
Write-Host ""
