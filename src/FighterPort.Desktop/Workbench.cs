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

public sealed class Workbench : Window
{
    private EditSession session=new(Demo.Create());
    private readonly Viewport3D viewport=new();
    private readonly Model3DGroup scene=new();
    private readonly ComboBox clips=new(){MinWidth=150}, channels=new(){MinWidth=140};
    private readonly TreeView tree=new(){Background=new SolidColorBrush(Color.FromRgb(26,34,45)),Foreground=Brushes.White,BorderThickness=new(0)};
    private readonly Slider timeline=new(){Minimum=0,Maximum=60,Width=430};
    private readonly TextBlock status=new(){Foreground=Brushes.LightSteelBlue,TextWrapping=TextWrapping.Wrap,Margin=new(12)};
    private readonly TextBlock inspector=new(){Foreground=Brushes.White,TextWrapping=TextWrapping.Wrap,Margin=new(12)};
    private readonly TextBox value=new(){Text="25",Width=70,Margin=new(4)};
    private readonly CheckBox skeleton=new(){Content="Skeleton",Foreground=Brushes.White,Margin=new(8),IsChecked=true};
    private readonly CheckBox skin=new(){Content="Skin",Foreground=Brushes.White,Margin=new(8),IsChecked=true};
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
    private NativeBackend Backend=>new(Path.Combine(root,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),Path.Combine(root,".local/jobs"));

