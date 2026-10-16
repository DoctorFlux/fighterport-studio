# FighterPort Studio

Read `CODEX_START_FROM_ZERO.md` (unaltered source requirements), `docs/MASTER_PLAN.md`,
`docs/DEVELOPMENT_STATUS.md`, and `docs/plans/current.md` before continuing.

- Fresh standalone, local/non-AI Windows character workbench. No runtime cloud or DCC requirement.
- Keep all requested games, channels, editing tools and round trips in the master plan.
- Capabilities are per format/version/operation; tests prove declared subsets only.
- Never fake native exporters, substitute demo data behind importers, or advertise playable fighters.
- Game inputs are read-only and stay outside Git. No secrets, commercial assets, builds or dependency clones in commits.
- Preserve source branch, exact binds, Euler winding, timing, opaque resources and explicit losses.
- Build/test each increment; update capability records, status, next task and evidence together.
- Scripts: `./scripts/build.ps1`, `./scripts/test.ps1`, `./scripts/run.ps1`,
  `./scripts/doctor.ps1`, `./scripts/publish.ps1`, `./scripts/review.ps1`.
- Before each push verify exact origin and private status against `config/repository.json`;
  review staged files AND diff. Never force push or reuse another repository.
- No automatic CI dispatch, paid service, global install, EULA acceptance or embedded input scripts.
- At a real boundary leave a tested checkpoint. Do not claim unobserved GUI/native-tool/in-game checks.
