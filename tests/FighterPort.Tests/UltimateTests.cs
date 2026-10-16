using FighterPort.Core;
using System.Diagnostics;
using System.Numerics;

internal static class UltimateTests
{
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test,bool native)
    {
        var root=Environment.CurrentDirectory;var backend=new UltimateBackend("python",Path.Combine(root,"scripts/ssbh-worker.py"),Path.Combine(output,"ssbh-jobs"));
        string fixture=Path.Combine(output,"ssbh-original");
        await test("M5.B original genuine SSBH mesh/rig/clip import, rigid and blended fractional skin, export/decode",async()=>
        {
            var start=new ProcessStartInfo("python"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true,RedirectStandardOutput=true};start.ArgumentList.Add(Path.Combine(root,"scripts/generate-ultimate-fixture.py"));start.ArgumentList.Add(fixture);
            using var child=Process.Start(start)!;var stdout=child.StandardOutput.ReadToEndAsync();var stderr=child.StandardError.ReadToEndAsync();using var limit=new CancellationTokenSource(10000);try{await child.WaitForExitAsync(limit.Token);}finally{if(!child.HasExited)child.Kill(true);}if(child.ExitCode!=0)throw new Exception("Fixture generator failed: "+await stderr);Console.WriteLine("  "+(await stdout).Trim());
            var p=await backend.ImportModel(Path.Combine(fixture,"model.numdlb"));var clip=await backend.ImportClip(p,Path.Combine(fixture,"motion.nuanmb"));p.Clips.Add(clip);
            if(p.Bones.Count!=2||p.Meshes.Count!=2||p.Meshes.Sum(m=>m.Vertices.Count)!=6)throw new Exception("Fixture topology changed");
            void Reject(CharacterProject invalid){try{ProjectStore.Validate(invalid);}catch(InvalidDataException){return;}throw new Exception("Malformed native canonical data accepted");}
            var invalid=ProjectStore.Clone(p);invalid.Bones[1].RestMatrix[12]+=1;Reject(invalid);
            invalid=ProjectStore.Clone(p);invalid.Meshes[0].NativeAttributes.Values.First()[0][0]=float.NaN;Reject(invalid);
            var rest=Animation.World(p,null,0);foreach(var v in p.Meshes.SelectMany(m=>m.Vertices))if(Vector3.Distance(Animation.Skin(v,p,rest).Position,Animation.V(v.Position))>.0001)throw new Exception("Rest skin mismatch");
            var pose=Animation.World(p,clip,.5f);var actual=Animation.Skin(p.Meshes[0].Vertices[1],p,pose).Position;
            var expected=new Vector3(.25f+.75f*MathF.Sqrt(.5f),2+.75f*MathF.Sqrt(.5f),0);if(Vector3.Distance(actual,expected)>.0001)throw new Exception("Independent blended skin expectation failed");
            var rigid=Animation.Skin(p.Meshes[1].Vertices[1],p,pose).Position;if(Vector3.Distance(rigid,new(MathF.Sqrt(.5f),2+MathF.Sqrt(.5f),0))>.0001)throw new Exception("Rigid parenting mismatch");
            string saved=Path.Combine(output,"ssbh.fpproj");ProjectStore.Save(p,saved);p=ProjectStore.Load(saved);
            await backend.ExportClip(p,p.Clips[0],Path.Combine(output,"original-export.nuanmb"));
            var decoded=await backend.ImportClip(p,Path.Combine(output,"original-export.nuanmb"));if(Vector3.Distance(Animation.Skin(p.Meshes[0].Vertices[1],p,Animation.World(p,decoded,.5f)).Position,expected)>.001)throw new Exception("Decoded playback mismatch");
        });
        if(native)await test("M3/M5 original CHR0 to NUANMB and NUANMB to CHR0 measured bakes",async()=>
        {
            var brawl=new NativeBackend(Path.Combine(root,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),Path.Combine(output,"bake-jobs"));
            var p=await brawl.ImportModel(Path.Combine(output,"original.mdl0"));p.Clips.Add(await brawl.ImportClip(p,Path.Combine(output,"original.chr0")));
            var baked=NativeBake.Bake(p,0,true);await backend.ExportClip(baked.Project,baked.Project.Clips[baked.ClipIndex],Path.Combine(output,"brawl-to-ultimate.nuanmb"));
            var u=await backend.ImportModel(Path.Combine(fixture,"model.numdlb"));var fast=await backend.ImportClip(u,Path.Combine(fixture,"motion.nuanmb"));u.Clips.Add(fast);
            bool rejected=false;try{NativeBake.Bake(u,0,false,angleTolerance:.1f,distanceTolerance:.01f);}catch(InvalidDataException){rejected=true;}if(!rejected)throw new Exception("High-error quaternion to Euler bake accepted");
            var slow=ProjectStore.Clone(fast);for(int i=0;i<3;i++)slow.TransformTracks[0].Samples[i].Rotation=[0,0,MathF.Sin(i*MathF.PI/12),MathF.Cos(i*MathF.PI/12)];u.Clips.Add(slow);
            string slowFile=Path.Combine(output,"slow-original.nuanmb");await backend.ExportClip(u,slow,slowFile);u.Clips.Add(await backend.ImportClip(u,slowFile));
            var reverse=NativeBake.Bake(u,u.Clips.Count-1,false);await brawl.ExportClip(reverse.Project,reverse.Project.Clips[reverse.ClipIndex],Path.Combine(output,"ultimate-to-brawl.chr0"));
            var edit=new EditSession(reverse.Project);var originalPose=Animation.World(edit.Project,edit.Project.Clips[reverse.ClipIndex],1)[1];
            edit.EditKey(reverse.ClipIndex,"OriginalArm",5,1,45);
            if(Animation.World(edit.Project,edit.Project.Clips[reverse.ClipIndex],1)[1]==originalPose)throw new Exception("Baked scalar edit had no effect");
            edit.Undo();if(Animation.World(edit.Project,edit.Project.Clips[reverse.ClipIndex],1)[1]!=originalPose)throw new Exception("Baked scalar undo failed");
            Console.WriteLine($"  Original forward max angle={baked.MaxAngleDegrees:R}, vertex={baked.MaxVertexDistance:R}; reverse angle={reverse.MaxAngleDegrees:R}, vertex={reverse.MaxVertexDistance:R}");
        });
    }
}
