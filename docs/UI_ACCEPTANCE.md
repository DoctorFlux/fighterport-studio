# Model Previewer authoring acceptance

The standing contract is FIGHTERPORT_MODEL_PREVIEWER_WORKSPACE.md. This guide records
the current surface and repeatable acceptance, not a second executable task queue.

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
Current Brawl adapter gates and FK limits remain authoritative. Mirror, fitting, IK/contact,
template fill, broader channels/model capture, non-Brawl/Modkit assets and vetted tools
need their own backends and usable controls here. No placeholders implement those features.
Gameplay scripting/hitboxes/damage/ACMD/PSA/Lua/playable compilation remains deferred.
