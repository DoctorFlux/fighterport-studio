using FighterPort.Core;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;

internal static class LocalReferenceTests
{
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test)
    {
        // Explicit opt-in. Configuration, game files, derived projects/reports never enter portable CI.
        var paths=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(".local/assets.json"))!;
        string worker=Path.GetFullPath("src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe");var native=new NativeBackend(worker,Path.Combine(output,"jobs"));
        var ultimate=new UltimateBackend("python",Path.GetFullPath("scripts/ssbh-worker.py"),Path.Combine(output,"ssbh-jobs"));
        await test("LOCAL Brawl PAC model/idle real skin, backend world poses, save/reopen/export",async()=>
        {
            string model=Path.Combine(paths["brawl"],"FitMario00.pac"),motion=Path.Combine(paths["brawl"],"FitMarioMotionEtc.pac");
            var models=await native.BrowseBundle(model);var clips=await native.BrowseBundle(motion);
            var main=models.Single(m=>m.Kind=="MDL0"&&m.Name=="FitMario00");var idle=clips.Single(c=>c.Kind=="CHR0"&&c.Name=="Wait1");
            var p=await native.ImportBundleModel(model,main.Id);p.Clips.Add(await native.ImportBundleClip(p,motion,idle.Id));
            var process=new WorkerProcess(worker,Path.Combine(output,"pose-jobs"));float max=0;
            foreach(float frame in new[]{0,.25f,12.5f,27.75f})
            {
                string request=Path.Combine(output,"pose-request.json");File.WriteAllText(request,JsonSerializer.Serialize(new BundlePoseRequest{ModelPath=model,ModelId=main.Id,ClipPath=motion,ClipId=idle.Id,Frame=frame},ProjectStore.Json));
                var backend=JsonSerializer.Deserialize<Dictionary<string,float[]>>(await process.Run("sample-bundle-pose",request))!;var canonical=Animation.World(p,p.Clips[0],frame);
                for(int i=0;i<p.Bones.Count;i++)foreach(var pair in Animation.A(canonical[i]).Zip(backend[p.Bones[i].Name]))max=Math.Max(max,Math.Abs(pair.First-pair.Second));
            }
            if(max>.003f)throw new Exception("BrawlLib/canonical world matrix discrepancy: "+max);
            VerifyMotion(p);string save=Path.Combine(output,"local-brawl.fpproj");ProjectStore.Save(p,save);p=ProjectStore.Load(save);VerifyMotion(p);
            await native.ExportClip(p,p.Clips[0],Path.Combine(output,"local-brawl-export.chr0"));Console.WriteLine($"  Brawl: {p.Bones.Count} bones / {p.Meshes.Sum(m=>m.Vertices.Count)} vertices; max world matrix element error={max:R}");
        });
        await test("LOCAL Ultimate complete geometry/rig + idle skeletal playback and guarded native write",async()=>
        {
            string model=Path.Combine(paths["ultimate"],"model/body/c00/model.numdlb"),motion=Path.Combine(paths["ultimate"],"motion/body/c00/a00wait1.nuanmb");
            var p=await ultimate.ImportModel(model);p.Clips.Add(await ultimate.ImportClip(p,motion));VerifyMotion(p);
            string save=Path.Combine(output,"local-ultimate.fpproj");ProjectStore.Save(p,save);p=ProjectStore.Load(save);VerifyMotion(p);
            bool rejected=false;try{await ultimate.ExportClip(p,p.Clips[0],Path.Combine(output,"must-not-publish.nuanmb"));}catch(InvalidDataException){rejected=true;}if(!rejected||File.Exists(Path.Combine(output,"must-not-publish.nuanmb")))throw new Exception("Ancillary loss was not gated");
            // No automatic loss approval for the supplied clip. User can explicitly approve in the desktop.
            Console.WriteLine($"  Ultimate: {p.Bones.Count} bones / {p.Meshes.Sum(m=>m.Vertices.Count)} vertices / {p.Clips[0].TransformTracks.Count} transform tracks; ancillary export blocked as intended");
        });
        await test("LOCAL every supplied source hash unchanged",()=>
        {
            var manifest=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(".local/references/continuation-input-hashes.json"))!;
            foreach(var pair in manifest)if(!Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(pair.Key))).Equals(pair.Value,StringComparison.OrdinalIgnoreCase))throw new Exception("Source hash changed");
            Console.WriteLine($"  {manifest.Count} source hashes unchanged");return Task.CompletedTask;
        });
    }
    private static void VerifyMotion(CharacterProject p)
    {
        var rest=Animation.World(p,null,0);var first=Animation.World(p,p.Clips[0],0);var next=Animation.World(p,p.Clips[0],12.5f);
        float maxRest=0,maxMotion=0;
        foreach(var v in p.Meshes.SelectMany(m=>m.Vertices))
        {
            maxRest=Math.Max(maxRest,Vector3.Distance(Animation.Skin(v,p,rest).Position,Animation.V(v.Position)));
            maxMotion=Math.Max(maxMotion,Vector3.Distance(Animation.Skin(v,p,first).Position,Animation.Skin(v,p,next).Position));
        }
        if(maxRest>.01f||!float.IsFinite(maxMotion)||maxMotion<.0001f)throw new Exception($"Skin check failed rest={maxRest}, motion={maxMotion}");
        Console.WriteLine($"  Rest vertex error={maxRest:R}; fractional displacement={maxMotion:R}");
    }
}
