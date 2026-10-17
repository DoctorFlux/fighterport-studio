// FighterPort Studio original restart verification harness, 2026-10-17.
// This executable hosts the real imported MainForm and model editor. It does not
// replace them with test doubles. GUI checks call control handlers; no manual-click
// or in-game verification is implied. Generated files contain only original artwork.
using BrawlCrate.NodeWrappers;
using BrawlCrate.UI;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlCrate.Sources;
using BrawlLib.Internal;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

internal static partial class NativeSmoke
{
    private static string _output;
    private static readonly List<string> Results = new List<string>();
    private static readonly Type NativeProgram = typeof(MainForm).Assembly.GetType("BrawlCrate.Program", true);

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--verify-companions") return VerifyCompanionRestart(args[1]);
        if (args.Length == 2 && args[0] == "--verify-dump") return VerifyDumpRestart(args[1]);
        if (args.Length == 3 && args[0] == "--inspect-brawl") return InspectConfiguredBrawl(args[1], args[2]);
        if (args.Length == 2 && args[0] == "--verify-container") return VerifyContainerRestart(args[1]);
        if (args.Length == 2 && args[0] == "--verify-library") return VerifyLibraryRestart(args[1]);
        if (args.Length == 2 && args[0] == "--guard-live")
        {
            try
            {
                SourceWriteGuard.Initialize(Path.Combine(args[1], "storage", "source-roots.txt"));
                SourceWriteGuard.RegisterRoot(Path.Combine(args[1], "live-child-source"));
                File.WriteAllText(Path.Combine(args[1], "guard-live-ready"), "ready");
                var watch = Stopwatch.StartNew();
                while (!File.Exists(Path.Combine(args[1], "guard-live-check")) && watch.ElapsedMilliseconds < 10000) Thread.Sleep(20);
                Assert(File.Exists(Path.Combine(args[1], "guard-live-check")), "concurrent guard rendezvous timeout");
                string protectedOutput = Path.Combine(args[1], "live-parent-source", "new.brres");
                using (var root = NodeFactory.FromFile(null, Path.Combine(args[1], "source", "original-paddle.brres")))
                    Expect<IOException>(() => root.Export(protectedOutput), "running process native Export missed newly registered source root");
                Assert(!File.Exists(protectedOutput), "concurrent blocked native output was created");
                Console.WriteLine("PASS: already-running process protects newly mounted source root");
                return 0;
            }
            catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        }
        if (args.Length == 2 && args[0] == "--verify-settings")
        {
            try
            {
                Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT", Path.Combine(args[1], "settings"));
                AssertPersistedSettings();
                Console.WriteLine("PASS: independent process reloaded application/library settings");
                return 0;
            }
            catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        }
        if (args.Length == 2 && args[0] == "--verify-guard")
        {
            try
            {
                SourceWriteGuard.Initialize(Path.Combine(args[1], "storage", "source-roots.txt"));
                Expect<IOException>(() => SourceWriteGuard.ThrowIfProtected(Path.Combine(args[1], "source", "original-paddle.brres")), "persistent source root was not reloaded");
                Console.WriteLine("PASS: source guard reloaded in independent process");
                return 0;
            }
            catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        }
        if (args.Length != 2 || !new[] { "--native", "--gui", "--companions-native", "--companions-gui", "--dump-native", "--dump-gui", "--container-native", "--container-gui", "--library-native", "--library-gui" }.Contains(args[0]))
        {
            Console.Error.WriteLine("Usage: NativeSmoke.exe --native|--gui|--companions-native|--companions-gui <new-output-directory>");
            return 2;
        }
        _output = Path.GetFullPath(args[1]);
        if (Directory.Exists(_output) && Directory.EnumerateFileSystemEntries(_output).Any())
            throw new IOException("Use a new/empty evidence directory; previous evidence is never overwritten.");
        Directory.CreateDirectory(_output);
        var watchdog = new Thread(() => { Thread.Sleep(90000); File.AppendAllText(Path.Combine(_output, "result.txt"), "FAIL: 90 second timeout\r\n"); Environment.Exit(3); }) { IsBackground = true };
        watchdog.Start();
        try
        {
            Application.EnableVisualStyles();
            Environment.SetEnvironmentVariable("FIGHTERPORT_DATA_ROOT", Path.Combine(_output, "settings"));
            SourceWriteGuard.Initialize(Path.Combine(_output, "storage", "source-roots.txt"));
            // Match application entrypoint ordering before fixture import or message
            // pumping can create WinForms handles on this STA thread.
            if (args[0].EndsWith("gui", StringComparison.Ordinal)) System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(NativeProgram.TypeHandle);
            VerifySettingsPersistence();
            if (args[0].StartsWith("--library", StringComparison.Ordinal))
            {
                ExerciseLibrary(args[0].EndsWith("gui", StringComparison.Ordinal));
                Log("PASS: completed " + args[0] + "; original rigs only; no retargeting/gameplay/game test"); return 0;
            }
            if (args[0].StartsWith("--container", StringComparison.Ordinal))
            {
                ExerciseContainers(args[0].EndsWith("gui", StringComparison.Ordinal));
                Log("PASS: completed " + args[0] + "; original fixtures only, no game test"); return 0;
            }
            if (args[0].StartsWith("--dump", StringComparison.Ordinal))
            {
                ExerciseDump(args[0].EndsWith("gui", StringComparison.Ordinal));
                Log("PASS: completed " + args[0] + "; no commercial inputs or game test");
                return 0;
            }
            if (args[0].StartsWith("--companions", StringComparison.Ordinal))
            {
                ExerciseCompanions(args[0].EndsWith("gui", StringComparison.Ordinal));
                Log("PASS: completed " + args[0] + "; no commercial inputs or game test");
                return 0;
            }
            Log("Generating original fixture with imported BrawlLib");
            string source = OriginalFixture.Create(Path.Combine(_output, "source"));
            string sourceHash = Hash(source);
            VerifyNative(source, 60);
            Log("PASS: genuine BRRES/MDL0 v11/CHR0 v4/TEX0 RGBA8 and material decode");
            var session = ExerciseSources(source);
            var originalFiles = Directory.GetFiles(Path.GetDirectoryName(source)).ToDictionary(path => path, Hash);
            if (args[0] == "--gui") ExerciseGui(source, session);
            else ExerciseNative(session.Stage(source, CancellationToken.None).StagedPath);
            Assert(Hash(source) == sourceHash, "source SHA-256 unchanged");
            foreach (var original in originalFiles) Assert(Hash(original.Key) == original.Value, "all source fixture hashes unchanged");
            File.WriteAllLines(Path.Combine(_output, "source.sha256"), originalFiles.Select(pair => pair.Value + "  " + Path.GetFileName(pair.Key)));
            Log("PASS: source SHA-256 unchanged: " + sourceHash);
            Log("PASS: all " + originalFiles.Count + " source-folder files unchanged after editing");
            Log("PASS: completed " + args[0] + "; no commercial inputs or game test");
            return 0;
        }
        catch (Exception error)
        {
            Log("FAIL: " + error);
            return 1;
        }
        finally
        {
            if (args[0].EndsWith("gui", StringComparison.Ordinal))
                foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
                    try { form.Dispose(); } catch (Exception error) { Log("GUI cleanup: " + error.Message); }
            // Exit the smoke process without touching unrelated user applications.
            File.WriteAllLines(Path.Combine(_output, "result.txt"), Results);
        }
    }

    private static void ExerciseNative(string source)
    {
        string saved = Path.Combine(_output, "edited-native.brres");
        using (var root = NodeFactory.FromFile(null, source))
        {
            Find<CHR0Node>(root).Children.OfType<CHR0EntryNode>().Single().SetKeyframe(5, 15, 35);
            root.Export(saved);
        }
        VerifyNative(saved, 35);
        Log("PASS: native edit/export/new file/decode at key frame 15 = 35 degrees");
    }

    private static SourceFolderSession ExerciseSources(string source)
    {
        string folder = Path.GetDirectoryName(source);
        string malformed = Path.Combine(folder, "truncated.brres");
        byte[] header = new byte[16];
        Array.Copy(System.Text.Encoding.ASCII.GetBytes("bres"), header, 4);
        File.WriteAllBytes(malformed, header);
        string compressed = Path.Combine(folder, "unsupported.pcs");
        header[0] = 0x11; header[1] = 0x20;
        File.WriteAllBytes(compressed, header);
        string spoofed = Path.Combine(folder, "truncated-native.mdl0");
        byte[] nativeHeader = new byte[16];
        Array.Copy(System.Text.Encoding.ASCII.GetBytes("MDL0"), nativeHeader, 4);
        nativeHeader[7] = 16; nativeHeader[11] = 11;
        File.WriteAllBytes(spoofed, nativeHeader);
        string disguised = Path.Combine(folder, "signature-not-extension.txt");
        File.Copy(source, disguised);
        string rawArc = Path.Combine(folder, "original-container.pac");
        using (var arc = new ARCNode { Name = "OriginalSourceContainer" })
        {
            arc.AddChild(NodeFactory.FromFile(null, source));
            arc.Export(rawArc);
        }
        var session = new SourceFolderSession(folder, Path.Combine(_output, "storage"));
        Expect<IOException>(() => new SourceFolderSession(AppDomain.CurrentDomain.BaseDirectory, Path.Combine(_output, "storage")), "application/helper directory cannot become a source mount");
        Expect<IOException>(() => new SourceFolderSession(Path.GetTempPath(), Path.Combine(_output, "storage")), "operating-system temporary directory cannot become a source mount");
        Expect<IOException>(() => new SourceFolderSession(FighterPortPaths.DataRoot, Path.Combine(_output, "storage")), "application data directory cannot become a source mount");
        var entries = session.EnumerateDirectory(folder, CancellationToken.None);
        Assert(entries.Single(e => e.Path == source).CanOpen, "BRRES signature recognized");
        Assert(!entries.Single(e => e.Path == compressed).CanOpen, "compressed source shown unsupported");
        Assert(entries.Single(e => e.Path == disguised).CanOpen, "native signature independent of extension");
        Expect<IOException>(() => session.Stage(malformed, CancellationToken.None), "invalid BRRES rejected before native parser");
        Expect<IOException>(() => session.Stage(spoofed, CancellationToken.None), "truncated native version header rejected");
        Expect<IOException>(() => session.Stage(compressed, CancellationToken.None), "compressed source rejected");
        Expect<IOException>(() => session.Stage(Path.Combine(_output, "outside.brres"), CancellationToken.None), "outside-root path rejected");
        Expect<OperationCanceledException>(() => session.Stage(source, new CancellationToken(true)), "cancelled staging rejected");
        Assert(!Directory.GetFiles(Path.Combine(_output, "storage"), "*.partial", SearchOption.AllDirectories).Any(), "failed/pre-cancelled staging leaves no partial output");
        StagedSource staged = session.Stage(source, CancellationToken.None);
        Assert(staged.StagedPath != source && Hash(source) == Hash(staged.StagedPath) && staged.Sha256 == Hash(source), "independent staged copy hash");
        StagedSource renamed = session.Stage(disguised, CancellationToken.None);
        Assert(Path.GetExtension(renamed.StagedPath) == ".brres" && Hash(renamed.StagedPath) == Hash(source), "staged extension follows verified native signature");
        StagedSource stagedArc = session.Stage(rawArc, CancellationToken.None);
        using (var arc = NodeFactory.FromFile(null, stagedArc.StagedPath))
            Assert(arc is ARCNode && Find<MDL0Node>(arc) != null && Find<CHR0Node>(arc).FrameCount == 31, "raw ARC containing original BRRES stages and decodes");
        Log("PASS: raw original ARC/PAC with embedded BRRES stages and decodes model/animation");
        Expect<IOException>(() => SourceWriteGuard.ThrowIfProtected(source), "mounted original write guard");
        string forbidden = Path.Combine(folder, "forbidden-output.brres");
        using (var root = NodeFactory.FromFile(null, staged.StagedPath))
            Expect<IOException>(() => root.Export(forbidden), "native Export cannot write under mounted source");
        Assert(!File.Exists(forbidden), "blocked output was not created");
        using (var stream = new FileStream(source, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
        using (var bitmap = new Bitmap(2, 2))
        {
            Expect<IOException>(() => BrawlLib.Imaging.TGA.ToStream(bitmap, stream), "native TGA stream writer refuses mounted source");
            Expect<IOException>(() => new BrawlLib.Imaging.GIF.AnimatedGifEncoder().Start(stream), "native GIF stream writer refuses mounted source");
            Expect<IOException>(() => { using (var map = new cFileMap(stream, FileMapProtect.ReadWrite, 0, (int)stream.Length)) { } }, "direct managed writable mapping refuses mounted source");
            using (var anonymous = new FileStream(stream.SafeFileHandle, FileAccess.ReadWrite))
                Expect<IOException>(() => { using (var map = FileMap.FromStream(anonymous, FileMapProtect.ReadWrite)) { } }, "native writable mapping resolves protected kernel handle");
        }
        Assert(Hash(source) == staged.Sha256, "blocked stream writes leave source unchanged");
        var shortName = new System.Text.StringBuilder(32768);
        if (GetShortPathName(source, shortName, (uint)shortName.Capacity) > 0 && shortName.ToString() != source)
        {
            Expect<IOException>(() => SourceWriteGuard.ThrowIfProtected(shortName.ToString()), "8.3 alias source write guard");
            Log("PASS: source write guard rejects actual Windows 8.3 alias");
        }
        else Log("SKIP: this volume did not expose a distinct 8.3 source alias");
        string hardlink = Path.Combine(_output, "guard-hardlink.brres");
        if (CreateHardLink(hardlink, source, IntPtr.Zero))
        {
            try
            {
                Expect<IOException>(() => SourceWriteGuard.ThrowIfProtected(hardlink), "outside hardlink source write guard");
                Log("PASS: source write guard rejects actual outside hardlink");
            }
            finally { File.Delete(hardlink); }
        }
        else Log("SKIP: hardlink creation unavailable; Windows error " + Marshal.GetLastWin32Error());
        VerifyLiveSourceProtection();
        using (var child = Process.Start(new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            Arguments = "--verify-guard \"" + _output + "\"",
            UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true, RedirectStandardError = true
        }))
        {
            if (!child.WaitForExit(10000)) { child.Kill(); throw new TimeoutException("Source-guard reload probe"); }
            Assert(child.ExitCode == 0, "persistent source guard: " + child.StandardError.ReadToEnd());
            Log(child.StandardOutput.ReadToEnd().Trim());
        }
        Log("PASS: folder enumeration, signature detection, hash-verified staging, invalid/compressed/outside-root/cancel rejection and native output guard");
        return session;
    }

    private static void VerifyLiveSourceProtection()
    {
        string parentRoot = Path.Combine(_output, "live-parent-source");
        string childRoot = Path.Combine(_output, "live-child-source");
        Directory.CreateDirectory(parentRoot); Directory.CreateDirectory(childRoot);
        using (var child = Process.Start(new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            Arguments = "--guard-live \"" + _output + "\"",
            UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true, RedirectStandardError = true
        }))
        {
            WaitFor(() => File.Exists(Path.Combine(_output, "guard-live-ready")) || child.HasExited, "concurrent source protection startup");
            if (child.HasExited) throw new InvalidOperationException("Source guard peer startup: " + child.StandardError.ReadToEnd());
            SourceWriteGuard.RegisterRoot(parentRoot);
            Expect<IOException>(() => SourceWriteGuard.ThrowIfProtected(Path.Combine(childRoot, "new.brres")), "active parent missed peer source mount");
            File.WriteAllText(Path.Combine(_output, "guard-live-check"), "check");
            if (!child.WaitForExit(10000)) { child.Kill(); throw new TimeoutException("Concurrent source protection check"); }
            Assert(child.ExitCode == 0, "concurrent source protection: " + child.StandardError.ReadToEnd());
            Log(child.StandardOutput.ReadToEnd().Trim());
            string[] roots = File.ReadAllLines(Path.Combine(_output, "storage", "source-roots.txt"));
            Assert(roots.Contains(SourceWriteGuard.NormalizePath(parentRoot)) && roots.Contains(SourceWriteGuard.NormalizePath(childRoot)), "independent source registrations merge without losing roots");
            Log("PASS: independent running processes observe and retain both source-root registrations");
        }
    }

    private static void ExerciseGui(string source, SourceFolderSession session)
    {
        VerifySafetyGates();
        Log("Showing actual MainForm (automatic update/reporting must already be disabled by product code)");
        var main = MainForm.Instance;
        main.StartPosition = FormStartPosition.Manual;
        main.Location = new Point(25, 25);
        main.Size = new Size(1050, 760);
        Exception failure = null;
        main.Shown += (sender, args) => main.BeginInvoke(new Action(() =>
        {
            try { ExerciseGuiContents(main, source, session); }
            catch (Exception error) { failure = error; }
            finally { main.Dispose(); Application.ExitThread(); }
        }));
        Application.Run(main);
        if (failure != null) throw new InvalidOperationException("Native GUI exercise failed", failure);
    }

    private static void ExerciseGuiContents(MainForm main, string source, SourceFolderSession session, CompanionWorkspace companions = null, Action<string, float> verify = null)
    {
        Log("Native WinForms loop: " + Application.MessageLoop + "; context=" + SynchronizationContext.Current?.GetType().FullName);
        using (var graphics = main.CreateGraphics())
            Log("GUI environment: x86=" + !Environment.Is64BitProcess + "; desktop=" + Screen.PrimaryScreen.Bounds.Size + "; main=" + main.Size + "; DPI=" + graphics.DpiX + "x" + graphics.DpiY);
        if (companions != null)
        {
            using (var browser = new CompanionWorkspaceForm(companions))
            {
                browser.Show(main);
                CaptureWindow(browser, "00-companion-resolution.png");
                ((Button)browser.Controls.Find("companionOpen", true).Single()).PerformClick();
                WaitFor(() => browser.IsDisposed || !browser.Visible, "companion Open assembled copy");
                Assert(browser.LastOpenedPath != null, "companion native open path");
            }
            Log("PASS: actual CompanionWorkspace Open button assembled selected dependencies and opened MainForm");
        }
        else using (var browser = new SourceBrowserForm(session))
        {
            browser.Show(main);
            var entries = (ListView)browser.Controls.Find("sourceEntries", true).Single();
            var open = (Button)browser.Controls.Find("sourceOpenButton", true).Single();
            WaitFor(() => entries.Items.Cast<ListViewItem>().Any(i => i.Text == "original-paddle.brres"), "source list load");
            var item = entries.Items.Cast<ListViewItem>().Single(i => i.Text == "original-paddle.brres");
            item.Selected = true;
            Pump(100);
            Assert(open.Enabled, "source Open editable copy enabled");
            CaptureWindow(browser, "00-open-source.png");
            open.PerformClick();
            try { WaitFor(() => browser.IsDisposed || !browser.Visible, "source button native open"); }
            catch
            {
                if (!browser.IsDisposed)
                {
                    Log("Source browser status: " + ((Label)typeof(SourceBrowserForm).GetField("_status", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(browser)).Text);
                    CaptureWindow(browser, "00-failed-source-open.png");
                }
                throw;
            }
        }
        Pump(250);
        var root = (ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null, null);
        Assert(root != null && (string)NativeProgram.GetProperty("RootPath").GetValue(null, null) != source, "source browser opens staged native copy");
        if (companions == null) Log("PASS: actual SourceBrowser list/button opens independent copy in MainForm");
        var model = Find<MDL0Node>(root);
        var texture = Find<TEX0Node>(root);
        var animation = Find<CHR0Node>(root);
        main.TargetResource(texture);
        Pump(250);
        Assert(main.resourceTree.SelectedNode is BaseWrapper, "real resource tree selection");
        CaptureWindow(main, "01-texture-editor.png");
        main.TargetResource(model._matList[0]);
        Pump(100);
        CaptureWindow(main, "02-material-editor.png");
        Log("PASS: MainForm resource tree selects TEX0 texture and MDL0 material controls");

        main.TargetResource(model);
        var wrapper = main.resourceTree.SelectedNode as MDL0Wrapper;
        Assert(wrapper != null, "native MDL0 context wrapper");
        wrapper.Preview();
        Pump(350);
        var editor = ModelEditControl.Instances.Single();
        var viewer = editor.FindForm();
        viewer.WindowState = FormWindowState.Normal;
        viewer.Location = new Point(45, 35);
        viewer.Size = new Size(1180, 820);
        viewer.TopMost = true;
        viewer.Activate();
        // Expand the inherited editor's initially collapsed panels through its
        // real toggle buttons so playback and numeric editing are visible actions.
        foreach (string name in new[] { "btnTopToggle", "btnLeftToggle", "btnRightToggle", "btnBottomToggle" })
            ((Button)editor.Controls.Find(name, true).Single()).PerformClick();
        // Select the viewport installed by inherited viewer settings, then frame
        // this original fixture with the native camera API. Its mesh lies at z=0;
        // an origin camera cannot see that plane through the near clip distance.
        editor.ModelPanel.CurrentViewport = (BrawlLib.Internal.Windows.Controls.Model_Panel.ModelPanelViewport)editor.ModelPanel.First();
        editor.ModelPanel.Camera.ZoomExtents(new Vector3(-5, 7, 0), 45);
        editor.ModelPanel.Invalidate();
        Pump(200);
        Log("Model editor window size: " + viewer.Size);
        Log("Model viewport: visible=" + editor.ModelPanel.Visible + "; size=" + editor.ModelPanel.Size + "; targets=" + editor.ModelPanel._renderList.Count + "; polygons=" + editor.ModelPanel.RenderPolygons);
        editor.SelectedCHR0 = animation;
        editor.SetFrame(1);
        Pump(150);
        var bone = model.BoneCache.OfType<MDL0BoneNode>().Single(b => b.Name == "Paddle");
        Assert(Math.Abs(bone.FrameState.Rotate._z) < 0.01f, "first GUI animation pose");
        CaptureWindow(viewer, "03-model-frame-1.png");
        CaptureViewport(editor, "03-viewport-frame-1.png");
        editor.SetFrame(16);
        Pump(150);
        Assert(Math.Abs(bone.FrameState.Rotate._z - 60) < 0.01f, "scrub GUI evaluated bone pose");
        CaptureWindow(viewer, "04-model-frame-16.png");
        CaptureViewport(editor, "04-viewport-frame-16.png");
        Log("PASS: actual model preview and GUI frame scrub evaluate 0/60 degree poses");

        editor.SetFrame(1);
        int playingFrame = 0;
        using (var stop = new System.Windows.Forms.Timer { Interval = 250 })
        {
            stop.Tick += (sender, eventArgs) => { playingFrame = editor.CurrentFrame; stop.Stop(); editor.StopAnim(); };
            Assert(editor.PlaybackPanel.btnPlay.Visible && editor.PlaybackPanel.btnPlay.Enabled, "native Play button available in expanded animation panel");
            stop.Start();
            editor.PlaybackPanel.btnPlay.PerformClick();
            WaitFor(() => playingFrame != 0, "playback timer completion");
        }
        Assert(playingFrame > 1, "native playback timer advances");
        Log("PASS: native Play button handler/timer advanced to GUI frame " + playingFrame);

        editor.SetFrame(16);
        editor.SelectedBone = bone;
        editor.CHR0Editor.numRotZ.Value = 35;
        editor.CHR0Editor.BoxChangedCreateUndo(editor.CHR0Editor.numRotZ, EventArgs.Empty);
        Pump(100);
        var entry = animation.Children.OfType<CHR0EntryNode>().Single();
        Assert(Math.Abs(entry.GetKeyframe(5, 15)._value - 35) < .01f, "CHR0 numeric editor change");
        Assert(Math.Abs(bone.FrameState.Rotate._z - 35) < .01f, "edited GUI pose");
        CaptureWindow(viewer, "05-edited-frame-16.png");
        Log("PASS: actual CHR0 numeric editor change handler writes 35 degree key and updates pose");

        Assert(root.IsDirty, "native edited document dirty state");
        string sourceBeforeBlockedSave = Hash(source);
        NativeProgram.GetField("_rootPath", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, source);
        bool blockedSave = false;
        try { blockedSave = !(bool)CallProgram("Save", new[] { typeof(bool) }, false); }
        catch (TargetInvocationException error) when (error.InnerException is IOException) { blockedSave = true; }
        Assert(blockedSave && root.IsDirty && Hash(source) == sourceBeforeBlockedSave, "actual application Save refuses source overwrite and preserves pending edits");
        Log("PASS: actual Program.Save refuses mounted source overwrite; source hash and dirty edits retained");

        string saved = Path.Combine(_output, root is ARCNode ? "edited-gui.pac" : "edited-gui.brres");
        // Supply a distinct output path to the native save handler; this bypasses
        // only the OS Save As file picker, not rebuilding or serialization.
        NativeProgram.GetField("_rootPath", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, saved);
        Assert((bool)CallProgram("Save", new[] { typeof(bool) }, false), "actual application Save handler");
        (verify ?? VerifyNative)(saved, 35);
        if (companions != null) VerifyPreservedBase(saved);
        Log("PASS: actual Program.Save wrote separate " + Path.GetExtension(saved).ToUpperInvariant() + "; native decode verifies edited key");
        viewer.Close();
        Pump(100);
        Assert((bool)CallProgram("Open", new[] { typeof(string), typeof(bool) }, saved, false), "main reopen native output");
        Pump(150);
        var reopened = (ResourceNode)NativeProgram.GetProperty("RootNode").GetValue(null, null);
        main.TargetResource(Find<CHR0Node>(reopened));
        CaptureWindow(main, "06-reopened-native.png");
        Assert(Math.Abs(Find<CHR0Node>(reopened).Children.OfType<CHR0EntryNode>().Single().GetKeyframe(5, 15)._value - 35) < .01f, "reopened application edited key");
        Log("PASS: actual MainForm reopens saved native container and edited CHR0");
        CallProgram("Close", Type.EmptyTypes);
        main.Close();
    }

    private static void VerifyNative(string path, float rotation)
    {
        using (var root = NodeFactory.FromFile(null, path))
        {
            Assert(root is BRRESNode, "native BRRES parser");
            var model = Find<MDL0Node>(root);
            model.Populate();
            Assert(model.Version == 11 && model.BoneCache.Length == 2 && model._objList.Count == 1, "two-bone MDL0 v11");
            var mesh = (MDL0ObjectNode)model._objList[0];
            Assert(mesh.VertexCount == 4, "original four-vertex mesh");
            foreach (float x in new[] { -3f, 3f }) foreach (float y in new[] { 0f, 18f })
                Assert(mesh.Vertices.Any(v => Math.Abs(v.Position._x - x) < .0001f && Math.Abs(v.Position._y - y) < .0001f && Math.Abs(v.Position._z) < .0001f), "original mesh positions preserved");
            foreach (var bone in model.BoneCache.OfType<MDL0BoneNode>())
                Assert(bone.BindMatrix == Matrix.Identity && bone.InverseBindMatrix == Matrix.Identity, "exact identity bind and inverse-bind matrices preserved");
            Assert(model._matList.Count == 1, "one original native material");
            Assert(model._matList[0].Children.OfType<MDL0MaterialRefNode>().Single().Texture == "OriginalQuadrants", "material texture binding");
            var texture = Find<TEX0Node>(root);
            using (var bitmap = texture.GetImage(0))
            {
                Assert(bitmap.Width == 32 && bitmap.Height == 32, "native TEX0 dimensions");
                Assert(bitmap.GetPixel(3, 3).ToArgb() == Color.Red.ToArgb(), "RGBA8 red texel");
                Assert(bitmap.GetPixel(24, 24).ToArgb() == Color.Yellow.ToArgb(), "RGBA8 yellow texel");
            }
            var animation = Find<CHR0Node>(root);
            Assert(animation.Version == 4 && animation.FrameCount == 31 && animation.Loop, "CHR0 version/timing/loop");
            var entry = animation.Children.OfType<CHR0EntryNode>().Single();
            Assert(entry.Name == "Paddle" && Math.Abs(entry.GetKeyframe(5, 15)._value - rotation) < .01f, "decoded rotation key");
        }
    }

    private static T Find<T>(ResourceNode node) where T : ResourceNode
    {
        if (node is T result) return result;
        foreach (var child in node.Children)
        {
            var found = Find<T>(child);
            if (found != null) return found;
        }
        return null;
    }
    private static object CallProgram(string name, Type[] types, params object[] values) =>
        NativeProgram.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null).Invoke(null, values);
    private static void VerifySafetyGates()
    {
        Assert(typeof(MainForm).Assembly.GetName().Name == "FighterPort.Studio", "independent application assembly identity");
        object[] arguments = { false, null };
        bool updater = (bool)NativeProgram.GetMethod("CanRunGithubApp", BindingFlags.Static | BindingFlags.Public).Invoke(null, arguments);
        Assert(!updater && arguments[1] == null, "inherited updater/process path unavailable");
        Assert(!(bool)NativeProgram.GetProperty("CanRunDiscordRPC").GetValue(null, null), "inherited RPC gate disabled");
        var settingsType = typeof(MainForm).Assembly.GetType("BrawlCrate.Properties.Settings", true);
        var settings = (ApplicationSettingsBase)settingsType.GetProperty("Default").GetValue(null, null);
        object previous = settings["DiscordRPCEnabled"];
        try
        {
            settings["DiscordRPCEnabled"] = true;
            var discord = typeof(MainForm).Assembly.GetType("BrawlCrate.Discord.DiscordSettings", true);
            discord.GetMethod("LoadSettings").Invoke(null, new object[] { true });
            Assert(!(bool)discord.GetProperty("DiscordControllerSet").GetValue(null, null), "direct RPC settings path stays disabled with enabled preference");
        }
        finally { settings["DiscordRPCEnabled"] = previous; }
        Log("PASS: independent assembly identity and runtime updater/RPC safety gates, including direct settings path");
    }
    private static void VerifySettingsPersistence()
    {
        var library = new BrawlLib.Properties.Settings();
        library.AudioVolumePercent = .625;
        library.Codes = new List<CodeStorage> { new CodeStorage { _name = "OriginalSettingsFixture", _description = "Original inert test text", _code = "original fixture value" } };
        library.ColladaImportOptions = new BrawlLib.Modeling.Collada.Collada.ImportOptions { ModelVersion = 9, ForceFloatVertices = true };
        library.Save();
        var settingsType = typeof(MainForm).Assembly.GetType("BrawlCrate.Properties.Settings", true);
        var settings = (ApplicationSettingsBase)Activator.CreateInstance(settingsType, true);
        var viewer = ModelEditorSettings.Default();
        viewer._width = 987; viewer._height = 654;
        var recent = new StringCollection(); recent.Add("original-one.brres"); recent.Add("original-two.brres");
        settings["RecentFiles"] = recent;
        settings["RecentFilesMax"] = 7;
        settings["ViewerSettings"] = viewer;
        settings["ViewerSettingsSet"] = true;
        settings.Save();
        var binary = new BinaryFixtureSettings { OriginalBytes = new byte[] { 0, 1, 2, 128, 255 }, OriginalLines = "a\r\nb\rc\n" };
        binary.Save();
        var loadedBinary = new BinaryFixtureSettings(); loadedBinary.Reload();
        Assert(loadedBinary.OriginalBytes.SequenceEqual(binary.OriginalBytes), "explicit binary settings representation round trip");
        Assert(loadedBinary.OriginalLines == binary.OriginalLines, "literal CR/CRLF/LF string settings preservation");
        loadedBinary.Reset();
        Assert(new BinaryFixtureSettings().OriginalBytes == null, "settings reset removes only selected test group");
        AssertPersistedSettings();
        using (var child = Process.Start(new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            Arguments = "--verify-settings \"" + _output + "\"",
            UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true, RedirectStandardError = true
        }))
        {
            if (!child.WaitForExit(10000)) { child.Kill(); throw new TimeoutException("Independent settings reload probe"); }
            Assert(child.ExitCode == 0, "persistent settings: " + child.StandardError.ReadToEnd());
            Log(child.StandardOutput.ReadToEnd().Trim());
        }
        Log("PASS: actual independent settings provider persists/reloads scalar, collection, import-options, complex viewer and binary settings; scoped reset retains application/library groups");
    }
    private static void AssertPersistedSettings()
    {
        var loadedLibrary = new BrawlLib.Properties.Settings();
        loadedLibrary.Reload();
        Assert(loadedLibrary.AudioVolumePercent == .625 && loadedLibrary.ColladaImportOptions.ModelVersion == 9 && loadedLibrary.ColladaImportOptions.ForceFloatVertices, "library scalar/complex settings round trip");
        Assert(loadedLibrary.Codes.Count == 1 && loadedLibrary.Codes[0]._description == "Original inert test text", "library collection settings round trip");
        var settingsType = typeof(MainForm).Assembly.GetType("BrawlCrate.Properties.Settings", true);
        var loaded = (ApplicationSettingsBase)Activator.CreateInstance(settingsType, true);
        loaded.Reload();
        var loadedViewer = (ModelEditorSettings)loaded["ViewerSettings"];
        Assert((int)loaded["RecentFilesMax"] == 7 && ((StringCollection)loaded["RecentFiles"])[1] == "original-two.brres", "application scalar/collection settings round trip");
        Assert(loadedViewer._width == 987 && loadedViewer._height == 654 && loadedViewer._viewports.Count == 1, "application complex viewer settings round trip");
    }
    private static void Assert(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static void Expect<T>(Action action, string message) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException(message); }
    private static void Log(string message) { Results.Add(message); Console.WriteLine(message); File.AppendAllText(Path.Combine(_output, "progress.txt"), message + "\r\n"); }
    private static string Hash(string path) { using (var sha = SHA256.Create()) using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(); }
    private static void Pump(int milliseconds) { var watch = Stopwatch.StartNew(); while (watch.ElapsedMilliseconds < milliseconds) { Application.DoEvents(); Thread.Sleep(5); } }
    private static void WaitFor(Func<bool> condition, string message) { var watch = Stopwatch.StartNew(); while (!condition() && watch.ElapsedMilliseconds < 10000) Pump(20); Assert(condition(), message); }
    private static void CaptureWindow(Form form, string name)
    {
        form.Refresh(); Pump(120);
        using (var bitmap = new Bitmap(form.Width, form.Height))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            IntPtr dc = graphics.GetHdc();
            try { Assert(PrintWindow(form.Handle, dc, 2), "app-only PrintWindow capture"); }
            finally { graphics.ReleaseHdc(dc); }
            bitmap.Save(Path.Combine(_output, name), ImageFormat.Png);
        }
    }
    private static void CaptureViewport(ModelEditControl editor, string name)
    {
        editor.ModelPanel.Capture();
        editor.ModelPanel.Invalidate(); editor.ModelPanel.Update(); Pump(80);
        editor.ModelPanel.Capture();
        using (var bitmap = editor.ModelPanel.GetScreenshot(editor.ModelPanel.CurrentViewport.RegionNoBorder, false))
        {
            bitmap.Save(Path.Combine(_output, name), ImageFormat.Png);
            var colors = new HashSet<int>();
            for (int y = 0; y < bitmap.Height; y += 3)
                for (int x = 0; x < bitmap.Width; x += 3) colors.Add(bitmap.GetPixel(x, y).ToArgb());
            Assert(colors.Count > 8, "native OpenGL framebuffer must contain drawn content; blank capture is not preview evidence");
        }
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetShortPathName(string path, System.Text.StringBuilder result, uint capacity);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLink(string fileName, string existingFileName, IntPtr reserved);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PrintWindow(IntPtr window, IntPtr dc, uint flags);
}

[SettingsProvider(typeof(FighterPortSettingsProvider))]
internal sealed class BinaryFixtureSettings : ApplicationSettingsBase
{
    [UserScopedSetting]
    public string OriginalLines { get => (string)this[nameof(OriginalLines)]; set => this[nameof(OriginalLines)] = value; }

    [UserScopedSetting]
    [SettingsSerializeAs(SettingsSerializeAs.Binary)]
    public byte[] OriginalBytes { get => (byte[])this[nameof(OriginalBytes)]; set => this[nameof(OriginalBytes)] = value; }
}
