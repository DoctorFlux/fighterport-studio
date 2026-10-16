$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
$files = @(git diff --cached --name-only)
foreach ($file in $files) {
    if ($file -match '(^|/)(Brawl mario|Smash Ultimate mario|artifacts|assets-local|\.deps|\.tools|\.local|bin|obj)/|\.(pac|pcs|brres|mdl0|chr0|dat|nusktb|numshb|numdlb|numatb|nuanmb|nutexb|nuhlpb|numshexb|adjb|iso|wbfs|nsp|xci|exe|dll|zip|pem|key|pfx|fpproj|fpwork)$|(^|/)\.env') { throw "Forbidden staged path: $file" }
    if ((Test-Path -LiteralPath $file) -and (Get-Item -LiteralPath $file).Length -gt 1MB) { throw "Review required for large source: $file" }
}
git diff --cached --check
if ($LASTEXITCODE -ne 0) { throw 'Diff whitespace check failed' }
git diff --cached --stat
git diff --cached --name-only
Write-Host 'Also inspect git diff --cached. This guard is not a substitute for human/source review.'
