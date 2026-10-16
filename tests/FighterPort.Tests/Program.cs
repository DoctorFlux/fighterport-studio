using FighterPort.Core;
using System.Numerics;
using System.Security.Cryptography;

if(args.FirstOrDefault()=="--fault-worker")return await WorkerFailureTests.Child(args);

int passed=0,failed=0;
string output=Path.Combine(Environment.CurrentDirectory,"artifacts/tests",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(output);
void Assert(bool condition,string message="Assertion failed"){if(!condition)throw new Exception(message);}
void Near(float a,float b,float tolerance=.001f){Assert(Math.Abs(a-b)<=tolerance,$"Expected {b}, got {a}, tolerance {tolerance}");}
void Reject(Action action){try{action();}catch(InvalidDataException){return;}throw new Exception("Invalid input was accepted");}
async Task Test(string name,Func<Task> run){try{await run();Console.WriteLine("PASS "+name);passed++;}catch(Exception e){Console.WriteLine("FAIL "+name+": "+e);failed++;}}
Task Sync(Action a){a();return Task.CompletedTask;}
if(args.Contains("--workspace-only")){await WorkspaceTests.Run(output,Test);if(args.Contains("--local-assets"))await WorkspaceTests.RunLocal(output,Test);Console.WriteLine($"RESULT {passed} passed; {failed} failed; output {output}");return failed==0?0:1;}
var demo=Demo.Create();
await Test("M1 original detailed rigs, blended skin and six motions",()=>Sync(()=>{foreach(int variant in new[]{0,1,2}){var p=Demo.Create(variant);Assert(p.Bones.Count>45);Assert(p.Meshes.Count>40);Assert(p.Meshes.SelectMany(m=>m.Vertices).Any(v=>v.Weights.Count(w=>w>0)>1));ProjectStore.Validate(p);}Assert(demo.Clips.Count==9);}));
await Test("M1 rest skin equals original mesh, exact inverse binds",()=>Sync(()=>{var world=Animation.World(demo,null,0);foreach(var v in demo.Meshes.SelectMany(m=>m.Vertices)){Near(Vector3.Distance(Animation.Skin(v,demo,world).Position,Animation.V(v.Position)),0,.0001f);}}));
await Test("M1 skin playback deforms actual weighted geometry",()=>Sync(()=>{var mesh=demo.Meshes.SelectMany(m=>m.Vertices).ToArray();var a=Animation.World(demo,demo.Clips[2],0);var b=Animation.World(demo,demo.Clips[2],30);Assert(mesh.Any(v=>Vector3.Distance(Animation.Skin(v,demo,a).Position,Animation.Skin(v,demo,b).Position)>5));}));
await Test("M1 XYZ transform independent expectation",()=>Sync(()=>{var m=Animation.Local([1,1,1],[0,0,90],[2,3,4]);var p=Vector3.Transform(Vector3.UnitX,m);Near(p.X,2);Near(p.Y,4);Near(p.Z,4);Near(Vector3.Transform(p,Animation.M(Inverse(m))).X,1);}));
await Test("M1 Hermite fractional tangent expectation",()=>Sync(()=>{var c=new Curve{Keys=[new(){Frame=0,Value=0,Tangent=2},new(){Frame=10,Value=10,Tangent=0}]};Near(Animation.Evaluate(c,2.5f),4.375f);Near(Animation.Evaluate(c,5),7.5f);}));
await Test("M1 full rotation winding retained at fractional frame",()=>Sync(()=>{var c=demo.Clips[5].Curves[0];Near(Animation.Evaluate(c,45),540);Near(Animation.Evaluate(c,12.5f),150);}));
await Test("M1 hold discontinuity and loop boundary",()=>Sync(()=>{var c=demo.Clips[6].Curves[0];Near(Animation.Evaluate(c,29.99f),0);Near(Animation.Evaluate(c,30),55);Near(Animation.Wrap(demo.Clips[0],61),0);Near(Animation.Wrap(demo.Clips[0],-.5f),60.5f);}));
await Test("M1 nonuniform normal transform remains unit length",()=>Sync(()=>{var p=Demo.Create();var w=Animation.World(p,p.Clips[7],30);foreach(var v in p.Meshes.SelectMany(m=>m.Vertices))Near(Animation.Skin(v,p,w).Normal.Length(),1);}));
await Test("M1 save/reopen and immutable undo/redo",()=>Sync(()=>{var s=new EditSession(ProjectStore.Clone(demo));string bone=demo.Clips[2].Curves[0].Bone;s.EditKey(2,bone,4,15,-42);var edited=Animation.Evaluate(s.Project.Clips[2].Curves[0],15);Near(edited,-42);Assert(s.Undo());Assert(Math.Abs(Animation.Evaluate(s.Project.Clips[2].Curves[0],15)-edited)>1);Assert(s.Redo());string path=Path.Combine(output,"roundtrip.fpproj");ProjectStore.Save(s.Project,path);var p=ProjectStore.Load(path);Near(Animation.Evaluate(p.Clips[2].Curves[0],15),-42);Assert(p.Clips[2].Curves[0].Keys.Any(k=>k.Frame==15&&k.Locked));}));
await Test("M1 malformed hierarchy rejected",()=>Sync(()=>{var p=ProjectStore.Clone(demo);p.Bones[1].Parent=1;Reject(()=>ProjectStore.Validate(p));}));
await Test("M1 invalid weights, topology, finite values rejected",()=>Sync(()=>{var p=ProjectStore.Clone(demo);p.Meshes[0].Vertices[0].Weights=[2,2];Reject(()=>ProjectStore.Validate(p));p=ProjectStore.Clone(demo);p.Meshes[0].Indices[0]=999999;Reject(()=>ProjectStore.Validate(p));p=ProjectStore.Clone(demo);p.Bones[0].Rotation[0]=float.NaN;Reject(()=>ProjectStore.Validate(p));}));
await Test("M1 missing bone binding and unknown schema rejected",()=>Sync(()=>{var p=ProjectStore.Clone(demo);p.Clips[0].Curves[0].Bone="missing";Reject(()=>ProjectStore.Validate(p));p=ProjectStore.Clone(demo);p.Version=99;Reject(()=>ProjectStore.Validate(p));}));
await Test("M1 retained resource hash verifies and detects corruption",()=>Sync(()=>{string path=Path.Combine(output,"original.bin");File.WriteAllBytes(path,[1,2,3,4]);var p=ProjectStore.Clone(demo);ProjectStore.Retain(p,path);ProjectStore.Validate(p);p.Resources[0].Base64="AQ==";Reject(()=>ProjectStore.Validate(p));Assert(File.ReadAllBytes(path).SequenceEqual(new byte[]{1,2,3,4}));}));
await Test("M1 duplicate key/disallowed interpolation rejected",()=>Sync(()=>{var p=ProjectStore.Clone(demo);p.Clips[0].Curves[0].Keys[1].Frame=0;Reject(()=>ProjectStore.Validate(p));p=ProjectStore.Clone(demo);p.Clips[0].Curves[0].Interpolation="guessed";Reject(()=>ProjectStore.Validate(p));}));

CharacterProject RenamedTarget()
{
    var target=ProjectStore.Clone(demo);target.Clips.Clear();target.Name="Renamed longer limbs";
    for(int i=0;i<target.Bones.Count;i++){target.Bones[i].Id=$"target-{i}";target.Bones[i].Name=$"other_{i}";target.Bones[i].Translation[0]*=1.3f;}
    Animation.SetInverseBinds(target);return target;
}
MappingProfile ManualProfile(CharacterProject target)=>new(){Approved=true,SourceFingerprint=Animation.Fingerprint(demo),TargetFingerprint=Animation.Fingerprint(target),BoneIds=demo.Bones.Select((b,i)=>(b,i)).ToDictionary(x=>x.b.Id,x=>target.Bones[x.i].Id)};
await Test("M1.D renamed FK profile transfers motion and preserves target mesh/binds",()=>Sync(()=>
{
    var target=RenamedTarget();var profile=ManualProfile(target);string before=System.Text.Json.JsonSerializer.Serialize(target);string sourceBefore=System.Text.Json.JsonSerializer.Serialize(demo);
    var result=FkRetarget.Transfer(demo,2,target,profile);Assert(result.Branch=="target/fk");Assert(result.Clips.Count==1);
    Assert(System.Text.Json.JsonSerializer.Serialize(target)==before&&System.Text.Json.JsonSerializer.Serialize(demo)==sourceBefore,"Input mutated");
    Assert(System.Text.Json.JsonSerializer.Serialize(result.Meshes)==System.Text.Json.JsonSerializer.Serialize(target.Meshes));Assert(System.Text.Json.JsonSerializer.Serialize(result.Bones)==System.Text.Json.JsonSerializer.Serialize(target.Bones));
    foreach(var c in result.Clips[0].Curves){var i=target.Bones.FindIndex(b=>b.Name==c.Bone);var original=demo.Clips[2].Curves.Single(x=>x.Bone==demo.Bones[i].Name&&x.Channel==c.Channel);Near(Animation.Evaluate(c,23.25f),Animation.Evaluate(original,23.25f));}
    string path=Path.Combine(output,"fk.fpproj");ProjectStore.Save(result,path);Assert(ProjectStore.Load(path).MappingProfiles[0].Approved);
}));
await Test("M1.D approval, stale fingerprint and incomplete map reject",()=>Sync(()=>
{
    var target=RenamedTarget();var profile=ManualProfile(target);profile.Approved=false;Reject(()=>FkRetarget.Transfer(demo,2,target,profile));profile.Approved=true;profile.SourceFingerprint="stale";Reject(()=>FkRetarget.Transfer(demo,2,target,profile));profile=ManualProfile(target);profile.BoneIds.Clear();Reject(()=>FkRetarget.Transfer(demo,2,target,profile));
}));
await Test("M1.D incompatible axes and hierarchy reject without guessing",()=>Sync(()=>
{
    var target=RenamedTarget();target.Bones[0].Rotation=[0,25,0];Animation.SetInverseBinds(target);Reject(()=>FkRetarget.Transfer(demo,2,target,ManualProfile(target)));target=RenamedTarget();target.Bones[4].Parent=1;Animation.SetInverseBinds(target);Reject(()=>FkRetarget.Transfer(demo,2,target,ManualProfile(target)));
}));
await Test("M1.D full turns retained and root motion requires policy",()=>Sync(()=>
{
    var target=RenamedTarget();var profile=ManualProfile(target);var spin=FkRetarget.Transfer(demo,5,target,profile);Near(Animation.Evaluate(spin.Clips[0].Curves[0],45),540);Reject(()=>FkRetarget.Transfer(demo,4,target,profile));profile.RootTranslation="CopyDelta";var jump=FkRetarget.Transfer(demo,4,target,profile);Near(Animation.Evaluate(jump.Clips[0].Curves[0],30),45);Reject(()=>FkRetarget.Transfer(demo,7,target,profile));
}));
await Test("M1.D automatic template is only an unapproved exact-name suggestion",()=>Sync(()=>{var target=RenamedTarget();var template=FkRetarget.Template(demo,target);Assert(!template.Approved&&template.BoneIds.Count==0);Reject(()=>FkRetarget.Transfer(demo,2,target,template));}));

await Test("M1.D2 mapping session approval transitions and snapshot isolation",()=>Sync(()=>
{
    var s=new MappingSession(demo,demo);s.Approve(true);s.Assign(demo.Bones[10].Id,null);Assert(!s.Profile.Approved);s.Assign(demo.Bones[10].Id,demo.Bones[10].Id);s.Approve(true);s.SetRootPolicy("CopyDelta");Assert(!s.Profile.Approved);
    s.Approve(true);s.Reset();Assert(!s.Profile.Approved);var copy=s.Source;copy.Name="mutated";Assert(s.Source.Name==demo.Name);s.Clear();s.Approve(true);Reject(()=>s.Preview(2));
}));
await Test("M1.D2 invalid profile load transactional and duplicate assignment rejected",()=>Sync(()=>
{
    var s=new MappingSession(demo,demo);s.Approve(true);var p=s.Profile;p.TargetFingerprint="bad";Reject(()=>s.LoadProfile(p));Assert(s.Profile.Approved);Reject(()=>s.Assign(demo.Bones[1].Id,demo.Bones[2].Id));Assert(s.Profile.BoneIds[demo.Bones[1].Id]==demo.Bones[1].Id);
    string path=Path.Combine(output,"mapping.json");s.SaveProfile(path);s.Clear();s.LoadProfile(path);Assert(s.Profile.Approved&&s.Profile.BoneIds.Count==demo.Bones.Count);
}));
await Test("M1.D2 new result only; existing target cannot be overwritten",()=>Sync(()=>
{
    var s=new MappingSession(demo,demo);s.Approve(true);string path=Path.Combine(output,"mapping-target.fpproj");s.SaveResult(2,path);byte[] before=File.ReadAllBytes(path);bool rejected=false;try{s.SaveResult(2,path);}catch(IOException){rejected=true;}Assert(rejected&&before.SequenceEqual(File.ReadAllBytes(path)));Assert(ProjectStore.Load(path).Branch=="target/fk");Assert(s.Source.Clips.Count==demo.Clips.Count&&s.Target.Clips.Count==demo.Clips.Count);
}));

await Test("M2.C encoded validator rejects error, winding loss, missing samples and nonfinite data",()=>Sync(()=>
{
    var clip=demo.Clips[5];var samples=new Dictionary<string,float[]>();
    foreach(string bone in clip.Curves.Select(c=>c.Bone).Distinct())for(int channel=0;channel<9;channel++)
    {
        var curve=clip.Curves.SingleOrDefault(c=>c.Bone==bone&&c.Channel==channel);
        samples[bone+":"+channel]=Enumerable.Range(0,clip.FrameCount*4).Select(i=>curve==null?(channel<3?1f:0f):Animation.Evaluate(curve,i*.25f)).ToArray();
    }
    EncodedClipValidator.Validate(clip,clip,samples);var id=clip.Curves[0].Bone+":4";var good=samples[id].ToArray();samples[id][180]-=360;Reject(()=>EncodedClipValidator.Validate(clip,clip,samples));samples[id]=good.ToArray();samples[id][1]=float.NaN;Reject(()=>EncodedClipValidator.Validate(clip,clip,samples));samples[id]=good[..^1];Reject(()=>EncodedClipValidator.Validate(clip,clip,samples));samples[id]=good;
    string unanimated=clip.Curves[0].Bone+":6";samples[unanimated][3]=.02f;Reject(()=>EncodedClipValidator.Validate(clip,clip,samples));samples[unanimated][3]=0;
    var decoded=ProjectStore.Clone(clip);decoded.Loop=!decoded.Loop;Reject(()=>EncodedClipValidator.Validate(clip,decoded,samples));var oversized=ProjectStore.Clone(clip);oversized.FrameCount=100000;Reject(()=>EncodedClipValidator.CheckBudget(oversized));
}));

if(args.Contains("--native"))
{
    var backend=new NativeBackend(Path.Combine(Environment.CurrentDirectory,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),Path.Combine(Environment.CurrentDirectory,".local/jobs"));
    await Test("M2 genuine MDL0 model + CHR0 import/edit/project/export/decode",async()=>
    {
        string mdl=Path.Combine(output,"original.mdl0"),chr=Path.Combine(output,"original.chr0"),edit=Path.Combine(output,"edited.chr0");
        await backend.GenerateModelFixture(demo,mdl);await backend.ExportClip(demo,demo.Clips[2],chr);
        Assert(System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(mdl),0,4)=="MDL0");Assert(System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(chr),0,4)=="CHR0");
        var before=SHA256.HashData(File.ReadAllBytes(mdl));var motionBefore=SHA256.HashData(File.ReadAllBytes(chr));
        var p=await backend.ImportModel(mdl);Assert(p.Bones.Count==demo.Bones.Count);Assert(p.Meshes.Sum(m=>m.Indices.Count)==demo.Meshes.Sum(m=>m.Indices.Count));
        foreach(var bone in p.Bones){var reference=demo.Bones.Single(b=>b.Name==bone.Name);for(int i=0;i<16;i++)Near(bone.InverseBind[i],reference.InverseBind[i],.002f);}
        var nativePositions=p.Meshes.SelectMany(m=>m.Vertices).Select(v=>Animation.V(v.Position)).ToArray();
        foreach(var v in demo.Meshes.SelectMany(m=>m.Vertices))Assert(nativePositions.Any(n=>Vector3.Distance(n,Animation.V(v.Position))<.01f),"Imported mesh differs from original fixture");
        for(int mi=0;mi<demo.Meshes.Count;mi++)foreach(var v in demo.Meshes[mi].Vertices)
        {
            var candidate=p.Meshes[mi].Vertices.FirstOrDefault(n=>Vector3.Distance(Animation.V(n.Position),Animation.V(v.Position))<.01f&&Math.Abs(n.UV[0]-v.UV[0])<.001f&&Math.Abs(n.UV[1]-v.UV[1])<.001f);
            Assert(candidate!=null,"Native UV or vertex mismatch");Near(Vector3.Distance(Animation.V(candidate!.Normal),Animation.V(v.Normal)),0,.001f);
            for(int wi=0;wi<v.Bones.Length;wi++)if(v.Weights[wi]>0){int nativeBone=p.Bones.FindIndex(b=>b.Name==demo.Bones[v.Bones[wi]].Name);int influence=Array.IndexOf(candidate.Bones,nativeBone);Assert(influence>=0,"Missing native influence");Near(candidate.Weights[influence],v.Weights[wi],.0001f);}
        }
        Assert(p.Meshes.SelectMany(m=>m.Vertices).Any(v=>v.Weights.Count(w=>w>0)>1));
        p.Clips.Add(await backend.ImportClip(p,chr));var baseline=Animation.World(p,p.Clips[0],30);
        var session=new EditSession(p);string boneName=demo.Clips[2].Curves[0].Bone;session.EditKey(0,boneName,4,15,-42);
        string saved=Path.Combine(output,"native.fpproj");ProjectStore.Save(session.Project,saved);p=ProjectStore.Load(saved);
        Assert(p.Resources.Count==2);await backend.ExportClip(p,p.Clips[0],edit);
        var decoded=await backend.ImportClip(p,edit);Near(Animation.Evaluate(decoded.Curves.Single(c=>c.Bone==boneName&&c.Channel==4),15),-42,.05f);
        var samples=await backend.Samples(edit);float max=0;
        foreach(var c in p.Clips[0].Curves)for(int i=0;i<samples[c.Bone+":"+c.Channel].Length;i++)max=Math.Max(max,Math.Abs(Animation.Evaluate(c,i*.25f)-samples[c.Bone+":"+c.Channel][i]));
        Assert(max<.15f,$"Decoded interpolation error {max}");Console.WriteLine($"  Native measured quarter-frame scalar error={max:R}; model={new FileInfo(mdl).Length} bytes; clip={new FileInfo(edit).Length} bytes");
        Assert(before.SequenceEqual(SHA256.HashData(File.ReadAllBytes(mdl)))&&motionBefore.SequenceEqual(SHA256.HashData(File.ReadAllBytes(chr))),"Source changed");
        var moving=p.Meshes.SelectMany(m=>m.Vertices).Any(v=>Vector3.Distance(Animation.Skin(v,p,Animation.World(p,null,0)).Position,Animation.Skin(v,p,baseline).Position)>5);Assert(moving,"Imported native playback is constant");
    });
    await Test("M2 native winding, loop, scale and tangents round trip",async()=>
    {
        foreach(int index in new[]{0,5,7,8})
        {
            string path=Path.Combine(output,$"fixture-{index}.chr0");await backend.ExportClip(demo,demo.Clips[index],path);var decoded=await backend.ImportClip(demo,path);var samples=await backend.Samples(path);
            Assert(decoded.FrameCount==demo.Clips[index].FrameCount&&decoded.Loop==demo.Clips[index].Loop);
            foreach(var c in demo.Clips[index].Curves)for(int i=0;i<samples[c.Bone+":"+c.Channel].Length;i++)Near(samples[c.Bone+":"+c.Channel][i],Animation.Evaluate(c,i*.25f),.15f);
            if(index==5)Near(Animation.Evaluate(decoded.Curves.Single(c=>c.Channel==4),45),540,.05f);
        }
    });
    await Test("M2 deterministic native model bytes and rotated alternate bind conversion",async()=>
    {
        string a=Path.Combine(output,"repeat-a.mdl0"),b=Path.Combine(output,"repeat-b.mdl0");await backend.GenerateModelFixture(demo,a);await backend.GenerateModelFixture(demo,b);Assert(File.ReadAllBytes(a).SequenceEqual(File.ReadAllBytes(b)),"Native fixture generation is nondeterministic");
        var alternate=Demo.Create(1);string path=Path.Combine(output,"orion.mdl0");await backend.GenerateModelFixture(alternate,path);var decoded=await backend.ImportModel(path);var points=decoded.Meshes.SelectMany(m=>m.Vertices).Select(v=>Animation.V(v.Position)).ToArray();foreach(var v in alternate.Meshes.SelectMany(m=>m.Vertices))Assert(points.Any(n=>Vector3.Distance(n,Animation.V(v.Position))<.02f),"Rotated native fixture differs");
    });
    await Test("M2 native rejects corrupt input and unsupported step export",async()=>
    {
        string bad=Path.Combine(output,"bad.mdl0");File.WriteAllBytes(bad,new byte[100]);bool rejected=false;try{await backend.ImportModel(bad);}catch(InvalidDataException){rejected=true;}Assert(rejected);
        rejected=false;try{await backend.ExportClip(demo,demo.Clips[6],Path.Combine(output,"hold.chr0"));}catch(InvalidDataException){rejected=true;}Assert(rejected);Assert(!File.Exists(Path.Combine(output,"hold.chr0")));
    });
}
else Console.WriteLine("SKIP native integration: pass --native after building the worker.");
if(args.Contains("--native"))await WorkspaceTests.Run(output,Test);
await WorkerFailureTests.Run(output,Test,args.Contains("--native"));
await SemanticTests.Run(output,Test);
await QuaternionRetargetTests.Run(output,Test);
if(args.Contains("--ultimate"))await UltimateTests.Run(output,Test,args.Contains("--native"));
else Console.WriteLine("SKIP optional Ultimate backend: pass --ultimate with pinned local Python dependencies installed");
if(args.Contains("--local-assets")){await LocalReferenceTests.Run(output,Test);await WorkspaceTests.RunLocal(output,Test);}
Console.WriteLine($"RESULT {passed} passed; {failed} failed; output {output}");
File.WriteAllText(Path.Combine(output,"result.txt"),$"{passed} passed; {failed} failed; native={args.Contains("--native")}");
return failed==0?0:1;

static float[] Inverse(Matrix4x4 m){Matrix4x4.Invert(m,out var i);return Animation.A(i);}
