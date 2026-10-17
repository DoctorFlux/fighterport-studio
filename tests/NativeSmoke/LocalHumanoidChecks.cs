// FighterPort opt-in native pair checks, 2026-10-17. No commercial paths/assets in source.
using BrawlCrate.Authoring;
using BrawlCrate.Sources;
using BrawlCrate.UI;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

internal static partial class NativeSmoke
{
    private static int LocalHumanoid(string configuration,string evidence)
    {
        try
        {
            if(Directory.Exists(evidence))throw new IOException("Use a new private evidence directory.");Directory.CreateDirectory(evidence);_output=Path.GetFullPath(evidence);
            var config=new XmlDocument{XmlResolver=null};using(var reader=XmlReader.Create(configuration,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=65536}))config.Load(reader);
            var inputs=config.SelectNodes("/localHumanoid/character").Cast<XmlElement>().ToArray();Assert(inputs.Length==2,"explicit local pair");
            Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT",Path.Combine(_output,"settings"));
            string storage=Path.Combine(_output,"storage");BrawlLib.Internal.IO.SourceWriteGuard.Initialize(Path.Combine(storage,"source-roots.txt"));var project=new CharacterProject(storage);project.MigrateToSharedLibrary(Path.Combine(_output,"library"));
            var hashes=new Dictionary<string,string>();
            foreach(var input in inputs)
            {
                string model=input.GetAttribute("model"),motion=input.GetAttribute("motion"),container=input.GetAttribute("motionContainer");hashes[model]=Hash(model);hashes[motion]=Hash(motion);
                var workspace=new CompanionWorkspace(storage);workspace.AddSource(model,CancellationToken.None);workspace.AddSelectedContainer(motion,container);
                workspace.SelectModel(workspace.Resources.Single(r=>r.Kind=="MDL0" && r.SourcePath==Path.GetFullPath(model) && r.TreePath==input.GetAttribute("modelPath")));
                foreach(string binding in workspace.RequiredTextures)workspace.SelectTexture(binding,workspace.TextureCandidates(binding).Single());
                foreach(string binding in workspace.RequiredPalettes)workspace.SelectPalette(binding,workspace.PaletteCandidates(binding).Single());
                var clip=workspace.Resources.Single(r=>r.Kind=="CHR0" && r.SourcePath==Path.GetFullPath(motion) && r.Name==input.GetAttribute("clip"));workspace.SelectMotion(clip);
                Assert(workspace.Validate().Count==0,string.Join("; ",workspace.Validate()));string resolution=Path.Combine(_output,input.GetAttribute("id")+".fpcompanion");workspace.Save(resolution);
                string id=project.RegisterSource(resolution,input.GetAttribute("id"));var character=project.State.Characters.Single(c=>c.Id==id);var animation=project.State.Animations.Single(a=>a.SourceCharacterId==id);
                string instance=project.Assign(animation.Id,id,"Own-rig source playback");project.ExportInstance(instance,Path.Combine(_output,input.GetAttribute("id")+"-own.pac"));
                using(var root=NodeFactory.FromFile(null,Path.Combine(_output,input.GetAttribute("id")+"-own.pac")))
                {
                    var nativeModel=(MDL0Node)LocalNativeAt(root,character.ModelPath);nativeModel.Populate();var nativeMotion=(CHR0Node)LocalNativeAt(root,animation.NativePath);
                    var report=new List<string>();
                    foreach(float time in new[]{0f,(animation.Frames-1)*.5f,animation.Frames-1f})
                    {
                        nativeModel.ApplyCHR(nativeMotion,time+1);foreach(var bone in nativeModel.BoneCache.OfType<MDL0BoneNode>())report.Add(time+"\t"+bone.Name+"\t"+bone.FrameState+"\t"+bone.Matrix[12]+","+bone.Matrix[13]+","+bone.Matrix[14]);
                        foreach(var vertex in nativeModel._objList.OfType<MDL0ObjectNode>().SelectMany(m=>m.Vertices))Assert(!float.IsNaN(vertex.WeightedPosition._x) && !float.IsNaN(vertex.WeightedPosition._y) && !float.IsNaN(vertex.WeightedPosition._z),"finite native source geometry");
                    }
                    File.WriteAllLines(Path.Combine(_output,input.GetAttribute("id")+"-native-poses.tsv"),report);
                }
                // Register once, add another explicitly selected source motion without a second model profile.
                var extra=workspace.Resources.Single(r=>r.Kind=="CHR0" && r.SourceContainer==container && r.Name==input.GetAttribute("extraClip"));project.AddAnimationFromContainer(id,motion,container,extra.TreePath);
                Console.WriteLine("PASS: selected local character "+input.GetAttribute("id")+" own-rig native assembly/decode and multiple source-bound clips; no semantic review/retarget claim.");
            }
            string saved=Path.Combine(_output,"local-pair.fpcharacter");project.Save(saved);var loaded=CharacterProject.Load(saved,storage);Assert(loaded.State.Characters.Count==2 && loaded.State.Animations.Count==4,"private pair native/project fresh reload");
            foreach(var character in project.State.Characters)project.DetectHumanoid(character.Id);
            Assert(project.State.Humanoids.All(p=>!p.Reviewed) && project.State.Bodies.All(b=>!b.Reviewed),"commercial suggestions are not agent-approved profiles");
            if(config.DocumentElement.GetAttribute("trial")=="true")
            {
                foreach(var source in project.State.Characters)
                {
                    var target=project.State.Characters.Single(c=>c.Id!=source.Id);var a=project.State.Animations.Single(n=>n.SourceCharacterId==source.Id&&n.Name==inputs.Single(i=>i.GetAttribute("id")==source.Name).GetAttribute("clip"));var b=project.State.Animations.Single(n=>n.SourceCharacterId==target.Id&&n.Name==inputs.Single(i=>i.GetAttribute("id")==target.Name).GetAttribute("clip"));
                    var recipe=project.CreateHumanoidRecipe(a.Id,b.Id);var trial=project.PreviewHumanoid(recipe);File.WriteAllText(Path.Combine(_output,source.Name+"-to-"+target.Name+"-fk.txt"),trial.Report.ToString());
                    Console.WriteLine("PASS: unreviewed diagnostic native FK trial "+source.Name+" -> "+target.Name+"; "+trial.Report);
                    trial.Dispose();
                    if(config.DocumentElement.GetAttribute("contacts")=="true" && (config.DocumentElement.GetAttribute("contactSource")=="" || config.DocumentElement.GetAttribute("contactSource")==source.Name))
                    {
                        var setup=project.HumanoidSetupFor(target.Id);using(var root=NodeFactory.FromFile(null,project.PreviewSource(b.Id)))
                        {var model=(MDL0Node)LocalNativeAt(root,target.ModelPath);model.Populate();model.ApplyCHR((CHR0Node)LocalNativeAt(root,b.NativePath),1);foreach(string side in new[]{"Left","Right"}){int bone=setup.Profile.Chain(side+"Leg").Bones.Last();var offset=setup.Body.Landmark(side+"Leg").Local;var point=model.BoneCache[bone].Matrix*new BrawlLib.Internal.Vector3((float)offset.X,(float)offset.Y,(float)offset.Z);recipe.Contacts.Add(new BrawlCrate.Authoring.Humanoid.ContactInterval{Chain=side+"Leg",Start=0,End=a.Frames-1,Margin=4,Goal=new BrawlCrate.Authoring.Humanoid.HVec(point._x,setup.Body.GroundY,point._z),Evidence="Opt-in diagnostic standing interval; target native reference start X/Z and measured rest sole ground Y. Unreviewed characterization, no game/motion contact certification."});}}
                        project.SaveHumanoidRecipe(recipe);var corrected=project.PreviewHumanoid(recipe);File.WriteAllText(Path.Combine(_output,source.Name+"-to-"+target.Name+"-contacts.txt"),corrected.Report.ToString());project.ExportHumanoidTrial(corrected,Path.Combine(_output,source.Name+"-to-"+target.Name+"-corrected.pac"));
                        Assert(corrected.Report.CorrectedContactError<corrected.Report.FkContactError && corrected.Report.CorrectedContactError<setup.Body.LegLength*.005,"diagnostic standing contacts improve measured FK error on freshly decoded target");Console.WriteLine("PASS: unreviewed diagnostic native contact trial "+source.Name+" -> "+target.Name+"; "+corrected.Report);corrected.Dispose();
                    }
                }
            }
            project.Save(saved);
            if(config.DocumentElement.GetAttribute("gui")=="true")LocalOwnRigGui(PreviewAuthoringSession.For(project));
            foreach(var hash in hashes)Assert(Hash(hash.Key)==hash.Value,"commercial source unchanged");File.WriteAllLines(Path.Combine(_output,"sources.sha256"),hashes.Select(h=>h.Value+"\t"+h.Key));
            Console.WriteLine("PASS: private own-rig native sources/reload, unchanged inputs; GUI evidence only when explicitly configured. No human review, retarget/contact/game certification.");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);File.WriteAllText(Path.Combine(_output,"failure.txt"),error.ToString());return 1;}
    }
    private static ResourceNode LocalNativeAt(ResourceNode root,string path)
    {foreach(string part in path.Split(new[]{'/'},StringSplitOptions.RemoveEmptyEntries))root=root.Children[int.Parse(part)];return root;}
    private static void LocalOwnRigGui(PreviewAuthoringSession session)
    {
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(NativeProgram.TypeHandle);var main=MainForm.Instance;Exception failure=null;
        main.Shown+=(s,e)=>main.BeginInvoke(new Action(()=>{try{
            foreach(var character in session.Project.State.Characters)
            {
                string instance=session.Project.State.Instances.Single(i=>i.TargetCharacterId==character.Id).Id;var editor=session.ShowInstance(instance);Pump(150);var animation=session.Project.NativeAnimation(instance);
                editor.SetFrame(1);CaptureViewport(editor,"local-"+character.Name+"-source-start.png");editor.SetFrame(Math.Max(1,animation.Frames/2));CaptureViewport(editor,"local-"+character.Name+"-source-middle.png");
                int before=editor.CurrentFrame;editor.PlaybackPanel.btnPlay.PerformClick();Pump(200);editor.PlaybackPanel.btnPlay.PerformClick();Assert(editor.CurrentFrame!=before,"own-rig actual native playback advanced");
            }
            foreach(var recipe in session.Project.State.HumanoidRecipes)
            {
                string instance=session.Project.State.Instances.Single(i=>i.TargetCharacterId==recipe.TargetCharacterId).Id;var editor=session.ShowInstance(instance);var host=editor.FindForm();var pane=Ws<PreviewWorkspacePane>(host,"previewWorkspace");if(!pane.Visible)Ws<ToolStrip>(host,"previewWorkspaceBar").Items.Cast<ToolStripItem>().Single(n=>n.Name=="previewWorkspaceToggle").PerformClick();WsChoose(Ws<ComboBox>(host,"workspaceTarget"),recipe.TargetCharacterId);WsChoose(Ws<ComboBox>(host,"workspaceClip"),recipe.AnimationId);WsChoose(Ws<ComboBox>(host,"workspaceDestination"),recipe.TargetAnimationId);HumanoidClick(host,"humanoidFit");Assert(editor.AuthoringReadOnly && editor.AuthoringDocument.Mode.StartsWith("Unreviewed"),"commercial comparisons visibly remain unreviewed and read-only");editor.SetFrame(15);CaptureViewport(editor,"local-"+recipe.Id+"-corrected.png");HumanoidClick(host,"humanoidPreviewFk");editor.SetFrame(15);CaptureViewport(editor,"local-"+recipe.Id+"-fk.png");HumanoidClick(host,"humanoidPreviewCorrected");int prior=editor.CurrentFrame;editor.PlaybackPanel.btnPlay.PerformClick();Pump(200);editor.PlaybackPanel.btnPlay.PerformClick();Assert(editor.CurrentFrame!=prior,"actual unreviewed corrected native playback advances");
            }
        }catch(Exception error){failure=error;}finally{main.Dispose();Application.ExitThread();}}));Application.Run(main);if(failure!=null)throw new IOException("Local own-rig native GUI",failure);
    }
}
