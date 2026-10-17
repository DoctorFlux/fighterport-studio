$ErrorActionPreference='Stop'
. $PSScriptRoot/environment.ps1
[pscustomobject]@{Windows=[Environment]::OSVersion;MSBuild=$msbuild;Framework472TargetingPack=(Test-Path "${env:ProgramFiles(x86)}/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2");Cargo=[bool](Get-Command cargo -ErrorAction SilentlyContinue);Runtime='net472 / x86';Publication=((Get-Content (Join-Path $repoRoot 'config/repository.json') -Raw | ConvertFrom-Json).publication + '; binary distribution audit pending')} | Format-List
& $msbuild -version -nologo
git -C $repoRoot status --short --branch
