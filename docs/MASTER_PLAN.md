# FighterPort Studio master plan — clean restart

Authority: [complete clean-restart brief](CODEX_CLEAN_BRAWLCRATE_RESTART.md), preserved verbatim.
It supersedes earlier WPF/bridge/migration and auto-push strategies. Original requirements remain
in CODEX_START_FROM_ZERO.md as history; the newer scope adds gameplay authoring and Dragon Ball
native-output goals. Implemented status comes only from FORMAT_SUPPORT.md and current evidence.

| Requirement | Permanent scope |
|---|---|
| REQ-01/02/03 | Independent local non-AI Windows editor; existing repository/history; verified recovery; provenance; local-only reset; persistent evidence |
| REQ-05-SMASH | Smash 64, Melee, Brawl/Project M/Project+, Wii U, 3DS, Ultimate: separate format/version read/edit/preview/native write/package/game checks |
| REQ-05-RIVALS2 | Rivals of Aether II installed-game and editable Modkit/project sources; real import/export/build and gameplay paths |
| REQ-05-DB | FighterZ, Sparking! ZERO, Xenoverse and Xenoverse 2; source adapters AND explicit target-native/playable-mod roadmap |
| REQ-05-EXT | Additional games through modular source/asset/target adapters; no universal Unreal reader or Brawl intermediate |
| REQ-09-SOURCE | Installation/dump/raw-character folders, supported platform images/containers, multiple mounts, deterministic overrides, lazy cancellable indexing, bounded caching, safe 64-bit offsets, guarded extraction and decompression |
| REQ-04-CHARACTER | New/imported characters, mesh parts, rigs, skinning/weights, textures/materials, real alternate-outfit meshes, project save/reopen, undo, native resource editing |
| REQ-05-CHANNELS | Skeletal, visibility, UV, texture/palette swaps, material/color, shape/morph/facial, camera/light/scene and declared ancillary channels; synchronized timing and explicit unsupported preservation |
| REQ-07-RETARGET | Skeleton fingerprints; reviewed semantic chains; axes/rest/bind/hierarchy/proportions; fingers/toes/face/tails/helpers/twist; scale and root/pelvis policies; IK and hand/foot/prop contacts; correction layers; target-rate bake/decode measurements |
| REQ-GAMEPLAY | States, attacks, hitboxes/hurtboxes, movement, throws, projectiles/articles, VFX, sound/events; target-specific gameplay translation/build separate from asset conversion |
| REQ-06-DATA | Native originals/opaque data; exact binds, IDs, Euler winding, tangents/timing; source-quality branch and independent targets; loss reports, safe atomic outputs and dependency resolution |
| REQ-08 | Encoded-size/error-aware animation optimization with protected events, fractional/loop tests; non-destructive material/style conversion, regions, UVs/palettes/color-space/alpha/mips and measured destination preview |
| REQ-05-INTERCHANGE | Verified bidirectional FBX, glTF/GLB subsets, COLLADA/DAE, SMD/ANIM; optional equal Blender/Maya/Max bridges and versioned conflict-aware manifests; no DCC needed for core editing |
| REQ-11 | Original fixtures; per-operation capabilities; decoded output and source hashes; independent numeric expectations; GUI, native-tool, Modkit and in-game checks reported separately |

No perfect automatic arbitrary-rig retargeting, guessed approved commercial maps, fabricated
native writers or playable-fighter claims. No bundled keys/access bypass; encrypted or unsupported
inputs report prerequisites. Cooked extraction is not recovered authoring data. No global installs,
automatic EULA acceptance or paid services. Binary distribution requires component/license audit.

Ordered executable milestones (only one current acceptance criterion lives in plans/current.md):

1. R0: verified external recovery; pinned application import; original build with documented minimal
   repairs; source/license inventory. Replace old active implementation, retain Git history.
2. R1: independent product/settings identity; inherited update/reporting/Discord and association
   safety; original native GUI open/preview/animate/edit/save-new/reopen and source hashes.
3. R2: first functional File > Open Source loose-folder path through existing native editors.
   Then companion graph, persistent multiple roots/overrides, cancellation/cache/resource budgets,
   installation providers, separate platform image/container readers and corruption checks.
4. R3: game-neutral character projects plus native original retention, semantic review/mapping,
   exact rest/axis/hierarchy and scale/root policies; measured cross-rig correction/baking on original
   changed-axis/proportion/helper/contact fixtures. Channel remapping remains separately declared.
5. R4: inspect actual installed Rivals Modkit version/schemas/interfaces; assemble configured Goku
   sources; transfer selected clips to the chosen target rig; correct contacts; bake/export; import
   using the real supported Modkit path; minimal state/attack; build/playtest when available.
6. R5: broaden Melee/Ultimate peer routes, Wii U/3DS/64 and Dragon Ball sources/targets, all channels,
   complete character asset/package writers, native gameplay compilers, optimization and DCC round
   trips. A missing proprietary tool blocks its validation, not independent original-fixture work.

Every game remains a target roadmap row even before its implementation. Track each directed route,
not a single generic conversion badge. An optional Goku authoring rig is never mandatory globally.
