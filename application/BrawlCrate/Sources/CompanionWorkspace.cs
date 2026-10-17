// FighterPort Studio addition, 2026-10-17. Licensed under the repository LGPL-3.0 terms.
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;

namespace BrawlCrate.Sources
{
    public sealed class CompanionResource
    {
        public string SourcePath { get; internal set; }
        public string SourceHash { get; internal set; }
        public string SourceContainer { get; internal set; }
        public string TreePath { get; internal set; }
        public string Name { get; internal set; }
        public string Kind { get; internal set; }
        public int Version { get; internal set; }
        public string Detail { get; internal set; }
        public string ContainerPath { get; internal set; }
        public string ContainerIdentity { get; internal set; }
        public string MountId { get; internal set; }
        public string MountRoot { get; internal set; }
        public string MountLabel { get; internal set; }
        public int MountPriority { get; internal set; }
        public string LogicalPath { get; internal set; }
        public string SelectionReason { get; internal set; }
        public string Identity => SourceHash + ":" + TreePath + ":" + Kind + ":" + Name;
        public override string ToString() => Name + " \u2014 " + Kind + " v" + Version + " [" + TreePath + "] \u2014 " + ContainerIdentity + " \u2014 " +
            (MountId == null ? SourcePath : MountLabel + " (priority " + MountPriority + ") / " + LogicalPath);
    }

    /// <summary>Brawl-specific explicit dependency choices, not a game-neutral project or retargeter.</summary>
    public sealed class CompanionWorkspace
    {
        private sealed class Source
        {
            public StagedSource File;
            public readonly List<CompanionResource> Resources = new List<CompanionResource>();
            public readonly List<string> OtherResources = new List<string>();
        }
        private readonly string _storageRoot;
        private readonly List<Source> _sources = new List<Source>();
        private readonly Dictionary<string, CompanionResource> _textures = new Dictionary<string, CompanionResource>(StringComparer.Ordinal);
        private readonly Dictionary<string, CompanionResource> _palettes = new Dictionary<string, CompanionResource>(StringComparer.Ordinal);
        private readonly List<string> _required = new List<string>();
        private readonly List<string> _requiredPalettes = new List<string>();
        public CompanionResource Model { get; private set; }
        public CompanionResource Motion { get; private set; }
        public string StorageRoot => _storageRoot;
        public BrawlDumpProfile DumpProfile { get; private set; } = new BrawlDumpProfile();
        public void SetDumpProfile(BrawlDumpProfile profile)
        {
            var snapshot = profile.Snapshot();
            foreach (var mount in snapshot.Mounts.Where(m => m.Enabled && Directory.Exists(m.Root))) new SourceFolderSession(mount.Root, _storageRoot);
            DumpProfile = snapshot;
        }
        public IReadOnlyList<CompanionResource> Resources => _sources.SelectMany(s => s.Resources).ToList();
        public IReadOnlyList<string> RequiredTextures => _required.AsReadOnly();
        public IReadOnlyDictionary<string, CompanionResource> TextureSelections => _textures;
        public IReadOnlyList<string> RequiredPalettes => _requiredPalettes.AsReadOnly();
        public IReadOnlyDictionary<string,CompanionResource> PaletteSelections => _palettes;
        public string AdditionalResourceNotice => string.Join("; ", _sources.Where(s => s.OtherResources.Count != 0).Select(s =>
            Path.GetFileName(s.File.SourcePath) + ": " + string.Join(", ", s.OtherResources.Take(8).Select(n => n.Length > 160 ? n.Substring(0, 160) + "…" : n)) +
            (s.OtherResources.Count > 8 ? " (and " + (s.OtherResources.Count - 8) + " more native resources)" : "") +
            (Model != null && string.Equals(s.File.SourcePath, Model.SourcePath, StringComparison.OrdinalIgnoreCase) ? " retained in base" : " not imported from this companion")));
        public static string LastResolutionPath => Path.Combine(FighterPortPaths.DataRoot, "CompanionResolutions", "last.fpcompanion");
        public const string ScopeNotice = "Brawl subset: MDL0 v9/v11, CHR0 v4; TEX0 v1 RGBA8/RGB5A3/RGB565/CMPR/CI8 and explicit PLT0 v1. Normal-material preview only; optional metal materials are retained, not supplied. Full model archive retained; explicitly selected companion BRRES may omit opaque siblings from decoding (full originals/provenance retained). Other channels are not imported. This is not retargeting.";

        public CompanionWorkspace(string storageRoot = null)
        {
            _storageRoot = SourceWriteGuard.NormalizePath(storageRoot ?? FighterPortPaths.DataRoot);
        }

