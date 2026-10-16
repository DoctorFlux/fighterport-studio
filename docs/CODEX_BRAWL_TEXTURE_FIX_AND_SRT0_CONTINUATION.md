# FighterPort Studio — Brawl texture fidelity, SRT0 editing, and continued implementation

Prepared from a read-only repository/source review on 2026-10-16.
Observed main: `4e5530774edfbb4c319eed96090a0a354a9e2b3b`.
This is a checkpoint reference, not an instruction to reset to that commit.

## Copy/paste implementation prompt

Continue the EXISTING FighterPort Studio repository. This is implementation work, not a fresh project or a request for another plan.

I see apparently mirrored/misplaced textures on Brawl Mario. Treat that as a rendering-fidelity bug to investigate, not proof that the original textures need flipping. Codex's proposed SRT0 editing/export task remains required, but first isolate and fix reproducible texture/UV/sampler problems so editing and export are judged against a reliable preview.

### 1. Recover the full project and current baseline

Read AGENTS.md, docs/plans/current.md, DEVELOPMENT_STATUS.md, RESOURCE_WORKSPACE.md, ARCHITECTURE.md, MATERIAL_PIPELINE.md, RETARGETING.md, TESTING.md, DEPENDENCIES.md, FORMAT_SUPPORT.md, config/capabilities.json, config/repository.json, and the actual implementation. Resolve their paths instead of assuming all are at the root.

Read the complete MASTER_PLAN.md, ROADMAP.md, and preserved continuation specifications, including CODEX_UNIFIED_RESOURCE_EDITOR_AND_FULL_CHARACTER_PIPELINE.md and CODEX_CONTINUE_ALL_GAME_WORKBENCH.md where present. Inventory other project docs and read relevant sections before implementing their features. Recover requirements from the repository, not a previous chat.

Reconcile historical entries with the latest verified code: do not resurrect superseded private-repository requirements or reimplement completed features. Preserve the existing WPF app, workers, project formats/migrations, tests, requirement IDs, resource browser, semantic/manual mapping, and full multi-game scope. Inspect Git status and preserve unrelated work.

The reviewed checkpoint reports 47 full-regression cases passing, unified native resource browsing, material-bound textures, CHR0/VIS0/SRT0 playback subsets, and VIS0 v3 editing/native export. Re-run the applicable baseline; do not assume those reports prove the current checkout. The next uncompleted native editing feature is SRT0 v4 curves/bindings/undo/native export. Do not repeat the old malformed-file milestone; retain its regressions.

Use the existing ignored local asset configuration to locate Brawl and Ultimate Mario. Do not ask me to supply files already configured. Hash and preserve original inputs. Read-only inspection, original fixtures, and independent tasks can proceed if one optional external application is unavailable.

### 2. Diagnose the apparent mirroring before patching anything

Start with the actual FitMario00 main model, not its separate shadow model. Reproduce the issue in the published application and the development build where possible. Identify the affected draw, material, sampler, texture/palette, UV channel and frame, rather than guessing which part I meant. Compare rest/static-material playback, CHR0 only, and CHR0 plus Wait2 SRT0 separately. Record observations and uncertainty.

Current source locations to inspect (verify they remain current):
- src/FighterPort.Desktop/WorkspaceCommands.cs: MaterialBrush, LoadTextures, texture decode/binding and preview.
- src/FighterPort.Desktop/Workbench.cs: Render, Draws, UV submission, DiffuseMaterial and BackMaterial.
- src/FighterPort.Core/UvEvaluator.cs: Support, Sample, Transform.
- Actual native geometry/material/texture importers, texture DTOs, workspace binding resolution, and existing test generators.

The reviewed MaterialBrush only selects mirrored tiling when BOTH axes mirror, repeated tiling when BOTH axes repeat, and TileMode.None otherwise. Treat that as a concrete coverage gap, not automatic proof of the Mario symptom. The current UV path also limits evaluation to sampler 0/UV0 and declines Maya rotation. Do not conceal those limitations under a new label.

Trace, in order:
1. Native texture bytes/format, palette identity, decoded channel values/alpha and bitmap row orientation.
2. Per-facepoint native UVs, all available UV sets, and the existing native-to-canonical V convention.
3. Static material texture matrix, animated SRT0 replacement/default semantics, matrix mode, pivots, negative scale, frame origin and loop/end behavior.
4. Texture-coordinate generation, matrix selection, material/sampler binding and indirect flags.
5. Independent U and V address modes, filtering, mip choice/LOD and rendered sample locations.
6. Per-draw material/triangle assignment, winding/culling, lighting/color inputs and alpha composition.

Do not globally flip images/UVs, force every material to Repeat, hard-code Mario names, or change game files to compensate for a viewer bug. Preserve legitimate mirrored UV islands and negative texture scales. A correct decoded PNG does not prove correct sampling on a mesh.

