# FighterPort Studio — Continue the complete multi-game workbench

Prepared 16 October 2026 from the repository's current planning/status documents and the user's updated requirements. This is a continuation instruction, not a new-project specification. Verify the local checkout and actual test results before treating any reported checkpoint as current.

Paste the instruction below into Codex in the existing repository, or place this file under `docs/` and tell Codex to read and execute it completely.

---

Continue the EXISTING FighterPort Studio repository. Implement working software, not another architecture proposal or a new project.

## 1. Recover the actual state and preserve the repository

Read AGENTS.md, docs/MASTER_PLAN.md, docs/DEVELOPMENT_STATUS.md, docs/plans/current.md, docs/RETARGETING.md, the architecture, capability matrix, and relevant source/tests. Preserve existing requirement IDs, implementations, scripts, UI framework, native workers, and unrelated changes.

The reported checkpoint includes manual mapping/profile save/load, restricted FK previews, real MDL0/CHR0 subsets, decoded CHR0 validation, and 27 passing tests. The planned next task is M2.C2: malformed-native inputs and worker/publication failure handling. Inspect and rerun; do not assume these are still the latest state or rebuild completed features.

Historical “start from zero” and “no game assets available” instructions are superseded. Do not create another GitHub repository.

The known workspace was D:/Users/ChatGPT/ROA2TOOL. Use the verified current repository root rather than assuming that path is unchanged.

Before network writes, verify config/repository.json, the exact origin, account/repository identity, and live privacy. The intended repository is DoctorFlux/fighterport-studio. An external check on 16 October reported it PUBLIC despite documentation saying private. Recheck instead of trusting either cached claim.

If it is public, unexpected, or unverifiable, do not push. Explain the discrepancy, request authorization for any necessary visibility change, and continue local development. Do not change visibility, recreate the repository, or rewrite history on assumed permission. Once the intended private remote is verified, continue the existing authorized reviewed commit/push workflow.

## 2. The product remains a standalone multi-game editor

Build a BrawlCrate-like character asset workbench for ALL FOUR primary destinations: Brawl/Project M/Project+, Melee, Smash Ultimate, and Rivals of Aether II.

Each needs native asset inspection, applicable editing/playback, import/export, dependency handling, and native-tool integration—not just a name in a dropdown. Capabilities are implemented incrementally and reported individually.

Dragon Ball Sparking! ZERO, FighterZ, Xenoverse 1 and Xenoverse 2 are additional SOURCE adapters. Export back to Dragon Ball games is not required. Preserve the additional Smash-platform/future-game roadmap.

Keep Create New Character, Load Character, Open Native Resources, source/target selection, 3D inspection, animation timeline/curves, posing, undo/redo, textures/materials, skeleton mapping, retargeting, model rebinding, batch jobs, and save/reopen in the product scope.

Support Blender, Maya, and 3ds Max as equal optional authoring tools, plus BrawlCrate, Melee tools, SSBH Editor, and the Rivals 2 Modkit. Do not replace this app with a Blender add-on, require Blender as the canonical data model, or force a UI rewrite.

Normal application processing must remain local and non-AI. Gameplay-script/moveset/hitbox conversion is still out of scope. A character asset project is not automatically a playable new fighter slot.

## 3. Use the supplied Mario assets as local integration references

I now have raw Ultimate Mario model/skeleton/animation/material/texture files and Brawl Mario resources inside PAC files.

Discover their existing configured locations and inspect:
- .local/references/brawl-mario00.md and .json
- .local/references/ultimate-mario-c00.md and .json
- Existing local asset manifests and path settings.

The previous reports counted 63 Brawl main-model bones, 18 shadow-model bones, and 98 Ultimate bones. Treat those as observations about those exact files—not universal game skeleton counts. Separate the shadow model from the main fighter and distinguish costume, model, and animation-bank dependencies.

Reconcile reported inventory with actual files. Never infer that full Ultimate mesh/animation import exists merely because its skeleton reader works. Do not fabricate missing models or clips. If some inputs are absent, report the specific dependency while continuing independent work.

Keep all supplied files read-only. Preserve hashes, external path mappings, original native data, and target branches. Put converted commercial content, derived reports, screenshots, and local integration results only in ignored local storage. Never upload them to GitHub, CI artifacts, issues, or model/inference services.

Use original generated fixtures for portable tests and bounded malformed-input tests. Never mutate my real files for failure tests. Retain local real-asset regression tests separately; use my supplied assets now rather than pretending none are available.

## 4. Execute a bounded acceptance queue, not disconnected tasks