        public void AddDiscoveredSource(BrawlDumpProfile profile, DumpCandidate candidate, string reason, CancellationToken token)
        {
            profile.Validate();
            var mount = profile.Mounts.SingleOrDefault(m => m.Id == candidate.MountId && m.Enabled);
            if (mount == null || mount.Root != candidate.Root || mount.Priority != candidate.Priority)
                throw new IOException("Root configuration changed; restart discovery before staging a new selection.");
            ValidateOrigin(candidate.Path, mount.Root, candidate.RelativePath);
            var session = new SourceFolderSession(mount.Root, _storageRoot);
            SourceEntry current = session.ProbeLoosePath(candidate.Path);
            if (!current.CanOpen || current.Signature != candidate.Signature || current.Length != candidate.Length || current.LastWriteTicks != candidate.LastWriteTicks)
                throw new IOException("Candidate is missing or changed since discovery; rescan and explicitly select it again: " + candidate.Path);
            // A rescan must never replace a previously pinned file/hash, even at the same path.
            VerifySources(token);
            AddSource(candidate.Path, token, candidate, "Explicit file choice: " + reason);
            SetDumpProfile(profile);
        }

        public void AddSource(string path, CancellationToken token) { AddSource(path, token, null, null); }
        public void AddSelectedContainer(string path,string containerPath,CancellationToken token=default(CancellationToken))=>AddSource(path,token,null,null,containerPath);
        private void AddSource(string path, CancellationToken token, DumpCandidate origin, string reason,string selectedContainer=null)
        {
            path = SourceWriteGuard.NormalizePath(path);
            if (_sources.Any(s => string.Equals(s.File.SourcePath, path, StringComparison.OrdinalIgnoreCase)))
            {if(_sources.Single(s=>string.Equals(s.File.SourcePath,path,StringComparison.OrdinalIgnoreCase)).File.SelectedContainer!=selectedContainer)throw new IOException("This file already has a different pinned container selection; use another workspace rather than rebinding it.");return;}
            if (_sources.Count >= 16) throw new IOException("Select at most 16 companion source files.");
            long length = new FileInfo(path).Length;
            if (length > SourceFolderSession.MaxFileBytes || _sources.Sum(s => s.File.Length) + length > 128L * 1024 * 1024)
                throw new IOException("Companion inventory is limited to 64 MiB per file and 128 MiB total.");
            var session = new SourceFolderSession(Path.GetDirectoryName(path), _storageRoot);
            if(selectedContainer==null)BrawlContainerAudit.Inspect(path, token).RequirePreservable();
            var source = new Source { File = selectedContainer==null?session.Stage(path, token):session.StageContainer(path,selectedContainer,token) };
            // Validate the exact hash-pinned bytes given to the native decoder as well.
            BrawlContainerAudit.Inspect(source.File.StagedPath, token).RequirePreservable();
            using (ResourceNode root = NodeFactory.FromFile(null, source.File.StagedPath))
            {
                if (!(root is ARCNode) && !(root is BRRESNode) && !(root is TEX0Node) && !(root is PLT0Node) && !(root is CHR0Node))
                    throw new IOException("Choose raw ARC/PAC containing BRRES, a model BRRES, texture BRRES/TEX0 or motion BRRES/CHR0. Other resource types are not companion inputs yet.");
                Walk(root, "", (node, treePath) =>
                {
                    token.ThrowIfCancellationRequested();
                    if (_sources.Sum(s => s.Resources.Count + s.OtherResources.Count) + source.Resources.Count + source.OtherResources.Count >= 4096)
                        throw new IOException("Companion inventory exceeds 4096 native resources. Select smaller source files.");
                    string kind = node is MDL0Node ? "MDL0" : node is TEX0Node ? "TEX0" : node is PLT0Node ? "PLT0" : node is CHR0Node ? "CHR0" : null;
                    if (kind == null)
                    {
                        if (node is BRESEntryNode) source.OtherResources.Add(node.GetType().Name + " '" + node.Name + "'");
                        return;
                    }
                    ResourceNode container = EnclosingBrres(node);
                    int version = node is MDL0Node model ? model.Version : node is TEX0Node texture ? texture.Version : node is PLT0Node palette ? palette.Version : ((CHR0Node)node).Version;
                    source.Resources.Add(new CompanionResource { SourcePath = path, SourceHash = source.File.Sha256,SourceContainer=selectedContainer,
                        TreePath = Prefix(selectedContainer,treePath), Name = node.Name, Kind = kind, Version = version,
                        ContainerPath = container == null ? null : Prefix(selectedContainer,NodePath(root, container)), ContainerIdentity = NativeLineage(node)+(selectedContainer==null?"":" / selected original BRRES ["+selectedContainer+"]"),
                        MountId = origin?.MountId, MountRoot = origin?.Root, MountLabel = origin?.RootLabel, MountPriority = origin?.Priority ?? 0,
                        LogicalPath = origin?.RelativePath, SelectionReason = reason,
                        Detail = node is TEX0Node t ? t.Format + (t.HasPalette ? " with palette" : "") : node is PLT0Node p?p.Format.ToString():"" });
                });
            }
            if (source.Resources.Count == 0) throw new IOException("No model, texture or skeletal motion was found in this file.");
            if (origin != null)
            {
                var now = new SourceFolderSession(origin.Root, _storageRoot).ProbeLoosePath(path);
                if (!now.CanOpen || now.Length != origin.Length || now.LastWriteTicks != origin.LastWriteTicks || now.Signature != origin.Signature)
                    throw new IOException("Candidate changed during selection; no choices were applied.");
            }
            _sources.Add(source);
        }

