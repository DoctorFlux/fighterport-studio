# FighterPort Studio: shared animation library, character slots, transition templates, and mirroring

## Simple workflow amendment — 2026-10-17 (after bee76e5)

The latest authority is [FIGHTERPORT_SIMPLE_AUTO_RETARGET_MISC_WORKFLOW.md](FIGHTERPORT_SIMPLE_AUTO_RETARGET_MISC_WORKFLOW.md).
Repair ordinary visible animation browsing/playback before new solver work, then retain
the independently failing Mario-to-Snake contact regression. Read-only allows navigation,
scrubbing, orbit, Play and comparisons. Keep one explicit target and shared live session;
source auditions never assign a slot. Automatically manage source/project registrations
behind contextual actions, with independent model-only registration and honest ambiguity.
Fixed-duration miscellaneous recipes need change preview, protected assignments, atomic
Apply/Undo and freshly decoded native timing/contact proof; no padding or blanket Idle/Fall
fit. These remain backend requirements, not placeholder controls. Selected pinned Godot
algorithm study does not migrate the application. All games/channels/native routes, vetted
app-local tools and optional DCC scope remain. Gameplay stays deferred; mirror is optional.
Only docs/plans/current.md defines the next executable task. The supplied body below remains
historical requirements where it conflicts with this later user amendment.

## Proportion-aware humanoid amendment — 2026-10-17

The user now prioritizes proportion-aware, contact-preserving humanoid retargeting in
the existing Model Previewer. See [FIGHTERPORT_PROPORTION_AWARE_HUMANOID_RETARGETING.md](FIGHTERPORT_PROPORTION_AWARE_HUMANOID_RETARGETING.md).
Mirroring is not a prerequisite for fitting or later work. Preserve compatible data and
useful inherited operations; coordinate-basis conversion is distinct from animation mirroring.
The historical proposals below are superseded wherever they impose mirror priority.
Only docs/plans/current.md defines the next executable acceptance; native/source/contact
quality and game evidence remain separate. Gameplay compilation remains deferred.


## Purpose and precedence

This is a continuation specification, not a restart. It records the user's latest scope: model and animation authoring/conversion first; gameplay scripting is deferred. Keep the actual BrawlCrate-derived FighterPort application, its native editors, completed source/container/project work, source protections, original fixtures, and all wider game support requirements.

The central user experience is:

**Register a source character once → add its animations to a library shared by every character project → drag a clip onto a target character's animation slot → retarget, optionally mirror, and fit its transitions → export validated target-native assets.**

Examples: one Brawl Mario forward-aerial library entry is reused for custom Brawl Goku, custom Rivals II Goku, and Ultimate Snake. Each target has independent rig mappings, edits, slot assignments, and native output. Editing one target must not change another target or the shared source clip.

Primary targets remain Brawl/Project M/Project+, Melee, Ultimate, and Rivals II. Preserve FighterZ, Sparking! ZERO, Xenoverse 1/2 and all earlier model/animation sources and future native-output requirements. Other already-recorded Smash platforms remain on the wider roadmap. Do not require other games' resources to pass through Brawl file formats.

### Active scope

- Models, native rigs/binds/skinning, material/texture dependencies, all requested animation channel families, conversion, source and target previews, semantic characterization, FK/IK retargeting, clip authoring, mirroring, transition fitting, reusable templates, native export and supported Modkit asset import.
- Shared library organization and durable character animation-slot projects.
- Ground/air and multi-part animation grouping, looping, pose anchors, root-motion policies, and authoring-only timing/contact markers.
- Existing source paths/dump/image providers and vetted app-local tool provisioning continue as supporting capabilities, not as prerequisites for every retargeting milestone.

### Deferred, not erased

Do not implement or require attack scripts, ACMD/PSA/Lua behavior, damage, hitboxes, cancel rules, input handling, combo execution, projectile logic, or playable moveset compilation in this phase. Existing manual gameplay drafts must load without data loss, but move them out of the primary animation workflow. Do not require a MoveAsset or damage values just to assign an animation.

