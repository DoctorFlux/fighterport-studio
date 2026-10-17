// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
// Native evaluation/curve encoding stays here; the humanoid solver has no Brawl dependency.
using BrawlCrate.Authoring.Humanoid;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace BrawlCrate.Authoring
{
    public sealed class HumanoidBakeReport
    {
        public string Algorithm {get;set;}="humanoid-contact-v1";
        public int Frames {get;set;}
        public int Samples {get;set;}
        public double NativePositionError {get;set;}
        public double NativeDegreesError {get;set;}
        public double NativeMeshError {get;set;}
        public double FkContactError {get;set;}
        public double CorrectedContactError {get;set;}
        public double CorrectedOrientationError {get;set;}
        public double EndpointContactError {get;set;}
        public int ContactSamples {get;set;}
        public int EndpointContactSamples {get;set;}
        public double MaximumPelvisOffset {get;set;}
        public int UnreachableSamples {get;set;}
        public string WorstPosition {get;set;}
        public string WorstRotation {get;set;}
        public bool ProfilesReviewed {get;set;}
        public List<string> Diagnostics {get;set;}=new List<string>();
        public override string ToString()=>string.Format(CultureInfo.InvariantCulture,"{0}; {1} native samples (quarter grid + withheld .137/.619). Native decode error pos={2:R}, degrees={3:R}, mesh={4:R} (limits .02/.15/.03). Contact quality FK={5:R}, corrected={6:R} native units; orientation={7:R} degrees; interval pelvis adjustment <= {8:R}; unreachable samples={9}. Profiles reviewed={10}. Contact samples={11}; held endpoints={13}; error={12:R}. Source optional channels reported, target reference retained; no gameplay/clearance/prop certification.",Algorithm,Samples,NativePositionError,NativeDegreesError,NativeMeshError,FkContactError,CorrectedContactError,CorrectedOrientationError,MaximumPelvisOffset,UnreachableSamples,ProfilesReviewed,ContactSamples,EndpointContactError,EndpointContactSamples);
    }
    public sealed class HumanoidTrial : IDisposable
    {
        public string ProjectId,TargetId,AnimationId,TargetAnimationId,TargetFingerprint,RecipeHash,FkPath,CorrectedPath,FkHash,CorrectedHash;
        public long Generation;
        public HumanoidBakeReport Report;
        internal Action Release;
        public void Dispose()=>Interlocked.Exchange(ref Release,null)?.Invoke();
    }
    internal static partial class BrawlProjectAdapter
    {
        internal static HumanoidStance CalibrateHumanoidStance(string sourceFile,SourceCharacterProfile source,AnimationAsset animation,string targetFile,SourceCharacterProfile target,AnimationAsset native,HumanoidSetup sp,HumanoidSetup tp,double sourceTime,double targetTime)
        {
            using(var sr=NodeFactory.FromFile(null,sourceFile))using(var tr=NodeFactory.FromFile(null,targetFile))
            {
                var sm=(MDL0Node)At(sr,source.ModelPath);sm.Populate();var tm=(MDL0Node)At(tr,target.ModelPath);tm.Populate();
                var reference=CommonSample(tm,Motion(tr,target,native),(float)targetTime,target,tp.Profile);var rig=CommonRig(target);
                var preferred=HumanoidSolver.Fk(CommonRig(source),sp.Profile,sp.Body,rig,tp.Profile,tp.Body,CommonSample(sm,Motion(sr,source,animation),(float)sourceTime,source,sp.Profile),reference);
                int pelvis=tp.Profile.Chain("Pelvis").Bones[0];
                return new HumanoidStance{SourceTime=sourceTime,TargetTime=targetTime,PelvisOffset=rig.World(reference)[pelvis].P-rig.World(preferred)[pelvis].P,Evidence="Explicit clip-specific native source/target pelvis reference alignment; constant world offset, no bind/mesh/length changes or semantic approval."};
            }
        }
        private static HPose[] CommonSample(MDL0Node model,CHR0Node motion,float time,SourceCharacterProfile profile,HumanoidProfile humanoid)
        {
            model.ApplyCHR(motion,time+1);var local=new List<HPose>();
            foreach(var bone in model.BoneCache.OfType<MDL0BoneNode>())
            {
                var p=bone.FrameState;
                if(new[]{p._scale._x,p._scale._y,p._scale._z,p._rotate._x,p._rotate._y,p._rotate._z,p._translate._x,p._translate._y,p._translate._z}.Any(v=>float.IsNaN(v)||float.IsInfinity(v)))throw new IOException("Non-finite native motion cannot be evaluated or certified as humanoid output.");
                if(Math.Abs(p._scale._x-1)+Math.Abs(p._scale._y-1)+Math.Abs(p._scale._z-1)>1e-5)throw new IOException("Humanoid native v1 does not yet support animated scale. Native source preview remains available.");
                local.Add(new HPose{P=new HVec(p._translate._x,p._translate._y,p._translate._z),R=NativeRotation(p._rotate._x,p._rotate._y,p._rotate._z)});
            }
            var rig=CommonRig(profile);var world=rig.World(local.ToArray());var active=humanoid.Chains.SelectMany(c=>c.Bones).ToArray();
            foreach(int i in active)
            {
                var matrix=model.BoneCache[i].Matrix;var native=new HVec(matrix[12],matrix[13],matrix[14]);
                if(!native.IsFinite)throw new IOException("Non-finite native world position.");
                if((native-world[i].P).Length>.002)throw new IOException("Native/common hierarchy evaluation mismatch; unsupported native scale/helper behavior.");
                string role=humanoid.Chains.Single(c=>c.Bones.Contains(i)).Role;
                if(role!="Pelvis" && role!="Root" && role!="MotionRoot" && (local[i].P-rig.Bones[i].Rest.P).Length>.001)
                {
                    var track=motion.Children.OfType<CHR0EntryNode>().SingleOrDefault(e=>e.Name==profile.Bones[i].Name);
                    // Constant native authored offsets are evaluated on the source, not
                    // copied into target lengths. Dynamic non-root translation still rejects.
                    if(track!=null && track.KeyArrays.Skip(6).Any(array=>{var keys=array.Entries.ToArray();return keys.Length>1 && keys.Any(k=>Math.Abs(k._value-keys[0]._value)>1e-6 || Math.Abs(k._tangent)>1e-6);}))
                        throw new IOException("Animated translation outside the explicit root/pelvis policy needs another route: "+role+" / "+profile.Bones[i].Name+" at native time "+time+"; offset delta="+(local[i].P-rig.Bones[i].Rest.P).Length);
                }
            }
            return local.ToArray();
        }
        internal static HumanoidBakeReport BakeHumanoid(string sourceFile,SourceCharacterProfile source,AnimationAsset animation,string targetFile,SourceCharacterProfile target,AnimationAsset native,
            HumanoidProfile sp,BodyProfile sb,HumanoidProfile tp,BodyProfile tb,HumanoidRecipe recipe,string fkOutput,string output,CancellationToken token)
        {
            var sr=CommonRig(source);var tr=CommonRig(target);sp.Validate(sr,false);tp.Validate(tr,false);sb.Validate(sr,false);tb.Validate(tr,false);recipe.Validate(animation.Frames);
            if(animation.Frames<2 || animation.Frames>1024 || (long)animation.Frames*target.Bones.Count*9>1000000)throw new IOException("Humanoid bake budget: 2..1024 native frames / one million scalar keys.");
            var preferred=new HPose[animation.Frames][];var corrected=new HPose[animation.Frames][];var report=new HumanoidBakeReport{Frames=animation.Frames,ProfilesReviewed=sp.Reviewed&&tp.Reviewed&&sb.Reviewed&&tb.Reviewed};
            report.Diagnostics.Add("InPlace actor motion; pelvis displacement adapted by target/source leg length, not physical units. Optional target channels use normalized target-reference timing; they are not source-channel transfer.");
            report.Diagnostics.Add("Constant authored source joint offsets are natively evaluated; target rest lengths retained. Dynamic non-root translation outside root/pelvis remains unsupported.");
            report.Diagnostics.Add("Source optional bones reported: "+sp.Optional.Count+"; target reference bones retained: "+tp.Optional.Count+". Clearance/props/twist distribution beyond characterized chains remain unsupported.");
            foreach(var chain in sp.Chains.Where(c=>!tp.Chains.Any(t=>t.Role==c.Role)))report.Diagnostics.Add("Source-only chain "+chain.Role+" has no target counterpart; it is not transferred.");
            using(var sroot=NodeFactory.FromFile(null,sourceFile))using(var troot=NodeFactory.FromFile(null,targetFile))
            {
                var sm=(MDL0Node)At(sroot,source.ModelPath);sm.Populate();var tm=(MDL0Node)At(troot,target.ModelPath);tm.Populate();var motion=Motion(sroot,source,animation);var reference=Motion(troot,target,native);
                long vertices=tm._objList.OfType<MDL0ObjectNode>().Sum(m=>(long)m.Vertices.Count);int end=motion.Loop?animation.Frames+1:animation.Frames-1;
                var times=HumanoidValidationTimes(end,recipe);
                if(vertices*times.Count*2>8000000)throw new IOException("Humanoid validation exceeds eight million decoded vertex samples; select a smaller motion/model.");
                foreach(var contact in recipe.Contacts)if(!times.Any(t=>contact.Weight(Math.Min(t,animation.Frames-1))>.999))throw new IOException("Contact interval cannot be sampled faithfully at native float time precision; widen/review its timing before baking.");
                foreach(var m in new[]{motion,reference})foreach(CHR0EntryNode e in m.Children)if(e.ScaleCompensateApply||e.ScaleCompensateParent||e.ClassicScaleOff)throw new IOException("Humanoid adapter does not support animated scale-compensation flags.");
                var activeNames=new System.Collections.Generic.HashSet<string>(sp.Chains.SelectMany(c=>c.Bones).Select(i=>source.Bones[i].Name));
                foreach(var entry in motion.Children.OfType<CHR0EntryNode>().Where(e=>activeNames.Contains(e.Name)))foreach(var array in entry.KeyArrays.Skip(3).Take(3))if(array.Entries.Any(k=>Math.Abs(k._tangent)>120))throw new IOException("Humanoid source winding/tangent exceeds 120 native degrees/frame; a denser supported route is required.");
                HPose[] priorSource=null;double[][] priorEuler=null;
                for(int f=0;f<animation.Frames;f++)
                {
                    token.ThrowIfCancellationRequested();var pose=CommonSample(sm,motion,f,source,sp);float rt=(float)(f*(native.Frames-1d)/(animation.Frames-1));var targetReference=CommonSample(tm,reference,rt,target,tp);
                    var sourceEuler=sm.BoneCache.OfType<MDL0BoneNode>().Select(b=>new double[]{b.FrameState._rotate._x,b.FrameState._rotate._y,b.FrameState._rotate._z}).ToArray();
                    if(priorEuler!=null)foreach(int i in sp.Chains.SelectMany(c=>c.Bones))if(sourceEuler[i].Zip(priorEuler[i],(a,b)=>Math.Abs(a-b)).Any(d=>d>120))throw new IOException("Humanoid source native Euler travel exceeds 120 degrees/frame; winding was not collapsed to endpoint matrices.");
                    // Do not infer winding from sparse endpoint matrices across a large jump.
                    if(priorSource!=null)foreach(int i in sp.Chains.SelectMany(c=>c.Bones))if(pose[i].R.DegreesFrom(priorSource[i].R)>120)throw new IOException("Humanoid bake needs a denser supported native time representation; rotation travel >120 degrees/frame.");
                    preferred[f]=HumanoidSolver.Fk(sr,sp,sb,tr,tp,tb,pose,targetReference);var solve=HumanoidSolver.Correct(tr,tp,tb,preferred[f],recipe,f);corrected[f]=solve.Local;priorSource=pose;priorEuler=sourceEuler;
                }
                Func<double,HPose[]> evaluate=time=>HumanoidSolver.Fk(sr,sp,sb,tr,tp,tb,CommonSample(sm,motion,(float)time,source,sp),CommonSample(tm,reference,(float)(time*(native.Frames-1d)/(animation.Frames-1)),target,tp));
                WriteHumanoid(troot,target,native,motion,preferred,evaluate,fkOutput,token); // Reopen the untouched target for the independent corrected branch.
            }
            using(var targetRoot=NodeFactory.FromFile(null,targetFile))using(var sroot=NodeFactory.FromFile(null,sourceFile))
            {
                var sm=(MDL0Node)At(sroot,source.ModelPath);sm.Populate();var tm=(MDL0Node)At(targetRoot,target.ModelPath);tm.Populate();var motion=Motion(sroot,source,animation);var reference=Motion(targetRoot,target,native);
                Func<double,HPose[]> evaluate=time=>HumanoidSolver.Correct(tr,tp,tb,HumanoidSolver.Fk(sr,sp,sb,tr,tp,tb,CommonSample(sm,motion,(float)time,source,sp),CommonSample(tm,reference,(float)(time*(native.Frames-1d)/(animation.Frames-1)),target,tp)),recipe,time).Local;
                WriteHumanoid(targetRoot,target,native,motion,corrected,evaluate,output,token);
            }
            ValidateEdit(targetFile,fkOutput,target,native,Path.GetDirectoryName(output));ValidateEdit(targetFile,output,target,native,Path.GetDirectoryName(output));
            using(var sroot=NodeFactory.FromFile(null,sourceFile))using(var targetRoot=NodeFactory.FromFile(null,targetFile))using(var fkRoot=NodeFactory.FromFile(null,fkOutput))using(var decoded=NodeFactory.FromFile(null,output))
            {
                var sm=(MDL0Node)At(sroot,source.ModelPath);sm.Populate();var referenceModel=(MDL0Node)At(targetRoot,target.ModelPath);referenceModel.Populate();var fm=(MDL0Node)At(fkRoot,target.ModelPath);fm.Populate();var tm=(MDL0Node)At(decoded,target.ModelPath);tm.Populate();
                var motion=Motion(sroot,source,animation);var reference=Motion(targetRoot,target,native);var fk=Motion(fkRoot,target,native);var baked=Motion(decoded,target,native);
                if(fk.FrameCount!=motion.FrameCount || baked.FrameCount!=motion.FrameCount || fk.Loop!=motion.Loop || baked.Loop!=motion.Loop)throw new IOException("Humanoid native bake changed duration/loop.");
                int end=motion.Loop?animation.Frames+1:animation.Frames-1;var targetRest=tr.World(tr.Rest());
                foreach(float sampleTime in HumanoidValidationTimes(end,recipe))
                {
                    token.ThrowIfCancellationRequested();float time=sampleTime;if(motion.Loop && time==end)time=0;double contactTime=Math.Min(time,animation.Frames-1);float referenceTime=(float)(contactTime*(native.Frames-1)/(animation.Frames-1d));
                    var wantedFk=HumanoidSolver.Fk(sr,sp,sb,tr,tp,tb,CommonSample(sm,motion,time,source,sp),CommonSample(referenceModel,reference,referenceTime,target,tp));var wanted=HumanoidSolver.Correct(tr,tp,tb,wantedFk,recipe,contactTime);
                    CheckHumanoidDecoded(fm,fk,time,tr,target,wantedFk,report);CheckHumanoidDecoded(tm,baked,time,tr,target,wanted.Local,report);
                    var fw=tr.World(CommonSample(fm,fk,time,target,tp));var actual=tr.World(CommonSample(tm,baked,time,target,tp));
                    foreach(var c in recipe.Contacts.Where(c=>c.Weight(contactTime)>.999))
                    {
                        int bone=tp.Chain(c.Chain).Bones.Last();var landmark=tb.Landmark(c.Chain);var goal=c.Anchor=="World"?c.Goal:actual[tp.Chain(c.Anchor).Bones.Last()].Point(c.Goal);
                        report.FkContactError=Math.Max(report.FkContactError,(fw[bone].Point(landmark.Local)-goal).Length);report.CorrectedContactError=Math.Max(report.CorrectedContactError,(actual[bone].Point(landmark.Local)-goal).Length);
                        report.ContactSamples++;if(time==0 && c.HoldStart || time==animation.Frames-1 && c.HoldEnd){report.EndpointContactSamples++;report.EndpointContactError=Math.Max(report.EndpointContactError,(actual[bone].Point(landmark.Local)-goal).Length);}
                        if(c.RestOrientation)report.CorrectedOrientationError=Math.Max(report.CorrectedOrientationError,actual[bone].R.DegreesFrom(targetRest[bone].R));
                    }
                    report.MaximumPelvisOffset=Math.Max(report.MaximumPelvisOffset,Math.Abs(wanted.PelvisOffset));if(wanted.Diagnostics.Count>0){report.UnreachableSamples++;foreach(string issue in wanted.Diagnostics)if(report.Diagnostics.Count<32 && !report.Diagnostics.Contains(issue))report.Diagnostics.Add(issue);}
                    report.Samples++;
                }
            }
            if(report.NativePositionError>.02 || report.NativeDegreesError>.15 || report.NativeMeshError>.03)throw new IOException("Humanoid native serialization/evaluation tolerance failed; no assignment committed. "+report+" Worst position: "+report.WorstPosition+"; worst rotation: "+report.WorstRotation);
            return report;
        }
        private static SortedSet<float> HumanoidValidationTimes(int end,HumanoidRecipe recipe)
        {
            var times=new SortedSet<float>();for(int q=0;q<=end*4;q++)times.Add(q/4f);
            for(int f=0;f<end;f++){times.Add(f+.137f);times.Add(f+.619f);}
            foreach(var contact in recipe.Contacts)foreach(double time in new[]{contact.Start,contact.End,contact.Start+contact.Margin,contact.End-contact.Margin,(contact.Start+contact.End)/2})times.Add((float)time);
            return times;
        }
        private static void WriteHumanoid(ResourceNode root,SourceCharacterProfile target,AnimationAsset native,CHR0Node source,HPose[][] values,Func<double,HPose[]> evaluate,string output,CancellationToken token)
        {
            var old=(CHR0Node)At(root,native.NativePath);var baked=new CHR0Node{Name=old.Name,Version=4,FrameCount=source.FrameCount,Loop=source.Loop};
            // Constrain every integer value exactly, then fit shared Hermite tangents
            // to five interior phases. Two thirds alone undersample curvature between
            // keys, notably when a transported anatomical pole changes several local
            // Euler channels together. Differencing almost coincident float poses is
            // unstable near straight knees. Withheld .137/.619 validation stays separate.
            var phases=new[]{1d/6,1d/3,.5,2d/3,5d/6};int intervals=values.Length-1,bones=target.Bones.Count;
            long scalarCount=(long)intervals*bones*phases.Length*6;
            if(scalarCount>32L*1024*1024/sizeof(double))throw new IOException("Humanoid tangent fitting exceeds its 32 MiB packed interior-sample budget.");
            var angles=new double[bones][][];
            for(int bone=0;bone<bones;bone++)
            {
                angles[bone]=new double[values.Length][];double[] previous=null;
                for(int f=0;f<values.Length;f++){angles[bone][f]=HumanoidEuler(values[f][bone].R,previous);previous=angles[bone][f];}
            }
            // Store only rotations/positions, not five native/common pose forests.
            // The existing million-scalar-key bound limits this buffer to <27 MiB.
            var interiors=new double[(int)scalarCount];
            for(int f=0;f<intervals;f++)for(int part=0;part<phases.Length;part++)
            {
                token.ThrowIfCancellationRequested();double phase=phases[part];var pose=evaluate(f+phase);
                for(int bone=0;bone<bones;bone++)
                {
                    var reference=new double[3];for(int channel=0;channel<3;channel++)reference[channel]=angles[bone][f][channel]+(angles[bone][f+1][channel]-angles[bone][f][channel])*phase;
                    var euler=HumanoidEuler(pose[bone].R,reference);int offset=((bone*intervals+f)*phases.Length+part)*6;
                    for(int channel=0;channel<3;channel++)interiors[offset+channel]=euler[channel];
                    interiors[offset+3]=pose[bone].P.X;interiors[offset+4]=pose[bone].P.Y;interiors[offset+5]=pose[bone].P.Z;
                }
            }
            for(int bone=0;bone<target.Bones.Count;bone++)
            {
                token.ThrowIfCancellationRequested();var entry=baked.CreateEntry(target.Bones[bone].Name);entry.PreserveInterpolation=true;
                for(int channel=0;channel<9;channel++)
                {
                    Func<int,double> value=f=>channel<3?1:channel<6?angles[bone][f][channel-3]:channel==6?values[f][bone].P.X:channel==7?values[f][bone].P.Y:values[f][bone].P.Z;
                    Func<int,int,double> sample=(f,part)=>channel<3?1:interiors[((bone*intervals+f)*phases.Length+part)*6+channel-3];
                    double slope=(value(values.Length-1)-value(0))/(values.Length-1);bool linear=Enumerable.Range(0,values.Length).All(f=>Math.Abs(value(f)-value(0)-slope*f)<1e-7);
                    // Test interiors too: equal integer values do not prove a constant curve.
                    linear=linear && Enumerable.Range(0,intervals).All(f=>Enumerable.Range(0,phases.Length).All(part=>Math.Abs(sample(f,part)-value(0)-slope*(f+phases[part]))<1e-7));
                    var tangents=linear?null:FitHumanoidTangents(Enumerable.Range(0,values.Length).Select(value).ToArray(),phases,sample,token);
                    foreach(int f in linear?new[]{0,values.Length-1}:Enumerable.Range(0,values.Length))
                    {var key=entry.SetKeyframe(channel,f,(float)value(f),true);key._tangent=linear?(float)slope:(float)tangents[f];}
                }
            }
            var parent=old.Parent;int index=old.Index;parent.InsertChild(baked,index);old.Remove();old.Dispose();
            if(root.CalculateSize(false)>CharacterProject.MaxNativeBytes)throw new IOException("Humanoid native output exceeds 32 MiB before export; working-copy reservation was preserved.");
            root.Export(output);
        }
        private static double[] FitHumanoidTangents(double[] values,double[] phases,Func<int,int,double> sample,CancellationToken token)
        {
            int n=values.Length;var diagonal=new double[n];var off=new double[n-1];var rhs=new double[n];
            for(int f=0;f<n-1;f++)
            {
                token.ThrowIfCancellationRequested();
                for(int part=0;part<phases.Length;part++)
                {
                    double u=phases[part],u2=u*u,u3=u2*u,a=u3-2*u2+u,b=u3-u2;
                    double residual=sample(f,part)-(2*u3-3*u2+1)*values[f]-(-2*u3+3*u2)*values[f+1];
                    diagonal[f]+=a*a;diagonal[f+1]+=b*b;off[f]+=a*b;rhs[f]+=a*residual;rhs[f+1]+=b*residual;
                }
            }
            for(int i=1;i<n;i++){double factor=off[i-1]/diagonal[i-1];diagonal[i]-=factor*off[i-1];rhs[i]-=factor*rhs[i-1];}
            var result=new double[n];for(int i=n-1;i>=0;i--)result[i]=(rhs[i]-(i+1<n?off[i]*result[i+1]:0))/diagonal[i];return result;
        }
        // Brawl XYZ Euler adapter: choose the nearest equivalent chart, including exact
        // gimbal poses. The generic solver never copies source Euler components.
        private static double[] HumanoidEuler(HRotation rotation,double[] previous)
        {
            var r=rotation.V;double y=Math.Atan2(-r[6],Math.Sqrt(r[0]*r[0]+r[3]*r[3])),x,z;
            if(Math.Abs(Math.Cos(y))>1e-7){x=Math.Atan2(r[7],r[8]);z=Math.Atan2(r[3],r[0]);}
            else{z=(previous?[2]??0)*Math.PI/180;x=y>0?Math.Atan2(r[1],r[2])+z:Math.Atan2(-r[1],-r[2])-z;}
            var a=new[]{x*180/Math.PI,y*180/Math.PI,z*180/Math.PI};if(previous==null)return a;var b=new[]{a[0]+180,180-a[1],a[2]+180};
            for(int i=0;i<3;i++){a[i]+=360*Math.Round((previous[i]-a[i])/360);b[i]+=360*Math.Round((previous[i]-b[i])/360);}
            return a.Zip(previous,(v,p)=>Math.Abs(v-p)).Sum()<=b.Zip(previous,(v,p)=>Math.Abs(v-p)).Sum()?a:b;
        }
        private static void CheckHumanoidDecoded(MDL0Node model,CHR0Node motion,float time,HRig rig,SourceCharacterProfile target,HPose[] expectedLocal,HumanoidBakeReport report)
        {
            model.ApplyCHR(motion,time+1);var expected=rig.World(expectedLocal);
            foreach(int i in Enumerable.Range(0,target.Bones.Count))
            {
                var m=model.BoneCache[i].Matrix;var p=new HVec(m[12],m[13],m[14]);var rotation=new HRotation{V=new double[]{m[0],m[4],m[8],m[1],m[5],m[9],m[2],m[6],m[10]}};
                // Native float matrix accumulation is not exactly orthonormal. A trace/acos
                // angle on that matrix falsely turns tiny length drift into a large angle.
                // Reject real scale/shear first, then compare proper rotation frames; position
                // and decoded skin errors still use the untouched native matrices/vertices.
                rotation.Validate();var cx=new HVec(m[0],m[1],m[2]).Unit();var cy=new HVec(m[4],m[5],m[6]);cy=(cy-cx*HVec.Dot(cx,cy)).Unit();var cz=HVec.Cross(cx,cy);rotation=new HRotation{V=new[]{cx.X,cy.X,cz.X,cx.Y,cy.Y,cz.Y,cx.Z,cy.Z,cz.Z}};
                double error=(p-expected[i].P).Length,degrees=rotation.DegreesFrom(expected[i].R);
                if(error>report.NativePositionError){report.NativePositionError=error;report.WorstPosition=target.Bones[i].Name+" @ "+time.ToString(CultureInfo.InvariantCulture);}
                if(degrees>report.NativeDegreesError){report.NativeDegreesError=degrees;var ancestors=new List<string>();for(int j=i;j>=0;j=rig.Bones[j].Parent){var a=model.BoneCache[j].FrameState._rotate;double delta=NativeRotation(a._x,a._y,a._z).DegreesFrom(expectedLocal[j].R);if(delta>.01)ancestors.Add(target.Bones[j].Name+" local="+delta.ToString("R",CultureInfo.InvariantCulture)+" native="+a+" expected="+string.Join(",",HumanoidEuler(expectedLocal[j].R,null).Select(v=>v.ToString("R",CultureInfo.InvariantCulture))));}report.WorstRotation=target.Bones[i].Name+" @ "+time.ToString(CultureInfo.InvariantCulture)+"; "+string.Join("; ",ancestors);}
            }
            foreach(var mesh in model._objList.OfType<MDL0ObjectNode>())foreach(var vertex in mesh.Vertices)
            {
                var inf=vertex.GetMatrixNode();if(inf==null)throw new IOException("Native mesh has no supported skin binding.");var weighted=new HVec();var v=vertex.Position;
                foreach(var weight in inf.Weights){int i=target.Bones.FindIndex(b=>b.Name==weight.Bone.Name);if(i<0)throw new IOException("Native influence bone identity changed.");var inverse=weight.Bone.InverseBindMatrix;
                    // Inherited MDL0 primary (single-bone) vertices are stored bone-local;
                    // blended influences store bind/world coordinates. Do not apply the
                    // inverse bind twice to a rigid vertex, or ignore mesh-level binding.
                    var bind=inf.IsPrimaryNode?new HVec(v._x,v._y,v._z):new HVec(inverse[0]*v._x+inverse[4]*v._y+inverse[8]*v._z+inverse[12],inverse[1]*v._x+inverse[5]*v._y+inverse[9]*v._z+inverse[13],inverse[2]*v._x+inverse[6]*v._y+inverse[10]*v._z+inverse[14]);weighted+=expected[i].Point(bind)*weight.Weight;}
                var p=vertex.WeightedPosition;report.NativeMeshError=Math.Max(report.NativeMeshError,(weighted-new HVec(p._x,p._y,p._z)).Length);
            }
        }
    }
}
