using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using FighterPort.Core;

namespace FighterPort.Desktop;

internal static class SamplingSmoke
{
    internal static int Run(string output,string? nativePng=null)
    {
        Directory.CreateDirectory(output);
        try
        {
            // Non-square, asymmetric texel colors; no renderer or native codec used for expected values.
            const int w=8,h=4;byte[] texels=new byte[w*h*4];
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=(y*w+x)*4;texels[i]=(byte)(20+y*50);texels[i+1]=(byte)(10+x*25);texels[i+2]=(byte)(200-x*15-y*20);texels[i+3]=(byte)(x>=6&&y==3?0:255);}
            BitmapSource image=BitmapSource.Create(w,h,96,96,PixelFormats.Bgra32,null,texels,w*4);
            if(nativePng!=null){using var input=File.OpenRead(nativePng);image=new PngBitmapDecoder(input,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];var native=new FormatConvertedBitmap(image,PixelFormats.Bgra32,null,0);byte[] decoded=new byte[texels.Length];native.CopyPixels(decoded,w*4,0);if(!decoded.SequenceEqual(texels))throw new InvalidOperationException("Native asymmetric decode differs from independent colors");}
            image.Freeze();var sampling=new TextureSampling();int checks=0;int maxError=0;var errors=new List<string>();
            // Orthographic quad covers 128x128; probe exact texel centers, including outside 0..1.
            foreach(string filter in new[]{"Nearest","Linear"})foreach(string u in new[]{"Clamp","Repeat","Mirror"})foreach(string v in new[]{"Clamp","Repeat","Mirror"})
            {
                var native=new[]{new Point(-1,-1),new Point(2,-1),new Point(2,2),new Point(-1,2)};
                var result=sampling.Prepare(image,native.Select(p=>new Point(p.X,1-p.Y)).ToArray(),u,v,filter);
                var rendered=Render(result.Brush,result.Coordinates);
                foreach(int py in new[]{16,48,80,112})foreach(int px in new[]{16,48,80,112})
                {
                    double a=-1+3*(px+.5)/128.0,b=-1+3*(py+.5)/128.0;
                    byte[] expected=Linear(texels,w,h,a,b,u,v);byte[] pixel=new byte[4];rendered.CopyPixels(new Int32Rect(px,py,1,1),pixel,4,0);
                    for(int c=0;c<4;c++){int error=Math.Abs(pixel[c]-expected[c]);maxError=Math.Max(maxError,error);if(error>3)errors.Add($"{u}/{v} ({px},{py}) c{c}: {pixel[c]} != {expected[c]}");}checks++;
                }
            }
            var project=new CharacterProject{Materials=[new(){Id="a",Textures=[new(){RestSrt=[-1.25f,.75f,0,.125f,-.25f]}]},new(){Id="b",Textures=[new(){RestSrt=[.5f,-1,0,-.5f,.125f]}]}]};
            var group=new UvGroup{FrameCount=21,Loop=true,MatrixMode="MatrixMaya",Tracks=[new(){MaterialName="a",MaterialId="a",Curves=[new(){Channel=3,Keys=[new(){Frame=0,Value=0,Tangent=.025f},new(){Frame=20,Value=.5f,Tangent=.025f}]}]}]};
            // Two distinct material submissions/UV ranges in one viewport, reversed scale and fractional seek/reset.
            foreach(float frame in new[]{0,5.25f,20.5f,21.25f,-.25f,10,0})foreach(bool enabled in new[]{true,false})
            {
                group.Enabled=enabled;
                var corners=new[]{new Point(-1,-1),new Point(2,-1),new Point(2,2),new Point(-1,2)};
                Point[] Sample(string id)=>corners.Select(p=>UvEvaluator.Sample(project,id,group,frame,new((float)p.X,(float)(1-p.Y)))).Select(p=>new Point(p.X,p.Y)).ToArray();
                var left=sampling.Prepare(image,Sample("a"),"Repeat","Clamp","Linear");var right=sampling.Prepare(image,Sample("b"),"Mirror","Repeat","Linear");
                var rendered=Render(left.Brush,left.Coordinates,right.Brush,right.Coordinates);
                for(int side=0;side<2;side++)foreach(int px in new[]{8,24,40,56})foreach(int py in new[]{16,48,80,112})
                {
                    double u=-1+3*(px+.5)/64,v=-1+3*(py+.5)/128;
                    double f=((frame%21)+21)%21,tx=enabled?Math.Min(f,20)*.025:.125;
                    double a=side==0?-1.25*(u-tx):.5*(u+.5),b=side==0?1-.75*(1-v+.25):1+(1-v-.125);
                    var expected=Linear(texels,w,h,a,b,side==0?"Repeat":"Mirror",side==0?"Clamp":"Repeat");byte[] pixel=new byte[4];rendered.CopyPixels(new Int32Rect(px+64*side,py,1,1),pixel,4,0);
                    for(int c=0;c<4;c++){int error=Math.Abs(pixel[c]-expected[c]);maxError=Math.Max(maxError,error);if(error>3)errors.Add($"Animated/reset {frame} {enabled} side {side} ({px},{py}) c{c}: {pixel[c]} != {expected[c]}");}checks++;
                }
            }
            if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors.Take(20)));
            var oldBrush=new ImageBrush(image){ViewportUnits=BrushMappingMode.Absolute,Viewport=new(0,0,1,1),TileMode=TileMode.Tile,Stretch=Stretch.Fill};
            var canonical=new[]{new Point(0,1),new Point(1,1),new Point(1,0),new Point(0,0)};
            var old=Render(oldBrush,canonical);var corrected=sampling.Prepare(image,canonical,"Repeat","Repeat","Linear");var current=Render(corrected.Brush,corrected.Coordinates);
            byte[] aPixel=new byte[4],bPixel=new byte[4];old.CopyPixels(new Int32Rect(16,16,1,1),aPixel,4,0);current.CopyPixels(new Int32Rect(16,16,1,1),bPixel,4,0);
            if(aPixel[0]<140||bPixel[0]>25)throw new InvalidOperationException("Expected old vertical-placement defect/new native-row orientation was not reproduced");
            Save(old,Path.Combine(output,"before-original.png"));Save(current,Path.Combine(output,"after-original.png"));
            File.WriteAllText(Path.Combine(output,"sampling.txt"),$"PASS {checks} rendered samples; all nine address combinations; max channel error {maxError}/255; native PNG {nativePng!=null}; old vertical-placement defect reproduced. WPF nearest request measured as bilinear; nearest/LOD fidelity unsupported.");return 0;
        }
        catch(Exception e){File.WriteAllText(Path.Combine(output,"sampling.txt"),e.ToString());return 1;}
    }
    private static void Save(BitmapSource image,string path){var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));using var file=File.Create(path);encoder.Save(file);}
    private static byte[] Linear(byte[] pixels,int w,int h,double u,double v,string wu,string wv)
    {
        double x=u*w-.5,y=v*h-.5;int ix=(int)Math.Floor(x),iy=(int)Math.Floor(y);double fx=x-ix,fy=y-iy;var result=new byte[4];
        for(int c=0;c<4;c++)
        {
            double value=0;for(int dy=0;dy<2;dy++)for(int dx=0;dx<2;dx++)
            {int xx=Expected((ix+dx+.5)/w,w,wu),yy=Expected((iy+dy+.5)/h,h,wv);value+=pixels[(yy*w+xx)*4+c]*(c==3?1:pixels[(yy*w+xx)*4+3]/255.0)*(dx==0?1-fx:fx)*(dy==0?1-fy:fy);}
            result[c]=(byte)Math.Round(value);
        }
        return result;
    }
    private static int Expected(double t,int size,string wrap)
    {
        if(wrap=="Clamp")t=Math.Clamp(t,0,1);
        else if(wrap=="Repeat")t-=Math.Floor(t);
        else {double r=t-2*Math.Floor(t/2);t=r<=1?r:2-r;}
        return Math.Min(size-1,(int)Math.Floor(t*size));
    }
    internal static RenderTargetBitmap Render(Brush brush,Point[] uv,Brush? second=null,Point[]? secondUv=null)
    {
        var viewport=new Viewport3D{Width=128,Height=128,Camera=new OrthographicCamera(new(0,0,2),new(0,0,-1),new(0,1,0),2)};
        viewport.Children.Add(new ModelVisual3D{Content=new AmbientLight(Colors.White)});
        void Add(Brush color,Point[] coordinates,double x0,double x1)
        {
            var mesh=new MeshGeometry3D{Positions=new(new[]{new Point3D(x0,1,0),new Point3D(x1,1,0),new Point3D(x1,-1,0),new Point3D(x0,-1,0)}),TextureCoordinates=new(coordinates),TriangleIndices=new(new[]{0,2,1,0,3,2})};
            var material=new DiffuseMaterial(color);viewport.Children.Add(new ModelVisual3D{Content=new GeometryModel3D(mesh,material){BackMaterial=material}});
        }
        Add(brush,uv,-1,second==null?1:0);if(second!=null)Add(second,secondUv!,0,1);
        viewport.Measure(new(128,128));viewport.Arrange(new Rect(0,0,128,128));viewport.UpdateLayout();
        var bitmap=new RenderTargetBitmap(128,128,96,96,PixelFormats.Pbgra32);bitmap.Render(viewport);return bitmap;
    }
}