Here, “attack slot” means an animation's intended location in a character's animation set, not a programmed attack. An authoring sequence is not a runtime state machine. Exporting more clips cannot make a game execute additional combo stages without a supported existing runtime binding or later gameplay work.

Read actual HEAD before implementation. Incorporate this amendment into MASTER_PLAN.md and the existing library specification with an explicit scope decision. Keep **docs/plans/current.md as the sole next executable task**, pointing to this document for the full requirements.

## 1. Reviewed repository baseline

Snapshot reviewed: `b45fcc34a4b437438cb94155f801c7be2feb5a73`, commit “Add source-bound character projects and native edit transactions,” dated 2026-10-17. Repository evidence is source inspection and project-reported tests, not a fresh Windows build or game test by the author of this specification. [R1–R5]

The source-bound v1 `.fpcharacter` increment is implemented for a narrow Brawl subset. It includes stable source/clip associations, independent instances, captured CHR0 edits, session undo/redo, saved state, native export/reopen and original-fixture evidence. The status records at most eight profiles, one selected CHR0 per profile, and bounded embedded native blobs. [R2]

Concrete implementation findings:

- `application/BrawlCrate/Authoring/ProjectRecords.cs`: `CharacterProjectState` owns the Characters/Animations/Moves/Instances lists. These are currently stored within a character-project document, not a separately managed application-wide collection.
- `AnimationAsset` has a source-character ID, rig fingerprint, revision, native identity, frame count and loop flag. Preserve these provenance concepts.
- `MoveInstance.Slot` is a string. It is not a typed, versioned game animation-slot schema with repeatable parts and ground/air variants.
- `MoveAsset` has manual Startup/Active/Recovery/Damage fields. Preserve old data, but do not expand gameplay authoring during the animation-first phase.
- `CharacterProject.RegisterSource()` selects one motion from its companion resolution. `Load()` uses a single-animation assumption for each source profile. Adding multiple clips per rig requires a real schema/validation migration, not only a new UI button.
- `CharacterProject.Assign()` rejects assignment when the target profile or rig differs from the source. Keep this protective failure until a reviewed retarget route actually exists; removing the condition is not retargeting.
- Native edit leases, fingerprints, source hashes, native validation and independent instance revisions are useful foundations to retain.

The current authoritative task is reviewed fingerprint-bound semantic mappings and measured FK transfer from original rig A to genuinely different rig B, then genuine CHR0 output, fresh decode and native preview on B. Keep this immediate task and extend its surrounding data model for shared-library references. Do not reopen completed multi-root or canonical nested-PAC work as though it were missing. [R3]

## 2. Library shared across every character project

### Storage and identity

Introduce an application/workspace-level `AnimationLibraryStore` outside any single `.fpcharacter` file. This is a proposed new service; adapt naming to the repository conventions.

A default user-data location can be selected by the app, with a configurable local library folder. Do not hard-code the developer's machine path. “Shared” means shared by the user's character projects; it does not require a cloud service or public upload.

The library owns immutable, versioned source profiles, clips, optional related channel bundles, reusable animation-only sequences, mappings and template packs. Character projects reference exact library asset/revision identities and store their own target-specific assignment/correction records.

Use stable IDs and verified content identity, never display names as keys. Allow multiple animations on the same profile and many profiles with the same name. One model/rig registration must not be repeated for every imported clip.

Separate metadata indexing from heavy native payloads. Use a disk-backed store, bounded lazy decoding, pagination/search, thumbnail caches, transactional writes and content deduplication. The existing tiny fixture/project limits are not acceptable as a permanent “one clip per rig” product rule. Preserve safety budgets as per-operation working-set constraints instead of removing them or loading the entire library into memory.

### Revision and project rules

