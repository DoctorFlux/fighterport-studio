// Original FighterPort nested-package and blended-skin fixtures, 2026-10-17.
// Same original-artwork permission as OriginalFixture.cs; no game data.
using BrawlCrate.Sources;
using BrawlCrate.Authoring;
using BrawlCrate.UI;
using BrawlLib.Internal;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

internal static partial class NativeSmoke
{
    private static void ArcChild(ARCNode parent, ARCEntryNode node, ARCFileType type, short index, byte group)
    { parent.AddChild(node); node.FileType = type; node.FileIndex = index; node.GroupID = group; }
    private static string CreateContainerFixture()
    {
        string complete = OriginalFixture.Create(Path.Combine(_output, "blended-build"), true);
        string[] companions = CreateCompanionFixtures();
        string inputs = Path.Combine(_output, "package-inputs"); Directory.CreateDirectory(inputs);
        string output = Path.Combine(inputs, "original-character.pac");
        using (var package = new ARCNode { Name = "OriginalNestedCharacter" })
        {
            var branch = new ARCNode { Name = "OriginalModelBranch" }; ArcChild(package, branch, ARCFileType.GroupedArchive, 7, 2);
            var model = (BRRESNode)NodeFactory.FromFile(null, complete);
            var texture = Find<TEX0Node>(model); texture.Remove(); texture.Dispose();
            var motion = Find<CHR0Node>(model); motion.Remove();
            var retained = model.CreateResource<SRT0Node>("PreservedBaseChannel"); retained.Version = 4; retained.FrameCount = 17; retained.Loop = false;
            ArcChild(branch, model, ARCFileType.ModelData, 3, 1);
            var dependencies = new ARCNode { Name = "OriginalDependencyBranch" }; ArcChild(package, dependencies, ARCFileType.GroupedArchive, 9, 4);
            ArcChild(dependencies, (BRRESNode)NodeFactory.FromFile(null, companions[2]), ARCFileType.TextureData, 12, 5);
            var motions = new BRRESNode { Name = "OriginalMotionSource" }; motions.GetOrCreateFolder<CHR0Node>().AddChild(motion);
            ArcChild(dependencies, motions, ARCFileType.AnimationData, 14, 6);
            // Same texture name, different pixels, in an unrelated native container.
            ArcChild(package, (BRRESNode)NodeFactory.FromFile(null, companions[1]), ARCFileType.TextureData, 18, 8);
            package.Export(output);
        }
        BrawlContainerAudit.Inspect(output).RequirePreservable(); return output;
    }
    private static CompanionWorkspace ResolveContainer(string source)
    {
        var workspace = new CompanionWorkspace(Path.Combine(_output, "storage")); workspace.AddSource(source, CancellationToken.None);
        workspace.SelectModel(workspace.Resources.Single(r => r.Kind == "MDL0"));
        Assert(workspace.TextureCandidates("OriginalQuadrants").Count == 2, "same names in distinct nested containers remain explicit candidates");
        Assert(workspace.TextureStatus("OriginalQuadrants").StartsWith("Ambiguous"), "no global name-based container choice");
        workspace.SelectTexture("OriginalQuadrants", workspace.Resources.Single(r => r.Kind == "TEX0" && r.ContainerPath == "1/0"));
        workspace.SelectMotion(workspace.Resources.Single(r => r.Kind == "CHR0"));
        Assert(workspace.Model.ContainerPath == "0/0" && workspace.Motion.ContainerPath == "1/1", "exact nested container identities");
        Assert(workspace.Validate().Count == 0, "container companions explicitly resolved"); return workspace;
    }
    private static void ExerciseContainers(bool gui)
    {
        string source = CreateContainerFixture();
        var originals = new[] { source }.Concat(Directory.GetFiles(Path.Combine(_output,"inputs"),"*",SearchOption.AllDirectories))
            .Concat(Directory.GetFiles(Path.Combine(_output,"blended-build"))).Concat(Directory.GetFiles(Path.Combine(_output,"fixture-build"))).ToDictionary(p => p,Hash);
        File.WriteAllLines(Path.Combine(_output,"container-originals.sha256"),originals.Select(p => p.Value+"\t"+p.Key));
        string sourceHash = Hash(source); File.WriteAllText(Path.Combine(_output, "container-source.sha256"), sourceHash);
        var workspace = ResolveContainer(source);
        string resolution = Path.Combine(_output, "container.fpcompanion"); workspace.Save(resolution);
        string assembled = workspace.Assemble(CancellationToken.None);
        Assert(Path.GetExtension(assembled) == ".pac", "native enclosing PAC output, never a flattened BRRES");
        VerifyContainerNative(assembled, 60); BrawlContainerAudit.VerifyPreserved(source, assembled, "0/0");
        Log("PASS: nested ARC/PAC -> selected BRRES assembly retains entry identities/hierarchy and exact unrelated payload bytes");
        string edited = Path.Combine(_output, "edited-native.pac");
        using (var root = NodeFactory.FromFile(null, assembled))
        { Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single().SetKeyframe(5, 15, 35); root.Export(edited); }
        VerifyContainerNative(edited, 35); BrawlContainerAudit.VerifyPreserved(source, edited, "0/0");
        // The new project workflow must preserve the enclosing native package too.
        var project = new CharacterProject(Path.Combine(_output,"storage"));
        string character = project.RegisterSource(resolution,"Original nested package");
        string instance = project.Assign(project.State.Animations.Single().Id,character,"NativePackageDraft");
        var lease = project.OpenInstance(instance);
        using (var root = NodeFactory.FromFile(null,lease.Path))
        {
            Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single().SetKeyframe(5,15,35);
            project.CaptureNativeEdits(lease,root,lease.Path);
        }
        string projectPath=Path.Combine(_output,"container-project.fpcharacter"); project.Save(projectPath);
        var restored=CharacterProject.Load(projectPath,Path.Combine(_output,"storage"));
        string projectNative=Path.Combine(_output,"container-project-edited.pac"); restored.ExportInstance(instance,projectNative);
        VerifyContainerNative(projectNative,35); BrawlContainerAudit.VerifyPreserved(source,projectNative,"0/0");
        Log("PASS: actual-edit project retains nested PAC and unrelated payloads through capture/save/load/native export/decode");
        CheckContainerFailures(source, resolution);
        using (var child = Process.Start(new ProcessStartInfo { FileName = Process.GetCurrentProcess().MainModule.FileName,
            Arguments = "--verify-container \"" + _output + "\"", UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true }))
        {
            if (!child.WaitForExit(30000)) { child.Kill(); throw new TimeoutException("Container resolution restart"); }
            Assert(child.ExitCode == 0, "container restart: " + child.StandardError.ReadToEnd()); Log(child.StandardOutput.ReadToEnd().Trim());
        }
        if (gui)
        {
            var main = MainForm.Instance; main.StartPosition = FormStartPosition.Manual; main.Location = new Point(25,25); main.Size = new Size(1050,760);
            Exception failure = null;
            main.Shown += (sender,args) => main.BeginInvoke(new Action(() =>
            {
                try
                {
                    var fresh = new CompanionWorkspace(Path.Combine(_output,"storage"));
                    using (var form = new CompanionWorkspaceForm(fresh))
                    {
                        form.Show(main); Task added = form.AddSourceAsync(source); WaitFor(() => added.IsCompleted, "native container inventory"); added.GetAwaiter().GetResult();
                        ((ComboBox)form.Controls.Find("companionModel",true).Single()).SelectedIndex = 0;
                        var bindings = (ListView)form.Controls.Find("companionBindings",true).Single(); bindings.Items[0].Selected = true; Pump(30);
                        ((ComboBox)form.Controls.Find("companionTexture",true).Single()).SelectedItem = fresh.Resources.Single(r => r.Kind == "TEX0" && r.ContainerPath == "1/0");
                        ((ComboBox)form.Controls.Find("companionMotion",true).Single()).SelectedIndex = 0;
                        form.SaveResolution(Path.Combine(_output,"container-ui.fpcompanion")); CaptureWindow(form,"00-container-choices.png");
                    }
                    ExerciseGuiContents(main,source,null,CompanionWorkspace.Load(Path.Combine(_output,"container-ui.fpcompanion"),Path.Combine(_output,"storage")),VerifyContainerNative);
                    BrawlContainerAudit.VerifyPreserved(source,Path.Combine(_output,"edited-gui.pac"),"0/0");
                }
                catch (Exception error) { failure = error; }
                finally { main.Dispose(); Application.ExitThread(); }
            }));
            Application.Run(main); if (failure != null) throw new InvalidOperationException("Container GUI exercise failed",failure);
        }
        Assert(Hash(source) == sourceHash, "original container SHA256 unchanged");
        foreach(var pair in originals) Assert(Hash(pair.Key)==pair.Value,"all original container/fixture source hashes unchanged");
        Log("PASS: all "+originals.Count+" original source SHA256 values unchanged");
        Log("PASS: original blended skinning/nonidentity bind/decoded target poses, native PAC edit/save/reopen, source overwrite rejection and unchanged source hash");
    }
    private static void VerifyContainerNative(string path, float angle)
    {
        using (var root = NodeFactory.FromFile(null,path))
        {
            Assert(root is ARCNode && root.Children.Count == 3 && root.Children[0] is ARCNode, "actual nested ARC/PAC hierarchy");
            var baseBrres = root.Children[0].Children[0]; var model = Find<MDL0Node>(baseBrres); model.Populate();
            var mesh = (MDL0ObjectNode)model._objList.Single();
            Assert(model.Version == 11 && model.BoneCache.Length == 2 && mesh.VertexCount == 4, "blended native model metadata");
            foreach(float x in new[]{-3f,3f}) foreach(float y in new[]{0f,18f})
                Assert(mesh.Vertices.Any(v=>Math.Abs(v.Position._x-x)<.0001 && Math.Abs(v.Position._y-y)<.0001 && Math.Abs(v.Position._z)<.0001),"independent expected original geometry retained");
            Assert(model.BoneCache.OfType<MDL0BoneNode>().Single(b=>b.Name=="Root").BindMatrix==Matrix.Identity,"original root bind identity");
            Assert(model._matList.Count==1 && model._matList[0].Children.OfType<MDL0MaterialRefNode>().Single().Texture=="OriginalQuadrants","original native material binding");
            var paddle = model.BoneCache.OfType<MDL0BoneNode>().Single(b => b.Name == "Paddle");
            Assert(paddle.Parent is MDL0BoneNode parent && parent.Name=="Root","original native bone hierarchy");
            Assert(Math.Abs((paddle.BindMatrix * new Vector3())._y - 6) < .0001 && Math.Abs((paddle.InverseBindMatrix * new Vector3())._y + 6) < .0001, "nonidentity bind and inverse bind retained");
            var animation = Find<CHR0Node>(baseBrres);
            Assert(animation.Version == 4 && animation.FrameCount == 31 && animation.Loop && Math.Abs(animation.Children.OfType<CHR0EntryNode>().Single().GetKeyframe(5,15)._value-angle)<.001, "decoded selected native motion/key/timing/loop");
            using (var image = Find<TEX0Node>(baseBrres).GetImage(0))
                for(int y=0;y<32;y++) for(int x=0;x<32;x++)
                    Assert(image.GetPixel(x,y).ToArgb()==(x<16?(y<16?Color.Red:Color.Blue):(y<16?Color.Lime:Color.Yellow)).ToArgb(),"all selected original RGBA8 pixels");
            model.ApplyCHR(animation,16);
            foreach (var vertex in mesh.Vertices)
            {
                float x = vertex.Position._x, y = vertex.Position._y, w = y < 1 ? .25f : .75f;
                Assert(vertex.MatrixNode.Weights.Count == 2 && Math.Abs(vertex.MatrixNode.Weights.Single(b => b.Bone.Name == "Paddle").Weight-w)<.0001, "both original blended weights retained");
                double radians = angle*Math.PI/180, rx = Math.Cos(radians)*x - Math.Sin(radians)*(y-6), ry = 6+Math.Sin(radians)*x + Math.Cos(radians)*(y-6);
                var actual = vertex.WeightedPosition;
                Assert(Math.Abs(actual._x-((1-w)*x+w*rx)) < .002 && Math.Abs(actual._y-((1-w)*y+w*ry)) < .002 && Math.Abs(actual._z)<.002,
                    "decoded blended vertex pose matches independent weighted rotation about bind pivot");
            }
        }
    }
    private static void CheckContainerFailures(string source,string resolution)
    {
        string storage = Path.Combine(_output,"storage");
        Expect<OperationCanceledException>(() => new CompanionWorkspace(storage).AddSource(source,new CancellationToken(true)),"cancelled container inventory");
        var doc = new XmlDocument { XmlResolver = null }; doc.Load(resolution);
        ((XmlElement)doc.DocumentElement.SelectSingleNode("choice")).SetAttribute("containerIdentity","wrong-container");
        string wrong = Path.Combine(_output,"wrong-container.fpcompanion"); doc.Save(wrong);
        Expect<IOException>(() => CompanionWorkspace.Load(wrong,storage),"changed native enclosing container identity rejected");
        string negatives = Path.Combine(_output,"negative-package-copies"); Directory.CreateDirectory(negatives);
        byte[] bytes = File.ReadAllBytes(source); bytes[8] = 1;
        string reserved = Path.Combine(negatives,"reserved.pac"); File.WriteAllBytes(reserved,bytes);
        Expect<IOException>(() => new CompanionWorkspace(storage).AddSource(reserved,CancellationToken.None),"unrepresented header bytes must not be silently discarded");
        bytes = File.ReadAllBytes(source); bytes[68]=0x7f; bytes[69]=0xff;
        string corrupt = Path.Combine(negatives,"bounds.pac"); File.WriteAllBytes(corrupt,bytes);
        Expect<IOException>(() => new CompanionWorkspace(storage).AddSource(corrupt,CancellationToken.None),"container entry bounds rejected");
        bytes = new byte[64]; bytes[0] = 0x11; File.WriteAllBytes(Path.Combine(negatives,"unsupported.pcs"),bytes);
        Expect<IOException>(() => new CompanionWorkspace(storage).AddSource(Path.Combine(negatives,"unsupported.pcs"),CancellationToken.None),"compressed PCS remains explicitly unsupported");
        bytes = File.ReadAllBytes(source); bytes[bytes.Length-1] ^= 1;
        string changed = Path.Combine(negatives,"changed.pac"); File.WriteAllBytes(changed,bytes);
        doc.Load(resolution); ((XmlElement)doc.DocumentElement.SelectSingleNode("source")).SetAttribute("path",changed); doc.Save(wrong);
        Expect<IOException>(() => CompanionWorkspace.Load(wrong,storage),"changed container source hash rejected");
        bytes = File.ReadAllBytes(source);
        var untouched = BrawlContainerAudit.Inspect(source).Records.Single(r => r.Tag=="TEX0" && r.Path.StartsWith("2/"));
        bytes[(int)untouched.Offset+0x40] ^= 1; File.WriteAllBytes(changed,bytes);
        Expect<IOException>(() => BrawlContainerAudit.VerifyPreserved(source,changed,"0/0"),"unrelated decoded-container payload alteration detected");
        Expect<IOException>(() => BrawlContainerAudit.VerifyPreserved(source,source,""),"ARC cannot masquerade as selected BRRES preservation scope");
        string depthFolder=Path.Combine(_output,"depth-negative-copy"); Directory.CreateDirectory(depthFolder);
        string deep = Path.Combine(depthFolder,"deep.pac");
        using(var arc = new ARCNode {Name="OriginalDepthLimit"})
        {
            var parent=arc;
            for(int i=0;i<17;i++) {var child=new ARCNode {Name="OriginalDepth"+i}; ArcChild(parent,child,ARCFileType.GroupedArchive,(short)i,0); parent=child;}
            ArcChild(parent,(BRRESNode)NodeFactory.FromFile(null,Path.Combine(_output,"inputs","textures","chosen-texture.brres")),ARCFileType.TextureData,0,0);
            arc.Export(deep);
        }
        Expect<IOException>(()=>new CompanionWorkspace(storage).AddSource(deep,CancellationToken.None),"container depth limit enforced before decoding");
        var collision = new CompanionWorkspace(storage);
        string complete = Path.Combine(_output,"blended-build","original-paddle.brres");
        collision.AddSource(complete,CancellationToken.None); collision.AddSource(source,CancellationToken.None);
        collision.SelectModel(collision.Resources.Single(r=>r.Kind=="MDL0" && r.SourcePath==complete));
        collision.SelectMotion(collision.Resources.Single(r=>r.Kind=="CHR0" && r.SourcePath==complete));
        collision.SelectTexture("OriginalQuadrants",collision.Resources.Single(r=>r.Kind=="TEX0" && r.SourcePath==source && r.ContainerPath=="1/0"));
        Expect<IOException>(()=>collision.Assemble(CancellationToken.None),"container override never silently replaces an embedded base resource");
        using (var root = NodeFactory.FromFile(null,source)) Expect<IOException>(() => root.Export(Path.Combine(Path.GetDirectoryName(source),"forbidden.pac")),"native container source overwrite guard");
    }
    private static int VerifyContainerRestart(string output)
    {
        try
        {
            _output=output; Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT",Path.Combine(output,"settings"));
            SourceWriteGuard.Initialize(Path.Combine(output,"storage","source-roots.txt"));
            var workspace=CompanionWorkspace.Load(Path.Combine(output,"container.fpcompanion"),Path.Combine(output,"storage"));
            Assert(workspace.Model.ContainerPath=="0/0" && workspace.TextureSelections["OriginalQuadrants"].ContainerPath=="1/0","native container choices survive process restart");
            VerifyContainerNative(workspace.Assemble(CancellationToken.None),60);
            Console.WriteLine("PASS: fresh process restored exact source/container/resource identities and genuine blended native PAC output"); return 0;
        }
        catch(Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    private static int InspectConfiguredBrawl(string source,string evidence)
    {
        try
        {
            evidence=Path.GetFullPath(evidence); if(Directory.Exists(evidence)) throw new IOException("Use a new inventory evidence directory.");
            Directory.CreateDirectory(evidence); Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT",Path.Combine(evidence,"settings"));
            new SourceFolderSession(Path.GetDirectoryName(source),Path.Combine(evidence,"storage"));
            string hash=Hash(source); var inventory=BrawlContainerAudit.Inspect(source);
            File.WriteAllLines(Path.Combine(evidence,"native-headers.tsv"), inventory.Records.Select(r=>r.Path+"\t"+r.Tag+"\t"+r.Version+"\t"+r.ArcEntry+"\t"+r.Length));
            File.WriteAllLines(Path.Combine(evidence,"issues.txt"),inventory.Issues);
            Assert(Hash(source)==hash,"opt-in source unchanged after read-only header inventory"); File.WriteAllText(Path.Combine(evidence,"source.sha256"),hash);
            Console.WriteLine("Read-only native header inventory: "+inventory.Records.Count+" records; "+inventory.Issues.Count+" explicit limitations; no native decoder/editor/game invoked"); return 0;
        }
        catch(Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
