using FighterPort.Core;
using System.Text.Json;

internal static class MaterialTests
{
    internal static async Task Run(string output,Func<string,Func<Task>,Task> test)
    {
        await test("M3.D1 genuine native TEV stages, separate sampler filtering, constants and alpha reference",async()=>
        {
            string worker=Path.GetFullPath("src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe"),jobs=Path.GetFullPath(".local/jobs"),request=Path.Combine(output,"material-fixture.json"),path=Path.Combine(output,"material.pac");
            var transport=new WorkerProcess(worker,jobs);File.WriteAllText(request,JsonSerializer.Serialize(new BundlePoseRequest{ModelPath=Path.Combine(output,"workspace.mdl0")},ProjectStore.Json));File.WriteAllBytes(path,await transport.Run("fixture-workspace-material",request));
            var ws=new ResourceWorkspace(worker,jobs,"scripts/ssbh-worker.py");var doc=await ws.Open(path);var inventory=await WorkspaceTests.Inventory(ws,doc);await ws.ActivateModel(new(){DocumentId=doc.Id,NodeId=inventory.Single(e=>e.Kind=="MDL0").Id});var material=ws.Data.Project.Materials[0];MaterialSurface.Validate(material);
            Check(material.Program.ActiveStages==3&&material.Program.Stages.Count==3&&material.Program.Constants[0].SequenceEqual(new[]{255,128,64,192}),"Native stage/constant metadata lost");
            var images=new Dictionary<int,MaterialImage>();foreach(var binding in material.Textures)images[binding.Slot]=await ws.TexturePixels(binding.Texture,binding.PaletteId);
            var pixels=MaterialSurface.Render(material,images,128,128);
            foreach(int y in new[]{1,31,63,64,95,126})foreach(int x in new[]{1,31,63,64,95,126})
            {
                double u=(x+.5)/128,v=(y+.5)/128;double[] a=Expected(0,u,v),b=Expected(1,u,v);double[] k=[1,128/255.0,64/255.0,192/255.0];for(int c=0;c<4;c++)Check(Math.Abs(pixels[(y*128+x)*4+c]-(int)Math.Round(a[c]*b[c]*k[c]*255))<=1,"Independent native-stage reference mismatch / textures composed before filtering");
            }
            string saved=Path.Combine(output,"native-material.fpwork");ws.Save(saved);var reopened=new ResourceWorkspace(worker,jobs,"scripts/ssbh-worker.py");reopened.Load(saved);Check(JsonSerializer.Serialize(material.Program,ProjectStore.Json)==JsonSerializer.Serialize(reopened.Data.Project.Materials[0].Program,ProjectStore.Json),"Native stage preservation on save");
            var nearest=ProjectStore.Clone(material);foreach(var binding in nearest.Textures)binding.Filter="Nearest";var nearestPixels=MaterialSurface.Render(nearest,images,128,128);double[] nearA=Expected(0,63.5/128,32.5/128,true),nearB=Expected(1,63.5/128,32.5/128,true);Check(nearestPixels[(32*128+63)*4]==(byte)Math.Round(nearA[0]*nearB[0]*255)&&nearestPixels[(32*128+63)*4]!=pixels[(32*128+63)*4],"Reference nearest filtering was silently linear");
            var arithmetic=ProjectStore.Clone(material);var final=arithmetic.Program.Stages[2];final.ColorOperation=1;final.Color[3]=12;final.ColorBias=1;final.ColorScale=3;
            var result=MaterialSurface.Pixel(arithmetic,(slot,_,_)=>slot==0?[.25f,.5f,.75f,.8f]:[.5f,.25f,1,.5f],.25f,.25f);Check(Math.Abs(result[0]-.6875f)<.00001f&&Math.Abs(result[1]-(.75f-.5f*.5f*.25f*128/255))<.00001f,"TEV subtract/bias/scale arithmetic");
            for(int compare=0;compare<8;compare++)
            {
                var alpha=ProjectStore.Clone(material);alpha.Program.Constants[0][3]=255;alpha.Program.Compare0=compare;alpha.Program.Reference0=128;
                var sample=MaterialSurface.Pixel(alpha,(slot,_,_)=>slot==0?[1,1,1,128/255f]:[1,1,1,1],0,0);bool pass=compare is 2 or 3 or 6 or 7;Check((sample[3]>0)==pass,"Alpha comparison boundary "+compare);
            }
            var edited=ProjectStore.Clone(material);edited.Program.Compare0=4;edited.Program.Reference0=100;var tested=MaterialSurface.Render(edited,images,128,128);Check(tested[(16*128+16)*4+3]==0&&tested[(100*128+16)*4+3]==192,"Native alpha-test source-alpha behavior");
            edited.Program.Blend=false;var opaque=MaterialSurface.Render(edited,images,128,128);Check(opaque[(100*128+16)*4+3]==255&&opaque[(16*128+16)*4+3]==0,"Opaque alpha/discard behavior");
            foreach(int variant in new[]{0,1,2,3,4})
            {
                var unsupported=ProjectStore.Clone(material);switch(variant){case 0:unsupported.Program.Stages[1].ColorOperation=8;break;case 1:unsupported.Program.Stages[1].NativeIndirect=1;break;case 2:unsupported.Program.Stages[1].TextureSwap=1;break;case 3:unsupported.Textures[1].Projection="STQ";break;case 4:unsupported.Program.Stages[1].Color[1]=10;break;}
                string before=JsonSerializer.Serialize(unsupported,ProjectStore.Json);bool rejected=false;try{MaterialSurface.Render(unsupported,images,32,32);}catch(InvalidDataException){rejected=true;}Check(rejected&&before==JsonSerializer.Serialize(unsupported,ProjectStore.Json),"Unsupported material was approximated or mutated");
            }
            bool missing=false;try{MaterialSurface.Render(material,new Dictionary<int,MaterialImage>{{0,images[0]}},128,128);}catch(InvalidDataException){missing=true;}Check(missing,"Missing sampler substituted");
            var minified=ProjectStore.Clone(material);minified.Textures[0].RestSrt[0]=8;bool rejectedLod=false;try{MaterialSurface.Render(minified,images,128,128);}catch(InvalidDataException){rejectedLod=true;}Check(rejectedLod,"Scaled minification silently used magnification filter");ResourceWorkspace.Verify(doc);
        });
    }
    private static double[] Expected(int slot,double u,double v,bool nearest=false)
    {
        // Independent fixture texel formulas; source pixels are not used to derive expected values.
        double x=u*32-.5,y=v*32-.5;int ix=(int)Math.Floor(nearest?u*32:x),iy=(int)Math.Floor(nearest?v*32:y);double fx=nearest?0:x-ix,fy=nearest?0:y-iy;double[] value=new double[4];
        for(int dy=0;dy<2;dy++)for(int dx=0;dx<2;dx++)
        {
            int xx=((ix+dx)%32+32)%32,yy=((iy+dy)%32+32)%32;double[] c=slot==0?(xx<16?(yy<16?[255,0,0,255]:[0,0,255,255]):(yy<16?[0,255,0,255]:[255,255,0,255])):[xx<16?128:255,yy<16?64:192,255-xx*4,yy<16?128:255];
            for(int channel=0;channel<4;channel++)value[channel]+=c[channel]/255*(dx==0?1-fx:fx)*(dy==0?1-fy:fy);
        }
        return value;
    }
    private static void Check(bool condition,string why){if(!condition)throw new Exception(why);}
}
