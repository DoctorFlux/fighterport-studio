. $PSScriptRoot/environment.ps1
$revision = 'd56cbac89dc2cd0e5c3f923e1d03041b16902add'
$source = '.deps/BrawlCrate'
if (!(Test-Path "$source/.git")) {
    git clone https://github.com/soopercool101/BrawlCrate.git $source
    if ($LASTEXITCODE -ne 0) { throw 'BrawlLib download failed' }
    git -C $source checkout --detach $revision
    if ($LASTEXITCODE -ne 0) { throw 'BrawlLib checkout failed' }
}
if ((git -C $source rev-parse HEAD) -ne $revision) { throw 'Unexpected BrawlLib revision' }
# SDK MSBuild requires this reference for legacy binary WinForms resources.
$project = Join-Path $source 'BrawlLib/BrawlLib.csproj'
$xml = Get-Content $project -Raw
if ($xml -notmatch 'System.Resources.Extensions') {
    $xml = $xml.Replace('</Project>', '<PropertyGroup><GenerateResourceUsePreserializedResources>true</GenerateResourceUsePreserializedResources></PropertyGroup><ItemGroup><PackageReference Include="System.Resources.Extensions" Version="8.0.0" /></ItemGroup></Project>')
    [IO.File]::WriteAllText((Join-Path $repoRoot $project), $xml)
}
# Endian floats must travel through integer registers on legacy x86, otherwise
# reversed IEEE patterns that resemble signaling NaNs are quieted by x87.
$endianFile = Join-Path $source 'BrawlLib/Internal/BigEndianTypes.cs'
$endian = Get-Content $endianFile -Raw
$old = 'return new bfloat {_data = val.Reverse()};'
$new = 'bfloat result = default(bfloat); *(uint*)&result = (*(uint*)&val).Reverse(); return result; // FighterPort: preserve endian bits on x86'
if ($endian.Contains($old)) { [IO.File]::WriteAllText((Join-Path $repoRoot $endianFile), $endian.Replace($old,$new)) }
Copy-Item config/brawllib.packages.lock.json "$source/BrawlLib/packages.lock.json" -Force
# Dense L1/L4 CHR0 streams regenerate tangents on decode and can change fractional
# poses. Keep the genuine backend's indexed full-float encoding for explicit keys.
$converterFile = Join-Path $source 'BrawlLib/Wii/Animations/AnimationConverter.cs'
$converter = Get-Content $converterFile -Raw
$marker = '                //calculate size'
$patch = "                // FighterPort: preserve explicit values and tangents with native I12.`r`n                if (format != AnimDataFormat.None) format = AnimDataFormat.I12;`r`n`r`n$marker"
if (!$converter.Contains('// FighterPort: preserve explicit values and tangents')) {
    if ([regex]::Matches($converter,[regex]::Escape($marker)).Count -ne 1) { throw 'Unexpected CHR0 encoder patch location' }
    [IO.File]::WriteAllText((Join-Path $repoRoot $converterFile), $converter.Replace($marker,$patch))
}
Invoke-Dotnet build $project -c Release '-p:Platform=x86' '-p:RestorePackagesWithLockFile=true' '-p:RestoreLockedMode=true' --nologo '-v:q'
