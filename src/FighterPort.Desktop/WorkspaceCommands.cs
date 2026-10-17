using FighterPort.Core;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Key = System.Windows.Input.Key;

namespace FighterPort.Desktop;

public sealed partial class Workbench
{
    private ResourceWorkspace workspace=null!;
    private readonly TreeView resourceTree=new(){BorderThickness=new(0)};
    private readonly TextBlock resourceProperties=new(){TextWrapping=TextWrapping.Wrap,Margin=new(12)};
    private readonly CheckBox sourceMaterials=new(){Content="Source textures (approx.)",Margin=new(8),IsChecked=true};
    private readonly CheckBox visibilityEnabled=new(){Content="VIS0",Margin=new(8),IsChecked=true};
    private readonly CheckBox uvEnabled=new(){Content="SRT0",Margin=new(8),IsChecked=true};
    private readonly Dictionary<string,BitmapSource> textureCache=[];
    private long textureBytes;
    private readonly TextureSampling textureSampling=new();
    private sealed record ResourceSelection(AssetHandle Handle,NativeTreeEntry Entry){public string DisplayName=>Entry.Name+"  ("+Entry.Kind+")";}
    private ResourceWorkspace CreateWorkspace()
    {
        string packaged=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../native/FighterPort.NativeWorker.exe"));
        return new(File.Exists(packaged)?packaged:Path.Combine(root,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),Path.Combine(root,".local/jobs"),Path.Combine(root,"scripts/ssbh-worker.py"));
    }
    private void InitializeWorkspace()
    {
        workspace=CreateWorkspace();
        resourceTree.SelectedItemChanged+=(_,e)=>{if(e.NewValue is TreeViewItem{Tag:ResourceSelection r})ShowProperties(r);};
        sourceMaterials.Checked+=(_,_)=>Render();sourceMaterials.Unchecked+=(_,_)=>Render();
        uvEnabled.Checked+=(_,_)=>{if(workspace.Data.Uv is {} uv)uv.Enabled=true;Render();};uvEnabled.Unchecked+=(_,_)=>{if(workspace.Data.Uv is {} uv)uv.Enabled=false;Render();};
        visibilityEnabled.Checked+=(_,_)=>SetVisibilityEnabled(true);visibilityEnabled.Unchecked+=(_,_)=>SetVisibilityEnabled(false);
        PreviewKeyDown+=(_,e)=>{if(Keyboard.Modifiers==(ModifierKeys.Control|ModifierKeys.Shift)&&e.Key==Key.S){SaveWorkspace(true);e.Handled=true;return;}if(Keyboard.Modifiers!=ModifierKeys.Control)return;if(e.Key==Key.O){OpenDialog();e.Handled=true;}if(e.Key==Key.S){SaveWorkspace(false);e.Handled=true;}if(Keyboard.FocusedElement is not System.Windows.Controls.Primitives.TextBoxBase){if(e.Key==Key.Z){History(false);e.Handled=true;}if(e.Key==Key.Y){History(true);e.Handled=true;}}};
        Closing+=(_,e)=>{if(!KeepUvEdits())e.Cancel=true;};
        AllowDrop=true;Drop+=(_,e)=>{if(e.Data.GetData(DataFormats.FileDrop) is string[] paths)Native(async()=>{foreach(string path in paths.Take(32))await OpenPath(path);});};
    }
    private void SetVisibilityEnabled(bool enabled){if(workspace.Data.Visibility is {} group)group.Enabled=enabled;Render();}
    private void OpenDialog()=>Native(async()=>
    {
        var d=new OpenFileDialog{Title="Open source or saved workspace",Filter="Supported sources / workspaces|*.pac;*.brres;*.mdl0;*.chr0;*.vis0;*.srt0;*.tex0;*.plt0;*.numdlb;*.numshb;*.nusktb;*.nuanmb;*.numatb;*.fpwork;*.fpproj|All files|*.*",Multiselect=true};
        if(d.ShowDialog()==true)foreach(string path in d.FileNames)await OpenPath(path);
    });
    private void AddMenus(DockPanel dock)
    {
        var menu=new Menu();menu.SetResourceReference(Control.BackgroundProperty,SystemColors.ControlBrushKey);menu.SetResourceReference(Control.ForegroundProperty,SystemColors.ControlTextBrushKey);DockPanel.SetDock(menu,Dock.Top);dock.Children.Add(menu);
        MenuItem Section(string name){var item=new MenuItem{Header=name};menu.Items.Add(item);return item;}
        void Command(MenuItem parent,string name,Action action){var item=new MenuItem{Header=name};item.Click+=(_,_)=>{try{action();}catch(Exception e){SetStatus(e.Message);}};parent.Items.Add(item);}
        var file=Section("_File");Command(file,"New character",()=>{if(!KeepUvEdits())return;uvEdit=null;visibilityEdit=null;indexedResources.Clear();contextEditor.Content=null;objectVisible.Clear();workspacePath=null;workspace=CreateWorkspace();resourceTree.Items.Clear();textureCache.Clear();SetProject(new CharacterProject());MarkClean();ShowWorkspace(false);});Command(file,"_Open…  Ctrl+O",OpenDialog);Command(file,"Open _folder…",()=>Native(async()=>{var dialog=new OpenFolderDialog{Title="Index supported files in one folder (up to 256; no disk scan)"};if(dialog.ShowDialog()==true)await OpenFolder(dialog.FolderName);}));Command(file,"_Save workspace…  Ctrl+S",SaveWorkspaceDialog);
        var recent=new MenuItem{Header="Recent sources"};file.Items.Add(recent);recent.SubmenuOpened+=(_,_)=>{recent.Items.Clear();foreach(string path in preferences.Recent){string selected=path;Command(recent,Path.GetFileName(path),()=>Native(()=>OpenPath(selected)));}};
        var edit=Section("_Edit");Command(edit,"Undo  Ctrl+Z",()=>History(false));Command(edit,"Redo  Ctrl+Y",()=>History(true));
        var view=Section("_View");Command(view,"Frame character",()=>{FitCharacter();Render();});Command(view,"Problems / dependency limits…",()=>MessageBox.Show(this,string.Join("\n",workspace.Data.Problems.Concat(P.Materials.SelectMany(m=>m.Limitations)).Distinct()),"Workspace problems"));
        Command(view,"Resource Browser",()=>ShowWorkspace(false));Command(view,"Model Editor",()=>ShowWorkspace(true));Command(view,"Reset classic layout",ResetLayout);Command(view,"Perspective / orthographic",()=>{orthographic=!orthographic;Render();});
        var tools=Section("_Tools");Command(tools,"Convert to destination…",ConvertDialog);Command(tools,"Map / Retarget…",()=>new MappingWindow(P,clips.SelectedIndex){Owner=this}.Show());Command(tools,"Cancel native job",()=>operation?.Cancel());
        var demos=Section("_Demos");foreach(var item in new[]{("Original humanoid",0),("Alternate rig",1),("Tail rig",2)})Command(demos,item.Item1,()=>{if(!KeepUvEdits())return;uvEdit=null;indexedResources.Clear();contextEditor.Content=null;objectVisible.Clear();workspacePath=null;workspace=CreateWorkspace();resourceTree.Items.Clear();textureCache.Clear();SetProject(Demo.Create(item.Item2));MarkClean();});
        var help=Section("_Help");Command(help,"Verified subset",()=>MessageBox.Show(this,"Brawl MDL0 v9/v11 geometry; CHR0 v4; VIS0 v3 tested (v4 unverified); TEX0 v1 decoding and native material references. Direct Maya ST and XSI/3dsMax SRT0 for an individually selected direct sampler/UV. CI8/RGB5A3 palette/alpha and two-mip original fixture tested; VIS0 v3 and all-track SRT0 v4 edit/export with decoded validation. Source appearance approximates the native shader. Other channels remain in original source files; edited container/model export is pending. Melee, Rivals 2, Dragon Ball and DCC milestones remain in the master plan.","FighterPort Studio"));
    }
    private async Task OpenFolder(string folder)
    {
        var extensions=new HashSet<string>(new[]{".pac",".brres",".mdl0",".chr0",".vis0",".srt0",".tex0",".numdlb",".numshb",".nusktb",".nuanmb",".numatb"},StringComparer.OrdinalIgnoreCase);
        int count=0;foreach(string path in Directory.EnumerateFiles(folder).Take(257))
        {
            if(++count>256)throw new InvalidDataException("Folder entry quota reached; already opened sources retained. Choose a smaller folder.");
            if(extensions.Contains(Path.GetExtension(path)))await OpenPath(path);
        }
    }
    private async Task OpenPath(string path)
    {
        if(Directory.Exists(path)){await OpenFolder(path);return;}
        if(Path.GetExtension(path).Equals(".fpwork",StringComparison.OrdinalIgnoreCase))
        {
            if(!KeepUvEdits())return;workspace.Load(path);workspacePath=path;uvEdit=null;indexedResources.Clear();resourceTree.Items.Clear();foreach(var source in workspace.Data.Documents){AddDocument(source);await IndexDocument(source);}
            int index=workspace.Data.ClipIndex;SetProject(workspace.Data.Project);refreshing=true;clips.SelectedIndex=index;refreshing=false;timeline.Maximum=Math.Max(0,(SelectedClip?.FrameCount??1)-1);visibilityEnabled.IsChecked=workspace.Data.Visibility?.Enabled??true;uvEnabled.IsChecked=workspace.Data.Uv?.Enabled??true;await LoadTextures();Render();MarkClean();RememberSource(path);SetStatus("Workspace reopened; every external source hash verified.");return;
        }
        if(Path.GetExtension(path).Equals(".fpproj",StringComparison.OrdinalIgnoreCase)){if(!KeepUvEdits())return;uvEdit=null;indexedResources.Clear();contextEditor.Content=null;objectVisible.Clear();workspacePath=null;workspace=CreateWorkspace();resourceTree.Items.Clear();textureCache.Clear();SetProject(ProjectStore.Load(path));MarkClean();RememberSource(path);return;}
        var doc=await workspace.Open(path,operation?.Token??default);
        if(!resourceTree.Items.OfType<TreeViewItem>().Any(i=>i.Tag is ResourceSelection r&&r.Handle.DocumentId==doc.Id))AddDocument(doc);
        await IndexDocument(doc);RememberSource(path);
        SetStatus("Source added read-only. Expand resources; double-click a model, skeletal clip, VIS0 or texture. Unimplemented resources remain visible.");
    }
    private void AddDocument(SourceDocument doc)=>resourceTree.Items.Add(MakeNode(new(){DocumentId=doc.Id,NodeId=doc.Root.Id},doc.Root));
    private TreeViewItem MakeNode(AssetHandle handle,NativeTreeEntry entry)
    {
        var selection=new ResourceSelection(handle,entry);var item=new TreeViewItem{Header=ResourceHeader(entry),ToolTip=entry.Kind+" v"+entry.Version+" — "+entry.Name,Tag=selection};
        if(entry.HasChildren)
        {
            item.Items.Add(new TreeViewItem{Header="Expand to read children…"});
            item.Expanded+=(_,e)=>{if(e.OriginalSource!=item||item.Items.Count!=1||item.Items[0] is not TreeViewItem{Tag:null})return;Native(()=>ExpandNode(item));};
        }
        item.MouseDoubleClick+=(_,e)=>{if(!item.IsSelected)return;e.Handled=true;Native(()=>Activate(selection));};
        var menu=new ContextMenu();item.ContextMenu=menu;
        void Action(string label,Func<Task> action){var m=new MenuItem{Header=label};m.Click+=(_,_)=>Native(action);menu.Items.Add(m);}
        if(entry.Kind is "MDL0" or "NUMDLB" or "NUMSHB")Action("Display model / resolve materials",()=>Activate(selection));
        if(entry.Kind is "CHR0" or "NUANMB")Action("Use skeletal animation",()=>Activate(selection));
        if(entry.Kind=="SRT0")
        {
            Action("Bind UV animation to materials / timeline",()=>Activate(selection));
            var editUv=new MenuItem{Header="Edit active SRT0 curves / bindings / export…"};menu.Items.Add(editUv);
            editUv.Click+=(_,_)=>{try{if(operation!=null)throw new InvalidOperationException("Wait for the native job");EditUv();}catch(Exception e){SetStatus(e.Message);}};
        }
        if(entry.Kind=="VIS0")
        {
            Action("Bind visibility to active model / timeline",()=>Activate(selection));
            var edit=new MenuItem{Header="Edit active visibility / export VIS0…"};menu.Items.Add(edit);
            edit.Click+=(_,_)=>{try{if(operation!=null)throw new InvalidOperationException("Wait for the active native job");EditVisibility();}catch(Exception e){SetStatus(e.Message);}};
        }
        if(entry.Kind=="MDL0Material")
        {
            var inspect=new MenuItem{Header="Inspect material / individual sampler…"};menu.Items.Add(inspect);
            inspect.Click+=(_,_)=>{try{if(operation!=null)throw new InvalidOperationException("Wait for the native job");InspectMaterial(selection);}catch(Exception e){SetStatus(e.Message);}};
        }
        if(entry.Kind=="TEX0")
        {
            Action("Preview texture",()=>PreviewTexture(selection));
            Action("Use for material sampler…",()=>OverrideTexture(selection));
            Action("Extract decoded PNG…",async()=>{var d=new SaveFileDialog{Filter="Decoded texture|*.png",FileName="texture.png"};if(d.ShowDialog()==true){workspace.CheckOutput(d.FileName);ProjectStore.AtomicWrite(d.FileName,await workspace.Texture(handle,ct:operation!.Token),overwrite:false);}});
        }
        if(workspace.Document(handle).Adapter=="Brawl")Action("Extract raw source slice (not portable native export)…",async()=>{var d=new SaveFileDialog{Filter="Raw unrelocated source slice|*.bin",FileName="resource-slice.bin"};if(d.ShowDialog()==true){workspace.CheckOutput(d.FileName);ProjectStore.AtomicWrite(d.FileName,await workspace.ExtractSlice(handle,operation!.Token),overwrite:false);}});
        foreach(string label in new[]{"Export edited native model / container","Replace native resource","Restore native resource","Rename native resource","Delete native resource"})menu.Items.Add(new MenuItem{Header=label,IsEnabled=false,ToolTip="Native model/container writer not implemented; source remains read-only"});
        Action("Show dependencies / capability",()=>{ShowProperties(selection);return Task.CompletedTask;});
        return item;
    }
    private void ShowProperties(ResourceSelection selection)
    {
        ShowClassicResourceFields(selection);
        var e=selection.Entry;resourceProperties.Text=$"{e.Kind} v{e.Version} • {e.Size:N0} bytes\n{e.Name}\nHandle: {e.Id}\n{e.Status}\nSource: {Path.GetFileName(workspace.Document(selection.Handle).Path)}";
        if(selection.Handle.DocumentId==workspace.Data.ActiveModel?.DocumentId&&P.Materials.SingleOrDefault(m=>m.Id==e.Id) is {} material)
            resourceProperties.Text+="\n"+string.Join("\n",material.Textures.Select(t=>$"Sampler {t.Slot} / UV{t.UVSet}: {t.Name}\nU: {t.WrapU}, V: {t.WrapV}, requested magnification: {t.Filter}; min: {t.MinFilter}; LOD bias {t.LodBias}\nTexgen {t.Texgen}, projection {t.Projection}\n{t.MatrixMode}; SRT: {string.Join(", ",t.RestSrt)}\n{t.Status}\nPreview: bounded bilinear mip 0; nearest/LOD and TEV composition unsupported; selected sampler {material.PreviewSlot}"));
    }
    private async Task ExpandNode(TreeViewItem item)
    {
        if(item.Tag is not ResourceSelection selection)return;
        if(item.Items.OfType<TreeViewItem>().Any(t=>t.Tag is ResourceSelection))return;
        var children=await workspace.Children(selection.Handle,operation?.Token??default);item.Items.Clear();foreach(var child in children)item.Items.Add(MakeNode(new(){DocumentId=selection.Handle.DocumentId,NodeId=child.Id},child));
    }
    private async Task Reveal(AssetHandle handle)
    {
        var item=resourceTree.Items.OfType<TreeViewItem>().Single(i=>i.Tag is ResourceSelection r&&r.Handle.DocumentId==handle.DocumentId);
        foreach(string index in handle.NodeId.Split('/').Skip(1)){await ExpandNode(item);item.IsExpanded=true;item=(TreeViewItem)item.Items[int.Parse(index)];}
        item.IsSelected=true;item.BringIntoView();
    }
    private async Task Activate(ResourceSelection selection)
    {
        if(selection.Entry.Kind is "MDL0" or "NUMDLB" or "NUMSHB" or "CHR0" or "NUANMB" or "SRT0" or "VIS0"){if(!KeepUvEdits())return;contextEditor.Content=null;uvEdit=null;visibilityEdit=null;}
        var token=operation?.Token??default;
        switch(selection.Entry.Kind)
        {
            case "MDL0":case "NUMDLB":case "NUMSHB":
                await workspace.ActivateModel(selection.Handle,token);SetProject(workspace.Data.Project);await LoadTextures();break;
            case "CHR0":case "NUANMB":
                workspace.Data.Project=P;await workspace.ActivateClip(selection.Handle,token);int index=workspace.Data.ClipIndex;SetProject(workspace.Data.Project);clips.SelectedIndex=index;break;
            case "SRT0":
                workspace.Data.Project=P;await workspace.ActivateUv(selection.Handle,token);uvEnabled.IsChecked=true;break;
            case "VIS0":
                workspace.Data.Project=P;await workspace.ActivateVisibility(selection.Handle,token);visibilityEnabled.IsChecked=true;break;
            case "TEX0":await PreviewTexture(selection);return;
            default:ShowProperties(selection);SetStatus("Resource preserved in its verified source. Evaluation and edited export for this node type are not implemented.");return;
        }
        MarkClean();RefreshClassicLists();if(selection.Entry.Kind is "MDL0" or "NUMDLB" or "NUMSHB")ShowWorkspace(true);
        ShowProperties(selection);Render();
        SetStatus($"{P.Name} • {P.Bones.Count} bones • {textureCache.Count} decoded textures • skeletal: {SelectedClip?.Name??"rest"} • visibility: {workspace.Data.Visibility?.Name??"none"}. Source appearance: selected direct sampler approximation; TEV composition, alpha-test, material/shape and Maya rotation pending. "+string.Join("; ",workspace.Data.Problems.Distinct().Take(3)));
    }
    private static string TextureKey(TextureBinding binding)=>binding.Texture.DocumentId+":"+binding.Texture.NodeId+":"+binding.PaletteId;
    private static BitmapSource Decode(byte[] bytes)
    {
        using var input=new MemoryStream(bytes);var decoder=new PngBitmapDecoder(input,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);var image=decoder.Frames[0];image.Freeze();return image;
    }
    private async Task LoadTextures()
    {
        textureCache.Clear();textureSampling.Clear();textureBytes=0;
        foreach(var binding in P.Materials.SelectMany(m=>m.Textures.Where(t=>t.Slot==m.PreviewSlot&&t.Texture.NodeId!="")))
        {
            string key=TextureKey(binding);if(textureCache.ContainsKey(key))continue;
            try
            {
                var image=Decode(await workspace.Texture(binding.Texture,palette:binding.PaletteId,ct:operation?.Token??default));long bytes=(long)image.PixelWidth*image.PixelHeight*4;
                if(textureBytes+bytes>32*1024*1024)throw new InvalidDataException("32 MiB decoded texture cache quota; neutral fallback");
                textureCache.Add(key,image);textureBytes+=bytes;
            }
            catch(OperationCanceledException){throw;}
            catch(Exception e){workspace.Data.Problems.Add(binding.Name+": "+e.Message);}
        }
    }
    private Brush? MaterialBrush(string id,System.Windows.Media.Media3D.MeshGeometry3D geometry,Mesh mesh)
    {
        var previewMaterial=P.Materials.SingleOrDefault(m=>m.Id==id);
        var binding=previewMaterial?.Textures.SingleOrDefault(t=>t.Slot==previewMaterial.PreviewSlot);
        if(binding==null||!textureCache.TryGetValue(TextureKey(binding),out var image))return null;
        try
        {
            var uv=UvEvaluator.MeshCoordinates(P,mesh,id,workspace.Data.Uv,(float)timeline.Value).Select(v=>new Point(v.X,v.Y)).ToArray();
            var sampled=textureSampling.Prepare(image,uv,binding.WrapU,binding.WrapV,binding.Filter);
            geometry.TextureCoordinates=new(sampled.Coordinates);return sampled.Brush;
        }
        catch(Exception e)when(e is InvalidOperationException or InvalidDataException){string problem=binding.Name+": "+e.Message;if(!workspace.Data.Problems.Contains(problem))workspace.Data.Problems.Add(problem);return null;}
    }
    private async Task PreviewTexture(ResourceSelection selection)
    {
        var doc=workspace.Document(selection.Handle);var palettes=(await Inventory(doc)).Where(e=>e.Entry.Kind=="PLT0").ToArray();
        var mip=new ComboBox{ItemsSource=Enumerable.Range(0,Math.Max(1,selection.Entry.MipCount)).Select(i=>"Mip "+i),SelectedIndex=0,Margin=new(4)};
        var palette=new ComboBox{ItemsSource=new[]{"Automatic palette in texture BRRES"}.Concat(palettes.Select(p=>p.Entry.Name+" ["+p.Handle.NodeId+"]")),SelectedIndex=0,Margin=new(4)};
        var image=new Image{Stretch=Stretch.Uniform};var details=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new(6)};
        var panel=new DockPanel();var controls=new StackPanel();DockPanel.SetDock(controls,Dock.Top);panel.Children.Add(controls);panel.Children.Add(image);
        controls.Children.Add(new TextBlock{Text=$"{selection.Entry.PixelFormat} / {selection.Entry.Width} × {selection.Entry.Height} / {selection.Entry.MipCount} native mips. Alpha retained; native material alpha-test is not simulated.",Margin=new(6),TextWrapping=TextWrapping.Wrap});
        controls.Children.Add(mip);controls.Children.Add(palette);controls.Children.Add(details);
        var decode=new Button{Content="Decode selected mip / palette",Margin=new(4)};controls.Children.Add(decode);
        var bind=new Button{Content="Use selected palette for active material bindings to this texture",Margin=new(4)};controls.Children.Add(bind);
        var window=new Window{Owner=this,Title=selection.Entry.Name+" — native texture",Width=650,Height=650,Content=panel};
        async Task Load(bool apply)
        {
            int level=mip.SelectedIndex;string pal=palette.SelectedIndex==0?"":palettes[palette.SelectedIndex-1].Handle.NodeId;
            var decoded=Decode(await workspace.Texture(selection.Handle,level,pal,operation?.Token??default));image.Source=decoded;
            var uses=P.Materials.SelectMany(m=>m.Textures.Where(t=>t.Texture.DocumentId==selection.Handle.DocumentId&&t.Texture.NodeId==selection.Handle.NodeId).Select(t=>(m,t))).ToArray();
            details.Text=$"Decoded {decoded.PixelWidth} × {decoded.PixelHeight}. Uses: "+string.Join(", ",uses.Select(u=>$"{u.m.Name} / sampler {u.t.Slot} / UV{u.t.UVSet} / {u.t.WrapU},{u.t.WrapV}"));
            if(apply){workspace.Data.Project=P;foreach(var u in uses)workspace.OverrideTexture(u.m.Id,u.t.Slot,selection.Handle,pal);await LoadTextures();Render();SetStatus("Palette override applied to listed uses; save workspace to retain it. Viewport uses mip 0.");}
        }
        decode.Click+=(_,_)=>Native(()=>Load(false));bind.Click+=(_,_)=>Native(()=>Load(true));
        window.Show();
        try{await Load(false);}catch(Exception e){details.Text=e.Message+" — select an explicit palette and decode.";}
        ShowProperties(selection);
    }
    private async Task OverrideTexture(ResourceSelection selection)
    {
        // Decode before mutating: a missing/ambiguous palette leaves the old binding intact.
        _=Decode(await workspace.Texture(selection.Handle,ct:operation?.Token??default));
        var bindings=P.Materials.SelectMany(m=>m.Textures.Select(t=>(Material:m,Binding:t))).ToArray();
        if(bindings.Length==0)throw new InvalidDataException("Active model has no native sampler bindings");
        var list=new ListBox{ItemsSource=bindings.Select(b=>$"{b.Material.Name} / sampler {b.Binding.Slot}: {b.Binding.Name}").ToArray()};
        var panel=new DockPanel();var window=new Window{Owner=this,Title="Explicit texture dependency override",Width=580,Height=420,Content=panel};
        var accept=new Button{Content="Bind selected sampler",Padding=new(10)};DockPanel.SetDock(accept,Dock.Bottom);panel.Children.Add(accept);panel.Children.Add(list);accept.Click+=(_,_)=>{if(list.SelectedIndex>=0)window.DialogResult=true;};
        if(window.ShowDialog()!=true)return;var selected=bindings[list.SelectedIndex];workspace.Data.Project=P;workspace.OverrideTexture(selected.Material.Id,selected.Binding.Slot,selection.Handle);await LoadTextures();Render();SetStatus("Texture override applied; Save workspace retains the exact source handle.");
    }
    private void SaveWorkspaceDialog()
    {
        var d=new SaveFileDialog{Filter="FighterPort resource workspace|*.fpwork",FileName="character.fpwork"};if(d.ShowDialog()!=true)return;
        workspace.Data.Project=P;workspace.Data.ClipIndex=clips.SelectedIndex;workspace.Save(d.FileName);workspacePath=d.FileName;MarkClean();RememberSource(d.FileName);SetStatus("Saved workspace. Original sources stay read-only; keep them at their verified paths. Relinking/moving sources is not implemented.");
    }
    private void ConvertDialog()
    {
        var destinations=new ComboBox{ItemsSource=new[]{"Brawl / Project M","Ultimate","Melee","Rivals 2"},SelectedIndex=0,Margin=new(8)};
        var partial=new CheckBox{Content="Explicit partial conversion: skeletal animation only",Margin=new(8)};
        var text=new TextBlock{Margin=new(8),TextWrapping=TextWrapping.Wrap,Text="Complete character conversion is not yet implemented for any destination. Skeletal-only conversion can bake a selected clip to genuine CHR0 v4 or NUANMB 2.0 and decode-validate it. This does not retarget to another rig. Geometry, textures, materials, VIS0 and other ancillary groups, constraints, opaque resources and fighter packaging are omitted from this output. Source workspace remains available. Melee and Rivals 2 native writers are not implemented."};
        var panel=new StackPanel{Margin=new(12)};var window=new Window{Owner=this,Title="Destination conversion / compatibility",Width=620,Height=400,Content=panel};panel.Children.Add(destinations);panel.Children.Add(text);panel.Children.Add(partial);
        var button=new Button{Content="Bake, write and decode-check…",Margin=new(8),Padding=new(8)};panel.Children.Add(button);
        button.Click+=(_,_)=>
        {
            if(!partial.IsChecked.GetValueOrDefault()||destinations.SelectedIndex>1||SelectedClip==null){text.Text+="\nBlocked: select an available skeletal destination, a clip, and explicitly accept partial output.";return;}
            bool ultimate=destinations.SelectedIndex==1;window.Close();Native(async()=>
            {
                var d=new SaveFileDialog{Filter=ultimate?"Ultimate skeletal animation|*.nuanmb":"Brawl skeletal animation|*.chr0",FileName=ultimate?"skeletal.nuanmb":"skeletal.chr0"};if(d.ShowDialog()!=true)return;
                await ConvertSkeletal(ultimate,d.FileName);
            });
        };window.ShowDialog();
    }
    private async Task ConvertSkeletal(bool ultimate,string path)
    {
        workspace.CheckOutput(path);
        var source=ProjectStore.Clone(P);var c=source.Clips[clips.SelectedIndex];c.Losses.RemoveAll(s=>s.Contains("native nodes")); // Explicit skeletal-only output; original workspace stays untouched.
        var baked=NativeBake.Bake(source,clips.SelectedIndex,ultimate);var clip=baked.Project.Clips[baked.ClipIndex];
        if(ultimate)await Ultimate.ExportClip(baked.Project,clip,path,true,operation?.Token??default);else await Backend.ExportClip(baked.Project,clip,path,operation?.Token??default);
        SetStatus($"Skeletal-only native output written and decoded. Bake sampled errors: {baked.MaxAngleDegrees:0.####}° / {baked.MaxJointDistance:0.####} cm. Ancillary/model resources omitted by explicit choice. Native-tool / in-game checks unrun.");
    }
    private async Task<List<ResourceSelection>> Inventory(SourceDocument doc)
    {
        var found=new List<ResourceSelection>();var queue=new Queue<NativeTreeEntry>();queue.Enqueue(doc.Root);
        while(found.Count<5000&&queue.TryDequeue(out var entry))
        {
            var handle=new AssetHandle{DocumentId=doc.Id,NodeId=entry.Id};found.Add(new(handle,entry));
            if(entry.HasChildren&&entry.Kind is "ARC" or "BRRES" or "BRESGroup")foreach(var child in await workspace.Children(handle))queue.Enqueue(child);
        }
        return found;
    }
    private async Task WorkspaceSmoke(string output,string modelPath,string motionPath)
    {
        Directory.CreateDirectory(output);
        try
        {
            await OpenPath(modelPath);await OpenPath(motionPath);var models=await Inventory(workspace.Data.Documents[0]);var motions=await Inventory(workspace.Data.Documents[1]);
            await Activate(models.First(e=>e.Entry.Kind=="MDL0"));if(textureCache.Count==0)throw new InvalidDataException("No resolved source textures");
            Capture(Path.Combine(output,"rest.png"));
            await Activate(motions.FirstOrDefault(e=>e.Entry.Kind=="CHR0"&&e.Entry.Name=="Wait2")??motions.First(e=>e.Entry.Kind=="CHR0"));
            timeline.Value=10.25;Render();Capture(Path.Combine(output,"skeletal-only.png"));
            var vis=motions.FirstOrDefault(e=>e.Entry.Kind=="VIS0"&&e.Entry.Name=="OriginalAccessory");if(vis!=null)await Activate(vis);
            var uv=motions.FirstOrDefault(e=>e.Entry.Kind=="SRT0"&&e.Entry.Name==SelectedClip!.Name)??motions.First(e=>e.Entry.Kind=="SRT0"&&e.Entry.Name=="OriginalUvSlide");await Activate(uv);
            var group=workspace.Data.Visibility;var state0=SceneEvaluator.Sample(P,SelectedClip,group,0);timeline.Value=10;
            if(modelPath.Contains("workspace-model"))
            {
                var state10=SceneEvaluator.Sample(P,SelectedClip,group,10);if(state0.DrawVisibility.SelectMany(v=>v).SequenceEqual(state10.DrawVisibility.SelectMany(v=>v)))throw new InvalidDataException("VIS draw state did not change");
                var image=new FormatConvertedBitmap(textureCache.Values.First(),PixelFormats.Bgra32,null,0);byte[] pixel=new byte[4];image.CopyPixels(new Int32Rect(4,4,1,1),pixel,4,0);if(pixel[2]!=255||pixel[1]!=0||pixel[0]!=0)throw new InvalidDataException("Native original upper-left red texel mismatch");
                image.CopyPixels(new Int32Rect(4,24,1,1),pixel,4,0);if(pixel[0]!=255||pixel[2]!=0)throw new InvalidDataException("Native original lower-left blue texel mismatch");
                await ConvertSkeletal(false,Path.Combine(output,"skeletal-only-"+Guid.NewGuid().ToString("N")+".chr0"));
                EditVisibility(smoke:true);await workspace.ExportVisibility(Path.Combine(output,"edited-visibility-"+Guid.NewGuid().ToString("N")+".vis0"));
                workspace.Data.Project=P;workspace.Save(Path.Combine(output,"visibility-edited.fpwork"));MarkClean();
                await Activate(motions.Single(e=>e.Entry.Name=="OriginalUvEditable"));EditUv(smoke:true);if(!UvHistory(false)||!UvHistory(true))throw new InvalidOperationException("SRT0 history lost on editor close");await workspace.ExportUv(Path.Combine(output,"edited-uv-"+Guid.NewGuid().ToString("N")+".srt0"));workspace.Save(Path.Combine(output,"uv-edited.fpwork"));MarkClean();
                await Activate(motions.Single(e=>e.Entry.Name=="OriginalUvMatrix3dsMax"));timeline.Value=20.25;Render();
                Capture(Path.Combine(output,"rotating-uv.png"));
                var material=P.Materials[0];var materialSelection=new ResourceSelection(new(){DocumentId=workspace.Data.ActiveModel!.DocumentId,NodeId=material.Id},new(){Id=material.Id,Name=material.Name,Kind="MDL0Material"});
                InspectMaterial(materialSelection,smoke:true);workspace.Save(Path.Combine(output,"uv1-inspection.fpwork"));MarkClean();await SelectSampler(material,0);
                string indexed=Path.Combine(Path.GetDirectoryName(modelPath)!,"indexed.pac");
                if(File.Exists(indexed))
                {
                    await OpenPath(indexed);var resources=await Inventory(workspace.Data.Documents.Last());await PreviewTexture(resources.Single(r=>r.Entry.Kind=="TEX0"&&r.Entry.Name=="IndexedAlpha"));
                    var viewer=OwnedWindows.Cast<Window>().Last();var dock=(DockPanel)viewer.Content;var controls=dock.Children.OfType<StackPanel>().Single();
                    controls.Children.OfType<ComboBox>().First().SelectedIndex=1;
                    var paletteChoice=controls.Children.OfType<ComboBox>().Last();paletteChoice.SelectedItem=paletteChoice.Items.Cast<string>().Single(s=>s.StartsWith("AlternatePalette"));
                    controls.Children.OfType<Button>().First().RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    while(operation!=null)await Task.Delay(50);
                    var preview=(BitmapSource)dock.Children.OfType<Image>().Single().Source;
                    if(preview.PixelWidth!=16||preview.PixelHeight!=16)throw new InvalidDataException("GUI mip decode did not change dimensions");
                    var pixels=new FormatConvertedBitmap(preview,PixelFormats.Bgra32,null,0);byte[] pixelValue=new byte[4];pixels.CopyPixels(new Int32Rect(4,4,1,1),pixelValue,4,0);
                    if(pixelValue[2]!=255||pixelValue[1]!=255||pixelValue[0]!=0)throw new InvalidDataException("GUI alternate palette mismatch");
                    pixels.CopyPixels(new Int32Rect(12,12,1,1),pixelValue,4,0);if(pixelValue[3]!=0)throw new InvalidDataException("GUI palette alpha mismatch");viewer.Close();
                }
            }
            workspace.Data.Project=P;workspace.Data.ClipIndex=clips.SelectedIndex;string saved=Path.Combine(output,"reopened.fpwork");workspace.Save(saved);MarkClean();await OpenPath(saved);timeline.Value=10;
            await Reveal(workspace.Data.ActiveModel!);await Reveal(workspace.Data.Uv!.Source);await Task.Delay(400);Capture(Path.Combine(output,"workspace.png"));
            File.WriteAllText(Path.Combine(output,"workspace-smoke.txt"),$"PASS actual WPF resource workflow; {P.Bones.Count} bones, {P.Meshes.Count} meshes, {textureCache.Count} textures; CHR0 {SelectedClip!.Name}; VIS0 {group?.Name??"absent"}, {group?.Tracks.Count(t=>t.BoneId!="")??0} bound tracks; SRT0 {workspace.Data.Uv!.Name} / {workspace.Data.Uv.Tracks.Count(t=>t.Status.StartsWith("Evaluated:"))} evaluated tracks; save/reopen/hash verification. Original fixtures additionally exercise rotating SRT0, VIS and all-track SRT editors/undo/redo/native export and mip/palette/alpha viewer checks when indexed.pac is present. Source appearance is approximate, other channels preserved unevaluated. Native-tool/in-game checks unrun.");Application.Current.Shutdown(0);
        }
        catch(Exception e){File.WriteAllText(Path.Combine(output,"workspace-smoke.txt"),e.ToString());Application.Current.Shutdown(1);}
    }
}
