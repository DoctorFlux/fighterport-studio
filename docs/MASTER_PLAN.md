# FighterPort Studio master plan

Continuation authority: docs/CODEX_CONTINUE_ALL_GAME_WORKBENCH.md preserves all four primary destinations and source-only Dragon Ball/DCC scope. Ultimate canonical import is moved forward after M2.C2 and M3.A to enable the supplied Mario pair. Latest user instruction keeps DoctorFlux/fighterport-studio PUBLIC; source-only reviewed pushes require exact identity and expected public visibility. Historical private/no-assets bootstrap requirements below are retained as history and superseded on those points.

Historical checkpoint addendum, 2026-10-16 (current work is in docs/plans/current.md): M2.C2 bounded malformed-native and worker/publication failure tests pass. M3.A1 semantic/manual roles and scalar WorldRestDelta-v1 changed-axis/hierarchy transfer pass original tests. M5.B1 real supplied Brawl PAC MDL0 v9/CHR0 and Ultimate MODL/MESH/SKEL/ANIM geometry/rig/skeletal playback pass opt-in local tests. Genuine original SSBH fixtures and same-rig CHR0↔NUANMB measured bakes/write/decode pass. These do not complete Mario cross-rig transfer or ancillary/native-model fidelity. Next is M3.A2 quaternion/translation/scale/helper policy; all remaining M0–M5 criteria stay in ROADMAP.md. User-supplied Brawl/Ultimate inputs and derived reports/projects/captures stay local and excluded from Git. All 688 original files remain unchanged. Official Rivals requirements remain in REFERENCE_INSPECTION.md. No native-tool GUI/editor/cook/in-game compatibility is claimed.

The full source specification below remains normative. No later milestone is removed by the first implemented subset. Stable requirement groups map to its numbered sections; acceptance IDs map to the roadmap and tests.

| IDs | Source section | Acceptance ownership |
|---|---|---|
| REQ-01 Authority/fresh/local | 1 | M0.A |
| REQ-02 Private Git/source hygiene | 2 | M0.B |
| REQ-03 Persistent specification/evidence | 3 | Every checkpoint |
| REQ-04 Desktop character workflow/exclusions | 4 | M1–M5 |
| REQ-05-BRAWL | 5 Brawl | M2, M3 channels |
| REQ-05-MELEE | 5 Melee | M5.A |
| REQ-05-ULTIMATE | 5 Ultimate | M5.B |
| REQ-05-RIVALS2 | 5 Rivals II | M4 |
| REQ-05-DB-SOURCE | 5 Dragon Ball source-only | M5.C |
| REQ-05-LEGACY | 5 Wii U, 3DS, 64 | M5.E |
| REQ-05-INTERCHANGE | 5 FBX/glTF/DAE/SMD/ANIM and native tools | M4, M5.D |
| REQ-06 Canonical data, workers, renderer | 6 | M1, M2, schema extensions |
| REQ-07 Mapping/FK/IK/rebinding | 7 | M1.D, M3.A |
| REQ-08 Optimization/material styles | 8 | M3.B, M3.D |
| REQ-09 Discovery/DCC/input safety | 9 | M0.C, M4, M5 |
| REQ-10 Incremental milestones | 10 | All |
| REQ-11 Honest evidence and delivery | 11 | All |
| REQ-12 Primary source registry | 12 | Integration-specific |

Every paragraph in each source section belongs to its requirement group. The current schema implements a declared subset: missing fields are work, never permission to discard input silently. Native originals retain opaque data; no lossless native model rewrite is advertised.

## Complete preserved scope

# FighterPort Studio — Start From Zero

**Self-contained implementation instruction for Codex CLI.**
Prepared 16 October 2026. No previous conversation, plan, repository, game assets, or uploaded ZIP is required. Read the entire file before implementing. This file describes the requested product; features listed here are requirements, not claims of existing support.

## 1. Your task and authority

Build FighterPort Studio from this workspace: a standalone Windows-first desktop character-asset editor and porter for Rivals of Aether II and Super Smash Bros. games, with Dragon Ball games as additional asset sources.

Act as the implementation engineer. Create the specification, roadmap, repository instructions, architecture, application, tests, build scripts, and a PRIVATE GitHub repository, then implement in dependency-ordered, buildable increments. Do not finish after producing only documents, empty interfaces, or a UI mock-up.

I have GitHub CLI (`gh`) and Codex CLI installed. Check whether Git and required development tools are actually available. I do NOT yet have commercial game assets, extracted files, ISOs, or ROMs. Generate original test assets and continue independent development without those inputs.

This is a fresh-start request. Inspect the workspace before changing it. It may contain this instruction file but no project. Do not import or delete an earlier FighterPort project elsewhere on my computer. If this workspace already contains unrelated work or belongs to a parent repository, do not overwrite it or accidentally commit the parent; identify the conflict and continue only in a safe, clearly named location with required authorization.

