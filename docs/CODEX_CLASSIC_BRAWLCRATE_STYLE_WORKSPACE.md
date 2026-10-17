# FighterPort Studio — Classic resource editor and material-fidelity continuation

Prepared 17 October 2026. This is a continuation of the existing application, not a request for a fork or a new application. Review reference: `DoctorFlux/fighterport-studio` main commit `a50c435d85209f21c72a00b339b522701adc5e22`. Your local repository may be newer; inspect it before deciding what remains.

## 1. Execute the work, preserve the product

Continue the existing FighterPort Studio source repository. The user wants its visual layout and everyday interactions to be substantially closer to BrawlCrate, while remaining an independently developed, standalone multi-game character editor and porter.

Retain WPF, the core, native worker boundaries, source-preservation rules, project formats, tested importers/exporters, retargeting mathematics, original fixtures, and authorized GitHub workflow. Do not fork BrawlCrate, restart FighterPort, replace the application framework, or transplant the BrawlCrate application shell. Its existing BrawlLib integration remains a deliberate reusable dependency.

The reference aesthetic is a compact classic Windows editor: light/system-gray chrome, white trees and property tables, thin separators, small conventional controls, dense readable lists, and a large neutral model viewport. Do not reinterpret this as a dark dashboard with cards, oversized branding, pills, excessive padding, or a new global button for every native file format. Keep FighterPort's own name and original or appropriately licensed UI assets.

Use BrawlCrate's layout and interaction patterns as references; implement the UI in this application's own presentation layer. Record provenance and honor dependency licenses for anything actually reused. Not creating a GitHub fork is not a substitute for checking copied-code or asset licensing.

Normal application use remains local and non-AI. Blender, Maya, 3ds Max, and native tools are optional bridges, not prerequisites to basic browsing/editing. Brawl, Melee, Ultimate and Rivals 2 remain peer bidirectional targets; Dragon Ball games remain additional source adapters. The full roadmap remains required. Gameplay/moveset-script conversion stays outside the present scope.

## 2. Recover current state, then reconcile the documents

Read AGENTS.md, docs/MASTER_PLAN.md, docs/ROADMAP.md, docs/plans/current.md, docs/DEVELOPMENT_STATUS.md, docs/RESOURCE_WORKSPACE.md, docs/TEXTURE_DIAGNOSIS.md, architecture, retargeting, materials, testing, dependency and capability documents, and the preserved continuation specifications they reference. Find actual paths instead of failing on an optional missing document.

Inspect the implementation and run the available baseline checks. The reviewed checkpoint reports:

- SRT0 v4 curve/tangent/binding editing, session undo, workspace persistence and all-track native export with decoded validation.
- Brawl bitmap-row/addressing corrections, asymmetric wrapping and rendered probes.
- Individual material-sampler inspection using retained native UV arrays, not full multi-sampler composition.
- A 50-case full regression and 736 rendered sample checks; these are reported evidence to verify, not counts to fabricate or freeze permanently.
- Pending CI4 palette/alpha/mip/ambiguity conformance, a bounded native TEV color/alpha/texgen/draw-state slice, Maya rotation discrepancy, subsequent PAT0/CLR0/SHP0, richer transactions, complete native model/container outputs, cross-rig validation and other games.

Do NOT restart SRT0 export work or malformed-worker testing that has already passed. Retain those tests.

Resolve stale "next task" paragraphs in appended historical receipts: preserve history under an explicit dated heading, but give docs/plans/current.md and the beginning of DEVELOPMENT_STATUS.md one unambiguous current task list. A task must not be simultaneously described as completed and current merely because old reports were appended.

Create or update one focused UI specification, preferably docs/UI_UX_PLAN.md, with this user's classic-layout requirement, source references, UI acceptance criteria and known differences. Link it from the master plan and current checkpoint; do not proliferate another competing master specification.

## 3. Inspect the actual BrawlCrate interface before implementing

Inspect the pinned upstream source and, when locally available, the running BrawlCrate application with ORIGINAL generated fixtures. Do not infer the whole layout from the README or a cropped image alone. Record the inspected revision and make a short reference-to-implementation checklist.

