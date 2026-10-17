// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Sources;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace BrawlCrate.Authoring
{
    public sealed partial class CharacterProject
    {
        public AnimationLibraryStore SharedLibrary => _library;
        // Deliberate migration: no v1 file is rewritten until the user saves elsewhere.
        public void MigrateToSharedLibrary(string libraryRoot=null)
        {
            if(!string.IsNullOrEmpty(_state.LibraryId))return;
            var store=new AnimationLibraryStore(libraryRoot);var next=State;
            foreach(var source in next.Characters)
            {
                var initial=next.Animations.First(a=>a.SourceCharacterId==source.Id);
                source.InitialAnimationId=source.InitialAnimationId??initial.Id;
                foreach(var animation in next.Animations.Where(a=>a.SourceCharacterId==source.Id))animation.NativeHash=animation.NativeHash??source.NativeHash;
            }
            foreach(string hash in UsedHashes(next).Concat(_blobs.Keys).Distinct())if(store.PutBlob(Data(hash))!=hash)throw new IOException("Migration payload changed.");
            next.LibraryPins.Clear();foreach(var c in next.Characters)next.LibraryPins.Add(store.Publish(c));foreach(var a in next.Animations)next.LibraryPins.Add(store.Publish(a));
            _library=store;
            Commit(s=> {s.Version=2;s.LibraryId=store.Id;s.LibraryRoot=store.Root;s.LibraryPins=next.LibraryPins;s.Characters=next.Characters;s.Animations=next.Animations;});
            _blobs.Clear();FilePath=null; // Never silently overwrite the embedded recovery file.
        }
        public void UseSharedClip(string clipId,int revision=1,string libraryRoot=null)
        {
            if(_library!=null && libraryRoot!=null && SourceWriteGuard.NormalizePath(libraryRoot)!=_library.Root)throw new IOException("Project is pinned to a different explicit library root.");
            var store=_library??new AnimationLibraryStore(libraryRoot);
            if(!string.IsNullOrEmpty(_state.LibraryId) && _state.LibraryId!=store.Id)throw new IOException("A project pins one explicit shared library.");
            if(_state.Characters.Count!=0 && string.IsNullOrEmpty(_state.LibraryId))MigrateToSharedLibrary(store.Root);
            _library=store;
            var clipPin=store.Pin("animations",clipId,revision);var animation=store.Read(clipPin).Animation??throw new IOException("Missing shared animation record.");
            var sourcePin=store.Pin("characters",animation.SourceCharacterId,1);var source=store.Read(sourcePin).Character??throw new IOException("Missing shared source record.");
            var clips=new[]{animation};
            if(source.InitialAnimationId!=animation.Id)clips=new[]{store.Read(store.Pin("animations",source.InitialAnimationId,1)).Animation,animation};
            if(source.Interpretation!=BrawlProjectAdapter.Interpretation || BrawlProjectAdapter.Fingerprint(source.Bones)!=source.RigFingerprint)throw new IOException("Shared source interpretation/fingerprint changed.");
            foreach(var clip in clips)
            {
                string archive=Materialize(clip.NativeHash??source.NativeHash,source.Extension);
                try {BrawlProjectAdapter.ValidateArchive(archive,source,clip,_scratch,true);}finally{File.Delete(archive);}
            }
            Commit(s=> {
                s.Version=2;s.LibraryId=store.Id;s.LibraryRoot=store.Root;
                if(!s.Characters.Any(c=>c.Id==source.Id)){s.Characters.Add(source);s.LibraryPins.Add(sourcePin);}
                foreach(var a in clips)if(!s.Animations.Any(n=>n.Id==a.Id)){s.Animations.Add(a);s.LibraryPins.Add(store.Pin("animations",a.Id,a.Revision));}
            });
        }
        public string AddAnimation(string characterId,string motionPath,CancellationToken token=default(CancellationToken))
        {
            CheckSources();token.ThrowIfCancellationRequested();var source=Character(characterId);
            var initial=Animation(source.InitialAnimationId??_state.Animations.First(a=>a.SourceCharacterId==characterId).Id);
            var session=new SourceFolderSession(Path.GetDirectoryName(SourceWriteGuard.NormalizePath(motionPath)),Path.GetDirectoryName(Path.GetDirectoryName(_scratch)));
            var staged=session.Stage(motionPath,token);BrawlContainerAudit.Inspect(staged.StagedPath,token).RequirePreservable();
            string original=Materialize(source.NativeHash,source.Extension),output=Path.Combine(_scratch,Id()+source.Extension);
            try
            {
                using(var root=NodeFactory.FromFile(null,original))
                using(var imported=NodeFactory.FromFile(null,staged.StagedPath))
                {
                    var motion=imported as CHR0Node??throw new IOException("Add animation currently accepts loose CHR0 v4 only; other adapters remain queued.");
                    if(motion.Version!=4)throw new IOException("This adapter requires CHR0 v4.");
                    var existing=(CHR0Node)BrawlProjectAdapter.At(root,initial.NativePath);var parent=existing.Parent;int index=existing.Index;
                    string copy=Path.Combine(_scratch,Id()+".chr0");try{motion.Export(copy);var node=(CHR0Node)NodeFactory.FromFile(null,copy);parent.InsertChild(node,index);existing.Remove();existing.Dispose();root.Export(output);}finally{if(File.Exists(copy))File.Delete(copy);}
                    var animation=new AnimationAsset {Id=Id(),SourceCharacterId=source.Id,SourceRigFingerprint=source.RigFingerprint,Name=motion.Name,NativePath=initial.NativePath,NativeHash=AddBlob(ReadBounded(output)),
                        NativeIdentity=staged.Sha256+":loose:CHR0:"+motion.Name,OriginalPayloadHash=BrawlProjectAdapter.PayloadHash(staged.StagedPath,""),NativeExportHash=BrawlProjectAdapter.ExportHash(motion,_scratch),Version=4,Frames=motion.FrameCount,Loop=motion.Loop};
                    animation.AdditionalOriginals.Add(new NativeSourceEvidence {Path=staged.SourcePath,Hash=AddBlob(ReadBounded(staged.StagedPath))});
                    BrawlProjectAdapter.ValidateArchive(output,source,animation,_scratch,true);
                    BrawlProjectAdapter.ValidateEdit(original,output,source,animation,_scratch,true);
                    if(Hash(ReadBounded(staged.SourcePath))!=staged.Sha256)throw new IOException("Motion source changed during import.");token.ThrowIfCancellationRequested();
                    var pin=_library?.Publish(animation);Commit(s=>{s.Version=2;s.Animations.Add(animation);if(pin!=null)s.LibraryPins.Add(pin);});return animation.Id;
                }
            }
            finally{File.Delete(original);if(File.Exists(output))File.Delete(output);}
        }
    }
}
