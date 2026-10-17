# Next executable acceptance criterion

R2 next: add a configured Brawl loose-dump folder provider with persistent multiple
source roots and explicit ordered overrides. Use original directory fixtures rather
than scanning a user's real installation implicitly. Discover supported loose BRRES,
TEX0 and CHR0 candidates lazily, with cancellation and explicit file/depth/resource
budgets. Show which root supplied each candidate and why an override applies; equal
priority collisions remain ambiguous until the user decides.

Acceptance: restore the configured roots and priority order after restarting; select
a model and texture/motion companions through the existing Companion Workspace;
visibly preview/play the selected resources, save a separate native BRRES and reopen
it. Expected override selection, missing/conflicting companions, cancelled discovery,
reparse rejection and unchanged source hashes must pass original-fixture checks.
Unsupported PAC/PCS nesting, compressed inputs and images must be reported explicitly
until their separate container-preserving readers/writers are implemented.

The explicit persistent companion milestone is complete: four separate source files,
missing/ambiguous cases, exact resource/hash restoration, native preview/play/edit/
save/reopen and two fresh product launches passed. Evidence is in DEVELOPMENT_STATUS.
After this one next task, continue installation/image/container providers, game-neutral
projects, measured cross-rig/contact/bake work and the configured Goku-to-installed-
Rivals-Modkit import/state/attack/build/playtest slice in MASTER_PLAN.md. This native
Brawl path is not an intermediate for other games. All work remains local.
