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

Next: apply independent branding/settings/network safety and the first staged source
workflow, then build and run new native/GUI regression checks. Full game scope and
license/provenance gaps remain explicit. Old FighterPort test totals are historical.
