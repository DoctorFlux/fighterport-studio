# FighterPort Studio — Unified Resource Editor and Full Character Pipeline

Continuation instruction for the EXISTING `DoctorFlux/fighterport-studio` repository.
Prepared 16 October 2026 from the live roadmap, status, desktop source and push workflow.
These are implementation requirements, not claims that features already work.

## 1. Execute this as a continuation

Read `AGENTS.md`, `docs/MASTER_PLAN.md`, `docs/DEVELOPMENT_STATUS.md`,
`docs/plans/current.md`, `docs/ARCHITECTURE.md`, `docs/RETARGETING.md`,
`docs/FORMAT_SUPPORT.md`, `config/capabilities.json`, the existing continuation
specifications and relevant source before editing. Inspect Git status and preserve
unrelated changes. Do not restart, replace the application framework, generate a
second repository, or repeat completed work just because an older prompt asks for it.

The reviewed checkpoint reports 38 tests; real Brawl and Ultimate Mario skeletal
playback; semantic/manual mapping; a restricted scalar rest-relative transfer;
same-rig CHR0/NUANMB conversion; and completed M2.C2 worker-failure tests. Verify the
actual current baseline, which may have advanced. Preserve assertions, not merely
an old test count. M3.A2 quaternion-aware rest transfer and explicit translation,
scale and helper policies is the currently recorded next technical criterion.

The renderer currently uses neutral shading; material/texture/visibility coverage
and complete native character bundles are NOT proved by the existing model imports.
Do not label same-rig file conversion as Mario-to-Mario retargeting.

The user wants a STANDALONE BrawlCrate-like resource editor for Brawl/Project M,
Melee, Ultimate and Rivals 2, capable of opening their data, browsing native
resources, previewing complete animated characters, editing and converting them.
Dragon Ball games are additional SOURCES: Sparking! ZERO, FighterZ, Xenoverse 1/2.
Blender, Maya and 3ds Max remain equal OPTIONAL editing bridges. Normal operation
must not depend on AI or require a DCC just to browse/play supported native assets.

Preserve the remaining Smash-platform, future-game, optimization, costume, game-path
and DCC requirements. Gameplay-script/hitbox/moveset conversion remains outside the
current asset scope. An exported model or animation is not a finished fighter.

The new product milestone is:

> Open source -> browse resources -> display textured character -> play synchronized
> supported animation channels -> edit/retarget -> export a destination bundle ->
> reopen and validate it, in one understandable workspace.

Implement this in small working increments. Do not stop after planning, add dozens
of empty adapters, or claim unlimited execution. At a real boundary leave a tested,
resumable checkpoint. Missing optional software blocks only its dependent tasks.

## 2. Preserve the exact repository and update GitHub at every tested checkpoint

Use ONLY `DoctorFlux/fighterport-studio` and the recorded exact origin/branch.
The current repository configuration explicitly records PUBLIC visibility following
the user's later instruction. This supersedes older private-only prompts. Do not
change visibility, create another remote, or restore the earlier private policy.
Re-read current explicit authority/configuration before acting.

After each coherent, tested implementation increment, and at the session's final
checkpoint when there are intended changes:

1. Build and run applicable tests; perform available UI/native checks and label
   unavailable checks unrun. Run the portable regression baseline before delivery.
2. Update the relevant roadmap/current task, status and operation-level capabilities
   with actual evidence. Do not spend the whole increment rewriting documentation.
3. Publish locally through the existing workflow when the deliverable changed;
   verify the published app for the increment. A publish step is not a GitHub release.
4. Inspect changed and staged file lists AND diffs. Use existing source guards and
   `scripts/review.ps1` after checking their real interfaces. Stage intended files,
   not a blind `git add .`. Check tracked files too; ignore rules alone are insufficient.
5. Commit a meaningful checkpoint when there are reviewed changes. Do not create
   empty/no-op commits merely to say GitHub was updated.
6. Use/improve the existing `scripts/push.ps1`, which checks exact identity,
   configured visibility and branch, then verifies the remote commit after pushing.
   Check native-command exit codes and fail closed on malformed/auth/network responses.
7. Fetch/read back the remote branch SHA and record the actual pushed SHA. A local
   commit is not proof of upload. If the remote advanced, reconcile safely without
   force-pushing or discarding work. On ambiguity, preserve the commit and report it.

