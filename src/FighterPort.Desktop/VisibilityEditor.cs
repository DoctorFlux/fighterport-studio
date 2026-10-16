using FighterPort.Core;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace FighterPort.Desktop;

public sealed partial class Workbench
{
    private void EditVisibility(bool smoke=false)
    {
        if(workspace.Data.Visibility is not {} source)throw new InvalidOperationException("Bind a VIS0 to the active model first");
        var ownerWorkspace=workspace;var edit=new VisibilityEditSession(P,source);
        var tracks=new ComboBox{Margin=new(5)};var bones=new ComboBox{ItemsSource=P.Bones.Select(b=>b.Name+" ["+b.Id+"]").ToArray(),Margin=new(5)};
        var frame=new TextBox{Text=((int)timeline.Value).ToString(),Margin=new(5)};var visible=new CheckBox{Content="Visible at this group frame",Margin=new(5),IsChecked=true};
        var status=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new(5)};
        var panel=new StackPanel{Margin=new(12)};var window=new Window{Owner=this,Title="Active VIS0 working copy — "+source.Name,Width=590,Height=510,Content=panel};
        panel.Children.Add(new TextBlock{Text="Edits apply to the working copy. Bind changes both the canonical bone and exported native target name. Frame is in this group's native timeline. Undo/redo below is visibility-only (16 edits); source PAC stays unchanged.",TextWrapping=TextWrapping.Wrap,Margin=new(5)});
        panel.Children.Add(tracks);panel.Children.Add(bones);panel.Children.Add(frame);panel.Children.Add(visible);
        bool Current()=>workspace==ownerWorkspace&&workspace.Data.Visibility==edit.Group;
        void Refresh(){int selected=tracks.SelectedIndex;tracks.ItemsSource=edit.Group.Tracks.Select(t=>t.Target+" → "+t.BoneId).ToArray();tracks.SelectedIndex=Math.Clamp(selected,0,Math.Max(0,edit.Group.Tracks.Count-1));workspace.Data.Visibility=edit.Group;Render();}
        void Button(string title,Action action){var b=new Button{Content=title,Margin=new(4),Padding=new(5)};panel.Children.Add(b);b.Click+=(_,_)=>{try{if(!Current())throw new InvalidOperationException("Active source changed; close and reopen this editor");action();}catch(Exception e){status.Text=e.Message;}};}
        Button("Set visibility at group frame",()=>{if(!int.TryParse(frame.Text,out int f))throw new InvalidOperationException("Enter an integer group frame");edit.SetFrame(tracks.SelectedIndex,f,visible.IsChecked==true);Refresh();status.Text="Working copy edited; Save workspace or export a new VIS0.";});
        Button("Bind selected track to selected bone",()=>{if(bones.SelectedIndex<0)throw new InvalidOperationException("Choose a bone");edit.Bind(tracks.SelectedIndex,P.Bones[bones.SelectedIndex].Id);Refresh();});
        Button("Undo visibility edit",()=>{edit.Undo();Refresh();});Button("Redo visibility edit",()=>{edit.Redo();Refresh();});
        Button("Export active VIS0 v3 / decode-check…",()=>Native(async()=>
        {
            var save=new SaveFileDialog{Filter="Native visibility animation|*.vis0",FileName="edited-visibility.vis0"};if(save.ShowDialog()!=true)return;
            workspace.Data.Project=P;await workspace.ExportVisibility(save.FileName,operation?.Token??default);status.Text="VIS0 written and decoded: metadata, targets, constant flags and every visibility sample match. Container repacking and in-game checks remain unimplemented.";
        }));
        panel.Children.Add(status);Refresh();
        Exception? smokeError=null;
        if(smoke)window.ContentRendered+=(_,_)=>
        {
            try
            {
                frame.Text="10";visible.IsChecked=true;
                void Click(string prefix)=>panel.Children.OfType<Button>().Single(b=>b.Content.ToString()!.StartsWith(prefix)).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                Click("Set visibility");if(!edit.Group.Tracks[0].Values[10])throw new InvalidOperationException("GUI visibility edit failed");
                Click("Undo visibility");if(edit.Group.Tracks[0].Values[10])throw new InvalidOperationException("GUI visibility undo failed");
                Click("Redo visibility");if(!edit.Group.Tracks[0].Values[10])throw new InvalidOperationException("GUI visibility redo failed");
            }
            catch(Exception e){smokeError=e;}finally{window.Close();}
        };
        window.ShowDialog();if(smokeError!=null)throw smokeError;
    }
}
