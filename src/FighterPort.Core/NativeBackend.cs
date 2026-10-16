using System.Diagnostics;
using System.Text.Json;

namespace FighterPort.Core;

public sealed class NativeBackend(string workerPath,string jobsDirectory)
{
    public string WorkerPath { get; }=Path.GetFullPath(workerPath);
    private async Task<byte[]> Run(string operation,string input,CancellationToken cancellation=default)
    {
        if(!File.Exists(WorkerPath))throw new FileNotFoundException("Native worker unavailable. Run scripts/build.ps1 -Native.",WorkerPath);
        if(new FileInfo(input).Length>ProjectStore.MaxBytes)throw new InvalidDataException("Worker quota");
        string directory=Path.Combine(Path.GetFullPath(jobsDirectory),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        string output=Path.Combine(directory,"result");
        try
        {
            var start=new ProcessStartInfo(WorkerPath){UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true,RedirectStandardOutput=true,WorkingDirectory=directory};
            foreach(string arg in new[]{"v1",operation,Path.GetFullPath(input),output})start.ArgumentList.Add(arg);
            using var process=Process.Start(start)??throw new IOException("Worker failed to start");
            var stdout=process.StandardOutput.ReadToEndAsync(cancellation);var stderr=process.StandardError.ReadToEndAsync(cancellation);
            using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellation);deadline.CancelAfter(TimeSpan.FromSeconds(45));
            try { await process.WaitForExitAsync(deadline.Token); }
            catch(OperationCanceledException) { if(!process.HasExited)process.Kill(true);await process.WaitForExitAsync(CancellationToken.None);throw new OperationCanceledException("Native operation canceled or exceeded 45 seconds",cancellation); }
            string error=await stderr;await stdout;
            if(process.ExitCode!=0)throw new InvalidDataException($"Native worker failed ({process.ExitCode}): {error}");
            if(!File.Exists(output)||new FileInfo(output).Length>ProjectStore.MaxBytes)throw new InvalidDataException("Missing/oversize worker response");
            return await File.ReadAllBytesAsync(output,cancellation);
        }
        finally { Directory.Delete(directory,true); }
    }
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
        if(clip.SkeletonFingerprint!=Animation.Fingerprint(p))throw new InvalidDataException("Skeleton mismatch");
        if(p.Resources.Any(r=>r.Name.Equals(Path.GetFileName(path),StringComparison.OrdinalIgnoreCase)))throw new IOException("Choose a new output name; source resources are read-only");
        if(File.Exists(path))throw new IOException("Choose a new export path");
        string temp=Path.Combine(Path.GetFullPath(jobsDirectory),Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(temp)!);
        try { await File.WriteAllTextAsync(temp,JsonSerializer.Serialize(clip,ProjectStore.Json),cancellation);ProjectStore.AtomicWrite(path,await Run("export-clip",temp,cancellation)); }
        finally { File.Delete(temp); }
    }
    public async Task GenerateModelFixture(CharacterProject original,string path)
    {
        if(File.Exists(path))throw new IOException("Fixture output exists");
        string directory=Path.Combine(Path.GetFullPath(jobsDirectory),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        string temp=Path.Combine(directory,"original-fixture.dae");
        try { ColladaFixture.Write(original,temp);ProjectStore.AtomicWrite(path,await Run("fixture-model",temp)); }finally { Directory.Delete(directory,true); }
    }
    public async Task<Dictionary<string,float[]>> Samples(string path) => JsonSerializer.Deserialize<Dictionary<string,float[]>>(await Run("sample-clip",path))!;
}
