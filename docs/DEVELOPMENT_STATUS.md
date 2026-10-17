# Development status — 2026-10-17

Classic Resource Browser and Model Editor are implemented and verified for the declared UI subset. CI4/palette conformance and a static native color/alpha reference surface also passed. Next: integrate the declared material subset into the skinned-model rendering backend. See [UI acceptance plan](UI_UX_PLAN.md) and [one authoritative task queue](plans/current.md). The dated receipts below are history, not current-task instructions.

Completed and retained: CHR0 editing/export, VIS0 v3 and all-track SRT0 v4 editing/undo/save/native export/decode, bitmap-row/address-mode fixes, individual direct sampler/UV inspection, retargeting subsets and the full multi-game roadmap. SRT0 export is not a pending implementation task. Baseline this run: native build succeeded, focused workspace **8 passed / 0 failed**, `artifacts/tests/20261017-013935` (original fixtures, no commercial inputs requested in that baseline). Updated full run: **49 passed / 1 failed**, `artifacts/tests/20261017-014719`; the failure is the unchanged-source manifest gate because 10 recorded extracted MDL0/PNG entries are absent. PAC and Ultimate integration and Mario all-track export still passed. Audit: **678 existing files match, zero changed, 10 missing**. The baseline manifest is preserved; no all-688-unchanged claim is made this run. Portable/native/SSBH run: **46/46 passed**, `artifacts/tests/20261017-014904`. Published 736 rendered samples passed with the original native asymmetric texture.

Repository remains only **PUBLIC** https://github.com/DoctorFlux/fighterport-studio on main. Prior verified HEAD: `a50c435d85209f21c72a00b339b522701adc5e22`. All source pushes require staged review and scripts/push.ps1 live remote/visibility/SHA verification. Current classic executable: `D:/Users/ChatGPT/ROA2TOOL/artifacts/publish-classic/desktop/FighterPort.Desktop.exe`. Default publish was locked by an open application; the tested build is published alongside it.

Remaining model renderer limits: native TEV composition, nearest/LOD, alpha-test/cull/depth, indirect/projected texgen and Maya rotation discrepancy; PAT0/CLR0/SHP0 and full native model/container outputs remain pending. The separate static reference surface does not close those 3D limits. Individual sampler inspection does not establish material fidelity. Brawl/Melee/Ultimate/Rivals 2 remain peer bidirectional targets; Dragon Ball source adapters and equal optional Blender/Maya/Max bridges remain required. Native-tool/game and remote CI checks are unrun.

Classic UI evidence: published original smoke at `artifacts/classic-smoke-20261017-015053`; actual screenshots inspected at 1280×800/1920×1080, 96 monitor DPI plus separately labelled 150% WPF scaling stress. Categorized fields, search/recent sources, model lists, shared session, embedded SRT/VIS services, dirty Cancel, property rejection/undo and export/decode/save/reopen pass. Full 150% OS-DPI, external BrawlCrate and in-game checks remain unrun. Current UI scope/known differences: UI_UX_PLAN.md.


CI4 checkpoint: **10/10 focused cases**, `artifacts/tests/20261017-015438`, including two new conformance cases. Native CI4 32×16 / RGB5A3, two mips, independently expected opaque/partial (109)/zero alpha; real same-name palettes in separate dictionaries, automatic ambiguity/missing-palette rejection, explicit handle decode and persistent override. Published classic smoke `artifacts/classic-smoke-20261017-015606` additionally selected mip 1 and alternate palette in the actual texture window, checked dimensions/color/alpha, applied override and saved. Genuine native fixture only; no general native texture writer claim. Classic checkpoint **16c8e26b6fa048cf2af8b2d63f86729e060d29fb** is pushed and remote-verified.

---

## Static material reference checkpoint — 2026-10-17

Native import retains active/all TEV stages, raw color/alpha/indirect words, separate texture-map/coordinate-generator indices, registers/constants, alpha-test/blend and depth/cull metadata. Unsupported programs remain preserved and the reference action is disabled with a reason. Existing native source links remain authoritative; there is no edited material/model writer.

A contextual material action renders a bounded static CPU color/alpha plane: direct UV0, supported static matrices, independently filtered nearest/linear samplers, add/subtract/bias/scale/clamped stages, constants, alpha compare and opaque/standard source-alpha output. Minification/LOD, raster lighting, indirect/projected texgen, nonidentity swaps, signed/unclamped/compare stages and animated materials reject. Normalized floating-point math is not a hardware precision guarantee. The 3D model viewport remains approximate.

