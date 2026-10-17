// Original procedural humanoid artwork/motions for FighterPort, 2026-10-17.
// Permission to use/modify/redistribute this generator and its original output for
// testing is granted with this notice retained. No commercial anatomy/data is embedded.
using BrawlCrate.Authoring.Humanoid;
using BrawlLib.Modeling.Collada;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using BrawlLib.Wii.Graphics;
using BrawlLib.Wii.Textures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

internal static class OriginalHumanoidFixture
{
    internal sealed class Bone {public string Name;public int Parent;public HVec P;public double RZ;}
    internal static List<Bone> Define(bool stout)
    {
        var b=new List<Bone>();Action<string,string,HVec,double> add=(name,parent,p,rz)=>b.Add(new Bone{Name=name,Parent=parent==null?-1:b.FindIndex(n=>n.Name==parent),P=p,RZ=rz});
        double first=stout?1.6:4.5,second=stout?3.9:5,height=first+second+.5,hip=stout?1.5:1;
        add("Root",null,new HVec(),0);add("MotionRoot","Root",new HVec(),0);add("Pelvis","MotionRoot",new HVec(0,height,0),0);add("Spine","Pelvis",new HVec(0,stout?.8:1.6,0),0);
        if(stout)add("Spine1","Spine",new HVec(0,.6,0),0);add("Chest",stout?"Spine1":"Spine",new HVec(0,stout?.7:1.7,0),0);add("Neck","Chest",new HVec(0,1,0),0);add("Head","Neck",new HVec(0,stout?1.2:1,0),0);
        foreach(string side in new[]{"Left","Right"})
        {
            double sign=side=="Left"?1:-1;
            add(side+"Clavicle","Chest",new HVec(sign*(stout?1.3:.6),.5,0),0);add(side+"UpperArm",side+"Clavicle",new HVec(sign*.7,0,0),stout?90:0);
            add(side+"Forearm",side+"UpperArm",stout?new HVec(0,-sign*1.7,0):new HVec(sign*3,0,0),stout?-90:0);
            string parent=side+"Forearm";if(stout){add(side+"TwistHelper",parent,new HVec(),0);parent=side+"TwistHelper";}
            add(side+"Hand",parent,new HVec(sign*(stout?1.3:3),0,0),0);
            foreach(string finger in new[]{"Index","Middle","Ring","Little","Thumb"})
            {
                int f=Array.IndexOf(new[]{"Index","Middle","Ring","Little","Thumb"},finger);add(side+finger+"1",side+"Hand",new HVec(sign*.45,0,(f-2)*.18),0);add(side+finger+"2",side+finger+"1",new HVec(sign*.35,0,0),0);if(stout)add(side+finger+"3",side+finger+"2",new HVec(sign*.2,0,0),0);
            }
            add(side+"Thigh","Pelvis",new HVec(sign*hip,0,0),0);add(side+"Shin",side+"Thigh",new HVec(0,-first,0),0);add(side+"Foot",side+"Shin",new HVec(0,-second,0),0);add(side+"Toe",side+"Foot",new HVec(0,-.2,1),0);
        }
        return b;
    }
    private static string F(double v)=>v.ToString("R",CultureInfo.InvariantCulture);
    private static string V(HVec p)=>F(p.X)+" "+F(p.Y)+" "+F(p.Z);
    private static HPose Local(Bone b)=>new HPose{P=b.P,R=HRotation.Axis(new HVec(0,0,1),b.RZ*Math.PI/180)};
    private static string Matrix(HPose p)
    {var r=p.R.V;return string.Join(" ",new[]{r[0],r[1],r[2],p.P.X,r[3],r[4],r[5],p.P.Y,r[6],r[7],r[8],p.P.Z,0,0,0,1}.Select(F));}
    internal static string Create(string directory,bool stout,int version=9,bool volumeVariant=false)
    {
        Directory.CreateDirectory(directory);var bones=Define(stout);var world=new HPose[bones.Count];for(int i=0;i<bones.Count;i++)world[i]=bones[i].Parent<0?Local(bones[i]):world[bones[i].Parent]*Local(bones[i]);
        var positions=new List<HVec>();var normals=new List<HVec>();var joints=new List<int>();var rigid=new List<bool>();var triangles=new List<int>();
        int[][] faces={new[]{0,1,2,3},new[]{5,4,7,6},new[]{4,0,3,7},new[]{1,5,6,2},new[]{3,2,6,7},new[]{4,5,1,0}};HVec[] ns={new HVec(0,0,-1),new HVec(0,0,1),new HVec(-1,0,0),new HVec(1,0,0),new HVec(0,1,0),new HVec(0,-1,0)};
        for(int bone=2;bone<bones.Count;bone++)
        {
            string name=bones[bone].Name;var center=world[bone].P;var half=new HVec(.16,.16,.16);bool fixedSkin=name.EndsWith("Foot")||name.EndsWith("Toe")||name.EndsWith("Hand");
            if(name=="Pelvis"||name=="Spine"||name=="Spine1"||name=="Chest")half=new HVec(stout?(volumeVariant?1.9:1.45):.7,.5,stout?.85:.4);
            if(name=="Head")half=new HVec(stout?(volumeVariant?1.9:1.5):.8,stout?1.5:1,stout?1.25:.7);
            if(name.EndsWith("Foot")){center+=new HVec(0,-.25,.4);half=new HVec(stout?.65:.45,.25,stout?1:.75);}
            if(name.EndsWith("Hand"))half=new HVec(stout?.5:.3,.25,.4);
            string next=name.EndsWith("UpperArm")?name.Replace("UpperArm","Forearm"):name.EndsWith("Forearm")?name.Replace("Forearm","Hand"):name.EndsWith("Thigh")?name.Replace("Thigh","Shin"):name.EndsWith("Shin")?name.Replace("Shin","Foot"):null;
            if(next!=null){var end=world[bones.FindIndex(b=>b.Name==next)].P;center=(center+end)*.5;var extent=end-world[bone].P;half=new HVec(Math.Max(.2,Math.Abs(extent.X)/2),Math.Max(.2,Math.Abs(extent.Y)/2),Math.Max(.2,Math.Abs(extent.Z)/2));}
            var corners=new[]{new HVec(-1,-1,-1),new HVec(1,-1,-1),new HVec(1,1,-1),new HVec(-1,1,-1),new HVec(-1,-1,1),new HVec(1,-1,1),new HVec(1,1,1),new HVec(-1,1,1)}.Select(p=>center+new HVec(p.X*half.X,p.Y*half.Y,p.Z*half.Z)).ToArray();
            for(int face=0;face<6;face++){int start=positions.Count;foreach(int corner in faces[face]){positions.Add(corners[corner]);normals.Add(ns[face]);joints.Add(bone);rigid.Add(fixedSkin);}triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});}
        }
        string Source(string id,string type,string data,int count,int stride,string param)=>"<source id=\""+id+"\"><"+type+"_array id=\""+id+"-array\" count=\""+(count*stride)+"\">"+data+"</"+type+"_array><technique_common><accessor source=\"#"+id+"-array\" count=\""+count+"\" stride=\""+stride+"\"><param name=\""+param+"\" type=\""+(stride==16?"float4x4":type=="Name"?"Name":"float")+"\"/></accessor></technique_common></source>";
        var s=new StringBuilder("<?xml version=\"1.0\"?><COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\"><asset><unit name=\"native\" meter=\"1\"/><up_axis>Y_UP</up_axis></asset>");
        s.Append("<library_effects><effect id=\"fx\"><profile_COMMON><technique sid=\"common\"><lambert><diffuse><color>1 1 1 1</color></diffuse></lambert></technique></profile_COMMON></effect></library_effects><library_materials><material id=\"material\"><instance_effect url=\"#fx\"/></material></library_materials>");
        s.Append("<library_geometries><geometry id=\"geometry\"><mesh>");s.Append(Source("positions","float",string.Join(" ",positions.Select(V)),positions.Count,3,"X"));s.Append(Source("normals","float",string.Join(" ",normals.Select(V)),normals.Count,3,"X"));s.Append(Source("uv","float",string.Join(" ",positions.Select((p,i)=>i%4==0?"0 0":i%4==1?"1 0":i%4==2?"1 1":"0 1")),positions.Count,2,"S"));
        s.Append("<vertices id=\"vertices\"><input semantic=\"POSITION\" source=\"#positions\"/></vertices><triangles count=\""+(triangles.Count/3)+"\" material=\"material\"><input semantic=\"VERTEX\" source=\"#vertices\" offset=\"0\"/><input semantic=\"NORMAL\" source=\"#normals\" offset=\"1\"/><input semantic=\"TEXCOORD\" source=\"#uv\" offset=\"2\" set=\"0\"/><p>"+string.Join(" ",triangles.Select(i=>i+" "+i+" "+i))+"</p></triangles></mesh></geometry></library_geometries>");
        s.Append("<library_controllers><controller id=\"skin\"><skin source=\"#geometry\"><bind_shape_matrix>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</bind_shape_matrix>");s.Append(Source("joints","Name",string.Join(" ",bones.Select(b=>b.Name)),bones.Count,1,"JOINT"));
        s.Append(Source("binds","float",string.Join(" ",world.Select(p=>Matrix(new HPose{R=p.R.Inverse(),P=p.R.Inverse().Apply(p.P)*-1}))),bones.Count,16,"TRANSFORM"));s.Append(Source("weights","float","0.25 0.75 1",3,1,"WEIGHT"));
        s.Append("<joints><input semantic=\"JOINT\" source=\"#joints\"/><input semantic=\"INV_BIND_MATRIX\" source=\"#binds\"/></joints><vertex_weights count=\""+positions.Count+"\"><input semantic=\"JOINT\" source=\"#joints\" offset=\"0\"/><input semantic=\"WEIGHT\" source=\"#weights\" offset=\"1\"/><vcount>"+string.Join(" ",rigid.Select(r=>r?"1":"2"))+"</vcount><v>"+string.Join(" ",joints.Select((b,i)=>rigid[i]?b+" 2":bones[b].Parent+" 0 "+b+" 1"))+"</v></vertex_weights></skin></controller></library_controllers>");
        s.Append("<library_visual_scenes><visual_scene id=\"Scene\">");Action<int> node=null;node=i=>{var b=bones[i];s.Append("<node id=\""+b.Name+"\" name=\""+b.Name+"\" sid=\""+b.Name+"\" type=\"JOINT\"><matrix>"+Matrix(Local(b))+"</matrix>");for(int j=0;j<bones.Count;j++)if(bones[j].Parent==i)node(j);s.Append("</node>");};node(0);
        s.Append("<node id=\"mesh\"><instance_controller url=\"#skin\"><skeleton>#Root</skeleton><bind_material><technique_common><instance_material symbol=\"material\" target=\"#material\"/></technique_common></bind_material></instance_controller></node></visual_scene></library_visual_scenes><scene><instance_visual_scene url=\"#Scene\"/></scene></COLLADA>");string dae=Path.Combine(directory,"original-humanoid.dae");File.WriteAllText(dae,s.ToString());
        Collada._importOptions=new Collada.ImportOptions{ModelVersion=version,SetOriginalPath=false,BlenderFBXBoneFix=false,ForceFloatVertices=true,ForceFloatNormals=true,ForceFloatUVs=true};
        using(var importer=new Collada())using(var archive=new BRRESNode{Name="OriginalHumanoid"})
        {
            var model=importer.ImportModel(dae,Collada.ImportType.MDL0) as MDL0Node??throw new IOException("Original humanoid import: "+Collada.Error);model.Name=stout?"OriginalStout":"OriginalTall";archive.GetOrCreateFolder<MDL0Node>().AddChild(model);model.Populate();
            using(var bitmap=new Bitmap(32,32)){for(int y=0;y<32;y++)for(int x=0;x<32;x++)bitmap.SetPixel(x,y,x<16?Color.CornflowerBlue:Color.Orange);var tex=archive.CreateResource<TEX0Node>("OriginalHumanoidColor");tex.ReplaceRaw(TextureConverter.RGBA8.EncodeTEX0Texture(bitmap,1));}
            foreach(MDL0MaterialNode material in model._matList){var sampler=material.Children.OfType<MDL0MaterialRefNode>().FirstOrDefault();if(sampler==null){sampler=new MDL0MaterialRefNode();material.AddChild(sampler);}sampler.Texture="OriginalHumanoidColor";sampler.Coordinates=TexSourceRow.TexCoord0;material.CullMode=CullMode.Cull_None;}
            var motion=archive.CreateResource<CHR0Node>("OriginalPlanted");motion.Version=4;motion.FrameCount=31;motion.Loop=false;
            foreach(var bone in model.BoneCache.OfType<MDL0BoneNode>())
            {
                var entry=motion.CreateEntry(bone.Name);entry.PreserveInterpolation=true;var rest=bone.BindState;var values=new float[31][];
                for(int f=0;f<31;f++)
                {
                    values[f]=new[]{1f,1,1,rest._rotate._x,rest._rotate._y,rest._rotate._z,rest._translate._x,rest._translate._y,rest._translate._z};double depth=.8*Math.Pow(Math.Sin(f*Math.PI/30),2),l1=stout?1.6:4.5,l2=stout?3.9:5,d=l1+l2-depth;
                    double alpha=Math.Acos(Math.Max(-1,Math.Min(1,(l1*l1+d*d-l2*l2)/(2*l1*d))))*180/Math.PI,beta=Math.Acos(Math.Max(-1,Math.Min(1,(l2*l2+d*d-l1*l1)/(2*l2*d))))*180/Math.PI;
                    if(bone.Name=="Pelvis")values[f][7]-=(float)depth;if(bone.Name.EndsWith("Thigh"))values[f][3]=(float)-alpha;if(bone.Name.EndsWith("Shin"))values[f][3]=(float)(alpha+beta);if(bone.Name.EndsWith("Foot"))values[f][3]=(float)-beta;
                    if(bone.Name.Contains("Index")||bone.Name.Contains("Thumb"))values[f][5]+=(float)(20*Math.Sin(f*Math.PI/30));
                }
                for(int channel=0;channel<9;channel++)for(int f=0;f<31;f++){var key=entry.SetKeyframe(channel,f,values[f][channel],true);int a=Math.Max(0,f-1),b=Math.Min(30,f+1);key._tangent=bone.Name=="Pelvis" && channel==7?(float)(-.8*Math.PI/30*Math.Sin(2*f*Math.PI/30)):(values[b][channel]-values[a][channel])/(b-a);}
            }
            foreach(string kind in new[]{"OriginalAerial","OriginalFullTurn","OriginalLoop"})
            {
                var extra=archive.CreateResource<CHR0Node>(kind);extra.Version=4;extra.FrameCount=31;extra.Loop=kind=="OriginalLoop";
                foreach(var bone in model.BoneCache.OfType<MDL0BoneNode>())
                {
                    var entry=extra.CreateEntry(bone.Name);entry.PreserveInterpolation=true;var rest=bone.BindState;
                    for(int channel=0;channel<9;channel++)for(int f=0;f<31;f++)
                    {
                        Func<int,float> value=k=>{float v=channel<3?1:channel==3?rest._rotate._x:channel==4?rest._rotate._y:channel==5?rest._rotate._z:channel==6?rest._translate._x:channel==7?rest._translate._y:rest._translate._z;
                            if(kind=="OriginalAerial" && bone.Name=="Pelvis" && channel==7)v+=(float)(2*Math.Pow(Math.Sin(k*Math.PI/30),2));
                            if(kind=="OriginalFullTurn" && bone.Name.EndsWith("Forearm") && channel==3)v+=k*12;
                            if(kind=="OriginalLoop" && bone.Name.Contains("Index") && channel==5)v+=(float)(20*Math.Sin(k*Math.PI/30));return v;};
                        var key=entry.SetKeyframe(channel,f,value(f),true);int a=Math.Max(0,f-1),b=Math.Min(30,f+1);key._tangent=(value(b)-value(a))/(b-a);
                    }
                }
            }
            string output=Path.Combine(directory,"original-humanoid.brres");archive.Export(output);return output;
        }
    }
}
