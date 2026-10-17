// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms. No game bindings are inferred.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlCrate.Authoring
{
    public sealed class AnimationPack
    {
        public string Id {get;set;}
        public int Version {get;set;} = 1;
        public string Profile {get;set;}
        public List<string> RequiredAnchors {get;set;} = new List<string>();
        public List<AnimationGroup> Groups {get;set;} = new List<AnimationGroup>();
        public static AnimationPack BrawlAuthoring()
        {
            var pack=new AnimationPack {Id="f054307a72d847f58d0af6acf818df3c",Version=2,Profile="Brawl native assets / semantic organization; runtime bindings unverified",RequiredAnchors=new List<string>{"Idle","Fall"}};
            string[] categories={"Jab","Forward tilt","Up tilt","Down tilt","Forward smash","Up smash","Down smash","Neutral aerial","Forward aerial","Back aerial","Up aerial","Down aerial","Neutral special","Side special","Down special","Up special","Taunt","Idle","Fall","Walk","Run","Jump","Land","Dodge","Damage","Guard","Ledge","Climb","Item / prop","Miscellaneous"};
            foreach(string category in categories)pack.Groups.Add(new AnimationGroup {Id=CharacterProject.Hash(System.Text.Encoding.UTF8.GetBytes(pack.Id+":"+category)).Substring(0,32),Label=category,Semantic=category,Phases=new List<string>{"single","start","hold","loop","release","follow-up","end"},Variants=new List<string>{"default","ground","air"}});
            return pack;
        }
    }
    public sealed class AnimationGroup
    {
        public string Id {get;set;}
        public string Label {get;set;}
        public string Semantic {get;set;}
        public List<string> Phases {get;set;} = new List<string>();
        public List<string> Variants {get;set;} = new List<string>();
    }
    public sealed class NativeSlotBinding
    {
        public string Profile {get;set;}
        public int PackVersion {get;set;}
        public string TargetId {get;set;}
        public string RigFingerprint {get;set;}
        public string AnimationId {get;set;}
        public string NativeIdentity {get;set;}
        public string NativePath {get;set;}
        public string NativeHash {get;set;}
        public string ModelIdentity {get;set;}
        public string ContainerIdentity {get;set;}
        // Verified asset destination only; never runtime behavior/ACMD/PSA evidence.
    }
    public sealed class AnimationPart
    {
        public string Id {get;set;}
        public string GroupId {get;set;}
        public string SlotId {get;set;}
        public string VariantId {get;set;}
        public string Label {get;set;}
        public string Phase {get;set;}
        public string Variant {get;set;}
        public string InstanceId {get;set;}
        public NativeSlotBinding Binding {get;set;}
    }
    public sealed class CharacterAnimationSet
    {
        public string Id {get;set;}
        public string TargetId {get;set;}
        public string RigFingerprint {get;set;}
        public AnimationPack Pack {get;set;}
        public string PackDigest {get;set;}
        public AnimationGroup CustomGroup {get;set;}
        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<AnimationGroup> Groups=>Pack.Groups.Concat(CustomGroup==null?Enumerable.Empty<AnimationGroup>():new[]{CustomGroup});
        public List<AnimationPart> Parts {get;set;} = new List<AnimationPart>();
    }
    public sealed class ReferenceAnchor
    {
        public string Id {get;set;}
        public string TargetId {get;set;}
        public string RigFingerprint {get;set;}
        public string Role {get;set;}
        public string InstanceId {get;set;}
        public int InstanceRevision {get;set;}
        public string WorkingHash {get;set;}
        public string ClipId {get;set;}
        public int ClipRevision {get;set;}
        public int Frames {get;set;}
        public bool Loop {get;set;}
        public double Frame {get;set;}
        public string Interpretation {get;set;} = "native-zero-based-frame-v1; loop period FrameCount+1; anchor is project metadata, not fitted motion";
    }
    public sealed partial class CharacterProject
    {
        public void Atomic(Action action)
        {
            if(_transactionDepth!=0)throw new IOException("Nested project batches are unsupported.");
            byte[] before=Serialize(_state);bool dirty=Dirty;_transactionDepth++;_publishedTransaction=before;
            try {action();ValidateState(_state);_undo.Add(before);_redo.Clear();TrimHistory();}
            catch {_state=Deserialize<CharacterProjectState>(before,MaxMetadataBytes);Dirty=dirty;_generation++;throw;}
            finally {_transactionDepth--;_publishedTransaction=null;Changed?.Invoke(this,EventArgs.Empty);}
        }
        public void RefreshNativeLease(NativeEditLease lease)
        {
            if(lease==null || !_leases.ContainsKey(lease) || lease.ProjectId!=_state.Id || Instance(lease.InstanceId).WorkingHash!=lease.BaselineHash)throw new IOException("Native document revision changed; reopen the explicit instance.");
            _leases[lease]=_generation;
        }
        public string InstallAnimationPack(string targetId,AnimationPack pack,CancellationToken token=default(CancellationToken))
        {
            CheckSources();token.ThrowIfCancellationRequested();ValidatePack(pack);var target=Character(targetId);
            if(_state.AnimationSets.Any(n=>n.TargetId==targetId))throw new IOException("This target already has an animation set; custom assignments will not be replaced.");
            var set=new CharacterAnimationSet {Id=Id(),TargetId=targetId,RigFingerprint=target.RigFingerprint,Pack=Deserialize<AnimationPack>(Serialize(pack),MaxMetadataBytes)};
            var custom=new AnimationGroup {Id=Id(),Label="Preserved custom assignments",Semantic="custom",Phases=new List<string>{"single"},Variants=new List<string>{"default"}};
            set.CustomGroup=custom;set.PackDigest=Hash(Serialize(set.Pack));
            foreach(var i in _state.Instances.Where(i=>i.TargetCharacterId==targetId)){var part=new AnimationPart {Id=Id(),GroupId=custom.Id,Label=i.Slot,Phase="single",Variant="default",InstanceId=i.Id};PartIdentity(set,part);set.Parts.Add(part);}
            token.ThrowIfCancellationRequested();Commit(s=>{s.Version=3;s.AnimationSets.Add(set);});return set.Id;
        }
        public void AddAnimationParts(string setId,string groupId,int count,string phase,string variant,CancellationToken token=default(CancellationToken))
        {
            CheckSources();if(count<1 || count>64)throw new IOException("Authoring batch budget: 1..64 parts; this is not a runtime limit.");token.ThrowIfCancellationRequested();
            Commit(s=>{var set=s.AnimationSets.Single(n=>n.Id==setId);for(int i=0;i<count;i++){token.ThrowIfCancellationRequested();var part=new AnimationPart {Id=Id(),GroupId=groupId,Label="Part "+(set.Parts.Count+1),Phase=phase,Variant=variant};PartIdentity(set,part);set.Parts.Add(part);}});
        }
        private static void PartIdentity(CharacterAnimationSet set,AnimationPart part)
        {part.VariantId=Hash(System.Text.Encoding.UTF8.GetBytes(set.Pack.Id+":"+part.GroupId+":"+part.Variant)).Substring(0,32);part.SlotId=Hash(System.Text.Encoding.UTF8.GetBytes(set.Id+":"+part.GroupId+":"+part.VariantId)).Substring(0,32);}
        public void RenameAnimationPart(string setId,string partId,string label)
        {CheckSources();CheckText(label,120);var part=_state.AnimationSets.Single(n=>n.Id==setId).Parts.Single(n=>n.Id==partId);if(part.InstanceId!=null)SetSlot(part.InstanceId,label);else Commit(s=>s.AnimationSets.Single(n=>n.Id==setId).Parts.Single(n=>n.Id==partId).Label=label);}
        public void MoveAnimationPart(string setId,string partId,int direction)
        {
            CheckSources();if(direction!=1 && direction!=-1)throw new IOException("Choose one earlier/later step.");
            Commit(s=>{var set=s.AnimationSets.Single(n=>n.Id==setId);var part=set.Parts.Single(n=>n.Id==partId);var siblings=set.Parts.Where(n=>n.SlotId==part.SlotId).ToList();int next=siblings.IndexOf(part)+direction;if(next<0 || next>=siblings.Count)throw new IOException("This part is already at the variant boundary.");int a=set.Parts.IndexOf(part),b=set.Parts.IndexOf(siblings[next]);set.Parts[a]=siblings[next];set.Parts[b]=part;});
        }
        public void BindAnimationPart(string setId,string partId,string destinationAnimationId)
        {
            CheckSources();var set=_state.AnimationSets.Single(n=>n.Id==setId);var source=Character(set.TargetId);var a=Animation(destinationAnimationId);
            if(a.SourceCharacterId!=source.Id)throw new IOException("Select an actual native animation destination on this target.");
            string path=PreviewSource(a.Id);try{BrawlProjectAdapter.ValidateArchive(path,source,a,_scratch,true);}finally{File.Delete(path);}
            var binding=new NativeSlotBinding {Profile=set.Pack.Profile,PackVersion=set.Pack.Version,TargetId=source.Id,RigFingerprint=source.RigFingerprint,AnimationId=a.Id,NativeIdentity=a.NativeIdentity,NativePath=a.NativePath,NativeHash=a.NativeHash??source.NativeHash,ModelIdentity=source.ModelIdentity,ContainerIdentity=source.ContainerIdentity};
            Commit(s=>s.AnimationSets.Single(n=>n.Id==setId).Parts.Single(n=>n.Id==partId).Binding=binding);
        }
        public void ClearAnimationPart(string setId,string partId,bool bindingOnly)
        {
            CheckSources();Commit(s=>{var part=s.AnimationSets.Single(n=>n.Id==setId).Parts.Single(n=>n.Id==partId);if(bindingOnly)part.Binding=null;else part.InstanceId=null;});
        }
        public string AssignAnimationPart(string setId,string partId,string animationId,string mapId=null,CancellationToken token=default(CancellationToken))
        {
            string result=null;Atomic(()=> {
                token.ThrowIfCancellationRequested();var set=_state.AnimationSets.Single(n=>n.Id==setId);var part=set.Parts.Single(n=>n.Id==partId);var a=Animation(animationId);
                if(part.InstanceId!=null)throw new IOException("Existing/custom assignment is preserved; clear it deliberately before replacement.");
                if(a.SourceCharacterId==set.TargetId)result=Assign(a.Id,set.TargetId,part.Label);
                else {if(part.Binding==null)throw new IOException("Cross-rig output needs an explicit native target destination and reviewed map.");result=AssignRetargeted(a.Id,part.Binding.AnimationId,mapId,part.Label,token);}
                Commit(s=>s.AnimationSets.Single(n=>n.Id==setId).Parts.Single(n=>n.Id==partId).InstanceId=result);token.ThrowIfCancellationRequested();
            });return result;
        }
        public void ChooseAnchor(string targetId,string role,string instanceId)
        {
            CheckSources();CheckText(role,64);var i=Instance(instanceId);var target=Character(targetId);var a=NativeAnimation(i.Id);
            if(i.TargetCharacterId!=targetId || i.TargetRigFingerprint!=target.RigFingerprint)throw new IOException("Reference must be an instance on the intended target rig.");
            string path=Materialize(i.WorkingHash,target.Extension);int frames;bool loop;
            try {using(var root=NodeFactory.FromFile(null,path)){var motion=BrawlProjectAdapter.Motion(root,target,a);frames=motion.FrameCount;loop=motion.Loop;}}finally{File.Delete(path);}
            var anchor=new ReferenceAnchor {Id=Id(),TargetId=targetId,RigFingerprint=target.RigFingerprint,Role=role,InstanceId=i.Id,InstanceRevision=i.Revision,WorkingHash=i.WorkingHash,ClipId=a.Id,ClipRevision=a.Revision,Frames=frames,Loop=loop};
            Commit(s=>{s.Version=3;s.Anchors.RemoveAll(n=>n.TargetId==targetId && n.Role==role);s.Anchors.Add(anchor);});
        }
        public string AnchorProblem(ReferenceAnchor a)
        {
            var i=_state.Instances.SingleOrDefault(n=>n.Id==a.InstanceId);var c=_state.Characters.SingleOrDefault(n=>n.Id==a.TargetId);
            var clip=i==null?null:_state.Animations.SingleOrDefault(n=>n.Id==(i.NativeAnimationId??i.AnimationId));
            return i==null || c==null?"Missing reference instance/target":i.TargetCharacterId!=a.TargetId || c.RigFingerprint!=a.RigFingerprint?"Reference rig mismatch":clip==null || clip.Id!=a.ClipId || clip.Revision!=a.ClipRevision?"Missing/stale reference clip revision":i.Revision!=a.InstanceRevision || i.WorkingHash!=a.WorkingHash?"Stale reference revision; choose the updated instance explicitly":null;
        }
        public void SetAnchorPhase(string targetId,string role,string previewedInstanceId,double nativeFrame)
        {
            CheckSources();var a=_state.Anchors.Single(n=>n.TargetId==targetId && n.Role==role);string error=AnchorProblem(a);
            if(error!=null)throw new IOException(error);
            if(a.InstanceId!=previewedInstanceId)throw new IOException("Preview this reference on its target before choosing its playhead phase.");
            if(double.IsNaN(nativeFrame) || double.IsInfinity(nativeFrame) || nativeFrame!=Math.Truncate(nativeFrame) || nativeFrame<0 || nativeFrame>(a.Loop?a.Frames:a.Frames-1))throw new IOException("Reference phase must be an integer native-playhead frame in the supported playback range.");
            Commit(s=>s.Anchors.Single(n=>n.Id==a.Id).Frame=nativeFrame);
        }
        private static void ValidatePack(AnimationPack pack)
        {
            if(pack==null || !Guid.TryParseExact(pack.Id,"N",out _) || pack.Version<1 || pack.Version>10000 || pack.Profile==null || pack.Profile.Length>160 || pack.Groups==null || pack.Groups.Count<1 || pack.Groups.Count>64)throw new IOException("Invalid animation pack/version/budget.");
            if(pack.Groups.Select(n=>n.Id).Distinct().Count()!=pack.Groups.Count)throw new IOException("Duplicate pack group identity.");
            if(pack.RequiredAnchors==null || pack.RequiredAnchors.Count>32 || pack.RequiredAnchors.Distinct().Count()!=pack.RequiredAnchors.Count)throw new IOException("Invalid required anchor roles.");foreach(string role in pack.RequiredAnchors)CheckText(role,64);
            foreach(var g in pack.Groups){if(!Guid.TryParseExact(g.Id,"N",out _) || g.Phases==null || g.Variants==null || g.Phases.Count<1 || g.Phases.Count>32 || g.Variants.Count<1 || g.Variants.Count>32 || g.Phases.Distinct().Count()!=g.Phases.Count || g.Variants.Distinct().Count()!=g.Variants.Count)throw new IOException("Invalid group phases/variants.");CheckText(g.Label,120);CheckText(g.Semantic,120);foreach(string v in g.Phases.Concat(g.Variants))CheckText(v,64);}
        }
        private void ValidateAnimationSets(CharacterProjectState s)
        {
            if(s.AnimationSets==null || s.Anchors==null || s.AnimationSets.Count>8 || s.Anchors.Count>64 || s.AnimationSets.Any(n=>n.Parts==null) || s.AnimationSets.Sum(n=>n.Parts.Count)>512)throw new IOException("Animation-set/anchor budget exceeded.");
            if((s.AnimationSets.Count+s.Anchors.Count)>0 && s.Version!=3)throw new IOException("Slots/references require project v3.");
            var ids=s.AnimationSets.Select(n=>n.Id).Concat(s.AnimationSets.SelectMany(n=>n.Parts.Select(p=>p.Id))).Concat(s.Anchors.Select(n=>n.Id)).ToList();
            if(ids.Any(id=>!Guid.TryParseExact(id,"N",out _)) || ids.Distinct().Count()!=ids.Count || s.AnimationSets.Select(n=>n.TargetId).Distinct().Count()!=s.AnimationSets.Count || s.Anchors.Select(n=>n.TargetId+":"+n.Role).Distinct().Count()!=s.Anchors.Count)throw new IOException("Duplicate/invalid animation-set/anchor identities.");
            foreach(var set in s.AnimationSets)
            {
                ValidatePack(set.Pack);if(set.PackDigest!=Hash(Serialize(set.Pack)) || set.Groups.Select(g=>g.Id).Distinct().Count()!=set.Groups.Count())throw new IOException("Animation pack changed without a new reviewed version.");
                if(set.CustomGroup!=null)ValidatePack(new AnimationPack{Id=set.Pack.Id,Profile="custom",Groups=new List<AnimationGroup>{set.CustomGroup}});
                var target=s.Characters.SingleOrDefault(n=>n.Id==set.TargetId);
                if(target==null || target.RigFingerprint!=set.RigFingerprint)throw new IOException("Animation set target rig changed.");
                var destinations=new HashSet<string>();
                foreach(var part in set.Parts)
                {
                    var group=set.Groups.SingleOrDefault(n=>n.Id==part.GroupId);CheckText(part.Label,120);
                    var identity=new AnimationPart {GroupId=part.GroupId,Variant=part.Variant};PartIdentity(set,identity);if(part.SlotId!=identity.SlotId || part.VariantId!=identity.VariantId)throw new IOException("Stable slot/variant identity changed.");
                    if(group==null || !group.Phases.Contains(part.Phase) || !group.Variants.Contains(part.Variant) || (part.InstanceId!=null && !s.Instances.Any(i=>i.Id==part.InstanceId && i.TargetCharacterId==set.TargetId)))throw new IOException("Invalid part/variant/assignment.");
                    var b=part.Binding;if(b==null)continue;var a=s.Animations.SingleOrDefault(n=>n.Id==b.AnimationId);
                    if(a==null || a.SourceCharacterId!=target.Id || b.TargetId!=target.Id || b.RigFingerprint!=target.RigFingerprint || b.Profile!=set.Pack.Profile || b.PackVersion!=set.Pack.Version || b.NativeIdentity!=a.NativeIdentity || b.NativePath!=a.NativePath || b.NativeHash!=(a.NativeHash??target.NativeHash) || b.ModelIdentity!=target.ModelIdentity || b.ContainerIdentity!=target.ContainerIdentity)throw new IOException("Stale native asset binding; labels do not approve bindings.");
                    if(!destinations.Add(b.NativeIdentity))throw new IOException("Conflicting native destination binding; this profile does not permit sharing.");
                    if(part.InstanceId!=null && (s.Instances.Single(i=>i.Id==part.InstanceId).NativeAnimationId??s.Instances.Single(i=>i.Id==part.InstanceId).AnimationId)!=b.AnimationId)throw new IOException("Assigned native output differs from the bound destination.");
                }
            }
            foreach(var a in s.Anchors){CheckText(a.Role,64);if(a.Interpretation!=new ReferenceAnchor().Interpretation || a.Frames<1 || a.Frames>100000 || double.IsNaN(a.Frame) || double.IsInfinity(a.Frame) || a.Frame!=Math.Truncate(a.Frame) || a.Frame<0 || a.Frame>(a.Loop?a.Frames:a.Frames-1) || a.InstanceRevision<1 || a.ClipRevision<1)throw new IOException("Invalid anchor phase/time interpretation.");}
        }
    }
}
