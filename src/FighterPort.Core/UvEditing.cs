using System.Text.Json;

namespace FighterPort.Core;

public sealed class UvEditSession(CharacterProject project,UvGroup source)
{
    public UvGroup Group { get; private set; }=ProjectStore.Clone(source);
    private readonly List<UvGroup> undo=[];
    private readonly Stack<UvGroup> redo=[];
    private void Change(Action<UvGroup> action)
    {
        var next=ProjectStore.Clone(Group);action(next);UvEvaluator.Validate(project,next);
        foreach(var t in next.Tracks)t.Status=UvEvaluator.Support(project,next,t);
        if(undo.Count==16)undo.RemoveAt(0);undo.Add(Group);redo.Clear();Group=next;
    }
    public void SetKey(int track,int channel,int frame,float value,float tangent)
    {
        if(track<0||track>=Group.Tracks.Count||channel<0||channel>4||frame<0||frame>=Group.FrameCount+(Group.Loop?1:0))throw new InvalidDataException("Choose a track, channel and native integer frame");
        Change(g=>
        {
            var t=g.Tracks[track];var c=t.Curves.SingleOrDefault(c=>c.Channel==channel);
            if(c==null){c=new(){Channel=channel};t.Curves.Add(c);}
            c.Keys.RemoveAll(k=>k.Frame==frame);c.Keys.Add(new(){Frame=frame,Value=value,Tangent=tangent});c.Keys=c.Keys.OrderBy(k=>k.Frame).ToList();
        });
    }
    public void DeleteKey(int track,int channel,int frame)
    {
        Change(g=>
        {
            var curve=g.Tracks[track].Curves.Single(c=>c.Channel==channel);
            if(curve.Keys.Count==1)throw new InvalidDataException("Keep a constant key; set its value explicitly");
            if(curve.Keys.RemoveAll(k=>k.Frame==frame)!=1)throw new InvalidDataException("No key at this native frame");
        });
    }
    public void MoveKey(int track,int channel,int from,int to)
    {
        if(to<0||to>=Group.FrameCount+(Group.Loop?1:0))throw new InvalidDataException("Key time outside native group");
        Change(g=>
        {
            var c=g.Tracks[track].Curves.Single(c=>c.Channel==channel);
            if(c.Keys.Any(k=>k.Frame==to))throw new InvalidDataException("Destination already has a key");
            c.Keys.Single(k=>k.Frame==from).Frame=to;c.Keys=c.Keys.OrderBy(k=>k.Frame).ToList();
        });
    }
    public void Bind(int track,string materialId,string nativeName,int slot,bool indirect)
    {
        if(track<0||track>=Group.Tracks.Count||string.IsNullOrWhiteSpace(nativeName)||nativeName.Length>255||nativeName.Contains('\0')||slot<0||slot>(indirect?2:7))throw new InvalidDataException("Invalid native material/sampler binding");
        Change(g=>{var t=g.Tracks[track];t.MaterialId=materialId;t.MaterialName=nativeName;t.Slot=slot;t.Indirect=indirect;});
    }
    public bool Undo(){if(undo.Count==0)return false;redo.Push(Group);Group=undo[^1];undo.RemoveAt(undo.Count-1);return true;}
    public bool Redo(){if(redo.Count==0)return false;undo.Add(Group);Group=redo.Pop();return true;}
}

