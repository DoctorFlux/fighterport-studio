param([switch]$Native)
. $PSScriptRoot/environment.ps1
& $PSScriptRoot/capabilities.ps1 -Check
if ($Native) { Invoke-Dotnet run --project tests/FighterPort.Tests -c Release -- --native } else { Invoke-Dotnet run --project tests/FighterPort.Tests -c Release }
