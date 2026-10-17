using System.Text.Json;

namespace FighterPort.Core;

public sealed record MaterialImage(int Width,int Height,byte[] Rgba);

/// <summary>Bounded static color/alpha reference surface. Each sampler is filtered before TEV;
/// this does not replace the model viewport's lighting, depth/cull or hardware precision.</summary>
public static class MaterialSurface
{
    public static void Validate(PreviewMaterial material)
    {
        var p=material.Program;
        if(p.ActiveStages<1||p.ActiveStages>16||p.ActiveStages>p.Stages.Count||p.Stages.Count>16||p.Colors.Length!=4||p.Constants.Length!=4||p.Colors.Concat(p.Constants).Any(c=>c.Length!=4||c.Any(v=>v<0||v>255)))throw new InvalidDataException("Missing/unsupported native TEV stages or signed registers");
        if(!p.IdentitySwap0)throw new InvalidDataException("Native swap table unsupported; retained");
        if(p.BlendLogic||p.SubtractBlend||p.Blend&&(p.SourceFactor!=4||p.DestinationFactor!=5))throw new InvalidDataException("Only opaque / source-alpha blend reference supported");
        if(p.Compare0 is <0 or >7||p.Compare1 is <0 or >7||p.AlphaLogic is <0 or >3||p.Reference0 is <0 or >255||p.Reference1 is <0 or >255)throw new InvalidDataException("Unknown alpha-test state");
        bool previousColor=false,previousAlpha=false;
        foreach(var s in p.Stages.Take(p.ActiveStages))
        {
            if(s.Color.Length!=4||s.Alpha.Length!=4||s.Color.Any(v=>v<0||v>15||v is 10 or 11)||s.Alpha.Any(v=>v<0||v>7||v==5)||s.ColorOperation is <0 or >1||s.AlphaOperation is <0 or >1||!s.ColorClamp||!s.AlphaClamp||s.ColorRegister is <0 or >3||s.AlphaRegister is <0 or >3||s.ColorBias is <0 or >2||s.AlphaBias is <0 or >2||s.ColorScale is <0 or >3||s.AlphaScale is <0 or >3||s.NativeIndirect!=0||s.TextureSwap!=0||s.RasterSwap!=0)throw new InvalidDataException("Unsupported native TEV raster/compare/swap/indirect/unclamped stage; retained");
            if(!previousColor&&s.Color.Contains(0)||!previousAlpha&&(s.Color.Contains(1)||s.Alpha.Contains(0)))throw new InvalidDataException("Uninitialized previous-register dependency unsupported");
            if(s.Color.Contains(14)&&!ConstantSupported(s.ConstantColor,true)||s.Alpha.Contains(6)&&!ConstantSupported(s.ConstantAlpha,false))throw new InvalidDataException("Unsupported TEV constant selector");
            bool texture=s.Color.Any(v=>v is 8 or 9)||s.Alpha.Contains(4);
            if(texture)
            {
                if(!s.TextureEnabled||!material.Textures.Any(t=>t.Slot==s.TextureMap))throw new InvalidDataException("Unresolved native stage texture");
                var gen=material.Textures.SingleOrDefault(t=>t.Slot==s.TextureCoord)??throw new InvalidDataException("Unresolved native stage coordinate generator");
                if(gen.RestSrt.Length!=5||gen.RestSrt.Any(v=>!float.IsFinite(v))||!UvEvaluator.DirectCoordinates(gen)||gen.UVSet!=0||gen.MatrixMode is not ("MatrixMaya" or "MatrixXSI" or "Matrix3dsMax")||gen.MatrixMode=="MatrixMaya"&&gen.RestSrt[2]!=0)throw new InvalidDataException("Reference surface requires direct UV0 / supported static matrix; native records retained");
                var sampler=material.Textures.Single(t=>t.Slot==s.TextureMap);if(sampler.Filter is not ("Nearest" or "Linear")||!new[]{"Clamp","Repeat","Mirror"}.Contains(sampler.WrapU)||!new[]{"Clamp","Repeat","Mirror"}.Contains(sampler.WrapV))throw new InvalidDataException("Unsupported sampler addressing/filter");
            }
            previousColor|=s.ColorRegister==0;previousAlpha|=s.AlphaRegister==0;
        }
        if(!previousColor||!previousAlpha)throw new InvalidDataException("Native program does not write final color and alpha");
    }
    private static bool ConstantSupported(int value,bool color)=>value is >=0 and <=7||color&&value is >=12 and <=15||value is >=16 and <=31;
    public static byte[] Render(PreviewMaterial material,IReadOnlyDictionary<int,MaterialImage> textures,int width,int height)
    {
        Validate(material);
        if(width<1||height<1||width>256||height>256||(long)width*height*material.Program.ActiveStages>1048576||textures.Values.Sum(i=>(long)i.Rgba.Length)>32*1024*1024)throw new InvalidDataException("Material reference surface quota");
        foreach(var image in textures.Values)if(image.Width<1||image.Height<1||image.Width>4096||image.Height>4096||(long)image.Width*image.Height*4!=image.Rgba.Length)throw new InvalidDataException("Invalid material image");
        foreach(var stage in material.Program.Stages.Take(material.Program.ActiveStages).Where(s=>s.TextureEnabled&&(s.Color.Any(v=>v is 8 or 9)||s.Alpha.Contains(4))))
        {
            var image=textures.GetValueOrDefault(stage.TextureMap)??throw new InvalidDataException("Missing decoded native sampler");var gen=material.Textures.Single(t=>t.Slot==stage.TextureCoord);
            var origin=UvEvaluator.Transform(gen.MatrixMode,gen.RestSrt,new(0,0));var dx=UvEvaluator.Transform(gen.MatrixMode,gen.RestSrt,new(1f/width,0))-origin;var dy=UvEvaluator.Transform(gen.MatrixMode,gen.RestSrt,new(0,1f/height))-origin;
            float Footprint(System.Numerics.Vector2 d)=>MathF.Sqrt(d.X*d.X*image.Width*image.Width+d.Y*d.Y*image.Height*image.Height);
            if(Footprint(dx)>1.00001f||Footprint(dy)>1.00001f)throw new InvalidDataException("Reference surface requires magnification; minification/LOD is not implemented");
        }
        var output=new byte[width*height*4];for(int y=0;y<height;y++)for(int x=0;x<width;x++)
        {
            var c=Evaluate(material,(slot,u,v)=>Sample(textures.GetValueOrDefault(slot)??throw new InvalidDataException("Missing decoded native sampler"),material.Textures.Single(t=>t.Slot==slot),u,v),(x+.5f)/width,(y+.5f)/height);
            for(int channel=0;channel<4;channel++)output[(y*width+x)*4+channel]=(byte)Math.Clamp((int)MathF.Round(c[channel]*255),0,255);
        }
        return output;
    }
    public static float[] Pixel(PreviewMaterial material,Func<int,float,float,float[]> sample,float u,float v)
    {
        if(!float.IsFinite(u)||!float.IsFinite(v))throw new InvalidDataException("Nonfinite material coordinate");
        Validate(material);return Evaluate(material,sample,u,v);
    }
    private static float[] Evaluate(PreviewMaterial material,Func<int,float,float,float[]> sample,float u,float v)
    {
        var p=material.Program;var registers=p.Colors.Select(c=>c.Select(x=>x/255f).ToArray()).ToArray();
        foreach(var s in p.Stages.Take(p.ActiveStages))
        {
            float[] tex=[0,0,0,0];if(s.TextureEnabled&&(s.Color.Any(v=>v is 8 or 9)||s.Alpha.Contains(4))){var gen=material.Textures.Single(t=>t.Slot==s.TextureCoord);var uv=UvEvaluator.Transform(gen.MatrixMode,gen.RestSrt,new(u,1-v));tex=sample(s.TextureMap,uv.X,1-uv.Y);if(tex.Length!=4||tex.Any(v=>!float.IsFinite(v)||v<0||v>1))throw new InvalidDataException("Invalid texture sample");}
            float Color(int selector,int c)=>selector switch{0=>registers[0][c],1=>registers[0][3],>=2 and <=7=>registers[selector/2][selector%2==0?c:3],8=>tex[c],9=>tex[3],12=>1,13=>.5f,14=>Constant(p,s.ConstantColor,c,true),15=>0,_=>throw new InvalidDataException("Unsupported raster selector")};
            float Alpha(int selector)=>selector switch{>=0 and <=3=>registers[selector][3],4=>tex[3],6=>Constant(p,s.ConstantAlpha,3,false),7=>0,_=>throw new InvalidDataException("Unsupported raster alpha")};
            float[] rgb=new float[3];for(int c=0;c<3;c++)rgb[c]=Combine(Color(s.Color[0],c),Color(s.Color[1],c),Color(s.Color[2],c),Color(s.Color[3],c),s.ColorOperation,s.ColorBias,s.ColorScale);
            float alpha=Combine(Alpha(s.Alpha[0]),Alpha(s.Alpha[1]),Alpha(s.Alpha[2]),Alpha(s.Alpha[3]),s.AlphaOperation,s.AlphaBias,s.AlphaScale);
            for(int c=0;c<3;c++)registers[s.ColorRegister][c]=rgb[c];registers[s.AlphaRegister][3]=alpha;
        }
        var result=registers[0];int a=(int)MathF.Round(result[3]*255);bool c0=Compare(p.Compare0,a,p.Reference0),c1=Compare(p.Compare1,a,p.Reference1);bool pass=p.AlphaLogic switch{0=>c0&&c1,1=>c0||c1,2=>c0!=c1,3=>c0==c1,_=>false};if(!pass)return [0,0,0,0];if(!p.Blend)result[3]=1;return result;
    }
    private static float Combine(float a,float b,float c,float d,int operation,int bias,int scale)=>Math.Clamp((d+(operation==0?1:-1)*((1-c)*a+c*b)+(bias==1?.5f:bias==2?-.5f:0))*(scale==1?2:scale==2?4:scale==3?.5f:1),0,1);
    private static float Constant(NativeMaterialProgram p,int selector,int component,bool color)=>selector<=7?1-selector/8f:selector<=15?p.Constants[selector-12][component]/255f:p.Constants[(selector-16)%4][(selector-16)/4]/255f;
    private static bool Compare(int op,int a,int b)=>op switch{0=>false,1=>a<b,2=>a==b,3=>a<=b,4=>a>b,5=>a!=b,6=>a>=b,7=>true,_=>throw new InvalidDataException("Invalid alpha comparison")};
    private static float[] Sample(MaterialImage image,TextureBinding binding,float u,float v)
    {
        int Address(int i,int n,string mode){if(mode=="Clamp")return Math.Clamp(i,0,n-1);int period=mode=="Mirror"?n*2:n;int k=((i%period)+period)%period;return k<n?k:period-1-k;}
        float[] Texel(int x,int y){int i=(Address(y,image.Height,binding.WrapV)*image.Width+Address(x,image.Width,binding.WrapU))*4;return Enumerable.Range(0,4).Select(c=>image.Rgba[i+c]/255f).ToArray();}
        if(!float.IsFinite(u)||!float.IsFinite(v)||Math.Abs(u)>1024||Math.Abs(v)>1024)throw new InvalidDataException("Material UV quota");
        if(binding.Filter=="Nearest")return Texel((int)MathF.Floor(u*image.Width),(int)MathF.Floor(v*image.Height));
        float x=u*image.Width-.5f,y=v*image.Height-.5f;int ix=(int)MathF.Floor(x),iy=(int)MathF.Floor(y);float fx=x-ix,fy=y-iy;var output=new float[4];for(int dy=0;dy<2;dy++)for(int dx=0;dx<2;dx++){var t=Texel(ix+dx,iy+dy);for(int c=0;c<4;c++)output[c]+=t[c]*(dx==0?1-fx:fx)*(dy==0?1-fy:fy);}return output;
    }
}

public sealed partial class ResourceWorkspace
{
    public async Task<MaterialImage> TexturePixels(AssetHandle texture,string palette="",CancellationToken ct=default)
    {
        var decoded=JsonSerializer.Deserialize<TexturePixels>(await Request("resource-texture-pixels",Document(texture),texture.NodeId,ct,palette:palette),ProjectStore.Json)??throw new InvalidDataException("Empty native texture pixels");
        return new(decoded.Width,decoded.Height,Convert.FromBase64String(decoded.Rgba));
    }
}