### 3. Compare the actual BrawlCrate implementation

Inspect the pinned BrawlLib/BrawlCrate revision and local patches from DEPENDENCIES.md/prepare-native.ps1. The reviewed dependency pin is d56cbac89dc2cd0e5c3f923e1d03041b16902add; verify it locally. Compare relevant upstream changes separately without an unreviewed whole-library update.

Use these upstream source areas:
- BrawlLib/SSBB/ResourceNodes/MDL0/MDL0TextureNode.cs — palette lookup, binding, UWrapMode/VWrapMode, min/mag filters and LOD.
- BrawlLib/SSBB/ResourceNodes/MDL0/MDL0MaterialRefNode.cs — sampler/material reference state and ApplySRT0Texture.
- BrawlLib/SSBB/ResourceNodes/MDL0/MDL0MaterialNode.cs — ApplySRT dispatch, resets, material state and shader interaction.
- BrawlLib/Modeling/FrameState.cs and BrawlLib/Internal/Matrix34.cs — texture frame flags, static state and Maya/XSI/3dsMax matrix branches.
- BrawlLib/SSBB/ResourceNodes/Animations/SRT0Node.cs — verify path/API for actual native parsing, mutation, rebuild, export and interpolation.
- BrawlLib/Wii/Graphics/ShaderGenerator.cs and the corresponding shader/TEV stage nodes — texture-coordinate generation and color/alpha composition.
- BrawlLib texture/palette codecs and GLTexture upload path, as located in the pinned source.

BrawlCrate sets U and V address modes independently. Its actual model-material path is the reference to inspect; do not take the separate 2D texture viewer's default wrapping as a material rule.

Compare BrawlCrate model preview with matching inputs/frame/settings when available. Record application/revision/settings and whether that comparison was actually performed. Reusing BrawlLib in both applications is not independent codec verification; use independent analytic/pixel tests as well. BrawlCrate GUI agreement is not a retail-game test.

Resolve the documented Maya rotation-only/general-branch discrepancy with a minimal original case and evidence. Do not average conflicting formulas, silently choose one, or advertise unsupported modes as fixed. Keep any unresolved mode visible and preserve its data.

### 4. Fix the real rendering boundary and add strong regressions

Create original asymmetric fixtures: labelled/direction-marked corners, unequal colored regions, an asymmetric alpha cutout, and a non-square texture. Include triangles with coordinates below zero and above one, spanning seams, negative texture scales, off-center pivots, multiple materials, and different UV0/UV1 data.

Test all nine Clamp/Repeat/Mirror U-V combinations, including mixed modes, with independently expected sampled colors at interior/seam/edge positions. Test nearest and linear filtering; retain separate minification/magnification/mip semantics where supported. Do not equate WPF TileMode.None with native ClampToEdge.

Verify WPF FlipX/FlipY/FlipXY/Tile, brush viewport/viewbox units and TextureCoordinates bounds empirically. Some combinations may be representable by the current renderer; others may need a sampler-capable rendering component. Do not impose modulo/clamp on vertices and interpolate them across a triangle as a substitute for texture sampling. That can destroy seam-spanning coordinates.

Preserve the WPF shell and interfaces. Prefer a small verified correction first. If correct sampling or native material features require a different viewport backend, introduce it behind the rendering boundary with a tested slice, documented dependency/license choice, and a retained labelled fallback. Do not restart the desktop app or fabricate a fully capable WPF ImageBrush implementation.

Add tests for the entire chain: decoded pixels -> coordinate conversion -> static/animated matrix -> sampler -> rendered pixel. Matrix equality alone is insufficient. Cover fractional frames, clip switching, reverse scrubbing, mute/reset, frame zero and loop endpoints. Keep existing tolerances unless a separately documented justified requirement changes.

Keep commercial assets and screenshots local and outside Git/external image services, as the repository requires. Use original generated fixtures for shareable regression captures. If the precise Mario symptom cannot be reproduced in this environment, report that limitation, fix only established defects, and retain an exact local reproduction recipe.

### 5. Implement complete declared SRT0 v4 editing and real native export

Use the existing resource tree/properties/timeline and command system. Add context-sensitive SRT0 controls instead of another row of format buttons.

Support typed edits to scale U/V, rotation, translation U/V, key times/values/tangents, and explicit material/sampler bindings for the declared native subset. Preserve direct/indirect identities, native names, source references, constant channels, matrix mode, frame count, loop metadata, and verified original-path/other metadata. Do not reinterpret indirect indices as ordinary texture sampler indices.

Separate runtime target bindings from native export names. Reject duplicate/ambiguous bindings transactionally. Track dirty state; make key and binding edits undoable/redoable; persist edits and command-relevant state appropriately through FPWORK save/reopen. Route keyboard/menu undo consistently with the active editor so editing a texture curve cannot accidentally undo a bone key.

