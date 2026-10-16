using System.Numerics;

namespace FighterPort.Core;

public sealed record SceneState(Matrix4x4[] Pose,bool[][] DrawVisibility);
public static class SceneEvaluator
{
    public static void Validate(CharacterProject project,VisibilityGroup group)
    {
        if(group.SkeletonFingerprint!=Animation.Fingerprint(project)||group.FrameCount<1||group.FrameCount>100000||group.Tracks.Count>512||!float.IsFinite(group.Rate)||group.Rate<=0||!float.IsFinite(group.Offset))throw new InvalidDataException("Invalid visibility timing/binding");
        if(group.Tracks.Select(t=>t.Target).Distinct().Count()!=group.Tracks.Count||group.Tracks.Any(t=>t.Values.Length!=1&&t.Values.Length!=group.FrameCount||t.BoneId!=""&&!project.Bones.Any(b=>b.Id==t.BoneId)))throw new InvalidDataException("Invalid visibility targets/samples");
        var bound=group.Tracks.Where(t=>t.BoneId!="").ToArray();
        if(bound.Select(t=>t.BoneId).Distinct().Count()!=bound.Length||group.Tracks.Sum(t=>(long)t.Values.Length)>1000000)throw new InvalidDataException("Duplicate visibility bindings/sample quota");
    }
    public static SceneState Sample(CharacterProject project,Clip? clip,VisibilityGroup? visibility,float frame,bool skeletalEnabled=true)
    {
        if(!float.IsFinite(frame))throw new InvalidDataException("Nonfinite scene frame");
        var result=project.Meshes.Select(m=>m.Draws.Count==0?new[]{true}:m.Draws.Select(d=>d.DefaultVisible).ToArray()).ToArray();
        if(visibility is {Enabled:true})
        {
            float f=frame*visibility.Rate+visibility.Offset;f=visibility.Loop?((f%visibility.FrameCount)+visibility.FrameCount)%visibility.FrameCount:Math.Clamp(f,0,visibility.FrameCount-1);
            var byBone=visibility.Tracks.Where(t=>t.BoneId!="").ToDictionary(t=>t.BoneId,t=>t.Values[t.Values.Length==1?0:(int)MathF.Floor(f)]);
            for(int m=0;m<project.Meshes.Count;m++)for(int d=0;d<project.Meshes[m].Draws.Count;d++)if(byBone.TryGetValue(project.Meshes[m].Draws[d].VisibilityBoneId,out bool visible))result[m][d]=visible;
        }
        return new(Animation.World(project,skeletalEnabled?clip:null,frame),result);
    }
}
