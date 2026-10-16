# FighterPort Studio

Windows-first local character workbench. Original skinned demos and a tested BrawlLib MDL0 v11 / CHR0 v4 subset. Early implementation, not a complete game porter or playable-fighter builder. No commercial assets, AI service or running DCC is needed.

## Setup and run

Windows x64, PowerShell 7, Git, .NET SDK **10.0.402**, .NET Framework 4.7.2+ for the native worker. The current workspace has a project-local SDK in `.tools/dotnet`; other checkouts may install the exact official SDK or use `scripts/setup.ps1`. No global install is performed by project scripts.

```powershell
./scripts/build.ps1 -Native
./scripts/test.ps1 -Native
./scripts/run.ps1
```

The app opens Aster. Click **Open Demo**, **Alternate Rig** or **Tail Demo**; choose a clip, Play/Pause or scrub. Drag to orbit, Shift-drag to pan, wheel to zoom. Select a bone in the left tree, choose a scalar channel/value and Set key; Undo/Redo and Save Project are active. New creates an empty custom character. Load Project reopens it with native resources. Skin/Skeleton toggles aid inspection. Generic rigs are not game reference profiles.

```powershell
. ./scripts/environment.ps1
Invoke-Dotnet run --project src/FighterPort.Cli -c Release -- native-fixture artifacts/demo-native
Invoke-Dotnet run --project src/FighterPort.Cli -c Release -- import artifacts/demo-native/aster.mdl0 artifacts/demo-native/punch.chr0 artifacts/imported.fpproj
```

Use a **new** fixture directory. In the desktop use Import MDL0 then Attach CHR0, or Load Project on `artifacts/demo-native/native.fpproj`. Exact bone names resolve clips; no auto-map needed for same-skeleton data. CHR0 display rate defaults to 30 fps because native files have no fps field. Export CHR0 writes a **new**, separate file; source data stays read-only. Unsupported step/fractional-key/dual-key subsets are rejected.

`./scripts/doctor.ps1` checks runtime/worker and bounded optional tool records. `./scripts/gui-smoke.ps1` actually launches/captures/exits the GUI. `./scripts/publish.ps1` stages framework-dependent desktop/CLI and native worker under ignored `artifacts/publish`; publishing here means local build output, no online release. `.NET 10 Desktop Runtime` must be installed to launch the apphost directly. Tests use an executable assertion harness, so `scripts/test.ps1` is the authoritative test command.

Only generated original fixture code is tracked. Dependency clones/caches, builds, private config, user projects and native/game binaries are ignored. License obligations and corresponding backend source are documented in [dependencies](docs/DEPENDENCIES.md). Manual-only CI is prepared and unrun. Before pushing use `scripts/review.ps1`, inspect `git diff --cached`, commit, then `scripts/push.ps1` which verifies privacy and remote commit identity.

See [complete scope](docs/MASTER_PLAN.md), [roadmap](docs/ROADMAP.md), [status](docs/DEVELOPMENT_STATUS.md), [current task](docs/plans/current.md), [format operations](docs/FORMAT_SUPPORT.md), [testing](docs/TESTING.md) and [GitHub status](docs/GITHUB_STATUS.md). Melee, Ultimate, Rivals II, Dragon Ball source adapters, native ancillary tracks, general DCC/interchange, automatic mapping/rebinding/optimization remain planned. No native-tool, retail-asset or in-game validation is claimed.

## Current local build and restricted FK

Launch `artifacts/publish/desktop/FighterPort.Desktop.exe` (verified Windows launch). Load `artifacts/demo-native/native.fpproj` for the genuine native-import demo; Open Demo starts the original canonical character. CLI executable: `artifacts/publish/cli/FighterPort.Cli.exe`; backend: `artifacts/publish/native/FighterPort.NativeWorker.exe`. Keep the publish subfolders together.

The CLI also exposes `map-template <source> <target> <profile.json>` and `retarget <source> <clip-index> <target> <approved-profile.json> <new-project>`. Review the unapproved exact-name suggestions and set the profile's explicit approval only after mapping is correct. Matching rest axes/hierarchy and unit rest scales are required; root motion needs CopyDelta policy. See [retargeting](docs/RETARGETING.md). Desktop mapping controls are the next task.

Current evidence: 23 automated tests passed, native and original GUI smoke passed, packaged CLI exercised. Published apphost ran on installed .NET 10.0.11; project-local SDK commands use 10.0.13. No remote CI or native-tool/in-game checks ran.
