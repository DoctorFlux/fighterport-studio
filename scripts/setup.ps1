. $PSScriptRoot/environment.ps1
if (!(Test-Path '.tools/dotnet/dotnet.exe')) {
    New-Item -ItemType Directory -Force .tools | Out-Null
    Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile '.tools/dotnet-install.ps1'
    & ./.tools/dotnet-install.ps1 -Version '10.0.402' -InstallDir (Join-Path $repoRoot '.tools/dotnet') -NoPath
    if ($LASTEXITCODE -ne 0) { throw 'Local SDK installation failed' }
}
& $PSScriptRoot/prepare-native.ps1
