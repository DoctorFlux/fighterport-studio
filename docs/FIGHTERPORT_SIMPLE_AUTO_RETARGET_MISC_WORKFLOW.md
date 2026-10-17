# FighterPort Studio — simple source-to-character workflow
## Visible animation browsing, automatic rig setup, stable retargeting, and fixed-duration miscellaneous filling

Date: 2026-10-17
Reviewed FighterPort snapshot: `bee76e53e5e19cc719bbd9ef21a2d2e7741d3b9f`.
Reviewed Godot reference: `4.6-stable`, commit `89cea143987d564363e15d207438530651d943ac`.

This is a requirements and implementation brief, not a claim that the changes below were implemented or tested in this review. Re-read actual HEAD before working. Preserve newer passing work.

## 1. Authority and scope

The user wants the added FighterPort experience substantially simplified. Refactor or replace the added UI/orchestration where necessary, but retain the actual BrawlCrate application, native editors, working codecs, source protection, reusable library data, project compatibility, and passing regression tests. Do not perform another destructive restart or move the product into Godot merely because Godot contains useful algorithms.

Current work is models, all relevant animation channels, shared libraries, automatic characterization, IK/FK, proportion/contact fitting, animation-set slots, miscellaneous templates, and native asset output/verified Modkit asset import. Gameplay scripts, damage/hitboxes, combo execution, and complete playable-fighter compilation remain later work. Preserve legacy draft records without exposing them as mandatory steps. Mirroring is not a priority or a prerequisite.

Primary authoring destinations: Brawl/Project M/Project+, Melee, Ultimate, Rivals II. Retain Dragon Ball FighterZ, Sparking! ZERO, Xenoverse 1/2 as sources and the previously retained explicit native-target roadmap. Preserve wider Smash/source/channel/optional-DCC requirements. No mandatory Blender, Maya, 3ds Max, cloud, or runtime AI.

Update the standing specifications with this amendment. Keep `docs/plans/current.md` as the only authoritative next executable task; do not create contradictory independent queues.

## 2. Observed status versus diagnosis

The reviewed repository documents humanoid/source admission and contact-aware native output. Both local Snake/Mario FK directions pass as diagnostics; Snake-to-Mario standing correction passes its declared sampled checks. Mario-to-Snake standing correction remains rejected: 108 unreachable samples, contact residual 1.145646 native units, and native errors .368992 position / 7.881017 degrees / .417223 mesh units against .02 / .15 / .03 limits. These are repository-reported results, not new local tests performed by this review. Retain the failing case.

The user reports that animations are not visible/available through the FighterPort Model Previewer. Do not dismiss this because an automated fixture path passed.

Verified implementation details:

- `application/BrawlCrate/UI/Model Previewer/ModelEditControl/Authoring.cs`: `AuthoringLayout(true)` sets `showLeft`, `showRight`, and `showTop` to false, while keeping the bottom visible. `SetAuthoringReadOnly` disables entire left/right/control/menu surfaces, not just mutation commands.
- `application/BrawlCrate/UI/ModelForm.Authoring.cs`: automatically activates compact authoring layout; `Native details` refuses to expand when the editor is narrower than 650 pixels. It selects a document-bound CHR0 but does not provide automatic discovery of every related source animation.
- `application/BrawlCrate/UI/PreviewWorkspacePane.cs`: separates explicitly browsed shared catalogue records from registered project clips. Setup currently exposes `.fpcompanion` and `.fpcharacter` prerequisites. Native container clip discovery requires separate actions. Project clip selection and native animation selection are not one universal catalogue.

These are concrete usability/binding suspects, not a proven sole cause of the user's exact symptom. First distinguish: missing/hidden list; empty unregistered catalogue; motion container not linked; selected clip not bound; playback clock not advancing; animation changes bones but geometry remains hidden/frozen. Log and test them separately.

## 3. The normal user experience

Provide five primary actions, available from both the main resource-browser surface and the Model Previewer through the same services:

1. Add Game / Mod Folder / Disc Source.
2. Open Character / Use This Model as Target.
3. Apply Animation.
4. Fill Missing Miscellaneous Animations.
5. Save / Export.

