using FighterPort.Core;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FighterPort.Desktop;

internal sealed class MappingWindow : Window
{
    private CharacterProject source,target;
    private MappingSession session;
    private CharacterProject? result;
    private readonly StackPanel rows=new();
    private readonly Dictionary<string,ComboBox> assignments=new();
    private readonly ComboBox clips=new(){MinWidth=140},policy=new(){ItemsSource=new[]{"Reject","CopyDelta"},Width=110};
    private readonly CheckBox approval=new(){Content="I reviewed and approve this mapping",Margin=new(8),Foreground=Brushes.White};
    private readonly Slider time=new(){Minimum=0,Width=330};
    private readonly TextBlock message=new(){TextWrapping=TextWrapping.Wrap,Margin=new(8),Foreground=Brushes.LightSteelBlue};
    private readonly TextBlock names=new(){Margin=new(8),Foreground=Brushes.White};
    private readonly CharacterPreview left=new(),right=new();
    private bool refreshing;
    private sealed record Choice(string Id,string Name);

    public MappingWindow(CharacterProject current,int clipIndex=0)
    {
        source=ProjectStore.Clone(current);target=Demo.Create();session=new(source,target);
        Title="FighterPort • Manual mapping / restricted FK";Width=1250;Height=850;MinWidth=950;MinHeight=650;Background=new SolidColorBrush(Color.FromRgb(15,22,31));
        var dock=new DockPanel();Content=dock;
        var top=new StackPanel{Margin=new(10)};DockPanel.SetDock(top,Dock.Top);dock.Children.Add(top);
        top.Children.Add(new TextBlock{Text="MANUAL BONE MAPPING",FontSize=23,Foreground=Brushes.White});
        top.Children.Add(new TextBlock{Text="Restricted FK: identical local rest axes, unit rest scale, matching mapped ancestors. Rotation deltas only; root translation needs CopyDelta. No IK or anatomical inference. Orion's rotated root is rejected.",TextWrapping=TextWrapping.Wrap,Foreground=Brushes.LightSteelBlue,Margin=new(0,8,0,8)});
        var tools=new WrapPanel();top.Children.Add(tools);
        Add(tools,"Load source",()=>Load(true));Add(tools,"Load target",()=>Load(false));
        Add(tools,"Reset exact names",()=>{session.Reset();Refresh();});Add(tools,"Unassign all",()=>{session.Clear();Refresh();});
        Add(tools,"Load profile",()=>{var d=new OpenFileDialog{Filter="Mapping profile|*.json"};if(d.ShowDialog()==true){session.LoadProfile(d.FileName);Refresh();}});
        Add(tools,"Save profile",()=>{var d=new SaveFileDialog{Filter="Mapping profile|*.json",FileName="mapping.json"};if(d.ShowDialog()==true)session.SaveProfile(d.FileName);});
        Add(tools,"Preview transfer",Preview);
        Add(tools,"Save new target project",()=>{var d=new SaveFileDialog{Filter="FighterPort project|*.fpproj",FileName="target-fk.fpproj"};if(d.ShowDialog()==true){session.SaveResult(clips.SelectedIndex,d.FileName);message.Text="Saved new target/fk project. Source and target snapshots preserved.";}});
        var options=new WrapPanel();top.Children.Add(options);options.Children.Add(clips);options.Children.Add(new TextBlock{Text="  Root translation: ",Foreground=Brushes.White,VerticalAlignment=VerticalAlignment.Center});options.Children.Add(policy);options.Children.Add(approval);options.Children.Add(time);top.Children.Add(names);
        DockPanel.SetDock(message,Dock.Bottom);dock.Children.Add(message);
        var previews=new Grid{Height=310};previews.ColumnDefinitions.Add(new());previews.ColumnDefinitions.Add(new());previews.Children.Add(left);Grid.SetColumn(right,1);previews.Children.Add(right);DockPanel.SetDock(previews,Dock.Bottom);dock.Children.Add(previews);
        var scroll=new ScrollViewer{VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Content=rows,Margin=new(10)};dock.Children.Add(scroll);
        clips.SelectionChanged+=(_,_)=>{if(refreshing)return;Invalidate();time.Maximum=Math.Max(0,(SelectedClip?.FrameCount??1)-1);time.Value=0;Draw();};
        policy.SelectionChanged+=(_,_)=>{if(refreshing)return;session.SetRootPolicy((string)policy.SelectedItem);SyncApproval();};
        approval.Checked+=(_,_)=>{if(!refreshing){session.Approve(true);Invalidate();}};approval.Unchecked+=(_,_)=>{if(!refreshing){session.Approve(false);Invalidate();}};
        time.ValueChanged+=(_,_)=>{if(!refreshing)Draw();};
        Refresh();clips.SelectedIndex=Math.Clamp(clipIndex,0,Math.Max(0,source.Clips.Count-1));
    }
    private Clip? SelectedClip=>clips.SelectedIndex>=0&&clips.SelectedIndex<source.Clips.Count?source.Clips[clips.SelectedIndex]:null;
    private void Add(Panel p,string label,Action action)
    {
        var b=new Button{Content=label,Padding=new(8,5,8,5),Margin=new(3)};p.Children.Add(b);b.Click+=(_,_)=>{try{action();}catch(Exception e){message.Text=e.Message;}};
    }
    private void Load(bool isSource)
    {
        var d=new OpenFileDialog{Filter="FighterPort project|*.fpproj"};if(d.ShowDialog()!=true)return;
        var p=ProjectStore.Load(d.FileName);if(isSource)source=p;else target=p;session=new(source,target);Refresh();
    }
    private void Invalidate(){result=null;message.Text="Preview invalidated. Review mapping and approve before transfer.";Draw();}
    private void SyncApproval(){refreshing=true;approval.IsChecked=session.Profile.Approved;refreshing=false;Invalidate();}
    private void Refresh()
    {
        refreshing=true;result=null;rows.Children.Clear();assignments.Clear();var profile=session.Profile;
        names.Text=$"SOURCE: {source.Name} ({source.Bones.Count} bones)       TARGET: {target.Name} ({target.Bones.Count} bones) • right preview is rest pose until transfer";
        int old=clips.SelectedIndex;clips.ItemsSource=source.Clips.Select(c=>c.Name).ToArray();clips.SelectedIndex=source.Clips.Count>0?Math.Clamp(old,0,source.Clips.Count-1):-1;
        policy.SelectedItem=profile.RootTranslation;approval.IsChecked=profile.Approved;time.Maximum=Math.Max(0,(SelectedClip?.FrameCount??1)-1);
        var choices=new[]{new Choice("","— unassigned —")}.Concat(target.Bones.Select(b=>new Choice(b.Id,b.Name))).ToArray();
        foreach(var bone in source.Bones)
        {
            var row=new Grid{Margin=new(0,2,0,2)};row.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});row.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
            row.Children.Add(new TextBlock{Text=$"{bone.Name}  /  parent: {(bone.Parent<0?"root":source.Bones[bone.Parent].Name)}",Foreground=Brushes.White,VerticalAlignment=VerticalAlignment.Center});
            var combo=new ComboBox{ItemsSource=choices,DisplayMemberPath="Name",SelectedValuePath="Id",SelectedValue=profile.BoneIds.GetValueOrDefault(bone.Id,"")};Grid.SetColumn(combo,1);row.Children.Add(combo);rows.Children.Add(row);assignments.Add(bone.Id,combo);
            combo.SelectionChanged+=(_,_)=>{if(refreshing)return;try{session.Assign(bone.Id,(string?)combo.SelectedValue);SyncApproval();}catch(Exception e){refreshing=true;combo.SelectedValue=session.Profile.BoneIds.GetValueOrDefault(bone.Id,"");refreshing=false;message.Text=e.Message;}};
        }
        left.Fit(source);right.Fit(target);refreshing=false;message.Text="Review exact-name suggestions or assign each target manually. Profile approval is required.";Draw();
    }
    private void Preview()
    {
        result=session.Preview(clips.SelectedIndex);message.Text=$"Transferred {SelectedClip!.Name} into target/fk. Target skin and binds preserved. Scrub to compare; save creates a new project.";Draw();
    }
    private void Draw(){left.Show(source,SelectedClip,(float)time.Value);right.Show(result??target,result?.Clips.Last(),(float)time.Value);}

    internal async Task Smoke(string output)
    {
        Directory.CreateDirectory(output);
        try
        {
            string beforeSource=JsonSerializer.Serialize(source),beforeTarget=JsonSerializer.Serialize(target);
            void Check(bool value){if(!value)throw new InvalidDataException("Mapping GUI assertion failed");}
            void Reject(Action a){try{a();}catch(InvalidDataException){return;}throw new Exception("Expected rejection");}
            clips.SelectedIndex=2;Reject(Preview);
            string bone=source.Bones[10].Id;assignments[bone].SelectedValue="";approval.IsChecked=true;Reject(Preview);
            assignments[bone].SelectedValue=target.Bones[10].Id;Check(approval.IsChecked==false);approval.IsChecked=true;
            string profilePath=Path.Combine(output,"mapping.json");session.SaveProfile(profilePath);session.Reset();Refresh();session.LoadProfile(profilePath);Refresh();Check(approval.IsChecked==true);
            var stale=session.Profile;stale.TargetFingerprint="stale";Reject(()=>session.LoadProfile(stale));Check(session.Profile.Approved);
            Preview();time.Value=30;Check(result!=null&&result.Branch=="target/fk");
            string projectPath=Path.Combine(output,"mapped-"+Guid.NewGuid().ToString("N")+".fpproj");session.SaveResult(clips.SelectedIndex,projectPath);Check(ProjectStore.Load(projectPath).MappingProfiles.Last().Approved);
            Check(beforeSource==JsonSerializer.Serialize(source)&&beforeTarget==JsonSerializer.Serialize(target));
            var orion=Demo.Create(1);var incompatible=new MappingSession(source,orion);incompatible.Clear();for(int i=0;i<source.Bones.Count;i++)incompatible.Assign(source.Bones[i].Id,orion.Bones[i].Id);incompatible.Approve(true);Reject(()=>incompatible.Preview(2));
            await Task.Delay(500);var bitmap=new RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(this);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var stream=File.Create(Path.Combine(output,"mapping.png")))encoder.Save(stream);
            File.WriteAllText(Path.Combine(output,"mapping-smoke.txt"),"PASS: UI assign/unassign revokes approval; unapproved/missing/stale/rotated-root rejected; approved profile save/reopen; source/target unchanged; target/fk saved and skinned preview scrubbed.");Application.Current.Shutdown(0);
        }
        catch(Exception e){File.WriteAllText(Path.Combine(output,"mapping-smoke.txt"),e.ToString());Application.Current.Shutdown(1);}
    }
}
