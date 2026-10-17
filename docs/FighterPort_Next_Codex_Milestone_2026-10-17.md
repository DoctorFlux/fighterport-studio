# FighterPort Studio — next implementation brief

**Reviewed:** 17 October 2026  
**Repository snapshot:** `20a8dc572446bff036c3388c0a101ddf98407ce2`  
**Latest functional change reviewed:** `829527f1f42b982b3e305b1832313df24fbac507` — persistent Brawl companion resolution.

This is a continuation of the completed clean restart, not another replacement architecture. Current-state observations below come from the pinned repository documents and selected implementation files. Milestone refinements and acceptance tests are recommendations. This audit did not build the Windows application, inspect local test captures, or run a game/Modkit.

## 1. Decision: continue this application

The real BrawlCrate-derived WinForms/net472/x86 application is now the active product. Do not restart again, restore the old WPF shell, or replace existing native editors with a new imitation. Preserve BrawlCrate/BrawlLib provenance and the existing product's independent settings, update safeguards and source protections.

The immediate executable task remains the R2 criterion in `docs/plans/current.md`: persistent multi-root Brawl loose-dump discovery with explicit ordered overrides. Finish that as a bounded milestone. Follow it with Brawl character-source readiness and a real R3 character-project/retargeting/export path. Do not interpret R2 as requiring every installation and container format for every game before retargeting can begin. Keep additional providers as explicit, individually tested milestones rather than deleting them from scope.

## 2. Audited state — do not confuse these levels

### Present in the restarted product

- Actual BrawlCrate application, resource tree, native editors and OpenGL previewer.
- `SourceFolderSession`: read-only source registration, immediate-directory browsing, signature checks, bounded staging and source/copy hashes. It limits a listing to 512 entries and individual staged resources to 64 MiB.
- `CompanionWorkspace`: explicitly selected Brawl model, texture and skeletal-motion companions; missing/ambiguous candidates; persistent `.fpcompanion` resolutions; staged native assembly and output reopening.
- Repository-reported native and actual-control GUI checks, including fresh product launches. These are documented results, not tests rerun during this audit.

### Narrow boundaries visible in implementation

`application/BrawlCrate/Sources/CompanionWorkspace.cs` currently accepts BRRES/TEX0/CHR0 roots. Its `Validate()` restricts the companion route to MDL0 v11, TEX0 v1 RGBA8 without palettes, and CHR0 v4. Motion channels are checked for exact bone-name membership; this is not a rest-pose/hierarchy/fingerprint comparison and not cross-rig retargeting.

Only one selected motion is added. Other companion channels are not imported. `.fpcompanion` stores source choices, identities and hashes, not the unsaved native edits or a complete character project. `ValidateOutput()` checks resource presence and selected metadata; it does not establish complete curve, skinning, palette, material or opaque-data equivalence.

The documented native fixture is a two-bone, four-vertex rigid paddle. The retained SRT0 fixture tests an empty entry's name/timing/loop metadata. Neither establishes blended-skinning correctness or fully animated ancillary-channel fidelity.

### Still planned

Whole-game/install/image providers; complete Brawl companion/container coverage; game-neutral character projects; modern cross-rig/contact/bake services; other game adapters; Rivals Modkit integration; complete native target packages and gameplay compilers.

Do not transfer old pre-restart WPF/worker test counts into this product. Do not report all inherited BrawlCrate operations as newly verified merely because their source exists.

## 3. Permanent scope

Keep every row in the existing master plan:

- Smash 64; Melee; Brawl/Project M/Project+; Smash for Wii U; Smash for 3DS; Ultimate.
- Rivals of Aether II installed-game sources and editable Modkit/project sources.
- FighterZ; Sparking! ZERO; Xenoverse; Xenoverse 2; extensible additional games.
- Source AND target-native/playable-mod roadmap rows, including Dragon Ball. A reader does not imply a writer, and one conversion direction does not imply its reverse.
- Game/dump/raw-character folders; applicable ROM/disc/platform containers; multiple mounts and overrides; safe lazy indexing, decompression and staging.
- Native asset editing, new/imported characters, rig/weight/mesh work, real alternate-outfit meshes, materials/textures, keyframe/curve editing and clip mixing.
- Skeletal, visibility, UV, texture/palette swaps, material/color, shape/morph/facial, scene/camera/light and other declared channels.
- Retargeting, corrections, measured baking, optimization, standalone interchange and optional equal Blender/Maya/3ds Max bridges.
- States, attacks, hitboxes/hurtboxes, movement, throws, articles/projectiles, effects and sound/events, using separate target-game authoring/build paths.

