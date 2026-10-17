// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
// Common Y-up, right-handed rigid transforms. No native-format, renderer or UI dependency.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrawlCrate.Authoring.Humanoid
{
    public struct HVec
    {
        public double X,Y,Z;
        public HVec(double x,double y,double z){X=x;Y=y;Z=z;}
        public double Length=>Math.Sqrt(Dot(this,this));
        public HVec Unit(){double n=Length;if(n<1e-9 || !Finite(n))throw new IOException("Degenerate humanoid direction.");return this/n;}
        public static HVec operator +(HVec a,HVec b)=>new HVec(a.X+b.X,a.Y+b.Y,a.Z+b.Z);
        public static HVec operator -(HVec a,HVec b)=>new HVec(a.X-b.X,a.Y-b.Y,a.Z-b.Z);
        public static HVec operator *(HVec a,double b)=>new HVec(a.X*b,a.Y*b,a.Z*b);
        public static HVec operator /(HVec a,double b)=>a*(1/b);
        public static double Dot(HVec a,HVec b)=>a.X*b.X+a.Y*b.Y+a.Z*b.Z;
        public static HVec Cross(HVec a,HVec b)=>new HVec(a.Y*b.Z-a.Z*b.Y,a.Z*b.X-a.X*b.Z,a.X*b.Y-a.Y*b.X);
        public static bool Finite(double x)=>!double.IsNaN(x)&&!double.IsInfinity(x);
        public bool IsFinite=>Finite(X)&&Finite(Y)&&Finite(Z);
    }
    public sealed class HRotation
    {
        public double[] V {get;set;}=new double[]{1,0,0,0,1,0,0,0,1};
        public HRotation Clone()=>new HRotation{V=(double[])V.Clone()};
        public HVec Apply(HVec p)=>new HVec(V[0]*p.X+V[1]*p.Y+V[2]*p.Z,V[3]*p.X+V[4]*p.Y+V[5]*p.Z,V[6]*p.X+V[7]*p.Y+V[8]*p.Z);
        public HRotation Inverse()=>new HRotation{V=new[]{V[0],V[3],V[6],V[1],V[4],V[7],V[2],V[5],V[8]}};
        public static HRotation operator *(HRotation a,HRotation b)
        {var r=new HRotation();for(int i=0;i<3;i++)for(int j=0;j<3;j++){r.V[i*3+j]=0;for(int k=0;k<3;k++)r.V[i*3+j]+=a.V[i*3+k]*b.V[k*3+j];}return r;}
        public static HRotation Axis(HVec axis,double radians)
        {axis=axis.Unit();double x=axis.X,y=axis.Y,z=axis.Z,c=Math.Cos(radians),s=Math.Sin(radians),t=1-c;return new HRotation{V=new[]{t*x*x+c,t*x*y-s*z,t*x*z+s*y,t*x*y+s*z,t*y*y+c,t*y*z-s*x,t*x*z-s*y,t*y*z+s*x,t*z*z+c}};}
        public static HRotation Between(HVec from,HVec to,HVec pole)
        {
            var a=from.Unit();var b=to.Unit();double c=Math.Max(-1,Math.Min(1,HVec.Dot(a,b)));var cross=HVec.Cross(a,b);
            if(c>1-1e-12)return new HRotation();
            if(c< -1+1e-10){var axis=HVec.Cross(a,pole);if(axis.Length<1e-7)axis=HVec.Cross(a,Math.Abs(a.X)<.8?new HVec(1,0,0):new HVec(0,1,0));return Axis(axis,Math.PI);}
            return Axis(cross,Math.Acos(c));
        }
        // Relative axis-angle interpolation. Exact half turns choose a deterministic axis.
        public static HRotation Blend(HRotation a,HRotation b,double t)
        {
            if(t<=0)return a.Clone();if(t>=1)return b.Clone();var r=b*a.Inverse();double angle=Math.Acos(Math.Max(-1,Math.Min(1,(r.V[0]+r.V[4]+r.V[8]-1)/2)));
            if(angle<1e-6)return a.Clone();HVec axis;
            if(Math.Abs(Math.PI-angle)<1e-5)
            {
                double x=Math.Sqrt(Math.Max(0,(r.V[0]+1)/2)),y=Math.Sqrt(Math.Max(0,(r.V[4]+1)/2)),z=Math.Sqrt(Math.Max(0,(r.V[8]+1)/2));
                if(x>=y && x>=z && x>1e-8){y=(r.V[1]+r.V[3])/(4*x);z=(r.V[2]+r.V[6])/(4*x);}else if(y>=z && y>1e-8){x=(r.V[1]+r.V[3])/(4*y);z=(r.V[5]+r.V[7])/(4*y);}else{x=(r.V[2]+r.V[6])/(4*z);y=(r.V[5]+r.V[7])/(4*z);}axis=new HVec(x,y,z);
            }
            else axis=new HVec(r.V[7]-r.V[5],r.V[2]-r.V[6],r.V[3]-r.V[1]);
            return Axis(axis,angle*t)*a;
        }
        public double DegreesFrom(HRotation other)
        {var r=this*other.Inverse();return Math.Acos(Math.Max(-1,Math.Min(1,(r.V[0]+r.V[4]+r.V[8]-1)/2)))*180/Math.PI;}
        public void Validate()
        {
            if(V==null || V.Length!=9 || V.Any(v=>!HVec.Finite(v)))throw new IOException("Invalid common rotation.");
            var x=new HVec(V[0],V[3],V[6]);var y=new HVec(V[1],V[4],V[7]);var z=new HVec(V[2],V[5],V[8]);
            if(Math.Abs(x.Length-1)+Math.Abs(y.Length-1)+Math.Abs(z.Length-1)+Math.Abs(HVec.Dot(x,y))+Math.Abs(HVec.Dot(x,z))+Math.Abs(HVec.Dot(y,z))>1e-4 || HVec.Dot(HVec.Cross(x,y),z)<.9999)throw new IOException("Common rotations must be orthonormal and right handed; scale/basis policies belong to adapters.");
        }
    }
    public sealed class HPose
    {
        public HVec P {get;set;}
        public HRotation R {get;set;}=new HRotation();
        public HPose Clone()=>new HPose{P=P,R=R.Clone()};
        public HVec Point(HVec local)=>P+R.Apply(local);
        public static HPose operator *(HPose a,HPose b)=>new HPose{P=a.Point(b.P),R=a.R*b.R};
    }
    public sealed class HBone
    {
        public int Parent {get;set;}
        public HPose Rest {get;set;}=new HPose();
    }
    public sealed class HRig
    {
        public string Fingerprint {get;set;}
        public string Interpretation {get;set;}
        public List<HBone> Bones {get;set;}=new List<HBone>();
        public void Validate()
        {if(Bones==null || Bones.Count<1 || Bones.Count>256 || Bones[0].Parent!=-1)throw new IOException("Humanoid rigid core supports one parent-first root / 1..256 bones.");for(int i=0;i<Bones.Count;i++){if(i>0 && (Bones[i].Parent<0 || Bones[i].Parent>=i) || !Bones[i].Rest.P.IsFinite)throw new IOException("Invalid common hierarchy/rest position.");Bones[i].Rest.R.Validate();}}
        public HPose[] Rest()=>Bones.Select(b=>b.Rest.Clone()).ToArray();
        public HPose[] World(HPose[] local)
        {if(local==null || local.Length!=Bones.Count)throw new IOException("Common pose/rig count changed.");var world=new HPose[local.Length];for(int i=0;i<local.Length;i++){if(local[i]==null || !local[i].P.IsFinite || local[i].R==null)throw new IOException("Non-finite/missing common pose.");local[i].R.Validate();world[i]=Bones[i].Parent<0?local[i].Clone():world[Bones[i].Parent]*local[i];if(!world[i].P.IsFinite)throw new IOException("Common hierarchy position overflow.");}return world;}
    }
    public sealed class HChain
    {
        public string Role {get;set;}
        public List<int> Bones {get;set;}=new List<int>();
        public double Confidence {get;set;}
        public string Evidence {get;set;}
    }
    public sealed class HOptionalBone
    {
        public int Bone {get;set;}
        public string Policy {get;set;}="KeepTargetReference; report unmapped source motion";
        public string Evidence {get;set;}
    }
    public sealed class HumanoidProfile
    {
        public string Id {get;set;}
        public int Revision {get;set;}=1;
        public string Algorithm {get;set;}="humanoid-rest-world-v1";
        public string RigFingerprint {get;set;}
        public string Interpretation {get;set;}
        public string Calibration {get;set;}="Native rest; no Idle/Fall substitution";
        public bool Reviewed {get;set;}
        public string ReviewNote {get;set;}
        public string Digest {get;set;}
        public List<HChain> Chains {get;set;}=new List<HChain>();
        public List<HOptionalBone> Optional {get;set;}=new List<HOptionalBone>();
        public HChain Chain(string role)=>Chains.SingleOrDefault(c=>c.Role==role)??throw new IOException("Unresolved humanoid chain: "+role);
        public void Validate(HRig rig,bool requireReview,bool allowIncomplete=false)
        {
            rig.Validate();if(Algorithm!="humanoid-rest-world-v1" || Revision<1 || RigFingerprint!=rig.Fingerprint || Interpretation!=rig.Interpretation || Calibration!="Native rest; no Idle/Fall substitution" || requireReview && (!Reviewed || string.IsNullOrWhiteSpace(ReviewNote)))throw new IOException("Humanoid profile needs exact-rig/calibration review.");
            if(Chains==null || Optional==null || Chains.Count>64 || Chains.Any(c=>c.Bones==null || c.Bones.Count<1 || c.Bones.Count>32) || Chains.Select(c=>c.Role).Distinct().Count()!=Chains.Count)throw new IOException("Invalid humanoid chain budget/identity.");
            var indices=Chains.SelectMany(c=>c.Bones).Concat(Optional.Select(b=>b.Bone)).ToList();
            if(!indices.OrderBy(i=>i).SequenceEqual(Enumerable.Range(0,rig.Bones.Count)))throw new IOException("Every bone needs one semantic chain or explicit optional policy; roles cannot overlap.");
            foreach(var c in Chains){if(string.IsNullOrWhiteSpace(c.Role) || c.Role.Length>80 || c.Evidence==null || c.Evidence.Length>2048 || !HVec.Finite(c.Confidence) || c.Confidence<0 || c.Confidence>1)throw new IOException("Invalid characterization evidence.");for(int i=1;i<c.Bones.Count;i++)if(!Ancestor(rig,c.Bones[i-1],c.Bones[i]))throw new IOException("Characterization chain contradicts hierarchy: "+c.Role);}
            foreach(var b in Optional)if(b.Policy!="KeepTargetReference; report unmapped source motion")throw new IOException("Unknown optional/helper policy.");
            if(allowIncomplete && !Reviewed)return;
            foreach(string role in new[]{"Root","Pelvis","Spine","Head","LeftArm","RightArm","LeftLeg","RightLeg"})Chain(role);
            foreach(string role in new[]{"LeftArm","RightArm","LeftLeg","RightLeg"})if(Chain(role).Bones.Count!=3)throw new IOException("The first two-bone solve requires reviewed start/joint/end limbs; twist helpers remain explicit optional bones.");
            if(Chain("Root").Bones.Count!=1 || Chain("Root").Bones[0]!=0 || Chain("Pelvis").Bones.Count!=1)throw new IOException("Explicit actor root and pelvis are required.");
        }
        private static bool Ancestor(HRig rig,int a,int b){for(int i=b;i>=0;i=rig.Bones[i].Parent)if(i==a)return true;return false;}
    }
    public sealed class BodyLandmark
    {
        public string Role {get;set;}
        public HVec Local {get;set;}
        public HVec Pole {get;set;}=new HVec(0,0,1);
        public string Evidence {get;set;}
    }
    public sealed class BodyProfile
    {
        public string Id {get;set;}
        public int Revision {get;set;}=1;
        public string RigFingerprint {get;set;}
        public string ModelHash {get;set;}
        public string Units {get;set;}="Native units; physical scale unknown";
        public bool Reviewed {get;set;}
        public string ReviewNote {get;set;}
        public string Digest {get;set;}
        public double GroundY {get;set;}
        public double PelvisLimit {get;set;}
        public double TorsoRadius {get;set;}
        public double HeadRadius {get;set;}
        public double LegLength {get;set;}
        public double ShoulderWidth {get;set;}
        public double PelvisWidth {get;set;}
        public double TorsoHeight {get;set;}
        public List<BodyLandmark> Landmarks {get;set;}=new List<BodyLandmark>();
        public BodyLandmark Landmark(string role)=>Landmarks.Single(l=>l.Role==role);
        public void Validate(HRig rig,bool requireReview)
        {if(Revision<1 || RigFingerprint!=rig.Fingerprint || string.IsNullOrEmpty(ModelHash) || !HVec.Finite(GroundY) || !HVec.Finite(LegLength) || LegLength<=1e-6 || !HVec.Finite(PelvisLimit) || PelvisLimit<0 || PelvisLimit>LegLength*.5 || new[]{TorsoRadius,HeadRadius,ShoulderWidth,PelvisWidth,TorsoHeight}.Any(v=>!HVec.Finite(v)||v<0) || requireReview && (!Reviewed || string.IsNullOrWhiteSpace(ReviewNote)))throw new IOException("Body profile needs exact-model measurements/review; invalid limits.");if(Landmarks==null || Landmarks.Count>16 || Landmarks.Select(l=>l.Role).Distinct().Count()!=Landmarks.Count || Landmarks.Any(l=>!l.Local.IsFinite || !l.Pole.IsFinite || l.Pole.Length<1e-6))throw new IOException("Invalid body landmarks/poles.");foreach(string role in new[]{"LeftLeg","RightLeg","LeftArm","RightArm"})Landmark(role);}
    }
    public sealed class ContactInterval
    {
        public string Chain {get;set;}
        public double Start {get;set;}
        public double End {get;set;}
        public double Margin {get;set;}
        public HVec Goal {get;set;}
        public string Anchor {get;set;}="World";
        public bool RestOrientation {get;set;}=true;
        public bool UsePoleOverride {get;set;}
        public HVec Pole {get;set;}=new HVec(0,0,1);
        public string Evidence {get;set;}="Explicit user-authored target goal";
        public double Weight(double time)
        {if(time<Start || time>End)return 0;if(Margin<=0)return 1;double t=Math.Min(1,Math.Min((time-Start)/Margin,(End-time)/Margin));return t*t*(3-2*t);}
    }
    public sealed class HumanoidStance
    {
        public double SourceTime {get;set;}
        public double TargetTime {get;set;}
        public HVec PelvisOffset {get;set;}
        public string Evidence {get;set;}
    }
    public sealed class HumanoidRecipe
    {
        public string Id {get;set;}
        public int Revision {get;set;}=1;
        public string SourceProfile {get;set;}
        public string TargetProfile {get;set;}
        public string SourceBody {get;set;}
        public string TargetBody {get;set;}
        public string SourceCharacterId {get;set;}
        public string TargetCharacterId {get;set;}
        public string AnimationId {get;set;}
        public string TargetAnimationId {get;set;}
        public string AssignmentPartId {get;set;}
        public string RootPolicy {get;set;}="InPlace";
        public string Algorithm {get;set;}="humanoid-contact-v1";
        public double PelvisBias {get;set;}
        // Optional clip-specific calibration, distinct from native binds and Idle/Fall.
        public HumanoidStance Stance {get;set;}
        public List<ContactInterval> Contacts {get;set;}=new List<ContactInterval>();
        public void Validate(int frames)
        {if(Algorithm!="humanoid-contact-v1" || RootPolicy!="InPlace" || !HVec.Finite(PelvisBias) || Contacts==null || Contacts.Count>32)throw new IOException("Unsupported humanoid recipe/root policy or contact budget.");foreach(var c in Contacts){if(!new[]{"LeftLeg","RightLeg","LeftArm","RightArm"}.Contains(c.Chain) || c.Anchor!="World" && c.Anchor!="Head" && c.Anchor!="Spine" || !HVec.Finite(c.Start) || !HVec.Finite(c.End) || !HVec.Finite(c.Margin) || c.Start<0 || c.End<c.Start || c.End>frames-1 || c.Margin<0 || c.Margin>(c.End-c.Start)/2 || !c.Goal.IsFinite || c.UsePoleOverride && (!c.Pole.IsFinite || c.Pole.Length<1e-6) || string.IsNullOrWhiteSpace(c.Evidence) || c.Evidence.Length>2048)throw new IOException("Invalid contact interval/goal/evidence.");}foreach(var group in Contacts.GroupBy(c=>c.Chain)){var sorted=group.OrderBy(c=>c.Start).ToArray();for(int i=1;i<sorted.Length;i++)if(sorted[i].Start<=sorted[i-1].End)throw new IOException("Overlapping contacts on one chain require a multi-goal policy; no silent winner.");}}
    }
    public sealed class HumanoidResult
    {
        public HPose[] Local;
        public double ContactError,PreferredError,PelvisOffset;
        public List<string> Diagnostics=new List<string>();
    }
    public static class HumanoidSolver
    {
        public static HPose[] Fk(HRig source,HumanoidProfile sp,BodyProfile sb,HRig target,HumanoidProfile tp,BodyProfile tb,HPose[] sourcePose,HPose[] targetReference)
        {
            sp.Validate(source,false);tp.Validate(target,false);sb.Validate(source,false);tb.Validate(target,false);
            var sw=source.World(sourcePose);var sr=source.World(source.Rest());var tr=target.World(target.Rest());var result=targetReference.Select(p=>p.Clone()).ToArray();var tw=new HPose[result.Length];
            var desired=new Dictionary<int,HRotation>();
            foreach(var tc in tp.Chains)
            {
                var sc=sp.Chains.SingleOrDefault(c=>c.Role==tc.Role);if(sc==null)continue;
                for(int k=0;k<tc.Bones.Count;k++)
                {
                    double phase=tc.Bones.Count==1?sc.Bones.Count-1:k*(sc.Bones.Count-1d)/(tc.Bones.Count-1);int a=(int)Math.Floor(phase),b=Math.Min(sc.Bones.Count-1,a+1);
                    var delta=HRotation.Blend(sw[sc.Bones[a]].R*sr[sc.Bones[a]].R.Inverse(),sw[sc.Bones[b]].R*sr[sc.Bones[b]].R.Inverse(),phase-a);
                    desired[tc.Bones[k]]=delta*tr[tc.Bones[k]].R;
                }
            }
            int sourcePelvis=sp.Chain("Pelvis").Bones[0],targetPelvis=tp.Chain("Pelvis").Bones[0];double ratio=tb.LegLength/sb.LegLength;
            for(int i=0;i<result.Length;i++)
            {
                int parent=target.Bones[i].Parent;
                if(desired.TryGetValue(i,out var r)){result[i].R=parent<0?r:tw[parent].R.Inverse()*r;result[i].P=target.Bones[i].Rest.P;}
                if(i==targetPelvis)
                {
                    // Native actor-root movement is removed for InPlace. Pelvis motion is
                    // body-relative displacement, scaled by measured leg proportions, not units.
                    int motionRoot=sp.Chains.SingleOrDefault(c=>c.Role=="MotionRoot")?.Bones.Last()??0;
                    var displacement=(sw[sourcePelvis].P-sr[sourcePelvis].P)-(sw[motionRoot].P-sr[motionRoot].P);
                    var goal=tr[i].P+displacement*ratio;result[i].P=parent<0?goal:tw[parent].R.Inverse().Apply(goal-tw[parent].P);
                }
                if(i==0)result[i].P=target.Bones[0].Rest.P;
                tw[i]=parent<0?result[i].Clone():tw[parent]*result[i];
            }
            return result;
        }
        public static HumanoidResult Correct(HRig target,HumanoidProfile profile,BodyProfile body,HPose[] preferred,HumanoidRecipe recipe,double time)
        {
            var result=new HumanoidResult{Local=preferred.Select(p=>p.Clone()).ToArray()};var rest=target.World(target.Rest());var world=target.World(result.Local);
            int pelvis=profile.Chain("Pelvis").Bones[0],parent=target.Bones[pelvis].Parent;
            if(recipe.Stance!=null)
            {
                var offset=recipe.Stance.PelvisOffset;
                if(!offset.IsFinite)throw new IOException("Non-finite stance calibration.");
                result.Local[pelvis].P+=parent<0?offset:world[parent].R.Inverse().Apply(offset);world=target.World(result.Local);
            }
            var active=recipe.Contacts.Where(c=>c.Weight(time)>0).ToList();
            var ranges=new List<Tuple<double,double>>{Tuple.Create(-body.PelvisLimit,body.PelvisLimit)};
            foreach(var c in active.Where(c=>c.Chain.EndsWith("Leg")))
            {
                var ids=profile.Chain(c.Chain).Bones;var goal=Goal(c,profile,world);var orientation=c.RestOrientation?rest[ids[2]].R:world[ids[2]].R;var ankle=goal-orientation.Apply(body.Landmark(c.Chain).Local);double first=(rest[ids[1]].P-rest[ids[0]].P).Length,second=(rest[ids[2]].P-rest[ids[1]].P).Length,reach=first+second,inner=Math.Abs(first-second);
                var d=world[ids[0]].P-ankle;double horizontal=d.X*d.X+d.Z*d.Z;
                if(horizontal>reach*reach){result.Diagnostics.Add(c.Chain+": unreachable horizontal contact; no stretch");continue;}
                double center=ankle.Y-world[ids[0]].P.Y,height=Math.Sqrt(Math.Max(0,reach*reach-horizontal));
                // A two-bone limb occupies a shell, not a ball. Unequal segment
                // lengths leave an unreachable inner region with two possible Y intervals.
                double gap=Math.Sqrt(Math.Max(0,inner*inner-horizontal));
                var allowed=gap>0?new[]{Tuple.Create(center-height,center-gap),Tuple.Create(center+gap,center+height)}:new[]{Tuple.Create(center-height,center+height)};
                ranges=(from prior in ranges from next in allowed let lo=Math.Max(prior.Item1,next.Item1) let hi=Math.Min(prior.Item2,next.Item2) where lo<=hi select Tuple.Create(lo,hi)).ToList();
            }
            double shift=ranges.Select(r=>Math.Max(r.Item1,Math.Min(r.Item2,recipe.PelvisBias))).OrderBy(v=>Math.Abs(v-recipe.PelvisBias)).ThenBy(v=>v).DefaultIfEmpty(Math.Max(-body.PelvisLimit,Math.Min(body.PelvisLimit,recipe.PelvisBias))).First();
            // Fade coupled pelvis participation with the same explicit contact interval.
            // A full-weight threshold would cause a discontinuous pelvis jump at margins.
            double participation=active.Where(c=>c.Chain.EndsWith("Leg")).Select(c=>c.Weight(time)).DefaultIfEmpty(0).Max();shift=recipe.PelvisBias+(shift-recipe.PelvisBias)*participation;
            if(ranges.Count==0)result.Diagnostics.Add("Coupled pelvis lower/upper reach intervals conflict; no feasible bounded pelvis Y. Correct the explicit stance/goals.");
            result.Local[pelvis].P+=parent<0?new HVec(0,shift,0):world[parent].R.Inverse().Apply(new HVec(0,shift,0));result.PelvisOffset=shift;
            foreach(var contact in active)
            {
                world=target.World(result.Local);var ids=profile.Chain(contact.Chain).Bones;var landmark=body.Landmark(contact.Chain);double weight=contact.Weight(time);var old=world[ids[2]];var wanted=Goal(contact,profile,world);var rotation=contact.RestOrientation?rest[ids[2]].R:old.R;
                double error=(old.Point(landmark.Local)-wanted).Length;result.PreferredError=Math.Max(result.PreferredError,error);
                var endpoint=wanted-rotation.Apply(landmark.Local);var solved=SolveLimb(target,result.Local,ids,endpoint,contact.UsePoleOverride?contact.Pole:landmark.Pole,rotation,result.Diagnostics);
                // Blend locals only on explicitly active intervals; outside them the FK pose is exact.
                foreach(int bone in ids){result.Local[bone].R=HRotation.Blend(preferred[bone].R,solved[bone].R,weight);}
            }
            world=target.World(result.Local);
            foreach(var contact in active){var ids=profile.Chain(contact.Chain).Bones;result.ContactError=Math.Max(result.ContactError,(world[ids[2]].Point(body.Landmark(contact.Chain).Local)-Goal(contact,profile,world)).Length);}
            return result;
        }
        private static HVec Goal(ContactInterval c,HumanoidProfile profile,HPose[] world)=>c.Anchor=="World"?c.Goal:world[profile.Chain(c.Anchor).Bones.Last()].Point(c.Goal);
        private static HPose[] SolveLimb(HRig rig,HPose[] local,IList<int> ids,HVec goal,HVec pole,HRotation endRotation,List<string> issues)
        {
            var result=local.Select(p=>p.Clone()).ToArray();var world=rig.World(result);var a=world[ids[0]].P;var b=world[ids[1]].P;var c=world[ids[2]].P;double first=(b-a).Length,second=(c-b).Length;if(first<1e-7 || second<1e-7)throw new IOException("Degenerate two-bone limb.");
            var displacement=goal-a;double distance=displacement.Length;var direction=distance<1e-8?(c-a).Unit():displacement/distance;
            double bounded=Math.Max(Math.Max(1e-8,Math.Abs(first-second)),Math.Min(first+second,distance));if(Math.Abs(bounded-distance)>1e-5)issues.Add("Unreachable contact: residual reach "+Math.Abs(bounded-distance)+"; target limb lengths preserved.");
            var projected=pole-direction*HVec.Dot(pole,direction);if(projected.Length<1e-7){projected=new HVec(1,0,0)-direction*direction.X;if(projected.Length<1e-7)projected=new HVec(0,1,0)-direction*direction.Y;}
            var bend=projected.Unit();double along=(first*first-second*second+bounded*bounded)/(2*bounded),height=Math.Sqrt(Math.Max(0,first*first-along*along));var knee=a+direction*along+bend*height;var ankle=a+direction*bounded;
            SetWorldRotation(rig,result,ids[0],HRotation.Between(b-a,knee-a,pole)*world[ids[0]].R);world=rig.World(result);
            SetWorldRotation(rig,result,ids[1],HRotation.Between(world[ids[2]].P-world[ids[1]].P,ankle-world[ids[1]].P,pole)*world[ids[1]].R);
            SetWorldRotation(rig,result,ids[2],endRotation);return result;
        }
        private static void SetWorldRotation(HRig rig,HPose[] local,int bone,HRotation rotation)
        {int parent=rig.Bones[bone].Parent;local[bone].R=parent<0?rotation:rig.World(local)[parent].R.Inverse()*rotation;}
    }
}