- A project pins a library revision. New library revisions do not silently rebind projects.
- Per-character retargeting, mirrors, trims, transition fitting and native edits create instance changes or derived clips. They never overwrite library originals.
- “Save as library variant” is a deliberate operation creating a new asset/revision.
- Track provenance and dependencies transitively: library revision, source/target rig fingerprints, target game profile, mapping version, modifier version, anchor references, solver settings and exporter version.
- Changing the idle/fall reference marks dependent results stale and offers a previewable rebuild; do not rewrite already-reviewed exports automatically.
- Do not delete library assets needed by saved projects through an unguarded cleanup. Show dependents, permit tombstoning, and keep pinned content recoverable.
- Provide “collect referenced assets” for a portable project/library bundle using verified allowed inputs, not an automatic upload. Preserve source-protection/provenance policies.
- Multiple application windows must not corrupt the library. Use transactional storage/locking and version-conflict checks.

### v1 migration

Keep v1 project loading. On a deliberate import/migration, register its source-bound assets into the shared store, map old project-local IDs to new stable references, preserve every edited instance and old draft field, and retain the old file as recovery. Deduplicate verified identical content without collapsing same-named but different rigs or clips. Do not silently alter the old source-hash requirements or treat unavailable originals as validated.

Acceptance must use **two separately saved character project files** using the same library clip revision. A single file containing two profiles is not proof of application-wide sharing. Close/reopen the app, edit one assignment, and verify the other project and original library revision are unchanged.

## 3. Animation channels: “any type” is broader than skeletal motion

Plan and report support separately for:

- Skeletal translations, rotations, scales, root movement, fingers/toes, helpers, tails and costume chains.
- Visibility and draw toggles.
- UV transforms and texture/palette swaps.
- Material/color/parameter animation.
- Shape/morph/facial animation.
- Associated model/prop motion and declared camera/light/scene channels.

Preserve original native data and unsupported channels in library revisions. A capability record must separate read, evaluate, edit, retarget/rebind, mirror, bake, write and target-import validation.

FK/IK retargets skeleton motion; it does not transfer texture animations, material graphs or morph targets by itself. Add channel-specific bindings and clear compatibility reports. Morphs require named/semantic correspondences or an explicit alternative. UV/material tracks need compatible target properties and UV/material semantics. Discrete visibility/texture switches must not be interpolated like rotations.

A skeletal-only success must not be labelled complete all-channel conversion. The UI can show “body motion ready; facial bindings unresolved,” preserve the source, and require an explicit partial-export choice. Keep synchronized time mappings among the channels that are supported.

## 4. Animation slots and expandable parts

Create versioned game/build/character-profile slot schemas and distinguish semantic display slots from native identifiers.

A slot record should include stable ID, family, optional part/variant ID, ground/air context, expected channel types, loop policy, expected entry/exit anchors, timing constraints, target-native binding, required/optional/conditional/custom status, and validation evidence.

Native names/indices/archive paths/assets must be supplied by a verified adapter and selected template/character profile. Do not invent a universal filename list. Scan supported existing target assets to help populate mappings and detect unknowns. Human-readable labels and ordering must not be native export keys.

### Example authoring structure, not a claimed universal native format

```text
Character Animation Set
  Movement
    Idle / idle variants
    Walk / run / turns / stops
    Jump / fall / landing
    Crouch / stand
  Attacks
    Jab
      Part 1
      Part 2
      ... Add part
      Optional repeat loop / finisher
    Forward tilt
      Part 1
      ... Add part
    Forward smash
      Charge / hold (when appropriate)
      Part 1
      ... Add part
    Other tilts / smashes / aerials
      Optional variants / landing clips
  Specials
    Neutral
      Variant 1
        Ground: start / loop / release / end / additional parts
        Air: start / loop / release / end / additional parts
      ... Add variant
    Side
    Down
    Up
  Taunts
    ... Add variant / part
  Reactions and miscellaneous
    Hurt / tumble / landing reactions
    Shield / dodge / tech
    Ledge / grab / thrown / get-up poses and motions
    Entry / victory / defeat / item / character-specific groups
```

