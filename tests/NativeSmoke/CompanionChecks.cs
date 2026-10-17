// Original FighterPort companion fixtures and verification, 2026-10-17.
// Same original-artwork permission as OriginalFixture.cs. No game content.
using BrawlCrate.Sources;
using BrawlCrate.UI;
using BrawlLib.Imaging;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Textures;
using System;
using System.Collections.Generic;
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
    private static string[] CreateCompanionFixtures()
    {
        string complete = OriginalFixture.Create(Path.Combine(_output, "fixture-build"));
        string inputs = Path.Combine(_output, "inputs");
        string models = Path.Combine(inputs, "models"), textures = Path.Combine(inputs, "textures"), motions = Path.Combine(inputs, "motions");
        Directory.CreateDirectory(models); Directory.CreateDirectory(textures); Directory.CreateDirectory(motions);
        string modelPath = Path.Combine(models, "model.brres"), texturePath = Path.Combine(textures, "chosen-texture.brres"),
            duplicatePath = Path.Combine(textures, "ambiguous-texture.brres"), motionPath = Path.Combine(motions, "OriginalSwing.chr0");
        using (var root = (BRRESNode)NodeFactory.FromFile(null, complete))
        using (var textureRoot = new BRRESNode { Name = "OriginalTextureSource" })
        {
            var texture = Find<TEX0Node>(root);
            texture.Remove(); textureRoot.GetOrCreateFolder<TEX0Node>().AddChild(texture);
            textureRoot.Export(texturePath);
            var animation = Find<CHR0Node>(root);
            animation.Export(motionPath); animation.Remove(); animation.Dispose();
            var retained = root.CreateResource<SRT0Node>("PreservedBaseChannel"); retained.Version = 4; retained.FrameCount = 17; retained.Loop = false;
            root.Export(modelPath);
        }
        using (var root = new BRRESNode { Name = "OriginalAmbiguityFixture" })
        using (var bitmap = new Bitmap(32, 32))
        {
            using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(Color.Magenta);
            var texture = root.CreateResource<TEX0Node>("OriginalQuadrants");
            texture.ReplaceRaw(TextureConverter.RGBA8.EncodeTEX0Texture(bitmap, 1)); texture.Name = "OriginalQuadrants";
            root.Export(duplicatePath);
        }
        using (var root = NodeFactory.FromFile(null, modelPath))
        {
            Assert(Find<TEX0Node>(root) == null && Find<CHR0Node>(root) == null, "model source really has no texture or motion");
            Assert(Find<MDL0Node>(root) != null, "model-only native source");
        }
        using (var root = NodeFactory.FromFile(null, motionPath)) Assert(root is CHR0Node, "motion source really is loose CHR0");
        return new[] { modelPath, duplicatePath, texturePath, motionPath };
    }

    private static void ExerciseCompanions(bool gui)
    {
        string[] paths = CreateCompanionFixtures();
        var hashes = paths.ToDictionary(p => p, Hash);
        File.WriteAllLines(Path.Combine(_output, "companion-source.sha256"), hashes.Select(p => p.Value + "\t" + p.Key));
        string storage = Path.Combine(_output, "storage");
        var workspace = new CompanionWorkspace(storage);
        workspace.AddSource(paths[0], CancellationToken.None);
        workspace.SelectModel(workspace.Resources.Single(r => r.Kind == "MDL0"));
        Assert(workspace.Validate().Any(s => s.Contains("Missing")), "missing texture is reported");
        workspace.AddSource(paths[1], CancellationToken.None); workspace.AddSource(paths[2], CancellationToken.None);
        Assert(workspace.TextureStatus("OriginalQuadrants").StartsWith("Ambiguous"), "same-name textures are reported ambiguous");
        Expect<IOException>(() => workspace.Assemble(CancellationToken.None), "ambiguity must block assembly without explicit selection");
        workspace.AddSource(paths[3], CancellationToken.None);
        workspace.SelectMotion(workspace.Resources.Single(r => r.Kind == "CHR0"));
        // Deliberately choose the SECOND candidate, whose pixels differ from the first.
        workspace.SelectTexture("OriginalQuadrants", workspace.Resources.Single(r => r.Kind == "TEX0" && r.SourcePath == paths[2]));
        Assert(workspace.Validate().Count == 0, "explicit companion selections resolve dependencies");
        string resolution = Path.Combine(_output, "character.fpcompanion");
        workspace.Save(resolution);
        Expect<IOException>(() => workspace.Save(Path.Combine(Path.GetDirectoryName(paths[0]), "forbidden.fpcompanion")), "resolution save must not write mounted sources");
        var doc = new XmlDocument { XmlResolver = null }; doc.Load(resolution);
        foreach(string version in new[]{"1","2"})
        {
            doc.Load(resolution); doc.DocumentElement.SetAttribute("version",version);
            if(version=="1") doc.DocumentElement.RemoveChild(doc.DocumentElement.SelectSingleNode("brawlDumpRoots"));
            foreach(XmlElement choice in doc.DocumentElement.SelectNodes("choice")) {choice.RemoveAttribute("containerPath");choice.RemoveAttribute("containerIdentity");}
            string legacyResolution=Path.Combine(_output,"version"+version+".fpcompanion");doc.Save(legacyResolution);
            Assert(CompanionWorkspace.Load(legacyResolution,storage).TextureSelections["OriginalQuadrants"].SourcePath==paths[2],"existing version "+version+" resolutions restore exact selections");
        }
        doc.Load(resolution);
        ((XmlElement)doc.DocumentElement.SelectSingleNode("source")).SetAttribute("sha256", new string('0', 64));
        string stale = Path.Combine(_output, "changed-source.fpcompanion"); doc.Save(stale);
        Expect<IOException>(() => CompanionWorkspace.Load(stale, storage), "changed source hash must block saved selections");
        doc.Load(resolution); ((XmlElement)doc.DocumentElement.SelectSingleNode("source")).SetAttribute("path", Path.Combine(_output, "missing.brres"));
        string missing = Path.Combine(_output, "missing-source.fpcompanion"); doc.Save(missing);
        Expect<IOException>(() => CompanionWorkspace.Load(missing, storage), "missing source must be reported");
        doc.Load(resolution); ((XmlElement)doc.DocumentElement.SelectSingleNode("source")).SetAttribute("path", "model.brres");
        string relative = Path.Combine(_output, "relative-source.fpcompanion"); doc.Save(relative);
        Expect<IOException>(() => CompanionWorkspace.Load(relative, storage), "relative source paths must not depend on launch directory");
        doc.Load(resolution); ((XmlElement)doc.DocumentElement.SelectSingleNode("source")).SetAttribute("path", @"\\not-contacted.invalid\share\model.brres");
        string network = Path.Combine(_output, "network-source.fpcompanion"); doc.Save(network);
        Expect<IOException>(() => CompanionWorkspace.Load(network, storage), "network sources must be rejected before filesystem access");
        doc.Load(resolution); ((XmlElement)doc.DocumentElement.SelectSingleNode("choice")).SetAttribute("identity", "wrong-resource");
        string wrong = Path.Combine(_output, "wrong-identity.fpcompanion"); doc.Save(wrong);
        Expect<IOException>(() => CompanionWorkspace.Load(wrong, storage), "changed native identity must be rejected");
        File.WriteAllText(Path.Combine(_output, "unsafe-xml.fpcompanion"), "<!DOCTYPE test [<!ENTITY x SYSTEM 'file:///not-read'>]><fighterportCompanions version='1'>&x;</fighterportCompanions>");
        Expect<XmlException>(() => CompanionWorkspace.Load(Path.Combine(_output, "unsafe-xml.fpcompanion"), storage), "resolution must prohibit external entities");
        Log("PASS: missing/ambiguous dependency and changed-hash/missing-source/native-identity/XML-entity cases fail explicitly; no automatic first-match choice");
        var loaded = CompanionWorkspace.Load(resolution, storage);
        Assert(loaded.TextureSelections["OriginalQuadrants"].SourcePath == paths[2], "persisted explicit second candidate");
        string assembled = loaded.Assemble(CancellationToken.None);
        VerifyNative(assembled, 60); VerifyPreservedBase(assembled);
        Log("PASS: separate model BRRES + chosen texture BRRES + loose CHR0 assemble and decode; base SRT0 retained");
        using (var child = Process.Start(new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            Arguments = "--verify-companions \"" + _output + "\"", UseShellExecute = false, CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true
        }))
        {
            if (!child.WaitForExit(30000)) { child.Kill(); throw new TimeoutException("Companion resolution restart check"); }
            Assert(child.ExitCode == 0, "companion restart: " + child.StandardError.ReadToEnd());
            Log(child.StandardOutput.ReadToEnd().Trim());
        }
        if (gui)
        {
            var main = MainForm.Instance; main.StartPosition = FormStartPosition.Manual; main.Location = new Point(25, 25); main.Size = new Size(1050, 760);
            Exception failure = null;
            main.Shown += (sender, args) => main.BeginInvoke(new Action(() =>
            {
                try
                {
                    // Exercise the real selectors, including the visible ambiguity state.
                    var fresh = new CompanionWorkspace(storage);
                    using (var form = new CompanionWorkspaceForm(fresh))
                    {
                        form.Show(main);
                        foreach (string path in paths) { Task task = form.AddSourceAsync(path); WaitFor(() => task.IsCompleted, "companion source inventory"); task.GetAwaiter().GetResult(); }
                        var model = (ComboBox)form.Controls.Find("companionModel", true).Single(); model.SelectedIndex = 0;
                        var bindings = (ListView)form.Controls.Find("companionBindings", true).Single();
                        Assert(bindings.Items[0].SubItems[1].Text.StartsWith("Ambiguous"), "actual UI shows ambiguous binding");
                        bindings.Items[0].Selected = true; Pump(50);
                        var candidates = (ComboBox)form.Controls.Find("companionTexture", true).Single();
                        candidates.SelectedItem = fresh.Resources.Single(r => r.SourcePath == paths[2] && r.Kind == "TEX0");
                        ((ComboBox)form.Controls.Find("companionMotion", true).Single()).SelectedIndex = 0;
                        Assert(fresh.Validate().Count == 0, "actual selectors record explicit texture and motion");
                        form.SaveResolution(Path.Combine(_output, "ui-resolution.fpcompanion"));
                        CaptureWindow(form, "00-companion-choices.png");
                    }
                    ExerciseGuiContents(main, paths[0], null, CompanionWorkspace.Load(Path.Combine(_output, "ui-resolution.fpcompanion"), storage));
                }
                catch (Exception error) { failure = error; }
                finally { main.Dispose(); Application.ExitThread(); }
            }));
            Application.Run(main);
            if (failure != null) throw new InvalidOperationException("Companion GUI exercise failed", failure);
        }
        else { ExerciseNative(assembled); VerifyPreservedBase(Path.Combine(_output, "edited-native.brres")); }
        foreach (var pair in hashes) Assert(Hash(pair.Key) == pair.Value, "all companion source hashes unchanged");
        Log("PASS: all " + hashes.Count + " model/texture/motion source files unchanged after resolution, process restart and native edit/save/reopen");
    }

    private static int VerifyCompanionRestart(string output)
    {
        try
        {
            _output = output;
            Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT", Path.Combine(output, "settings"));
            SourceWriteGuard.Initialize(Path.Combine(output, "storage", "source-roots.txt"));
            var workspace = CompanionWorkspace.Load(Path.Combine(output, "character.fpcompanion"), Path.Combine(output, "storage"));
            Assert(workspace.TextureSelections["OriginalQuadrants"].SourcePath.EndsWith("chosen-texture.brres"), "second candidate survives process restart");
            string assembled = workspace.Assemble(CancellationToken.None);
            VerifyNative(assembled, 60); VerifyPreservedBase(assembled);
            foreach (string line in File.ReadAllLines(Path.Combine(output, "companion-source.sha256")))
            {
                string[] pair = line.Split('\t'); Assert(Hash(pair[1]) == pair[0], "source hash after restart");
            }
            Console.WriteLine("PASS: independent process restored exact model/texture/motion identities, reassembled native resources and verified every source hash");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    private static void VerifyPreservedBase(string path)
    {
        using (ResourceNode root = NodeFactory.FromFile(null, path))
        {
            var retained = Find<SRT0Node>(root);
            Assert(retained != null && retained.Name == "PreservedBaseChannel" && retained.Version == 4 && retained.FrameCount == 17 && !retained.Loop,
                "non-selected native base channel retained without silent removal");
        }
    }
}
