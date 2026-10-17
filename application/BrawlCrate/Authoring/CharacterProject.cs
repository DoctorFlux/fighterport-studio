// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Sources;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace BrawlCrate.Authoring
{
    public sealed class ProjectBlob
    {
        public string Hash { get; set; }
        public byte[] Data { get; set; }
    }
    public sealed class CharacterProjectFile
    {
        public CharacterProjectState State { get; set; }
        public List<ProjectBlob> Blobs { get; set; } = new List<ProjectBlob>();
    }
    /// <summary>Versioned local edit project. Immutable library data and independent native instance revisions.</summary>
    public sealed partial class CharacterProject
    {
        public const int MaxNativeBytes = 32 * 1024 * 1024;
        private const int MaxDocumentBytes = 48 * 1024 * 1024;
        private const int MaxMetadataBytes = 1024 * 1024;
        private CharacterProjectState _state = new CharacterProjectState();
        private readonly Dictionary<string, byte[]> _blobs = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        private readonly List<byte[]> _undo = new List<byte[]>(), _redo = new List<byte[]>();
        private readonly string _scratch;
        private AnimationLibraryStore _library;
        private long _generation;
        private int _transactionDepth;
        public event EventHandler Changed;
        private readonly Dictionary<NativeEditLease, long> _leases = new Dictionary<NativeEditLease, long>();
        private byte[] _publishedTransaction;
        public CharacterProjectState State => Deserialize<CharacterProjectState>(_publishedTransaction??Serialize(_state), MaxMetadataBytes);
        public bool CanUndo => _undo.Count != 0;
        public bool CanRedo => _redo.Count != 0;
        public bool Dirty { get; private set; }
        public string FilePath { get; private set; }
        public string StorageRoot=>Path.GetDirectoryName(Path.GetDirectoryName(_scratch));

        public CharacterProject(string storageRoot = null)
        {
            string root = Path.Combine(SourceWriteGuard.NormalizePath(storageRoot ?? FighterPortPaths.DataRoot), "CharacterSessions");
            SourceWriteGuard.ThrowIfProtected(root); Directory.CreateDirectory(root);
            if (Directory.GetDirectories(root).Length >= 256 || Directory.GetFiles(root, "*", SearchOption.AllDirectories).Sum(p => new FileInfo(p).Length) > 1024L * 1024 * 1024 - MaxNativeBytes)
                throw new IOException("Character session quota reached (256 sessions / 1 GiB). Archive edits before managing old sessions.");
            _scratch = Path.Combine(root, Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_scratch);
        }

        public string RegisterSource(string resolutionPath, string displayName, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested(); CheckText(displayName, 120);
            byte[] resolution = ReadBounded(resolutionPath, MaxMetadataBytes);
            var workspace = CompanionWorkspace.Load(resolutionPath, Path.GetDirectoryName(Path.GetDirectoryName(_scratch)), token);
            if (workspace.Validate().Count != 0) throw new IOException(string.Join("; ", workspace.Validate()));
            if (Hash(ReadBounded(resolutionPath, MaxMetadataBytes)) != Hash(resolution)) throw new IOException("Resolution changed during registration; no source profile was committed.");
            var provenance = Xml(resolution);
            string native = workspace.Assemble(token);
            byte[] nativeData = ReadBounded(native);
            var profile = new SourceCharacterProfile { Id = Id(), Name = displayName, NativeHash = AddBlob(nativeData), ResolutionHash = AddBlob(resolution),
                Extension = Path.GetExtension(native), ModelPath = workspace.Model.TreePath, ModelIdentity = workspace.Model.Identity,
                ContainerPath = workspace.Model.ContainerPath, ContainerIdentity = workspace.Model.ContainerIdentity, ModelVersion = workspace.Model.Version,
                ModelPayloadHash = BrawlProjectAdapter.PayloadHash(workspace.Model.SourcePath, workspace.Model.TreePath,workspace.Model.SourceContainer) };
            foreach (XmlElement e in provenance.DocumentElement.SelectNodes("source"))
            {
                token.ThrowIfCancellationRequested(); byte[] data = ReadBounded(e.GetAttribute("path"));
                if (Hash(data) != e.GetAttribute("sha256")) throw new IOException("Source changed during registration.");
                profile.Originals.Add(new NativeSourceEvidence { Path = e.GetAttribute("path"), Hash = AddBlob(data) });
            }
            AnimationAsset animation;
            using (var root = NodeFactory.FromFile(null, native))
            {
                var model = (MDL0Node)BrawlProjectAdapter.At(root, profile.ModelPath);
                profile.Bones = BrawlProjectAdapter.Skeleton(model); profile.RigFingerprint = BrawlProjectAdapter.Fingerprint(profile.Bones);
                profile.ModelExportHash = BrawlProjectAdapter.ExportHash(model, _scratch);
                var motion = ((BRRESNode)BrawlProjectAdapter.At(root, profile.ContainerPath)).Children.SelectMany(g => g.Children).OfType<CHR0Node>().Single(n => n.Name == workspace.Motion.Name);
                animation = new AnimationAsset { Id = Id(), SourceCharacterId = profile.Id, SourceRigFingerprint = profile.RigFingerprint,
                    Name = motion.Name, NativeHash=profile.NativeHash, NativePath = BrawlProjectAdapter.PathOf(root, motion), NativeIdentity = workspace.Motion.Identity,
                    OriginalPayloadHash = BrawlProjectAdapter.PayloadHash(workspace.Motion.SourcePath, workspace.Motion.TreePath,workspace.Motion.SourceContainer),
                    NativeExportHash = BrawlProjectAdapter.ExportHash(motion, _scratch), Version = motion.Version, Frames = motion.FrameCount, Loop = motion.Loop };
                BrawlProjectAdapter.Motion(root, profile, animation);
            }
            workspace.VerifySources(token); token.ThrowIfCancellationRequested();
            profile.InitialAnimationId=animation.Id;
            var pins=_library==null?null:new[]{_library.Publish(profile),_library.Publish(animation)};
            Commit(s => { s.Characters.Add(profile); s.Animations.Add(animation);if(pins!=null)s.LibraryPins.AddRange(pins); });
            return profile.Id;
        }

        public string CreateDraft(string animationId, string notes, int startup, int active, int recovery, float damage)
        {
            CheckSources(); CheckText(notes, 2048);
            var move = new MoveAsset { Id = Id(), AnimationId = animationId, Notes = notes, Startup = startup, Active = active, Recovery = recovery, Damage = damage };
            Commit(s => s.Moves.Add(move)); return move.Id;
        }
        public string Assign(string animationId, string targetId, string slot, string moveId = null)
        {
            CheckSources(); CheckText(slot, 120);
            var animation = Animation(animationId); var target = Character(targetId);
            if (target.Id != animation.SourceCharacterId || target.RigFingerprint != animation.SourceRigFingerprint)
                throw new IOException("Different source character/rig: a reviewed retargeting route is required. Matching names do not approve compatibility.");
            var instance = new MoveInstance { Id = Id(), AnimationId = animation.Id, AnimationRevision = animation.Revision, MoveId = moveId,
                TargetCharacterId = target.Id, TargetRigFingerprint = target.RigFingerprint, Slot = slot, WorkingHash = animation.NativeHash??target.NativeHash };
            Commit(s => s.Instances.Add(instance)); return instance.Id;
        }
        public void SetSlot(string instanceId, string slot)
        {
            CheckSources(); CheckText(slot, 120);
            Commit(s => { var instance = s.Instances.Single(i => i.Id == instanceId); instance.Slot = slot; instance.Revision++;foreach(var part in s.AnimationSets.SelectMany(n=>n.Parts).Where(n=>n.InstanceId==instanceId))part.Label=slot; });
        }
        public string ReviewRigMap(ReviewedRigMap proposed)
        {
            CheckSources();var map=Deserialize<ReviewedRigMap>(Serialize(proposed),MaxMetadataBytes);
            map.Id=Id();map.Reviewed=true;
            map.ReviewDigest=null;map.ReviewDigest=Hash(Serialize(map));
            RigTransfer.Validate(map,Character(map.SourceCharacterId),Character(map.TargetCharacterId));
            Commit(s=> {s.Version=Math.Max(s.Version,2);s.RigMaps.Add(map);});return map.Id;
        }
        public string AssignRetargeted(string animationId,string targetAnimationId,string mapId,string slot,CancellationToken token=default(CancellationToken))
        {
            CheckSources();CheckText(slot,120);token.ThrowIfCancellationRequested();
            var animation=Animation(animationId);var native=Animation(targetAnimationId);var source=Character(animation.SourceCharacterId);var target=Character(native.SourceCharacterId);
            var map=_state.RigMaps.SingleOrDefault(m=>m.Id==mapId)??throw new IOException("Select an explicitly reviewed map.");
            RigTransfer.Validate(map,source,target);
            string sourceFile=Materialize(animation.NativeHash??source.NativeHash,source.Extension),targetFile=Materialize(native.NativeHash??target.NativeHash,target.Extension),output=Path.Combine(_scratch,Id()+target.Extension);
            try
            {
                string evidence=BrawlProjectAdapter.Bake(sourceFile,source,animation,targetFile,target,native,map,output,token);
                var instance=new MoveInstance {Id=Id(),AnimationId=animation.Id,AnimationRevision=animation.Revision,TargetCharacterId=target.Id,TargetRigFingerprint=target.RigFingerprint,
                    NativeAnimationId=native.Id,MappingId=map.Id,MappingRevision=map.Revision,Slot=slot,WorkingHash=AddBlob(ReadBounded(output)),BakeEvidence=evidence};
                CheckSources();token.ThrowIfCancellationRequested();Commit(s=> {s.Version=Math.Max(s.Version,2);s.Instances.Add(instance);});return instance.Id;
            }
            finally {File.Delete(sourceFile);File.Delete(targetFile);if(File.Exists(output))File.Delete(output);}
        }
        public AnimationAsset NativeAnimation(string instanceId)
        {
            var i=Instance(instanceId);return Deserialize<AnimationAsset>(Serialize(_state.Animations.Single(a=>a.Id==(i.NativeAnimationId??i.AnimationId))),MaxMetadataBytes);
        }
        public string PreviewSource(string animationId)
        {
            var animation = Animation(animationId); var source = Character(animation.SourceCharacterId);
            return Materialize(animation.NativeHash??source.NativeHash, source.Extension);
        }
        public NativeEditLease OpenInstance(string instanceId)
        {
            var instance = Instance(instanceId); var profile = Character(instance.TargetCharacterId);
            var lease = new NativeEditLease { InstanceId = instance.Id, ProjectId = _state.Id, BaselineHash = instance.WorkingHash,
                Path = Materialize(instance.WorkingHash, profile.Extension) };
            _leases.Add(lease, _generation); return lease;
        }
        public void ReleaseNativeLease(NativeEditLease lease){if(lease!=null)_leases.Remove(lease);}
        public void CaptureNativeEdits(NativeEditLease lease, ResourceNode nativeRoot, string openedPath)
        {
            CheckSources();
            if (lease == null || !_leases.TryGetValue(lease, out long generation) || generation != _generation || lease.ProjectId != _state.Id ||
                !string.Equals(SourceWriteGuard.NormalizePath(openedPath), lease.Path, StringComparison.OrdinalIgnoreCase))
                throw new IOException("No current instance edit binding. Open this instance again; viewport selection is not source identity.");
            var instance = Instance(lease.InstanceId); var profile = Character(instance.TargetCharacterId); var animation = NativeAnimation(instance.Id);
            if (instance.WorkingHash != lease.BaselineHash || nativeRoot == null) throw new IOException("Stale native edit transaction.");
            string edited = Path.Combine(_scratch, Id() + profile.Extension);
            bool wasDirty=nativeRoot.IsDirty;
            try
            {
                nativeRoot.Export(edited);
                string baseline = Materialize(instance.WorkingHash, profile.Extension);
                try { BrawlProjectAdapter.ValidateEdit(baseline, edited, profile, animation, _scratch); }
                finally { File.Delete(baseline); }
                string hash = AddBlob(ReadBounded(edited));
                Commit(s => { var i = s.Instances.Single(n => n.Id == instance.Id); i.WorkingHash = hash; i.Revision++;
                    if((i.MappingId!=null || i.HumanoidRecipeId!=null) && !i.BakeEvidence.StartsWith("Native edits captured;"))i.BakeEvidence="Native edits captured; prior transfer measurement is historical, not a measurement of these edits. "+i.BakeEvidence; });
                // Continue editing this same explicitly bound native document. Each
                // capture is a new transaction with the just-committed baseline.
                lease.BaselineHash = hash;
                _leases[lease] = _generation;
            }
            catch {if(wasDirty)nativeRoot.IsDirty=true;throw;} // Native Export rebuilds/clears flags before validation; failed Apply must remain pending.
            finally { if (File.Exists(edited)) File.Delete(edited); }
        }
        public void ExportInstance(string instanceId, string destination)
        {
            CheckSources(); var instance = Instance(instanceId); var source = Character(instance.TargetCharacterId);
            destination = SourceWriteGuard.NormalizePath(destination); SourceWriteGuard.ThrowIfProtected(destination);
            if (File.Exists(destination)) throw new IOException("Choose a new native output file; existing files are not replaced.");
            if (!string.Equals(Path.GetExtension(destination), source.Extension, StringComparison.OrdinalIgnoreCase)) throw new IOException("Retain the native enclosing container extension " + source.Extension);
            string materialized = Materialize(instance.WorkingHash, source.Extension);
            try
            {
                BrawlProjectAdapter.ValidateArchive(materialized, source, NativeAnimation(instance.Id), _scratch, false);
                WriteNew(destination, Data(instance.WorkingHash));
                if (Hash(ReadBounded(destination)) != instance.WorkingHash) throw new IOException("Native output changed during save.");
            }
            finally { File.Delete(materialized); }
        }
        public void Undo() { Restore(_undo, _redo); }
        public void Redo() { Restore(_redo, _undo); }
        private void Restore(List<byte[]> from, List<byte[]> to)
        {
            if (from.Count == 0) throw new IOException("No project command to restore.");
            to.Add(Serialize(_state)); _state = Deserialize<CharacterProjectState>(from[from.Count - 1], MaxMetadataBytes); from.RemoveAt(from.Count - 1);
            TrimHistory();
            _generation++; Dirty = true;
            Changed?.Invoke(this,EventArgs.Empty);
        }
        private void Commit(Action<CharacterProjectState> change)
        {
            byte[] before = Serialize(_state); var next = Deserialize<CharacterProjectState>(before, MaxMetadataBytes);
            change(next); ValidateState(next);
            if (Serialize(next).Length > MaxMetadataBytes) throw new IOException("Project metadata exceeds 1 MiB.");
            if(_transactionDepth==0){_undo.Add(before); _redo.Clear();}
            TrimHistory();
            _state = next; _generation++; Dirty = true;
            if(_transactionDepth==0)Changed?.Invoke(this,EventArgs.Empty);
        }
        private void TrimHistory()
        {
            while (_undo.Count + _redo.Count > 32 || _undo.Concat(_redo).Sum(b => (long)b.Length) > 16 * 1024 * 1024)
            {
                var distant = _undo.Count != 0 ? _undo : _redo;
                distant.RemoveAt(0);
            }
        }
        public IList<string> SourceProblems()
        {
            var problems = new List<string>();
            foreach (var evidence in _state.Characters.SelectMany(c => c.Originals).Concat(_state.Animations.SelectMany(a=>a.AdditionalOriginals)).GroupBy(e => e.Path + ":" + e.Hash, StringComparer.OrdinalIgnoreCase).Select(g => g.First()))
                try
                {
                    new SourceFolderSession(Path.GetDirectoryName(evidence.Path), Path.GetDirectoryName(Path.GetDirectoryName(_scratch)));
                    if (Hash(ReadBounded(evidence.Path)) != evidence.Hash) problems.Add("Changed source: " + evidence.Path + "; assumptions invalid; register a new revision.");
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { problems.Add("Unavailable source: " + evidence.Path + ": " + e.Message); }
            return problems;
        }
        private void CheckSources() { var problems = SourceProblems(); if (problems.Count != 0) throw new IOException(string.Join(Environment.NewLine, problems)); }

        public void Save(string path)
        {
            ValidateState(_state); path = SourceWriteGuard.NormalizePath(path); SourceWriteGuard.ThrowIfProtected(path);
            byte[] bytes = Serialize(new CharacterProjectFile { State = _state, Blobs = string.IsNullOrEmpty(_state.LibraryId)?UsedHashes(_state).Select(h => new ProjectBlob { Hash = h, Data = Data(h) }).ToList():new List<ProjectBlob>() });
            if (bytes.Length > MaxDocumentBytes) throw new IOException("Project file exceeds 48 MiB.");
            Directory.CreateDirectory(Path.GetDirectoryName(path)); string temp = path + "." + Id() + ".tmp";
            try
            {
                WriteNew(temp, bytes); SourceWriteGuard.ThrowIfProtected(path);
                if (File.Exists(path))
                {
                    SourceWriteGuard.ThrowIfProtected(path + ".previous"); File.Replace(temp, path, path + ".previous");
                }
                else File.Move(temp, path);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
            FilePath = path; Dirty = false;
            Changed?.Invoke(this,EventArgs.Empty);
        }
        public static CharacterProject Load(string path, string storageRoot = null)
        {
            var document = Deserialize<CharacterProjectFile>(ReadBounded(path, MaxDocumentBytes), MaxDocumentBytes);
            var project = new CharacterProject(storageRoot);
            if (document.State == null || document.Blobs == null || document.Blobs.Count > 512) throw new IOException("Invalid project envelope.");
            if(!string.IsNullOrEmpty(document.State.LibraryId))
            {
                project._library=new AnimationLibraryStore(document.State.LibraryRoot,true);
                if(project._library.Id!=document.State.LibraryId)throw new IOException("Wrong shared library identity; pinned data was not rebound.");
            }
            foreach (var blob in document.Blobs)
                if (blob.Data == null || project.AddBlob(blob.Data) != blob.Hash) throw new IOException("Native blob hash mismatch.");
            project.ValidateState(document.State); project._state = document.State;
            foreach (var source in project._state.Characters)
            {
                if (source.Interpretation != BrawlProjectAdapter.Interpretation || BrawlProjectAdapter.Fingerprint(source.Bones) != source.RigFingerprint) throw new IOException("Changed rig interpretation/snapshot.");
                var resolution = Xml(project.Data(source.ResolutionHash));
                var model = resolution.DocumentElement.SelectSingleNode("choice[@role='model']") as XmlElement;
                var motion = resolution.DocumentElement.SelectSingleNode("choice[@role='motion']") as XmlElement;
                var animation = project._state.Animations.Single(a => a.SourceCharacterId == source.Id && a.NativeIdentity==motion?.GetAttribute("identity"));
                if (resolution.DocumentElement.Name != "fighterportCompanions" || model == null || motion == null ||
                    model.GetAttribute("identity") != source.ModelIdentity || model.GetAttribute("containerIdentity") != source.ContainerIdentity ||
                    motion.GetAttribute("identity") != animation.NativeIdentity || model.GetAttribute("tree") != source.ModelPath || model.GetAttribute("containerPath") != source.ContainerPath)
                    throw new IOException("Native resolution provenance changed.");
                var originals = resolution.DocumentElement.SelectNodes("source").OfType<XmlElement>().ToList();
                if (originals.Count != source.Originals.Count || source.Originals.Any(e => !originals.Any(o => o.GetAttribute("path") == e.Path && o.GetAttribute("sha256") == e.Hash)))
                    throw new IOException("Native original provenance changed.");
                project.VerifyPayload(source, model, source.ModelPayloadHash);
                project.VerifyPayload(source, motion, animation.OriginalPayloadHash);
                string archive = project.Materialize(source.NativeHash, source.Extension);
                try { BrawlProjectAdapter.ValidateArchive(archive, source, animation, project._scratch, true); }
                finally { File.Delete(archive); }
            }
            foreach (var instance in project._state.Instances)
            {
                var source = project.Character(instance.TargetCharacterId); var animation = project.NativeAnimation(instance.Id);
                string original = project.Materialize(animation.NativeHash??source.NativeHash, source.Extension), edited = project.Materialize(instance.WorkingHash, source.Extension);
                try { BrawlProjectAdapter.ValidateEdit(original, edited, source, animation, project._scratch); }
                finally { File.Delete(original); File.Delete(edited); }
            }
            foreach(var animation in project._state.Animations.Where(a=>a.NativeHash!=null))
            {
                var source=project.Character(animation.SourceCharacterId);string archive=project.Materialize(animation.NativeHash,source.Extension);
                try{BrawlProjectAdapter.ValidateArchive(archive,source,animation,project._scratch,true);}finally{File.Delete(archive);}
                foreach(var evidence in animation.AdditionalOriginals)
                    if(Hash(project.Data(evidence.Hash))!=evidence.Hash)throw new IOException("Additional motion original changed.");
            }
            project.SourceProblems(); // Register read-only roots, including offline/changed diagnostics for the UI.
            project.FilePath = SourceWriteGuard.NormalizePath(path); return project;
        }

        private void VerifyPayload(SourceCharacterProfile source, XmlElement choice, string expected)
        {
            var original = source.Originals.SingleOrDefault(o => o.Path == choice.GetAttribute("source"));
            if (original == null) throw new IOException("Native choice has no original source evidence.");
            string path = Materialize(original.Hash, ".native");
            try { if (BrawlProjectAdapter.PayloadHash(path, choice.GetAttribute("tree"),choice.HasAttribute("selectedContainer")?choice.GetAttribute("selectedContainer"):null) != expected) throw new IOException("Exact original native payload hash changed."); }
            finally { File.Delete(path); }
        }

        private void ValidateState(CharacterProjectState s)
        {
            if ((s.Version < 1 || s.Version > 3) || s.LibraryPins==null || s.RigMaps==null || s.RigMaps.Count>64 || s.UndoPolicy != new CharacterProjectState().UndoPolicy || !Guid.TryParseExact(s.Id, "N", out _) || s.Characters == null || s.Animations == null || s.Moves == null || s.Instances == null ||
                s.Characters.Count > 8 || s.Animations.Count > 128 || s.Moves.Count > 64 || s.Instances.Count > 64 || Serialize(s).Length > MaxMetadataBytes)
                throw new IOException("Unsupported project version or record/metadata budget exceeded.");
            var ids = s.Characters.Select(c => c.Id).Concat(s.Animations.Select(a => a.Id)).Concat(s.Moves.Select(m => m.Id)).Concat(s.Instances.Select(i => i.Id)).Concat(s.RigMaps.Select(m=>m.Id)).ToList();
            if (ids.Distinct().Count() != ids.Count || ids.Any(i => !Guid.TryParseExact(i, "N", out _))) throw new IOException("Duplicate/invalid stable project identity.");
            foreach (var c in s.Characters)
            {
                CheckText(c.Name,120);
                if (c.Adapter != "brawl-native-v1" || c.Revision != 1 || (c.ModelVersion != 9 && c.ModelVersion!=11) || (c.Extension != ".pac" && c.Extension != ".brres") ||
                    c.Bones == null || c.Bones.Count < 1 || c.Bones.Count > 512 || c.Originals == null || c.Originals.Count < 1 || c.Originals.Count > 16 ||
                    s.Animations.Count(a => a.SourceCharacterId == c.Id) < 1 || c.ModelPath == null || c.ContainerPath == null)
                    throw new IOException("Unsupported source profile.");
                foreach (var o in c.Originals) SourceWriteGuard.NormalizePath(o.Path);
            }
            foreach (var a in s.Animations)
                if (a.Revision != 1 || a.Version != 4 || a.AdditionalOriginals==null || a.AdditionalOriginals.Count>16 || !s.Characters.Any(c => c.Id == a.SourceCharacterId && c.RigFingerprint == a.SourceRigFingerprint)) throw new IOException("Animation source binding is invalid.");
            foreach (var m in s.Moves)
                if (m.Revision != 1 || !s.Animations.Any(a => a.Id == m.AnimationId) || m.Startup < 0 || m.Active < 0 || m.Recovery < 0 || m.Startup > 100000 || m.Active > 100000 || m.Recovery > 100000 || float.IsNaN(m.Damage) || float.IsInfinity(m.Damage) || m.Damage < 0 || m.Damage > 10000 || m.Notes == null || m.Notes.Length > 2048)
                    throw new IOException("Invalid manually authored draft move.");
            foreach (var i in s.Instances)
            {
                CheckText(i.Slot,120); var a = s.Animations.SingleOrDefault(n => n.Id == i.AnimationId); var c = s.Characters.SingleOrDefault(n => n.Id == i.TargetCharacterId);
                var native=s.Animations.SingleOrDefault(n=>n.Id==(i.NativeAnimationId??i.AnimationId));
                bool direct=a!=null && c!=null && a.SourceCharacterId==c.Id && a.SourceRigFingerprint==c.RigFingerprint;
                var map=s.RigMaps.SingleOrDefault(m=>m.Id==i.MappingId);
                if(map!=null && a!=null && c!=null) RigTransfer.Validate(map,s.Characters.Single(n=>n.Id==a.SourceCharacterId),c);
                var humanoid=s.HumanoidRecipes?.SingleOrDefault(r=>r.Id==i.HumanoidRecipeId);bool fitted=false;
                if(humanoid!=null){ValidateHumanoidRecipe(s,humanoid,true);fitted=humanoid.AnimationId==i.AnimationId && humanoid.TargetAnimationId==i.NativeAnimationId && humanoid.TargetCharacterId==i.TargetCharacterId && !string.IsNullOrEmpty(i.BakeEvidence);}
                if (a == null || c == null || native==null || native.SourceCharacterId!=c.Id || (!direct && !fitted && (map==null || map.Revision!=i.MappingRevision || string.IsNullOrEmpty(i.BakeEvidence))) || (i.HumanoidRecipeId!=null && !fitted) || i.TargetRigFingerprint != c.RigFingerprint || i.AnimationRevision != a.Revision || i.Revision < 1 ||
                    (i.MoveId != null && !s.Moves.Any(m => m.Id == i.MoveId && m.AnimationId == a.Id))) throw new IOException("Unsupported/stale instance binding. Different-rig transfer requires review and baking.");
            }
            foreach(var map in s.RigMaps) RigTransfer.Validate(map,s.Characters.Single(c=>c.Id==map.SourceCharacterId),s.Characters.Single(c=>c.Id==map.TargetCharacterId));
            if(!string.IsNullOrEmpty(s.LibraryId))
            {
                if(_library==null || s.Version<2 || _library.Id!=s.LibraryId || s.LibraryPins.Count!=s.Characters.Count+s.Animations.Count || s.LibraryPins.Select(p=>p.Kind+":"+p.Id).Distinct().Count()!=s.LibraryPins.Count)throw new IOException("Shared library pins are incomplete or duplicated.");
                foreach(var pin in s.LibraryPins)
                {
                    var record=_library.Read(pin);
                    byte[] expected=pin.Kind=="characters"?Serialize(s.Characters.Single(c=>c.Id==pin.Id && c.Revision==pin.Revision)):Serialize(s.Animations.Single(a=>a.Id==pin.Id && a.Revision==pin.Revision));
                    byte[] actual=pin.Kind=="characters"?Serialize(record.Character):Serialize(record.Animation);
                    if(Hash(expected)!=Hash(actual))throw new IOException("Project metadata differs from its pinned immutable library revision.");
                }
            }
            if (UsedHashes(s).Any(h => h == null || !HasData(h))) throw new IOException("Missing immutable native/project blob.");
            ValidateAnimationSets(s);
            ValidateHumanoids(s);
        }
        private static IEnumerable<string> UsedHashes(CharacterProjectState s) => s.Characters.SelectMany(c => new[] { c.NativeHash, c.ResolutionHash }.Concat(c.Originals.Select(o => o.Hash)))
            .Concat(s.Animations.Where(a=>a.NativeHash!=null).SelectMany(a=>new[]{a.NativeHash}.Concat(a.AdditionalOriginals.Select(o=>o.Hash)))).Concat(s.Instances.Select(i => i.WorkingHash)).Distinct();
        private bool HasData(string hash)=>_blobs.ContainsKey(hash)||(_library!=null && _library.ContainsBlob(hash));
        private byte[] Data(string hash)=>_blobs.TryGetValue(hash,out byte[] bytes)?bytes:_library?.ReadBlob(hash)??throw new IOException("Missing pinned payload: "+hash);
        private string AddBlob(byte[] bytes)
        {
            string hash = Hash(bytes);
            if(_library!=null)return _library.PutBlob(bytes);
            if (!_blobs.ContainsKey(hash))
            {
                if (_blobs.Count >= 512 || _blobs.Sum(p => (long)p.Value.Length) + bytes.Length > MaxNativeBytes) throw new IOException("Project native data/history budget reached (512 blobs / 32 MiB). Save/reload to discard unused session revisions or use a smaller selection.");
                _blobs.Add(hash, bytes);
            }
            return hash;
        }
        private string Materialize(string hash, string extension)
        {
            var files = Directory.GetFiles(_scratch);
            byte[] data=Data(hash);
            if (files.Length >= 256 || files.Sum(f => new FileInfo(f).Length) + data.Length > 128L * 1024 * 1024)
                throw new IOException("Project working-copy quota reached (256 files / 128 MiB per session). Save and close before managing retained working copies.");
            string path = Path.Combine(_scratch, Id() + extension); WriteNew(path, data); return path;
        }
        private SourceCharacterProfile Character(string id) => _state.Characters.Single(c => c.Id == id);
        private AnimationAsset Animation(string id) => _state.Animations.Single(a => a.Id == id);
        private MoveInstance Instance(string id) => _state.Instances.Single(i => i.Id == id);
        private static string Id() => Guid.NewGuid().ToString("N");
        private static void CheckText(string text, int limit) { if (string.IsNullOrWhiteSpace(text) || text.Length > limit) throw new IOException("A nonempty name/slot/description within " + limit + " characters is required."); }
        internal static string Hash(byte[] data) { using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(data)).Replace("-", "").ToLowerInvariant(); }
        internal static byte[] ReadBounded(string path, int max = MaxNativeBytes)
        {
            path = SourceWriteGuard.NormalizePath(path); SourceWriteGuard.EnsureNoReparsePoints(path);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length < 1 || stream.Length > max) throw new IOException("File exceeds the project input budget: " + path);
                var data = new byte[(int)stream.Length]; int offset = 0, count;
                while (offset < data.Length && (count = stream.Read(data, offset, data.Length - offset)) > 0) offset += count;
                if (offset != data.Length) throw new IOException("Truncated project input."); return data;
            }
        }
        private static void WriteNew(string path, byte[] bytes)
        {
            SourceWriteGuard.ThrowIfProtected(path);
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { SourceWriteGuard.ThrowIfProtected(stream); stream.Write(bytes,0,bytes.Length); stream.Flush(true); }
        }
        internal static byte[] Serialize<T>(T value)
        {
            using (var stream = new MemoryStream()) { new XmlSerializer(typeof(T)).Serialize(stream, value); return stream.ToArray(); }
        }
        internal static T Deserialize<T>(byte[] bytes, int max)
        {
            if (bytes.Length > max) throw new IOException("XML budget exceeded.");
            try
            {
                using (var stream = new MemoryStream(bytes, false))
                using (var reader = System.Xml.XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = max }))
                    return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
            }
            catch (InvalidOperationException e) { throw new IOException("Invalid project XML.", e); }
        }
        private static XmlDocument Xml(byte[] bytes)
        {
            var document = new XmlDocument { XmlResolver = null };
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = System.Xml.XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = MaxMetadataBytes })) document.Load(reader);
            return document;
        }
    }
}