Each group supports user-added parts and variants within explicit resource budgets. Do not hard-code three jabs, one clip per special direction, or only two-part tilts/smashes. Empty optional slots can be omitted. A character can have a single-part jab or a larger authored sequence.

Distinguish separate native clips from segments cut from a single source clip. A segment assignment records its source time range and loop/entry/exit metadata; export obeys the verified target binding.

Do not flatten a multi-part group into one long animation by default. Keep loopable stages, alternative exits, and optional follow-ups separate. Preview edges are animation authoring metadata, not gameplay execution rules.

Creating an arbitrary number of editor parts does not imply a game can invoke an arbitrary number of native actions. Mark unbound extra parts “animation authored; runtime/native slot mapping unresolved,” retain/export only the supported assets as appropriate, and defer new runtime behavior to the later scripting phase. Never silently discard parts to fit a target schema.

Game template completeness should show missing required slots, inherited fallbacks, unbound optional/custom clips, unsupported output operations and errors. Do not report full character-mod readiness from animation coverage alone.

## 5. Character anchors and reusable miscellaneous template packs

Ask for the target's important reference animations once: at least Idle and Fall, then Crouch, Run/Walk, Landing, Shield, LedgeHang or other anchors required by the selected template pack. Record each anchor as an animation revision plus an explicit frame/normalized phase, facing context and root policy.

An anchor is a sampled pose (and, where needed, local motion at that time), not just a bone naming preset. Do not always sample the idle's last frame or assume an exported loop includes a duplicate terminal frame. The target adapter defines actual frame origins, loop semantics, phase behavior, duration and sampling rules.

Reusable packs should combine source clips with adaptation recipes. A recipe defines candidate slot assignments, required anatomy/props, entry/exit anchors, contact intervals, protected action ranges, allowed blend margins, loop policy and target-profile limitations.

Example recipes:

- Idle -> grounded taunt -> Idle.
- Airborne reference -> aerial action -> Fall.
- Crouch -> stand-up transition -> Idle.
- Fall/contact -> landing -> Idle.
- Part 1 -> Part 2, or Part 1 -> Idle when the follow-up is not played.
- Special start -> hold loop -> release -> end, with separate ground and air endings when supported.

These are authoring proposals, not hard-coded assertions about every game's transitions. Preserve entry poses which intentionally start away from idle: flinches may need an immediate reaction, jump-squat may need an already-crouched pose, and ledge/throw interactions need scene contact constraints. The official Rivals state guide includes such state-specific differences; use it with the installed profile rather than forcing one universal transition recipe. [W2]

Provide a “Generate compatible miscellaneous animations” command that proposes/populates missing slots from a chosen template pack. Skip filled slots by default; show a coverage/change preview, required missing anchors and unsupported cases. Offer one undoable batch commit. Changing character reference clips can regenerate dependent generated slots without overwriting manually locked/custom ones.

Do not claim that idle and fall alone generate convincing get-ups, throws, ledge climbs or combat motions. Templates need actual source motion and constraints. Generic original template fixtures can ship; user-selected game assets remain local under the existing asset policy.

## 6. Smooth transitions without destroying the motion

### The simple UI

For an assigned clip expose:

- Match start: Auto / selected reference / Off.
- Match end: Auto / selected reference / Off.
- Blend margin and strength, with profile defaults.
- Preserve main action and total duration: On by default.
- Contact correction: Auto from reviewed intervals / Off / Edit.
- Preview sequence: reference -> assigned clip -> next reference or part.

Advanced controls can expose phase, joint masks, tangents, root policy and solver parameters. The user should not type bone names or output extensions in the everyday workflow.

### The implementation contract

