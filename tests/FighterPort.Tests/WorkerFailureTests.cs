using FighterPort.Core;
using System.Text.Json;

internal static class WorkerFailureTests
{
    // Explicit fault process only in test executable; never installed as a native backend.
    internal static async Task<int> Child(string[] args)
    {
        string mode=args[2],output=args[4];
        if(mode=="hang")await Task.Delay(10000);
        if(mode=="crash")return 37;
        if(mode=="logs")Console.Write(new string('x',50000));
        if(mode=="output")using(var file=File.Create(output))file.SetLength(2000000);
        if(mode=="missing"){}
        else if(mode!="output")File.WriteAllText(output,"ok");
        Console.WriteLine(mode=="protocol"?"wrong":"{\"protocol\":1,\"ok\":true}");return 0;
    }
    private static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    private static async Task<T> Reject<T>(Func<Task> action) where T:Exception
    {try{await action();}catch(T error){return error;}throw new Exception("Expected "+typeof(T).Name);}
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test,bool native)
    {
        string input=Path.Combine(output,"worker-input.txt"),jobs=Path.Combine(output,"jobs");File.WriteAllText(input,"original input");
        string exe=Path.Combine(AppContext.BaseDirectory,"FighterPort.Tests.exe");
        var child=new WorkerProcess(exe,jobs,["--fault-worker"],new(1000,4096,1024*1024));
        await test("M2.C2 bounded process crash/timeout/cancel/log/output/protocol/missing response",async()=>
        {
            foreach(string mode in new[]{"crash","logs","output","protocol","missing"})await Reject<InvalidDataException>(()=>child.Run(mode,input));
            await Reject<TimeoutException>(()=>child.Run("hang",input));
            using var cancel=new CancellationTokenSource(150);await Reject<OperationCanceledException>(()=>child.Run("hang",input,cancel.Token));
            Require(!Directory.EnumerateFileSystemEntries(jobs).Any(),"Leaked worker jobs");Require(File.ReadAllText(input)=="original input","Input changed");
        });
        if(!native)return;
        string worker=Path.Combine(Environment.CurrentDirectory,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe");
        var real=new WorkerProcess(worker,jobs,limits:new(5000));var p=Demo.Create();
        await test("M2.C2 deterministic malformed native sizes/offsets/counts/truncations",async()=>
        {
            int cases=0;
            foreach(var pair in new[]{("original.mdl0","import-model"),("original.chr0","import-clip")})
            {
                byte[] original=File.ReadAllBytes(Path.Combine(output,pair.Item1));
                int Read(int o)=>(original[o]<<24)|(original[o+1]<<16)|(original[o+2]<<8)|original[o+3];
                var mutations=new List<byte[]>{original[..12],original[..(original.Length/2)]};
                foreach(int offset in new[]{4,16,Read(16)+4,Read(16)+24+12})
                {var bad=original.ToArray();bad[offset]=0x7f;bad[offset+1]=bad[offset+2]=bad[offset+3]=0xff;mutations.Add(bad);}
                foreach(var bytes in mutations)
                {
                    string path=Path.Combine(output,"malformed-"+(cases++)+Path.GetExtension(pair.Item1));File.WriteAllBytes(path,bytes);
                    await Reject<InvalidDataException>(()=>real.Run(pair.Item2,path));Require(bytes.SequenceEqual(File.ReadAllBytes(path)),"Mutation input changed");
                }
                Require(original.SequenceEqual(File.ReadAllBytes(Path.Combine(output,pair.Item1))),"Original native changed");
            }
            Console.WriteLine($"  {cases} deterministic malformed native cases rejected");
        });
        await test("M2.C2 export validation/cancellation/crash and destination race publish no partial file",async()=>
        {
            foreach(string mode in new[]{"validation","cancel","crash","race"})
            {
                string destination=Path.Combine(output,"failure-"+mode+".chr0");using var cancel=new CancellationTokenSource();
                var hook=new Hook(async(op,path,ct)=>
                {
                    if(mode=="crash"&&op=="export-clip")return await child.Run("crash",path,ct);
                    var result=await real.Run(op,path,ct);
                    if(op=="sample-clip")
                    {
                        if(mode=="validation") {var samples=JsonSerializer.Deserialize<Dictionary<string,float[]>>(result)!;samples.Values.First()[0]=9999;return JsonSerializer.SerializeToUtf8Bytes(samples);}
                        if(mode=="cancel")cancel.Cancel();
                        if(mode=="race")File.WriteAllText(destination,"unrelated concurrent output");
                    }
                    return result;
                });
                var backend=new NativeBackend(worker,jobs,hook);
                if(mode=="cancel")await Reject<OperationCanceledException>(()=>backend.ExportClip(p,p.Clips[2],destination,cancel.Token));
                else if(mode=="race")await Reject<IOException>(()=>backend.ExportClip(p,p.Clips[2],destination));
                else await Reject<InvalidDataException>(()=>backend.ExportClip(p,p.Clips[2],destination));
                Require(mode=="race"?File.ReadAllText(destination)=="unrelated concurrent output":!File.Exists(destination),"Published partial/overwrote concurrent file");
                Require(!Directory.EnumerateFileSystemEntries(jobs).Any(),"Leaked export job");
            }
        });
    }
    private sealed class Hook(Func<string,string,CancellationToken,Task<byte[]>> run):IWorkerTransport
    {public Task<byte[]> Run(string operation,string input,CancellationToken cancellation=default)=>run(operation,input,cancellation);}
}