A selected character owns a stable target identity; browsing a source never silently replaces it. Show a persistent target badge and source-versus-target audition mode. Source and library revisions are read-only but always browsable and playable.

The normal flow is: choose source once -> browse characters and their animations -> choose the target already visible in the previewer -> apply -> preview -> export. Registration, dependency manifests, cache paths, clip-revision pinning, and edit leases are implementation details. Their diagnostic UI may remain under Advanced; users should not create manifest files manually to see a clip.

Auto-create or attach the necessary authoring project after an explicit contextual action. Preserve ambiguity handling when several models, costumes, or motion packages are plausible. Do not require a fake/default source animation merely to register a target model. Model/rig registration and adding motion clips must be independently supported.

Refactor reusable controls/presenters rather than duplicate project state in multiple forms. One active editable native document remains acceptable; comparisons may be read-only. Restore selection, playhead, camera, filters, and dirty state intentionally across operations.

## 4. P0 — repair visible animation discovery and playback first

Start the ordinary built application as the user would, with a clean portable profile and with an existing saved layout. Record actual executable path/version/hash so stale binaries are not confused with the code under test.

Use the already configured local Snake and Mario model/motion inputs read-only. Inspect local configuration rather than asking for uploads or assuming filenames. Current repository records located them in narrow local character folders, not necessarily at the workspace root. Never weaken source guards or mount the whole app/repository as a game folder.

Reproduce all supported entry points:

- Open model PAC from the main browser, then its separate motion PAC.
- Open an animation container first, then select its matching source model.
- Open a saved character project directly.
- Open the Model Previewer first, then attach a source and target.
- Resume from an old layout, hide/reopen panels, and use smaller window sizes.

Implement one visible, searchable animation browser with clear scopes: opened source; selected source character; shared library; current character assignments. Unindexed and unsupported sources must show a reason and action, not an unexplained empty list.

Separate permissions: browse, select, scrub, orbit, play, compare, mutate. Read-only must restrict mutation, not exploration. Compact mode must not remove the only clip list or playback access. Keep detailed legacy controls available without forcing the user to surrender the library panel to recover animation access.

For a selected clip, show its source model/rig, native path/identity, type, duration/frame count, and whether it is source preview, target audition, or committed assignment. A project metadata change must not clear the actual selected native clip accidentally.

Test through the real controls/events, not just programmatically setting `SelectedCHR0` behind an unusable UI. Verify visible clip names, active selection, advancing playhead, changing bone transforms, and visibly changing rendered geometry at multiple frames. Test actual product restart and plain file opening. Do not treat a running timer or screenshot of an empty panel as playback proof.

Any quick layout workaround should remain temporary: the final acceptance is that the ordinary workflow works without knowledge of hidden native panel toggles.

## 5. Source catalogue and shared library

### Separate three concepts

A source mount exposes files from a dump, installation, mod directory, or supported image. A character catalogue resolves models, skeletons, materials, costumes, and associated motions inside those mounts. The shared animation library indexes reusable, source-bound clip revisions from those characters. Character projects contain independent assignments and correction recipes.

Do not call a folder-picker implementation a character catalogue. Resolve dependency graphs and provenance.

### Required source profiles

- Brawl/Melee: extracted dumps plus separately implemented, tested GameCube/Wii disc filesystem providers.
- Project M/Project+: SD-card-style folder layouts as mod/overlay sources. A mod folder may omit unchanged base assets; record missing dependencies or resolve them from an explicitly associated base dump. Never guess a complete character from directory names alone.
- Ultimate: extracted data/mod layouts and explicitly supported Switch container routes. Do not call every platform container an ISO or pass them to a Wii reader.
- FighterZ/Sparking! ZERO: game installations with title/build-specific Unreal package profiles. Similar engine ancestry is not proof of package compatibility.
- Xenoverse 1/2: separate supported native container/asset profiles.
- Rivals II: distinguish the cooked installed-game source from the editable Modkit/project source. Cooked extraction is not recovered editable project source.
- Loose custom model/animation directories remain first-class.

