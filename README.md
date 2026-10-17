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

This is a local development checkpoint, not a released binary or a playable-fighter exporter.
All remaining games and conversion goals remain in [the master plan](docs/MASTER_PLAN.md).
Current evidence: [development status](docs/DEVELOPMENT_STATUS.md).
One next task: [current plan](docs/plans/current.md).

Derived from BrawlCrate / BrawlBox / BrawlLib. Original authors and licenses are retained.
See [UPSTREAM](UPSTREAM.md), [third-party notices](THIRD_PARTY_NOTICES.md),
[LGPL](LICENSE), and component license files. Independent product; no upstream endorsement.
No branch, tag, public PR, artifact or release is automatically published for this restart.