You are authorized to initialize local Git, create ONE new private repository in the currently authenticated personal GitHub account, make reviewed project commits, and push those commits to that verified private repository. This is not permission to create an organization, change another repository, switch accounts, make a repository public, invite collaborators, purchase services, publish game assets, or bypass access controls.

Choose routine technical details yourself and document consequential decisions. Respect sandbox, network, and installation approvals. Ask only for genuinely necessary authorization, identity ambiguity, or unavailable private input, and continue independent work while a specific operation is blocked.

Continue the implement/test/fix cycle during the active run, rather than returning a suggestion to start. A single instruction delegates the work; it is not a guarantee of unlimited runtime. At a real runtime, permission, or usage boundary, leave an honest tested checkpoint and a concrete next task. Do not install an endless self-restarting agent loop or claim to keep working after execution stops.

## 2. Bootstrap the PRIVATE GitHub repository safely

Do this early, alongside environment setup; a GitHub/authentication problem must not prevent local implementation.

1. Check versions and authentication with `git --version`, `gh --version`, `codex --version`, and `gh auth status --hostname github.com`. Never use flags that reveal tokens, read credential stores, or print secrets.
2. Identify the active account through an authenticated read, for example `gh api user --jq .login`. Use that account's personal namespace. Do not guess the owner from Windows usernames or earlier conversations. If login is needed, request the user's browser/device authorization; do not request a token pasted into the chat. Do not confuse network denial with authentication failure.
3. Default repository name: `fighterport-studio`. Check whether that name is already used. Do not silently repurpose an existing repository, even a private one. For a genuine name collision, choose the first available clearly named suffix such as `fighterport-studio-fresh`, then `fighterport-studio-fresh-2`. Distinguish confirmed absence from API/network errors; do not loop through names on authentication failure. Announce the selected owner/name. On a resumed run, reuse the matching verified repository recorded in local project metadata rather than creating duplicates.
4. Initialize a new local repository on `main` only after verifying the intended root. Create `.gitignore` and a pre-push review/check workflow BEFORE staging files. Use an existing appropriate Git identity; if none is set, retrieve the authenticated user's GitHub-provided commit identity/private email and configure it only for this repository. Do not invent an identity, expose a private email unnecessarily, change global author settings, or impersonate another person. If identity cannot be resolved, continue development and record why committing is blocked.
5. Create the remote explicitly as PRIVATE without uploading content yet. The normal fresh-repository operation is:

   ```text
   gh repo create OWNER/NAME --private --source=. --remote=origin
   ```

   Resolve OWNER/NAME from the verified account and name. Check installed CLI help for exact syntax. Do not use `--public`, `--internal`, or an automatic visibility-changing fallback. Do not combine creation with `--push`: verify visibility first.
6. Verify the exact remote and privacy, using a read such as:

   ```text
   gh repo view OWNER/NAME --json nameWithOwner,isPrivate,url
   git remote -v
   ```

   Require a successful response, the intended owner/name, and `isPrivate: true` before the first push and subsequent push sessions. Abort network writes if verification fails. Do not rewrite an unexpected existing origin. A successful create command alone is not sufficient verification.
7. Review the staged diff and file list, commit only intended source/docs/tests/scripts/lockfiles, then push the new `main` branch normally. Never force-push, reset unrelated changes, disable push protection, or treat `.gitignore` as sufficient proof that already tracked files are safe. Check remote branch/commit identity after a successful push. If a push fails, preserve the local commit and report its state accurately.
8. Add concise README setup instructions and a small milestone/issue backlog in the new private repository. Use stable milestone IDs; avoid duplicate issues on retries. Local roadmap/status files are authoritative so development works without GitHub access. Avoid time estimates or invented completion percentages. Do not stop coding to create dozens of issues.
9. Prepare a minimal GitHub Actions build/test workflow using original fixtures, least-privilege permissions, verified pinned actions/dependencies, and `workflow_dispatch` initially. Do not dispatch it, enable scheduled agents, activate paid services, or change account billing. Keep local tests usable independently. Report any required remote-CI authorization or unavailable capability. Do not claim CI ran just because its YAML exists.

Track only project code and intentionally redistributable files. Ignore game installations/dumps/images, extracted commercial content, cache, local asset libraries, credentials, `.env` secrets, private absolute-path settings, unrelated conversations, generated builds, and large diagnostic artifacts. Use narrow, documented exceptions for small ORIGINAL generated test fixtures where needed. Do not use Git LFS as a way to upload commercial assets.

Store external game locations in ignored local configuration. Sanitize public/project diagnostic reports. Preserve dependency license notices; a private repository does not remove third-party licensing obligations. Keep any dependency clones and binaries managed deliberately, not accidentally staged. The application's game-asset data must never automatically be committed or synchronized to GitHub.

