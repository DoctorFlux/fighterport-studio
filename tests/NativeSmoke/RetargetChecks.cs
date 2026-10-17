// Original FK fixtures/analytic expectations, FighterPort 2026-10-17. No game data.
// Same original-artwork permission as OriginalFixture.cs.
using BrawlCrate.Authoring;
using BrawlCrate.UI;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlLib.Internal;
using BrawlLib.Modeling;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

internal static partial class NativeSmoke
{
    private static string FkFixture(bool target)
    {
        string original=OriginalFixture.Create(Path.Combine(_output,target?"build-b":"build-a"),target,target);
        string folder=Path.Combine(_output,target?"fk-b":"fk-a");Directory.CreateDirectory(folder);string result=Path.Combine(folder,"original.brres");
        using(var root=NodeFactory.FromFile(null,original))
        {
            var model=Find<MDL0Node>(root);model.Populate();var bone=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name=="Paddle");
            if(target)
            {
                var parent=(MDL0BoneNode)bone.Parent;bone.Remove();
                var helper=new MDL0BoneNode {Name="AxisBridge",BindState=new FrameState(new Vector3(1),new Vector3(0,0,90),new Vector3(0,4,0))};parent.AddChild(helper);helper.AddChild(bone);
                bone.BindState=new FrameState(new Vector3(1),new Vector3(),new Vector3(0,12,0));
                parent.BindState=new FrameState(new Vector3(1),new Vector3(),new Vector3(5,0,0));parent.RecalcBindState(false,false);model._linker.RegenerateBoneCache(true);
            }
            var motion=Find<CHR0Node>(root);var entry=motion.Children.OfType<CHR0EntryNode>().Single();
            entry.SetKeyframe(5,0,0,true)._tangent=12;entry.SetKeyframe(5,15,180,true)._tangent=12;entry.SetKeyframe(5,30,360,true)._tangent=12;
            root.Export(result);
        }
        return result;
    }
    private static ReviewedRigMap OriginalMap(SourceCharacterProfile source,SourceCharacterProfile target)
    {
        var map=new ReviewedRigMap {SourceCharacterId=source.Id,TargetCharacterId=target.Id,SourceFingerprint=source.RigFingerprint,TargetFingerprint=target.RigFingerprint,
            SourceInterpretation=source.Interpretation,TargetInterpretation=target.Interpretation,ReviewNote="Original fixture author's explicit root/effector characterization; extra helper bones preserve local rest. Not commercial rig approval.",RootPolicy="InPlace"};
        foreach(string name in new[]{"Root","Paddle"})map.Pairs.Add(new RigRolePair {Role=name=="Root"?"root":"original-effector",Source=source.Bones.FindIndex(b=>b.Name==name),Target=target.Bones.FindIndex(b=>b.Name==name)});
        map.IgnoredSource.AddRange(Enumerable.Range(0,source.Bones.Count).Where(i=>!map.Pairs.Any(p=>p.Source==i)));
        map.TargetRestOnly.AddRange(Enumerable.Range(0,target.Bones.Count).Where(i=>!map.Pairs.Any(p=>p.Target==i)));return map;
    }
    private static void ExerciseRetarget(bool gui)
    {
        string a=FkFixture(false),b=FkFixture(true);var originals=new[]{a,b}.ToDictionary(p=>p,Hash);
        var project=new CharacterProject(Path.Combine(_output,"storage"));string aid=project.RegisterSource(LibraryResolution(a,"fk-a"),"Rig A"),bid=project.RegisterSource(LibraryResolution(b,"fk-b"),"Rig B");
        var source=project.State.Characters.Single(c=>c.Id==aid);var target=project.State.Characters.Single(c=>c.Id==bid);var aa=project.State.Animations.Single(c=>c.SourceCharacterId==aid);var ab=project.State.Animations.Single(c=>c.SourceCharacterId==bid);
        Assert(source.Bones.Count==2 && target.Bones.Count==4,"different bone counts, parents, local/rest axes and proportions");
        var proposed=OriginalMap(source,target);
        Expect<IOException>(()=>RigTransfer.Validate(proposed,source,target),"unreviewed maps reject");
        Expect<IOException>(()=>project.Assign(aa.Id,bid,"unsafe"),"old mismatch rejection remains");
        string map=project.ReviewRigMap(proposed);string second=project.Assign(ab.Id,bid,"UnchangedB");string hashBefore=project.State.Instances.Single(i=>i.Id==second).WorkingHash;
        string instance=project.AssignRetargeted(aa.Id,ab.Id,map,"TransferredAtoB");
        string output=Path.Combine(_output,"fk-output.brres");project.ExportInstance(instance,output);VerifyFkAnalytic(output,true);
        project.Undo();Assert(project.State.Instances.Count==1,"transfer is one undoable command");project.Redo();Assert(project.State.Instances.Count==2,"transfer redo");
        Assert(project.State.Instances.Single(i=>i.Id==second).WorkingHash==hashBefore,"independent instance unchanged");
        string saved=Path.Combine(_output,"fk.fpcharacter");project.Save(saved);var loaded=CharacterProject.Load(saved,Path.Combine(_output,"storage"));
        loaded.ExportInstance(instance,Path.Combine(_output,"fk-reopened.brres"));VerifyFkAnalytic(Path.Combine(_output,"fk-reopened.brres"),true);
        // Reverse has its own reviewed policy and its own decoded native proof.
        var reverse=OriginalMap(target,source);string reverseMap=project.ReviewRigMap(reverse);string rev=project.AssignRetargeted(ab.Id,aa.Id,reverseMap,"TransferredBtoA");
        project.ExportInstance(rev,Path.Combine(_output,"fk-reverse.brres"));VerifyFkAnalytic(Path.Combine(_output,"fk-reverse.brres"),false);
        var stale=OriginalMap(source,target);stale.SourceFingerprint="changed";Expect<IOException>(()=>project.ReviewRigMap(stale),"changed fingerprint invalidates review");
        var unsupported=OriginalMap(source,target);unsupported.TargetRestOnly.Clear();Expect<IOException>(()=>project.ReviewRigMap(unsupported),"missing helper policy rejects");
        var scaled=project.State.Characters.Single(c=>c.Id==aid);scaled.Bones[0].Rest="2,1,1,0,0,0,0,0,0";
        Expect<IOException>(()=>RigTransfer.Validate(proposed,scaled,target,false),"non-unit scale rejects");
        var reviewed=project.State.RigMaps.Single(m=>m.Id==map);var pose=source.Bones.Select(RigPose.Rest).ToArray();pose[0].X+=3;
        Assert(Math.Abs(RigTransfer.Transfer(reviewed,source,target,pose)[0].X-5)<.00001,"explicit in-place root policy");
        var translated=OriginalMap(source,target);translated.RootPolicy="ScaledNativeTranslation";translated.NativeUnitScale=2;
        string translationMap=project.ReviewRigMap(translated);var translationReview=project.State.RigMaps.Single(m=>m.Id==translationMap);
        Assert(Math.Abs(RigTransfer.Transfer(translationReview,source,target,pose)[0].X-11)<.00001,"explicit calibrated native root translation");
        translationReview.NativeUnitScale=3;Expect<IOException>(()=>RigTransfer.Validate(translationReview,source,target),"policy tampering invalidates review digest");
        pose[1].Y+=1;Expect<IOException>(()=>RigTransfer.Transfer(reviewed,source,target,pose),"animated non-root translation rejects");
        var reversePose=target.Bones.Select(RigPose.Rest).ToArray();reversePose[reverse.IgnoredSource.First()].RZ+=3;
        Expect<IOException>(()=>RigTransfer.Transfer(project.State.RigMaps.Single(m=>m.Id==reverseMap),target,source,reversePose),"animated ignored helper rejects");
        Expect<OperationCanceledException>(()=>project.AssignRetargeted(aa.Id,ab.Id,map,"cancel",new CancellationToken(true)),"cancelled native bake");
        string fastDirectory=Path.Combine(_output,"fast-original");Directory.CreateDirectory(fastDirectory);string fastFile=Path.Combine(fastDirectory,"fast.chr0");
        using(var root=NodeFactory.FromFile(null,a))
        {
            var motion=Find<CHR0Node>(root);var track=motion.Children.OfType<CHR0EntryNode>().Single();
            foreach(int frame in new[]{0,15,30})track.SetKeyframe(5,frame,frame*480,true)._tangent=480;motion.Export(fastFile);
        }
        originals.Add(fastFile,Hash(fastFile));string fastClip=project.AddAnimation(aid,fastFile);int beforeReject=project.State.Instances.Count;
        Expect<IOException>(()=>project.AssignRetargeted(fastClip,ab.Id,map,"AmbiguousFastTurn"),"fast winding must reject instead of silently losing turns");Assert(project.State.Instances.Count==beforeReject,"failed bake is atomic");
        if(gui) FkGui(loaded,instance,source.Id,target.Id);
        foreach(var original in originals)Assert(Hash(original.Key)==original.Value,"original FK source unchanged");
        File.WriteAllLines(Path.Combine(_output,"fk-originals.sha256"),originals.Select(p=>p.Value+"\t"+p.Key));
        Log("PASS: reviewed original A->B and independent B->A, native decode at integer/fractional times, full turn, loop endpoint, binds/blends, save/reopen/undo/redo, rejection and unchanged originals");
    }
    private static void VerifyFkAnalytic(string file,bool target)
    {
        using(var root=NodeFactory.FromFile(null,file))
        {
            var model=Find<MDL0Node>(root);model.Populate();var motion=Find<CHR0Node>(root);var paddle=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name=="Paddle");
            Assert(motion.FrameCount==31 && motion.Loop,"native timing/loop");
            var track=motion.Children.OfType<CHR0EntryNode>().Single(e=>e.Name=="Paddle");
            Assert(Math.Abs(track.GetFrameValue(5,30)-360)<.01,"native Euler full turn preserved rather than collapsed");
            foreach(float time in new[]{0f,.5f,3.25f,7.5f,15f,22.75f,29.5f,30f,30.25f,30.5f,30.75f,31f,31.25f,31.5f,31.75f,32f})
            {
                float phase=time==32?0:time;model.ApplyCHR(motion,phase+1);double degrees=phase*12;
                // Inherited looping CHR0 uses FrameCount+1: last key at 30 -> first at 32.
                // Independently evaluate that native Hermite seam, not just equal endpoints.
                if(phase>30){double u=(phase-30)/2d;degrees=(2*u*u*u-3*u*u+1)*360+(u*u*u-2*u*u+u)*24+(u*u*u-u*u)*24;}
                double angle=degrees*Math.PI/180,c=Math.Cos(angle),s=Math.Sin(angle);
                var global=paddle.Matrix;
                double offset=target?Math.PI/2:0;Assert(Math.Abs(global[0]-Math.Cos(angle+offset))<.001 && Math.Abs(global[1]-Math.Sin(angle+offset))<.001,"independent expected world bone orientation");
                foreach(var v in ((MDL0ObjectNode)model._objList.Single()).Vertices)
                {
                    double px=target?-7:0,py=target?4:0,w=target?(v.Position._y<1?.25:.75):1;
                    double x=px+c*(v.Position._x-px)-s*(v.Position._y-py),y=py+s*(v.Position._x-px)+c*(v.Position._y-py);
                    var actual=v.WeightedPosition;Assert(Math.Abs(actual._x-((1-w)*v.Position._x+w*x))<.005 && Math.Abs(actual._y-((1-w)*v.Position._y+w*y))<.005,"independent decoded LBS expected pose (native units)");
                }
            }
        }
    }
    private static void FkGui(CharacterProject project,string instance,string sourceId,string targetId)
    {
        var main=MainForm.Instance;Exception failure=null;main.Shown+=(s,e)=>main.BeginInvoke(new Action(()=> {
            try
            {
                using(var form=new CharacterLibraryForm(project))
                {
                    form.Show(main);var list=(ListView)form.Controls.Find("libraryInstances",true).Single();list.Items.Cast<ListViewItem>().Single(i=>(string)i.Tag==instance).Selected=true;
                    using(var review=new RigMapReviewForm(project,sourceId,targetId))
                    {
                        review.Show(main);((Button)review.Controls.Find("rigApprove",true).Single()).PerformClick();
                        Assert(review.ReviewedMapId==null,"empty GUI mapping cannot approve by names");
                        var proposal=OriginalMap(project.State.Characters.Single(c=>c.Id==sourceId),project.State.Characters.Single(c=>c.Id==targetId));
                        foreach(var pair in proposal.Pairs)
                        {
                            review.Pairs.Rows[pair.Source].Cells[1].Value=pair.Role;
                            review.Pairs.Rows[pair.Source].Cells[2].Value=((DataGridViewComboBoxColumn)review.Pairs.Columns[2]).Items[pair.Target+1];
                        }
                        foreach(int i in proposal.TargetRestOnly)review.RestOnly.SetItemChecked(i,true);review.ReviewNote.Text=proposal.ReviewNote;
                        CaptureWindow(review,"fk-reviewed-map.png");((Button)review.Controls.Find("rigApprove",true).Single()).PerformClick();Assert(review.ReviewedMapId!=null,"explicit GUI map approval");
                    }
                    form.RefreshProject();list.Items.Cast<ListViewItem>().Single(i=>(string)i.Tag==instance).Selected=true;
                    LibraryButton(form,"libraryEdit");var editor=ModelEditControl.Instances.Single();PrepareLibraryEditor(editor);editor.SetFrame(16);Pump(150);CaptureViewport(editor,"fk-native-preview.png");
                    float played=0;using(var timer=new System.Windows.Forms.Timer {Interval=250})
                    {timer.Tick+=(a,b)=>{played=editor.CurrentFrame;editor.StopAnim();timer.Stop();};timer.Start();editor.PlaybackPanel.btnPlay.PerformClick();WaitFor(()=>played!=0,"retarget native playback");}
                    Assert(played!=16,"retarget native playback advanced");
                    project.Save(Path.Combine(_output,"fk-gui-reviewed.fpcharacter"));
                    CaptureWindow(form,"fk-project.png");CallProgram("Close",Type.EmptyTypes);
                }
            }
            catch(Exception error){failure=error;}finally{main.Dispose();Application.ExitThread();}
        }));Application.Run(main);if(failure!=null)throw new InvalidOperationException("FK GUI",failure);
    }
}
