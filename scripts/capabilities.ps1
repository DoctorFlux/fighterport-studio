param([switch]$Check)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$data = Get-Content (Join-Path $root 'config/capabilities.json') -Raw | ConvertFrom-Json
$lines = @('# Format operation evidence', '', 'Generated from config/capabilities.json. SyntheticTested is not retail/native-tool/in-game compatibility. Grouped planned formats must split per version/resource when researched.', '', '| Game | Format/version | Operation | State | Evidence / limitations |', '|---|---|---|---|---|')
foreach ($op in $data.operations) { $lines += "| $($op.game) | $($op.format) $($op.version) | $($op.operation) | $($op.state) | $($op.evidence) $($op.limitations) |" }
$text = ($lines -join "`n") + "`n"
$path = Join-Path $root 'docs/FORMAT_SUPPORT.md'
if ($Check) { if (([IO.File]::ReadAllText($path).Replace("`r`n","`n")) -ne $text) { throw 'Capability JSON and format documentation differ; run scripts/capabilities.ps1' } } else { [IO.File]::WriteAllText($path,$text) }
