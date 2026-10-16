using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FighterPort.Desktop;

/// <summary>Finite native-addressed image domain. Affine coordinates preserve interpolation across seams.</summary>
internal sealed class TextureSampling
{
    private readonly Dictionary<(BitmapSource,string,string,int,int,int,int),BitmapSource> cache=[];
    private long bytes;
    public void Clear(){cache.Clear();bytes=0;}
    public (Brush Brush,Point[] Coordinates) Prepare(BitmapSource source,Point[] canonical,string wrapU,string wrapV,string filter)
    {
        if(canonical.Length==0)throw new InvalidOperationException("Missing texture coordinates");
        // Brawl decoding/GL upload uses bitmap row 0 at native t=0; canonical V was inverted at import.
        var uv=canonical.Select(p=>new Point(p.X,1-p.Y)).ToArray();
        if(uv.Any(p=>!double.IsFinite(p.X)||!double.IsFinite(p.Y)||Math.Abs(p.X)>1024||Math.Abs(p.Y)>1024))throw new InvalidOperationException("Texture coordinate preview quota");
        int x0=(int)Math.Floor(uv.Min(p=>p.X)),y0=(int)Math.Floor(uv.Min(p=>p.Y));
        int x1=Math.Max(x0+1,(int)Math.Ceiling(uv.Max(p=>p.X))),y1=Math.Max(y0+1,(int)Math.Ceiling(uv.Max(p=>p.Y)));
        int w=source.PixelWidth,h=source.PixelHeight,aw=checked((x1-x0)*w+2),ah=checked((y1-y0)*h+2);
        aw=PowerOfTwo(aw);ah=PowerOfTwo(ah);
        if(aw>4096||ah>4096||(long)aw*ah>4*1024*1024)throw new InvalidOperationException("Addressed texture preview exceeds 4 megapixels; neutral fallback");
        var key=(source,wrapU,wrapV,x0,y0,x1,y1);
        if(!cache.TryGetValue(key,out var atlas))
        {
            var bitmap=new FormatConvertedBitmap(source,PixelFormats.Bgra32,null,0);byte[] input=new byte[w*h*4],output=new byte[aw*ah*4];bitmap.CopyPixels(input,w*4,0);
            for(int y=0;y<ah;y++)for(int x=0;x<aw;x++)
            {int src=(Address(x+x0*w-1,w,wrapU)+Address(y+y0*h-1,h,wrapV)*w)*4;Buffer.BlockCopy(input,src,output,(y*aw+x)*4,4);}
            if(bytes+output.Length>32*1024*1024)Clear();
            atlas=BitmapSource.Create(aw,ah,96,96,PixelFormats.Bgra32,null,output,aw*4);atlas.Freeze();cache.Add(key,atlas);bytes+=output.Length;
        }
        var brush=new ImageBrush(atlas){ViewportUnits=BrushMappingMode.Absolute,Viewport=new(0,0,1,1),ViewboxUnits=BrushMappingMode.RelativeToBoundingBox,Viewbox=new(0,0,1,1),Stretch=Stretch.Fill,TileMode=TileMode.None};
        RenderOptions.SetBitmapScalingMode(brush,filter.Contains("Nearest")?BitmapScalingMode.NearestNeighbor:BitmapScalingMode.Linear);brush.Freeze();
        return (brush,uv.Select(p=>new Point(((p.X-x0)*w+1)/aw,((p.Y-y0)*h+1)/ah)).ToArray());
    }
    private static int PowerOfTwo(int n){int value=1;while(value<n&&value<8192)value*=2;return value;}
    private static int Address(int index,int size,string mode)
    {
        if(mode=="Clamp")return Math.Clamp(index,0,size-1);
        int period=mode=="Mirror"?2*size:mode=="Repeat"?size:throw new InvalidOperationException("Unknown native wrap mode: "+mode);
        int i=((index%period)+period)%period;return i<size?i:period-1-i;
    }
}
