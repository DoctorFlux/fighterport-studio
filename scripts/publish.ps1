param([string]$Destination='artifacts/publish')
. $PSScriptRoot/environment.ps1
& $PSScriptRoot/build.ps1 -Native
$destination = [IO.Path]::GetFullPath((Join-Path $repoRoot $Destination))
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (!$destination.StartsWith($artifactRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Publish destination must be inside this workspace artifacts directory' }
Invoke-Dotnet publish src/FighterPort.Desktop -c Release --no-restore --self-contained false -o "$destination/desktop"
Invoke-Dotnet publish src/FighterPort.Cli -c Release --no-restore --self-contained false -o "$destination/cli"
New-Item -ItemType Directory -Force "$destination/native", "$destination/notices" | Out-Null
Copy-Item src/FighterPort.NativeWorker/bin/Release/net472/* "$destination/native" -Recurse -Force
Copy-Item .deps/BrawlCrate/LICENSE "$destination/notices/BrawlLib-LGPL-3.0.txt" -Force
Copy-Item docs/DEPENDENCIES.md "$destination/notices/DEPENDENCIES.md" -Force
$lock = Get-Content src/FighterPort.NativeWorker/packages.lock.json -Raw | ConvertFrom-Json -AsHashtable
foreach ($target in $lock.dependencies.Values) {
    foreach ($entry in $target.GetEnumerator()) {
        if (!$entry.Value.resolved) { continue }
        $package = Join-Path $env:NUGET_PACKAGES "$($entry.Key.ToLowerInvariant())/$($entry.Value.resolved)"
        $noticeDir = Join-Path "$destination/notices" "$($entry.Key)-$($entry.Value.resolved)"
        New-Item -ItemType Directory -Force $noticeDir | Out-Null
        Get-ChildItem -LiteralPath $package -File | Where-Object Name -Match 'LICENSE|NOTICE' | Copy-Item -Destination $noticeDir -Force
    }
}
# Redistribution preparation includes corresponding upstream source and exact local changes.
git -C .deps/BrawlCrate archive --format=zip --output="$destination/notices/BrawlLib-source.zip" HEAD
if ($LASTEXITCODE -ne 0) { throw 'Source archive failed' }
git -C .deps/BrawlCrate diff --output="$destination/notices/BrawlLib-local-changes.patch"
if ($LASTEXITCODE -ne 0) { throw 'Backend patch export failed' }
Write-Host "Local framework-dependent build staged in $destination. No release uploaded. Final redistribution license audit remains required."
