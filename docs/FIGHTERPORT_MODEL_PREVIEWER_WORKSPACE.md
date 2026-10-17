# FighterPort Studio — Model Previewer Authoring Workspace

## Proportion-aware humanoid amendment — 2026-10-17

The user now prioritizes proportion-aware, contact-preserving humanoid retargeting in
the existing Model Previewer. See [FIGHTERPORT_PROPORTION_AWARE_HUMANOID_RETARGETING.md](FIGHTERPORT_PROPORTION_AWARE_HUMANOID_RETARGETING.md).
Mirroring is not a prerequisite for fitting or later work. Preserve compatible data and
useful inherited operations; coordinate-basis conversion is distinct from animation mirroring.
The historical proposals below are superseded wherever they impose mirror priority.
Only docs/plans/current.md defines the next executable acceptance; native/source/contact
quality and game evidence remain separate. Gameplay compilation remains deferred.


## Purpose and reviewed baseline

This amendment makes the existing BrawlCrate-derived Model Previewer a first-class authoring workspace for all FighterPort model/animation features. It is not a new application, a renderer replacement, or a restart.

Reviewed repository: https://github.com/DoctorFlux/fighterport-studio

Reviewed commit: `8136ea72d66d27493b963e528413ecbf6bfd88c9` (2026-10-17).

Upstream reference: https://github.com/soopercool101/BrawlCrate at the retained pin `d56cbac89dc2cd0e5c3f923e1d03041b16902add`.

Re-read actual HEAD before implementation. Preserve any newer passing work. This document proposes requirements and implementation steps; it does not claim that the proposed workspace already exists. The review inspected documentation and selected source files, not a running Windows application or commercial game assets.

## 1. Product decision and immediate scope

The next executable task remains typed, expandable animation slots and persistent Idle/Fall anchors with verified native asset bindings or explicit unbound status. Implement that task inside the actual model previewer, not only in Character Library windows.

Standing rule: every new model/animation authoring feature needs a practical control surface in the Model Previewer, working against the same project/session and showing its effect or validation there. A service, command-line test, disconnected dialog, or button opening another authoring application is not complete user-facing integration.

The resource browser and standalone library remain useful entry points, but must share services and state with the previewer. A global setup operation such as selecting a game path may use an owned dialog opened from the previewer; it does not need to occupy the 3D canvas. Routine authoring should remain nonmodal so the user can see, scrub and inspect the model while configuring it.

Active scope: models; all requested animation channels; shared library; native editing; rig mapping and FK/IK retargeting; animation slots; Idle/Fall and other references; mirroring; transition fitting; template fill; native asset export; supported Modkit asset import; and supporting source/tool configuration.

Gameplay scripting, damage/hitboxes, ACMD/PSA/Lua, combo execution, cancels and playable moveset compilation remain deferred. Existing legacy draft data must remain compatible but must not be required for animation work. An attack slot is an animation-organizing slot, not implemented game behavior.

## 2. Source-derived findings

These findings are limited to the reviewed commit and inspected files. They are not broad in-game validation.

1. `docs/plans/current.md` identifies typed expandable slots and Idle/Fall anchors as checkpoint C. It says reviewed FK/shared storage checkpoints A/B pass for their declared original Brawl subset. Do not redo them. [S1]
2. `docs/DEVELOPMENT_STATUS.md`, section “Shared library and reviewed FK,” records disk-backed immutable revisions, multiple clips per source, separate character projects and measured original-rig FK transfer in both directions. It also explicitly lists typed slots, anchors, automatic characterization, mirror, fitting, IK and non-Brawl adapters as still pending. These are reported project results, not tests rerun in this review. [S2]
3. `CharacterLibraryForm` owns a `CharacterProject`, target/slot controls and the native edit lease. It opens a native preview and has a separate Capture native edits operation. `OpenNative` obtains an editor using `ModelEditControl.Instances.Last()` and uses `Program.RootNode`/`RootPath`. This is an integration point to improve; the review does not claim a wrong-editor incident was observed. [S3]
4. FighterPort's inspected `ModelForm.cs` has the same blob SHA as the pinned upstream file, `0ec1516702d62c0a724a1c250d71eb4f6e736d9e`. Its form hosts the actual `ModelEditControl`, and the upstream host subscribes to model/viewer changes and manages viewer initialization/settings. Extend this host instead of recreating its viewport. [S4, S5]
5. `ProjectRecords.cs` still represents a slot as `MoveInstance.Slot`, a string. It also retains stable IDs, target rig fingerprints, native animation IDs, map revisions, bake evidence and shared-library pins. Preserve those identities while introducing typed slots. [S6]
6. `RigMapReviewForm` supplies a reviewed mapping grid, helper/rest-only choices, root policy and unit ratio. It should become reusable in a previewer panel, without weakening its fingerprint/policy approval. It is not automatic characterization or IK. [S7]
7. `ModelEditControl/Animation.cs` already has animation-change handling and toggles for existing weight/vertex editors. New panels must coexist with these controls and the native animation/curve workflow. [S8]

