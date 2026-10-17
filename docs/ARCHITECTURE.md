# Architecture after the clean restart

The active executable is `application/BrawlCrate/BrawlCrate.csproj`, branded FighterPort Studio.
It retains upstream WinForms, .NET Framework 4.7.2, x86, OpenTK/OpenGL, native wrappers,
resource tree/property grid and ModelEditControl. BrawlLib is an in-process native resource
library. This is the imported application, not a separate editor launched by a WPF shell.

`application/` preserves the upstream tracked layout, including support sources, resources,
original documentation and nested inactive CI configuration. Root docs/scripts describe this
independent product. Updater and ColorSmash sources remain present; upstream updater execution
is disabled, and ColorSmash compilation is pending an explicitly configured local Rust toolchain.
Opaque credential helper and private signing key are excluded; see UPSTREAM.md.

Folder source workflow: read-only mount -> bounded signature inventory -> verified staged copy
-> existing native editor -> new output -> native reopen. Protected source roots must guard
native export/save paths. No arbitrary source script executes. Native File > Open remains.
The first provider is a narrow loose uncompressed Brawl resource slice, not a game-install,
encrypted image, arbitrary archive or dependency-assembly implementation.

Future modules attach to this visible foundation: source providers; game-specific asset
adapters; native resource layer; game-neutral character project preserving native originals;
animation/retargeting services; separate target asset writers and gameplay compilers; validation.
Other games will not be encoded through MDL0/CHR0 as a universal intermediate. Modern/64-bit
workers may isolate large archives or incompatible dependencies without a speculative UI rewrite.

Project state must preserve exact IDs/binds/winding/timing, opaque records, semantic maps,
correction layers, source and target branches, companion ambiguity and explicit losses.
Same-rig conversion, cross-rig retargeting, mesh rebinding and gameplay translation are distinct.
Local non-AI operation and no required DCC remain invariants. Legacy FPPROJ/FPWORK compatibility
is not a restart prerequisite; old implementation is only in recovery/history.
