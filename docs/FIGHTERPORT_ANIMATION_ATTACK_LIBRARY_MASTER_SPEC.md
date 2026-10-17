# FighterPort Studio — reusable animation and attack authoring master specification

## Proportion-aware humanoid amendment — 2026-10-17

The user now prioritizes proportion-aware, contact-preserving humanoid retargeting in
the existing Model Previewer. See [FIGHTERPORT_PROPORTION_AWARE_HUMANOID_RETARGETING.md](FIGHTERPORT_PROPORTION_AWARE_HUMANOID_RETARGETING.md).
Mirroring is not a prerequisite for fitting or later work. Preserve compatible data and
useful inherited operations; coordinate-basis conversion is distinct from animation mirroring.
The historical proposals below are superseded wherever they impose mirror priority.
Only docs/plans/current.md defines the next executable acceptance; native/source/contact
quality and game evidence remain separate. Gameplay compilation remains deferred.


## Model Previewer amendment — 2026-10-17

[FIGHTERPORT_MODEL_PREVIEWER_WORKSPACE.md](FIGHTERPORT_MODEL_PREVIEWER_WORKSPACE.md)
adds the standing integration rule: model/animation features need functional controls
inside the actual Model Previewer, using the same project/session as the library and
resource entry points. Preserve native rendering/camera/playback/editors. Explicit
project/target/fingerprint/instance/document/editor bindings and safe pending-edit
handoffs take precedence over last-window/global-root inference. Project slot/anchor
metadata, native motion fidelity and game/Modkit evidence remain separate.

## Scope amendment — 2026-10-17

The [shared animation/library amendment](FIGHTERPORT_SHARED_ANIMATION_LIBRARY_SLOTS_TRANSITIONS.md)
sets the current phase: model/animation authoring and native asset import/export first.
The historical specification below retains long-term requirements. Gameplay scripting,
damage/hitboxes, ACMD/PSA/Lua, cancel/combo execution and playable moveset compilation
are deferred. Preserve existing manual drafts without exposing them as animation prerequisites.
The library must be shared across separately saved projects, with many clips per source,
immutable pinned revisions and lazy bounded disk storage. Animation slots have expandable
parts/variants and verified native bindings; arbitrary authoring parts remain explicitly
unbound. Reviewed rig transfer precedes mirror and target-reference fitting. Idle/Fall
anchors do not synthesize all motions. Mirror/transition/template/IK and non-Brawl routes
need their own native decoded evidence. No desktop-only blend proves a baked transition.

Prepared: 2026-10-17.
Reviewed repository snapshot: `DoctorFlux/fighterport-studio` at `19ae7b2aa14c24d9cd6e6e8ef0bbe6769e9cbda4`.

This is a proposed product and implementation specification, not an implemented-feature report. Re-read actual HEAD before acting. The audit below is source/document review; no local Windows build, commercial game extraction, Modkit session or in-game test was run for this specification.

## 1. Product decision and priorities

FighterPort Studio is the existing BrawlCrate-derived application extended into a local, standalone, multi-game character and moveset authoring environment. It is not merely a converter, an animation viewer, an external-tool launcher, or a Brawl-only editor.

The central workflow is:

**Register game sources → register a source character/rig → add animations and attacks to a reusable library → choose a target character → retarget and adapt a move → assign it to a moveset → generate native assets and gameplay → build and validate the target mod.**

Primary full-character/moveset targets in this request: Brawl (including the retained Project M/Project+ profiles), Melee, Ultimate and Rivals of Aether II. Required additional model/animation sources: Dragon Ball FighterZ, Sparking! ZERO, Xenoverse 1 and Xenoverse 2. Preserve the existing master plan's broader Smash 64/Wii U/3DS, Dragon Ball native-output, interchange, optimization and optional DCC scope; do not erase it when prioritizing the four primary targets.

The user's defining acceptance example is Brawl Mario's forward aerial applied to Ultimate Snake's native skeleton and forward-aerial slot, and the reverse direction tested independently. Goku assembled from multiple Dragon Ball/Smash sources and authored for Rivals II is another required end-to-end case, not the only character the architecture supports.

Do not restart, replace the current UI with WPF, or force all games into MDL0/CHR0. Keep the actual BrawlCrate-derived editors and native capabilities. The user should work in one FighterPort application; compatible libraries and supervised headless workers can implement different backends without exposing a collection of unrelated GUIs.

## 2. What the inspected repository actually establishes

The inspected commit contains a completed bounded multi-root Brawl loose-dump discovery checkpoint and standing authorization for reviewed passing source updates. The authoritative next task has moved beyond folder discovery to a container-preserving nested ARC/PAC → BRRES companion slice.

`docs/ARCHITECTURE.md` identifies the active application as `application/BrawlCrate/BrawlCrate.csproj`, retaining WinForms, .NET Framework 4.7.2, x86 and OpenTK/OpenGL. BrawlLib is in-process. Modern/64-bit worker boundaries are already a permitted architectural direction for large archives or incompatible dependencies.

Actual code in `application/BrawlCrate/Sources/CompanionWorkspace.cs` establishes these boundaries:

- `CompanionWorkspace` describes itself as Brawl-specific dependency selection, not a game-neutral project or retargeter.
- `ScopeNotice` restricts its implemented assembly path to MDL0 v11, TEX0 v1 RGBA8 without palettes and CHR0 v4.
- `Validate()` checks animation track names against model bone names; it does not solve different rigs.
- The path selects one motion and selected textures, not a reusable animation/attack collection.
- `.fpcompanion` records source resolution, not dirty edits, a character project, or a moveset library.