The core remains local, standalone and non-AI, without a required DCC or cloud service. An optional Goku authoring rig must not become a mandatory skeleton for all characters or games.

## 4. R2 next — multi-root discovery that feeds the existing workspace

### Implementation

Extend `application/BrawlCrate/Sources/`, retaining current staging and source-write protections. Suggested NEW responsibilities, not existing files or APIs:

- `SourceMount`: stable mount identity, local root, profile, priority, enabled state and path semantics.
- `ISourceProvider`: bounded enumerate/probe/read operations; no editor coupling.
- `SourceCatalog`: lazy metadata/resource index with invalidation and explicit budgets.
- `BrawlLooseDumpProvider`: discovers supported loose resources and exposes candidates to the current companion UI.
- `OverrideResolver`: applies documented, deterministic rules and reports the winner, excluded candidates and ambiguity.

Do not eagerly call `CompanionWorkspace.AddSource()` for every file in a whole dump. It stages/parses explicit sources and has intentionally small working-set limits. Discovery should index lightweight candidates; stage and decode only selected resources/dependencies within their own budgets.

Root priority must apply to the same logical asset path or a game-profile-defined resource scope, not globally to every texture or animation with the same name. Preserve BRRES/container identity and source lineage. Identical names in unrelated characters must not collide.

An explicit user-selected resource/hash remains authoritative until deliberately changed. Equal-priority unresolved collisions must not become a filesystem-order or first-match decision. Show the chosen root and the reason for the resolution. Source override selection must not silently bypass the existing prohibition on overwriting embedded base-BRRES resources; introduce any replacement as an explicit, tested edit transaction.

Keep root registrations/priorities separate from pending character edits. Missing roots, changed source hashes and changed native identities need actionable diagnostics. Preserve explicit user choices when a new scan discovers additional candidates.

Provide cancellation and visible progress. Keep file-count, depth, parsed-resource, memory/cache and per-operation byte budgets; report truncation rather than presenting a partial scan as complete. Handle reparse points, stale caches, missing directories and overlapping roots consistently with existing guards.

### Acceptance

Using original fixtures, configure a base dump, one override folder and a second conflicting folder. Restart the actual application and restore roots and priorities. Select a model and texture/motion companions through the existing workspace. Show the winning source, visibly preview/play, save a separate native BRRES and reopen it.

Test equal-priority ambiguity, an explicit manual override, unrelated same-name resources, missing dependencies, changed hashes, cancelled discovery, budget exhaustion, reparse rejection, repeated restart and unchanged original hashes. Cancelling discovery must not discard already saved work.

Keep unsupported PAC/PCS companion nesting, compressed inputs and images explicitly unsupported in this milestone. Do not quietly broaden admission checks without a provider-specific validation path.

## 5. Brawl character-source readiness — close the real input gap

After the immediate R2 task, bring the Brawl character workflow beyond the narrow paddle fixture. This is an operation-coverage increment using the inherited native implementation, not a rewrite of its renderer and editors.

Inspect the configured local Goku/Brawl inputs only as explicit opt-in integration sources. Record actual archive, model, texture and motion versions before deciding what to implement. Never assume they use the fixture's MDL0 v11, RGBA8 or CHR0 v4 subset.

Add container-aware companion resolution for the required nested ARC/PAC/PCS/BRRES layouts, with bounded decompression, entry identity and native hierarchy preservation. Reading an image, reading a contained archive and rebuilding a character package are separate capabilities. Do not flatten a native fighter package into BRRES and call it a package round trip.

Expand the operation matrix for MDL0, CHR0, SRT0, PAT0, CLR0, VIS0, SHP0, SCN0, TEX0/PLT0 and material/shader data. Every row must state whether it is imported, evaluated, editable, preserved, exported, decoded and independently checked. Complete Brawl support remains required; individual unsupported rows stay visible rather than disappearing from scope.