## 3. Proposed everyday workflow

1. Open a target model through the resource browser, shared library or character project.
2. In the previewer choose “Set up character.” Select/create a project, confirm the target model/game profile and choose an animation-set template.
3. Select or review the source/target mapping when a cross-rig clip needs it. Reuse valid reviewed maps; do not ask for the same mapping per clip.
4. Open Library or Animation Set next to the visible model. Drag a clip onto a slot or choose it and click Assign.
5. Use the native playhead to set an Idle/Fall reference clip and its pose/phase. Preview each reference immediately on the target.
6. Add parts/variants, scrub assigned motion, inspect binding status and save the project. Export a separate native asset and show its actual validation result.

The user should not need to return to the main resource browser, search for a temporary file or manually find the most recently opened model editor to perform these steps.

For a plain native model with no project, keep native viewing/editing available. Show “Not associated with a character project” and an explicit setup action. Never guess that the first project, character or last-opened editor is the right association.

## 4. Proposed layout in the existing model editor

Keep the existing viewport, rendering, camera, native animation controls and detailed editors.

Suggested layout, not an existing screenshot:

```text
Project | Target game/profile | Target model | Current slot/part | Save | Export
+---------------------+-------------------------------+----------------------+
| Library /           |                               | Character setup      |
| Animation Set       | Existing ModelEditControl     | References           |
|                     | and native model viewport     | Rig / Retarget       |
| Source clips        |                               | Adjustments          |
| Slots / parts       | Source preview or target      | Output / Diagnostics |
| Binding status      | instance clearly identified  |                      |
+---------------------+-------------------------------+----------------------+
| Existing native timeline / frame controls / key and channel editors      |
| Reference phase controls; later fitting margins and contact intervals    |
+-------------------------------------------------------------------------+
```

Use resizable/collapsible tabbed panels. Native WinForms controls are sufficient for the first checkpoint; do not block this on a new docking framework, modern UI rewrite or runtime migration. Optional detachable panels may follow, but they must reference the same session.

Preserve a useful viewport at small sizes. Offer animation-authoring, rig-review and native-editing layout presets, a reset-layout command, keyboard access and text labels. Display capability status in text/icons, not color alone.

Layout/monitor positions belong in application preferences. Rig maps, anchors, slot bindings and corrections belong in versioned project data. A remembered panel layout is not persistence of a character's authoring state.

## 5. Feature-to-previewer integration contract

