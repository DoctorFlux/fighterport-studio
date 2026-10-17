using FighterPort.Core;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FighterPort.Desktop;

public sealed partial class Workbench
{
    private readonly TabControl workspaces=new();
    private readonly ContentControl browserPreview=new(),modelPreview=new(),contextEditor=new();
    private readonly PropertyTable resourceFields=new(),boneFields=new();
    private readonly ListBox animationList=new(),objectList=new(),materialList=new(),textureList=new(),searchResults=new();
    private readonly ComboBox animationType=new(){ItemsSource=new[]{"All animation resources","Skeletal (CHR0 / NUANMB)","Visibility (VIS0)","Texture transform (SRT0)","Preserved / unevaluated"},SelectedIndex=0};
    private readonly TextBox resourceSearch=new(),animationSearch=new(),boneSearch=new(),frameEntry=new(){Width=68};
    private readonly ComboBox activeModel=new(){MinWidth=150,MaxWidth=360};
    private readonly TextBlock previewCaption=new(){Margin=new(4)},frameRange=new(){Margin=new(4),VerticalAlignment=VerticalAlignment.Center};
    private readonly ProgressBar jobProgress=new(){Width=90,Height=12,Visibility=Visibility.Collapsed,ToolTip="Supervised native job; use Tools → Cancel native job"};
    private readonly TabControl editingTabs=new();
    private readonly List<ResourceSelection> indexedResources=[];
    private readonly Border viewportBorder=new(){BorderBrush=SystemColors.ActiveBorderBrush,BorderThickness=new(1),Background=Brushes.LightGray};
    private readonly Grid browserLayout=new(),modelLayout=new();
    private readonly Dictionary<string,bool> objectVisible=[];
    private bool listUpdating,orthographic,autoFrame=true;
    private string? workspacePath;
    private string cleanState="";
    private int previewGeneration;
    private ClassicPreferences preferences=new();
    private bool preferencesWritable=true;
    private sealed class ClassicPreferences
    {
        public int Version {get;set;}=1;
        public double BrowserLeft {get;set;}=270;
        public double BrowserProperties {get;set;}=230;
        public double ModelLeft {get;set;}=235;
        public double ModelRight {get;set;}=240;
        public List<string> Recent {get;set;}=[];
    }
    private void BuildClassicShell(DockPanel dock)
    {
        LoadPreferences();
        var toolbar=new WrapPanel();DockPanel.SetDock(toolbar,Dock.Top);dock.Children.Add(toolbar);
        Button(toolbar,"Open…",OpenDialog);Button(toolbar,"Save",()=>SaveWorkspace(false));Button(toolbar,"Resource Browser",()=>ShowWorkspace(false));Button(toolbar,"Model Editor",()=>ShowWorkspace(true));
        var statusBar=new DockPanel();DockPanel.SetDock(statusBar,Dock.Bottom);dock.Children.Add(statusBar);DockPanel.SetDock(jobProgress,Dock.Right);statusBar.Children.Add(jobProgress);status.Margin=new(4,2,4,2);status.MaxHeight=42;status.ToolTip=status.Text;statusBar.Children.Add(status);
        dock.Children.Add(workspaces);
        // Resource browser: tree, real property table above contextual preview.
        browserLayout.ColumnDefinitions.Add(new(){Width=new(preferences.BrowserLeft),MinWidth=180});browserLayout.ColumnDefinitions.Add(new(){Width=new(4)});browserLayout.ColumnDefinitions.Add(new(){Width=new(1,GridUnitType.Star),MinWidth=300});
        var browserLeft=new DockPanel();var searchBar=new DockPanel();DockPanel.SetDock(searchBar,Dock.Top);browserLeft.Children.Add(searchBar);searchBar.Children.Add(new TextBlock{Text="Find",Margin=new(4),VerticalAlignment=VerticalAlignment.Center});searchBar.Children.Add(resourceSearch);
        searchResults.MaxHeight=170;searchResults.Visibility=Visibility.Collapsed;DockPanel.SetDock(searchResults,Dock.Top);browserLeft.Children.Add(searchResults);browserLeft.Children.Add(resourceTree);browserLayout.Children.Add(browserLeft);
        AddSplitter(browserLayout,1,false);
        var browserRight=new Grid();browserRight.RowDefinitions.Add(new(){Height=new(preferences.BrowserProperties),MinHeight=100});browserRight.RowDefinitions.Add(new(){Height=new(4)});browserRight.RowDefinitions.Add(new(){Height=new(1,GridUnitType.Star),MinHeight=150});Grid.SetColumn(browserRight,2);browserLayout.Children.Add(browserRight);browserRight.Children.Add(resourceFields);AddSplitter(browserRight,1,true);
        var previewDock=new DockPanel();Grid.SetRow(previewDock,2);browserRight.Children.Add(previewDock);DockPanel.SetDock(previewCaption,Dock.Top);previewDock.Children.Add(previewCaption);previewDock.Children.Add(browserPreview);
        workspaces.Items.Add(new TabItem{Header="Resource Browser",Content=browserLayout});
        // Model editor: dense lists, central viewport, bone controls, contextual editor and transport.
        var modelDock=new DockPanel();var modelBar=new WrapPanel();DockPanel.SetDock(modelBar,Dock.Top);modelDock.Children.Add(modelBar);modelBar.Children.Add(new TextBlock{Text="Active model",Margin=new(4),VerticalAlignment=VerticalAlignment.Center});modelBar.Children.Add(activeModel);Button(modelBar,"Frame model",()=>{FitCharacter();Render();});modelBar.Children.Add(skeleton);modelBar.Children.Add(skin);modelBar.Children.Add(sourceMaterials);
        var transport=new DockPanel{LastChildFill=true,Margin=new(2)};DockPanel.SetDock(transport,Dock.Bottom);modelDock.Children.Add(transport);var transportButtons=new WrapPanel();DockPanel.SetDock(transportButtons,Dock.Left);transport.Children.Add(transportButtons);
        Button(transportButtons,"◀",()=>StepFrame(-1));Button(transportButtons,"Play / Pause",()=>playing=!playing);Button(transportButtons,"▶",()=>StepFrame(1));transportButtons.Children.Add(frameEntry);transportButtons.Children.Add(frameRange);
        timeline.Width=double.NaN;timeline.MinWidth=60;timeline.VerticalAlignment=VerticalAlignment.Center;transport.Children.Add(timeline);
        editingTabs.Height=220;DockPanel.SetDock(editingTabs,Dock.Bottom);modelDock.Children.Add(editingTabs);
        var boneEditor=new WrapPanel{Margin=new(2)};channels.ItemsSource=new[]{"Scale X","Scale Y","Scale Z","Rotate X °","Rotate Y °","Rotate Z °","Translate X cm","Translate Y cm","Translate Z cm"};channels.SelectedIndex=5;
        boneEditor.Children.Add(new TextBlock{Text="Skeletal key",Margin=new(4)});boneEditor.Children.Add(clips);boneEditor.Children.Add(channels);boneEditor.Children.Add(value);Button(boneEditor,"Set key",EditKey);Button(boneEditor,"Undo",()=>History(false));Button(boneEditor,"Redo",()=>History(true));
        editingTabs.Items.Add(new TabItem{Header="Skeletal keys",Content=boneEditor});editingTabs.Items.Add(new TabItem{Header="Contextual animation editor",Content=contextEditor});
        var channelPanel=new WrapPanel();channelPanel.Children.Add(visibilityEnabled);channelPanel.Children.Add(uvEnabled);Button(channelPanel,"Edit active VIS0",()=>EditVisibility());Button(channelPanel,"Edit active SRT0",()=>EditUv());Button(channelPanel,"Convert / Retarget…",ConvertDialog);editingTabs.Items.Add(new TabItem{Header="Channels / conversion",Content=channelPanel});
        modelLayout.ColumnDefinitions.Add(new(){Width=new(preferences.ModelLeft),MinWidth=170});modelLayout.ColumnDefinitions.Add(new(){Width=new(4)});modelLayout.ColumnDefinitions.Add(new(){Width=new(1,GridUnitType.Star),MinWidth=240});modelLayout.ColumnDefinitions.Add(new(){Width=new(4)});modelLayout.ColumnDefinitions.Add(new(){Width=new(preferences.ModelRight),MinWidth=180});
        var lists=new Grid();for(int i=0;i<4;i++)lists.RowDefinitions.Add(new(){Height=new(i==0?2:1,GridUnitType.Star),MinHeight=65});
        var animDock=new DockPanel();DockPanel.SetDock(animationType,Dock.Top);DockPanel.SetDock(animationSearch,Dock.Top);animDock.Children.Add(animationType);animDock.Children.Add(animationSearch);animDock.Children.Add(animationList);
        lists.Children.Add(Group("Animations — double-click to activate",animDock));var objects=Group("Objects — select to inspect",objectList);Grid.SetRow(objects,1);lists.Children.Add(objects);var materials=Group("Materials — double-click to inspect",materialList);Grid.SetRow(materials,2);lists.Children.Add(materials);var textures=Group("Textures — double-click to preview",textureList);Grid.SetRow(textures,3);lists.Children.Add(textures);modelLayout.Children.Add(lists);
        AddSplitter(modelLayout,1,false);AddSplitter(modelLayout,3,false);Grid.SetColumn(modelPreview,2);modelLayout.Children.Add(modelPreview);
        var bones=new Grid();bones.RowDefinitions.Add(new(){Height=new(26)});bones.RowDefinitions.Add(new(){Height=new(1,GridUnitType.Star)});bones.RowDefinitions.Add(new(){Height=new(210),MinHeight=100});bones.Children.Add(boneSearch);Grid.SetRow(tree,1);bones.Children.Add(tree);Grid.SetRow(boneFields,2);bones.Children.Add(boneFields);Grid.SetColumn(bones,4);modelLayout.Children.Add(bones);modelDock.Children.Add(modelLayout);
        workspaces.Items.Add(new TabItem{Header="Model Editor",Content=modelDock});
        editingTabs.SelectionChanged+=(_,e)=>{if(e.Source==editingTabs)editingTabs.Height=editingTabs.SelectedIndex==1?220:80;};editingTabs.Height=80;
        viewport.SizeChanged+=(_,_)=>{if(autoFrame){FitCharacter();Render();}};
        viewportBorder.Child=viewport;
        workspaces.SelectionChanged+=(_,e)=>{if(e.Source==workspaces)MoveViewport();};workspaces.SelectedIndex=0;MoveViewport();
        resourceSearch.ToolTip="Search indexed native resources (maximum 5,000 nodes / 100 displayed matches). Selecting a match reveals its ancestors.";
        resourceSearch.TextChanged+=(_,_)=>FilterResources();animationType.SelectionChanged+=(_,_)=>RefreshAnimationList();animationSearch.TextChanged+=(_,_)=>RefreshAnimationList();
        searchResults.SelectionChanged+=(_,_)=>{if(searchResults.SelectedItem is ResourceSelection r)Native(()=>Reveal(r.Handle));};
        animationList.MouseDoubleClick+=(_,_)=>{if(animationList.SelectedItem is ResourceSelection r)Native(()=>Activate(r));};
        materialList.MouseDoubleClick+=(_,_)=>{if(materialList.SelectedItem is PreviewMaterial m&&workspace.Data.ActiveModel is {} model)InspectMaterial(new(new(){DocumentId=model.DocumentId,NodeId=m.Id},new(){Id=m.Id,Name=m.Name,Kind="MDL0Material"}));};
        materialList.SelectionChanged+=(_,_)=>{if(materialList.SelectedItem is PreviewMaterial m)ShowMaterialFields(m);};
        objectList.SelectionChanged+=(_,_)=>ShowObjectFields();
        textureList.MouseDoubleClick+=(_,_)=>{if(textureList.SelectedItem is ResourceSelection r)Native(()=>PreviewTexture(r));};
        boneSearch.TextChanged+=(_,_)=>FilterBoneTree();
        activeModel.SelectionChanged+=(_,_)=>{if(!listUpdating&&activeModel.SelectedItem is ResourceSelection r&&(r.Handle.NodeId!=workspace.Data.ActiveModel?.NodeId||r.Handle.DocumentId!=workspace.Data.ActiveModel?.DocumentId))Native(()=>Activate(r));};
        frameEntry.KeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Enter){if(double.TryParse(frameEntry.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out double f)&&double.IsFinite(f)&&f>=0&&f<=timeline.Maximum)timeline.Value=f;else SetStatus("Frame must be finite and within the displayed range");e.Handled=true;}};
        timeline.ValueChanged+=(_,_)=>{frameEntry.Text=timeline.Value.ToString("0.###",CultureInfo.InvariantCulture);frameRange.Text=$"/ {timeline.Maximum:0}   {SelectedClip?.FramesPerSecond??60:0.##} fps   {(SelectedClip?.Loop==true?"loop":"clamp")}";};
        PreviewKeyDown+=TransportKey;
        Closed+=(_,_)=>SavePreferences();
        ShowProjectFields();MarkClean();
    }
    private static GroupBox Group(string title,UIElement content)=>new(){Header=title,Content=content,Padding=new(1),Margin=new(1)};
    private static FrameworkElement ResourceHeader(NativeTreeEntry entry)
    {
        var panel=new StackPanel{Orientation=Orientation.Horizontal};panel.Children.Add(new Border{Width=14,Height=14,Margin=new(0,1,4,1),BorderThickness=new(1),BorderBrush=SystemColors.ActiveBorderBrush,Background=SystemColors.ControlBrush,Child=new TextBlock{Text=entry.Kind.Length>0?entry.Kind[..1]:"?",FontSize=9,HorizontalAlignment=HorizontalAlignment.Center}});panel.Children.Add(new TextBlock{Text=entry.Name,VerticalAlignment=VerticalAlignment.Center});return panel;
    }
    private static void AddSplitter(Grid grid,int index,bool horizontal)
    {
        var splitter=new GridSplitter{Background=SystemColors.ControlBrush,HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch};if(horizontal)Grid.SetRow(splitter,index);else Grid.SetColumn(splitter,index);grid.Children.Add(splitter);
    }
    private void ShowWorkspace(bool model){workspaces.SelectedIndex=model?1:0;MoveViewport();}
    private void MoveViewport()
    {
        if(viewportBorder.Child==null)return;browserPreview.Content=null;modelPreview.Content=null;
        if(workspaces.SelectedIndex==1)modelPreview.Content=viewportBorder;else browserPreview.Content=viewportBorder;
        previewCaption.Text="Active character preview — resource selection leaves the animation session intact";Render();
    }
    private static Brush CheckerBrush()
    {
        var drawing=new DrawingGroup();drawing.Children.Add(new GeometryDrawing(SystemColors.WindowBrush,null,new RectangleGeometry(new Rect(0,0,16,16))));drawing.Children.Add(new GeometryDrawing(SystemColors.ControlLightBrush,null,new RectangleGeometry(new Rect(0,0,8,8))));drawing.Children.Add(new GeometryDrawing(SystemColors.ControlLightBrush,null,new RectangleGeometry(new Rect(8,8,8,8))));return new DrawingBrush(drawing){TileMode=TileMode.Tile,ViewportUnits=BrushMappingMode.Absolute,Viewport=new(0,0,16,16)};
    }
    private void HostEditor(FrameworkElement panel,string title)
    {
        ShowWorkspace(true);contextEditor.Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};((TabItem)editingTabs.Items[1]).Header=title;editingTabs.SelectedIndex=1;
    }
    private void StepFrame(int delta)=>timeline.Value=Math.Clamp(timeline.Value+delta,0,timeline.Maximum);
    private void TransportKey(object sender,KeyEventArgs e)
    {
        if(Keyboard.Modifiers!=ModifierKeys.None||Keyboard.FocusedElement is TextBoxBase or ComboBox||workspaces.SelectedIndex!=1)return;
        if(e.Key==System.Windows.Input.Key.Space){playing=!playing;e.Handled=true;}else if(e.Key==System.Windows.Input.Key.Left){StepFrame(-1);e.Handled=true;}else if(e.Key==System.Windows.Input.Key.Right){StepFrame(1);e.Handled=true;}
    }
    private void ShowProjectFields()=>resourceFields.Show(new[]{new PropertyField("Project","Name",()=>P.Name,"Working character name; native source names remain independent"),new PropertyField("Project","Bones",()=>P.Bones.Count.ToString(),"Canonical skeleton"),new PropertyField("Project","Meshes",()=>P.Meshes.Count.ToString(),"Imported mesh objects")});
    private void ShowClassicResourceFields(ResourceSelection r)
    {
        ++previewGeneration;
        var e=r.Entry;var fields=new List<PropertyField>{new("Native source","Name",()=>e.Name,"Native source is read-only; archive rename/rebuild is not implemented"),new("Native source","Type",()=>e.Kind,"Resource format"),new("Native source","Version",()=>e.Version.ToString(),"Native format version"),new("Native source","Bytes",()=>e.Size.ToString(),"Original native resource size"),new("Native source","Handle",()=>e.Id,"Stable ordinal path within its hash-verified source"),new("Native source","Source",()=>Path.GetFileName(workspace.Document(r.Handle).Path),"Original document"),new("Capability","Status",()=>e.Status,"Operation capability; preserved data is not editable support")};
        if(e.Kind=="TEX0"){fields.Add(new("Texture","Size",()=>$"{e.Width} × {e.Height}","Native dimensions"));fields.Add(new("Texture","Format",()=>e.PixelFormat,"Native texel encoding"));fields.Add(new("Texture","Mip count",()=>e.MipCount.ToString(),"Viewer can decode selected native mip; viewport uses mip zero"));}
        resourceFields.Show(fields);if(workspaces.SelectedIndex==1)boneFields.Show(fields);
        if(r.Handle.DocumentId==workspace.Data.ActiveModel?.DocumentId&&P.Materials.SingleOrDefault(m=>m.Id==e.Id) is {} material)ShowMaterialFields(material);
        if(e.Kind=="TEX0"&&workspaces.SelectedIndex==0&&operation==null)Native(()=>BrowserTexture(r));
    }
    private async Task BrowserTexture(ResourceSelection r)
    {
        int generation=++previewGeneration;var bytes=await workspace.Texture(r.Handle,ct:operation?.Token??default);if(generation!=previewGeneration||workspaces.SelectedIndex!=0)return;
        browserPreview.Content=null;browserPreview.Content=new Border{Background=CheckerBrush(),Child=new Image{Source=Decode(bytes),Stretch=Stretch.Uniform,Margin=new(8)}};previewCaption.Text=r.Entry.Name+" — decoded texture; material composition is separate";
    }
    private void ShowMaterialFields(PreviewMaterial material)
    {
        var fields=new List<PropertyField>{new("Material","Native name",()=>material.Name,"Native identities retained"),new("Inspection","Sampler",()=>material.PreviewSlot.ToString(),"Select one native sampler. Individual inspection does not compose native TEV stages.",v=>{if(operation!=null)throw new InvalidOperationException("Wait for the active native job");int slot=int.Parse(v,CultureInfo.InvariantCulture);if(!material.Textures.Any(t=>t.Slot==slot))throw new InvalidDataException("Sampler does not exist");Native(()=>SelectSampler(material,slot));})};
        foreach(var t in material.Textures){string category="Sampler "+t.Slot;fields.AddRange(new[]{new PropertyField(category,"Texture",()=>t.Name,t.Status),new(category,"UV set",()=>t.UVSet.ToString(),"Native coordinate source; missing/unsupported texgen falls back"),new(category,"Wrap U / V",()=>t.WrapU+" / "+t.WrapV,"Independent native address modes"),new(category,"Min / Mag",()=>t.MinFilter+" / "+t.Filter,"Native filtering reported; WPF bilinear approximation"),new(category,"Texgen / projection",()=>t.Texgen+" / "+t.Projection,"Native generation flags"),new(category,"Matrix",()=>t.MatrixMode,"Maya rotation remains unsupported")});}
        resourceFields.Show(fields);if(workspaces.SelectedIndex==1)boneFields.Show(fields);
    }
    private void ShowObjectFields()
    {
        if(objectList.SelectedItem is not Mesh m)return;
        resourceFields.Show(new[]{new PropertyField("Object","Name",()=>m.Name,"Native mesh name"),new("Object","Vertices",()=>m.Vertices.Count.ToString(),"Facepoint vertices"),new("Preview","Visible",()=>!objectVisible.TryGetValue(m.Name,out bool v)||v?"True":"False","Preview-only object visibility; VIS0/native data is unchanged",v=>{objectVisible[m.Name]=bool.Parse(v);Render();})});
    }
    private void ShowBoneFields()
    {
        if(selectedBone<0||selectedBone>=P.Bones.Count)return;int bone=selectedBone;var b=P.Bones[bone];
        var fields=new List<PropertyField>{new("Identity","Name",()=>b.Name,"Native skeleton identity; rename/rebind requires a dedicated operation"),new("Identity","ID",()=>b.Id,"Persistent canonical identity"),new("Hierarchy","Parent",()=>b.Parent<0?"Root":P.Bones[b.Parent].Name,"Native hierarchy retained")};
        for(int channel=0;channel<9;channel++){int c=channel;fields.Add(new("Selected key",channels.Items[c]?.ToString()??c.ToString(),()=>{var curve=SelectedClip?.Curves.SingleOrDefault(x=>x.Bone==b.Name&&x.Channel==c);return (curve==null?new[]{b.Scale,b.Rotation,b.Translation}[c/3][c%3]:Animation.Evaluate(curve,(float)timeline.Value)).ToString("R",CultureInfo.InvariantCulture);},"Commit creates a validated skeletal key at the rounded timeline frame; unsupported scalar edits reject",v=>{if(operation!=null)throw new InvalidOperationException("Wait for the active native job");float n=float.Parse(v,CultureInfo.InvariantCulture);if(!float.IsFinite(n))throw new InvalidDataException("Value must be finite");if(SelectedClip==null)throw new InvalidDataException("Activate a skeletal animation first");uvHistoryActive=false;visibilityHistoryActive=false;session.EditKey(clips.SelectedIndex,P.Bones[bone].Name,c,(float)Math.Round(timeline.Value),n);Render();}));}
        boneFields.Show(fields);
    }
    private void RefreshClassicLists()
    {
        frameEntry.Text=timeline.Value.ToString("0.###",CultureInfo.InvariantCulture);frameRange.Text=$"/ {timeline.Maximum:0}   {SelectedClip?.FramesPerSecond??60:0.##} fps   {(SelectedClip?.Loop==true?"loop":"clamp")}";
        listUpdating=true;objectList.ItemsSource=P.Meshes;objectList.DisplayMemberPath=nameof(Mesh.Name);materialList.ItemsSource=P.Materials;materialList.DisplayMemberPath=nameof(PreviewMaterial.Name);activeModel.ItemsSource=indexedResources.Where(r=>r.Entry.Kind is "MDL0" or "NUMDLB" or "NUMSHB").ToArray();activeModel.DisplayMemberPath="Entry.Name";activeModel.SelectedItem=activeModel.Items.Cast<ResourceSelection>().SingleOrDefault(r=>r.Handle.DocumentId==workspace.Data.ActiveModel?.DocumentId&&r.Handle.NodeId==workspace.Data.ActiveModel?.NodeId);textureList.ItemsSource=indexedResources.Where(r=>r.Entry.Kind=="TEX0").ToArray();textureList.DisplayMemberPath="Entry.Name";listUpdating=false;RefreshAnimationList();
    }
    private void RefreshAnimationList()
    {
        var kinds=animationType.SelectedIndex switch{1=>new[]{"CHR0","NUANMB"},2=>new[]{"VIS0"},3=>new[]{"SRT0"},4=>new[]{"PAT0","CLR0","SHP0","SCN0"},_=>new[]{"CHR0","NUANMB","VIS0","SRT0","PAT0","CLR0","SHP0","SCN0"}};
        animationList.ItemsSource=indexedResources.Where(r=>kinds.Contains(r.Entry.Kind)&&r.Entry.Name.Contains(animationSearch.Text,StringComparison.OrdinalIgnoreCase)).ToArray();animationList.DisplayMemberPath="DisplayName";
    }
    private async Task IndexDocument(SourceDocument doc)
    {
        if(indexedResources.Any(r=>r.Handle.DocumentId==doc.Id))return;
        foreach(var r in await Inventory(doc)){if(indexedResources.Count>=5000){SetStatus("Resource search index reached 5,000 nodes; native tree remains lazy");break;}indexedResources.Add(r);}RefreshClassicLists();FilterResources();
    }
    private void FilterResources()
    {
        searchResults.Visibility=string.IsNullOrWhiteSpace(resourceSearch.Text)?Visibility.Collapsed:Visibility.Visible;searchResults.ItemsSource=indexedResources.Where(r=>r.Entry.Name.Contains(resourceSearch.Text,StringComparison.OrdinalIgnoreCase)||r.Entry.Kind.Contains(resourceSearch.Text,StringComparison.OrdinalIgnoreCase)).Take(100).ToArray();searchResults.DisplayMemberPath="DisplayName";
    }
    private void FilterBoneTree()
    {
        bool Visit(TreeViewItem item){bool child=item.Items.OfType<TreeViewItem>().Select(Visit).Any(v=>v);bool match=item.Header?.ToString()?.Contains(boneSearch.Text,StringComparison.OrdinalIgnoreCase)==true;item.Visibility=match||child?Visibility.Visible:Visibility.Collapsed;if(child&&boneSearch.Text!="")item.IsExpanded=true;return match||child;}
        foreach(var root in tree.Items.OfType<TreeViewItem>())Visit(root);
    }
    private string StateFingerprint()=>JsonSerializer.Serialize(new{Project=P,workspace.Data.Visibility,workspace.Data.Uv},ProjectStore.Json);
    private void MarkClean()=>cleanState=StateFingerprint();
    private bool HasEdits=>cleanState!=""&&cleanState!=StateFingerprint();
    private void History(bool redo)
    {
        if(operation!=null){SetStatus("Wait for the active native job");return;}if(Keyboard.FocusedElement is TextBoxBase)return;
        if(visibilityHistoryActive&&visibilityEdit!=null&&workspace.Data.Visibility==visibilityEdit.Group){if(redo)visibilityEdit.Redo();else visibilityEdit.Undo();workspace.Data.Visibility=visibilityEdit.Group;refreshVisUi?.Invoke();}else if(!UvHistory(redo)){if(redo)session.Redo();else session.Undo();}Render();ShowBoneFields();
    }
    private void SaveWorkspace(bool saveAs)
    {
        if(saveAs||workspacePath==null){SaveWorkspaceDialog();return;}workspace.Data.Project=P;workspace.Data.ClipIndex=clips.SelectedIndex;workspace.Save(workspacePath);MarkClean();SetStatus("Workspace saved");
    }
    private void LoadPreferences()
    {
        string path=Path.Combine(root,".local/classic-workspace.json");if(!File.Exists(path))return;
        try{if(new FileInfo(path).Length>65536)throw new InvalidDataException();var loaded=JsonSerializer.Deserialize<ClassicPreferences>(File.ReadAllText(path));if(loaded?.Version!=1)throw new InvalidDataException();preferences=loaded;preferences.BrowserLeft=Math.Clamp(preferences.BrowserLeft,180,500);preferences.BrowserProperties=Math.Clamp(preferences.BrowserProperties,100,450);preferences.ModelLeft=Math.Clamp(preferences.ModelLeft,170,400);preferences.ModelRight=Math.Clamp(preferences.ModelRight,180,400);preferences.Recent=preferences.Recent.Take(10).ToList();}catch(Exception){preferencesWritable=false;}
    }
    private void SavePreferences()
    {
        if(!preferencesWritable)return;preferences.BrowserLeft=browserLayout.ColumnDefinitions[0].ActualWidth;preferences.ModelLeft=modelLayout.ColumnDefinitions[0].ActualWidth;preferences.ModelRight=modelLayout.ColumnDefinitions[4].ActualWidth;
        if(browserLayout.Children.OfType<Grid>().LastOrDefault() is {} right)preferences.BrowserProperties=right.RowDefinitions[0].ActualHeight;
        string path=Path.Combine(root,".local/classic-workspace.json");ProjectStore.AtomicWrite(path,JsonSerializer.SerializeToUtf8Bytes(preferences));
    }
    private void RememberSource(string path){preferences.Recent.RemoveAll(p=>p.Equals(path,StringComparison.OrdinalIgnoreCase));preferences.Recent.Insert(0,Path.GetFullPath(path));preferences.Recent=preferences.Recent.Take(10).ToList();}
    private void ResetLayout(){browserLayout.ColumnDefinitions[0].Width=new(270);modelLayout.ColumnDefinitions[0].Width=new(235);modelLayout.ColumnDefinitions[4].Width=new(240);if(browserLayout.Children.OfType<Grid>().LastOrDefault() is {} right)right.RowDefinitions[0].Height=new(230);}
}
