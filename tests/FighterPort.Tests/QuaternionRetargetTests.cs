using FighterPort.Core;
using System.Numerics;
using System.Text.Json;

internal static class QuaternionRetargetTests
{
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test)
    {
        void Near(Matrix4x4 a,Matrix4x4 b){if(Animation.A(a).Zip(Animation.A(b)).Any(x=>Math.Abs(x.First-x.Second)>.0002f))throw new Exception("Independent quaternion target pose mismatch");}
        void Reject(Action a){try{a();}catch(InvalidDataException){return;}throw new Exception("Unsupported retarget accepted");}
        (CharacterProject s,CharacterProject t,MappingProfile m) Fixture()
        {
            var s=new CharacterProject{Bones=[new(){Id="root",Name="root"},new(){Id="arm",Name="arm",Parent=0,Translation=[0,2,0]}]};Animation.SetInverseBinds(s);
            var q=Quaternion.CreateFromAxisAngle(Vector3.UnitZ,MathF.PI/2);
            s.Clips.Add(new(){FrameCount=2,FramesPerSecond=60,SkeletonFingerprint=Animation.Fingerprint(s),TransformTracks=[new(){Bone="arm",Samples=[new(){Translation=[0,2,0]},new(){Translation=[0,2,0],Rotation=[q.X,q.Y,q.Z,q.W]}]}]});
            var t=new CharacterProject{Bones=[new(){Id="tr",Name="tr",Rotation=[0,30,0]},new(){Id="helper",Name="helper",Parent=0,Translation=[0,3,0],Rotation=[15,0,0]},new(){Id="ta",Name="ta",Parent=1,Translation=[0,4,0],Rotation=[0,0,20]}]};Animation.SetInverseBinds(t);
            var m=new MappingProfile{Approved=true,SourceFingerprint=Animation.Fingerprint(s),TargetFingerprint=Animation.Fingerprint(t),BoneIds=new(){{"root","tr"},{"arm","ta"}}};return(s,t,m);
        }
        await test("M3.A2 native quaternion changed-axis/parent fractional transfer, exact source and reopen",()=>
        {
            var(s,t,m)=Fixture();s.Clips[0].Losses.Add("Visibility: 1 native nodes retained opaque; not evaluated");string before=JsonSerializer.Serialize(s);var r=RestPoseRetarget.Transfer(s,0,t,m);if(!r.Clips.Last().Losses.SequenceEqual(s.Clips[0].Losses))throw new Exception("Ancillary loss guard lost during retarget");var rest=Animation.World(t,null,0);var expected=rest[2];expected.Translation=Vector3.Zero;expected*=Matrix4x4.CreateRotationZ(MathF.PI/4);expected.Translation=rest[2].Translation;
            Near(Animation.World(r,r.Clips.Last(),.5f)[2],expected);Near(Animation.World(r,r.Clips.Last(),0)[2],rest[2]);Near(Animation.World(r,r.Clips.Last(),.5f)[1],rest[1]);
            var path=Path.Combine(output,"quaternion-retarget.fpproj");ProjectStore.Save(r,path);var reopened=ProjectStore.Load(path);Near(Animation.World(reopened,reopened.Clips.Last(),.5f)[2],expected);
            if(before!=JsonSerializer.Serialize(s)||JsonSerializer.Serialize(t.Meshes)!=JsonSerializer.Serialize(r.Meshes))throw new Exception("Input/target changed");return Task.CompletedTask;
        });
        await test("M3.A2 translation basis, explicit approval, unit scales and unmapped helpers",()=>
        {
            var(s,t,m)=Fixture();s.Clips[0].TransformTracks[0].Samples[1].Translation=[2,2,0];Reject(()=>RestPoseRetarget.Transfer(s,0,t,m));
            m.NonRootTranslation="RestParentDelta";var r=RestPoseRetarget.Transfer(s,0,t,m);var actual=Animation.World(r,r.Clips.Last(),.5f)[2];var rest=Animation.World(t,null,0)[2];if(Vector3.Distance(actual.Translation,rest.Translation+Vector3.UnitX)>.0001f)throw new Exception("Translation was not transferred through parent rest basis");
            var session=new MappingSession(s,t);session.LoadProfile(m);session.SetNonRootPolicy("Reject");if(session.Profile.Approved)throw new Exception("Policy change retained approval");
            s.Clips[0].TransformTracks[0].Samples[1].Scale=[1,1.2f,1];Reject(()=>RestPoseRetarget.Transfer(s,0,t,m));s.Clips[0].TransformTracks[0].Samples[1].Scale=[1,1,1];m.BoneIds.Remove("arm");Reject(()=>RestPoseRetarget.Transfer(s,0,t,m));
            return Task.CompletedTask;
        });
        await test("M3.A2 exact native rest matrices, root delta and unsupported policy rejection",()=>
        {
            var(s,t,m)=Fixture();foreach(var b in s.Bones)b.RestMatrix=Animation.A(Animation.RestLocal(b));
            s.Clips[0].TransformTracks.Add(new(){Bone="root",Samples=[new(),new(){Translation=[0,0,2]}]});Reject(()=>RestPoseRetarget.Transfer(s,0,t,m));m.RootTranslation="CopyDelta";
            var r=RestPoseRetarget.Transfer(s,0,t,m);var pose=Animation.World(r,r.Clips.Last(),.5f);if(Vector3.Distance(pose[0].Translation,Vector3.UnitZ)>.0001f)throw new Exception("Root delta lost");
            m.HelperPolicy="Simulate";Reject(()=>RestPoseRetarget.Transfer(s,0,t,m));return Task.CompletedTask;
        });
    }
}
