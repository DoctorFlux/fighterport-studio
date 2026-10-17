param([Parameter(Mandatory)][string]$Fixtures,[switch]$Published,[string]$PublishRoot='artifacts/publish')
. $PSScriptRoot/environment.ps1
$output = Join-Path $repoRoot ('artifacts/classic-smoke-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Force $output | Out-Null
$arguments = @('--classic-smoke',('"' + $output + '"'),('"' + (Resolve-Path -LiteralPath (Join-Path $Fixtures 'workspace-model.pac')).Path + '"'),('"' + (Resolve-Path -LiteralPath (Join-Path $Fixtures 'workspace-motion.pac')).Path + '"'))
$executable = if ($Published) { Join-Path $repoRoot "$PublishRoot/desktop/FighterPort.Desktop.exe" } else { $dotnet }
if (!$Published) { $arguments = @('src/FighterPort.Desktop/bin/Release/net10.0-windows/FighterPort.Desktop.dll') + $arguments }
$app = Start-Process -FilePath $executable -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$app.WaitForExit(120000)) { Stop-Process -Id $app.Id; throw "Classic GUI timeout; $output" }
Get-Content (Join-Path $output 'classic-smoke.txt')
if ($app.ExitCode -ne 0) { throw "Classic GUI failed; $output" }
Write-Host "Original running-app captures: $output"
