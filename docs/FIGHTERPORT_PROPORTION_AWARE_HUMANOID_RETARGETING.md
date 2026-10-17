# FighterPort Studio — proportion-aware humanoid retargeting

## Product amendment and implementation directive

Date: 2026-10-17.
Reviewed remote commit: `7458be7d9a11f95c4c4c1dd586819fd9dc40e831`.
Repository: https://github.com/DoctorFlux/fighterport-studio

This is an amendment to the existing BrawlCrate-derived product, not another restart.
Re-read actual HEAD and preserve newer passing work before making changes.

**Replace the queued mirroring milestone with proportion-aware, contact-preserving humanoid retargeting inside the existing Model Previewer.** The user no longer wants mirroring prioritized. Remove it as a prerequisite from the current acceptance queue and active roadmap. Do not delete compatible saved data, useful inherited commands, uncommitted work, or coordinate-system conversion code merely because mirroring is no longer a requested feature.

The intended experience is: register a source character once; choose any of its library animations; select a target character or slot; automatically suggest and reuse humanoid profiles; preview a target-proportioned result; make occasional visual corrections; save and export a genuine target-native animation. This must work across character projects and eventually all requested game adapters, not only Brawl.

Immediate real inputs: the user says that Brawl Snake and Brawl Mario model and animation `.pac` files are present at the **local workspace root**, not on GitHub. Locate and inspect these in Codex's local workspace. Do not ask the user to upload them when a local bounded inspection can resolve their identities. This review did not access those files and does not establish their versions or contents.

Active scope remains models and animation channels, library/slot authoring, retargeting, editor IK/FK, boundary fitting, native asset export and supported Modkit asset import. Gameplay scripting, hitbox/damage translation, combo execution and complete playable-fighter compilation remain deferred. Preserve their longer-term requirements and existing compatible draft data.

## 1. What the inspected source establishes

### FighterPort status

At the reviewed commit, `docs/plans/current.md` schedules per-assignment mirroring. The current `DEVELOPMENT_STATUS.md` instead records slots/anchors checkpoint C as complete for its declared original Brawl subset: shared Model Previewer surfaces, one authoritative project/session, expandable slots and variants, explicit native asset destinations or unbound status, Idle/Fall references, native edit application, undo/redo, restart and native-output checks. These are repository-reported results, not tests rerun in this review. [S1, S2]

The current `RigTransfer.cs` separates rigid FK mathematics from native sampling/writing and the renderer. It transfers rest-relative world rotations through reviewed bone pairs and reconstructs target locals. Its snapshot validation still checks imported flags/billboard metadata, so a truly multi-game boundary must interpret native flags in adapters rather than spread Brawl-specific bit masks into new generic solvers. Preserve its tested behavior as a regression baseline. It is not yet a proportion-aware contact solver. Its admission rules include unit rest scale, no compensation/billboard flags, a parent-first single-root tree, no animated non-root translation, and explicit static-ignore/rest-only helper policies. It rejects certain Euler singularities and has a fixed bone budget. [S3]

`BrawlProjectTransfer.cs` currently samples native CHR0 through BrawlLib, bakes integer-frame values, writes genuine native output, then checks reopened bone transforms and weighted geometry at fractional samples. Animated scale and certain winding/compensation cases reject. These checks are valuable serialization/evaluation evidence, but fidelity to the existing FK calculation does not prove natural body proportions, contact preservation, or freedom from body penetration. [S4]

The current documented integrated authoring gates still include MDL0 v11/CHR0 v4/RGBA8/canonical-container restrictions. Historical read-only inspection observed MDL0 v9 in a configured Mario source. That observation does not establish the newly supplied root files' versions, nor mean that every inherited native-editor capability is exposed through the guarded project adapter. Inspect actual files before selecting the next admission increment. [S1, S2]

### BrawlCrate CHR0 “Port”

The inspected upstream `CHR0Node.Port(targetModel, externalModel)` finds source and target bones by matching animation-entry names and offsets existing translation keys against bind translations. Its rotation-adjustment switch cases are commented out. It does not provide semantic chain mapping, proportion-aware goals, IK contacts, torso clearance or general cross-game conversion. [S5]

