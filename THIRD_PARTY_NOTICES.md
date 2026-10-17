# Third-party notices and distribution audit

This is an independently hosted derivative, not a claim of original authorship or upstream
endorsement. Preserve all notices in application/ and the root LICENSE. This inventory is
source evidence and an unresolved distribution checklist, not legal clearance.

BrawlCrate derives from BrawlBox and BrawlLib. Upstream credits include soopercool101,
NuclearNecrosis, Simon381, Onium, Bryan Moulton (Kryal), BlackJax96, libertyernie, Sammi Husky,
Squidgy and other credited contributors. Upstream README, CONTRIBUTING, Changelog, About
strings and source-specific copyright notices remain available in application/.

| Component | Evidence / license status |
|---|---|
| BrawlCrate/BrawlBox/BrawlLib | Upstream root LICENSE: LGPL version 3 incorporating GPL version 3. Root copy retained; incorporated GPL text in licenses/GPL-3.0.txt. Component-specific notices also apply. |
| ColorSmash 0.3.2 | application/ColorSmash/LICENSE is AGPL-3; Peter Hatch and soopercool101 credited. Source/Cargo.lock retained; helper not built/shipped by current app build. Wildcard Cargo.toml declarations need locked toolchain/dependency audit. |
| OpenTK.dll | Required inherited direct reference; file version 1.0.278.44921; SHA256 931e38b47b824c316a55b5a54d55650c978ca90dde1daea02299eddd94e719c0. Exact modified source/binary provenance and corresponding notices still need verification. No binary redistribution clearance. |
| discord-rpc.dll | Inherited opaque dependency retained in source import; runtime use hard-disabled. Exact provenance/license/source verification pending; omit from distributed packages until resolved. |
| CredentialsHelper.exe | Excluded opaque executable; source absent. Hash/reason in UPSTREAM. Never executed. |
| HexBox.snk | Excluded unused private signing-key blob; not a distributable dependency. |
| Ookii.Dialogs | BSD-style license.txt and source notices retained in BrawlLib Windows forms tree. |
| NVIDIA NVDXT | NVIDIA 2007 / Ignacio Castano MIT notice retained in NVDXT.cs. |
| Deque/GenericDeque | Leslie Sanford 2006 MIT source notices retained. |
| GIF/NeuQuant | Anthony Dekker 1994 permissive notice; Kevin Weiner/gOODiDEA encoder lineage preserved. |
| IronPython / StdLib 3.4.1 | NuGet dependency of application; package license/source and transitive dependencies must accompany future distribution as applicable. |
| System.IO.Hashing 8.0.0 | BrawlLib NuGet dependency; resolved package/license inventory required before distribution. |
| Octokit 5.0.0 | Updater project dependency; updater source retained, runtime updates disabled and updater not part of independent app build. |
| Doxygen 1.8.14 | Inherited build/documentation tooling dependency; not a product license blanket. |
| Icons/resources/documentation | Preserved upstream resources; per-asset provenance/redistribution audit remains open. No user game assets imported. |

Private local development does not constitute a completed distribution audit. Before any binary
distribution, identify exact source/build inputs and notices for every included component, resolve
opaque binary/resource provenance, prepare exact corresponding source and modification notices,
and verify applicable LGPL/GPL/AGPL conditions including replacement/relink requirements where
applicable. No installer, release, public upload or automatic publication is authorized here.

New FighterPort source files mark their modifications/date and compatible terms. Original fixture
design/output permission is stated in the new test generator; generated game-format fixtures are
local and ignored. Commercial input folders and their derivatives remain outside all commits.