Use nontrivial original fixtures: multiple bones and blended weights; changed bone parents/rest axes; multiple materials; asymmetric textures and UVs; paletted alpha/mips; nonempty animated visibility/UV/material/shape channels. Test synchronized playback, editing and native output on those channels. Source preservation and rendering equivalence are different assertions.

Preserve unrelated native entries and unknown records when the writer can do so reliably; otherwise fail with a precise loss explanation. Resource-name presence is not proof of opaque-data preservation. Reopen rebuilt containers and compare actual decoded content. Keep original-path overwrites prohibited.

## 6. R3 — the first real character project and retargeter

### R3.1: native data plus editable cross-game state

Create a versioned character-project layer separate from `.fpcompanion`. Suggested NEW module boundaries are `application/FighterPort.Core`, a Brawl adapter, retargeting services and validation services. Names can follow local conventions; responsibilities must remain distinct. Integrate them with the existing visible BrawlCrate-derived application.

Preserve:

- Source mounts, native/container paths, hashes, native versions, dependencies and opaque records.
- Bone IDs, parents, bind/rest matrices, coordinate conventions, units, skin weights and relevant scale/compensation semantics.
- Clip times/rates, curves, tangents, Euler winding/full turns, event times and ancillary bindings.
- Materials, textures, mesh parts, alternate outfits, morph/facial and attachment information.
- Source-quality state, independently editable target branches, mappings, correction layers and explicit losses.
- Actual edits across save/restart, with undo/redo and safe atomic writes.

Keep native source data authoritative for source-format round trips. A canonical representation must not silently erase source semantics or replace the native archival copy. Non-Brawl assets must not be serialized through MDL0/CHR0 merely to fit the editor. Supply a renderer-facing scene/animation adapter instead.

Do not blindly migrate the x86 UI to a new runtime. Keep a compatible core for the first integration. Where modern dependencies or large archives require 64-bit execution, add an isolated worker with a versioned bounded protocol, cancellation, crash diagnostics and 64-bit file offsets. This must still be one user-facing application, not an external-editor launcher.

### R3.2: transfer motion between genuinely different skeletons

Implement and expose source/target skeleton inspection, fingerprinting and reviewed semantic chain maps. Names are hints, not rig equivalence. Fingerprints need relevant hierarchy, bind/rest and transform semantics, not just a list of names.

Provide rest-pose calibration, local-axis/parent-space conversion, explicit unit/handedness conventions, root/pelvis policies and missing/extra-bone treatment. Add proportion-aware limb/spine chains, twist/helper distribution and separate fingers/toes/tails/facial policies. Do not invent a mapping for ambiguous bones and mark it approved.

Keep unsupported scale/shear/compensation cases explicit until implemented. A solution that discards source scale or normalizes away an intentional motion must not pass by default.

Implement editable contact windows and IK/contact correction for planted feet, hand grips and props, with knee/elbow bend direction and reach diagnostics. Goku-specific examples include planted feet during a charge and keeping both hands together for a beam pose. These are examples for acceptance, not hard-coded solver behavior.

Use non-destructive correction layers and reusable profiles keyed to verified rig identities. Support applying a reviewed profile to batches of clips without redoing the map, while retaining per-clip contact/correction data. Animation retargeting and mesh rebinding/weight transfer remain separate operations.

### R3.3: bake and prove the exported result

First implement an actual different-rig Brawl-to-Brawl animation route on original assets: source rig A and target rig B must differ meaningfully. This is cross-rig proof even though the file family is unchanged. Then add a second game/native adapter or the actual Rivals interchange route to prove the shared core is not Brawl-only.

Evaluate source-native animation correctly, transfer onto the target rig, apply corrections, bake with explicit sample/event timing, write a genuine output, reopen it and recompute target-space poses/skinning. Report results separately for the in-editor transfer and the decoded output.

Acceptance fixture suite:

1. Identity transfer preserves expected motion/rest/binds.
2. Same bone names with changed axes or parents cannot pass as identical rigs.
3. Different names, proportions and bone counts transfer using a reviewed map.
4. Root-motion and in-place policies are explicit and separately tested.
5. Twist/helper and scale/compensation cases either work with evidence or reject.
6. Planted-foot and two-hand/prop constraints have measured errors.
7. Euler full turns, fractional key times, loop seams and important event timings survive the declared output path.
8. Blended-skinning positions are checked, not merely bone transforms or a rigid mesh.
9. Missing/ambiguous bones, unreachable constraints and unsupported channels produce explicit diagnostics/loss reports.
10. Save/restart/undo/redo reproduces the mapping, corrections and baked target branch.

