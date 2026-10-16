. $PSScriptRoot/environment.ps1
$expected = Get-Content config/repository.json -Raw | ConvertFrom-Json
if ($expected.private -isnot [bool] -or $expected.nameWithOwner -ne 'DoctorFlux/fighterport-studio' -or $expected.branch -ne 'main') { throw 'Malformed repository policy' }
$origin = git remote get-url origin
if ($LASTEXITCODE -ne 0 -or $origin -ne $expected.remote) { throw 'Unexpected origin' }
$response = gh repo view $expected.nameWithOwner --json nameWithOwner,isPrivate,url
if ($LASTEXITCODE -ne 0) { throw 'Live repository query failed' }
$actual = $response | ConvertFrom-Json -ErrorAction Stop
if ($actual.isPrivate -isnot [bool] -or ($actual.isPrivate -ne $expected.private) -or $actual.nameWithOwner -ne $expected.nameWithOwner -or $actual.url -ne $expected.url) { throw 'Exact remote/visibility verification failed' }
$branch = git branch --show-current
if ($LASTEXITCODE -ne 0 -or $branch -ne $expected.branch) { throw 'Expected reviewed main branch' }
$head = git rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or $head -notmatch '^[a-f0-9]{40}$') { throw 'Invalid local commit' }
git push -u origin main
if ($LASTEXITCODE -ne 0) { throw 'Push failed; local commit retained' }
$remote = gh api "repos/$($expected.nameWithOwner)/commits/main" --jq .sha
if ($LASTEXITCODE -ne 0 -or $remote -ne $head) { throw 'Remote commit verification failed' }
New-Item -ItemType Directory -Force .local/checkpoints | Out-Null
@{ commit=$head; remote=$remote; repository=$actual.url; private=$actual.isPrivate; verifiedUtc=[DateTime]::UtcNow.ToString('o') } | ConvertTo-Json | Set-Content -LiteralPath ".local/checkpoints/$head.json"
Write-Host "Verified expected-visibility push: $remote"
