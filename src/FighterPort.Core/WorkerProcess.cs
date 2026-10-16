using System.Diagnostics;
using System.Text;

namespace FighterPort.Core;

public interface IWorkerTransport
{
    Task<byte[]> Run(string operation,string input,CancellationToken cancellation=default);
}

public sealed record WorkerLimits(int TimeoutMilliseconds=45000,int LogCharacters=16384,int OutputBytes=ProjectStore.MaxBytes,long MemoryBytes=512L*1024*1024);

/// <summary>Bounded process/IO supervision, not an OS security sandbox.</summary>
public sealed class WorkerProcess(string executable,string jobsDirectory,IReadOnlyList<string>? prefix=null,WorkerLimits? limits=null) : IWorkerTransport
{
    private readonly WorkerLimits bounds=limits??new();
    public async Task<byte[]> Run(string operation,string input,CancellationToken cancellation=default)
    {
        cancellation.ThrowIfCancellationRequested();
        if(new FileInfo(input).Length>ProjectStore.MaxBytes)throw new InvalidDataException("Worker input quota exceeded");
        string directory=Path.Combine(Path.GetFullPath(jobsDirectory),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        string output=Path.Combine(directory,"result");
        Process? process=null;Task<string>? stdout=null,stderr=null;
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellation);deadline.CancelAfter(bounds.TimeoutMilliseconds);
        try
        {
            var start=new ProcessStartInfo(executable){UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true,RedirectStandardOutput=true,WorkingDirectory=directory};
            foreach(string arg in (prefix??[]).Concat(new[]{"v1",operation,Path.GetFullPath(input),output}))start.ArgumentList.Add(arg);
            process=Process.Start(start)??throw new IOException("Worker failed to start");
            stdout=ReadBounded(process.StandardOutput,deadline.Token);stderr=ReadBounded(process.StandardError,deadline.Token);
            while(!process.HasExited)
            {
                deadline.Token.ThrowIfCancellationRequested();
                if(stdout.IsFaulted)await stdout;if(stderr.IsFaulted)await stderr;
                if(File.Exists(output)&&new FileInfo(output).Length>bounds.OutputBytes)throw new InvalidDataException("Worker output quota exceeded");
                try { process.Refresh();if(process.WorkingSet64>bounds.MemoryBytes)throw new InvalidDataException("Worker memory quota exceeded"); }
                catch(InvalidOperationException) when(process.HasExited) { /* Exit can race process statistics. */ }
                await Task.Delay(20,deadline.Token);
            }
            string completion=await stdout,error=await stderr;
            if(process.ExitCode!=0)throw new InvalidDataException($"Worker exited with code {process.ExitCode}: {error}");
            if(completion.Trim()!="{\"protocol\":1,\"ok\":true}")throw new InvalidDataException("Invalid worker completion protocol");
            if(!File.Exists(output)||new FileInfo(output).Length>bounds.OutputBytes)throw new InvalidDataException("Missing/oversize worker response");
            return await File.ReadAllBytesAsync(output,deadline.Token);
        }
        catch(OperationCanceledException) when(!cancellation.IsCancellationRequested)
        { throw new TimeoutException($"Worker exceeded {bounds.TimeoutMilliseconds} ms deadline"); }
        finally
        {
            if(process!=null)
            {
                if(!process.HasExited)process.Kill(true);
                await process.WaitForExitAsync(CancellationToken.None);
                deadline.Cancel();
                if(stdout!=null)try{await stdout;}catch(Exception){}
                if(stderr!=null)try{await stderr;}catch(Exception){}
                process.Dispose();
            }
            Directory.Delete(directory,true);
        }
    }
    private async Task<string> ReadBounded(StreamReader reader,CancellationToken cancellation)
    {
        var text=new StringBuilder();char[] buffer=new char[1024];int read;
        while((read=await reader.ReadAsync(buffer.AsMemory(),cancellation))!=0)
        {
            if(text.Length+read>bounds.LogCharacters)throw new InvalidDataException("Worker log quota exceeded");
            text.Append(buffer,0,read);
        }
        return text.ToString();
    }
}