## 3. Persist the complete project, without burying implementation in documents

Create concise, useful documents and update them as code changes:

- Root `AGENTS.md`: essential invariants, required reading, exact commands, verification rules, and continuation protocol. Keep it short; link the detailed specification rather than pasting everything into it. Do not change global Codex instructions.
- `docs/MASTER_PLAN.md`: the complete scope in this file, including later milestones and exclusions; stable requirement IDs.
- `docs/ARCHITECTURE.md` and `docs/decisions/`: dependency boundaries and consequential decisions.
- `docs/ROADMAP.md`, `docs/DEVELOPMENT_STATUS.md`, and `docs/plans/current.md`: milestones, evidence, blockers, and the next executable acceptance criterion.
- `docs/FORMAT_SUPPORT.md` and `config/capabilities.json`: matched human/machine-readable operation-level capability records.
- `docs/DEPENDENCIES.md` and `docs/SOURCES.md`: verified APIs, primary-source links, checked dates, tested revisions, license/distribution requirements, and integration methods.
- `docs/RETARGETING.md`, `docs/OPTIMIZATION.md`, `docs/MATERIAL_PIPELINE.md`, and `docs/TESTING.md`: implementation-specific conventions and tests, expanded only when useful.
- `docs/GITHUB_STATUS.md`: intended repository URL/owner, verified privacy, branch, and push state; never secrets.
- Root `README.md`: exact setup, demo, build, test, publish, limitations, and links.

Retain this original request as source requirements. Do not overwrite it to make requirements disappear. Instructions in a future session must recover the goal from the repository, not assume access to a previous chat. Link acceptance criteria to relevant tests/code. A passed test count is not a substitute for describing tested operations.

## 4. The product and user workflow

The app should feel like BrawlCrate with multi-game capabilities: its own asset tree, 3D viewport, skeleton inspection, playback, posing/key editing, retargeting, material controls, conversion jobs, and build validation.

It is NOT only a Blender add-on, an Unreal project, a collection of launch buttons, a web dashboard, or a Goku-only converter. Basic viewing, editing, mapping, and conversion must not require Blender, Maya, Max, or Unreal to be running.

Normal application processing must be LOCAL and NON-AI: no inference API, model weights, cloud conversion, AI subscription, or prompts required by the application. Codex may assist development; that must not become a runtime dependency. Optional commercial programs still have their own licensing; initial tool installation and Workshop/GitHub publication are distinct online operations.

The intended flow:

```text
Create New Character / Load Character / Open Demo Character
    -> choose destination game, fighter/slot profile, or custom skeleton
    -> select source installation, supported game image, extracted folder,
       archive, or loose native assets
    -> automatically resolve model/skeleton/animation/material dependencies
    -> preview the actual imported skinned character and synchronized tracks
    -> suggest anatomical mappings, review confidence, manually correct
    -> retarget, rebind if needed, edit, adapt materials, optimize
    -> save/reopen the character project with provenance and target branches
    -> export verified destination-native assets or a real native-tool package
    -> inspect/test in BrawlCrate, Melee tools, SSBH Editor, or Rivals 2 Modkit
```

An empty character workspace or original demo must be usable before I supply a game's reference skeleton. A generic demo rig must never be mislabeled as an exact game skeleton. Creating a character project, producing assets, replacing a fighter slot, and completing a playable fighter are different states.

Gameplay-script/moveset/hitbox-logic conversion is OUT OF SCOPE for now. Preserve related opaque resources where safe and show remaining game-specific integration steps. Do not advertise a finished playable fighter from asset conversion alone. Automatic geometry reduction is deferred, but validity restrictions on weights, indices, bones, transforms, texture formats, or resource budgets still apply.

## 5. Complete adapter and format scope

Treat this as the investigation/implementation roadmap, not proof of compatibility. Verify exact formats and versions against actual source code/docs. Every adapter declares read, write, preservation, editing, evaluation, packaging, and validation separately.

### Core bidirectional games

**Brawl / Project M / Project+**

Use/integrate BrawlCrate and BrawlLib where appropriate. Cover PAC/PCS/BRRES containers, MDL0 models/skeletons, CHR0 skeletal animation, TEX0/PLT0 textures/palettes, and VIS0 visibility, SRT0 texture transforms, PAT0 texture/palette selection, CLR0 color animation, SHP0 shape animation. Track other resources and SCN0 where relevant separately. Preserve synchronized clips and shared dependencies. Do not confuse a successful resource copy with imported editable playback.

**Melee**

