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
| ssbh_data_py | 0.9.1, CPython 3.10 Windows x64 wheel | Optional local skeleton inspector; Rust ssbh_data bindings. Upstream MIT license verified; packaged skel_data.pyi and METADATA inspected and actual read/write calls executed. Not included in desktop distribution. |
| NumPy | 2.2.6, CPython 3.10 Windows x64 wheel | Optional inspector matrix dependency; BSD-3-Clause plus bundled-component notices in package license. Not included in desktop distribution. |

Core/UI/CLI have no third-party NuGet dependencies. Worker and BrawlLib restore lockfiles are retained. `scripts/prepare-native.ps1` applies two documented modifications to the pinned source. `scripts/publish.ps1` stages local executables, backend license, NuGet license/third-party notices, upstream source archive and local patch under ignored artifacts; this is not permission or a claim of readiness for external binary redistribution. Do not bundle commercial game files. Original fixture code/assets may be redistributed under this project's fixture notice (tests/FIXTURES.md).


Continuation: ssbh_data_py 0.9.1 / NumPy 2.2.6 now also power the optional native model/animation worker and original fixture generator. Packaged modl_data/mesh_data/skel_data/anim_data/matl_data .pyi APIs were inspected and actually executed. This local publish remains workspace-dependent for Ultimate: Python and .deps/ssbh-python are not included in a relocatable binary package; no runtime cloud/DCC dependency.

prepare-native.ps1 now has **three** modifications: legacy resource reference, x86 bfloat bit-preserving write, and CHR0 indexed I12 encoding to preserve explicit tangents instead of regenerated dense-stream tangents. Source/patch/license are staged locally by publish.ps1. I12 is the existing genuine backend native format, not a custom exporter. File size can increase; decode validation remains mandatory.

Source-only research checkout ssbh_wgpu revision f5f9e14a83e48e0e9282c77add192b8fcfd4c24b verified normalized quaternion interpolation and rigid-parent skinning semantics. It is not linked, copied into runtime or distributed.