# FighterPort Studio

Read docs/CODEX_CLEAN_BRAWLCRATE_RESTART.md completely, then UPSTREAM.md,
THIRD_PARTY_NOTICES.md, docs/MASTER_PLAN.md, docs/ARCHITECTURE.md,
docs/DEVELOPMENT_STATUS.md, docs/FORMAT_SUPPORT.md, docs/TESTING.md and
docs/plans/current.md. The clean restart supersedes the earlier bridge/WPF strategy.

- Main application is the actual BrawlCrate-derived WinForms application in application/.
  Preserve native editors; no external-editor launcher or rebuilt imitation shell.
- Local, standalone, non-AI core; no mandatory DCC/cloud. All requested games, channels,
  character/gameplay authoring, source providers and round trips remain in the master plan.
- Preserve upstream copyright, license texts, authorship and modification provenance.
  Root LGPL is not a blanket license for every component; see third-party audit.
- Capabilities are per format/version/operation. Inherited source is not new test evidence.
  No fake exporters, demo substitution, guessed approved mappings or playable-fighter claims.
- Game inputs are read-only and excluded from Git. Preserve exact binds, Euler winding,
  timing, native originals/opaque records and separate target branches with explicit losses.
- Build/test each increment; update evidence, capabilities/status and the single next task.
  Commands: scripts/build.ps1, test.ps1, run.ps1, doctor.ps1, review.ps1.
- Publication requires explicit user authorization. On 2026-10-17 the user authorized
  uploading this source replacement to the existing public origin/main; that instruction
  supersedes the reset's local-only restriction for this upload. Automatic pushes remain off.
  Verify exact origin and expected public status against config/repository.json, and review
  staged files AND the outgoing content diff before every authorized push. Preserve history;
  no force/mirror push, repository recreation, visibility change, automatic tags, public PRs,
  CI dispatch, artifact upload or binary release. Existing origin:
  https://github.com/DoctorFlux/fighterport-studio.git (public, independently hosted).
- No global installs, EULA acceptance, paid services or automatic source-script execution.
- Recovery outside the working tree must remain intact. At a real boundary record the exact
  failure and a coherent checkpoint; never claim unobserved GUI/native-tool/game checks.
