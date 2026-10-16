using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FighterPort.Core;

public sealed class SourceDocument
{
    public string Id { get; set; }=Guid.NewGuid().ToString("N");
    public string Path { get; set; }="";
    public string Sha256 { get; set; }="";
    public long Length { get; set; }
    public string Adapter { get; set; }="";
    public NativeTreeEntry Root { get; set; }=new();
}
public sealed class WorkspaceData
{
    public int Version { get; set; }=1;
    public List<SourceDocument> Documents { get; set; }=[];
    public CharacterProject Project { get; set; }=new();
    public AssetHandle? ActiveModel { get; set; }
    public int ClipIndex { get; set; }=-1;
    public AssetHandle? ActiveClip { get; set; }
    public VisibilityGroup? Visibility { get; set; }
    public UvGroup? Uv { get; set; }
    public List<string> Problems { get; set; }=[];
}

/// <summary>Bounded, verified external source documents; no texture pixels or temporary paths in saves.</summary>
public sealed class ResourceWorkspace(string workerPath,string jobs,string pythonScript)
{
    private readonly WorkerProcess worker=new(workerPath,jobs);
    private readonly UltimateBackend ultimate=new("python",pythonScript,jobs);
    public WorkspaceData Data { get; private set; }=new();
    private async Task<byte[]> Request(string operation,SourceDocument doc,string id,CancellationToken ct,int mip=0,string palette="")
    {
        Verify(doc);Directory.CreateDirectory(jobs);string request=System.IO.Path.Combine(jobs,Guid.NewGuid()+".json");
        try{await File.WriteAllTextAsync(request,JsonSerializer.Serialize(new BundleRequest{Path=doc.Path,Id=id,Mip=mip,PaletteId=palette},ProjectStore.Json),ct);return await worker.Run(operation,request,ct);}finally{File.Delete(request);}
    }
    public SourceDocument Document(AssetHandle handle)=>Data.Documents.Single(d=>d.Id==handle.DocumentId);
    public static void Verify(SourceDocument doc)
    {
        var info=new FileInfo(doc.Path);if(!info.Exists||info.Length!=doc.Length||info.Length>32L*1024*1024)throw new InvalidDataException("Source missing/size changed: "+System.IO.Path.GetFileName(doc.Path));
        using var stream=File.OpenRead(doc.Path);if(Convert.ToHexString(SHA256.HashData(stream))!=doc.Sha256)throw new InvalidDataException("Source hash changed; reopen/review source: "+info.Name);
    }
    public async Task<SourceDocument> Open(string path,CancellationToken ct=default)
    {
        path=System.IO.Path.GetFullPath(path);var existing=Data.Documents.SingleOrDefault(d=>d.Path.Equals(path,StringComparison.OrdinalIgnoreCase));if(existing!=null){Verify(existing);return existing;}
        if(Data.Documents.Count>=256)throw new InvalidDataException("Workspace document quota");
        var info=new FileInfo(path);if(info.Length>32L*1024*1024)throw new InvalidDataException("Source document exceeds 32 MiB quota");
        byte[] header=new byte[4];using(var stream=File.OpenRead(path)){if(stream.Read(header)!=4)throw new InvalidDataException("Truncated source header");}
        string magic=Encoding.ASCII.GetString(header);string adapter=magic is "ARC\0" or "bres" or "MDL0" or "CHR0" or "VIS0" or "TEX0" or "PLT0" or "SRT0" or "PAT0" or "CLR0" or "SHP0" or "SCN0"?"Brawl":magic=="HBSS"?"Ultimate":"Unsupported";
        if(adapter=="Unsupported")throw new InvalidDataException("No verified resource reader for this header; Melee/Rivals/Dragon Ball routes remain planned");
        string hash;using(var stream=File.OpenRead(path))hash=Convert.ToHexString(SHA256.HashData(stream));
        var doc=new SourceDocument{Path=path,Length=info.Length,Sha256=hash,Adapter=adapter};
        doc.Root=adapter=="Brawl"?JsonSerializer.Deserialize<NativeTreeEntry>(await Request("resource-info",doc,"0",ct),ProjectStore.Json)!:new(){Id="0",Name=info.Name,Kind=info.Extension.TrimStart('.').ToUpperInvariant(),Size=info.Length,Status="SSBH native source; skeletal model/clip subset; other operations pending"};
        Data.Documents.Add(doc);return doc;
    }
    public async Task<List<NativeTreeEntry>> Children(AssetHandle handle,CancellationToken ct=default)
    {
        var doc=Document(handle);if(doc.Adapter!="Brawl")return [];
        return JsonSerializer.Deserialize<List<NativeTreeEntry>>(await Request("resource-children",doc,handle.NodeId,ct),ProjectStore.Json)!;
    }
    public async Task ActivateModel(AssetHandle handle,CancellationToken ct=default)
    {
        var doc=Document(handle);Verify(doc);
        var p=doc.Adapter=="Brawl"?JsonSerializer.Deserialize<CharacterProject>(await Request("resource-model",doc,handle.NodeId,ct),ProjectStore.Json)!:await ultimate.ImportModel(doc.Path,ct);
        foreach(var binding in p.Materials.SelectMany(m=>m.Textures))binding.Texture.DocumentId=doc.Id;
        ProjectStore.Validate(p);Data.Project=p;Data.ActiveModel=ProjectStore.Clone(handle);Data.ClipIndex=-1;Data.ActiveClip=null;Data.Visibility=null;Data.Uv=null;
        Data.Problems=p.Limitations.ToList();Data.Problems.AddRange(p.Materials.SelectMany(m=>m.Textures).Where(t=>t.Texture.NodeId=="").Select(t=>t.Name+": "+t.Status));
    }
    public async Task ActivateClip(AssetHandle handle,CancellationToken ct=default)
    {
        if(Data.Project.Bones.Count==0)throw new InvalidDataException("Activate a model first");var doc=Document(handle);Verify(doc);
        var clip=doc.Adapter=="Brawl"?JsonSerializer.Deserialize<Clip>(await Request("resource-clip",doc,handle.NodeId,ct),ProjectStore.Json)!:await ultimate.ImportClip(Data.Project,doc.Path,ct);
        clip.FramesPerSecond=60;clip.SkeletonFingerprint=Animation.Fingerprint(Data.Project);
        var candidate=ProjectStore.Clone(Data.Project);candidate.Clips.Add(clip);ProjectStore.Validate(candidate);
        Data.Project=candidate;Data.ClipIndex=candidate.Clips.Count-1;Data.ActiveClip=ProjectStore.Clone(handle);Data.Visibility=null;Data.Uv=null;
        Data.Problems.AddRange(clip.Losses); // Separate sources are never paired by clip display name alone.
        if(doc.Adapter=="Brawl")Data.Problems.Add("Brawl preview timing profile: 60 fps. CHR0 encodes frame indices, not fps; original frame count and keys retained.");
    }
    public async Task ActivateVisibility(AssetHandle handle,CancellationToken ct=default)
    {
        if(Data.Project.Bones.Count==0)throw new InvalidDataException("Activate a model first");
        var group=JsonSerializer.Deserialize<VisibilityGroup>(await Request("resource-vis",Document(handle),handle.NodeId,ct),ProjectStore.Json)!;
        group.Source=ProjectStore.Clone(handle);group.SkeletonFingerprint=Animation.Fingerprint(Data.Project);
        foreach(var track in group.Tracks)track.BoneId=Data.Project.Bones.SingleOrDefault(b=>b.Name==track.Target)?.Id??"";
        if(!group.Tracks.Any(t=>t.BoneId!=""&&Data.Project.Meshes.SelectMany(m=>m.Draws).Any(d=>d.VisibilityBoneId==t.BoneId)))throw new InvalidDataException("VIS0 has no compatible draw visibility bones; choose another source or mapping");
        SceneEvaluator.Validate(Data.Project,group);Data.Visibility=group;
        foreach(var t in group.Tracks.Where(t=>t.BoneId==""))Data.Problems.Add("VIS0 target unresolved and preserved: "+t.Target);
    }
    public Task<byte[]> Texture(AssetHandle handle,int mip=0,string palette="",CancellationToken ct=default)=>Request("resource-texture",Document(handle),handle.NodeId,ct,mip,palette);
    public async Task ActivateUv(AssetHandle handle,CancellationToken ct=default)
    {
        var group=JsonSerializer.Deserialize<UvGroup>(await Request("resource-uv",Document(handle),handle.NodeId,ct),ProjectStore.Json)!;group.Source=ProjectStore.Clone(handle);
        foreach(var track in group.Tracks)
        {
            var materials=Data.Project.Materials.Where(m=>m.Name==track.MaterialName).ToArray();track.MaterialId=materials.Length==1?materials[0].Id:"";
            track.Status=UvEvaluator.Support(Data.Project,group,track);
        }
        UvEvaluator.Validate(Data.Project,group);
        if(!group.Tracks.Any(t=>t.Status.StartsWith("Evaluated:")))throw new InvalidDataException("SRT0 has no evaluable direct Maya sampler 0 scale/translation tracks; source retained. Other modes/channels require further implementation.");
        Data.Uv=group;Data.Problems.AddRange(group.Tracks.Where(t=>!t.Status.StartsWith("Evaluated:")).Select(t=>t.MaterialName+" / "+t.Slot+": "+t.Status));
    }
    public Task<byte[]> ExtractSlice(AssetHandle handle,CancellationToken ct=default)=>Request("resource-extract",Document(handle),handle.NodeId,ct);
    public void OverrideTexture(string materialId,int slot,AssetHandle texture,string palette="")
    {
        _=Document(texture);var binding=Data.Project.Materials.Single(m=>m.Id==materialId).Textures.Single(t=>t.Slot==slot);
        binding.Texture=ProjectStore.Clone(texture);binding.PaletteId=palette;binding.Status="Explicit user override (persisted)";
    }
    public void Save(string path)
    {
        CheckOutput(path);foreach(var doc in Data.Documents)Verify(doc);Validate(Data);ProjectStore.AtomicWrite(path,JsonSerializer.SerializeToUtf8Bytes(Data,ProjectStore.Json));
    }
    public void CheckOutput(string path)
    {
        string full=System.IO.Path.GetFullPath(path);
        if(Data.Documents.Any(d=>d.Path.Equals(full,StringComparison.OrdinalIgnoreCase)))throw new InvalidDataException("Original source is read-only; choose another output path");
    }
    public void Load(string path)
    {
        if(new FileInfo(path).Length>ProjectStore.MaxBytes)throw new InvalidDataException("Workspace size quota");
        var candidate=JsonSerializer.Deserialize<WorkspaceData>(File.ReadAllBytes(path),ProjectStore.Json)??throw new InvalidDataException("Empty workspace");
        Validate(candidate);foreach(var doc in candidate.Documents)Verify(doc);Data=candidate;
    }
    public static void Validate(WorkspaceData data)
    {
        if(data.Version!=1||data.Documents.Count>256||data.Documents.Select(d=>d.Id).Distinct().Count()!=data.Documents.Count)throw new InvalidDataException("Workspace schema/document identity quota");
        ProjectStore.Validate(data.Project);if(data.Visibility!=null)SceneEvaluator.Validate(data.Project,data.Visibility);if(data.Uv!=null)UvEvaluator.Validate(data.Project,data.Uv);
        if(data.ClipIndex< -1||data.ClipIndex>=data.Project.Clips.Count)throw new InvalidDataException("Workspace clip binding");
        var handles=new[]{data.ActiveModel,data.ActiveClip,data.Visibility?.Source,data.Uv?.Source}.Where(h=>h!=null).Cast<AssetHandle>().Concat(data.Project.Materials.SelectMany(m=>m.Textures).Where(t=>t.Texture.NodeId!="").Select(t=>t.Texture));
        if(handles.Any(h=>!data.Documents.Any(d=>d.Id==h.DocumentId)))throw new InvalidDataException("Missing source document binding");
    }
}
