using System.Numerics;

namespace FighterPort.Core;

/// <summary>Direct Maya scale/translation subset. Canonical V is the inverse of native V.</summary>
public static class UvEvaluator
{
    public static string Support(CharacterProject project,UvGroup group,UvTrack track)
    {
        var sampler=project.Materials.SingleOrDefault(m=>m.Id==track.MaterialId)?.Textures.SingleOrDefault(t=>t.Slot==track.Slot);
        if(sampler==null)return "Unresolved material/sampler; retained unevaluated";
        if(track.Indirect||track.Slot!=0||sampler.UVSet!=0)return "Indirect/additional sampler/UV channel retained unevaluated";
        if(group.MatrixMode!="MatrixMaya"||sampler.MatrixMode!="MatrixMaya")return "Non-Maya UV matrix retained unevaluated";
        if(sampler.RestSrt[2]!=0||track.Curves.Where(c=>c.Channel==2).Any(c=>c.Keys.Any(k=>k.Value!=0||k.Tangent!=0)))return "Rotating UV matrix retained unevaluated";
        return "Evaluated: direct Maya sampler 0 UV0 scale/translation";
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
        var binding=project.Materials.SingleOrDefault(m=>m.Id==materialId)?.Textures.SingleOrDefault(t=>t.Slot==0);
        if(binding==null||binding.UVSet!=0||binding.MatrixMode!="MatrixMaya"||binding.RestSrt[2]!=0)return uv;
        var values=binding.RestSrt.ToArray();
        if(group is {Enabled:true})
        {
            var track=group.Tracks.SingleOrDefault(t=>t.MaterialId==materialId&&t.Slot==0&&!t.Indirect);
            if(track!=null&&Support(project,group,track).StartsWith("Evaluated:"))
            {
                float f=group.Loop?((frame%group.FrameCount)+group.FrameCount)%group.FrameCount:Math.Clamp(frame,0,group.FrameCount-1);
                foreach(var c in track.Curves)values[c.Channel]=Animation.Evaluate(c,f);
            }
        }
        // Native Maya ST: u'=su*(u-tu), v'=sv*(v+tv-1)+1. Convert native v=1-canonicalV.
        return new(values[0]*(uv.X-values[3]),values[1]*(uv.Y-values[4]));
    }
}
