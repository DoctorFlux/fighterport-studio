param([switch]$Native)
. $PSScriptRoot/environment.ps1
if ($Native) { Invoke-Dotnet run --project tests/FighterPort.Tests -c Release -- --native } else { Invoke-Dotnet run --project tests/FighterPort.Tests -c Release }
