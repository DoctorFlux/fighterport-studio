# Model Previewer authoring acceptance

Reach/pole/endpoint controls added after 77d64ce (2026-10-17): select a saved contact in
the existing Rig / FK panel to edit its interval, goal and policies. **New** starts an
independent interval and rejects an existing chain/start instead of overwriting it.
Use **Already planted at clip start** and **Still planted at clip end**
only for contact that exists at those boundaries. The explicit full-clip standing action
sets 0..last, holds both ends and enables 0.05 reach blend if the saved value was zero.
It never adds contacts automatically to an aerial. Interior edges retain their margins.

Advanced pole/reach choices declare World versus TransportedRest and Nearest versus
RestHemisphere. Changing a saved pole override's space requires explicit clearing first.
The saved reach-blend control is a 0..0.1 fraction of leg length; Save is undoable and
does not stretch limbs or resize meshes. Keep source/FK/corrected comparison and export
in the same explicit session/target/native-document binding. Save a new native output,
reopen it and inspect the first, last and fractional frames. Matching endpoint metadata
alone is insufficient. Whole sole/toe clearance and complex loop/constraint transitions
remain separate from the bounded planted-landmark result. Final observed receipts are
recorded in DEVELOPMENT_STATUS.md; this section specifies the real control surface.

H3 standing workflow (2026-10-17): Brawl multi-model archive Preview and Open Character
catalogue indexed MDL0 choices; choose the actual character. Only that model is attached to
native rendering/playback. Shadows/articles remain in the native hierarchy for preservation.
Adding a separate motion with Open models off does not load its article models.

In the existing Rig / FK humanoid controls, choose the explicit source clip, target and
native destination. Preview the intended target. Set `StanceSourcePhase`, then **Align source
phase to native target at playhead**. The saved status shows both phases and world pelvis
offset; it evaluates the immutable native target reference, not unsaved target edits. Keep
binds, Idle/Fall and this clip-specific calibration distinct. Clear calibration is deliberate
and undoable. Existing contact goals/intervals and Fit/FK/corrected/export controls are reused.
The independently tested Mario -> Snake standing diagnostic uses source 10.25, target 0;
this is an explicit local diagnostic recipe, not an automatically reviewed mapping/template.
Use a new output destination and reopen its animation on the selected native target model.
Unreachable goals still report losses/reject assignment; broad pole/whole-body solving remains next.


Ordinary animation path (2026-10-17, after bee76e5): File > Animation workspace opens
the same native viewer, or use a model's native Preview. The visible, resizable Animations
browser stays above the viewport even when native side panels/workspace are hidden.
Open Character > browse/inspect/select/Open stages a model; Add Source attaches a separate
motion file (Open models defaults off once a model exists). For a motion PAC with opaque
siblings, choose the intended raw BRRES path explicitly. No .fpcompanion/project is needed
to audition opened motion. Model-first and motion-first are supported, with no inferred
rig compatibility or target project association.

Choose Opened sources / Selected character / Project / Shared library / Assignments,
search names, select a row and Preview. Types/native paths disambiguate same-name channels.
Read-only retains this navigation, native timeline/Play and camera; mutation stays gated.
Previewing source A must retain a different selected target B. Shared selection does not
assign a slot; Apply Animation uses the existing registered/reviewed project route. Native
unregistered auditions cannot silently become assignments. Advanced Setup/Library selectors
remain views over the same session. Save/Export opens project output, or exports a separate
matching native skeletal-motion document with a fresh key/tangent/timing/flag decode check.
Opaque/ancillary fidelity and merged fighter packages are not certified by this export.

Animations hides/reopens the browser; Frame model reframes current geometry after resize.
Compact and restored old layouts keep playback visible. Actual 850x620 controls and native
framebuffer motion are tested; real 96-DPI product launches restore the ordinary path.
No automatic character catalogue/model-only project registration, miscellaneous recipe
fill or Godot integration is claimed. Those require working backends; no placeholder fill
button was added. H3 Mario-to-Snake contact correction remains immediately next.

The standing contract is FIGHTERPORT_MODEL_PREVIEWER_WORKSPACE.md. This guide records
the current surface and repeatable acceptance, not a second executable task queue.

The 2026-10-17 proportion-aware humanoid amendment supersedes mirror priority. The
Rig/FK tab now also hosts characterization/body profiles and interval retarget controls;
the older rigid review route remains. Choose explicit source clip, target and native
destination, Auto-detect, inspect the exact source/target proposals, then deliberately
review chain/landmark evidence. Profiles are reusable and immutable after review. Draft
Advanced controls edit ordered bone indices, sole/wrist landmarks/poles, ground and
pelvis limit; unresolved rigs need explicit correction, not a guessed approval.

