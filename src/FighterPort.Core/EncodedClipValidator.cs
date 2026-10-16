namespace FighterPort.Core;

/// <summary>Scalar encoded CHR0 check, not a world-space or independent native-tool proof.</summary>
public static class EncodedClipValidator
{
    public const int MaxSamples=1_000_000;
    public static float Tolerance(int channel)=>channel<3?.001f:channel<6?.05f:.01f;
    public static void CheckBudget(Clip clip)
    {
        long samples=(long)clip.Curves.Select(c=>c.Bone).Distinct().Count()*9*clip.FrameCount*4;
        if(samples>MaxSamples)throw new InvalidDataException("Export validation exceeds 1,000,000 scalar samples; split the clip explicitly");
    }
    public static void Validate(Clip original,Clip decoded,IReadOnlyDictionary<string,float[]> samples)
    {
        CheckBudget(original);
        if(original.FrameCount!=decoded.FrameCount||original.Loop!=decoded.Loop||decoded.NativeVersion!=4)
            throw new InvalidDataException("Encoded clip timing, loop or version mismatch");
        int count=checked(original.FrameCount*4);
        var expected=original.Curves.ToDictionary(c=>c.Bone+":"+c.Channel);
        var bones=original.Curves.Select(c=>c.Bone).Distinct().ToArray();
        if(decoded.Curves.Any(c=>!bones.Contains(c.Bone)))throw new InvalidDataException("Encoded clip introduced an unknown bone");
        foreach(string bone in bones)for(int channel=0;channel<9;channel++)
        {
            string id=bone+":"+channel;
            if(!samples.TryGetValue(id,out var values)||values.Length!=count)throw new InvalidDataException("Missing or truncated encoded samples: "+id);
            expected.TryGetValue(id,out var curve);
            for(int i=0;i<count;i++)
            {
                float reference=curve==null?(channel<3?1:0):Animation.Evaluate(curve,i*.25f);
                if(!float.IsFinite(values[i])||Math.Abs(reference-values[i])>Tolerance(channel))
                    throw new InvalidDataException($"Encoded error exceeds {Tolerance(channel)} on {id} at frame {i*.25f}");
            }
        }
        foreach(var curve in original.Curves)
        {
            // Also check a possible loop-end key at exactly FrameCount, outside the playback sample grid.
            var native=decoded.Curves.SingleOrDefault(c=>c.Bone==curve.Bone&&c.Channel==curve.Channel);
            foreach(var key in curve.Keys)
            {
                float value=native==null?(curve.Channel<3?1:0):Animation.Evaluate(native,key.Frame);
                if(!float.IsFinite(value)||Math.Abs(key.Value-value)>Tolerance(curve.Channel))throw new InvalidDataException("Encoded key value mismatch");
            }
        }
    }
}