`docs/FORMAT_SUPPORT.md` marks retargeting, contacts, baking, full character authoring and gameplay conversion as future work. Broad inherited Brawl readers/editors are not equivalent to newly verified support. Do not transfer old pre-restart WPF test counts or capabilities into this codebase.

The current next task explicitly reports Brawl/Ultimate Mario locations in local configuration but no configured Goku source. Do not pretend to have inspected missing Goku assets. Portable original fixtures must allow progress without commercial inputs.

Sources: [R1]–[R6].

## 3. A reusable library, not a directory of detached clips

Introduce the following versioned concepts. These names describe proposed domain objects, not existing classes or finalized file extensions.

### SourceCharacterProfile

One registered source character/model is associated with its exact source skeleton and native identity. Store:

- Source game/build/platform/profile and mount identity.
- Source resource paths, complete nested-container entry identities, source hashes and native format versions.
- Source model identity and skin binding, including the bind/inverse-bind data required to interpret the source mesh correctly.
- Skeleton IDs, bone names, parent graph, native local axes, coordinate basis, units, reference/rest transforms, scale conventions and relevant native flags.
- A separate fingerprint of the skeleton/interpretation metadata; do not key compatibility only by the displayed character name.
- Semantic characterization and rig-control profile revisions, with confidence, reason and approval state.
- Optional outfit/mesh variants sharing a verified compatible skeleton; a changed skeleton invalidates dependent assumptions.

Selecting the model that a clip belongs to should establish this association once, and batch-added clips can inherit it. Each clip still needs track/binding validation. Never silently attach a standalone animation to the currently displayed model just because the names look plausible.

A skeleton profile can be sufficient for retargeting where the mesh is unavailable, but mesh preview, weight analysis and geometry-based recognition must then be marked unavailable. Retain the skeleton snapshot and provenance required to interpret a saved library entry; relinking to a different source must be explicit.

### AnimationAsset

Store immutable or revisioned source motion plus editable authoring layers:

- Source character profile and exact skeleton fingerprint.
- Native source identity and original native data/opaque fields.
- Clip duration, source timebase, sample/key times, looping, interpolation, tangents, Euler rotation order and winding where present.
- Absolute/additive/reference-pose semantics, missing-track defaults, translation and scale rules, constraints/helpers and native evaluation requirements.
- Skeletal, facial/morph, visibility, UV, texture/palette swap, material/color and supported scene tracks, each independently capability-tagged.
- Contact annotations, root-motion interpretation, action tags, preview settings, and optional events.

The library is not an FBX-only or glTF-only archive. Interchange exports are derived outputs and can lose data. Source evaluators and raw native retention remain available.

### MoveAsset

A reusable attack combines animation references with an authored gameplay description and its native source evidence:

- One or more AnimationAssets; grounded/aerial/landing/charge/release/recovery variants where applicable.
- Startup/active/recovery windows, transitions, interrupts/cancels and source-time-to-gameplay-frame mapping.
- Hitbox and optional hurtbox/invulnerability changes, semantic anchors, source bone IDs and local frames, shapes, offsets, radii and on-hit data.
- Groundedness, movement, root-motion policy, gravity/facing policies and landing behavior.
- Sounds, VFX, trails, attachments, projectiles/articles, props and dependency references.
- Throw/paired-character roles, synchronized clips, grip points, release events and victim bindings when relevant.
- Source scripts/native records and target-independent typed operations where a translation is understood.
- Explicit unresolved native commands and target-specific extension nodes where automatic translation is not established.

An imported animation is not automatically an imported attack. An animation-only entry can exist immediately, but the UI must distinguish it from a MoveAsset with verified attack data. Importing gameplay requires its actual source files or explicit manual authoring; never infer damage, active frames or hitboxes from poses alone.

### MoveInstance

Applying a library move to a character creates an instance containing:

- The selected library revisions and target character/rig profile.
- The reviewed retarget profile and per-clip corrections.
- Target slot assignment and timing/physics/size choices.
- Target-specific gameplay overrides, dependency replacements and validation findings.
- Generated-file manifest and tool/adapter/version fingerprints.

Editing Snake's instance must not modify Mario's library original. Library updates should be reviewed revisions, with dependent instances marked stale rather than silently rewritten. Cache keys include input hashes, skeleton fingerprints, solver/mapping revisions, target profile, parameters and tool versions.

### Library interaction

Implement source-model groups, searchable tags, folders/collections, thumbnails, looping previews, favorites, batch import, duplicate detection and source relinking. A library card clearly distinguishes animation-only, attack-data-present, unresolved-dependencies and target-build status.

Provide Apply Animation Only, Apply Animation + Attack, Apply Attack Data Only, and Create New Move. Allow drag/drop onto a target moveset slot with an explicit compatibility/loss preview and undoable commit. Partial-body masks, clip mixing, mirroring, timing edits and additive layers belong in the authoring system, with their effects on events and dependencies tracked.

Keep game assets and derived commercial previews local by default. Sharing a library manifest or a tool source repository must not automatically upload referenced assets. Original/shareable assets and recipes may be packaged separately according to their provenance and permissions.

## 4. Character and moveset projects