Investigate HSDLib/HSDRaw and Melee Modding Wizard. Support the relevant HSD DAT model, joint, skin, material/texture, AnimJoint/FigaTree, and other applicable animation structures. Verify material/texture/visibility behavior through actual structures; do not just rename Brawl track types. Track fighter slot/build/revision dependencies.

**Smash Ultimate**

Investigate SSBH libraries, SSBH Editor, and the Smash Ultimate Blender ecosystem. Cover verified NUMDLB/NUMSHB/NUSKTB/NUMATB/NUANMB/NUTEXB and associated resources. Distinguish transform, visibility, material, and other supported tracks. Do not assume unsupported morph/physics channels map automatically.

**Rivals of Aether II**

Use the official Rivals 2 Modkit workflow, not Rivals 1. Import/export source assets through documented editor/interchange routes and version-aware editor automation where available. Track skeletal meshes, skeletons, animation sequences, materials, textures, attachments, and related character references. Verify the installed Modkit/engine version and available APIs. Cooked extraction is a separate capability from editable-source export; Unreal engine commonality does not permit blindly copying cooked assets between games.

### Dragon Ball SOURCE-ONLY games

Sparking! ZERO, Dragon Ball FighterZ, Xenoverse 1, and Xenoverse 2. No requirement to export back to these games. Investigate compatible FModel/CUE4Parse, UE Viewer/export tools, and LibXenoverse/LibXenoverse2 (including EMD/ESK/EAN/EMM/EMB and related data). Use separate version/game profiles, resolve master/external skeletons, animation compression and dependencies, and report unhandled facial/material tracks. Do not promise compatibility just because a mesh can be extracted.

### Remaining platform-fighter roadmap

Separate Smash Wii U and Smash 3DS adapters; investigate Smash Forge and actual NUD/NUT/VBN/OMO/BCH usage where applicable. Smash 64 needs its own documented container/model/animation pipeline. Add future platform fighters through independent adapters and profiles; do not assume a generic ROM reader solves native asset conversion.

### Interchange/native-tool round trips

FBX, declared glTF/GLB subsets, COLLADA/DAE, and SMD/ANIM when a verified workflow requires them. Blender, Maya, and 3ds Max are equal supported editing options. Include BOTH directions with BrawlCrate, appropriate Melee tools, SSBH Editor, and Rivals 2 Modkit for supported editable data. Reading a format is not proof that a backend can write it.

All applicable animation categories remain in scope: skeletal, visibility, texture transforms/swaps, material parameters/colors, shape/morph/facial, and relevant ancillary tracks. Store synchronization and explicit per-target loss/fallback policies. A destination without an equivalent may need an approved bake/substitution, target-specific work, or an unsupported result; never silently discard the channel.

## 6. Architecture, project data, and renderer

Default to Windows-first C#/.NET with Avalonia/MVVM, a headless core and shared CLI, plus a real 3D rendering backend. Inspect current supported SDKs and library requirements, then pin a compatible toolchain. A different well-supported choice needs a short evidence-based decision, not a lengthy framework debate.

Separate project data, import/export adapters, animation evaluation, retargeting, optimization, rendering, materials, validation, UI, and tool discovery. Do not put UI or DCC dependencies in the core. Avoid dozens of empty projects, unnecessary databases/servers, and premature universal abstractions.

Legacy .NET, C++, Rust, Python, Autodesk SDK, and Unreal components may need versioned worker processes. Verify actual build/API requirements; provide a small typed protocol, errors, timeout/cancellation, progress, tool detection, and crash handling. A child process is not automatically a security sandbox, nor a license workaround.

Implement a depth-buffered skeletal viewport with usable camera controls, original normals/UVs/weights, playback, pause/scrub, skeleton overlays, basic transforms/key editing, and undo/redo. Advanced graph/IK editing can be staged. Do not build a painter-sorted triangle mock-up that must be discarded for real meshes. Neutral shading is acceptable before native-material fidelity, but label it honestly.

Use a versioned canonical project representation, NOT FBX/glTF as the sole master. Preserve:

- Meshes, topology, UVs, custom normals/tangents, vertex colors, weights, morphs, materials.
- Exact skeleton IDs/order/hierarchy, rest/bind/inverse-bind transforms, local axes, parent-scale behavior, native metadata.
- Native curves, tangents/interpolation, time domains/rates, loops/end semantics, translation/rotation/scale, intentional full-turn rotation winding, root motion, synchronized ancillary tracks.
- Source hashes/provenance, retained original resources, unresolved dependencies, semantic mapping profiles, non-destructive correction layers, export recipes, target-specific branches, and capability/loss reports.

Document internal units, handedness, coordinate conventions, matrix/quaternion layout, and evaluation order. Test reversible boundary conversions. Do not erase Euler winding by forcing shortest-path quaternions, flatten scale/shear silently, or replace exact bind data with a guessed rest pose.

