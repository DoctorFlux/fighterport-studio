using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FighterPort.Core;

public sealed class AnatomyProfile
{
    public int Version { get; set; }=1;
    public string Fingerprint { get; set; }="";
    public string Convention { get; set; }="RH cm Y-up; character-local negative X is left";
    public bool Approved { get; set; }
    public List<AnatomyAnnotation> Bones { get; set; }=[];
}
public sealed class AnatomyAnnotation
{
    public string BoneId { get; set; }="";
    public string Role { get; set; }="unresolved";
    public string Side { get; set; }="center";
    public string Chain { get; set; }="";
    public int Segment { get; set; }
    public string Kind { get; set; }="unknown";
    public bool Terminal { get; set; }
    public bool Excluded { get; set; }
    public bool Locked { get; set; }
    public string Confidence { get; set; }="unresolved";
    public List<string> Evidence { get; set; }=[];
    public List<string> Alternatives { get; set; }=[];
}

public static class Anatomy
{
    public static AnatomyProfile Suggest(CharacterProject p)
    {
        ProjectStore.Validate(p);var world=Animation.World(p,null,0);
        var children=Enumerable.Range(0,p.Bones.Count).Select(i=>Enumerable.Range(0,p.Bones.Count).Where(j=>p.Bones[j].Parent==i).ToArray()).ToArray();
        var result=new AnatomyProfile{Fingerprint=Animation.Fingerprint(p),Bones=p.Bones.Select((b,i)=>new AnatomyAnnotation{BoneId=b.Id,Terminal=children[i].Length==0}).ToList()};
        var weighted=p.Meshes.SelectMany(m=>m.Vertices).SelectMany(v=>v.Bones.Zip(v.Weights)).Where(x=>x.Second>0).Select(x=>x.First).ToHashSet();
        void Set(int i,string role,string side,string chain,int segment,string evidence,string confidence="tentative")
        {
            var a=result.Bones[i];if(a.Role!="unresolved"&&a.Confidence=="alias"&&confidence!="alias"){a.Chain=chain;a.Segment=segment;if(a.Role!=role)a.Alternatives.Add(role);a.Evidence.Add(evidence);return;}
            a.Role=role;a.Side=side;a.Chain=chain;a.Segment=segment;a.Confidence=confidence;
            a.Kind=weighted.Contains(i)?"deform":"unweighted/helper";a.Evidence.Add(evidence);
        }
        for(int i=0;i<p.Bones.Count;i++)
        {
            string name=Regex.Replace(p.Bones[i].Name.ToLowerInvariant(),"[^a-z0-9]","");
            string side=name.Contains("left")?"left":name.Contains("right")?"right":"center";
            if(side!="center")name=name.Replace(side,"");
            var sideMatch=Regex.Match(name,@"^(?<side>[lr])(?<part>clavicle|shoulder|upperarm|forearm|arm|hand|wrist|thigh|leg|knee|shin|foot|toe|thumb|finger)");
            if(sideMatch.Success){side=sideMatch.Groups["side"].Value=="l"?"left":"right";name=name[1..];}
            var suffix=Regex.Match(name,@"^(clavicle|shoulder|arm|hand|leg|knee|foot|toe)(l|r)$");if(suffix.Success){side=suffix.Groups[2].Value=="l"?"left":"right";name=suffix.Groups[1].Value;}
            string role=name switch {"root" or "topn"=>"root","trans" or "transn"=>"motion-root","pelvis" or "hip" or "hipn"=>"pelvis","spine" or "waist" or "waistn"=>"spine","chest" or "bust" or "bustn"=>"chest","neck" or "neckn"=>"neck","head" or "headn"=>"head","shouldern"=>"clavicle","shoulderj" or "shoulder"=>"upperarm","armj" or "arm"=>"forearm","legj" or "leg"=>"thigh",_=>""};
            string chain="torso";int segment=0;
            foreach(var pair in new[]{("clavicle","clavicle"),("upperarm","upperarm"),("forearm","forearm"),("wrist","hand"),("hand","hand"),("thigh","thigh"),("shin","shin"),("knee","shin"),("foot","foot"),("toe","toes.base")})
                if(name.StartsWith(pair.Item1)&&!name.Contains("end")&&!(name.StartsWith("foot")&&name.EndsWith("e"))){role=pair.Item2;chain=role is "thigh" or "shin" or "foot" or "toes.base"?"leg":"arm";break;}
            var finger=Regex.Match(name,@"^(thumb|finger[1-4])(\d+)$");
            if(finger.Success){role=finger.Groups[1].Value;segment=int.Parse(finger.Groups[2].Value);chain="hand."+role;}
            foreach(string extra in new[]{"tail","wing","hair","face","grab","attachment","twist"})if(name.Contains(extra)){role=extra;chain=extra;}
            if(name.Contains("breast")){role="accessory";chain="accessory";}
            if(role!="")Set(i,role,side,chain,segment,"Name alias; review against hierarchy", "alias");
        }
        // Conservative topology fallback for unnamed humanoids. Character-local positions remove root orientation.
        int root=p.Bones.FindIndex(b=>b.Parent<0);
        if(root>=0)
        {
            Matrix4x4.Invert(world[root],out var rootInverse);var positions=world.Select(w=>Vector3.Transform(w.Translation,rootInverse)).ToArray();
            Vector3 Delta(int parent,int child)=>positions[child]-positions[parent];
            int[] down(int i)=>children[i].Where(j=>Delta(i,j).Y<-.1f).ToArray();
            int pelvis=Enumerable.Range(0,p.Bones.Count).FirstOrDefault(i=>down(i).Length==2&&children[i].Any(j=>Delta(i,j).Y>.1f),-1);
            Set(root,"root","center","root",0,"Single-root ancestry", "tentative");
            if(pelvis>=0)
            {
                Set(pelvis,"pelvis","center","torso",0,"Two descending limb branches plus ascending torso");
                foreach(int thigh in down(pelvis))
                {
                    string side=positions[thigh].X<positions[pelvis].X?"left":"right";int j=thigh;
                    foreach(string role in new[]{"thigh","shin","foot","toes.base"})
                    {Set(j,role,side,"leg",Array.IndexOf(new[]{"thigh","shin","foot","toes.base"},role),"Descending leg chain; side from configured character-local X");if(children[j].Length!=1)break;j=children[j][0];}
                }
                int torso=children[pelvis].First(j=>Delta(pelvis,j).Y>.1f),level=0;
                while(true)
                {
                    var lateral=children[torso].Where(j=>Math.Abs(Delta(torso,j).X)>Math.Abs(Delta(torso,j).Y)*.75f&&Math.Abs(Delta(torso,j).X)>.1f).ToArray();
                    Set(torso,lateral.Length==2?"chest":"spine","center","torso",++level,"Ascending torso topology");
                    if(lateral.Length==2)
                    {
                        foreach(int shoulder in lateral)
                        {
                            string side=Delta(torso,shoulder).X<0?"left":"right";int j=shoulder;
                            foreach(string role in new[]{"clavicle","upperarm","forearm","hand"})
                            {Set(j,role,side,"arm",Array.IndexOf(new[]{"clavicle","upperarm","forearm","hand"},role),"Lateral four-joint arm chain; review helper/twist alternatives");if(children[j].Length!=1)break;j=children[j][0];}
                            if(children[j].Length>=3)foreach(int f in children[j])
                            {int k=f,s=0;while(true){if(result.Bones[k].Role=="unresolved"){Set(k,"finger.unknown",side,"hand.unknown-"+f,s,"Branched hand chain; digit identity unresolved");result.Bones[k].Alternatives=["thumb","index","middle","ring","little"];}if(children[k].Length!=1)break;k=children[k][0];s++;}}
                        }
                        int neck=children[torso].FirstOrDefault(j=>!lateral.Contains(j)&&Delta(torso,j).Y>0,-1);
                        if(neck>=0){Set(neck,"neck","center","neck",0,"Ascending branch above shoulder fork");if(children[neck].Length==1)Set(children[neck][0],"head","center","neck",1,"Single child above neck; review terminal marker");}
                        break;
                    }
                    if(children[torso].Length!=1)break;torso=children[torso][0];
                }
            }
        }
        foreach(var a in result.Bones)if(a.Role=="unresolved")a.Evidence.Add("No unambiguous alias/topology evidence; manual assignment required");
        return result;
    }
    public static void Correct(AnatomyProfile profile,string boneId,string role,string side,string chain,int segment,bool excluded=false)
    {
        if(side is not ("left" or "right" or "center")||segment<0||string.IsNullOrWhiteSpace(role))throw new InvalidDataException("Invalid semantic annotation");
        var a=profile.Bones.Single(b=>b.BoneId==boneId);a.Role=role;a.Side=side;a.Chain=chain;a.Segment=segment;a.Excluded=excluded;a.Locked=true;a.Confidence="manual";a.Evidence=["User correction"];profile.Approved=false;
    }
    public static void Validate(AnatomyProfile profile,CharacterProject p)
    {
        if(profile.Version!=1||profile.Fingerprint!=Animation.Fingerprint(p)||profile.Bones.Count!=p.Bones.Count||profile.Bones.Select(b=>b.BoneId).Distinct().Count()!=p.Bones.Count||profile.Bones.Any(a=>!p.Bones.Any(b=>b.Id==a.BoneId)))throw new InvalidDataException("Semantic profile version/fingerprint/identity mismatch");
        if(profile.Bones.Any(a=>a.Side is not ("left" or "right" or "center")||a.Segment<0||string.IsNullOrWhiteSpace(a.Role)))throw new InvalidDataException("Invalid semantic annotation");
    }
    public static MappingProfile Map(CharacterProject source,AnatomyProfile a,CharacterProject target,AnatomyProfile b)
    {
        Validate(a,source);Validate(b,target);var map=FkRetarget.Template(source,target);map.BoneIds.Clear();
        foreach(var s in a.Bones.Where(x=>!x.Excluded&&x.Role!="unresolved"&&x.Role!="finger.unknown"))
        {
            var sameSource=a.Bones.Count(x=>!x.Excluded&&x.Role==s.Role&&x.Side==s.Side&&x.Segment==s.Segment);
            var candidates=b.Bones.Where(x=>!x.Excluded&&x.Role==s.Role&&x.Side==s.Side&&x.Segment==s.Segment).ToArray();
            if(sameSource==1&&candidates.Length==1)map.BoneIds.Add(s.BoneId,candidates[0].BoneId);
        }
        return map; // Suggestions always require fresh mapping approval.
    }
}
