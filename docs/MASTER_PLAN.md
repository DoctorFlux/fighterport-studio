# FighterPort Studio master plan — clean restart

Current phase decision (2026-10-17): [shared animation/library amendment](FIGHTERPORT_SHARED_ANIMATION_LIBRARY_SLOTS_TRANSITIONS.md)
supersedes gameplay priority in the earlier briefs. Focus on models and every declared
animation family, shared disk-backed library/multiple clips per source, reviewed FK/IK,
animation slots/parts, references/templates, mirror then transition fitting, native assets
and supported Modkit asset import. Gameplay scripts, damage/hitboxes, ACMD/PSA/Lua,
cancel/combo execution and playable moveset compilation are deferred, not erased.
Existing manual drafts remain lossless compatibility data, never animation prerequisites.
An attack slot in this phase is an animation assignment, not executable behavior.
Reviewed original FK and shared-library/multi-clip migration checkpoints now pass within
the subsets in FORMAT_SUPPORT.md. Next are typed expandable slots/anchors, mirror/fitting,
template fill and non-Brawl asset routes. Only plans/current.md defines the next executable acceptance criterion.

Authority: [complete clean-restart brief](CODEX_CLEAN_BRAWLCRATE_RESTART.md), preserved verbatim.
It supersedes earlier WPF/bridge/migration and auto-push strategies. Retained product requirements
in CODEX_START_FROM_ZERO.md remain applicable except where the clean brief supersedes architecture,
scope or publication instructions. The newer scope adds gameplay authoring and Dragon Ball
native-output goals. Implemented status comes only from FORMAT_SUPPORT.md and current evidence.
The user's 2026-10-17 source-upload instruction supersedes the reset's local-only restriction
for publishing this replacement to the existing origin/main while preserving history.
The user's subsequent instruction on the same date gives standing authorization to
commit and push each completed, passing source update after review and exact public
origin verification. Binary releases, artifacts, tags and CI dispatch remain unauthorized.
Supplement: [reviewed continuation milestone](FighterPort_Next_Codex_Milestone_2026-10-17.md),
preserved as supplied (reviewed snapshot 20a8dc5). Its R2/readiness/R3/R4 refinements
continue this application. They do not create a second current-task queue.
Product definition: a source-character-bound animation/attack library, character/moveset
editor, semantic rigging/IK/FK retargeting tool and native target-game builder inside this
application. The [animation/attack library specification](FIGHTERPORT_ANIMATION_ATTACK_LIBRARY_MASTER_SPEC.md)
retains its supplied body with an explicit scope amendment; proposed APIs/features are
requirements, not implementation evidence. The user's requested canonical filename takes precedence over the alternative
filename suggested inside the supplied document. Actual HEAD at continuation was 19ae7b2.