Genuine original fixture: two RGBA8 textures, three native TEV stages and a color/alpha constant. Independent analytic expectations cover 36 positions/four channels, including filter seams; nearest, arithmetic, all eight alpha comparisons, unsupported preservation and FPWORK retention are checked. Actual WPF reference image has four independently expected premultiplied pixel checks. Full portable/native/SSBH run **49/49 passed**, `artifacts/tests/20261017-020519`. Final focused **11/11 passed**, `artifacts/tests/20261017-020904`, including transformed-minification rejection. Final publish succeeded with zero warnings/errors. Published classic smoke **passed**, `artifacts/classic-smoke-20261017-020959`, including CI4 and four material reference pixels; actual original captures inspected. Published sampling **736/736 passed**, max channel error 3/255. Previous pushed checkpoints: classic **16c8e26b6fa048cf2af8b2d63f86729e060d29fb**, CI4 **9da3dc01c65ad16ccdf1bac604c05fa9132b55d2**, both remote-verified. The reviewed material commit is identified by Git HEAD and its ignored verified push receipt.

# Historical receipts — 2026-10-16

Current tested checkpoint: **M3.C1 / M3.D1 Brawl bitmap-row/addressing correction and all-track SRT0 v4 editing/native export**, following the retained M3.A2 and unified resource workspace subsets. Existing WPF/native workers/tests are preserved. Full requirements, IDs and multi-game scope remain in MASTER_PLAN.md, ROADMAP.md and the unified continuation specification.

## Repository

Only https://github.com/DoctorFlux/fighterport-studio, configured origin/main. **PUBLIC**, explicitly authorized by the user; historical private-only instructions are superseded. No visibility mutation. scripts/push.ps1 checks live identity/visibility and actual remote SHA, then writes an ignored .local/checkpoints receipt. Reviewed source only; no game/derived content, local configuration, dependencies or binaries committed. CI remains unrun/manual-only.

M3.A2 checkpoint **fb386fdb19ab1250c637c48b14a04dea77a16518** was pushed and remote-verified this run. Unified workspace checkpoint **73a0c5492eb6bb0bfb7ba62ecada868e43f9fc6c** was also remote-verified. The current ancillary checkpoint is identified by Git HEAD and its local receipt after review/push. A local commit alone is not proof of upload.

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

Next **M3.C/D material fidelity**: CI4/palette ambiguity, a declared native TEV/color-alpha subset and actual multi-sampler composition. SRT0 v4 curve/binding edits/undo and all-track decoded standalone export are tested; asymmetric rendered wrapping and distinct UV1 retention now have regressions. VIS0 v3 editing/export and CI8/RGB5A3 alpha/two-mip fixtures are now tested. XSI/3dsMax rotation is tested; Maya rotation-only backend discrepancy remains unresolved. Then PAT0/CLR0/SHP0/constraints and full native model/package round trips. Complete M1.E search/recent/dirty-state, viewport semantic picking, conservative companion assembly and source relinking. Keep unsupported modes visible/preserved.

Continue M3.A2 ambiguity/helper refinements, human-reviewed exact-fingerprint Mario maps, both target-rig transfer directions, measured bake/native decode. No guessed commercial map is approved. Proportions, twist, scale compensation, IK/contact and continuous-error proof remain incomplete.

Keep Rivals Modkit/interchange M4.A, Melee M5.A, optimization M3.B, Dragon Ball sources M5.C, equal optional Blender/Maya/3ds Max M4.B, discovery/legacy M0.C/M5.D/E and every master requirement. All four full-character writers/packages remain required.

Appearance is approximate: one selected direct sampler/native UV decoded color/alpha, Maya ST and XSI/3dsMax SRT, VIS0. No full TEV composition, alpha-test, other color/texture/shape/constraint evaluation or native edited model/container. CI8/RGB5A3 palettes/alpha and mip 0/1 have independent original pixel checks; other palette/format/wrap coverage remains incomplete. Ultimate companion/material/ancillary work remains pending. FPWORK needs unchanged originals at existing paths; relinking/portable bundles are pending. Undo covers scalar keys, VIS and SRT0 edits, not all workspace commands; SRT0 dirty prompts exist, while broader transactions and group mute/solo/rate controls remain incomplete.

