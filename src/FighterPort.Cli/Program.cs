using FighterPort.Core;
using System.Text.Json;

try
{
    string root=Environment.CurrentDirectory;
    var backend=new NativeBackend(Path.Combine(root,"src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),Path.Combine(root,".local/jobs"));
    if(args.Length==0) { Console.WriteLine("FighterPort: demo <project> | inspect <project> | native-fixture <new-dir> | import <model.mdl0> <clip.chr0> <project> | export <project> <clip-index> <new.chr0> | doctor");return 0; }
    switch(args[0])
    {
        case "demo": ProjectStore.Save(Demo.Create(),args[1]);Console.WriteLine("Original character saved.");break;
        case "inspect":
            var p=ProjectStore.Load(args[1]);Console.WriteLine(JsonSerializer.Serialize(new{p.Name,Bones=p.Bones.Count,Meshes=p.Meshes.Count,Clips=p.Clips.Select(c=>new{c.Name,c.FrameCount,c.Loop}),p.TargetProfile,p.Limitations},ProjectStore.Json));break;
        case "native-fixture":
            string dir=Path.GetFullPath(args[1]);if(Directory.Exists(dir))throw new IOException("Use a new fixture directory");Directory.CreateDirectory(dir);
            var original=Demo.Create();await backend.GenerateModelFixture(original,Path.Combine(dir,"aster.mdl0"));
            await backend.ExportClip(original,original.Clips.First(c=>c.Name=="Punch"),Path.Combine(dir,"punch.chr0"));
            await backend.ExportClip(original,original.Clips.First(c=>c.Name=="Full spin"),Path.Combine(dir,"spin.chr0"));
            var imported=await backend.ImportModel(Path.Combine(dir,"aster.mdl0"));imported.Clips.Add(await backend.ImportClip(imported,Path.Combine(dir,"punch.chr0")));ProjectStore.Save(imported,Path.Combine(dir,"native.fpproj"));Console.WriteLine("Genuine BrawlLib MDL0 v11 and CHR0 v4 fixtures generated and imported.");break;
        case "import":
            var model=await backend.ImportModel(args[1]);model.Clips.Add(await backend.ImportClip(model,args[2]));ProjectStore.Save(model,args[3]);Console.WriteLine("Model and exact-name clip bindings imported; source bytes retained.");break;
        case "export":
            var project=ProjectStore.Load(args[1]);await backend.ExportClip(project,project.Clips[int.Parse(args[2])],args[3]);Console.WriteLine("CHR0 written. Native-tool check remains separate.");break;
        case "doctor":
            Console.WriteLine($"Runtime: {Environment.Version}; Windows: {OperatingSystem.IsWindows()}; Native worker: {File.Exists(backend.WorkerPath)}; No AI/cloud runtime. DCC discovery is pending; DCCs are optional.");break;
        default: throw new ArgumentException("Unknown command");
    }
    return 0;
}
catch(Exception e){Console.Error.WriteLine(e.Message);return 1;}
