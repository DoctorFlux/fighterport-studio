# Testing the restarted application

Use Windows, installed Visual Studio 2022 MSBuild and the .NET Framework 4.7.2
targeting pack. The verified toolchain also has .NET SDK 9.0.300. Run from the
repository root:

```powershell
./scripts/doctor.ps1
./scripts/build.ps1
./scripts/test.ps1
./scripts/test.ps1 -Gui
./scripts/test.ps1 -Companions -Gui
./scripts/test.ps1 -DumpSources -Gui
./scripts/test.ps1 -Containers -Gui
./scripts/run.ps1
```

The app remains net472/x86/WinForms/OpenTK. `Directory.Build.props` and the build
script apply `build/Independent.targets`: no inherited updater invocation, recursive
output deletion, Cargo invocation or global tool installation. ColorSmash source is
present but its native helper has not been built. NuGet lock files are committed;
`build.ps1 -UpdateLocks` is an explicit dependency-maintenance operation.

The executable is `application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe`.
`run.ps1` opens that application directly. `test.ps1` builds and runs the new
`tests/NativeSmoke` executable, using a fresh ignored `artifacts/restart/native-*`
directory. `-Gui` additionally opens real editor windows on the interactive desktop
and writes `gui-*` captures. Allow those windows to finish; avoid interacting with
them during the short check. A 90-second watchdog fails a hung check. Logs and native
outputs stay local. No previous FighterPort tests or worker are used.

The original procedural fixture is a two-bone rigid paddle with four vertices, exact
identity binds, one material/sampler, a 32x32 RGBA8 quadrant texture and a looping
31-frame CHR0 v4 animation in BRRES/MDL0 v11. Native checks assert independent expected
positions, binds, texture pixels and the edited key after a new-file round trip.
The writer and reader share BrawlLib; this is not independent native-tool validation.

The GUI harness uses actual MainForm, SourceBrowserForm, wrappers, ModelEditControl,
CHR0 numeric control, Play button/timer and Program.Save/Open. It opens an editable
staged copy, previews/scrubs/plays, changes the 60-degree key to 35 degrees, refuses
a source overwrite while retaining dirty edits, saves a separate BRRES and reopens it.
It bypasses the operating-system folder/Save As pickers by supplying test paths.
This is automated handler/control verification, not manual clicking. Inspect both
window and OpenGL framebuffer captures before claiming visible geometry or textures.

Source tests cover signatures independent of filename, raw ARC/PAC with embedded
BRRES, malformed/truncated/compressed rejection, cancellation/outside-root rejection,
SHA256-verified staging, native output/stream protection, hard links, application/temp
directory rejection and persistence across processes. Distinct 8.3 aliases depend on
the volume; a skipped alias check is not a pass. Keep any focused junction/8.3 probes
separately identified from the repeatable harness. Commercial reference folders are
not loaded; their hashes can be compared with the verified recovery inventory.

Preferences, registrations and staging use executable-local `UserData`, or absolute
local `FIGHTERPORT_DATA_ROOT`. Harness runs use an isolated override. Settings tests
save/reload application and library scalars, collections, COLLADA options and complex
viewer settings in fresh instances and a separate process. Launch the actual product
separately to check its entry point and default portable profile. Preserve data folders
when updating; no automatic cleanup of edited copies or imported profile migration.

Original baseline build receipts: `D:/Users/ChatGPT/FighterPort-Upstream-d56cbac/`
contains `baseline-build.log` (Cargo pre-build failure), `baseline-repaired-build.log`
(managed application compiled using hook override) and `LocalBaseline.targets`.
See UPSTREAM.md for the exact boundary and DEVELOPMENT_STATUS.md for final evidence.

Review before each local commit with `scripts/review.ps1`, staged paths AND content
diff. `python scripts/verify-upstream.py` compares preserved baseline hashes and reports
intentional changes/missing files; new files are reviewed in Git. Source publication
requires exact origin/public-status verification and review of all outgoing files/commits.
The user's standing instruction on 2026-10-17 authorizes committing and pushing each
completed, passing source update; no repeat permission request is needed. Preserve history
with normal pushes to origin/main. CI dispatch, releases, tags, artifact uploads and binary
distribution remain unauthorized. The source/component audit remains required before
binary distribution.

No broad fidelity, arbitrary skeleton retargeting, separate native tool, Modkit build,
console or game result follows from these checks. Keep each format/version/operation,
manual GUI observation and native/in-game validation separately declared.

Companion verification uses four original input files: a model/material BRRES with no
texture or CHR0, two texture BRRES files offering the same texture name with different
pixels, and a loose CHR0 in another folder. The explicit second texture must win; no
first-match fallback is allowed. A base SRT0 v4 entry must retain its name/timing/loop
metadata. Missing/ambiguous choices, source-hash mismatch, absent source, wrong resource
identity, guarded manifest output and prohibited XML entities are checked separately.

`-Companions` runs companion native tests instead of the single-file suite; add `-Gui`
to exercise real model/texture/motion selectors, persistent resolution, Open assembled
copy, native texture/material controls, OpenGL poses/playback, numeric key edit and
separate Save/reopen. A separate hidden process restores the resolution and checks all
four source hashes again. No folder or Save As picker is driven by these tests.