Keep commercial input files, native/retargeted commercial outputs, texture caches,
local reports/projects, screenshots of commercial content, private paths/config,
credentials, dependencies and build binaries out of commits. The real Mario assets
remain local. Public source/tests may use original generated fixtures with provenance.
Never upload game data or captures to external analysis/AI services.

No force-push, history rewriting, visibility mutation, automatic paid CI, releases,
or global credential changes. When authentication, permissions, visibility or the
remote is wrong/unverifiable, stop network writes, continue local work and report the
pending commit. Do not claim a push occurred. Do not leave an unattended restart loop.

If useful, add a thin checkpoint script around the EXISTING build/test/review/push
operations. Do not invent a second incompatible Git workflow. Use actual script
parameters; verify their interfaces before calling them. Store a receipt locally
rather than creating an endless commit about the preceding commit's hash.

## 3. Replace format-button clutter with one resource-oriented workspace

Inspect `src/FighterPort.Desktop/Workbench.cs`, `CharacterPreview.cs`,
`MappingWindow.cs` and `AnatomyWindow.cs`. The main window currently creates a large
WrapPanel of format-specific actions. Reuse working backend services and separate
commands/document state from UI construction incrementally; do not rewrite the app.

Default layout:

- Traditional File / Edit / View / Tools / Help menus.
- A small toolbar: New, Open, Save, Convert. Playback controls live on the timeline.
- Left: searchable, lazy expandable Resources tree, with documents/archive roots.
- Center: large model preview; a texture viewer or another appropriate view appears
  for selected resources. Preserve an active model while browsing related assets.
- Right: context-sensitive Properties; advanced settings collapsed initially.
- Bottom: one timeline/clip list with optional tracks and a compact problems/status area.

New Character, Open File, Open Folder, recent sources, Save As and native-tool handoff
are menu/command options, not separate top-level buttons for every extension.
Sample rigs belong under New/Demos, not permanent toolbar buttons.

Interaction requirements:

- Ctrl+O or dropping a supported file opens it using validated format detection.
- Folder input opens through a folder picker or drop, with bounded indexing.
- Expand a container to see its actual nested resources, not only a modal flat
  MDL0/CHR0 selection dialog. Keep the model/animation selection helpers as optional.
- Double-click a model to make it active and frame it without erasing other open sources.
- Double-click a compatible animation to preview it on the active model. Multi-track
  clips activate their supported channel groups together by default.
- Single-click selects/inspects; it must not trigger destructive replacement.
- Select a texture to see the image, channel/mip controls and material usages. Select
  a material to see its bindings and optionally isolate its meshes in the viewport.
- Context menu: Preview, Apply to active model, Edit, Extract original, Export edited,
  Replace in working copy, Show dependencies, Convert to. Expose only applicable
  actions; unsupported output has a reason rather than a success-shaped stub.
- Drag animation onto a different skeleton offers auto/saved mapping and a compact
  confidence/loss review. Do not silently bind the wrong skeleton.
- Clicking a bone in the viewport selects the native tree node and semantic role;
  allow manual assignment, locking and exclusion without editing JSON.
- Undo/redo, keyboard alternatives, dirty indicators, source badges, cancel/progress,
  dock resizing, clear missing-dependency messages and restore-default-layout work.
- Fewer visible controls must NOT mean deleting editing features or hiding errors.

Keep existing hot paths and automated UI coverage. Use original fixtures for public
screenshot tests. Do not spend a milestone drawing an attractive nonfunctional shell.
A simple functional layout is preferred to a new docking-framework migration.

## 4. Model source documents and dependencies, not one fictional universal archive

There are two related views of the same source data:

1. Native resource tree: preserve hierarchy, resource IDs, formats/versions and origin.
2. Character view: a logical grouping of model, skeleton, materials, textures, clips,
   helpers and related resources, which may span several files/archives.

Opening a Brawl PAC should retain the PAC node and all safely enumerable child types.
Main character and shadow models are distinct; do not silently merge their skeletons.
The model PAC and motion PAC may differ. Opening the second adds a source document
and resolves references without discarding the active character.

Ultimate commonly needs a descriptor and several companion files/folders; expose a
logical character bundle and native file view rather than pretend it is a PAC.
Melee DAT requires its actual internal object/reference relationships. Rivals 2 needs
an editable Modkit/source bridge for source assets; cooked extraction is a separate
reader. Dragon Ball installation/archive readers are separately versioned adapters.