Continue build/test/fix work during this active run. Complete and validate cohesive increments; do not end merely because you wrote a plan or finished one easy checkbox when further work is available. Respect actual runtime, usage, permission, and tool boundaries; never create an endless restart loop or claim background progress.

First complete remaining M2.C2 criteria: deterministic malformed size/offset/count/truncation cases, bounded subprocess handling, crash/timeout/cancellation diagnostics, output/log limits, unchanged inputs, no partial publication, and destination-race tests. Reuse original fixtures and actual backend checks. Do not turn this into an unbounded fuzzing project or call a worker process a security sandbox.

Then implement M3.A anatomical mapping and rest-axis/hierarchy correction on original mismatched rigs. Preserve the existing strict FK path until the generalized path is independently tested; do not remove rejection checks to make Mario appear supported.

Next establish real Brawl and Ultimate canonical import/playback in dependency-sized slices: model, skeleton, skin, a selected skeletal clip, save/reopen, and documented preview materials. Reuse the Brawl backend and optional SSBH reader, extending what is missing.

Develop full Ultimate mesh/animation support earlier than its previous backlog position where necessary for the two supplied Mario rigs. Record this dependency-based reprioritization without dropping Rivals or Melee.

Then connect the generalized retargeter to both imported rigs and demonstrate Brawl-to-Ultimate and Ultimate-to-Brawl animation transfer, with separate CHR0/NUANMB write and decoded-validation gates. Skeleton-only transfer can be an intermediate test, not full mesh compatibility.

After that, continue the next ready channel/material, Rivals Modkit, Melee, optimization, Dragon Ball, or DCC milestone. Track all four destinations with concrete acceptance criteria so this never becomes a permanently Brawl-only app. Prefer complete tested operations over simultaneous empty adapters.

A blocked external-tool task must not stop independent work. Preserve exact blockers and continue an eligible item.

## 5. Build semantic anatomy profiles, not just a bone-name replacement list

Preserve exact native bone names, stable identities/order, parents, bind/rest transforms, and game-specific metadata. Add semantic roles as separate annotations.

Support root/motion root, pelvis/waist, ordered spine/chest/bust, neck/head, shoulders/clavicles, upper arms, forearms/elbow pivots, wrists/hands, fingers and segments, thighs, shins/knee pivots, ankles/feet, toes, plus helpers/twists, face, grab/attachments, tails, wings, hair and costume chains.

Represent left/right, chain membership, segment order, terminal markers, and deform/helper roles explicitly. Joint pivots such as “knee” are not necessarily extra bones. Distinguish chest from breast/accessory bones and a toe joint from a foot end-marker.

Read actual names from decoded skeletons. Example aliases in old research are not confirmed mappings. Never hard-code Mario as every character's skeleton.

For a toe, the relationship should be:
actual source bone identity -> semantic role such as left.leg.toes.base -> actual chosen target bone identity.

That semantic identifier is our annotation, not a mandatory renamed native joint. A missing target toe requires a visible, approved fallback or an unsupported result; do not silently map it to the ankle.

Use deterministic naming aliases, hierarchy, character-local spatial relations, limb geometry, skin weights when available, and saved profiles. Show the evidence/confidence and unresolved candidates; heuristics are not calibrated probabilities unless measured. Bone matching must remain useful with missing geometry.

Provide auto-suggest, viewport picking, manual assignments, side correction, chain editing, assign/unassign, exclusions, and versioned saved profiles. Revoke approval after relevant edits. Scope profiles to exact source/target skeleton fingerprints and conventions; distinguish exact reuse from tentative suggestions for a new character.

Use original renamed, axis-rotated, asymmetric, missing-bone, extra-chain, and finger/toe fixtures to prevent Mario-specific overfitting. User-approved local Mario maps stay local.

## 6. Generalize retargeting without corrupting animation

A shared editable representation and semantic layer do not require one universal physical skeleton.

Keep each source rig and each destination-specific production rig. A Goku_R2_Master may be one later destination; do not force it onto Brawl, Melee, or Ultimate. Rivals custom naming does not imply a universal Rivals bone dictionary.

Separate animation transfer from mesh rebinding and control-rig evaluation. Initially transfer motion onto the target's existing mesh and bind data. A model swap/rebinding operation has separate correctness tests.

Specify matrix spaces, multiplication conventions, handedness, and units before implementing reference-pose correction. Do not blindly copy Euler components or a schematic formula from research. Derive and test axis changes, parent-chain differences, local/world transforms, and rest-pose identity behavior.

Handle root versus pelvis motion explicitly. Use per-chain translation/scale policies and optional proportion scaling. Preserve duration, source curves/tangents, multi-turn winding, loop semantics, and non-skeletal synchronization. Flag unsupported scale inheritance/shear instead of silently approximating it.