        public void SelectModel(CompanionResource resource)
        {
            RequireMember(resource, "MDL0");
            if(resource.SourceContainer!=null)throw new IOException("The model base must retain its complete supported native archive. Selected-container staging currently admits companion resources only.");
            if (Model == resource) return; // Refreshing the same model must not clear explicit texture choices.
            using (ResourceNode root = OpenSource(resource))
            {
                if (resource.ContainerPath == null) throw new IOException("The model must be selected from a BRRES so its native container and other resources can be retained.");
                var model = (MDL0Node)Locate(root, resource);
                model.Populate();
                var refs=model._matList.OfType<MDL0MaterialNode>().Where(m=>!m.IsMetal).SelectMany(m=>m.Children.OfType<MDL0MaterialRefNode>()).ToList();
                var names = refs
                    .Select(r => r.Texture).Where(n => !string.IsNullOrEmpty(n)).Distinct(StringComparer.Ordinal).OrderBy(n => n, StringComparer.Ordinal).ToList();
                if (names.Count > 64) throw new IOException("This companion slice supports at most 64 texture bindings.");
                _required.Clear(); _required.AddRange(names);
                _requiredPalettes.Clear();_requiredPalettes.AddRange(refs.Select(r=>r.Palette).Where(n=>!string.IsNullOrEmpty(n)).Distinct(StringComparer.Ordinal).OrderBy(n=>n,StringComparer.Ordinal));
                if(_requiredPalettes.Count>64)throw new IOException("At most 64 palette bindings are supported.");
            }
            Model = resource;
            _textures.Clear();
            _palettes.Clear();
        }

        public void SelectMotion(CompanionResource resource) { RequireMember(resource, "CHR0"); Motion = resource; }
        public void SelectTexture(string bindingName, CompanionResource resource)
        {
            RequireMember(resource, "TEX0");
            if (!_required.Contains(bindingName) || resource.Name != bindingName)
                throw new IOException("Select an exact-name texture for this binding. Texture renaming/remapping is not implemented here.");
            _textures[bindingName] = resource;
        }
        public IList<CompanionResource> TextureCandidates(string name) => Resources.Where(r => r.Kind == "TEX0" && r.Name == name).ToList();
        public IList<CompanionResource> PaletteCandidates(string name)=>Resources.Where(r=>r.Kind=="PLT0" && r.Name==name).ToList();
        public void SelectPalette(string bindingName,CompanionResource resource)
        {RequireMember(resource,"PLT0");if(!_requiredPalettes.Contains(bindingName) || resource.Name!=bindingName)throw new IOException("Choose an exact explicit palette binding.");_palettes[bindingName]=resource;}
        public string PaletteStatus(string name)=>_palettes.TryGetValue(name,out var p)?"Selected: "+p:PaletteCandidates(name).Count==0?"Missing palette":"Choose palette explicitly ("+PaletteCandidates(name).Count+" candidates)";
        public string TextureStatus(string name)
        {
            if (_textures.TryGetValue(name, out CompanionResource selected)) return "Selected: " + selected;
            int count = TextureCandidates(name).Count;
            return count == 0 ? "Missing: add a texture source" : count == 1 ? "One candidate; select explicitly" : "Ambiguous: " + count + " candidates; select explicitly";
        }

