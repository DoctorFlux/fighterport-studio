param([string]$Project, [switch]$Mapping)
. $PSScriptRoot/environment.ps1
$relativeOutput = if ($Mapping) { 'artifacts/gui-mapping-smoke' } elseif ($Project) { 'artifacts/gui-native-smoke' } else { 'artifacts/gui-smoke' }
$output = Join-Path $repoRoot $relativeOutput
New-Item -ItemType Directory -Force $output | Out-Null
$mode = if ($Mapping) { '--mapping-smoke' } else { '--smoke' }
$arguments = @('src/FighterPort.Desktop/bin/Release/net10.0-windows/FighterPort.Desktop.dll',$mode,$relativeOutput)
if ($Project) { $arguments += ('"' + (Resolve-Path -LiteralPath $Project).Path + '"') }
$app = Start-Process -FilePath $dotnet -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$app.WaitForExit(20000)) { Stop-Process -Id $app.Id; throw 'GUI smoke timeout' }
if ($app.ExitCode -ne 0) { throw 'GUI smoke failed' }
$report = if ($Mapping) { 'mapping-smoke.txt' } else { 'gui-smoke.txt' }
Get-Content (Join-Path $output $report)
