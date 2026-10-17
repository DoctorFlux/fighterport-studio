// Original generated intake regressions, FighterPort 2026-10-17. No game data.
using BrawlCrate.Authoring;
using BrawlCrate.Sources;
using BrawlLib.Imaging;
using BrawlLib.Internal.IO;
using BrawlLib.Internal;
using BrawlLib.Internal.Drawing;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Textures;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

internal static partial class NativeSmoke
{
    private static void CheckHumanoidAdmission(string original)
    {
        // Independently specified finite values whose reversed bit patterns are sNaNs.
        // The former float-backed endian storage quieted them on x86 (e.g. .25 degree loss).
        foreach(uint bits in new uint[]{0x3f819f7f,0xc3109fff,0x3f800000,0x80000000,0x7f7fffff,0x00800000,0x007fffff,0x00000001})
        {float value=BitConverter.ToSingle(BitConverter.GetBytes(bits),0);bfloat encoded=value;uint swapped=(bits>>24)|((bits>>8)&0xff00)|((bits<<8)&0xff0000)|(bits<<24);Assert(encoded._data==swapped && BitConverter.ToUInt32(BitConverter.GetBytes((float)encoded),0)==bits,"finite big-endian IEEE bits preserved independently of reversed-float NaN semantics");var bytes=new[]{(byte)(bits>>24),(byte)(bits>>16),(byte)(bits>>8),(byte)bits};Assert(BitConverter.ToUInt32(BitConverter.GetBytes((float)(bfloat)bytes),0)==bits,"native big-endian byte vector decodes exactly");}
        string storage=Path.Combine(_output,"storage");
        foreach(var format in new[]{WiiPaletteFormat.RGB565,WiiPaletteFormat.RGB5A3})
        {
            string dir=Path.Combine(_output,"selected-inputs",format.ToString());Directory.CreateDirectory(dir);
            string modelFile=Path.Combine(dir,format+"-model.brres"),companionFile=Path.Combine(dir,format+"-motions.pac"),payload=Path.Combine(_output,format+"-payload.brres");
            using(var root=(BRRESNode)NodeFactory.FromFile(null,original))using(var extra=new BRRESNode{Name="OriginalSelectedMotions"})using(var pixels=new Bitmap(32,32))
            {
                for(int y=0;y<32;y++)for(int x=0;x<32;x++)pixels.SetPixel(x,y,x<16?(y<16?Color.Red:Color.Blue):(y<16?Color.Lime:Color.FromArgb(format==WiiPaletteFormat.RGB5A3?0:255,255,255,0)));
                var texture=Find<TEX0Node>(root);texture.ReplaceRaw(TextureConverter.CI8.EncodeTextureIndexed(pixels,3,16,format,QuantizationAlgorithm.MedianCut,out var paletteData));
                var palette=extra.CreateResource<PLT0Node>("OriginalHumanoidColor");palette.ReplaceRaw(paletteData);
                var model=Find<MDL0Node>(root);model.Populate();foreach(var material in model._matList.OfType<MDL0MaterialNode>())foreach(var sampler in material.Children.OfType<MDL0MaterialRefNode>())sampler.Palette="OriginalHumanoidColor";
                var motion=root.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(m=>m.Name=="OriginalPlanted");motion.Remove();extra.GetOrCreateFolder<CHR0Node>().AddChild(motion);extra.Export(payload);root.Export(modelFile);
            }
            // A valid raw ARC directory with unknown and empty/redirect siblings. Never decode them.
            byte[] brres=File.ReadAllBytes(payload);using(var stream=new MemoryStream())
            {
                var header=new byte[64];header[0]=(byte)'A';header[1]=(byte)'R';header[2]=(byte)'C';header[4]=header[5]=1;header[7]=3;stream.Write(header,0,64);
                foreach(byte[] data in new[]{new byte[]{(byte)'J',(byte)'U',(byte)'N',(byte)'K',1,2,3,4},new byte[0],brres})
                {var entry=new byte[32];entry[1]=1;for(int i=0;i<4;i++)entry[4+i]=(byte)((uint)data.Length>>(24-i*8));stream.Write(entry,0,32);stream.Write(data,0,data.Length);while(stream.Length%32!=0)stream.WriteByte(0);}
                File.WriteAllBytes(companionFile,stream.ToArray());
            }
            string originalHash=Hash(companionFile);var source=new SourceFolderSession(dir,storage);Expect<IOException>(()=>source.Stage(companionFile,CancellationToken.None),"whole opaque/redirect package remains rejected");
            Assert(BrawlContainerSelection.List(companionFile).Single().Path=="2","bounded directory preserves exact selected index with opaque siblings");
            Expect<OperationCanceledException>(()=>source.StageContainer(companionFile,"2",new CancellationToken(true)),"selected staging cancellation");
            Expect<IOException>(()=>source.StageContainer(original,"",CancellationToken.None),"selected file outside narrow source root rejected");
            var workspace=new CompanionWorkspace(storage);workspace.AddSource(modelFile,CancellationToken.None);workspace.AddSelectedContainer(companionFile,"2");workspace.SelectModel(workspace.Resources.Single(r=>r.Kind=="MDL0"));workspace.SelectTexture("OriginalHumanoidColor",workspace.TextureCandidates("OriginalHumanoidColor").Single());workspace.SelectMotion(workspace.Resources.Single(r=>r.Kind=="CHR0"&&r.Name=="OriginalPlanted"));
            Assert(workspace.Validate().Any(e=>e.Contains("palette")),"CI8 palette is explicitly required, never silently guessed");workspace.SelectPalette("OriginalHumanoidColor",workspace.PaletteCandidates("OriginalHumanoidColor").Single());Assert(workspace.Validate().Count==0,"CI8 with selected RGB palette admitted");
            string resolution=Path.Combine(_output,format+"-selected.fpcompanion");workspace.Save(resolution);var restored=CompanionWorkspace.Load(resolution,storage);Assert(restored.Motion.TreePath.StartsWith("2/") && restored.Motion.SourceContainer=="2","persistent original container/native identities survive reload");string assembled=restored.Assemble(CancellationToken.None);
            using(var root=NodeFactory.FromFile(null,assembled))
            {
                var texture=Find<TEX0Node>(root);var palette=Find<PLT0Node>(root);Assert(texture.Format==WiiPixelFormat.CI8 && texture.LevelOfDetail==3 && palette.Format==format,"native CI8 palette and mip count preserved");
                for(int level=0;level<3;level++)using(var image=texture.GetImage(level)){Assert(image.Width==(32>>level) && image.Height==(32>>level),"decoded native mip dimensions");Assert(image.GetPixel(1,1).R>240 && image.GetPixel(1,1).G<10 && image.GetPixel(image.Width-2,1).G>240,"independent original palette pixels at every mip");if(format==WiiPaletteFormat.RGB5A3)Assert(image.GetPixel(image.Width-2,image.Height-2).A==0,"native palette alpha retained");}
            }
            var project=new CharacterProject(storage);project.MigrateToSharedLibrary(Path.Combine(_output,"intake-library-"+format));string character=project.RegisterSource(resolution,"Original indexed source");var clip=project.State.Animations.Single();string instance=project.Assign(clip.Id,character,"Palette/mip native roundtrip");string saved=Path.Combine(_output,format+"-selected.fpcharacter");project.Save(saved);CharacterProject.Load(saved,storage).ExportInstance(instance,Path.Combine(_output,format+"-reopened.brres"));
            var staged=source.StageContainer(companionFile,"2");Assert(staged.Sha256==originalHash && Hash(staged.StagedPath)==staged.StagedHash,"whole-original and selected-byte hashes are separate");
            byte[] changed=File.ReadAllBytes(companionFile);changed[96+4]^=1;string corrupt=Path.Combine(dir,format+"-bounds.pac");var bounds=(byte[])changed.Clone();for(int i=4;i<8;i++)bounds[64+i]=255;File.WriteAllBytes(corrupt,bounds);Expect<IOException>(()=>BrawlContainerSelection.List(corrupt),"malformed ARC ranges rejected before native decoding");
            File.WriteAllBytes(companionFile,changed);Expect<IOException>(()=>CompanionWorkspace.Load(resolution,storage),"changed unselected opaque sibling invalidates full-source pin");File.WriteAllBytes(companionFile,File.ReadAllBytes(companionFile).Select((v,i)=>i==100?(byte)(v^1):v).ToArray());Assert(Hash(companionFile)==originalHash,"original generated package restored exactly after negative test");
            string missing=companionFile+".missing";File.Move(companionFile,missing);Expect<IOException>(()=>CompanionWorkspace.Load(resolution,storage),"missing selected container source rejected");File.Move(missing,companionFile);
            string link=Path.Combine(dir,"reparse");using(var command=System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd.exe","/c mklink /J \""+link+"\" \""+dir+"\""){UseShellExecute=false,CreateNoWindow=true,WindowStyle=System.Diagnostics.ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true})){Assert(command.WaitForExit(10000)&&command.ExitCode==0,"original fixture reparse setup");}
            Expect<IOException>(()=>source.StageContainer(Path.Combine(link,Path.GetFileName(companionFile)),"2"),"selected staging rejects reparse traversal");Expect<IOException>(()=>BrawlContainerSelection.List(Path.Combine(link,Path.GetFileName(companionFile))),"selected directory listing rejects reparse traversal");
        }
        Expect<IOException>(()=>new SourceFolderSession(AppDomain.CurrentDomain.BaseDirectory,storage),"selected intake never relaxes application-root guard");
        Log("PASS: original MDL0 v9/CI8/PLT0 RGB565/RGB5A3 alpha/three mips; opaque/redirect ARC companion selection, full identity/hash pins, separate native/project reopen, cancellation, bounds, missing palette and source guards.");
    }
}
