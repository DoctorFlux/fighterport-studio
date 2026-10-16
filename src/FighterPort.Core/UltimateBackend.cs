using System.Text.Json;

namespace FighterPort.Core;

public sealed class UltimateBackend(string python,string script,string jobs)
{
    private readonly WorkerProcess worker=new(python,jobs,[Path.GetFullPath(script)]);
    public async Task<CharacterProject> ImportModel(string path,CancellationToken cancellation=default)
    {
        var p=JsonSerializer.Deserialize<CharacterProject>(await worker.Run("import-model",path,cancellation),ProjectStore.Json)??throw new InvalidDataException("Empty model");
        ProjectStore.Validate(p);return p;
    }
    public async Task<Clip> ImportClip(CharacterProject p,string path,CancellationToken cancellation=default)
    {
        var clip=JsonSerializer.Deserialize<Clip>(await worker.Run("import-clip",path,cancellation),ProjectStore.Json)??throw new InvalidDataException("Empty clip");
        clip.SkeletonFingerprint=Animation.Fingerprint(p);var candidate=ProjectStore.Clone(p);candidate.Clips.Add(clip);ProjectStore.Validate(candidate);
        ProjectStore.Retain(p,path);return clip;
    }
    public async Task ExportClip(CharacterProject p,Clip clip,string path,bool allowAncillaryLoss=false,CancellationToken cancellation=default)
    {
        ProjectStore.Validate(p);clip=ProjectStore.Clone(clip);
        var candidate=ProjectStore.Clone(p);candidate.Clips=[clip];ProjectStore.Validate(candidate);
        if(File.Exists(path))throw new IOException("Choose a new NUANMB output path");
        if(clip.Transfer!=null||clip.Curves.Count>0||clip.TransformTracks.Count==0||clip.Loop)throw new InvalidDataException("NUANMB write subset requires nonloop quaternion samples; bake scalar/layer clips explicitly");
        if(!allowAncillaryLoss&&clip.Losses.Any(s=>s.Contains("native nodes")))throw new InvalidDataException("Skeletal-only export would omit native visibility/material nodes; explicit loss approval required");
        if((long)clip.FrameCount*clip.TransformTracks.Count*4>1000000)throw new InvalidDataException("Native validation sample quota");
        Directory.CreateDirectory(jobs);string temp=Path.Combine(jobs,Guid.NewGuid()+".json"),encoded=temp+".nuanmb";
        try
        {
            await File.WriteAllTextAsync(temp,JsonSerializer.Serialize(clip,ProjectStore.Json),cancellation);byte[] bytes=await worker.Run("export-clip",temp,cancellation);await File.WriteAllBytesAsync(encoded,bytes,cancellation);
            var decoded=JsonSerializer.Deserialize<Clip>(await worker.Run("import-clip",encoded,cancellation),ProjectStore.Json)!;
            decoded.SkeletonFingerprint=Animation.Fingerprint(p);candidate.Clips=[decoded];ProjectStore.Validate(candidate);
            if(decoded.FrameCount!=clip.FrameCount||decoded.TransformTracks.Count!=clip.TransformTracks.Count)throw new InvalidDataException("Decoded NUANMB timing/binding mismatch");
            foreach(var track in clip.TransformTracks)
            {
                var actual=decoded.TransformTracks.Single(t=>t.Bone==track.Bone);
                for(float f=0;f<clip.FrameCount;f+=.25f)
                {
                    var a=Animation.SampleTransform(track,f);var b=Animation.SampleTransform(actual,f);
                    if(Animation.A(a).Zip(Animation.A(b)).Any(x=>Math.Abs(x.First-x.Second)>.001f))throw new InvalidDataException("Decoded NUANMB matrix error exceeds .001 per element");
                }
            }
            cancellation.ThrowIfCancellationRequested();ProjectStore.AtomicWrite(path,bytes,overwrite:false);
        }
        finally{File.Delete(temp);File.Delete(encoded);}
    }
}