        public IList<string> Validate()
        {
            var errors = new List<string>();
            if (Model == null) errors.Add("Select a model from its BRRES.");
            else if (Model.Version != 9 && Model.Version != 11) errors.Add("This companion slice supports MDL0 v9/v11 only.");
            if (Motion == null) errors.Add("Select a skeletal motion explicitly.");
            else if (Motion.Version != 4) errors.Add("This companion slice supports CHR0 v4 only.");
            foreach (string name in _required)
            {
                if (!_textures.TryGetValue(name, out CompanionResource texture)) errors.Add(name + ": " + TextureStatus(name));
                else if (texture.Version != 1 || !new[]{"RGBA8","RGB5A3","RGB565","CMPR","CI8 with palette"}.Contains(texture.Detail)) errors.Add(name + ": unsupported texture version/format.");
            }
            foreach(string name in _requiredPalettes)if(!_palettes.TryGetValue(name,out var p))errors.Add(name+": "+PaletteStatus(name));else if(p.Version!=1 || !new[]{"RGB565","RGB5A3"}.Contains(p.Detail))errors.Add(name+": unsupported palette version/format.");
            if (Model != null && Motion != null)
            {
                using (ResourceNode modelRoot = OpenSource(Model))
                using (ResourceNode motionRoot = OpenSource(Motion))
                {
                    if (Container(modelRoot, Model).Children.OfType<BRESGroupNode>().Any(g => g.Children.GroupBy(n => n.Name, StringComparer.Ordinal).Any(group => group.Count() > 1)))
                        errors.Add("The base BRRES contains duplicate resource names; resolve them in the native editor before assembling companions.");
                    var model = (MDL0Node)Locate(modelRoot, Model); model.Populate();
                    foreach(var r in model._matList.OfType<MDL0MaterialNode>().Where(m=>!m.IsMetal).SelectMany(m=>m.Children.OfType<MDL0MaterialRefNode>()))
                        if(_textures.TryGetValue(r.Texture??"",out var t) && t.Detail=="CI8 with palette" && (string.IsNullOrEmpty(r.Palette) || r.Palette!=r.Texture || !_palettes.ContainsKey(r.Palette)))errors.Add("Indexed texture requires its explicit same-name material palette in this admission slice: "+r.Texture);
                    var bones = new HashSet<string>(model.BoneCache.OfType<MDL0BoneNode>().Select(b => b.Name), StringComparer.Ordinal);
                    var motion = (CHR0Node)Locate(motionRoot, Motion);
                    foreach (ResourceNode entry in motion.Children)
                        if (!bones.Contains(entry.Name)) errors.Add("Motion target has no exact bone match: " + entry.Name + ". No retargeting is performed.");
                }
            }
            var used = new[] { Model, Motion }.Concat(_textures.Values).Concat(_palettes.Values).Where(r => r != null).Select(r => r.SourcePath).Distinct(StringComparer.OrdinalIgnoreCase);
            if (_sources.Where(s => used.Contains(s.File.SourcePath, StringComparer.OrdinalIgnoreCase)).Sum(s => s.File.Length) > SourceFolderSession.MaxFileBytes)
                errors.Add("Selected source files exceed the conservative 64 MiB assembly budget.");
            return errors;
        }

