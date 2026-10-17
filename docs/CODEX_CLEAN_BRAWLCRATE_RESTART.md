# FighterPort Studio — clean restart directly from BrawlCrate

Implementation instruction for Codex · 17 October 2026

## Decision

Replace the existing FighterPort implementation with a complete, pinned BrawlCrate source baseline, then develop FighterPort directly on top of that application.

Existing destination: `https://github.com/DoctorFlux/fighterport-studio`

Upstream source: `https://github.com/soopercool101/BrawlCrate`

This is a direct BrawlCrate-derived application, not the old FighterPort WPF shell, not an imitation UI, and not a bridge that merely launches a second BrawlCrate editor. GitHub hosting should remain independent: do not use GitHub's Fork operation. Independent hosting does not remove upstream copyright, attribution, licensing or corresponding-source obligations.

This instruction supersedes the previous bridge/preserve-the-old-core migration strategy in `FighterPort_BrawlCrate_Rework_Codex_Brief.md`. Preserve the user's product requirements and safety constraints, not the previous implementation architecture. Legacy FighterPort compatibility is not a prerequisite for this restart.

## 1. Prepare a recoverable replacement

Read the actual current repository and user requirements before changing files. Identify the real workspace root, worktree status, origin, remote HEAD and current repository visibility. Do not assume an older reported commit is still current.

Create and verify an offline recovery copy outside the new working tree. Preserve committed history, staged/unstaged modifications, untracked work, local configurations, test evidence and user assets. A Git tag or bundle alone does not preserve untracked and ignored files. Do not upload the recovery copy, local configuration, secrets or assets. Protect the backup with the same local access restrictions as the original.

Use a fresh sibling working directory or a controlled local restart branch, retaining the existing destination repository's Git identity/history. Replace the old application's tracked source, tests, project files, outdated instructions and build arrangement in the new active tree. Do not leave the old application as an active dependency or default startup project. Old code may remain only in the verified external recovery copy/history.

Do not delete the GitHub repository, delete its issues/releases, alter its visibility, rewrite shared history, force-push, mirror-push into it, or run blanket destructive cleanup commands. Replacing the active implementation is the requested clean restart; remote destruction is not needed.

Work locally for this reset. Do not push any branch/tag, open a public pull request, upload build artifacts or publish a release automatically. Earlier auto-push instructions are superseded for this reset because the user raised public-visibility concerns. Report the resulting local commits and publication status accurately.

## 2. Import the actual complete upstream baseline

Fetch the official upstream into a separate staging directory, determine its current default branch and record the exact selected commit. Choose and explain a reproducible baseline. Copy the complete tracked source/resources/build tree into the new active implementation without copying upstream's `.git` over the destination's metadata.

Include the actual application, BrawlLib, native resource editors, model/animation previewer, support projects and required resources. Do not copy only BrawlLib and recreate the application around it. Handle any LFS/submodule requirements explicitly; do not assume a downloaded archive includes everything.

Keep upstream source provenance in `UPSTREAM.md`: repository, commit, original version, retrieval date, baseline tree/hash manifest and subsequent modifications. A source import commit is allowed; it must not present imported code as newly authored by us.

Retain existing copyright headers, notices and upstream credits. Preserve the applicable LGPL/GPL texts and audit component-specific licenses, bundled libraries, resources and binaries. Do not replace the copied code's license with a blanket MIT/proprietary license or remove provenance to conceal the base. Mark modifications and relevant dates. Retain meaningful existing legal notices in the application.

Add clear third-party notices and an About/Credits entry identifying the BrawlCrate/BrawlBox/BrawlLib lineage and the actual component authors. These are proposed compliance locations, not a claim that these files alone satisfy every component's license. Before redistributing binaries, verify exact corresponding source, build instructions and applicable replacement/relink requirements. Keep downloaded game assets and unidentified third-party executables out of commits and releases.

## 3. Make the original application build before redesigning it

Inspect the actual solution, project files, startup code and dependencies. Build the imported baseline before changing frameworks, CPU architecture, namespaces, renderer or UI. Reproduce the upstream build configuration first. Treat any toolchain repair as a small documented deviation and retain the otherwise unmodified baseline for comparison.

The intended main application is the real BrawlCrate-derived editor. Do not resurrect the old WPF application or make a launcher for it. Do not do a mass rename of internal namespaces merely for branding.

