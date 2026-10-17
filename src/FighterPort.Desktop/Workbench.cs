using FighterPort.Core;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace FighterPort.Desktop;

public sealed partial class Workbench : Window
{
    private EditSession session=new(Demo.Create());
    private readonly Viewport3D viewport=new();
    private readonly Model3DGroup scene=new();
    private readonly ComboBox clips=new(){MinWidth=150}, channels=new(){MinWidth=140};
    private readonly TreeView tree=new(){BorderThickness=new(0)};
    private readonly Slider timeline=new(){Minimum=0,Maximum=60,Width=430};
    private readonly TextBlock status=new(){TextWrapping=TextWrapping.Wrap,Margin=new(12)};
    private readonly TextBlock inspector=new(){TextWrapping=TextWrapping.Wrap,Margin=new(12)};
    private readonly TextBox value=new(){Text="25",Width=70,Margin=new(4)};
    private readonly CheckBox skeleton=new(){Content="Skeleton",Margin=new(8),IsChecked=true};
    private readonly CheckBox skin=new(){Content="Skin",Margin=new(8),IsChecked=true};
    private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(33)};
    private readonly Stopwatch watch=Stopwatch.StartNew();
    private readonly string root;
    private bool playing,refreshing;
    private int selectedBone;
    private double yaw=.35,pitch=.12,distance=320,panX,panY=80,last;
    private Point mouse;
    private CancellationTokenSource? operation;
    private CharacterProject P=>session.Project;
    private Clip? SelectedClip=>clips.SelectedIndex>=0&&clips.SelectedIndex<P.Clips.Count?P.Clips[clips.SelectedIndex]:null;
    private NativeBackend Backend
    {
        get
        {
            string packaged=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../native/FighterPort.NativeWorker.exe"));
            return new(File.Exists(packaged)?packaged:Path.Combine(root,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),Path.Combine(root,".local/jobs"));
        }
    }

    public Workbench(string[] args)
    {
        root=FindRoot();Title="FighterPort Studio";Width=1280;Height=800;MinWidth=900;MinHeight=600;
        var dock=new DockPanel();Content=dock;
        InitializeWorkspace();AddMenus(dock);BuildClassicShell(dock);
        var border=viewportBorder;
        viewport.Children.Add(new ModelVisual3D{Content=scene});viewport.ClipToBounds=true;
        border.MouseDown+=(_,e)=>{autoFrame=false;mouse=e.GetPosition(border);border.CaptureMouse();};
        border.MouseUp+=(_,_)=>border.ReleaseMouseCapture();
        border.MouseMove+=(_,e)=>{if(!border.IsMouseCaptured)return;var pos=e.GetPosition(border);var delta=pos-mouse;mouse=pos;if(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)){panX-=delta.X*.3;panY+=delta.Y*.3;}else{yaw-=delta.X*.01;pitch=Math.Clamp(pitch+delta.Y*.01,-1.3,1.3);}Render();};
        border.MouseWheel+=(_,e)=>{autoFrame=false;distance=Math.Clamp(distance*Math.Exp(-e.Delta*.001),1,10000);Render();};
        timeline.ValueChanged+=(_,_)=>{if(!refreshing)Render();};
        clips.SelectionChanged+=(_,_)=>{if(refreshing)return;if(workspace.Data.ClipIndex!=clips.SelectedIndex){if(!KeepUvEdits()){refreshing=true;clips.SelectedIndex=workspace.Data.ClipIndex;refreshing=false;return;}workspace.ClearAncillary();uvEdit=null;}workspace.Data.ClipIndex=clips.SelectedIndex;timeline.Maximum=Math.Max(0,(SelectedClip?.FrameCount??1)-1);timeline.Value=0;Render();};
        skeleton.Checked+=(_,_)=>Render();skeleton.Unchecked+=(_,_)=>Render();skin.Checked+=(_,_)=>Render();skin.Unchecked+=(_,_)=>Render();
        tree.SelectedItemChanged+=(_,e)=>{if(e.NewValue is TreeViewItem t&&t.Tag is int i){selectedBone=i;ShowBoneFields();Render();}};
        timer.Tick+=(_,_)=>{double now=watch.Elapsed.TotalSeconds;if(playing&&SelectedClip is { } c){double next=timeline.Value+(now-last)*c.FramesPerSecond;if(next>timeline.Maximum){if(c.Loop)next%=c.FrameCount;else{next=timeline.Maximum;playing=false;}}timeline.Value=next;}last=now;};timer.Start();
        Closed+=(_,_)=>{timer.Stop();operation?.Cancel();};
        Refresh();
        if(args.Length==4&&args[0]=="--classic-smoke")Loaded+=async(_,_)=>await ClassicSmoke(args[1],args[2],args[3]);
        if(args.Length==4&&args[0]=="--workspace-smoke")Loaded+=async(_,_)=>await WorkspaceSmoke(args[1],args[2],args[3]);
        if(args.Length>=2&&args[0]=="--smoke")
        {
            if(args.Length==3)SetProject(ProjectStore.Load(args[2]));
            Loaded+=async(_,_)=>await Smoke(args[1]);
        }
        if(args.Length==2&&args[0]=="--project")SetProject(ProjectStore.Load(args[1]));
        if(args.Length==2&&args[0]=="--mapping-smoke")Loaded+=async(_,_)=>{var window=new MappingWindow(P,2){Owner=this};window.Show();await window.Smoke(args[1]);};
        if(args.Length==3&&args[0]=="--playback-smoke"){SetProject(ProjectStore.Load(args[2]));Loaded+=async(_,_)=>await PlaybackSmoke(args[1]);}
    }
    private static string FindRoot()
    {
        for(var d=new DirectoryInfo(AppContext.BaseDirectory);d!=null;d=d.Parent)if(File.Exists(Path.Combine(d.FullName,"config/repository.json")))return d.FullName;
        return Environment.CurrentDirectory;
    }
    private UltimateBackend Ultimate=>new("python",Path.Combine(root,"scripts/ssbh-worker.py"),Path.Combine(root,".local/jobs"));
    private void Bake(bool quaternion)
    {
        if(SelectedClip==null)throw new InvalidDataException("Select a clip");
        var result=NativeBake.Bake(P,clips.SelectedIndex,quaternion);SetProject(result.Project);clips.SelectedIndex=result.ClipIndex;
        SetStatus($"Baked into a new target branch; original clips retained. Quarter-frame errors: angle {result.MaxAngleDegrees:0.####}°, joint {result.MaxJointDistance:0.####} cm, sampled vertices {result.MaxVertexDistance:0.####} cm. Export separately; continuous error remains unproven.");
    }
    private BundleResource? ChooseResource(List<BundleResource> entries)
    {
        var dialog=new Window{Owner=this,Title="Native resources — select model or skeletal animation",Width=650,Height=600};var dock=new DockPanel();dialog.Content=dock;
        var list=new ListBox{ItemsSource=entries.Select(e=>$"{e.Kind} v{e.Version}  {e.Name}  [{e.Id}]").ToArray()};
        var button=new Button{Content="Open selected resource",Padding=new(8)};DockPanel.SetDock(button,Dock.Bottom);dock.Children.Add(button);dock.Children.Add(list);
        button.Click+=(_,_)=>{if(list.SelectedIndex>=0)dialog.DialogResult=true;};return dialog.ShowDialog()==true?entries[list.SelectedIndex]:null;
    }
    private void Button(Panel panel,string text,Action action)
    {
        var b=new Button{Content=text};
        b.Click+=(_,_)=>{try{action();}catch(Exception e){SetStatus(e.Message);}};panel.Children.Add(b);
    }
    private async void Native(Func<Task> job)
    {
        if(operation!=null){SetStatus("A native job is already running.");return;}
        operation=new();jobProgress.Visibility=Visibility.Visible;jobProgress.IsIndeterminate=true;playing=false;SetStatus("Native worker running…");
        try{await job();}catch(Exception e){SetStatus(e.Message);}finally{operation.Dispose();operation=null;jobProgress.IsIndeterminate=false;jobProgress.Visibility=Visibility.Collapsed;if(status.Text=="Native worker running…")SetStatus("Ready");}
    }
    private void SetProject(CharacterProject p){ProjectStore.Validate(p);session=new(p);selectedBone=0;playing=false;FitCharacter();Refresh();}
    private void FitCharacter()
    {
        var points=P.Meshes.SelectMany(m=>m.Vertices).Select(v=>Animation.V(v.Position)).ToArray();if(points.Length==0)return;
        var min=points.Aggregate(System.Numerics.Vector3.Min);var max=points.Aggregate(System.Numerics.Vector3.Max);var center=(min+max)/2;panX=center.X;panY=center.Y;double aspect=viewport.ActualWidth/Math.Max(1,viewport.ActualHeight);var extent=max-min;distance=Math.Max(10,Math.Max(extent.X,extent.Y*aspect)*.6/Math.Tan(Math.PI/8)+extent.Z*.6);autoFrame=true;
    }
    private void SetStatus(string text){status.Text=text;status.ToolTip=text;}
    private void Refresh()
    {
        refreshing=true;clips.ItemsSource=P.Clips.Select(c=>c.Name).ToArray();clips.SelectedIndex=P.Clips.Count>0?0:-1;timeline.Maximum=Math.Max(0,(SelectedClip?.FrameCount??1)-1);timeline.Value=0;
        tree.Items.Clear();var model=new TreeViewItem{Header=P.Name,IsExpanded=true};tree.Items.Add(model);
        model.Items.Add(new TreeViewItem{Header=$"{P.Meshes.Count} mesh regions • {P.Clips.Count} clips"});
        var nodes=new List<TreeViewItem>();for(int i=0;i<P.Bones.Count;i++){var b=P.Bones[i];var item=new TreeViewItem{Header=b.Name,Tag=i,IsExpanded=i<8};nodes.Add(item);if(b.Parent<0)model.Items.Add(item);else nodes[b.Parent].Items.Add(item);}
        refreshing=false;RefreshClassicLists();ShowBoneFields();SetStatus($"{P.Name} • {P.Bones.Count} joints • {P.Meshes.Sum(m=>m.Vertices.Count)} vertices • {P.TargetProfile}");Render();
    }
    private void EditKey()
    {
        uvHistoryActive=false;visibilityHistoryActive=false;
        if(operation!=null)throw new InvalidOperationException("Wait for the active native job");
        if(SelectedClip==null||P.Bones.Count==0)throw new InvalidOperationException("Select a character and clip.");
        session.EditKey(clips.SelectedIndex,P.Bones[selectedBone].Name,channels.SelectedIndex,(float)Math.Round(timeline.Value),float.Parse(value.Text,CultureInfo.InvariantCulture));Render();SetStatus("Key changed; undo/redo available. Save Project to retain edits.");
    }
    private static Point3D Point(System.Numerics.Vector3 v)=>new(v.X,v.Y,v.Z);
    private void Render()
    {
        if(refreshing)return;
        scene.Children.Clear();scene.Children.Add(new AmbientLight(Color.FromRgb(90,90,90)));scene.Children.Add(new DirectionalLight(Colors.White,new(-1,-2,-3)));
        var target=new Point3D(panX,panY,0);var eye=new Point3D(panX+distance*Math.Sin(yaw)*Math.Cos(pitch),panY+distance*Math.Sin(pitch),distance*Math.Cos(yaw)*Math.Cos(pitch));
        viewport.Camera=orthographic?new OrthographicCamera(eye,target-eye,new(0,1,0),distance*.8){NearPlaneDistance=.1,FarPlaneDistance=3000}:new PerspectiveCamera(eye,target-eye,new(0,1,0),45){NearPlaneDistance=.1,FarPlaneDistance=3000};
        var state=SceneEvaluator.Sample(P,SelectedClip,workspace.Data.Visibility,(float)timeline.Value);var world=state.Pose;
        if(skin.IsChecked==true)for(int mi=0;mi<P.Meshes.Count;mi++)
        {
            var mesh=P.Meshes[mi];if(objectVisible.TryGetValue(mesh.Name,out bool shown)&&!shown)continue;if(!state.DrawVisibility[mi].Any(v=>v))continue;
            var geometry=new MeshGeometry3D();foreach(var v in mesh.Vertices){var skinned=Animation.Skin(v,P,world);geometry.Positions.Add(Point(skinned.Position));geometry.Normals.Add(new(skinned.Normal.X,skinned.Normal.Y,skinned.Normal.Z));geometry.TextureCoordinates.Add(new(v.UV[0],v.UV[1]));}
            geometry.TriangleIndices=new Int32Collection(mesh.Indices);
            var color=Color.FromScRgb(1,mesh.Color[0],mesh.Color[1],mesh.Color[2]);
            var drawing=new DrawingGroup();drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(color),null,new RectangleGeometry(new(0,0,16,16))));
            drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromArgb(55,255,255,255)),null,new RectangleGeometry(new(0,0,8,8))));drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromArgb(55,255,255,255)),null,new RectangleGeometry(new(8,8,8,8))));
            var brush=new DrawingBrush(drawing){TileMode=TileMode.Tile,Viewport=new(0,0,.25,.25),ViewportUnits=BrushMappingMode.RelativeToBoundingBox};
            var draws=mesh.Draws.Count==0?new[]{new MeshDraw()}:mesh.Draws.ToArray();
            for(int di=0;di<draws.Length;di++)if(state.DrawVisibility[mi][di])
            {
                var drawnGeometry=geometry.Clone();
                Brush appearance=sourceMaterials.IsChecked==true?MaterialBrush(draws[di].MaterialId,drawnGeometry,mesh)??brush:brush;
                var material=new DiffuseMaterial(appearance);scene.Children.Add(new GeometryModel3D(drawnGeometry,material){BackMaterial=material});
            }
        }
        if(skeleton.IsChecked==true)for(int i=0;i<world.Length;i++)
        {
            var pos=world[i].Translation;float r=i==selectedBone?2.4f:1.3f;
            var g=new MeshGeometry3D();foreach(var d in new[]{new System.Numerics.Vector3(r,0,0),new(0,r,0),new(-r,0,0),new(0,-r,0),new(0,0,r),new(0,0,-r)})g.Positions.Add(Point(pos+d));
            g.TriangleIndices=new([0,1,4,1,2,4,2,3,4,3,0,4,1,0,5,2,1,5,3,2,5,0,3,5]);var m=new EmissiveMaterial(i==selectedBone?Brushes.Orange:Brushes.White);scene.Children.Add(new GeometryModel3D(g,m){BackMaterial=m});
        }
        if(P.Bones.Count>0){var b=P.Bones[selectedBone];inspector.Text=$"{b.Name}\nID: {b.Id}\nParent: {(b.Parent<0?"none":P.Bones[b.Parent].Name)}\nBind XYZ: {string.Join(", ",b.Translation.Select(v=>v.ToString("0.##")))}\nFrame: {timeline.Value:0.00}";}else inspector.Text="Empty character. Open a demo or import a supported model.";
    }
    private async Task Smoke(string output)
    {
        try
        {
            Directory.CreateDirectory(output);await Task.Delay(700);Capture(Path.Combine(output,"demo.png"));
            int clipIndex=Math.Min(2,P.Clips.Count-1);
            timeline.Value=30;clips.SelectedIndex=clipIndex;timeline.Value=30;channels.SelectedIndex=4;selectedBone=10;value.Text="-45";EditKey();session.Undo();session.Redo();
            string project=Path.Combine(output,"gui.fpproj");ProjectStore.Save(P,project);SetProject(ProjectStore.Load(project));MarkClean();clips.SelectedIndex=clipIndex;timeline.Value=30;
            await Task.Delay(300);Capture(Path.Combine(output,"edited.png"));
            string native="";
            if(P.Resources.Count>0)
            {
                string exported=Path.Combine(output,"gui-edited-"+Guid.NewGuid().ToString("N")+".chr0");await Backend.ExportClip(P,SelectedClip!,exported);var decoded=await Backend.ImportClip(P,exported);
                if(Math.Abs(Animation.Evaluate(decoded.Curves.Single(c=>c.Bone==P.Bones[10].Name&&c.Channel==4),30)+45)>.05f)throw new InvalidDataException("GUI native edit did not round trip");
                native=" Native source project loaded; GUI edit exported and decoded successfully.";
            }
            File.WriteAllText(Path.Combine(output,"gui-smoke.txt"),$"PASS: Loaded window {ActualWidth}x{ActualHeight}; skinned viewport; scrub; key edit; undo/redo; save/reopen; screenshot. Rendering tier: {RenderCapability.Tier>>16}.{native} Native tool/in-game checks unrun.");
            MarkClean();Application.Current.Shutdown(0);
        }
        catch(Exception e){File.WriteAllText(Path.Combine(output,"gui-smoke.txt"),e.ToString());Application.Current.Shutdown(1);}
    }
    private async Task PlaybackSmoke(string output)
    {
        Directory.CreateDirectory(output);
        try
        {
            if(P.Meshes.Count==0||P.Clips.Count==0)throw new InvalidDataException("Real model and clip required");
            var clip=SelectedClip!;var a=Animation.World(P,clip,0);timeline.Value=Math.Min(12.5,clip.FrameCount-1);var b=Animation.World(P,clip,(float)timeline.Value);
            bool moves=P.Meshes.SelectMany(m=>m.Vertices).Any(v=>System.Numerics.Vector3.Distance(Animation.Skin(v,P,a).Position,Animation.Skin(v,P,b).Position)>.0001f);
            if(!moves)throw new InvalidDataException("Imported playback is constant");
            await Task.Delay(300);string saved=Path.Combine(output,"reopened.fpproj");ProjectStore.Save(P,saved);SetProject(ProjectStore.Load(saved));timeline.Value=12.5;
            // Commercial screenshots stay local and are not sent to any model service.
            await Task.Delay(300);Capture(Path.Combine(output,"local-preview.png"));
            File.WriteAllText(Path.Combine(output,"playback-smoke.txt"),$"PASS: actual WPF launch; {P.Bones.Count} bones, {P.Meshes.Count} meshes, {P.Meshes.Sum(m=>m.Vertices.Count)} vertices; fractional motion, save/reopen. Neutral preview; native materials/ancillary/constraints are not verified.");Application.Current.Shutdown(0);
        }
        catch(Exception e){File.WriteAllText(Path.Combine(output,"playback-smoke.txt"),e.ToString());Application.Current.Shutdown(1);}
    }
    private void Capture(string path)
    {
        var bitmap=new RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(this);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(path);encoder.Save(stream);
    }
}