Matching first and last poses alone is insufficient for smooth motion: the approach to those poses can still change abruptly. Measure position/orientation differences and local motion discontinuities where the target semantics permit them. Do not claim perfect arbitrary phase switching from a static clip.

Retarget first. Apply a deliberate mirror before fitting to the selected final target anchors. Apply endpoint corrections only inside declared start/end margins, using rest-aware transforms and smooth time weights. Preserve protected action keys/intervals, characteristic poses and exact total duration by default. Keep root travel separate from in-place skeletal motion. Do not add or delete frames silently.

Use velocity-aware boundary matching when it is compatible with the destination's actual start phase, playback rate and constraints. Preserve meaningful full turns/Euler winding and fractional timing; a shortest-arc interpolation must not erase intended rotations.

Then resolve contact constraints while preserving explicit endpoint and protected-pose priorities. An unconstrained IK pass after endpoint fitting must not silently ruin the fitted seam. Where constraints conflict, optimize under the declared priorities or report an unsatisfied seam/contact instead of claiming success. Keep aerial motions free of automatic ground locks.

Treat idle/fall/run as loops with explicit playback assumptions. When a runtime can exit the source or enter the destination at variable phases, show that a baked fixed-boundary correction cannot guarantee every interruption is seamless. Preview the declared profile, and label runtime-dependent cases. Do not reintroduce gameplay scripting to solve this phase automatically.

The official Rivals guide says the main character uses directly played clips without runtime blending/dynamic retargeting/IK. Therefore target preview and export must demonstrate the baked transitions, not hide discontinuities with a desktop-only runtime blend. [W1]

### Protected timing and action ranges

The user can mark important poses/times, repeat seams and protected ranges as animation metadata without writing attack code. Short clips with no available margin must request a narrower correction or explicit length change; do not destroy the whole clip to satisfy anchors.

Phase/profile defaults should select safe anchors, but never force every move's beginning/end onto Idle. A follow-up can end in the next part, an aerial can end in Fall, and landing uses its own context-specific route. Use appropriate masks for facial expressions and visibility: no blanket linear blend of discrete channels.

Generated variants remain non-destructive modifier results with reproducible dependency hashes. Native exports are new files that can be decoded and rechecked independently.

## 7. Mirroring

Add a prominent **Mirror left/right** toggle, saved per assignment or as a deliberate library variant. Separate it from facing/heading conversion, reverse playback and texture-image mirroring.

The mirror profile is tied to exact rig interpretation and contains a declared reflection plane, left/right role pairs, central self-mapped bones, root policy, local-axis/rest-basis conversion, optional channel pairs and prop/contact handling. Do not assume one raw X-axis works in every game's coordinate convention.

Reflect the pose and map paired anatomy with correct local/rest transforms. Swapping names or negating all Euler values is not sufficient. Do not use permanent negative object scale as the native export solution. Preserve the target mesh, UVs, clothing and skin weights; the command mirrors motion, not the model artwork.

Paired hand/foot contacts, pole controls, asymmetric facial curves and compatible attachment tracks must follow the mirror mapping. One-sided props/bones or unsupported channels must display a precise warning/require a policy. A right-handed weapon attachment is not automatically interchangeable with a nonexistent left-handed one.

Mirror before target-specific entry/exit fitting so a mirrored attack can still start/end in the character's chosen, possibly asymmetric normal idle. Store modifier ordering explicitly; mirror and retarget/fit are not assumed to commute.

Epic's documented mirror tables cover paired and central bones as well as curves and markers. This is a relevant reference for the mapping model, not a requirement to depend on Unreal for the editor. [W3]

Test mirroring twice at the pure mirror layer against the original within a declared numeric tolerance before lossy baking/refitting. Test native output separately for both mirrored and unmirrored clips. Include centre bones, changed rest axes, fingers, root trajectory, loop seams and at least one asymmetric-prop rejection.

## 8. Retargeting and native outputs

