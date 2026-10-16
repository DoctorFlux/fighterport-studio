# Brawl sampling and SRT0 checkpoint — 2026-10-16

The confirmed renderer defect was at the canonical-to-bitmap boundary. Native import retains the established `(u, 1-nativeV)` canonical convention. Pinned BrawlLib GLTexture uploads decoded bitmap row zero without another flip; the old WPF draw submitted canonical V directly to that bitmap. The original non-square native TEX0 reproduces the old vertical placement and verifies the corrected boundary. No source UV, image, material name or game file is rewritten.

The other confirmed defect was address-mode coverage: TileMode.None for Clamp or mixed axes did not implement native ClampToEdge. TextureSampling now builds a bounded, independently addressed image domain, including border texels, and applies an affine coordinate conversion. It never wraps/clamps individual triangle vertices. Clamp, Repeat and Mirror are independent per axis. Negative UVs, mirrored islands and negative scales survive. The power-of-two image domain avoids additional WPF 3D resampling of arbitrary bitmap dimensions; it is capped at 4096 per axis / 4 megapixels with a 32 MiB cache and explicit neutral fallback.

This remains a bilinear mip-zero approximation. The actual WPF DiffuseMaterial probe measures bilinear samples even when NearestNeighbor is requested. No nearest, native minification/LOD, anisotropy or full shader claim is made. Tests use controlled white ambient light to separate texture sampling from the workbench's approximate lighting. The original RGBA8 8×4 gradient has unequal channels and an asymmetric alpha cutout. Before/after captures and decoded PNGs remain under artifacts, not in Git.

## Supplied Mario findings and uncertainty

Actual FitMario00 main model: 63 bones, 14 meshes. Body materials use Repeat/Repeat; eye layers use Clamp/Clamp. Body UVs legitimately extend outside 0–1, including negative U. Static sampler transforms inspected here are identity MatrixMaya. Thus mixed wrapping alone does not explain the reported body symptom. Eye materials reference four samplers/UV channels; displaying only sampler 0 cannot reproduce their native composition. Additional UV arrays are now retained canonically; their retention alone is not a rendering claim.

The original fixture proves a defect in the same renderer used for Mario. Published Mario workflow captures rest, CHR0-only and CHR0+Wait2 SRT0 states locally. Commercial captures were not uploaded to image/model services. The precise area the user called mirrored was not independently visually identified; remaining hypotheses include layered eye TEV composition, unsupported texgen/material state, alpha-test/cull/depth behavior and filtering. Do not claim every Mario appearance issue fixed.

## Compared implementation

Backend pin remains `d56cbac89dc2cd0e5c3f923e1d03041b16902add`, with the existing three build/bfloat/CHR0 patches only. Inspected: GLTexture bitmap upload; MDL0TextureNode palette lookup and separate wrap/min/mag/LOD setup; MDL0MaterialRefNode bind-state reset and keyed SRT replacement; MDL0MaterialNode direct/indirect dispatch; TextureFrameState/Matrix34 Maya/XSI/3dsMax branches; ShaderGenerator texgen and TEV/alpha-test behavior; SRT0Node parsing/mutation/native rebuild. WPF DiffuseMaterial plus BackMaterial does not reproduce native TEV or draw state. Maya rotation-only's d[6]/d[7] discrepancy remains unresolved and rotating Maya preview remains unsupported.

BrawlCrate GUI was not found in the dependency checkout, PATH or registered App Paths. No BrawlCrate GUI or retail-game check was performed. Shared BrawlLib encode/decode is supplemented by independent color, alpha, wrap and matrix expectations; it is not independent native-tool validation.

## SRT0 v4 declared editing/export subset

Contextual editor exposes all five Hermite channels, integer key times, values/tangents, set/move/delete, separate preview material and native material/direct-or-indirect sampler binding. Sixteen snapshot undo steps survive closing/reopening the editor; FPWORK stores the edited group, stable source-track IDs and native metadata. Unsaved SRT0 prompts protect source/workspace/model/clip replacement and closing. Full workspace undo remains a separate milestone.

The worker loads the verified original resource and modifies that working copy. It preserves every direct/additional/indirect track, empty material entries, name, version, matrix mode, timing, loop and original path. It restores explicit tangents after native key insertion. It rejects missing/duplicate identities, unknown flag bits, noncanonical loop/matrix metadata, v5, discontinuous keys and unrepresentable constants. Rebuilding relocates strings and may normalize known encoding flags; this is numerical/structural preservation, not byte-identical native preservation. PAC/BRRES repacking is not implemented.

Output is decoded before atomic create-new publication. Validation checks every track/binding/channel/key, exact float values/tangents and metadata, including unsupported-preview tracks. Identical Hermite coefficients establish the same polynomial between keys; additional quarter/mid/three-quarter segment probes exercise evaluation. This is not a continuous final-render/world-space guarantee. Export rejects a disabled group rather than silently dropping its override. Failed worker/decode, cancellation and destination races leave the working copy and existing destination intact.

## Evidence

- Full preserved suite: **49 passed, 0 failed**, `artifacts/tests/20261016-200030`; **688 source hashes unchanged**.
- Rendered native-fixture probe: **736 samples**, all nine U/V address combinations, requested nearest/linear behavior, alpha, two material submissions, negative scale, fractional frames, loop/reverse/mute/reset; maximum channel difference **3/255**. This new pixel bound does not relax existing **0.00001 UV** comparisons.
- Distinct original native UV0/UV1 import checks; SRT fixture direct 0/1/7 and indirect 2, nonzero tangents, constant-to-animated edit, binding changes, preserved empty entry, undo/redo/save/reopen, missing-track rejection and failed publication checks.
- Published workflow and final push evidence are recorded in DEVELOPMENT_STATUS.md and ignored checkpoint receipts.
