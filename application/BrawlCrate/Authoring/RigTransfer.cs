// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
// Evaluated rigid poses and FK contain no native-format or renderer dependency.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BrawlCrate.Authoring
{
    public sealed class RigRolePair
    {
        public string Role { get; set; }
        public int Source { get; set; }
        public int Target { get; set; }
    }
    public sealed class ReviewedRigMap
    {
        public string Id { get; set; }
        public int Revision { get; set; } = 1;
        public string SourceCharacterId { get; set; }
        public string TargetCharacterId { get; set; }
        public string SourceFingerprint { get; set; }
        public string TargetFingerprint { get; set; }
        public string SourceInterpretation { get; set; }
        public string TargetInterpretation { get; set; }
        public string Algorithm { get; set; } = "world-rest-delta-fk-v1";
        public string RootPolicy { get; set; } = "InPlace";
        public double NativeUnitScale { get; set; } = 1;
        public string ReviewNote { get; set; }
        public bool Reviewed { get; set; }
        public string ReviewDigest { get; set; }
        public List<RigRolePair> Pairs { get; set; } = new List<RigRolePair>();
        public List<int> TargetRestOnly { get; set; } = new List<int>();
        public List<int> IgnoredSource { get; set; } = new List<int>();
    }
    public sealed class RigPose
    {
        public double X, Y, Z, RX, RY, RZ;
        public RigPose Clone() => (RigPose)MemberwiseClone();
        public static RigPose Rest(BoneSnapshot bone)
        {
            var v=bone.Rest.Split(',').Select(s=>double.Parse(s,CultureInfo.InvariantCulture)).ToArray();
            if(v.Length!=9 || v.Any(x=>!Finite(x)) || v.Take(3).Any(x=>Math.Abs(x-1)>1e-6) || (bone.Flags & 0xE0)!=0 || bone.Billboard!=0)
                throw new IOException("FK v1 requires unit rest scale, no scale compensation/classic-scale or billboard flags.");
            return new RigPose { RX=v[3],RY=v[4],RZ=v[5],X=v[6],Y=v[7],Z=v[8] };
        }
        internal static bool Finite(double x) => !double.IsNaN(x) && !double.IsInfinity(x);
    }
    public sealed class RigidTransform
    {
        public double[] R = {1,0,0,0,1,0,0,0,1};
        public double X,Y,Z;
        public static RigidTransform From(RigPose p)
        {
            double a=p.RX*Math.PI/180,b=p.RY*Math.PI/180,c=p.RZ*Math.PI/180;
            double sx=Math.Sin(a),cx=Math.Cos(a),sy=Math.Sin(b),cy=Math.Cos(b),sz=Math.Sin(c),cz=Math.Cos(c);
            return new RigidTransform {X=p.X,Y=p.Y,Z=p.Z,R=new[]{cy*cz,sx*sy*cz-cx*sz,cx*sy*cz+sx*sz,cy*sz,sx*sy*sz+cx*cz,cx*sy*sz-sx*cz,-sy,sx*cy,cx*cy}};
        }
        public static RigidTransform Multiply(RigidTransform a,RigidTransform b)
        {
            var result=new RigidTransform();
            for(int i=0;i<3;i++) for(int j=0;j<3;j++) {result.R[3*i+j]=0;for(int k=0;k<3;k++)result.R[3*i+j]+=a.R[3*i+k]*b.R[3*k+j];}
            result.X=a.X+a.R[0]*b.X+a.R[1]*b.Y+a.R[2]*b.Z;
            result.Y=a.Y+a.R[3]*b.X+a.R[4]*b.Y+a.R[5]*b.Z;
            result.Z=a.Z+a.R[6]*b.X+a.R[7]*b.Y+a.R[8]*b.Z;return result;
        }
        public RigidTransform Inverse()
        {
            var result=new RigidTransform {R=new[]{R[0],R[3],R[6],R[1],R[4],R[7],R[2],R[5],R[8]}};
            result.X=-(result.R[0]*X+result.R[1]*Y+result.R[2]*Z);result.Y=-(result.R[3]*X+result.R[4]*Y+result.R[5]*Z);result.Z=-(result.R[6]*X+result.R[7]*Y+result.R[8]*Z);return result;
        }
        public RigPose Euler(RigPose previous)
        {
            double y=Math.Asin(Math.Max(-1,Math.Min(1,-R[6])));
            if(Math.Abs(Math.Cos(y))<1e-5) throw new IOException("FK v1 cannot bake this Euler singularity. A different representation/solver is required.");
            var p=new RigPose {X=X,Y=Y,Z=Z,RX=Math.Atan2(R[7],R[8])*180/Math.PI,RY=y*180/Math.PI,RZ=Math.Atan2(R[3],R[0])*180/Math.PI};
            if(previous!=null) {p.RX=Unwrap(p.RX,previous.RX);p.RY=Unwrap(p.RY,previous.RY);p.RZ=Unwrap(p.RZ,previous.RZ);}return p;
        }
        private static double Unwrap(double angle,double previous) => angle+360*Math.Round((previous-angle)/360);
        public double RotationError(RigidTransform other) => R.Zip(other.R,(a,b)=>(a-b)*(a-b)).Sum();
    }
    public static class RigTransfer
    {
        public static void Validate(ReviewedRigMap map,SourceCharacterProfile source,SourceCharacterProfile target,bool requireReview=true)
        {
            if(requireReview && map!=null)
            {
                var copy=CharacterProject.Deserialize<ReviewedRigMap>(CharacterProject.Serialize(map),1024*1024);copy.ReviewDigest=null;
                if(map.ReviewDigest!=CharacterProject.Hash(CharacterProject.Serialize(copy)))throw new IOException("Mapping policies changed after review; review this version again.");
            }
            if(map==null || (requireReview && !map.Reviewed) || map.Algorithm!="world-rest-delta-fk-v1" || map.Revision!=1 ||
                map.SourceCharacterId!=source.Id || map.TargetCharacterId!=target.Id || map.SourceFingerprint!=source.RigFingerprint || map.TargetFingerprint!=target.RigFingerprint ||
                map.SourceInterpretation!=source.Interpretation || map.TargetInterpretation!=target.Interpretation || string.IsNullOrWhiteSpace(map.ReviewNote) || map.ReviewNote.Length>2048 ||
                (map.RootPolicy!="InPlace" && map.RootPolicy!="ScaledNativeTranslation") || !RigPose.Finite(map.NativeUnitScale) || map.NativeUnitScale<=0 || map.NativeUnitScale>100)
                throw new IOException("Mapping is unreviewed, stale, or has unsupported interpretation/root policy.");
            foreach(var rig in new[]{source,target})
            {
                if(rig.Bones.Count>128 || rig.Bones.Count==0 || rig.Bones[0].Parent!=-1) throw new IOException("FK v1 requires one root and at most 128 bones.");
                for(int i=0;i<rig.Bones.Count;i++) {if(i>0 && (rig.Bones[i].Parent<0 || rig.Bones[i].Parent>=i))throw new IOException("Rig hierarchy is not a supported parent-first tree.");RigPose.Rest(rig.Bones[i]);}
            }
            var ss=map.Pairs.Select(p=>p.Source).Concat(map.IgnoredSource).ToList(); var ts=map.Pairs.Select(p=>p.Target).Concat(map.TargetRestOnly).ToList();
            if(ss.Count!=source.Bones.Count || ts.Count!=target.Bones.Count || !ss.OrderBy(i=>i).SequenceEqual(Enumerable.Range(0,source.Bones.Count)) || !ts.OrderBy(i=>i).SequenceEqual(Enumerable.Range(0,target.Bones.Count)) ||
                map.Pairs.Any(p=>string.IsNullOrWhiteSpace(p.Role) || p.Role.Length>120) || map.Pairs.Select(p=>p.Role).Distinct().Count()!=map.Pairs.Count || !map.Pairs.Any(p=>p.Source==0 && p.Target==0))
                throw new IOException("Every bone needs an explicit unique role pair or reviewed ignore/rest-only policy; root must map to root.");
        }
        public static RigidTransform[] World(IList<BoneSnapshot> bones,IList<RigPose> local)
        {
            var world=new RigidTransform[bones.Count];
            for(int i=0;i<bones.Count;i++) {var p=RigidTransform.From(local[i]);world[i]=bones[i].Parent<0?p:RigidTransform.Multiply(world[bones[i].Parent],p);}return world;
        }
        public static RigPose[] Transfer(ReviewedRigMap map,SourceCharacterProfile source,SourceCharacterProfile target,RigPose[] local,RigPose[] previous=null)
        {
            Validate(map,source,target);
            var sr=source.Bones.Select(RigPose.Rest).ToArray();var tr=target.Bones.Select(RigPose.Rest).ToArray();
            if(local.Length!=sr.Length)throw new IOException("Pose/rig bone count mismatch.");
            for(int i=1;i<sr.Length;i++) if(Math.Abs(local[i].X-sr[i].X)+Math.Abs(local[i].Y-sr[i].Y)+Math.Abs(local[i].Z-sr[i].Z)>1e-5)throw new IOException("Animated non-root translations require a reviewed proportion/translation route; FK v1 rejects them.");
            foreach(int i in map.IgnoredSource) if(RigidTransform.From(local[i]).RotationError(RigidTransform.From(sr[i]))>1e-10)throw new IOException("Ignored source helper is animated; explicit transfer support is required.");
            var sw=World(source.Bones,local);var sb=World(source.Bones,sr);var tb=World(target.Bones,tr);var result=new RigPose[tr.Length];var tw=new RigidTransform[tr.Length];
            for(int i=0;i<tr.Length;i++)
            {
                var pair=map.Pairs.SingleOrDefault(p=>p.Target==i);result[i]=tr[i].Clone();
                if(pair!=null)
                {
                    var desired=RigidTransform.Multiply(RigidTransform.Multiply(sw[pair.Source],sb[pair.Source].Inverse()),tb[i]);
                    var relative=target.Bones[i].Parent<0?desired:RigidTransform.Multiply(tw[target.Bones[i].Parent].Inverse(),desired);
                    var angles=relative.Euler(previous==null?tr[i]:previous[i]);result[i].RX=angles.RX;result[i].RY=angles.RY;result[i].RZ=angles.RZ;
                }
                if(i==0 && map.RootPolicy=="ScaledNativeTranslation") {result[i].X+=map.NativeUnitScale*(local[0].X-sr[0].X);result[i].Y+=map.NativeUnitScale*(local[0].Y-sr[0].Y);result[i].Z+=map.NativeUnitScale*(local[0].Z-sr[0].Z);}
                var transform=RigidTransform.From(result[i]);tw[i]=target.Bones[i].Parent<0?transform:RigidTransform.Multiply(tw[target.Bones[i].Parent],transform);
            }
            return result;
        }
    }
}
