# FighterPort Studio

Read `CODEX_START_FROM_ZERO.md` (unaltered source requirements), `docs/MASTER_PLAN.md`,
`docs/DEVELOPMENT_STATUS.md`, `docs/plans/current.md`, and `docs/CODEX_CONTINUE_ALL_GAME_WORKBENCH.md` before continuing.
Also read `docs/CODEX_CLASSIC_BRAWLCRATE_STYLE_WORKSPACE.md` completely and `docs/UI_UX_PLAN.md`; current task instructions supersede dated receipts.
Also read `docs/CODEX_BRAWL_TEXTURE_FIX_AND_SRT0_CONTINUATION.md` completely and `docs/TEXTURE_DIAGNOSIS.md` for the tested boundary and remaining limitations.
Also read `docs/CODEX_UNIFIED_RESOURCE_EDITOR_AND_FULL_CHARACTER_PIPELINE.md`; its unified resource workspace queue supersedes the older format-button workflow.

- Existing standalone, local/non-AI Windows character workbench. No runtime cloud or DCC requirement.
- Keep all requested games, channels, editing tools and round trips in the master plan.
- Capabilities are per format/version/operation; tests prove declared subsets only.
- Never fake native exporters, substitute demo data behind importers, or advertise playable fighters.
- Game inputs are read-only and stay outside Git. No secrets, commercial assets, builds or dependency clones in commits.
- Preserve source branch, exact binds, Euler winding, timing, opaque resources and explicit losses.
- Build/test each increment; update capability records, status, next task and evidence together.
- Scripts: `./scripts/build.ps1`, `./scripts/test.ps1`, `./scripts/run.ps1`,
  `./scripts/doctor.ps1`, `./scripts/publish.ps1`, `./scripts/review.ps1`.
- Before each push verify exact origin and expected PUBLIC status (user-authorized 2026-10-16) against `config/repository.json`;
  review staged files AND diff. Never force push or reuse another repository.
- No automatic CI dispatch, paid service, global install, EULA acceptance or embedded input scripts.
- At a real boundary leave a tested checkpoint. Do not claim unobserved GUI/native-tool/in-game checks.
