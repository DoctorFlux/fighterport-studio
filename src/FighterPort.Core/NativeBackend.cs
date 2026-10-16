using System.Diagnostics;
using System.Text.Json;

namespace FighterPort.Core;

public sealed class NativeBackend(string workerPath,string jobsDirectory,IWorkerTransport? transport=null)
{
    public string WorkerPath { get; }=Path.GetFullPath(workerPath);
    private readonly IWorkerTransport worker=transport??new WorkerProcess(Path.GetFullPath(workerPath),jobsDirectory);
    private Task<byte[]> Run(string operation,string input,CancellationToken cancellation=default)=>worker.Run(operation,input,cancellation);
    public async Task<CharacterProject> ImportModel(string path,CancellationToken cancellation=default)
    {
        var p=JsonSerializer.Deserialize<CharacterProject>(await Run("import-model",path,cancellation),ProjectStore.Json)!;
        ProjectStore.Retain(p,path);ProjectStore.Validate(p);return p;
    }
    public async Task<Clip> ImportClip(CharacterProject p,string path,float fps=30,CancellationToken cancellation=default)
    {
        var clip=JsonSerializer.Deserialize<Clip>(await Run("import-clip",path,cancellation),ProjectStore.Json)!;
        clip.FramesPerSecond=fps;clip.SkeletonFingerprint=Animation.Fingerprint(p);
        var candidate=ProjectStore.Clone(p);candidate.Clips.Add(clip);ProjectStore.Validate(candidate);
        ProjectStore.Retain(p,path);return clip;
    }
    public async Task ExportClip(CharacterProject p,Clip clip,string path,CancellationToken cancellation=default)
    {
        ProjectStore.Validate(p);
        if(clip.Transfer!=null||clip.TransformTracks.Count>0)throw new InvalidDataException("Retarget/quaternion clip needs an explicit measured native bake; direct CHR0 export is not implemented");
        if(clip.SkeletonFingerprint!=Animation.Fingerprint(p))throw new InvalidDataException("Skeleton mismatch");
        if(p.Resources.Any(r=>r.Name.Equals(Path.GetFileName(path),StringComparison.OrdinalIgnoreCase)))throw new IOException("Choose a new output name; source resources are read-only");
        if(File.Exists(path))throw new IOException("Choose a new export path");
        var candidate=ProjectStore.Clone(p);candidate.Clips=[ProjectStore.Clone(clip)];ProjectStore.Validate(candidate);
        clip=candidate.Clips[0]; // Freeze the validated export snapshot across asynchronous worker calls.
        EncodedClipValidator.CheckBudget(clip);
        string temp=Path.Combine(Path.GetFullPath(jobsDirectory),Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(temp)!);
        string encoded=temp+".chr0";
        try
        {
            await File.WriteAllTextAsync(temp,JsonSerializer.Serialize(clip,ProjectStore.Json),cancellation);
            byte[] bytes=await Run("export-clip",temp,cancellation);await File.WriteAllBytesAsync(encoded,bytes,cancellation);
            var decoded=JsonSerializer.Deserialize<Clip>(await Run("import-clip",encoded,cancellation),ProjectStore.Json)??throw new InvalidDataException("Empty encoded clip");
            var samples=JsonSerializer.Deserialize<Dictionary<string,float[]>>(await Run("sample-clip",encoded,cancellation))??throw new InvalidDataException("Empty encoded samples");
            EncodedClipValidator.Validate(clip,decoded,samples);cancellation.ThrowIfCancellationRequested();
            ProjectStore.AtomicWrite(path,bytes,overwrite:false);
        }
        finally { File.Delete(temp);File.Delete(encoded); }
    }
    public async Task GenerateModelFixture(CharacterProject original,string path)
    {
        if(File.Exists(path))throw new IOException("Fixture output exists");
        string directory=Path.Combine(Path.GetFullPath(jobsDirectory),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        string temp=Path.Combine(directory,"original-fixture.dae");
        try { ColladaFixture.Write(original,temp);ProjectStore.AtomicWrite(path,await Run("fixture-model",temp)); }finally { Directory.Delete(directory,true); }
    }
    public async Task<Dictionary<string,float[]>> Samples(string path) => JsonSerializer.Deserialize<Dictionary<string,float[]>>(await Run("sample-clip",path))!;
    public async Task<List<BundleResource>> BrowseBundle(string path,CancellationToken cancellation=default)=>JsonSerializer.Deserialize<List<BundleResource>>(await Run("browse-bundle",path,cancellation),ProjectStore.Json)??throw new InvalidDataException("Empty bundle response");
    private async Task<byte[]> Bundle(string operation,string path,string id,CancellationToken cancellation)
    {
        string temp=Path.Combine(Path.GetFullPath(jobsDirectory),Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(temp)!);
        try{await File.WriteAllTextAsync(temp,JsonSerializer.Serialize(new BundleRequest{Path=Path.GetFullPath(path),Id=id},ProjectStore.Json),cancellation);return await Run(operation,temp,cancellation);}finally{File.Delete(temp);}
    }
    public async Task<CharacterProject> ImportBundleModel(string path,string id,CancellationToken cancellation=default)
    {
        var p=JsonSerializer.Deserialize<CharacterProject>(await Bundle("model-from-bundle",path,id,cancellation),ProjectStore.Json)!;ProjectStore.Retain(p,path);ProjectStore.Validate(p);return p;
    }
    public async Task<Clip> ImportBundleClip(CharacterProject p,string path,string id,CancellationToken cancellation=default)
    {
        var clip=JsonSerializer.Deserialize<Clip>(await Bundle("clip-from-bundle",path,id,cancellation),ProjectStore.Json)!;clip.FramesPerSecond=60;clip.SkeletonFingerprint=Animation.Fingerprint(p);
        var candidate=ProjectStore.Clone(p);candidate.Clips.Add(clip);ProjectStore.Validate(candidate);ProjectStore.Retain(p,path);return clip;
    }
}
