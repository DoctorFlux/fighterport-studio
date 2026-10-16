using System;
using System.IO;
using System.Linq;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using FighterPort.Core;

internal static partial class ResourceOperations
{
    private static unsafe void ExportUv(UvExportRequest request,string output)
    {
        if(new FileInfo(request.Path).Length>32*1024*1024)throw new InvalidDataException("Source quota");
        using(var root=NodeFactory.FromFile(null,request.Path))
        {
            var node=Resolve(root,request.Id) as SRT0Node??throw new InvalidDataException("SRT0 source required");
            if(node.Version!=4)throw new NotSupportedException("Only SRT0 v4 working-copy export is verified");
            var sourceHeader=(BrawlLib.Internal.buint*)node.WorkingUncompressed.Address;
            if((uint)sourceHeader[9]>1||(uint)sourceHeader[8]>2)throw new NotSupportedException("Noncanonical SRT0 loop/matrix metadata; export blocked");
            var before=ReadUv(node);var g=request.Group;
            if(g.Name!=before.Name||g.NativeOriginalPath!=before.NativeOriginalPath||g.MatrixMode!=before.MatrixMode||g.FrameCount!=before.FrameCount||g.Loop!=before.Loop||g.NativeVersion!=4||!g.NativeMaterials.SequenceEqual(before.NativeMaterials))throw new InvalidDataException("Source metadata must be retained");
            if(g.Tracks.Count!=before.Tracks.Count||g.Tracks.Select(t=>t.NativeTrackId).Distinct().Count()!=g.Tracks.Count||g.Tracks.Any(t=>!before.Tracks.Any(b=>b.NativeTrackId==t.NativeTrackId)))throw new InvalidDataException("Every native track must be retained exactly once");
            if(g.Tracks.Select(t=>Tuple.Create(t.MaterialName,t.Slot,t.Indirect)).Distinct().Count()!=g.Tracks.Count)throw new InvalidDataException("Duplicate native binding");
            var materials=node.Children.Cast<SRT0EntryNode>().ToArray();
            // Reject unknown bits instead of allowing the backend rebuild to normalize them silently.
            foreach(var material in materials)
            {
                var header=(BrawlLib.Internal.buint*)material.WorkingUncompressed.Address;
                if(((uint)header[1]&~255u)!=0||((uint)header[2]&~7u)!=0)throw new NotSupportedException("Unknown native sampler flags");
                foreach(SRT0TextureNode sampler in material.Children)
                {
                    uint code=*(BrawlLib.Internal.buint*)sampler.WorkingUncompressed.Address;
                    if((code&~1023u)!=0||(code&1)==0)throw new NotSupportedException("Unknown SRT0 track flags; export blocked to preserve source");
                }
            }
            var tracks=materials.SelectMany(m=>m.Children.Cast<SRT0TextureNode>()).ToDictionary(t=>t.Parent.Index+"/"+t.Index);
            foreach(var t in g.Tracks)
            {
                if(string.IsNullOrWhiteSpace(t.MaterialName)||t.MaterialName.Length>255||t.MaterialName.Contains('\0')||t.Slot<0||t.Slot>(t.Indirect?2:7)||t.Curves.Count>5||t.Curves.Select(c=>c.Channel).Distinct().Count()!=t.Curves.Count)throw new InvalidDataException("Invalid SRT0 binding/channels");
                var sampler=tracks[t.NativeTrackId];var original=before.Tracks.Single(b=>b.NativeTrackId==t.NativeTrackId);
                if(Json.Serialize(original.Curves)!=Json.Serialize(t.Curves))
                {
                    if(t.Curves.Sum(c=>c.Keys.Count)>100000)throw new InvalidDataException("SRT0 key quota");
                    foreach(var curve in t.Curves)
                    {
                        if(curve.Channel<0||curve.Channel>4||curve.Interpolation!="Hermite"||curve.Keys.Count==0)throw new InvalidDataException("Unsupported curve");
                        float previous=-1;
                        foreach(var k in curve.Keys)
                        {
                            if(float.IsNaN(k.Value)||float.IsInfinity(k.Value)||float.IsNaN(k.Tangent)||float.IsInfinity(k.Tangent)||k.Frame!=Math.Floor(k.Frame)||k.Frame<=previous||k.Frame<0||k.Frame>=g.FrameCount+(g.Loop?1:0))throw new InvalidDataException("Invalid native key");previous=k.Frame;
                        }
                        if(curve.Keys.Count==1&&(curve.Keys[0].Frame!=0||curve.Keys[0].Tangent!=0))throw new InvalidDataException("Native constant must be at frame zero with zero tangent");
                    }
                    foreach(var curve in original.Curves)foreach(var k in curve.Keys)sampler.RemoveKeyframe(curve.Channel,(int)k.Frame);
                    foreach(var curve in t.Curves)foreach(var k in curve.Keys)sampler.SetKeyframe(curve.Channel,(int)k.Frame,k.Value);
                    // BrawlLib insertion generates neighboring tangents. Restore explicit tangents last.
                    foreach(var curve in t.Curves)foreach(var k in curve.Keys)sampler.GetKeyframe(curve.Channel,(int)k.Frame)._tangent=k.Tangent;
                }
                if(t.MaterialName!=original.MaterialName||t.Slot!=original.Slot||t.Indirect!=original.Indirect)
                {
                    var parent=node.Children.OfType<SRT0EntryNode>().SingleOrDefault(m=>m.Name==t.MaterialName);
                    if(parent==null){parent=new SRT0EntryNode{Name=t.MaterialName};node.AddChild(parent);}
                    sampler.Parent.RemoveChild(sampler);sampler._textureIndex=t.Slot;sampler._indirect=t.Indirect;sampler.Name=(t.Indirect?"Ind":"")+"Texture"+t.Slot;parent.AddChild(sampler);
                }
            }
            // Native masks encode direct 0..7 followed by indirect 0..2. Preserve empty materials too.
            foreach(var material in node.Children.OfType<SRT0EntryNode>())
            {
                var ordered=material.Children.Cast<SRT0TextureNode>().OrderBy(t=>t.Indirect?8+t.TextureIndex:t.TextureIndex).ToArray();
                foreach(var t in ordered)material.RemoveChild(t);
                foreach(var t in ordered)material.AddChild(t);
            }
            node.Export(output);
        }
    }
}