Use typed, stable asset handles and a dependency graph covering:
- Source document + native node identity/offset/path as appropriate.
- Native resource type/version and declared decoder capabilities.
- Model/skeleton, meshes, skins, material slots, texture samplers/UV sets, palettes.
- Clips and their bone/mesh/material/morph/texture/constraint targets.
- References outside the selected file, missing/cyclic/ambiguous bindings and shared data.
- Provenance, hashes, ownership, target branches and safe relative project references.

A display name is not a globally unique ID. Duplicate materials/mesh names/animation
names, differing costumes, external banks, subindices and separate shadow models
must not cross-bind. Show ambiguous matches for review. Persist overrides.

Resolve references through declared paths, manifests and native identifiers first;
use bounded heuristics as suggestions. Never scan all disks automatically or demand
that the user select every texture separately when reliable references exist.

List safely recognized but undecoded resources with their type/size/preservation
status. Do not claim unknown payloads are previewable. Malformed input still requires
bounded parsing, cancellation and worker isolation; retain existing hardening.

Do not unconditionally embed every texture and animation bank into base64 JSON.
The current 8/32 MiB limits and large native libraries need a deliberate, versioned,
bounded asset-storage strategy: lazy decoding, content-addressed project-owned blobs
or verified external links, safe paths, quotas and migration. Never solve the limits
by removing all memory bounds. Save/reopen must not rely on temporary caches.

## 5. Evaluate a complete animation bundle on one timeline

Introduce/extend typed clip track groups without flattening everything into bone
rotation curves. A preview sample should evaluate supported channels into an
explicit scene state: pose, skin/morph deformation, visibility, material parameters,
UV transforms, sampled textures/palettes and applicable helpers.

Retain native timing, duration, frame-zero/end conventions, loop modes, interpolation,
step changes, tangents, Euler winding, quaternion sample semantics and source flags.
An FPS value absent from a format must come from an explicit profile, not be invented.
Rivals output-specific resampling must not change the retained source timing.

The supported track groups use one scene time and explicit per-track binding,
rate/offset/range/loop policy. Matching names alone are insufficient evidence that
separate clips belong together. Auto-assemble confidently resolved companions and
show manual selection for ambiguity. No default blending of incompatible clips.

Critical behavior:
- At a selected time, skeletal motion, facial/shape changes, visibility, color,
  UV movement and texture switches evaluate in sync where implemented.
- Discrete visibility/texture selections use verified native discrete semantics,
  not interpolation of indices or boolean values.
- Mute/solo individual groups; inspect/edit native binding and relevant curves.
- Switching clips, going backward, seeking after loop boundaries, and stopping
  restore correct defaults. Do not leak visibility/material state from the last clip.
- Stateless channels are reproducible by random access. State-dependent constraints
  or simulated secondary motion require an explicit deterministic evaluation/bake
  policy; otherwise show unsupported status. Do not silently fake physics.
- Skinning, morph/shape deformation and helper evaluation order follows the native
  adapter's verified semantics. Do not force one universal order on all engines.
- Unknown channels survive save/reopen; inability to evaluate/export is explicit.

Implement the actual categories per platform:

Brawl: CHR0, VIS0, SRT0, PAT0, CLR0, SHP0; TEX0/PLT0 and material/shader bindings.
Include SCN0 and additional discoverable resources in the resource inventory; route
camera/light/environment tracks to appropriate scene previews, not skeletal bones.
Verify VIS0 target/inheritance rules and shape topology semantics in the pinned code.

Ultimate: inspect native NUANMB group/node/track types and sample values, including
transform, visibility and material groups. Preserve material identifiers, subindices,
helper constraints and additional data. Do not invent morph support merely by giving
it a checkbox. Existing ssbh_data_py/native code and the Ultimate tooling are sources
for semantics; FBX is not a replacement for native visibility/material tracks.

Melee: use real HSD DAT/JObj/AObj/FObj/AnimJoint/FigaTree and material/texture animation
structures as verified by the selected library. Enumerate supported subsets; do not
rename Brawl channel types into imaginary Melee file extensions.

Rivals 2: expose the actual Modkit-editable skeletal/morph assets and renderer/material
mechanisms. Determine which non-skeletal animated effects can be represented in the
selected supported character renderer, which need generated visual assets, and which
need manual work. Generic Unreal feature availability is not proof the Modkit's
main-character playback path consumes it.

