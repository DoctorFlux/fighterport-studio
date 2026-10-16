using FighterPort.Core;
using System.Text.Json;

internal static class WorkspaceTests
{
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test)
    {
        await test("M1.E/M3.C1/D1 native resource workspace, material binding, VIS0 boundaries and durable sources",async()=>
        {
            string root=Environment.CurrentDirectory,worker=Path.Combine(root,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),jobs=Path.Combine(root,".local/jobs");
            var backend=new NativeBackend(worker,jobs);var transport=new WorkerProcess(worker,jobs);var demo=Demo.Create();
            string mdl=Path.Combine(output,"workspace.mdl0"),chr=Path.Combine(output,"workspace.chr0"),model=Path.Combine(output,"workspace-model.pac"),motion=Path.Combine(output,"workspace-motion.pac");
            await backend.GenerateModelFixture(demo,mdl);await backend.ExportClip(demo,demo.Clips[2],chr);
            string request=Path.Combine(output,"fixture-request.json");File.WriteAllText(request,JsonSerializer.Serialize(new BundlePoseRequest{ModelPath=mdl,ClipPath=chr},ProjectStore.Json));
            File.WriteAllBytes(model,await transport.Run("fixture-workspace-model",request));File.WriteAllBytes(motion,await transport.Run("fixture-workspace-motion",request));
            var ws=new ResourceWorkspace(worker,jobs,Path.Combine(root,"scripts/ssbh-worker.py"));
            var loose=await ws.Open(mdl);Check(loose.Root.Kind=="MDL0","Unified standalone preflight/read");
            var a=await ws.Open(model);var b=await ws.Open(motion);
            var models=await Inventory(ws,a);var motions=await Inventory(ws,b);
            AssetHandle Handle(SourceDocument d,NativeTreeEntry e)=>new(){DocumentId=d.Id,NodeId=e.Id};
            await ws.ActivateModel(Handle(a,models.Single(e=>e.Kind=="MDL0")));
            Check(ws.Data.Project.Materials.Count>0,"No native materials");
            foreach(var binding in ws.Data.Project.Materials.SelectMany(m=>m.Textures))Check(binding.Texture.NodeId!=""&&binding.Texture.DocumentId==a.Id,"Unresolved original binding");
            Check(models.Count(e=>e.Kind=="TEX0"&&e.Name=="OriginalQuadrants")==2,"Scoped duplicate fixture missing");
            var texture=ws.Data.Project.Materials[0].Textures[0].Texture;Check(texture.NodeId.StartsWith("0/1/"),"Wrong ARC texture group selected");byte[] png=await ws.Texture(texture);Check(png.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}),"Native PNG decode");File.WriteAllBytes(Path.Combine(output,"quadrants.png"),png);
            await ws.ActivateClip(Handle(b,motions.Single(e=>e.Kind=="CHR0")));await ws.ActivateVisibility(Handle(b,motions.Single(e=>e.Kind=="VIS0")));
            var p=ws.Data.Project;var vis=ws.Data.Visibility!;int mesh=p.Meshes.FindIndex(m=>m.Draws.Any(d=>d.VisibilityBoneId==vis.Tracks[0].BoneId));Check(mesh>=0,"VIS target missing");
            foreach(float f in new[]{0,9.75f,10,19.99f,20,10,0,-1,vis.FrameCount+10})
            {
                float wrapped=vis.Loop?((f%vis.FrameCount)+vis.FrameCount)%vis.FrameCount:Math.Clamp(f,0,vis.FrameCount-1);
                var state=SceneEvaluator.Sample(p,p.Clips[0],vis,f);Check(state.DrawVisibility[mesh][0]==(wrapped<10||wrapped>=20),"VIS boundary/reverse/loop "+f);
                File.WriteAllText(request,JsonSerializer.Serialize(new BundlePoseRequest{ModelPath=model,ModelId=ws.Data.ActiveModel!.NodeId,ClipPath=motion,ClipId=vis.Source.NodeId,Frame=wrapped},ProjectStore.Json));
                var native=JsonSerializer.Deserialize<bool[][]>(await transport.Run("resource-vis-pose",request),ProjectStore.Json)!;
                Check(native.SelectMany(d=>d).SequenceEqual(state.DrawVisibility.SelectMany(d=>d)),"Canonical VIS differs from native ApplyVIS at "+f);
            }
            vis.Enabled=false;Check(SceneEvaluator.Sample(p,p.Clips[0],vis,10).DrawVisibility[mesh][0],"Mute does not restore draw default");vis.Enabled=true;
            Check(motions.Any(e=>e.Kind=="SRT0"&&e.Name=="PreservedUvGap"),"Unevaluated native channel lost");
            await ws.ActivateUv(Handle(b,motions.Single(e=>e.Kind=="SRT0"&&e.Name=="OriginalUvSlide")));
            await CheckUv(ws,transport,request,model,motion);
            var binding0=p.Materials[0].Textures[0];ws.OverrideTexture(p.Materials[0].Id,binding0.Slot,texture);
            string saved=Path.Combine(output,"workspace.fpwork");ws.Save(saved);var reopened=new ResourceWorkspace(worker,jobs,Path.Combine(root,"scripts/ssbh-worker.py"));reopened.Load(saved);
            Check((await reopened.Texture(texture)).SequenceEqual(png),"Texture after reopen");Check(reopened.Data.Visibility!.Tracks[0].Values.SequenceEqual(vis.Tracks[0].Values),"VIS lost");Check(reopened.Data.Project.Materials[0].Textures[0].Status.Contains("override"),"Override lost");
            Check(reopened.Data.Uv!.Tracks.Count==1,"UV lost on reopen");
            await reopened.ActivateClip(Handle(b,motions.Single(e=>e.Kind=="CHR0")));Check(reopened.Data.Visibility==null,"VIS leaked to another clip activation");
            byte[] original=File.ReadAllBytes(motion);File.AppendAllText(motion,"x");bool rejected=false;try{ws.Save(saved);}catch(InvalidDataException){rejected=true;}finally{File.WriteAllBytes(motion,original);}Check(rejected,"Changed source accepted");
            foreach(var d in ws.Data.Documents)ResourceWorkspace.Verify(d);
            rejected=false;try{ws.Save(model);}catch(InvalidDataException){rejected=true;}Check(rejected,"Source overwrite accepted");
            File.WriteAllText(request,JsonSerializer.Serialize(new BundlePoseRequest{ModelPath=mdl,ClipPath=chr},ProjectStore.Json));
            string ambiguous=Path.Combine(output,"ambiguous.pac");File.WriteAllBytes(ambiguous,await transport.Run("fixture-workspace-ambiguous",request));
            var ambiguousDoc=await ws.Open(ambiguous);var nodes=await Inventory(ws,ambiguousDoc);await ws.ActivateModel(Handle(ambiguousDoc,nodes.Single(n=>n.Kind=="MDL0")));
            Check(ws.Data.Project.Materials.SelectMany(m=>m.Textures).All(t=>t.Texture.NodeId==""&&t.Status.StartsWith("Ambiguous")),"Duplicate eligible bank silently resolved");
        });
        await test("M3.C1 UV independent Maya ST expectation and unsupported rotation retention",()=>
        {
            var p=new CharacterProject{Materials=[new(){Id="eye",Name="eye",Textures=[new(){Slot=0,UVSet=0,RestSrt=[2,3,0,0,0]}]}]};
            var group=new UvGroup{FrameCount=11,MatrixMode="MatrixMaya",Tracks=[new(){MaterialId="eye",MaterialName="eye",Slot=0,Curves=[new(){Channel=3,Keys=[new(){Frame=0,Value=0},new(){Frame=10,Value=.5f}]}]}]};
            UvEvaluator.Validate(p,group);var uv=UvEvaluator.Sample(p,"eye",group,5,new(.25f,.75f));Check(Math.Abs(uv.X)<1e-6&&Math.Abs(uv.Y-2.25f)<1e-6,"Independent UV midpoint expectation");
            group.Enabled=false;Check(UvEvaluator.Sample(p,"eye",group,5,new(.25f,.75f)).X==.5f,"UV mute leaked");group.Enabled=true;
            group.Tracks[0].Curves.Add(new(){Channel=2,Keys=[new(){Frame=0,Value=30}]});Check(!UvEvaluator.Support(p,group,group.Tracks[0]).StartsWith("Evaluated:"),"Rotation silently supported");Check(group.Tracks[0].Curves.Count==2,"Unsupported track dropped");
            var duplicate=ProjectStore.Clone(group.Tracks[0]);duplicate.MaterialName="DifferentDisplayName";group.Tracks.Add(duplicate);bool rejected=false;try{UvEvaluator.Validate(p,group);}catch(InvalidDataException){rejected=true;}Check(rejected,"Duplicate bound material target accepted");
            return Task.CompletedTask;
        });
    }
    internal static async Task RunLocal(string output,Func<string,Func<Task>,Task> test)
    {
        await test("M1.E/M3.D1/C1 supplied Mario scoped textures and native SRT0 fractional comparison",async()=>
        {
            var settings=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(".local/assets.json"))!;
            string root=Environment.CurrentDirectory,worker=Path.Combine(root,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),jobs=Path.Combine(root,".local/jobs");
            var ws=new ResourceWorkspace(worker,jobs,Path.Combine(root,"scripts/ssbh-worker.py"));var transport=new WorkerProcess(worker,jobs);
            string model=Path.Combine(settings["brawl"],"FitMario00.pac"),motion=Path.Combine(settings["brawl"],"FitMarioMotionEtc.pac");
            var a=await ws.Open(model);var b=await ws.Open(motion);
            await ws.ActivateModel(new(){DocumentId=a.Id,NodeId="0/0/0/0"});Check(ws.Data.Project.Bones.Count==63,"Wrong Mario model");
            var textures=ws.Data.Project.Materials.SelectMany(m=>m.Textures).Where(t=>t.Texture.NodeId!="").DistinctBy(t=>t.Texture.NodeId).ToArray();Check(textures.Length>=8,"Missing scoped Mario textures");
            foreach(var t in textures)Check((await ws.Texture(t.Texture,palette:t.PaletteId)).Length>8,"Texture decode failed");
            await ws.ActivateClip(new(){DocumentId=b.Id,NodeId="0/1/0/0/5"});await ws.ActivateUv(new(){DocumentId=b.Id,NodeId="0/1/0/2/1"});
            Check(ws.Data.Uv!.Tracks.Count(t=>t.Status.StartsWith("Evaluated:"))==4,"Expected four bound eye sampler-0 tracks");
            await CheckUv(ws,transport,Path.Combine(output,"local-uv-request.json"),model,motion);
            ws.Save(Path.Combine(output,"mario-workspace.fpwork"));foreach(var doc in ws.Data.Documents)ResourceWorkspace.Verify(doc);
            Console.WriteLine($"  Mario {textures.Length} unique resolved textures; {ws.Data.Uv.Tracks.Count} retained SRT tracks / 4 evaluated; native UV comparison <= 0.00001");
        });
    }
    private static async Task CheckUv(ResourceWorkspace ws,WorkerProcess transport,string request,string model,string motion)
    {
        var group=ws.Data.Uv!;var p=ws.Data.Project;bool moves=false;
        foreach(float f in new[]{0,.5f,9.75f,20,20.25f,40,(float)group.FrameCount-1}.Concat(group.Tracks.Where(t=>t.Slot==0).SelectMany(t=>t.Curves).SelectMany(c=>c.Keys).Select(k=>k.Frame+.25f)).Distinct().Take(32))
        {
            float frame=Math.Min(f,group.FrameCount-1);
            File.WriteAllText(request,JsonSerializer.Serialize(new BundlePoseRequest{ModelPath=model,ModelId=ws.Data.ActiveModel!.NodeId,ClipPath=motion,ClipId=group.Source.NodeId,Frame=frame},ProjectStore.Json));
            var native=JsonSerializer.Deserialize<Dictionary<string,float[]>>(await transport.Run("resource-uv-pose",request),ProjectStore.Json)!;
            foreach(var track in group.Tracks.Where(t=>UvEvaluator.Support(p,group,t).StartsWith("Evaluated:")))
            {
                var uv=UvEvaluator.Sample(p,track.MaterialId,group,frame,new(.25f,.75f));var expected=native[track.MaterialId+":0"];
                Check(Math.Abs(uv.X-expected[0])<.00001f&&Math.Abs(uv.Y-expected[1])<.00001f,"Native SRT matrix mismatch at "+frame);
                if(System.Numerics.Vector2.Distance(uv,UvEvaluator.Sample(p,track.MaterialId,group,0,new(.25f,.75f)))>.00001)moves=true;
            }
        }
        Check(moves,"UV animation is constant");
    }
    internal static async Task<List<NativeTreeEntry>> Inventory(ResourceWorkspace ws,SourceDocument doc)
    {
        var found=new List<NativeTreeEntry>();var queue=new Queue<NativeTreeEntry>();queue.Enqueue(doc.Root);
        while(queue.TryDequeue(out var entry))
        {
            found.Add(entry);
            // Resource bodies remain lazy. Only archive/group containers are expanded for inventory.
            if(entry.HasChildren&&entry.Kind is "ARC" or "BRRES" or "BRESGroup")foreach(var child in await ws.Children(new(){DocumentId=doc.Id,NodeId=entry.Id}))queue.Enqueue(child);
        }
        return found;
    }
    private static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
}