Do not rename that routine, uncomment Euler offsets, or remove validation guards and present the result as a modern retargeter. Keep legacy behavior available where appropriate; make the normal FighterPort action invoke the shared, reviewed retargeting service on an independent target instance.

## 2. Definition of success

The target is a reusable humanoid system, not a special-case Snake/Mario converter.

For a supported humanoid rig and valid source animation, FighterPort should preserve the meaning and timing of the motion while adapting it to the target's real skeleton, limb lengths, body width, head/hand/foot dimensions and reachable contacts.

“Looks normal” is not identical to “every source angle copied exactly,” and not identical to “every source world-space hand position copied exactly.” These demands can conflict when proportions differ. The solver needs explicit priorities and visible residual error rather than hidden stretching or a universal success badge.

Support tall/slender, short/stout, broad/heavy, long/short limb, large head, large hand and large foot variants within declared tested ranges. Reject invalid/degenerate rigs precisely. A genuinely unreachable gesture must produce an adapted reachable pose or a specific correction request, not impossible joint transforms.

Preserve target mesh, bind matrices, skin weights and deform skeleton during animation transfer. Mesh rebinding is a separate operation. Avoid automatic limb stretching or mesh resizing to force a contact. Authored squash/stretch can be handled later through an explicit, tested policy; it must not be silently discarded.

## 3. One shared solution, several distinct data layers

Retain native originals and immutable source revisions. Add or evolve the following concepts; names below are design suggestions, not claims that these classes exist.

### Humanoid characterization

A reusable per-rig record identifies:

- Scene/actor root versus pelvis/waist; deform, helper, attachment, and control roles.
- Pelvis, spine chain, chest/bust/upper torso, neck and head.
- Left/right clavicle, upper arm, forearm and wrist/hand.
- Thumb and finger chains with their actual joint counts and optional segments.
- Thigh, shin/knee joint, ankle/foot and toes.
- Optional twist, face, jaw, eye, tail, hair, outfit and prop chains.
- Authored rest/bind transforms, calibrated retarget pose, local anatomical axes, bone order, parentage and evaluator interpretation.

A role is not necessarily one named bone. A knee is the hinge within a leg chain. A heel or sole target may be a virtual landmark. Do not require fake bones to be added to every source or target.

Chest/bust as the main torso role is distinct from optional secondary deformation chains. Unknown optional anatomy must remain visible and preserved rather than assigned an invented role.

### Character body profile

Rig fingerprints alone are insufficient for body-shape corrections. Store a separately versioned body profile tied to the relevant mesh/weights/rest pose:

- Limb segment lengths and chain lengths.
- Pelvis and shoulder widths, torso dimensions and head envelope.
- Palm/grip landmarks and foot heel/sole/toe landmarks.
- Conservative editable collision/contact proxies derived from actual skinned geometry where supported.
- Ground plane, facing/up convention and calibrated units or explicit unknown-unit state.

Use simple capsules/ellipsoids and landmarks for an initial practical solver. Do not describe these approximations as exact arbitrary mesh collision. Exclude broad costume/hair geometry from body proxies unless explicitly selected. Two meshes sharing a skeleton may require different body-fit profiles.

### Shared retarget profile and per-assignment recipe

A reviewed source-target profile connects characterization, body profiles and policies. Bind it to exact revisions and interpretation fingerprints; persist it in the shared store for reuse across projects, not only a single form session.

Per-animation or per-slot recipes record contacts, intent choices, root policy, protected times, correction layers and target Idle/Fall references. The same immutable source clip may have different recipes for Brawl Mario, Ultimate Mario and a custom Goku.

Edits to a target instance must not mutate the source library clip, other assignments, or another project's map/profile revision.

## 4. Automatic setup with explicit uncertainty

Implement an “Auto-detect humanoid” action inside the existing previewer's Setup/Rig surface. Use, in order:

1. An exact, already reviewed profile and compatible body-profile revision.
2. Vetted game/character-family templates and aliases.
3. Hierarchy, rest-space geometry, anatomical symmetry and available skin-weight evidence.
4. Focused visual review of uncertain or contradictory results.