Primary source locations:

- BrawlCrate/UI/MainForm.cs — resource tree, selection routing, property grid, preview/editor panels, menus and recent files.
- BrawlCrate/NodeWrappers/BaseWrapper.cs and relevant wrappers — resource-specific context actions and command availability.
- BrawlCrate/UI/Model Previewer/LeftPanel.cs — animation/object/texture lists and search/filter behavior.
- BrawlCrate/UI/Model Previewer/ModelEditControl/Designer.cs — model-editor panel arrangement, typed animation editors, menus and playback area.
- BrawlCrate/UI/Model Previewer/ModelEditControl/Animation.cs and Rendering.cs — animation and viewport interactions, not a mandate to adopt the same engine.
- Relevant BrawlLib model-viewer controls discovered from those files.

The reference application separates the archive/resource browsing workflow from its detailed model editor. Recreate those two contexts rather than squeezing every command into one crowded view.

Use local screenshots at fixed viewport dimensions where possible. Do not upload commercial game captures or assets to GitHub/model services. For externally shareable screenshots and regression baselines, use the existing original demo fixtures. If BrawlCrate cannot be launched here, record that and use inspected source plus legitimately available reference images; do not pretend to have run it.

## 4. Deliver two classic workspaces backed by one editing session

### A. Resource Browser — default opening context

Use a compact standard menu and optional small toolbar. The principal layout is:

```
File  Edit  View  Tools  Help
[small Open / Save / navigation commands]
+----------------------+--------------------------------------+
| Native resource tree | Categorized name/value property grid |
| Search/filter        | Description / validation feedback     |
|                      +--------------------------------------+
| Files and archives   | Selected resource preview             |
| Original hierarchy   | Texture / model / animation / details |
+----------------------+--------------------------------------+
| Status and bounded background-job progress                   |
+-------------------------------------------------------------+
```

- Left: native resource tree with familiar compact rows, type icons, names, expansion markers and a native-type detail/tooltip. Do not clutter every row with long conversion warnings.
- Upper-right: a real categorized property-grid-style control. Show field names, values, categories, helpful descriptions, read-only state and validation. Replace long paragraphs masquerading as an editable property inspector.
- Lower-right: contextual preview; images over checkerboard when useful, model preview, curve/animation information, or safe metadata for unsupported resource kinds.
- Preserve multiple open documents, original hierarchy and source-specific stable handles. Model and animation archives can be open simultaneously.
- A selection updates properties without deleting or switching away from the active character's animation session. Double-clicking a model opens/activates its detailed Model Editor. Provide an explicit context action as an accessible alternative.
- Open Folder and supported game-source operations live under File. Their native extraction/format availability stays capability-gated; a generic file dialog is not a new format adapter.
- Add recent sources, a bounded search/filter with visible ancestors, stable selection/expansion on refresh, and a context menu driven by selected resource capability.

Context actions should feel familiar: Preview/Edit, Export Original, Export Edited, Replace, Restore, Rename, Delete, Show Dependencies and Convert To. Implement/enable actions only for supported semantics. Unsupported writes must be unavailable with a concise reason, not succeed cosmetically. Rename/replace/delete in a working project must not imply a native archive writer exists.

Keep raw source-slice extraction explicitly distinct from a relocated, usable native export.

### B. Model Editor — detailed animation workspace

Provide a reusable view, initially docked/tabbed within the existing application; an optional separate preview window is a later increment and must reuse the same session. Do not require a large docking framework or duplicate all model state just to resemble BrawlCrate.

```
File  Edit  View  Tools    Active model: [name]
+-----------------+--------------------------+------------------+
| Animations      |                          | Bones            |
| Type + filter   | Large model viewport     | Search / tree    |
| Objects         |                          | Selected fields  |
| Materials       |                          |                  |
| Textures        |                          |                  |
+-----------------+--------------------------+------------------+
| Contextual transform / SRT / visibility / curve editor       |
+------------------------------------------------------------+
| Previous  Play/Pause  Next   Frame / Range   FPS   Loop       |
+------------------------------------------------------------+
```

