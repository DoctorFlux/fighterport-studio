param([ValidateSet('Debug','Release')][string]$Configuration='Release', [switch]$Gui, [switch]$Companions)
. $PSScriptRoot/environment.ps1
$project = Join-Path $repoRoot 'tests/NativeSmoke/NativeSmoke.csproj'
$logDir = Join-Path $repoRoot 'artifacts/restart'
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
& $msbuild $project /restore /t:Build /m /nologo /v:minimal "/p:Configuration=$Configuration" /p:Platform=x86 "/p:CustomAfterMicrosoftCommonTargets=$independentTargets" /p:RestoreLockedMode=true /fl "/flp:logfile=$logDir/test-build-$Configuration.log"
if ($LASTEXITCODE -ne 0) { throw "Test build failed ($LASTEXITCODE); see $logDir" }
$exe = Join-Path $repoRoot "tests/NativeSmoke/bin/x86/$Configuration/net472/NativeSmoke.exe"
if (!(Test-Path -LiteralPath $exe)) { throw "Test executable missing: $exe" }
$modes = @('native')
if ($Gui) { $modes += 'gui' }
if ($Companions) {
    $modes = @('companions-native')
    if ($Gui) { $modes += 'companions-gui' }
}
foreach ($mode in $modes) {
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
    $evidence = Join-Path $logDir "$mode-$stamp"
    # GUI mode intentionally opens the actual editor on the interactive desktop.
    & $exe "--$mode" $evidence
    if ($LASTEXITCODE -ne 0) { throw "$mode check failed ($LASTEXITCODE); see $evidence/result.txt" }
    Write-Output "Passed $mode checks: $evidence"
}
