using FighterPort.Core;
using System.Text.Json;

internal static class PaletteTests
{
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test)
    {
        string worker=Path.GetFullPath("src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),jobs=Path.GetFullPath(".local/jobs");var transport=new WorkerProcess(worker,jobs);
        ResourceWorkspace Workspace()=>new(worker,jobs,"scripts/ssbh-worker.py");
        await test("M3.D1 native CI4 non-square alpha/mips, independent palette values and probe bounds",async()=>
        {
            string request=Path.Combine(output,"ci4-request.json"),path=Path.Combine(output,"ci4.pac");File.WriteAllText(request,"{}");File.WriteAllBytes(path,await transport.Run("fixture-workspace-ci4",request));
            var ws=Workspace();var doc=await ws.Open(path);var nodes=await WorkspaceTests.Inventory(ws,doc);var tex=nodes.Single(n=>n.Name=="Indexed4Alpha"&&n.Kind=="TEX0");
            Check(tex.PixelFormat=="CI4"&&tex.Width==32&&tex.Height==16&&tex.MipCount==2,"CI4 fixture metadata");
            foreach(int mip in new[]{0,1})
            {
                int w=32>>mip,h=16>>mip;
                var requestData=new BundleRequest{Path=path,Id=tex.Id,Mip=mip,ProbePoints=[[w/4,h/4],[3*w/4,h/4],[w/4,3*h/4],[3*w/4,3*h/4],[w-1,h-1]]};File.WriteAllText(request,JsonSerializer.Serialize(requestData,ProjectStore.Json));
                var decoded=JsonSerializer.Deserialize<Probe>(await transport.Run("resource-texture-probe",request),ProjectStore.Json)!;
                Check(decoded.Width==w&&decoded.Height==h&&decoded.Pixels.SequenceEqual(new[]{unchecked((int)0xffff0000),unchecked((int)0xff00ff00),unchecked((int)0xff0000ff),unchecked((int)0x6dff00ff),0}),"CI4 nibble order/RGB5A3 alpha/mip colors");
                File.WriteAllBytes(Path.Combine(output,$"ci4-mip{mip}.png"),await ws.Texture(new(){DocumentId=doc.Id,NodeId=tex.Id},mip));
            }
            await Reject(()=>ws.Texture(new(){DocumentId=doc.Id,NodeId=tex.Id},2));
            File.WriteAllText(request,JsonSerializer.Serialize(new BundleRequest{Path=path,Id=tex.Id,ProbePoints=[[32,0]]},ProjectStore.Json));await Reject(()=>transport.Run("resource-texture-probe",request));ResourceWorkspace.Verify(doc);
        });
        await test("M3.D1 CI4 ambiguous/missing palette rejects; explicit native handle override persists",async()=>
        {
            string path=Path.Combine(output,"ci4.pac"),request=Path.Combine(output,"ci4-ambiguous.json");var ws=Workspace();ws.Load(Path.Combine(output,"workspace.fpwork"));var doc=await ws.Open(path);var nodes=await WorkspaceTests.Inventory(ws,doc);
            var texture=nodes.Single(n=>n.Name=="Indexed4Ambiguous"&&n.Kind=="TEX0");var palettes=nodes.Where(n=>n.Name==texture.Name&&n.Kind=="PLT0").ToArray();Check(palettes.Length==2,"Native ambiguity fixture lost a palette");var handle=new AssetHandle{DocumentId=doc.Id,NodeId=texture.Id};
            await Reject(()=>ws.Texture(handle));await Reject(()=>ws.Texture(new(){DocumentId=doc.Id,NodeId=nodes.Single(n=>n.Name=="Indexed4Missing").Id}));await Reject(()=>ws.Texture(handle,palette:texture.Id));
            for(int i=0;i<palettes.Length;i++)
            {
                File.WriteAllText(request,JsonSerializer.Serialize(new BundleRequest{Path=path,Id=texture.Id,PaletteId=palettes[i].Id},ProjectStore.Json));var probe=JsonSerializer.Deserialize<Probe>(await transport.Run("resource-texture-probe",request),ProjectStore.Json)!;Check(probe.Pixels[0]==unchecked((int)(i==0?0xffff0000:0xffffff00)),"Explicit palette handle chose wrong bank");
            }
            var material=ws.Data.Project.Materials[0];ws.OverrideTexture(material.Id,0,handle,palettes[1].Id);string save=Path.Combine(output,"ci4-override.fpwork");ws.Save(save);var reopened=Workspace();reopened.Load(save);var binding=reopened.Data.Project.Materials[0].Textures.Single(t=>t.Slot==0);Check(binding.PaletteId==palettes[1].Id&&binding.Texture.DocumentId==doc.Id,"CI4 override persistence");Check((await reopened.Texture(binding.Texture,palette:binding.PaletteId)).Length>8,"Persisted palette failed decode");foreach(var source in reopened.Data.Documents)ResourceWorkspace.Verify(source);
        });
    }
    private sealed class Probe{public int Width{get;set;}public int Height{get;set;}public int[] Pixels{get;set;}=[];}
    private static async Task Reject(Func<Task<byte[]>> action){try{await action();}catch(InvalidDataException){return;}throw new Exception("Unsafe palette/probe request accepted");}
    private static void Check(bool condition,string why){if(!condition)throw new Exception(why);}
}