        public string Assemble(CancellationToken token)
        {
            IList<string> errors = Validate();
            if (errors.Count != 0) throw new IOException(string.Join(Environment.NewLine, errors));
            VerifySources(token);
            string workRoot = Path.Combine(_storageRoot, "CompanionWorkspaces");
            SourceWriteGuard.ThrowIfProtected(workRoot);
            Directory.CreateDirectory(workRoot);
            var sessions = Directory.GetDirectories(workRoot);
            if (sessions.Length >= 128 || sessions.Sum(d => Directory.GetFiles(d).Sum(f => new FileInfo(f).Length)) > 1024L * 1024 * 1024 - SourceFolderSession.MaxFileBytes)
                throw new IOException("Companion workspace quota reached (128 sessions / 1 GiB). Archive your edits before removing old sessions.");
            string folder = Path.Combine(workRoot, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            string output;
            using (var root = OpenSource(Model))
            {
                output = Path.Combine(folder, root is ARCNode ? "assembled.pac" : "assembled.brres");
                BRRESNode target = Container(root, Model);
                foreach (var pair in _textures) Import<TEX0Node>(target, pair.Value, folder, token);
                foreach (var pair in _palettes) Import<PLT0Node>(target,pair.Value,folder,token);
                Import<CHR0Node>(target, Motion, folder, token);
                token.ThrowIfCancellationRequested();
                root.Export(output);
            }
            if (new FileInfo(output).Length > SourceFolderSession.MaxFileBytes) throw new IOException("Assembled output exceeds 64 MiB; it was not opened.");
            ValidateOutput(output);
            BrawlContainerAudit.VerifyPreserved(Model.SourcePath, output, Model.ContainerPath, token);
            VerifySources(token);
            Save(Path.Combine(folder, "resolution.fpcompanion"));
            return output;
        }

        private void ValidateOutput(string output)
        {
            using (var decoded = NodeFactory.FromFile(null, output))
            using (ResourceNode modelSource = OpenSource(Model))
            using (ResourceNode motionSource = OpenSource(Motion))
            {
                if (decoded == null || decoded.GetType() != modelSource.GetType()) throw new IOException("Assembled native container type changed.");
                var entries = Container(decoded, Model).Children.OfType<BRESGroupNode>().SelectMany(g => g.Children).ToList();
                foreach (ResourceNode original in Container(modelSource, Model).Children.OfType<BRESGroupNode>().SelectMany(g => g.Children))
                    if (!entries.Any(n => n.GetType() == original.GetType() && n.Name == original.Name))
                        throw new IOException("A base resource was lost during native assembly: " + original.Name);
                if (entries.OfType<MDL0Node>().Single(n => n.Name == Model.Name).Version != Model.Version)
                    throw new IOException("Assembled model version changed.");
                foreach (var pair in _textures)
                {
                    TEX0Node texture = entries.OfType<TEX0Node>().Single(n => n.Name == pair.Key);
                    var choice=pair.Value;
                    if (texture.Version != choice.Version || texture.Format+(texture.HasPalette?" with palette":"")!=choice.Detail)
                        throw new IOException("Assembled texture metadata changed: " + pair.Key);
                    using(var source=OpenSource(choice))if(NativeBytes(Locate(source,choice))!=NativeBytes(texture))throw new IOException("Selected encoded texture changed during native assembly.");
                }
                foreach(var pair in _palettes){var actualPalette=entries.OfType<PLT0Node>().Single(n=>n.Name==pair.Key);using(var source=OpenSource(pair.Value))if(NativeBytes(Locate(source,pair.Value))!=NativeBytes(actualPalette))throw new IOException("Selected palette changed during native assembly.");}
                CHR0Node expected = (CHR0Node)Locate(motionSource, Motion), actual = entries.OfType<CHR0Node>().Single(n => n.Name == Motion.Name);
                if (actual.Version != expected.Version || actual.FrameCount != expected.FrameCount || actual.Loop != expected.Loop ||
                    !actual.Children.Select(n => n.Name).SequenceEqual(expected.Children.Select(n => n.Name)))
                    throw new IOException("Assembled motion metadata or target names changed.");
                RequireSameMotion(expected,actual);
            }
        }

        // Intake preserves native evaluation, not merely names/frame counts. Unchanged
        // entries use inherited raw moves; catch any accidental re-encoding/loss here.
        internal static void RequireSameMotion(CHR0Node expected,CHR0Node actual)
        {
            if(expected.Version!=actual.Version || expected.FrameCount!=actual.FrameCount || expected.Loop!=actual.Loop || expected.Children.Count!=actual.Children.Count)throw new IOException("Imported native motion timing/version changed.");
            long keys=0;
            for(int track=0;track<expected.Children.Count;track++)
            {
                var a=(CHR0EntryNode)expected.Children[track];var b=(CHR0EntryNode)actual.Children[track];if(a.Name!=b.Name || !new[]{a.UseModelScale,a.UseModelRotate,a.UseModelTranslate,a.ScaleCompensateApply,a.ScaleCompensateParent,a.ClassicScaleOff}.SequenceEqual(new[]{b.UseModelScale,b.UseModelRotate,b.UseModelTranslate,b.ScaleCompensateApply,b.ScaleCompensateParent,b.ClassicScaleOff}))throw new IOException("Imported native motion identity/evaluator flags changed.");
                for(int channel=0;channel<9;channel++){var x=a.KeyArrays[channel].Entries.ToArray();var y=b.KeyArrays[channel].Entries.ToArray();keys+=x.Length;if(keys>1000000 || x.Length!=y.Length || x.Where((k,i)=>k._index!=y[i]._index || k._value!=y[i]._value || k._tangent!=y[i]._tangent).Any())throw new IOException("Imported native motion keys/tangents changed or exceed one million scalar keys.");}
            }
        }

        private void Import<T>(BRRESNode root, CompanionResource choice, string folder, CancellationToken token) where T : BRESEntryNode
        {
            token.ThrowIfCancellationRequested();
            if (string.Equals(choice.SourcePath, Model.SourcePath, StringComparison.OrdinalIgnoreCase) && choice.ContainerPath == Model.ContainerPath) return; // Already retained in the base BRRES.
            BRESGroupNode group = root.GetOrCreateFolder<T>();
            if (group.Children.Any(n => n.Name == choice.Name))
                throw new IOException("The base BRRES already contains " + choice.Kind + " " + choice.Name + ". No existing resource is overwritten. Select that embedded resource or edit the base explicitly.");
            string transfer = Path.Combine(folder, "transfer." + choice.Kind.ToLowerInvariant());
            try
            {
                using (ResourceNode source = OpenSource(choice)) Locate(source, choice).Export(transfer);
                T added = root.CreateResource<T>(choice.Name);
                added.Replace(transfer);
                added.Name = choice.Name;
            }
            finally { if (File.Exists(transfer)) File.Delete(transfer); }
        }

        public void VerifySources(CancellationToken token)
        {
            foreach (Source source in _sources)
            {
                token.ThrowIfCancellationRequested();
                if (HashFile(source.File.SourcePath) != source.File.Sha256)
                    throw new IOException("Source changed; explicitly reselect dependencies: " + source.File.SourcePath);
            }
        }

        public void Save(string path)
        {
            SourceWriteGuard.ThrowIfProtected(path);
            var doc = new XmlDocument { XmlResolver = null };
            XmlElement root = doc.CreateElement("fighterportCompanions"); root.SetAttribute("version", _palettes.Count!=0 || _sources.Any(s=>s.File.SelectedContainer!=null)?"4":"3");
            root.SetAttribute("scope", ScopeNotice); root.SetAttribute("otherNativeResources", AdditionalResourceNotice); doc.AppendChild(root);
            root.AppendChild(DumpProfile.Write(doc));
            foreach (Source source in _sources)
            {
                XmlElement file = doc.CreateElement("source"); file.SetAttribute("path", source.File.SourcePath);
                file.SetAttribute("sha256", source.File.Sha256); root.AppendChild(file);
                if(source.File.SelectedContainer!=null)file.SetAttribute("selectedContainer",source.File.SelectedContainer);
                CompanionResource origin = source.Resources.First();
                if (origin.MountId != null)
                {
                    file.SetAttribute("mountId", origin.MountId); file.SetAttribute("mountRoot", origin.MountRoot);
                    file.SetAttribute("mountLabel", origin.MountLabel); file.SetAttribute("mountPriority", origin.MountPriority.ToString());
                    file.SetAttribute("logicalPath", origin.LogicalPath); file.SetAttribute("selectionReason", origin.SelectionReason);
                }
            }
            if (Model != null) WriteChoice(doc, root, "model", "", Model);
            if (Motion != null) WriteChoice(doc, root, "motion", "", Motion);
            foreach (var pair in _textures.OrderBy(p => p.Key, StringComparer.Ordinal)) WriteChoice(doc, root, "texture", pair.Key, pair.Value);
            foreach (var pair in _palettes.OrderBy(p=>p.Key,StringComparer.Ordinal))WriteChoice(doc,root,"palette",pair.Key,pair.Value);
            string parent = Path.GetDirectoryName(Path.GetFullPath(path));
            Directory.CreateDirectory(parent);
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            SourceWriteGuard.ThrowIfProtected(temporary);
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    doc.Save(stream); stream.Flush(true);
                }
                if (File.Exists(path)) File.Replace(temporary, path, null); else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        public static CompanionWorkspace Load(string path, string storageRoot = null, CancellationToken token = default(CancellationToken))
        {
            path = SourceWriteGuard.NormalizePath(path);
            SourceWriteGuard.EnsureNoReparsePoints(path);
            if (new FileInfo(path).Length > 1024 * 1024) throw new IOException("Companion resolution exceeds 1 MiB.");
            var doc = new XmlDocument { XmlResolver = null };
            using (var reader = System.Xml.XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 1024 * 1024 })) doc.Load(reader);
            XmlElement root = doc.DocumentElement;
            if (root?.Name != "fighterportCompanions" || !new[]{"1","2","3","4"}.Contains(root.GetAttribute("version"))) throw new IOException("Unsupported companion resolution version.");
            var files = root.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "source").ToList();
            var choices = root.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "choice").ToList();
            var profiles = root.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "brawlDumpRoots").ToList();
            if (files.Count > 16 || choices.Count > (root.GetAttribute("version")=="4"?130:128) || profiles.Count > 1 || root.ChildNodes.OfType<XmlElement>().Any(e => e.Name != "source" && e.Name != "choice" && e.Name != "brawlDumpRoots")) throw new IOException("Invalid or excessive companion records.");
            var result = new CompanionWorkspace(storageRoot);
            if (profiles.Count == 1) result.SetDumpProfile(BrawlDumpProfile.Read(profiles[0]));
            foreach (XmlElement file in files)
            {
                string sourcePath = file.GetAttribute("path");
                if (sourcePath.Length < 3 || !char.IsLetter(sourcePath[0]) || sourcePath[1] != ':' ||
                    (sourcePath[2] != '\\' && sourcePath[2] != '/'))
                    throw new IOException("Companion sources must use absolute local drive paths.");
                sourcePath = SourceWriteGuard.NormalizePath(sourcePath); // Reject network/device/alternate-stream paths before any file access.
                if (!File.Exists(sourcePath)) throw new IOException("Missing companion source: " + sourcePath);
                SourceWriteGuard.EnsureNoReparsePoints(sourcePath);
                if (new FileInfo(sourcePath).Length > SourceFolderSession.MaxFileBytes) throw new IOException("Companion source exceeds 64 MiB: " + sourcePath);
                if (HashFile(sourcePath) != file.GetAttribute("sha256")) throw new IOException("Changed companion source: " + sourcePath + ". Explicitly reselect it; saved choices were not applied.");
                if(file.HasAttribute("selectedContainer"))result.AddSelectedContainer(sourcePath,file.GetAttribute("selectedContainer"),token);else result.AddSource(sourcePath, token);
                Source staged = result._sources.Single(s => string.Equals(s.File.SourcePath, SourceWriteGuard.NormalizePath(sourcePath), StringComparison.OrdinalIgnoreCase));
                if (staged.File.Sha256 != file.GetAttribute("sha256")) throw new IOException("Changed companion source: " + sourcePath + ". Explicitly reselect it; saved choices were not applied.");
                if (file.HasAttribute("mountId"))
                {
                    string mountRoot = BrawlDumpProfile.LocalAbsolute(file.GetAttribute("mountRoot"));
                    ValidateOrigin(sourcePath, mountRoot, file.GetAttribute("logicalPath"));
                    if (!Guid.TryParseExact(file.GetAttribute("mountId"), "N", out _) || !int.TryParse(file.GetAttribute("mountPriority"), out int priority) || priority < -999 || priority > 999 ||
                        file.GetAttribute("mountLabel").Length > 120 || file.GetAttribute("selectionReason").Length > 2048)
                        throw new IOException("Invalid saved source lineage.");
                    // Keep the selection's original lineage even if today's profile was deliberately changed.
                    foreach (var resource in staged.Resources)
                    {
                        resource.MountId = file.GetAttribute("mountId"); resource.MountRoot = mountRoot;
                        resource.MountLabel = file.GetAttribute("mountLabel"); resource.MountPriority = priority;
                        resource.LogicalPath = file.GetAttribute("logicalPath"); resource.SelectionReason = file.GetAttribute("selectionReason");
                    }
                }
            }
            foreach (XmlElement choice in choices.OrderBy(c => c.GetAttribute("role") == "model" ? 0 : 1))
            {
                string source = choice.GetAttribute("source"), tree = choice.GetAttribute("tree"), role = choice.GetAttribute("role");
                CompanionResource resource = result.Resources.SingleOrDefault(r => string.Equals(r.SourcePath, source, StringComparison.OrdinalIgnoreCase) && r.TreePath == tree && r.Identity == choice.GetAttribute("identity") && r.Version.ToString() == choice.GetAttribute("nativeVersion"));
                if (resource != null && (root.GetAttribute("version") == "3" || root.GetAttribute("version")=="4") && ((resource.ContainerPath ?? "") != choice.GetAttribute("containerPath") || resource.ContainerIdentity != choice.GetAttribute("containerIdentity")))
                    throw new IOException("Saved enclosing container identity changed.");
                if (resource == null) throw new IOException("Saved native resource identity is missing or changed: " + source + " [" + tree + "]");
                if(choice.HasAttribute("selectedContainer")!=(resource.SourceContainer!=null) || (resource.SourceContainer!=null && choice.GetAttribute("selectedContainer")!=resource.SourceContainer))throw new IOException("Selected original container provenance changed.");
                if (role == "model") { if (result.Model != null) throw new IOException("Duplicate model selection."); result.SelectModel(resource); }
                else if (role == "motion") { if (result.Motion != null) throw new IOException("Duplicate motion selection."); result.SelectMotion(resource); }
                else if (role == "texture") { string binding = choice.GetAttribute("binding"); if (result._textures.ContainsKey(binding)) throw new IOException("Duplicate texture binding."); result.SelectTexture(binding, resource); }
                else if(role=="palette"){string binding=choice.GetAttribute("binding");if(result._palettes.ContainsKey(binding))throw new IOException("Duplicate palette binding.");result.SelectPalette(binding,resource);}
                else throw new IOException("Unsupported companion role: " + role);
            }
            return result;
        }

        private static void WriteChoice(XmlDocument doc, XmlElement root, string role, string binding, CompanionResource resource)
        {
            XmlElement e = doc.CreateElement("choice"); e.SetAttribute("role", role); e.SetAttribute("binding", binding);
            e.SetAttribute("source", resource.SourcePath); e.SetAttribute("tree", resource.TreePath); e.SetAttribute("identity", resource.Identity);
            e.SetAttribute("nativeVersion", resource.Version.ToString());
            if(resource.SourceContainer!=null)e.SetAttribute("selectedContainer",resource.SourceContainer);
            e.SetAttribute("containerPath", resource.ContainerPath ?? ""); e.SetAttribute("containerIdentity", resource.ContainerIdentity); root.AppendChild(e);
        }
        private static void ValidateOrigin(string source, string root, string relative)
        {
            source = BrawlDumpProfile.LocalAbsolute(source); root = BrawlDumpProfile.LocalAbsolute(root);
            if (!SourceWriteGuard.IsWithin(source, root) || string.Equals(source, root, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(source.Substring(root.TrimEnd('\\', '/').Length + 1).Replace('\\', '/'), relative, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Source/root/logical-file identity is inconsistent.");
        }
        private void RequireMember(CompanionResource resource, string kind)
        {
            if (resource == null || resource.Kind != kind || !Resources.Contains(resource)) throw new IOException("Select an inventoried " + kind + " resource.");
        }
        private ResourceNode OpenSource(CompanionResource resource)
        {
            Source source = _sources.Single(s => string.Equals(s.File.SourcePath, resource.SourcePath, StringComparison.OrdinalIgnoreCase));
            if (HashFile(source.File.StagedPath) != (source.File.StagedHash??source.File.Sha256)) throw new IOException("Staged source changed; reload the resolution.");
            return NodeFactory.FromFile(null, source.File.StagedPath) ?? throw new IOException("Native resource could not be read.");
        }
        private static unsafe string NativeBytes(ResourceNode node)
        {
            var bytes=new byte[node.WorkingUncompressed.Length];System.Runtime.InteropServices.Marshal.Copy((IntPtr)node.WorkingUncompressed.Address,bytes,0,bytes.Length);
            // Only relocations change when a resource moves between BRRES dictionaries.
            // Preserve encoded pixels/mips/palette entries and every other header byte.
            if(bytes.Length<64)throw new IOException("Truncated native texture/palette.");
            foreach(int offset in new[]{12,20,node is TEX0Node?48:32})Array.Clear(bytes,offset,4);
            using(var h=SHA256.Create())return ((BRESEntryNode)node).Name+"|"+((BRESEntryNode)node).OriginalPath+"|"+BitConverter.ToString(h.ComputeHash(bytes));
        }
        private static ResourceNode Locate(ResourceNode root, CompanionResource resource)
        {
            ResourceNode node = root;
            string relative=Relative(resource.SourceContainer,resource.TreePath);
            if (relative.Length != 0)
                foreach (string segment in relative.Split('/'))
                {
                    if (!int.TryParse(segment, out int index) || index < 0 || index >= node.Children.Count) throw new IOException("Native resource path changed.");
                    node = node.Children[index];
                }
            if (node.Name != resource.Name || node.GetType().Name != resource.Kind + "Node") throw new IOException("Native resource identity changed.");
            return node;
        }
        private static void Walk(ResourceNode node, string path, Action<ResourceNode, string> visit)
        {
            visit(node, path);
            if (!(node is ARCNode) && !(node is BRRESNode) && !(node is BRESGroupNode)) return; // Inventory only native entries, not every mesh/animation descendant.
            for (int i = 0; i < node.Children.Count; i++) Walk(node.Children[i], path.Length == 0 ? i.ToString() : path + "/" + i, visit);
        }
        private static ResourceNode EnclosingBrres(ResourceNode node)
        {
            while (node != null && !(node is BRRESNode)) node = node.Parent;
            return node;
        }
        private static string NodePath(ResourceNode root, ResourceNode node)
        {
            var indices = new Stack<int>();
            while (node != root) { if (node.Parent == null) throw new IOException("Resource is outside its source container."); indices.Push(node.Index); node = node.Parent; }
            return string.Join("/", indices);
        }
        private static ResourceNode AtPath(ResourceNode root, string path)
        {
            if (path == null) throw new IOException("No enclosing BRRES was selected.");
            foreach (string segment in path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            { if (!int.TryParse(segment, out int i) || i < 0 || i >= root.Children.Count) throw new IOException("Enclosing container path changed."); root = root.Children[i]; }
            return root;
        }
        private static BRRESNode Container(ResourceNode root, CompanionResource resource) =>
            AtPath(root, Relative(resource.SourceContainer,resource.ContainerPath)) as BRRESNode ?? throw new IOException("Selected native entry is no longer inside a BRRES.");
        private static string Prefix(string container,string path)=>string.IsNullOrEmpty(container)?path:string.IsNullOrEmpty(path)?container:container+"/"+path;
        private static string Relative(string container,string path)
        {if(string.IsNullOrEmpty(container))return path;if(path==container)return "";if(path==null || !path.StartsWith(container+"/",StringComparison.Ordinal))throw new IOException("Resource is outside its pinned selected container.");return path.Substring(container.Length+1);}
        private static string NativeLineage(ResourceNode node)
        {
            var result = new Stack<string>();
            while (node != null)
            {
                if (node is ARCNode || node is BRRESNode)
                {
                    string identity = node.GetType().Name + " '" + node.Name + "'";
                    if (node is ARCEntryNode entry && node.Parent is ARCNode)
                        identity += " [" + node.Index + ":" + entry.FileType + ":" + entry.FileIndex + ":" + entry.GroupID + ":" + entry.RedirectIndex + "]";
                    result.Push(identity);
                }
                node = node.Parent;
            }
            return string.Join(" > ", result);
        }
        private static string HashFile(string path)
        {
            SourceWriteGuard.EnsureNoReparsePoints(path);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var hash = SHA256.Create())
            {
                if (stream.Length < 16 || stream.Length > SourceFolderSession.MaxFileBytes) throw new IOException("Source changed or exceeds the 64 MiB hash budget: " + path);
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
