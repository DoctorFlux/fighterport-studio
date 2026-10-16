using System.Numerics;

namespace FighterPort.Core;

public static class Demo
{
    public static CharacterProject Create(int variant=0)
    {
        var p=new CharacterProject{Name=variant==0?"Aster • original humanoid":variant==1?"Orion • alternate rig":"Comet • extra tail chain"};
        int Add(string name,int parent,Vector3 position)
        {
            int i=p.Bones.Count;
            p.Bones.Add(new(){Id=$"original-{variant}-{i}",Name=variant==1?$"joint_{i:D2}":name,Parent=parent,NativeIndex=i,Translation=Animation.A(position)}); return i;
        }
        float width=variant==1?1.3f:1,height=variant==1?.8f:1;
        int root=Add("Root",-1,new(0,0,0));
        if(variant==1) p.Bones[root].Rotation=[0,25,0];
        int pelvis=Add("Pelvis",root,new(0,90*height,0));
        int spine=Add("Spine",pelvis,new(0,14*height,0));
        int chest=Add("Chest",spine,new(0,20*height,0));
        int neck=Add("Neck",chest,new(0,20*height,0));
        int head=Add("Head",neck,new(0,10*height,0));
        Add("Crown",head,new(0,17*height,0));
        var arms=new List<int>(); var legs=new List<int>(); var knees=new List<int>();
        foreach(int side in new[]{-1,1})
        {
            string s=side<0?"Left":"Right";
            int clavicle=Add(s+"Clavicle",chest,new(side*10*width,9*height,0));
            int upper=Add(s+"UpperArm",clavicle,new(side*11*width,0,0)); arms.Add(upper);
            int fore=Add(s+"Forearm",upper,new(side*25*width,-3,0));
            int hand=Add(s+"Hand",fore,new(side*23*width,0,0));
            for(int finger=0;finger<5;finger++)
            {
                int parent=hand;
                for(int segment=0;segment<3;segment++) parent=Add(s+(finger==0?"Thumb":$"Finger{finger}")+segment,parent,
                    new(side*(segment==0?6:3),finger==0?-3:0,segment==0?(finger-2)*2:0));
            }
            int thigh=Add(s+"Thigh",pelvis,new(side*9*width,-4,0)); legs.Add(thigh);
            int shin=Add(s+"Shin",thigh,new(0,-40*height,0)); knees.Add(shin);
            int foot=Add(s+"Foot",shin,new(0,-39*height,0));
            Add(s+"Toe",foot,new(0,-5,14));
        }
        if(variant==2)
        {
            int parent=pelvis;
            for(int i=0;i<5;i++) parent=Add($"Tail{i}",parent,new(0,i==0?-3:2,-15));
        }
        Animation.SetInverseBinds(p); var world=Animation.World(p,null,0);
        for(int i=2;i<p.Bones.Count;i++)
        {
            var b=p.Bones[i]; var a=world[b.Parent].Translation; var end=world[i].Translation;
            string n=variant==1?"":b.Name;
            float radius=i<=4?12:i==6?11:n.Contains("Finger")||n.Contains("Thumb")?1.1f:n.Contains("Tail")?3:5;
            string region=i is 5 or 6||n.Contains("Hand")||n.Contains("Finger")||n.Contains("Thumb")?"skin":"cloth";
            var mesh=new Mesh{Name=$"segment-{i}",Region=region,Color=region=="skin"?[.95f,.66f,.4f]:[.12f,.6f,.76f]};
            Tube(mesh,a,end,radius,b.Parent,i); p.Meshes.Add(mesh);
        }
        void Track(Clip clip,int bone,int channel,params float[] values)
        {
            var c=new Curve{Bone=p.Bones[bone].Name,Channel=channel};
            for(int i=0;i<values.Length;i++) c.Keys.Add(new(){Frame=i*60f/(values.Length-1),Value=values[i],Tangent=0});
            clip.Curves.Add(c);
        }
        foreach(string name in new[]{"Idle","Walk","Punch","Crouch","Jump","Full spin","Hold","Scale","Tangent"})
        {
            var c=new Clip{Name=name,Loop=name is "Idle" or "Walk",SkeletonFingerprint=Animation.Fingerprint(p)};
            switch(name)
            {
                case "Idle": Track(c,chest,5,-2,2,-2); break;
                case "Walk": for(int i=0;i<2;i++) { float s=i==0?1:-1; Track(c,legs[i],3,25*s,-25*s,25*s); Track(c,arms[i],3,-25*s,25*s,-25*s); } break;
                case "Punch": Track(c,arms[1],4,0,-70,0); Track(c,chest,4,0,18,0); break;
                case "Crouch": Track(c,pelvis,7,90*height,60*height,90*height); foreach(int b in legs) Track(c,b,3,0,-50,0); foreach(int b in knees) Track(c,b,3,0,70,0); break;
                case "Jump": Track(c,root,7,0,45,0); Track(c,chest,3,0,-12,0); break;
                case "Full spin": Track(c,root,4,0,360,720); foreach(var k in c.Curves[0].Keys) k.Tangent=12; break;
                case "Hold": Track(c,arms[0],5,0,55,0); c.Curves[0].Interpolation="Step"; break;
                case "Scale": Track(c,chest,0,1,1.2f,1); break;
                case "Tangent": Track(c,chest,5,0,45,0); c.Curves[0].Keys[0].Tangent=2; c.Curves[0].Keys[1].Tangent=-1; break;
            }
            p.Clips.Add(c);
        }
        p.Limitations.Add("Original generic rig, not a game reference skeleton. Neutral procedural checker materials.");
        ProjectStore.Validate(p); return p;
    }
    private static void Tube(Mesh mesh,Vector3 start,Vector3 end,float radius,int parent,int child)
    {
        const int sides=8,rings=5;
        var axis=Vector3.Normalize(end-start); var u=Vector3.Normalize(Vector3.Cross(axis,Math.Abs(axis.Y)>.9?Vector3.UnitX:Vector3.UnitY)); var v=Vector3.Cross(axis,u);
        for(int r=0;r<rings;r++) for(int s=0;s<sides;s++)
        {
            float t=r/(float)(rings-1),angle=s*MathF.Tau/sides; var normal=u*MathF.Cos(angle)+v*MathF.Sin(angle);
            mesh.Vertices.Add(new(){Position=Animation.A(Vector3.Lerp(start,end,t)+normal*radius),Normal=Animation.A(normal),UV=[s/(float)sides,t],Bones=[parent,child],Weights=[1-.5f*t,.5f*t]});
        }
        for(int r=0;r<rings-1;r++) for(int s=0;s<sides;s++)
        {
            int a=r*sides+s,b=r*sides+(s+1)%sides,c=a+sides,d=b+sides;
            mesh.Indices.AddRange([a,b,c,b,d,c]);
        }
        // Closed end caps use existing ring vertices; intentionally simple procedural normals.
        for(int s=1;s<sides-1;s++) { mesh.Indices.AddRange([0,s+1,s]); int o=(rings-1)*sides; mesh.Indices.AddRange([o,o+s,o+s+1]); }
    }
}