Use bounded streaming, 64-bit offsets, cancellation, selected-entry extraction, source-version/hash evidence, protected originals, and disk-backed cache. Avoid loading entire images or installations into the x86 UI. Background parsing must never freeze the viewport; completion must bind only to the still-current target/session generation.

Known layouts may resolve companions automatically when the supported profile has an unambiguous result. Record the choice and confidence; show alternatives only on conflict. Overrides apply to a scoped logical asset identity, not every file/bone/texture with the same name. Keep explicit user choices pinned.

Tool provisioning is app-local and profile-driven: approved sources, pinned versions, checksums/signatures where available, license records, bounded download/extraction, compatibility probes, offline cache, manual path override, cancellation, rollback. Never execute arbitrary source scripts or a remote 'latest' installer. Official game/Modkit licenses and account-dependent installs may require user interaction. Do not download game content or keys.

## 6. Automatic characterization and easy IK/FK

Define reusable versioned profiles at three levels:

1. Game/format interpretation: axes, units, matrix conventions, bind/rest/pose representation, animation timing, scale compensation, parent inheritance, coordinate conversion.
2. Skeleton characterization: stable native identities; semantic roles/chains; rest/local orientation; parent topology; fingers/toes/twist/helpers; preferred limb bends; root/pelvis policy.
3. Mesh/body fitting: limb/torso proportions, widths, sole and grip landmarks, head/body clearance proxies, attachments/costume identity.

Known bone-name aliases are useful evidence, not a sufficient certificate. Resolve exact previously verified fingerprint profiles automatically. A tested family rule may automate setup after structural, orientation, and landmark checks. Otherwise offer a proposed mapping with uncertain roles highlighted; save the review once and reuse it. Do not require repeated approval per clip. Never relabel existing diagnostic commercial proposals as reviewed automatically.

Cover head, neck, waist/pelvis, spine/chest/bust, clavicles/shoulders, upper/lower arms, hands/fingers, thighs/shins/knees, feet/toes and optional helper/appendage roles. Use chains rather than a single mandatory bone count. Native skeleton/binds and skin weights stay unchanged; a calibration pose is separate from a native bind pose and from Idle/Fall animation anchors.

Create editor-only IK/FK controls automatically from the characterized chains. Normal users operate 'Auto Rig' and 'Fit to Character'; advanced users can adjust handles, poles, contact intervals, pelvis limits, and per-clip corrections. Never export editor control bones as though they belong to the game skeleton.

## 7. H3 — retain and fix the real contact boundary

Keep the current Mario-to-Snake regression after repairing basic visibility. Do not solve it by loosening numeric tolerances or silently stretching the target.

Retarget the motion intent with FK as a preferred pose and proportion-aware goals/contacts as constraints. Create stance goals from target calibration/sole/body geometry rather than blindly carrying source-space foot positions into a different body. Track original desired goals separately from projected feasible alternatives, and disclose any contact relocation.

For a two-segment chain, the ideal unconstrained geometric reach interval is `abs(L1-L2) <= d <= L1+L2`. Joint limits, body clearance, and multiple contacts further restrict it. Both too-close and too-far goals matter. A single pelvis-Y clamp is not a general coupled body solution. Add bounded pelvis translation/orientation and, as needed, spine/clavicle participation without moving the target bind skeleton.

Use stable preferred bend planes through near-straight, folded, collinear-pole, and reach-boundary cases. A binary knee-side switch or naive interpolation of world coordinates is not acceptable. Preserve explicit intentional sharp poses and avoid hiding bad geometry through unconditional temporal smoothing.

Random-access preview and export must be deterministic. A solver may use a deterministic clip prepass, fixed traversal, cached trajectory, or reproducible warm-up, but arbitrary UI scrub order must not change the result. Cache keys include source/target/mesh/profile/recipe/algorithm versions. A previous-frame angular limiter alone is not scrub-independent.

Check contact residual, foot sliding, bend continuity, joint limits, target body clearance, style deviation, and native serialization error separately. Reopening output and comparing it to the same solver only proves serialization; retain independently authored goal/analytic tests and visible review.

## 8. Godot research and reuse decision

