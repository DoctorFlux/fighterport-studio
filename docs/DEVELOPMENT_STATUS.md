# Clean restart status — 2026-10-17

Current checkpoint: [persistent companion resolution](#persistent-companion-resolution--2026-10-17)
is implemented and verified. The initial restart receipts below remain historical;
the companion entry records the latest executable, checks and next-task boundary.

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