Configure and record tolerances for target pose/orientation, end-effector/contact error, foot sliding, loop seams and post-export curve/pose error. Use independent analytic expectations on original fixtures where possible. Do not compare raw world-space source positions to proportion-changed target positions as though they should be identical. Check the intended target motion and constraints.

Test both directions independently. Different rigs or formats may be lossy; a successful A-to-B transfer does not promise exact A-to-B-to-A recovery. A writer and reader sharing a library are useful checks but not independent-tool validation.

## 7. R4 — Goku becomes a real Rivals II target, not just an exported model

Use configured local source paths and the installed Rivals Modkit. Inspect the actual version, asset templates, schemas and available import/build hooks. Do not hard-code an engine version from an earlier chat or invent APIs.

Assemble the user's chosen Goku source model/rig and selected motions; keep alternate costumes as real outfit branches, including needed additional bones/materials. When another game's source adapter is not ready, report that blocker and continue original-fixture development without calling a substitute fixture a completed Goku port.

The first target acceptance should be modest but end-to-end: the selected rigged mesh plus idle, crouch/run, jump/fall and one attack, with required state configuration/hurtboxes. Retarget at least one genuinely different-rig clip. Export baked target animation, import into the actual Modkit, configure a real attack using supported target assets, build/cook through the real workflow and observe it in the editor/game when available.

Rivals documentation requires 60-fps authored/baked joint/morph animation and states that the main character mesh does not support dynamic IK/retargeting. Finish those corrections before exporting. Its FBX import needs bones, geometry and skin weights for a skeletal mesh. Validate the actual imported Skeleton/Skeletal Mesh/Animation Sequence, not just that a file called `.fbx` exists.

Select a genuine standalone-capable interchange writer after verifying its export API and dependency/license requirements. Do not assume an import-only library can export FBX, write renamed JSON as FBX, or impose Blender as a mandatory workaround. Optional DCC bridges remain optional. A missing local Modkit or licensed tool blocks its validation, not all independent development.

Animation conversion does not recreate Brawl moveset execution. Keep attack/state timing, hurtboxes, hitboxes, root movement, throws, projectiles, effects and sound events in a target-specific gameplay branch. Translate representable behavior, require explicit treatment of unsupported behavior and tune the result in the target game. A playable smoke test is not evidence that Goku's full moveset is complete.

## 8. Adapter roadmap — independent source and target capabilities

These are research candidates, NOT existing FighterPort integrations or guarantees of game-build compatibility:

| Family | Candidate basis / next work |
|---|---|
| Brawl / Project M / Project+ | Retain BrawlLib and the actual native editors. Expand operation/version coverage and container-preserving character assembly/output. |
| Ultimate | Evaluate `ssbh_lib` / `ssbh_data` for SSBH models, skeletons and NUANMB motion. Texture/material and helper semantics, gameplay and mod packaging remain separate. |
| Melee | Evaluate HSDLib/HSDRaw for HSD DAT structures. Character assembly, animation semantics and executable/gameplay changes need their own adapter and validation. |
| Smash Wii U | Evaluate Smash Forge as a format/editor reference for NUD/NUT/VBN/OMO and related data. Do not assume this covers Smash 3DS. |
| Smash 3DS and Smash 64 | Maintain distinct game/platform adapters, format inventories and original fixtures. Do not claim implementation merely because they have roadmap rows. |
| FighterZ and Sparking! ZERO | Evaluate CUE4Parse and its export facilities as an Unreal reading foundation, with distinct tested profiles per title/build. Reading cooked assets is not a native mod cooker or recovered source project. |
| Xenoverse / Xenoverse 2 | LibXenoverse-family projects document ESK skeleton, EMD mesh, EAN animation and material/texture companions. Review per-file licensing and legacy SDK dependencies before code reuse; do not assume an unrestricted library license. Keep each title's writer/gameplay/package route separate. |
| Rivals II | Separate installed cooked-source inspection from editable Modkit authoring/import/build. Validate through the installed target tools. |
| GameCube/Wii images | Evaluate `nod` as a bounded image/filesystem reader. Opening ISO/GCM or another supported image does not provide character decoding or character-package writing by itself. |

