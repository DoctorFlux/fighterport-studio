// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring.Humanoid;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BrawlCrate.Authoring
{
    public sealed class HumanoidSetup
    {
        public HumanoidProfile Profile;
        public BodyProfile Body;
        public string Diagnostics;
    }
    public sealed partial class CharacterProject
    {
        public HRig HumanoidRigFor(string characterId)=>BrawlProjectAdapter.CommonRig(Character(characterId));
        public HumanoidSetup HumanoidSetupFor(string characterId)
        {
            var c=Character(characterId);return new HumanoidSetup{Profile=State.Humanoids.SingleOrDefault(p=>p.RigFingerprint==c.RigFingerprint),Body=State.Bodies.SingleOrDefault(b=>b.RigFingerprint==c.RigFingerprint && b.ModelHash==c.ModelExportHash)};
        }
        public void DetectHumanoid(string characterId)
        {
            CheckSources();var c=Character(characterId);var profile=HumanoidSetupFor(characterId).Profile??BrawlProjectAdapter.SuggestHumanoid(c);
            var stored=HumanoidSetupFor(characterId).Profile==null?_library?.TryProfile("humanoids",profile.Id):null;if(stored?.Humanoid!=null)profile=stored.Humanoid;
            string bodyId=Hash(System.Text.Encoding.UTF8.GetBytes("body-v1:"+c.RigFingerprint+":"+c.ModelExportHash)).Substring(0,32);
            var body=HumanoidSetupFor(characterId).Body??_library?.TryProfile("bodies",bodyId)?.Body;
            if(body==null)
            {
                string archive=Materialize(c.NativeHash,c.Extension);
                try{using(var root=NodeFactory.FromFile(null,archive))body=BrawlProjectAdapter.MeasureBody(root,c,profile);}
                catch(IOException){profile.Validate(BrawlProjectAdapter.CommonRig(c),false,true); /* Keep a visible unresolved draft; no fake body or approved mapping. */}
                finally{File.Delete(archive);}
            }
            var pins=new List<LibraryPin>();if(profile.Reviewed && _library!=null)pins.Add(_library.Pin("humanoids",profile.Id,profile.Revision));if(body?.Reviewed==true && _library!=null)pins.Add(_library.Pin("bodies",body.Id,body.Revision));
            Commit(s=>{s.Version=3;if(!s.Humanoids.Any(p=>p.Id==profile.Id))s.Humanoids.Add(profile);if(body!=null && !s.Bodies.Any(b=>b.Id==body.Id))s.Bodies.Add(body);foreach(var pin in pins)if(!s.HumanoidPins.Any(p=>p.Kind==pin.Kind&&p.Id==pin.Id))s.HumanoidPins.Add(pin);});
        }
        public void UpdateHumanoidChain(string characterId,string role,IEnumerable<int> indices)
        {
            CheckSources();CheckText(role,80);var setup=HumanoidSetupFor(characterId);var c=Character(characterId);var p=setup.Profile??throw new IOException("Auto-detect or load a characterization first.");if(p.Reviewed)throw new IOException("Reviewed shared revision is immutable. A new profile-revision workflow is required before changing it; source/other projects were not modified.");
            var chain=indices.ToList();if(chain.Count<1 || chain.Count>32 || chain.Any(i=>i<0 || i>=c.Bones.Count) || chain.Distinct().Count()!=chain.Count)throw new IOException("Choose 1..32 explicit unique bone indices.");
            p.Chains.RemoveAll(n=>n.Role==role);p.Chains.Add(new HChain{Role=role,Bones=chain,Confidence=1,Evidence="Explicit local chain edit; awaiting review."});
            if(p.Chains.SelectMany(n=>n.Bones).Distinct().Count()!=p.Chains.Sum(n=>n.Bones.Count))throw new IOException("Bone already belongs to another chain. Resolve the conflicting role explicitly.");
            p.Optional=Enumerable.Range(0,c.Bones.Count).Where(i=>!p.Chains.Any(n=>n.Bones.Contains(i))).Select(i=>new HOptionalBone{Bone=i,Evidence="Explicit optional policy: retain target reference, report source loss."}).ToList();
            Commit(s=>{s.Humanoids[s.Humanoids.FindIndex(n=>n.Id==p.Id)]=p;s.Bodies.RemoveAll(b=>b.RigFingerprint==p.RigFingerprint && !b.Reviewed);});
        }
        public void UpdateBodyProfile(string characterId,BodyProfile edited)
        {
            CheckSources();var current=HumanoidSetupFor(characterId).Body??throw new IOException("Measure a body profile first.");var body=Deserialize<BodyProfile>(Serialize(edited),MaxMetadataBytes);
            if(current.Reviewed)throw new IOException("Reviewed mesh/body revision is immutable; no shared profile was overwritten.");
            if(body.Id!=current.Id || body.ModelHash!=current.ModelHash || body.RigFingerprint!=current.RigFingerprint || body.Reviewed || body.Digest!=null)throw new IOException("Body identity/review cannot be changed by an edit.");body.Validate(BrawlProjectAdapter.CommonRig(Character(characterId)),false);
            Commit(s=>s.Bodies[s.Bodies.FindIndex(b=>b.Id==body.Id)]=body);
        }
        public void ReviewHumanoid(string characterId,string note)
        {
            CheckSources();CheckText(note,2048);var setup=HumanoidSetupFor(characterId);var c=Character(characterId);var rig=BrawlProjectAdapter.CommonRig(c);
            var profile=setup.Profile??throw new IOException("Characterize this rig first.");var body=setup.Body??throw new IOException("Measure/review this mesh body first.");
            if(profile.Reviewed && body.Reviewed)return;
            profile.Validate(rig,false);body.Validate(rig,false);
            if(!profile.Reviewed){profile.Reviewed=true;profile.ReviewNote=note;profile.Digest=null;profile.Digest=Hash(Serialize(profile));}
            if(!body.Reviewed){body.Reviewed=true;body.ReviewNote=note;body.Digest=null;body.Digest=Hash(Serialize(body));}
            var pins=_library==null?new LibraryPin[0]:new[]{_library.Publish(profile),_library.Publish(body)};
            Commit(s=>{s.Humanoids[s.Humanoids.FindIndex(p=>p.Id==profile.Id)]=profile;s.Bodies[s.Bodies.FindIndex(b=>b.Id==body.Id)]=body;foreach(var pin in pins)if(!s.HumanoidPins.Any(p=>p.Kind==pin.Kind&&p.Id==pin.Id))s.HumanoidPins.Add(pin);});
        }
        public HumanoidRecipe CreateHumanoidRecipe(string animationId,string targetAnimationId)
        {
            var a=Animation(animationId);var t=Animation(targetAnimationId);var source=HumanoidSetupFor(a.SourceCharacterId);var target=HumanoidSetupFor(t.SourceCharacterId);
            if(source.Profile==null || source.Body==null || target.Profile==null || target.Body==null)throw new IOException("Set up source and target humanoid/body profiles first.");
            return new HumanoidRecipe{Id=Id(),SourceCharacterId=a.SourceCharacterId,TargetCharacterId=t.SourceCharacterId,AnimationId=a.Id,TargetAnimationId=t.Id,SourceProfile=source.Profile.Id,TargetProfile=target.Profile.Id,SourceBody=source.Body.Id,TargetBody=target.Body.Id};
        }
        public void SaveHumanoidRecipe(HumanoidRecipe proposed)
        {
            CheckSources();var recipe=Deserialize<HumanoidRecipe>(Serialize(proposed),MaxMetadataBytes);ValidateHumanoidRecipe(_state,recipe,false);
            if(_state.Instances.Any(i=>i.HumanoidRecipeId==recipe.Id) && Hash(Serialize(_state.HumanoidRecipes.Single(r=>r.Id==recipe.Id)))!=Hash(Serialize(recipe)))throw new IOException("An assigned correction recipe is immutable. Create a new recipe ID for a new output; prior instance dependencies were preserved.");
            Commit(s=>{s.Version=3;int i=s.HumanoidRecipes.FindIndex(r=>r.Id==recipe.Id);if(i<0)s.HumanoidRecipes.Add(recipe);else s.HumanoidRecipes[i]=recipe;});
        }
        public HumanoidTrial PreviewHumanoid(HumanoidRecipe recipe,CancellationToken token=default(CancellationToken))
        {
            CheckSources();ValidateHumanoidRecipe(_state,recipe,false);token.ThrowIfCancellationRequested();var a=Animation(recipe.AnimationId);var native=Animation(recipe.TargetAnimationId);var source=Character(a.SourceCharacterId);var target=Character(native.SourceCharacterId);
            var sp=_state.Humanoids.Single(p=>p.Id==recipe.SourceProfile);var tp=_state.Humanoids.Single(p=>p.Id==recipe.TargetProfile);var sb=_state.Bodies.Single(p=>p.Id==recipe.SourceBody);var tb=_state.Bodies.Single(p=>p.Id==recipe.TargetBody);
            // Reserve the two inputs and both worst-case outputs before creating any working copy.
            CheckHumanoidScratch(4,2L*MaxNativeBytes+Data(a.NativeHash??source.NativeHash).Length+Data(native.NativeHash??target.NativeHash).Length);
            string sf=Materialize(a.NativeHash??source.NativeHash,source.Extension),tf=Materialize(native.NativeHash??target.NativeHash,target.Extension),fk=Path.Combine(_scratch,Id()+target.Extension),corrected=Path.Combine(_scratch,Id()+target.Extension);
            try
            {
                var report=BrawlProjectAdapter.BakeHumanoid(sf,source,a,tf,target,native,sp,sb,tp,tb,recipe,fk,corrected,token);CheckSources();token.ThrowIfCancellationRequested();
                return new HumanoidTrial{ProjectId=_state.Id,TargetId=target.Id,AnimationId=a.Id,TargetAnimationId=native.Id,TargetFingerprint=target.RigFingerprint,RecipeHash=Hash(Serialize(recipe)),Generation=_generation,FkPath=fk,CorrectedPath=corrected,FkHash=Hash(ReadBounded(fk)),CorrectedHash=Hash(ReadBounded(corrected)),Report=report,Release=()=>{if(File.Exists(fk))File.Delete(fk);if(File.Exists(corrected))File.Delete(corrected);}};
            }
            catch{if(File.Exists(fk))File.Delete(fk);if(File.Exists(corrected))File.Delete(corrected);throw;}
            finally{File.Delete(sf);File.Delete(tf);}
        }
        public string AssignHumanoid(string recipeId,string slot,CancellationToken token=default(CancellationToken))
        {
            CheckText(slot,120);var recipe=_state.HumanoidRecipes.Single(r=>r.Id==recipeId);ValidateHumanoidRecipe(_state,recipe,true);using(var trial=PreviewHumanoid(recipe,token)){
            if(trial.Report.UnreachableSamples!=0 || trial.Report.CorrectedContactError>_state.Bodies.Single(b=>b.Id==recipe.TargetBody).LegLength*.005)throw new IOException("Contact quality needs correction; inspect trial residuals before committing an assignment.");
            var a=Animation(recipe.AnimationId);var target=Character(recipe.TargetCharacterId);var instance=new MoveInstance{Id=Id(),AnimationId=a.Id,AnimationRevision=a.Revision,TargetCharacterId=target.Id,TargetRigFingerprint=target.RigFingerprint,Slot=slot,NativeAnimationId=recipe.TargetAnimationId,HumanoidRecipeId=recipe.Id,WorkingHash=AddBlob(ReadBounded(trial.CorrectedPath)),BakeEvidence=trial.Report.ToString()};
            CheckSources();token.ThrowIfCancellationRequested();Commit(s=>{s.Version=3;s.Instances.Add(instance);});return instance.Id;}
        }
        public string AssignHumanoidPart(string setId,string partId,string recipeId,CancellationToken token=default(CancellationToken))
        {
            string id=null;Atomic(()=>{var set=_state.AnimationSets.Single(s=>s.Id==setId);var part=set.Parts.Single(p=>p.Id==partId);var recipe=_state.HumanoidRecipes.Single(r=>r.Id==recipeId);
                if(part.InstanceId!=null)throw new IOException("Existing/custom assignment is preserved; clear it deliberately before fitting a new instance.");
                if(set.TargetId!=recipe.TargetCharacterId || part.Binding==null || part.Binding.AnimationId!=recipe.TargetAnimationId)throw new IOException("Bind this target part to its actual native destination first.");
                id=AssignHumanoid(recipe.Id,part.Label,token);Commit(s=>s.AnimationSets.Single(a=>a.Id==setId).Parts.Single(p=>p.Id==partId).InstanceId=id);});return id;
        }
        internal string MaterializeHumanoidTrial(HumanoidTrial trial,bool corrected)
        {
            if(trial.ProjectId!=_state.Id || trial.Generation!=_generation || Character(trial.TargetId).RigFingerprint!=trial.TargetFingerprint || Animation(trial.TargetAnimationId).SourceCharacterId!=trial.TargetId)throw new IOException("Trial belongs to another or changed project/target. Regenerate this comparison.");
            string path=corrected?trial.CorrectedPath:trial.FkPath;byte[] data=ReadBounded(path);if(Hash(data)!=(corrected?trial.CorrectedHash:trial.FkHash))throw new IOException("Trial native data changed.");CheckHumanoidScratch(1,data.Length);string copy=Path.Combine(_scratch,Id()+Character(trial.TargetId).Extension);WriteNew(copy,data);return copy;
        }
        private void CheckHumanoidScratch(int extraFiles,long extraBytes)
        {var files=Directory.GetFiles(_scratch);if(files.Length+extraFiles>256 || files.Sum(f=>new FileInfo(f).Length)+extraBytes>128L*1024*1024)throw new IOException("Project working-copy quota reached (256 files / 128 MiB per session). Save and close before managing retained comparisons.");}
        public void ExportHumanoidTrial(HumanoidTrial trial,string destination)
        {
            CheckSources();var target=Character(trial.TargetId);if(File.Exists(destination) || Path.GetExtension(destination)!=target.Extension)throw new IOException("Choose a new native file with the target archive extension.");string copy=MaterializeHumanoidTrial(trial,true);
            try{BrawlProjectAdapter.ValidateArchive(copy,target,Animation(trial.TargetAnimationId),_scratch,false);WriteNew(destination,ReadBounded(copy));}finally{File.Delete(copy);}
        }
        private static void CheckReviewDigest(HumanoidProfile p)
        {if(!p.Reviewed)return;var copy=Deserialize<HumanoidProfile>(Serialize(p),MaxMetadataBytes);copy.Digest=null;if(p.Digest!=Hash(Serialize(copy)))throw new IOException("Humanoid characterization changed after review.");}
        private static void CheckReviewDigest(BodyProfile b)
        {if(!b.Reviewed)return;var copy=Deserialize<BodyProfile>(Serialize(b),MaxMetadataBytes);copy.Digest=null;if(b.Digest!=Hash(Serialize(copy)))throw new IOException("Body measurements changed after review.");}
        private void ValidateHumanoidRecipe(CharacterProjectState s,HumanoidRecipe r,bool review)
        {
            if(r==null || !Guid.TryParseExact(r.Id,"N",out _) || r.Revision!=1)throw new IOException("Invalid humanoid recipe identity/version.");
            var a=s.Animations.Single(n=>n.Id==r.AnimationId);var t=s.Animations.Single(n=>n.Id==r.TargetAnimationId);var source=s.Characters.Single(c=>c.Id==a.SourceCharacterId);var target=s.Characters.Single(c=>c.Id==t.SourceCharacterId);
            if(source.Id!=r.SourceCharacterId || target.Id!=r.TargetCharacterId)throw new IOException("Recipe source/target identities changed.");
            if(r.AssignmentPartId!=null){var set=s.AnimationSets.SingleOrDefault(aSet=>aSet.Parts.Any(p=>p.Id==r.AssignmentPartId));var part=set?.Parts.Single(p=>p.Id==r.AssignmentPartId);if(set==null || set.TargetId!=target.Id || part.Binding!=null && part.Binding.AnimationId!=r.TargetAnimationId)throw new IOException("Recipe part/native destination binding changed.");}
            var sp=s.Humanoids.Single(p=>p.Id==r.SourceProfile);var tp=s.Humanoids.Single(p=>p.Id==r.TargetProfile);var sb=s.Bodies.Single(b=>b.Id==r.SourceBody);var tb=s.Bodies.Single(b=>b.Id==r.TargetBody);
            sp.Validate(BrawlProjectAdapter.CommonRig(source),review);tp.Validate(BrawlProjectAdapter.CommonRig(target),review);sb.Validate(BrawlProjectAdapter.CommonRig(source),review);tb.Validate(BrawlProjectAdapter.CommonRig(target),review);
            if(sb.ModelHash!=source.ModelExportHash || tb.ModelHash!=target.ModelExportHash)throw new IOException("Body profile belongs to another mesh/weights/rest revision.");CheckReviewDigest(sp);CheckReviewDigest(tp);CheckReviewDigest(sb);CheckReviewDigest(tb);r.Validate(a.Frames);
            if(Math.Abs(r.PelvisBias)>tb.PelvisLimit)throw new IOException("Recipe pelvis bias exceeds the reviewed target body's adjustment limit.");
        }
        private void ValidateHumanoids(CharacterProjectState s)
        {
            if(s.Humanoids==null || s.Bodies==null || s.HumanoidRecipes==null || s.HumanoidPins==null || s.Humanoids.Count>8 || s.Bodies.Count>8 || s.HumanoidRecipes.Count>64 || s.HumanoidPins.Count>16)throw new IOException("Humanoid profile/recipe budget exceeded.");
            if(s.Humanoids.Count+s.Bodies.Count+s.HumanoidRecipes.Count>0 && s.Version!=3)throw new IOException("Humanoid records require project v3.");
            foreach(var p in s.Humanoids){var c=s.Characters.FirstOrDefault(n=>n.RigFingerprint==p.RigFingerprint)??throw new IOException("Humanoid rig dependency missing.");p.Validate(BrawlProjectAdapter.CommonRig(c),false,true);CheckReviewDigest(p);}
            foreach(var b in s.Bodies){var c=s.Characters.FirstOrDefault(n=>n.RigFingerprint==b.RigFingerprint && n.ModelExportHash==b.ModelHash)??throw new IOException("Body model dependency missing.");b.Validate(BrawlProjectAdapter.CommonRig(c),false);CheckReviewDigest(b);}
            foreach(var r in s.HumanoidRecipes)ValidateHumanoidRecipe(s,r,false);
            var identities=s.Humanoids.Select(p=>p.Id).Concat(s.Bodies.Select(b=>b.Id)).Concat(s.HumanoidRecipes.Select(r=>r.Id)).ToList();if(identities.Any(i=>!Guid.TryParseExact(i,"N",out _)) || identities.Distinct().Count()!=identities.Count || s.Humanoids.Select(p=>p.RigFingerprint).Distinct().Count()!=s.Humanoids.Count || s.Bodies.Select(b=>b.ModelHash).Distinct().Count()!=s.Bodies.Count)throw new IOException("Duplicate/invalid humanoid identities.");
            if(_library!=null)
            {
                if(s.HumanoidPins.Count!=s.Humanoids.Count(p=>p.Reviewed)+s.Bodies.Count(b=>b.Reviewed) || s.HumanoidPins.Select(p=>p.Kind+":"+p.Id).Distinct().Count()!=s.HumanoidPins.Count)throw new IOException("Reviewed humanoid shared pins are incomplete/duplicated.");
                foreach(var pin in s.HumanoidPins){var record=_library.Read(pin);byte[] actual,expected;if(pin.Kind=="humanoids"){actual=Serialize(record.Humanoid);expected=Serialize(s.Humanoids.Single(p=>p.Id==pin.Id&&p.Revision==pin.Revision));}else if(pin.Kind=="bodies"){actual=Serialize(record.Body);expected=Serialize(s.Bodies.Single(p=>p.Id==pin.Id&&p.Revision==pin.Revision));}else throw new IOException("Invalid humanoid pin kind.");if(Hash(actual)!=Hash(expected))throw new IOException("Humanoid shared revision changed.");}
            }
        }
    }
}