Use Godot as a well-documented implementation reference and possible source donor. Do not infer that any one Godot node supplies source extraction, anatomical auto-identification for Smash/Dragon Ball, coupled contact fitting, native writers, or fixed-duration miscellaneous templates.

Verified references:

- The 2022 retargeting article demonstrates `SkeletonProfileHumanoid`, `BoneMap`, source animation libraries, and target setup. It explicitly warns that matching names does not resolve different bone rests.
- The 4.6 import documentation describes name-pattern auto-mapping, rest/axis normalization, motion scaling, and optional track removal. It warns that overwriting rests can be unsuitable when original rests matter. Do not copy destructive normalization or strip accessory/material tracks from native masters silently.
- `RetargetModifier3D` is a rest-relative runtime alternative. Its documented global-pose mode requires matching bone lengths; it is not the default solution for Mario versus Snake. Pose conversion must retain source native data and reconstruct target native locals.
- `SkeletonModifier3D` supplies ordered pose processing. Treat ordering and explicit post-modifier readback as part of the design.
- `TwoBoneIK3D` offers a deterministic pole-based solver. At the pinned 4.6 source, `_process_joints` handles upper and lower length reach bounds and selects the knee/elbow circle intersection using the pole. It still needs correct target goals, degeneracy handling, coordination, and explicit residual diagnostics in FighterPort.
- Iterative solvers such as FABRIK/CCD/Jacobian have different convergence and temporal properties. Their existence is not proof of collision-aware whole-body retargeting. `BoneTwistDisperser3D` is relevant to twist distribution.
- The IK article distinguishes frame-independent solver evaluation from smoothing that can involve temporal state. Preserve deterministic clip evaluation in the offline authoring pipeline.
- `godot-cpp` is GDExtension bindings for code running with Godot; it is not a standalone IK SDK to link blindly into WinForms.
- Engine source, version-matched documentation, and reproducible demos play different roles. Proposals and issue discussions are design evidence, not shipped functionality. Do not claim every supplied Godot repository was exhaustively audited.

Recommended integration: retain FighterPort's format-neutral solver interface and BrawlCrate UI. Prototype a selected Godot-derived/reference two-bone implementation against the current analytic tests. Prefer a small, audited port where practical. A pinned headless Godot worker can serve as a differential reference or an alternative backend only after an explicit spike verifies rest/pose conversion, dependencies, exact sample readback, deployment, and results. Do not embed an entire engine/editor as the first fix for the invisible animation browser.

Any copied source must retain applicable Godot MIT copyright/permission notices and provenance. Existing BrawlCrate licensing and component notices remain in force. Audit third-party components independently. Pin exact revisions rather than tracking `master` invisibly.

## 9. Batch 'Fill Missing Miscellaneous' with no extra duration

### User contract

The user has already authored movement, attacks, specials, taunts, and win motions. Protect those assignments. The user selects a source pack (e.g. Brawl or Ultimate), target character, and references, then previews and applies all compatible missing miscellaneous motions in a small number of actions.

The feature must support category selection, donor fallback order, per-clip override, atomic apply/undo, cancellation, and clear 'ready / needs attention / unsupported / already custom' states. Never overwrite custom work by default.

### Recipe per motion, not a blanket Idle blend

Each recipe stores:

- semantic slot and validated target native binding or honest unbound status;
- immutable donor/source-rig revision;
- target rig/body profile versions;
- target entry and exit references, preferred first-frame phases where appropriate;
- fixed target timeline length/rate and looping convention;
- allowed boundary-edit margins and protected action intervals;
- contact intervals and ground/ledge/prop/pair markers;
- root-motion ownership and target-specific translation policy;
- channel mappings, dependencies, tolerances, and regeneration version.

Grounded recovery can end in Idle. Airborne recovery can end in Fall. Ledge grab normally continues into ledge hang; get-up begins in knockdown; some hit reactions must have an immediate reaction on their first frame. Do not convert all starts/ends to Idle/Fall. Paired interactions and item motions may require a prop/other-body reference. Keep these unresolved when prerequisites are absent.

### Fixed-duration boundary fitting