Dragon Ball: source adapters preserve the full decoded model/animation/material/
texture dependency set and report missing master skeletons or unsupported compression.
Use separate game/version profiles. Do not claim all archives work because one mesh
was recovered. No export back to Dragon Ball games is required.

## 6. Textures/materials need real implementations, not a renamed flat color

First implement a useful tested textured preview before attempting full native shader
emulation. Preserve source material graphs/settings as native data while mapping a
verified subset into the renderer.

Represent texture content and interpretation separately: UV sets, orientation,
samplers/filter/wrap modes, mip levels, palettes, sRGB/data intent, alpha mode,
normal/tangent conventions, vertex colors, material slots, flags and animated targets.
Decode actual TEX0/PLT0 and NUTEXB data through suitable verified code. Resolve maps
through material bindings, not by applying the first texture to every mesh.

Original tests must expose UV inversion, duplicate texture names, alpha clipping,
blending, material colors, texture swaps and scrolling. Do not infer a normal map from
an arbitrary suffix when native bindings specify another use.

Expose distinct preview choices:
- Original/source approximation, with exactly which native effects are evaluated.
- Neutral diagnostic/texture channels.
- Destination-style preview, explicitly an approximation until checked in the game.

Rebuild equivalent destination materials using a versioned style/material adapter;
never claim a Wii/GameCube material program and an Unreal graph are interchangeable.
Unsupported graph effects must yield a loss/fallback report rather than a fabricated
"converted shader". Baking a static look is not preserving an animated material.

Retain user-editable semantic regions (cloth, skin, hair, metal, etc.), masks and
per-target corrections. Implement deterministic map packing/resizing/color transforms
without making AI necessary. Respect skin/costume palette alternatives and avoid
unrequested destructive texture merging or geometry simplification.

Verify Rivals requirements against CURRENT official docs and local Modkit. Its ORMS
layout is outline R, roughness G, metallic B, shading A; account for documented base
color exceptions and import/compression settings. Do not use generic AO/R/M packing.
Do not hard-code an engine version from an old report. Outlines and other effects may
need the character renderer context for validation, not only the skeletal mesh viewer.

If current WPF rendering cannot meet a specific shader requirement, demonstrate the
limitation and replace/extend only the rendering adapter. Keep the application and
headless core. Do not pivot to a Blender-only app or rewrite the entire desktop UI.

## 7. Finish proper retargeting while separating four kinds of mapping

Complete the pending M3.A2 against original changed-axis/changed-parent fixtures before
claiming both-direction Mario transfer. Existing source quaternions must evaluate in
the native convention. Preserve native rest/inverse-bind matrices and Euler winding.
Define root/nonroot translation, animated scale, scale-compensation/override flags,
helper chains, twist distribution and contact policies explicitly. Implement and test
supported cases; reject the rest. Do not make the tests pass by deleting safety checks.

Four independent mapping layers are needed:
1. Bone/chain anatomy and attachment/helper roles.
2. Mesh/object visibility targets.
3. Materials/parameters/textures/palettes/UV sets.
4. Morph/shape/facial channels and required topology/semantic correspondences.

Native identifiers remain unchanged. A semantic role such as left.leg.toes.base is
not a command to rename source/target bones. Names, hierarchy, geometry and weights
provide evidence; display confidence and alternatives, allow manual selection/locks,
and bind reusable approved profiles to fingerprints. Missing toes cannot silently
become ankles. Preserve left/right and finger-segment distinctions.

There is no single mandatory physical skeleton for all games. Use an internal semantic
model plus each destination game/build/fighter's chosen skeleton. A Goku Rivals master
rig is one target project, not a global rig forced onto Mario/Melee/Ultimate exports.
Keep same-skeleton playback independent of semantic recognition.

On new uncertain mappings require review. Reuse already approved profiles when valid;
revoke approval on relevant identity/rest/binding/policy changes. Avoid repetitive
confirmation for unchanged approved mappings. Preview source and target side by side
only when requested; keep the ordinary resource browser simple.

Animation transfer and mesh rebinding are separate features. First validate animation
on an existing target mesh/skeleton, then implement native model rebinding/writing as
a separate acceptance criterion. Preserve target hierarchy/skin and original sources.

## 8. Make conversion a short workflow with an explicit compatibility contract

One Convert command or context action, with three visible stages:

