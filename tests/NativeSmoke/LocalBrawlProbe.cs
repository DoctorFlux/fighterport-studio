// FighterPort opt-in bounded local native probe, 2026-10-17. No inputs/receipts are published.
using BrawlCrate.Sources;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

internal static partial class NativeSmoke
{
    private static int ProbeBrawl(string source,string evidence)
    {
        try
        {
            if(Directory.Exists(evidence))throw new IOException("Use a new private probe directory.");Directory.CreateDirectory(evidence);
            Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT",Path.Combine(evidence,"settings"));
            var session=new SourceFolderSession(Path.GetDirectoryName(source),Path.Combine(evidence,"storage"));string before=Hash(source);
            var choices=BrawlContainerSelection.List(source);if(choices.Count>64)throw new IOException("Select a smaller container: local probe budget is 64 BRRES.");
            var lines=new List<string>();
            foreach(var choice in choices)
            {
                var staged=session.StageContainer(source,choice.Path);lines.Add("CONTAINER\t"+choice.Path+"\t"+choice.Length);
                using(var root=NodeFactory.FromFile(null,staged.StagedPath))
                foreach(var entry in root.Children.SelectMany(g=>g.Children))
                {
                    if(entry is MDL0Node model)
                    {
                        model.Populate();lines.Add("MODEL\t"+model.Name+"\t"+model.Version+"\tbones="+model.BoneCache.Length+"\tvertices="+model._objList.OfType<MDL0ObjectNode>().Sum(m=>m.Vertices.Count));
                        foreach(var bone in model.BoneCache.OfType<MDL0BoneNode>())lines.Add("BONE\t"+bone.Name+"\tparent="+bone.Parent?.Name+"\trest="+bone.BindState+"\tflags="+(uint)bone._boneFlags+"\tbillboard="+(uint)bone._billboardFlags+"\tworld="+bone.BindMatrix[12]+","+bone.BindMatrix[13]+","+bone.BindMatrix[14]);
                        foreach(var material in model._matList.OfType<MDL0MaterialNode>())foreach(var r in material.Children.OfType<MDL0MaterialRefNode>())lines.Add("MATERIAL\t"+material.Name+"\ttexture="+r.Texture+"\tpalette="+r.Palette);
                    }
                    else if(entry is TEX0Node texture)lines.Add("TEXTURE\t"+texture.Name+"\t"+texture.Version+"\t"+texture.Format+"\tpalette="+texture.HasPalette+"\t"+texture.Width+"x"+texture.Height);
                    else if(entry is PLT0Node palette)lines.Add("PALETTE\t"+palette.Name+"\t"+palette.Version+"\t"+palette.Format);
                    else if(entry is CHR0Node motion)lines.Add("MOTION\t"+motion.Name+"\t"+motion.Version+"\t"+motion.FrameCount+"\tloop="+motion.Loop+"\ttracks="+motion.Children.Count+"\tcomp="+motion.Children.OfType<CHR0EntryNode>().Count(e=>e.ScaleCompensateApply||e.ScaleCompensateParent||e.ClassicScaleOff));
                    else lines.Add("ANCILLARY\t"+entry.GetType().Name+"\t"+entry.Name);
                }
            }
            File.WriteAllLines(Path.Combine(evidence,"native.tsv"),lines);Assert(Hash(source)==before,"private probe source hash unchanged");File.WriteAllText(Path.Combine(evidence,"source.sha256"),before);
            Console.WriteLine("PASS: bounded selected-BRRES native probe; "+choices.Count+" containers. Private native.tsv retained; no playback/retarget claim.");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
