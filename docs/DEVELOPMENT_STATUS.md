# Clean restart status — 2026-10-17

R0 recovery and source baseline are complete. All 17,605 workspace files (2,668,733,450
bytes), including Git history, untracked briefs, ignored configurations, tool caches,
evidence and commercial inputs, were copied outside this tree and SHA256 verified.
Copied Git history passes `git fsck --full`; root/settings/Git/asset-root access rules
and owners match. Recovery: `D:/Users/ChatGPT/FighterPort-Recovery-20261017-01`.

Existing origin remains https://github.com/DoctorFlux/fighterport-studio.git, public,
independent repository; remote main was 86f698a3f75eeeb6ea82bd02c9623fe7019c4962.
Local branch: codex/clean-brawlcrate-restart. No remote writes occurred.

Pinned source d56cbac89dc2cd0e5c3f923e1d03041b16902add is imported under application/.
6,235 raw blobs match upstream; two security exclusions are documented in UPSTREAM.md.
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
The functional integration is recorded in the follow-up commit containing this status;
`git log -2` identifies both. No branch/tag push, PR, upload, CI dispatch or release occurred.
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
and does not assemble companions. Native parsers and arbitrary user scripts are not
sandboxed. Other source providers, character projects, cross-rig/contact/bake services
and target gameplay/native-output routes remain planned in MASTER_PLAN.md.

Next authoritative acceptance criterion: Brawl companion assembly in plans/current.md.
Old FighterPort test totals are historical and are not evidence for this product.
