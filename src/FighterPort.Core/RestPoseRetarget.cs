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
        result.Clips.Add(new Clip{Name=clip.Name+" • rest delta",FrameCount=clip.FrameCount,FramesPerSecond=clip.FramesPerSecond,Loop=clip.Loop,Losses=ProjectStore.Clone(clip.Losses),SkeletonFingerprint=Animation.Fingerprint(target),Transfer=new(){SourceBones=ProjectStore.Clone(source.Bones),SourceClip=ProjectStore.Clone(clip),Mapping=ProjectStore.Clone(map)}});
        result.MappingProfiles.Add(ProjectStore.Clone(map));result.Branch="target/rest-delta";
        foreach(var r in source.Resources)if(!result.Resources.Any(t=>t.Sha256==r.Sha256))result.Resources.Add(ProjectStore.Clone(r));
        result.Limitations.Add("WorldRestDelta-v2: retained scalar/quaternion source, unit scales; approved root CopyDelta or nonroot RestParentDelta; unmapped target helpers retain rest locals. No proportion scaling, twist distribution, IK or contact correction. Ancillary channels require separate mapping.");
        ProjectStore.Validate(result);return result;
    }
    internal static void ValidateMapping(CharacterProject source,Clip clip,CharacterProject target,MappingProfile map)
    {
        if(map.Version!=1||!map.Approved||map.SourceFingerprint!=Animation.Fingerprint(source)||map.TargetFingerprint!=Animation.Fingerprint(target))throw new InvalidDataException("Approve an exact matching skeleton profile first");
        if(clip.Transfer!=null)throw new InvalidDataException("Generate targets from the original source, not another retarget layer");
        ValidatePolicies(map);
        bool Constant(Curve c,float value)=>c.Keys.All(k=>Math.Abs(k.Value-value)<.00001f&&(c.Interpolation!="Hermite"||Math.Abs(k.Tangent)<.00001f));
        if(source.Bones.Concat(target.Bones).Any(b=>b.Scale.Any(s=>Math.Abs(s-1)>.0001f))||clip.Curves.Any(c=>c.Channel<3&&!Constant(c,1))||clip.TransformTracks.Any(t=>t.Samples.Any(v=>v.Scale.Any(s=>Math.Abs(s-1)>.00001f))))throw new InvalidDataException("UnitOnly policy rejects nonunit rest/animated scales; compensation/shear unsupported");
        foreach(var bone in source.Bones.Concat(target.Bones))
        {
            var rest=Animation.RestLocal(bone);rest.Translation=Vector3.Zero;
            if(Animation.A(rest*Matrix4x4.Transpose(rest)).Zip(Animation.A(Matrix4x4.Identity)).Any(x=>Math.Abs(x.First-x.Second)>.0002f)||rest.GetDeterminant()<0)throw new InvalidDataException("Rest orientation must be proper orthonormal rotation");
        }
        if(map.BoneIds.Count==0||map.BoneIds.Values.Distinct().Count()!=map.BoneIds.Count||map.BoneIds.Any(x=>!source.Bones.Any(b=>b.Id==x.Key)||!target.Bones.Any(b=>b.Id==x.Value)))throw new InvalidDataException("Empty, duplicate or unknown mapping IDs");
        foreach(var name in clip.Curves.Select(c=>c.Bone).Concat(clip.TransformTracks.Select(t=>t.Bone)).Distinct())
        {
            var b=source.Bones.Single(b=>b.Name==name);
            if(!map.BoneIds.ContainsKey(b.Id))throw new InvalidDataException("Unmapped animated bone: "+b.Name);
            var t=target.Bones.Single(t=>t.Id==map.BoneIds[b.Id]);
            var track=clip.TransformTracks.SingleOrDefault(x=>x.Bone==name);
            bool translation=clip.Curves.Any(c=>c.Bone==name&&c.Channel>=6&&!Constant(c,b.Translation[c.Channel-6]))||track?.Samples.Any(v=>Vector3.Distance(Animation.V(v.Translation),Animation.RestLocal(b).Translation)>.00001f)==true;
            if(translation)
            {
                if((b.Parent<0)!=(t.Parent<0))throw new InvalidDataException("Root/nonroot translation mapping requires a separate policy");
                if(b.Parent<0?map.RootTranslation!="CopyDelta":map.NonRootTranslation!="RestParentDelta")throw new InvalidDataException("Translation differs from rest; approve root CopyDelta or nonroot RestParentDelta explicitly");
            }
        }
    }
    public static void ValidatePolicies(MappingProfile map)
    {
        if(map.RootTranslation is not ("Reject" or "CopyDelta")||map.NonRootTranslation is not ("Reject" or "RestParentDelta")||map.ScalePolicy!="UnitOnly"||map.HelperPolicy!="TargetRest")throw new InvalidDataException("Unsupported translation/scale/helper policy");
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
                if(bone.Parent>=0&&source.Bones[s].Parent>=0&&layer.Mapping.NonRootTranslation=="RestParentDelta")
                {
                    int sp=source.Bones[s].Parent;Matrix4x4.Invert(sa[sp],out var invAnimatedSourceParent);
                    var sourceLocal=sa[s]*invAnimatedSourceParent;
                    var delta=sourceLocal.Translation-Animation.RestLocal(source.Bones[s]).Translation;
                    Matrix4x4.Invert(Rotation(tr[bone.Parent]),out var invTargetRestParent);
                    local.Translation+=Vector3.TransformNormal(delta,Rotation(sr[sp])*invTargetRestParent);
                }
            }
            result[i]=local*parent;
        }
        return result;
    }
}