Show simple body-role highlighting and a short list of unresolved chains. Selection in the role panel should select/highlight the relevant bones; viewport selection should reveal their role. Keep raw technical fields under Advanced.

Unfamiliar mappings are proposals, not silent approvals. Do not have the coding agent falsely sign off a human review. Verified known profiles can be reused automatically; new ambiguous commercial profiles need an honest review state and a usable review workflow.

Do not require the user to map every finger anew for every clip. Batch characterization and registration are per rig, with body fitting per mesh profile and corrections only where an animation needs them.

Separate all three of these:

- **Bind/rest pose:** native skeletal/skin reference.
- **Retarget calibration pose:** editor-side alignment between semantic frames.
- **Idle/Fall anchors:** target motion references for later entry/exit fitting.

Selecting an Idle frame must not silently replace the skeleton's bind pose.

## 5. Retargeting pipeline

### A. Correct native evaluation first

Decode each animation on its own source skeleton before any transfer. Verify units, handedness, up/forward axes, local versus world transforms, frame conventions, Euler order/quaternion interpolation, root semantics, scale compensation and inherited helper behavior.

Use the native adapter to evaluate source poses faithfully. Do not replace unknown native behavior with guessed normalized Euler curves. Preserve curve tangents, full-turn/winding information, rest defaults, time origin and loop conventions in the source representation.

A coordinate-system basis conversion may include sign changes. That is not the removed user-facing mirror-animation feature and must not be disabled.

### B. Rest-aware semantic chain transfer

Transfer motion in a documented common interpretation using rotation matrices/quaternions and explicit calibration frames. Derive target local transforms using the target's animated parent hierarchy. Do not copy source Euler components or add bind Euler angles as a universal solution.

Extend from one-to-one bone pairs to chain mappings. Multiple spine/neck/finger joints, twist helpers and different parent layouts must have explicit distribution policies. Preserve target bone lengths/local rest offsets by default.

Retain the existing tested rigid FK path as a base pose and comparison. Add new algorithm/profile versions rather than silently changing old saved recipe meaning.

### C. Proportion-aware movement goals

Derive goals according to motion intent, not one global height multiplier:

- A free arm swing primarily preserves the source's pose rhythm and direction.
- A hand near the face is related to the target head/face landmark.
- A hand touching a shoulder remains related to that target shoulder.
- A two-hand pose preserves the hand-to-hand relationship.
- A grip is related to an explicitly available prop/grip transform.
- A planted foot is related to a floor-contact location and target sole, not simply ankle height zero.
- Locomotion can use an explicit stride/root policy while maintaining planted intervals.

Keep unit conversion separate from proportion adaptation. Do not infer real-world centimeters from character height without a calibrated profile.

Contact candidates may be detected from speed, distance, orientation and relevant body/prop geometry, then edited in the previewer. Nearness alone is not proof of intentional contact. Store whether evidence was user-authored, template-derived, or automatically proposed.

### D. Coupled IK/FK correction

Use the FK result as the preferred animated pose, not an unrelated rigid T-pose. Apply contact-aware limb IK and a coordinated pelvis/spine/clavicle solve where necessary. A practical staged implementation may begin with stable two-bone limb solvers plus bounded body adjustments, then general multi-goal solving.

Required behaviors:

- Foot and hand position/orientation goals, explicit elbow/knee bend directions.
- Smooth contact entry/exit and intervals, not permanent endpoint pinning.
- Pelvis compensation, torso/shoulder participation and controlled joint limits.
- Swing/twist separation and tested twist distribution.
- Reach limits and reported residuals; no uncontrolled stretch.
- Finger curl/spread/orientation transfer with optional contact correction for supported grip geometry.
- Editor controls remain virtual and are not silently exported as deform bones.

Do not lock airborne feet to the floor. Do not force realistic motion limits on stylized fighting poses without an explicit profile choice. Preserve important action poses and clip timing unless the user requests a change.

### E. Body-shape clearance and intentional contacts

Body proportions include volume, not only joint distances. Correct obvious arm/hand intersections with a stout torso or large head using reviewed body proxies while preserving deliberate contact pairs.