Every existing track must survive export, INCLUDING unsupported-preview additional/indirect tracks. Use a native working copy/verified rehydration approach, not reconstruction from only displayed tracks. Verify how unknown metadata survives native rebuilding; source bytes alone are not evidence that the output preserved it. If preservation cannot be established, block a claimed complete export and explain the exact unsupported information. Do not silently upgrade/down-convert versions.

Implement the actual BrawlLib SRT0 writer, not raw archive slicing, renamed JSON, a no-op copy, or an unrelated interchange export. Make at least one meaningful curve edit and binding edit in generated fixtures.

Export to an atomic new file; decode the result; verify names, mode, timing, loops, constants, track identities/counts, values, tangents and retained unedited data. Evaluate supported decoded curves and UV transforms at fractional samples, endpoints, keys and appropriate extrema/adaptive samples. Validate unsupported-preview tracks structurally and numerically where decoding supports them; keep rendering claims separate. Bound workloads and report sampled versus stronger error guarantees accurately.

Test metadata preservation, nonzero tangents, constant-to-animated edits, undo/redo, save/reopen, mixed displayed/unevaluated tracks, source immutability, cancellation, worker failure, existing destinations and publication races. Reuse the existing failure infrastructure.

Open edited SRT0 on the matching model in BrawlCrate when available; label the check performed/unrun. Standalone native SRT0 export does not prove PAC/BRRES repacking or complete model export.

### 6. Continue the actual material pipeline, not just texture placement

Keep Source fidelity, Neutral debug, and Destination style as separate concepts. A Brawl preview error must not be 'fixed' by a style preset that rewrites the source.

Inventory the supplied character's actual material requirements and implement the next useful supported slice: extra sampler/UV selection, correct texture matrices/texgen, vertex and material colors, native alpha test versus alpha blend, culling/depth/draw state, and necessary TEV color/alpha stages. Do not multiply every texture together or assume every slot contains diffuse color. Preserve normal/environment/mask/ramp/indirect semantics.

Generic WPF DiffuseMaterial plus BackMaterial is not evidence of native shader fidelity. Use BrawlCrate's shader code as reference/integration material with licensing/provenance preserved, not as a claim that GLSL can be pasted into ImageBrush.

Show per-material evaluated/approximate/unsupported features in an expandable inspector. Keep one large preview, four primary commands, contextual resource actions and one timeline. Put specialist diagnostics under View/Tools rather than exposing new global buttons.

After the texture fix and SRT0 gate pass, continue dependency-ready work: remaining palette/wrap/material fixtures; PAT0/CLR0/SHP0 and constraint handling; dirty prompts/undo/search/recent/relinking/companion assembly; native model/container round trips; and the existing cross-rig transfer gates. Preserve synchronized state and defaults when switching clips/models.

### 7. Retain and execute the FULL roadmap without claiming universal support

Brawl, Melee, Ultimate and Rivals 2 remain peer bidirectional destinations. Dragon Ball Sparking! ZERO, FighterZ and Xenoverse 1/2 remain additional sources. Blender/Maya/3ds Max remain equal optional bridges; normal core processing stays local and non-AI.

Continue the documented Rivals Modkit/interchange, Ultimate ancillary/material/companion, Melee, animation optimization, Dragon Ball extraction, DCC and later platform milestones in dependency order. Preserve game/source revision profiles and all animation categories. Inspect the current official Rivals docs/installed Modkit when reaching that integration; do not hard-code old engine or import-screen assumptions.

Keep semantic anatomy, native bone identities, material/mesh/morph bindings and target export contracts distinct. Same-skeleton format conversion is not cross-rig retargeting. Read/edit/evaluate/write/package/native-tool/in-game validation are separate capabilities. Missing commercial inputs block only the checks that need them, not original-fixture implementation.

No automatic gameplay-script conversion in this stage. A complete editable character asset collection is not a finished playable fighter. Do not remove unmet requirements or generate dozens of empty adapters to create the appearance of progress.

Maintain an ordered executable queue. At every completed gate, take the next ready task during this run. Use parallel work only for genuinely independent changes with clear ownership and integrated tests. Do not spawn endless self-resuming agents, bypass approvals, or claim background work after execution stops.

### 8. Build, publish, document and update GitHub at each checkpoint

Use only the existing repository DoctorFlux/fighterport-studio and its recorded PUBLIC visibility. Do not create another repository or change visibility.