- Keep compact animations, objects and textures/materials lists, active model selector, bone list and bottom editing/playback strip.
- Type filters expose actual native categories with human-readable descriptions: e.g. texture-transform animation (SRT0) when a Brawl document is selected, not fabricated extensions for other games.
- Load source clips from any open eligible document. Binding suggestions must be unambiguous and reviewable; coincidentally equal names alone are not proof of compatibility.
- Reuse existing CHR/key, VIS and SRT editing code/services. Host contextual editors inside the lower panel or a consistent editor tab. Do not duplicate exporters or silently change curve semantics.
- Retain support for simultaneous skeletal and ancillary channels, independent enable/mute state, correct reset on switching, fractional scrub, proper loop behavior and explicit time mapping.
- Do not put every animation type on the global toolbar as a new button. Show applicable categories in lists, menus or contextual editors; show unavailable evaluation clearly.
- Selection of a bone, material, UV track or visibility target drives the appropriate fields. Safe viewport bone picking/manual semantic role correction can be a following UI increment.
- View controls include frame selection/model, camera reset, grid, axes, perspective/orthographic choices and skeleton/mesh display where implemented. Keep them in View or a small viewport toolbar.

### C. Cross-game work lives in a focused Convert/Retarget workflow

Use a context action or Tools command to open the existing mapping/conversion workflow, not a conversion dashboard on the opening screen.

Select source resources -> choose destination game/profile/model -> review mapping and losses -> preview -> export a new output.

Preserve native identities while semantic roles connect actual joints, including toes, fingers, helpers, tails and additional chains. Do not rename/rebind skeletons merely to match labels. Materials, meshes, textures, visibility bindings and morphs need their own mapping policies.

No generic rig should masquerade as the required rig for all characters in a game. No skeletal-only export should claim a complete fighter. Unsupported destination writes stay blocked. Keep peer destinations visible without inventing support.

## 5. Visual and interaction requirements that can be tested

Create shared theme resources instead of scattering hard-coded colors, margins and font sizes through window constructors. Use the existing WPF styling/templating system; replacing WPF with WinForms is not required for classic visual behavior.

- Make the classic light/system theme the new default, with an explicit layout reset and migration of existing preferences. Do not silently wipe a user's saved sessions or layout. A secondary dark option is optional and lower priority.
- Use standard Windows-like typography, compact but readable line heights, thin borders, conventional checkboxes/dropdowns and 16-pixel-class icons at 100% scale. These are design targets, not a demand to hard-code physical pixels at every DPI.
- No rounded viewport card, huge title/logo banner, thick decorative margins or long permanent help paragraphs. Put help in descriptions/tooltips and detailed problems in a collapsible pane.
- Keep splitter positions and panel visibility saved in ignored user configuration. Ensure the viewport expands with available space.
- Test 1280x800 and 1920x1080 at 100% scale plus at least 150% scaling. Check truncation, text contrast, keyboard focus and tab order. Do not break high-contrast/system accessibility behavior to achieve visual similarity.
- Reuse conventional shortcuts where safe: Ctrl+O, Ctrl+S, Ctrl+Shift+S, Ctrl+Z/Y and context-menu access. Space/arrow transport shortcuts must not hijack text entry or property editing. Record differences from BrawlCrate instead of blindly copying conflicting shortcuts.
- A bare UI screenshot is not acceptance. Exercise native tree selection, property edits, native export, dirty guards and actual preview rendering through the new layout.

Introduce a small, shared command/capability layer as needed so menus, context menus, toolbar and shortcuts use the same handlers and CanExecute rules. Do not build a general plugin platform merely to rearrange panels.

Treat each accepted numeric/property edit as a validated transaction. Distinguish text-entry undo from committed resource edits; route Ctrl+Z to the active editor/session. Extend dirty-state prompts to close, open replacement, switch workspace and exit without losing existing SRT safety. Do not expose mutable backend objects directly through a property grid and bypass validation.

## 6. Keep material correctness as the next technical slice

