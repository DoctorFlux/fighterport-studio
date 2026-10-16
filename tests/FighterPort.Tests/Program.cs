using FighterPort.Core;
using System.Numerics;
using System.Security.Cryptography;

int passed=0,failed=0;
string output=Path.Combine(Environment.CurrentDirectory,"artifacts/tests",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(output);
void Assert(bool condition,string message="Assertion failed"){if(!condition)throw new Exception(message);}
void Near(float a,float b,float tolerance=.001f){Assert(Math.Abs(a-b)<=tolerance,$"Expected {b}, got {a}, tolerance {tolerance}");}
void Reject(Action action){try{action();}catch(InvalidDataException){return;}throw new Exception("Invalid input was accepted");}
async Task Test(string name,Func<Task> run){try{await run();Console.WriteLine("PASS "+name);passed++;}catch(Exception e){Console.WriteLine("FAIL "+name+": "+e);failed++;}}
Task Sync(Action a){a();return Task.CompletedTask;}
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
    await Test("M2 native rejects corrupt input and unsupported step export",async()=>
    {
        string bad=Path.Combine(output,"bad.mdl0");File.WriteAllBytes(bad,new byte[100]);bool rejected=false;try{await backend.ImportModel(bad);}catch(InvalidDataException){rejected=true;}Assert(rejected);
        rejected=false;try{await backend.ExportClip(demo,demo.Clips[6],Path.Combine(output,"hold.chr0"));}catch(InvalidDataException){rejected=true;}Assert(rejected);Assert(!File.Exists(Path.Combine(output,"hold.chr0")));
    });
}
else Console.WriteLine("SKIP native integration: pass --native after building the worker.");
Console.WriteLine($"RESULT {passed} passed; {failed} failed; output {output}");
File.WriteAllText(Path.Combine(output,"result.txt"),$"{passed} passed; {failed} failed; native={args.Contains("--native")}");
return failed==0?0:1;

static float[] Inverse(Matrix4x4 m){Matrix4x4.Invert(m,out var i);return Animation.A(i);}