| Feature | Previewer surface and interaction | Current checkpoint versus later |
|---|---|---|
| Shared animation library | Search/browse source-bound clips, add selected native clip(s), inspect source rig/revision, preview without modifying sources, assign to active slot | Reuse current shared services now |
| Animation sets | Typed tree, Add part/variant, ground/air branches, selection and drag/drop or Assign button, native binding/unbound details | Implement now |
| Idle/Fall references | Choose a target-compatible clip/instance and frame/phase, “Use selected clip as Idle/Fall,” “Set reference phase from playhead,” preview and clear | Implement now |
| Project/model setup | Open/create/attach project, explicitly choose target and selected game/template profile, show source and destination paths separately | Implement enough now to complete the workflow |
| Reviewed FK | Reusable map grid, source/target roles and helper/root policies, invoke existing measured route, show diagnostics/evidence | Expose existing route now; preserve its limits |
| Bone/mesh inspection | Reuse native bone selection, weight/vertex controls and model panels; synchronize semantic selection where exposed | Preserve now; expand without duplicate state |
| Mirror | Per-instance toggle, reviewed pair editor, source/target comparison and native validation | Later backend checkpoint; no fake working toggle |
| Transition fitting | Anchor selection, editable start/end margins, protected action range, raw/fitted/reference sequence preview | Anchors now, actual fitting later |
| IK/FK and contacts | Bone/role picking, hand/foot/prop controls, pole targets and interval lanes; clear airborne behavior | Later solver checkpoint |
| Miscellaneous templates | Select pack, preview affected slots, Fill compatible missing animations, atomic apply/undo | Later generation checkpoint |
| Models/materials/animation channels | Existing native controls plus declared adapter/channel capabilities; unsupported details remain explicit | Preserve native workflows and extend by capability |
| Native export/Modkit assets | Export active animation or supported animation-set subset, target binding/resource preview, actual write/decode report | Current supported Brawl path now; other adapters later |
| Source paths and tools | Settings/setup access from previewer to the shared source manager and future vetted app-local tool manager | Reuse what exists; provisioning stays queued |

A roadmap panel can say an operation is not implemented. A disabled button or a recorded flag does not count as implementing its solver, writer or usable workflow. Do not delay slots/anchors until all future features exist, and do not omit their integration requirement from their later milestones.

## 6. Setup controls and semantics

### 6.1 Target identity

Always show the active project, target character, rig fingerprint/review status, selected slot/part and whether the viewport contains a source-library preview or an editable target instance.

Registering or selecting a character model is not proof that its rig or native output bindings are correct. Existing supported format gates remain authoritative. Unknown or unsupported game/profile selections must not activate a fictional writer.

Opening a second model for comparison must not silently change the export target. Display-only offsets/camera framing must never be written into the character's skeleton or root animation.

### 6.2 Typed slots

Introduce stable group/slot/part/variant IDs and a versioned template-pack identity. Preserve legacy free-text assignments as custom/unbound records without data loss.

Support repeatable jab/tilt/smash/aerial parts, neutral/side/down/up special variants, ground/air variants, start/hold/loop/release/follow-up/end phases, taunts, movement and miscellaneous categories. No small hard-coded gameplay count; bounded authoring quotas must diagnose capacity rather than masquerade as game rules.

Each assignment needs its pinned shared clip revision, target rig/instance and mapping dependencies. Reordering parts must not change stable binding identity. Do not flatten all parts into one long clip unless an explicit supported export operation requests that.

Show binding state independently from clip availability: for example, “assigned; unbound,” “verified native asset binding,” “missing reference,” or “unsupported writer.” A slot label is not evidence. Verification must identify the actual selected target resource/identifier, template/profile version and validation evidence. Native asset binding is not a claim of tested runtime combo behavior.

Duplicate destination bindings must be rejected or handled only by an explicit profile-supported alias/shared-resource operation. Extra parts remain saved and visible even when no game binding exists.

### 6.3 Idle/Fall and other anchors

Save references per target character using stable target-compatible clip/instance revision and frame/phase. Record the loop/time interpretation and rig dependency. A source-rig pose cannot be used as a target anchor merely because its clip has the right name.

“Use selected animation as Idle” chooses the reference motion. “Set Idle phase from playhead” chooses a phase in that motion. Keep these distinct from capturing an arbitrary pose. If pose snapshots are supported, store evaluated target-space/local-bone pose data with rig identity, not a screenshot.

Phase changes should visibly update the target preview without changing the original source clip. Missing/stale revisions need relink/recompute actions or explicit blocked status. Self/cyclic anchor dependencies must not silently create recursive generation.

Setting an anchor does not perform smoothing. Until fitting exists, show the chosen reference and raw clip honestly; do not crossfade only in the editor and label the exported file corrected.

