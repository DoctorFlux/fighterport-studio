param([switch]$PinnedImport)
$ErrorActionPreference='Stop'
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Set-Location -LiteralPath $repoRoot
if ((git remote get-url origin) -ne 'https://github.com/DoctorFlux/fighterport-studio.git') { throw 'Unexpected origin' }
$paths=@(git diff --cached --name-only --diff-filter=ACMR)
foreach ($path in $paths) {
  if ($path -match '(^|/)(bin|obj|artifacts|assets-local|\.local|\.deps|\.tools|Brawl mario|Smash Ultimate mario)/|CredentialsHelper\.exe$|\.(snk|pem|pfx|key)$|(^|/)\.env') { throw "Excluded staged path: $path" }
  if ($path -match '\.(exe|dll)$' -and $path -notin @('application/BrawlLib/OpenTK.dll','application/BrawlCrate/discord-rpc.dll')) { throw "Unexpected staged binary: $path" }
}
git diff --cached --shortstat
if ($PinnedImport) {
  # Preserve upstream formatting; exact imported blob IDs are verified separately.
  git diff --cached --check -- . ':!application'
} else {
  git diff --cached --check
}
if ($LASTEXITCODE) { throw 'Staged whitespace check failed' }
Write-Output 'Inspect staged content diff too. Local review only; this script never pushes.'
