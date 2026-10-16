param([switch]$Native,[switch]$Ultimate,[switch]$LocalAssets)
. $PSScriptRoot/environment.ps1
& $PSScriptRoot/capabilities.ps1 -Check
$testArgs = @()
if ($Native) { $testArgs += '--native' }
if ($Ultimate) { $testArgs += '--ultimate' }
if ($LocalAssets) { $testArgs += '--local-assets' }
Invoke-Dotnet run --project tests/FighterPort.Tests -c Release -- @testArgs
