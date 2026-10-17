# Upstream provenance

Source: https://github.com/soopercool101/BrawlCrate
Selected commit: `d56cbac89dc2cd0e5c3f923e1d03041b16902add` (live default `master`, checked 2026-10-17).
Original app: BrawlCrate v0.42 Hotfix 1, assembly 0.42.0.1 (project Version 42.0.1).
Selection: current upstream default HEAD at retrieval, pinned exactly; no GitHub Fork operation.

The complete tracked source/application/library/editor/support/resource/documentation tree is
independently imported under `application/`, with **two explicit security exclusions** below.
No upstream .git metadata replaces FighterPort history. Nested .github remains inactive source.
No submodules or Git LFS entries were present. `docs/upstream-tree.tsv` records all 6,237 Git
entries; `docs/upstream-manifest.json` records Git blob IDs and SHA256. Each of the 6,235 imported
files was compared against its exact upstream Git blob before product modifications.

Excluded, retained only in external pristine staging/recovery:
- `Updater/CredentialsHelper.exe`: SHA256 407fb7e3ce18d4e659cc9262ac9d051491dca7e773b06eaba4748b243c55e152;
  opaque credential helper without identified source. Not executed, committed or distributed.
- `BrawlLib/Internal/Windows/Controls/Hex Editor/HexBox.snk`: SHA256
  ab3d1fb2a63685bdce5b848b25db8d420971ff5f4b2dc2d40f23d0bc684a1b1f;
  unused private signing-key blob. Not imported or distributed.

Baseline build, before branding/runtime/UI changes: installed Visual Studio 2022 MSBuild
17.14, original solution Debug/x86/net472. Untouched build restored dependencies and built
BrawlLib/Updater, then failed MSB3073/9009 because the pre-build hook invokes unavailable Cargo.
A local CustomAfterMicrosoftCommonTargets override of PreBuild/PostBuild compiled the actual
BrawlCrate executable without managed source edits. An inherited BrawlLib PreBuildEvent emitted
an empty-path shell syntax error; it performed no observed deletion. The independent product
build now also overrides PreBuildEvent/PostBuildEvent. Those bounded hooks avoid updater
-killAll, recursive output deletion and shell copying; ColorSmash compilation remains unverified.
No old FighterPort bfloat/animation-codec patches were silently carried into this baseline.
Original checkout and failed/success logs remain external; local copies of logs are ignored.

FighterPort modifications dated 2026-10-17 are separate from that verified baseline:
- Product/executable/version/settings identity, upstream lineage in About; original copyrights remain.
- Shared portable settings provider preserving the existing scalar/XML/binary representations;
  no upstream profile migration or encryption/ACL changes.
- Hard gates for inherited updater/issue submission/Discord; local diagnostic reports; safe defaults,
  scripting opt-in/whitelist, no inherited cleanup or file-association writes.
- File > Open Source staged folder workflow and source write protection.
- Explicit Brawl model/texture/motion companion selection, versioned hash-pinned local
  resolution files, guarded assembly with fresh native output validation, and native-editor
  resume through File > Open. This remains a Brawl-specific subset, not a universal project.
- Bounded independent build hooks and new original-fixture regression harness.
- Multi-root Brawl loose-dump metadata discovery and explicit file-scoped overrides,
  independent root profiles, selection-time mount/container lineage in version 2
  companion resolutions (version 1 loading retained), existing native editor integration,
  and original base/override/conflict/restart/native-GUI tests. Native format gates remain unchanged.
- Raw nested ARC/PAC companion traversal, version 3 enclosing-container identities,
  canonical-header admission and exact unrelated-payload preservation audit, native PAC
  assembly/editor integration, opt-in read-only header inventory and original blended-
  skinning/nonidentity-bind tests. Model/texture/motion version gates remain unchanged.

The baseline manifest remains unchanged as a comparison anchor. Git diffs and local commits record
each modification. `scripts/verify-upstream.py` reports divergence and validates baseline entries.
No blanket authorship/license replacement is intended. See THIRD_PARTY_NOTICES.md before any
binary distribution. Corresponding-source/component audit remains incomplete.