A versioned character project owns source references, models/parts, skinning, rigs, materials, real alternate outfits, animation instances, move slots, state graphs, target branches and build settings. `.fpcompanion` may remain a source-resolution input; do not expand it ad hoc into every unrelated responsibility.

Save/reopen must preserve actual edits, mapping approvals, IK/FK controls, correction layers, event/timing edits and target overrides. Commands must be transactional and undoable; define which undo history is persisted and never imply that saved state alone is persisted history.

Target branches are independent. Exporting a lower-detail Brawl version must not reduce the master/source-quality mesh, skeleton, textures or animations. Model conversion, animation transfer, mesh rebinding and gameplay translation must have distinct commands and capabilities.

Support new original characters as well as imported ones, native keyframe and curve editing, mesh parts and weights, textures/materials, face/morph editing, secondary bones, accessories and genuine outfit variants. Recoloring is not a substitute for a different gi or costume mesh.

Suggested project organization (FighterPort project layout, NOT a claim of native game folder layouts):

```text
MyCharacter/
  project.json
  sources/
  rigs/
  library-links/
  animations/
  moves/
  costumes/
  materials/
  targets/
    brawl/
    melee/
    ultimate/
    rivals2/
  generated/
  validation/
```

## 5. Automatic skeleton characterization

Automatic characterization is required, but uncertainty must be represented rather than concealed. It should not require a cloud/AI service.

Use a layered, deterministic pipeline:

1. Exact previously reviewed skeleton fingerprint/profile match.
2. Versioned game/character-family templates and aliases for native naming conventions.
3. Hierarchy/topology analysis, connected-chain structure, symmetry, normalized rest positions and relative bone lengths.
4. Skin-weight regions and mesh geometry when available.
5. Optional motion evidence from selected calibration clips, without assuming every clip is a standing idle.
6. Constraint validation and a review screen for ambiguous/missing results.

An algorithmic match score is not automatically a calibrated probability. Show High/Medium/Low confidence and the actual evidence or conflict. A previously reviewed exact fingerprint may apply automatically; an unknown or changed rig must not silently become an approved mapping.

Required humanoid role vocabulary:

- World/motion root separately from pelvis/hips/waist.
- Ordered spine chain, chest/upper chest, neck chain, head and jaw/face roles when available.
- Left/right clavicle/shoulder, upper arm, forearm, wrist/hand, elbow landmark and bend direction.
- Thumb and each available finger chain, metacarpals/phalanges where present.
- Left/right hip/thigh, shin, ankle/foot, knee landmark, ball/toes and heel/sole contact landmarks.
- Twist/helpers, attachments/props, tails, ears, wings, hair, clothing and other secondary/deform bones.

Some roles are landmarks or semantic chains, not additional bones. A knee often refers to the joint at the shin's parent relationship; a heel may be a virtual contact point rather than a native bone. A missing finger/toe bone must not be invented or assigned to a random helper. Preserve optional bust/secondary-chest deformation independently of the central chest/spine role.

Use one-to-many/many-to-one chain relationships when rigs have different spine/neck/finger counts. Preserve stable native bone IDs, names and hierarchy on target rigs unless the user explicitly requests rig editing. A shared semantic vocabulary is not one mandatory universal export skeleton.

For nonhumanoids, provide extensible rig classes, custom chains and manual role overrides. Any-character support means the architecture can represent and adapt those characters, not that an arbitrary topology always has a meaningful fully automatic humanoid mapping.

Reference: Epic's automatic chain generation analyzes hierarchy and names against known templates and explicitly reports unmatched cases [R19]. Use the behavior as a reference, not Unreal source code transplanted without its applicable license.

## 6. Editor-side IK/FK control rigs

Add virtual control rigs over native deformation skeletons inside FighterPort. Do not insert editor-only controls into a target's exported native bone list.

FK is required for preserving pose language and direct bone/chain editing. IK is required for keeping a hand, foot or prop at an intended contact/goal. Provide arm and leg two-bone IK, pole controls, hand/foot orientation, heel/toe/sole landmarks, pelvis compensation, head/aim controls, chain/spine controls, IK/FK blends, pose matching without pops and correction layers.

Use bounded solvers with clear limits. Handle straight limbs, near-singular poses, unreachable goals, zero-length helpers, temporal pole continuity, orientation constraints, joint limits and stretch policies explicitly. Do not advertise full-body constraint solving because a two-bone solver exists. Multi-contact tasks need pelvis/body coordination and explicit priorities.

Contact annotations must contain an interval, contact type, coordinate space/anchor, blend-in/out weights and confidence. Candidate foot contacts can be proposed from motion and sole position; require context because low velocity alone does not establish ground contact.

Never globally clamp feet to the ground. A forward aerial, leap, fall, kick, flight animation or deliberate foot slide must remain unconstrained unless the recipe explicitly requires contact. Airborne/root motion and gameplay movement must be treated separately to prevent applying displacement twice.

Support planted foot, hand-to-prop, hand-to-hand, weapon grip, ledge/wall and paired-character constraints as separate types. Bake the solved motion into target-supported deformation channels before native export. Rivals II's documented main-character path requires baked motion rather than editor/runtime IK [R13].

Reference implementation candidate: MIT-licensed ozz-animation sampling/blending and two-bone/foot IK examples [R20]. Its samples are building blocks, not a finished automatic retargeter/contact authoring system. Audit transitive tooling dependencies separately, especially FBX-related tooling.

