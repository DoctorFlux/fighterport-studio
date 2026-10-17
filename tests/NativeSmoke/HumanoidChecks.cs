// Original authored humanoid/contact goals, FighterPort 2026-10-17. No game data.
using BrawlCrate.Authoring;
using BrawlCrate.Authoring.Humanoid;
using BrawlCrate.Sources;
using BrawlCrate.UI;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

internal static partial class NativeSmoke
{
    private static string HumanoidResolution(string file,string name)
    {
        var w=new CompanionWorkspace(Path.Combine(_output,"storage"));w.AddSource(file,CancellationToken.None);w.SelectModel(w.Resources.Single(r=>r.Kind=="MDL0"));foreach(string n in w.RequiredTextures)w.SelectTexture(n,w.TextureCandidates(n).Single());w.SelectMotion(w.Resources.Single(r=>r.Kind=="CHR0"&&r.Name=="OriginalPlanted"));string path=Path.Combine(_output,name+".fpcompanion");w.Save(path);return path;
    }
    private static HumanoidRecipe PlantedRecipe(CharacterProject p,string clip,string native,bool targetStout)
    {
        var r=p.CreateHumanoidRecipe(clip,native);foreach(string side in new[]{"Left","Right"})r.Contacts.Add(new ContactInterval{Chain=side+"Leg",Start=0,End=30,Goal=new HVec((side=="Left"?1:-1)*(targetStout?1.5:1),0,targetStout?.4:.405),Evidence="Original fixture author's independent sole goal from authored rest mesh: floor Y=0, fixed hip-width X, design sole-center Z. Not computed by the retarget solver."});return r;
    }
    private static void ExerciseHumanoid(bool gui)
    {
        string a=OriginalHumanoidFixture.Create(Path.Combine(_output,"humanoid-a"),false,9),b=OriginalHumanoidFixture.Create(Path.Combine(_output,"humanoid-b"),true,11);
        CheckHumanoidAdmission(a);
        var originals=new[]{a,b}.ToDictionary(n=>n,Hash);var p=new CharacterProject(Path.Combine(_output,"storage"));p.MigrateToSharedLibrary(Path.Combine(_output,"library"));string aid=p.RegisterSource(HumanoidResolution(a,"human-a"),"Original tall"),bid=p.RegisterSource(HumanoidResolution(b,"human-b"),"Original stout");
        foreach(string id in new[]{aid,bid})
        {
            p.DetectHumanoid(id);var setup=p.HumanoidSetupFor(id);Assert(setup.Profile!=null && !setup.Profile.Reviewed && !setup.Body.Reviewed,"detection proposes, never approves");
            var c=p.State.Characters.Single(n=>n.Id==id);Assert(c.Bones.Count>40 && setup.Profile.Chain("LeftArm").Bones.Count==3 && setup.Profile.Chain("LeftLeg").Bones.Count==3,"full humanoid authored limbs/fingers/toes and explicit helpers");
            foreach(var chain in setup.Profile.Chains)foreach(int index in chain.Bones)Assert(index>=0 && index<c.Bones.Count,"author verifies exact original index/identity");
            p.ReviewHumanoid(id,"Original fixture author's exact hierarchy/rest/mesh/sole review; known generated anatomy. This is not commercial mapping approval.");
        }
        var ac=p.State.Animations.Single(n=>n.SourceCharacterId==aid);var bc=p.State.Animations.Single(n=>n.SourceCharacterId==bid);Expect<IOException>(()=>p.Assign(ac.Id,bid,"unsafe"),"direct different-rig rejection retained");
        var recipe=PlantedRecipe(p,ac.Id,bc.Id,true);p.SaveHumanoidRecipe(recipe);var trial=p.PreviewHumanoid(recipe);File.WriteAllText(Path.Combine(_output,"humanoid-contact-report.txt"),trial.Report.ToString());
        Assert(trial.Report.FkContactError>.1 && trial.Report.CorrectedContactError<.005 && trial.Report.CorrectedContactError<trial.Report.FkContactError/10,"independent planted goals improve over FK by >10x, not serialization-only validation");
        VerifyHumanoidFeet(trial.CorrectedPath,true);VerifyHumanoidSourcePlant(a);
        string instance=p.AssignHumanoid(recipe.Id,"Original fitted animation");p.ExportInstance(instance,Path.Combine(_output,"humanoid-output.brres"));VerifyHumanoidFeet(Path.Combine(_output,"humanoid-output.brres"),true);p.Undo();Assert(!p.State.Instances.Any(i=>i.Id==instance),"fit commits one undoable independent instance");p.Redo();
        var reverse=PlantedRecipe(p,bc.Id,ac.Id,false);p.SaveHumanoidRecipe(reverse);var reverseTrial=p.PreviewHumanoid(reverse);Assert(reverseTrial.Report.FkContactError>.1 && reverseTrial.Report.CorrectedContactError<.005,"reverse proportions/contact proof is independent");VerifyHumanoidFeet(reverseTrial.CorrectedPath,false);
        CheckHumanoidDeterminism(p,recipe,a,b);
        CheckHumanoidReachShell(p,bid);
        CheckHumanoidHandGoals(p,ac.Id,bc.Id);
        var other=new CharacterProject(Path.Combine(_output,"storage"));other.UseSharedClip(ac.Id,1,p.SharedLibrary.Root);other.UseSharedClip(bc.Id,1,p.SharedLibrary.Root);other.DetectHumanoid(aid);other.DetectHumanoid(bid);Assert(other.State.Humanoids.All(h=>h.Reviewed) && other.State.Bodies.All(h=>h.Reviewed),"reviewed rig/body reused across separately saved projects without re-mapping clips");other.Assign(bc.Id,bid,"Untouched other project");string otherFile=Path.Combine(_output,"humanoid-other.fpcharacter");other.Save(otherFile);string otherHash=Hash(otherFile),sourcePin=p.SharedLibrary.Pin("animations",ac.Id).MetadataHash;
        string saved=Path.Combine(_output,"humanoid.fpcharacter");p.Save(saved);var loaded=CharacterProject.Load(saved,Path.Combine(_output,"storage"));loaded.ExportInstance(instance,Path.Combine(_output,"humanoid-reopened.brres"));VerifyHumanoidFeet(Path.Combine(_output,"humanoid-reopened.brres"),true);
        var bad=PlantedRecipe(p,ac.Id,bc.Id,true);bad.Contacts[0].Goal=new HVec(1.5,100,.4);p.SaveHumanoidRecipe(bad);var unreachable=p.PreviewHumanoid(bad);Assert(unreachable.Report.UnreachableSamples>0 && unreachable.Report.CorrectedContactError>1,"unreachable goals report residuals without stretching");int count=p.State.Instances.Count;Expect<IOException>(()=>p.AssignHumanoid(bad.Id,"unreachable"),"bad quality cannot become a passing assignment");Assert(p.State.Instances.Count==count,"failed fit leaves independent assignments intact");
        Expect<OperationCanceledException>(()=>p.PreviewHumanoid(recipe,new CancellationToken(true)),"cancel before native work");
        var invalid=PlantedRecipe(p,ac.Id,bc.Id,true);invalid.Contacts[0].Anchor="Prop";Expect<IOException>(()=>p.SaveHumanoidRecipe(invalid),"missing prop/native relationship cannot be invented");
        var changed=PlantedRecipe(p,ac.Id,bc.Id,true);changed.Id=recipe.Id;changed.Contacts[0].Goal=new HVec(0,1,0);Expect<IOException>(()=>p.SaveHumanoidRecipe(changed),"assigned recipe remains immutable");
        var body=p.HumanoidSetupFor(bid).Body;body.GroundY+=1;Expect<IOException>(()=>p.UpdateBodyProfile(bid,body),"reviewed shared body revision immutable");
        string volume=OriginalHumanoidFixture.Create(Path.Combine(_output,"humanoid-volume"),true,11,true);originals.Add(volume,Hash(volume));string vid=p.RegisterSource(HumanoidResolution(volume,"human-volume"),"Original same rig / wider body");p.DetectHumanoid(vid);var volumeSetup=p.HumanoidSetupFor(vid);Assert(volumeSetup.Profile.RigFingerprint==p.HumanoidSetupFor(bid).Profile.RigFingerprint && volumeSetup.Body.Id!=p.HumanoidSetupFor(bid).Body.Id && volumeSetup.Body.TorsoRadius>p.HumanoidSetupFor(bid).Body.TorsoRadius,"same skeleton, different mesh volume => separate body-fit revision");
        CheckHumanoidChannels(p,aid,bid,a,b,originals);
        p.Save(saved);if(gui){p.SaveHumanoidRecipe(PlantedRecipe(p,ac.Id,bc.Id,true));HumanoidGui(p,instance,aid,bid,ac.Id,bc.Id);}
        Assert(Hash(otherFile)==otherHash && p.SharedLibrary.Pin("animations",ac.Id).MetadataHash==sourcePin,"untouched other project and immutable shared clip unchanged");foreach(var h in originals)Assert(Hash(h.Key)==h.Value,"original humanoid inputs unchanged");File.WriteAllLines(Path.Combine(_output,"humanoid-originals.sha256"),originals.Select(h=>h.Value+"\t"+h.Key));
        File.WriteAllText(Path.Combine(_output,"humanoid-instance.txt"),instance);Log("PASS: full original humanoid rigs with different proportions/axes/parents/spine/finger/helper counts; planted source, native FK versus contact quality, reverse, body identity, deterministic scrubbing, shared reviews, independent project/native reload, budgets/rejections and unchanged originals.");
    }
    private static void VerifyHumanoidSourcePlant(string file)
    {
        using(var root=NodeFactory.FromFile(null,file)){var model=Find<MDL0Node>(root);model.Populate();var motion=root.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(m=>m.Name=="OriginalPlanted");foreach(float time in new[]{0f,7.25f,15f,23.75f,30f}){model.ApplyCHR(motion,time+1);foreach(string side in new[]{"Left","Right"}){var foot=model.BoneCache.OfType<MDL0BoneNode>().Single(n=>n.Name==side+"Foot");Assert(Math.Abs(foot.Matrix[13]-.5)<.005 && Math.Abs(foot.Matrix[14])<.005,"independently authored source leg cosine-law contact, fractional native decode");}}}
    }
    private static void CheckHumanoidReachShell(CharacterProject project,string target)
    {
        var rig=project.HumanoidRigFor(target);var setup=project.HumanoidSetupFor(target);var pose=rig.Rest();var rest=rig.World(pose);var ids=setup.Profile.Chain("LeftLeg").Bones;
        double first=(rest[ids[1]].P-rest[ids[0]].P).Length,second=(rest[ids[2]].P-rest[ids[1]].P).Length,inner=Math.Abs(first-second),distance=inner-.25;
        var goal=rest[ids[0]].P+new HVec(0,-distance,0)+rest[ids[2]].R.Apply(setup.Body.Landmark("LeftLeg").Local);
        var recipe=new HumanoidRecipe();recipe.Contacts.Add(new ContactInterval{Chain="LeftLeg",Start=0,End=30,Goal=goal});
        var solved=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,pose,recipe,15);
        Assert(Math.Abs(solved.PelvisOffset-.25)<1e-6 && solved.ContactError<1e-6 && solved.Diagnostics.Count==0,"independent unequal-limb inner reach shell requires +.25 pelvis Y, not a maximum-reach ball");
        var right=setup.Profile.Chain("RightLeg").Bones;recipe.Contacts.Add(new ContactInterval{Chain="RightLeg",Start=0,End=30,Goal=rest[right[0]].P+new HVec(0,distance,0)+rest[right[2]].R.Apply(setup.Body.Landmark("RightLeg").Local)});
        solved=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,pose,recipe,15);Assert(solved.Diagnostics.Any(d=>d.Contains("conflict")) && solved.ContactError>.1,"opposing inner reach shells diagnose an impossible coupled pelvis instead of stretching");
        recipe.Contacts.RemoveAt(1);recipe.Stance=new HumanoidStance{PelvisOffset=new HVec(.1,.2,.3)};recipe.Contacts.Clear();solved=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,pose,recipe,0);var shifted=rig.World(solved.Local);
        Assert((shifted[ids[0]].P-rest[ids[0]].P-new HVec(.1,.2,.3)).Length<1e-9 && Math.Abs((shifted[ids[1]].P-shifted[ids[0]].P).Length-first)<1e-9,"clip-specific stance translates pelvis without changing native limb lengths");
        Log("PASS: independent unequal-segment lower reach and opposing-contact diagnostics; explicit stance keeps target lengths.");
    }
    private static void VerifyHumanoidFeet(string file,bool stout)
    {
        using(var root=NodeFactory.FromFile(null,file))
        {
            var model=Find<MDL0Node>(root);model.Populate();var motion=root.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(m=>m.Name=="OriginalPlanted");Assert(model.Version==(stout?11:9) && motion.FrameCount==31 && !motion.Loop,"native versions/timing preserved");
            foreach(float time in new[]{0f,.25f,7.25f,15f,23.75f,29.75f,30f})
            {
                model.ApplyCHR(motion,time+1);foreach(string side in new[]{"Left","Right"})
                {
                    var foot=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name==side+"Foot");var center=foot.Matrix*new BrawlLib.Internal.Vector3(0,-.5f,.4f);Assert(Math.Abs(center._x-(side=="Left"?1:-1)*(stout?1.5:1))<.005 && Math.Abs(center._y)<.005 && Math.Abs(center._z-.4)<.005,"independent native sole center/floor/slide goal <= .005 units");
                    var vertices=model._objList.OfType<MDL0ObjectNode>().SelectMany(m=>m.Vertices).Where(v=>v.GetMatrixNode().IsPrimaryNode && v.GetMatrixNode().Weights[0].Bone==foot).ToArray();Assert(vertices.Length>0 && vertices.Min(v=>v.WeightedPosition._y)>-.005 && Math.Abs(vertices.Min(v=>v.WeightedPosition._y))<.005,"decoded rigid sole geometry stays on floor without mesh resizing");
                }
            }
        }
    }
    private static HPose[] HumanoidSample(MDL0Node model,CHR0Node motion,float time)
    {model.ApplyCHR(motion,time+1);return model.BoneCache.OfType<MDL0BoneNode>().Select(b=>{var p=b.FrameState;var r=RigidTransform.From(new RigPose{RX=p._rotate._x,RY=p._rotate._y,RZ=p._rotate._z});return new HPose{P=new HVec(p._translate._x,p._translate._y,p._translate._z),R=new HRotation{V=r.R}};}).ToArray();}
    private static void CheckHumanoidDeterminism(CharacterProject p,HumanoidRecipe r,string a,string b)
    {
        using(var ar=NodeFactory.FromFile(null,a))using(var br=NodeFactory.FromFile(null,b))
        {
            var am=Find<MDL0Node>(ar);am.Populate();var bm=Find<MDL0Node>(br);bm.Populate();var aa=ar.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(m=>m.Name=="OriginalPlanted");var ba=br.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(m=>m.Name=="OriginalPlanted");var sp=p.HumanoidSetupFor(r.SourceCharacterId);var tp=p.HumanoidSetupFor(r.TargetCharacterId);var sr=p.HumanoidRigFor(r.SourceCharacterId);var tr=p.HumanoidRigFor(r.TargetCharacterId);
            Func<float,HPose[]> solve=time=>HumanoidSolver.Correct(tr,tp.Profile,tp.Body,HumanoidSolver.Fk(sr,sp.Profile,sp.Body,tr,tp.Profile,tp.Body,HumanoidSample(am,aa,time),HumanoidSample(bm,ba,time)),r,time).Local;
            var expected=solve(7.25f);foreach(float time in new[]{29.75f,0f,15f,3.5f,30f})solve(time);var actual=solve(7.25f);for(int i=0;i<actual.Length;i++)Assert((actual[i].P-expected[i].P).Length==0 && actual[i].R.V.SequenceEqual(expected[i].R.V),"random/reverse scrubbing does not alter solved pose");
            var invalidPose=sr.Rest();invalidPose[0].P=new HVec(double.NaN,0,0);Expect<IOException>(()=>sr.World(invalidPose),"NaN pose cannot bypass native quality comparisons");invalidPose=sr.Rest();invalidPose[0].R.V[0]=double.PositiveInfinity;Expect<IOException>(()=>sr.World(invalidPose),"non-finite common rotation rejects before solving/baking");
            // Re-express every source bone in a different proper local coordinate basis.
            // This changes axes, not the physical rig/motion. World rest deltas must agree.
            var basis=sr.Bones.Select((bone,i)=>HRotation.Axis(new HVec(0,0,1),(i%3)*.35)).ToArray();var reexpressed=new HRig{Fingerprint=sr.Fingerprint,Interpretation=sr.Interpretation};var sourcePose=HumanoidSample(am,aa,7.25f);var rebased=new HPose[sourcePose.Length];
            for(int i=0;i<sr.Bones.Count;i++){var parent=sr.Bones[i].Parent<0?new HRotation():basis[sr.Bones[i].Parent];reexpressed.Bones.Add(new HBone{Parent=sr.Bones[i].Parent,Rest=new HPose{P=parent.Inverse().Apply(sr.Bones[i].Rest.P),R=parent.Inverse()*sr.Bones[i].Rest.R*basis[i]}});rebased[i]=new HPose{P=parent.Inverse().Apply(sourcePose[i].P),R=parent.Inverse()*sourcePose[i].R*basis[i]};}
            var reference=HumanoidSample(bm,ba,7.25f);var before=HumanoidSolver.Fk(sr,sp.Profile,sp.Body,tr,tp.Profile,tp.Body,sourcePose,reference);var after=HumanoidSolver.Fk(reexpressed,sp.Profile,sp.Body,tr,tp.Profile,tp.Body,rebased,reference);for(int i=0;i<before.Length;i++)Assert((before[i].P-after[i].P).Length<1e-9 && before[i].R.DegreesFrom(after[i].R)<.00001,"physical FK result invariant under independent proper source-local basis re-expression");
        }
    }
    private static void CheckHumanoidChannels(CharacterProject p,string aid,string bid,string a,string b,System.Collections.Generic.Dictionary<string,string> originals)
    {
        var w=new CompanionWorkspace(Path.Combine(_output,"storage"));w.AddSource(a,CancellationToken.None);var aerial=w.Resources.Single(r=>r.Kind=="CHR0"&&r.Name=="OriginalAerial");string aerialId=p.AddAnimationFromContainer(aid,a,"",aerial.TreePath);string native=p.State.Animations.Single(n=>n.SourceCharacterId==bid&&n.Name=="OriginalPlanted").Id;var recipe=p.CreateHumanoidRecipe(aerialId,native);p.SaveHumanoidRecipe(recipe);var trial=p.PreviewHumanoid(recipe);
        using(var root=NodeFactory.FromFile(null,trial.CorrectedPath)){var model=Find<MDL0Node>(root);model.Populate();var motion=root.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(n=>n.Name=="OriginalPlanted");model.ApplyCHR(motion,16);var foot=model.BoneCache.OfType<MDL0BoneNode>().Single(n=>n.Name=="LeftFoot");Assert(foot.Matrix[13]>1.5,"aerial output has no default planted-foot constraint");}
        var turn=w.Resources.Single(r=>r.Kind=="CHR0"&&r.Name=="OriginalFullTurn");string turnId=p.AddAnimationFromContainer(aid,a,"",turn.TreePath);var turning=p.CreateHumanoidRecipe(turnId,native);var turned=p.PreviewHumanoid(turning);
        using(var root=NodeFactory.FromFile(null,turned.CorrectedPath)){var motion=root.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(n=>n.Name=="OriginalPlanted");var track=motion.Children.OfType<CHR0EntryNode>().Single(n=>n.Name=="LeftForearm");Assert(Math.Abs(track.GetFrameValue(3,30)-track.GetFrameValue(3,0))>359,"native winding does not collapse a full source turn");}
        var loop=w.Resources.Single(r=>r.Kind=="CHR0"&&r.Name=="OriginalLoop");string loopId=p.AddAnimationFromContainer(aid,a,"",loop.TreePath);var looping=p.CreateHumanoidRecipe(loopId,native);var looped=p.PreviewHumanoid(looping);Assert(looped.Report.Samples==129,"nonempty native finger loop includes quarter-frame inherited seam/wrap samples");
        string negatives=Path.Combine(_output,"humanoid-negative-inputs");Directory.CreateDirectory(negatives);
        foreach(string kind in new[]{"fast-winding","joint-translation","animated-scale","frame-budget"})
        {
            string folder=Path.Combine(negatives,kind);Directory.CreateDirectory(folder);string path=Path.Combine(folder,kind+".chr0");using(var motion=new CHR0Node{Name=kind,Version=4,FrameCount=kind=="frame-budget"?1025:3,Loop=false}){var track=motion.CreateEntry("LeftForearm");track.PreserveInterpolation=true;if(kind=="fast-winding"){track.SetKeyframe(3,0,0,true)._tangent=720;track.SetKeyframe(3,2,1440,true)._tangent=720;}else if(kind=="joint-translation"){track.SetKeyframe(7,0,0,true);track.SetKeyframe(7,1,1,true);track.SetKeyframe(7,2,0,true);}else if(kind=="animated-scale")track.SetKeyframe(0,0,2,true);else track.SetKeyframe(3,0,0,true);motion.Export(path);}originals.Add(path,Hash(path));string unsupported=p.AddAnimation(aid,path);var bad=p.CreateHumanoidRecipe(unsupported,native);int before=p.State.Instances.Count;Expect<IOException>(()=>p.PreviewHumanoid(bad),"explicit humanoid unsupported "+kind+" rejects, retaining original FK guards");Assert(p.State.Instances.Count==before,"unsupported native trial never commits a partial assignment");
        }
        Log("PASS: independent original aerial/no-lock, full-turn native channel and animated-finger native loop-seam checks.");
    }
    private static void CheckHumanoidHandGoals(CharacterProject p,string clip,string native)
    {
        var recipe=p.CreateHumanoidRecipe(clip,native);var rig=p.HumanoidRigFor(recipe.TargetCharacterId);var setup=p.HumanoidSetupFor(recipe.TargetCharacterId);var rest=rig.World(rig.Rest());
        // Author-selected reachable gesture beside the head, in head-local coordinates.
        recipe.Contacts.Add(new ContactInterval{Chain="LeftArm",Anchor="Head",Start=0,End=30,Goal=new HVec(1.5,-1.5,.5),Evidence="Original fixture author's hand-to-head offset; target goal is independent of the solver."});var trial=p.PreviewHumanoid(recipe);
        VerifyNativeHumanoidGoal(trial.CorrectedPath,"LeftHand","Head",new HVec(1.5,-1.5,.5));
        // Both wrists meet at one independently chosen world point in front of the chest.
        var meeting=p.CreateHumanoidRecipe(clip,native);double chestY=rest[setup.Profile.Chain("Spine").Bones.Last()].P.Y;
        foreach(string side in new[]{"Left","Right"})meeting.Contacts.Add(new ContactInterval{Chain=side+"Arm",Start=0,End=30,Goal=new HVec(0,chestY-.5,1.5),Evidence="Original shared two-wrist contact point, not a prop/native attachment claim."});
        var joined=p.PreviewHumanoid(meeting);foreach(string side in new[]{"Left","Right"})VerifyNativeHumanoidGoal(joined.CorrectedPath,side+"Hand",null,new HVec(0,chestY-.5,1.5));
        var margin=meeting.Contacts[0];margin.Margin=5;Assert(margin.Weight(0)==0 && Math.Abs(margin.Weight(2.5)-.5)<1e-12 && margin.Weight(10)==1 && margin.Weight(30)==0,"explicit contact margins preserve entry/exit phases");
        var lowering=p.CreateHumanoidRecipe(clip,native);foreach(string side in new[]{"Left","Right"}){var foot=rest[setup.Profile.Chain(side+"Leg").Bones.Last()].Point(setup.Body.Landmark(side+"Leg").Local);lowering.Contacts.Add(new ContactInterval{Chain=side+"Leg",Start=0,End=30,Margin=5,Goal=foot+new HVec(0,-.5,0)});}var start=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,rig.Rest(),lowering,0);var half=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,rig.Rest(),lowering,2.5);var full=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,rig.Rest(),lowering,5);Assert(Math.Abs(start.PelvisOffset)<1e-9 && Math.Abs(half.PelvisOffset+.25)<1e-6 && Math.Abs(full.PelvisOffset+.5)<1e-6,"independent half-weight pelvis displacement is continuous through contact margins, no full-weight threshold jump");
        Log("PASS: fresh native body-relative hand-to-head and independent two-wrist contact goals, separate from prop support; explicit interval margins.");
    }
    private static void VerifyNativeHumanoidGoal(string file,string end,string anchor,HVec goal)
    {
        using(var root=NodeFactory.FromFile(null,file)){var model=Find<MDL0Node>(root);model.Populate();var motion=root.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(n=>n.Name=="OriginalPlanted");foreach(float time in new[]{0f,7.25f,15f,29.75f,30f}){model.ApplyCHR(motion,time+1);var hand=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name==end);var expected=goal;if(anchor!=null){var bone=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name==anchor);var v=bone.Matrix*new BrawlLib.Internal.Vector3((float)goal.X,(float)goal.Y,(float)goal.Z);expected=new HVec(v._x,v._y,v._z);}Assert((new HVec(hand.Matrix[12],hand.Matrix[13],hand.Matrix[14])-expected).Length<.005,"decoded native wrist meets independently authored body/world goal <=.005 units");}}
    }
    private static void HumanoidGui(CharacterProject p,string instance,string source,string target,string clip,string native)
    {
        var session=PreviewAuthoringSession.For(p);var main=MainForm.Instance;Exception failure=null;
        main.Shown+=(s,e)=>main.BeginInvoke(new Action(()=>{
            try
            {
                var selectedWorkspace=new CompanionWorkspace(p.StorageRoot);using(var picker=new BrawlContainerImportForm(selectedWorkspace))
                {picker.Show(main);Ws<TextBox>(picker,"containerSourcePath").Text=Path.Combine(_output,"selected-inputs","RGB5A3","RGB5A3-motions.pac");Ws<Button>(picker,"containerList").PerformClick();WaitFor(()=>Ws<ListBox>(picker,"containerChoices").Items.Count==1,"actual raw container candidate list");Ws<ListBox>(picker,"containerChoices").SelectedIndex=0;Ws<Button>(picker,"containerStage").PerformClick();WaitFor(()=>picker.IsDisposed,"actual selected BRRES staging closes picker");Assert(selectedWorkspace.Resources.Any(n=>n.Kind=="PLT0") && selectedWorkspace.Resources.All(n=>n.TreePath.StartsWith("2/")),"actual picker admits only explicitly selected original container, retaining palette and indexed identities");}
                var editor=session.ShowInstance(instance);var host=editor.FindForm();PrepareLibraryEditor(editor);Pump(80);
                var bar=Ws<ToolStrip>(host,"previewWorkspaceBar");if(!Ws<PreviewWorkspacePane>(host,"previewWorkspace").Visible)bar.Items.Cast<ToolStripItem>().Single(n=>n.Name=="previewWorkspaceToggle").PerformClick();
                WsChoose(Ws<ComboBox>(host,"workspaceTarget"),target);WsChoose(Ws<ComboBox>(host,"workspaceClip"),clip);WsChoose(Ws<ComboBox>(host,"workspaceDestination"),native);
                int clips=p.State.Animations.Count;using(var picker=new BrawlContainerImportForm(session,source))
                {picker.Show(host);Ws<TextBox>(picker,"containerSourcePath").Text=Path.Combine(_output,"selected-inputs","RGB565","RGB565-motions.pac");Ws<Button>(picker,"containerList").PerformClick();WaitFor(()=>Ws<ListBox>(picker,"containerChoices").Items.Count==1,"source clip container list");Ws<ListBox>(picker,"containerChoices").SelectedIndex=0;Ws<Button>(picker,"containerStage").PerformClick();WaitFor(()=>Ws<ListBox>(picker,"containerMotionChoices").Items.Count>0,"selected container motion choices");Pump(20);Ws<ListBox>(picker,"containerMotionChoices").SelectedIndex=0;Ws<Button>(picker,"containerImportClip").PerformClick();WaitFor(()=>p.State.Animations.Count==clips+1 && !session.Busy,"actual source-bound container import");Pump(30);picker.Close();}Assert(p.State.Animations.Count==clips+1,"register once and add selected native motion through actual session dialog");
                HumanoidClick(host,"humanoidDetect");Assert(Ws<DataGridView>(host,"humanoidChains").Rows.Count>20,"previewer reuses reviewed semantic chains without overwriting them");
                HumanoidClick(host,"humanoidAdvanced");Assert(Ws<DataGridView>(host,"humanoidLandmarks").Rows.Count==4,"advanced mesh-bound soles/wrists/poles in same workspace");
                var beforeStance=WorkspaceStateXml(p);editor.SetFrame(1);Ws<NumericUpDown>(host,"humanoidStanceSourcePhase").Value=7.25m;HumanoidClick(host,"humanoidCalibrateStance");
                Assert(p.State.HumanoidRecipes.Last(r=>r.AnimationId==clip&&r.TargetAnimationId==native).Stance.SourceTime==7.25 && Ws<Label>(host,"humanoidStanceStatus").Text.Contains(7.25.ToString()),"actual calibration control persists explicit fractional source phase and target native phase");
                WsClick(host,"workspaceUndo");Assert(beforeStance.SequenceEqual(WorkspaceStateXml(p)),"stance calibration is one undoable project command");WsClick(host,"workspaceRedo");p.Save(p.FilePath);Assert(WorkspaceStateXml(CharacterProject.Load(p.FilePath,p.StorageRoot)).SequenceEqual(WorkspaceStateXml(p)),"stance phases and offset survive project reload");WsClick(host,"workspaceUndo");
                HumanoidClick(host,"humanoidFit");var panel=Ws<HumanoidWorkspacePanel>(host,"humanoidWorkspace");Assert(panel.Trial!=null && panel.Trial.Report.ProfilesReviewed,"actual Fit button generated reviewed native trial");
                HumanoidClick(host,"humanoidPreviewFk");editor.SetFrame(16);CaptureViewport(editor,"humanoid-fk-native.png");Assert(editor.AuthoringReadOnly && editor.AuthoringDocument.TargetId==target,"FK comparison is explicit read-only target");
                HumanoidClick(host,"humanoidPreviewCorrected");editor.SetFrame(16);CaptureViewport(editor,"humanoid-corrected-native.png");
                int before=editor.CurrentFrame;editor.PlaybackPanel.btnPlay.PerformClick();Pump(200);editor.PlaybackPanel.btnPlay.PerformClick();Assert(editor.CurrentFrame!=before,"corrected output uses inherited native playback");
                Ws<TextBox>(host,"humanoidExportPath").Text=Path.Combine(_output,"humanoid-gui-trial.brres");HumanoidClick(host,"humanoidExportTrial");VerifyHumanoidFeet(Path.Combine(_output,"humanoid-gui-trial.brres"),true);
                // An assigned recipe forks on edit; the current independent output remains unchanged.
                string outputHash=p.State.Instances.Single(i=>i.Id==instance).WorkingHash;var prior=WorkspaceStateXml(p);
                Ws<ComboBox>(host,"humanoidContactLimb").SelectedItem="LeftLeg";editor.SetFrame(1);HumanoidClick(host,"humanoidReadContact");HumanoidClick(host,"humanoidSaveContact");
                Assert(p.State.HumanoidRecipes.Count>0 && !prior.SequenceEqual(WorkspaceStateXml(p)),"contact controls save a new recipe transaction");WsClick(host,"workspaceUndo");Assert(prior.SequenceEqual(WorkspaceStateXml(p)),"contact edit has one project undo");WsClick(host,"workspaceRedo");
                Ws<CheckBox>(host,"humanoidEditConstraints").Checked=true;editor.SetFrame(16);CaptureViewport(editor,"humanoid-handles-native.png");
                var handles=panel.GetType().GetMethod("Handles",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);var points=((System.Collections.IEnumerable)handles.Invoke(panel,null)).Cast<object>().ToArray();Assert(points.Length==3,"contact/pole/pelvis virtual viewport handles");
                var pelvis=points.Single(h=>(string)h.GetType().GetField("Kind").GetValue(h)=="pelvis");var world=(HVec)pelvis.GetType().GetField("World").GetValue(pelvis);var vp=editor.ModelPanel.CurrentViewport;var screen=vp.Project(new BrawlLib.Internal.Vector3((float)world.X,(float)world.Y,(float)world.Z));
                int x=(int)(screen._x+vp.Region.X),y=(int)vp.LocalToWorldYf(screen._y);var old=WorkspaceStateXml(p);
                panel.GetType().GetMethod("HandleDown",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(panel,new object[]{editor.ModelPanel,new MouseEventArgs(MouseButtons.Left,1,x,y,0)});string otherClip=p.State.Animations.First(a=>a.SourceCharacterId==source && a.Id!=clip).Id;WsChoose(Ws<ComboBox>(host,"workspaceClip"),otherClip);panel.GetType().GetMethod("HandleUp",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(panel,new object[]{editor.ModelPanel,new MouseEventArgs(MouseButtons.Left,1,x,y-8,0)});Assert(old.SequenceEqual(WorkspaceStateXml(p)) && Ws<Label>(host,"humanoidStatus").Text.StartsWith("Stopped:"),"changing source clip during a viewport drag cannot mutate another recipe");WsChoose(Ws<ComboBox>(host,"workspaceClip"),clip);
                foreach(var action in new[]{new{Method="HandleDown",Y=y},new{Method="HandleMove",Y=y-8},new{Method="HandleUp",Y=y-8}})panel.GetType().GetMethod(action.Method,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(panel,new object[]{editor.ModelPanel,new MouseEventArgs(MouseButtons.Left,1,x,action.Y,0)});
                Assert(!old.SequenceEqual(WorkspaceStateXml(p)),"viewport pelvis handle commits an actual recipe change");WsClick(host,"workspaceUndo");Assert(old.SequenceEqual(WorkspaceStateXml(p)),"viewport drag commits exactly one undo transaction");WsClick(host,"workspaceRedo");Ws<CheckBox>(host,"humanoidEditConstraints").Checked=false;
                Assert(p.State.Instances.Single(i=>i.Id==instance).WorkingHash==outputHash,"virtual controls never mutate already baked output");
                // Refit an explicitly saved contact recipe, then apply through the actual previewer.
                WsClick(host,"workspaceUndo");HumanoidClick(host,"humanoidFit");HumanoidClick(host,"humanoidApply");Assert(editor.AuthoringDocument.Editable && editor.AuthoringDocument.TargetId==target,"Apply opens independent editable target");
                string applied=editor.AuthoringDocument.InstanceId;var motion=editor.SelectedCHR0;var finger=motion.Children.OfType<CHR0EntryNode>().First(n=>n.Name.Contains("Index"));float previous=finger.GetFrameValue(3,15);finger.SetKeyframe(3,15,previous+2);WsClick(host,"workspaceApply");Assert(p.State.Instances.Single(i=>i.Id==applied).BakeEvidence.StartsWith("Native edits"),"native edit invalidates historical fit quality certificate");WsClick(host,"workspaceUndo");WsClick(host,"workspaceRedo");
                WsClick(host,"workspaceSave");var fresh=CharacterProject.Load(p.FilePath,p.StorageRoot);Assert(WorkspaceStateXml(fresh).SequenceEqual(WorkspaceStateXml(p)),"previewer saves exact profiles, constraints and native edits");
                host.Size=new Size(1000,720);Pump(80);CaptureWindow(host,"humanoid-small-window.png");Assert(editor.ModelPanel.Width>100 && editor.ModelPanel.Height>100,"viewport remains visible at small window");
                bar.Items.Cast<ToolStripItem>().Single(n=>n.Name=="previewWorkspaceToggle").PerformClick();bar.Items.Cast<ToolStripItem>().Single(n=>n.Name=="previewWorkspaceToggle").PerformClick();Assert(Ws<HumanoidWorkspacePanel>(host,"humanoidWorkspace").Trial!=null,"hide/reopen keeps same live humanoid workspace");
                Log("PASS: actual Model Previewer profile/landmark controls, Fit, native FK/corrected rendering/playback, contact/viewport handles, grouped undo, independent Apply, native edit/save/reload, small-window and hide/reopen (handler automation).");
            }
            catch(Exception error){failure=error;}finally{main.Dispose();Application.ExitThread();}
        }));Application.Run(main);if(failure!=null)throw new IOException("Humanoid previewer GUI",failure);
    }
    private static void HumanoidClick(Control host,string name)
    {WsInvoke(host,name);Assert(!Ws<Label>(host,"humanoidStatus").Text.StartsWith("Stopped:"),name+": "+Ws<Label>(host,"humanoidStatus").Text);}
}