Bounds: 256 documents, 32 MiB per source/save/worker response, 32 MiB decoded cache, 4-megapixel texture decode, 45-second supervised native jobs. Legacy FPPROJ/Ultimate retention still has 8 MiB dependency bounds. This is not an OS parser sandbox. Folder indexing is immediate-folder/256-entry bounded; no all-disk scan.

Framework-dependent publish needs installed .NET 10 Desktop and .NET Framework 4.7.2+. Local SDK 10.0.402 uses runtime 10.0.13; apphost observed on 10.0.11. Ultimate uses workspace Python 3.10/ssbh_data_py 0.9.1/NumPy 2.2.6, not a relocatable bundled backend. Fresh-machine, BrawlCrate/SSBH Editor GUI, Modkit/editor/cook, DCC and game checks remain unrun. No binary release uploaded.


## Final checkpoint verification

Final publish completed with zero build warnings/errors. Changed workspace tests passed **3/3**, `artifacts/tests/20261016-190656`, including ambiguous eligible texture banks and reused standalone native preflight. Script-driven **published apphost** smokes passed at `artifacts/gui-workspace-workspace-model` (native tree, textured model, CHR0/VIS0/SRT0, source checks, save/reopen and skeletal-only native CHR0 bake/write/decode) and `artifacts/gui-workspace-FitMario00` (actual Mario textures, Wait2 skeletal/UV groups, save/reopen). Existing published key-edit/undo/redo, mapping and Ultimate playback smokes passed at `artifacts/gui-smoke`, `artifacts/gui-mapping-smoke`, `artifacts/gui-playback-ultimate`. No new capability is inferred from native-tool/in-game checks, which remain unrun.

Final review follow-up: duplicate bound UV targets now reject before rendering; original workspace tests **2/2 passed** at `artifacts/tests/20261016-191030`, and the published original workflow passed again. Reviewed 25 staged source/test/script/document files; staged path, whitespace, UTF-8 and credential-pattern checks passed. Commercial assets and all build/test artifacts remain excluded.


## Palette / UV / ancillary checkpoint — 2026-10-16

- Build succeeds with zero warnings/errors. Full regression **47 passed / 0 failed**, `artifacts/tests/20261016-192451`; all **688 supplied source hashes unchanged**. Existing tests retained, including native failure regression; no reimplementation of that milestone.
- CI8 TEX0 + RGB5A3 PLT0 are generated through the pinned real encoder. Independent red/green/blue/transparent quadrant checks cover 32×32 mip 0 and 16×16 mip 1, plus a yellow alternate palette. The texture viewer exposes native metadata, mip/palette choice, decoded dimensions, sampler/UV/wrap usages and persisted palette overrides. Viewport stays on mip 0.
- XSI/3dsMax UV SRT: 32 native matrix combinations, independent 90-degree expectations, and genuine rotating SRT0 v4 fractional ApplySRT comparisons (tolerance 0.00001 UV). Maya rotation remains unsupported because its pinned rotation-only branch differs from its general formula. Additional samplers/UVs remain unevaluated.
- VIS0 v3 working-copy sample and explicit target-binding edits, separate bounded undo/redo, FPWORK persistence, genuine native standalone export and decoded metadata/target/constant/sample validation. Original-path metadata is preserved from the verified source, including older workspace saves. Constant expansion/undo and duplicate binding rejection are tested. Source overwrite, existing destination and unencoded timing overrides reject. v4 user data, opaque metadata beyond this subset, native container repacking and native-tool/game checks are not claimed.
- First published original workflow passed with actual visibility editor buttons, undo/redo, native export, rotating UV capture and texture-window mip/palette/alpha checks. Only original screenshots were viewed; commercial captures remain local. Final targeted/source-metadata and publish checks follow in the checkpoint record below.

At that historical checkpoint the pending task was typed SRT0 v4 key/tangent and binding edits/undo, a genuine native writer and decoded fractional validation without dropping the currently unevaluated direct/indirect sampler tracks. Keep all four peer character destinations and the entire multi-game roadmap.


