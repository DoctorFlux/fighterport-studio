# Development status — 2026-10-16

Two tested implementation increments completed during the fresh-start run. Full requirements remain in MASTER_PLAN.md; M1/M2 are partial milestones, not blanket game support.

## Actual evidence

- Locked Release builds: core, CLI, desktop and net472 x86 native worker succeeded with zero warnings/errors. SDK 10.0.402 / local runtime 10.0.13.
- **23 automated cases passed, 0 failed** (19 core/FK, 4 native). Final test artifact directory: `artifacts/tests/20261016-163943`. This includes generated MDL0/CHR0, same native importer, geometry/binds/UVs/normals/weights, rotated alternate rig, deterministic native bytes, skinned motion, edit/save/reopen/export/decode, full-turn/loop/scale/tangent checks and invalid-input rejection. Quarter-frame scalar error 0.000012993813; model 124816 bytes, edited clip 240 bytes.
- Actual Windows GUI launch and inspected screenshots: original demo and imported native model. Both source build and published executable exercised scrub, edit, undo/redo, save/reopen. Native GUI smoke additionally exported/decoded its edited CHR0. WPF tier 2, 1280x820. `artifacts/gui-native-smoke` and `artifacts/published-gui-smoke` contain evidence.
- Packaged CLI doctor, mapping-template/approved retarget, native import/export commands executed successfully. Published apphosts ran using installed runtime 10.0.11; project-local dotnet commands use 10.0.13.
- Setup (existing local SDK path), run -Smoke, build, test, doctor and publish scripts executed. Fresh-machine installation and remote CI remain unrun.

## What works

Original Aster (53 joints, 2040 vertices), differently named/oriented/proportioned Orion and Comet tail rig. Nine clips: idle/walk/punch/crouch/jump/spin/hold/scale/tangent. Depth-buffered WPF skinned neutral/checker rendering, camera orbit/pan/zoom, skeleton markers/inspection, play/pause/scrub, scalar keys, undo/redo, New/Load/Open Demo, atomic saved projects with hash-verified original native bytes. Imported native geometry expands facepoints (5100 vertices for Aster).

Real BrawlLib MDL0 v11 fixture/read + CHR0 v4 read/edit/write subset. Deterministic fixture generation uses the inspected backend; documented x86 endian-bit fix prevents corruption. Source native files remain unchanged. Restricted FK core/CLI now supports reviewed fingerprinted profiles and preserves target mesh/binds and original projects. Complete manual desktop mapping UI is next.

## Delivery and repository

Private https://github.com/DoctorFlux/fighterport-studio created and privacy verified; first checkpoint 2192e76753182005deca7785a07552a141961d61 pushed and remote SHA verified. Follow-up source is reviewed/pushed with scripts/push.ps1; current exact commit is `git rev-parse HEAD` / `git rev-parse origin/main`. Issues #1–#3 mirror M1/M2, M3, M4/M5; local roadmap remains authoritative. Manual-only CI workflow prepared, never dispatched.

Verified executable paths relative to this repository:
- `artifacts/publish/desktop/FighterPort.Desktop.exe`
- `artifacts/publish/cli/FighterPort.Cli.exe`
- `artifacts/publish/native/FighterPort.NativeWorker.exe`

Generated native demo: `artifacts/demo-native/aster.mdl0`, `punch.chr0`, `spin.chr0`, `native.fpproj`. Load the last file in the desktop, or use Import MDL0 then Attach CHR0. Output is framework-dependent and remains local/ignored; no binary release uploaded.

## Limits / prerequisites

Native shaders/materials and undeclared channels are opaque originals, neutral preview only. No general MDL0 writer, archives, ancillary playback, automatic anatomical mapping, retarget axis correction/IK/rebinding, optimization, other game adapters or DCC/Modkit integration yet. Step clips play locally but native export rejects them. Native special scale semantics/dual keys/fractional native keys unsupported. Worker timeout/crash isolation is not a hostile-parser security sandbox; fuzzing and full resource quotas remain.

No BrawlCrate native-tool UI, retail/game assets, in-game check, Modkit/cook or remote CI has run. BrawlCrate and BrawlLib share code. Doctor found Blender and Maya 2022/3ds Max 2023 registry entries; this does not prove executable/license/bridge availability, and no DCC was launched. Corresponding native backend source, local patch and notices accompany the local package; final OpenTK/binary redistribution audit remains before any external release. None blocks the next local task in docs/plans/current.md.