| Requirement | Permanent scope |
|---|---|
| REQ-01/02/03 | Independent local non-AI Windows editor; existing repository/history; verified recovery; provenance; local-only reset; persistent evidence |
| REQ-05-SMASH | Smash 64, Melee, Brawl/Project M/Project+, Wii U, 3DS, Ultimate: separate format/version read/edit/preview/native write/package/game checks |
| REQ-05-RIVALS2 | Rivals of Aether II installed-game and editable Modkit/project sources; real import/export/build and gameplay paths |
| REQ-05-DB | FighterZ, Sparking! ZERO, Xenoverse and Xenoverse 2; source adapters AND explicit target-native/playable-mod roadmap |
| REQ-05-EXT | Additional games through modular source/asset/target adapters; no universal Unreal reader or Brawl intermediate |
| REQ-09-SOURCE | Installation/dump/raw-character folders, supported platform images/containers, multiple mounts, deterministic overrides, lazy cancellable indexing, bounded caching, safe 64-bit offsets, guarded extraction and decompression |
| REQ-04-CHARACTER | New/imported characters, mesh parts, rigs, skinning/weights and mesh rebinding, textures/materials, real alternate-outfit meshes, face/morph/costume/hair handling, versioned project master distinct from FBX/glTF-only interchange, project save/reopen, undo, native resource editing, keyframe/curve editing and clip mixing |
| REQ-05-CHANNELS | Skeletal, visibility, UV, texture/palette swaps, material/color, shape/morph/facial, camera/light/scene and declared ancillary channels; synchronized timing and explicit unsupported preservation |
| REQ-07-RETARGET | Skeleton fingerprints; semantic chains with confidence/uncertainty and manual mapping review; axes/rest/bind/hierarchy/proportions; fingers/toes/face/tails/helpers/twist; scale and root/pelvis policies; IK and hand/foot/prop contacts; correction layers; target-rate bake/decode measurements; configurable pose/orientation, contact/foot-slide, loop-seam and decoded-motion thresholds |
| REQ-GAMEPLAY | States, attacks, hitboxes/hurtboxes, movement, throws, projectiles/articles, VFX, sound/events; explicit source-time to gameplay-frame mapping; target-specific gameplay translation/build separate from asset conversion |
| REQ-06-DATA | Native originals/opaque data; exact binds, IDs, Euler winding, tangents/timing; source-quality branch and independent targets; loss reports, safe atomic outputs and dependency resolution |
| REQ-08 | Encoded-size/error-aware animation optimization with protected events, fractional/loop tests; non-destructive material/style conversion, regions, UVs/palettes/color-space/alpha/mips and measured destination preview |
| REQ-05-INTERCHANGE | Verified bidirectional FBX, glTF/GLB subsets, COLLADA/DAE, SMD/ANIM; optional equal Blender/Maya/Max bridges and versioned conflict-aware manifests; no DCC needed for core editing |
| REQ-11 | Original fixtures; per-operation capabilities; decoded output and source hashes; independent numeric expectations; GUI, native-tool, Modkit and in-game checks reported separately |
| REQ-10-TOOLS | Managed app-local provisioning after supported automatic setup is enabled: vetted manifests, pinned immutable official downloads/size/SHA256/trust, dependency setup, architecture/profile compatibility, health checks, project locks, offline cache, cancellation, rollback/revocation and manual paths. No arbitrary remote scripts, global installs, silent EULAs, credentials, paid dependency or bundled game data. Detection alone is insufficient. |
| REQ-LIB-SOURCE | SourceCharacterProfile: exact source game/build/mount/container/model/skeleton identity, native versions/data, binds/inverse binds, hierarchy, axes/units/scale semantics, fingerprint and independently reviewed semantic characterization. Batch clips remain bound to that rig; matching names are not compatibility. |
| REQ-LIB-ASSET | AnimationAsset: source-bound native motion, timing/curves/tangents/winding, ancillary channels, dependencies/evidence and revisioned layers. MoveAsset: actual native attack evidence or explicitly authored draft gameplay, typed timing/anchors/hitboxes/movement/landing/effects/audio/props and unresolved commands. Animation-only import never fabricates attack behavior. |
| REQ-LIB-INSTANCE | MoveInstance: non-destructive target assignment, reviewed retarget/correction layers, target slot/time/gameplay overrides, independent versioned outputs and dependency/tool/mapping fingerprints; source/library revisions remain immutable and stale instances/builds are visible. |
| REQ-LIB-PROJECT | Versioned persistent character projects distinct from .fpcompanion: actual edits, native originals, rigs/binds/weights, curves/timing, materials/outfits, mappings/approvals, corrections, target branches, transactional commands and an explicit undo-history persistence policy. |
| REQ-RIG-SEMANTIC | Automatic deterministic recognition: reviewed exact profiles, versioned game templates, naming/hierarchy/rest/geometry/weight/motion evidence, confidence/reasons/conflicts and correction UI. Unknown/changed rigs are never silently approved; missing fingers/toes remain missing; extensible nonhumanoid chains. |
| REQ-RIG-IKFK | Editor-side chains/FK/two-bone IK/poles, pelvis/root policy, twist/helpers, orientations, per-clip contact intervals/corrections/blends and explicit solver limits; aerial attacks have no default planted feet. Bake supported native deformation bones, never export control rigs. |
| REQ-ATTACK-NATIVE | Separate native gameplay adapters/compilers: semantic hitbox anchors and local frames, exact source/gameplay time mappings, explicit size/physics policy, movement/landing, dependencies/effects/audio/props and unresolved-command reporting. Animation transfer, mesh rebinding and gameplay conversion remain distinct. |
| REQ-TEMPLATE | Real versioned Brawl/PM/P+, Melee, Ultimate and Rivals II target assets/references/framework/slot setup, native writers and validated build paths. Inspect installed Modkit/templates/interfaces/engine build; no guessed APIs or folder-only playable claims. |
| REQ-E2E-LIB | Defining route: Brawl Mario forward aerial -> library -> Ultimate Snake native rig/moveset slot -> genuine native asset + gameplay output -> decoded validation -> game test when configured. Reverse independently. Original genuinely different rigs, a Melee route and Goku Rivals II mesh/animations/template plus one real attack are required independent proofs. |

No perfect automatic arbitrary-rig retargeting, guessed approved commercial maps, fabricated
native writers or playable-fighter claims. No bundled keys/access bypass; encrypted or unsupported
inputs report prerequisites. Cooked extraction is not recovered authoring data. No global installs,
automatic EULA acceptance or paid services. Binary distribution requires component/license audit.

Ordered executable milestones (only one current acceptance criterion lives in plans/current.md):

