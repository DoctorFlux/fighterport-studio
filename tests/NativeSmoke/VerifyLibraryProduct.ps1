# FighterPort original-fixture entrypoint/restart check, 2026-10-17. No game data.
param([Parameter(Mandatory=$true)][string]$Project)
$ErrorActionPreference='Stop'
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$exe=Join-Path $repoRoot 'application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe'
$evidence=Join-Path $repoRoot ('artifacts/restart/library-product-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $evidence | Out-Null
$savedProject=Join-Path $evidence 'restart.fpcharacter'
Copy-Item -LiteralPath $Project -Destination $savedProject
[xml]$expected=Get-Content -LiteralPath $savedProject -Raw
$instance=@($expected.CharacterProjectFile.State.Instances.MoveInstance)[0]
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class LibraryWindows {
 public delegate bool EnumProc(IntPtr hwnd,IntPtr data);
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; }
 [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback,IntPtr data);
 [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent,EnumProc callback,IntPtr data);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetClassName(IntPtr hwnd,StringBuilder name,int count);
 [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hwnd,uint msg,IntPtr wp,IntPtr lp);
 [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)] public static extern IntPtr SendText(IntPtr hwnd,uint msg,IntPtr wp,StringBuilder text);
 [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)] public static extern IntPtr SetText(IntPtr hwnd,uint msg,IntPtr wp,string text);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd,IntPtr dc,uint flags);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hwnd,uint msg,IntPtr wp,IntPtr lp);
 public static string Text(IntPtr hwnd) { var text=new StringBuilder(65536);SendText(hwnd,13,new IntPtr(text.Capacity),text);return text.ToString(); }
 public static string Class(IntPtr hwnd) {var text=new StringBuilder(256);GetClassName(hwnd,text,text.Capacity);return text.ToString();}
 public static IntPtr[] Children(IntPtr hwnd) {var found=new List<IntPtr>();EnumChildWindows(hwnd,(h,d)=>{found.Add(h);return true;},IntPtr.Zero);return found.ToArray();}
 public static IntPtr Find(uint pid,string title) {IntPtr found=IntPtr.Zero;EnumWindows((h,d)=>{uint p;GetWindowThreadProcessId(h,out p);if(p==pid && IsWindowVisible(h) && Text(h).Contains(title)){found=h;return false;}return true;},IntPtr.Zero);return found;}
}
'@
function Wait-Product([scriptblock]$condition,[string]$message) {
 $watch=[Diagnostics.Stopwatch]::StartNew()
 do {if (& $condition) {return};Start-Sleep -Milliseconds 100} while($watch.Elapsed.TotalSeconds -lt 25)
 throw $message
}
function Capture-Library([IntPtr]$handle,[string]$name) {
 $rect=New-Object LibraryWindows+Rect
 if (![LibraryWindows]::GetWindowRect($handle,[ref]$rect)) {throw 'Window rectangle missing'}
 $bitmap=New-Object Drawing.Bitmap ($rect.Right-$rect.Left),($rect.Bottom-$rect.Top)
 $graphics=[Drawing.Graphics]::FromImage($bitmap)
 try {$dc=$graphics.GetHdc();try {if (![LibraryWindows]::PrintWindow($handle,$dc,2)) {throw 'Capture failed'}} finally {$graphics.ReleaseHdc($dc)};$bitmap.Save((Join-Path $evidence $name),[Drawing.Imaging.ImageFormat]::Png)} finally {$graphics.Dispose();$bitmap.Dispose()}
}
function Click-Library([IntPtr]$handle,[string]$text) {
 $button=@([LibraryWindows]::Children($handle) | Where-Object {[LibraryWindows]::Text($_) -eq $text}) | Select-Object -First 1
 if (!$button) {throw "Missing product button: $text"}
 $null=[LibraryWindows]::SendMessage($button,0xF5,[IntPtr]::Zero,[IntPtr]::Zero)
}
$prior=$env:FIGHTERPORT_DATA_ROOT
$records=@()
try {
 for($attempt=1;$attempt -le 2;$attempt++) {
  $env:FIGHTERPORT_DATA_ROOT=Join-Path $evidence "profile-$attempt"
  $process=Start-Process -FilePath $exe -ArgumentList ('"'+$savedProject+'"') -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru
  Wait-Product { [LibraryWindows]::Find($process.Id,'Character Library') -ne [IntPtr]::Zero } 'Product project window did not open'
  $window=[LibraryWindows]::Find($process.Id,'Character Library')
  $children=[LibraryWindows]::Children($window)
  $tree=@($children | Where-Object {[LibraryWindows]::Class($_) -like '*SysTreeView32*'})[0]
  $root=[LibraryWindows]::SendMessage($tree,0x110A,[IntPtr]::Zero,[IntPtr]::Zero)
  $clip=[LibraryWindows]::SendMessage($tree,0x110A,[IntPtr]4,$root)
  $null=[LibraryWindows]::SendMessage($tree,0x110B,[IntPtr]9,$clip)
  $list=@($children | Where-Object {[LibraryWindows]::Class($_) -like '*SysListView32*'})[0]
  $null=[LibraryWindows]::SendMessage($list,0x100,[IntPtr]0x24,[IntPtr]::Zero)
  Wait-Product { [LibraryWindows]::SendMessage($list,0x100C,[IntPtr](-1),[IntPtr]2).ToInt32() -eq 0 } 'First target instance was not selected'
  $texts=@([LibraryWindows]::Children($window) | ForEach-Object {[LibraryWindows]::Text($_)})
  if (!($texts -match 'Sources verified') -or !($texts -match 'Source profile') -or !($texts -match 'Manual draft:')) {throw 'Restored source/draft data not visible'}
  Capture-Library $window "project-$attempt.png"
  Click-Library $window 'Open instance in native editor'
  Wait-Product {[LibraryWindows]::Find($process.Id,'Advanced Model Editor') -ne [IntPtr]::Zero} 'Actual native model editor did not open'
  $native=@(Get-ChildItem -LiteralPath (Join-Path $env:FIGHTERPORT_DATA_ROOT 'CharacterSessions') -Recurse -File -Filter '*.brres')
  if ($native.Count -ne 1 -or (Get-FileHash -LiteralPath $native[0].FullName -Algorithm SHA256).Hash.ToLowerInvariant() -ne $instance.WorkingHash) {throw 'Product did not restore exact edited native data'}
  Capture-Library ([LibraryWindows]::Find($process.Id,'Advanced Model Editor')) "native-$attempt.png"
  $output=Join-Path $evidence "product-output-$attempt.brres"
  $edit=@([LibraryWindows]::Children($window) | Where-Object {[LibraryWindows]::Class($_) -like '*EDIT*' -and [LibraryWindows]::Text($_) -eq ''})
  if($edit.Count -ne 1) {throw 'Native output path control not found unambiguously'}
  $null=[LibraryWindows]::SetText($edit[0],12,[IntPtr]::Zero,$output)
  Click-Library $window 'Export native copy...'
  Wait-Product {Test-Path -LiteralPath $output} 'Product failed to export native copy'
  if((Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash.ToLowerInvariant() -ne $instance.WorkingHash) {throw 'Native output differs from saved instance revision'}
  Click-Library $window 'Save project'
  [xml]$actual=Get-Content -LiteralPath $savedProject -Raw
  if($actual.CharacterProjectFile.State.OuterXml -ne $expected.CharacterProjectFile.State.OuterXml) {throw 'Product altered saved source bindings/edits/assignments'}
  $nativeWindow=[LibraryWindows]::Find($process.Id,$native[0].Name)
  if($nativeWindow -eq [IntPtr]::Zero) {throw 'Native main resource window missing'}
  $null=[LibraryWindows]::PostMessage($nativeWindow,0x10,[IntPtr]::Zero,[IntPtr]::Zero)
  if(!$process.WaitForExit(10000) -or $process.ExitCode -ne 0) {throw 'Product did not exit normally'}
  $records += [pscustomobject]@{Attempt=$attempt;Project=$savedProject;Instance=$instance.Id;Slot=$instance.Slot;NativeOutput=$output;SHA256=$instance.WorkingHash;NormalExit=$true}
 }
 $records | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidence 'result.json') -Encoding UTF8
 Write-Output $evidence
 $records | ConvertTo-Json -Depth 4
} finally {
 if($process -and !$process.HasExited) {$null=$process.CloseMainWindow();if(!$process.WaitForExit(3000)){$process.Kill()}}
 $env:FIGHTERPORT_DATA_ROOT=$prior
}
