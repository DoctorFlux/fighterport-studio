# Development status — 2026-10-16

Continuation checkpoint: M1.D2 desktop manual mapping and M2.C1 decoded-export validation implemented and tested. Full requirements remain in MASTER_PLAN.md; milestones remain partial, with no blanket game support.

## Actual evidence

- Locked Release builds: core, CLI, desktop and net472 x86 native worker succeeded with zero warnings/errors. SDK 10.0.402 / local runtime 10.0.13.
- **27 automated cases passed, 0 failed** (23 core including restricted FK/session/export validator; four native). Artifact directory: `artifacts/tests/20261016-165712`. Existing 23 tests retained; three mapping session cases and one encoded validator case added. Measured native quarter-frame scalar error 0.000012993813; original MDL0 124816 bytes, edited clip 240 bytes.
- Actual Windows mapping GUI launch, screenshot inspection and assertions passed in both source and published builds: `artifacts/gui-mapping-smoke` and `artifacts/published-mapping-smoke`. Assign/unassign, approval revocation, approved-profile save/reopen, missing/stale/unapproved/rotated-root rejection, skinned target preview, new target project and unchanged input snapshots verified. Viewport aspect framing corrected after screenshot review.
- Native GUI load/scrub/edit/undo/redo/save/reopen/export/decode rechecked with the new validation: `artifacts/gui-native-smoke`. Earlier original and published native GUI evidence remains. Published CLI doctor passes; installed runtime 10.0.11 runs apphosts.
- Optional ssbh_data_py 0.9.1 / NumPy 2.2.6 generated a genuine original two-bone SKEL 1.0 and read it back with names/parent/local/world checks plus corrupt-input rejection: `artifacts/ssbh-test`. Original MDL0 also passed the new skeleton inspector.
- Supplied Brawl PAC read through pinned BrawlLib: main MDL0 63 bones, shadow 18. Ultimate c00 SKEL read through ssbh: 98 bones. Reports and full input hash manifest stay in ignored `.local/references`. **All 678 supplied files verified unchanged.** These are reference skeleton reads only, not full commercial mesh/animation or native compatibility tests.
- Capability JSON/Markdown consistency checked. Fresh-machine installation, remote CI, native-tool GUI, Modkit/cook and in-game checks remain unrun.

## What works

Original Aster (53 joints, 2040 vertices), differently named/oriented/proportioned Orion and Comet tail rig; nine motion/edge-case clips. Depth-buffered WPF skinned rendering, orbit/pan/zoom, skeleton markers/inspection, play/pause/scrub, scalar keys, undo/redo, New/Load/Demo, atomic project save with hash-verified original native bytes. Imported Aster geometry expands to 5100 facepoint vertices.

Desktop **Map / Retarget** now loads source/target projects, assigns/unassigns bones, resets suggestions, saves/loads fingerprinted approved profiles and compares skinned motion. Changes revoke approval. Restricted FK requires matching rest axes/hierarchy/unit rest scales; root translation is explicit. Transfer snapshots inputs and saves a new target/fk project without overwriting an existing file. Orion's rotated root fails clearly. No automatic anatomical mapping or general rest-axis correction yet.

Real BrawlLib MDL0 v11 fixture/read and CHR0 v4 read/edit/write subset. Every CHR0 export decodes its encoded output and checks timing/loop, keys and quarter-frame native samples before atomic create-new publication. Absolute tolerances: scale .001, rotation .05 degrees, translation .01 cm. One-million-sample quota; no silent tolerance relaxation. Fixed-grid scalar checks and shared-library decode are not continuous/world-space or independent native-tool validation.

## Delivery and repository

Private repository: https://github.com/DoctorFlux/fighterport-studio. Existing exact origin in config/repository.json; reverified before each push. Prior reviewed checkpoint 1825e7541cda0be5b1ee92f004b9bcbfe615134c; this continuation uses the same staged-review/private-remote/SHA-verified push process. Resolve current commit with `git rev-parse HEAD` and `git rev-parse origin/main`; the final response reports the pushed SHA. Issues #1–#3 mirror the full roadmap; local files remain authoritative. Manual CI workflow never dispatched.

Verified paths relative to this workspace:

- `artifacts/publish/desktop/FighterPort.Desktop.exe`
- `artifacts/publish/cli/FighterPort.Cli.exe`
- `artifacts/publish/native/FighterPort.NativeWorker.exe`

Build: `./scripts/build.ps1 -Native`; test: `./scripts/test.ps1 -Native`; run: `./scripts/run.ps1`; mapping smoke: `./scripts/gui-smoke.ps1 -Mapping`; local package: `./scripts/publish.ps1`.

Native demo remains `artifacts/demo-native/aster.mdl0`, `punch.chr0`, `spin.chr0`, `native.fpproj`. Load the project, or Import MDL0 then Attach CHR0. Package is framework-dependent and local/ignored; no binary release or game assets uploaded.

## Limits / next task

Native materials and undeclared channels stay opaque originals with neutral preview. No general MDL0 rewrite, container editing, ancillary playback, full Ultimate mesh/animation adapter, automatic mapping, rest-axis correction/IK/rebinding, optimization, DCC bridge or Rivals Modkit integration. Step clips play locally but native export rejects them; special scale semantics/dual keys/fractional native keys remain unsupported. Optional skeleton readers do not implement full canonical import or native model export.

Next is **M2.C2 malformed-native regression and worker failure/publication-boundary hardening**, detailed in plans/current.md. Timeout/crash isolation is not a hostile-parser sandbox; fuller fuzzing/resource limits and continuous pose-error validation remain. BrawlCrate and BrawlLib share code. Blender/Maya/Max registry findings do not prove working bridges/licenses; none launched. Local package includes corresponding backend source/patch/notices; final OpenTK/binary redistribution audit remains before any external release. None blocks the next local criterion.
