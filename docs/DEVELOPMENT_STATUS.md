# Development status — 2026-10-16

Current tested checkpoint: **M3.A2 quaternion/rest-parent transfer subset**, followed by **M1.E / M1.C2 / M3.C1 / M3.D1 initial unified resource workspace**. Existing WPF/native workers/tests are preserved. Full requirements, IDs and multi-game scope remain in MASTER_PLAN.md, ROADMAP.md and the unified continuation specification.

## Repository

Only https://github.com/DoctorFlux/fighterport-studio, configured origin/main. **PUBLIC**, explicitly authorized by the user; historical private-only instructions are superseded. No visibility mutation. scripts/push.ps1 checks live identity/visibility and actual remote SHA, then writes an ignored .local/checkpoints receipt. Reviewed source only; no game/derived content, local configuration, dependencies or binaries committed. CI remains unrun/manual-only.

M3.A2 checkpoint **fb386fdb19ab1250c637c48b14a04dea77a16518** was pushed and remote-verified this run. The subsequent workspace checkpoint is identified by Git HEAD and its local receipt after review/push. A local commit alone is not proof of upload.

## Local application

- Desktop: `D:/Users/ChatGPT/ROA2TOOL/artifacts/publish/desktop/FighterPort.Desktop.exe`
- CLI: `artifacts/publish/cli/FighterPort.Cli.exe`
- Worker: `artifacts/publish/native/FighterPort.NativeWorker.exe`
- Build/run: `./scripts/build.ps1 -Native`, `./scripts/run.ps1`, `./scripts/publish.ps1`.

[Exact clicks and limits](RESOURCE_WORKSPACE.md): Open FitMario00.pac, activate Model Data [0] > 3DModels(NW4R) > FitMario00. Open FitMarioMotionEtc.pac, expand FitMarioMotion > Animation Data [0], activate Wait2 CHR0 and then Wait2 SRT0. Play/scrub uses one timeline; save/reopen FPWORK. Main/shadow remain separate. Nine texture references resolve; five sampler-0 textures display. Four eye UV tracks evaluate; eight additional-sampler tracks remain retained and unevaluated. Main Mario motion has no VIS0; genuine original fixtures exercise VIS0, SRT0 and CHR0 together.

Menus, four primary toolbar commands, lazy native tree, resizable viewport/properties, context actions and scalar key undo/redo reuse WPF. Tools > Map / Retarget retains manual/semantic mapping. Convert lists four peer destinations, blocks full character output, and requires explicit skeletal-only loss acceptance for existing measured native CHR0/NUANMB writers. Raw extraction is an unrelocated byte slice, not a portable native exporter.

## Verification

- Baseline build/native succeeded; **38 passed**, `artifacts/tests/20261016-183139`. Completed native-failure work was retained, not reimplemented.
- M3.A2: **30 portable cases passed**, including three new independent quaternion/changed-axis/parent, translation-policy, exact-rest and immutable source/save cases. Published mapping smoke: `artifacts/m3a2-published-smoke`. Explicit UnitOnly, CopyDelta, RestParentDelta and TargetRest policies; legacy v1 layers reopen.
- Full regression: **44 passed, 0 failed**, `artifacts/tests/20261016-185841`: existing worker/native/SSBH/bake/local cases and new resource/UV cases. All **688 original input hashes unchanged**.
- Workspace tests: genuine ARC/MDL0/TEX0/CHR0/VIS0/SRT0 fixtures; scoped duplicate-name texture banks and ambiguous eligible-bank rejection; decoded texture pixels checked in WPF; VIS0 fractional boundary/reverse/loop/mute/default checks versus native ApplyVIS; original and Mario Wait2 UVs versus ApplySRT within **0.00001**; independent analytic UV expectation; unsupported rotation retained; save/reopen, persisted texture override, changed-source and overwrite rejection.
- Published apphost smokes: `artifacts/workspace-original` and `artifacts/workspace-mario`. Actual tree expansion, model/textures/clip/groups, scrub, save/reopen and capture. Original capture visually reviewed; commercial captures remained local and were not sent to an image/model service.
- Final targeted tests and script-driven published verification are recorded below. No native-tool GUI or game fidelity claim.