The standalone executable also accepts a `.fpcompanion` argument through its normal
file-open path. The recorded product check launches it twice with an isolated data
root, restores the saved choices, invokes Open assembled copy, compares the exact
resolution identities persisted by the product and observes normal exits. Application
window captures and native framebuffer captures are visually inspected separately.
The resolution stores pinned source choices; save native edits to a separate BRRES.
See DEVELOPMENT_STATUS.md for exact latest paths, hashes and tested scope.

Multi-root original-fixture verification (`-DumpSources`, add `-Gui` for real controls):

- Three matching dump roots contain a model-only BRRES, separate same-name texture
  alternatives and a loose CHR0; an unrelated character has the same texture name.
  Discovery pages visit lightweight headers only. No staged directory may exist after
  discovery. It checks ordered winners, equal-priority ambiguity and explicit selection
  of a lower-priority file whose decoded pixels differ from the recommended source.
- A later priority change recommends a different file but must leave the explicit
  selected resource/hash unchanged. Root IDs/priorities and selection-time lineage
  survive two independent verification processes. A cancelled rescan leaves saved
  resolution bytes and pinned choices unchanged. Separate negative copies cover stale
  metadata, changed full hashes and missing inputs; originals are not mutated.
- Lower entry, depth, directory, candidate, header-byte and metadata budgets must
  report incomplete/truncated discovery. Missing roots, overlap, relative/network
  roots, unsupported compression/containers and a real Windows junction are checked.
- GUI checks operate the actual dump list/page/stage buttons, existing companion
  selectors, native OpenGL preview/Play/numeric editor and separate Save/reopen.
  All ten original inputs retain SHA256. Folder and Save As pickers are bypassed.

To verify the standalone entrypoint rather than the harness host, use a generated
original-fixture `dump-ui.fpcompanion` from the latest dump GUI evidence directory:

```powershell
./tests/NativeSmoke/VerifyDumpProduct.ps1 -Resolution <absolute-original-fixture-resolution-path>
```

This launches the actual FighterPort.Studio.exe twice with an isolated profile,
opens Dump folders to capture restored roots/priorities, returns to the companion
workspace, opens the assembled native copy, compares exact root/source/hash/resource
records persisted by the product, and observes normal exits. The second launch uses
the product's last resolution. It stores app-only captures and JSON under ignored
artifacts/restart/dump-product-*. Inspect the captures; this is Win32 control automation,
not manual clicking or independent native-tool/game validation. This script is for
original fixture resolutions; commercial derivatives/captures must remain local.

Run the existing default and `-Companions` suites separately; the new selection does
not replace them. Companion checks verify version 1 and 2 resolution compatibility without version 3 container attributes.


Container acceptance (`-Containers`, add `-Gui`): original raw ARC/PAC with two nested
branches, a selected model BRRES, texture/motion BRRES companions and an unrelated
same-name texture. Explicit indexed choices must create enclosing PAC output. Checks
compare ARC entry type/index/group/redirect/name/hierarchy, exact unrelated payload
hashes, original geometry, two blended influences, translated child bind/inverse bind,
material binding, every original RGBA8 texel, motion timing/loop and edited key.

The original mesh blends Root/Paddle by 75/25 at its bottom and 25/75 at its top, with
child bind pivot y=6. Native decoded weighted positions at the selected key are compared
with independent weighted rotation about that pivot: 0.002 native position units, 0.0001
for bind/weight checks and 0.001 degrees for the key. This is declared sampled/key-frame
LBS verification, not arbitrary skeleton support or a continuous-error bound. The real
GUI uses the inherited model preview, Play, CHR0 numeric edit, Program.Save and reopen.
The saved output is edited-gui.pac. Nine original source hashes must remain unchanged.

Negative cases cover cancelled inventory, entry bounds/depth, stale source/container
identity, changed unrelated payloads, reserved header bytes the native writer would
discard, compressed PCS and implicit embedded-resource replacement. Malformed inputs
are separate disposable copies. The first negative-fixture generation attempt correctly
hit the source write guard; generation was moved outside the mounted negative folder.
A source-label encoding issue found in captures was corrected before accepting the final GUI run.

Standalone product check (two launches, exact version 3 choices, normal native PAC open):

```powershell
./tests/NativeSmoke/VerifyDumpProduct.ps1 -NativeExtension pac -Resolution <absolute-original-container-ui.fpcompanion>
```

For explicit opt-in local native-header inventory only, after building the harness:

```powershell
./tests/NativeSmoke/bin/x86/Release/net472/NativeSmoke.exe --inspect-brawl <absolute-configured-PAC-path> <new-local-evidence-directory>
```

This registers read-only protection, reads bounded raw header metadata without a native
model/moveset decoder, records versions/unsupported entries locally and checks the one
input hash. It does not scan a drive, decode commercial geometry or demonstrate gameplay.
Commercial metadata files/captures/derivatives remain ignored. Header inventory and
companion admission are separate: unsupported MDL0 versions/opaque contents remain unsupported.