1. Destination: game/build and existing/new character target, skeleton, output folder.
2. Review: auto-resolved resources, approved mapping, visual preview and only actionable
   uncertainties/losses. Advanced timing/codec/material/weight options collapsed.
3. Build: convert, package, decode/reopen, validate and offer the native-tool handoff.

Remember destination profiles and safe defaults. With compatible approved profiles,
repeat batches should not require reassigning every bone/texture. Unknowns and lossy
fallbacks must never be hidden to save clicks.

Keep operations distinct:
- Extract original native bytes.
- Save the non-destructive FighterPort project.
- Write edited data back in a supported source-native format/container copy.
- Convert to destination-native formats or a verified Modkit import package.

No one action may silently mean one of the others. Never change extensions to fake
compatibility. Same-format editing and archive repackaging have their own tests.

For every channel/dependency, classify:
- Native equivalent implemented and tested.
- Loss-bounded bake implemented and measured.
- Explicit approved visual replacement/approximation.
- Preserved in source project only, not in destination runtime.
- Unsupported/blocking.

A complete-output mode blocks unresolved required losses. A separately selected
partial-output mode may create explicitly labeled partial assets after review.
An opaque original stored in the project does not mean the destination plays it.
Do not silently remove VIS0/material tracks to make skeletal export succeed.

Texture swaps might be represented by a native animated binding, an atlas or a renderer
mechanism, depending on the destination. Offer only IMPLEMENTED validated alternatives;
do not present a conceptual atlas as a completed universal fallback. No hard-coded
assumption that FBX transports all material/visibility animation.

Complete native model/material/texture writers and container packaging are required
milestones. They cannot be replaced by animation-only export. Unknown archive entries
must be retained safely when rebuilding a working copy, with atomic publication and
reference/repack validation. Writing bytes alone is not in-game compatibility.

## 9. Prove supported conversions with a feature/direction matrix

Track Brawl, Melee, Ultimate and Rivals 2 as four peers. There are 12 distinct directed
cross-game routes, plus four same-game editing/round-trip paths. A shared core reduces
repeated implementation; it does not prove every direction automatically.

For each route track model/skin, bones/helpers, skeletal, visibility, UV, texture/
palette switch, material/color, shape/facial, static textures/materials and packaging.
Record per-version read/edit/evaluate/map/write/validate evidence and open blockers.
Dragon Ball adds source-to-target rows; no reverse Dragon Ball requirement.

Use a deliberately original multi-channel conformance character containing:
- A blended skinned limb and differently oriented/proportioned target skeleton.
- A hideable accessory, changing expression texture, scrolling UV patch and animated tint.
- Alpha-cutout geometry, multiple material slots and ambiguous-looking duplicate names.
- A morph/shape and helper/constraint test where the native format supports it.
- Discrete changes at boundaries, Hermite extrema, loops and full rotations.

Generate REAL native fixtures via verified libraries. Omit unsupported native constructs
with an explicit test gap, not fake data. Synthetic tests run without commercial files.
Use the user's existing Brawl/Ultimate assets as opt-in LOCAL integration cases; native
Mario resources need not exercise every feature. Do not replace them with demos while
claiming commercial playback was checked.

End-to-end gates per implemented subset:
- Open actual native root/folder -> correct tree/dependencies -> textured character.
- Play and randomly scrub multiple channel groups together; reset without state leakage.
- Edit -> undo/redo -> save/reopen -> identical evaluated state within declared bounds.
- Retarget/bake -> native export -> decode/reopen -> compare pose, visible objects,
  parameters, texture selection/UVs and deformation, not only numeric rotation keys.
- Invalid inputs/cancel/worker crashes never damage source or publish partial output.
- Inspect in BrawlCrate, HSDRaw, SSBH Editor or the actual Rivals renderer when available;
  record shared-library limitations and separate Modkit/cook/in-game checks.

Keep measured pose/vertex/material/texture errors separate. Compare images under a
controlled original test scene where useful, with explicit tolerances; do not demand
pixel equality across intentionally different styles/renderers. Discrete errors such
as a hidden mesh becoming visible must not be averaged away. Check fractional times,
loop seams and brief changes. Fixed sampled checks are not continuous-error proofs.

For optimization preserve timing, protected events/poses and sources. Measure encoded
bytes and post-decoding errors; do not merely remove every other key. Existing larger
CHR0 I12 outputs are a codec/size issue to address explicitly, not by dropping tangents.
Do not claim universal game file-size limits without verified target profiles.

