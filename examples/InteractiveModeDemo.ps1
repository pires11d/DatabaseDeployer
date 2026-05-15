# Demo of Interactive Mode
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DatabaseDeployer - Interactive Mode Demo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "When you double-click the .exe file (or run it with no arguments)," -ForegroundColor Yellow
Write-Host "it will:" -ForegroundColor Yellow
Write-Host "  1. Show the help menu with all available options" -ForegroundColor White
Write-Host "  2. Prompt you to enter your command" -ForegroundColor White
Write-Host "  3. Execute the command you entered" -ForegroundColor White
Write-Host "  4. Wait for you to press any key before closing" -ForegroundColor White
Write-Host ""
Write-Host "Example session:" -ForegroundColor Green
Write-Host "  > DBD.exe" -ForegroundColor Gray
Write-Host "  [Help menu displays]" -ForegroundColor Gray
Write-Host "  Enter your command: --connection ""Server=localhost;..."" --dacpac ""Database.dacpac""" -ForegroundColor Gray
Write-Host "  [Deployment executes]" -ForegroundColor Gray
Write-Host "  Press any key to exit..." -ForegroundColor Gray
Write-Host ""
Write-Host "To exit without running anything, just type: exit" -ForegroundColor Cyan
Write-Host ""
Write-Host "Try it yourself:" -ForegroundColor Yellow
Write-Host "  cd src\DatabaseDeployer.Cli\bin\Release\net8.0" -ForegroundColor White
Write-Host "  .\DBD.exe" -ForegroundColor White
Write-Host ""
