// Original independent analytic rigid-limb tests. FighterPort, 2026-10-17.
// Repository LGPL-3.0 terms. No game assets or native-codec assumptions.
using BrawlCrate.Authoring.Humanoid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal static partial class NativeSmoke
{
    private static void CheckHumanoidContinuousReachPoles()
    {
        foreach(bool axes in new[]{false,true})
        {
            var rig=ReachAnalyticRig(3,2,axes);var ids=new[]{2,3,4};var profile=ReachAnalyticProfile("LeftArm");var body=ReachAnalyticBody("LeftArm");
            var origin=rig.World(rig.Rest())[2].P;var samples=new Dictionary<int,HPose[]>();HPose[] previous=null;
            // Cross the OLD fixed +Z pole on both sides. The analytic minimal-swing
            // bend is (0, cos(t), -sin(t)); it does not reverse as t crosses zero.
            for(int index=0;index<=400;index++)
            {
                double t=(index-200)*.001,distance=3.5,along=(9-4+distance*distance)/(2*distance),height=Math.Sqrt(9-along*along);
                var direction=new HVec(0,Math.Sin(t),Math.Cos(t));var bend=new HVec(0,Math.Cos(t),-Math.Sin(t));var goal=origin+direction*distance;
                var recipe=ReachAnalyticRecipe("LeftArm",goal,"TransportedRest","Nearest");var solved=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),recipe,index*.05);var world=rig.World(solved.Local);
                Assert(solved.Diagnostics.Count==0 && (world[4].P-goal).Length<1e-8 && (world[3].P-(origin+direction*along+bend*height)).Length<1e-8,"independent cosine-law knee and continuous transported pole through world-pole degeneracy");
                ReachLengths(world,3,2);
                if(previous!=null)foreach(int id in ids)Assert(previous[id].R.DegreesFrom(solved.Local[id].R)<.2,"no rotation flip between dense pole-crossing samples");
                previous=solved.Local;samples[index]=solved.Local;
            }
            foreach(int index in Enumerable.Range(0,401).OrderBy(n=>(n*173)%401))
            {
                double t=(index-200)*.001;var goal=origin+new HVec(0,Math.Sin(t),Math.Cos(t))*3.5;
                var actual=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),ReachAnalyticRecipe("LeftArm",goal,"TransportedRest","Nearest"),index*.05).Local;
                for(int bone=0;bone<actual.Length;bone++)Assert(actual[bone].R.V.SequenceEqual(samples[index][bone].R.V) && actual[bone].P.X==samples[index][bone].P.X && actual[bone].P.Y==samples[index][bone].P.Y && actual[bone].P.Z==samples[index][bone].P.Z,"shuffled fractional pole evaluation is bit-identical without prior-frame state");
            }
            // Exact unequal-limb inner/outer bounds and withheld nearby distances.
            // Positions, not solver-reported contact errors, meet the independent goal.
            foreach(double distance in new[]{1.0,1.00000001,1.0001,1.1,2.25,4.9,4.9999,4.99999999,5.0})
            {
                var goal=origin+new HVec(0,-distance,0);var solved=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),ReachAnalyticRecipe("LeftArm",goal,"TransportedRest","Nearest"),7.125);var world=rig.World(solved.Local);
                Assert(solved.Diagnostics.Count==0 && (world[4].P-goal).Length<1e-7,"exact and near folded/straight unequal-limb reach meets independent effector position");ReachLengths(world,3,2);
            }
            foreach(double distance in new[]{.5,5.5})
            {
                var solved=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),ReachAnalyticRecipe("LeftArm",origin+new HVec(0,-distance,0),"TransportedRest","Nearest"),7.125);
                Assert(solved.Diagnostics.Any(d=>d.Contains("Unreachable")) && Math.Abs((rig.World(solved.Local)[4].P-origin).Length-(distance<1?1:5))<1e-7,"both reach bounds diagnose residuals and clamp without stretching");ReachLengths(rig.World(solved.Local),3,2);
            }
            Expect<IOException>(()=>HumanoidSolver.Correct(rig,profile,body,rig.Rest(),ReachAnalyticRecipe("LeftArm",origin+new HVec(0,0,3.5),"World","Nearest"),0),"legacy world-pole singularity rejects rather than inventing an axis");
            Expect<IOException>(()=>HumanoidSolver.Correct(rig,profile,body,rig.Rest(),ReachAnalyticRecipe("LeftArm",origin+new HVec(0,3.5,0),"TransportedRest","Nearest"),0),"antipodal transported-rest chart is explicitly unsupported");
            var parallel=ReachAnalyticRecipe("LeftArm",origin+new HVec(0,-3.5,0),"TransportedRest","Nearest");parallel.Contacts[0].UsePoleOverride=true;parallel.Contacts[0].Pole=new HVec(0,-1,0);
            Expect<IOException>(()=>HumanoidSolver.Correct(rig,profile,body,rig.Rest(),parallel,0),"invalid rest-pole chart rejects before a native bake");
        }
        // Reversed segment proportions put the upper joint on the other side of
        // the root at a full fold. Dense analytic samples must approach that exact
        // rotation continuously; just testing a long upper segment misses it.
        foreach(var lengths in new[]{Tuple.Create(2d,3d),Tuple.Create(.75,5d),Tuple.Create(5d,.75)})
        {
            var rig=ReachAnalyticRig(lengths.Item1,lengths.Item2,true);var origin=rig.World(rig.Rest())[2].P;var profile=ReachAnalyticProfile("LeftArm");var body=ReachAnalyticBody("LeftArm");HPose[] previous=null;
            double inner=Math.Abs(lengths.Item1-lengths.Item2),outer=lengths.Item1+lengths.Item2;
            for(int index=0;index<=1000;index++)
            {
                // Quadratic endpoint approach has a finite angular limit at reach
                // bounds, unlike an impossible constant-speed radial cusp.
                double phase=index/1000d,distance=inner+(outer-inner)*(1-Math.Cos(phase*Math.PI))/2;
                var goal=origin+new HVec(0,-distance,0);var solved=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),ReachAnalyticRecipe("LeftArm",goal,"TransportedRest","Nearest"),17.875);var world=rig.World(solved.Local);
                Assert(solved.Diagnostics.Count==0 && (world[4].P-goal).Length<1e-7,"contrasting short/long upper-segment trajectories meet independent exact/fractional goals");ReachLengths(world,lengths.Item1,lengths.Item2);
                if(previous!=null)foreach(int id in new[]{2,3,4})Assert(previous[id].R.DegreesFrom(solved.Local[id].R)<1,"no near-folded/near-straight rotation flip for contrasting segment proportions");previous=solved.Local;
            }
        }
        var opposingRig=ReachAnalyticRig(3,2,true);var opposingProfile=ReachAnalyticProfile("LeftArm");var opposingBody=ReachAnalyticBody("LeftArm");var opposingRest=opposingRig.World(opposingRig.Rest());HPose[] priorOpposing=null;
        for(int index=0;index<=400;index++)
        {
            double angle=Math.PI+(index-200)*.001;var preferred=opposingRig.Rest();preferred[2].R=opposingRest[1].R.Inverse()*HRotation.Axis(new HVec(1,0,0),angle)*opposingRest[2].R;
            var goal=opposingRest[2].P+new HVec(0,-3.5,0);var solved=HumanoidSolver.Correct(opposingRig,opposingProfile,opposingBody,preferred,ReachAnalyticRecipe("LeftArm",goal,"TransportedRest","Nearest"),9.375);var world=opposingRig.World(solved.Local);
            Assert((world[4].P-goal).Length<1e-7,"preferred segment crosses opposition while corrected effector preserves independently authored goal");ReachLengths(world,3,2);
            if(priorOpposing!=null)foreach(int id in new[]{2,3,4})Assert(priorOpposing[id].R.DegreesFrom(solved.Local[id].R)<.2,"stable reviewed-plane swing avoids shortest-arc segment antipode flip");priorOpposing=solved.Local;
        }
        var perpendicular=opposingRig.Rest();perpendicular[2].R=opposingRest[1].R.Inverse()*HRotation.Axis(new HVec(0,0,1),Math.PI/2)*opposingRest[2].R;
        Expect<IOException>(()=>HumanoidSolver.Correct(opposingRig,opposingProfile,opposingBody,perpendicular,ReachAnalyticRecipe("LeftArm",opposingRest[2].P+new HVec(0,-3.5,0),"TransportedRest","Nearest"),9.375),"preferred segment perpendicular to reviewed solve plane requires a different explicit chart");
        CheckHumanoidReachHemisphere();
        CheckHumanoidReachBlend();
        var equal=ReachAnalyticRig(2,2,false);var equalOrigin=equal.World(equal.Rest())[2].P;
        Expect<IOException>(()=>HumanoidSolver.Correct(equal,ReachAnalyticProfile("LeftArm"),ReachAnalyticBody("LeftArm"),equal.Rest(),ReachAnalyticRecipe("LeftArm",equalOrigin,"TransportedRest","Nearest"),0),"coincident equal-segment full fold requires an explicit different calibration chart");
        Log("PASS: independent continuous rest-pole crossings, exact/near inner and outer reach, analytic knee positions, preserved unequal lengths/axes, deterministic shuffled fractional samples, stable pelvis hemisphere and explicit singular/ambiguous rejection.");
    }
    private static void CheckHumanoidReachHemisphere()
    {
        var rig=ReachAnalyticRig(3,2,true);var profile=ReachAnalyticProfile("LeftLeg");var body=ReachAnalyticBody("LeftLeg");var origin=rig.World(rig.Rest())[2].P;HumanoidResult previous=null;
        for(int i=0;i<=200;i++)
        {
            double offset=(i-100)*.005;var goal=origin+new HVec(0,offset,0);var recipe=ReachAnalyticRecipe("LeftLeg",goal,"TransportedRest","RestHemisphere");var actual=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),recipe,10.375);
            Assert(actual.Diagnostics.Count==0 && Math.Abs(actual.PelvisOffset-(1+offset))<1e-8 && (rig.World(actual.Local)[4].P-goal).Length<1e-7,"independent inner shell hemisphere stays hip-above-goal across nearest-branch tie");
            if(previous!=null)Assert(Math.Abs(actual.PelvisOffset-previous.PelvisOffset)<.005001,"no coupled pelvis branch flip across inner-shell center");previous=actual;
        }
        var ambiguous=ReachAnalyticRecipe("LeftLeg",origin+new HVec(0,.1,0),"World","Nearest");
        Assert(HumanoidSolver.Correct(rig,profile,body,rig.Rest(),ambiguous,10).Diagnostics.Any(d=>d.Contains("Ambiguous")),"old nearest recipes preserve decode but cannot silently approve disconnected feasible branches");
        var impossible=ReachAnalyticRecipe("LeftLeg",origin+new HVec(1.5,4,0),"TransportedRest","RestHemisphere");
        Assert(HumanoidSolver.Correct(rig,profile,body,rig.Rest(),impossible,10).Diagnostics.Any(d=>d.Contains("conflict")),"disappearing selected reach hemisphere diagnoses failure instead of flipping to another branch");
    }
    private static void CheckHumanoidReachBlend()
    {
        var rig=ReachAnalyticRig(3,2,true);var profile=ReachAnalyticProfile("LeftLeg");var body=ReachAnalyticBody("LeftLeg");var origin=rig.World(rig.Rest())[2].P;
        const double band=.25;var samples=new Dictionary<int,HPose[]>();
        foreach(bool outer in new[]{true,false})
        {
            HPose[] previous=null;
            for(int index=0;index<=1000;index++)
            {
                double delta=-.1+index*.0005,distance=outer?5-delta:1+delta;var goal=origin+new HVec(0,-distance,0);var recipe=ReachAnalyticRecipe("LeftLeg",goal,"TransportedRest","RestHemisphere");recipe.ReachBlend=.05;
                var solved=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),recipe,11.375);var world=rig.World(solved.Local);
                double expected=delta<=0?delta:delta>=band?0:delta-band*Math.Pow(delta/band,4)*(10-15*(delta/band)+6*Math.Pow(delta/band,2));if(!outer)expected=-expected;
                Assert(solved.Diagnostics.Count==0 && Math.Abs(solved.PelvisOffset-expected)<1e-9,"independent quartic upper/lower reach-bound pelvis projection matches explicit formula");
                Assert((world[4].P-goal).Length<1e-7 && Math.Abs(solved.PelvisOffset)<=body.PelvisLimit,"reach blend never changes contact goal, target lengths or bounded pelvis range");ReachLengths(world,3,2);
                if(previous!=null)foreach(int bone in new[]{2,3,4})Assert(previous[bone].R.DegreesFrom(solved.Local[bone].R)<1,"quartic straight/folded boundary approach avoids a knee rotation cusp at dense samples");previous=solved.Local;
                if(outer)samples[index]=solved.Local;
            }
        }
        foreach(int index in Enumerable.Range(0,1001).OrderBy(i=>(i*557)%1001))
        {
            double delta=-.1+index*.0005;var recipe=ReachAnalyticRecipe("LeftLeg",origin+new HVec(0,-5+delta,0),"TransportedRest","RestHemisphere");recipe.ReachBlend=.05;
            var solved=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),recipe,11.375);for(int bone=0;bone<solved.Local.Length;bone++)Assert(solved.Local[bone].R.V.SequenceEqual(samples[index][bone].R.V) && (solved.Local[bone].P-samples[index][bone].P).Length==0,"reach-band result is bit-identical under shuffled fractional scrubbing");
        }
        // A knee angle goes as sqrt(reach slack). Quartic positional slack therefore
        // approaches zero quadratically, with zero first derivative at the boundary.
        var angles=new List<double>();foreach(double delta in new[]{.002,.004})
        {
            var recipe=ReachAnalyticRecipe("LeftLeg",origin+new HVec(0,-5+delta,0),"TransportedRest","RestHemisphere");recipe.ReachBlend=.05;var world=rig.World(HumanoidSolver.Correct(rig,profile,body,rig.Rest(),recipe,11.375).Local);var upper=world[3].P-world[2].P;angles.Add(Math.Atan2(upper.Z,-upper.Y));
        }
        Assert(angles[0]>0 && angles[1]/angles[0]>3.8 && angles[1]/angles[0]<4.1,"independent measured knee entry has quadratic rather than square-root boundary behavior");
        var legacy=ReachAnalyticRecipe("LeftLeg",origin+new HVec(0,-4.9,0),"World","Nearest");Assert(HumanoidSolver.Correct(rig,profile,body,rig.Rest(),legacy,11.375).PelvisOffset==0,"default zero reach band keeps the old exact clamp behavior");
        var margin=ReachAnalyticRecipe("LeftLeg",origin+new HVec(0,-4.8,0),"TransportedRest","RestHemisphere");margin.ReachBlend=.05;margin.Contacts[0].Margin=5;
        foreach(double time in new[]{0d,.001,.1,1,2.5,5,15})
        {
            var enter=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),margin,time);var leave=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),margin,30-time);Assert(enter.Diagnostics.Count==0 && leave.Diagnostics.Count==0,"reachable partial-contact margins need no stretch or hidden unreachable-goal exception");ReachLengths(rig.World(enter.Local),3,2);
            for(int bone=0;bone<enter.Local.Length;bone++)Assert((enter.Local[bone].P-leave.Local[bone].P).Length<1e-9 && enter.Local[bone].R.DegreesFrom(leave.Local[bone].R)<.00001,"explicit smooth margin entry and exit are symmetric under reversed fractional sampling");
            if(time==0)for(int bone=0;bone<enter.Local.Length;bone++)Assert((enter.Local[bone].P-rig.Rest()[bone].P).Length==0 && enter.Local[bone].R.V.SequenceEqual(rig.Rest()[bone].R.V),"zero-weight margin leaves preferred FK exactly intact");
            if(time>=5)Assert((rig.World(enter.Local)[4].P-margin.Contacts[0].Goal).Length<1e-7,"full-weight reachable margin meets the independent contact goal");
        }
        CheckHumanoidCrossingReachBounds();
    }
    private static void CheckHumanoidCrossingReachBounds()
    {
        var rig=ReachAnalyticRig(3,2,false);var rest=rig.World(rig.Rest());var profile=ReachAnalyticProfile("LeftLeg");var body=ReachAnalyticBody("LeftLeg");
        rig.Bones.Add(new HBone{Parent=1,Rest=new HPose{P=new HVec(1,0,0)}});rig.Bones.Add(new HBone{Parent=5,Rest=new HPose{P=new HVec(0,-3,0)}});rig.Bones.Add(new HBone{Parent=6,Rest=new HPose{P=new HVec(0,-2,0)}});
        profile.Chains.Add(new HChain{Role="RightLeg",Bones=new List<int>{5,6,7}});body.Landmarks.Add(new BodyLandmark{Role="RightLeg",Pole=new HVec(0,0,1)});rest=rig.World(rig.Rest());double? previous=null;
        for(int index=0;index<=200;index++)
        {
            double offset=(index-100)*.001;var left=rest[2].P+new HVec(0,-5+offset,0);var right=rest[5].P+new HVec(0,-5-offset,0);var recipe=ReachAnalyticRecipe("LeftLeg",left,"TransportedRest","RestHemisphere");recipe.ReachBlend=.05;recipe.Contacts.Add(new ContactInterval{Chain="RightLeg",Start=0,End=30,Goal=right,PolePolicy="TransportedRest",ReachPolicy="RestHemisphere"});
            var solved=HumanoidSolver.Correct(rig,profile,body,rig.Rest(),recipe,12.625);var world=rig.World(solved.Local);
            Assert(solved.Diagnostics.Count==0 && Math.Abs(solved.PelvisOffset+Math.Abs(offset))<1e-9 && (world[4].P-left).Length<1e-7 && (world[7].P-right).Length<1e-7,"opposing upper bounds intersect exactly while active bound winner crosses");ReachLengths(world,3,2);
            Assert(Math.Abs((world[6].P-world[5].P).Length-3)<1e-9 && Math.Abs((world[7].P-world[6].P).Length-2)<1e-9,"crossing shared-pelvis constraints preserve both unequal limbs");
            if(previous.HasValue)Assert(Math.Abs(solved.PelvisOffset-previous.Value)<=.001001,"opposing winner crossing is C0 without a pelvis branch jump");previous=solved.PelvisOffset;
        }
        // This max/min corner is intentionally not claimed C1: a native bake must
        // still satisfy its independent reopened pose/mesh thresholds or reject it.
    }
    // Minimal core-only rig. Full reviewed-profile/native validation uses the separate
    // original humanoid fixtures; these tests isolate two-bone geometry analytically.
    private static HRig ReachAnalyticRig(double first,double second,bool axes)
    {
        var rig=new HRig();var points=new[]{new HVec(),new HVec(.25,5,-.5),new HVec(.25,5,-.5),new HVec(.25,5-first,-.5),new HVec(.25,5-first-second,-.5)};
        var frames=Enumerable.Range(0,5).Select(i=>axes?HRotation.Axis(new HVec(1,.5,.2),i*.23):new HRotation()).ToArray();
        for(int i=0;i<5;i++){int parent=i-1;rig.Bones.Add(new HBone{Parent=parent,Rest=new HPose{P=parent<0?points[i]:frames[parent].Inverse().Apply(points[i]-points[parent]),R=parent<0?frames[i]:frames[parent].Inverse()*frames[i]}});}return rig;
    }
    private static HumanoidProfile ReachAnalyticProfile(string limb)=>new HumanoidProfile{Chains=new List<HChain>{new HChain{Role="Pelvis",Bones=new List<int>{1}},new HChain{Role=limb,Bones=new List<int>{2,3,4}}}};
    private static BodyProfile ReachAnalyticBody(string limb)=>new BodyProfile{LegLength=5,PelvisLimit=2.5,Landmarks=new List<BodyLandmark>{new BodyLandmark{Role=limb,Local=new HVec(),Pole=new HVec(0,0,1)}}};
    private static HumanoidRecipe ReachAnalyticRecipe(string limb,HVec goal,string pole,string reach)=>new HumanoidRecipe{Contacts=new List<ContactInterval>{new ContactInterval{Chain=limb,Start=0,End=30,Goal=goal,PolePolicy=pole,ReachPolicy=reach}}};
    private static void ReachLengths(HPose[] world,double first,double second)
    {Assert(Math.Abs((world[3].P-world[2].P).Length-first)<1e-9 && Math.Abs((world[4].P-world[3].P).Length-second)<1e-9,"analytic solve retains exact original unequal segment lengths");}
}
