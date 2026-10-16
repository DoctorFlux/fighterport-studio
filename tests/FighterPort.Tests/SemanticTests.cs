using FighterPort.Core;
using System.Numerics;
using System.Text.Json;

internal static class SemanticTests
{
    private static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
    private static void Near(Matrix4x4 a,Matrix4x4 b){Check(Animation.A(a).Zip(Animation.A(b)).All(x=>Math.Abs(x.First-x.Second)<.001f),"Matrix mismatch");}
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test)
    {
        Task Sync(Action a){a();return Task.CompletedTask;}
        var source=Demo.Create();
        await test("M3.A semantic aliases, renamed topology, toes/fingers, correction and stale profiles",()=>Sync(()=>
        {
            var a=Anatomy.Suggest(source);var renamed=Demo.Create(1);var b=Anatomy.Suggest(renamed);
            Check(a.Bones.Single(x=>x.BoneId==source.Bones.Single(b=>b.Name=="LeftToe").Id).Role=="toes.base","Toe alias");
            Check(b.Bones.Count(x=>x.Role=="toes.base")==2&&b.Bones.Count(x=>x.Role=="hand")==2,"Unnamed topology");
            Check(b.Bones.Any(x=>x.Role=="finger.unknown"&&x.Alternatives.Count==5),"Uncertain digit names must remain unresolved");
            a.Approved=true;var id=source.Bones[0].Id;Anatomy.Correct(a,id,"motion-root","center","motion",0);Check(!a.Approved&&a.Bones[0].Locked,"Correction must revoke approval");
            string file=Path.Combine(output,"anatomy.json");ProjectStore.AtomicWrite(file,JsonSerializer.SerializeToUtf8Bytes(a,ProjectStore.Json));Anatomy.Validate(JsonSerializer.Deserialize<AnatomyProfile>(File.ReadAllText(file),ProjectStore.Json)!,source);
            bool rejected=false;try{Anatomy.Validate(a,renamed);}catch(InvalidDataException){rejected=true;}Check(rejected,"Stale profile accepted");
            var tail=Anatomy.Suggest(Demo.Create(2));Check(tail.Bones.Count(x=>x.Role=="tail")==5,"Extra chain lost");
            var missing=ProjectStore.Clone(source);int toe=missing.Bones.FindLastIndex(b=>b.Name=="RightToe");missing.Bones.RemoveAt(toe);missing.Meshes.Clear();missing.Clips.Clear();Animation.SetInverseBinds(missing);
            var map=Anatomy.Map(source,Anatomy.Suggest(source),missing,Anatomy.Suggest(missing));Check(!map.Approved&&!map.BoneIds.ContainsKey(source.Bones[toe].Id),"Missing toe silently mapped");
        }));
        await test("M3.A rest-delta identity and independently expected changed-parent rotation",()=>Sync(()=>
        {
            var s=new CharacterProject{Bones=[new(){Id="s0",Name="s0"},new(){Id="s1",Name="s1",Parent=0,Translation=[0,10,0]}]};Animation.SetInverseBinds(s);
            var c=new Clip{FrameCount=3,SkeletonFingerprint=Animation.Fingerprint(s),Curves=[new(){Bone="s1",Channel=5,Interpolation="Linear",Keys=[new(){Frame=0,Value=0},new(){Frame=2,Value=90}]}]};s.Clips.Add(c);
            var t=new CharacterProject{Bones=[new(){Id="t0",Name="t0",Rotation=[0,30,0]},new(){Id="helper",Name="helper",Parent=0,Translation=[0,4,0]},new(){Id="t1",Name="t1",Parent=1,Translation=[0,8,0],Rotation=[25,0,0]}]};Animation.SetInverseBinds(t);
            var profile=new MappingProfile{Approved=true,SourceFingerprint=Animation.Fingerprint(s),TargetFingerprint=Animation.Fingerprint(t),BoneIds=new(){{"s0","t0"},{"s1","t1"}}};var result=RestPoseRetarget.Transfer(s,0,t,profile);var clip=result.Clips.Last();
            var rest=Animation.World(t,null,0);var zero=Animation.World(result,clip,0);for(int i=0;i<rest.Length;i++)Near(rest[i],zero[i]);
            var actual=Animation.World(result,clip,.5f)[2];var expected=rest[2];expected.Translation=Vector3.Zero;expected*=Matrix4x4.CreateRotationZ(MathF.PI/8);expected.Translation=rest[2].Translation;Near(actual,expected);
            string path=Path.Combine(output,"rest-transfer.fpproj");ProjectStore.Save(result,path);var loaded=ProjectStore.Load(path);Near(Animation.World(loaded,loaded.Clips.Last(),.5f)[2],actual);
        }));
        await test("M3.A rotated asymmetric rig fractional skin, winding retained and input immutability",()=>Sync(()=>
        {
            var t=Demo.Create(1);t.Clips.Clear();t.Bones[9].Translation[0]*=1.17f;Animation.SetInverseBinds(t);var map=new MappingProfile{Approved=true,SourceFingerprint=Animation.Fingerprint(source),TargetFingerprint=Animation.Fingerprint(t),BoneIds=source.Bones.Select((b,i)=>(b,i)).ToDictionary(x=>x.b.Id,x=>t.Bones[x.i].Id)};
            string before=JsonSerializer.Serialize(t);var result=RestPoseRetarget.Transfer(source,5,t,map);Check(JsonSerializer.Serialize(t)==before,"Target mutated");Check(JsonSerializer.Serialize(t.Meshes)==JsonSerializer.Serialize(result.Meshes),"Target mesh changed");
            var c=result.Clips.Last();Check(Animation.Evaluate(c.Transfer.SourceClip.Curves[0],45)==540,"Source winding lost");
            foreach(float frame in new[]{0f,.25f,12.5f,29.75f,45f,60f}){var world=Animation.World(result,c,frame);Check(world.All(m=>Animation.A(m).All(float.IsFinite)),"Invalid target pose");Check(result.Meshes.SelectMany(m=>m.Vertices).Take(30).All(v=>float.IsFinite(Animation.Skin(v,result,world).Position.X)),"Invalid skin");}
            bool rejected=false;try{RestPoseRetarget.Transfer(source,7,t,map);}catch(InvalidDataException){rejected=true;}Check(rejected,"Scale silently accepted");
        }));
    }
}
