# Next executable acceptance criterion

Implement typed, expandable animation-set slots and character reference anchors in the
existing shared-library/.fpcharacter workflow (checkpoint C of
[FIGHTERPORT_SHARED_ANIMATION_LIBRARY_SLOTS_TRANSITIONS.md](../FIGHTERPORT_SHARED_ANIMATION_LIBRARY_SLOTS_TRANSITIONS.md)).
The reviewed FK and shared-storage checkpoints A/B now pass for their declared original
Brawl subset; see DEVELOPMENT_STATUS.md. Do not reimplement discovery, containers or these
checkpoints. Preserve direct cross-rig assignment rejection; cross-rig use must go through
an exact fingerprint-bound reviewed map and measured native baking.

Add versioned template-pack identities, semantic slot groups, expandable parts and variants
with independent assignments. Cover arbitrary jab counts; tilts/smashes/aerials; specials
with ground/air and start/hold/loop/release/follow-up/end; taunts, movement and miscellaneous
categories. Keep part continuation boundaries distinct from Idle/Fall exits. Every native
binding must carry exact selected target resource/identifier evidence; unverified/extra
slots remain visibly unbound and retain their clips. A fixture pack is original evidence,
not certification of Brawl/Melee/Ultimate/Rivals game bindings. No scripting is required.

Let each target select and persist Idle and Fall once, plus pack-specific anchors, using
explicit clip revisions/poses/phases. Keep existing custom/free-text assignments and v1/v2
loading lossless. Update the actual Character Library controls and preserve native edits,
undo/redo, shared source immutability, lazy indexing, budgets and source guards.

Acceptance: two separate saved original character projects share one immutable clip;
expand a group past a fixed jab/special count, assign distinct parts/ground-air variants,
select different target references, preserve an existing/custom assignment and an unbound
extra part, undo/redo the batch, save, restart the actual product and reopen each native
output. Verify slot/anchor IDs and dependencies, actual native timing/keys/poses, unchanged
other project/shared revisions and original hashes. Cancelled/invalid packs, missing or
stale references, duplicate bindings and unsupported channels must produce diagnostics
without partial project changes. Never label a free-text name a verified runtime binding.

This phase is models/animations and supported Modkit asset import. Gameplay scripting,
damage/hitboxes, ACMD/PSA/Lua, cancel/combo execution and playable compilation are deferred;
legacy manual drafts remain compatible and hidden behind the explicit legacy action.
An attack slot means an animation slot.

Following checkpoints: reviewed left/right pairs and native mirror before final-target
boundary fitting; editable margins/protected action/duration/root/loop policies; interval
hand/foot/prop IK and per-clip corrections (no default aerial ground locks); reusable motion
templates and previewable atomic Fill compatible missing animations with saved regeneration
dependencies. Test double-mirror recovery, native mirrored output and reopened fitted poses
separately. Prove a non-Brawl adapter through the same core and inspect the actual installed
Rivals II Modkit before claiming supported asset import. No guessed APIs or engine version.

Retain the complete Brawl fidelity queue: observed MDL0 v9 and required versions; bounded
PCS/decompression/recompression; opaque/redirect/header preservation; palettes, alpha/mips,
multiple materials; broader skinning/rest/scale/helpers and genuinely animated
SRT0/PAT0/CLR0/VIS0/SHP0/SCN0 read/evaluate/edit/preserve/write proofs. Empty SRT0 metadata
is not animated fidelity; a BRRES is not a fighter-package round trip.

Keep Brawl/Project M/Project+, Melee, Ultimate, Rivals II; Smash 64/Wii U/3DS; FighterZ,
Sparking! ZERO, Xenoverse 1/2 and wider modular providers/targets/channels/optional DCC routes.
Brawl Mario aerial -> Ultimate Snake native animation assets and the reverse require separate
real-input evidence; Melee and Goku -> actual Rivals Modkit asset routes remain queued.
Gameplay/game acceptance remains a later independent milestone. No mandatory DCC/cloud/AI.
Develop vetted pinned app-local tools with the first external adapter; no global installs.

Build/test/review and push passing source checkpoints under standing authorization. Verify
exact public origin/main and remote SHA; preserve history and exclude inputs, derivatives,
settings, evidence and binaries. MASTER_PLAN.md and the amended library specifications retain
the full requirements; this is the sole executable task queue.
