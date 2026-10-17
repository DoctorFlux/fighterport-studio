param([ValidateSet('Debug','Release')][string]$Configuration='Release')
$ErrorActionPreference='Stop'
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $repoRoot "application/BrawlCrate/bin/x86/$Configuration/net472/FighterPort.Studio.exe"
if (!(Test-Path -LiteralPath $exe)) { throw 'Build first: ./scripts/build.ps1' }
# This command explicitly launches the user's interactive application.
Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe)