Keep the exact current FK task. Implement reviewed semantic maps and source-relative transfer on genuinely different rigs; then add richer original arm/leg/finger rigs for the library's actual humanoid/contact requirements. Existing two-/three-bone fixtures remain useful regression tests, but are not full humanoid recognition proof.

Do not treat “any character” as a requirement to falsely approve arbitrary anatomy. Auto-recognize supported rigs using reviewed exact profiles, aliases, topology, rest geometry and other verified evidence. Ask for correction only where necessary; reuse accepted profiles for every clip on the same rig. Revalidate when the relevant fingerprints/interpretation change.

Track local axes, hierarchy, missing/additional helpers, proportions, root/pelvis policy, units and scale compensation. Where units remain unknown, report errors in native/normalized units, not fictitious centimetres. Separate skeletal retargeting from mesh rebinding.

Integrate game-neutral evaluated curves/pose data alongside preserved native representations. Do not couple the shared library schema to `CHR0Node` or a Brawl container. Keep adapter-specific identifiers/opaque data in typed native extensions.

Native bindings/exporters handle Brawl, Ultimate, Melee and Rivals separately. Exporting a CHR0 first is a valid proof of rig transfer, not proof of another format's writer. A later non-Brawl adapter must consume the same library/project/retargeting core directly.

For Rivals, deliver the actual supported mesh and baked animation asset import path through the installed Modkit, validate the imported skeleton, clip names/rates/loops and references. Do not claim native cooked/package readiness from an FBX filename. Do not generate gameplay logic in this phase.

Keep required source installation/dump/image paths, original native asset preservation and automatic pinned app-local tool provisioning on the existing roadmap. Add provisioning with the first dependent backend. Do not build all platform readers before delivering a useful shared-library retarget workflow.

## 9. Simple visible workflow

Recommended everyday workflow:

1. **New character animation set**: choose target game/profile and model. Review detected rig only when needed; select Idle/Fall and additional template anchors.
2. **Shared library**: browse/search/preview clips on their source or current target. Register a source model once; batch-add its clips. Filter by slot category and supported channels.
3. **Assign**: drag onto a slot, add parts/variants, toggle Mirror, choose Auto-fit start/end, and review the continuous sequence.
4. **Fill miscellaneous**: choose a pack, preview proposed changes, and apply compatible missing animations in one undoable batch.
5. **Export animation set**: report missing required slots, unresolved mappings, unsupported channels and unbound native slots; write/validate supported native output.

An initial reviewed mapping can require user input; subsequent reuse should not repeatedly demand approval for unchanged evidence. Do not make the user manipulate source-resolution manifests for each ordinary library assignment. Keep the native resource tree and expert editor accessible, not mandatory for common steps.

State badges should distinguish source validation, rig review, instance preview, template fitting, native write/decode, target-tool import and in-game checks. “Animations complete” is separate from “gameplay implemented.” A dialog full of ignored warnings is not an easy workflow; expose specific actionable blockers at the relevant slot.

## 10. Ordered implementation checkpoints

### A. Existing FK transfer, with shared-reference-compatible identity

Finish the current original A->B reviewed map / measured FK / native CHR0 decode task. Persist map and instance state. Do not remove mismatch rejection without implementing the replacement. Preserve original and other-instance hashes; test reverse separately. If library separation is not yet implemented, label that milestone project-local and move immediately into checkpoint B.

### B. True shared library and multiple clips per source

Extract storage responsibility from `CharacterProjectState` into a shared store while retaining project revisions/native evidence. Migrate v1 reads, support several clips per profile, prove reuse across two separately saved projects and independent instances. Implement usable source registration, batch import and library browsing—not only interfaces.

### C. Typed animation slots, parts and template anchors

Replace the bare-string slot assumption with versioned schemas and explicit native mappings. Implement expandable parts/variants, ground/air groups, Idle/Fall reference selection, animation-only workflows, required/optional/custom coverage, and lossless preservation of deferred gameplay drafts.

