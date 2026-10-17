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
requires explicit user authorization, exact origin/public-status verification, and review
of all outgoing files/commits. The user authorized the source replacement upload on
2026-10-17. No automatic push, CI dispatch, release, artifact upload or binary distribution
is authorized. The source/component audit remains required before binary distribution.

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