## 7. Retargeting pipeline and native semantics

1. Decode and evaluate the source natively. Verify bind/rest conventions, default tracks, constraints, interpolation, source timing and ancillary channels before transferring motion.
2. Normalize basis/units without losing the native representation. Keep each format's Euler order, scale compensation, inheritance, additive/reference-pose and helper evaluation semantics.
3. Characterize/map source and target chains using reviewed profiles and explicit missing-role policies.
4. Solve rest-axis/orientation and parent-space changes. Transferring numeric local rotations unchanged is not retargeting.
5. Transfer pose intent across chain lengths/proportions. Separate angular similarity, end-effector placement, trajectory, root motion and silhouette priorities.
6. Resolve twist/helpers and scale explicitly. Reject unsupported negative scale/shear/inheritance cases precisely rather than normalizing them silently.
7. Apply time-varying IK contacts and per-move correction layers.
8. Remap gameplay/attachment anchors in the correct coordinate frames.
9. Bake for the target's timebase and supported channels; retain protected event times and winding. Use adaptive sampling/optimization only under measured error bounds.
10. Write genuine target data, decode/reopen it and measure against the intended result.

Animation format conversion on an identical rig and different-rig transfer in the same format are separate tests. A native CHR0 on target rig B is a useful first cross-rig proof, but at least one non-Brawl route must exercise the shared core before calling it multi-game.

The visual editor, export evaluator and validator should share versioned source/target interpretation rules. A viewport-only approximation cannot stand in for validating native output. Use independent fixtures/analytic expected results to avoid having a parser and writer repeat the same error.

## 8. Attacks are more than animation

Create a typed shared move description plus separate native source records and target-specific extensions. Do not pretend the shared representation is a universal lossless compiler for arbitrary gameplay code.

Represent source timing in an exact timebase and preserve native frame/event meanings. Track animation playback speed separately from simulation time, hitstop, state windows and root movement. Retiming a pose clip must not silently shift active frames, effect spawns or cancel windows; the user chooses whether to preserve gameplay timing or retime the whole move.

Hitbox transfer requires source-to-semantic-to-target coordinate frames. Map the semantic striking body part, transform offsets/orientations correctly and resolve target-native bone IDs. Provide explicit size policy: preserve source-world size, scale with a limb/character, or target-authored size. None should silently become a balance decision.

Retain damage, knockback and other source parameters with units/semantics. Use target translators for supported equivalents; mark approximate translations and expose target tuning. Identical numbers across two games are not proof of identical behavior. The same applies to gravity, hitlag/hitstun, collision shapes, angle encodings, cancel rules, landing behavior and immunity states.

Projectiles, props, trails, audio and effects require dependency remapping, target-compatible replacements or new implementations. Unsupported native opcodes must stay visible and block a complete-build claim. A target-specific script escape hatch can preserve expressiveness but is not portable to every destination; never execute imported scripts automatically inside the editor.

Throws and multi-actor attacks require actor roles, attachment/grip spaces, synchronized animation and release events. Treat those as a later explicit tier, not as ordinary one-character clip transfer.

XenoKit is a relevant workflow reference: its documented BAC copy/paste imports referenced animation/audio data, and it previews actions/hitboxes/effects. It also labels itself alpha and says skill/character creation is not yet available [R10]. Do not claim that importing its code completes FighterPort.

## 9. Source registration and direct access

Add a single Source Manager for game installations, extracted dumps, raw character/mod folders, supported platform images/containers and Rivals Modkit projects. Keep base-game, updates, DLC and mod override layers distinct, with visible scope and priority.

Each game profile stores a local path, detected game/build/platform, archive/codec profile, mount identities, user-supplied prerequisites, cache budget and adapter/tool versions. Detect/probe bounded approved locations; manual path overrides remain available. Do not scan every drive by default or execute any file found in a selected game folder.

Source access and asset interpretation are separate layers:

- Folder/image/container provider: lists and streams bytes with stable identities.
- Game asset adapter: decodes meshes, rigs, clips, materials and gameplay data.
- Dependency resolver: locates the correct companions and explains ambiguity/overrides.
- Target adapter: writes/builds data for a particular game/version/mod framework.

Read directly from virtual archive entries when possible and stage only chosen assets. Use 64-bit offsets, bounded streams, cancellation, nested-depth/size/decompression limits and path traversal protections. Do not remove the current protections simply to open a large image.

GameCube/Wii image candidate: encounter/nod supports disc traversal and individual file streams, with a CLI for extraction and formats including ISO/GCM, RVZ/WIA and others [R18]. Dolphin's extractor is not a requirement to make users open a second GUI. Its code can be studied separately under its applicable license; choose one tested provider integration, not a screenshot-driven workflow.

Ultimate uses its own container/resource path, not a generic Wii ISO reader. PC Dragon Ball installations use separate title/build-specific archive providers. A file extension or generic Unreal reader is not a game compatibility guarantee.

Maintain read-only originals and atomic staged outputs. Generated mods are separate from source mounts. Relocation/relinking must verify identities, while stale hashes invalidate cached assumptions.

Cooked Unreal package access is an extraction/import route, not recovery of the original authoring project. Rivals installed-game sources and editable Modkit sources are distinct profiles. Protected/unsupported inputs should report the exact prerequisites without bundling keys or acquiring credentials/game data.

## 10. Automatic tool provisioning