Perform target-space constrained curve fitting within the existing timeline. Do not append transition clips, duplicate padding frames, lengthen the animation, or silently move important timing markers. Additional key samples within existing time are permissible; they are not additional duration. A different source/target rate or required native length needs an explicit time-conversion contract before applying, not a hidden stretch.

Typical processing:

1. Decode and evaluate native source motion, preserving native evidence.
2. Map/retarget onto the target character without changing its physical proportions.
3. Resolve entry/exit references and transform/root alignment in target space.
4. Add smooth pose corrections within declared leading/trailing margins, with protected interior action and fixed duration.
5. Jointly enforce contacts/reach/clearance and endpoint constraints; do not let a last IK pass silently destroy a fitted endpoint.
6. Match endpoint pose and, where feasible/required, endpoint angular/linear velocity. Do not use componentwise Euler averaging across wrap boundaries. Preserve full-turn information and export-order semantics.
7. Bake native curves and evaluate the freshly decoded result at boundaries, subframes, loop seams, protected poses, and contact intervals.

For a deliberately chosen target entry/exit first frame, match that actual phase. Never substitute bind pose or automatically force frame zero everywhere. A fixed-duration, protected-action, exact-contact problem can be overconstrained; return a per-clip conflict for review instead of silently relaxing quality or inserting time. One baked clip cannot promise seamless matching to every arbitrary interrupted source state.

Rivals II's official guide says its main character clips play without runtime blending/dynamic IK/retargeting. Bake the fitting into supported joint/morph curves. A desktop crossfade alone is not acceptance.

### First batch acceptance

Use original fixtures for ground recovery, aerial recovery, get-up, ledge grab/hang, ledge return, and an item grip, plus intentional missing dependencies/short overconstrained clips. Fill two independent projects from one shared source pack. Assert unchanged chosen duration/frame count, protected authored slots/hashes, references/revisions, stable source evidence, per-clip results, batch undo/redo, restart, and fresh native outputs. Add real local Brawl examples separately. Non-Brawl batch output requires its own adapter evidence.

## 10. Multi-game native output remains a separate contract

One visible workflow, separate providers/evaluators/writers. Do not force foreign assets through MDL0/CHR0. Skeleton retargeting, mesh conversion, facial/material-channel mapping, native asset writing, package rebuilding, Modkit import, and gameplay behavior are separate capabilities.

After the first useful complete Brawl workflow, prove a real non-Brawl native path through the same shared core, preferably configured Ultimate or genuine original native fixtures. Melee, Dragon Ball, and actual Rivals Modkit workflows follow their own verified profiles. Do not block all retargeting or UI repairs behind every ROM reader. Keep complete scope explicit while progressing through runnable paths.

For any target template, distinguish actual native asset destination from game-runtime action binding. Exporting an extra animation does not make a new attack execute. Do not invent native writers or Modkit APIs, hard-code an engine version from chat, or claim a playable character from animation export alone.

## 11. Concrete source areas

Inspected/current UI files:

- `application/BrawlCrate/UI/Model Previewer/ModelEditControl/Authoring.cs`
- `application/BrawlCrate/UI/ModelForm.Authoring.cs`
- `application/BrawlCrate/UI/PreviewWorkspacePane.cs`
- `application/BrawlCrate/UI/PreviewAuthoringSession.cs` (directory identity verified; inspect implementation before changes)
- `application/BrawlCrate/UI/HumanoidWorkspacePanel.cs` and `HumanoidConstraintHandles.cs` (directory identities verified; inspect before changes)
- `application/BrawlCrate/UI/CharacterLibraryForm.cs`
- existing `application/BrawlCrate/Authoring/` and `Sources/` services, shared storage, animation sets, and adapters.

Do not invent a new competing project model. Extend/migrate the authoritative records and use the existing command boundary. Keep session-only undo-history policy distinct from saved actual state. Replace implementation paths only after regression coverage exists.

## 12. Delivery order and continuation instruction

P0: repair and simplify ordinary source/animation visibility, selection, and playback through both main browser and previewer.

P1: make source/target setup automatic for verified profiles and character layouts; retain explicit ambiguity and target ownership. Fold multiple technical prerequisites into one 'Add Character' action.

