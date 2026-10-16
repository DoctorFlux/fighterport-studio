param([switch]$Native,[switch]$Ultimate,[switch]$LocalAssets,[switch]$WorkspaceOnly)
. $PSScriptRoot/environment.ps1
& $PSScriptRoot/capabilities.ps1 -Check
$testArgs = @()
if ($Native) { $testArgs += '--native' }
if ($Ultimate) { $testArgs += '--ultimate' }
if ($LocalAssets) { $testArgs += '--local-assets' }
if ($WorkspaceOnly) { $testArgs += '--workspace-only' }
Invoke-Dotnet run --project tests/FighterPort.Tests -c Release -- @testArgs