The newest request adds automatic acquisition, not merely optional-tool detection. Update the existing tool requirement accordingly while retaining local-only editing and scoped installation boundaries.

Create a managed Toolchain Manager. After the user enables automatic setup for a supported profile, it should obtain the required vetted tools/adapters automatically, verify them, install into a user-local versioned directory, run health checks and continue the requested operation. Do not prompt for the same already-approved package on every conversion.

Use a trusted, reviewed manifest containing:

- Tool ID, exact version/commit, supported OS/architecture and compatible adapter/game profiles.
- Official/approved immutable download locations; expected size and SHA-256 and signature information where available.
- License/notice/source obligations, whether redistribution or automated retrieval is allowed, and dependency relationships.
- Allowed install recipe, command arguments, version/health probes and runtime requirements.
- Update policy, rollback location, revocation/trust information and offline/manual-install options.

Download to staging, verify against the trusted manifest, reject path-traversing archives and oversized/unexpected content, then atomically activate a version. A checksum fetched from the same untrusted download location is not a trust root. Do not use arbitrary 'latest.exe', scraped download buttons, curl-to-shell, repository-supplied post-install scripts or imported project manifests as installation authority.

Use the smallest necessary prebuilt adapter/CLI where available. Runtime users should not need a C++/Rust/Python development environment simply because those languages were used to build an adapter. If a runtime is needed, obtain it through the same reviewed local provisioning mechanism where permitted. Building from source is a separate developer operation with its own permissions.

Maintain per-project lockfiles. Installing a newer tool must not silently alter an existing project's output; preserve the old version or provide an explicit migration. Expose Installed, Downloading, Verified, Ready, Unavailable, Awaiting authentication/EULA and Failed states with actionable diagnostics. Support cancellation, interrupted download recovery, rollback, offline use and manual path overrides.

Use supervised workers for incompatible/64-bit tools, with structured bounded IPC, local outputs, sanitized arguments and limited environment inheritance. Worker isolation is not automatically an operating-system sandbox. Stronger OS containment can be a separately implemented capability.

Automatically install approved app-local tools; do not automatically change system PATH, install drivers, alter antivirus settings, modify console/system software, accept account terms, enter credentials, pay for services or upload game assets. Account/EULA steps are explicit boundaries, not a reason to abandon unrelated offline development.

The Rivals Modkit has an official Epic Games Launcher distribution and documented Steam-login requirements [R15]. Use supported discovery/launch/install mechanisms and verify the real installation; do not mirror an engine/Modkit archive unofficially. Sparking's referenced mod project requires a custom UE build with account-linked access [R12]. Do not pretend it is an unrestricted redistributable dependency.

License audit matters: the original LibXenoverse README still says 'License (todo)' [R11]. Treat unclear packages as research references or blocked integration candidates pending a component-level permission review, not as automatically redistributable because their code is public. XenoKit/Xv2CoreLib and their dependencies need their own audits.

## 11. Native target templates and build recipes

A target template is a versioned recipe tied to a game build, mod framework, target slot/identity and verified native interfaces. It contains required assets/states, naming/ID rules, format writers, compiler steps, native metadata, packaging layout, validation and a test-launch profile.

Expose separate project modes: animation replacement, existing-character moveset replacement, model/costume replacement, full custom character on a supported base, and new roster slot where the selected target framework demonstrably supports it. Do not label a skin replacement an independent added fighter.

Use the game's actual existing-character template or supported modkit-generated project as the baseline. Do not ship commercial baseline files in the public tool; resolve or derive them locally from configured sources when appropriate. Preserve unrelated target character functions when replacing a single move.

Per-target responsibilities:

- **Brawl/Project M/Project+:** Brawl native models/animations/ancillary resources, action/event data, required companions, PAC/PCS/container structure, IDs and selected mod-framework rules. Do not claim an isolated CHR0 or flattened BRRES is a playable fighter package.
- **Melee:** HSD/DAT models/animations/character data and versioned gameplay/action structures. Treat vanilla replacement and MEX-based expansion as distinct profiles. HSDRaw is a source/read-write candidate; MEX tooling is a workflow reference, not automatically a cleared dependency [R7–R8].
- **Ultimate:** SSBH model/skeleton/material/animation data, textures/helpers, motion/state metadata and the selected gameplay/plugin pipeline. Asset replacement via ARCropolis and gameplay scripting via Smashline are different concerns [R9].
- **Rivals II:** create or attach to the actual Modkit character template, import skeletal mesh/rig/baked animations, assign character states, attack data, hurtbox/physics, materials, skins, sounds and effects, then build/playtest via real supported interfaces [R13–R16].

Rivals' official template creation generates interrelated character assets and references, not merely directories. Inspect the installed Modkit's schema and supported automation endpoints before generating anything. A tested editor plugin or automation bridge is acceptable; an invented Python API or writing arbitrary bytes to a `.uasset` filename is not.

Important version correction: the official installation page retrieved in this review states Unreal Engine 5.4. Earlier conversation text said 5.8. Do not hard-code either assumption: detect the installed Modkit's actual engine/build and use matching interfaces [R15]. The Epic general retargeting documentation's 5.8 page is not evidence of Rivals' engine version.

Local library metadata and neutral move JSON are inputs to these builders, not native playable output. Report parsed/compiled, decoded/native-tool validated, Modkit-imported, built and game-tested as separate stages. Never mark a mod playable solely because directories were generated or an FBX was written.

