# Current verification — 2026-10-17

Classic UI: `scripts/classic-smoke.ps1 -Published -PublishRoot artifacts/publish-classic -Fixtures <original-test-output>` launches the actual application, captures both views and embedded editors, checks shared state/search, typed DataGrid validation, routed keyboard transport/text focus, undo/redo, dirty Cancel logic, save/reopen and genuine SRT/VIS export/decode. Two window sizes plus explicit 150% WPF scaling stress; actual monitor 96 DPI. Real 150% OS-DPI and external native-tool/game checks remain unrun. Published 736 original rendered-pixel probes pass.

Full local-assets run currently reports **49 pass / 1 failure** (`artifacts/tests/20261017-014719`): missing baseline `Brawl mario/FitMario00.mdl0` prevents the all-source-integrity gate. Do not alter that manifest or describe the gate as passed. Portable/native/SSBH checks passed **46/46**, `artifacts/tests/20261017-014904`. Manifest audit reports **678 existing unchanged, zero changed, 10 missing**. The following receipts are historical and their counts/next-task wording do not supersede current.md.

---

# Testing and evidence

## Current continuation evidence — supersedes historical counts/limitations above

Final core/native/SSBH/local suite: **38 passed, 0 failed**, artifacts/tests/20261016-181042; 35 portable/original cases plus three opt-in commercial-local cases. Command: ./scripts/test.ps1 -Native -Ultimate -LocalAssets. Existing 27 cases retained. Optional Ultimate tests require local Python 3.10 and pinned packages; LocalAssets reads ignored configuration/reports and never belongs in public CI.

Added: worker crash/deadline/cancel/log/output/protocol/missing-response tests; 12 deterministic malformed MDL0/CHR0 cases; export validation/cancel/crash/destination-race atomicity. Semantic ambiguity/correction/stale/missing-toe/tail cases; independent changed-axis/parent rest-relative poses and asymmetric rig skinning. Original genuine SSBH writer-generated model/animation, independent rigid/blended fractional poses, exact-bind/attribute negative cases, NUANMB export/decode. Original same-rig CHR0↔NUANMB measured bakes, excessive-error rejection and genuine exports. The dense CHR0 regression exposed backend tangent regeneration; using genuine I12 fixed it without changing tolerances.

Supplied Brawl main v9 geometry and Wait1 skeletal playback compared with BrawlLib native fractional world matrices; supplied Ultimate c00 full geometry/rig and idle transform tracks imported/reopened/skinned. Ancillary loss guard rejects unapproved export. All 688 input hashes unchanged. Local test data/projects/captures stay ignored. These are skeletal subset tests, not native material/constraint/game correctness.

Updated mapping GUI smoke passed strict and rest-delta UI modes, approval invalidation, save/reopen and fractional dual skin previews. Original screenshot reviewed. Actual supplied-project playback/save/reopen smoke passed for both; commercial captures stayed local and were not sent to image services. Native-tool GUIs, Modkit/cook, DCC, game runs, fresh-machine setup and remote CI remain unrun.

Bake measurements use a fixed quarter-frame grid and sampled vertices; narrow extrema, adaptive error, loop tangent continuity, all end-effectors and contact preservation remain acceptance work. CHR0 scalar validator tolerances remain .001 scale / .05 degrees / .01 cm; NUANMB decoded matrix element tolerance is .001. Shared encoders/decoders can share bugs.
## Historical verification details



Earlier increment: **27 passed / 0 failed** in `artifacts/tests/20261016-165712` (23 core including mapping/export validator; four native integration cases). All previous cases retained. `./scripts/gui-smoke.ps1 -Mapping` passed actual assignment/approval/reload/rejection/immutable-input/new-target checks and captured dual skinned previews. Screenshot review found and corrected viewport aspect framing. Optional Python SKEL fixture self-test passed separately, as did supplied reference skeleton reads. These counts do not imply game compatibility.

