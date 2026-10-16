# Verified source registry

Checked 2026-10-16 unless noted. No inference API is used at runtime.

- https://github.com/soopercool101/BrawlCrate at d56cbac89dc2cd0e5c3f923e1d03041b16902add: full clone inspected locally; BrawlLib.csproj (net472, platform targets, NuGet refs), LICENSE (LGPL-3.0), CHR0Node.cs, KeyframeCollection.cs, MDL0Node/MDL0BoneNode/MDL0ObjectNode, PrimitiveManager/Vertex, ColladaImporter/AssetDecoder/Exporter, Matrix/BigEndianTypes. APIs actually compiled and executed. See ADR 0002 for local modifications.
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/3-d-graphics-overview : Viewport3D/mesh normals/UVs/camera integration.
- https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core : .NET 10 LTS choice.
- https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json and https://dot.net/v1/dotnet-install.ps1 : SDK 10.0.402 metadata and local install.
- GitHub CLI 2.102.0 installed `gh repo create --help`, authenticated `gh api user`, repo absence check, create PRIVATE then `gh repo view --json nameWithOwner,isPrivate,url` and exact origin verified. No tokens recorded.
- actions/checkout v4 11d5960a326750d5838078e36cf38b85af677262 and actions/setup-dotnet v4 67a3573c9a986a3f9c594539f4ab511d57bb3ce9 verified via GitHub tag API. Workflow is manual-only, unrun.

- https://github.com/ScanMountGoat/ssbh_data_py and its MIT LICENSE: installed version 0.9.1 wheel, packaged skel_data.pyi and METADATA verify read_skel/SkelData/BoneData/save/calculate_world_transform. Genuine two-bone SKEL 1.0 write/read and local c00 skeleton inspection executed. No full Ultimate adapter claim.
- https://rivals2.com/workshop/knowledge-base/character-creation/rig-your-character/ and https://rivals2.com/workshop/knowledge-base/character-creation/texture-your-character/: current official beta constraints summarized in REFERENCE_INSPECTION.md. Modkit editor/cooking remains unrun.

All later game/tool starting links remain in the complete master specification. They are planned research, not evidence of working support. Melee/Rivals/Dragon Ball, full Ultimate model/animation conversion, FBX and DCC bridges remain unimplemented.