    public Workbench(string[] args)
    {
        root=FindRoot();Title="FighterPort Studio • original asset workbench";Width=1280;Height=820;MinWidth=1000;MinHeight=600;Background=new SolidColorBrush(Color.FromRgb(15,22,31));Foreground=Brushes.White;
        var dock=new DockPanel();Content=dock;
        var header=new StackPanel{Margin=new(14,10,14,8)};DockPanel.SetDock(header,Dock.Top);dock.Children.Add(header);
        header.Children.Add(new TextBlock{Text="FIGHTERPORT  /  STUDIO",FontSize=24,FontWeight=FontWeights.SemiBold,Foreground=Brushes.White});
        header.Children.Add(new TextBlock{Text="Local character workbench   •   Neutral shading   •   Original fixtures, no game assets",Foreground=Brushes.LightSteelBlue,Margin=new(0,4,0,10)});
        var tools=new WrapPanel();header.Children.Add(tools);
        Button(tools,"New",()=>SetProject(new CharacterProject()));
        Button(tools,"Open Demo",()=>SetProject(Demo.Create()));
        Button(tools,"Alternate Rig",()=>SetProject(Demo.Create(1)));
        Button(tools,"Tail Demo",()=>SetProject(Demo.Create(2)));
        Button(tools,"Load Project",()=>{var d=new OpenFileDialog{Filter="FighterPort project|*.fpproj"};if(d.ShowDialog()==true)SetProject(ProjectStore.Load(d.FileName));});
        Button(tools,"Save Project",()=>{var d=new SaveFileDialog{Filter="FighterPort project|*.fpproj",FileName="character.fpproj"};if(d.ShowDialog()==true){ProjectStore.Save(P,d.FileName);SetStatus("Saved with native resources and hashes.");}});
        Button(tools,"Import MDL0",()=>Native(async()=>{var d=new OpenFileDialog{Filter="MDL0 v11|*.mdl0"};if(d.ShowDialog()==true)SetProject(await Backend.ImportModel(d.FileName,operation!.Token));}));
        Button(tools,"Attach CHR0",()=>Native(async()=>{if(P.Bones.Count==0)throw new InvalidOperationException("Load a model first.");var d=new OpenFileDialog{Filter="CHR0 v4|*.chr0"};if(d.ShowDialog()==true){P.Clips.Add(await Backend.ImportClip(P,d.FileName,30,operation!.Token));Refresh();SetStatus("Bound by exact bone names; 30 fps display rate (CHR0 does not encode fps). Native originals retained.");}}));
        Button(tools,"Export CHR0",()=>Native(async()=>{var c=SelectedClip??throw new InvalidOperationException("Select a clip.");var d=new SaveFileDialog{Filter="CHR0 v4|*.chr0",FileName="edited.chr0"};if(d.ShowDialog()==true){await Backend.ExportClip(P,c,d.FileName,operation!.Token);SetStatus("Exported CHR0 v4. Native-tool/in-game compatibility is not yet checked.");}}));
        Button(tools,"Cancel job",()=>operation?.Cancel());
        DockPanel.SetDock(status,Dock.Bottom);dock.Children.Add(status);
        var playback=new WrapPanel{Margin=new(10)};DockPanel.SetDock(playback,Dock.Bottom);dock.Children.Add(playback);
        Button(playback,"Play / Pause",()=>playing=!playing);playback.Children.Add(clips);playback.Children.Add(timeline);playback.Children.Add(skeleton);playback.Children.Add(skin);
        var left=new DockPanel{Width=245,Margin=new(10)};DockPanel.SetDock(left,Dock.Left);dock.Children.Add(left);
        var leftTitle=new TextBlock{Text="CHARACTER / SKELETON",Foreground=Brushes.LightSteelBlue,Margin=new(4,0,0,10)};DockPanel.SetDock(leftTitle,Dock.Top);left.Children.Add(leftTitle);left.Children.Add(tree);
        var right=new StackPanel{Width=235,Margin=new(8)};DockPanel.SetDock(right,Dock.Right);dock.Children.Add(right);
        right.Children.Add(new TextBlock{Text="BONE & KEY INSPECTOR",Foreground=Brushes.LightSteelBlue,Margin=new(12)});right.Children.Add(inspector);
        channels.ItemsSource=new[]{"Scale X","Scale Y","Scale Z","Rotate X °","Rotate Y °","Rotate Z °","Translate X cm","Translate Y cm","Translate Z cm"};channels.SelectedIndex=5;
        right.Children.Add(channels);right.Children.Add(value);Button(right,"Set key at current frame",EditKey);Button(right,"Undo",()=>{session.Undo();Render();});Button(right,"Redo",()=>{session.Redo();Render();});
        Button(right,"Frame character",()=>{yaw=.35;pitch=.12;distance=320;panX=0;panY=80;Render();});
        right.Children.Add(new TextBlock{Text="Drag: orbit\nShift + drag: pan\nWheel: zoom\n\nKeys preserve Euler turns and Hermite tangents. Editing adds a locked key.\n\nGame profiles, native materials and playable fighter builds remain planned.",Foreground=Brushes.LightSteelBlue,TextWrapping=TextWrapping.Wrap,Margin=new(12)});
        var border=new Border{Background=new SolidColorBrush(Color.FromRgb(22,30,42)),CornerRadius=new(8),Margin=new(0,10,0,0),Child=viewport};dock.Children.Add(border);
        viewport.Children.Add(new ModelVisual3D{Content=scene});viewport.ClipToBounds=true;
        border.MouseDown+=(_,e)=>{mouse=e.GetPosition(border);border.CaptureMouse();};
        border.MouseUp+=(_,_)=>border.ReleaseMouseCapture();
        border.MouseMove+=(_,e)=>{if(!border.IsMouseCaptured)return;var pos=e.GetPosition(border);var delta=pos-mouse;mouse=pos;if(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)){panX-=delta.X*.3;panY+=delta.Y*.3;}else{yaw-=delta.X*.01;pitch=Math.Clamp(pitch+delta.Y*.01,-1.3,1.3);}Render();};
        border.MouseWheel+=(_,e)=>{distance=Math.Clamp(distance*Math.Exp(-e.Delta*.001),60,1500);Render();};
        timeline.ValueChanged+=(_,_)=>{if(!refreshing)Render();};
        clips.SelectionChanged+=(_,_)=>{if(refreshing)return;timeline.Maximum=Math.Max(0,(SelectedClip?.FrameCount??1)-1);timeline.Value=0;Render();};
        skeleton.Checked+=(_,_)=>Render();skeleton.Unchecked+=(_,_)=>Render();skin.Checked+=(_,_)=>Render();skin.Unchecked+=(_,_)=>Render();
        tree.SelectedItemChanged+=(_,e)=>{if(e.NewValue is TreeViewItem t&&t.Tag is int i){selectedBone=i;Render();}};
        timer.Tick+=(_,_)=>{double now=watch.Elapsed.TotalSeconds;if(playing&&SelectedClip is { } c){double next=timeline.Value+(now-last)*c.FramesPerSecond;if(next>timeline.Maximum){if(c.Loop)next%=c.FrameCount;else{next=timeline.Maximum;playing=false;}}timeline.Value=next;}last=now;};timer.Start();
        Closed+=(_,_)=>{timer.Stop();operation?.Cancel();};
        Refresh();
        if(args.Length==2&&args[0]=="--smoke") Loaded+=async(_,_)=>await Smoke(args[1]);
        if(args.Length==2&&args[0]=="--project")SetProject(ProjectStore.Load(args[1]));
    }
    private static string FindRoot()
    {
        for(var d=new DirectoryInfo(AppContext.BaseDirectory);d!=null;d=d.Parent)if(File.Exists(Path.Combine(d.FullName,"config/repository.json")))return d.FullName;
        return Environment.CurrentDirectory;
    }
    private void Button(Panel panel,string text,Action action)
    {
        var b=new Button{Content=text,Margin=new(3),Padding=new(9,6,9,6),Background=new SolidColorBrush(Color.FromRgb(43,59,77)),Foreground=Brushes.White,BorderThickness=new(0)};
        b.Click+=(_,_)=>{try{action();}catch(Exception e){SetStatus(e.Message);}};panel.Children.Add(b);
    }
    private async void Native(Func<Task> job)
    {
        if(operation!=null){SetStatus("A native job is already running.");return;}
        operation=new();playing=false;SetStatus("Native worker running…");
        try{await job();}catch(Exception e){SetStatus(e.Message);}finally{operation.Dispose();operation=null;}
    }
    private void SetProject(CharacterProject p){ProjectStore.Validate(p);session=new(p);selectedBone=0;playing=false;Refresh();}
    private void SetStatus(string text)=>status.Text=text;
    private void Refresh()
    {
        refreshing=true;clips.ItemsSource=P.Clips.Select(c=>c.Name).ToArray();clips.SelectedIndex=P.Clips.Count>0?0:-1;timeline.Maximum=Math.Max(0,(SelectedClip?.FrameCount??1)-1);timeline.Value=0;
        tree.Items.Clear();var model=new TreeViewItem{Header=P.Name,IsExpanded=true,Foreground=Brushes.White};tree.Items.Add(model);
        model.Items.Add(new TreeViewItem{Header=$"{P.Meshes.Count} mesh regions • {P.Clips.Count} clips",Foreground=Brushes.White});
        var nodes=new List<TreeViewItem>();for(int i=0;i<P.Bones.Count;i++){var b=P.Bones[i];var item=new TreeViewItem{Header=b.Name,Tag=i,IsExpanded=i<8,Foreground=Brushes.White};nodes.Add(item);if(b.Parent<0)model.Items.Add(item);else nodes[b.Parent].Items.Add(item);}
        refreshing=false;SetStatus($"{P.Name} • {P.Bones.Count} joints • {P.Meshes.Sum(m=>m.Vertices.Count)} vertices • {P.TargetProfile}");Render();
    }
    private void EditKey()
    {
        if(SelectedClip==null||P.Bones.Count==0)throw new InvalidOperationException("Select a character and clip.");
        session.EditKey(clips.SelectedIndex,P.Bones[selectedBone].Name,channels.SelectedIndex,(float)Math.Round(timeline.Value),float.Parse(value.Text,CultureInfo.InvariantCulture));Render();SetStatus("Key changed; undo/redo available. Save Project to retain edits.");
    }
    private static Point3D Point(System.Numerics.Vector3 v)=>new(v.X,v.Y,v.Z);
    private void Render()
    {
        if(refreshing)return;
        scene.Children.Clear();scene.Children.Add(new AmbientLight(Color.FromRgb(90,90,90)));scene.Children.Add(new DirectionalLight(Colors.White,new(-1,-2,-3)));
        var target=new Point3D(panX,panY,0);var eye=new Point3D(panX+distance*Math.Sin(yaw)*Math.Cos(pitch),panY+distance*Math.Sin(pitch),distance*Math.Cos(yaw)*Math.Cos(pitch));
        viewport.Camera=new PerspectiveCamera(eye,target-eye,new(0,1,0),45){NearPlaneDistance=.1,FarPlaneDistance=3000};
        var world=Animation.World(P,SelectedClip,(float)timeline.Value);
        if(skin.IsChecked==true)foreach(var mesh in P.Meshes)
        {
            var geometry=new MeshGeometry3D();foreach(var v in mesh.Vertices){var skinned=Animation.Skin(v,P,world);geometry.Positions.Add(Point(skinned.Position));geometry.Normals.Add(new(skinned.Normal.X,skinned.Normal.Y,skinned.Normal.Z));geometry.TextureCoordinates.Add(new(v.UV[0],v.UV[1]));}
            geometry.TriangleIndices=new Int32Collection(mesh.Indices);
            var color=Color.FromScRgb(1,mesh.Color[0],mesh.Color[1],mesh.Color[2]);
            var drawing=new DrawingGroup();drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(color),null,new RectangleGeometry(new(0,0,16,16))));
            drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromArgb(55,255,255,255)),null,new RectangleGeometry(new(0,0,8,8))));drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromArgb(55,255,255,255)),null,new RectangleGeometry(new(8,8,8,8))));
            var brush=new DrawingBrush(drawing){TileMode=TileMode.Tile,Viewport=new(0,0,.25,.25),ViewportUnits=BrushMappingMode.RelativeToBoundingBox};
            var material=new DiffuseMaterial(brush);scene.Children.Add(new GeometryModel3D(geometry,material){BackMaterial=material});
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
            timeline.Value=30;clips.SelectedIndex=2;timeline.Value=30;channels.SelectedIndex=4;selectedBone=10;value.Text="-45";EditKey();session.Undo();session.Redo();
            string project=Path.Combine(output,"gui.fpproj");ProjectStore.Save(P,project);SetProject(ProjectStore.Load(project));clips.SelectedIndex=2;timeline.Value=30;
            await Task.Delay(300);Capture(Path.Combine(output,"edited.png"));
            File.WriteAllText(Path.Combine(output,"gui-smoke.txt"),$"PASS: Loaded window {ActualWidth}x{ActualHeight}; demo viewport; scrub; key edit; undo/redo; save/reopen; screenshot. Rendering tier: {RenderCapability.Tier>>16}. Native tool/in-game checks unrun.");
            Application.Current.Shutdown(0);
        }
        catch(Exception e){File.WriteAllText(Path.Combine(output,"gui-smoke.txt"),e.ToString());Application.Current.Shutdown(1);}
    }
    private void Capture(string path)
    {
        var bitmap=new RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(this);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(path);encoder.Save(stream);
    }
}