After each coherent passing implementation checkpoint:
- Run applicable builds/tests and published-app smoke checks through verified existing scripts.
- Update affected persistent docs, capability JSON/Markdown, defects and current plan. Record real evidence, limitations and exact next acceptance criterion; retain the master scope. Do not spend the run rewriting every unchanged document.
- Publish locally without overwriting unrelated files. Record the actual executable path.
- Review the staged diff/file list for unrelated changes, credentials, commercial assets/derived content, caches and binaries.
- Commit reviewed source/tests/docs and push using the existing verified scripts/push.ps1 workflow. Recheck live exact origin/visibility/branch and verify remote SHA equals the pushed commit. Keep the local receipt.
- Do not force-push, reset user work, change Git identity globally or dispatch remote CI without authorization. If pushing is blocked, preserve the commit, continue independent local work and report 'not pushed' honestly.

Before ending provide: diagnosed cause(s) versus remaining hypotheses; files and features changed; before/after evidence from original fixtures and any actual local Mario/BrawlCrate checks; exact app path and click sequence; real build/test results; SRT0 retained/exported subset; all pushed commit SHAs and failures; and the next task. Do not report unperformed GUI/native-tool/game tests as passed.

Start implementing now. Do not stop after planning, the first easy test, or a recommendation that I paste another prompt. Finish the strongest tested sequence possible within the active execution and leave a resumable checkpoint at an actual boundary.

---

## Review evidence and primary-source pointers

These observations came from source inspection, not a local Windows run or visual confirmation of Mario's symptom. Recheck the actual checkout before changing code.

### FighterPort

- Observed branch commit: 4e5530774edfbb4c319eed96090a0a354a9e2b3b, 'Expand palette and UV preview; edit and validate native VIS0 exports'.
- docs/DEVELOPMENT_STATUS.md reports 47/47 full regression results and later focused/published checks. These were not independently rerun during this review.
- docs/plans/current.md explicitly identifies SRT0 v4 editing/native export, asymmetric wrapping and extra-UV/material tests as unfinished.
- WorkspaceCommands.MaterialBrush chooses FlipXY for Mirror/Mirror, Tile for Repeat/Repeat, None otherwise; uses sampler 0 and UV0.
- UvEvaluator rejects rotating Maya animation and additional/indirect sampler playback. Its Transform method has a no-transform fallback for unsupported rotation/modes.
- Workbench.Render uses DiffuseMaterial and the same BackMaterial rather than a full native material shader.
- scripts/push.ps1 verifies exact repo policy/live visibility/branch, verifies the remote commit after push, and writes an ignored local checkpoint receipt.

Repository-relative source URLs:
https://github.com/DoctorFlux/fighterport-studio/blob/4e5530774edfbb4c319eed96090a0a354a9e2b3b/docs/plans/current.md
https://github.com/DoctorFlux/fighterport-studio/blob/4e5530774edfbb4c319eed96090a0a354a9e2b3b/src/FighterPort.Desktop/WorkspaceCommands.cs
https://github.com/DoctorFlux/fighterport-studio/blob/4e5530774edfbb4c319eed96090a0a354a9e2b3b/src/FighterPort.Core/UvEvaluator.cs
https://github.com/DoctorFlux/fighterport-studio/blob/4e5530774edfbb4c319eed96090a0a354a9e2b3b/src/FighterPort.Desktop/Workbench.cs

### BrawlCrate and WPF

- MDL0TextureNode.ApplyGLTextureParameters sets ClampToEdge/Repeat/MirroredRepeat through separate U and V fields, and treats min/mag filtering and LOD separately.
- MDL0MaterialRefNode.ApplySRT0Texture starts from material bind state; material dispatch and FrameState/Matrix34 contain additional relevant semantics that must be read in full for implementation.
- ShaderGenerator exposes native material/ambient/register constants and multiple sampler uniforms; this is materially different from displaying one image as generic diffuse color.
- Microsoft documents TileMode.None as drawing only the base tile, and FlipX/FlipY/FlipXY as alternate-tile mirroring. This does not establish equivalence to every native texture address-mode combination or WPF 3D coordinate mapping; rendered tests are required.

https://github.com/soopercool101/BrawlCrate/blob/d56cbac89dc2cd0e5c3f923e1d03041b16902add/BrawlLib/SSBB/ResourceNodes/MDL0/MDL0TextureNode.cs
https://github.com/soopercool101/BrawlCrate/blob/d56cbac89dc2cd0e5c3f923e1d03041b16902add/BrawlLib/SSBB/ResourceNodes/MDL0/MDL0MaterialRefNode.cs
https://github.com/soopercool101/BrawlCrate/blob/d56cbac89dc2cd0e5c3f923e1d03041b16902add/BrawlLib/Internal/Matrix34.cs
https://github.com/soopercool101/BrawlCrate/blob/d56cbac89dc2cd0e5c3f923e1d03041b16902add/BrawlLib/Wii/Graphics/ShaderGenerator.cs
https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/tilebrush-overview
https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-create-different-tile-patterns-with-a-tilebrush
