# Next executable acceptance criterion

Implement reviewed per-assignment left/right mirroring with genuine native output inside
the existing Model Previewer workspace (checkpoint D1 of
[FIGHTERPORT_SHARED_ANIMATION_LIBRARY_SLOTS_TRANSITIONS.md](../FIGHTERPORT_SHARED_ANIMATION_LIBRARY_SLOTS_TRANSITIONS.md)).
Slots/anchors checkpoint C now passes for the declared original Brawl subset; see
DEVELOPMENT_STATUS.md and UI_ACCEPTANCE.md. Preserve the completed shared library, reviewed
FK, typed packs/parts/variants, anchors, discovery and container work. Do not reimplement
these checkpoints. This file remains the only next executable task queue.

Extend the authoritative CharacterProject/PreviewAuthoringSession and its existing
Animation Set/Project controls. Add a per-assignment mirror toggle with reviewed exact
rig fingerprints, explicit left/right pairs, central bones, rest/axis transforms, root
policy and unsupported channel/prop diagnostics. Matching names are evidence for review,
not approval. Keep mirroring distinct from facing, reverse playback and texture flips;
negative model scale is not a native mirror solution. Do not silently discard asymmetric
props/channels. Preserve immutable source revisions, independent instances, exact binds,
curves/winding/timing and enclosing native resources. Mirror before future final-target
fitting. Do not implement preview-only mirroring and label it baked output.

Acceptance: two separately saved original projects use the same immutable library clip.
From the real Model Previewer, activate an explicit target/part, review its rig pairs,
preview the original, enable mirror for that assignment, use the native timeline, undo/
redo the change, save/restart and export a new supported native archive. Verify pure
numeric double-mirror recovery independently from freshly decoded mirrored native output.
Compare intended reflected bone poses/orientations and skinned vertices at declared
fractional times, full turns and loop boundaries; declare tolerances and reject unsupported
losses. Exercise a genuinely different reviewed rig route independently, including root
policy and asymmetric-channel rejection. Inspect rendered geometry and distinguish source
preview, target instance and original comparison. Check cancellation, stale/tampered
profiles, pending edits/safe handoff and source-overwrite guards. Original inputs/shared
revisions, the untouched second project and unrelated native records must retain hashes.
Native pose evidence, project metadata and product restart evidence remain separate.

Every new model/animation backend needs usable controls in this same previewer/session:
[FIGHTERPORT_MODEL_PREVIEWER_WORKSPACE.md](../FIGHTERPORT_MODEL_PREVIEWER_WORKSPACE.md).
Keep the inherited viewport, playback and native editors. Explicit bindings identify
project/target/instance/native document/editor; no last-opened/global-root inference.
Retain one editable authoring document with safe handoff/read-only comparisons, pending
Apply/Discard/Cancel, separate native/project/text undo and bounded cancellable work.
Maintain v1/v2/v3 loading, custom/unbound parts, stable IDs, exact asset bindings and
reference revisions/phases. Direct cross-rig assignment still rejects; the existing
fingerprint-bound reviewed measured transfer is the only admitted cross-rig route.

Following scope stays in MASTER_PLAN.md: final-target boundary fitting with editable
margins/protected action/duration/root/loop policies; interval hand/foot/prop IK and
per-clip corrections without aerial ground locks; reusable motion templates and previewed
atomic Fill compatible missing animations with saved regeneration dependencies. No claim
that Idle/Fall alone generates complex motions. Prove a non-Brawl route through the same
core and inspect actual installed Rivals II Modkit interfaces before supported asset-import
claims. Source providers and vetted pinned app-local tools support that work without
blocking all retargeting behind every game reader. No mandatory DCC/cloud/AI or global
installs.

Retain the complete Brawl fidelity queue: observed MDL0 v9 and required versions; bounded
PCS/decompression/recompression; opaque/redirect/header preservation; palettes, alpha/mips,
multiple materials; broader skinning/rest/scale/helpers and genuinely animated
SRT0/PAT0/CLR0/VIS0/SHP0/SCN0 read/evaluate/edit/preserve/write proofs. Empty metadata is
not animated fidelity; a BRRES is not a fighter-package round trip. Keep all Brawl/Project
M/Project+, Melee, Ultimate, Rivals II; Smash 64/Wii U/3DS; FighterZ, Sparking! ZERO,
Xenoverse 1/2 and wider modular sources/targets/channels/optional DCC requirements. Native
Brawl Mario aerial -> Ultimate Snake and reverse, Melee and Goku -> real Rivals Modkit
assets require separate evidence. Gameplay scripting/hitboxes/damage/ACMD/PSA/Lua,
cancel/combo execution and playable compilation remain deferred; preserve legacy drafts.
An attack slot currently means an animation slot.

Run focused and retained regression/build/doctor/review scripts. Push the coherent passing
source checkpoint under standing authorization after exact public origin/main, staged and
outgoing content review; verify remote SHA. Preserve history and recovery; exclude inputs,
derivatives, settings, captures and binaries. No force pushes, releases, tags or public PRs.
