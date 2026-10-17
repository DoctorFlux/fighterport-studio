// Original FighterPort library/project acceptance, 2026-10-17. No game data.
using BrawlCrate.Authoring;
using BrawlCrate.NodeWrappers;
using BrawlCrate.Sources;
using BrawlCrate.UI;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlLib.Internal;
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
    private static string LibraryResolution(string source, string name)
    {
        var workspace = new CompanionWorkspace(Path.Combine(_output,"storage")); workspace.AddSource(source,CancellationToken.None);
        workspace.SelectModel(workspace.Resources.Single(r => r.Kind == "MDL0"));
        workspace.SelectMotion(workspace.Resources.Single(r => r.Kind == "CHR0"));
        workspace.SelectTexture("OriginalQuadrants",workspace.Resources.Single(r => r.Kind == "TEX0"));
        string path = Path.Combine(_output,name + ".fpcompanion"); workspace.Save(path); return path;
    }
    private static void ExerciseLibrary(bool gui)
    {
        string a = OriginalFixture.Create(Path.Combine(_output,"rig-a"));
        string b = OriginalFixture.Create(Path.Combine(_output,"rig-b"),true,true);
        var originals = Directory.GetFiles(Path.GetDirectoryName(a)).Concat(Directory.GetFiles(Path.GetDirectoryName(b))).ToDictionary(p=>p,Hash);
        File.WriteAllLines(Path.Combine(_output,"library-originals-before.sha256"),originals.Select(p=>p.Value+"\t"+p.Key));
        string ra = LibraryResolution(a,"rig-a"), rb = LibraryResolution(b,"rig-b");
        var project = new CharacterProject(Path.Combine(_output,"storage"));
        string aid = project.RegisterSource(ra,"Duplicate character"), bid = project.RegisterSource(rb,"Duplicate character");
        var state = project.State; var ca = state.Characters.Single(c=>c.Id==aid); var cb = state.Characters.Single(c=>c.Id==bid);
        var aa = state.Animations.Single(n=>n.SourceCharacterId==aid); var ab = state.Animations.Single(n=>n.SourceCharacterId==bid);
        Assert(ca.Bones.Count==2 && cb.Bones.Count==3 && ca.RigFingerprint!=cb.RigFingerprint,"genuinely different rigs, not name compatibility");
        Assert(cb.Bones.Single(n=>n.Name=="OriginalTipB").Parent==cb.Bones.FindIndex(n=>n.Name=="Paddle"),"rig B rotated extra descendant hierarchy");
        Assert(ca.Bones.Single(n=>n.Name=="Paddle").Bind!=cb.Bones.Single(n=>n.Name=="Paddle").Bind,"same misleading names retain different binds");
        Assert(ca.ModelPayloadHash!=cb.ModelPayloadHash && ca.Originals.Single().Hash==Hash(a),"exact model and original native evidence");
        Assert(state.Moves.Count==0,"animation-only registration fabricates no attack data");
        Expect<IOException>(()=>project.Assign(aa.Id,bid,"Rejected"),"different-rig assignment must reject");
        Expect<OperationCanceledException>(()=>project.RegisterSource(ra,"Cancelled",new CancellationToken(true)),"cancelled registration");
        string move = project.CreateDraft(aa.Id,"Original manual draft; no imported hitbox",7,3,18,9.5f);
        string first = project.Assign(aa.Id,aid,"ForwardAerialDraft",move);
        string second = project.Assign(aa.Id,aid,"SecondIndependentSlot");
        string third = project.Assign(ab.Id,bid,"RigBPreview");
        string immutableLibrary = project.State.Characters.Single(c=>c.Id==aid).NativeHash, secondHash = project.State.Instances.Single(i=>i.Id==second).WorkingHash;
        project.SetSlot(first,"EditedSlot"); project.Undo(); Assert(project.State.Instances.Single(i=>i.Id==first).Slot=="ForwardAerialDraft","slot undo");
        project.Redo(); Assert(project.State.Instances.Single(i=>i.Id==first).Slot=="EditedSlot","slot redo");
        var lease = project.OpenInstance(first);
        using (var root = NodeFactory.FromFile(null,lease.Path))
        {
            var entry = Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single(); entry.SetKeyframe(5,15,35);
            project.CaptureNativeEdits(lease,root,lease.Path);
            entry.SetKeyframe(5,15,45); project.CaptureNativeEdits(lease,root,lease.Path);
        }
        project.Undo(); VerifyProjectInstance(project,first,35,false,"continued-capture-undo.brres");
        project.Redo(); VerifyProjectInstance(project,first,45,false,"continued-capture-redo.brres"); project.Undo();
        project.Undo(); VerifyProjectInstance(project,first,60,false,"undo.brres");
        project.Redo(); VerifyProjectInstance(project,first,35,false,"redo.brres");
        string beforeRejectedCapture=project.State.Instances.Single(i=>i.Id==first).WorkingHash;
        var rejectedLease = project.OpenInstance(first);
        using (var changed = NodeFactory.FromFile(null,rejectedLease.Path))
        {
            var changedModel=Find<MDL0Node>(changed); changedModel.Populate();
            var bone=changedModel.BoneCache.OfType<MDL0BoneNode>().Single(n=>n.Name=="Paddle");
            var bind=bone.BindState; bind._translate._y+=2; bone.BindState=bind; ((MDL0BoneNode)bone.Parent).RecalcBindState(false,false);
            Expect<IOException>(()=>project.CaptureNativeEdits(rejectedLease,changed,rejectedLease.Path),"unreviewed rig edits cannot be captured as motion-only edits");
        }
        Assert(project.State.Instances.Single(i=>i.Id==first).WorkingHash==beforeRejectedCapture,"failed capture preserves current revision");
        Assert(project.State.Instances.Single(i=>i.Id==second).WorkingHash==secondHash && project.State.Characters.Single(c=>c.Id==aid).NativeHash==immutableLibrary,"source/library and second instance immutable");
        Expect<IOException>(()=>project.ExportInstance(first,a),"project native output cannot overwrite source");
        Expect<IOException>(()=>project.ExportInstance(first,Path.Combine(_output,"wrong.pac")),"cannot flatten/change enclosing native format");
        var stale = project.OpenInstance(first); project.SetSlot(first,"FinalSlot");
        using (var root = NodeFactory.FromFile(null,stale.Path)) Expect<IOException>(()=>project.CaptureNativeEdits(stale,root,stale.Path),"stale transaction rejects even matching native names");
        string file = Path.Combine(_output,"library.fpcharacter"); project.Save(file);
        var detached = project.State; detached.Characters.Clear(); Assert(project.State.Characters.Count==2,"external records are detached snapshots");
        var loaded = CharacterProject.Load(file,Path.Combine(_output,"storage"));
        Assert(!loaded.CanUndo && !loaded.CanRedo && !loaded.Dirty,"documented session-only history resets on load");
        VerifyProjectInstance(loaded,first,35,false,"reloaded.brres"); VerifyProjectInstance(loaded,second,60,false,"second.brres"); VerifyProjectInstance(loaded,third,60,true,"rig-b.brres");
        var history=CharacterProject.Load(file,Path.Combine(_output,"storage"));
        for(int i=1;i<=40;i++) history.SetSlot(first,"Budget"+i);
        int commands=0; while(history.CanUndo) {history.Undo();commands++;}
        Assert(commands==32 && history.State.Instances.Single(i=>i.Id==first).Slot=="Budget8","bounded session history retains the latest 32 commands");
        commands=0; while(history.CanRedo) {history.Redo();commands++;}
        Assert(commands==32 && history.State.Instances.Single(i=>i.Id==first).Slot=="Budget40","bounded redo restores the final state");
        var doc = new XmlDocument { XmlResolver=null }; doc.Load(file);
        XmlNode characters = doc.SelectSingleNode("//State/Characters"); characters.PrependChild(characters.LastChild);
        string reordered = Path.Combine(_output,"reordered.fpcharacter"); doc.Save(reordered);
        var reorderedProject = CharacterProject.Load(reordered,Path.Combine(_output,"storage"));
        Assert(reorderedProject.State.Animations.Single(n=>n.Id==aa.Id).SourceCharacterId==aid && reorderedProject.State.Characters[0].Id==bid,"reordering and duplicate display names cannot rebind clips");
        doc.Load(file); doc.SelectSingleNode("//State/Characters/SourceCharacterProfile/RigFingerprint").InnerText="changed";
        string invalid = Path.Combine(_output,"invalid.fpcharacter"); doc.Save(invalid);
        Expect<IOException>(()=>CharacterProject.Load(invalid,Path.Combine(_output,"storage")),"changed rig snapshot/fingerprint rejects");
        doc.Load(file); var data=doc.SelectSingleNode("//Blobs/ProjectBlob/Data"); byte[] blob=Convert.FromBase64String(data.InnerText); blob[blob.Length-1]^=1; data.InnerText=Convert.ToBase64String(blob); doc.Save(invalid);
        Expect<IOException>(()=>CharacterProject.Load(invalid,Path.Combine(_output,"storage")),"tampered native/project blob rejects");
        File.WriteAllText(invalid,"<!DOCTYPE x [<!ENTITY e SYSTEM 'file:///C:/Windows/win.ini'>]><CharacterProjectFile>&e;</CharacterProjectFile>");
        Expect<IOException>(()=>CharacterProject.Load(invalid,Path.Combine(_output,"storage")),"project entities prohibited");
        CheckLibraryChangedInputs(a);
        Log("PASS: stable source-bound library, two different original rigs, manual draft, independent slots, native key transaction/undo/redo/save/load, reordered identities and stale/source/schema rejection");
        if (gui) ExerciseLibraryGui(loaded,first,aid);
        using (var child = Process.Start(new ProcessStartInfo { FileName=Process.GetCurrentProcess().MainModule.FileName, Arguments="--verify-library \""+_output+"\"", UseShellExecute=false, CreateNoWindow=true, WindowStyle=ProcessWindowStyle.Hidden, RedirectStandardOutput=true, RedirectStandardError=true }))
        {
            if (!child.WaitForExit(25000)) { child.Kill(); throw new TimeoutException("Project independent restart"); }
            Assert(child.ExitCode==0,"project child: "+child.StandardError.ReadToEnd()); Log(child.StandardOutput.ReadToEnd().Trim());
        }
        foreach (var p in originals) Assert(Hash(p.Key)==p.Value,"all original rig source hashes unchanged");
        File.WriteAllLines(Path.Combine(_output,"library-originals.sha256"),originals.Select(p=>p.Value+"\t"+p.Key));
        Log("PASS: all "+originals.Count+" original rig files unchanged after native authoring and restart");
    }
    private static void CheckLibraryChangedInputs(string original)
    {
        string folder=Path.Combine(_output,"negative-source-copy"); Directory.CreateDirectory(folder); string copy=Path.Combine(folder,"copy.brres"); File.Copy(original,copy);
        string resolution=LibraryResolution(copy,"negative-resolution"); var project=new CharacterProject(Path.Combine(_output,"storage"));
        string id=project.RegisterSource(resolution,"Negative copy"); string animation=project.State.Animations.Single().Id; string instance=project.Assign(animation,id,"NegativeSlot");
        string file=Path.Combine(_output,"negative.fpcharacter"); project.Save(file);
        File.Move(copy,copy+".missing");
        var offline=CharacterProject.Load(file,Path.Combine(_output,"storage")); Assert(offline.SourceProblems().Count>0,"missing source visible with recoverable saved state");
        Expect<IOException>(()=>offline.ExportInstance(instance,Path.Combine(_output,"missing-output.brres")),"missing originals cannot silently export");
        File.Move(copy+".missing",copy); var bytes=File.ReadAllBytes(copy); bytes[bytes.Length-1]^=1; File.WriteAllBytes(copy,bytes);
        Assert(offline.SourceProblems().Any(p=>p.Contains("Changed source")),"changed input invalidates assumptions visibly");
        Expect<IOException>(()=>offline.Assign(animation,id,"ChangedSlot"),"changed inputs cannot assign");
    }
    private static void VerifyProjectInstance(CharacterProject project,string instance,float angle,bool rigB,string outputName)
    {
        string output=Path.Combine(_output,outputName); project.ExportInstance(instance,output);
        VerifyLibraryNative(output,angle,rigB);
    }
    private static void VerifyLibraryNative(string file,float angle,bool rigB)
    {
        using (var root=NodeFactory.FromFile(null,file))
        {
            var model=Find<MDL0Node>(root); model.Populate(); var motion=Find<CHR0Node>(root); var entry=motion.Children.OfType<CHR0EntryNode>().Single();
            Assert(model.BoneCache.Length==(rigB?3:2) && motion.FrameCount==31 && motion.Loop,"native rig count/timing/loop survived project output");
            Assert(Math.Abs(entry.GetKeyframe(5,15)._value-angle)<.001,"native edited key decoded");
            model.ApplyCHR(motion,16);
            foreach (var vertex in ((MDL0ObjectNode)model._objList.Single()).Vertices)
            {
                float x=vertex.Position._x,y=vertex.Position._y,w=rigB?(y<1?.25f:.75f):1,pivot=rigB?6:0;
                double radians=angle*Math.PI/180,rx=Math.Cos(radians)*x-Math.Sin(radians)*(y-pivot),ry=pivot+Math.Sin(radians)*x+Math.Cos(radians)*(y-pivot);
                var actual=vertex.WeightedPosition;
                Assert(Math.Abs(actual._x-((1-w)*x+w*rx))<.002 && Math.Abs(actual._y-((1-w)*y+w*ry))<.002,"decoded output positions meet independent intended native pose (0.002 units)");
            }
            if (rigB)
            {
                var tip=model.BoneCache.OfType<MDL0BoneNode>().Single(n=>n.Name=="OriginalTipB");
                Assert(tip.Parent.Name=="Paddle" && Math.Abs(tip.BindState._rotate._z-90)<.001 && Math.Abs(tip.BindState._translate._y-9)<.001,"rig B helper axis/hierarchy/bind");
            }
            using (var image=Find<TEX0Node>(root).GetImage(0)) Assert(image.GetPixel(3,3).ToArgb()==Color.Red.ToArgb() && image.GetPixel(24,24).ToArgb()==Color.Yellow.ToArgb(),"native texture retained");
        }
    }
    private static int VerifyLibraryRestart(string directory)
    {
        try
        {
            _output=directory; Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT",Path.Combine(directory,"settings"));
            BrawlLib.Internal.IO.SourceWriteGuard.Initialize(Path.Combine(directory,"storage","source-roots.txt"));
            string file=Path.Combine(directory,File.Exists(Path.Combine(directory,"library-ui.fpcharacter"))?"library-ui.fpcharacter":"library.fpcharacter");
            var project=CharacterProject.Load(file,Path.Combine(directory,"storage")); var state=project.State;
            var edited=state.Instances.Single(i=>i.Slot=="FinalSlot");
            VerifyProjectInstance(project,edited.Id,File.Exists(Path.Combine(directory,"library-ui.fpcharacter"))?25:35,false,"child-output.brres");
            VerifyProjectInstance(project,state.Instances.Single(i=>i.Slot=="SecondIndependentSlot").Id,60,false,"child-second.brres");
            VerifyProjectInstance(project,state.Instances.Single(i=>i.Slot=="RigBPreview").Id,60,true,"child-rig-b.brres");
            VerifyLibraryNative(project.PreviewSource(edited.AnimationId),60,false);
            Assert(state.Moves.Single().Damage==9.5f && state.Moves.Single().Startup==7,"actual manual draft data persisted");
            Assert(state.Characters.Count==2 && state.Instances.Count==3 && !project.CanUndo,"stable project instances and explicit undo policy after restart");
            foreach (var line in File.ReadAllLines(Path.Combine(directory,"library-originals-before.sha256"))) { var parts=line.Split('\t'); Assert(Hash(parts[1])==parts[0],"restart originals unchanged"); }
            Console.WriteLine("PASS: independent process restored actual edited native key, source bindings, draft data and independent instances; regenerated and decoded native output"); return 0;
        }
        catch(Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    private static void ExerciseLibraryGui(CharacterProject project,string instance,string sourceId)
    {
        var main=MainForm.Instance; main.StartPosition=FormStartPosition.Manual; main.Location=new Point(20,20); main.Size=new Size(1080,760);
        Exception failure=null;
        main.Shown+=(s,e)=>main.BeginInvoke(new Action(()=> {
            try
            {
                using(var form=new CharacterLibraryForm(project))
                {
                    form.Show(main); Pump(100);
                    var tree=(TreeView)form.Controls.Find("characterLibrary",true).Single();
                    string clip=project.State.Animations.Single(a=>a.SourceCharacterId==sourceId).Id;
                    string otherClip=project.State.Animations.Single(a=>a.SourceCharacterId!=sourceId).Id;
                    tree.SelectedNode=tree.Nodes.Cast<TreeNode>().SelectMany(n=>n.Nodes.Cast<TreeNode>()).Single(n=>(string)n.Tag==otherClip);
                    LibraryButton(form,"libraryPreview");
                    var otherEditor=ModelEditControl.Instances.Single(); PrepareLibraryEditor(otherEditor); otherEditor.SetFrame(16); Pump(150);
                    Assert(Find<MDL0Node>((ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null,null)).BoneCache.Length==3,"real preview uses rig B's own source binding");
                    CaptureViewport(otherEditor,"09-library-rig-b-native.png");
                    tree.SelectedNode=tree.Nodes.Cast<TreeNode>().SelectMany(n=>n.Nodes.Cast<TreeNode>()).Single(n=>(string)n.Tag==clip);
                    ((ComboBox)form.Controls.Find("libraryTarget",true).Single()).SelectedIndex=0;
                    ((TextBox)form.Controls.Find("librarySlot",true).Single()).Text="UI assignment draft";
                    LibraryButton(form,"libraryAssign"); Assert(project.State.Instances.Count==4,"real UI slot assignment");
                    LibraryButton(form,"libraryUndo"); Assert(project.State.Instances.Count==3,"real UI assignment undo");
                    LibraryButton(form,"libraryRedo"); Assert(project.State.Instances.Count==4,"real UI assignment redo"); LibraryButton(form,"libraryUndo");
                    var list=(ListView)form.Controls.Find("libraryInstances",true).Single(); list.Items.Cast<ListViewItem>().Single(i=>(string)i.Tag==instance).Selected=true;
                    CaptureWindow(form,"10-source-library.png");
                    LibraryButton(form,"libraryEdit");
                    var root=(ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null,null); var model=Find<MDL0Node>(root); var motion=Find<CHR0Node>(root);
                    var editor=ModelEditControl.Instances.Single(); PrepareLibraryEditor(editor); var viewer=editor.FindForm();
                    editor.SelectedCHR0=motion; editor.SetFrame(16); editor.SelectedBone=model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name=="Paddle"); Pump(130);
                    CaptureViewport(editor,"11-library-native-preview.png");
                    editor.SetFrame(1); int played=0;
                    using(var timer=new System.Windows.Forms.Timer{Interval=250}) { timer.Tick+=(a,b)=>{played=editor.CurrentFrame;editor.StopAnim();timer.Stop();};timer.Start();editor.PlaybackPanel.btnPlay.PerformClick();WaitFor(()=>played!=0,"library native playback"); }
                    Assert(played>1,"library native playback advanced");
                    editor.SetFrame(16); editor.CHR0Editor.numRotZ.Value=25; editor.CHR0Editor.BoxChangedCreateUndo(editor.CHR0Editor.numRotZ,EventArgs.Empty); Pump(100);
                    Assert(Math.Abs(motion.Children.OfType<CHR0EntryNode>().Single().GetKeyframe(5,15)._value-25)<.001,"real native numeric edit");
                    CaptureViewport(editor,"12-library-native-edited.png");
                    LibraryButton(form,"libraryCapture"); Assert(project.State.Instances.Single(i=>i.Id==instance).WorkingHash!=project.State.Characters.Single(c=>c.Id==sourceId).NativeHash,"actual native edit captured");
                    LibraryButton(form,"libraryUndo"); AssertCurrentLibraryKey(35); LibraryButton(form,"libraryRedo"); AssertCurrentLibraryKey(25);
                    form.SaveProject(Path.Combine(_output,"library-ui.fpcharacter")); CaptureWindow(form,"13-library-persisted.png");
                    string output=Path.Combine(_output,"library-ui-native.brres");
                    ((TextBox)form.Controls.Find("libraryOutputPath",true).Single()).Text=output; LibraryButton(form,"libraryExport"); VerifyLibraryNative(output,25,false);
                    Assert((bool)CallProgram("Open",new[]{typeof(string),typeof(bool)},output,false),"native reopen project output"); AssertCurrentLibraryKey(25);
                    CaptureWindow(main,"14-library-native-reopened.png");
                    CallProgram("Close",Type.EmptyTypes);
                }
                Log("PASS: actual library controls -> native preview/Play/numeric edit -> capture -> project undo/redo -> save -> native output/reopen; pickers bypassed");
            }
            catch(Exception error) { failure=error; }
            finally { main.Dispose();Application.ExitThread(); }
        }));
        Application.Run(main); if(failure!=null) throw new InvalidOperationException("Library GUI",failure);
    }
    private static void LibraryButton(CharacterLibraryForm form,string name)
    {
        ((Button)form.Controls.Find(name,true).Single()).PerformClick(); Pump(70);
        string text=((Label)form.Controls.Find("libraryStatus",true).Single()).Text; Assert(!text.StartsWith("Stopped:"),name+": "+text);
    }
    private static void PrepareLibraryEditor(ModelEditControl editor)
    {
        var viewer=editor.FindForm(); viewer.Size=new Size(1180,820); viewer.Location=new Point(30,30);
        foreach(string side in new[]{"Top","Left","Right","Bottom"})
        {
            var menu=(ToolStripMenuItem)typeof(ModelEditControl).GetField("show"+side,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(editor);
            if(!menu.Checked) ((Button)editor.Controls.Find("btn"+side+"Toggle",true).Single()).PerformClick();
        }
        // The product frames the bound model; do not repair its camera in this suite.
        Pump(100);
    }
    private static void AssertCurrentLibraryKey(float value)
    {
        var root=(ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null,null);
        Assert(Math.Abs(Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single().GetKeyframe(5,15)._value-value)<.001,"project undo/redo/reopen updates actual native document");
    }
}