Preserve native references in project-managed storage or verified relinking, not a temporary cache that vanishes on save/reopen. Treat opaque native preservation as separate from editable canonical support. Block a promised lossless rewrite when preservation is impossible.

Always retain a high-quality source branch. Generate each target from that branch and its edits, not from previously reduced exports. Returning a Brawl-specific edit must not silently overwrite the Melee/Rivals/Ultimate branches.

## 7. Deterministic anatomical mapping, retargeting, and rebinding

Identify body roles independently of bone names: root, pelvis/waist, spine, chest/bust, neck, head, left/right clavicles/shoulders, upper arms, forearms/elbows, wrists/hands, finger chains/thumb segments, thighs/hips, shins/knees, ankles/feet, toes. Support multiple spine/neck bones, twist/helper bones, face bones, tails, wings, extra limbs, and attachments.

Joints such as knees/elbows are anatomical roles within a chain; do not assume every rig has an extra bone literally named Knee or Elbow.

Use saved fingerprints/profiles, naming aliases, hierarchy, position/orientation, bone lengths, and skin-weight evidence. Work locally without AI. Expose scores/uncertainty and alternatives. Test renamed/ambiguous bones and unusual proportions. Never silently batch-convert low-confidence mappings.

Provide source/target 3D selection, a manual mapping table, assign/unassign, correction of left/right and axes/rest pose, chain editing, manual locks, reset/recompute, and reusable profiles. Once a user approves a compatible mapping, reuse it across clips.

Profiles are per game/build/fighter-or-slot/exact skeleton, not one compulsory identical skeleton for every fighter. Keep required helper/grab/attachment identities. Generate native reference profiles from later user-supplied assets rather than bundling commercial skeletons. Verify Rivals 2's custom hierarchy/single-root rules instead of imposing invented universal names.

Separate animation transfer, mesh rebinding, and external control-rig evaluation. Animation-only conversion must not replace an existing target mesh. Rebinding must update weights/binds consistently while retaining intended rest geometry.

Transfer motion using tested transform mathematics, rest-pose alignment, axis correction, proportions, root policy, twist distribution, chain fallbacks, contact/IK corrections, and saved manual layers. Implement/document a restricted FK subset first; stage IK and special cases with explicit limits. Facial/material retargeting needs its own mapping, not the body-bone mapper.

## 8. Animation optimization and visual styles

Optimize after retargeting against a retained high-fidelity reference. Preserve duration, timing, held/discontinuous poses, loop boundaries, full rotations, contact/impact/user-locked frames, and root motion.

Use target-aware reduction/curve fitting/quantization with joint-angle and world-space/end-effector errors, and representative skinned-vertex checks where useful. Evaluate intermediate times and target interpolation/overshoot. Encode native bytes, decode again, and compare. Report actual file size separately from known runtime/container budgets. Fewer keys are not automatically smaller files. Do not force one modern codec on every native format.

Expose tolerances and fidelity-versus-size tradeoffs. If a requested size and fidelity cannot both be met, report the conflict, not a hidden relaxation. No universal invented Brawl/Melee budget or automatic timing change.

Build non-destructive material/style presets for Melee, Brawl/Project M, Ultimate, and Rivals 2. Preserve source textures/custom normals/palettes/ramps/masks/vertex colors/transparency. Offer user-editable material regions (skin/hair/cloth/metal), color adjustments, sizing, procedural detail, packing, and supported baking/shader setup. Label external baking dependencies. Do not claim flat color contains recoverable sculpted detail or guarantee perfect style matching.

Verify current Rivals ORMS convention (red outline, green roughness, blue metallic, alpha shading) and base-color/import settings from the official guide. Keep color/data textures in correct color spaces. Preview neutral and approximate destination shading separately; native-tool/in-game checks remain distinct.

## 9. Tool discovery, game inputs, and DCC bridges

Automatically discover Blender, Maya, Max, BrawlCrate, relevant Melee/Ultimate tools, and the Rivals 2 Modkit via bounded installed-program records, registered paths, standard locations, and user configuration. List versions, chosen executable, availability, manual overrides, and a doctor report. Do not scan the entire disk or execute arbitrary discovered scripts. Missing optional tools must not block app launch.

Use versioned send/receive manifests for Blender Python, Maya Python/API/batch, and Max pymxs/MAXScript/batch as actually supported. Preserve asset IDs, skeleton fingerprints, units/axes, ranges, assignments, and hashes; detect stale/conflicting edits. Bake host control rigs to designated game/export skeletons. Native .blend/.ma/.mb/.max evaluation belongs to the corresponding installed host. FBX is not a promise of perfect host-rig round trips. Do not require Blender for every Maya/Max or standalone operation; identify any DCC-assisted interchange fallback honestly.