## 10. Ordered delivery queue and first visible result

Use existing milestone IDs and add bounded child criteria rather than creating a
competing roadmap. Avoid an endless generic security/refactor phase and avoid trying
to finish all games in a single untested change.

A. Verify baseline and finish the currently executable M3.A2 subset using original
   tests and explicit policies; record new limits. Do not repeat completed M2.C2.
   Independent UI/texture work need not wait for every future IK/nonhumanoid feature.

B. Deliver the unified Open/resource tree and one real textured, synchronized preview.
   Start with Brawl PAC because it directly exercises the user's desired workflow,
   then Ultimate descriptor/folder through the SAME browser/services.
   Minimum product acceptance: open model source and animation source, select model,
   resolve its materials/textures, play motion and at least one correctly bound
   ancillary group such as visibility, save/reopen, using few ordinary controls.
   Verify the first multi-channel case with original fixtures even if Mario lacks it.

C. Extend channel binding/evaluation/editing to remaining verified Brawl/Ultimate groups,
   complete both-direction reviewed target-rig transfer, and material/texture handling.
   Retain explicit unsupported status until each acceptance test passes.

D. Implement missing model/material/texture native writers and archive/package output
   incrementally, alongside their decode/reopen tests and loss contracts. Do not let
   all native model writing remain indefinitely replaced by animation-only exports.

E. Add real Rivals Modkit/interchange and Melee native resource adapters as peer routes.
   Use exact installed Modkit version and declared Melee structures; material/ancillary
   behavior needs tests, not only an FBX idle or DAT tree. Keep all four games visible
   in the matrix even while subsets are incomplete. Missing local tools produce exact
   unrun integration tasks while original fixture work continues.

F. Expand Dragon Ball source adapters, DCC round trips, texture-style production,
   optimization, installation/image discovery and remaining preserved roadmap items.
   Reuse the browser/evaluator/export contract for every adapter. No DCC lock-in.

The specific next executable task must reflect the current code when this is read.
Keep proceeding through ready acceptance criteria during the active run. Do not return
only this plan. At the boundary, publish locally, update docs and push each reviewed
checkpoint according to section 2.

## 11. Required final receipt

Report what actually changed, operation-level capabilities, test and GUI results,
unrun native-tool/Modkit/in-game checks, remaining losses, the verified local executable
path, and a short click-by-click workflow to test the new resource browser/preview.

Report the actual commit and remote SHA for each pushed checkpoint, or exact local
pending-commit status on a push blocker. Do not claim a public binary release or CI run.

Keep one precise next executable task in `docs/plans/current.md`, with tests and
acceptance conditions. Maintain the full master goal across sessions without asking
the user to repeat it. Start implementing now.

## Primary references to verify during implementation

Existing project and code:
- https://github.com/DoctorFlux/fighterport-studio
- docs/DEVELOPMENT_STATUS.md, docs/plans/current.md, docs/ARCHITECTURE.md
- src/FighterPort.Desktop/Workbench.cs
- scripts/push.ps1 and config/repository.json

Native tools/libraries; inspect exact APIs and licenses before integrating:
- https://github.com/soopercool101/BrawlCrate
- https://soopercool101.github.io/BrawlCrate/
- https://github.com/Ploaj/HSDLib
- https://github.com/DRGN-DRC/Melee-Modding-Wizard
- https://github.com/ultimate-research/ssbh_lib
- https://github.com/ScanMountGoat/ssbh_data_py
- https://github.com/ScanMountGoat/ssbh_editor
- https://github.com/ssbucarlos/smash-ultimate-blender
- https://github.com/ScanMountGoat/ultimate_tex
- https://github.com/4sval/FModel
- https://github.com/FabianFG/CUE4Parse
- https://github.com/gildor2/UEViewer
- https://github.com/Olganix/LibXenoverse2

Rivals official requirements (version-sensitive):
- https://rivals2.com/workshop/
- https://rivals2.com/workshop/knowledge-base/character-creation/rig-your-character/
- https://rivals2.com/workshop/knowledge-base/character-creation/animation-overview/
- https://rivals2.com/workshop/knowledge-base/character-creation/texture-your-character/
- https://rivals2.com/workshop/knowledge-base/character-creation/import-into-unreal/

These references are research/integration inputs, not proof that every requested
operation is implemented. Preserve current architecture and tested library revisions
until a verified change is needed.