Final ancillary checkpoint verification: **6/6 focused cases passed**, `artifacts/tests/20261016-192803`, including original-path metadata, constant expansion/undo, duplicate-binding rejection and opt-in Mario. Final publish completed with **zero warnings/errors**. Published original and Mario workspace smokes passed at `artifacts/gui-workspace-workspace-model` and `artifacts/gui-workspace-FitMario00`; original uses the newly generated fixtures in the targeted test directory. Packaged CLI doctor reports runtime 10.0.11, Windows true, native worker true. Source review/push uses the exact PUBLIC repository workflow; actual commit identity is recorded in Git and the ignored verified receipt. No commercial assets, native outputs, local settings or build artifacts are staged.


## Texture / SRT0 checkpoint — 2026-10-16

See [diagnosis and exact scope](TEXTURE_DIAGNOSIS.md). Full suite **49/49**, `artifacts/tests/20261016-200030`; zero build warnings/errors, all **688 supplied input hashes unchanged**. Native asymmetric RGBA8 fixture and WPF DiffuseMaterial: **736 rendered samples**, independent address modes/alpha/negative scales/fractional loop/reset/two materials, max **3/255**. Distinct original UV1 survives genuine native generation/import. Additional UV arrays are retained but not yet composed in preview.

SRT0 v4 contextual set/move/delete/value/tangent/binding editor, persistent 16-step session undo, FPWORK saving and SRT0 dirty prompts. Native working-copy export retains every track and empty material; exact decoded metadata/keys/tangents and fractional probes precede atomic create-new publication. Worker failure, corruption, cancellation, dropped tracks and destination races reject without output/state loss. Original fixture includes direct 0/1/7 and indirect 2. No native-tool/game or container export claim.

Local published executable remains `D:/Users/ChatGPT/ROA2TOOL/artifacts/publish/desktop/FighterPort.Desktop.exe`. Published sampling **736/736** passed. Published original and Mario workspaces passed at `artifacts/gui-workspace-workspace-model` and `artifacts/gui-workspace-FitMario00`; original smoke exercised the SRT0 editor and native export. Mario local captures include rest, CHR0-only and CHR0+SRT0. The reviewed commit and verified public push receipt identify this checkpoint; source-only public repo policy remains unchanged. Remaining: precise user-reported Mario area not visually confirmed; Maya rotation discrepancy, nearest/LOD, eye TEV/multisampler, alpha-test/cull/depth, PAT0/CLR0/SHP0 and full character writers. Continue the full retained multi-game queue.


## Individual sampler inspection checkpoint — 2026-10-16

Texture/SRT0 checkpoint **8b85515a2a119b1baef7e528f12167a714287549** was pushed and its SHA verified against the live PUBLIC remote. The next increment adds the material-tree context action **Inspect material / individual sampler…**, expandable properties and saved per-material sampler selection. It uses that sampler's native UV array and static/animated transform, wrap modes and binding. Min/mag filtering, LOD bias, projection and texgen are preserved/reported; unsupported STQ/emboss texgen or missing UVs produce neutral fallback/problems. This is isolated-layer inspection, not native material composition.

Focused tests **9/9**, `artifacts/tests/20261016-200811`, including genuine native slot-1/UV1 Mirror/Clamp, native fractional matrix comparisons, distinct UV arrays, save/reopen and rejection of missing UV/projected/embossed texgen. The 736 rendered tests now submit distinct UV0 and UV1 through the same MeshCoordinates path as the viewport. Mario Wait2 standalone SRT0 export/decode preserves all **12 tracks**; derived output remains ignored/local. Published original/Mario workflows and pixel probes passed; the original smoke applies sampler 1 using the actual inspector button and verifies SRT0 undo/redo remains available after closing its editor. Original capture visually reviewed; no commercial capture sent externally.

Next executable task: original CI4 palette/alpha/mip conformance and ambiguous-palette override tests, followed by the smallest justified native TEV color/alpha/texgen/draw-state slice with independent pixels. Resolve Maya rotation-only semantics before enabling it. PAT0/CLR0/SHP0, broader editor transactions, full native model/container export, reviewed cross-rig transfer and all retained game/DCC milestones remain required. Final full regression: **50 passed, 0 failed**, `artifacts/tests/20261016-200949`; all **688 source hashes unchanged**. Published original/Mario resource workflows and **736 rendered samples** passed; build/publish has zero warnings/errors. Exact final commit is identified by Git HEAD and its verified local push receipt.
