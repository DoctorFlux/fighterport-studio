# FighterPort Studio

FighterPort Studio is restarting directly on the complete BrawlCrate application source.
The main program is the real native resource editor, targeting .NET Framework 4.7.2 / x86.
The former WPF application is retained only in Git history and verified offline recovery.

Build on Windows with installed Visual Studio 2022 MSBuild and the .NET Framework 4.7.2
targeting pack: `./scripts/build.ps1`. Run: `./scripts/run.ps1`. Original-fixture checks:
`./scripts/test.ps1`. The scripts do not install tools globally or publish online.

Use native File > Open for the inherited editor workflow. File > Open Source adds a bounded
read-only folder mount and opens recognized Brawl resources as separate staged working copies.
See [testing](docs/TESTING.md) for the exact verified workflow and limitations.

File > Brawl Companions resolves a model BRRES, exact-name RGBA8 textures, and one
skeletal motion from separately selected files. Choose every dependency explicitly;
missing and ambiguous matches are shown. Save a `.fpcompanion` resolution and reopen
it with File > Open, Load resolution, or Resume last resolution after restarting.
Changed or missing sources stop restoration. Open assembled copy uses the native
editors. Save edited work separately as a native BRRES: the resolution records source
choices and rebuilds from those pinned originals, rather than storing pending edits.
Verify this slice with `./scripts/test.ps1 -Companions -Gui`.
Add source files also accepts canonical raw PAC/ARC containing nested BRRES. Indexed
paths distinguish same-name resources in different entries. Select companions explicitly;
Open assembled copy retains the enclosing PAC hierarchy and validates unrelated payloads.
Native edits save separately as PAC and reopen through the inherited tree/editor.
Compressed/opaque/redirect entries and unrepresentable header bytes fail explicitly.
Verify this subset with `./scripts/test.ps1 -Containers -Gui`.

For matching loose-dump/override folder layouts, open Brawl Companions > Dump folders.
Add non-overlapping roots, set priorities (larger numbers win) and Save roots. Discover
pages of candidate files; the list shows winners, shadowed sources and unresolved ties
for each complete relative file path. Stage only chosen files, return to companions,
choose native resources and Save resolution. Scans never replace pinned choices.
Cancellation and budget exhaustion leave saved work intact. PAC/PCS/container nesting,
compressed data and images are explicitly outside this provider. Check the original
fixture workflow with `./scripts/test.ps1 -DumpSources -Gui`.

Preferences, source-root registrations and staged edits live in `UserData` beside the
executable. Set `FIGHTERPORT_DATA_ROOT` to an absolute local folder to choose another
location. Preserve that folder when updating the application; staged edits are not
automatically deleted. Upstream profiles are not imported.

This is a source development checkpoint, not a released binary or a playable-fighter exporter.
The product roadmap is a source-character-bound animation/attack library and moveset
editor with semantic rigging/IK/FK and native target builders. The library/project/
retarget/gameplay/template/tool-provisioning services remain planned; see the
[master specification](docs/FIGHTERPORT_ANIMATION_ATTACK_LIBRARY_MASTER_SPEC.md).
All remaining games and conversion goals remain in [the master plan](docs/MASTER_PLAN.md).
Current evidence: [development status](docs/DEVELOPMENT_STATUS.md).
One next task: [current plan](docs/plans/current.md).

Derived from BrawlCrate / BrawlBox / BrawlLib. Original authors and licenses are retained.
See [UPSTREAM](UPSTREAM.md), [third-party notices](THIRD_PARTY_NOTICES.md),
[LGPL](LICENSE), and component license files. Independent product; no upstream endorsement.
The user authorized pushing each completed, passing source update to this existing
repository on 2026-10-17. Each update is committed and reviewed, with exact origin/public
status verified before a normal history-preserving push to main. The old application
remains recoverable in Git history. Tags, public PRs, CI dispatch, artifact uploads and
binary releases are not authorized. Local settings, game inputs, builds and test outputs
are excluded. Binary distribution remains pending the component/license audit.
