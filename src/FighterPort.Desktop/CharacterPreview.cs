using FighterPort.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FighterPort.Desktop;

/// <summary>Depth-buffered CPU-skinned comparison viewport; no native material claims.</summary>
internal sealed class CharacterPreview : Border
{
    private readonly Viewport3D viewport=new();
    private readonly Model3DGroup scene=new();
    private double yaw=.3,pitch=.1,distance=320;
    private System.Numerics.Vector3 center=new(0,80,0);
    private Point mouse;
    public CharacterPreview()
    {
        Background=new SolidColorBrush(Color.FromRgb(22,30,42));Child=viewport;Margin=new(4);MinHeight=230;
        viewport.ClipToBounds=true;viewport.Children.Add(new ModelVisual3D{Content=scene});
        SizeChanged+=(_,_)=>Camera();
        MouseDown+=(_,e)=>{mouse=e.GetPosition(this);CaptureMouse();};MouseUp+=(_,_)=>ReleaseMouseCapture();
        MouseMove+=(_,e)=>{if(!IsMouseCaptured)return;var p=e.GetPosition(this);var d=p-mouse;mouse=p;yaw-=d.X*.01;pitch=Math.Clamp(pitch+d.Y*.01,-1.3,1.3);Camera();};
        MouseWheel+=(_,e)=>{distance=Math.Clamp(distance*Math.Exp(-e.Delta*.001),1,10000);Camera();};
    }
    public void Fit(CharacterProject p)
    {
        var points=p.Meshes.SelectMany(m=>m.Vertices).Select(v=>Animation.V(v.Position)).Concat(Animation.World(p,null,0).Select(m=>m.Translation)).ToArray();
        if(points.Length==0)return;
        var min=points.Aggregate(System.Numerics.Vector3.Min);var max=points.Aggregate(System.Numerics.Vector3.Max);
        center=(min+max)/2;distance=Math.Max(10,(max-min).Length()*1.6);Camera();
    }
    private void Camera()
    {
        var target=new Point3D(center.X,center.Y,center.Z);
        double fittedDistance=distance*Math.Max(1,ActualWidth/Math.Max(1,ActualHeight));
        var eye=new Point3D(center.X+fittedDistance*Math.Sin(yaw)*Math.Cos(pitch),center.Y+fittedDistance*Math.Sin(pitch),center.Z+fittedDistance*Math.Cos(yaw)*Math.Cos(pitch));
        viewport.Camera=new PerspectiveCamera(eye,target-eye,new(0,1,0),45){NearPlaneDistance=.01,FarPlaneDistance=50000};
    }
    public void Show(CharacterProject p,Clip? clip,float frame)
    {
        scene.Children.Clear();scene.Children.Add(new AmbientLight(Color.FromRgb(95,95,95)));scene.Children.Add(new DirectionalLight(Colors.White,new(-1,-2,-3)));
        var world=Animation.World(p,clip,frame);
        foreach(var mesh in p.Meshes)
        {
            var g=new MeshGeometry3D();foreach(var v in mesh.Vertices){var s=Animation.Skin(v,p,world);g.Positions.Add(new(s.Position.X,s.Position.Y,s.Position.Z));g.Normals.Add(new(s.Normal.X,s.Normal.Y,s.Normal.Z));}
            g.TriangleIndices=new(mesh.Indices);var material=new DiffuseMaterial(new SolidColorBrush(Color.FromScRgb(1,mesh.Color[0],mesh.Color[1],mesh.Color[2])));
            scene.Children.Add(new GeometryModel3D(g,material){BackMaterial=material});
        }
        // Visible rest skeleton even for a project without meshes.
        foreach(var w in world)
        {
            var p0=w.Translation;float r=(float)Math.Max(.1,distance/250);
            var g=new MeshGeometry3D();foreach(var d in new[]{new System.Numerics.Vector3(r,0,0),new(0,r,0),new(-r,0,0),new(0,-r,0),new(0,0,r),new(0,0,-r)}){var v=p0+d;g.Positions.Add(new(v.X,v.Y,v.Z));}
            g.TriangleIndices=new([0,1,4,1,2,4,2,3,4,3,0,4,1,0,5,2,1,5,3,2,5,0,3,5]);var mat=new EmissiveMaterial(Brushes.White);scene.Children.Add(new GeometryModel3D(g,mat){BackMaterial=mat});
        }
        Camera();
    }
}