Primary full authoring targets are Brawl/Project M/Project+, Melee, Ultimate and Rivals II;
FighterZ, Sparking! ZERO and Xenoverse 1/2 are required model/animation sources. Every
wider game/native-output/channel/interchange/optional DCC row above remains permanent.
The first useful library/project increment now implements two original source-character
profiles, correctly associated clips, native previews, honest manual-draft status,
independent slot assignments and actual native edit/project save/reopen/undo. Its
original v1 subset remains loadable. Version 2 now uses shared immutable disk-backed
source/clip records across separate projects and supports several CHR0 v4 clips per source.
Explicit reviewed original rig A -> B and independent reverse FK routes pass sampled
fresh-native pose/skinning checks. IK, mirror, fitting and other native channels remain
queued. Next are typed expandable animation slots/anchors, then mirror/fitting and template
fill; native gameplay/attack translation remains deferred under the current scope.
Develop managed provisioning alongside the first external adapter: original harmless
package installation/rejection/rollback tests plus one genuinely integrated pinned tool.

1. R0: verified external recovery; pinned application import; original build with documented minimal
   repairs; source/license inventory. Replace old active implementation, retain Git history.
2. R1: independent product/settings identity; inherited update/reporting/Discord and association
   safety; original native GUI open/preview/animate/edit/save-new/reopen and source hashes.
3. R2: first functional File > Open Source loose-folder path through existing native editors.
   The initial folder slice and explicit persistent MDL0/TEX0/CHR0 companion slice now pass their
   original-fixture gates. Persistent multi-root loose discovery now passes its bounded
   original-fixture/native GUI gates: scoped ordered file overrides, cancellation, quotas,
   selected-file staging and pinned identities across restart. Installation providers,
   separate platform image/container readers and corruption checks remain queued.
   Raw nested ARC/PAC -> BRRES selection, enclosing PAC assembly/edit/save/reopen, exact
   unrelated payload/entry identity checks and original blended skinning now pass.
   Current configured Brawl header versions were inventoried opt-in; no retail decode/game claim.
   Remaining Brawl readiness: required native versions (observed MDL0 v9); nested
   ARC/PAC/PCS/BRRES lineage and container-preserving output; palettes/alpha/mips, multiple
   materials, nontrivial blended skinning and genuinely animated SRT0/PAT0/CLR0/VIS0/SHP0/SCN0
   fixtures. Read/preview/edit/preserve/write/package evidence stays separate. No flattening
   a fighter package into BRRES and calling it a package round trip.
4. R3: the first visible source-character-bound library and real edit-project increment
   passes its original-fixture acceptance (see status/testing). Shared disk-backed clips,
   explicit fingerprint-bound role review and a measured original FK subset now pass.
   Next: typed expandable animation slots/anchors, then reviewed mirror, fitting/IK
   and dependency-aware template fill. Preserve native originals,
   immutable/revisioned source profiles/animations/moves, non-destructive target instances, semantic review/mapping,
   exact rest/axis/hierarchy and scale/root policies; measured cross-rig correction/baking on original
   changed-axis/proportion/helper/contact fixtures with declared pose/orientation, contact/foot-slide,
   loop-seam and post-export decoded-motion tolerances. Channel remapping remains separately declared.
   Version 1 .fpcharacter persists actual CHR0 edits, full native originals, resolution
   provenance, skeleton/bind fingerprints, manual draft data and independent instances.
   Undo/redo is transactional and session-only; saved state persists, history clears on load.
   Broader projects must persist native originals, binds/weights,
   curves/tangents/winding/fractional and event timing, materials/outfits, mappings,
   per-clip correction/contact layers and independent targets. They are not .fpcompanion.
   Reviewed fingerprint/chain profiles and batches must handle or reject changed axes,
   parents, proportions, bone counts, helpers/twist and scale/compensation explicitly.
   Prove genuine rig A -> different rig B -> native output -> decoded target poses/skinning,
   contacts and loop seams against configured tolerances; test both directions independently.
   Then prove a non-Brawl native/interchange adapter through that same core. Matching names
   alone are not rig equivalence. Do not defer this work behind every game's source provider.
5. R4: inspect actual installed Rivals Modkit version/schemas/interfaces; assemble configured Goku
   sources; transfer selected clips to the chosen target rig; correct contacts; bake/export; import
   using the real supported Modkit asset path. Runtime state/attack/build/playtest remains
   a separate deferred gameplay milestone, not a prerequisite for animation authoring.
   Verify actual installed tools/templates/import/build interfaces, imported skeletal mesh,
   skeleton and baked animation assets. Keep movement states, one attack and target gameplay
   build/playtest distinct from the animation export; no playable claim from interchange alone.
6. R5: broaden Melee/Ultimate peer routes, Wii U/3DS/64 and Dragon Ball sources/targets, all channels,
   complete character asset/package writers, native gameplay compilers, optimization and DCC round
   trips. A missing proprietary tool blocks its validation, not independent original-fixture work.

Every game remains a target roadmap row even before its implementation. Track each directed route,
not a single generic conversion badge. An optional Goku authoring rig is never mandatory globally.
