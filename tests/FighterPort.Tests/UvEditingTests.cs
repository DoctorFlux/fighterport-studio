using FighterPort.Core;
using System.Text.Json;

internal static class UvEditingTests
{
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test)
    {
        string worker=Path.GetFullPath("src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),jobs=Path.GetFullPath(".local/jobs");
        var transport=new WorkerProcess(worker,jobs);
        ResourceWorkspace Workspace(IWorkerTransport? proxy=null)=>new(worker,jobs,"scripts/ssbh-worker.py",proxy);
        await test("M3.C1 SRT0 all-track working-copy edits, tangents, bindings, persistence and decoded export",async()=>
        {
            var ws=Workspace();ws.Load(Path.Combine(output,"workspace.fpwork"));
            var doc=ws.Data.Documents.Single(d=>d.Path.EndsWith("workspace-motion.pac"));var entries=await WorkspaceTests.Inventory(ws,doc);
            await ws.ActivateUv(new(){DocumentId=doc.Id,NodeId=entries.Single(e=>e.Name=="OriginalUvEditable").Id});
            var original=ProjectStore.Clone(ws.Data.Uv!);Check(original.Tracks.Count==4&&original.Tracks.Any(t=>t.Indirect)&&original.NativeMaterials.Contains("RetainedEmptyMaterial"),"Fixture coverage");
            var edit=new UvEditSession(ws.Data.Project,original);edit.SetKey(0,3,10,.375f,.03125f);ws.Data.Uv=edit.Group;Check(ws.IsUvDirty,"Dirty edit missing");
            Check(edit.Undo(),"Undo missing");ws.Data.Uv=edit.Group;Check(!ws.IsUvDirty,"Undo failed to restore clean state");Check(edit.Redo(),"Redo missing");
            edit.MoveKey(0,3,10,11);edit.DeleteKey(0,3,11);Check(edit.Undo(),"Delete undo");edit.MoveKey(0,3,11,10);
            edit.SetKey(0,0,10,-1.5f,.05f); // Constant to animated, preserving frame-zero constant.
            edit.Bind(1,"","EditedUnpreviewedMaterial",6,false);
            Reject(()=>edit.Bind(1,original.Tracks[0].MaterialId,original.Tracks[0].MaterialName,0,false));
            ws.Data.Uv=edit.Group;string saved=Path.Combine(output,"uv-edited.fpwork"),export=Path.Combine(output,"edited.srt0");ws.Save(saved);Check(!ws.IsUvDirty,"Save did not clear dirty state");
            var reopened=Workspace();reopened.Load(saved);await reopened.ExportUv(export);
            string request=Path.Combine(output,"decode-uv.json");File.WriteAllText(request,JsonSerializer.Serialize(new BundleRequest{Path=export,Id="0"},ProjectStore.Json));
            var decoded=JsonSerializer.Deserialize<UvGroup>(await transport.Run("resource-uv",request),ProjectStore.Json)!;
            ResourceWorkspace.ValidateUvExport(edit.Group,decoded);
            foreach(var untouched in original.Tracks.Skip(2))Check(JsonSerializer.Serialize(untouched.Curves,ProjectStore.Json)==JsonSerializer.Serialize(decoded.Tracks.Single(t=>t.Slot==untouched.Slot&&t.Indirect==untouched.Indirect).Curves,ProjectStore.Json),"Unevaluated native track changed");
            var wrong=ProjectStore.Clone(decoded);wrong.Tracks.RemoveAt(3);Reject(()=>ResourceWorkspace.ValidateUvExport(edit.Group,wrong));
            wrong=ProjectStore.Clone(decoded);wrong.Tracks[0].Curves[3].Keys[0].Tangent+=.01f;Reject(()=>ResourceWorkspace.ValidateUvExport(edit.Group,wrong));
            foreach(var source in ws.Data.Documents)ResourceWorkspace.Verify(source);
            await Fail(()=>reopened.ExportUv(export));
            reopened.Data.Uv!.Tracks.RemoveAt(3);await Fail(()=>reopened.ExportUv(export+".dropped.srt0"));Check(!File.Exists(export+".dropped.srt0"),"Partial-track output published");
        });
        await test("M3.C1 SRT0 export failure/cancellation/publication race preserves state and destinations",async()=>
        {
            string saved=Path.Combine(output,"uv-edited.fpwork");
            foreach(string mode in new[]{"fail","cancel","race","corrupt"})
            {
                string dest=Path.Combine(output,mode+".srt0");using var cancel=new CancellationTokenSource();
                var proxy=new Proxy(async(op,input,ct)=>
                {
                    if(op=="export-uv"&&mode=="fail")throw new InvalidDataException("Injected native worker failure");
                    byte[] bytes=await transport.Run(op,input,ct);
                    if(op=="export-uv")
                    {
                        if(mode=="cancel")cancel.Cancel();
                        if(mode=="race")File.WriteAllBytes(dest,[1,2,3]);
                        if(mode=="corrupt")return [0,0,0,0];
                    }
                    return bytes;
                });
                var ws=Workspace(proxy);ws.Load(saved);string before=JsonSerializer.Serialize(ws.Data,ProjectStore.Json);
                await Fail(()=>ws.ExportUv(dest,cancel.Token));Check(before==JsonSerializer.Serialize(ws.Data,ProjectStore.Json),"Failed export changed working copy");
                Check(mode=="race"?File.ReadAllBytes(dest).SequenceEqual(new byte[]{1,2,3}):!File.Exists(dest),"Failed export published/overwrote output");
                Check(!Directory.EnumerateFiles(jobs,"*.srt0").Any(),"Temporary SRT0 leaked");
            }
        });
    }
    private sealed class Proxy(Func<string,string,CancellationToken,Task<byte[]>> run):IWorkerTransport{public Task<byte[]> Run(string op,string input,CancellationToken cancellation=default)=>run(op,input,cancellation);}
    private static async Task Fail(Func<Task> action){try{await action();}catch(Exception e)when(e is InvalidDataException or IOException or OperationCanceledException){return;}throw new Exception("Invalid export succeeded");}
    private static void Reject(Action action){try{action();}catch(InvalidDataException){return;}throw new Exception("Invalid change accepted");}
    private static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
}