## 12. Rendering and the single-editor requirement

Keep BrawlCrate's native tree/property grid, original editors and preview behavior intact. Add library, source manager, character moveset, rig controls and target/build panels to the current application.

Introduce a game-neutral preview scene/evaluator that non-Brawl adapters can populate without serializing temporary Brawl files. It should coexist with the native Brawl editor and make approximation/fidelity status visible. Reusing the existing native renderer for Brawl is desirable; it does not mean every other game's material or skeleton must fit MDL0.

A selected resource should show its native format/provenance as well as shared semantic roles. The animation/attack timeline should preview target pose, contact constraints, hitboxes, attachments and events together. Provide source/target synchronized views and target-model wireframe/weights diagnostics.

Native game shading, physics and gameplay remain target-specific. A neutral preview is useful but must not be represented as exact in-game appearance. Preserve original shader/material parameters, alternate UV sets, alpha, texture swaps, morphs and unsupported data for later native round trips.

## 13. Implementation boundaries and suggested code organization

Existing files to extend or integrate with after inspecting current code:

- `application/BrawlCrate/Sources/SourceFolderSession.cs`
- `application/BrawlCrate/Sources/CompanionWorkspace.cs`
- Existing Brawl dump provider/profile and source forms identified by actual repository search.
- Existing ModelEditControl and native BrawlLib resource APIs.

Proposed modules, not claims of existing paths:

```text
Sources/             provider registry, game installations, images, dependency catalog
Authoring/           versioned character project, transactions, source/target branches
Library/             source profiles, AnimationAsset, MoveAsset, revisions, indexing
Rigging/             characterization, fingerprints, chains, virtual controls, IK/FK
Retargeting/         native evaluators, mappings, contacts, corrections, bake/error analysis
Gameplay/            typed move model, semantic hitbox anchors, events, target extensions
Preview/             shared scene representation and synchronized pose/event overlays
Targets/             Brawl, Melee, Ultimate, Rivals2 writers/templates/build/validation
Toolchains/          reviewed manifest, local package installation, health/locks/rollback
Workers/             separately built backends for incompatible runtime/architecture
```

Implement actual adapter capabilities rather than empty classes that imply support. Suggested interfaces can separate `IGameSourceProvider`, `IAssetDecoder`, `IAnimationEvaluator`, `INativeAssetWriter`, `IMoveTranslator`, `ITargetProjectBuilder` and `IToolInstaller`; use the current language/runtime constraints rather than pasting modern APIs blindly into net472.

Use a rendering-independent authoring core with stable schemas and versioned worker messages. Keep native ResourceNode identity and opaque data authoritative for native round trips, but do not make every game implement Brawl-specific ResourceNode internals.

## 14. Updated milestone sequence

Keep exactly one next executable criterion in `docs/plans/current.md`. This specification refines the whole product; it is not a parallel competing task queue.

### A. Finish Brawl character readiness

Preserve the completed multi-root discovery. Finish the current nested raw PAC/ARC/BRRES identity-preserving edit/save/reopen task, then queued PCS/compression, palette/material, blended skinning and genuinely animated ancillary-channel coverage. Maintain the user's full Brawl operation inventory and avoid silently dropping formats to rush into other games.

Use source-independent original fixtures and opt-in configured commercial files. Do not stall on absent Goku assets; do not silently invent them. Broad native UI preservation and exact tested operation coverage are separate status axes.

### B. First useful library in the real application

Persist two original source-character profiles with genuinely different rigs. Import clips bound to their correct source profiles. Add the visible library panel, previews, a draft MoveAsset, assignment to a target slot, and real project save/reopen/undoable edits. Association and provenance must survive source-list reordering and application restart.

This milestone need not claim finished retargeting, but it must ship usable library behavior rather than just schemas. Animation-only and manually authored draft-attack status must be honest.

### C. Characterization plus FK transfer

Implement reviewed fingerprint/template/heuristic characterization and manual correction, native rest/basis/hierarchy interpretation and measured different-rig output. Export a genuine native target animation and independently reopen it. Test bidirectionality independently; do not require the return trip to reproduce the source exactly after declared losses.

### D. IK/FK and contact-aware retargeting

Implement controls, foot/hand/prop contact tracks, pole continuity, pelvis compensation, transition blends, constraint limits and per-clip corrections. Add planted-foot, airborne, changed-proportion and paired-hand tests. Bake and measure native output. Do not promote the solver as production-quality without those cases.

### E. Animation plus attack library transfer

Add one genuinely parsed source attack subset and one target gameplay writer/translator, with semantic hitbox anchoring, source/event time mapping and dependency resolution. Test applying an attack and animation independently and together. Preserve unknown native instructions visibly.

### F. Required cross-game proofs and primary target templates

Implement a genuine Ultimate route early enough to prove Brawl Mario → Ultimate Snake, plus its reverse, without a Brawl-only intermediate. Exercise original analogues even when local Mario/Snake clips are absent. Add Melee's profile through the same core, not a second retargeter.

Implement Rivals II template/import/state/attack/build integration and the Goku vertical slice. Demonstrate one library move on at least two target characters/rigs and test one actual attack, not only an idle animation. Each of the four primary targets requires its own complete-template checklist and build/playtest evidence.

### G. Dragon Ball adapters, direct sources and tool provisioning

Tool-manager foundation and one vetted real backend should be developed alongside the first external adapter, not left as manual-install documentation forever. Test installation with an original harmless package fixture and pin one actually integrated backend.

