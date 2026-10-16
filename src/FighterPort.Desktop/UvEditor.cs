using FighterPort.Core;
using Microsoft.Win32;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FighterPort.Desktop;

public sealed partial class Workbench
{
    private UvEditSession? uvEdit;
    private bool UvHistory(bool redo)
    {
        if(uvEdit==null||workspace.Data.Uv!=uvEdit.Group)return false;
        if(redo)uvEdit.Redo();else uvEdit.Undo();workspace.Data.Uv=uvEdit.Group;Render();return true;
    }
    private bool KeepUvEdits()
    {
        if(!workspace.IsUvDirty)return true;
        var result=MessageBox.Show(this,"SRT0 has unsaved working-copy edits. Save the workspace before replacing it? Yes saves; No discards; Cancel keeps editing.","Unsaved SRT0",MessageBoxButton.YesNoCancel);
        if(result==MessageBoxResult.Cancel)return false;
        if(result==MessageBoxResult.Yes){SaveWorkspaceDialog();return !workspace.IsUvDirty;}return true;
    }
    private void EditUv(bool smoke=false)
    {
        var source=workspace.Data.Uv??throw new InvalidOperationException("Activate an SRT0 first");
        if(uvEdit==null||uvEdit.Group!=source)uvEdit=new(P,source);
        var edit=uvEdit;workspace.Data.Uv=edit.Group;
        var panel=new StackPanel{Margin=new(12)};var window=new Window{Owner=this,Title="SRT0 curves and native bindings — "+source.Name,Width=690,Height=740,Content=new ScrollViewer{Content=panel}};
        var tracks=new ComboBox{Margin=new(4)};var channel=new ComboBox{ItemsSource=new[]{"Scale U","Scale V","Rotation degrees","Translation U","Translation V"},SelectedIndex=3,Margin=new(4)};
        var frame=new TextBox{Text="0",Margin=new(4)};var value=new TextBox{Text="0",Margin=new(4)};var tangent=new TextBox{Text="0",Margin=new(4)};
        var nativeName=new TextBox{Margin=new(4)};var slot=new TextBox{Text="0",Margin=new(4)};var indirect=new CheckBox{Content="Indirect native track (retained; preview unsupported)",Margin=new(4)};
        var materials=new ComboBox{ItemsSource=new[]{"Unbound"}.Concat(P.Materials.Select(m=>m.Name+" ["+m.Id+"]")).ToArray(),SelectedIndex=0,Margin=new(4)};
        var keys=new ListBox{Height=110,Margin=new(4)};var status=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new(4)};
        void Label(string text,UIElement control){panel.Children.Add(new TextBlock{Text=text,Margin=new(4)});panel.Children.Add(control);}
        panel.Children.Add(new TextBlock{Text="All native tracks are listed, including unevaluated samplers. Hermite tangents are units per native frame. Native binding and preview material are explicit. Ctrl+Z / Ctrl+Y affect this SRT0; history survives closing this editor. Sources stay read-only.",TextWrapping=TextWrapping.Wrap,Margin=new(4)});
        panel.Children.Add(tracks);panel.Children.Add(channel);panel.Children.Add(keys);
        var fields=new UniformGrid{Columns=3};
        foreach(var pair in new[]{("Integer frame",frame),("Value",value),("Tangent",tangent)}){var stack=new StackPanel();stack.Children.Add(new TextBlock{Text=pair.Item1});stack.Children.Add(pair.Item2);fields.Children.Add(stack);}panel.Children.Add(fields);
        void RefreshKeys()
        {
            if(tracks.SelectedIndex<0)return;var t=edit.Group.Tracks[tracks.SelectedIndex];
            keys.ItemsSource=t.Curves.SingleOrDefault(c=>c.Channel==channel.SelectedIndex)?.Keys.Select(k=>$"{k.Frame:R} : {k.Value:R}   tangent {k.Tangent:R}").ToArray()??[];
            status.Text=(workspace.IsUvDirty?"Unsaved edits. ":"Saved/source state. ")+UvEvaluator.Support(P,edit.Group,t);
        }
        void Refresh()
        {
            int selected=tracks.SelectedIndex;workspace.Data.Uv=edit.Group;
            tracks.ItemsSource=edit.Group.Tracks.Select(t=>$"{t.MaterialName} / {(t.Indirect?"indirect":"direct")} {t.Slot} [{t.NativeTrackId}]").ToArray();tracks.SelectedIndex=Math.Clamp(selected,0,Math.Max(0,edit.Group.Tracks.Count-1));RefreshKeys();Render();
        }
        tracks.SelectionChanged+=(_,_)=>{if(tracks.SelectedIndex<0)return;var t=edit.Group.Tracks[tracks.SelectedIndex];nativeName.Text=t.MaterialName;slot.Text=t.Slot.ToString();indirect.IsChecked=t.Indirect;materials.SelectedIndex=P.Materials.FindIndex(m=>m.Id==t.MaterialId)+1;RefreshKeys();};
        channel.SelectionChanged+=(_,_)=>RefreshKeys();
        keys.SelectionChanged+=(_,_)=>{if(tracks.SelectedIndex<0||keys.SelectedIndex<0)return;var k=edit.Group.Tracks[tracks.SelectedIndex].Curves.Single(c=>c.Channel==channel.SelectedIndex).Keys[keys.SelectedIndex];frame.Text=k.Frame.ToString("R",CultureInfo.InvariantCulture);value.Text=k.Value.ToString("R",CultureInfo.InvariantCulture);tangent.Text=k.Tangent.ToString("R",CultureInfo.InvariantCulture);};
        int Integer(TextBox box)=>int.Parse(box.Text,CultureInfo.InvariantCulture);
        float Number(TextBox box)=>float.Parse(box.Text,CultureInfo.InvariantCulture);
        void Button(string text,Action action){var b=new Button{Content=text,Margin=new(4),Padding=new(4)};panel.Children.Add(b);b.Click+=(_,_)=>{try{if(operation!=null)throw new InvalidOperationException("Wait for the native job");action();}catch(Exception e){status.Text=e.Message;}};}
        Button("Set key / explicit tangent",()=>{edit.SetKey(tracks.SelectedIndex,channel.SelectedIndex,Integer(frame),Number(value),Number(tangent));Refresh();});
        Button("Move selected key to entered frame",()=>{if(keys.SelectedIndex<0)throw new InvalidOperationException("Select an existing key");int from=(int)edit.Group.Tracks[tracks.SelectedIndex].Curves.Single(c=>c.Channel==channel.SelectedIndex).Keys[keys.SelectedIndex].Frame;edit.MoveKey(tracks.SelectedIndex,channel.SelectedIndex,from,Integer(frame));Refresh();});
        Button("Delete key",()=>{edit.DeleteKey(tracks.SelectedIndex,channel.SelectedIndex,Integer(frame));Refresh();});
        Label("Preview material (does not rewrite the native name automatically)",materials);Label("Native exported material name",nativeName);Label("Native sampler index",slot);panel.Children.Add(indirect);
        Button("Apply native / preview binding",()=>{edit.Bind(tracks.SelectedIndex,materials.SelectedIndex==0?"":P.Materials[materials.SelectedIndex-1].Id,nativeName.Text,Integer(slot),indirect.IsChecked==true);Refresh();});
        Button("Undo SRT0 edit",()=>{edit.Undo();Refresh();});Button("Redo SRT0 edit",()=>{edit.Redo();Refresh();});
        Button("Save workspace…",()=>{SaveWorkspaceDialog();RefreshKeys();});
        Button("Export SRT0 v4 / decode validate…",()=>Native(async()=>{var save=new SaveFileDialog{Filter="Native UV animation|*.srt0",FileName="edited.srt0"};if(save.ShowDialog()!=true)return;workspace.Data.Project=P;await workspace.ExportUv(save.FileName,operation!.Token);status.Text="Written and decoded: all tracks, bindings, metadata, keys and tangents retained. PAC/BRRES repacking and native-tool rendering are separate checks.";}));
        panel.Children.Add(status);
        window.PreviewKeyDown+=(_,e)=>{if(Keyboard.Modifiers==ModifierKeys.Control&&e.Key is System.Windows.Input.Key.Z or System.Windows.Input.Key.Y){if(e.Key==System.Windows.Input.Key.Z)edit.Undo();else edit.Redo();Refresh();e.Handled=true;}};
        Refresh();Exception? failure=null;
        if(smoke)window.ContentRendered+=(_,_)=>{try{frame.Text="10";value.Text="0.375";tangent.Text="0.03125";panel.Children.OfType<Button>().Single(b=>b.Content.ToString()!.StartsWith("Set key")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));if(!edit.Group.Tracks[0].Curves.Single(c=>c.Channel==3).Keys.Any(k=>k.Frame==10&&k.Tangent==.03125f))throw new InvalidOperationException("SRT0 GUI key edit failed");edit.Undo();Refresh();edit.Redo();Refresh();}catch(Exception e){failure=e;}finally{window.Close();}};
        window.ShowDialog();if(failure!=null)throw failure;
    }
}