Add twist/spine distribution and optional hand/foot/weapon contact correction as separately tested capabilities. Keep uncertain maps out of unattended batches.

Compare poses, end-effector trajectories, and selected skinned vertices at fractional times as well as keys. Preserve exact source data and record approximation/error when the target needs baking. Same-library round trips and fixed-grid scalar checks do not prove continuous or independent correctness.

Save retargeted results in new target branches with provenance. Do not repeatedly convert reduced exports into new masters or overwrite source/target projects.

## 7. Include every applicable animation category

Maintain separate capability records for reading, editing, preview evaluation, retarget/remap, native writing, packaging, and validation.

Brawl roadmap: PAC/PCS/BRRES, MDL0, TEX0/PLT0, CHR0, VIS0, SRT0, PAT0, CLR0, SHP0, plus relevant ancillary resources.

Ultimate roadmap: NUMDLB/NUMSHB/NUSKTB, NUMATB/NUTEXB, NUANMB and its actual supported track groups, plus verified associated data.

Melee roadmap: HSD DAT, joints/skinning/materials/textures, AnimJoint/FigaTree, and applicable native material/texture/visibility structures. Inspect actual HSD structures; do not invent matching Brawl extensions.

Rivals roadmap: editable/source skeletal meshes, animation sequences, textures, materials, morph/visibility substitutes where supported, and official Modkit import/export/build integration. Cooked extraction is a distinct capability.

Bone mappings do not remap materials automatically. Use separate mesh/material/texture/parameter/morph/visibility binding identities. Preserve shared dependencies and time synchronization across tracks.

When a target has no equivalent, report one of: direct mapping, explicitly approved baking/substitution, native-tool integration required, preserved but not playable, or unsupported. Opaque preservation is not working playback or cross-game conversion.

Add genuine visibility, UV motion, texture swap, color, and morph fixtures and matching timeline controls incrementally. Never claim all channels work because skeletal animation does.

## 8. Resolve whole characters and support native editing

Make Open Game/Folder/Archive discover logical characters and their dependencies rather than requiring me to choose every file individually. Show unresolved resources, duplicate names, costume variants, and shared animation banks.

Support raw folders first, then version-verified installed-game and image readers. Preserve Brawl/Melee ISO, Ultimate extracted native data and accessible container/image routes in the roadmap. No downloads of commercial content, keys, or access-control bypasses; inaccessible inputs get explicit status.

Use library APIs/workers and documented native-tool bridges rather than fragile mouse macros. Integrate BrawlCrate/BrawlLib, HSD tools, SSBH libraries/Editor, official Rivals tooling, and appropriate source-only CUE4Parse/FModel/UE Viewer/LibXenoverse backends after version/license verification.

For New Character, distinguish asset workspace creation, destination profile, fighter replacement, and a fully configured in-game fighter. Preserve exact native slot/skeleton requirements when needed.

Add a native resource tree, animation browser, bone inspector, curves, texture/material views, import/replace/export actions, undo/redo, and loss reports to the existing app—not just launch buttons for other programs.

## 9. Materials, optimization, and optional DCC bridges

Preserve UVs, normals/tangents, weights, morphs, vertex colors, palettes/ramps, texture color spaces, transparency, material parameters, and animation bindings. Store unsupported native fields explicitly.

Separate basic textured preview, editable native material data, approximate target-style preview, and native shader fidelity. Generate target styles non-destructively; do not claim flat textures contain recoverable detail they never had.

Optimize animation against the encoded destination format with protected poses, measured error, and actual byte counts. Track file/container/runtime budgets separately. Preserve timing and held poses. An impossible quality/size constraint must fail or request an explicit tradeoff—not silently relax limits.

Keep automatic mesh reduction deferred unless the existing plan authorizes it; still enforce validity requirements.

Discover Blender, Maya, Max, modding tools, and Modkit versions through bounded known locations/settings, with manual overrides. Do not scan the entire disk. Discovery is not a tested bridge or a license grant.

Build versioned send/receive manifests carrying identities, units, axes, clip timing, skeleton fingerprints, and conflict detection. Evaluate .blend/.ma/.mb/.max in the matching installed application. Core viewing/mapping/native editing must not require any DCC to be running.

Use verified FBX or other interchange backends and label DCC-assisted fallbacks honestly. Do not claim arbitrary host control rigs survive generic interchange.

## 10. Rivals 2 and the eventual mixed-source Goku

Verify current official Workshop documentation and the installed Modkit rather than hard-coding an old engine version or screenshots.

