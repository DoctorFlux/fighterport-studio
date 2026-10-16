using FighterPort.Core;
using System.Numerics;
using System.Text.Json;

internal static class AncillaryTests
{
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test)
    {
        string root=Environment.CurrentDirectory,worker=Path.Combine(root,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),jobs=Path.Combine(root,".local/jobs");
        var transport=new WorkerProcess(worker,jobs);
        ResourceWorkspace Workspace()=>new(worker,jobs,Path.Combine(root,"scripts/ssbh-worker.py"));
        await test("M3.D1 original CI8 RGB5A3 palette/alpha/mips and explicit alternate palette",async()=>
        {
            string request=Path.Combine(output,"palette-request.json"),path=Path.Combine(output,"indexed.pac");
            File.WriteAllText(request,"{}");File.WriteAllBytes(path,await transport.Run("fixture-workspace-palette",request));
            var ws=Workspace();var doc=await ws.Open(path);var entries=await WorkspaceTests.Inventory(ws,doc);var tex=entries.Single(e=>e.Name=="IndexedAlpha"&&e.Kind=="TEX0");var alternate=entries.Single(e=>e.Name=="AlternatePalette");
            File.WriteAllBytes(Path.Combine(output,"asymmetric.png"),await ws.Texture(new(){DocumentId=doc.Id,NodeId=entries.Single(e=>e.Name=="Asymmetric").Id}));
            Check(tex.PixelFormat=="CI8"&&tex.Width==32&&tex.Height==32&&tex.MipCount==2,"Native texture metadata mismatch");
            foreach(int mip in new[]{0,1})foreach(string palette in new[]{"",alternate.Id})
            {
                File.WriteAllText(request,JsonSerializer.Serialize(new BundleRequest{Path=path,Id=tex.Id,Mip=mip,PaletteId=palette},ProjectStore.Json));
                var decoded=JsonSerializer.Deserialize<PixelProbe>(await transport.Run("resource-texture-probe",request),ProjectStore.Json)!;
                Check(decoded.Width==(32>>mip)&&decoded.Height==(32>>mip),"Mip dimensions");
                var expected=new[]{unchecked((int)(palette==""?0xffff0000:0xffffff00)),unchecked((int)0xff00ff00),unchecked((int)0xff0000ff),0};
                Check(decoded.Pixels.SequenceEqual(expected),"Independent palette quadrant colors/alpha mismatch mip "+mip);
            }
            bool rejected=false;try{await ws.Texture(new(){DocumentId=doc.Id,NodeId=tex.Id},2);}catch(InvalidDataException){rejected=true;}Check(rejected,"Out-of-range mip accepted");
            ResourceWorkspace.Verify(doc);
        });
        await test("M3.C1 XSI/3dsMax SRT all flag branches against native and independent quarter turns",async()=>
        {
            var cases=new List<TextureBinding>();
            foreach(string mode in new[]{"MatrixXSI","Matrix3dsMax"})foreach(float angle in new[]{0,90,-37.5f,720.25f})foreach(bool scale in new[]{false,true})foreach(bool translate in new[]{false,true})
                cases.Add(new(){MatrixMode=mode,RestSrt=[scale?2:1,scale?3:1,angle,translate?.125f:0,translate?-.25f:0]});
            string request=Path.Combine(output,"matrix-request.json");File.WriteAllText(request,JsonSerializer.Serialize(cases,ProjectStore.Json));
            var native=JsonSerializer.Deserialize<float[][]>(await transport.Run("resource-uv-matrices",request),ProjectStore.Json)!;
            for(int i=0;i<cases.Count;i++)Near(UvEvaluator.Transform(cases[i].MatrixMode,cases[i].RestSrt,new(.25f,.75f)),new(native[i][0],native[i][1]));
            Near(UvEvaluator.Transform("MatrixXSI",[1,1,90,0,0],new(.25f,.75f)),new(.75f,-.25f));
            Near(UvEvaluator.Transform("Matrix3dsMax",[1,1,90,0,0],new(.25f,.75f)),new(.25f,.25f));
            var p=new CharacterProject{Materials=[new(){Id="m",Textures=[new(){Slot=0}]}]};
            var g=new UvGroup{FrameCount=11,MatrixMode="Matrix3dsMax",Tracks=[new(){MaterialId="m",MaterialName="m",Curves=[new(){Channel=2,Keys=[new(){Frame=0,Value=0,Tangent=18},new(){Frame=10,Value=180,Tangent=18}]}]}]};
            UvEvaluator.Validate(p,g);Near(UvEvaluator.Sample(p,"m",g,5,new(.25f,.75f)),new(.25f,.25f));g.Enabled=false;Near(UvEvaluator.Sample(p,"m",g,5,new(.25f,.75f)),new(.25f,.75f));
        });
        await test("M3.C1 VIS0 edit/bind undo, persistence, genuine native export and decoded rejection",async()=>
        {
            var ws=Workspace();ws.Load(Path.Combine(output,"workspace.fpwork"));var original=ProjectStore.Clone(ws.Data.Visibility!);var session=new VisibilityEditSession(ws.Data.Project,original);
            session.SetFrame(0,10,true);Check(session.Group.Tracks[0].Values[10]&&!original.Tracks[0].Values[10],"Edit mutated original");Check(session.Undo()&&!session.Group.Tracks[0].Values[10],"Undo failed");Check(session.Redo()&&session.Group.Tracks[0].Values[10],"Redo failed");
            var rootBone=ws.Data.Project.Bones.First(b=>b.Name=="Root");session.Bind(0,rootBone.Id);Check(session.Group.Tracks[0].Target=="Root","Native binding not changed");session.Undo();
            // Include constant false and true tracks to check encoded flags, including unbound native names.
            session.Group.Tracks.Add(new(){Target="OriginalConstantHidden",Values=[false]});session.Group.Tracks.Add(new(){Target="OriginalConstantShown",Values=[true]});
            var constantEdit=new VisibilityEditSession(ws.Data.Project,session.Group);constantEdit.SetFrame(1,5,true);
            Check(constantEdit.Group.Tracks[1].Values.Length==original.FrameCount&&constantEdit.Group.Tracks[1].Values.Count(v=>v)==1,"Constant edit expansion changed other frames");
            constantEdit.Undo();Check(constantEdit.Group.Tracks[1].Values.Length==1&&!constantEdit.Group.Tracks[1].Values[0],"Constant undo lost flags");
            Reject(()=>constantEdit.Bind(1,session.Group.Tracks[0].BoneId));Check(constantEdit.Group.Tracks[1].BoneId=="","Rejected duplicate binding mutated working copy");
            ws.Data.Visibility=session.Group;string saved=Path.Combine(output,"visibility-edited.fpwork"),export=Path.Combine(output,"edited.vis0");ws.Save(saved);var reopened=Workspace();reopened.Load(saved);await reopened.ExportVisibility(export);
            var doc=await reopened.Open(export);await reopened.ActivateVisibility(new(){DocumentId=doc.Id,NodeId="0"});ResourceWorkspace.ValidateVisibilityExport(session.Group,reopened.Data.Visibility!);
            var wrong=ProjectStore.Clone(reopened.Data.Visibility!);wrong.Tracks[0].Values[10]=false;Reject(()=>ResourceWorkspace.ValidateVisibilityExport(session.Group,wrong));
            wrong=ProjectStore.Clone(reopened.Data.Visibility!);wrong.Loop=!wrong.Loop;Reject(()=>ResourceWorkspace.ValidateVisibilityExport(session.Group,wrong));
            wrong=ProjectStore.Clone(reopened.Data.Visibility!);wrong.NativeOriginalPath="lost";Reject(()=>ResourceWorkspace.ValidateVisibilityExport(session.Group,wrong));
            string req=Path.Combine(output,"edited-vis-pose.json");
            foreach(float f in new[]{9.75f,10,10.99f,11,20,(float)original.FrameCount-1})
            {
                var model=reopened.Document(reopened.Data.ActiveModel!);
                File.WriteAllText(req,JsonSerializer.Serialize(new BundlePoseRequest{ModelPath=model.Path,ModelId=reopened.Data.ActiveModel!.NodeId,ClipPath=export,ClipId="0",Frame=f},ProjectStore.Json));
                var native=JsonSerializer.Deserialize<bool[][]>(await transport.Run("resource-vis-pose",req),ProjectStore.Json)!;
                Check(native.SelectMany(x=>x).SequenceEqual(SceneEvaluator.Sample(reopened.Data.Project,null,reopened.Data.Visibility,f).DrawVisibility.SelectMany(x=>x)),"Edited native draw mismatch");
            }
            foreach(var source in ws.Data.Documents)ResourceWorkspace.Verify(source);
            bool rejected=false;try{await ws.ExportVisibility(export);}catch(IOException){rejected=true;}Check(rejected,"Existing output overwritten");
            ws.Data.Visibility.Rate=2;rejected=false;try{await ws.ExportVisibility(export+".rate.vis0");}catch(InvalidDataException){rejected=true;}Check(rejected&&!File.Exists(export+".rate.vis0"),"Rate override silently discarded");
        });
    }
    private sealed class PixelProbe{public int Width{get;set;}public int Height{get;set;}public int[] Pixels{get;set;}=[];}
    private static void Near(Vector2 a,Vector2 b)=>Check(Vector2.Distance(a,b)<.00001f,$"UV expected {b}, got {a}");
    private static void Reject(Action action){try{action();}catch(InvalidDataException){return;}throw new Exception("Invalid decode accepted");}
    private static void Check(bool ok,string reason){if(!ok)throw new Exception(reason);}
}