Part-to-part continuation and return-to-Idle/Fall are distinct relationships. Aerial motions get no default planted-foot constraint. Unknown transition policies remain explicit.

## 7. Shared session and editor binding

### 7.1 One authoritative project, multiple views

Extract or introduce an application-side authoring session/controller around the existing `CharacterProject`, shared library and services. A possible name is `PreviewAuthoringSession`; this is a proposed name, not an existing class requirement.

The standalone Character Library and the previewer's panels must use the same live session for a given project. Do not instantiate independent CharacterProjects and synchronize them by saving/reloading files. Different projects remain independent while sharing immutable library records.

Pass an explicit editor/session/document binding when opening a model. Do not use “last ModelEditControl instance,” window order, a displayed name or global Program.RootNode alone as project/instance authority.

A preview binding should identify project ID, target ID/fingerprint, instance ID/revision, native animation/resource identity, edit lease and request generation. Source preview is read-only with a separate identity. A switch cannot reassign ownership of the prior editable document.

Where legacy global native-document ownership prevents simultaneous editing, initially allow one explicitly active editable native document and clearly read-only comparisons. A safe explicit handoff is preferable to claiming full multiple-document editing. Test the handoff and prevent stale writes.

### 7.2 Native edits and commands

Reuse the current native lease/capture validation and project transaction boundaries. Provide a clear Apply edits action in the previewer, or commit supported native changes at defined edit boundaries. Saving/switching with pending edits must offer Apply/Discard/Cancel as appropriate. Failed validation retains the pending edits and does not partially modify project data.

Do not silently capture model/material/channel changes the current project adapter cannot persist. Explain the supported save path and preserve those edits. Immutable library source previews never become writable target instances by accident.

Route Undo/Redo through the active edit context. Show whether the command concerns a native key edit or a project structural change. Do not invoke both undo stacks for one keystroke or let Ctrl+Z in a text field mutate another target. Integrate compound operations atomically and keep all views consistent. Saved state must survive restart; retaining undo history across restart is not newly required.

Selecting a library clip previews it; assigning it creates/updates an explicit target instance after prerequisite checks. Preview should not mutate a slot, a source revision or the other project. Cancelled drag/drop/setup/map review leaves state unchanged.

### 7.3 Preview and timeline

Bind panel selection, playhead, active clip/part, current target model and native animation channel intentionally. Use the existing native playback path rather than an independent timer/evaluator that drifts from the viewport.

Add explicit source, target and reference viewing modes. Reuse the existing model viewport infrastructure. Comparing two models requires explicit identities; display transforms are separate from animation transforms.

Do not reset the camera, reopen a modal model window or rebuild a whole archive for a metadata-only anchor/slot edit. Reuse live/native cached documents where valid and invalidate them by actual revision changes. Native bake/export can still use guarded staging at defined commit points.

Long index/retarget/bake work needs cancellation and revision-tagged results. Superseded results cannot populate a newly selected slot. Perform control/GL updates on the appropriate UI/render thread and respect native-document lifetimes. Preserve budgets; never load an entire library/dump into the previewer.

Closing/reopening a panel must unsubscribe and rebind safely, avoiding duplicate handlers, duplicate commands and stale edit leases. Closing a project while its model viewer is open needs a clear save/discard/cancel path.

## 8. Suggested file-level work

These are inspected existing files and proposed responsibilities, not a requirement to create duplicate implementations:

- `application/BrawlCrate/UI/ModelForm.cs`: host collapsible authoring panels alongside the existing control; accept an explicit session/editor context; add setup/save/export entry points and layout management.
- `application/BrawlCrate/UI/Model Previewer/ModelEditControl/`: integrate selection/playhead/channel events and contextual commands while retaining rendering, camera, native editors and shortcuts. Inspect relevant base APIs before adding hooks.
- `application/BrawlCrate/UI/CharacterLibraryForm.cs`: separate reusable controls/presenters from form-local state; keep this window as another view. Replace reliance on the last editor instance with an explicit returned editor binding.
- `application/BrawlCrate/UI/RigMapReviewForm.cs`: reuse the actual reviewed-map services and grid in an embeddable panel. Selecting a row/bone should reveal the corresponding identity; do not auto-approve roles.
- `application/BrawlCrate/Authoring/ProjectRecords.cs`: typed slot/template/anchor records and migrations; retain native/source/rig/library identities and custom assignments.
- `application/BrawlCrate/Authoring/CharacterProject.cs` and its existing authoring services: atomic slot/anchor commands, validations, dirty-state ownership and explicit editor leases; preserve shared/FK regression behavior.
- Existing shared-library browser/storage: reuse immutable revisions and lazy cursor, not a second store or per-character copy of the library.
- Native GUI and application-startup tests: verify the new workflow from the actual Model Previewer, not only old library-form handlers.

