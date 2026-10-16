using System.Globalization;
using System.Xml.Linq;
using System.Numerics;

namespace FighterPort.Core;

// Deliberately a generated-fixture writer, not a general interchange support claim.
public static class ColladaFixture
{
    private static readonly XNamespace Ns="http://www.collada.org/2005/11/COLLADASchema";
    private static XElement E(string name,params object[] values)=>new(Ns+name,values);
    private static XAttribute A(string name,object value)=>new(name,value);
    private static string Floats(IEnumerable<float> f)=>string.Join(" ",f.Select(x=>x.ToString("R",CultureInfo.InvariantCulture)));
    private static string Matrix(Matrix4x4 m)=>Floats(Animation.A(Matrix4x4.Transpose(m)));
    private static XElement Source(string id,IEnumerable<float> values,int stride,string[] names)
    {
        var v=values.ToArray();return E("source",A("id",id),E("float_array",A("id",id+"-array"),A("count",v.Length),Floats(v)),E("technique_common",E("accessor",A("source","#"+id+"-array"),A("count",v.Length/stride),A("stride",stride),names.Select(n=>E("param",A("name",n),A("type",stride==16?"float4x4":"float"))))));
    }
    public static void Write(CharacterProject p,string path)
    {
        ProjectStore.Validate(p);
        var geometry=E("library_geometries"); var controllers=E("library_controllers"); var scene=E("visual_scene",A("id","Scene"));
        for(int mi=0;mi<p.Meshes.Count;mi++)
        {
            var m=p.Meshes[mi]; string id=$"mesh{mi}",skin=$"skin{mi}";
            geometry.Add(E("geometry",A("id",id),A("name",m.Name),E("mesh",
                Source(id+"-pos",m.Vertices.SelectMany(v=>v.Position),3,["X","Y","Z"]),
                Source(id+"-norm",m.Vertices.SelectMany(v=>v.Normal),3,["X","Y","Z"]),
                Source(id+"-uv",m.Vertices.SelectMany(v=>v.UV),2,["S","T"]),
                E("vertices",A("id",id+"-verts"),E("input",A("semantic","POSITION"),A("source","#"+id+"-pos"))),
                E("triangles",A("count",m.Indices.Count/3),A("material","neutral"),
                    E("input",A("semantic","VERTEX"),A("source","#"+id+"-verts"),A("offset",0)),
                    E("input",A("semantic","NORMAL"),A("source","#"+id+"-norm"),A("offset",1)),
                    E("input",A("semantic","TEXCOORD"),A("source","#"+id+"-uv"),A("offset",2),A("set",0)),
                    E("p",string.Join(" ",m.Indices.SelectMany(i=>new[]{i,i,i})))))));
            if(m.NativeAttributes.TryGetValue("BrawlUV1",out var uv1))
            {
                var mesh=geometry.Elements().Last().Element(Ns+"mesh")!;var triangles=mesh.Element(Ns+"triangles")!;
                mesh.AddFirst(Source(id+"-uv1",uv1.SelectMany(v=>v),2,["S","T"]));
                triangles.AddFirst(E("input",A("semantic","TEXCOORD"),A("source","#"+id+"-uv1"),A("offset",3),A("set",1)));
                triangles.Element(Ns+"p")!.Value=string.Join(" ",m.Indices.SelectMany(i=>new[]{i,i,i,i}));
            }
            // Omit zero influences: BrawlLib reduces influences after choosing rigid vs blended
            // storage, so sending an explicit zero can select the wrong storage convention.
            var influences=m.Vertices.Select(v=>v.Bones.Zip(v.Weights).Where(w=>w.Second>0).ToArray()).ToArray();
            var weights=influences.SelectMany(v=>v.Select(w=>w.Second)).ToArray(); int wi=0;
            var pairs=influences.SelectMany(v=>v.Select(w=>$"{w.First} {wi++}")).ToArray();
            controllers.Add(E("controller",A("id",skin),E("skin",A("source","#"+id),
                E("bind_shape_matrix",Matrix(Matrix4x4.Identity)),
                E("source",A("id",skin+"-joints"),E("Name_array",A("id",skin+"-names"),A("count",p.Bones.Count),string.Join(" ",p.Bones.Select(b=>b.Name))),E("technique_common",E("accessor",A("source","#"+skin+"-names"),A("count",p.Bones.Count),A("stride",1),E("param",A("name","JOINT"),A("type","Name"))))),
                Source(skin+"-binds",p.Bones.SelectMany(b=>Animation.A(Matrix4x4.Transpose(Animation.M(b.InverseBind)))),16,["TRANSFORM"]),
                Source(skin+"-weights",weights,1,["WEIGHT"]),
                E("joints",E("input",A("semantic","JOINT"),A("source","#"+skin+"-joints")),E("input",A("semantic","INV_BIND_MATRIX"),A("source","#"+skin+"-binds"))),
                E("vertex_weights",A("count",m.Vertices.Count),E("input",A("semantic","JOINT"),A("source","#"+skin+"-joints"),A("offset",0)),E("input",A("semantic","WEIGHT"),A("source","#"+skin+"-weights"),A("offset",1)),E("vcount",string.Join(" ",influences.Select(v=>v.Length))),E("v",string.Join(" ",pairs))))));
            scene.Add(E("node",A("id",id+"-node"),E("instance_controller",A("url","#"+skin),E("skeleton","#"+p.Bones[0].Name),E("bind_material",E("technique_common",E("instance_material",A("symbol","neutral"),A("target","#neutral")))))));
        }
        var nodes=new List<XElement>();
        for(int i=0;i<p.Bones.Count;i++)
        {
            var b=p.Bones[i]; var n=E("node",A("id",b.Name),A("sid",b.Name),A("name",b.Name),A("type","JOINT"),E("matrix",Matrix(Animation.Local(b.Scale,b.Rotation,b.Translation))));
            nodes.Add(n); if(b.Parent<0)scene.Add(n);else nodes[b.Parent].Add(n);
        }
        var doc=new XDocument(E("COLLADA",A("version","1.4.1"),E("asset",E("unit",A("name","centimeter"),A("meter","0.01")),E("up_axis","Y_UP")),
            E("library_effects",E("effect",A("id","neutral-fx"),E("profile_COMMON",E("technique",A("sid","common"),E("lambert",E("diffuse",E("color","0.2 0.65 0.8 1"))))))),
            E("library_materials",E("material",A("id","neutral"),E("instance_effect",A("url","#neutral-fx")))),geometry,controllers,E("library_visual_scenes",scene),E("scene",E("instance_visual_scene",A("url","#Scene")))));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!); doc.Save(path);
    }
}
