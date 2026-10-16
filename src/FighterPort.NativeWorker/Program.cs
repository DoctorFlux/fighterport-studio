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
                case "sample-bundle-pose":
                    var pose=Json.Deserialize<BundlePoseRequest>(File.ReadAllText(input));
                    if(new FileInfo(pose.ModelPath).Length>32*1024*1024||new FileInfo(pose.ClipPath).Length>32*1024*1024)throw new InvalidDataException("Bundle quota");
                    using(var modelRoot=NodeFactory.FromFile(null,pose.ModelPath))
                    using(var clipRoot=NodeFactory.FromFile(null,pose.ClipPath))
                    {
                        var model=(MDL0Node)Walk(modelRoot).Single(x=>x.Key==pose.ModelId).Value;
                        var clip=(CHR0Node)Walk(clipRoot).Single(x=>x.Key==pose.ClipId).Value;
                        model.Populate();model.ApplyCHR(clip,pose.Frame+1); // BrawlLib display index is 1-based.
                        Write(output,model.BoneCache.Cast<MDL0BoneNode>().ToDictionary(b=>b.Name,b=>Mat(b.Matrix)));
                    }
                    break;
                case "browse-bundle":
                    using(var node=NodeFactory.FromFile(null,input))
                    { Write(output,Walk(node).Where(x=>x.Value is MDL0Node||x.Value is CHR0Node).Select(x=>new BundleResource{Id=x.Key,Name=x.Value.Name,Kind=x.Value is MDL0Node?"MDL0":"CHR0",Version=x.Value is MDL0Node m?m.Version:((CHR0Node)x.Value).Version}).ToArray()); }
                    break;
                case "model-from-bundle":
                case "clip-from-bundle":
                    var request=Json.Deserialize<BundleRequest>(File.ReadAllText(input));
                    if(new FileInfo(request.Path).Length>32*1024*1024)throw new InvalidDataException("Bundle quota");
                    using(var node=NodeFactory.FromFile(null,request.Path))
                    {
                        var chosen=Walk(node).Single(x=>x.Key==request.Id).Value;
                        if(op=="model-from-bundle")
                        {var m=chosen as MDL0Node??throw new InvalidDataException("Select an MDL0 resource");if(m.Version!=9&&m.Version!=11)throw new NotSupportedException("MDL0 v9/v11 subset");Write(output,ReadModel(m));}
                        else {var c=chosen as CHR0Node??throw new InvalidDataException("Select a CHR0 resource");if(c.Version!=4)throw new NotSupportedException("CHR0 v4 subset");Write(output,ReadClip(c));}
                    }
                    break;
                case "inspect-bundle":
                    using(var node=NodeFactory.FromFile(null,input))
                    {
                        if(node==null)throw new InvalidDataException("Unrecognized resource");
                        var models=new List<object>();var resources=new Dictionary<string,int>();int visited=0;
                        Action<ResourceNode,int> walk=null;
                        walk=(n,depth)=>
                        {
                            if(++visited>25000||depth>32)throw new InvalidDataException("Resource traversal quota");
                            string type=n.GetType().Name;resources[type]=resources.ContainsKey(type)?resources[type]+1:1;
                            if(n is MDL0Node m)
                            {
                                m.Populate();var bones=m.BoneCache.Cast<MDL0BoneNode>().ToArray();
                                if(bones.Length>4096)throw new InvalidDataException("Bone quota");
                                models.Add(new { name=m.Name,version=m.Version,bones=bones.Select(b=>new { name=b.Name,index=b.BoneIndex,parent=(b.Parent as MDL0BoneNode)?.BoneIndex??-1,translation=Vec(b.Translation),rotation=Vec(b.Rotation),scale=Vec(b.Scale),inverseBind=Mat(b.InverseBindMatrix),segmentScaleCompensate=b.SegScaleCompApply }).ToArray() });
                                return; // Skeleton inspection only; do not decode mesh or texture data.
                            }
                            foreach(var child in n.Children)walk(child,depth+1);
                        };
                        walk(node,0);Write(output,new { backend="BrawlLib d56cbac",scope="read-only skeleton/resource inspection; no container import/export support claim",models,resources });
                    }
                    break;
                case "fixture-model":
                    // Only accepts our generated DAE job input. General DAE import is not exposed.
                    Collada._importOptions=new Collada.ImportOptions { ModelVersion=11,SetOriginalPath=false,BlenderFBXBoneFix=false,ForceFloatVertices=true,ForceFloatNormals=true,ForceFloatUVs=true };
                    using(var importer=new Collada())
                    using(var model=(MDL0Node)importer.ImportModel(input,Collada.ImportType.MDL0))
                    { if(model==null)throw new InvalidDataException(Collada.Error); model.Name="AsterFixture"; model.Export(output); }
                    break;
                case "import-model":
                    Header(input,"MDL0",-1);
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
                        if((long)clip.Children.Count*9*clip.FrameCount*4>1000000)throw new InvalidDataException("Native sample quota");
                        foreach(CHR0EntryNode b in clip.Children) for(int channel=0;channel<9;channel++)
                        { var values=new List<float>();for(float t=0;t<clip.FrameCount;t+=.25f)values.Add(b.GetFrameValue(channel,t));samples[b.Name+":"+channel]=values.ToArray(); }
                        Write(output,samples);
                    }
                    break;
                default: ResourceOperations.Run(op,input,output);break;
            }
            Console.WriteLine("{\"protocol\":1,\"ok\":true}");return 0;
        }
        catch(Exception e) { Console.Error.WriteLine(e.GetType().Name+": "+e.Message);return 1; }
    }
    private static void Write(string path,object value)=>File.WriteAllText(path,Json.Serialize(value));
    private static IEnumerable<KeyValuePair<string,ResourceNode>> Walk(ResourceNode root)
    {
        if(root==null)throw new InvalidDataException("Unknown bundle");
        var stack=new Stack<KeyValuePair<string,ResourceNode>>();stack.Push(new KeyValuePair<string,ResourceNode>("0",root));int visited=0;
        while(stack.Count>0)
        {
            var pair=stack.Pop();if(++visited>25000||pair.Key.Count(c=>c=='/')>32)throw new InvalidDataException("Bundle traversal quota");
            yield return pair;
            if(pair.Value is MDL0Node||pair.Value is CHR0Node)continue;
            var children=pair.Value.Children;for(int i=children.Count-1;i>=0;i--)stack.Push(new KeyValuePair<string,ResourceNode>(pair.Key+"/"+i,children[i]));
        }
    }
    private static void Header(string path,string magic,int version)
    {
        NativePreflight.Check(path,magic,version);
        byte[] b=File.ReadAllBytes(path);
        if(b.Length<64||b.Length>8*1024*1024||Encoding.ASCII.GetString(b,0,4)!=magic)throw new InvalidDataException("Invalid native header/quota");
        int size=(b[4]<<24)|(b[5]<<16)|(b[6]<<8)|b[7],v=(b[8]<<24)|(b[9]<<16)|(b[10]<<8)|b[11];
        if(size<64||size>b.Length||(version!=-1&&v!=version))throw new InvalidDataException("Unsupported native size/version");
    }
    private static float[] Vec(Vector3 v)=>new[]{v._x,v._y,v._z};
    private static float[] Mat(Matrix m)=>Enumerable.Range(0,16).Select(i=>m[i]).ToArray();
    internal static unsafe CharacterProject ReadModel(MDL0Node model)
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
            for(int set=1;set<8;set++)if(pm._faceData[4+set]!=null)
            {
                Vector2* coordinates=(Vector2*)pm._faceData[4+set].Address;var retained=new float[pm._pointCount][];
                for(int i=0;i<pm._pointCount;i++)retained[i]=new[]{coordinates[i]._x,1-coordinates[i]._y};
                mesh.NativeAttributes["BrawlUV"+set]=retained;
            }
            mesh.Indices=triangles._indices.Select(i=>checked((int)i)).ToList(); project.Meshes.Add(mesh);
        }
        project.TargetProfile="Brawl MDL0 v"+model.Version;
        project.Limitations.Add("MDL0 v"+model.Version+" geometry/skeleton/UV0/normal/1-4 weights subset. Available extra UV arrays retained; native shaders, color channels and ancillary resources retained opaque only; neutral preview. No general MDL0 rewrite.");
        return project;
    }
    internal static Clip ReadClip(CHR0Node node)
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