Possible new small UI components: LibraryPane, AnimationSetPane, CharacterSetupPane, ReferenceAnchorsPane and PreviewSessionBinding. Choose names consistent with the current code. Do not build an abstract plugin framework in place of a working checkpoint.

## 9. Immediate implementation sequence

### C0: establish previewer session integration

Read current docs/code and run the baseline. Make the existing library/FK/project commands reachable from collapsible controls in the actual model window. Preserve the native model editor. Bind the exact project/instance and provide Save project/Apply edits/Export native actions there.

Acceptance: open an original existing project, preview its instance, make a supported native edit, commit, undo/redo, save and reopen through these controls. The standalone library, when also visible, reflects the same state. Source preview remains unchanged.

### C1: implement typed slots and anchors in that workspace

Implement the current authoritative slot/anchor task, with versioned records and v1/v2 migration. Add an original fixture template, expandable groups/parts/variants, explicit bindings/unbound status, target Idle/Fall selection and playhead phases.

Acceptance includes the full current-plan tests plus previewer-first usage. No game-specific binding can be inferred from a test fixture label.

### C2: run integrated regression and publish reviewed source

Exercise independent project/library state, native output decode, migrations, resizing, close/reopen, dirty changes, stale results and unavailable capabilities. Recheck the current plan before advancing, then record the next single executable task. Follow the existing standing source-only publication authorization with exact public origin/main and remote SHA checks.

Later mirror, fitting, IK, template-fill and non-Brawl/Modkit adapter milestones must add functional controls to the same workspace when their backends are implemented. Do not claim them completed by creating this panel shell.

## 10. Concrete acceptance checklist

### User-visible workflow

- Start the real application with an original target fixture; open its Model Previewer normally.
- From that window attach/create the project and select the original fixture template. Keep the target model visible during routine setup.
- Browse the real shared library and assign an existing same-rig or reviewed cross-rig clip through the current valid route.
- Expand a group beyond a fixed three-part/two-part assumption, include special ground/air variants, and preserve an existing custom assignment.
- Display one evidence-bound resource and one intentionally unbound extra part; both retain their clips.
- Set Idle and Fall to explicit target-compatible reference revisions/phases using previewer controls and the native playhead.
- Preview each reference, switch back to the assigned motion, play/scrub and inspect actual rendered poses. Selecting a slot loads the correct instance, not a same-name clip on another target.
- Undo/redo a grouped slot/reference change; all visible views agree. Native editing and structural undo contexts do not conflict.
- Save, close and restart the actual product; reopen the same previewer workspace. Authoring state, stable IDs, bindings, phases and clips are correct. Layout preference is tested separately.
- Export a new supported native asset from the previewer and decode it independently. Compare actual motion timing/keys/poses, rig binds/weights, hierarchy and retained resources under declared existing tolerances.

### Important evidence distinction

Slot/anchor authoring metadata is verified in the saved project. It must not be claimed to be encoded into a native format that has no representation for it. Native checks prove the assigned/baked motion and actual supported binding/output, not that project-only UI metadata magically exists in CHR0 or PAC.

### Independence and failure paths

