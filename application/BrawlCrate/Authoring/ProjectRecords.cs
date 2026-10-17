// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using System;
using System.Collections.Generic;

namespace BrawlCrate.Authoring
{
    // Serialized records are detached snapshots. CharacterProject alone commits changes.
    public sealed class NativeSourceEvidence
    {
        public string Path { get; set; }
        public string Hash { get; set; }
    }
    public sealed class BoneSnapshot
    {
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Rest { get; set; }
        public string Bind { get; set; }
        public string InverseBind { get; set; }
        public uint Flags { get; set; }
        public uint Billboard { get; set; }
    }
    public sealed class SourceCharacterProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Revision { get; set; } = 1;
        public string Adapter { get; set; } = "brawl-native-v1";
        public string ResolutionHash { get; set; }
        public string NativeHash { get; set; }
        public string Extension { get; set; }
        public string ModelPath { get; set; }
        public string ModelIdentity { get; set; }
        public string ModelPayloadHash { get; set; }
        public string ModelExportHash { get; set; }
        public string ContainerPath { get; set; }
        public string ContainerIdentity { get; set; }
        public int ModelVersion { get; set; }
        public string RigFingerprint { get; set; }
        public string Interpretation { get; set; } = BrawlProjectAdapter.Interpretation;
        public List<BoneSnapshot> Bones { get; set; } = new List<BoneSnapshot>();
        public List<NativeSourceEvidence> Originals { get; set; } = new List<NativeSourceEvidence>();
        // Characterization is deliberately unknown; native identity is not semantic approval.
    }
    public sealed class AnimationAsset
    {
        public string Id { get; set; }
        public string SourceCharacterId { get; set; }
        public string SourceRigFingerprint { get; set; }
        public int Revision { get; set; } = 1;
        public string Name { get; set; }
        public string NativePath { get; set; }
        public string NativeIdentity { get; set; }
        public string OriginalPayloadHash { get; set; }
        public string NativeExportHash { get; set; }
        public int Version { get; set; }
        public int Frames { get; set; }
        public bool Loop { get; set; }
    }
    public sealed class MoveAsset
    {
        public string Id { get; set; }
        public string AnimationId { get; set; }
        public int Revision { get; set; } = 1;
        public string Notes { get; set; }
        public int Startup { get; set; }
        public int Active { get; set; }
        public int Recovery { get; set; }
        public float Damage { get; set; }
        // Explicitly manually authored draft; contains no parsed native attack commands.
    }
    public sealed class MoveInstance
    {
        public string Id { get; set; }
        public string AnimationId { get; set; }
        public int AnimationRevision { get; set; }
        public string MoveId { get; set; }
        public string TargetCharacterId { get; set; }
        public string TargetRigFingerprint { get; set; }
        public string Slot { get; set; }
        public string WorkingHash { get; set; }
        public int Revision { get; set; } = 1;
    }
    public sealed class CharacterProjectState
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int Version { get; set; } = 1;
        public string UndoPolicy { get; set; } = "Session-only: 32 atomic commands / 16 MiB metadata. Saved state persists; history clears on load.";
        public List<SourceCharacterProfile> Characters { get; set; } = new List<SourceCharacterProfile>();
        public List<AnimationAsset> Animations { get; set; } = new List<AnimationAsset>();
        public List<MoveAsset> Moves { get; set; } = new List<MoveAsset>();
        public List<MoveInstance> Instances { get; set; } = new List<MoveInstance>();
    }
    public sealed class NativeEditLease
    {
        public string InstanceId { get; internal set; }
        public string Path { get; internal set; }
        public string BaselineHash { get; internal set; }
        internal string ProjectId;
    }
}
