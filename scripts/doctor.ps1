. $PSScriptRoot/environment.ps1
Invoke-Dotnet --version
Invoke-Dotnet run --project src/FighterPort.Cli -c Release -- doctor
# Bounded discovery; never run discovered tools or include private install paths in output.
$patterns = 'Blender|Autodesk Maya|Autodesk 3ds Max|BrawlCrate|Melee Modding Wizard|SSBH|Rivals.*Modkit'
$keys = @('HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*')
$found = @(Get-ItemProperty $keys -ErrorAction SilentlyContinue | Where-Object DisplayName -Match $patterns | Select-Object DisplayName,DisplayVersion | Sort-Object DisplayName,DisplayVersion -Unique)
if ($found.Count) { $found | Format-Table } else { Write-Host 'No optional tools found in bounded uninstall registry records. Manual overrides/other install layouts are pending.' }