### D. Mirroring and boundary fitting

Implement reviewed mirror maps, native decoded mirror tests, non-destructive endpoint modifiers, phase/root policies and continuous preview. Preserve clip length/protected action, show conflicts and prove smooth declared seams after export. Add interval-based IK and richer contact fixtures to the same modifier/constraint system; do not claim contact fidelity from FK alone.

### E. Reusable miscellaneous template packs

Implement source-motion-based template recipes, previewable batch fill, reuse per-character anchors, undo/restart persistence, dependent regeneration and locked-custom-clip protection. Cover grounded, aerial, looped and multi-part examples. Do not populate every missing slot with Idle and claim a finished animation set.

### F. Non-Brawl routes and actual target asset import

Connect at least one non-Brawl reader/writer/importer through this same library and core. Prove independently the requested Brawl/Ultimate routes, a Melee route and Rivals Modkit asset import as support becomes real. Keep Dragon Ball importers on the same schema rather than separate per-game libraries. Retain the full channel/model fidelity queue and source providers without inventing completion.

## 11. Acceptance suite

- Two independent saved character projects reference one library clip/revision. Different target rigs and a target-specific edited/mirrored instance do not alter the shared source or other project. Repeat after restart.
- Register one source profile with several clips; same-name profiles/clips from other sources never collide. Import v1 without losing native edits or manual drafts.
- Reviewed original rig A->B FK transfer, genuine export/fresh decode and intended world poses plus blended skinning. Test changed axes/parents/proportions/helpers/scale policy, fractional times, loop seams, rotation winding and independent reverse transfer.
- One-, two- and larger-count jab sequences; multi-part tilt/smash; several neutral/side/down/up-special variants with start/loop/end and ground/air children; arbitrary authoring parts retained with honest native/runtime binding status.
- Same clip on two slots with different references, root policy, timing and mirror settings. Stable IDs survive rename/reorder/reload.
- Grounded clip returns to the chosen Idle phase; an aerial returns to the chosen Fall phase with no foot locks; a multi-part continuation matches the next part rather than forced Idle.
- Endpoint position/orientation error and local motion discontinuity measured where applicable. Protected action frames and original duration remain unchanged by default. Conflicts, insufficient blend margins and phase-dependent cases produce diagnostics.
- Contact IK: planted-foot intervals, hand/prop relationship, unreachable targets and seam/contact conflict. No grounded constraint on an aerial merely because the character has feet.
- Loop wrapping with non-duplicate endpoint conventions; root motion and in-place policies; full-turn and fractional native decode.
- Pure double-mirror round trip, central/self-mapped bones, paired fingers/contacts, changed axes, asymmetric attachment rejection, native mirrored output and unmodified target mesh/UVs.
- Ancillary channel retention/remapping with unsupported combinations visible. Skeletal success must not falsely pass incompatible UV/morph/material channels.
- Template batch fill skips populated/locked slots, rolls back atomically, survives restart and rebuilds only affected dependencies after an Idle/Fall change.
- Target names/resources/indices reflect a verified selected profile. Unmapped added stages are not advertised as game-executable. No gameplay scripts/hitboxes are produced by an animation-only operation.
- Native export is reopened and compared against intended poses/channels/seams; display previews and native-tool/Modkit import results are reported separately. Original-source hashes remain unchanged.
- Bounded store/index operations, cancelled batches, concurrent-library edits, malformed data, cache/revision invalidation and source write guards remain covered.

Use original authored fixtures and expected mappings for portable tests. Commercial input absence does not block them. A test-authored expected map is not a claim of human approval for a commercial rig. Do not claim game/Modkit verification without observing it.

## 12. Concrete code touchpoints and regression commands

Inspected existing code to extend:

- `application/BrawlCrate/Authoring/ProjectRecords.cs`: separate reusable library records from instance state; model typed slot groups, target anchors, channel bindings and modifier recipes; keep compatibility.
- `application/BrawlCrate/Authoring/CharacterProject.cs`: replace the one-clip-per-profile and in-document-only-library assumptions; preserve atomic transactions, native edit leases, revisions and guarded exports. Keep clean contracts with the shared store.
- `application/BrawlCrate/Authoring/BrawlProjectAdapter.cs`: retain Brawl-native validation behind an adapter; keep target-independent pose/slot/library services free of CHR0-only assumptions.
- Existing Character Library UI and model editor: locate actual definitions, wire real controls to the shared store, reviewed mapping, slot tree, mirror/fitting preview and native edit services. Do not replace the application shell.
- `docs/plans/current.md`: keep one next passing acceptance; explicitly defer native gameplay/compiler tasks for this phase.
- `MASTER_PLAN.md`, existing master library spec, `ARCHITECTURE.md`, `FORMAT_SUPPORT.md`, `DEVELOPMENT_STATUS.md`, `TESTING.md` and `AGENTS.md`: reconcile new scope and current implementation without erasing earlier receipts.

Verified current test script flags [R5]:

```powershell
./scripts/build.ps1
./scripts/test.ps1 -Library -Gui
./scripts/test.ps1 -Containers -Gui
./scripts/test.ps1 -DumpSources -Gui
./scripts/test.ps1 -Companions -Gui
./scripts/test.ps1 -Gui
```

Run focused suites separately; the script rejects selecting more than one focus switch together. Add appropriately named tests/modes when implementing new features rather than inventing unimplemented flags. Use the actual available Windows/interactive environment and disclose unrun GUI checks.

Follow the repository's current reviewed source-publication workflow and applicable user authorization: verify exact public origin/main, normal history-preserving push and remote SHA; no force push, repository reset, visibility change, assets/derivatives/configuration/binaries or unauthorized release/CI actions. This specification does not itself modify or publish the repository.

Finish coherent runnable increments rather than replacing implementation with an endless plan. At blockers, preserve the tested checkpoint and record the exact failed acceptance and next task.

## Source register

Repository claims above are grounded in the reviewed snapshot. The rest is proposed implementation behavior based on the user's requirements. External documentation informs target limitations/reference behavior, not claims that those systems are already integrated.

```text
[R1] Commit and metadata:
https://github.com/DoctorFlux/fighterport-studio/commit/b45fcc34a4b437438cb94155f801c7be2feb5a73

[R2] Status, section “Source-bound library and projects”:
https://github.com/DoctorFlux/fighterport-studio/blob/b45fcc34a4b437438cb94155f801c7be2feb5a73/docs/DEVELOPMENT_STATUS.md

[R3] Authoritative current acceptance:
https://github.com/DoctorFlux/fighterport-studio/blob/b45fcc34a4b437438cb94155f801c7be2feb5a73/docs/plans/current.md

[R4] Inspected source records and implementation:
https://github.com/DoctorFlux/fighterport-studio/blob/b45fcc34a4b437438cb94155f801c7be2feb5a73/application/BrawlCrate/Authoring/ProjectRecords.cs
https://github.com/DoctorFlux/fighterport-studio/blob/b45fcc34a4b437438cb94155f801c7be2feb5a73/application/BrawlCrate/Authoring/CharacterProject.cs

[R5] Existing regression runner:
https://github.com/DoctorFlux/fighterport-studio/blob/b45fcc34a4b437438cb94155f801c7be2feb5a73/scripts/test.ps1

[W1] Rivals II official animation guide:
https://rivals2.com/workshop/knowledge-base/character-creation/animation-overview/

[W2] Rivals II official animation-state reference:
https://rivals2.com/workshop/knowledge-base/miscellaneous/animation-states/

[W3] Epic official mirroring documentation:
https://dev.epicgames.com/documentation/unreal-engine/mirroring-animation-in-unreal-engine
```
