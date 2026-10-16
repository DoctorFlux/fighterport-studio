. $PSScriptRoot/environment.ps1
& $PSScriptRoot/build.ps1 -Native
$destination = Join-Path $repoRoot 'artifacts/publish'
Invoke-Dotnet publish src/FighterPort.Desktop -c Release --no-restore --self-contained false -o "$destination/desktop"
Invoke-Dotnet publish src/FighterPort.Cli -c Release --no-restore --self-contained false -o "$destination/cli"
New-Item -ItemType Directory -Force "$destination/native", "$destination/notices" | Out-Null
Copy-Item src/FighterPort.NativeWorker/bin/Release/net472/* "$destination/native" -Recurse -Force
Copy-Item .deps/BrawlCrate/LICENSE "$destination/notices/BrawlLib-LGPL-3.0.txt" -Force
Copy-Item docs/DEPENDENCIES.md "$destination/notices/DEPENDENCIES.md" -Force
# Redistribution preparation includes corresponding upstream source and exact local changes.
git -C .deps/BrawlCrate archive --format=zip --output="$destination/notices/BrawlLib-source.zip" HEAD
if ($LASTEXITCODE -ne 0) { throw 'Source archive failed' }
git -C .deps/BrawlCrate diff --output="$destination/notices/BrawlLib-local-changes.patch"
if ($LASTEXITCODE -ne 0) { throw 'Backend patch export failed' }
Write-Host "Local framework-dependent build staged in $destination. No release uploaded. Final redistribution license audit remains required."
