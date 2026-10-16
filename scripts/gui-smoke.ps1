param([string]$Project, [switch]$Mapping,[switch]$Playback,[string]$WorkspaceModel,[string]$WorkspaceMotion,[switch]$Published)
. $PSScriptRoot/environment.ps1
if (($Mapping -and $Project) -or ($Playback -and !$Project)) { throw 'Mapping uses original fixtures; Playback requires a project' }
if (($WorkspaceModel -and !$WorkspaceMotion) -or ($WorkspaceMotion -and !$WorkspaceModel) -or ($WorkspaceModel -and ($Project -or $Mapping -or $Playback))) { throw 'Workspace smoke requires model and motion sources, without other modes' }
$relativeOutput = if ($WorkspaceModel) { 'artifacts/gui-workspace-' + [IO.Path]::GetFileNameWithoutExtension($WorkspaceModel) } elseif ($Playback) { 'artifacts/gui-playback-' + [IO.Path]::GetFileNameWithoutExtension($Project) } elseif ($Mapping) { 'artifacts/gui-mapping-smoke' } elseif ($Project) { 'artifacts/gui-native-smoke' } else { 'artifacts/gui-smoke' }
$output = Join-Path $repoRoot $relativeOutput
New-Item -ItemType Directory -Force $output | Out-Null
$mode = if ($WorkspaceModel) { '--workspace-smoke' } elseif ($Playback) { '--playback-smoke' } elseif ($Mapping) { '--mapping-smoke' } else { '--smoke' }
$arguments = @('src/FighterPort.Desktop/bin/Release/net10.0-windows/FighterPort.Desktop.dll',$mode,$relativeOutput)
if ($Published) { $arguments = @($mode,$relativeOutput) }
if ($WorkspaceModel) { $arguments += @(('"' + (Resolve-Path -LiteralPath $WorkspaceModel).Path + '"'), ('"' + (Resolve-Path -LiteralPath $WorkspaceMotion).Path + '"')) }
if ($Project) { $arguments += ('"' + (Resolve-Path -LiteralPath $Project).Path + '"') }
$executable = if ($Published) { Join-Path $repoRoot 'artifacts/publish/desktop/FighterPort.Desktop.exe' } else { $dotnet }
$app = Start-Process -FilePath $executable -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$app.WaitForExit(60000)) { Stop-Process -Id $app.Id; throw 'GUI smoke timeout' }
if ($app.ExitCode -ne 0) { throw 'GUI smoke failed' }
$report = if ($WorkspaceModel) { 'workspace-smoke.txt' } elseif ($Playback) { 'playback-smoke.txt' } elseif ($Mapping) { 'mapping-smoke.txt' } else { 'gui-smoke.txt' }
Get-Content (Join-Path $output $report)