Separate installation/disc-container access from asset decoding. Start with loose files/extracted directories, then verified images/installations. Investigate nod/decomp-toolkit for GameCube/Wii. Switch/3DS/Wii U/N64 need separate readers and access requirements. Do not imply an encrypted Ultimate image is immediately readable; show prerequisites and an extracted-data route. No bundled keys, ROM downloads, DRM bypass, or guessed access credentials.

For each supported source path, detect game/platform/revision with evidence, lazily index bounded directories/archives, discover logical character bundles, resolve dependencies, and preview before importing. One path should eventually be sufficient for supported accessible layouts; ambiguity and missing dependencies need an actionable report, not a fabricated success.

All source files stay read-only. Use staged atomic outputs, cancellation, quotas, archive-path checks, bounds checks, decompression limits, corruption diagnostics, and safe loading. Never execute embedded source scripts. User game data stays outside Git and optional remote CI.

## 10. Milestones: real application first, real native integration early

Preserve the whole roadmap, but execute in coherent slices. Before spending heavily on polished UI, investigate a working BrawlLib native read/write integration so format constraints inform the core.

### M0 — Bootstrap

Verify environment and dependencies; establish private GitHub as above; write minimal persistent instructions; create a pinned buildable solution, CLI/core tests, scripts, and first safe commit. This checkpoint is not the final deliverable: continue into implementation.

### M1 — Original demo animation workbench

Create original redistributable skinned test characters with multiple joints, blended weights, UVs/simple textures, and material regions. Include one anatomically detailed humanoid and a second differently named/oriented/proportioned rig, plus a nonhumanoid/extra-chain case when relevant. Goku assets are not required and must not be downloaded.

Include idle, walk, punch, crouch, jump, and targeted loop/hold/full-spin/scale/tangent fixtures. Implement New/Load/Open Demo, real skinned playback, skeleton inspection, play/pause/scrub, basic editing/undo, saved project/reopen, and documented FK mapping/retargeting subsets. Use a small original sample rather than rigid root-only triangles. Deliver runnable code, not inactive panels as proof of features.

### M2 — Actual Brawl MDL0/CHR0 path

Reuse verified BrawlLib/native backends to generate genuine native MDL0/CHR0 fixtures for declared versions/subsets. Integrate them through the SAME importer used for user files. Cover model/skeleton/weight/bind import, nonconstant CHR0 motion, meaningful edits, preserved timing/tangents/winding, save/reopen, native CHR0 export, and decode/compare. Track native model writing separately from animation writing.

Acceptance path:

```text
original fixture -> real native files -> normal file import
-> skinned playback -> edit -> save/reopen project
-> separate native animation export -> decode and compare
```

Do not inject the original fixture behind the importer, rename JSON to a game extension, or copy unchanged bytes and call it editing. Resolve model/clip bindings explicitly. Keep same-skeleton import independent of auto anatomical mapping.

If the native backend is genuinely blocked, implement independent M1 work, isolate and document the actual obstacle, and do not call M2 complete. No missing commercial assets excuse for abandoning original native-fixture tests.

### M3 — Complete character-workspace behavior

Implement confidence-aware automatic anatomical mapping with manual controls, reusable profiles, improved retargeting/rebinding/corrections, New/Load target selection, and deterministic optimization. Add real synchronized VIS0/SRT0/PAT0/CLR0/SHP0 subsets with generated visibility, UV, swaps, colors, and shape fixtures. Do not claim channel playback just because bytes can be preserved. Expand material preview and per-target styles.

### M4 — First cross-game/native-tool route

Add verified standalone or explicitly labeled assisted interchange, with a Brawl-to-Rivals 2 import package/editor bridge and reverse animation route from editable/exportable Rivals source data to a destination Brawl profile. Stage and validate assets through the matching installed Modkit when available. Without it, implement the adapter/package tests and report editor/cook checks unrun. Preserve script-conversion exclusion.

### M5 — Expand source/target support

Add Melee and Ultimate read/write subsets/native-tool checks; Dragon Ball source adapters; full Maya/Max/Blender round trips; broader image/install browsing; costumes and remaining material/animation categories. Then separate Smash Wii U, 3DS, 64, and future adapters. Maintain the full planned scope, but count only demonstrated capabilities. Adjust ordering for a documented dependency or a validated smaller path, never to hide incomplete work.

At every usable checkpoint build, test, update the docs/capability matrix, review the diff, commit, and push only after verifying the intended private remote. Continue the next ready acceptance criterion during the active run. Do not stop because M0 documentation is finished; do not pretend all milestones must fit one run.

## 11. Evidence, quality, and deliverables

Track each game/version/resource/direction/operation separately using states such as Planned, Researched, ImplementedForDeclaredSubset, SyntheticTested, NativeToolChecked, RealAssetChecked, InGameChecked, and Blocked. These are not one blanket supported badge; some checks can be independent. Record fixture IDs, tool revisions, measured errors, losses, and limitations.

