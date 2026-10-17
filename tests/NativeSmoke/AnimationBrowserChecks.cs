// Original FighterPort ordinary-animation workflow regression, 2026-10-17.
using BrawlCrate.UI;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlCrate.Authoring;
using BrawlCrate.Sources;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

internal static partial class NativeSmoke
{
    private static void ExerciseAnimationBrowser(bool gui)
    {
        string original=OriginalFixture.Create(Path.Combine(_output,"build"));
        string different=OriginalFixture.Create(Path.Combine(_output,"build-b"),true,true);
        string inputs=Path.Combine(_output,"inputs");Directory.CreateDirectory(inputs);
        string changedInput=Path.Combine(_output,"changed-input.pac");
        string model=Path.Combine(inputs,"model.pac"),motion=Path.Combine(inputs,"motion.pac");
        using(var bres=(BRRESNode)NodeFactory.FromFile(null,original))
        using(var motions=new BRRESNode{Name="SeparateMotions"})
        using(var a=new ARCNode{Name="Model"})
        using(var b=new ARCNode{Name="Motions"})
        {
            var clip=Find<CHR0Node>(bres);clip.Remove();motions.GetOrCreateFolder<CHR0Node>().AddChild(clip);
            var routingOnly=motions.CreateResource<SRT0Node>("NavigationOnlyUv");routingOnly.Version=4;routingOnly.FrameCount=17;routingOnly.Loop=false;
            ArcChild(a,bres,ARCFileType.ModelData,0,0);ArcChild(b,motions,ARCFileType.AnimationData,0,0);a.Export(model);b.Export(motion);
        }
        File.Copy(motion,changedInput);
        string multiple=Path.Combine(inputs,"multiple-models.pac");
        using(var multi=new ARCNode{Name="ExplicitModels"}){ArcChild(multi,(ARCEntryNode)NodeFactory.FromFile(null,original),ARCFileType.ModelData,0,0);ArcChild(multi,(ARCEntryNode)NodeFactory.FromFile(null,different),ARCFileType.ModelData,1,0);multi.Export(multiple);}
        var hashes=new[]{original,different,model,motion,multiple}.ToDictionary(p=>p,Hash);
        new SourceFolderSession(inputs,Path.Combine(_output,"storage"));
        var project=new CharacterProject(Path.Combine(_output,"storage"));
        string character=project.RegisterSource(LibraryResolution(original,"browser-original"),"Browser character");
        string differentCharacter=project.RegisterSource(LibraryResolution(different,"browser-different"),"Different target");
        project.MigrateToSharedLibrary(Path.Combine(_output,"shared-library"));
        string clipId=project.State.Animations.Single(a=>a.SourceCharacterId==character).Id,instance=project.Assign(clipId,character,"Original custom slot");
        project.Assign(project.State.Animations.Single(a=>a.SourceCharacterId==differentCharacter).Id,differentCharacter,"Untouched target");
        string projectPath=Path.Combine(_output,"browser.fpcharacter");project.Save(projectPath);
        var other=CharacterProject.Load(projectPath,Path.Combine(_output,"storage"));string otherPath=Path.Combine(_output,"other.fpcharacter");
        string extraDirectory=Path.Combine(_output,"extra-source");Directory.CreateDirectory(extraDirectory);string extra=Path.Combine(extraDirectory,"shared-extra.chr0");
        using(var native=NodeFactory.FromFile(null,original)){var clip=Find<CHR0Node>(native);clip.Name="AdditionalShared";clip.Export(extra);}
        other.AddAnimation(character,extra);other.Save(otherPath);string otherHash=Hash(otherPath);
        var sharedHashes=Directory.GetFiles(project.SharedLibrary.Root,"*",SearchOption.AllDirectories).ToDictionary(p=>p,Hash);
        var pin=project.SharedLibrary.Pin("animations",clipId);
        if(!gui){foreach(var pair in hashes)Assert(Hash(pair.Key)==pair.Value,"browser source unchanged");Log("PASS: original separate PAC and immutable project/shared fixtures generated");return;}
        Exception failure=null;var main=MainForm.Instance;
        main.Shown+=(s,e)=>main.BeginInvoke(new Action(()=>{
            try
            {
                Assert((bool)CallProgram("Open",new[]{typeof(string),typeof(bool)},model,false),"ordinary main browser model PAC open");
                var root=(ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null,null);
                main.TargetResource(Find<MDL0Node>(root));((BrawlCrate.NodeWrappers.MDL0Wrapper)main.resourceTree.SelectedNode).Preview();
                var editor=ModelEditControl.Instances.Single();editor.OpenFile(motion,false,true,false);Pump(150);
                Log("Observed ordinary list count="+editor.leftPanel.listAnims.Items.Count+" visible="+editor.leftPanel.listAnims.Visible);
                editor.FindForm().Size=new Size(1100,800);editor.AuthoringLayout(true);editor.SetAuthoringReadOnly(true);Pump(100);
                Log("Observed compact read-only list visible="+editor.leftPanel.listAnims.Visible+" enabled="+editor.leftPanel.listAnims.Enabled);
                var host=editor.FindForm();var browser=Ws<PreviewAnimationBrowser>(host,"previewAnimationBrowser");
                Assert(browser.Visible && Ws<ListBox>(host,"animationClips").Items.Count==2,"compact/read-only browser visible with native channel names");
                var list=Ws<ListBox>(host,"animationClips");Assert(list.Items.Cast<object>().Any(n=>n.ToString().Contains("OriginalSwing")),"displayed native clip name");
                Ws<TextBox>(host,"animationSearch").Text="no match";Assert(list.Items.Count==0,"search filters native list");Ws<TextBox>(host,"animationSearch").Text="Swing";
                editor.leftPanel.fileType.SelectedIndex=1;list.SelectedIndex=0;Ws<Button>(host,"animationPreview").PerformClick();Pump(100);
                Assert(editor.SelectedCHR0!=null && editor.SelectedCHR0.Name=="OriginalSwing","real browser Preview selects inherited native motion");
                BrowserMotion(editor,"ordinary");
                Ws<TextBox>(host,"animationSearch").Text="NavigationOnlyUv";Ws<Button>(host,"animationPreview").PerformClick();Assert(editor.TargetAnimation is SRT0Node && editor.TargetAnimation.Name=="NavigationOnlyUv","browser routes the selected native channel type, not a stale saved type");
                Expect<IOException>(()=>PreviewNativeExportForm.Export(editor,Path.Combine(_output,"wrong-channel.pac")),"skeletal export rejects another selected channel");
                Ws<TextBox>(host,"animationSearch").Text="Swing";Ws<Button>(host,"animationPreview").PerformClick();
                Assert(!root.IsDirty && !editor.SelectedCHR0.RootNode.IsDirty,"read-only browsing/scrub/play did not mutate inputs");
                editor.AuthoringLayout(false);Assert(editor.leftPanel.listAnims.Enabled,"native detailed animation navigation remains enabled read-only");editor.AuthoringLayout(true);
                var bar=Ws<ToolStrip>(host,"previewWorkspaceBar");var toggle=bar.Items.Cast<ToolStripItem>().Single(i=>i.Name=="previewAnimationsToggle");toggle.PerformClick();toggle.PerformClick();Assert(browser.Visible,"hide/reopen animation browser");
                editor.SetAuthoringReadOnly(false);editor.SetFrame(16);editor.SelectedBone=((MDL0Node)editor.TargetModel).BoneCache.OfType<MDL0BoneNode>().Single(bone=>bone.Name=="Paddle");
                editor.CHR0Editor.numRotZ.Value=35;editor.CHR0Editor.BoxChangedCreateUndo(editor.CHR0Editor.numRotZ,EventArgs.Empty);
                using(var export=(Form)Activator.CreateInstance(typeof(PreviewNativeExportForm),System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic,null,new object[]{editor},null))
                {
                    export.Show(host);Ws<TextBox>(export,"previewNativeOutput").Text=motion;Ws<Button>(export,"previewNativeExport").PerformClick();Assert(Ws<Label>(export,"previewNativeOutputStatus").Text.StartsWith("Stopped:"),"source overwrite blocked in real export control");
                    Ws<TextBox>(export,"previewNativeOutput").Text=Path.Combine(_output,"edited-motion.pac");Ws<Button>(export,"previewNativeExport").PerformClick();Assert(Ws<Label>(export,"previewNativeOutputStatus").Text.StartsWith("Saved"),"real export validates decoded motion: "+Ws<Label>(export,"previewNativeOutputStatus").Text);export.Close();
                }
                using(var decoded=NodeFactory.FromFile(null,Path.Combine(_output,"edited-motion.pac")))Assert(Math.Abs(Find<CHR0Node>(decoded).Children.OfType<CHR0EntryNode>().Single().GetFrameValue(5,15)-35)<.001,"fresh native edited key");
                // Clear the test working edit before normal native close (original files remain protected).
                editor.SelectedCHR0.RootNode.IsDirty=false;host.Close();
                // Previewer-first and motion-first path uses actual protected picker controls.
                var empty=(Form)Activator.CreateInstance(typeof(MainForm).Assembly.GetType("BrawlCrate.UI.ModelForm"),true);empty.Show(main);Pump(50);
                var nextEditor=ModelEditControl.Instances.Single();
                PickerFailureChecks(empty,changedInput);
                PickerOpen(empty,motion,false);Assert(nextEditor.TargetModel==null,"motion-first does not invent a model");
                PickerOpen(empty,model,true);Assert(nextEditor.TargetModel!=null,"explicit model attaches after motion");
                Ws<ListBox>(empty,"animationClips").SelectedIndex=0;Ws<Button>(empty,"animationPreview").PerformClick();BrowserMotion(nextEditor,"motion-first");
                empty.Size=new Size(850,620);Pump(100);Assert(Ws<PreviewAnimationBrowser>(empty,"previewAnimationBrowser").Visible && nextEditor.ModelPanel.Width>=220 && nextEditor.PlaybackPanel.Visible,"small-window browser/viewport/native timeline remain visible");CaptureWindow(empty,"browser-small.png");
                Ws<ToolStrip>(empty,"previewWorkspaceBar").Items.Cast<ToolStripItem>().Single(i=>i.Name=="previewWorkspaceToggle").PerformClick();Pump(80);
                Assert(Ws<ListBox>(empty,"animationClips").Height>=30 && nextEditor.ModelPanel.Height>=150,"small window with both workspace and browser retains usable clip list and viewport");CaptureWindow(empty,"browser-small-workspace.png");empty.Close();
                var multipleHost=(Form)Activator.CreateInstance(typeof(MainForm).Assembly.GetType("BrawlCrate.UI.ModelForm"),true);multipleHost.Show(main);PickerOpen(multipleHost,multiple,true);
                var multiEditor=ModelEditControl.Instances.Single();Assert(multiEditor.TargetModel==null && multiEditor._targetModels.Count==0 && multiEditor.ModelPanel._renderList.Count==0,"multiple models are catalogued without loading shadows or inferring a character");var modelSelector=Ws<ComboBox>(multipleHost,"animationModel");Assert(modelSelector.Visible && modelSelector.Items.Count==2,"indexed native model choices are visible");modelSelector.SelectedIndex=1;var selected=multiEditor.TargetModel;Assert(selected!=null && multiEditor._targetModels.Count==1 && multiEditor.ModelPanel._renderList.Count==1 && !multiEditor.EditingAll,"only explicitly selected character is loaded for rendering/playback");modelSelector.SelectedIndex=0;Assert(!ReferenceEquals(selected,multiEditor.TargetModel) && multiEditor._targetModels.Count==1 && multiEditor.ModelPanel._renderList.Count==1,"switching characters detaches previous geometry while retaining complete source hierarchy");multipleHost.Close();
                Assert((bool)CallProgram("Open",new[]{typeof(string),typeof(bool)},multiple,false),"native archive browser opens multi-model fixture");
                var archiveRoot=(ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null,null);main.TargetResource(archiveRoot);((BrawlCrate.NodeWrappers.ARCWrapper)main.resourceTree.SelectedNode).PreviewAll();Pump(80);
                var archiveEditor=ModelEditControl.Instances.Single();var archiveHost=archiveEditor.FindForm();Assert(archiveEditor._targetModels.Count==0 && archiveEditor.ModelPanel._renderList.Count==0,"native archive preview does not load every Brawl model");Ws<ComboBox>(archiveHost,"animationModel").SelectedIndex=1;Assert(archiveEditor._targetModels.Count==1 && archiveEditor.ModelPanel._renderList.Count==1,"native archive route renders one explicitly selected character");archiveHost.Close();
                // Registered, shared and assignments all drive the same session without replacing target identity.
                var session=PreviewAuthoringSession.For(project);var bound=session.ShowInstance(instance);var boundHost=bound.FindForm();
                bound.leftPanel.fileType.SelectedIndex=1;Expect<IOException>(()=>session.Apply(bound.AuthoringDocument),"Apply rejects a different selected native channel despite a retained CHR0");
                WsInvoke(boundHost,"workspaceExport");Assert(Ws<Label>(boundHost,"workspaceStatus").Text.StartsWith("Stopped:"),"project export rejects another selected channel before asking for a destination");
                WsChoose(Ws<ComboBox>(boundHost,"workspaceTarget"),differentCharacter);
                Ws<ComboBox>(boundHost,"animationScope").SelectedItem="Project";var projectClips=Ws<ListBox>(boundHost,"animationClips");projectClips.SelectedItem=projectClips.Items.Cast<object>().Single(n=>n.ToString().Contains(character.Substring(0,8)));Ws<Button>(boundHost,"animationPreview").PerformClick();Pump(80);
                Assert(bound.AuthoringReadOnly && bound.AuthoringDocument.Mode.StartsWith("Source") && bound.AuthoringDocument.TargetId==character && Ws<PreviewWorkspacePane>(boundHost,"previewWorkspace").TargetId==differentCharacter,"source preview retains a different explicitly selected target");
                Assert(bound.TargetAnimation is CHR0Node,"bound source preview restores the intended skeletal channel after a different native editor type");
                BrowserMotion(bound,"project-source");
                byte[] beforeBrowse=WorkspaceStateXml(project);Ws<ComboBox>(boundHost,"animationScope").SelectedItem="Shared library";var sharedClips=Ws<ListBox>(boundHost,"animationClips");Assert(sharedClips.Items.Count==3,"shared and project revisions in one browser");
                sharedClips.SelectedItem=sharedClips.Items.Cast<object>().Single(n=>n.ToString().StartsWith("AdditionalShared |"));Assert(WorkspaceStateXml(project).SequenceEqual(beforeBrowse),"metadata browse/select does not register or assign");
                Ws<Button>(boundHost,"animationPreview").PerformClick();Assert(project.State.Animations.Any(a=>a.Name=="AdditionalShared") && !bound.AuthoringDocument.Root.IsDirty,"contextual shared preview registers the immutable revision in the same project");
                Ws<ComboBox>(boundHost,"animationScope").SelectedItem="Assignments";Ws<Button>(boundHost,"animationPreview").PerformClick();Assert(bound.AuthoringReadOnly && bound.AuthoringDocument.TargetId==differentCharacter,"assignment audition is the explicitly selected target's read-only instance");
                Assert(Hash(otherPath)==otherHash && project.SharedLibrary.Pin("animations",clipId).MetadataHash==pin.MetadataHash,"other project/shared revision preserved");
                foreach(var pair in sharedHashes)Assert(Hash(pair.Key)==pair.Value,"all prepared shared metadata/native revisions unchanged");
                WsChoose(Ws<ComboBox>(boundHost,"workspaceTarget"),character);WsClick(boundHost,"workspaceInstallPack");Ws<NumericUpDown>(boundHost,"workspacePartCount").Value=1;WsClick(boundHost,"workspaceAddParts");
                var set=project.State.AnimationSets.Single(s0=>s0.TargetId==character);var part=set.Parts.Last();WsSelectPart(boundHost,part.Id);
                Ws<ComboBox>(boundHost,"animationScope").SelectedItem="Project";var applyClips=Ws<ListBox>(boundHost,"animationClips");applyClips.SelectedItem=applyClips.Items.Cast<object>().Single(n=>n.ToString().StartsWith("OriginalSwing |") && n.ToString().Contains(character.Substring(0,8)));
                Ws<Button>(boundHost,"animationApply").PerformClick();WaitFor(()=>!session.Busy,"browser Apply Animation");Pump(50);
                Assert(project.State.AnimationSets.Single(s0=>s0.Id==set.Id).Parts.Single(p0=>p0.Id==part.Id).InstanceId!=null && bound.AuthoringDocument.Editable,"browser Apply Animation reuses existing valid assignment and sole editable binding");
                project.Save(projectPath);boundHost.Close();
                foreach(var pair in hashes)Assert(Hash(pair.Key)==pair.Value,"original source hashes unchanged");
                Log("PASS: ordinary separate PAC, motion-first picker, real browser scopes/search/selection/Play, changed native transforms and framebuffer geometry, separate edit/export/decode; originals/shared/other project unchanged");
            }
            catch(Exception error){failure=error;Log("Focused failure: "+error);}
            finally{if(project.Dirty)project.Save(projectPath);foreach(var editor in ModelEditControl.Instances.ToArray()){foreach(var opened in editor._openedFiles)opened.IsDirty=false;editor.SetAuthoringReadOnly(false);editor.FindForm().Close();}main.Close();}
        }));
        Application.Run(main);if(failure!=null)throw failure;
    }
    private static void PickerOpen(Form host,string file,bool models,string container=null)
    {
        using(var picker=(Form)Activator.CreateInstance(typeof(PreviewSourcePicker),System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic,null,new object[]{host,false,Path.Combine(_output,"storage")},null))
        {
            picker.Show(host);Ws<TextBox>(picker,"previewSourcePath").Text=file;Ws<Button>(picker,"previewSourceInspect").PerformClick();WaitFor(()=>Ws<Button>(picker,"previewSourceInspect").Enabled,"source inspect completed");
            var choices=Ws<ListBox>(picker,"previewSourceChoices");Assert(choices.Items.Count>0,"source choices visible");
            if(container==null)choices.SelectedIndex=0;else choices.SelectedItem=choices.Items.Cast<object>().Single(n=>n.ToString().Contains("["+container+"]"));Ws<CheckBox>(picker,"previewSourceModels").Checked=models;
            Ws<Button>(picker,"previewSourceOpen").PerformClick();WaitFor(()=>Ws<Button>(picker,"previewSourceOpen").Enabled,"protected source open completed");
            Assert(!Ws<Label>(picker,"previewSourceStatus").Text.StartsWith("Stopped:"),Ws<Label>(picker,"previewSourceStatus").Text);picker.Close();
        }
    }
    private static void PickerFailureChecks(Form host,string changedInput)
    {
        using(var picker=(Form)Activator.CreateInstance(typeof(PreviewSourcePicker),System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic,null,new object[]{host,false,Path.Combine(_output,"storage")},null))
        {
            picker.Show(host);Ws<TextBox>(picker,"previewSourcePath").Text=changedInput;
            Ws<Button>(picker,"previewSourceInspect").PerformClick();Ws<Button>(picker,"previewSourceCancel").PerformClick();WaitFor(()=>Ws<Button>(picker,"previewSourceInspect").Enabled,"cancelled source discovery");
            Assert(Ws<Label>(picker,"previewSourceStatus").Text.StartsWith("Stopped:"),"cancelled inspection reports interruption rather than a complete catalogue");
            Ws<Button>(picker,"previewSourceInspect").PerformClick();WaitFor(()=>Ws<Button>(picker,"previewSourceInspect").Enabled,"source inspection");
            File.SetLastWriteTimeUtc(changedInput,DateTime.UtcNow.AddMinutes(5));Ws<CheckBox>(picker,"previewSourceModels").Checked=false;Ws<Button>(picker,"previewSourceOpen").PerformClick();WaitFor(()=>Ws<Button>(picker,"previewSourceOpen").Enabled,"changed source rejection");
            Assert(Ws<Label>(picker,"previewSourceStatus").Text.Contains("changed after inspection"),"changed selected source rejected before staging");
            Ws<TextBox>(picker,"previewSourcePath").Text=Path.Combine(_output,"absent.pac");Ws<Button>(picker,"previewSourceInspect").PerformClick();WaitFor(()=>Ws<Button>(picker,"previewSourceInspect").Enabled,"missing source rejection");
            Assert(Ws<Label>(picker,"previewSourceStatus").Text.StartsWith("Stopped:"),"missing source reports reason");picker.Close();
        }
    }
    private static void BrowserMotion(ModelEditControl editor,string label)
    {
        Assert(editor.SelectedCHR0!=null,"browser bound clip");var model=(MDL0Node)editor.TargetModel;var bone=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name=="Paddle");
        editor.SetFrame(1);Pump(50);float before=bone.FrameState.Rotate._z;CaptureViewport(editor,"browser-"+label+"-first.png");
        editor.SetFrame(16);Pump(50);Assert(Math.Abs(bone.FrameState.Rotate._z-before)>20,"browser-selected clip changes native bone transform");CaptureViewport(editor,"browser-"+label+"-middle.png");
        using(var first=new Bitmap(Path.Combine(_output,"browser-"+label+"-first.png")))using(var second=new Bitmap(Path.Combine(_output,"browser-"+label+"-middle.png")))
        {int changed=0;for(int y=0;y<first.Height;y++)for(int x=0;x<first.Width;x++)if(first.GetPixel(x,y)!=second.GetPixel(x,y))changed++;Assert(changed>100,"rendered geometry actually changes between native frames");Log("Observed "+label+" framebuffer changed pixels="+changed);}
        editor.SetFrame(1);int played=0;
        using(var stop=new System.Windows.Forms.Timer{Interval=250}){stop.Tick+=(s,e)=>{played=editor.CurrentFrame;editor.StopAnim();stop.Stop();};stop.Start();editor.PlaybackPanel.btnPlay.PerformClick();WaitFor(()=>played!=0,"native playback button timer");}
        Assert(played>1,"real native Play advances after browser selection");
    }
    private static int LocalAnimationBrowser(string configuration,string evidence)
    {
        try
        {
            if(Directory.Exists(evidence))throw new IOException("Use a new private evidence directory.");Directory.CreateDirectory(evidence);_output=Path.GetFullPath(evidence);
            var config=new System.Xml.XmlDocument{XmlResolver=null};using(var reader=System.Xml.XmlReader.Create(configuration,new System.Xml.XmlReaderSettings{DtdProcessing=System.Xml.DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=65536}))config.Load(reader);
            var inputs=config.SelectNodes("/localHumanoid/character").Cast<System.Xml.XmlElement>().ToArray();Assert(inputs.Length==2,"explicit configured pair");
            Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT",Path.Combine(_output,"settings"));string storage=Path.Combine(_output,"storage");SourceWriteGuard.Initialize(Path.Combine(storage,"source-roots.txt"));
            Application.EnableVisualStyles();System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(NativeProgram.TypeHandle);
            var hashes=inputs.SelectMany(i=>new[]{i.GetAttribute("model"),i.GetAttribute("motion")}).Distinct().ToDictionary(p=>p,Hash);
            Exception failure=null;
            // Unexpected paint/message-loop errors must fail this check, not wait in
            // the product's interactive diagnostic dialog and later report a clean pass.
            var reportMethod=NativeProgram.GetMethod("Application_ThreadException",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
            var nativeReport=(ThreadExceptionEventHandler)Delegate.CreateDelegate(typeof(ThreadExceptionEventHandler),reportMethod);
            Application.ThreadException-=nativeReport;
            Application.ThreadException+=(sender,args)=>{failure=args.Exception;Log("FAIL: native GUI thread exception: "+args.Exception);File.WriteAllText(Path.Combine(_output,"gui-thread-failure.txt"),args.Exception.ToString());};
            var watch=new Thread(()=>{Thread.Sleep(120000);File.WriteAllText(Path.Combine(_output,"timeout.txt"),"Local browser did not finish within 120 seconds.");Environment.Exit(3);}){IsBackground=true};watch.Start();
            var main=MainForm.Instance;
            main.Shown+=(s,e)=>main.BeginInvoke(new Action(()=>{
                try
                {
                    foreach(var input in inputs)
                    {
                        string file=input.GetAttribute("model");var staged=new SourceFolderSession(Path.GetDirectoryName(file),storage).Stage(file,CancellationToken.None);
                        Assert((bool)CallProgram("Open",new[]{typeof(string),typeof(bool)},staged.StagedPath,false),"ordinary main browser selected model PAC");
                        var root=(ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null,null);var model=(MDL0Node)LocalNativeAt(root,input.GetAttribute("modelPath"));
                        main.TargetResource(model);((BrawlCrate.NodeWrappers.MDL0Wrapper)main.resourceTree.SelectedNode).Preview();
                        var editor=ModelEditControl.Instances.Single();var host=editor.FindForm();host.Size=new Size(1180,850);
                        PickerOpen(host,input.GetAttribute("motion"),false,input.GetAttribute("motionContainer"));
                        Ws<TextBox>(host,"animationSearch").Text=input.GetAttribute("clip");var clips=Ws<ListBox>(host,"animationClips");
                        clips.SelectedItem=clips.Items.Cast<object>().Single(n=>n.ToString().StartsWith(input.GetAttribute("clip")+" | CHR0 |"));Ws<Button>(host,"animationPreview").PerformClick();Pump(80);
                        Assert(ReferenceEquals(model,editor.TargetModel) && editor.SelectedCHR0?.Name==input.GetAttribute("clip"),"actual separate-motion browser binding retains selected model");editor.SetAuthoringReadOnly(true);
                        editor.SetFrame(1);var first=model.BoneCache.OfType<MDL0BoneNode>().Select(b=>b.Matrix).ToArray();CaptureViewport(editor,"local-"+input.GetAttribute("id")+"-first.png");
                        editor.SetFrame(1+editor.SelectedCHR0.FrameCount/2);Pump(80);Assert(model.BoneCache.OfType<MDL0BoneNode>().Where((b,i)=>b.Matrix!=first[i]).Any(),"local selected motion changes bone matrices");CaptureViewport(editor,"local-"+input.GetAttribute("id")+"-middle.png");
                        Assert(Hash(Path.Combine(_output,"local-"+input.GetAttribute("id")+"-first.png"))!=Hash(Path.Combine(_output,"local-"+input.GetAttribute("id")+"-middle.png")),"local rendered geometry changes");
                        int played=0;editor.SetFrame(1);using(var timer=new System.Windows.Forms.Timer{Interval=300}){timer.Tick+=(a,b)=>{played=editor.CurrentFrame;editor.StopAnim();timer.Stop();};timer.Start();editor.PlaybackPanel.btnPlay.PerformClick();WaitFor(()=>played!=0,"local native Play");}Assert(played>1,"local native playhead advances");
                        string output=Path.Combine(_output,input.GetAttribute("id")+"-motion.brres");PreviewNativeExportForm.Export(editor,output);
                        using(var reopened=NodeFactory.FromFile(null,output))Assert(Find<CHR0Node>(reopened)!=null,"local separate native motion reopens");
                        CaptureWindow(host,"local-"+input.GetAttribute("id")+"-browser.png");Assert(!root.IsDirty,"local source model unchanged in memory");host.Close();
                        Log("PASS: configured "+input.GetAttribute("id")+" ordinary model/separate-PAC browser selection, geometry, native Play and separate motion BRRES decode; no rig review or game claim");
                    }
                }
                catch(Exception error){failure=error;Log("FAIL: "+error);}
                finally{foreach(var editor in ModelEditControl.Instances.ToArray()){foreach(var opened in editor._openedFiles)opened.IsDirty=false;editor.FindForm().Close();}main.Close();}
            }));
            Application.Run(main);if(failure!=null)throw failure;
            foreach(var pair in hashes)Assert(Hash(pair.Key)==pair.Value,"configured original hashes unchanged");File.WriteAllLines(Path.Combine(_output,"source-hashes.txt"),hashes.Select(p=>p.Value+"\t"+p.Key));File.WriteAllLines(Path.Combine(_output,"result.txt"),Results);return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);File.WriteAllText(Path.Combine(_output,"failure.txt"),error.ToString());return 1;}
    }
}
