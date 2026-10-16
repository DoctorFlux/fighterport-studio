using FighterPort.Core;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FighterPort.Desktop;

internal sealed class AnatomyWindow : Window
{
    private readonly CharacterProject project;
    private readonly Action<AnatomyProfile> changed;
    private AnatomyProfile profile;
    private readonly StackPanel rows=new();
    private readonly TextBlock status=new(){Foreground=Brushes.LightSteelBlue,Margin=new(8),TextWrapping=TextWrapping.Wrap};
    internal AnatomyWindow(CharacterProject project,AnatomyProfile profile,Action<AnatomyProfile> changed)
    {
        this.project=project;this.profile=ProjectStore.Clone(profile);this.changed=changed;
        Title="Semantic roles • "+project.Name;Width=1180;Height=760;Background=new SolidColorBrush(Color.FromRgb(20,28,38));
        var dock=new DockPanel();Content=dock;var toolbar=new WrapPanel{Margin=new(8)};DockPanel.SetDock(toolbar,Dock.Top);dock.Children.Add(toolbar);
        void Button(string text,Action action){var b=new Button{Content=text,Margin=new(3),Padding=new(8)};toolbar.Children.Add(b);b.Click+=(_,_)=>{try{action();}catch(Exception e){status.Text=e.Message;}};}
        Button("Recompute suggestions (keep locked)",()=>{var next=Anatomy.Suggest(project);foreach(var a in this.profile.Bones.Where(x=>x.Locked))next.Bones[next.Bones.FindIndex(x=>x.BoneId==a.BoneId)]=ProjectStore.Clone(a);this.profile=next;changed(next);Refresh();});
        Button("Save roles",()=>{var d=new SaveFileDialog{Filter="Semantic profile|*.json",FileName="roles.json"};if(d.ShowDialog()==true)ProjectStore.AtomicWrite(d.FileName,JsonSerializer.SerializeToUtf8Bytes(this.profile,ProjectStore.Json));});
        Button("Load roles",()=>{var d=new OpenFileDialog{Filter="Semantic profile|*.json"};if(d.ShowDialog()!=true)return;if(new FileInfo(d.FileName).Length>1024*1024)throw new InvalidDataException("Profile quota");var next=JsonSerializer.Deserialize<AnatomyProfile>(File.ReadAllText(d.FileName),ProjectStore.Json)??throw new InvalidDataException("Empty profile");Anatomy.Validate(next,project);this.profile=next;changed(next);Refresh();});
        status.Text="Native identities stay unchanged. Edit role / side / chain / segment; leave a field to apply. Evidence labels are heuristics, not probabilities. Corrections lock the row and revoke mapping approval.";
        DockPanel.SetDock(status,Dock.Bottom);dock.Children.Add(status);dock.Children.Add(new ScrollViewer{Content=rows,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});Refresh();
    }
    private void Refresh()
    {
        rows.Children.Clear();
        foreach(var a in profile.Bones)
        {
            var panel=new StackPanel{Margin=new(8,4,8,4)};rows.Children.Add(panel);var row=new WrapPanel();panel.Children.Add(row);
            row.Children.Add(new TextBlock{Text=project.Bones.Single(b=>b.Id==a.BoneId).Name,Foreground=Brushes.White,Width=180,VerticalAlignment=VerticalAlignment.Center});
            var role=new TextBox{Text=a.Role,Width=140};var side=new ComboBox{ItemsSource=new[]{"center","left","right"},SelectedItem=a.Side,Width=80};var chain=new TextBox{Text=a.Chain,Width=140};var segment=new TextBox{Text=a.Segment.ToString(),Width=45};var excluded=new CheckBox{Content="Exclude",IsChecked=a.Excluded,Foreground=Brushes.White,Margin=new(8)};
            row.Children.Add(role);row.Children.Add(side);row.Children.Add(chain);row.Children.Add(segment);row.Children.Add(excluded);
            panel.Children.Add(new TextBlock{Text=$"{a.Confidence} • {a.Kind} • {(a.Terminal?"terminal":"branch")} • {string.Join("; ",a.Evidence)} • Alternatives: {string.Join(", ",a.Alternatives)}",Foreground=Brushes.LightSteelBlue,TextWrapping=TextWrapping.Wrap});
            void Apply(){try{Anatomy.Correct(profile,a.BoneId,role.Text,(string)side.SelectedItem,chain.Text,int.Parse(segment.Text),excluded.IsChecked==true);changed(ProjectStore.Clone(profile));status.Text="Correction saved in this session; mapping approval revoked. Save roles to persist.";}catch(Exception e){status.Text=e.Message;}}
            role.LostKeyboardFocus+=(_,_)=>Apply();chain.LostKeyboardFocus+=(_,_)=>Apply();segment.LostKeyboardFocus+=(_,_)=>Apply();side.SelectionChanged+=(_,_)=>Apply();excluded.Click+=(_,_)=>Apply();
        }
    }
}
