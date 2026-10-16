. $PSScriptRoot/environment.ps1
$expected = Get-Content config/repository.json -Raw | ConvertFrom-Json
if ((git remote get-url origin) -ne $expected.remote) { throw 'Unexpected origin' }
$actual = gh repo view $expected.nameWithOwner --json nameWithOwner,isPrivate,url | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or !$actual.isPrivate -or $actual.nameWithOwner -ne $expected.nameWithOwner -or $actual.url -ne $expected.url) { throw 'Private remote verification failed' }
if ((git branch --show-current) -ne 'main') { throw 'Expected reviewed main branch' }
git push -u origin main
if ($LASTEXITCODE -ne 0) { throw 'Push failed; local commit retained' }
$remote = gh api "repos/$($expected.nameWithOwner)/commits/main" --jq .sha
if ($LASTEXITCODE -ne 0 -or $remote -ne (git rev-parse HEAD)) { throw 'Remote commit verification failed' }
Write-Host "Verified private push: $remote"
