using System.Security.Cryptography;
using System.Text.Json;

namespace FighterPort.Core;

public static class ProjectStore
{
    public const int MaxBytes=32*1024*1024;
    public static readonly JsonSerializerOptions Json=new(){WriteIndented=true,UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow};
    public static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value,Json),Json)!;
    public static CharacterProject Load(string path)
    {
        if(new FileInfo(path).Length>MaxBytes) throw new InvalidDataException("Project exceeds 32 MiB quota");
        var p=JsonSerializer.Deserialize<CharacterProject>(File.ReadAllBytes(path),Json)??throw new InvalidDataException("Empty project");
        Validate(p); return p;
    }
    public static void Save(CharacterProject p,string path)
    {
        Validate(p); AtomicWrite(path,JsonSerializer.SerializeToUtf8Bytes(p,Json));
    }
    public static void AtomicWrite(string path,byte[] bytes,bool overwrite=true)
    {
        if(bytes.Length>MaxBytes) throw new InvalidDataException("Output exceeds quota");
        path=Path.GetFullPath(path); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try { File.WriteAllBytes(temp,bytes); File.Move(temp,path,overwrite); } finally { if(File.Exists(temp)) File.Delete(temp); }
    }
    public static void Retain(CharacterProject p,string path)
    {
        if(new FileInfo(path).Length>8*1024*1024) throw new InvalidDataException("Native input exceeds 8 MiB initial subset quota");
        byte[] data=File.ReadAllBytes(path); string hash=Convert.ToHexString(SHA256.HashData(data));
        if(!p.Resources.Any(r=>r.Sha256==hash)) p.Resources.Add(new(){Name=Path.GetFileName(path),Sha256=hash,Base64=Convert.ToBase64String(data)});
    }
    public static void Validate(CharacterProject p)
    {
        void Require(bool ok,string why) { if(!ok) throw new InvalidDataException(why); }
        bool Finite(float[] a,int count)=>a.Length==count&&a.All(float.IsFinite);
        Require(p.Version==1,"Unsupported project version; migration required");
        Require(p.Bones.Count<=512&&p.Meshes.Count<=128&&p.Clips.Count<=256,"Project quota exceeded");
        Require(p.Bones.Select(b=>b.Name).Distinct().Count()==p.Bones.Count,"Duplicate bone names need explicit binding");
        Require(p.Bones.Select(b=>b.Id).Distinct().Count()==p.Bones.Count,"Duplicate bone IDs");
        for(int i=0;i<p.Bones.Count;i++)
        {
            var b=p.Bones[i]; Require(b.Parent>=-1&&b.Parent<i,"Hierarchy must be acyclic and parent-first");
            Require(Finite(b.Translation,3)&&Finite(b.Rotation,3)&&Finite(b.Scale,3)&&Finite(b.InverseBind,16),"Invalid bone transform");
            Require(b.Scale.All(x=>Math.Abs(x)>1e-6f),"Singular scale");
            Require(System.Numerics.Matrix4x4.Invert(Animation.M(b.InverseBind),out _),"Singular inverse bind");
            Require(b.RestMatrix.Length==0||Finite(b.RestMatrix,16),"Invalid native rest matrix");
        }
        if(p.Bones.Any(b=>b.RestMatrix.Length>0))
        {
            var rest=Animation.World(p,null,0);
            for(int i=0;i<rest.Length;i++)
            {
                Require(Animation.A(Animation.M(p.Bones[i].InverseBind)*rest[i]).Zip(Animation.A(System.Numerics.Matrix4x4.Identity)).All(x=>Math.Abs(x.First-x.Second)<.002f),"Native rest matrix does not match fingerprinted inverse bind");
                if(p.Bones[i].RestMatrix.Length>0)Require(System.Numerics.Vector3.Distance(Animation.RestLocal(p.Bones[i]).Translation,Animation.V(p.Bones[i].Translation))<.0001f,"Native rest translation mismatch");
            }
        }
        foreach(var mesh in p.Meshes)
        {
            Require(mesh.Vertices.Count<=200000&&mesh.Indices.Count<=600000,"Mesh quota exceeded");
            Require(Finite(mesh.Color,3),"Invalid material color");
            Require(mesh.NativeAttributes.Count<=64&&mesh.NativeAttributes.All(a=>a.Value.Length==mesh.Vertices.Count&&a.Value.All(v=>v.Length is >=1 and <=4&&v.All(float.IsFinite))),"Invalid retained vertex attributes");
            Require(mesh.Indices.Count%3==0&&mesh.Indices.All(i=>i>=0&&i<mesh.Vertices.Count),"Invalid triangle index");
            foreach(var v in mesh.Vertices)
            {
                Require(Finite(v.Position,3)&&Finite(v.Normal,3)&&Finite(v.UV,2),"Invalid vertex");
                Require(v.Bones.Length>0&&v.Bones.Length<=4&&v.Bones.Length==v.Weights.Length,"Expected 1-4 influences");
                Require(v.Bones.All(b=>b>=0&&b<p.Bones.Count)&&v.Weights.All(w=>float.IsFinite(w)&&w>=0)&&Math.Abs(v.Weights.Sum()-1)<.001f,"Invalid skin weights");
            }
        }
        foreach(var clip in p.Clips)
        {
            if(clip.Transfer is { } layer)
            {
                Require(layer.Method=="WorldRestDelta-v1"&&layer.SourceClip!=null&&layer.SourceClip.Transfer==null&&layer.Mapping!=null&&clip.Curves.Count==0&&clip.TransformTracks.Count==0,"Invalid/nested retarget layer");
                var source=new CharacterProject{Bones=layer.SourceBones,Clips=[layer.SourceClip!]};Validate(source);
                Require(layer.Mapping!.Approved&&layer.Mapping.SourceFingerprint==Animation.Fingerprint(source)&&layer.Mapping.TargetFingerprint==Animation.Fingerprint(p),"Stale/unapproved retarget layer");
                Require(layer.Mapping.BoneIds.Values.Distinct().Count()==layer.Mapping.BoneIds.Count&&layer.Mapping.BoneIds.All(x=>source.Bones.Any(b=>b.Id==x.Key)&&p.Bones.Any(b=>b.Id==x.Value)),"Invalid retarget mapping");
                Require(source.Bones.Concat(p.Bones).All(b=>b.Scale.All(s=>Math.Abs(s-1)<.0001f))&&!layer.SourceClip!.Curves.Any(c=>c.Channel<3),"Unsupported retarget scale");
                RestPoseRetarget.ValidateMapping(source,layer.SourceClip!,p,layer.Mapping!);
                Require(clip.FrameCount==layer.SourceClip!.FrameCount&&clip.Loop==layer.SourceClip.Loop&&clip.FramesPerSecond==layer.SourceClip.FramesPerSecond,"Retarget timing mismatch");
            }
            Require(clip.FrameCount>0&&clip.FrameCount<=100000&&float.IsFinite(clip.FramesPerSecond)&&clip.FramesPerSecond>0,"Invalid clip timing");
            Require(clip.Curves.Count<=4608,"Curve quota exceeded");
            Require(clip.Curves.Select(c=>(c.Bone,c.Channel)).Distinct().Count()==clip.Curves.Count,"Duplicate channels");
            Require(clip.SkeletonFingerprint==Animation.Fingerprint(p),"Clip belongs to a different skeleton");
            Require(clip.TransformTracks.Count<=512&&clip.TransformTracks.Select(t=>t.Bone).Distinct().Count()==clip.TransformTracks.Count,"Duplicate/excess transform tracks");
            foreach(var track in clip.TransformTracks)
            {
                Require(p.Bones.Any(b=>b.Name==track.Bone)&&!clip.Curves.Any(c=>c.Bone==track.Bone),"Unbound/mixed transform track");
                Require(track.Samples.Count==1||track.Samples.Count==clip.FrameCount,"Transform sample count mismatch");
                foreach(var sample in track.Samples)Require(Finite(sample.Scale,3)&&Finite(sample.Rotation,4)&&Finite(sample.Translation,3)&&sample.Scale.All(s=>s>1e-6f)&&Math.Abs(sample.Rotation.Sum(q=>q*q)-1)<.002f,"Invalid transform sample");
            }
            foreach(var c in clip.Curves)
            {
                Require(p.Bones.Any(b=>b.Name==c.Bone)&&c.Channel>=0&&c.Channel<9,"Unresolved clip binding");
                Require(c.Interpolation is "Hermite" or "Linear" or "Step","Unsupported interpolation");
                Require(c.Keys.Count>0&&c.Keys.Count<=100000,"Invalid key count");
                float previous=-1;
                foreach(var k in c.Keys)
                {
                    Require(float.IsFinite(k.Frame)&&float.IsFinite(k.Value)&&float.IsFinite(k.Tangent)&&k.Frame>previous&&k.Frame<clip.FrameCount+(clip.Loop?1:0),"Invalid/duplicate/out-of-order key");
                    if(c.Channel<3) Require(k.Value>1e-6f,"Scale must be positive in initial subset");
                    previous=k.Frame;
                }
            }
        }
        foreach(var r in p.Resources)
        {
            Require(r.Name==Path.GetFileName(r.Name),"Resource names cannot contain paths");
            Require(Convert.ToHexString(SHA256.HashData(Convert.FromBase64String(r.Base64)))==r.Sha256,"Native resource hash mismatch");
        }
    }
}