Body-clearance rules, root rules and contact constraints must be solved together or revalidated after correction. Moving an arm to avoid the torso must not break a protected two-hand grip without a warning.

Keep visual fitting proxies separate from gameplay hurtboxes/hitboxes. This phase does not author combat collision.

### F. Temporal consistency and boundary fitting

Prevent elbow/knee flips, jitter, discontinuous IK activation and root drift. Smooth only where permitted; preserve sharp attack poses, impacts, protected intervals and timing. Native channels may deliberately contain discontinuities; do not smooth all channels indiscriminately.

The result must be deterministic across playback, reverse scrubbing, random frame selection and export. Stateful IK should be solved/cached on a deterministic clip timeline rather than depending on which frame the user last viewed.

Use existing Idle/Fall anchors for explicit target entry/exit fitting after the motion transfer is correct. Anchor fitting and contacts may interact, so validate constraints after fitting or incorporate them into the boundary solve. Do not require mirroring first, do not force every motion to Idle, and do not return multipart continuations to Idle between parts.

### G. Native bake and decoded validation

Bake the final target pose onto supported native bones/channels. Carry over native timing and declared root policy; use the actual adapter's frame/loop/curve semantics. Add adaptive samples or richer curve construction where integer baking fails fractional-time validation. Quaternion processing must not erase source full turns or invent a shorter rotation path.

Reopen output through its actual target decoder. Separate:

1. Native write/decode error relative to the solved target animation.
2. Retarget quality: intended target contacts, limb lengths, body clearance, joint behavior and temporal continuity.
3. Target application/Modkit/game validation.

Passing the first does not prove the second or third. Compare analytical original fixtures to independent expectations, not only solver output against the same solver.

Publish declared per-profile tolerances for contacts, floor penetration, foot slide, orientation, skinned-vertex export error, root path, loop seams and temporal continuity. Normalize body-fit measures to relevant target limb/foot dimensions when physical units are unknown. Report sampling density and unsupported intervals; do not call sampled checks a continuous-error proof.

## 6. Required simple Model Previewer experience

Keep the actual native Model Previewer, shared `CharacterProject` and `PreviewAuthoringSession`. Do not create another shell or disconnected configuration workflow.

The normal workflow:

1. Add character/model and its motions; select target game/profile.
2. Auto-detect humanoid, review only unresolved roles/landmarks, and save the reusable profile.
3. Drag a shared library clip into an animation slot.
4. Choose **Auto Retarget / Fit to Character** and inspect synchronized source, FK-only and corrected target previews.
5. Apply, save and export using the verified destination.

Default controls should be a small set such as mode (Auto/Grounded/Aerial/Contact), preserve timing, root movement policy, and contact/clearance correction. Mode is a constraint preset, not inferred gameplay code. Expose detailed weights and diagnostics only as needed.

Provide visual hand/foot/pelvis/pole handles, contact intervals and body-proxy overlays in an optional Edit Constraints mode. Persist corrections, not only the screenshot. No mandatory node graph or repeated per-bone dialogs.

Use clear states: source preview (read-only), target instance, needs rig review, missing prerequisite, native output validated, quality needs correction, and target-game validation not run. An overall green badge must not hide unsupported channels.

Reuse the existing source/target/instance/editor binding, single editable-document handoff and Apply/Discard/Cancel protections. Preserve independent native/project/text undo and project restart behavior. Project metadata changes must not reset the camera or detach the active target.

On successful assignment, reuse the exact profile automatically for further clips. Batches are bounded/cancellable with per-clip results and non-destructive commits. Do not leave a batch half-assigned under a false all-success label.

## 7. Real Snake/Mario inputs: first practical route

### Local discovery and protection

Read the current working directory and ignored local configuration. Inspect the stated workspace root and only relevant explicitly configured child folders. Find candidate `.pac` model/motion files, inspect signatures/entry inventories, and record exact local identities; filenames are hints, not proof.

Do not recursively scan all user drives. Do not publish root assets, native derivatives, previews, fingerprints containing private paths, or screenshots. Keep the existing source/recovery rules. Verify that local commercial inputs and generated data are excluded from staging and outgoing commits.

