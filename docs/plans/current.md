# Next executable acceptance criterion

Implement reviewed semantic characterization and a measured FK transfer on the existing
source-bound library/project. Extend the actual app; do not restart or replace its native
editors. The first library/.fpcharacter acceptance is complete for its declared v1 subset:
two different original native rigs, stable source/clip associations, independent instances,
manual drafts, actual CHR0 edits, session undo/redo, project restart and native output/reopen.
See DEVELOPMENT_STATUS.md for current evidence and limits.

Add explicit reviewed chain/role mappings tied to both exact rig fingerprints and a
versioned interpretation. Keep unknown roles unapproved. Review must account for local
axes/rest orientation, hierarchy/parent differences, proportions, extra/missing helpers,
root/pelvis policy and scale/compensation support or explicit rejection. Matching bone
names alone cannot approve a mapping. Persist mappings/review decisions in the character
project with transactional undo/redo and visible invalidation when either source changes.

Acceptance: original rig A -> genuinely different original rig B -> separate genuine native
CHR0 on B -> fresh decode and native preview/reopen on B. Persist the source-bound clip,
reviewed map and independent target instance without changing source/library revisions or
another instance. Compare decoded target bone positions/orientations and weighted mesh
poses to independently intended target transforms at declared sampled/fractional times,
including a loop seam and full-turn/winding case. Record numeric tolerances and losses.
Unsupported scale/helpers/unmapped required chains must reject explicitly. Test reverse
transfer independently before claiming it. No automatic gameplay or mesh-rebinding claim.

Then add interval-based IK/hand/foot/prop contacts and editable per-clip corrections,
reviewed reusable profiles/batches and validated baking. Aerial motions receive no default
ground locks. Prove a non-Brawl route through the same core with its own adapter, without
encoding other games through MDL0/CHR0. Keep broader persistent edits/materials/outfits,
multi-clip source revisions, target overrides and all attack/template requirements queued.

Retain the complete Brawl fidelity queue: observed MDL0 v9 and other required versions;
bounded PCS/decompression/recompression; opaque/redirect/header preservation; palettes,
alpha/mips/multiple materials; wider skinning/rest/scale/helpers; genuinely animated
SRT0/PAT0/CLR0/VIS0/SHP0/SCN0 with separate read/evaluate/edit/preserve/write proofs.
Do not call the empty SRT0 fixture animated-channel fidelity or a BRRES a fighter package.
Multi-root discovery and canonical nested PAC readiness remain completed, not replacement tasks.

Following milestones remain native parsed attacks/anchors/timing and target builders;
Brawl Mario aerial -> Ultimate Snake native rig/slot/assets/gameplay and reverse separately;
a Melee route; Goku -> actual installed Rivals II Modkit template/mesh/animations/attack/
build/playtest when configured. Preserve every wider game/provider/channel/native-output
and optional DCC route. Develop vetted pinned app-local tool provisioning alongside the
first external adapter. No mandatory DCC/cloud/AI or invented Modkit interfaces.

Full requirements: MASTER_PLAN.md and FIGHTERPORT_ANIMATION_ATTACK_LIBRARY_MASTER_SPEC.md.
Build/test/review and push passing source checkpoints under standing authorization; verify
exact public origin/main, preserve history and exclude inputs/derivatives/settings/binaries.