A classic interface will not fix material-rendering semantics. Preserve all prior texture-row, address-mode and SRT regressions. Do not resurrect a global texture flip, force repeat, hard-code Mario fixes, or flatten multiple samplers into one bitmap by assumption.

The current technical queue is:

1. Original CI4 TEX0/PLT0 palette, alpha and mip conformance, including ambiguous palette selection/override persistence and safe failure. Use genuine native fixtures and independent decoded pixel expectations.
2. Inspect material data used by the locally available fixtures and Mario references, then implement the smallest justified native TEV/color-alpha/texture-coordinate/draw-state subset with independent expected rendered pixels.
3. Include multi-sampler composition where the native stages actually require it, rather than treating individual-sampler inspection as full material rendering. Preserve stages, texture/UV sources, constants, vertex colors, alpha comparisons, blend/depth/cull policies and unsupported modes explicitly.
4. Resolve or explicitly retain the Maya texture-matrix rotation-only discrepancy before enabling that behavior. Differential comparisons against the same backend are not independent proof of native semantics.
5. Continue PAT0, CLR0, SHP0 and applicable constraints, then validated native model/container outputs and the other game adapters in the existing dependency order.

BrawlCrate implementation references:
- BrawlLib/SSBB/ResourceNodes/MDL0/MDL0TextureNode.cs
- BrawlLib/SSBB/ResourceNodes/MDL0/MDL0MaterialRefNode.cs
- BrawlLib/SSBB/ResourceNodes/MDL0/MDL0MaterialNode.cs
- BrawlLib/SSBB/ResourceNodes/MDL0/MDL0ShaderNode.cs and related stage definitions
- BrawlLib/Modeling/FrameState.cs
- BrawlLib/Internal/Matrix34.cs
- BrawlLib/Wii/Graphics/ShaderGenerator.cs

Check exact paths/revision before using APIs. Sharing a dependency with BrawlCrate does not prove that FighterPort currently renders through the same shader path.

The existing WPF DiffuseMaterial path cannot be assumed to express every native shader. If verified material work needs a shader-capable renderer, isolate that backend behind the viewport interface with a small tested integration. Keep the WPF shell, project sessions and existing fallback. Do not paste OpenGL/GLSL code into WPF and assume it executes, or restart the whole renderer without a bounded justified need.

Label neutral preview, isolated-sampler inspection, supported source-material rendering and target-style approximation distinctly. A layout change is not a material-fidelity claim. The specific user-reported Mario area remains unconfirmed unless actually reproduced and compared locally; do not declare it fixed solely from original-fixture tests.

## 7. Ordered delivery with no regressions

Execute during the active run, in small tested checkpoints:

A. Baseline verification, current-doc reconciliation and a concise BrawlCrate layout comparison.
B. Shared classic theme, functional Resource Browser, typed property-grid subset and contextual preview/commands. Preserve all existing operations.
C. Reusable Model Editor layout, integrate existing animation editors, active-session command routing and dirty-state protection.
D. CI4/ambiguous-palette conformance and the justified first native material-composition subset.
E. Continue the next ready milestone from the full roadmap, including per-game channel evaluation/round trips, reviewed cross-rig transfer, Ultimate companions, Melee, Rivals Modkit, optimization, Dragon Ball sources and optional DCC bridges.

If a UI increment is blocked, do ready technical work rather than spend the entire run on decorative CSS-like edits. If material rendering is blocked, finish independent editor usability work. Do not broaden an unresolved shader slice into implementing every renderer/game at once.

Keep the complete anatomy/manual mapping, New/Load Character, game-installation/image inputs, archive browsing, materials/style conversion, keyframe optimization, source retention, native-tool handoffs and future-game requirements. Do not remove them because this instruction concentrates on the editor shell.

No game/DCC install or asset available means a particular integration check is unavailable, not permission to fabricate a pass. Original fixtures remain the portable automated test path; local commercial assets are opt-in integration inputs only.

## 8. Required evidence before claiming a checkpoint complete

