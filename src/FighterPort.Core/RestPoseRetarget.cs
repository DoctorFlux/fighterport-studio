using System.Numerics;

namespace FighterPort.Core;

/// <summary>World-space rest-relative orientation transfer onto existing target binds.
/// Row vectors: targetRestRotation * inverse(sourceRestRotation) * sourceAnimatedRotation.
/// Target local offsets remain unchanged; parents are evaluated in target order.</summary>
public static class RestPoseRetarget
{
    public static CharacterProject Transfer(CharacterProject source,int clipIndex,CharacterProject target,MappingProfile map)
    {
        ProjectStore.Validate(source);ProjectStore.Validate(target);var clip=source.Clips[clipIndex];
        ValidateMapping(source,clip,target,map);
        var result=ProjectStore.Clone(target);
        result.Clips.Add(new Clip{Name=clip.Name+" • rest delta",FrameCount=clip.FrameCount,FramesPerSecond=clip.FramesPerSecond,Loop=clip.Loop,SkeletonFingerprint=Animation.Fingerprint(target),Transfer=new(){SourceBones=ProjectStore.Clone(source.Bones),SourceClip=ProjectStore.Clone(clip),Mapping=ProjectStore.Clone(map)}});
        result.MappingProfiles.Add(ProjectStore.Clone(map));result.Branch="target/rest-delta";
        foreach(var r in source.Resources)if(!result.Resources.Any(t=>t.Sha256==r.Sha256))result.Resources.Add(ProjectStore.Clone(r));
        result.Limitations.Add("WorldRestDelta-v1: exact source curves retained, arbitrary-frame orientation evaluation; target local offsets/binds preserved. Unit scale, common world axes, root-only translation. No pelvis scaling, twist distribution, IK, contact correction or native baking yet.");
        ProjectStore.Validate(result);return result;
    }
    internal static void ValidateMapping(CharacterProject source,Clip clip,CharacterProject target,MappingProfile map)
    {
        if(map.Version!=1||!map.Approved||map.SourceFingerprint!=Animation.Fingerprint(source)||map.TargetFingerprint!=Animation.Fingerprint(target))throw new InvalidDataException("Approve an exact matching skeleton profile first");
        if(clip.Transfer!=null)throw new InvalidDataException("Generate targets from the original source, not another retarget layer");
        if(clip.TransformTracks.Count>0)throw new InvalidDataException("Quaternion track retarget policy requires separate scale/translation validation; not enabled yet");
        if(source.Bones.Concat(target.Bones).Any(b=>b.Scale.Any(s=>Math.Abs(s-1)>.0001f))||clip.Curves.Any(c=>c.Channel<3))throw new InvalidDataException("Rest-delta transfer requires unit rest scales and no scale animation; shear/scale inheritance unsupported");
        if(map.RootTranslation is not ("Reject" or "CopyDelta"))throw new InvalidDataException("Unknown translation policy");
        if(map.BoneIds.Count==0||map.BoneIds.Values.Distinct().Count()!=map.BoneIds.Count||map.BoneIds.Any(x=>!source.Bones.Any(b=>b.Id==x.Key)||!target.Bones.Any(b=>b.Id==x.Value)))throw new InvalidDataException("Empty, duplicate or unknown mapping IDs");
        foreach(var c in clip.Curves)
        {
            var b=source.Bones.Single(b=>b.Name==c.Bone);
            if(!map.BoneIds.ContainsKey(b.Id))throw new InvalidDataException("Unmapped animated bone: "+b.Name);
            var t=target.Bones.Single(t=>t.Id==map.BoneIds[b.Id]);
            if(c.Channel>=6&&(b.Parent>=0||t.Parent>=0||map.RootTranslation!="CopyDelta"))throw new InvalidDataException("Translation transfer currently requires mapped root-only CopyDelta; pelvis/proportion policies pending");
        }
    }
    private static Matrix4x4 Rotation(Matrix4x4 m){m.Translation=Vector3.Zero;return m;}
    public static Matrix4x4[] World(CharacterProject target,Clip clip,float frame)
    {
        var layer=clip.Transfer!;var source=new CharacterProject{Bones=layer.SourceBones};
        var sr=Animation.World(source,null,0);var sa=Animation.World(source,layer.SourceClip,frame);var tr=Animation.World(target,null,0);
        var sourceIndices=source.Bones.Select((b,i)=>(b,i)).ToDictionary(x=>x.b.Id,x=>x.i);
        var byTarget=layer.Mapping.BoneIds.ToDictionary(x=>x.Value,x=>sourceIndices[x.Key]);var result=new Matrix4x4[target.Bones.Count];
        for(int i=0;i<result.Length;i++)
        {
            var bone=target.Bones[i];var parent=bone.Parent<0?Matrix4x4.Identity:result[bone.Parent];
            var local=Animation.RestLocal(bone);
            if(byTarget.TryGetValue(bone.Id,out int s))
            {
                Matrix4x4.Invert(Rotation(sr[s]),out var invSourceRest);
                var wanted=Rotation(tr[i])*invSourceRest*Rotation(sa[s]);
                Matrix4x4.Invert(Rotation(parent),out var inverseParent);
                local=wanted*inverseParent;local.Translation=Animation.V(bone.Translation);
                if(bone.Parent<0&&source.Bones[s].Parent<0&&layer.Mapping.RootTranslation=="CopyDelta")local.Translation+=sa[s].Translation-sr[s].Translation;
            }
            result[i]=local*parent;
        }
        return result;
    }
}