Because inputs are at the repository root, do not mount the entire repository/application directory as a game-source root. Use a narrow selected-file import or protected staged-source registration with immutable original evidence. If the current guard lacks a safe selected-file route for root files, implement and test that route rather than disabling the guard or treating source code as game assets.

### Readiness before retargeting

For both Snake and Mario, inventory model/animation versions, nested containers, compression, texture/palette references, skeleton hierarchy, scale flags, root/helper roles and actual clip lists. Open source motions on their own native model first.

The project adapter's MDL0 v11 gate may block actual inputs even when inherited BrawlCrate can view them. Implement required version/container/texture admission with focused tests before removing restrictions. Never claim valid retargeting when the source is already decoded incorrectly.

Select available clips that exercise grounded movement, a crouch or body-relative gesture, a free/aerial motion, and a hand/prop or two-hand interaction when corresponding assets exist. Do not assume exact animation names or that an absent prop was recovered. Original authored contact fixtures cover missing categories.

### Measured transfer

Perform Brawl Snake -> Brawl Mario and Brawl Mario -> Brawl Snake independently. Preview source, FK baseline and corrected target inside the same workspace. Save a new native target animation/container, reopen it, and verify target skeleton/binds/weights and unrelated records remain intact.

Use the user's local assets to check visual usefulness, not as the only automated fixture set. Do not commit them. Human acceptance of the real rendered motions should be recorded separately from synthetic numerical checks.

Do not claim successful Brawl Snake -> Ultimate Mario from this test. That route requires the actual Ultimate target model/rig, its own adapter, decoded native output and separate validation.

## 8. Multi-game architecture remains mandatory

Proposed shared route:

`native source reader/evaluator -> source-bound animation + humanoid/body profiles -> game-neutral retarget/contact pipeline -> target-native writer -> decoded validation -> optional Modkit/game check`

Retain a parallel native-resource layer and the existing Brawl editor. CHR0/MDL0 are not the canonical interchange for other games.

Primary peer asset targets remain Brawl/Project M/Project+, Melee, Ultimate and Rivals II. Preserve Smash 64/Wii U/3DS and the wider roadmap. Dragon Ball FighterZ, Sparking! ZERO and Xenoverse 1/2 remain source adapters with separately declared native-output/playable-mod roadmap goals. Each version/direction/operation has its own evidence.

Immediately after the initial useful Snake/Mario route, prove a non-Brawl native route through the same solver. Ultimate is a strong next acceptance because the requested Brawl Snake -> Ultimate Mario example forces different native assets and skeleton interpretations. Inspect configured Ultimate inputs; use genuine original native fixtures when commercial targets are unavailable. Do not fake a writer or relabel JSON/CHR0 as another game's native animation.

For Rivals II, inspect the installed Modkit and supported import workflow. Its official guide requires joint/morph baking and excludes dynamic retargeting/IK on the main character mesh, so preview-only corrections are insufficient. Do not hard-code an engine version from old conversations. [S9]

Melee and Dragon Ball game/version-specific adapters retain their own evaluators, package readers, channel mappings and native writers. Merely using Unreal technology does not make two cooked asset formats or games interchangeable.

Keep one visible app. Use supervised modern/64-bit workers for dependencies or large archives where needed; do not move the entire UI or load full game installations into the legacy x86 process. Keep pinned, vetted app-local tool provisioning, version probes, verified downloads, cache/offline support and manual overrides. No arbitrary source scripts, bundled game assets, global installs or automatic EULA acceptance.

## 9. Validation matrix

Add real humanoid original fixtures instead of relying exclusively on two-/four-bone paddles. Use original meshes, anatomically identified limbs, fingers/toes, blended weights and independently authored motion goals.

Required categories:

