# Testing the restarted application

Use the installed Visual Studio 2022 MSBuild and .NET Framework 4.7.2 targeting pack.
The product stays net472/x86. `scripts/build.ps1` replaces inherited pre/post shell
hooks; no updater invocation, recursive output deletion or global tool installation.
ColorSmash remains source-present but unbuilt without a configured Rust toolchain.

Original baseline evidence is described in UPSTREAM.md and DEVELOPMENT_STATUS.md.
New focused native and WinForms/OpenGL tests are being integrated in tests/NativeSmoke.
No legacy test executable is an active dependency or evidence for the restart.

Use only original fixtures for portable checks. Commercial references require explicit
local configuration; never upload files, derivatives or captures. Keep actual native
decode, source hash comparison, programmatic live-control invocation, visual inspection,
manual clicking, separate native tools, Modkit build and in-game checks distinct.

The next checkpoint must open an original resource, display model/texture/material,
play/scrub CHR0, edit a real key, save a new native file and reopen it, then assert the
original SHA256. Test Open Source detection/staging and blocked source overwrite too.
Do not infer complete native fidelity from a shared BrawlLib writer/reader round trip.
