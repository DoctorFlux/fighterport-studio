using System.Text.Json;

namespace FighterPort.Core;

/// <summary>Owns immutable source/target snapshots and revokes approval on every mapping change.</summary>
public sealed class MappingSession
{
    private readonly CharacterProject source,target;
    private MappingProfile profile;
    public CharacterProject Source => ProjectStore.Clone(source);
    public CharacterProject Target => ProjectStore.Clone(target);
    public MappingProfile Profile => ProjectStore.Clone(profile);
    public MappingSession(CharacterProject source,CharacterProject target)
    {
        ProjectStore.Validate(source);ProjectStore.Validate(target);
        this.source=ProjectStore.Clone(source);this.target=ProjectStore.Clone(target);
        profile=FkRetarget.Template(this.source,this.target);
    }
    public void Assign(string sourceId,string? targetId)
    {
        if(!source.Bones.Any(b=>b.Id==sourceId))throw new InvalidDataException("Unknown source bone ID");
        if(string.IsNullOrEmpty(targetId)){profile.BoneIds.Remove(sourceId);profile.Approved=false;return;}
        if(!target.Bones.Any(b=>b.Id==targetId))throw new InvalidDataException("Unknown target bone ID");
        if(profile.BoneIds.Any(p=>p.Key!=sourceId&&p.Value==targetId))throw new InvalidDataException("Target is already assigned; unassign that row first");
        profile.BoneIds[sourceId]=targetId;profile.Approved=false;
    }
    public void SetRootPolicy(string policy)
    {
        if(policy is not ("Reject" or "CopyDelta"))throw new InvalidDataException("Unknown root translation policy");
        if(profile.RootTranslation!=policy){profile.RootTranslation=policy;profile.Approved=false;}
    }
    public void Approve(bool approved)=>profile.Approved=approved;
    public void Reset()=>profile=FkRetarget.Template(source,target);
    public void Clear(){profile.BoneIds.Clear();profile.Approved=false;}
    public void LoadProfile(MappingProfile candidate)
    {
        if(candidate.Version!=1||candidate.SourceFingerprint!=Animation.Fingerprint(source)||candidate.TargetFingerprint!=Animation.Fingerprint(target))
            throw new InvalidDataException("Profile fingerprint mismatch: select its exact source and target skeletons");
        if(candidate.RootTranslation is not ("Reject" or "CopyDelta")||candidate.BoneIds.Count>source.Bones.Count||candidate.BoneIds.Values.Distinct().Count()!=candidate.BoneIds.Count)
            throw new InvalidDataException("Invalid mapping profile policy or duplicate target");
        if(candidate.BoneIds.Any(p=>!source.Bones.Any(b=>b.Id==p.Key)||!target.Bones.Any(b=>b.Id==p.Value)))throw new InvalidDataException("Unknown bone ID in profile");
        profile=ProjectStore.Clone(candidate);
    }
    public void LoadProfile(string path)
    {
        if(new FileInfo(path).Length>1024*1024)throw new InvalidDataException("Profile exceeds 1 MiB quota");
        LoadProfile(JsonSerializer.Deserialize<MappingProfile>(File.ReadAllText(path),ProjectStore.Json)??throw new InvalidDataException("Empty profile"));
    }
    public void SaveProfile(string path)=>ProjectStore.AtomicWrite(path,JsonSerializer.SerializeToUtf8Bytes(profile,ProjectStore.Json));
    public CharacterProject Preview(int clipIndex,bool restDelta=false)=>restDelta?RestPoseRetarget.Transfer(source,clipIndex,target,profile):FkRetarget.Transfer(source,clipIndex,target,profile);
    public void SaveResult(int clipIndex,string newPath,bool restDelta=false)
    {
        var result=Preview(clipIndex,restDelta);ProjectStore.Validate(result);
        ProjectStore.AtomicWrite(newPath,JsonSerializer.SerializeToUtf8Bytes(result,ProjectStore.Json),overwrite:false);
    }
}
