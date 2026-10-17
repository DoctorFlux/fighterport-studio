# Clean restart status — 2026-10-17

Publication: on 2026-10-17 the user first authorized the source replacement upload,
then gave standing authorization to push each completed, passing source update.
Commit and review staged/outgoing content, verify the exact public origin and use a
normal history-preserving push to main. This includes the tested R2 checkpoint
`c3c991cfc4f7caa39b5fd3adbc5ce4bd17e09673`. No history deletion, repository recreation,
force/mirror push, visibility change, tags, PRs, CI dispatch, artifact upload or binary
release is authorized. Earlier local-only receipts below describe their historical
checkpoints; the subsequent standing instruction supersedes that publication policy.
Source upload adds no GUI/game evidence and does not resolve the distribution audit.
Verify publication by comparing remote main with the uploaded local HEAD. Settings,
inputs, builds and evidence remain local.

Current checkpoint: [shared library and reviewed FK](#shared-library-and-reviewed-fk--2026-10-17).
The first authoring increment follows the completed container checkpoint, preserving HEAD
9f9405fb925f50b4df33ec1ae41e17b1dd77b6dc and all newer passing work. Earlier receipts
below describe their historical checkpoints. docs/plans/current.md remains the only
authoritative next executable acceptance criterion.

## Initial restart receipts

R0 recovery and source baseline are complete. All 17,605 workspace files (2,668,733,450
bytes), including Git history, untracked briefs, ignored configurations, tool caches,
evidence and commercial inputs, were copied outside this tree and SHA256 verified.
Copied Git history passes `git fsck --full`; root/settings/Git/asset-root access rules
and owners match. Recovery: `D:/Users/ChatGPT/FighterPort-Recovery-20261017-01`.

Existing origin remains https://github.com/DoctorFlux/fighterport-studio.git, public,
independent repository; remote main was 86f698a3f75eeeb6ea82bd02c9623fe7019c4962.
Local branch: codex/clean-brawlcrate-restart. No remote writes occurred.

Pinned source d56cbac89dc2cd0e5c3f923e1d03041b16902add is imported under application/.
At import, all 6,235 raw blobs matched upstream before product edits; two security
exclusions are documented in UPSTREAM.md.
The legacy src/tests/scripts/docs/build structure was removed only after recovery.
Local input/settings/cache/evidence directories remain preserved and ignored.

Original Visual Studio MSBuild Debug/x86/net472 build failed on missing Cargo in its
pre-build hook. A build-only hook override compiled BrawlCrate, BrawlLib and Updater
without managed source changes. Original native BRRES/MDL0 v11/CHR0 v4/RGBA8 fixture
encode/edit/new-file/reopen passed against that baseline; the input SHA256 stayed
unchanged. No GUI or game result is claimed at this source-import checkpoint.

R1 and the first R2 loose-folder slice are now complete. The actual BrawlCrate-derived
application is FighterPort Studio v0.1.0 restart, still WinForms/net472/x86/OpenTK.
Its updater/report submission/Discord and automatic plugin subscription paths are
hard-disabled; local reports, independent portable settings and guarded source mounts
are implemented. Four native companion-resource commands use the renamed executable.
Root build policy applies to both scripts and direct Visual Studio project builds.

Local source-import commit: `47e95131cae1d98bfa0a0131c6b8d2d9913835b0`.
Initial functional integration commit: `f6980b4619c5eae14edf88e0cab6c2625caf8b3a`.
Later local checkpoints follow in Git history. No branch/tag push, PR, upload, CI dispatch or release occurred.
The canonical supplied brief has SHA256
`6edca8d2e33a648766b42cacc182d242a322efc544429a93710a8db452c5ed63`, matching recovery exactly.

Final primary `scripts/build.ps1` and `scripts/test.ps1 -Gui` succeeded. Primary build:
0 warnings, 0 errors, locked packages. Actual executable:
`D:/Users/ChatGPT/ROA2TOOL/application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe`.
Executable SHA256: `60dad92347f32198a23c50545cd8ab0a32274c7a94f9c64f94eac6ffe9e8d7db`.

Observed original-fixture results, all from this restarted codebase:
- Real SourceBrowser list/Open editable copy opens a verified staged BRRES in MainForm.
- Native TEX0/material controls and the actual OpenGL model editor were exercised.
  Captures visibly show the four-color rigid mesh upright, rotated 60 degrees, then
  edited to 35 degrees; the numeric field shows 35. Native Play advanced to frame 16.
- Actual Program.Save rejects mounted-source overwrite while retaining dirty edits,
  writes a separate BRRES and MainForm reopens it. Native decode confirms MDL0 v11,
  geometry/identity binds/material binding, TEX0 v1 RGBA8 pixels and CHR0 v4 timing/key.
- All seven original source-folder files retain their SHA256. Source BRRES:
  `60d532743a03adda12f518127e08c1eba55ee0a9bf3ff1be5a4e819829377321`.
  Native and GUI edited outputs both:
  `68a1211e192aa21e62d193cd2bf4b9b97df8db1802a4b63d508b70e2d9b701e4`.
- Settings scalar/collection/XML/binary/viewer values, mixed newlines, scoped reset
  and independent-process reload passed. Path/stream/map guards, actual hard links,
  malformed/compressed/outside-root/cancel cases and concurrent persistent mounts passed.
- Root separately launched the real executable with the saved original fixture using
  the default portable UserData path, visually inspected its resource tree/texture,
  confirmed settings creation and observed normal exit. This was not the harness entrypoint.
- All 679 existing commercial reference files matched verified recovery hashes after
  the work. None was loaded into these tests, rendered or added to Git.

Evidence under ignored `artifacts/restart/`:
- `build-Release.log`, `primary-build-20261017-054555.log`
- `test-wrapper-20261017-054521.log`
- `native-20261017-054523-481/result.txt`
- `gui-20261017-054526-294/result.txt`, `source.sha256` and captures 00 through 06
- `product-startup/result.json`, `product-startup/product-main.png`
- `commercial-source-hashes.json`, `native-header-observations.json`, `upstream-comparison.json`

The GUI check uses real controls/handlers in Application.Run, ordinary panel toggles,
active viewport selection and native camera framing. OS folder/Save As pickers are
bypassed; no manual-click claim. Earlier failed diagnostics are retained: inherited
Framework settings replacement failed under the EFS profile; the independent provider
resolved it without changing profile encryption/ACLs. Harness-only message-loop,
hidden-panel/camera and capture issues were corrected. Blank or occluded captures
were not accepted as preview evidence. D: exposed no distinct 8.3 alias (test skipped);
separate focused C: alias/junction probes passed, not part of the repeatable harness.

Remaining boundaries: one original rigid mesh/material/animation subset only; no
blended-skinning/all-channel claim, independent native-tool validation, Modkit build
or in-game check. ColorSmash remains unbuilt because Cargo is unavailable. Upstream
opaque DLL/resource provenance and corresponding-source distribution audit remain
open. Source staging rejects compression/opaque children, limits files to 64 MiB,
and does not discover companions automatically. The explicit companion slice below
now assembles a bounded subset. Native parsers and arbitrary user scripts are not
sandboxed. Other source providers, character projects, cross-rig/contact/bake services
and target gameplay/native-output routes remain planned in MASTER_PLAN.md.

The current authoritative acceptance criterion is maintained in plans/current.md.
Old FighterPort test totals are historical and are not evidence for this product.

## Persistent companion resolution — 2026-10-17

The explicit companion milestone is complete. File > Brawl Companions inventories
selected files, requires explicit model/texture/motion choices, and reports missing
or ambiguous exact-name textures. Version 1 `.fpcompanion` files pin absolute source
paths, SHA256, indexed native identities and versions. Load resolution, Resume last,
native File > Open and command-line file opening restore those choices after restart.
Hash mismatch, missing source or changed identity stops restoration before choices
are applied. The resolution records source choices; native edits save separately.

Assembly creates a guarded new BRRES, retains the base resource tree, rejects existing
resource-name replacement, imports only chosen textures/CHR0 and performs a fresh native
decode before opening. Unsupported companion channels are identified; a base SRT0 v4
fixture retains its name, 17-frame timing and loop flag through edit/save/reopen.
This is still WinForms/net472/x86 with the original native editor and renderer.
Final `scripts/build.ps1`: 0 warnings, 0 errors. Current executable remains
`application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe`, SHA256
`6704945c51413c69b53f02c0b37d91549d45aabb47b2b8af0e628f6a70373ed6`.
The edited GUI output SHA256 is
`bc0b14d683e533a4cecf5dd6b2985e35e20a80bc1206d4d7ae03ca14490b00c0`.

Executed checks:
- `scripts/test.ps1 -Companions -Gui` passed native and actual-control GUI checks.
  The model/material BRRES contains no texture/CHR0; textures are in separate BRRES
  files and the selected CHR0 is loose in a separate motion folder. Two same-name
  textures have different pixels; explicit selection of the second is preserved.
- Missing/ambiguous dependencies, expected-source-hash mismatch, missing source,
  invalid native identity, relative/network source paths, source-protected manifest output and XML entities fail
  explicitly. Negative manifests are altered copies; original inputs are unchanged.
- A separate fresh process restores the exact resolution, reassembles, decodes and
  rechecks four source hashes. Actual GUI selectors, resolution Save and Open assembled
  copy feed MainForm; visible native OpenGL poses and texture/material controls pass.
  Native Play advanced to frame 16, a key was edited to 35 degrees, mounted-source Save
  was blocked with dirty edits retained, and a separate saved BRRES reopened at 35.
- Two standalone FighterPort.Studio.exe launches restored saved choices with an
  isolated local profile, enabled the real workspace controls, opened newly assembled
  native BRRES files, persisted identical resource choices and exited normally.
  The second launch resumed the actual product's saved last resolution.
- Existing `scripts/test.ps1` native regression passed, including seven unchanged
  original source-folder files and the previously declared guard checks. D: distinct
  8.3-alias coverage remains skipped, not newly passed.
- All four companion inputs still match after the product launches. All 679 existing
  commercial reference files also match verified recovery; none was opened or imported.

Latest evidence (ignored, local only):
- `artifacts/restart/companions-native-20261017-090347-261/result.txt`
- `artifacts/restart/companions-gui-20261017-090351-036/result.txt`,
  `companion-source.sha256`, resolution files and captures 00 through 06
- `artifacts/restart/companion-product-20261017-090452-628/result.json` and app-only captures
- `artifacts/restart/native-20261017-085510-392/result.txt` (existing native regression)
- `artifacts/restart/companion-final-hashes.json`, `companion-commercial-source-hashes.json`

Final captures were visually inspected: actual dependency choices, four-color textured
mesh, upright/60-degree poses, numeric 35-degree edit, restored standalone workspace
and native resource tree/texture. No manual-click, external native-tool, Modkit or game
verification is claimed. Automated tests bypass OS file pickers; the fresh-product
check invokes the real Open button through Win32 and compares persisted identities.

Boundaries: only MDL0 v11, TEX0 v1 RGBA8 without palettes, and CHR0 v4 with exact bone
names; no retargeting, texture remapping, ARC/PAC companion assembly, automatic folder
discovery or dirty-edit project persistence. Base-resource presence/metadata checks
do not prove arbitrary opaque-field fidelity. Existing licensing/distribution gaps
remain unchanged. All work stays on the local restart branch with no publication.
Next: the single dump-folder/multiple-root acceptance criterion in plans/current.md.

## Multi-root Brawl loose dumps — 2026-10-17

Continued actual HEAD `20a8dc572446bff036c3388c0a101ddf98407ce2` on the existing
codex/clean-brawlcrate-restart branch. Only the supplied supplementary continuation
brief was initially untracked; no newer implementation work was discarded. The brief
is preserved at docs/FighterPort_Next_Codex_Milestone_2026-10-17.md, SHA256
`deffced27759919733f5aceed11cc58d55d4e7eb27409a194853c905de3a2718`.
The application/runtime/renderer/upstream pin and verified external recovery are unchanged.
This increment is local; the previous upload was not permanent future push permission.

R2 configured multi-root loose discovery is complete for the declared subset.
File > Brawl Companions > Dump folders configures/saves stable roots, enabled flags
and priorities, pages through lightweight file candidates, shows complete relative-
file/container scope and priority/conflict reasons, and stages only explicit choices.
The current Companion Workspace and native editor perform selected decoding, assembly
and editing. No dump is passed wholesale through AddSource. Root configuration and
version 2 companion snapshots preserve selection-time mount/container/native identity,
full source hash and explicit-choice reason; version 1 resolution loading still works.
Rescans/new priorities never change pins. Embedded base resources are not overwritten.
Working-set limits remain; native inventory is additionally capped at 4,096 entries.
All discovery budgets and partial/cancelled/unavailable diagnostics are documented in ARCHITECTURE.

Executed original-fixture checks:

- Doctor found installed MSBuild 17.14.10, net472 targeting pack and x86 runtime path;
  Cargo remains unavailable. Actual application build passed, 0 warnings / 0 errors.
- `scripts/test.ps1 -DumpSources -Gui` passed: base/override/conflict roots, genuine
  different texture pixels, priority winner, equal-priority ambiguity, unrelated
  same-name resources in different character paths and explicit lower-priority choice.
- Discovery construction/pages create no staged copies. Empty/disposed cursors never
  claim completeness. Six lowered budgets report truncation; missing roots, stale discovery metadata, changed full hashes, missing
  selected input, grown pinned-file hash limits, overlapping/relative/network roots,
  unsupported compression/container signatures and actual junction/reparse traversal/root rejection passed. Negative
  tests change only separate disposable copies, never the original fixture inputs.
- Mid-discovery cancellation passed. Cancelled rediscovery leaves a saved resolution
  byte-for-byte unchanged and keeps explicit native-resource pins. Two independent
  processes restored roots/priorities and historical selection lineage even after a
  different file became the highest-priority recommendation.
- Actual GUI page/stage buttons feed the existing companion selectors. Native texture/
  material controls and visible four-color OpenGL poses were inspected; Play advanced
  to frame 16. Actual numeric edit changed 60 to 35 degrees, protected-source Save was
  refused without losing dirty state, separate BRRES Save succeeded and MainForm
  reopened it. Native decoding verifies the edited key and declared geometry/binds/
  texture/timing subset. All ten original input hashes stayed unchanged.
- `tests/NativeSmoke/VerifyDumpProduct.ps1` launched the actual product executable
  twice through normal file-open startup, opened the real Dump folders window with
  restored roots/priorities, returned to companions and opened assembled native output.
  Exact root/source/hash/choice records persisted identically; both processes exited
  normally. The second launch used the product's last resolution, not the harness host.
- Existing default native/GUI regressions and companion native/GUI regressions were
  retained and rerun. Their seven/four original input hashes stayed unchanged. D:
  distinct 8.3 aliases remain a skip, not newly passed. Version 1 compatibility is
  now explicitly exercised by the companion suite.

Evidence (ignored/local):

- `artifacts/restart/dump-native-20261017-094925-375/result.txt`
- `artifacts/restart/dump-gui-20261017-094930-512/result.txt`, `dump-source.sha256`,
  catalogue/companion choices, texture/material, OpenGL poses, 35-degree edit and reopen captures
- `artifacts/restart/dump-product-20261017-095000-283/result.json`, roots/resume/native-open captures
- `artifacts/restart/native-20261017-094429-619/result.txt`, `gui-20261017-094431-266/result.txt`
- `artifacts/restart/companions-native-20261017-095033-796/result.txt`,
  `companions-gui-20261017-094540-243/result.txt`
- `artifacts/restart/dump-final-source-hashes.json` (ten inputs rechecked after final product launches)
- `artifacts/restart/build-Release.log`, `test-build-Release.log`

Executable: `D:/Users/ChatGPT/ROA2TOOL/application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe`.
SHA256: `160dfbb66906f02b6a0b3f4f6d6bdafdc579a9b51a16851179f73179678c843f`.
Edited GUI BRRES SHA256: `bc0b14d683e533a4cecf5dd6b2985e35e20a80bc1206d4d7ae03ca14490b00c0`.
The first compile exposed an XmlReader name ambiguity, corrected by qualification.
The first dump GUI run exposed a reentrant native checkbox/disposal callback; fixed
by avoiding rebuilding a ListView inside its ItemChecked notification. Failed evidence
remains local; only the corrected passing runs above support completion.

Limits remain explicit: file-scoped raw BRRES/TEX0/CHR0 discovery, not installations,
images, decompression or PAC/PCS companion nesting. Native assembly remains MDL0 v11 /
TEX0 v1 RGBA8 without palette / CHR0 v4 matching-name subset, not rig equivalence or
retargeting. Original geometry is still a rigid paddle, not blended skinning. Empty
SRT0 metadata does not prove animated SRT0 fidelity. No character-edit project, target
gameplay, separate native tool, Modkit or in-game verification was added. No commercial
input was decoded in these tests; only the explicit local assets configuration was
read (Brawl/Ultimate Mario roots, no configured Goku). Distribution/provenance audit
gaps, optional ColorSmash/Cargo limitation and native-parser sandbox limitations persist.

The next single acceptance criterion is Brawl container-preserving readiness in
plans/current.md. Full channels, nontrivial fixtures, actual character projects and
measured A-to-different-B/native-output/non-Brawl retargeting, then installed Rivals
Modkit movement/attack/build/playtest remain in the preserved milestone/master plan.
No branch/tag push, PR, remote upload, CI dispatch or binary release occurred this increment.

## Raw nested Brawl containers — 2026-10-17

Continued actual HEAD `19ae7b2aa14c24d9cd6e6e8ef0bbe6769e9cbda4`; no newer completed
work was replaced. The only initial untracked file was the supplied library master spec.
It is preserved byte-for-byte as docs/FIGHTERPORT_ANIMATION_ATTACK_LIBRARY_MASTER_SPEC.md
(case rename on Windows), SHA256
`63a211fdb31307530b2cbd84500ebd082516329708c8f6299fc1ee06db26a6d0`.
The user's requested filename takes precedence over the different path proposed inside
that document. MASTER_PLAN/AGENTS/ARCHITECTURE now trace SourceCharacterProfile,
AnimationAsset/MoveAsset/MoveInstance, actual-edit projects, semantic recognition,
interval IK/FK, native attack/template builders and vetted app-local provisioning.
Those are product requirements, not fabricated implemented services or attack data.

The current container-preserving acceptance criterion is complete for canonical raw
ARC/PAC 0x0101 -> BRRES with the existing MDL0 v11 / TEX0 v1 RGBA8 / CHR0 v4 gates.
Companion inventory now traverses nested ARC children and displays indexed resources
and native lineage. Version 3 resolutions pin enclosing BRRES identity; versions 1/2
remain readable. Explicit dependencies in different BRRES of the same PAC are imported
into the selected base, rather than skipped just because their source file is the same.
Embedded-resource replacement still fails explicitly. Assembly retains the enclosing
native root and exports a real PAC. Fresh native decode checks the selected subset;
the raw audit checks ARC names/type/index/group/redirect/hierarchy and exact unrelated
payload hashes. Opaque/compressed/redirect/empty entries and noncanonical reserved/name/
alignment/trailing bytes that the writer would discard are rejected before native decode.
Prior file/inventory/assembly quotas remain, with bounded audit depth/records/diagnostics.

Executed checks:

- `scripts/build.ps1`: actual WinForms/net472/x86 app, 0 warnings / 0 errors. Doctor
  confirms installed MSBuild 17.14.10 and net472 targeting pack; Cargo remains absent.
  Doctor now reports the actual reviewed-source publication policy from configuration.
- `scripts/test.ps1 -Containers -Gui`: original PAC with two nested ARC branches,
  selected model BRRES, separate texture/motion BRRES and unrelated same-name texture.
  Explicit choices remain distinct and restore in a fresh process. Native PAC assembly,
  key edit/new-file export/reopen and exact unrelated bytes/entry metadata passed.
- Original model: four known vertices, Root/Paddle with translated child bind y=6 and
  inverse bind -6, mixed 75/25 and 25/75 weights, one material, all 1,024 RGBA8 texels,
  31-frame looping CHR0. Decoded geometry/hierarchy/binds/weights/materials/timing/key
  and native weighted vertices match independent expected geometry and analytic LBS.
  Tolerances: 0.002 native position units, 0.0001 binds/weights, 0.001 degree key;
  declared key samples only, not arbitrary rig fidelity or continuous error bounds.
- Real companion selectors show nested identities. The actual native tree, texture/
  material controls and OpenGL preview were exercised; weighted 0/60-degree poses and
  35-degree edit were visually inspected. Play advanced to frame 15. Actual Program.Save
  blocked source overwrite without losing dirty edits, wrote a separate edited-gui.pac,
  and MainForm reopened its enclosing hierarchy and edited motion. The harness bypasses
  OS file pickers; this is real-control automation, not manual clicking.
- Cancelled inventory, entry bounds/depth, wrong saved enclosing identity, changed
  source hash, altered unrelated payload, reserved header loss, compressed PCS and
  implicit embedded-resource replacement fail explicitly. Negative inputs are copies.
- All nine original source/fixture hashes stayed unchanged, including a post-standalone-
  launch recheck. Exact untouched dependency BRRES payloads remain in the edited PAC.
- Two normal FighterPort.Studio.exe startups restored the final version 3 resolution,
  preserved exact source/container/resource choices, opened assembled.pac and exited
  normally. The second launch used the product's own last resolution. This is separate
  from the harness-hosted GUI test, not an external editor or a game run.
- Existing default native/GUI, companion native/GUI and multi-root native/GUI suites
  passed. Original seven/four/ten input hashes remain unchanged in their own suites.
  A focused check loads versions 1 and 2 without the new container attributes. The
  inherited distinct D: 8.3 alias probe remains skipped, not newly passed.

Ignored local evidence:

- `artifacts/restart/container-native-20261017-104434-483/result.txt`
- `artifacts/restart/container-gui-20261017-104438-067/result.txt`, container-originals.sha256,
  choices/texture/material/OpenGL/edited-key/native-PAC-reopen captures
- `artifacts/restart/container-product-20261017-104524-543/result.json`, resume/native-open captures
- `artifacts/restart/container-final-source-hashes.json`
- `artifacts/restart/native-20261017-103701-932/result.txt`, `gui-20261017-103704-827/result.txt`
- `artifacts/restart/companions-native-20261017-104030-262/result.txt`, `companions-gui-20261017-103718-585/result.txt`
- `artifacts/restart/dump-native-20261017-103454-406/result.txt`, `dump-gui-20261017-103458-472/result.txt`
- `artifacts/restart/build-Release.log`, `test-build-Release.log`

Opt-in configured Brawl inspection ran only the bounded read-only header inventory on
two existing files, with before/after hashes unchanged. The model file reports 17 native
records: ARC 0x0101, BRRES, MDL0 v9, TEX0 v1. The other file reports 120 records including
CHR0 v4, CLR0 v3, SRT0 v4 and four opaque/EFLS/REFF/REFT entries. Raw TSV, issues and
hashes remain ignored under configured-brawl-model-20261017-103017-042 and
configured-brawl-moves-20261017-103017-133. No commercial geometry, animation or gameplay
was decoded/previewed/edited/exported by this inspection. Finding v9 does not widen the
companion gate. There is still no configured Goku input; no Goku/Modkit/game result is claimed.

Final executable: `D:/Users/ChatGPT/ROA2TOOL/application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe`.
SHA256: `68a4e115c1c5500421b0669c18d7fc819561348dd5a230df76825acbdec4da3b`.
Edited native PAC SHA256: `82393ff3881adb49aaa6ffc24a8931261b083f2b51331413be8c53f48f36c19b`.
The first expanded negative test correctly hit source protection while generating into
an already mounted negative folder; generation was moved to an unmounted fixture folder.
An encoding issue visible in early source labels was fixed with UTF-8/Unicode escapes.
Early evidence remains local; only the final passing captures/results support this entry.

Remaining: PCS/decompression/recompression, opaque/redirect/noncanonical header fidelity,
MDL0 v9 and other required versions, palettes/alpha/mips/multiple materials, more complex
skinning/rest/scale/helper semantics and genuinely animated ancillary channels. The empty
SRT0 test still proves metadata only. Modified-BRRES arbitrary opaque data is not certified.
There is no new retargeter, imported attack behavior, character-edit project/library,
semantic IK/FK, target framework/template builder or automatic tool installer yet. No
separate native tool, Modkit, in-game or broad retail-character validation ran. Existing
licensing/corresponding-source audit and legacy parser sandbox limitations remain open.
No assets, derivatives, captures, settings, binaries or recovery contents enter Git.

At that checkpoint, the next task was the first visible source-bound library and
persistent edit-project increment on two genuinely different original rigs. Complete
Brawl fidelity, all wider game/target routes, native attack conversion, bidirectional Mario/
Snake proof, Melee and real Goku/Rivals template/attack/build/playtest remain required.
Source publication follows standing authorization after exact public-origin verification,
review and normal history-preserving push; publication success is verified separately.

## Source-bound library and projects — 2026-10-17

Historical receipt at b45fcc3; the shared/FK checkpoint below extends its declared limits. Current acceptance is in plans/current.md.

The first R3 authoring acceptance is implemented inside the existing application.
File > Character Library and .fpcharacter file opening provide source model groups,
source-bound clips, native previews, explicit manual drafts and independent slot instances.
Version 1 saves actual edited native archives and complete original native/source evidence,
resolution/mount/container identities, stable IDs/revision pins and skeleton/rest/bind/
inverse-bind/axis/unit interpretation fingerprints. The physical unit scale is unknown;
semantic roles are unapproved. Registration creates animation-only data, never hitboxes.
Draft startup/active/recovery/damage are explicit user-authored project values, not parsed
native attacks or compiled game slots. Revision 1 library sources stay immutable.

Observed/tested:

- Original rig A: two bones, identity binds, rigid four-vertex paddle. Rig B: shared
  misleading Root/Paddle names, three bones, child bind y=6, rotated helper descendant,
  larger proportions and 25/75 blended weights. Duplicate character/model/clip names
  and reordered profile records retain correct stable associations. Different-profile/
  rig assignment rejects even when names match; no transfer was silently performed.
- Actual native key edits (60 -> 35, GUI -> 25 degrees), separate native output and
  reopen pass. Decoded key/poses match independent intended rotation/LBS calculations
  within 0.001 degrees / 0.002 native position units. Native binds, texture samples,
  hierarchy, timing/loop and the second instance's unchanged 60-degree motion survive.
  The source library clip also stays at 60. This is sampled native editing, not a new
  retargeter, continuous curve proof or arbitrary model/channel fidelity certification.
- Real library assignment controls, inherited native preview/Play/numeric edit, Capture,
  project Undo/Redo, Save project, Export native copy and MainForm reopen were exercised.
  Undo/redo restores the actual native document. The bounded 32-command history was
  tested independently. History is session-only; actual saved state persists across load.
- Changed/missing external sources remain recoverable as saved project/native previews
  with visible diagnostics; assignment/capture/export block. Stale/unbound edits,
  unreviewed model/rig changes, tampered blobs/fingerprints and XML entities reject.
- Two fresh FighterPort.Studio.exe processes restore visible source/draft/assignment data,
  open the edited instance in the inherited model editor, export the exact persisted
  native revision, preserve project records on save and exit normally. Standalone checks
  use normal file opening, not a harness-only startup flag or an external editor.
- Nested PAC project capture/save/load/export/decode additionally retains enclosing ARC
  hierarchy and exact unrelated payloads. This extends project evidence without flattening
  the package. Existing single-source, companion, multi-root and container native/GUI
  suites pass; their original source hashes remain unchanged. The distinct 8.3 alias
  probe remains skipped. No commercial assets were loaded for this authoring increment.

The native framebuffer checks caught an inherited queued startup camera reset; product
framing now runs afterward. Final library captures show both rigs and the edited textured
pose without a test camera override. Earlier failed/partial evidence remains local and
does not support this checkpoint. Win32 product-window captures may omit OpenGL content;
the inspected native framebuffer captures are the visible geometry evidence. GUI actions
are control automation; no manual clicking, separate native-tool or in-game check is claimed.

Limits: 8 profiles with one selected CHR0 each, 64 drafts/instances, 32 MiB deduplicated
native/session blob data, 1 MiB metadata, 48 MiB document; 32 undo commands / 16 MiB
metadata. Working copies: 256 files/128 MiB per session; 256 sessions/1 GiB at creation.
Save/reload discards unused revisions and undo history. Capture supports the selected
CHR0 only; model/material/texture/other resource edits must be saved separately in the
native editor for now. Project saves are atomic and retain .previous. Native export
requires a new .brres/.pac file. No semantic recognition/review, IK/FK/contacts, native
attack compiler, non-Brawl project adapter, real target templates or provisioning exists yet.
All Brawl fidelity/version/channel and wider multi-game requirements remain queued.

Recovery remains untouched at D:/Users/ChatGPT/FighterPort-Recovery-20261017-01/workspace.
The upstream pin is unchanged: d56cbac89dc2cd0e5c3f923e1d03041b16902add. Baseline comparison
found no missing imported files; intentional product modifications remain identified.
No dependencies or licenses were replaced. Existing binary distribution audit remains open.
The single next acceptance is reviewed fingerprint-bound semantic mappings and measured
original rig A -> different rig B -> genuine native animation -> decoded target validation,
as specified in plans/current.md. Reverse and non-Brawl directions require independent proof.

Current local receipts (all generated assets/captures/binaries remain ignored):

- `artifacts/restart/library-native-20261017-112129-682/result.txt`
- `artifacts/restart/library-gui-20261017-112143-057/result.txt`, original hash list,
  library/model groups, both native framebuffers, edited pose and saved/native-reopen captures
- `artifacts/restart/library-product-20261017-112238-915/result.json`, two normal exits,
  exact restored records and native outputs through product controls
- `artifacts/restart/library-final-source-hashes.json`: four originals unchanged after product runs
- `artifacts/restart/native-20261017-110654-271/result.txt`, `gui-20261017-110655-808/result.txt`
- `artifacts/restart/companions-native-20261017-111235-346/result.txt`, `companions-gui-20261017-111239-252/result.txt`
- `artifacts/restart/dump-native-20261017-111308-271/result.txt`, `dump-gui-20261017-111312-560/result.txt`
- `artifacts/restart/container-native-20261017-111531-355/result.txt`, `container-gui-20261017-111535-895/result.txt`
- `artifacts/restart/build-Release.log`: actual application Release/x86/net472, zero warnings/errors

Executable: `D:/Users/ChatGPT/ROA2TOOL/application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe`.
SHA256: `7c88c1b89f1c3a52ff427c9d78b7ae78c22895ec017e08221992eb280b6d1137`.
Final edited native BRRES and both product exports SHA256:
`99affe0bd6106fb12fa034b8bba7b95f9086e34ea1a83eef970f233b45d42944`.
The master spec remains byte-identical (SHA256
`63a211fdb31307530b2cbd84500ebd082516329708c8f6299fc1ee06db26a6d0`).
Publication uses the standing reviewed-source authorization; the exact source commit and
remote verification are recorded in Git and the local publication receipt after pushing.
No assets, binaries, releases, public PRs or CI dispatch are part of this source update.


## Shared library and reviewed FK — 2026-10-17

Continued actual HEAD b45fcc34a4b437438cb94155f801c7be2feb5a73 without resetting or
replacing the BrawlCrate-derived application. The supplied shared-library/slots/transitions
brief is persisted verbatim as FIGHTERPORT_SHARED_ANIMATION_LIBRARY_SLOTS_TRANSITIONS.md,
SHA256 c706891b56834b088880d1882fb563d5a942d76c87001b7eaacc778e69db8667.
The previous library spec retains its supplied body under an explicit scope amendment;
its older byte-identical hash above describes that historical checkpoint only.
Models/animations are the active phase; gameplay scripting/damage/hitboxes/ACMD/PSA/Lua,
cancel/combo execution and playable compilation are deferred. Editable legacy draft fields
are behind Legacy drafts and animation assignments need no MoveAsset.

Checkpoint A: real explicit rig-review grid, exact source/target fingerprints and native
interpretations, semantic role/index pairs, helper policies, unit ratio and root policy,
with undo/redo and persisted review invalidation digest. Direct cross-rig Assign still
rejects. A separate reviewed FK route transfers original rig A (2 bones) to genuinely
different rig B (4 bones, changed rest axes/parents/proportions and blended skinning),
then independently proves B -> A. Native target model/binds/weights, unrelated resources
and enclosing archive remain intact; this is animation transfer, not mesh rebinding.

Fresh native CHR0 decode checks world bone positions/orientations and weighted mesh at
quarter frames, including fractional native Hermite loop-boundary phases and wrapped
start (129 samples for the 31-frame original). Independent analytic expectations test
both directions at declared fractional data/seam times. A 360-degree Euler key survives.
Forward maxima: position 4.44e-15 native units, orientation 2.565e-14 degrees, mesh
2.142e-6 native units; acceptance limits .002 / .05 / .005 respectively. These are
sampled bounds; general nonlinear/multi-axis winding and other loop conventions remain
unverified. The source's inherited loop return is preserved, not automatically improved.

Native GUI observations: empty mapping approval rejects, explicit test-authored mapping
is accepted, actual inherited model editor displays the textured blended target and Play
advances. The inspected OpenGL framebuffer shows deformation; native editor/library captures
are local. OriginalFixture author mappings are not commercial-rig review approval.
Native edits mark prior FK measurements historical so later manual changes are not passed
off as measured target constraints.

Checkpoint B: immutable disk-backed shared records/payloads and a bounded metadata cursor,
multiple clips on one source registration, explicit v1 migration preserving stable IDs,
actual native edits/provenance and legacy drafts, plus separate .fpcharacter projects.
New GUI projects use the app-wide store; v1 migration leaves its original file untouched
and requires a new save path. Project XML contains selected metadata/hash pins, no global
library or native blob inventory. Two projects pin the same immutable source clip revision;
editing target one to 25 degrees leaves target two at 180 and shared data unchanged.
A fresh process edits/undoes again with the same independence. Session undo/redo persists
actual current edits on save; undo history intentionally clears on restart.

Observed four fresh normal FighterPort.Studio.exe launches (two per separate project):
restored exact project records and working hashes, opened the inherited native editor,
exported through the real output path/button, saved and exited normally. Outputs match
previous native decode: project one 6a5b491f69877db387cc18d3192d1e5f62cd0ad0cdd2f5f705375ce5925e7a5a;
project two 0c7fa14b0e44b0c9284205f02c9814c6a458e4231084b8c4d5962d4447c343b9.
Product PrintWindow images prove UI restoration; focused OpenGL captures prove visible
geometry. File pickers are bypassed; no manual-click/external-native-tool claim.

Focused rejection checks: stale/unreviewed/tampered policies, incomplete helpers, non-unit
rest scales, animated ignored helpers/non-root translations, cancelled/fast-winding bake,
immutable metadata conflict, changed pinned records, concurrent writers, cancelled index/
import, page budget, cursor generation invalidation and missing pinned-library rejection.
Existing changed/missing original, reparse/hardlink, transaction/source guards and container
regressions remain passing. Post-product receipt rechecks seven original hashes across the
new focused fixtures, shared metadata/native revisions and the untouched second project.

Failures caught and resolved during development: native BRES sole-child replacement could
remove its group (insert-before-remove fixes it); inherited dense rotation encoding changed
fractional tangents (exact linear curves remain sparse; strict decode rejects other losses);
concurrent lock-file preflight needed bounded retry; the GUI playback test needed its own
bounded stop timer. Final passing tests below supersede the failed exploratory receipts.

Current limits: Brawl adapter MDL0 v11 / CHR0 v4 with existing RGBA8/canonical-container
admission. Additional clips currently import loose CHR0 v4. Shared records are immutable
revision 1; revision-management UI, portable bundles, cleanup/tombstones and shared map/
template records are queued. Maps currently reuse across clips within one project. Store:
4 GiB / 100,000 records; native blob <=32 MiB; metadata page <=256 visited / 64 results;
animation record <=128 KiB / character <=1 MiB. Projects: 8 profiles / 128 clips / 64
instances, drafts and maps / 1 MiB metadata; original session/undo budgets still apply.
FK: <=128 bones, 2..2048 frames, <=1 million scalar keys / 8 million vertex samples;
unit scales only, no compensation/billboards, static ignored helpers, no animated
non-root translation. Euler singularities, ambiguous fast winding and decoded errors
reject. Calibrated root translation is service-tested; native fixtures use in-place.
No automatic semantic recognition, IK/contacts, mirror, fitting, typed native-slot packs,
Idle/Fall anchors, template fill or non-Brawl adapter is implemented yet. No current
commercial fighter, external native-tool, Modkit import or game test was observed.
All Brawl fidelity/version/channel and wider source/target requirements remain queued.

Final primary build: Release/x86/net472, zero warnings/errors, no new dependency.
Executable: D:/Users/ChatGPT/ROA2TOOL/application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe.
SHA256: 812147291853a632fe07599298bdada76d9fc48fda8fd199c9c6fda8f9d1fccf.
Recovery remains untouched at D:/Users/ChatGPT/FighterPort-Recovery-20261017-01/workspace;
upstream pin remains d56cbac89dc2cd0e5c3f923e1d03041b16902add. Licenses/notices retained;
binary distribution audit remains open. Assets/settings/binaries/evidence stay ignored.

Local receipts:

- artifacts/restart/retarget-native-20261017-122507-964/result.txt
- artifacts/restart/retarget-gui-20261017-122350-296/result.txt and inspected map/viewport captures
- artifacts/restart/shared-native-20261017-122435-716/result.txt
- artifacts/restart/shared-gui-20261017-122451-150/result.txt and inspected catalogue
- artifacts/restart/library-product-20261017-122519-339/result.json (two project-one launches)
- artifacts/restart/library-product-20261017-122530-076/result.json (two project-two launches)
- artifacts/restart/shared-fk-final-source-hashes.json (post-product hashes/independence)
- artifacts/restart/library-native-20261017-121954-034/result.txt and library-gui-20261017-122008-530/result.txt
- artifacts/restart/container-native-20261017-122032-318/result.txt and container-gui-20261017-122036-138/result.txt
- artifacts/restart/dump-native-20261017-122050-597/result.txt and dump-gui-20261017-122055-187/result.txt
- artifacts/restart/companions-native-20261017-122109-989/result.txt and companions-gui-20261017-122113-581/result.txt
- artifacts/restart/native-20261017-122126-803/result.txt and gui-20261017-122128-403/result.txt
- artifacts/restart/build-Release.log; doctor passed with net472 targeting pack/MSBuild, Cargo absent (independent hooks do not require it)

The single next acceptance in plans/current.md is typed expandable animation slots and
persistent character references/anchors, with verified bindings or honest unbound status,
separate projects, undo/redo/restart/native-output checks. Mirror precedes later fitting;
IK, atomic template fill and non-Brawl/actual Modkit asset routes follow. Gameplay remains
a later independent milestone. Publication follows standing reviewed source authorization;
commit/remote SHA verification lives in Git and the ignored publication receipt. No release,
assets, binaries, tags, PR or CI dispatch is authorized by this checkpoint.
