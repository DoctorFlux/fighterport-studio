param([ValidateSet('Debug','Release')][string]$Configuration='Release', [switch]$UpdateLocks)
. $PSScriptRoot/environment.ps1
$project = Join-Path $repoRoot 'application/BrawlCrate/BrawlCrate.csproj'
$output = [IO.Path]::GetFullPath((Join-Path $repoRoot "application/BrawlCrate/bin/x86/$Configuration/net472"))
if (!$output.StartsWith($repoRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Output escaped workspace' }
$logDir = Join-Path $repoRoot 'artifacts/restart'
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$locked = (Test-Path -LiteralPath (Join-Path $repoRoot 'application/BrawlCrate/packages.lock.json')) -and !$UpdateLocks
& $msbuild $project /restore /t:Build /m /nologo /v:minimal "/p:Configuration=$Configuration" /p:Platform=x86 "/p:CustomAfterMicrosoftCommonTargets=$independentTargets" /p:RestorePackagesWithLockFile=true "/p:RestoreLockedMode=$locked" /fl "/flp:logfile=$logDir/build-$Configuration.log"
if ($LASTEXITCODE -ne 0) { throw "Build failed ($LASTEXITCODE); see $logDir" }
$exe = Join-Path $output 'FighterPort.Studio.exe'
if (!(Test-Path -LiteralPath $exe)) { throw "Expected application missing: $exe" }
Write-Output $exe
