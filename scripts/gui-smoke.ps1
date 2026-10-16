param([string]$Project)
. $PSScriptRoot/environment.ps1
$relativeOutput = if ($Project) { 'artifacts/gui-native-smoke' } else { 'artifacts/gui-smoke' }
$output = Join-Path $repoRoot $relativeOutput
New-Item -ItemType Directory -Force $output | Out-Null
$arguments = @('src/FighterPort.Desktop/bin/Release/net10.0-windows/FighterPort.Desktop.dll','--smoke',$relativeOutput)
if ($Project) { $arguments += ('"' + (Resolve-Path -LiteralPath $Project).Path + '"') }
$app = Start-Process -FilePath $dotnet -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$app.WaitForExit(20000)) { Stop-Process -Id $app.Id; throw 'GUI smoke timeout' }
if ($app.ExitCode -ne 0) { throw 'GUI smoke failed' }
Get-Content (Join-Path $output 'gui-smoke.txt')