Expand Xenoverse 1/2, FighterZ and Sparking source models/animations into the same profile/library pipeline. Read native game installations/images on demand through their dedicated providers. Keep Dragon Ball target-native/playable goals and other retained games in the permanent queue, with independently verified output capabilities.

Do not block all library/retargeting work behind every game's image extractor. Do not declare the product complete after the Goku case either.

## 15. Acceptance cases that define success

1. **Source association:** Add one clip bound to source model A and another bound to B; same display names cannot mix their skeletons. Reopen and retain exact bindings. Wrong/missing source rig produces a diagnostic rather than guessed playback.
2. **Revision safety:** Editing one target instance does not modify the library source or another target. Library/mapping changes mark dependent builds stale. Save/reopen retains actual edits.
3. **Skeleton characterization:** Known fingerprints restore reviewed roles; changed parent/rest transforms invalidate approval. Unknown bone names with known topology propose roles and expose ambiguity. Missing fingers/toes remain missing.
4. **Native evaluation:** Original animated rigs cover nonidentity binds, blended weights, scale semantics, axis changes, explicit winding/full turns, fractional time, additive/reference-pose cases and native helper constraints where supported.
5. **Different-rig transfer:** Source/target differ in names, lengths, axes, parent chains and helper/spine counts. A real target-native file reopens with measured pose/orientation and skinning error within declared thresholds.
6. **Contacts:** Planted intervals preserve heel/sole goals within thresholds; the airborne part of an aerial never gains ground locks. Contact release/blending, pole stability and unreachable-goal diagnostics are tested.
7. **Attack binding:** Hitboxes attach to the intended semantic target body part and local frame. Timing/size policies are explicit. Native output uses valid target IDs and keeps unrelated moves unchanged.
8. **Time correctness:** Retiming, hitstop, differing motion rates, fractional events and loop seams do not silently desynchronize hitbox/effect/cancel timing. Protected events survive compression/baking.
9. **Dependency behavior:** Missing projectile/audio/prop data, duplicate names and unsupported native commands remain visible. A full-mod build does not falsely pass while required behavior is unresolved.
10. **Real-user example:** Brawl Mario forward aerial → Ultimate Snake and Ultimate Snake motion → Brawl Mario. Source clips/builds must actually be provided and identified; otherwise original structurally similar fixtures prove the implementation without claiming the commercial case passed.
11. **Rivals workflow:** New/attached Modkit character template, imported mesh/skeleton/baked clips, assigned states, one actual attack, build and playtest. Distinguish generated files, editor import, packaging and in-game success.
12. **Tool provisioning:** Valid package installs locally; checksum mismatch, untrusted recipe, traversal, wrong architecture, offline mode, cancellation and interrupted upgrade behave safely. Reopening a project preserves its locked toolchain.
13. **Source safety:** Native package hierarchy and unrelated entries survive the declared operations. Original input hashes remain unchanged; malformed/compressed/oversized inputs fail precisely.
14. **Coverage:** For each game/build/format/version/direction report discovery, decode, native evaluate, edit, retarget, asset write, gameplay compile, package, native-tool validation and game validation separately.

Use measured angular/position/contact/foot-slide/loop/export errors with units, sampling policy and configured tolerances. Do not invent a universal threshold or label several sample points a proof of continuous-error bounds. Include independent expected results and native-tool checks where available, not only writer→same-parser round trips.

## 16. Codex execution and continuity

Read actual HEAD, AGENTS.md, all authoritative requirements and `docs/plans/current.md`. Persist this specification as `docs/ANIMATION_ATTACK_LIBRARY_MASTER_SPEC.md`, link it from MASTER_PLAN.md/AGENTS.md, and add traceable requirements for the library, semantic rigs, contacts, attack recipes, templates and managed tool provisioning. Preserve existing requirement IDs and full Brawl coverage. Replace the old detection-only tool wording with the new app-local automatic provisioning requirement.

Do not stop after rewriting plans. Continue the actual current native-container task unless newer evidence proves it complete. Then implement the first visible model-bound library increment in this same application, with real state persistence and tests, before expanding into untested all-game stubs.

Keep one next executable acceptance criterion. Update architecture, feature/evidence matrix, development status, testing, dependencies and upstream provenance after each coherent passing increment. Use the repository's real build/test/doctor/run/review workflow and exact current publication policy.

The inspected HEAD records standing permission to commit/push completed reviewed passing source updates to the exact existing public repository. Preserve and reverify that policy rather than replacing it with the older no-push reset text. It does not authorize force-pushes, repository deletion/visibility changes, binaries, artifacts, tags, automatic Workshop publication or CI dispatch. This specification itself does not perform any repository write or publication.

Preserve uncommitted work, source assets and recovery data. Do not bundle commercial inputs/derivatives, credentials, keys, private paths or local screenshots in Git. Retain BrawlCrate/BrawlLib and third-party attribution and applicable notices. A public GitHub repository is not blanket permission to copy every included binary or dependency.

At a real blocker, finish the largest coherent tested increment and record the exact missing tool/interface/input and next acceptance. Do not invent GUI/native/game results; do not repeatedly restart the project or substitute a generic roadmap for implementation.

## 17. Research references and what they support

These are investigation references, not a list of dependencies automatically approved for linking, bundling or installation. Pin and audit actual revisions before implementation. Third-party title/build coverage was not executed in this review.

