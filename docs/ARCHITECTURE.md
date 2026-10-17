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
image and arbitrary archive providers remain pending. Configured Brawl loose dumps
now have the bounded file-level discovery provider described below; it does not
automatically choose native resources or discover installations.

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

Version 2 `.fpcompanion` XML (with version 1 loading retained) records absolute source paths, complete-file SHA256, indexed
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

`BrawlDumpProfile` records up to 16 stable mount IDs, local roots, labels, enabled state
and integer priorities (-999..999; higher wins). Configuration is saved independently
in `DataRoot/SourceProfiles/brawl-loose.xml`; snapshots accompany companion resolutions.
Overlapping roots are rejected. Enabled existing roots use SourceFolderSession protections;
missing/unavailable/reparse roots are diagnosed rather than traversed.

File > Brawl Companions > Dump folders opens `BrawlDumpSourcesForm`. `BrawlLooseDumpProvider`
is a resumable breadth-first metadata cursor: each explicit page visits at most 128
filesystem entries by default. SourceFolderSession probes at most 16 header bytes per
file; there is no full-file hashing, staging or native decoding during discovery.
Candidates are tentative BRRES/TEX0/CHR0 files, not parsed resources. Header recognition
does not establish validity or operation coverage. A fresh scan replaces the catalogue,
not the working companion inventory or choices. No persistent content cache is trusted.

The override key is the entire normalized relative file/container path within the matching
dump layouts, using Windows case-insensitive path semantics. Native resource names never
form a global override key. Different character/container paths remain distinct. Highest
priority is recommended with its reason; equal highest priorities are ambiguous. Partial,
cancelled, unavailable or truncated catalogues never assert a guaranteed final winner.
Every stage action is an explicit user choice, including choosing a lower-priority or
tied source. Native bindings still require explicit choices after selected-file decoding.

Budgets: 512 entries per directory, 8,192 visited entries, 16 directory levels, 1,024
directories, 2,048 candidate files, 128 KiB conservatively charged header I/O, 4 MiB
estimated queued/candidate metadata, and 128 diagnostics of at most 800 characters.
Lower budgets are testable; raising the hard defaults is rejected. These bound catalogue
work, not hostile native-parser peak allocation. Cancellation disposes the directory
cursor and reports partial results without modifying saved work. Unsupported containers,
compression/images and skipped paths are reported explicitly, not hidden as a complete scan.

Only Stage selected file invokes CompanionWorkspace.AddDiscoveredSource. It verifies
root/relative-file identity and discovery metadata, stages through the existing validator,
pins the full source SHA256 and native indexed identity, and retains mount/path/priority/
selection-reason lineage. Stale metadata, missing inputs or changed pinned hashes fail.
Native inventory is additionally capped at 4,096 entries; the previous 16-file, 128 MiB
inventory and 64 MiB selected assembly limits remain. A priority change/rescan cannot
rebind a pinned resource. Its selection-time lineage is retained independently of the
current root configuration. Assembly still refuses implicit replacement of embedded
base resources. Resolution snapshots preserve source choices, not pending native edits.

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
