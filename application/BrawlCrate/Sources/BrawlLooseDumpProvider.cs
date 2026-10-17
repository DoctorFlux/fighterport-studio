// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlLib.Internal.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace BrawlCrate.Sources
{
    public sealed class SourceMount
    {
        public string Id { get; internal set; }
        public string Label { get; internal set; }
        public string Root { get; internal set; }
        public int Priority { get; internal set; }
        public bool Enabled { get; internal set; } = true;
    }

    // Root configuration is not an edit project. Snapshots also accompany pinned resolutions.
    public sealed class BrawlDumpProfile
    {
        private readonly List<SourceMount> _mounts = new List<SourceMount>();
        public IReadOnlyList<SourceMount> Mounts => _mounts.AsReadOnly();
        public const int MaxRoots = 16;
        public static string DefaultPath => Path.Combine(FighterPortPaths.DataRoot, "SourceProfiles", "brawl-loose.xml");
        public SourceMount Add(string root, string label, int priority)
        {
            var mount = new SourceMount { Id = Guid.NewGuid().ToString("N"), Root = LocalAbsolute(root), Label = label, Priority = priority };
            _mounts.Add(mount);
            try { Validate(); } catch { _mounts.Remove(mount); throw; }
            return mount;
        }
        public void Configure(string id, int priority, bool enabled)
        {
            if (priority < -999 || priority > 999) throw new IOException("Priority must be -999 to 999; larger numbers win.");
            var mount = _mounts.Single(m => m.Id == id); mount.Priority = priority; mount.Enabled = enabled;
        }
        public void Remove(string id) => _mounts.RemoveAll(m => m.Id == id);
        public void Validate()
        {
            if (_mounts.Count > MaxRoots) throw new IOException("At most 16 dump roots are supported.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var mount in _mounts)
            {
                if (!Guid.TryParseExact(mount.Id, "N", out _) || !ids.Add(mount.Id)) throw new IOException("Invalid or duplicate mount identity.");
                if (string.IsNullOrWhiteSpace(mount.Label) || mount.Label.Length > 120 || mount.Priority < -999 || mount.Priority > 999)
                    throw new IOException("Invalid root label/priority.");
                mount.Root = LocalAbsolute(mount.Root);
            }
            // Overlapping mounts have two identities for one physical tree. Reject, even when disabled.
            foreach (var a in _mounts)
                foreach (var b in _mounts.Where(b => b != a))
                    if (SourceWriteGuard.IsWithin(a.Root, b.Root)) throw new IOException("Dump roots must not overlap: " + a.Label + " / " + b.Label);
        }
        internal static string LocalAbsolute(string path)
        {
            if (path == null || path.Length < 3 || !char.IsLetter(path[0]) || path[1] != ':' || (path[2] != '\\' && path[2] != '/'))
                throw new IOException("Use an absolute local drive path, not a relative/network/device path.");
            return SourceWriteGuard.NormalizePath(path);
        }
        internal XmlElement Write(XmlDocument doc)
        {
            Validate();
            var root = doc.CreateElement("brawlDumpRoots"); root.SetAttribute("version", "1");
            foreach (var mount in _mounts)
            {
                var e = doc.CreateElement("mount"); e.SetAttribute("id", mount.Id); e.SetAttribute("label", mount.Label);
                e.SetAttribute("path", mount.Root); e.SetAttribute("priority", mount.Priority.ToString(System.Globalization.CultureInfo.InvariantCulture));
                e.SetAttribute("enabled", mount.Enabled ? "true" : "false"); root.AppendChild(e);
            }
            return root;
        }
        internal static BrawlDumpProfile Read(XmlElement root)
        {
            if (root?.Name != "brawlDumpRoots" || root.GetAttribute("version") != "1" || root.ChildNodes.Count > MaxRoots)
                throw new IOException("Unsupported or oversized dump-root profile.");
            var profile = new BrawlDumpProfile();
            foreach (XmlElement e in root.ChildNodes.OfType<XmlElement>())
            {
                if (e.Name != "mount" || !int.TryParse(e.GetAttribute("priority"), out int priority) || !bool.TryParse(e.GetAttribute("enabled"), out bool enabled))
                    throw new IOException("Invalid dump-root record.");
                profile._mounts.Add(new SourceMount { Id = e.GetAttribute("id"), Label = e.GetAttribute("label"), Root = e.GetAttribute("path"), Priority = priority, Enabled = enabled });
            }
            profile.Validate(); return profile;
        }
        public BrawlDumpProfile Snapshot()
        {
            var doc = new XmlDocument { XmlResolver = null }; return Read(Write(doc));
        }
        public void Save(string path)
        {
            var doc = new XmlDocument { XmlResolver = null }; doc.AppendChild(Write(doc)); WriteAtomic(doc, path);
        }
        internal static void WriteAtomic(XmlDocument doc, string path)
        {
            path = LocalAbsolute(path); SourceWriteGuard.ThrowIfProtected(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            SourceWriteGuard.ThrowIfProtected(temporary);
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { doc.Save(stream); stream.Flush(true); }
                SourceWriteGuard.ThrowIfProtected(path);
                if (File.Exists(path)) File.Replace(temporary, path, null); else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        public static BrawlDumpProfile Load(string path)
        {
            path = LocalAbsolute(path); SourceWriteGuard.EnsureNoReparsePoints(path);
            if (new FileInfo(path).Length > 65536) throw new IOException("Dump-root profile exceeds 64 KiB.");
            var doc = new XmlDocument { XmlResolver = null };
            using (var reader = System.Xml.XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 65536 })) doc.Load(reader);
            return Read(doc.DocumentElement);
        }
    }

    public sealed class DumpCandidate
    {
        public string MountId { get; internal set; }
        public string Root { get; internal set; }
        public string RootLabel { get; internal set; }
        public int Priority { get; internal set; }
        public string RelativePath { get; internal set; }
        public string Path { get; internal set; }
        public string Signature { get; internal set; }
        public long Length { get; internal set; }
        public long LastWriteTicks { get; internal set; }
        // The *entire container/file path*, not the names of resources inside it.
        public string LogicalKey => RelativePath;
        public string Identity => MountId + ":" + RelativePath;
    }

    public sealed class DumpDiscoveryBudget
    {
        public int MaxEntries { get; set; } = 8192;
        public int MaxDepth { get; set; } = 16;
        public int MaxDirectories { get; set; } = 1024;
        public int MaxCandidates { get; set; } = 2048;
        public long MaxProbeBytes { get; set; } = 128 * 1024;
        public long MaxMetadataBytes { get; set; } = 4 * 1024 * 1024;
        internal DumpDiscoveryBudget Copy()
        {
            if (MaxEntries < 1 || MaxEntries > 8192 || MaxDepth < 0 || MaxDepth > 16 || MaxDirectories < 1 || MaxDirectories > 1024 ||
                MaxCandidates < 1 || MaxCandidates > 2048 || MaxProbeBytes < 16 || MaxProbeBytes > 128 * 1024 || MaxMetadataBytes < 1 || MaxMetadataBytes > 4 * 1024 * 1024)
                throw new IOException("Discovery budgets must be positive and no larger than the bounded defaults (depth may be zero).");
            return (DumpDiscoveryBudget)MemberwiseClone();
        }
    }

    // A resumable metadata cursor. Nothing is staged/decoded by constructing it or reading pages.
    public sealed class BrawlLooseDumpProvider : IDisposable
    {
        private sealed class Folder { public SourceMount Mount; public string Path; public int Depth; public SourceFolderSession Session; }
        private readonly Queue<Folder> _folders = new Queue<Folder>();
        private readonly List<DumpCandidate> _candidates = new List<DumpCandidate>();
        private readonly List<string> _issues = new List<string>();
        private readonly BrawlDumpProfile _profile;
        private readonly DumpDiscoveryBudget _budget;
        private readonly string _storage;
        private IEnumerator<SourceEntry> _cursor;
        private Folder _folder;
        private bool _started;
        private int _directoryCount;
        private long _metadataBytes;
        public IReadOnlyList<DumpCandidate> Candidates => _candidates.AsReadOnly();
        public IReadOnlyList<string> Issues => _issues.AsReadOnly();
        public int EntriesVisited { get; private set; }
        public long ProbeByteBudgetUsed => EntriesVisited * 16L; // Conservative charge includes directories/rejected entries.
        public bool Finished { get; private set; }
        public bool Cancelled { get; private set; }
        public bool Truncated { get; private set; }
        public bool Complete => Finished && !Cancelled && !Truncated && _issues.Count == 0;
        public string Status => (Complete ? "Complete" : Cancelled ? "Cancelled; partial catalogue" : Finished ? "Incomplete catalogue" : "Partial catalogue; discover next page") +
            "; " + EntriesVisited + " entries, " + _candidates.Count + " candidate files; " + ProbeByteBudgetUsed + " header-byte budget used.";

        public BrawlLooseDumpProvider(BrawlDumpProfile profile, string storageRoot, DumpDiscoveryBudget budget = null)
        { _profile = profile.Snapshot(); _storage = storageRoot; _budget = (budget ?? new DumpDiscoveryBudget()).Copy(); }

        public void ReadNext(int entries, CancellationToken token, Action<string> progress = null)
        {
            if (entries < 1 || entries > 512) throw new ArgumentOutOfRangeException(nameof(entries));
            if (Finished) return;
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_started)
                {
                    _started = true;
                    if (!_profile.Mounts.Any(m => m.Enabled)) { Issue("No enabled dump roots. Add or enable a root before discovery."); Finished = true; return; }
                    foreach (var mount in _profile.Mounts.Where(m => m.Enabled))
                    {
                        token.ThrowIfCancellationRequested();
                        if (_directoryCount >= _budget.MaxDirectories) { Limit("Root/directory budget exhausted"); return; }
                        try { _folders.Enqueue(new Folder { Mount = mount, Path = mount.Root, Session = new SourceFolderSession(mount.Root, _storage) }); _directoryCount++; }
                        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException) { Issue(mount.Label + ": " + error.Message); }
                    }
                }
                int pageEntries = 0;
                while (pageEntries < entries)
                {
                    token.ThrowIfCancellationRequested();
                    if (_cursor == null)
                    {
                        if (_folders.Count == 0) { Finished = true; break; }
                        _folder = _folders.Dequeue(); _cursor = _folder.Session.DiscoverDirectory(_folder.Path).GetEnumerator();
                    }
                    if (EntriesVisited >= _budget.MaxEntries || ProbeByteBudgetUsed + 16 > _budget.MaxProbeBytes) { Limit("Entry/header-byte budget exhausted"); break; }
                    SourceEntry entry;
                    try { if (!_cursor.MoveNext()) { CloseCursor(); continue; } entry = _cursor.Current; }
                    catch (Exception error) when (error is IOException || error is UnauthorizedAccessException) { Issue(_folder.Path + ": " + error.Message); CloseCursor(); continue; }
                    EntriesVisited++; pageEntries++;
                    progress?.Invoke(_folder.Mount.Label + ": " + entry.Name + " (" + EntriesVisited + " entries)");
                    token.ThrowIfCancellationRequested();
                    if (entry.Truncated) { Truncated = true; Issue(entry.Description + ": " + entry.Path); continue; }
                    if (entry.IsDirectory)
                    {
                        if (_folder.Depth >= _budget.MaxDepth) { Truncated = true; Issue("Depth budget reached: " + entry.Path); continue; }
                        if (_directoryCount >= _budget.MaxDirectories) { Limit("Directory budget exhausted"); break; }
                        long folderBytes = 512 + 2L * entry.Path.Length;
                        if (_metadataBytes + folderBytes > _budget.MaxMetadataBytes) { Limit("Metadata cache budget exhausted"); break; }
                        _metadataBytes += folderBytes;
                        _directoryCount++; _folders.Enqueue(new Folder { Mount = _folder.Mount, Session = _folder.Session, Path = entry.Path, Depth = _folder.Depth + 1 });
                        continue;
                    }
                    if (!entry.CanOpen) { Issue(entry.Path + ": " + entry.Description); continue; }
                    if (_candidates.Count >= _budget.MaxCandidates) { Limit("Candidate-file budget exhausted"); break; }
                    string relative = entry.Path.Substring(_folder.Mount.Root.TrimEnd('\\', '/').Length + 1).Replace('\\', '/');
                    long bytes = 512 + 2L * (entry.Path.Length + relative.Length + _folder.Mount.Root.Length + _folder.Mount.Label.Length);
                    if (_metadataBytes + bytes > _budget.MaxMetadataBytes) { Limit("Metadata cache budget exhausted"); break; }
                    _metadataBytes += bytes;
                    _candidates.Add(new DumpCandidate { MountId = _folder.Mount.Id, Root = _folder.Mount.Root, RootLabel = _folder.Mount.Label,
                        Priority = _folder.Mount.Priority, Path = entry.Path, RelativePath = relative, Signature = entry.Signature, Length = entry.Length, LastWriteTicks = entry.LastWriteTicks });
                }
            }
            catch (OperationCanceledException) { Cancelled = Finished = true; CloseCursor(); _folders.Clear(); }
        }
        public string Explain(DumpCandidate candidate)
        {
            if (!_candidates.Contains(candidate)) throw new IOException("Candidate is not in this catalogue.");
            var group = _candidates.Where(c => string.Equals(c.LogicalKey, candidate.LogicalKey, StringComparison.OrdinalIgnoreCase)).ToList();
            int maximum = group.Max(c => c.Priority), tied = group.Count(c => c.Priority == maximum);
            string reason = candidate.Priority < maximum ? "Shadowed by priority " + maximum : tied > 1 ? "Ambiguous: " + tied + " roots tied at priority " + maximum :
                group.Count == 1 ? "Winner: only candidate for this logical file" : "Winner: highest priority " + maximum + " for this logical file";
            return reason + (Complete ? "" : " (partial catalogue; no final winner guaranteed)");
        }
        private void Issue(string text)
        {
            if (_issues.Count < 128) _issues.Add(text.Length > 800 ? text.Substring(0, 800) : text);
            else Truncated = true;
        }
        private void Limit(string text) { Truncated = Finished = true; Issue(text + "; use narrower roots"); CloseCursor(); _folders.Clear(); }
        private void CloseCursor() { _cursor?.Dispose(); _cursor = null; }
        public void Dispose() { if (!Finished) Cancelled = true; CloseCursor(); _folders.Clear(); Finished = true; }
    }
}