Use the documented single-root custom-skeleton workflow and 60 fps destination animation requirements. Keep source timing separately; do not globally force other games to the Rivals profile.

Bake required retarget/control/IK results before import. Validate destination axes, units, scale, influences, animation sampling, and material packing through an explicit profile. Warn separately about guidance and hard constraints.

Prepare real mesh/clip interchange output plus material assets and an import manifest. Integrate the official generated-character/template workflow without overwriting unrelated mod content. Verify available editor APIs before scripting them. Do not copy cooked Unreal packages between games and call it a conversion.

Use verified local Rivals reference rigs when present. Otherwise an original single-root target may test the workflow, but must not be called an official Rivals skeleton. An uninstalled Modkit leaves editor/cook/in-game tests unrun, not passed.

Long-term, one chosen Goku model/production rig should accept selected Project M, Ultimate, Sparking, FighterZ, and Xenoverse animations. Add non-destructive clip composition/transition editing on that chosen rig, then generate separate destination outputs. Mario proves portions of the reusable pipeline; it is not a substitute for later Goku integration tests.

Keep gameplay attacks, Lua, hitboxes, Articles, and publishing as explicit downstream integration work, not silently added automatic script conversion.

## 11. Acceptance, persistence, and delivery

For each increment record exact versions, operations, fixtures/assets, losses, tests, and independent/native-tool evidence. Separate generated fixtures, real-asset import, export/redecode, native-tool validation, Modkit import/cook, and in-game checks. Do not label unrun checks passed or skip commercial tests now available without explaining why.

The first real cross-game acceptance demonstration should eventually let me:
open both supplied Mario bundles -> preview each on its native skeleton -> inspect semantic suggestions -> approve/correct a map -> transfer selected clips in both directions -> save/reopen -> export CHR0/NUANMB into new paths -> decode/compare -> inspect in native tools when available.

Choose actual available idle/locomotion/attack or other informative clips, not invented filenames. A missing channel or editor must have a precise status. Neither a bone report nor a converted filename satisfies this demonstration.

Maintain original-fixture coverage for roles/channels not exercised by Mario. Add Melee, Rivals, and each Dragon Ball route through equivalent per-operation gates as backends/assets become available.

Keep AGENTS.md concise and update the master plan, support matrix, current plan, and evidence after completed slices. Do not erase requirements or regenerate giant documents instead of coding. Record this continuation as a scoped addendum so later sessions recover both the full product and current task.

Run the existing build/test/GUI/publish scripts as applicable; verify their paths/options first. Known scripts include build.ps1 -Native, test.ps1 -Native, gui-smoke.ps1 -Mapping, and publish.ps1. A changed architecture may require updated checks, not assumed results.

Inspect the diff, staged file list, source immutability, and privacy before any authorized push. Never upload commercial fixtures, screenshots, detailed private reports, secrets, or machine-local configuration. Do not automatically dispatch CI, publish Workshop content, accept licenses, or force-push.

Publish a local updated app without deleting unrelated files. Report its verified executable path, a precise workflow I can try, actual tests, unrun checks, remaining losses, commit/push status, and the next executable criterion.

Start with the remaining current gate and continue eligible work now. Do not restart the project or stop at “we should build this.”

---

## Primary references for verification

Use the repository's pinned sources first and verify new integration APIs against current primary documentation.

- Rivals Workshop: https://rivals2.com/workshop/
- Rigging: https://rivals2.com/workshop/knowledge-base/character-creation/rig-your-character/
- Animation: https://rivals2.com/workshop/knowledge-base/character-creation/animation-overview/
- Modkit: https://rivals2.com/workshop/knowledge-base/getting-started/installing-the-modkit/
- BrawlCrate: https://github.com/soopercool101/BrawlCrate
- HSDLib: https://github.com/Ploaj/HSDLib
- SSBH: https://github.com/ultimate-research/ssbh_lib
- SSBH Editor: https://github.com/ScanMountGoat/ssbh_editor
- Ultimate authoring reference: https://github.com/ssbucarlos/smash-ultimate-blender
- FModel: https://github.com/4sval/FModel
- CUE4Parse: https://github.com/FabianFG/CUE4Parse
- UE Viewer: https://github.com/gildor2/UEViewer
- LibXenoverse2: https://github.com/Olganix/LibXenoverse2
- GitHub privacy verification: https://cli.github.com/manual/gh_repo_view
- Codex repository instructions: https://developers.openai.com/codex/guides/agents-md/

These are integration candidates/references, not a declaration that every parser, writer, platform version, or license combination has already been verified.
