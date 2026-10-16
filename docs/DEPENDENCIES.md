# Dependencies and licenses

Checked 2026-10-16. Dependencies are local ignored tools, not vendored binaries.

| Dependency | Revision | Integration / license |
|---|---|---|
| .NET SDK | 10.0.402 | Official Microsoft SDK installed in .tools/dotnet; global.json pins exact SDK. Framework-dependent app requires .NET 10 Desktop runtime. SDK/runtime MIT components plus Microsoft distribution notices; installer source official. |
| WPF/System.Numerics/System.Text.Json | net10.0 | SDK/framework references; no extra NuGet frontend packages. MIT .NET sources/notices. |
| BrawlLib/BrawlCrate | d56cbac89dc2cd0e5c3f923e1d03041b16902add | Modified LGPL-3.0 library in net472 x86 worker. Preserve license and exact corresponding source/local patches, permit library replacement and fulfill LGPL requirements before redistribution. Private Git does not waive licensing. |
| OpenTK | 1.1 upstream assembly | Required by BrawlLib. Upstream bundled binary; MIT/X11 license/source provenance needs a final redistribution audit; no public binary release is authorized. |
| System.IO.Hashing | 8.0.0 | BrawlLib/worker NuGet, .NET MIT; content hashes locked. |
| System.Resources.Extensions | 8.0.0 | Legacy resource loading under SDK MSBuild, .NET MIT; content hashes locked. |
| System.Buffers, System.Memory, System.Numerics.Vectors, Unsafe | see lockfiles | NuGet transitive framework dependencies; preserve package license metadata. |

Core/UI/CLI have no third-party NuGet dependencies. Worker and BrawlLib restore lockfiles are retained. `scripts/prepare-native.ps1` applies two documented modifications to the pinned source. `scripts/publish.ps1` stages local executables, backend license, NuGet license/third-party notices, upstream source archive and local patch under ignored artifacts; this is not permission or a claim of readiness for external binary redistribution. Do not bundle commercial game files. Original fixture code/assets may be redistributed under this project's fixture notice (tests/FIXTURES.md).
