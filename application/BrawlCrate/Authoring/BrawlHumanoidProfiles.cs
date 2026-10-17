// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
// Native flags, bone naming templates and geometry are interpreted only in this adapter.
using BrawlCrate.Authoring.Humanoid;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BrawlCrate.Authoring
{
    internal static partial class BrawlProjectAdapter
    {
        internal static HRig CommonRig(SourceCharacterProfile source)
        {
            var rig=new HRig{Fingerprint=source.RigFingerprint,Interpretation=source.Interpretation};
            foreach(var bone in source.Bones)
            {
                var v=bone.Rest.Split(',').Select(s=>double.Parse(s,CultureInfo.InvariantCulture)).ToArray();
                if(v.Length!=9 || v.Any(x=>!HVec.Finite(x)) || v.Take(3).Any(x=>Math.Abs(x-1)>1e-6) || (bone.Flags&0xE0)!=0 || bone.Billboard!=0)throw new IOException("Humanoid Brawl adapter needs unit rest scale and explicitly unsupported compensation/billboard semantics resolved first.");
                rig.Bones.Add(new HBone{Parent=bone.Parent,Rest=new HPose{P=new HVec(v[6],v[7],v[8]),R=NativeRotation(v[3],v[4],v[5])}});
            }
            rig.Validate();return rig;
        }
        private static HRotation NativeRotation(double x,double y,double z)
        {var r=RigidTransform.From(new RigPose{RX=x,RY=y,RZ=z});return new HRotation{V=r.R};}
        internal static HumanoidProfile SuggestHumanoid(SourceCharacterProfile source)
        {
            var rig=CommonRig(source);var world=rig.World(rig.Rest());
            var profile=new HumanoidProfile{Id=CharacterProject.Hash(System.Text.Encoding.UTF8.GetBytes("humanoid-rest-world-v1:"+source.RigFingerprint+":"+source.Interpretation)).Substring(0,32),RigFingerprint=source.RigFingerprint,Interpretation=source.Interpretation};
            var used=new HashSet<int>();
            Action<string,string[][]> chain=(role,aliases)=>{
                var indices=new List<int>();foreach(var options in aliases){var found=source.Bones.Select((bone,index)=>new{bone,index}).Where(n=>options.Contains(n.bone.Name,StringComparer.OrdinalIgnoreCase)).ToArray();if(found.Length==1 && !used.Contains(found[0].index))indices.Add(found[0].index);}
                if(indices.Count==0)return;
                bool ordered=true;for(int i=1;i<indices.Count;i++){int p=indices[i];while(p>=0 && p!=indices[i-1])p=rig.Bones[p].Parent;if(p<0)ordered=false;}
                double span=(world[indices.Last()].P-world[indices[0]].P).Length;
                var item=new HChain{Role=role,Bones=indices,Confidence=ordered && (indices.Count==1 || span>1e-5)?.9:.25,Evidence="Brawl/common aliases; "+(ordered?"hierarchy consistent":"hierarchy conflict")+"; rest span="+span.ToString("R",CultureInfo.InvariantCulture)+" native units. Proposal only; no human approval."};profile.Chains.Add(item);foreach(int i in indices)used.Add(i);
            };
            profile.Chains.Add(new HChain{Role="Root",Bones=new List<int>{0},Confidence=.9,Evidence="Single parent-first actor root; proposal, not semantic review."});used.Add(0);
            chain("MotionRoot",new[]{new[]{"TransN","MotionRoot"}});chain("RootFrame",new[]{new[]{"XRotN"},new[]{"YRotN"}});
            chain("Pelvis",new[]{new[]{"HipN","Pelvis","Hips"}});
            chain("Spine",new[]{new[]{"WaistN","BodyN","Spine"},new[]{"Spine1"},new[]{"BustN","Chest"}});
            chain("Neck",new[]{new[]{"NeckN","Neck"}});chain("Head",new[]{new[]{"HeadN","Head"}});
            foreach(string side in new[]{"Left","Right"})
            {
                string p=side=="Left"?"L":"R";
                chain(side+"Clavicle",new[]{new[]{p+"ShoulderN",side+"Clavicle"}});
                chain(side+"Arm",new[]{new[]{p+"ShoulderJ",side+"UpperArm"},new[]{p+"ArmJ",side+"Forearm"},new[]{p+"HandN",side+"Hand"}});
                chain(side+"Leg",new[]{new[]{p+"LegJ",side+"Thigh"},new[]{p+"KneeJ",side+"Shin"},new[]{p+"FootJ",side+"Foot"}});
                chain(side+"Toe",new[]{new[]{p+"ToeN",side+"Toe"}});
                for(int finger=0;finger<5;finger++)
                {
                    string common=new[]{"Index","Middle","Ring","Little","Thumb"}[finger],native=finger==4?"Thumb":new[]{"1st","2nd","3rd","4th"}[finger];
                    chain(side+common,new[]{new[]{p+native+"Na",side+common+"1"},new[]{p+native+"Nb",side+common+"2"},new[]{side+common+"3"}});
                }
                chain(side+"Attachment",new[]{new[]{p+"HaveN",side+"Attachment"}});
            }
            foreach(int i in Enumerable.Range(0,source.Bones.Count).Where(i=>!used.Contains(i)))profile.Optional.Add(new HOptionalBone{Bone=i,Evidence="Unresolved/helper/attachment: target native reference is preserved; source motion is explicitly reported, not transferred."});
            return profile;
        }
        internal static BodyProfile MeasureBody(ResourceNode root,SourceCharacterProfile source,HumanoidProfile profile)
        {
            var rig=CommonRig(source);profile.Validate(rig,false);var world=rig.World(rig.Rest());var model=(MDL0Node)At(root,source.ModelPath);model.Populate();model.ApplyCHR(null,0);
            var body=new BodyProfile{Id=CharacterProject.Hash(System.Text.Encoding.UTF8.GetBytes("body-v1:"+source.RigFingerprint+":"+source.ModelExportHash)).Substring(0,32),RigFingerprint=source.RigFingerprint,ModelHash=source.ModelExportHash};
            var points=new List<Tuple<HVec,int[]>>();
            foreach(var mesh in model._objList.OfType<MDL0ObjectNode>())foreach(var vertex in mesh.Vertices)
            {
                var influence=vertex.GetMatrixNode();if(influence==null)continue;var v=vertex.WeightedPosition;
                points.Add(Tuple.Create(new HVec(v._x,v._y,v._z),influence.Weights.Where(w=>w.Weight>.25).Select(w=>source.Bones.FindIndex(b=>b.Name==w.Bone.Name)).ToArray()));
            }
            double ground=double.PositiveInfinity,legLength=0;
            foreach(string side in new[]{"Left","Right"})
            {
                var leg=profile.Chain(side+"Leg").Bones;legLength+=(world[leg[1]].P-world[leg[0]].P).Length+(world[leg[2]].P-world[leg[1]].P).Length;
                var toes=profile.Chains.SingleOrDefault(c=>c.Role==side+"Toe")?.Bones??new List<int>();
                var footPoints=points.Where(p=>p.Item2.Contains(leg[2]) || p.Item2.Any(i=>toes.Contains(i))).Select(p=>p.Item1).ToArray();
                if(footPoints.Length<3)throw new IOException("Review sole landmark manually: insufficient skinned foot/toe geometry for "+side+".");
                double floor=footPoints.Min(p=>p.Y);ground=Math.Min(ground,floor);var sole=new HVec((footPoints.Min(p=>p.X)+footPoints.Max(p=>p.X))/2,floor,(footPoints.Min(p=>p.Z)+footPoints.Max(p=>p.Z))/2);
                body.Landmarks.Add(new BodyLandmark{Role=side+"Leg",Local=world[leg[2]].R.Inverse().Apply(sole-world[leg[2]].P),Pole=new HVec(0,0,1),Evidence="Conservative sole center/min-Y from vertices with >.25 foot/toe influence; Y-up proposal, requires review."});
                body.Landmarks.Add(new BodyLandmark{Role=side+"Arm",Local=new HVec(),Pole=new HVec(0,0,-1),Evidence="Wrist origin; grip offset not yet characterized. Explicit review/edit required."});
            }
            body.LegLength=legLength/2;body.GroundY=ground;body.PelvisLimit=body.LegLength*.2;
            var head=world[profile.Chain("Head").Bones.Last()].P;var chest=world[profile.Chain("Spine").Bones.Last()].P;
            int headBone=profile.Chain("Head").Bones.Last(),chestBone=profile.Chain("Spine").Bones.Last();
            body.HeadRadius=points.Where(p=>p.Item2.Contains(headBone)).Select(p=>(p.Item1-head).Length).DefaultIfEmpty(0).Max();
            body.TorsoRadius=points.Where(p=>p.Item2.Contains(chestBone)).Select(p=>Math.Sqrt(Math.Pow(p.Item1.X-chest.X,2)+Math.Pow(p.Item1.Z-chest.Z,2))).DefaultIfEmpty(0).Max();
            body.ShoulderWidth=(world[profile.Chain("LeftArm").Bones[0]].P-world[profile.Chain("RightArm").Bones[0]].P).Length;
            body.PelvisWidth=(world[profile.Chain("LeftLeg").Bones[0]].P-world[profile.Chain("RightLeg").Bones[0]].P).Length;
            body.TorsoHeight=(chest-world[profile.Chain("Pelvis").Bones[0]].P).Length;body.Validate(rig,false);return body;
        }
    }
}