Every CHR0 export now validates the encoded file before atomic create-new publication: version/frame-count/loop match, original key values (including loop-end key), native quarter-frame scalar samples including default unanimated channels. Absolute tolerances: scale 0.001, Euler rotation 0.05 degrees, translation 0.01 cm; no modulo-angle comparison. Maximum one million scalar samples. Failures/cancellation leave no published file. Negative tests cover lost 360-degree turns, NaN, truncated samples, unintended channel motion, timing mismatch, and excessive sample budget. This is a bounded scalar grid with shared-library encode/decode, not continuous/world-space pose error proof or native-tool/in-game validation.

`./scripts/build.ps1 -Native`; `./scripts/test.ps1 -Native`; `./scripts/gui-smoke.ps1`.
The assertion runner is a normal executable that returns nonzero on any failure; use these commands, not `dotnet test` (no test-SDK adapter). Core tests require no commercial assets, network, hosts or licenses after SDK setup. Native tests additionally require the locally built BrawlLib worker; omission is printed as SKIP, not counted as pass.

First checkpoint: 17 passed / 0 failed, including three native scenarios. Covers original rig complexity/blended weights, rest inverse-bind identity, actual skinned motion, independent transform/Hermite expectations, Euler winding, fractional times/holds/loop endpoints, normals under nonuniform scale, key edit/undo/redo/save/reopen, malformed hierarchy/weights/indices/numbers/bindings/schema/keys, retained-resource hashes, genuine MDL0+CHR0 edit/reopen/export/decode, native scale/loop/tangent/winding, corrupt-header/unsupported-step rejection and source immutability. Quarter-frame error measured 0.000012993813; MDL0 fixture 125836 bytes at that checkpoint, edited CHR0 240 bytes.

Windows GUI smoke actually launched at 1280x820, WPF rendering tier 2; rendered screenshots inspected. Script exercises scrub/key edit/undo/redo/save/reopen and captures frames; this is not a full automated pixel correctness/UI interaction suite. Native-tool GUI, retail assets, Modkit/cooking, in-game checks and remote CI are UNRUN. BrawlLib encode/decode can share bugs; independent analytic math and source mesh comparisons supplement, not replace native-tool validation.

Generated results/screenshots/native/project files stay in ignored artifacts. Fixture source is deterministic original code; no game assets in CI. Known negative experiments and corrections are documented in decisions/0002-native-worker.md. Current status overrides old checkpoint counts when more criteria are added.

Follow-up increment: **23 passed / 0 failed**, including five restricted-FK cases and one further native case. Native tests now also compare UV0/normals/weight identity, deterministic repeated MDL0 bytes and a rotated alternate original rig. Native GUI project was loaded into both source and published desktop builds; edited CHR0 export/decode passed. Packaged CLI map-template/retarget/import/export/doctor were exercised. Native fixture model is now 124816 bytes after deterministic resource names removed random job UUIDs. Capability JSON/Markdown equality is checked by test.ps1. Setup tested its idempotent existing-SDK path; a fresh machine/remote CI remains unrun.


## Unified resource/ancillary verification

Full suite: `./scripts/test.ps1 -Native -Ultimate -LocalAssets` (44 cases at this checkpoint). Targeted changed workspace cases: `./scripts/test.ps1 -WorkspaceOnly -LocalAssets`; native worker must already be built. This runs original actual native fixture generation and optional supplied Mario texture/SRT checks, without rerunning completed worker-failure cases.

WorkspaceTests verifies ARC bank scope/ambiguity, decoded source textures, native ApplyVIS Boolean states including fractional boundaries/loop/mute/reset, SRT keys and native fractional ApplySRT matrices, independent Maya UV expectation, opaque/unevaluated retention, save/reopen, overrides, hashes and source overwrite rejection. Original RGBA8 quadrants are checked in WPF; palette/mip/alpha/multiple-UV conformance remains pending.