public sealed partial class ResourceWorkspace
{
    public async Task ExportUv(string path,CancellationToken ct=default)
    {
        CheckOutput(path);if(File.Exists(path))throw new IOException("Choose a new native output path");
        foreach(var doc in Data.Documents)Verify(doc);
        var group=ProjectStore.Clone(Data.Uv??throw new InvalidDataException("Activate an SRT0 first"));
        UvEvaluator.Validate(Data.Project,group);
        var source=JsonSerializer.Deserialize<UvGroup>(await Request("resource-uv",Document(group.Source),group.Source.NodeId,ct),ProjectStore.Json)!;
        if(source.NativeVersion!=4||group.NativeVersion!=4||group.FrameCount>65535||!group.Enabled)throw new InvalidDataException("Export requires enabled source SRT0 v4, at most 65535 frames");
        // Older saves predate stable track identities. Only an unambiguous complete binding set can migrate.
        foreach(var t in group.Tracks.Where(t=>t.NativeTrackId==""))
            t.NativeTrackId=source.Tracks.SingleOrDefault(s=>s.MaterialName==t.MaterialName&&s.Slot==t.Slot&&s.Indirect==t.Indirect)?.NativeTrackId??throw new InvalidDataException("Cannot recover native track identity");
        group.NativeOriginalPath=source.NativeOriginalPath;group.NativeMaterials=source.NativeMaterials.ToList();
        if(group.Tracks.Count!=source.Tracks.Count||group.Tracks.Select(t=>t.NativeTrackId).Distinct().Count()!=source.Tracks.Count||group.Tracks.Any(t=>!source.Tracks.Any(s=>s.NativeTrackId==t.NativeTrackId)))throw new InvalidDataException("Every source SRT0 track must survive export");
        Directory.CreateDirectory(jobs);string request=Path.Combine(jobs,Guid.NewGuid()+".json"),encoded=request+".srt0",read=request+".read.json";
        try
        {
            await File.WriteAllTextAsync(request,JsonSerializer.Serialize(new UvExportRequest{Path=Document(group.Source).Path,Id=group.Source.NodeId,Group=group},ProjectStore.Json),ct);
            byte[] bytes=await worker.Run("export-uv",request,ct);await File.WriteAllBytesAsync(encoded,bytes,ct);
            await File.WriteAllTextAsync(read,JsonSerializer.Serialize(new BundleRequest{Path=encoded,Id="0"},ProjectStore.Json),ct);
            var decoded=JsonSerializer.Deserialize<UvGroup>(await worker.Run("resource-uv",read,ct),ProjectStore.Json)??throw new InvalidDataException("Empty SRT0 decode");
            ValidateUvExport(group,decoded);ct.ThrowIfCancellationRequested();foreach(var doc in Data.Documents)Verify(doc);ProjectStore.AtomicWrite(path,bytes,overwrite:false);
        }
        finally{File.Delete(request);File.Delete(encoded);File.Delete(read);}
    }
    public static void ValidateUvExport(UvGroup expected,UvGroup actual)
    {
        if(expected.Name!=actual.Name||expected.NativeOriginalPath!=actual.NativeOriginalPath||expected.NativeVersion!=actual.NativeVersion||expected.MatrixMode!=actual.MatrixMode||expected.FrameCount!=actual.FrameCount||expected.Loop!=actual.Loop||expected.Tracks.Count!=actual.Tracks.Count)throw new InvalidDataException("Decoded SRT0 metadata/track count mismatch");
        var materials=expected.NativeMaterials.Concat(expected.Tracks.Select(t=>t.MaterialName)).Distinct().Order().ToArray();
        if(!materials.SequenceEqual(actual.NativeMaterials.Order()))throw new InvalidDataException("SRT0 material entries lost or changed");
        long samples=0;
        foreach(var t in expected.Tracks)
        {
            var a=actual.Tracks.SingleOrDefault(a=>a.MaterialName==t.MaterialName&&a.Slot==t.Slot&&a.Indirect==t.Indirect)??throw new InvalidDataException("SRT0 binding lost");
            if(t.Curves.Count!=a.Curves.Count)throw new InvalidDataException("SRT0 channel count mismatch");
            foreach(var c in t.Curves)
            {
                var d=a.Curves.SingleOrDefault(d=>d.Channel==c.Channel)??throw new InvalidDataException("SRT0 channel lost");
                if(c.Interpolation!=d.Interpolation||c.Keys.Count!=d.Keys.Count)throw new InvalidDataException("SRT0 key/constant mismatch");
                for(int i=0;i<c.Keys.Count;i++)
                {
                    var k=c.Keys[i];var b=d.Keys[i];
                    if(k.Frame!=b.Frame||k.Value!=b.Value||k.Tangent!=b.Tangent)throw new InvalidDataException("SRT0 key/value/tangent changed during encoding");
                }
                // Identical Hermite coefficients imply equal continuous curves. Fractional probes additionally
                // exercise evaluation, including original unsupported-preview tracks; no sampled max-error claim.
                foreach(float f in c.Keys.Select(k=>k.Frame).Concat(c.Keys.Zip(c.Keys.Skip(1),(x,y)=>x.Frame+(y.Frame-x.Frame)*.25f)).Concat(c.Keys.Zip(c.Keys.Skip(1),(x,y)=>x.Frame+(y.Frame-x.Frame)*.5f)).Concat(c.Keys.Zip(c.Keys.Skip(1),(x,y)=>x.Frame+(y.Frame-x.Frame)*.75f)))
                {
                    if(++samples>500000)throw new InvalidDataException("SRT0 validation quota");
                    float x=Animation.Evaluate(c,f),y=Animation.Evaluate(d,f);if(!float.IsFinite(x)||x!=y)throw new InvalidDataException("Decoded fractional SRT0 mismatch");
                }
            }
        }
    }
}
