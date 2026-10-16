# FighterPort Studio

Standalone local Windows character workbench for Brawl, Melee, Ultimate and Rivals 2, with Dragon Ball source adapters and optional Blender/Maya/3ds Max bridges on the full roadmap. Current implementation is a tested subset, not a complete game porter or playable-fighter builder. No cloud/AI or running DCC is required.

## Run this workspace

Launch `artifacts/publish/desktop/FighterPort.Desktop.exe`. **Open Demo** uses original skinned rigs and motions. **Load Project** can open the locally imported `artifacts/local-mario/brawl.fpproj` or `ultimate.fpproj`; those commercial-derived files are ignored and never shipped with source.

Use **Open Brawl PAC** to choose a main MDL0 resource, then a CHR0 from a motion PAC. Main and shadow models are listed separately. **Open Ultimate model** loads NUMDLB with neighboring skeleton/mesh/material dependencies; **Attach NUANMB** adds a skeletal clip. Scrub/play renders actual weighted geometry with neutral shading. Native textures, material/visibility animation and helper constraints are not yet evaluated.

**Map / Retarget** provides source/target selection, manual mapping, semantic role suggestions/correction/locks, explicit approval and dual preview. Strict FK and the separately selected rest-axis/hierarchy subset have different gates; see [retargeting](docs/RETARGETING.md). **Bake for CHR0 / NUANMB** makes a measured new clip; export writes a separate native file and validates decoded results. Original same-rig cross-format fixtures pass. **Mario-to-Mario cross-rig transfer remains incomplete.**

## Build and verify

Windows x64, PowerShell 7, Git, .NET SDK **10.0.402**, installed .NET 10 Desktop Runtime and .NET Framework 4.7.2+ for the Brawl worker. This workspace has a local SDK in .tools/dotnet. Ultimate operations additionally use Python 3.10 and local pinned ssbh_data_py 0.9.1 / NumPy 2.2.6 in .deps/ssbh-python; the optional backend is not bundled into a relocatable desktop package. No global installation is performed by build scripts.

```powershell
./scripts/build.ps1 -Native
./scripts/test.ps1 -Native -Ultimate
./scripts/run.ps1
./scripts/gui-smoke.ps1 -Mapping
./scripts/publish.ps1
```

For the supplied local references only, add `-LocalAssets` to test.ps1. The assertion runner is an executable; use the test script, not dotnet test. Latest full suite: **38 passed, 0 failed**, including three opt-in local-asset cases. GUI mapping and both real-reference playback smokes passed. See [status](docs/DEVELOPMENT_STATUS.md) for exact evidence and limitations.

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