Retained earlier results: Brawl main MDL0 v9, 63 bones/14 meshes/10,252 facepoint vertices; shadow 18 bones. Skeletal matrices versus BrawlLib error 1.9073486e-06; rest skin error 1.067905e-06 cm. Ultimate c00, 98 bones/62 objects/24,098 vertices; a00wait1 has 78 quaternion tracks, exact rests and rigid/blended skin; rest error 1.06950065e-05 cm. Eleven visibility/two material nodes remain opaque. Genuine original SSBH fixtures and CHR0-to-NUANMB/reverse measured bake/write/decode still pass; these are same-rig conversions, not Mario retarget proof.

## Next executable task and limits

Next **M3.C/D ancillary fidelity/editing**: original indexed palette, alpha, wrap/mip and additional-UV fixtures; verified rotating SRT matrices and sampler composition; explicit ancillary binding edits/undo; genuine VIS0/SRT0 writers with decoded validation. Then PAT0/CLR0/SHP0/constraints and full native model/package round trips. Complete M1.E search/recent/dirty-state, viewport semantic picking, conservative companion assembly and source relinking. Keep unsupported modes visible/preserved.

Continue M3.A2 ambiguity/helper refinements, human-reviewed exact-fingerprint Mario maps, both target-rig transfer directions, measured bake/native decode. No guessed commercial map is approved. Proportions, twist, scale compensation, IK/contact and continuous-error proof remain incomplete.

Keep Rivals Modkit/interchange M4.A, Melee M5.A, optimization M3.B, Dragon Ball sources M5.C, equal optional Blender/Maya/3ds Max M4.B, discovery/legacy M0.C/M5.D/E and every master requirement. All four full-character writers/packages remain required.

Appearance is approximate: sampler 0/UV0 decoded color/alpha, nonrotating Maya SRT scale/translation, VIS0. No full TEV, alpha-test, extra samplers, other color/texture/shape/constraint evaluation or native edited model/container. PLT0/mip support is not proved by TEX0 tests. Ultimate companion/material/ancillary work remains pending. FPWORK needs unchanged originals at existing paths; relinking/portable bundles are pending. Undo covers scalar keys, not all workspace commands; dirty prompts and richer group mute/solo/rate controls remain incomplete.

Bounds: 256 documents, 32 MiB per source/save/worker response, 32 MiB decoded cache, 4-megapixel texture decode, 45-second supervised native jobs. Legacy FPPROJ/Ultimate retention still has 8 MiB dependency bounds. This is not an OS parser sandbox. Folder indexing is immediate-folder/256-entry bounded; no all-disk scan.

Framework-dependent publish needs installed .NET 10 Desktop and .NET Framework 4.7.2+. Local SDK 10.0.402 uses runtime 10.0.13; apphost observed on 10.0.11. Ultimate uses workspace Python 3.10/ssbh_data_py 0.9.1/NumPy 2.2.6, not a relocatable bundled backend. Fresh-machine, BrawlCrate/SSBH Editor GUI, Modkit/editor/cook, DCC and game checks remain unrun. No binary release uploaded.


## Final checkpoint verification

Final publish completed with zero build warnings/errors. Changed workspace tests passed **3/3**, `artifacts/tests/20261016-190656`, including ambiguous eligible texture banks and reused standalone native preflight. Script-driven **published apphost** smokes passed at `artifacts/gui-workspace-workspace-model` (native tree, textured model, CHR0/VIS0/SRT0, source checks, save/reopen and skeletal-only native CHR0 bake/write/decode) and `artifacts/gui-workspace-FitMario00` (actual Mario textures, Wait2 skeletal/UV groups, save/reopen). Existing published key-edit/undo/redo, mapping and Ultimate playback smokes passed at `artifacts/gui-smoke`, `artifacts/gui-mapping-smoke`, `artifacts/gui-playback-ultimate`. No new capability is inferred from native-tool/in-game checks, which remain unrun.

Final review follow-up: duplicate bound UV targets now reject before rendering; original workspace tests **2/2 passed** at `artifacts/tests/20261016-191030`, and the published original workflow passed again. Reviewed 25 staged source/test/script/document files; staged path, whitespace, UTF-8 and credential-pattern checks passed. Commercial assets and all build/test artifacts remain excluded.
