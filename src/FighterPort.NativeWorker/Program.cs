using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using BrawlLib.Modeling;
using System.Web.Script.Serialization;
using BrawlLib.Internal;
using BrawlLib.Modeling.Collada;
using BrawlLib.SSBB.ResourceNodes;
using FighterPort.Core;

internal static class Program
{
    private static readonly JavaScriptSerializer Json=new JavaScriptSerializer { MaxJsonLength=32*1024*1024, RecursionLimit=128 };
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if(args.Length!=4||args[0]!="v1") throw new InvalidDataException("Protocol: v1 operation input output");
            string op=args[1],input=Path.GetFullPath(args[2]),output=Path.GetFullPath(args[3]);
            if(input==output) throw new InvalidDataException("Sources are read-only");
            if(new FileInfo(input).Length>32*1024*1024) throw new InvalidDataException("Worker input quota");
            if(File.Exists(output)) throw new IOException("Worker output must be new");
            switch(op)
            {
                case "fixture-model":
                    // Only accepts our generated DAE job input. General DAE import is not exposed.
                    Collada._importOptions=new Collada.ImportOptions { ModelVersion=11,SetOriginalPath=false,BlenderFBXBoneFix=false,ForceFloatVertices=true,ForceFloatNormals=true,ForceFloatUVs=true };
                    using(var importer=new Collada())
                    using(var model=(MDL0Node)importer.ImportModel(input,Collada.ImportType.MDL0))
                    { if(model==null)throw new InvalidDataException(Collada.Error); model.Name="AsterFixture"; model.Export(output); }
                    break;
                case "import-model":
                    Header(input,"MDL0",11);
                    using(var node=NodeFactory.FromFile(null,input))
                    { var model=node as MDL0Node??throw new InvalidDataException("Expected MDL0"); Write(output,ReadModel(model)); }
                    break;
                case "import-clip":
                    Header(input,"CHR0",4);
                    using(var node=NodeFactory.FromFile(null,input))
                    { var clip=node as CHR0Node??throw new InvalidDataException("Expected CHR0"); Write(output,ReadClip(clip)); }
                    break;
                case "export-clip":
                    WriteClip(Json.Deserialize<Clip>(File.ReadAllText(input)),output); break;
                case "sample-clip":
                    Header(input,"CHR0",4);
                    using(var node=NodeFactory.FromFile(null,input))
                    {
                        var clip=(CHR0Node)node;var samples=new Dictionary<string,float[]>();
                        foreach(CHR0EntryNode b in clip.Children) for(int channel=0;channel<9;channel++)
                        { var values=new List<float>();for(float t=0;t<clip.FrameCount;t+=.25f)values.Add(b.GetFrameValue(channel,t));samples[b.Name+":"+channel]=values.ToArray(); }
                        Write(output,samples);
                    }
                    break;
                default: throw new InvalidDataException("Unknown worker operation");
            }
            Console.WriteLine("{\"protocol\":1,\"ok\":true}");return 0;
        }
        catch(Exception e) { Console.Error.WriteLine(e.GetType().Name+": "+e.Message);return 1; }
    }
    private static void Write(string path,object value)=>File.WriteAllText(path,Json.Serialize(value));
    private static void Header(string path,string magic,int version)
    {
        byte[] b=File.ReadAllBytes(path);
        if(b.Length<64||b.Length>8*1024*1024||Encoding.ASCII.GetString(b,0,4)!=magic)throw new InvalidDataException("Invalid native header/quota");
        int size=(b[4]<<24)|(b[5]<<16)|(b[6]<<8)|b[7],v=(b[8]<<24)|(b[9]<<16)|(b[10]<<8)|b[11];
        if(size<64||size>b.Length||v!=version)throw new InvalidDataException("Unsupported native size/version");
    }
    private static float[] Vec(Vector3 v)=>new[]{v._x,v._y,v._z};
    private static float[] Mat(Matrix m)=>Enumerable.Range(0,16).Select(i=>m[i]).ToArray();
    private static unsafe CharacterProject ReadModel(MDL0Node model)
    {
        model.Populate();model.ApplyCHR(null,0);
        var project=new CharacterProject { Name=model.Name };
        var bones=model.BoneCache.Cast<MDL0BoneNode>().ToList();
        var ordered=new List<MDL0BoneNode>();
        Action<MDL0BoneNode> add=null;
        add=b=>{ if(ordered.Contains(b))return; if(b.Parent is MDL0BoneNode parent)add(parent);ordered.Add(b); };
        foreach(var b in bones)add(b);
        foreach(var b in ordered)
        {
            if(b.SegScaleCompApply||b.BillboardSetting!=0)throw new NotSupportedException("Segment-scale compensation/billboards need a separate evaluator");
            project.Bones.Add(new Bone { Id="mdl0-"+b.BoneIndex,NativeIndex=b.BoneIndex,Name=b.Name,Parent=ordered.IndexOf(b.Parent as MDL0BoneNode),Translation=Vec(b.Translation),Rotation=Vec(b.Rotation),Scale=Vec(b.Scale),InverseBind=Mat(b.InverseBindMatrix) });
        }
        if(model._objList==null)throw new InvalidDataException("MDL0 has no geometry");
        foreach(MDL0ObjectNode obj in model._objList)
        {
            var pm=obj._manager;
            // Pinned upstream has no public triangle-index accessor. Read its inspected field;
            // fail explicitly on ABI changes instead of guessing topology.
            var triangles=typeof(PrimitiveManager).GetField("_triangles",BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(pm) as GLPrimitive;
            if(triangles==null)throw new NotSupportedException("Only triangle geometry is editable; pinned triangle accessor required");
            if(pm._faceData[1]==null||pm._faceData[4]==null)throw new NotSupportedException("Initial model subset requires normals and UV0");
            if(pm._pointCount>200000)throw new InvalidDataException("Mesh quota");
            var mesh=new Mesh { Name=obj.Name };
            ushort* vi=(ushort*)pm._indices.Address;
            Vector3* normals=(Vector3*)pm._faceData[1].Address;
            Vector2* uv=(Vector2*)pm._faceData[4].Address;
            for(int i=0;i<pm._pointCount;i++)
            {
                var v=pm._vertices[vi[i]];var weights=v.GetBoneWeights();
                if(weights==null||weights.Count==0||weights.Count>4)throw new NotSupportedException("Expected 1-4 weights");
                var normal=v.GetMatrix().GetRotationMatrix()*normals[i];
                mesh.Vertices.Add(new Vertex { Position=Vec(v.WeightedPosition),Normal=Vec(normal),UV=new[]{uv[i]._x,1-uv[i]._y},Bones=weights.Select(w=>ordered.IndexOf((MDL0BoneNode)w.Bone)).ToArray(),Weights=weights.Select(w=>w.Weight).ToArray() });
            }
            mesh.Indices=triangles._indices.Select(i=>checked((int)i)).ToList(); project.Meshes.Add(mesh);
        }
        project.Limitations.Add("MDL0 v11 geometry/skeleton/UV0/normal/1-4 weights subset. Native shaders, other UV/color channels and ancillary resources retained opaque only; neutral preview. No general MDL0 rewrite.");
        return project;
    }
    private static Clip ReadClip(CHR0Node node)
    {
        var clip=new Clip { Name=node.Name,NativeVersion=node.Version,FrameCount=node.FrameCount,Loop=node.Loop,FramesPerSecond=30 };
        foreach(CHR0EntryNode bone in node.Children)
        {
        if(bone.ScaleCompensateApply||bone.ClassicScaleOff)throw new NotSupportedException("Special CHR0 scale semantics are not editable");
        for(int channel=0;channel<9;channel++)
        {
            var curve=new Curve { Bone=bone.Name,Channel=channel };
            for(int f=0;f<node.FrameCount+(node.Loop?1:0);f++)
            {
                var key=bone.GetKeyframe(channel,f);if(key==null)continue;
                if(key.Second!=null)throw new NotSupportedException("Dual in/out keys require discontinuity support");
                curve.Keys.Add(new Key { Frame=f,Value=key._value,Tangent=key._tangent });
            }
            if(curve.Keys.Count>0)clip.Curves.Add(curve);
        }
        }
        return clip;
    }
    private static void WriteClip(Clip clip,string path)
    {
        if(clip.NativeVersion!=4||clip.FrameCount<=0||clip.FrameCount>100000)throw new NotSupportedException("CHR0 v4 subset only");
        using(var node=new CHR0Node { Name=clip.Name,Version=4,FrameCount=clip.FrameCount,Loop=clip.Loop })
        {
            foreach(var group in clip.Curves.GroupBy(c=>c.Bone))
            {
                var bone=node.CreateEntry(group.Key);
                foreach(var curve in group)
                {
                    if(curve.Interpolation!="Hermite")throw new NotSupportedException("Native export requires explicit Hermite curves; no silent baking");
                    foreach(var k in curve.Keys)
                    {
                        if(k.Frame!=(int)k.Frame)throw new NotSupportedException("Fractional native key indices unsupported");
                        bone.SetKeyframe(curve.Channel,(int)k.Frame,k.Value,true)._tangent=k.Tangent;
                    }
                }
            }
            node.Export(path);
        }
    }
}
