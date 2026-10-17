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
Root `Directory.Build.props` applies the same bounded hook override to direct project
builds in Visual Studio; the authoritative build target is the application project,
not the inherited updater/release solution configuration.

Folder source workflow: read-only mount -> bounded signature inventory -> verified staged copy
-> existing native editor -> new output -> native reopen. Protected source roots must guard
native export/save paths. No arbitrary source script executes. Native File > Open remains.
The first provider is a narrow loose uncompressed Brawl resource slice. The companion
workspace below assembles explicitly selected Brawl resources; game-install, encrypted
image, arbitrary archive and automatic companion-discovery providers remain pending.

The source browser reads immediate directories lazily (512 entries per listing), stages
one file at a time (64 MiB maximum), validates its declared resource layout and matches
source/copy SHA256 before invoking the existing editor. Staging is limited to 1 GiB and
1,024 sessions; saved edits remain until the user manages them. ARC traversal is limited
to 16 levels and 4,096 children. This single-file browser does not assemble companions. Recognizing a
resource tag does not establish editing fidelity for that format/version.

`CompanionWorkspace` is a separate Brawl-specific service used by File > Brawl Companions.
It inventories only explicitly added files through the same guarded staging path. A model
is selected from a base BRRES; exact-name textures and one skeletal motion are selected
explicitly. Even one matching candidate requires a choice. Multiple matches remain
ambiguous until selected. The implemented gate is MDL0 v11, TEX0 v1 RGBA8 without palettes,
and CHR0 v4 with exact matching bone names; no cross-rig correction is performed.

Version 1 `.fpcompanion` XML records absolute source paths, complete-file SHA256, indexed
native resource paths, type/name identities, native versions and texture bindings. Loading
rejects relative/network source paths before file access and verifies hashes before
native parsing and again after staging; missing/changed sources
or identities stop restoration. XML is bounded to 1 MiB with DTD/entities disabled.
Native File > Open and command-line file opening route this extension to the workspace.
Save/Resume last uses `DataRoot/CompanionResolutions/last.fpcompanion`; the format stores
resolution choices, not dirty native edits or a game-neutral character project.

Assembly retains the entire base BRRES resource tree, imports only selected textures and
one motion, and refuses existing-name replacement. Other native entries are listed as
retained base data or unimported companion data. A fresh native decode checks expected
entries and motion metadata before opening the new working BRRES. Native edits use the
existing editor/Save As path and are saved separately; original source files are never
rebuilt in place. This original fixture also verifies retention of a base SRT0 v4 entry's
name/timing; it does not prove arbitrary opaque-field fidelity.

Companion budgets: 16 source files, 128 MiB inventory, 64 MiB per file and conservative
selected-source assembly budget, 64 texture bindings, and 128 retained work sessions /
1 GiB under `DataRoot/CompanionWorkspaces`. Existing per-file staging quotas also apply.
Work sessions and source receipts remain until the user archives them. This path does
not discover installations, unpack containers or act as an intermediate for other games.

`FighterPortPaths.DataRoot` selects portable `UserData` beside the executable, or an
explicit absolute local `FIGHTERPORT_DATA_ROOT`. The application and BrawlLib settings
use a shared independent provider with qualified groups, .NET value serialization,
bounded XML loading, process locking and recoverable snapshots. No upstream settings
upgrade, profile deletion, registry association or Windows encryption change is needed.
An invalid/unwritable profile produces a startup error rather than silently losing
preferences. Source registrations and staging share this identity. Preserve the data
folder when moving or updating the product.

Native write paths check protected roots, path aliases, hard links and reparse points;
stream writes check actual handle identity. Registrations merge across processes using
the same data root and are reloaded before writes. Application/helper, OS temporary,
and settings/staging directories (including overlapping ancestors/descendants) cannot
be mounted as sources. Many exporter UI branches remain unexercised. This is
application-level protection, not an operating-system sandbox against hostile filesystem
races, unsafe legacy parsers or explicitly enabled arbitrary user scripts/executables.
Inherited updater, report submission, automatic plugin fetching and Discord presence
are disabled in executable code, in addition to defaults/UI. Error reports save locally.

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
