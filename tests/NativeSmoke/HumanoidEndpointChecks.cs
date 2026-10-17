// Original analytic endpoint/contact policy regressions, FighterPort 2026-10-17.
// Repository LGPL-3.0 terms. No game inputs, models or captures are embedded.
using BrawlCrate.Authoring;
using BrawlCrate.Authoring.Humanoid;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

internal static partial class NativeSmoke
{
    private static void CheckHumanoidEndpointPolicy(CharacterProject project,string clip,string native,bool targetStout)
    {
        var edge=new ContactInterval{Chain="LeftLeg",Start=0,End=30,Margin=4};
        var serializer=new XmlSerializer(typeof(ContactInterval));byte[] serialized;
        using(var stream=new MemoryStream()){serializer.Serialize(stream,edge);serialized=stream.ToArray();}
        var legacyXml=new XmlDocument();using(var stream=new MemoryStream(serialized,false))legacyXml.Load(stream);
        Assert(legacyXml.SelectSingleNode("/ContactInterval/HoldStart")==null && legacyXml.SelectSingleNode("/ContactInterval/HoldEnd")==null && legacyXml.SelectSingleNode("/ContactInterval/PolePolicy")==null && legacyXml.SelectSingleNode("/ContactInterval/ReachPolicy")==null,"legacy defaults omit new XML fields, preserving existing source/project metadata identities");
        ContactInterval legacy;using(var stream=new MemoryStream(serialized,false))legacy=(ContactInterval)serializer.Deserialize(stream);
        Assert(!legacy.HoldStart && !legacy.HoldEnd && legacy.PolePolicy=="World" && legacy.ReachPolicy=="Nearest" && legacy.Weight(0)==0 && legacy.Weight(30)==0,"old interval XML retains fade-at-both-edges and old pole/reach policies");
        Assert(Math.Abs(edge.Weight(2)-.5)<1e-12 && Math.Abs(edge.Weight(28)-.5)<1e-12,"independent smoothstep half-margin weights remain .5");
        edge.HoldStart=true;
        Assert(edge.Weight(0)==1 && edge.Weight(.125)==1 && edge.Weight(30)==0 && Math.Abs(edge.Weight(28)-.5)<1e-12,"clip-start hold does not silently remove the exit ramp");
        edge.HoldStart=false;edge.HoldEnd=true;
        Assert(edge.Weight(0)==0 && edge.Weight(30)==1 && edge.Weight(29.875)==1 && Math.Abs(edge.Weight(2)-.5)<1e-12,"clip-end hold does not silently remove the entry ramp");
        edge.HoldStart=true;
        foreach(double time in new[]{0d,.137,2,4,15,26,29.619,30})Assert(edge.Weight(time)==1,"both explicit standing endpoints keep the entire contact plateau");
        Assert(edge.Weight(-.001)==0 && edge.Weight(30.001)==0,"held endpoints never extend the saved contact interval");
        var invalid=project.CreateHumanoidRecipe(clip,native);invalid.Contacts.Add(new ContactInterval{Chain="LeftLeg",Start=1,End=29,Margin=4,HoldStart=true});
        var recipeSerializer=new XmlSerializer(typeof(HumanoidRecipe));
        using(var stream=new MemoryStream())
        {
            recipeSerializer.Serialize(stream,invalid);stream.Position=0;var recipeXml=new XmlDocument();recipeXml.Load(stream);
            Assert(recipeXml.SelectSingleNode("/HumanoidRecipe/ReachBlend")==null,"zero reach blend omits its optional XML field for legacy recipe identities");
            stream.Position=0;Assert(((HumanoidRecipe)recipeSerializer.Deserialize(stream)).ReachBlend==0,"old recipe XML restores the exact zero reach-blend default");
        }
        Expect<IOException>(()=>invalid.Validate(31),"held interior start would create an instantaneous entry and rejects");invalid.Contacts[0].HoldStart=false;invalid.Contacts[0].HoldEnd=true;
        Expect<IOException>(()=>invalid.Validate(31),"held interior end would create an instantaneous exit and rejects");

        var rig=project.HumanoidRigFor(project.State.Animations.Single(a=>a.Id==native).SourceCharacterId);var setup=project.HumanoidSetupFor(project.State.Animations.Single(a=>a.Id==native).SourceCharacterId);var rest=rig.World(rig.Rest());
        var faded=new HumanoidRecipe();
        foreach(string side in new[]{"Left","Right"})
        {
            int end=setup.Profile.Chain(side+"Leg").Bones.Last();var sole=rest[end].Point(setup.Body.Landmark(side+"Leg").Local);
            faded.Contacts.Add(new ContactInterval{Chain=side+"Leg",Start=0,End=30,Margin=4,Goal=sole+new HVec(0,-.25,0),ReachPolicy="RestHemisphere"});
        }
        var firstFaded=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,rig.Rest(),faded,0);
        var fadedWorld=rig.World(firstFaded.Local);
        foreach(var contact in faded.Contacts)
        {
            int end=setup.Profile.Chain(contact.Chain).Bones.Last();
            Assert(Math.Abs((fadedWorld[end].Point(setup.Body.Landmark(contact.Chain).Local)-contact.Goal).Length-.25)<1e-8,"independent original goal proves fading loses .25 units at the first frame");
        }
        foreach(var contact in faded.Contacts)contact.HoldStart=contact.HoldEnd=true;
        foreach(double time in new[]{0d,.137,2,15,29.619,30})
        {
            var corrected=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,rig.Rest(),faded,time);var world=rig.World(corrected.Local);
            Assert(Math.Abs(corrected.PelvisOffset+.25)<1e-7 && corrected.Diagnostics.Count==0,"explicit held endpoints preserve the independently required -.25 pelvis displacement");
            foreach(var contact in faded.Contacts){int end=setup.Profile.Chain(contact.Chain).Bones.Last();Assert((world[end].Point(setup.Body.Landmark(contact.Chain).Local)-contact.Goal).Length<1e-7,"original analytic first/last/withheld contact positions match the actual saved goal");}
        }
        var aerial=HumanoidSolver.Correct(rig,setup.Profile,setup.Body,rig.Rest(),new HumanoidRecipe(),0);
        Assert(aerial.Local.Zip(rig.Rest(),(a,b)=>(a.P-b.P).Length==0 && a.R.V.SequenceEqual(b.R.V)).All(v=>v),"no contacts means no default floor lock or changed pose");

        // New original native output: the desired standing sole is .25 units below
        // the authored ground, independently specified rather than sampled from IK.
        var held=PlantedRecipe(project,clip,native,targetStout);
        held.ReachBlend=.05;
        foreach(var contact in held.Contacts){contact.Goal+=new HVec(0,-.25,0);contact.Margin=4;contact.HoldStart=contact.HoldEnd=true;contact.ReachPolicy="RestHemisphere";}
        var before=WorkspaceStateXml(project);project.SaveHumanoidRecipe(held);var after=WorkspaceStateXml(project);
        project.Undo();Assert(before.SequenceEqual(WorkspaceStateXml(project)),"contact endpoint flags save in one project undo command");project.Redo();Assert(after.SequenceEqual(WorkspaceStateXml(project)),"contact endpoint policy redo restores exact native dependencies and metadata");
        string saved=Path.Combine(_output,"humanoid-contact-endpoints.fpcharacter");project.Save(saved);var fresh=CharacterProject.Load(saved,project.StorageRoot);var reloaded=fresh.State.HumanoidRecipes.Single(r=>r.Id==held.Id);
        Assert(reloaded.ReachBlend==.05 && reloaded.Contacts.All(c=>c.HoldStart&&c.HoldEnd&&c.Margin==4&&c.ReachPolicy=="RestHemisphere"),"endpoint, continuous reach blend and reach policy survive real project save/reload");
        string output=Path.Combine(_output,"humanoid-contact-endpoints.brres");using(var trial=fresh.PreviewHumanoid(reloaded)){Assert(trial.Report.UnreachableSamples==0,"held standing output has no unreachable samples");fresh.ExportHumanoidTrial(trial,output);}
        VerifyHumanoidEndpointFeet(output,targetStout,-.25);
        Log("PASS: backward-compatible endpoint policy, independent original fade mismatch versus held sole goals, exact undo/redo/project reload, new native save/fresh reopen and withheld endpoint positions; no automatic aerial contacts.");
    }
    private static void VerifyHumanoidEndpointFeet(string file,bool stout,double ground)
    {
        using(var root=NodeFactory.FromFile(null,file))
        {
            var model=Find<MDL0Node>(root);model.Populate();var motion=root.Children.SelectMany(g=>g.Children).OfType<CHR0Node>().Single(n=>n.Name=="OriginalPlanted");
            Assert(motion.FrameCount==31 && !motion.Loop,"contact hold keeps the exact original duration and loop flag");
            foreach(float time in new[]{0f,.137f,.619f,4.137f,15.619f,29.137f,29.619f,30f})
            {
                model.ApplyCHR(motion,time+1);
                foreach(string side in new[]{"Left","Right"})
                {
                    var foot=model.BoneCache.OfType<MDL0BoneNode>().Single(n=>n.Name==side+"Foot");var point=foot.Matrix*new BrawlLib.Internal.Vector3(0,-.5f,.4f);var expected=new HVec((side=="Left"?1:-1)*(stout?1.5:1),ground,.4);
                    Assert((new HVec(point._x,point._y,point._z)-expected).Length<.005,"freshly reopened native first/last/withheld sole meets independent <=.005 unit goal");
                }
            }
        }
    }
    private static void CheckHumanoidEndpointControls(Control host,CharacterProject project,string clip,string native)
    {
        var original=WorkspaceStateXml(project);var recipe=project.State.HumanoidRecipes.Last(r=>r.AnimationId==clip && r.TargetAnimationId==native);recipe.Id=Guid.NewGuid().ToString("N");recipe.ReachBlend=0;
        var authored=recipe.Contacts.First(c=>c.Chain=="LeftLeg");authored.Margin=4;authored.HoldStart=authored.HoldEnd=false;authored.RestOrientation=false;authored.PolePolicy="World";authored.UsePoleOverride=true;authored.Pole=new HVec(1,2,3);authored.Evidence="Original custom contact evidence retained during endpoint editing.";project.SaveHumanoidRecipe(recipe);
        var list=Ws<ListBox>(host,"humanoidContacts");list.SelectedIndex=Enumerable.Range(0,list.Items.Count).First(i=>list.Items[i].ToString().StartsWith("LeftLeg "));
        Assert(!Ws<CheckBox>(host,"humanoidContactHoldStart").Checked && Ws<ComboBox>(host,"humanoidPolePolicy").SelectedItem.ToString()=="World","actual contact selection restores legacy edge and pole policy");
        Ws<NumericUpDown>(host,"humanoidContactStart").Value=2;Ws<NumericUpDown>(host,"humanoidContactEnd").Value=28;
        HumanoidClick(host,"humanoidFullClipContact");
        var saved=project.State.HumanoidRecipes.Last(r=>r.AnimationId==clip && r.TargetAnimationId==native).Contacts.Single(c=>c.Chain=="LeftLeg");
        Assert(project.State.HumanoidRecipes.Last(r=>r.Id==recipe.Id).ReachBlend==.05 && Ws<NumericUpDown>(host,"humanoidReachBlend").Value==.05m && saved.Start==0 && saved.End==30 && saved.HoldStart && saved.HoldEnd && !saved.RestOrientation && saved.UsePoleOverride && saved.PolePolicy=="World" && saved.Pole.X==1 && saved.Pole.Y==2 && saved.Pole.Z==3 && saved.Evidence==authored.Evidence,"real full-clip action restores 0..last, holds both endpoints, saves explicit .05 reach blend and preserves custom orientation, override, pole values and evidence");
        var beforeDuplicate=WorkspaceStateXml(project);HumanoidClick(host,"humanoidNewContact");WsInvoke(host,"humanoidSaveContact");
        Assert(Ws<Label>(host,"humanoidStatus").Text.StartsWith("Stopped:") && Ws<Label>(host,"humanoidStatus").Text.Contains("already uses") && beforeDuplicate.SequenceEqual(WorkspaceStateXml(project)),"explicit New rejects a duplicate without overwriting the saved custom interval");
        list.SelectedIndex=Enumerable.Range(0,list.Items.Count).First(i=>list.Items[i].ToString().StartsWith("LeftLeg "));
        var beforeBlend=WorkspaceStateXml(project);Ws<NumericUpDown>(host,"humanoidReachBlend").Value=.025m;HumanoidClick(host,"humanoidSaveReachBlend");
        Assert(project.State.HumanoidRecipes.Last(r=>r.Id==recipe.Id).ReachBlend==.025,"actual Advanced reach-blend action persists the deliberate value");WsClick(host,"workspaceUndo");Assert(beforeBlend.SequenceEqual(WorkspaceStateXml(project)),"Advanced reach blend saves as one undoable recipe edit");
        WsClick(host,"workspaceUndo");Assert(project.State.HumanoidRecipes.Last(r=>r.Id==recipe.Id).Contacts.First(c=>c.Chain=="LeftLeg").HoldStart==false,"actual project Undo restores contact endpoint policy");WsClick(host,"workspaceRedo");
        Ws<ComboBox>(host,"humanoidPolePolicy").SelectedItem="TransportedRest";WsInvoke(host,"humanoidSaveContact");
        Assert(Ws<Label>(host,"humanoidStatus").Text.StartsWith("Stopped:") && Ws<Label>(host,"humanoidStatus").Text.Contains("pole override"),"changing saved override coordinate policy requires deliberate reset");
        Ws<ComboBox>(host,"humanoidPolePolicy").SelectedItem="World";
        HumanoidClick(host,"humanoidClearPoleOverride");Assert(!project.State.HumanoidRecipes.Last(r=>r.Id==recipe.Id).Contacts.First(c=>c.Chain=="LeftLeg").UsePoleOverride,"actual reset clears only the selected saved pole override");
        WsClick(host,"workspaceUndo");WsClick(host,"workspaceUndo");WsClick(host,"workspaceUndo");
        Assert(original.SequenceEqual(WorkspaceStateXml(project)),"endpoint control checks restore untouched original project/native state");
        Log("PASS: actual previewer contact list, endpoint flags, legacy pole restore, custom-data preservation, explicit pole reset and contextual undo/redo.");
    }
}
