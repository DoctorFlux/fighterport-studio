// FighterPort original previewer workspace acceptance, 2026-10-17. No commercial data.
using BrawlCrate.Authoring;
using BrawlCrate.UI;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

internal static partial class NativeSmoke
{
    private static void ExerciseWorkspace(bool gui)
    {
        string a=FkFixture(false),b=FkFixture(true),storage=Path.Combine(_output,"storage"),library=Path.Combine(_output,"library");
        var p=new CharacterProject(storage);string aid=p.RegisterSource(LibraryResolution(a,"ws-a"),"Workspace A"),bid=p.RegisterSource(LibraryResolution(b,"ws-b"),"Workspace B");
        Assert(p.State.Version==1,"embedded source remains v1 until deliberate authoring");p.InstallAnimationPack(aid,AnimationPack.BrawlAuthoring());Assert(p.State.Version==3,"v1 to v3 slots migration");p.Undo();Assert(p.State.Version==1 && p.State.AnimationSets.Count==0,"migration undo preserves v1 state");
        p.MigrateToSharedLibrary(library);string clip=p.State.Animations.Single(n=>n.SourceCharacterId==aid).Id;
        string extraFolder=Path.Combine(_output,"extra-original");Directory.CreateDirectory(extraFolder);string extra=Path.Combine(extraFolder,"fall.chr0");
        using(var root=NodeFactory.FromFile(null,a)){var m=Find<CHR0Node>(root);m.Name="OriginalFallVariant";var t=m.Children.OfType<CHR0EntryNode>().Single();foreach(int f in new[]{0,15,30})t.SetKeyframe(5,f,f*4,true)._tangent=4;m.Export(extra);}
        string fallClip=p.AddAnimation(aid,extra),idle=p.Assign(clip,aid,"Preserved custom assignment"),fall=p.Assign(fallClip,aid,"Fall motion");
        string first=Path.Combine(_output,"workspace-one.fpcharacter"),second=Path.Combine(_output,"workspace-two.fpcharacter");p.Save(first);
        var other=new CharacterProject(storage);other.UseSharedClip(clip,1,library);other.Assign(clip,aid,"Independent other project");other.Save(second);string secondHash=Hash(second),pin=p.SharedLibrary.Pin("animations",clip).MetadataHash;
        var originals=new[]{a,b,extra}.ToDictionary(path=>path,Hash);
        var session=PreviewAuthoringSession.For(p);
        if(gui)WorkspaceGui(session,other,aid,clip,idle,fall,first,a);else
        {
            string set=p.InstallAnimationPack(aid,AnimationPack.BrawlAuthoring());var group=p.State.AnimationSets.Single().Pack.Groups.First();
            p.AddAnimationParts(set,group.Id,5,"single","default");var parts=p.State.AnimationSets.Single().Parts.Where(n=>n.GroupId==group.Id).ToArray();
            p.BindAnimationPart(set,parts[0].Id,clip);p.AssignAnimationPart(set,parts[0].Id,clip);p.AssignAnimationPart(set,parts[4].Id,clip);
            var special=p.State.AnimationSets.Single().Pack.Groups.Single(n=>n.Label=="Neutral special");p.AddAnimationParts(set,special.Id,3,"start","ground");p.AddAnimationParts(set,special.Id,2,"loop","air");
            p.Atomic(()=>{p.ChooseAnchor(aid,"Idle",idle);p.ChooseAnchor(aid,"Fall",fall);});p.Undo();Assert(p.State.Anchors.Count==0,"anchor pair atomic undo");p.Redo();
            p.SetAnchorPhase(aid,"Idle",idle,5);p.SetAnchorPhase(aid,"Fall",fall,10);EditWorkspaceKey(p,idle,25);
            // Changes invalidate a reference explicitly; reselect its revision, then phase.
            Assert(p.AnchorProblem(p.State.Anchors.Single(n=>n.Role=="Idle"))!=null,"native edit makes anchor stale");p.ChooseAnchor(aid,"Idle",idle);p.SetAnchorPhase(aid,"Idle",idle,5);p.Save(first);
            p.ExportInstance(idle,Path.Combine(_output,"workspace-output.brres"));
        }
        CheckWorkspaceGraph(p,aid);VerifyWorkspaceNative(Path.Combine(_output,"workspace-output.brres"));
        string before=Hash(first);Expect<IOException>(()=>p.BindAnimationPart(p.State.AnimationSets.Single(n=>n.TargetId==aid).Id,p.State.AnimationSets.Single(n=>n.TargetId==aid).Parts.Last().Id,clip),"duplicate destination rejected");
        var bad=AnimationPack.BrawlAuthoring();bad.Version=0;Expect<IOException>(()=>p.InstallAnimationPack(bid,bad),"invalid pack");Expect<OperationCanceledException>(()=>p.InstallAnimationPack(bid,AnimationPack.BrawlAuthoring(),new CancellationToken(true)),"cancelled pack");
        Expect<IOException>(()=>p.Atomic(()=>p.ChooseAnchor(aid,"WrongRig",p.Assign(p.State.Animations.Single(n=>n.SourceCharacterId==bid).Id,bid,"Other rig"))),"target-incompatible anchor");
        Expect<IOException>(()=>p.SetAnchorPhase(aid,"Idle",idle,99999),"phase bounds");
        Expect<IOException>(()=>p.ExportInstance(idle,a),"source overwrite rejected");Assert(Hash(first)==before,"failed operations leave saved metadata intact");
        // Missing references remain recoverable metadata with precise diagnostics.
        var xml=new XmlDocument{XmlResolver=null};xml.Load(first);xml.SelectSingleNode("//ReferenceAnchor/InstanceId").InnerText=Guid.NewGuid().ToString("N");string missing=Path.Combine(_output,"missing-reference.fpcharacter");xml.Save(missing);
        var missingProject=CharacterProject.Load(missing,storage);Assert(missingProject.AnchorProblem(missingProject.State.Anchors.First()).StartsWith("Missing"),"missing anchor diagnostic");
        var loaded=CharacterProject.Load(first,storage);CheckWorkspaceGraph(loaded,aid);loaded.ExportInstance(idle,Path.Combine(_output,"workspace-reopened.brres"));AssertSharedKey(Path.Combine(_output,"workspace-reopened.brres"),25);
        Assert(Hash(second)==secondHash && p.SharedLibrary.Pin("animations",clip).MetadataHash==pin,"independent project and shared clip remain unchanged");
        foreach(var item in originals)Assert(Hash(item.Key)==item.Value,"workspace original source unchanged");File.WriteAllLines(Path.Combine(_output,"workspace-originals.sha256"),originals.Select(n=>n.Value+"\t"+n.Key));
        File.WriteAllText(Path.Combine(_output,"workspace-target.txt"),aid);File.WriteAllText(Path.Combine(_output,"workspace-instance.txt"),idle);File.WriteAllText(Path.Combine(_output,"workspace-other.sha256"),secondHash);
        using(var child=Process.Start(new ProcessStartInfo{FileName=Process.GetCurrentProcess().MainModule.FileName,Arguments="--verify-workspace \""+_output+"\"",UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardError=true,RedirectStandardOutput=true}))
        {if(!child.WaitForExit(25000)){child.Kill();throw new TimeoutException("Workspace reload");}Assert(child.ExitCode==0,child.StandardError.ReadToEnd());Log(child.StandardOutput.ReadToEnd().Trim());}
        Log("PASS: typed repeatable slots, bound/unbound parts, ground/air phases, distinct anchors, migration/custom preservation, atomic undo, fresh native output and original/shared/other-project hashes");
    }
    private static void EditWorkspaceKey(CharacterProject p,string instance,float value)
    {var lease=p.OpenInstance(instance);using(var root=NodeFactory.FromFile(null,lease.Path)){Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single(e=>e.Name=="Paddle").SetKeyframe(5,15,value);p.CaptureNativeEdits(lease,root,lease.Path);}}
    private static void CheckWorkspaceGraph(CharacterProject p,string target)
    {
        var set=p.State.AnimationSets.Single(n=>n.TargetId==target);var jab=set.Pack.Groups.Single(n=>n.Label=="Jab");var parts=set.Parts.Where(n=>n.GroupId==jab.Id).ToArray();
        Assert(p.State.Version==3 && parts.Length==5,"v3 repeatable five-part group");Assert(parts.Any(n=>n.Binding!=null && n.InstanceId!=null) && parts.Any(n=>n.Binding==null && n.InstanceId!=null),"assigned bound and intentionally unbound parts");
        Assert(set.Parts.Any(n=>n.Variant=="ground" && n.Phase=="start") && set.Parts.Any(n=>n.Variant=="air" && n.Phase=="loop"),"ground/air parts remain distinct");
        Assert(set.Parts.Any(n=>n.Label=="Preserved custom assignment"),"legacy custom assignment preserved");
        Assert(p.State.Anchors.Single(n=>n.Role=="Idle").Frame==5 && p.State.Anchors.Single(n=>n.Role=="Fall").Frame==10,"distinct native phases saved");
    }
    private static T Ws<T>(Control host,string name)where T:Control=>(T)host.Controls.Find(name,true).Single();
    private static void WsInvoke(Control host,string name)
    {var b=Ws<Button>(host,name);Control parent=b;while(parent!=null && !(parent is TabPage))parent=parent.Parent;if(parent!=null)Ws<TabControl>(host,"workspaceTabs").SelectedTab=(TabPage)parent;b.PerformClick();Pump(30);var workspace=Ws<PreviewWorkspacePane>(host,"previewWorkspace");WaitFor(()=>workspace.Session?.Busy!=true,"bounded previewer operation");Pump(30);}
    private static void WsClick(Control host,string name)
    {WsInvoke(host,name);string status=Ws<Label>(host,"workspaceStatus").Text;Assert(!status.StartsWith("Stopped:"),name+": "+status);}
    private static void WsChoose(ComboBox box,string id)
    {box.SelectedItem=box.Items.Cast<object>().Single(n=>(string)n.GetType().GetField("Id").GetValue(n)==id);}
    private static void WsSelectPart(Control host,string id)
    {var tree=Ws<TreeView>(host,"workspaceParts");tree.SelectedNode=tree.Nodes.Cast<TreeNode>().SelectMany(n=>n.Nodes.Cast<TreeNode>()).Single(n=>(string)n.Tag==id);Pump(50);}
    private static byte[] WorkspaceStateXml(CharacterProject project)
    {using(var stream=new MemoryStream()){new System.Xml.Serialization.XmlSerializer(typeof(CharacterProjectState)).Serialize(stream,project.State);return stream.ToArray();}}
    [System.Runtime.InteropServices.DllImport("user32.dll",EntryPoint="SendMessageW",CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern IntPtr WorkspaceReplaceText(IntPtr handle,int message,IntPtr undo,string text);
    private static void CheckWorkspaceUndoContexts(Form host,PreviewAuthoringSession session,ModelEditControl editor)
    {
        var field=Ws<TextBox>(host,"workspacePartLabel");Control page=field;while(!(page is TabPage))page=page.Parent;Ws<TabControl>(host,"workspaceTabs").SelectedTab=(TabPage)page;
        host.Activate();((ScrollableControl)page).ScrollControlIntoView(field);field.Focus();Pump(30);
        Assert(field.Focused,"text shortcut test owns the intended input focus");
        field.Text="Local label draft";field.ClearUndo();field.SelectAll();WorkspaceReplaceText(field.Handle,0xC2,(IntPtr)1,"Changed local draft");
        Assert(field.CanUndo,"native EM_REPLACESEL creates an undoable text edit");
        var state=WorkspaceStateXml(session.Project);var root=editor.AuthoringDocument.Root;
        var route=host.GetType().GetMethod("ProcessCmdKey",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        var message=Message.Create(field.Handle,0x100,(IntPtr)Keys.Z,IntPtr.Zero);
        Assert(!(bool)route.Invoke(host,new object[]{message,Keys.Control|Keys.Z}),"previewer leaves text Ctrl-Z for the native text control");
        field.Undo();Assert(field.Text=="Local label draft" && state.SequenceEqual(WorkspaceStateXml(session.Project)) && ReferenceEquals(root,editor.AuthoringDocument.Root) && !root.IsDirty,"text undo does not also undo native/project edits");
        Assert(!(bool)route.Invoke(host,new object[]{message,Keys.Control|Keys.Y}) && state.SequenceEqual(WorkspaceStateXml(session.Project)),"text Ctrl-Y cannot invoke a native/project menu shortcut");
        Ws<TreeView>(host,"workspaceParts").Focus();Pump(20);message=Message.Create(host.Handle,0x100,(IntPtr)Keys.Z,IntPtr.Zero);
        Assert((bool)route.Invoke(host,new object[]{message,Keys.Control|Keys.Z}),"workspace Ctrl-Z is consumed once by project history");
        Assert(!state.SequenceEqual(WorkspaceStateXml(session.Project)),"workspace shortcut undoes the applied project revision");
        Assert((bool)route.Invoke(host,new object[]{message,Keys.Control|Keys.Y}) && state.SequenceEqual(WorkspaceStateXml(session.Project)),"workspace Ctrl-Y restores exact project state");
        Log("PASS: focused text Undo leaves native/project state unchanged; workspace Ctrl-Z/Y routes once to project history (handler automation)");
    }
    private static void WorkspaceGui(PreviewAuthoringSession session,CharacterProject other,string target,string clip,string idle,string fall,string saved,string native)
    {
        var main=MainForm.Instance;Exception failure=null;main.Shown+=(s,e)=>main.BeginInvoke(new Action(()=>{
            try
            {
                Assert((bool)CallProgram("Open",new[]{typeof(string),typeof(bool)},native,false),"plain native source open");var original=(ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null,null);main.TargetResource(Find<MDL0Node>(original));((BrawlCrate.NodeWrappers.MDL0Wrapper)main.resourceTree.SelectedNode).Preview();
                var editor=ModelEditControl.Instances.Single();var host=editor.FindForm();var bar=Ws<ToolStrip>(host,"previewWorkspaceBar");bar.Items.Cast<ToolStripItem>().Single(n=>n.Name=="previewWorkspaceToggle").PerformClick();
                Ws<TextBox>(host,"workspaceProjectPath").Text=saved;WsClick(host,"workspaceAttach");WsChoose(Ws<ComboBox>(host,"workspaceTarget"),target);WsChoose(Ws<ComboBox>(host,"workspaceInstances"),idle);WsClick(host,"workspaceActivate");
                Assert(editor.AuthoringDocument.Session==session && editor.AuthoringDocument.InstanceId==idle,"explicit attached live session/instance");
                using(var library=new CharacterLibraryForm(session.Project))
                {
                    library.Show(main);Assert(ReferenceEquals(library.Project,session.Project),"standalone and previewer share authoritative live project");
                    WsClick(host,"workspaceLibrarySearch");Assert(Ws<ListBox>(host,"workspaceCatalogue").Items.Count==3,"actual shared metadata catalogue");
                    WsClick(host,"workspaceInstallPack");var set=session.Project.State.AnimationSets.Single();string group=set.Pack.Groups.First().Id;
                    var rootBefore=editor.AuthoringDocument.Root;editor.SetFrame(6);WsChoose(Ws<ComboBox>(host,"workspaceGroup"),group);Ws<NumericUpDown>(host,"workspacePartCount").Value=5;WsClick(host,"workspaceAddParts");
                    Assert(ReferenceEquals(editor.AuthoringDocument.Root,rootBefore) && editor.CurrentFrame==6,"metadata commands retain native document/playhead");
                    WsClick(host,"workspaceUndo");Assert(!session.Project.State.AnimationSets.Single().Parts.Any(n=>n.GroupId==group),"grouped part undo");WsClick(host,"workspaceRedo");
                    var parts=session.Project.State.AnimationSets.Single().Parts.Where(n=>n.GroupId==group).ToArray();WsSelectPart(host,parts[0].Id);WsChoose(Ws<ComboBox>(host,"workspaceDestination"),clip);WsClick(host,"workspaceBind");WsChoose(Ws<ComboBox>(host,"workspaceClip"),clip);
                    int instancesBeforeCancel=session.Project.State.Instances.Count;Ws<Button>(host,"workspaceAssign").PerformClick();Assert(session.Busy,"background assignment exposes busy state");Expect<IOException>(()=>session.Execute(()=>session.Project.SetSlot(idle,"wrong")),"other views cannot mutate a busy project");Ws<Button>(host,"workspaceCancel").PerformClick();WaitFor(()=>!session.Busy,"cancelled background assignment");Pump(50);Assert(session.Project.State.Instances.Count==instancesBeforeCancel && session.Project.State.AnimationSets.Single().Parts.Single(n=>n.Id==parts[0].Id).InstanceId==null,"cancelled worker has no partial assignment");WsClick(host,"workspaceAssign");
                    WsSelectPart(host,parts[4].Id);WsClick(host,"workspaceAssign");string slotId=session.Project.State.AnimationSets.Single().Parts.Single(n=>n.Id==parts[4].Id).SlotId;
                    Ws<TextBox>(host,"workspacePartLabel").Text="Intentionally unbound extra";WsClick(host,"workspaceRenamePart");WsClick(host,"workspacePartEarlier");Assert(session.Project.State.AnimationSets.Single().Parts.Single(n=>n.Id==parts[4].Id).SlotId==slotId,"renaming/reordering retains stable slot/part/variant identity");
                    string special=session.Project.State.AnimationSets.Single().Pack.Groups.Single(n=>n.Label=="Neutral special").Id;WsChoose(Ws<ComboBox>(host,"workspaceGroup"),special);Ws<ComboBox>(host,"workspacePhase").SelectedItem="start";Ws<ComboBox>(host,"workspaceVariant").SelectedItem="ground";Ws<NumericUpDown>(host,"workspacePartCount").Value=3;WsClick(host,"workspaceAddParts");
                    Ws<ComboBox>(host,"workspacePhase").SelectedItem="loop";Ws<ComboBox>(host,"workspaceVariant").SelectedItem="air";Ws<NumericUpDown>(host,"workspacePartCount").Value=2;WsClick(host,"workspaceAddParts");
                    WsChoose(Ws<ComboBox>(host,"workspaceInstances"),idle);WsClick(host,"workspaceActivate");var model=editor.AuthoringDocument.Model;editor.SelectedBone=model.BoneCache.OfType<MDL0BoneNode>().Single(n=>n.Name=="Paddle");PrepareLibraryEditor(editor);editor.SetFrame(16);
                    editor.CHR0Editor.numRotZ.Value=25;editor.CHR0Editor.BoxChangedCreateUndo(editor.CHR0Editor.numRotZ,EventArgs.Empty);
                    string unappliedHash=session.Project.State.Instances.Single(n=>n.Id==idle).WorkingHash;editor.Undo();Assert(Math.Abs(Find<CHR0Node>(editor.AuthoringDocument.Root).Children.OfType<CHR0EntryNode>().Single().GetFrameValue(5,15)-180)<.01,"native undo concerns the key editor only");editor.Redo();Assert(session.Project.State.Instances.Single(n=>n.Id==idle).WorkingHash==unappliedHash,"native undo/redo does not also invoke project history");WsClick(host,"workspaceApply");AssertSharedKeyExport(session.Project,idle,25,"applied.brres");
                    WsClick(host,"workspaceUndo");AssertSharedKeyExport(session.Project,idle,180,"undo.brres");WsClick(host,"workspaceRedo");AssertSharedKeyExport(session.Project,idle,25,"redo.brres");
                    CheckWorkspaceUndoContexts(host,session,editor);
                    WsChoose(Ws<ComboBox>(host,"workspaceIdleInstance"),idle);WsChoose(Ws<ComboBox>(host,"workspaceFallInstance"),fall);WsClick(host,"workspaceChooseAnchors");WsClick(host,"workspaceUndo");Assert(session.Project.State.Anchors.Count==0,"reference pair atomic undo from previewer");WsClick(host,"workspaceRedo");
                    WsClick(host,"workspacePreviewIdle");editor.SetFrame(6);WsClick(host,"workspacePhaseIdle");CaptureViewport(editor,"workspace-idle-native.png");
                    WsClick(host,"workspacePreviewFall");editor.SetFrame(11);WsClick(host,"workspacePhaseFall");CaptureViewport(editor,"workspace-fall-native.png");
                    Ws<TextBox>(host,"workspaceReferenceRole").Text="Landing";WsChoose(Ws<ComboBox>(host,"workspaceNamedReferenceInstance"),fall);WsClick(host,"workspaceChooseNamedReference");WsClick(host,"workspacePreviewNamedReference");editor.SetFrame(8);WsClick(host,"workspacePhaseNamedReference");Assert(session.Project.State.Anchors.Single(n=>n.Role=="Landing").Frame==7,"other explicit target reference role uses native playhead");
                    var frozen=session.Project.State.Instances.Select(n=>n.WorkingHash).ToArray();WsClick(host,"workspaceSourcePreview");Assert(editor.AuthoringReadOnly && editor.PlaybackPanel.Enabled,"read-only source retains native timeline");editor.SetFrame(16);CaptureViewport(editor,"workspace-source-native.png");Assert(!editor.AuthoringDocument.Root.IsDirty && frozen.SequenceEqual(session.Project.State.Instances.Select(n=>n.WorkingHash)),"source preview cannot mutate assignments/shared data");
                    WsChoose(Ws<ComboBox>(host,"workspaceInstances"),idle);WsClick(host,"workspaceActivate");var doc=editor.AuthoringDocument;Find<CHR0Node>(doc.Root).Children.OfType<CHR0EntryNode>().Single().SetKeyframe(5,15,35);
                    session.PendingDecision=d=>PendingEditDecision.Cancel;WsInvoke(host,"workspaceSourcePreview");Assert(editor.AuthoringDocument==doc && doc.Root.IsDirty,"cancel pending switch retains exact document");
                    session.PendingDecision=d=>PendingEditDecision.Discard;WsClick(host,"workspaceSourcePreview");WsClick(host,"workspaceActivate");
                    // Unsupported model edits reject and remain pending until explicit discard.
                    doc=editor.AuthoringDocument;doc.Model.BoneCache.OfType<MDL0BoneNode>().Last().Name="UnsupportedBoneEdit";WsInvoke(host,"workspaceApply");Assert(doc.Root.IsDirty && Ws<Label>(host,"workspaceStatus").Text.StartsWith("Stopped:"),"unsupported native changes retained after failed apply");session.PendingDecision=d=>PendingEditDecision.Discard;WsClick(host,"workspaceSourcePreview");WsClick(host,"workspaceActivate");
                    string targetB=session.Project.State.Characters.Single(n=>n.Id!=target).Id;WsChoose(Ws<ComboBox>(host,"workspaceTarget"),targetB);WsClick(host,"workspaceInstallPack");
                    var setB=session.Project.State.AnimationSets.Single(n=>n.TargetId==targetB);WsChoose(Ws<ComboBox>(host,"workspaceGroup"),setB.Pack.Groups.First().Id);Ws<NumericUpDown>(host,"workspacePartCount").Value=1;Ws<ComboBox>(host,"workspacePhase").SelectedItem="single";Ws<ComboBox>(host,"workspaceVariant").SelectedItem="default";WsClick(host,"workspaceAddParts");
                    string crossPart=session.Project.State.AnimationSets.Single(n=>n.TargetId==targetB).Parts.Single().Id;WsSelectPart(host,crossPart);WsChoose(Ws<ComboBox>(host,"workspaceDestination"),session.Project.State.Animations.Single(n=>n.SourceCharacterId==targetB).Id);WsClick(host,"workspaceBind");WsChoose(Ws<ComboBox>(host,"workspaceClip"),clip);
                    var expectedMap=OriginalMap(session.Project.State.Characters.Single(n=>n.Id==target),session.Project.State.Characters.Single(n=>n.Id==targetB));var grid=Ws<DataGridView>(host,"rigPairs");
                    foreach(var pair in expectedMap.Pairs){grid.Rows[pair.Source].Cells[1].Value=pair.Role;grid.Rows[pair.Source].Cells[2].Value=((DataGridViewComboBoxColumn)grid.Columns[2]).Items[pair.Target+1];}
                    foreach(int i in expectedMap.TargetRestOnly)Ws<CheckedListBox>(host,"rigRestOnly").SetItemChecked(i,true);Ws<TextBox>(host,"rigReviewNote").Text=expectedMap.ReviewNote;WsClick(host,"rigApprove");WsClick(host,"workspaceAssign");
                    string crossInstance=session.Project.State.AnimationSets.Single(n=>n.TargetId==targetB).Parts.Single().InstanceId;session.Project.ExportInstance(crossInstance,Path.Combine(_output,"workspace-fk.brres"));VerifyFkAnalytic(Path.Combine(_output,"workspace-fk.brres"),true);editor.SetFrame(16);CaptureViewport(editor,"workspace-fk-native.png");
                    WsChoose(Ws<ComboBox>(host,"workspaceTarget"),target);WsChoose(Ws<ComboBox>(host,"workspaceInstances"),idle);WsClick(host,"workspaceActivate");
                    var secondSession=PreviewAuthoringSession.For(other);var secondEditor=secondSession.ShowInstance(other.State.Instances.Single().Id);Assert(editor.AuthoringReadOnly && secondEditor.AuthoringDocument.Editable,"second viewer explicitly takes sole edit ownership");Expect<IOException>(()=>session.Apply(editor.AuthoringDocument),"stale comparison cannot apply into another project");
                    WsClick(host,"workspaceActivate");Assert(editor.AuthoringDocument.Editable && secondEditor.AuthoringReadOnly,"explicit handback restores intended target");
                    host.Size=new Size(850,620);Pump(50);Assert(editor.ModelPanel.Width>=220 && editor.ModelPanel.Height>=150,"small-window canvas remains usable");Assert(editor.PlaybackPanel.Visible && editor.PlaybackPanel.Right<=editor.animEditors.ClientSize.Width+2,"native timeline remains inside the small-window client area");CaptureWindow(host,"workspace-small.png");
                    var split=Ws<SplitContainer>(host,"previewWorkspaceSplit");int commands=session.Project.State.AnimationSets.Single(n=>n.TargetId==target).Parts.Count;bar.Items.Cast<ToolStripItem>().Single(n=>n.Name=="previewWorkspaceToggle").PerformClick();bar.Items.Cast<ToolStripItem>().Single(n=>n.Name=="previewWorkspaceToggle").PerformClick();Assert(!split.Panel1Collapsed && session.Project.State.AnimationSets.Single(n=>n.TargetId==target).Parts.Count==commands,"panel hide/reopen retains state without duplicate commands");
                    host.Scale(new SizeF(1.5f,1.5f));host.Size=new Size(1280,900);Pump(80);CaptureWindow(host,"workspace-simulated-150pct.png");Assert(Ws<ComboBox>(host,"workspaceTarget").Width<Ws<PreviewWorkspacePane>(host,"previewWorkspace").Width,"simulated scaling keeps dropdown actions inside the authoring pane");CaptureViewport(editor,"workspace-target-native.png");
                    Ws<TextBox>(host,"workspaceOutputPath").Text=Path.Combine(_output,"workspace-output.brres");WsClick(host,"workspaceExport");WsClick(host,"workspaceSave");WsClick(host,"workspaceDiagnostics");Assert(!session.Project.Dirty && library.Project.State.Anchors.Count==3,"previewer save and standalone state agree");
                    CaptureWindow(host,"workspace-persisted.png");session.PendingDecision=null;
                    CallProgram("Close",Type.EmptyTypes);
                }
                Log("PASS: real Model Previewer attach/library/typed slots/anchors/native apply/undo/save/export, source comparison, explicit two-view handoff, pending rejection, small window and simulated scaling");
            }catch(Exception error){failure=error;}finally{main.Dispose();Application.ExitThread();}
        }));Application.Run(main);if(failure!=null)throw new InvalidOperationException("Previewer workspace GUI",failure);
    }
    private static void AssertSharedKeyExport(CharacterProject p,string instance,float value,string file){string path=Path.Combine(_output,file);p.ExportInstance(instance,path);AssertSharedKey(path,value);}
    private static void VerifyWorkspaceNative(string path)
    {
        using(var root=NodeFactory.FromFile(null,path))
        {
            var model=Find<MDL0Node>(root);model.Populate();var motion=Find<CHR0Node>(root);Assert(model.BoneCache.Length==2 && motion.FrameCount==31 && motion.Loop,"decoded native model/timing/loop distinct from project metadata");
            foreach(var sample in new[]{new[]{0f,0f},new[]{15f,25f},new[]{30f,360f}})
            {
                model.ApplyCHR(motion,sample[0]+1);double c=Math.Cos(sample[1]*Math.PI/180),s=Math.Sin(sample[1]*Math.PI/180);
                foreach(var v in ((MDL0ObjectNode)model._objList.Single()).Vertices){var p=v.WeightedPosition;Assert(Math.Abs(p._x-(c*v.Position._x-s*v.Position._y))<.002 && Math.Abs(p._y-(s*v.Position._x+c*v.Position._y))<.002,"independent decoded native target pose <=.002 native units");}
            }
        }
    }
    private static int VerifyWorkspaceRestart(string directory)
    {
        try{_output=directory;Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT",Path.Combine(directory,"settings"));BrawlLib.Internal.IO.SourceWriteGuard.Initialize(Path.Combine(directory,"storage","source-roots.txt"));var p=CharacterProject.Load(Path.Combine(directory,"workspace-one.fpcharacter"),Path.Combine(directory,"storage"));CheckWorkspaceGraph(p,File.ReadAllText(Path.Combine(directory,"workspace-target.txt")));p.ExportInstance(File.ReadAllText(Path.Combine(directory,"workspace-instance.txt")),Path.Combine(directory,"fresh-workspace-output.brres"));VerifyWorkspaceNative(Path.Combine(directory,"fresh-workspace-output.brres"));Assert(Hash(Path.Combine(directory,"workspace-two.fpcharacter"))==File.ReadAllText(Path.Combine(directory,"workspace-other.sha256")),"other project unchanged after fresh reload");Console.WriteLine("PASS: fresh process restores exact slots/parts/anchors and genuine native motion independently");return 0;}catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
