param([switch]$Smoke)
. $PSScriptRoot/environment.ps1
if ($Smoke) { Invoke-Dotnet run --project src/FighterPort.Desktop -c Release -- --smoke artifacts/run-script-smoke } else { Invoke-Dotnet run --project src/FighterPort.Desktop -c Release }
