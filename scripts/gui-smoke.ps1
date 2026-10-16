. $PSScriptRoot/environment.ps1
$output = Join-Path $repoRoot 'artifacts/gui-smoke'
New-Item -ItemType Directory -Force $output | Out-Null
$app = Start-Process -FilePath $dotnet -ArgumentList @('src/FighterPort.Desktop/bin/Release/net10.0-windows/FighterPort.Desktop.dll','--smoke','artifacts/gui-smoke') -WindowStyle Hidden -PassThru
if (!$app.WaitForExit(20000)) { Stop-Process -Id $app.Id; throw 'GUI smoke timeout' }
if ($app.ExitCode -ne 0) { throw 'GUI smoke failed' }
Get-Content (Join-Path $output 'gui-smoke.txt')