Core automated tests run without commercial assets, DCC licenses, game installations, or AI. Test transform math, inverse binds, units/axes, hierarchy, winding, fractional timing, looping, serialization/migration, rebinding, optimizer bounds, channel synchronization, malformed inputs, missing dependencies, and source immutability.

A writer/reader round trip from one library can share a bug: add independent mathematical/reference expectations and native-tool checks when possible. BrawlCrate and BrawlLib share code; state that rather than calling them fully independent validation. Synthetic tests do not prove retail-character or in-game compatibility.

Add platform-appropriate GUI smoke tests; a headless compile is not an observed GUI launch. Report passed/failed/skipped/unrun with reasons. Never invent screenshots, exported file correctness, test counts, observed rendering, remote CI success, or delivery paths.

Use deterministic fixture generators and known licenses, dependency lockfiles, safe user-local dependency installation where authorized, and bounded build scripts. Request approval for privileged/global installation, EULAs, purchases, or unavailable execution. Do not weaken tests to obtain green output. Verify dependencies before bundling and retain their notices; do not assume every GitHub repository is permissively licensed.

Provide tested setup/run/test/doctor/publish scripts. Stage executable builds under an ignored repository-local `artifacts` directory unless an explicit delivery path is configured. Never overwrite a game installation or unrelated folder. Keep source code in GitHub and commercial input data local. A private source repository is not itself a released application.

Before ending the run, report:

1. Exact verified local repository path and, if created, private GitHub URL and privacy result.
2. What the desktop app/CLI actually do; exact run and Open Demo instructions.
3. Native operations/versions/subsets that work, remaining losses, and unsupported operations.
4. Tests and manual checks actually run, results, and those not run.
5. Exact built executable/artifact paths that exist, and whether a Windows launch was observed.
6. Local commit, actual push status, unresolved prerequisites, and the next executable task saved in `docs/plans/current.md`.

## 12. Primary-source starting registry

Consult these when their integration is needed; inspect exact APIs and versions instead of assuming this list guarantees complete capabilities. Record sources/revisions in repository docs. Do not spend the whole first run researching later adapters.

Codex/GitHub operations:
- https://developers.openai.com/codex/cli/reference
- https://developers.openai.com/codex/agent-configuration/agents-md
- https://learn.chatgpt.com/docs/models
- https://cli.github.com/manual/gh_auth_status
- https://cli.github.com/manual/gh_auth_login
- https://cli.github.com/manual/gh_repo_create
- https://cli.github.com/manual/gh_repo_view
- https://cli.github.com/manual/gh_api

Core game tools:
- https://github.com/soopercool101/BrawlCrate
- https://soopercool101.github.io/BrawlCrate/
- https://github.com/Ploaj/HSDLib
- https://github.com/DRGN-DRC/Melee-Modding-Wizard
- https://github.com/ScanMountGoat/ssbh_editor
- https://github.com/ultimate-research/ssbh_lib
- https://github.com/ScanMountGoat/ssbh_data_py
- https://github.com/ssbucarlos/smash-ultimate-blender
- https://github.com/jam1garner/Smash-Forge

Rivals 2:
- https://rivals2.com/workshop/
- https://rivals2.com/workshop/knowledge-base/getting-started/installing-the-modkit/
- https://rivals2.com/workshop/knowledge-base/character-creation/rig-your-character/
- https://rivals2.com/workshop/knowledge-base/character-creation/import-into-unreal/
- https://rivals2.com/workshop/knowledge-base/character-creation/texture-your-character/

Source access:
- https://github.com/4sval/FModel
- https://github.com/FabianFG/CUE4Parse
- https://github.com/gildor2/UEViewer
- https://github.com/Olganix/LibXenoverse2
- https://github.com/encounter/nod
- https://github.com/encounter/decomp-toolkit

Use official .NET, Avalonia, Blender, Autodesk, and Epic documentation for the selected versions. If network access is unavailable, clearly mark unverified assumptions and work from available inspectable code. No invented interfaces or unsupported compatibility claims.

**Begin now: verify the workspace, establish the minimal persistent project and private repository safely, then implement and test the first runnable workbench/native-integration slice.**

## Historical unified editor continuation — M3.A2 checkpoint

Read the complete CODEX_UNIFIED_RESOURCE_EDITOR_AND_FULL_CHARACTER_PIPELINE.md. Preserve all existing requirement IDs and scope; add M1.E resource workspace, M3.C1 synchronized VIS0, M3.D1 texture/material approximation and M1.C2 bounded external-source workspace storage as child criteria. All four destinations remain peers; four mapping layers (bone, visibility, material, shape) stay distinct. Native model/package writers remain required after the first visible workspace.