- Two separately saved projects pin the same immutable shared clip. Edit target one, its slot and anchor. Target two and shared revisions remain unchanged after restart.
- Open a second preview window/project, then complete or cancel an operation started in the first. It cannot apply to the wrong document/slot. If only one active edit lease is supported, test explicit handoff and read-only status.
- Stale/missing source, library revision, map, anchor, template and target resource generate precise diagnostics. Known valid revisions must not be silently rebound.
- Invalid/cancelled packs, duplicate target bindings, bad phase bounds, unsupported channels and identity mismatches leave project/library state unchanged.
- Reject source overwrite and stale native capture. Preserve dirty edits on validation failure.
- Test source preview versus target edit, text-field Undo versus project Undo, closing the viewer with pending native edits, panel hide/reopen and reopen after layout changes.
- Check normal/small window sizes and high-DPI behavior; no hidden mandatory controls or unusably small viewport. Label simulated scaling separately from real OS-DPI tests.
- Capture actual rendered geometry using an appropriate native/GL framebuffer path; a window screenshot with a blank GL region is not render evidence.
- Recheck unchanged original hashes and native/shared revisions after actual product-startup tests.

No commercial, external native-tool, Modkit or in-game check is claimed unless actually run. Original fixtures keep this task executable without commercial input availability. Native decoded output is not proof that a new runtime moveset part is executable.

## 11. Documentation and publication

Update the existing master plan, architecture, UI acceptance documentation, format/capability evidence, tests and current status. Add a discoverable reference from AGENTS.md. Preserve the supplied previous specifications and annotate this as their previewer-integration amendment.

`docs/plans/current.md` remains the one executable task queue. Its immediate acceptance must explicitly require slots and references working inside the actual Model Previewer. Do not replace it with a disconnected “UI someday” roadmap or require every future feature before passing this bounded increment.

Use current build/test/doctor/review scripts, inspect staged/outgoing source and preserve all previous regressions. Commit and push only a coherent passing source checkpoint under the repository's current standing authorization. Verify exact PUBLIC origin/main and the remote SHA. Preserve history; no force pushes, assets, derived captures, settings, binaries, tags, releases or CI dispatch unless separately authorized.

Keep upstream notices and source provenance. The previewer remains FighterPort's BrawlCrate-derived application. Do not delete/recreate the repository or reintroduce the old WPF shell.

Report actual build/test results, which previewer controls were exercised, native decoded measurements and original hashes, local executable path, remaining limitations, and publication result. Never replace missing GUI/native verification with a convincing screenshot description.

## 12. Sources inspected

[S1] Current task:
https://github.com/DoctorFlux/fighterport-studio/blob/8136ea72d66d27493b963e528413ecbf6bfd88c9/docs/plans/current.md

[S2] Status, “Shared library and reviewed FK”:
https://github.com/DoctorFlux/fighterport-studio/blob/8136ea72d66d27493b963e528413ecbf6bfd88c9/docs/DEVELOPMENT_STATUS.md

[S3] Character Library form and its current native-preview integration:
https://github.com/DoctorFlux/fighterport-studio/blob/8136ea72d66d27493b963e528413ecbf6bfd88c9/application/BrawlCrate/UI/CharacterLibraryForm.cs

[S4] FighterPort model-preview host:
https://github.com/DoctorFlux/fighterport-studio/blob/8136ea72d66d27493b963e528413ecbf6bfd88c9/application/BrawlCrate/UI/ModelForm.cs

[S5] Pinned upstream model-preview host:
https://github.com/soopercool101/BrawlCrate/blob/d56cbac89dc2cd0e5c3f923e1d03041b16902add/BrawlCrate/UI/ModelForm.cs

[S6] Current project records:
https://github.com/DoctorFlux/fighterport-studio/blob/8136ea72d66d27493b963e528413ecbf6bfd88c9/application/BrawlCrate/Authoring/ProjectRecords.cs

[S7] Reviewed rig-mapping form:
https://github.com/DoctorFlux/fighterport-studio/blob/8136ea72d66d27493b963e528413ecbf6bfd88c9/application/BrawlCrate/UI/RigMapReviewForm.cs

[S8] Native model editor animation/control integration:
https://github.com/DoctorFlux/fighterport-studio/blob/8136ea72d66d27493b963e528413ecbf6bfd88c9/application/BrawlCrate/UI/Model%20Previewer/ModelEditControl/Animation.cs