| Category | Required evidence |
|---|---|
| Identity | Same rig/profile transfer preserves the declared source motion and timing. |
| Proportions | Tall/slender -> short/stout, reverse, broad/heavy, long/short arms and legs, enlarged head/hands/feet. |
| Body shape | Two targets sharing one rig but different mesh volumes receive different clearance profiles. |
| Axes/hierarchy | T/A/calibration differences, different local axes and parent layouts, extra twist helpers, different spine/finger counts. |
| Coordinate invariance | Re-express equivalent source/target bones with adjusted local bases/binds/curves; rendered motion and semantic result remain equivalent. |
| Feet | Planted intervals, heel/sole/toe orientation, pelvis compensation, takeoff/landing, no aerial locks. |
| Hands | Hand-to-head/shoulder, hand-to-hand and supported prop grip under different body proportions. |
| Reach/limits | Unreachable targets produce bounded adaptation/diagnostics, not stretching, flips, NaNs or jitter. |
| Style | Free motion retains rhythm, protected attack poses and duration; stylized allowed joint ranges are not silently clamped away. |
| Temporal/native | Fractional samples, multiple full turns, sharp poses, loop seams, random-access preview and decoded native curves. |
| Persistence | Profile/recipe revisions, contacts, anchors, undo/redo, actual restart, shared-library and other-project independence. |
| Robustness | Stale/tampered mappings, missing source/prop, unknown units, unsupported scales/channels, cancellation, solver nonconvergence, source overwrite guards. |
| Cross-game | One genuine non-Brawl output through the same solver and independent reverse-direction results before reciprocal claims. |

Test contact quality against independently defined target goals. Do not make all contact goals equal whatever the solver already produced. Quality metrics require both positions and orientations where relevant.

Retain all existing native, shared, library, FK, source, container and previewer suite families. Keep old FK regression expectations versioned. Do not falsely carry old certificates over a changed algorithm or manual edit.

## 10. Execute as bounded, useful checkpoints

### H1 — real-input admission and reusable humanoid setup

First reconcile the mirror cancellation in current docs. Inspect local Snake/Mario files, verify own-rig playback, implement only the needed supported admission extensions, and add a usable humanoid/body-profile setup in the Model Previewer. Produce an actual source-to-target FK baseline on the selected real files when supported. Supplement with full humanoid fixtures and exact constraints for H2.

Do not stop at an inventory or empty interfaces if the inputs/build permit a working feature. If genuine source support is missing, finish its coherent tested admission slice and make the blocker explicit.

### H2 — first contact-preserving native output

Implement the smallest coupled proportion/IK correction that gives a real improvement: grounded foot/sole goals with pelvis compensation, then a body-relative hand interaction with simple torso/head clearance. Preserve aerial motion without ground locks. Use the same generic profiles/services for Snake/Mario and original morphology fixtures.

Bake, reopen, measure and visually inspect. Keep source -> FK-only -> corrected comparison. Save profile/recipe/instance data and verify restart/undo/native export. Passing means an actual rendered improvement and decoded target evidence, not new controls alone.

### H3 — broaden humanoid and temporal coverage

Extend spine/clavicle/finger/toe/twist handling, general multi-goal coordination, profile reuse across projects, uncertain auto-mapping review, temporal stabilization, additional source channels/scales where supported, and existing anchor fitting. Keep motion style and explicit losses.

### H4 — non-Brawl native route

Prove Brawl Snake -> Ultimate Mario or a genuine original Ultimate target through the same retargeting pipeline; decode native output. Test reverse separately. Follow with the real Rivals Modkit asset route, Melee and Dragon Ball source/native routes, each without requiring a new user workflow.

The queue must not revert to mirroring between these checkpoints. No further restart. Do not block retargeting behind every game-image reader or unrelated Brawl format; retain those backlog requirements with exact coverage evidence.

## 11. Repository and reporting contract

Read and update `AGENTS.md`, `MASTER_PLAN.md`, `ARCHITECTURE.md`, `DEVELOPMENT_STATUS.md`, format/capability evidence, UI acceptance, and the prior library/slots/previewer specifications with an explicit dated amendment. Preserve older source requirements and evidence; remove mirror priority/dependency statements that conflict with the new user instruction.

`docs/plans/current.md` remains the sole authoritative next executable task. Give it a concrete H1/H2 acceptance rather than a generic “support all games perfectly” demand.

Use actual existing code as the starting point:

