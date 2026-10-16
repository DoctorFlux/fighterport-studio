# FighterPort Studio

Standalone local Windows character workbench for Brawl, Melee, Ultimate and Rivals 2, with Dragon Ball source adapters and optional Blender/Maya/3ds Max bridges on the full roadmap. Current implementation is a tested subset, not a complete game porter or playable-fighter builder. No cloud/AI or running DCC is required.

## Run this workspace

Launch `artifacts/publish/desktop/FighterPort.Desktop.exe`. **Open** adds native sources or a saved workspace. Expand a Brawl PAC, double-click a model, then open its motion PAC and activate skeletal/ancillary groups. Material references resolve textures through native BRRES/ARC scope. **Save workspace** keeps verified external originals and canonical state; texture pixels are not embedded. See the [click-by-click Mario and original-fixture workflow](docs/RESOURCE_WORKSPACE.md).

The initial preview supports CHR0 skeletal motion, original VIS0 draw visibility and direct nonrotating Maya SRT0 sampler-0 UV scale/translation on one timeline. Supplied Mario Wait2 has four evaluated eye tracks; extra samplers and native shader fidelity remain incomplete. Unknown resources stay in their original sources and appear in the native tree. **Demos** contains original rigs. Legacy FPPROJ and Ultimate native model/animation sources still open through the same command; Ultimate materials/ancillary groups remain pending.

**Tools > Map / Retarget** retains semantic/manual roles and approval-bound strict FK/rest-axis modes, including a tested quaternion/rest-parent translation subset. **Convert** offers all four destinations; full-character output blocks until implemented. Explicit skeletal-only CHR0/NUANMB output uses measured bake and genuine native write/decode, with disclosed ancillary/model omissions. **Mario-to-Mario cross-rig transfer remains incomplete.**

## Build and verify

Windows x64, PowerShell 7, Git, .NET SDK **10.0.402**, installed .NET 10 Desktop Runtime and .NET Framework 4.7.2+ for the Brawl worker. This workspace has a local SDK in .tools/dotnet. Ultimate operations additionally use Python 3.10 and local pinned ssbh_data_py 0.9.1 / NumPy 2.2.6 in .deps/ssbh-python; the optional backend is not bundled into a relocatable desktop package. No global installation is performed by build scripts.

```powershell
./scripts/build.ps1 -Native
./scripts/test.ps1 -Native -Ultimate
./scripts/run.ps1
./scripts/gui-smoke.ps1 -Mapping
./scripts/publish.ps1
```

For the supplied local references only, add `-LocalAssets` to test.ps1. The assertion runner is an executable; use the test script, not dotnet test. Latest full suite: **44 passed, 0 failed**, including opt-in local-asset cases. Published resource workflows passed for original and Mario sources; existing mapping/playback coverage is retained. See [status](docs/DEVELOPMENT_STATUS.md) for exact evidence and limitations.

publish.ps1 stages local framework-dependent desktop/CLI/native binaries and backend notices/source under ignored artifacts/publish. It uploads nothing. Keep the publish subfolders together and retain this workspace for the optional Ultimate backend. doctor.ps1 checks installed runtime and worker; fresh-machine setup, native-tool GUIs, Modkit/cooking, DCC and game checks remain unrun.

Generate original genuine Brawl native fixtures with:
```powershell
. ./scripts/environment.ps1
Invoke-Dotnet run --project src/FighterPort.Cli -c Release -- native-fixture artifacts/new-native-fixture
```

The fixture output must be new. Original SSBH native fixtures are generated/tested through scripts/generate-ultimate-fixture.py. These are real library-generated formats; fixture materials are neutral test data, not game-ready shaders.

## Source and complete scope

This repository is **public by explicit user instruction**. Before each source push, scripts/push.ps1 verifies exact origin and live expected visibility from config/repository.json, then checks the remote commit SHA. Run scripts/review.ps1 and inspect the staged diff before committing. No game assets, derived projects/captures/reports, credentials, local configuration, builds or dependency clones belong in Git. CI remains manual-only and unrun.

[Master requirements](docs/MASTER_PLAN.md), [roadmap](docs/ROADMAP.md), [next acceptance criterion](docs/plans/current.md), [format operation evidence](docs/FORMAT_SUPPORT.md), [tests](docs/TESTING.md), [dependencies](docs/DEPENDENCIES.md), [GitHub status](docs/GITHUB_STATUS.md). The complete Melee/Rivals/Dragon Ball/legacy, channel/material, optimization and DCC scope is preserved.