- Build and regression checks on the actual updated code, with counts and failed/skipped reasons.
- Actual published desktop launch when the environment supports it; headless build is not a performed GUI run.
- Original-fixture screenshots of Resource Browser, Model Editor and a contextual animation editor, at recorded size/DPI. Inspect them against reference layout and accessibility targets. New screenshots must be of the running app, not mockups.
- Resource tree/search selection, typed property validation, model/clip/texture selection and correct stable active session.
- Existing VIS/SRT edit -> undo/redo -> save/reopen -> genuine native export/decode tests through the new UI.
- Negative tests: blocked native model/container write, unsupported shader/track, dirty cancel, missing dependency, ambiguous palette and malformed inputs.
- All source hashes unchanged; unknown/native records preserved. No commercial screenshots or derived content uploaded.
- Native-tool checks recorded only if actually performed; BrawlCrate shared-backend comparisons are not independent game correctness.

Retain original numeric/pixel tolerances; do not relax them to pass the redesign. If deterministic image tests vary with platform/DPI, document that separately and use stable regions/property assertions without concealing visible layout defects.

## 9. GitHub and delivery at every coherent tested checkpoint

Use only the existing repository `DoctorFlux/fighterport-studio`, configured main branch and expected PUBLIC visibility. Do not create a new repository/fork or change visibility.

For each completed increment: build/test -> update affected docs/capabilities -> review intended staged source/diff -> commit -> use existing scripts/push.ps1 -> verify remote SHA -> retain the local receipt. Inspect and preserve local changes before staging; avoid indiscriminate git add or destructive resets.

Do not upload game files, commercial derivatives/screenshots, local asset mappings/reports, credentials, source clones, caches, binaries or machine-specific configuration. Original UI/fixture screenshots may only be committed deliberately with clear provenance. No automatic CI dispatch, billing changes, release publication, global installations or safeguards bypass.

Network/approval failure must leave work local and report the pending commit honestly. Do not force push, manufacture a receipt, or claim the app was delivered outside accessible paths.

Publish the updated app locally through the existing scripts, test the published output where possible, and give the verified executable path and exact click sequence.

## 10. Finish with usable software and a recoverable state

Do not stop after drafting this specification, a theme-only mockup, or adding disabled controls. Continue ready acceptance criteria during the active execution, respecting its actual time/usage/permission boundaries. Do not install an endless agent loop or claim work continues after the run ends.

Before ending, update the authoritative current status/task and report:

- What changed visually and behaviorally, with actual app screenshots/evidence where available.
- What existing native features still pass and which material capabilities were newly verified.
- Which expected BrawlCrate features are deliberately not implemented yet and why.
- Actual build/test outcomes, unrun checks and remaining renderer limits.
- Verified local executable path and a short browsing/editing/export demonstration.
- Pushed commit SHA(s) and remote verification, or explicit pending-push state.
- The next single executable criterion, without stale duplicates.

Start implementation now.

## Source registry for this instruction

Checked 17 October 2026; recheck actual implementation revisions locally.

- https://github.com/DoctorFlux/fighterport-studio/commit/a50c435d85209f21c72a00b339b522701adc5e22
- https://github.com/DoctorFlux/fighterport-studio/blob/a50c435d85209f21c72a00b339b522701adc5e22/docs/plans/current.md
- https://github.com/DoctorFlux/fighterport-studio/blob/a50c435d85209f21c72a00b339b522701adc5e22/docs/DEVELOPMENT_STATUS.md
- https://github.com/DoctorFlux/fighterport-studio/blob/a50c435d85209f21c72a00b339b522701adc5e22/src/FighterPort.Desktop/Workbench.cs
- https://github.com/soopercool101/BrawlCrate/blob/master/BrawlCrate/UI/MainForm.cs
- https://github.com/soopercool101/BrawlCrate/tree/master/BrawlCrate/UI/Model%20Previewer
- https://github.com/soopercool101/BrawlCrate/blob/master/BrawlCrate/UI/Model%20Previewer/ModelEditControl/Designer.cs
- https://github.com/soopercool101/BrawlCrate/blob/master/BrawlCrate/NodeWrappers/BaseWrapper.cs
- https://github.com/soopercool101/BrawlCrate/blob/master/LICENSE
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/styles-templates-overview
