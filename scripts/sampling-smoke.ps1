param([string]$NativePng,[switch]$Published)
. $PSScriptRoot/environment.ps1
$output = Join-Path $repoRoot 'artifacts/sampling-smoke'
New-Item -ItemType Directory -Force $output | Out-Null
$arguments = @('src/FighterPort.Desktop/bin/Release/net10.0-windows/FighterPort.Desktop.dll','--sampling-smoke',('"' + $output + '"'))
if ($Published) { $arguments = @('--sampling-smoke',('"' + $output + '"')) }
if ($NativePng) { $arguments += ('"' + (Resolve-Path -LiteralPath $NativePng).Path + '"') }
$executable = if ($Published) { Join-Path $repoRoot 'artifacts/publish/desktop/FighterPort.Desktop.exe' } else { $dotnet }
$app = Start-Process -FilePath $executable -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$app.WaitForExit(60000)) { Stop-Process -Id $app.Id; throw 'Sampling smoke timeout' }
Get-Content (Join-Path $output 'sampling.txt')
if ($app.ExitCode -ne 0) { throw 'Sampling smoke failed' }