P2: close the existing H3 Mario-to-Snake reachable-goal/pole regression with independent quality checks and native output, using the simplified workflow. Compare a pinned Godot-derived/reference solver where useful; do not postpone the defect behind an engine migration.

P3: add fixed-duration miscellaneous recipes and the atomic batch filling workflow.

P4: deliver non-Brawl native output and additional source providers through the same UI/core. Develop approved app-local provisioning alongside actual backend integration.

These are milestones, not authorization to stop after a design document. Execute the first runnable increment now, retain failing regression receipts, and leave the exact next acceptance in current.md. Preserve larger requirements and do not restore mirror or gameplay priorities implicitly.

Run existing doctor/build/focused/native/GUI/restart/regression scripts. Add ordinary user-entry coverage rather than relying exclusively on fixture-only helper entry points. Inspect actual geometry and verify saved exports, original hashes, target binds/weights, untouched sibling records, and independent projects. Report unrun Windows/Modkit/game tests honestly.

Follow the existing standing source-only commit/push authorization after staged and outgoing review, exact PUBLIC origin/main checks, and remote SHA verification. No force pushes, restarts, repository recreation, game content, derivatives, private paths, settings, screenshots, binaries, tags, releases, or CI dispatch. Preserve recovery and licensing.

## Sources and scope of review

Repository documents and selected source at the pinned snapshot were inspected, not built or run in the user's Windows environment. The UI cause remains a source-supported hypothesis until reproduced. Godot docs/articles and selected two-bone source were inspected; no Godot solver was integrated or benchmarked in this review.

- FighterPort current task: https://github.com/DoctorFlux/fighterport-studio/blob/bee76e53e5e19cc719bbd9ef21a2d2e7741d3b9f/docs/plans/current.md
- FighterPort status: https://github.com/DoctorFlux/fighterport-studio/blob/bee76e53e5e19cc719bbd9ef21a2d2e7741d3b9f/docs/DEVELOPMENT_STATUS.md
- Compact/read-only controls: https://github.com/DoctorFlux/fighterport-studio/blob/bee76e53e5e19cc719bbd9ef21a2d2e7741d3b9f/application/BrawlCrate/UI/Model%20Previewer/ModelEditControl/Authoring.cs
- Model host: https://github.com/DoctorFlux/fighterport-studio/blob/bee76e53e5e19cc719bbd9ef21a2d2e7741d3b9f/application/BrawlCrate/UI/ModelForm.Authoring.cs
- Workspace: https://github.com/DoctorFlux/fighterport-studio/blob/bee76e53e5e19cc719bbd9ef21a2d2e7741d3b9f/application/BrawlCrate/UI/PreviewWorkspacePane.cs
- Godot 4.0 article: https://godotengine.org/article/animation-retargeting-in-godot-4-0/
- Godot 4.6 IK article: https://godotengine.org/article/inverse-kinematics-returns-to-godot-4-6/
- Versioned import retargeting documentation: https://docs.godotengine.org/en/4.6/tutorials/assets_pipeline/retargeting_3d_skeletons.html
- RetargetModifier3D: https://docs.godotengine.org/en/4.6/classes/class_retargetmodifier3d.html
- SkeletonModifier3D: https://docs.godotengine.org/en/4.6/classes/class_skeletonmodifier3d.html
- TwoBoneIK3D: https://docs.godotengine.org/en/4.6/classes/class_twoboneik3d.html
- IterateIK3D: https://docs.godotengine.org/en/4.6/classes/class_iterateik3d.html
- BoneTwistDisperser3D: https://docs.godotengine.org/en/4.6/classes/class_bonetwistdisperser3d.html
- Pinned two-bone implementation: https://github.com/godotengine/godot/blob/89cea143987d564363e15d207438530651d943ac/scene/3d/two_bone_ik_3d.cpp
- Godot license: https://godotengine.org/license/
- godot-cpp role: https://docs.godotengine.org/en/4.5/tutorials/scripting/cpp/about_godot_cpp.html
- Rivals II animation guide: https://rivals2.com/workshop/knowledge-base/character-creation/animation-overview/
- Rivals II state-specific animation requirements: https://rivals2.com/workshop/knowledge-base/miscellaneous/animation-states/
