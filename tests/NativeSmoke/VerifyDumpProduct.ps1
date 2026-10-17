# FighterPort original-fixture product-entrypoint verification, 2026-10-17. No game content.
param([Parameter(Mandatory=$true)][string]$Resolution)
$ErrorActionPreference='Stop'
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$exe=Join-Path $repoRoot 'application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe'
$evidence=Join-Path $repoRoot ('artifacts/restart/dump-product-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $evidence | Out-Null
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class DumpProductWindows {
 public delegate bool EnumProc(IntPtr hwnd, IntPtr data);
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; }
 [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr data);
 [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback, IntPtr data);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr hwnd,StringBuilder text,int count);
 [DllImport("user32.dll")] public static extern bool IsWindowEnabled(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hwnd,uint msg,IntPtr wp,IntPtr lp);
 [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hwnd,uint msg,IntPtr wp,IntPtr lp);
 [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)] public static extern IntPtr SendText(IntPtr hwnd,uint msg,IntPtr wp,StringBuilder text);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd,IntPtr dc,uint flags);
 public static string Text(IntPtr hwnd) {var s=new StringBuilder(32768);SendText(hwnd,0x000D,new IntPtr(s.Capacity),s);return s.ToString();}
 public static IntPtr[] Children(IntPtr hwnd) {var r=new List<IntPtr>();EnumChildWindows(hwnd,(h,d)=>{r.Add(h);return true;},IntPtr.Zero);return r.ToArray();}
 public static IntPtr Find(uint pid,string title) {IntPtr found=IntPtr.Zero;EnumWindows((h,d)=>{uint p;GetWindowThreadProcessId(h,out p);if(p==pid && IsWindowVisible(h) && Text(h).Contains(title)){found=h;return false;}return true;},IntPtr.Zero);return found;}
}
'@
function Capture-CompanionWindow([IntPtr]$handle,[string]$path) {
 $rect=New-Object DumpProductWindows+Rect
 if (![DumpProductWindows]::GetWindowRect($handle,[ref]$rect)) {throw 'Window rectangle unavailable'}
 $bitmap=New-Object Drawing.Bitmap ($rect.Right-$rect.Left),($rect.Bottom-$rect.Top)
 $graphics=[Drawing.Graphics]::FromImage($bitmap)
 try {
  $dc=$graphics.GetHdc()
  try {if (![DumpProductWindows]::PrintWindow($handle,$dc,2)) {throw 'App-only capture failed'}} finally {$graphics.ReleaseHdc($dc)}
  $bitmap.Save($path,[Drawing.Imaging.ImageFormat]::Png)
 } finally {$graphics.Dispose();$bitmap.Dispose()}
}
$prior=$env:FIGHTERPORT_DATA_ROOT
try {
 $env:FIGHTERPORT_DATA_ROOT=Join-Path $evidence 'profile'
 $records=@()
 for($attempt=1;$attempt -le 2;$attempt++) {
  $inputPath=if($attempt -eq 1){$Resolution}else{Join-Path $env:FIGHTERPORT_DATA_ROOT 'CompanionResolutions/last.fpcompanion'}
  $process=Start-Process -FilePath $exe -ArgumentList ('"'+$inputPath+'"') -WorkingDirectory (Split-Path $exe) -WindowStyle Normal -PassThru
  $null=$process.WaitForInputIdle(10000)
  $watch=[Diagnostics.Stopwatch]::StartNew()
  $button=[IntPtr]::Zero
  do {
   Start-Sleep -Milliseconds 200
   $process.Refresh()
   if($process.HasExited){throw 'Product exited before resolution loaded'}
   $handle=[DumpProductWindows]::Find($process.Id,'Brawl Companions')
   $children=if($handle -ne [IntPtr]::Zero){[DumpProductWindows]::Children($handle)}else{@()}
   $texts=@($children | ForEach-Object {[DumpProductWindows]::Text($_)})
   $button=@($children | Where-Object {[DumpProductWindows]::Text($_) -eq 'Open assembled copy'}) | Select-Object -First 1
  } until (($button -and [DumpProductWindows]::IsWindowEnabled($button) -and ($texts -match 'OriginalSwing')) -or $watch.Elapsed.TotalSeconds -gt 25)
  if($handle -eq [IntPtr]::Zero){throw 'Companion product window not found'}
  Capture-CompanionWindow $handle (Join-Path $evidence "resume-$attempt.png")
  if(!$button -or ![DumpProductWindows]::IsWindowEnabled($button) -or !($texts -match 'OriginalSwing') -or !($texts -match 'OriginalPaddle')) {throw 'Product failed to restore explicit choices'}
  $add=@($children | Where-Object {[DumpProductWindows]::Text($_) -eq 'Add source files…'}) | Select-Object -First 1
  if(![DumpProductWindows]::IsWindowEnabled($handle) -or !$add -or ![DumpProductWindows]::IsWindowEnabled($add)){throw 'Restored workspace controls did not return to an interactive state'}
  $dumpButton=@($children | Where-Object {[DumpProductWindows]::Text($_) -eq 'Dump folders…'}) | Select-Object -First 1
  if(!$dumpButton) {throw 'Dump folders action missing'}
  # Post the click: the modal window needs the product's message loop to continue.
  $null=[DumpProductWindows]::PostMessage($dumpButton,0x00F5,[IntPtr]::Zero,[IntPtr]::Zero)
  $watch.Restart()
  do {Start-Sleep -Milliseconds 100;$dumpWindow=[DumpProductWindows]::Find($process.Id,'Brawl dump folders')} until($dumpWindow -ne [IntPtr]::Zero -or $watch.Elapsed.TotalSeconds -gt 10)
  if($dumpWindow -eq [IntPtr]::Zero) {throw 'Actual product did not restore dump workspace'}
  Capture-CompanionWindow $dumpWindow (Join-Path $evidence "roots-$attempt.png")
  $return=@([DumpProductWindows]::Children($dumpWindow) | Where-Object {[DumpProductWindows]::Text($_) -eq 'Return to companions'}) | Select-Object -First 1
  if(!$return) {throw 'Dump return action missing'}
  $null=[DumpProductWindows]::SendMessage($return,0x00F5,[IntPtr]::Zero,[IntPtr]::Zero)
  $null=[DumpProductWindows]::SendMessage($button,0x00F5,[IntPtr]::Zero,[IntPtr]::Zero)
  $watch.Restart()
  do {Start-Sleep -Milliseconds 200;$process.Refresh()} until ($process.MainWindowTitle -match 'assembled.brres' -or $watch.Elapsed.TotalSeconds -gt 25)
  if($process.MainWindowTitle -notmatch 'assembled.brres'){throw 'Product did not open assembled native output'}
  # The selected texture is a ListView subitem, not window text. Verify all exact
  # resource identities in the resolution that the actual product just persisted.
  [xml]$saved=Get-Content -LiteralPath (Join-Path $env:FIGHTERPORT_DATA_ROOT 'CompanionResolutions/last.fpcompanion') -Raw
  [xml]$expected=Get-Content -LiteralPath $Resolution -Raw
  $expectedChoices=@($expected.fighterportCompanions.choice | ForEach-Object { $_.OuterXml } | Sort-Object)
  $actualChoices=@($saved.fighterportCompanions.choice | ForEach-Object { $_.OuterXml } | Sort-Object)
  if (@(Compare-Object $expectedChoices $actualChoices).Count -ne 0) {throw 'Actual product changed persisted dependency identities'}
  if ($saved.fighterportCompanions.brawlDumpRoots.OuterXml -ne $expected.fighterportCompanions.brawlDumpRoots.OuterXml) {throw 'Product changed configured roots or priorities'}
  $actualSources=@($saved.fighterportCompanions.source | ForEach-Object {$_.OuterXml} | Sort-Object)
  $expectedSources=@($expected.fighterportCompanions.source | ForEach-Object {$_.OuterXml} | Sort-Object)
  if (@(Compare-Object $actualSources $expectedSources).Count -ne 0) {throw 'Product changed pinned hashes or source lineage'}
  Capture-CompanionWindow $process.MainWindowHandle (Join-Path $evidence "native-open-$attempt.png")
  $records += [pscustomobject]@{Attempt=$attempt;PID=$process.Id;Resolution=$inputPath;NativeTitle=$process.MainWindowTitle;Choices=$texts | Where-Object {$_ -match 'OriginalPaddle|OriginalQuadrants|OriginalSwing'}}
  $null=$process.CloseMainWindow()
  if(!$process.WaitForExit(10000) -or $process.ExitCode -ne 0){throw 'Product did not exit normally'}
 }
 $records | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidence 'result.json') -Encoding UTF8
 Write-Output $evidence
 $records | ConvertTo-Json -Depth 4
} finally {
 if ($process -and !$process.HasExited) {
  $null=$process.CloseMainWindow()
  if (!$process.WaitForExit(3000)) {$process.Kill()}
 }
 $env:FIGHTERPORT_DATA_ROOT=$prior
}
