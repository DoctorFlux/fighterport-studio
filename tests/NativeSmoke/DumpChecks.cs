// Original FighterPort dump-layout fixtures, 2026-10-17. Same permission as OriginalFixture.cs.
using BrawlCrate.Sources;
using BrawlCrate.UI;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
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
    private const string ModelFile = "fighter/Original/model.brres", TextureFile = "fighter/Original/textures/skin.brres", MotionFile = "fighter/Original/motion/swing.chr0";
    private static void CopyDumpFile(string input, string root, string relative)
    { string path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.Copy(input, path); }
    private static BrawlLooseDumpProvider DiscoverAll(BrawlDumpProfile profile, DumpDiscoveryBudget budget = null)
    {
        var scan = new BrawlLooseDumpProvider(profile, Path.Combine(_output, "storage"), budget);
        while (!scan.Finished) scan.ReadNext(3, CancellationToken.None);
        return scan;
    }
    private static void ExerciseDump(bool gui)
    {
        string[] sources = CreateCompanionFixtures();
        string dump = Path.Combine(_output, "dumps"), baseRoot = Path.Combine(dump, "base"), overrideRoot = Path.Combine(dump, "override"), conflictRoot = Path.Combine(dump, "conflict");
        CopyDumpFile(sources[0], baseRoot, ModelFile); CopyDumpFile(sources[1], baseRoot, TextureFile); CopyDumpFile(sources[3], baseRoot, MotionFile);
        CopyDumpFile(sources[2], overrideRoot, TextureFile); CopyDumpFile(sources[1], conflictRoot, TextureFile);
        CopyDumpFile(sources[1], overrideRoot, "fighter/Unrelated/textures/skin.brres");
        var hashes = Directory.GetFiles(dump, "*", SearchOption.AllDirectories).Concat(sources).ToDictionary(p => p, Hash);
        File.WriteAllLines(Path.Combine(_output, "dump-source.sha256"), hashes.Select(p => p.Value + "\t" + p.Key));
        string storage = Path.Combine(_output, "storage");
        var profile = new BrawlDumpProfile();
        var baseMount = profile.Add(baseRoot, "Original base", 0);
        var overrideMount = profile.Add(overrideRoot, "Chosen override", 10);
        var conflictMount = profile.Add(conflictRoot, "Conflicting override", 10);
        Expect<IOException>(() => profile.Add(Path.Combine(baseRoot, "fighter"), "Overlapping", 1), "overlapping roots must be rejected");
        Expect<IOException>(() => profile.Add(@"\\not-contacted.invalid\dump", "Network", 1), "network roots rejected before access");
        using (var empty = DiscoverAll(new BrawlDumpProfile())) Assert(!empty.Complete && empty.Issues.Any(), "no roots is actionable, not a completed catalogue");
        var disposed = new BrawlLooseDumpProvider(profile, storage); disposed.Dispose();
        Assert(!disposed.Complete && disposed.Cancelled, "disposed partial cursor never becomes a complete catalogue");
        using (var lazy = new BrawlLooseDumpProvider(profile, storage))
        {
            Assert(lazy.EntriesVisited == 0 && lazy.Candidates.Count == 0, "provider construction is lazy");
            lazy.ReadNext(1, CancellationToken.None);
            Assert(lazy.EntriesVisited == 1 && !lazy.Finished, "one-entry discovery page leaves rest unvisited");
            using (var cancel = new CancellationTokenSource())
            {
                lazy.ReadNext(2, cancel.Token, progress => cancel.Cancel());
                Assert(lazy.Cancelled && !lazy.Complete, "mid-discovery cancellation is explicit");
            }
            Assert(!Directory.Exists(Path.Combine(storage, "StagedSources")), "discovery never stages/decodes any file");
        }
        using (var scan = DiscoverAll(profile))
        {
            Assert(scan.Complete && scan.Candidates.Count == 6, "all original dump files discovered");
            var texture = scan.Candidates.Single(c => c.MountId == overrideMount.Id && c.RelativePath == TextureFile);
            Assert(scan.Explain(texture).StartsWith("Ambiguous"), "equal-priority collisions remain ambiguous");
            Assert(scan.Explain(scan.Candidates.Single(c => c.RelativePath.Contains("Unrelated"))).StartsWith("Winner"), "unrelated same-name native textures do not collide");
            profile.Configure(conflictMount.Id, 5, true);
            using (var ordered = DiscoverAll(profile))
            {
                var chosen = ordered.Candidates.Single(c => c.MountId == overrideMount.Id && c.RelativePath == TextureFile);
                Assert(ordered.Explain(chosen).Contains("highest priority 10"), "ordered overrides select correct logical file");
                var workspace = new CompanionWorkspace(storage);
                var modelFile = ordered.Candidates.Single(c => c.RelativePath == ModelFile);
                workspace.AddDiscoveredSource(profile, modelFile, ordered.Explain(modelFile), CancellationToken.None);
                workspace.SelectModel(workspace.Resources.Single(r => r.Kind == "MDL0"));
                Assert(workspace.Validate().Any(e => e.Contains("Missing")), "missing discovered texture dependency remains actionable");
                foreach (var item in ordered.Candidates.Where(c => c.RelativePath == ModelFile || c.RelativePath == MotionFile || c == chosen))
                    workspace.AddDiscoveredSource(profile, item, ordered.Explain(item), CancellationToken.None);
                workspace.SelectModel(workspace.Resources.Single(r => r.Kind == "MDL0"));
                workspace.SelectMotion(workspace.Resources.Single(r => r.Kind == "CHR0"));
                workspace.SelectTexture("OriginalQuadrants", workspace.Resources.Single(r => r.Kind == "TEX0"));
                string pinned = workspace.TextureSelections["OriginalQuadrants"].Identity;
                // Manual lower-priority file choice is allowed, but never applied to the binding implicitly.
                var lower = ordered.Candidates.Single(c => c.MountId == baseMount.Id && c.RelativePath == TextureFile);
                workspace.AddDiscoveredSource(profile, lower, ordered.Explain(lower), CancellationToken.None);
                var lowerResource = workspace.Resources.Single(r => r.Kind == "TEX0" && r.MountId == baseMount.Id);
                workspace.SelectTexture("OriginalQuadrants", lowerResource);
                using (var decoded = NodeFactory.FromFile(null, workspace.Assemble(CancellationToken.None)))
                using (var image = Find<TEX0Node>(decoded).GetImage(0)) Assert(image.GetPixel(3, 3).ToArgb() == Color.Magenta.ToArgb(), "explicit manual override actually selects lower-priority pixels");
                workspace.SelectTexture("OriginalQuadrants", workspace.Resources.Single(r => r.Kind == "TEX0" && r.MountId == overrideMount.Id));
                profile.Configure(conflictMount.Id, 20, true);
                workspace.SetDumpProfile(profile);
                using (var rescanned = DiscoverAll(profile))
                    Assert(rescanned.Explain(rescanned.Candidates.Single(c => c.MountId == conflictMount.Id)).Contains("highest priority 20"), "new scan reports new recommended source");
                Assert(workspace.TextureSelections["OriginalQuadrants"].Identity == pinned, "rescan/priority changes do not replace explicit pinned choices");
                workspace.SelectModel(workspace.Model);
                Assert(workspace.TextureSelections["OriginalQuadrants"].Identity == pinned, "same-model refresh retains explicit bindings");
                profile.Save(BrawlDumpProfile.DefaultPath); workspace.Save(Path.Combine(_output, "dump.fpcompanion"));
                string resolutionHash = Hash(Path.Combine(_output, "dump.fpcompanion"));
                using (var cancelled = new BrawlLooseDumpProvider(profile, storage))
                using (var cancel = new CancellationTokenSource())
                {
                    cancelled.ReadNext(10, cancel.Token, progress => cancel.Cancel());
                    Assert(cancelled.Cancelled && workspace.TextureSelections["OriginalQuadrants"].Identity == pinned && Hash(Path.Combine(_output, "dump.fpcompanion")) == resolutionHash,
                        "cancelled rediscovery preserves pinned choices and saved work byte-for-byte");
                }
                Expect<IOException>(() => profile.Save(Path.Combine(baseRoot, "forbidden.xml")), "profiles cannot write source roots");
                string assembled = workspace.Assemble(CancellationToken.None); VerifyNative(assembled, 60); VerifyPreservedBase(assembled);
                if (!gui) ExerciseNative(assembled);
            }
        }
        foreach (var budget in new[] { new DumpDiscoveryBudget { MaxEntries = 1 }, new DumpDiscoveryBudget { MaxDepth = 0 },
            new DumpDiscoveryBudget { MaxCandidates = 1 }, new DumpDiscoveryBudget { MaxDirectories = 3 }, new DumpDiscoveryBudget { MaxProbeBytes = 16 }, new DumpDiscoveryBudget { MaxMetadataBytes = 1 } })
            using (var limited = DiscoverAll(profile, budget)) Assert(limited.Truncated && !limited.Complete, "budget truncation must be explicit");
        var missing = profile.Snapshot(); missing.Add(Path.Combine(dump, "missing-root"), "Missing root", 0);
        using (var scan = DiscoverAll(missing)) Assert(!scan.Complete && scan.Issues.Any(i => i.Contains("Missing root")), "missing root diagnostic");
        CheckDumpInvalidInputs(sources[2], storage);
        // Fresh processes restore root config and pinned source/container/resource identities twice.
        for (int attempt = 0; attempt < 2; attempt++)
            using (var child = Process.Start(new ProcessStartInfo { FileName = Process.GetCurrentProcess().MainModule.FileName,
                Arguments = "--verify-dump \"" + _output + "\"", UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true }))
            {
                if (!child.WaitForExit(30000)) { child.Kill(); throw new TimeoutException("Dump restart"); }
                Assert(child.ExitCode == 0, "dump restart: " + child.StandardError.ReadToEnd()); Log(child.StandardOutput.ReadToEnd().Trim());
            }
        if (gui) ExerciseDumpGui(baseRoot, overrideRoot, conflictRoot, storage);
        foreach (var pair in hashes) Assert(Hash(pair.Key) == pair.Value, "all original dump/companion sources unchanged");
        Log("PASS: lazy paged header discovery, ordered logical-file overrides, ties/manual choice, unrelated names, preserved pins, all six budgets, cancellation, missing/changed input, reparse rejection and repeated restart");
        Log("PASS: all " + hashes.Count + " original source SHA256 values unchanged after dump resolution/native edit/save/reopen");
    }
    private static void CheckDumpInvalidInputs(string source, string storage)
    {
        string root = Path.Combine(_output, "negative-copy"), path = Path.Combine(root, "changed.brres");
        Directory.CreateDirectory(root); File.Copy(source, path);
        var profile = new BrawlDumpProfile(); profile.Add(root, "Negative copies only", 0);
        using (var scan = DiscoverAll(profile))
        {
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(2));
            Expect<IOException>(() => new CompanionWorkspace(storage).AddDiscoveredSource(profile, scan.Candidates.Single(), "test", CancellationToken.None), "stale discovery metadata rejected");
        }
        var workspace = new CompanionWorkspace(storage); workspace.AddSource(path, CancellationToken.None);
        string saved = Path.Combine(_output, "negative.fpcompanion"); workspace.Save(saved);
        byte[] originalCopy = File.ReadAllBytes(path);
        using (var grown = new FileStream(path, FileMode.Open, FileAccess.Write)) grown.SetLength(SourceFolderSession.MaxFileBytes + 1);
        Expect<IOException>(() => workspace.VerifySources(CancellationToken.None), "grown pinned source rejected before unbounded hashing");
        File.WriteAllBytes(path, originalCopy);
        byte[] bytes = File.ReadAllBytes(path); bytes[bytes.Length - 1] ^= 1; File.WriteAllBytes(path, bytes);
        Expect<IOException>(() => CompanionWorkspace.Load(saved, storage), "changed full hash rejected before selection restore");
        File.Move(path, Path.Combine(root, "removed.brres"));
        Expect<IOException>(() => CompanionWorkspace.Load(saved, storage), "missing selected source rejected");
        File.WriteAllBytes(Path.Combine(root, "unsupported.pcs"), new byte[32]);
        using (var scan = DiscoverAll(profile)) Assert(scan.Issues.Any(s => s.Contains("Unsupported loose companion")), "compressed/container inputs explicitly unsupported");
        string link = Path.Combine(root, "reparse"), target = Path.Combine(_output, "link-target"); Directory.CreateDirectory(target);
        using (var command = Process.Start(new ProcessStartInfo("cmd.exe", "/c mklink /J \"" + link + "\" \"" + target + "\"") { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true }))
        { Assert(command.WaitForExit(10000) && command.ExitCode == 0, "original-fixture junction creation"); }
        using (var scan = DiscoverAll(profile)) Assert(scan.Issues.Any(s => s.Contains("reparse")), "discovered reparse directory rejected");
        Expect<IOException>(() => new SourceFolderSession(link, storage), "reparse root rejected");
        var doc = new XmlDocument { XmlResolver = null }; doc.Load(BrawlDumpProfile.DefaultPath);
        ((XmlElement)doc.DocumentElement.FirstChild).SetAttribute("path", "relative-root");
        string invalid = Path.Combine(_output, "invalid-profile.xml"); doc.Save(invalid);
        Expect<IOException>(() => BrawlDumpProfile.Load(invalid), "relative profile root rejected");
    }
    private static int VerifyDumpRestart(string output)
    {
        try
        {
            _output = output; Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT", Path.Combine(output, "settings"));
            SourceWriteGuard.Initialize(Path.Combine(output, "storage", "source-roots.txt"));
            var profile = BrawlDumpProfile.Load(BrawlDumpProfile.DefaultPath);
            Assert(profile.Mounts.Count == 3 && profile.Mounts.Single(m => m.Label == "Conflicting override").Priority == 20, "roots and priority restore");
            var workspace = CompanionWorkspace.Load(Path.Combine(output, "dump.fpcompanion"), Path.Combine(output, "storage"));
            var texture = workspace.TextureSelections["OriginalQuadrants"];
            Assert(texture.MountLabel == "Chosen override" && texture.MountPriority == 10 && texture.LogicalPath == TextureFile && texture.SelectionReason.Contains("highest priority 10"), "original source lineage and reason retained independently of current priorities");
            VerifyNative(workspace.Assemble(CancellationToken.None), 60);
            foreach (string line in File.ReadAllLines(Path.Combine(output, "dump-source.sha256"))) { string[] pair = line.Split('\t'); Assert(Hash(pair[1]) == pair[0], "restart source hash"); }
            Console.WriteLine("PASS: independent process restored roots, priorities, explicit source lineage/hash/native identities and reassembled selected pixels/motion with unchanged originals"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    private static void ExerciseDumpGui(string baseRoot, string overrideRoot, string conflictRoot, string storage)
    {
        var main = MainForm.Instance; main.StartPosition = FormStartPosition.Manual; main.Location = new Point(25, 25); main.Size = new Size(1050, 760);
        Exception failure = null;
        main.Shown += (sender, args) => main.BeginInvoke(new Action(() =>
        {
            try
            {
                var workspace = new CompanionWorkspace(storage);
                using (var form = new BrawlDumpSourcesForm(workspace))
                {
                    form.Show(main); Assert(form.Profile.Mounts.Count == 3, "actual dump UI restores saved roots");
                    while (form.Discovery == null || !form.Discovery.Finished)
                    { ((Button)form.Controls.Find("dumpNextPage", true).Single()).PerformClick(); WaitFor(() => !form.IsBusy, "actual UI discovery page"); }
                    var list = (ListView)form.Controls.Find("dumpCandidates", true).Single();
                    Assert(list.Items.Cast<ListViewItem>().Any(i => i.SubItems[4].Text.Contains("highest priority 20")), "actual UI shows winner and reason");
                    CaptureWindow(form, "00-dump-catalogue.png");
                    foreach (string relative in new[] { ModelFile, TextureFile, MotionFile })
                    {
                        foreach (ListViewItem item in list.Items) item.Selected = false;
                        var chosen = list.Items.Cast<ListViewItem>().Single(i => ((DumpCandidate)i.Tag).RelativePath == relative && ((DumpCandidate)i.Tag).Root == (relative == TextureFile ? overrideRoot : baseRoot));
                        chosen.Selected = true;
                        ((Button)form.Controls.Find("dumpStageSelected", true).Single()).PerformClick(); WaitFor(() => !form.IsBusy, "actual UI stages explicit source");
                    }
                    Assert(workspace.Resources.Count(r => r.Kind == "MDL0") == 1 && workspace.Resources.Count(r => r.Kind == "TEX0") == 1 && workspace.Resources.Count(r => r.Kind == "CHR0") == 1, "only three selected files enter companion inventory");
                    form.SaveProfile(); CaptureWindow(form, "00-dump-pinned-file.png");
                }
                using (var companions = new CompanionWorkspaceForm(workspace))
                {
                    companions.Show(main);
                    ((ComboBox)companions.Controls.Find("companionModel", true).Single()).SelectedIndex = 0;
                    var bindings = (ListView)companions.Controls.Find("companionBindings", true).Single(); bindings.Items[0].Selected = true; Pump(30);
                    ((ComboBox)companions.Controls.Find("companionTexture", true).Single()).SelectedIndex = 0;
                    ((ComboBox)companions.Controls.Find("companionMotion", true).Single()).SelectedIndex = 0;
                    companions.SaveResolution(Path.Combine(_output, "dump-ui.fpcompanion")); CaptureWindow(companions, "00-dump-companions.png");
                }
                ExerciseGuiContents(main, Path.Combine(baseRoot, ModelFile.Replace('/', Path.DirectorySeparatorChar)), null,
                    CompanionWorkspace.Load(Path.Combine(_output, "dump-ui.fpcompanion"), storage));
            }
            catch (Exception error) { failure = error; }
            finally { main.Dispose(); Application.ExitThread(); }
        }));
        Application.Run(main);
        if (failure != null) throw new InvalidOperationException("Dump GUI exercise failed", failure);
    }
}
