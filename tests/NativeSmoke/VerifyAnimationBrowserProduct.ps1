# Original-fixture ordinary product entrypoint/restart check, 2026-10-17.
param([Parameter(Mandatory=$true)][string]$Model,[Parameter(Mandatory=$true)][string]$Motion,[string]$SavedSettings)
$ErrorActionPreference='Stop'
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$exe=Join-Path $repoRoot 'application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe'
$evidence=Join-Path $repoRoot ('artifacts/restart/browser-product-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $evidence | Out-Null
Add-Type -AssemblyName System.Drawing,System.Windows.Forms
Add-Type @'
using System;using System.Collections.Generic;using System.Text;using System.Runtime.InteropServices;
public static class AnimationWindows {
 public delegate bool Callback(IntPtr h,IntPtr d);
 [StructLayout(LayoutKind.Sequential)]public struct Rect{public int Left,Top,Right,Bottom;}
 [StructLayout(LayoutKind.Sequential)]public struct Gui{public int Size,Flags;public IntPtr Active,Focus,Capture,Menu,Move,Caret;public Rect Rect;}
 [DllImport("user32.dll")]public static extern bool EnumWindows(Callback c,IntPtr d);
 [DllImport("user32.dll")]public static extern bool EnumChildWindows(IntPtr h,Callback c,IntPtr d);
 [DllImport("user32.dll")]public static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")]public static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
 [DllImport("user32.dll")]public static extern bool GetGUIThreadInfo(uint thread,ref Gui info);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]public static extern int GetClassName(IntPtr h,StringBuilder b,int n);
 [DllImport("user32.dll")]public static extern IntPtr SendMessage(IntPtr h,uint m,IntPtr w,IntPtr l);
 [DllImport("user32.dll")]public static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);
 [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)]public static extern IntPtr SetText(IntPtr h,uint m,IntPtr w,string text);
 [DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=CharSet.Unicode)]public static extern IntPtr ReadText(IntPtr h,uint m,IntPtr w,StringBuilder text);
 [DllImport("user32.dll")]public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")]public static extern bool ShowWindow(IntPtr h,int show);
 [DllImport("user32.dll")]public static extern bool GetWindowRect(IntPtr h,out Rect r);
 [DllImport("user32.dll")]public static extern bool PrintWindow(IntPtr h,IntPtr dc,uint flags);
 public static string Text(IntPtr h){var b=new StringBuilder(65536);ReadText(h,13,new IntPtr(b.Capacity),b);return b.ToString();}
 public static string Item(IntPtr h,int index){var b=new StringBuilder(4096);ReadText(h,0x189,new IntPtr(index),b);return b.ToString();}
 public static string Class(IntPtr h){var b=new StringBuilder(256);GetClassName(h,b,256);return b.ToString();}
 public static IntPtr[] Children(IntPtr h){var a=new List<IntPtr>();EnumChildWindows(h,(c,d)=>{a.Add(c);return true;},IntPtr.Zero);return a.ToArray();}
 public static IntPtr Find(uint pid,string title){IntPtr result=IntPtr.Zero;EnumWindows((h,d)=>{uint p;GetWindowThreadProcessId(h,out p);if(p==pid&&Text(h).Contains(title)){result=h;return false;}return true;},IntPtr.Zero);return result;}
 public static void Workspace(IntPtr main){foreach(var child in Children(main))if(Text(child)=="menuStrip1"){PostMessage(child,0x201,new IntPtr(1),new IntPtr((10<<16)|20));PostMessage(child,0x202,IntPtr.Zero,new IntPtr((10<<16)|20));return;}throw new Exception("Main menu strip missing");}
 public static void WorkspaceKey(IntPtr main){uint pid;uint thread=GetWindowThreadProcessId(main,out pid);var gui=new Gui{Size=Marshal.SizeOf(typeof(Gui))};if(!GetGUIThreadInfo(thread,ref gui)||gui.Focus==IntPtr.Zero)throw new Exception("Menu focus missing");PostMessage(gui.Focus,0x102,new IntPtr('w'),IntPtr.Zero);}
}
'@
function Wait-Browser([scriptblock]$condition,[string]$message){$watch=[Diagnostics.Stopwatch]::StartNew();do{if(&$condition){return};Start-Sleep -Milliseconds 100}while($watch.Elapsed.TotalSeconds -lt 25);throw $message}
function Click-Browser([IntPtr]$window,[string]$text){$b=@([AnimationWindows]::Children($window)|Where-Object{[AnimationWindows]::Text($_) -eq $text -and [AnimationWindows]::IsWindowVisible($_)});if($b.Count -ne 1){throw "Missing/ambiguous control: $text"};$null=[AnimationWindows]::PostMessage($b[0],0xF5,[IntPtr]::Zero,[IntPtr]::Zero);Start-Sleep -Milliseconds 150}
function Capture-Browser([IntPtr]$window,[string]$name){$r=New-Object AnimationWindows+Rect;$null=[AnimationWindows]::GetWindowRect($window,[ref]$r);$b=New-Object Drawing.Bitmap ($r.Right-$r.Left),($r.Bottom-$r.Top);$g=[Drawing.Graphics]::FromImage($b);try{$dc=$g.GetHdc();try{$null=[AnimationWindows]::PrintWindow($window,$dc,2)}finally{$g.ReleaseHdc($dc)};$b.Save((Join-Path $evidence $name),[Drawing.Imaging.ImageFormat]::Png)}finally{$g.Dispose();$b.Dispose()}}
function Open-BrowserSource([IntPtr]$viewer,[string]$path,[bool]$model){Click-Browser $viewer $(if($model){'Open Character…'}else{'Add Source…'});Wait-Browser{[AnimationWindows]::Find($process.Id,'Add native source') -ne [IntPtr]::Zero}'Source picker missing';$picker=[AnimationWindows]::Find($process.Id,'Add native source');$edit=@([AnimationWindows]::Children($picker)|Where-Object{[AnimationWindows]::Class($_) -like '*EDIT*'})[0];$null=[AnimationWindows]::SetText($edit,12,[IntPtr]::Zero,$path);Click-Browser $picker 'Inspect selected file';Wait-Browser{@([AnimationWindows]::Children($picker)|Where-Object{[AnimationWindows]::Text($_) -like 'Select the intended*'}).Count -gt 0}'Source inspection failed';Click-Browser $picker 'Open selected source';Wait-Browser{@([AnimationWindows]::Children($picker)|Where-Object{[AnimationWindows]::Text($_) -like 'Source staged separately*'}).Count -gt 0}'Source staging failed';$null=[AnimationWindows]::PostMessage($picker,0x10,[IntPtr]::Zero,[IntPtr]::Zero);Wait-Browser{[AnimationWindows]::Find($process.Id,'Add native source') -eq [IntPtr]::Zero}'Source picker close failed'}
$prior=$env:FIGHTERPORT_DATA_ROOT;$records=@();$before=@{};foreach($p in @($Model,$Motion)){$before[$p]=(Get-FileHash -LiteralPath $p).Hash}
if($SavedSettings){$before[$SavedSettings]=(Get-FileHash -LiteralPath $SavedSettings).Hash}
try{
 $env:FIGHTERPORT_DATA_ROOT=Join-Path $evidence 'profile'
 $attempts=if($SavedSettings){3}else{2}
 for($attempt=1;$attempt -le $attempts;$attempt++){
  if($SavedSettings -and $attempt -eq 2){$settingsDirectory=Join-Path $env:FIGHTERPORT_DATA_ROOT 'Settings';New-Item -ItemType Directory -Path $settingsDirectory -Force|Out-Null;Copy-Item -LiteralPath $SavedSettings -Destination (Join-Path $settingsDirectory 'settings.xml')}
  $process=Start-Process -FilePath $exe -ArgumentList ('"'+[IO.Path]::GetFullPath($Model)+'"') -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru
  Wait-Browser{[AnimationWindows]::Find($process.Id,'FighterPort Studio') -ne [IntPtr]::Zero}'Main product window missing'
  $main=[AnimationWindows]::Find($process.Id,'FighterPort Studio');$null=[AnimationWindows]::ShowWindow($main,5);$null=[AnimationWindows]::SetForegroundWindow($main)
  Wait-Browser{@([AnimationWindows]::Children($main)|Where-Object{[AnimationWindows]::Text($_) -eq 'menuStrip1'}).Count -gt 0}'Main menu initialization missing'
  [AnimationWindows]::Workspace($main)
  Start-Sleep -Milliseconds 200
  [AnimationWindows]::WorkspaceKey($main)
  Wait-Browser{[AnimationWindows]::Find($process.Id,'Advanced Model Editor') -ne [IntPtr]::Zero}'File > Animation workspace did not open'
  $viewer=[AnimationWindows]::Find($process.Id,'Advanced Model Editor');Open-BrowserSource $viewer ([IO.Path]::GetFullPath($Model)) $true;Open-BrowserSource $viewer ([IO.Path]::GetFullPath($Motion)) $false
  $lists=@([AnimationWindows]::Children($viewer)|Where-Object{[AnimationWindows]::IsWindowVisible($_) -and [AnimationWindows]::Class($_) -like '*LISTBOX*' -and [AnimationWindows]::SendMessage($_,0x18B,[IntPtr]::Zero,[IntPtr]::Zero).ToInt32() -gt 0})
  $list=@($lists|Where-Object{[AnimationWindows]::Item($_,0) -like 'OriginalSwing*'})
  if($list.Count -ne 1){throw 'Visible animation name missing/ambiguous'}
  $clipName=[AnimationWindows]::Item($list[0],0);Click-Browser $viewer 'Preview';Click-Browser $viewer 'Play';Start-Sleep -Milliseconds 300;Click-Browser $viewer 'Stop'
  Capture-Browser $viewer "ordinary-$attempt.png"
  Click-Browser $viewer 'Save/Export…';Wait-Browser{[AnimationWindows]::Find($process.Id,'Save separate native motion') -ne [IntPtr]::Zero}'Native export missing'
  $outputWindow=[AnimationWindows]::Find($process.Id,'Save separate native motion');$output=Join-Path $evidence "motion-$attempt.pac";$edit=@([AnimationWindows]::Children($outputWindow)|Where-Object{[AnimationWindows]::Class($_) -like '*EDIT*'})[0];$null=[AnimationWindows]::SetText($edit,12,[IntPtr]::Zero,$output);Click-Browser $outputWindow 'Export and validate';Wait-Browser{Test-Path -LiteralPath $output}'Native output not produced'
  $null=[AnimationWindows]::PostMessage($outputWindow,0x10,[IntPtr]::Zero,[IntPtr]::Zero);$null=[AnimationWindows]::PostMessage($viewer,0x10,[IntPtr]::Zero,[IntPtr]::Zero);Wait-Browser{[AnimationWindows]::Find($process.Id,'Advanced Model Editor') -eq [IntPtr]::Zero}'Viewer close failed';$null=[AnimationWindows]::PostMessage($main,0x10,[IntPtr]::Zero,[IntPtr]::Zero)
  if(!$process.WaitForExit(10000) -or $process.ExitCode -ne 0){throw 'Product did not exit normally'}
  $records+=[pscustomobject]@{Attempt=$attempt;Settings=$(if($attempt -eq 1){'clean'}elseif($SavedSettings){'copied existing local settings; originals read-only'}else{'persisted first launch'});Clip=$clipName;OutputSHA256=(Get-FileHash -LiteralPath $output).Hash;NormalExit=$true;ExecutableSHA256=(Get-FileHash -LiteralPath $exe).Hash}
 }
 foreach($p in $before.Keys){if((Get-FileHash -LiteralPath $p).Hash -ne $before[$p]){throw 'Original input changed'}}
 $records|ConvertTo-Json -Depth 4|Set-Content -LiteralPath (Join-Path $evidence 'result.json');Write-Output $evidence
}finally{if($process -and !$process.HasExited){$null=$process.CloseMainWindow();if(!$process.WaitForExit(2000)){$process.Kill()}};$env:FIGHTERPORT_DATA_ROOT=$prior}