M3.A2 first executable subset tested: WorldRestDelta-v2 evaluates retained native quaternion sources at fractional times on changed rest axes/parents. UnitOnly admits constant unit scale but rejects actual scale motion; root CopyDelta and nonroot RestParentDelta are explicit approved policies. Nonroot translation deltas transform through source-parent rest basis into target-parent rest basis, without automatic proportion scaling. Unmapped target helpers retain rest locals; animated source coverage remains mandatory. Unsupported compensation, twist distribution, IK/contact and uncertain commercial mapping remain blocked/pending. Legacy v1 layers reopen with their old policy subset.

Baseline build/native/SSBH/local suite: 38 passed in artifacts/tests/20261016-183139. Increment portable suite: 30 passed in artifacts/tests/20261016-183315, including three independent quaternion/translation/native-matrix cases. The completed native-failure milestone was retained, not reimplemented. Next immediate product criterion: M1.E + M3.C1/D1 Brawl native resource tree, real material-bound textures and synchronized VIS0 on a single timeline, with original genuine multi-channel fixtures and opt-in Mario checks. Full target-rig Mario transfer still requires reviewed mapping/loss policies.


## Unified workspace checkpoint - 2026-10-16

M1.E/M1.C2/M3.C1/M3.D1 initial slice: actual native resource trees and one Open workflow, hash-bound source handles, verified external-source FPWORK storage, ARC group-aware texture dependencies/overrides, resizable WPF preview, shared CHR0/VIS0/limited SRT0 sampling and explicit skeletal-only destination conversion. Original genuine multi-channel fixtures and supplied Mario Wait2 are tested. This does not close the full editor milestone: palette/mip/alpha/multisampler fidelity, rotating UV modes, PAT0/CLR0/SHP0/constraints, ancillary editing/writers, dependency graph, advanced interactions and all native character/package writers remain required. See RESOURCE_WORKSPACE.md and current.md. Four peer destinations, Dragon Ball sources, equal optional DCC bridges and every existing requirement ID remain. Full suite 44 passed; 688 source hashes unchanged; commercial content excluded from Git.


## Palette / UV / visibility checkpoint — 2026-10-16

M3.D1 now has original native CI8/RGB5A3 indexed palette/alpha/mip 0/1 checks and a mip/palette/usage viewer. M3.C1 adds verified XSI/3dsMax direct UV0 sampler-0 SRT rotation plus visibility working-copy sample/binding edits, bounded undo and genuine standalone VIS0 v3 write/decode validation. Full regression 47 passed; all 688 local inputs unchanged. This does not close material or ancillary scope: Maya rotation discrepancy, CI4/full palette/wrap coverage, extra UV sets/multiple-material conformance, shader/sampler composition, SRT0 and other channel edits/writers, full native model/container/character round trips remain. Next executable criterion is SRT0 v4 editing and decoded native export. Every prior requirement ID, four peer targets, Dragon Ball sources and optional equal DCC bridges remains unchanged.


## Brawl texture / SRT0 checkpoint — 2026-10-16

Execute and preserve CODEX_BRAWL_TEXTURE_FIX_AND_SRT0_CONTINUATION.md. M3.D1 bitmap-row boundary and independent address modes now have 736 rendered original-fixture checks; nearest/LOD/native shader fidelity remains unsupported. M3.C1 SRT0 v4 typed curves/tangents/bindings, bounded undo/persistence and genuine all-track native working-copy export/decode are tested; unsupported-preview tracks survive. Full suite 49 passed, 688 inputs unchanged. See TEXTURE_DIAGNOSIS.md for diagnoses, hypotheses and exact limits. Next extra-sampler/UV inspection and material pipeline, then remaining ancillary/editor/retarget criteria in current.md. Every earlier requirement and all four peer bidirectional targets, Dragon Ball sources and equal optional DCC bridges remain required.

M3.D1 next tested slice: contextual isolated native sampler/UV selection, retained min/mag/LOD/texgen metadata, native UV1 matrix checks and distinct UV0/UV1 rendered-pixel regressions. Complete native composition remains required. Mario Wait2 standalone SRT0 all-12-track decoded preservation is locally verified; no native-tool/game claim. Next CI4/palette ambiguity, then native material and remaining ancillary/editor/cross-game queue.

## Classic workspace continuation — 2026-10-17

Execute CODEX_CLASSIC_BRAWLCRATE_STYLE_WORKSPACE.md and the focused [UI_UX_PLAN.md](UI_UX_PLAN.md). Two classic views share the existing session and tested native services; current task list is only docs/plans/current.md. Dated earlier next-task statements are historical; SRT0 v4 editing/export is complete for its declared subset. Follow UI changes with CI4/palette and a bounded material-rendering slice, retaining every master game/channel/bridge requirement.
