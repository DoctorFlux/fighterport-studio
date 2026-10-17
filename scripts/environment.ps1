$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
if (!(Test-Path -LiteralPath $vswhere)) { throw 'Visual Studio MSBuild discovery is unavailable; run doctor.ps1.' }
$msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/MSBuild.exe' | Select-Object -First 1
if (!$msbuild -or !(Test-Path -LiteralPath $msbuild)) { throw 'Installed Visual Studio MSBuild is required. No global installation was attempted.' }
$independentTargets = Join-Path $repoRoot 'build/Independent.targets'