`gui-smoke.ps1 -Published -WorkspaceModel <model.pac> -WorkspaceMotion <motion.pac>` launches the real published apphost, exercises shared commands and native tree expansion, textures, skeletal plus ancillary playback and FPWORK save/reopen. Original fixtures additionally exercise explicit skeletal CHR0 write/decode. See RESOURCE_WORKSPACE.md for paths/clicks. Reports and captures are ignored; commercial captures must stay local. Existing default key-edit/undo and mapping/playback smoke modes remain available, now also with `-Published`.


Palette/UV/VIS increment: full suite **47/47** at `artifacts/tests/20261016-192451`. AncillaryTests adds independent CI8/RGB5A3 quadrant pixels/alpha/mips/alternate palette; all-flag XSI/3dsMax native matrix probes plus independent quarter turns; immutable VIS edits/binds/undo/constant expansion/save/export/native draw comparison and decoded mismatch rejection. WorkspaceTests also compares genuine rotating SRT0 fixtures at fractional times. Published original workspace smoke opens the actual texture viewer and checks selected mip/alternate palette/alpha, clicks visibility edit/undo/redo, exports VIS0 and captures rotating UV. Shared-backend checks are supplemented by independent expectations, not described as game/tool validation.

Fixture development caught a premature FileMap disposal: BrawlLib ReplaceRaw transfers ownership to ResourceNode. Closing the encoded texture/palette maps before ARC rebuild corrupted the generated fixture. Keeping node ownership through rebuild fixed the fixture; worker crash was contained and the corrected native files decode with independently expected pixels.

Texture/SRT0 increment: full suite **49/49**, `artifacts/tests/20261016-200030`, all 688 source hashes unchanged. `scripts/sampling-smoke.ps1 [-Published] -NativePng <test-output>/asymmetric.png` verifies 736 actual WPF DiffuseMaterial pixel probes (3/255 bound) and old/new row placement. Native all-track SRT0 edit/export, exact keys/tangents, fractional probes, binding/time edits, dirty/save, failure/cancel/corrupt/race checks are in UvEditingTests. Original distinct native UV1 values checked independently. See TEXTURE_DIAGNOSIS.md; no BrawlCrate/game check claimed.

Sampler inspection adds one focused case: native slot-1/UV1 metadata, independent distinct UV1 coordinates, native animated matrix comparisons, persistent selection and missing-UV/STQ/emboss rejection. SamplingSmoke now routes distinct UV0/UV1 through MeshCoordinates before checking rendered pixels. Published original GUI smoke invokes the actual sampler inspector/apply button; the local Mario case additionally exports/decode-validates all 12 Wait2 SRT0 tracks. Focused **9/9** at `artifacts/tests/20261016-200811`.

Final sampler increment full regression: **50/50**, `artifacts/tests/20261016-200949`; all 688 input hashes unchanged. Published original/Mario workflows and 736 rendered probes passed.

CI4 increment: 10/10 focused cases at artifacts/tests/20261017-015438; new PaletteTests verify native CI4 mip/alpha/nibble ordering and ambiguity/override persistence. Published classic smoke at artifacts/classic-smoke-20261017-015606 verifies actual viewer mip/palette/partial alpha/apply plus existing UI/edit/export tests.

## Current classic / palette / material verification — 2026-10-17

Portable/native/SSBH full suite: **49 passed / 0 failed**, `artifacts/tests/20261017-020519`. This excludes optional commercial assets. New material case uses a genuine native three-stage/two-sampler fixture, 36 independent RGBA expectations including bilinear seams, nearest versus linear, add/subtract/bias/scale, eight alpha comparisons, opaque/discard behavior, unsupported-mode rejection without mutation, transformed-minification rejection and native metadata save/reopen. Final focused rerun **11/11 passed**, `artifacts/tests/20261017-020904`, includes the last minification guard. Published classic smoke passed at `artifacts/classic-smoke-20261017-020959`; final build/publish had zero warnings/errors; published sampling 736/736 passed. The published classic smoke additionally checks four actual WPF premultiplied material pixels when material.pac is present. This does not establish 3D native shader fidelity or native-tool/game correctness.