### Inspected FighterPort snapshot

[R1] Repository commit: https://github.com/DoctorFlux/fighterport-studio/commit/19ae7b2aa14c24d9cd6e6e8ef0bbe6769e9cbda4

[R2] Authoritative current task: https://github.com/DoctorFlux/fighterport-studio/blob/19ae7b2aa14c24d9cd6e6e8ef0bbe6769e9cbda4/docs/plans/current.md

[R3] Master scope and publication policy: https://github.com/DoctorFlux/fighterport-studio/blob/19ae7b2aa14c24d9cd6e6e8ef0bbe6769e9cbda4/docs/MASTER_PLAN.md

[R4] Architecture: https://github.com/DoctorFlux/fighterport-studio/blob/19ae7b2aa14c24d9cd6e6e8ef0bbe6769e9cbda4/docs/ARCHITECTURE.md

[R5] Capability evidence: https://github.com/DoctorFlux/fighterport-studio/blob/19ae7b2aa14c24d9cd6e6e8ef0bbe6769e9cbda4/docs/FORMAT_SUPPORT.md

[R6] Inspected CompanionWorkspace implementation: https://github.com/DoctorFlux/fighterport-studio/blob/19ae7b2aa14c24d9cd6e6e8ef0bbe6769e9cbda4/application/BrawlCrate/Sources/CompanionWorkspace.cs

### Brawl, Melee and Ultimate

BrawlCrate foundation: https://github.com/soopercool101/BrawlCrate

Brawl native action node source inspected through repository search: https://github.com/soopercool101/BrawlCrate/blob/d56cbac89dc2cd0e5c3f923e1d03041b16902add/BrawlLib/SSBB/ResourceNodes/Moveset/Actions/MoveDefActionNode.cs

[R7] HSDRaw/HSDRawViewer, HSD DAT reading/writing and editing: https://github.com/Ploaj/HSDLib

[R8] MEX ecosystem workflow reference: https://github.com/akaneia/mexTool
Also inspect current MexManager code and compatibility: https://github.com/Ploaj/MexManager
The latter repository metadata was accessible but a root README.md fetch returned 404 during this review. No complete MexManager interface or licensing audit is claimed.

[R9] Ultimate assets: https://github.com/ultimate-research/ssbh_lib
Preview/validation reference: https://github.com/ScanMountGoat/ssbh_editor
Asset loading/mod profile: https://github.com/Raytwo/ARCropolis
Gameplay integration: https://github.com/HDR-Development/smashline
Inspected game_acmd registration source: https://github.com/HDR-Development/smashline/blob/94e813ec178551a4fc0dfdd80c5953f719ae1510/crates/smashline/src/builder.rs

### Dragon Ball and Unreal sources

[R10] XenoKit, documented integrated animation/BAC/action/dependency workflow: https://github.com/LazyBone152/XenoKit
Xv2CoreLib and supporting tools: https://github.com/LazyBone152/XV2-Tools
Example source showing animation, BAC/BCM/BDM and effects/audio file types: https://github.com/LazyBone152/XV2-Tools/blob/master/Xv2CoreLib/Xenoverse2/Xenoverse2.cs

[R11] Xenoverse source-format references: https://github.com/DarioSamo/LibXenoverse
XV2-oriented variant: https://github.com/Olganix/LibXenoverse2
The original README's unresolved license heading requires investigation before copying/distributing components.

[R12] Unreal package reader/conversion candidate: https://github.com/FabianFG/CUE4Parse
UE4 extraction reference: https://github.com/gildor2/UEViewer
Gildor's compatibility table lists FighterZ skeletal meshes, textures and animations as supported; this is upstream evidence, not verification of every current build: https://www.gildor.org/projects/umodel/compat?filt=fpv
Sparking target-project reference, requiring a custom UE build: https://github.com/WistfulHopes/SparkingZERO_ModProject
A shared Unreal archive layer does not replace title/build-specific adapters or a target-native cooking pipeline.

### Rivals II official documentation

[R13] Animation authoring/baking and main-mesh dynamic-animation limits: https://rivals2.com/workshop/knowledge-base/character-creation/animation-overview/

[R14] Actual character template creation and generated references: https://rivals2.com/workshop/knowledge-base/character-creation/create-your-character-mod/

[R15] Modkit installation/distribution/login and published engine-version statement: https://rivals2.com/workshop/knowledge-base/getting-started/installing-the-modkit/

[R16] Attack data, bones, grounded/aerial slots, windows and relative hitbox timing: https://rivals2.com/workshop/knowledge-base/character-creation/setting-up-an-attack/

[R17] State checklist: https://rivals2.com/workshop/knowledge-base/miscellaneous/animation-states/

### Disc and rigging foundations

[R18] GameCube/Wii images, virtual file reads and CLI extraction: https://github.com/encounter/nod

[R19] Automatic chain matching/alignment and its template limitations: https://dev.epicgames.com/documentation/en-us/unreal-engine/auto-retargeting-in-unreal-engine

[R20] Renderer-independent animation and IK building blocks: https://github.com/guillaumeblanc/ozz-animation
Two-bone sample: https://github.com/guillaumeblanc/ozz-animation/blob/master/samples/two_bone_ik/sample_two_bone_ik.cc
Foot IK sample: https://github.com/guillaumeblanc/ozz-animation/blob/master/samples/foot_ik/sample_foot_ik.cc
