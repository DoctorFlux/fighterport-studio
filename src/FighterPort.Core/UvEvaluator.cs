using System.Numerics;

namespace FighterPort.Core;

/// <summary>Direct Maya ST and XSI/3dsMax SRT. Canonical V is the inverse of native V.</summary>
public static class UvEvaluator
{
    public static string Support(CharacterProject project,UvGroup group,UvTrack track)
    {
        var material=project.Materials.SingleOrDefault(m=>m.Id==track.MaterialId);
        var sampler=material?.Textures.SingleOrDefault(t=>t.Slot==track.Slot);
        if(sampler==null)return "Unresolved material/sampler; retained unevaluated";
        if(track.Indirect||track.Slot!=material!.PreviewSlot)return "Indirect/nonselected sampler retained unevaluated";
        if(!DirectCoordinates(sampler))return "Unsupported texture-coordinate generation; retained unevaluated";
        if(!Known(group.MatrixMode)||!Known(sampler.MatrixMode))return "Unknown UV matrix retained unevaluated";
        if(group.MatrixMode=="MatrixMaya"&&(sampler.RestSrt[2]!=0||track.Curves.Where(c=>c.Channel==2).Any(c=>c.Keys.Any(k=>k.Value!=0||k.Tangent!=0))))return "Rotating Maya UV matrix retained unevaluated (backend rotation-only discrepancy)";
        return "Evaluated: direct "+group.MatrixMode+" isolated sampler "+track.Slot+" UV"+sampler.UVSet+"; composition unsupported";
    }
    public static void Validate(CharacterProject project,UvGroup group)
    {
        if(group.FrameCount<1||group.FrameCount>100000||group.Tracks.Count>1024||group.Tracks.Sum(t=>t.Curves.Sum(c=>(long)c.Keys.Count))>100000)throw new InvalidDataException("UV group quota");
        if(group.Tracks.Select(t=>(t.MaterialName,t.Slot,t.Indirect)).Distinct().Count()!=group.Tracks.Count)throw new InvalidDataException("Duplicate UV track binding");
        var bound=group.Tracks.Where(t=>t.MaterialId!="").ToArray();
        if(bound.Select(t=>(t.MaterialId,t.Slot,t.Indirect)).Distinct().Count()!=bound.Length)throw new InvalidDataException("Duplicate bound UV target");
        foreach(var t in group.Tracks)
        {
            if(t.Slot<0||t.Slot>7||t.MaterialId!=""&&!project.Materials.Any(m=>m.Id==t.MaterialId)||t.Curves.Count>5||t.Curves.Select(c=>c.Channel).Distinct().Count()!=t.Curves.Count)throw new InvalidDataException("Invalid UV sampler/channels");
            foreach(var c in t.Curves)
            {
                if(c.Channel<0||c.Channel>4||c.Interpolation!="Hermite"||c.Keys.Count==0)throw new InvalidDataException("Unsupported UV curve");
                float previous=-1;
                foreach(var key in c.Keys){if(!float.IsFinite(key.Frame)||!float.IsFinite(key.Value)||!float.IsFinite(key.Tangent)||key.Frame<=previous||key.Frame<0||key.Frame>group.FrameCount)throw new InvalidDataException("Invalid UV keys");previous=key.Frame;}
            }
        }
    }
    public static Vector2 Sample(CharacterProject project,string materialId,UvGroup? group,float frame,Vector2 uv)
    {
        var material=project.Materials.SingleOrDefault(m=>m.Id==materialId);
        var binding=material?.Textures.SingleOrDefault(t=>t.Slot==material.PreviewSlot);
        if(binding==null||!DirectCoordinates(binding)||!Known(binding.MatrixMode))return uv;
        var values=binding.RestSrt.ToArray();
        string mode=binding.MatrixMode;
        if(group is {Enabled:true})
        {
            var track=group.Tracks.SingleOrDefault(t=>t.MaterialId==materialId&&t.Slot==binding.Slot&&!t.Indirect);
            if(track!=null&&Support(project,group,track).StartsWith("Evaluated:"))
            {
                float f=group.Loop?((frame%group.FrameCount)+group.FrameCount)%group.FrameCount:Math.Clamp(frame,0,group.FrameCount-1);
                foreach(var c in track.Curves)values[c.Channel]=Animation.Evaluate(c,f);
                mode=group.MatrixMode;
            }
        }
        return Transform(mode,values,uv);
    }
    private static bool Known(string mode)=>mode is "MatrixMaya" or "MatrixXSI" or "Matrix3dsMax";
    public static bool DirectCoordinates(TextureBinding binding)=>binding.UVSet is >=0 and <=7&&binding.Projection is "" or "ST"&&binding.Texgen is "" or "Regular";
    public static Vector2[] MeshCoordinates(CharacterProject project,Mesh mesh,string materialId,UvGroup? group,float frame)
    {
        var material=project.Materials.Single(m=>m.Id==materialId);var binding=material.Textures.Single(t=>t.Slot==material.PreviewSlot);
        if(!DirectCoordinates(binding))throw new InvalidDataException("Unsupported native texgen/projection; neutral fallback");
        var uv=binding.UVSet==0?mesh.Vertices.Select(v=>v.UV).ToArray():mesh.NativeAttributes.GetValueOrDefault("BrawlUV"+binding.UVSet);
        if(uv==null||uv.Length!=mesh.Vertices.Count||uv.Any(v=>v.Length!=2))throw new InvalidDataException("Selected native UV set is absent on this mesh; neutral fallback");
        return uv.Select(v=>Sample(project,materialId,group,frame,new(v[0],v[1]))).ToArray();
    }
    public static Vector2 Transform(string mode,float[] srt,Vector2 uv)
    {
        if(srt.Length!=5||srt.Any(v=>!float.IsFinite(v))||!float.IsFinite(uv.X)||!float.IsFinite(uv.Y))throw new InvalidDataException("Nonfinite/invalid UV transform");
        float sx=srt[0],sy=srt[1],tx=srt[3],ty=srt[4];
        float r=srt[2]*(MathF.PI/180),c=MathF.Cos(r),s=MathF.Sin(r);
        // Algebraic affine forms of the pinned native branches, after the native-to-canonical V flip.
        if(mode=="MatrixXSI")return new(sx*(c*(uv.X-tx)+s*(uv.Y-ty)),sy*(-s*(uv.X-tx)+c*(uv.Y-ty)));
        if(mode=="Matrix3dsMax")return new(.5f+sx*(c*(uv.X-tx-.5f)-s*(uv.Y-ty-.5f)),.5f+sy*(s*(uv.X-tx-.5f)+c*(uv.Y-ty-.5f)));
        if(mode=="MatrixMaya"&&srt[2]==0)return new(sx*(uv.X-tx),sy*(uv.Y-ty));
        return uv; // Explicitly unsupported Maya rotation/unknown mode: keep the source, no invented matrix.
    }
}
