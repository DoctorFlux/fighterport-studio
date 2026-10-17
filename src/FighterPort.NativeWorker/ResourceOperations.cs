using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using BrawlLib.Wii;
using BrawlLib.Wii.Textures;
using FighterPort.Core;

internal static partial class ResourceOperations
{
    private static readonly JavaScriptSerializer Json=new JavaScriptSerializer{MaxJsonLength=32*1024*1024,RecursionLimit=128};
    internal static void Run(string operation,string input,string output)
    {
        if(operation=="export-uv"){ExportUv(Json.Deserialize<UvExportRequest>(File.ReadAllText(input)),output);return;}
        if(operation=="resource-uv-matrices")
        {
            var bindings=Json.Deserialize<TextureBinding[]>(File.ReadAllText(input));
            if(bindings.Length>256)throw new InvalidDataException("Matrix probe quota");
            Write(output,bindings.Select(b=>
            {
                var v=b.RestSrt;var state=new BrawlLib.Modeling.TextureFrameState(new BrawlLib.Internal.Vector2(v[0],v[1]),v[2],new BrawlLib.Internal.Vector2(v[3],v[4]),(TexMatrixMode)Enum.Parse(typeof(TexMatrixMode),b.MatrixMode),false);
                var uv=state._transform*new BrawlLib.Internal.Vector3(.25f,.25f,0);return new[]{uv._x,1-uv._y};
            }).ToArray());return;
        }
        if(operation=="export-visibility")
        {
            var g=Json.Deserialize<VisibilityGroup>(File.ReadAllText(input));
            if(g.NativeVersion!=3||g.FrameCount<1||g.FrameCount>100000||g.Tracks.Count>512||g.Tracks.Sum(t=>(long)t.Values.Length)>1000000||g.Tracks.Any(t=>t.Values.Length!=1&&t.Values.Length!=g.FrameCount))throw new InvalidDataException("VIS0 v3 writer quota/subset");
            using(var node=new VIS0Node{Name=g.Name,OriginalPath=g.NativeOriginalPath,Version=3,FrameCount=g.FrameCount,Loop=g.Loop})
            {
                foreach(var track in g.Tracks){var entry=node.CreateEntry();entry.Name=track.Target;if(track.Values.Length==1)entry.MakeConstant(track.Values[0]);else for(int f=0;f<g.FrameCount;f++)entry.SetEntry(f,track.Values[f]);}
                node.Export(output);
            }
            return;
        }
        if(operation.StartsWith("fixture-workspace-")){Fixture(operation,Json.Deserialize<BundlePoseRequest>(File.ReadAllText(input)),output);return;}
        if(operation=="resource-vis-pose"||operation=="resource-uv-pose")
        {
            var pose=Json.Deserialize<BundlePoseRequest>(File.ReadAllText(input));
            if(new FileInfo(pose.ModelPath).Length>32*1024*1024||new FileInfo(pose.ClipPath).Length>32*1024*1024)throw new InvalidDataException("Native comparison source quota");
            using(var modelRoot=NodeFactory.FromFile(null,pose.ModelPath))using(var motionRoot=NodeFactory.FromFile(null,pose.ClipPath))
            {
                var model=Resolve(modelRoot,pose.ModelId) as MDL0Node??throw new InvalidDataException("MDL0 required");
                model.Populate();
                if(operation=="resource-vis-pose")
                {
                    var vis=Resolve(motionRoot,pose.ClipId) as VIS0Node??throw new InvalidDataException("VIS0 required");
                    if(pose.Frame<0||pose.Frame>=vis.FrameCount)throw new InvalidDataException("Native comparison frame out of range");
                    model.ApplyVIS(vis,pose.Frame+1);
                    Write(output,model._objList.Cast<MDL0ObjectNode>().Select(m=>m._drawCalls.Select(d=>d._render).ToArray()).ToArray());
                }
                else
                {
                    var srt=Resolve(motionRoot,pose.ClipId) as SRT0Node??throw new InvalidDataException("SRT0 required");model.ApplySRT(srt,pose.Frame+1);
                    var result=new Dictionary<string,float[]>();
                    foreach(MDL0MaterialNode mat in model._matList)foreach(var sampler in mat.Children.OfType<MDL0MaterialRefNode>())
                    {
                        var uv=sampler.GetTransform(false)*new BrawlLib.Internal.Vector3(.25f,.25f,0);
                        result[Id(mat)+":"+sampler.Index]=new[]{uv._x,1-uv._y};
                    }
                    Write(output,result);
                }
            }
            return;
        }
        var request=Json.Deserialize<BundleRequest>(File.ReadAllText(input));
        if(new FileInfo(request.Path).Length>32*1024*1024)throw new InvalidDataException("Resource document quota");
        using(var source=File.OpenRead(request.Path))
        {
            var header=new byte[4];if(source.Read(header,0,4)!=4)throw new InvalidDataException("Truncated native source");
            string magic=System.Text.Encoding.ASCII.GetString(header);
            if(magic=="MDL0")NativePreflight.Check(request.Path,magic,-1);
            if(magic=="CHR0")NativePreflight.Check(request.Path,magic,4);
        }
        using(var root=NodeFactory.FromFile(null,request.Path))
        {
            if(root==null)throw new InvalidDataException("Unrecognized native resource");
            var node=Resolve(root,request.Id);
            switch(operation)
            {
                case "resource-info": Write(output,Describe(node));break;
                case "resource-children":
                    if(node.Children.Count>25000)throw new InvalidDataException("Child quota");
                    Write(output,node.Children.Select(Describe).ToArray());break;
                case "resource-model":
                    var mdl=node as MDL0Node??throw new InvalidDataException("Select MDL0");
                    if(mdl.Version!=9&&mdl.Version!=11)throw new NotSupportedException("MDL0 v9/v11 subset");
                    var project=Program.ReadModel(mdl);Appearance(mdl,project);Write(output,project);break;
                case "resource-clip":
                    var chr=node as CHR0Node??throw new InvalidDataException("Select CHR0");
                    if(chr.Version!=4)throw new NotSupportedException("CHR0 v4 subset");Write(output,Program.ReadClip(chr));break;
                case "resource-vis": Write(output,ReadVisibility(node as VIS0Node??throw new InvalidDataException("Select VIS0")));break;
                case "resource-uv": Write(output,ReadUv(node as SRT0Node??throw new InvalidDataException("Select SRT0")));break;
                case "resource-texture":case "resource-texture-probe":case "resource-texture-pixels":
                    var texture=node as TEX0Node??throw new InvalidDataException("Select TEX0");
                    if(texture.Width<1||texture.Height<1||texture.Width>4096||texture.Height>4096||(long)texture.Width*texture.Height>4*1024*1024||request.Mip<0||request.Mip>=texture.LevelOfDetail)throw new InvalidDataException("Texture dimension/mip quota");
                    var candidates=texture.BRESNode?.Children.SelectMany(g=>g.Children).OfType<PLT0Node>().Where(p=>p.Name==texture.Name).ToArray()??new PLT0Node[0];
                    if(texture.HasPalette&&string.IsNullOrEmpty(request.PaletteId)&&candidates.Length!=1)throw new InvalidDataException("Missing/ambiguous palette; explicit palette handle required");
                    var palette=string.IsNullOrEmpty(request.PaletteId)?(texture.HasPalette?candidates.SingleOrDefault():null):Resolve(root,request.PaletteId) as PLT0Node;
                    if(texture.HasPalette&&palette==null)throw new InvalidDataException("Required palette missing");
                    using(var bitmap=texture.GetImage(request.Mip,palette)??throw new InvalidDataException("Texture decode failed"))
                    {
                        if(operation=="resource-texture")bitmap.Save(output,ImageFormat.Png);
                        else if(operation=="resource-texture-pixels")
                        {
                            var rgba=new byte[checked(bitmap.Width*bitmap.Height*4)];for(int y=0;y<bitmap.Height;y++)for(int x=0;x<bitmap.Width;x++){var c=bitmap.GetPixel(x,y);int i=(y*bitmap.Width+x)*4;rgba[i]=c.R;rgba[i+1]=c.G;rgba[i+2]=c.B;rgba[i+3]=c.A;}Write(output,new TexturePixels{Width=bitmap.Width,Height=bitmap.Height,Rgba=Convert.ToBase64String(rgba)});
                        }
                        else
                        {
                            var points=request.ProbePoints.Count==0?new[]{new[]{bitmap.Width/4,bitmap.Height/4},new[]{3*bitmap.Width/4,bitmap.Height/4},new[]{bitmap.Width/4,3*bitmap.Height/4},new[]{3*bitmap.Width/4,3*bitmap.Height/4}}:request.ProbePoints.ToArray();
                            if(points.Length>64||points.Any(p=>p.Length!=2||p[0]<0||p[1]<0||p[0]>=bitmap.Width||p[1]>=bitmap.Height))throw new InvalidDataException("Pixel probe quota/range");
                            Write(output,new{Width=bitmap.Width,Height=bitmap.Height,Pixels=points.Select(p=>bitmap.GetPixel(p[0],p[1]).ToArgb()).ToArray()});
                        }
                    }
                    break;
                case "resource-extract":
                    // Raw source slice, not a renamed/rebuilt native export. Standalone resource relocation is separate.
                    if(node.WorkingUncompressed.Length<=0||node.WorkingUncompressed.Length>8*1024*1024)throw new InvalidDataException("Raw resource slice quota");
                    Raw(node,output);break;
                default:throw new InvalidDataException("Unknown worker operation");
            }
        }
    }
    private static unsafe void Raw(ResourceNode node,string output)
    {
        var data=new byte[node.WorkingUncompressed.Length];System.Runtime.InteropServices.Marshal.Copy((IntPtr)node.WorkingUncompressed.Address,data,0,data.Length);File.WriteAllBytes(output,data);
    }
    private static void Write(string path,object value)=>File.WriteAllText(path,Json.Serialize(value));
    private static ResourceNode Resolve(ResourceNode root,string id)
    {
        var parts=id.Split('/');if(parts.Length>33||parts[0]!="0")throw new InvalidDataException("Invalid native handle");
        var node=root;foreach(var part in parts.Skip(1)){if(!int.TryParse(part,out int i)||i<0||i>=node.Children.Count)throw new InvalidDataException("Stale native handle");node=node.Children[i];}return node;
    }
    internal static string Id(ResourceNode node)=>node.Parent==null?"0":Id(node.Parent)+"/"+node.Index;
    private static NativeTreeEntry Describe(ResourceNode node)
    {
        string kind=node is MDL0Node?"MDL0":node.GetType().Name.Replace("Node","");
        bool preview=node is MDL0Node||node is CHR0Node||node is VIS0Node||node is TEX0Node||node is SRT0Node;
        var texture=node as TEX0Node;
        return new NativeTreeEntry{Id=Id(node),Name=node.Name,Kind=kind,Version=(node as BRESEntryNode)?.Version??0,Size=node.UncompressedSize,HasChildren=node.HasChildren,Width=texture?.Width??0,Height=texture?.Height??0,MipCount=texture?.LevelOfDetail??0,PixelFormat=texture?.Format.ToString()??"",Status=preview?"Preview subset available; source retained read-only":"Source retained; evaluation and edited export not implemented"};
    }
    private static void Appearance(MDL0Node model,CharacterProject project)
    {
        var materials=model._matList?.Cast<MDL0MaterialNode>().ToArray()??new MDL0MaterialNode[0];
        var bres=model.BRESNode;
        // Search only actual sibling texture/palette groups in the owning BRRES; never global first-name matches.
        var resources=bres==null?new ResourceNode[0]:bres.Children.SelectMany(g=>g.Children).ToArray();
        var banks=new List<BRRESNode>();
        if(bres?.Parent is ARCNode)
        {
            // Pinned BrawlLib MDL0TextureNode.Load: TextureData siblings with matching/global group,
            // including explicit ARC redirects. Keep all eligible matches to report ambiguity.
            foreach(var entry in bres.Parent.Children.OfType<ARCEntryNode>())
            {
                if(entry.FileType!=ARCFileType.TextureData||entry.GroupID!=bres.GroupID&&entry.GroupID!=0)continue;
                var bank=entry.RedirectNode as BRRESNode??entry as BRRESNode;
                if(bank!=null&&!banks.Contains(bank))banks.Add(bank);
            }
        }
        foreach(var mat in materials)
        {
            var result=new PreviewMaterial{Id=Id(mat),Name=mat.Name};
            result.Program=ReadMaterialProgram(mat);
            result.Limitations.Add("Source approximation: one selected direct sampler / native UV and decoded color/alpha only. Native TEV, lighting, alpha-test, sampler composition, nearest/mip filtering and animated materials remain unevaluated.");
            foreach(var sampler in mat.Children.OfType<MDL0MaterialRefNode>())
            {
                var matches=resources.OfType<TEX0Node>().Where(t=>t.Name==sampler.Texture).ToArray();string scope="owning BRRES";
                if(matches.Length==0){matches=banks.SelectMany(b=>b.Children.SelectMany(g=>g.Children)).OfType<TEX0Node>().Where(t=>t.Name==sampler.Texture).ToArray();scope="ARC TextureData matching/global group";}
                var palettes=matches.Length==1?(matches[0].BRESNode?.Children.SelectMany(g=>g.Children).OfType<PLT0Node>().Where(p=>p.Name==sampler.Palette).ToArray()??new PLT0Node[0]):new PLT0Node[0];
                var binding=new TextureBinding{Slot=sampler.Index,Name=sampler.Texture??"",PaletteName=sampler.Palette??"",UVSet=sampler.TextureCoordId,WrapU=sampler.UWrapMode.ToString(),WrapV=sampler.VWrapMode.ToString(),Filter=sampler.MagFilter.ToString()};
                binding.MinFilter=sampler.MinFilter.ToString();binding.LodBias=sampler.LODBias;binding.Projection=sampler.Projection.ToString();binding.Texgen=sampler.Type.ToString();
                binding.RestSrt=Enumerable.Range(0,5).Select(i=>sampler._bindState[i]).ToArray();binding.MatrixMode=sampler._bindState.MatrixMode.ToString();
                if(matches.Length==1&&(!matches[0].HasPalette||palettes.Length==1))
                {binding.Texture.NodeId=Id(matches[0]);binding.PaletteId=palettes.Length==1?Id(palettes[0]):"";binding.Status="Resolved by native material reference: "+scope;}
                else binding.Status=matches.Length>1||palettes.Length>1?"Ambiguous native binding; override required":"Missing texture/palette in native BRRES/ARC scope; add source and override";
                result.Textures.Add(binding);
            }
            project.Materials.Add(result);
        }
        for(int i=0;i<project.Meshes.Count;i++)
        {
            var obj=(MDL0ObjectNode)model._objList[i];
            foreach(var draw in obj._drawCalls)project.Meshes[i].Draws.Add(new MeshDraw{MaterialId=draw.MaterialNode==null?"":Id(draw.MaterialNode),VisibilityBoneId=draw.VisibilityBoneNode==null?"":"mdl0-"+draw.VisibilityBoneNode.BoneIndex,DefaultVisible=draw._render,Pass=draw.DrawPass.ToString()});
        }
        project.Limitations.RemoveAll(s=>s.Contains("neutral preview"));
        project.Limitations.Add("Source appearance approximation: one selected direct sampler / native UV, decoded color/alpha; native TEV, alpha-test, nearest/mip filtering, other color channels, material/shape and unsupported UV animation and edited model export remain unevaluated. Original source documents retained through verified external links.");
    }
    private static VisibilityGroup ReadVisibility(VIS0Node node)
    {
        if(node.Version!=3&&node.Version!=4)throw new NotSupportedException("VIS0 v3/v4 subset");
        if(node.FrameCount<1||node.FrameCount>100000||node.Children.Count>512||(long)node.FrameCount*node.Children.Count>1000000)throw new InvalidDataException("VIS0 quota");
        var group=new VisibilityGroup{Name=node.Name,NativeOriginalPath=node.OriginalPath??"",NativeVersion=node.Version,FrameCount=node.FrameCount,Loop=node.Loop};
        foreach(VIS0EntryNode entry in node.Children)group.Tracks.Add(new VisibilityTrack{Target=entry.Name,Values=entry.EntryCount==0?new[]{entry.Enabled}:Enumerable.Range(0,node.FrameCount).Select(entry.GetEntry).ToArray()});
        return group;
    }
    private static UvGroup ReadUv(SRT0Node node)
    {
        if(node.Version!=4&&node.Version!=5)throw new NotSupportedException("SRT0 v4/v5 reader");
        if(node.FrameCount<1||node.FrameCount>100000||node.Children.Count>128)throw new InvalidDataException("UV animation quota");
        var group=new UvGroup{Name=node.Name,NativeOriginalPath=node.OriginalPath??"",NativeMaterials=node.Children.Select(m=>m.Name).ToList(),NativeVersion=node.Version,FrameCount=node.FrameCount,Loop=node.Loop,MatrixMode=node.MatrixMode.ToString()};
        int keys=0;
        foreach(SRT0EntryNode material in node.Children)foreach(SRT0TextureNode sampler in material.Children)
        {
            var track=new UvTrack{NativeTrackId=material.Index+"/"+sampler.Index,MaterialName=material.Name,Slot=sampler.TextureIndex,Indirect=sampler.Indirect};
            for(int channel=0;channel<5;channel++)
            {
                var curve=new Curve{Channel=channel};
                for(int f=0;f<node.FrameCount+(node.Loop?1:0);f++)
                {
                    var key=sampler.GetKeyframe(channel,f);if(key==null)continue;
                    if(key.Second!=null)throw new NotSupportedException("Dual SRT keys need explicit discontinuity support");
                    if(++keys>100000)throw new InvalidDataException("UV key quota");curve.Keys.Add(new Key{Frame=f,Value=key._value,Tangent=key._tangent});
                }
                if(curve.Keys.Count>0)track.Curves.Add(curve);
            }
            group.Tracks.Add(track);
        }
        return group;
    }
    private static void Fixture(string operation,BundlePoseRequest request,string output)
    {
        using(var arc=new ARCNode{Name="OriginalConformance"})
        {
            var bres=new BRRESNode{Name="OriginalResources",FileType=ARCFileType.ModelData,FileIndex=0,GroupID=0};arc.AddChild(bres);
            if(operation=="fixture-workspace-ci4")CreateCi4Fixture(bres);
            else if(operation=="fixture-workspace-palette")
            {
                using(var asymmetric=new Bitmap(8,4))
                {
                    for(int y=0;y<4;y++)for(int x=0;x<8;x++)asymmetric.SetPixel(x,y,Color.FromArgb(x>=6&&y==3?0:255,200-x*15-y*20,10+x*25,20+y*50));
                    var tex=bres.CreateResource<TEX0Node>("Asymmetric");tex.ReplaceRaw(TextureConverter.RGBA8.EncodeTEX0Texture(asymmetric,1));tex.Name="Asymmetric";
                }
                using(var image=new Bitmap(32,32,PixelFormat.Format8bppIndexed))
                {
                    var colors=image.Palette;colors.Entries[0]=Color.Red;colors.Entries[1]=Color.Lime;colors.Entries[2]=Color.Blue;colors.Entries[3]=Color.FromArgb(0,0,0,0);image.Palette=colors;
                    var bits=image.LockBits(new Rectangle(0,0,32,32),ImageLockMode.WriteOnly,PixelFormat.Format8bppIndexed);
                    try{for(int y=0;y<32;y++){var row=new byte[bits.Stride];for(int x=0;x<32;x++)row[x]=(byte)((x<16?0:1)+(y<16?0:2));System.Runtime.InteropServices.Marshal.Copy(row,0,IntPtr.Add(bits.Scan0,y*bits.Stride),row.Length);}}finally{image.UnlockBits(bits);}
                    BrawlLib.Internal.IO.FileMap palette;
                    // ReplaceRaw transfers FileMap ownership to the node; disposal before ARC rebuild invalidates its source.
                    var encoded=TextureConverter.CI8.EncodeTEX0TextureIndexed(image,2,WiiPaletteFormat.RGB5A3,out palette);
                    {
                        var tex=bres.CreateResource<TEX0Node>("IndexedAlpha");tex.ReplaceRaw(encoded);tex.Name="IndexedAlpha";
                        var pal=bres.CreateResource<PLT0Node>("IndexedAlpha");pal.ReplaceRaw(palette);pal.Name="IndexedAlpha";
                    }
                    colors.Entries[0]=Color.Yellow;image.Palette=colors;
                    {var pal=bres.CreateResource<PLT0Node>("AlternatePalette");pal.ReplaceRaw(TextureConverter.EncodePLT0Palette(image.Palette,WiiPaletteFormat.RGB5A3));pal.Name="AlternatePalette";}
                }
            }
            else if(operation=="fixture-workspace-model"||operation=="fixture-workspace-ambiguous"||operation=="fixture-workspace-material")
            {
                var model=NodeFactory.FromFile(null,request.ModelPath) as MDL0Node??throw new InvalidDataException("Original MDL0 required");
                bres.GetOrCreateFolder<MDL0Node>().AddChild(model);model.Populate();
                var bank=new BRRESNode{Name="OriginalTextureBank",FileType=ARCFileType.TextureData,FileIndex=0,GroupID=0};arc.AddChild(bank);
                var otherBank=new BRRESNode{Name="OtherGroup",FileType=ARCFileType.TextureData,FileIndex=1,GroupID=(byte)(operation=="fixture-workspace-ambiguous"?0:2)};arc.AddChild(otherBank);
                using(var image=new Bitmap(32,32))
                {
                    for(int y=0;y<32;y++)for(int x=0;x<32;x++)image.SetPixel(x,y,x<16?(y<16?Color.Red:Color.Blue):(y<16?Color.Lime:Color.Yellow));
                    var tex=bank.CreateResource<TEX0Node>("OriginalQuadrants");tex.ReplaceRaw(TextureConverter.Get(WiiPixelFormat.RGBA8).EncodeTEX0Texture(image,1));tex.Name="OriginalQuadrants";
                    using(var wrong=new Bitmap(32,32)){using(var graphics=Graphics.FromImage(wrong))graphics.Clear(Color.Magenta);var duplicate=otherBank.CreateResource<TEX0Node>("OriginalQuadrants");duplicate.ReplaceRaw(TextureConverter.Get(WiiPixelFormat.RGBA8).EncodeTEX0Texture(wrong,1));duplicate.Name="OriginalQuadrants";}
                }
                foreach(MDL0MaterialNode material in model._matList)
                {
                    var sampler=material.Children.OfType<MDL0MaterialRefNode>().FirstOrDefault();
                    if(sampler==null){sampler=new MDL0MaterialRefNode();material.AddChild(sampler);}
                    sampler.Texture="OriginalQuadrants";sampler.Coordinates=BrawlLib.Wii.Graphics.TexSourceRow.TexCoord0;
                    var extra=new MDL0MaterialRefNode();material.AddChild(extra);extra.Texture="OriginalQuadrants";extra.Coordinates=BrawlLib.Wii.Graphics.TexSourceRow.TexCoord1;extra.UWrapMode=MatWrapMode.Mirror;extra.VWrapMode=MatWrapMode.Clamp;
                }
                foreach(MDL0ObjectNode mesh in model._objList)foreach(var draw in mesh._drawCalls)draw.VisibilityBoneNode=model.BoneCache.OfType<MDL0BoneNode>().First(b=>b.Name==(mesh==model._objList.Last()?"LeftHand":"Root"));
                if(operation=="fixture-workspace-material")CreateMaterialFixture(model,bank);
            }
            else if(operation=="fixture-workspace-motion")
            {
                var chr=NodeFactory.FromFile(null,request.ClipPath) as CHR0Node??throw new InvalidDataException("Original CHR0 required");bres.GetOrCreateFolder<CHR0Node>().AddChild(chr);
                var vis=bres.CreateResource<VIS0Node>("OriginalAccessory");vis.Version=3;vis.OriginalPath="OriginalFixtures/Accessory.anim";vis.FrameCount=chr.FrameCount;vis.Loop=chr.Loop;
                var entry=vis.CreateEntry();entry.Name="LeftHand";for(int i=0;i<vis.FrameCount;i++)entry.SetEntry(i,i<10||i>=20);
                var opaque=bres.CreateResource<SRT0Node>("PreservedUvGap");opaque.FrameCount=chr.FrameCount; // Real native empty channel fixture; no UV animation support claim.
                using(var model=NodeFactory.FromFile(null,request.ModelPath) as MDL0Node)
                {
                    model.Populate();var uv=bres.CreateResource<SRT0Node>("OriginalUvSlide");uv.FrameCount=chr.FrameCount;uv.Loop=chr.Loop;
                    var mat=new SRT0EntryNode{Name=model._matList[0].Name};uv.AddChild(mat);var sampler=new SRT0TextureNode(0,false);mat.AddChild(sampler);uv.FrameCount=chr.FrameCount;uv.Loop=chr.Loop;
                    sampler.SetKeyframe(3,0,0);sampler.SetKeyframe(3,20,.5f);sampler.SetKeyframe(3,40,0);
                    var editable=bres.CreateResource<SRT0Node>("OriginalUvEditable");editable.OriginalPath="original/fixtures/asymmetric.srt0";editable.MatrixMode=TexMatrixMode.Matrix3dsMax;
                    var editableMat=new SRT0EntryNode{Name=model._matList[0].Name};editable.AddChild(editableMat);
                    foreach(int slot in new[]{0,1,7,10})
                    {
                        var track=new SRT0TextureNode(slot==10?2:slot,slot==10);editableMat.AddChild(track);editable.FrameCount=chr.FrameCount;editable.Loop=chr.Loop;
                        track.SetKeyframe(0,0,slot==1?-1.25f:1);track.SetKeyframe(3,0,.125f);track.SetKeyframe(3,20,.75f);track.SetKeyframe(3,40,-.25f);
                        track.GetKeyframe(3,0)._tangent=.07f;track.GetKeyframe(3,20)._tangent=-.04f;track.GetKeyframe(3,40)._tangent=.02f;
                    }
                    editable.AddChild(new SRT0EntryNode{Name="RetainedEmptyMaterial"});
                    foreach(var mode in new[]{TexMatrixMode.MatrixXSI,TexMatrixMode.Matrix3dsMax})
                    {
                        var rotating=bres.CreateResource<SRT0Node>("OriginalUv"+mode);rotating.MatrixMode=mode;
                        var material=new SRT0EntryNode{Name=model._matList[0].Name};rotating.AddChild(material);var channel=new SRT0TextureNode(0,false);material.AddChild(channel);rotating.FrameCount=chr.FrameCount;rotating.Loop=chr.Loop;
                        channel.SetKeyframe(0,0,1.5f);channel.SetKeyframe(1,0,.75f);
                        channel.SetKeyframe(2,0,0);channel.SetKeyframe(2,20,90);channel.SetKeyframe(2,40,-90);
                        channel.SetKeyframe(3,0,.125f);channel.SetKeyframe(4,0,-.25f);
                    }
                }
            }
            else throw new InvalidDataException("Unknown original fixture mode");
            arc.Export(output);
        }
    }
}
