namespace FighterPort.Core;

/// <summary>Explicitly approved, matching-rest-axis FK subset. No anatomical inference.</summary>
public static class FkRetarget
{
    public static MappingProfile Template(CharacterProject source,CharacterProject target)
    {
        ProjectStore.Validate(source);ProjectStore.Validate(target);
        return new(){SourceFingerprint=Animation.Fingerprint(source),TargetFingerprint=Animation.Fingerprint(target),
            BoneIds=source.Bones.Where(s=>target.Bones.Any(t=>t.Name==s.Name)).ToDictionary(s=>s.Id,s=>target.Bones.Single(t=>t.Name==s.Name).Id)};
    }

    public static CharacterProject Transfer(CharacterProject source,int clipIndex,CharacterProject target,MappingProfile profile)
    {
        ProjectStore.Validate(source);ProjectStore.Validate(target);
        if(source.Clips[clipIndex].Transfer!=null||source.Clips[clipIndex].TransformTracks.Count>0)throw new InvalidDataException("Strict scalar FK does not accept native quaternion/retarget-layer clips");
        if(source.Bones.Concat(target.Bones).Any(b=>b.RestMatrix.Length>0))throw new InvalidDataException("Native rest matrices require the separately validated rest-delta path");
        void Require(bool ok,string why){if(!ok)throw new InvalidDataException(why);}
        Require(profile.Version==1&&profile.Approved,"Review and explicitly approve the mapping profile first");
        Require(profile.SourceFingerprint==Animation.Fingerprint(source)&&profile.TargetFingerprint==Animation.Fingerprint(target),"Stale/incompatible skeleton fingerprint");
        Require(profile.RootTranslation is "Reject" or "CopyDelta","Unknown root translation policy");
        Require(profile.BoneIds.Values.Distinct().Count()==profile.BoneIds.Count,"Multiple sources cannot map to one target in this subset");
        var sourceById=source.Bones.ToDictionary(b=>b.Id);var targetById=target.Bones.ToDictionary(b=>b.Id);
        foreach(var pair in profile.BoneIds)
        {
            Require(sourceById.ContainsKey(pair.Key)&&targetById.ContainsKey(pair.Value),"Mapping contains unknown bone ID");
            var s=sourceById[pair.Key];var t=targetById[pair.Value];
            Require(s.Rotation.Zip(t.Rotation).All(x=>Math.Abs(x.First-x.Second)<.0001f),"Rest-axis correction is required; this FK subset requires identical local rest Euler axes");
            Require(s.Scale.Concat(t.Scale).All(x=>Math.Abs(x-1)<.0001f),"Scaled rest rigs require a different retarget evaluator");
            if(s.Parent<0)Require(t.Parent<0,"Root must map to root");
            else Require(t.Parent>=0&&profile.BoneIds.TryGetValue(source.Bones[s.Parent].Id,out string? parent)&&parent==target.Bones[t.Parent].Id,"Mapped hierarchy must match and include every ancestor");
        }
        var original=source.Clips[clipIndex];var clip=ProjectStore.Clone(original);clip.Name=original.Name+" • FK";clip.SkeletonFingerprint=Animation.Fingerprint(target);
        foreach(var curve in clip.Curves)
        {
            var s=source.Bones.Single(b=>b.Name==curve.Bone);
            Require(profile.BoneIds.ContainsKey(s.Id),$"Unmapped animated bone: {s.Name}");
            var t=targetById[profile.BoneIds[s.Id]];
            Require(curve.Channel>=3,"Scale transfer not implemented; no channel was silently dropped");
            if(curve.Channel>=6)Require(s.Parent<0&&t.Parent<0&&profile.RootTranslation=="CopyDelta","Translation transfer requires root-only CopyDelta policy");
            float sourceRest=curve.Channel<6?s.Rotation[curve.Channel-3]:s.Translation[curve.Channel-6];
            float targetRest=curve.Channel<6?t.Rotation[curve.Channel-3]:t.Translation[curve.Channel-6];
            curve.Bone=t.Name;foreach(var k in curve.Keys)k.Value=k.Value-sourceRest+targetRest;
        }
        var result=ProjectStore.Clone(target);result.Branch="target/fk";result.Clips.Add(clip);result.MappingProfiles.Add(ProjectStore.Clone(profile));
        foreach(var r in source.Resources)if(!result.Resources.Any(t=>t.Sha256==r.Sha256))result.Resources.Add(ProjectStore.Clone(r));
        result.Limitations.Add("Restricted FK transfer: approved exact hierarchy and equal rest axes; rotations and explicit root translation deltas only. No IK, contact correction, rebinding or anatomical inference. Source project remains unchanged.");
        ProjectStore.Validate(result);return result;
    }
}
