param([switch]$Native)
. $PSScriptRoot/environment.ps1
if ($Native) { & $PSScriptRoot/prepare-native.ps1 }
Invoke-Dotnet build FighterPort.sln -c Release '-p:RestoreLockedMode=true' --nologo
if ($Native) { Invoke-Dotnet build src/FighterPort.NativeWorker -c Release '-p:RestoreLockedMode=true' --nologo }