Prioritize Brawl readiness, the shared retarget core and the first Rivals target. Ultimate provides a useful early non-Brawl native-format test. Add the requested Dragon Ball sources and remaining Smash adapters without replacing the core or forcing cross-game data through Brawl resources. Keep all reverse/native-output routes as independent obligations rather than promising automatic lossless conversions.

Track capabilities per game build, format/version, operation and directed route: probe, enumerate, decode, preview, edit, preserve, retarget, bake, native-write, package, Modkit-build and in-game test. Labels should distinguish planned, implemented and independently verified. No single green “supported” badge.

## 9. Codex operating instructions

Read the real current `AGENTS.md`, clean-restart brief, original requirements, upstream/component records, master plan, architecture, status, format matrix, testing guide and `docs/plans/current.md`. Compare actual HEAD to this reviewed snapshot and preserve later completed work.

Do not produce another master-plan-only response. Implement the current R2 acceptance first, finish a coherent passing checkpoint, then advance the one authoritative queue toward Brawl readiness and R3. A complete phase requires user-visible behavior and persisted state, not only interfaces, mock data or test-name changes.

Retain the existing commands:

```powershell
./scripts/doctor.ps1
./scripts/build.ps1
./scripts/test.ps1
./scripts/test.ps1 -Gui
./scripts/test.ps1 -Companions -Gui
./scripts/run.ps1
```

Add focused test selection where needed without dropping previous suites. Original fixtures are the portable baseline; configured commercial assets are opt-in and stay local. Never overwrite source games, upload their contents/captures, execute arbitrary source scripts, accept EULAs automatically or install globally.

Update existing `docs/MASTER_PLAN.md`, `docs/ARCHITECTURE.md`, `docs/FORMAT_SUPPORT.md`, `docs/DEVELOPMENT_STATUS.md`, `docs/TESTING.md` and the single `docs/plans/current.md` after each passing increment. Link this continuation from `AGENTS.md` only as a supplement, not a replacement for the clean restart and original requirements. Keep detailed future work in a milestone plan, not several competing “current task” files.

The repository records authorization for the previous source-replacement upload only. Follow current authorization for any new publication; do not infer permanent automatic push permission from this planning request. Preserve exact origin/history, review staged and outgoing diffs, and do not force-push or publish binaries. Where no new push authorization exists, keep the checkpoint local and report it accurately.

At a genuine blocker, complete the largest coherent tested increment and record the exact failing prerequisite, diagnostic and next executable acceptance. Do not claim local GUI, native-tool, Modkit, game or remote-publication results that were not observed.

## Sources

Pinned repository sources supporting the audit:

- [README](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/README.md)
- [Current acceptance criterion](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/docs/plans/current.md)
- [Development status](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/docs/DEVELOPMENT_STATUS.md)
- [Architecture](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/docs/ARCHITECTURE.md)
- [Master plan](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/docs/MASTER_PLAN.md)
- [Format support](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/docs/FORMAT_SUPPORT.md)
- [Testing](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/docs/TESTING.md)
- [CompanionWorkspace.cs](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/application/BrawlCrate/Sources/CompanionWorkspace.cs)
- [SourceFolderSession.cs](https://github.com/DoctorFlux/fighterport-studio/blob/20a8dc572446bff036c3388c0a101ddf98407ce2/application/BrawlCrate/Sources/SourceFolderSession.cs)

External primary references, checked during this review; local installed versions still require inspection:

- [Rivals animation guide](https://rivals2.com/workshop/knowledge-base/character-creation/animation-overview/)
- [Rivals skeletal-mesh/animation import](https://rivals2.com/workshop/knowledge-base/character-creation/import-into-unreal/)
- [Rivals attack setup](https://rivals2.com/workshop/knowledge-base/character-creation/setting-up-an-attack/)
- [SSBH libraries](https://github.com/ultimate-research/ssbh_lib)
- [HSDLib](https://github.com/Ploaj/HSDLib)
- [Smash Forge](https://github.com/jam1garner/Smash-Forge)
- [CUE4Parse](https://github.com/FabianFG/CUE4Parse)
- [LibXenoverse](https://github.com/DarioSamo/LibXenoverse) and [LibXenoverse2](https://github.com/Olganix/LibXenoverse2)
- [nod image library](https://github.com/encounter/nod)