Inspect updater, crash-reporting, issue-submission, release-channel, telemetry and other network integration code before running it. Keep upstream source references and credits, but prevent this independent build from fetching/installing official BrawlCrate updates or sending user data to inherited endpoints automatically. Document disabled or redirected integrations. Do not silently remove original editing features under the name of cleanup.

After a reproducible baseline succeeds, use FighterPort Studio product branding, an independent version, executable identity and separate settings/cache locations. Retain licensing and credits. Avoid overwriting normal BrawlCrate settings or taking file associations automatically. Do not imply upstream endorsement.

## 4. Preserve the whole native editor, with evidence

Inventory the actual upstream menus, resource types, contextual editors, preview modes, import/export/rebuild operations, animation tools, texture/material tools, audio tools and scripting entry points. Preserve the inherited workflows even if not immediately used for character porting. Do not advertise universal Brawl support merely because a parser class exists.

Use original/self-created fixtures for portable tests. Use local commercial reference files only when explicitly configured and do not publish them, derivatives or captures. Do not fabricate passing tests for fixtures that are absent.

Exercise the actual running application, not only project compilation. Establish a baseline for opening resources, displaying a model, playing a skeletal animation, inspecting texture/material data, a real edit, saving to a new destination and reopening it. Check native decode results and unchanged source hashes. Record failures as inherited, introduced or not yet classified; do not hide them.

Where native behavior has known gaps, record them rather than claiming hardware-perfect rendering, universal undo or complete native fidelity. Add focused regression tests as the application is extended.

## 5. Extend this application into the full product

Record these permanent requirements in `docs/MASTER_PLAN.md` and a capability matrix. They are future work until independently implemented and validated.

PRIMARY TARGETS

- Smash 64, Melee, Brawl/Project M, Smash for Wii U, Smash for 3DS and Ultimate.
- Rivals of Aether II, including installed-game sources and Modkit projects.

ADDITIONAL GAME FAMILIES

- Dragon Ball FighterZ, Sparking! ZERO, Xenoverse and Xenoverse 2.
- Extensible source adapters and separately declared native-output support for additional games.

Smash and Rivals are primary character-authoring and conversion targets. Dragon Ball native-output/playable-mod goals must remain explicit roadmap items instead of being silently dropped. Decode, edit, native write, package and in-game validation are different capabilities; support for one does not imply the rest.

Keep development modular: source providers, game-specific asset adapters, a native resource layer, a game-neutral character project, animation/retargeting services, target gameplay compilers and validation. Preserve original native data alongside editable project state. Do not force assets from every other game through MDL0/CHR0 or make Brawl the universal intermediate format.

Keep the familiar BrawlCrate editor as the visible foundation. Add project/source/retargeting/character workspaces to it. Plan modern or 64-bit workers for large archives and incompatible dependencies where necessary, but do not make a speculative global runtime rewrite the first milestone.

OPEN SOURCE

Implement File > Open Source for installation folders, extracted dumps, loose character/raw-asset folders, supported disc/container images and Rivals Modkit/project folders. Keep the existing native File > Open workflow.

Use signature/layout-based detection, lazy enumeration, cancellable indexing, bounded caching, large-file-safe offsets and dependency-aware character assembly. Keep multiple source roots and deterministic overrides. Show missing and ambiguous companions rather than guessing them silently.

Source mounts are read-only. Stage native edits away from source installations/images, block source overwrite and validate newly written outputs. Do not load whole games or ISOs into memory, hash an entire installation before showing its tree, or extract everything by default. Guard extraction paths, decompression limits and symlink/reparse loops. Do not execute programs or scripts merely because they appear in a selected source folder.

Not every Smash version is distributed as an ISO. Add distinct platform/container readers and report unsupported/encrypted/missing-prerequisite cases truthfully. Do not bundle keys, bypass access controls or fetch secrets automatically. Do not claim the same Unreal container reader makes FighterZ, Sparking and Rivals mutually compatible.

CHARACTER CREATOR

Support imported and newly authored characters: mesh parts, skinning/weights, rigs, materials/textures, keyframe and curve editing, clip mixing, facial/morph channels, actual alternate-outfit meshes, states, attacks, hitboxes/hurtboxes, movement, throws, projectiles/articles, VFX, sound events and target-specific behavior. Animation conversion and gameplay translation remain separate operations with explicit losses and manual work.

RETARGETING

Build production-quality cross-rig authoring: skeleton fingerprints, reviewed semantic chain maps, bind/rest-pose and axis correction, hierarchy/proportion differences, twist/helpers, explicit scale policies, root/pelvis motion, IK/contact correction, hand/foot/prop constraints, non-destructive correction layers and target-rate baking. Keep source timing and gameplay-frame mappings explicit.

