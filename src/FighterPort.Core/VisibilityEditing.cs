using System.Text.Json;

namespace FighterPort.Core;

/// <summary>A bounded visibility-only undo history, independent of skeletal key editing.</summary>
public sealed class VisibilityEditSession(CharacterProject project,VisibilityGroup source)
{
    public VisibilityGroup Group { get; private set; }=ProjectStore.Clone(source);
    private readonly List<VisibilityGroup> undo=[];
    private readonly Stack<VisibilityGroup> redo=[];
    private void Change(Action<VisibilityGroup> edit)
    {
        var candidate=ProjectStore.Clone(Group);edit(candidate);SceneEvaluator.Validate(project,candidate);
        if(undo.Count==16)undo.RemoveAt(0);undo.Add(Group);redo.Clear();Group=candidate;
    }
    public void SetFrame(int track,int frame,bool visible)
    {
        if(track<0||track>=Group.Tracks.Count||frame<0||frame>=Group.FrameCount)throw new InvalidDataException("Choose a visibility track and integer group frame");
        Change(g=>{var t=g.Tracks[track];if(t.Values.Length==1)t.Values=Enumerable.Repeat(t.Values[0],g.FrameCount).ToArray();t.Values[frame]=visible;});
    }
    public void Bind(int track,string boneId)
    {
        if(track<0||track>=Group.Tracks.Count)throw new InvalidDataException("Choose a visibility track");
        var bone=project.Bones.Single(b=>b.Id==boneId);
        if(project.Bones.Count(b=>b.Name==bone.Name)!=1)throw new InvalidDataException("Native VIS0 target name must be unique");
        Change(g=>{g.Tracks[track].BoneId=boneId;g.Tracks[track].Target=bone.Name;});
    }
    public bool Undo(){if(undo.Count==0)return false;redo.Push(Group);Group=undo[^1];undo.RemoveAt(undo.Count-1);return true;}
    public bool Redo(){if(redo.Count==0)return false;undo.Add(Group);Group=redo.Pop();return true;}
}

public sealed partial class ResourceWorkspace
{
    public async Task ExportVisibility(string path,CancellationToken ct=default)
    {
        CheckOutput(path);if(File.Exists(path))throw new IOException("Choose a new native output path");
        foreach(var doc in Data.Documents)Verify(doc);
        var group=ProjectStore.Clone(Data.Visibility??throw new InvalidDataException("Activate a VIS0 first"));
        SceneEvaluator.Validate(Data.Project,group);
        // Recover source-path metadata even for FPWORK saved before this field was introduced.
        var source=JsonSerializer.Deserialize<VisibilityGroup>(await Request("resource-vis",Document(group.Source),group.Source.NodeId,ct),ProjectStore.Json)??throw new InvalidDataException("Missing VIS0 source metadata");
        if(source.NativeVersion!=3)throw new InvalidDataException("Only source VIS0 v3 is verified for export");
        group.NativeOriginalPath=source.NativeOriginalPath;
        if(group.NativeVersion!=3||group.Rate!=1||group.Offset!=0||!group.Enabled)throw new InvalidDataException("VIS0 v3 export requires enabled group, native rate 1 and offset 0; timeline overrides are not encoded");
        if(string.IsNullOrWhiteSpace(group.Name)||group.Name.Length>255||group.Name.Contains('\0')||group.Tracks.Any(t=>string.IsNullOrWhiteSpace(t.Target)||t.Target.Length>255||t.Target.Contains('\0')))throw new InvalidDataException("Invalid native VIS0 name");
        Directory.CreateDirectory(jobs);string request=System.IO.Path.Combine(jobs,Guid.NewGuid()+".json"),encoded=request+".vis0",read=request+".read.json";
        try
        {
            await File.WriteAllTextAsync(request,JsonSerializer.Serialize(group,ProjectStore.Json),ct);
            byte[] bytes=await worker.Run("export-visibility",request,ct);await File.WriteAllBytesAsync(encoded,bytes,ct);
            await File.WriteAllTextAsync(read,JsonSerializer.Serialize(new BundleRequest{Path=encoded,Id="0"},ProjectStore.Json),ct);
            var decoded=JsonSerializer.Deserialize<VisibilityGroup>(await worker.Run("resource-vis",read,ct),ProjectStore.Json)??throw new InvalidDataException("Empty VIS0 decode");
            ValidateVisibilityExport(group,decoded);ct.ThrowIfCancellationRequested();ProjectStore.AtomicWrite(path,bytes,overwrite:false);
        }
        finally{File.Delete(request);File.Delete(encoded);File.Delete(read);}
    }
    public static void ValidateVisibilityExport(VisibilityGroup expected,VisibilityGroup actual)
    {
        if(expected.Name!=actual.Name||expected.NativeOriginalPath!=actual.NativeOriginalPath||expected.NativeVersion!=actual.NativeVersion||expected.FrameCount!=actual.FrameCount||expected.Loop!=actual.Loop||expected.Tracks.Count!=actual.Tracks.Count)throw new InvalidDataException("Decoded VIS0 metadata mismatch");
        for(int i=0;i<expected.Tracks.Count;i++)
            if(expected.Tracks[i].Target!=actual.Tracks[i].Target||!expected.Tracks[i].Values.SequenceEqual(actual.Tracks[i].Values))throw new InvalidDataException("Decoded VIS0 binding/sample/constant flag mismatch");
    }
}