Add explicit limb/contact intervals and World/Head/Spine goals, or read a target point
at the native playhead. Feet may explicitly project to the measured ground; aerials have
no default locks. Fit creates separate read-only FK/corrected native comparisons with
serialization/contact/review diagnostics. Unreviewed diagnostics cannot become approved
assignments. Edit contact/pole/pelvis virtual handles in the real viewport; release records
one undo transaction, then regenerate Fit to bake. Handles do not edit/export control rigs.
Apply creates an independent target instance; native edits use existing leases and Apply.
New native export and project save remain separate. Editing invalidates old bake-quality
evidence; it never changes immutable shared clips or the already baked other instance.

Source paths / companions now includes explicit raw BRRES selection and palette binding.
Library can add a selected container motion to an already registered source, preserving
full original provenance and the live project. Model bases retain their complete supported
archive; unknown/empty companion siblings are not decoded or claimed as round-tripped.

H1/H2 actual GUI acceptance includes native textured source/FK/corrected geometry and
Play, profile/landmark surfaces, explicit goal save, viewport pelvis drag, one-step undo/
redo, independent Apply/native edit/project reload, small-window and hide/reopen checks.
Commercial trial profiles remain visibly unreviewed. The reverse local standing-contact
trial is rejected; full anatomical clearance/temporal/twist/scale/prop solving and reviewed
revision-fork UI remain next work. No Modkit, external native tool or game check occurred.

Open a native model normally and choose **FighterPort workspace**, or open a character
project through the library and its native-instance preview. Setup attaches a project
explicitly, creates one from a saved companion resolution, or migrates an old embedded
project to shared storage. Select the target and inspect an explicit instance. Plain
models stay unassociated until this action. Project identity, target/fingerprint,
instance/revision, part and source/target/reference mode remain explicit.

The resizable pane contains these tabs:

- Setup: project/resolution paths, target/instance selection, current adapter status and
  access to the actual companion/source configuration. No unsupported writer is invented.
- Library: bounded shared metadata search/pages, use an immutable revision, add supported
  clips and preview sources read-only. Selecting metadata alone does not assign a part.
- Animation Set: install the Brawl semantic pack, select group/phase/variant and add a
  bounded batch. Rename/reorder parts without changing identities. Bind an actual native
  asset destination or retain an assigned unbound part. Clear explicitly; existing/custom
  assignments are never silently replaced. Assign uses same-source or reviewed FK routes.
- References: choose Idle/Fall motions independently from native-playhead phases; choose
  the pair as one undoable command. Named pack/custom anchors use the same service.
  Preview each on the target, then set its phase. Missing/stale references need deliberate
  re-selection; there is no automatic fitting or default aerial foot plant.
- Rig / FK: the reusable exact-fingerprint review grid and helper/root policies. No name-
  based approvals. Assign a cross-rig clip to a bound target destination after review;
  numeric native validation must pass before the instance is committed.
- Project / Output: Apply edits, new native output path/export, separate native drafts for
  unsupported changes, project save/new path, structural Undo/Redo and cancellable source/
  reference diagnostics. New/migrated projects require a new file.

The native viewport, camera, frame controls, playback and detailed editors are retained.
Authoring layout keeps playback visible in a small window; Native details restores the
wide inherited key/curve controls. Hide/reopen the workspace without discarding its state.
Layout preferences belong to app data; slots, mappings and anchors belong to the project.

One authoring view can edit at a time. Source/reference and other project comparisons
keep camera/playback while edit controls/hotkeys/mouse transforms are gated. Activation
resolves pending Apply/Discard/Cancel before handoff. Failed Apply retains unsupported
changes; use a separate native draft or explicitly discard. Do not capture unsupported
model/material/channel changes as if the current project adapter stores them. Closing a
busy view requires cancellation/completion. Native keys, project commands and text input
use their own Undo contexts and one command must not trigger two histories.

Current repeatable evidence: scripts/test.ps1 -Workspace -Gui, both -PreviewWorkspace
product restart checks, and every retained focused/default native/GUI suite. Inspect GL
framebuffers for rendered geometry. Small-window acceptance is 850x620; 150% scaling is
simulated, with dropdowns inside the pane. Real desktop DPI was 96; a second physical DPI
and monitor transition remain unverified. OS file pickers are bypassed by automation.

Native decoded output verifies actual supported motion/rig/resource preservation.
Project-only slot/anchor records do not become CHR0/PAC metadata or new game behavior.
Current Brawl adapter gates and the separate rigid-FK/humanoid limits remain authoritative.
The bounded interval contact route above is implemented. Broader fitting/IK, mirror,
template fill, channel/model capture, non-Brawl/Modkit assets and vetted tools still need
their backends and controls here. No placeholders implement those features.
Gameplay scripting/hitboxes/damage/ACMD/PSA/Lua/playable compilation remains deferred.