Use configurable measured acceptance criteria for pose/orientation error, contacts, foot sliding, loop seams and post-export decoded motion. Do not approve guessed commercial bone mappings. Do not call same-rig file conversion cross-rig retargeting or promise perfect one-click transfers for every skeleton pair.

Maintain original source rigs and separate target branches; an optional Goku authoring rig must not become a mandatory universal skeleton. Treat face/morph/costume/hair data separately where needed.

RIVALS/GOKU

After the native baseline and first source-provider slice, create an end-to-end Goku slice: assemble configured source assets, retarget selected clips onto a chosen target rig, correct contacts, bake, export, import through the installed Rivals Modkit's real supported workflow, create a minimal character/state/attack setup and test it when the local environment permits.

Verify the installed Modkit version, schemas, toolchain and actual interfaces. Do not invent APIs or treat extracted cooked runtime assets as a recovered authoring project. An interchange model or a preview is not a playable character. Record exactly which import/build/playtest steps ran.

No mandatory Blender/Maya/3ds Max, cloud or runtime AI dependency for the core editor. Optional DCC bridges remain supplementary. Target build tools may be required for cooking/package creation and must be disclosed separately. Do not install globally, accept EULAs or incur charges automatically.

## 6. Work to execute now

Do not stop at a roadmap. Complete the largest coherent tested increment in this order:

A. Verified offline recovery, clean replacement tree, pinned upstream import and provenance/license inventory.
B. Reproducible actual BrawlCrate baseline; small documented build fixes only as needed.
C. Independent FighterPort branding/settings/update safety, with native workflows retained and exercised.
D. One real folder-source workflow added to the application: enumerate a selected folder, identify supported Brawl resources, open one through existing editors using a staged editable copy, save/reopen and prove the original is unchanged. Unsupported image/other-game adapters remain explicitly pending.
E. Continue source/container providers, project services, cross-rig retargeting and Goku target integration in the recorded dependency order.

Do not try to implement all games in one untestable change. Do not delay the native working application behind a new universal framework. Do not carry old test counts forward as evidence for this new codebase.

Create or update `AGENTS.md`, `UPSTREAM.md`, `THIRD_PARTY_NOTICES.md`, `docs/MASTER_PLAN.md`, `docs/ARCHITECTURE.md`, `docs/DEVELOPMENT_STATUS.md`, `docs/FORMAT_SUPPORT.md`, `docs/TESTING.md` and `docs/plans/current.md`. These are intended new documentation paths, not assumptions about upstream files. Preserve the original upstream docs separately where new documents would collide. Keep exactly one authoritative next executable acceptance criterion.

At a real blocker, record the exact failing command/environment/dependency, preserve a coherent checkpoint and report completed versus blocked work without inventing success.

Final report: recovery location; upstream commit; local branch/commits; replaced legacy areas; build commands/results; executable location; observed GUI checks; native write/reopen results; source-immutability evidence; licensing gaps; game capabilities actually implemented; next task. Explicitly state that no remote deletion, visibility change or public upload occurred.

## References checked for this instruction

These sources support the repository/hosting/license/build distinctions; the product architecture above is a proposed implementation decision, not a feature claim.

- GitHub documentation, duplicating without the Fork workflow: `https://docs.github.com/en/repositories/creating-and-managing-repositories/duplicating-a-repository`
- Live destination metadata: `https://api.github.com/repos/DoctorFlux/fighterport-studio` — public, `fork: false` when checked. Codex must recheck live state.
- BrawlCrate root license: `https://github.com/soopercool101/BrawlCrate/blob/master/LICENSE` — GNU LGPL version 3, incorporating GPL version 3 terms. Component-level auditing remains necessary.
- Upstream project: `https://github.com/soopercool101/BrawlCrate/blob/master/BrawlCrate/BrawlCrate.csproj` — inspected `net472`, default x86 configuration. This is not a new-build success claim.
- GNU GPL conditions: `https://www.gnu.org/licenses/gpl.en.html` — preservation/modification notices and distribution conditions, as incorporated and supplemented by LGPL.
- GNU FAQ on private modifications: `https://www.gnu.org/licenses/gpl-faq.en.html#GPLRequireSourcePostedPublic` — private development does not itself require public release; distributing copies has separate obligations.

No repository was modified, deleted, made private or published while preparing this brief. No Windows application build or local-game validation was performed here.