- `application/BrawlCrate/Authoring/RigTransfer.cs`
- `application/BrawlCrate/Authoring/BrawlProjectTransfer.cs`
- `application/BrawlCrate/Authoring/BrawlProjectAdapter.cs`
- `application/BrawlCrate/Authoring/CharacterProject.cs` and its related partials
- `application/BrawlCrate/Authoring/AnimationLibraryStore.cs`
- `application/BrawlCrate/Authoring/AnimationSets.cs`
- `application/BrawlCrate/Authoring/ProjectRecords.cs`
- The current `PreviewAuthoringSession`, previewer panels, `ModelForm` and native editor integration discovered in actual HEAD.

New reusable solver/profile modules should have no native-format/UI dependencies. Translate native flags, evaluation rules, coordinate systems and source bone order through adapters into explicit shared policies. Keep BrawlLib evaluation and write details in adapters. Reuse native editors and session ownership, do not copy disconnected project state into another window.

Implement, test, review and push coherent passing **source-only** updates under the repository's current standing authorization. Verify exact public origin/main, staged/outgoing content and remote SHA. No force pushes, history deletion, visibility changes, asset uploads, binaries, tags, releases or CI dispatch. Local user data and recovery remain untouched.

Report what was actually built, which source/target files were privately tested, what was observed in the native viewport, measured native/quality checks, unresolved prerequisites and the next exact acceptance. Do not claim natural motion based solely on a skeleton diagram, source-header inventory, old test counts or serialized metadata.

## 12. Research references and boundaries

Repository facts above were read through the GitHub connector at the pinned revisions. The retargeting design is a proposed implementation informed by the following primary documentation; no cited external editor is a required runtime dependency or an already integrated backend.

- [S1] Current executable task: https://github.com/DoctorFlux/fighterport-studio/blob/7458be7d9a11f95c4c4c1dd586819fd9dc40e831/docs/plans/current.md
- [S2] Current and historical development receipts: https://github.com/DoctorFlux/fighterport-studio/blob/7458be7d9a11f95c4c4c1dd586819fd9dc40e831/docs/DEVELOPMENT_STATUS.md
- [S3] Existing generic rigid FK: https://github.com/DoctorFlux/fighterport-studio/blob/7458be7d9a11f95c4c4c1dd586819fd9dc40e831/application/BrawlCrate/Authoring/RigTransfer.cs
- [S4] Native sampling/bake/decode: https://github.com/DoctorFlux/fighterport-studio/blob/7458be7d9a11f95c4c4c1dd586819fd9dc40e831/application/BrawlCrate/Authoring/BrawlProjectTransfer.cs
- [S5] Upstream legacy Port: https://github.com/soopercool101/BrawlCrate/blob/d56cbac89dc2cd0e5c3f923e1d03041b16902add/BrawlLib/SSBB/ResourceNodes/Animations/CHR0Node.cs
- [S6] Epic IK Rig Retargeting: https://dev.epicgames.com/documentation/en-us/unreal-engine/ik-rig-animation-retargeting-in-unreal-engine
- [S7] Epic IK solvers: https://dev.epicgames.com/documentation/en-us/unreal-engine/ik-rig-solvers-in-unreal-engine
- [S8] Epic retarget operation stack, including experimental relative/body-contact retargeting: https://dev.epicgames.com/documentation/unreal-engine/retargeting-operation-stack-in-unreal-engine-5-8
- [S9] Rivals II animation authoring and baking: https://rivals2.com/workshop/knowledge-base/character-creation/animation-overview/
- [S10] Epic automatic characterization/alignment: https://dev.epicgames.com/documentation/unreal-engine/auto-retargeting-in-unreal-engine
- [S11] SideFX Biped Retarget: https://www.sidefx.com/docs/houdini/nodes/sop/kinefx--biped_retarget.html

Epic documents chain-based transfer across differing bone structures and poses, multi-goal IK with preferred bends/limits, and automatic template-based characterization. Its newer relative-retarget operation is explicitly experimental and motivates preserving body-relative contact rather than copying world coordinates. SideFX describes biped transfer using full-body IK. These are design references, not proof that FighterPort already implements those systems, nor a license to copy a proprietary engine implementation. Audit any reused code/dependency separately.

This brief was authored from source inspection and public documentation. No FighterPort Windows build, local Snake/Mario decode, Modkit import, gameplay test or repository mutation was performed in this review.
