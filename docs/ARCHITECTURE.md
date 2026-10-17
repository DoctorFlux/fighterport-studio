# Architecture after the clean restart

2026-10-17 continuous-reach continuation after 77d64ce: the existing contact recipe stores
optional HoldStart/HoldEnd flags, World/TransportedRest pole and Nearest/RestHemisphere
reach policies, plus ReachBlend. XML defaults omit false holds, World, Nearest and zero
blend, retaining the old project representations and fade behavior. Holds are valid only
at the corresponding clip boundary; interior entry/exit margins still blend. Outside the
saved interval the contact is inactive. A full-clip standing command deliberately requests
both holds and, if previously zero, a 0.05 reach blend. The same previewer panel/session
edits these records through validated transactions and immutable assigned-recipe forks.
Selecting or changing endpoint flags alone does not rewrite a legacy pole or reach policy.

The format-neutral solver projects World poles with an explicit parallel-direction guard.
TransportedRest minimally transports the reviewed rest pole with the rest effector axis;
its antipodal chart, coincident goals and invalid rest poles reject rather than choosing
an arbitrary fallback. RestHemisphere fixes a leg's inner-shell branch from exact native
rest geometry. Nearest remains the old-data policy and reports disconnected ambiguity.
An optional 0..0.1 target-leg-length ReachBlend smooths approach to feasible pelvis limits
without leaving the allowed interval or stretching a limb. Its zero default retains older
hard limits. Intersections of several moving reach bounds can remain C0; this is not a
globally smooth whole-body solver or body-clearance system. Singular charts and impossible
opposing goals remain correction requests. Deterministic sampling uses no scrub history.

The Brawl adapter retains exact integer-frame values and fits shared Hermite tangents at
1/6, 1/3, 1/2, 2/3 and 5/6 within each interval. Packed rotation/position samples have a
32 MiB budget, alongside the existing one-million-scalar-key and 32 MiB native-file gates.
Fresh native decode validates the quarter grid (including withheld 1/4 and 3/4), independent
.137/.619 samples, contact boundaries, margin transitions and interval midpoints. The
eight-million decoded vertex-sample budget includes those additional times. Position,
orientation and skinned-geometry tolerances remain .02/.15 degrees/.03. Full-weight contact
residuals and held first/last-frame residuals are separate from serialization error; no
held endpoint coverage must not be presented as a perfect endpoint result. Native binds,
weights, unrelated records, original files, source/library revisions and other projects
remain protected. Final native/GUI evidence is recorded only after observed checks.

The previewer preserves explicit project/target/instance/document/editor identity and only
the chosen Brawl character render target. Its contact list restores saved timing, goals
and advanced policies. New rejects duplicate intervals; editing preserves custom pole,
orientation and evidence. A deliberate pole reset precedes a coordinate-policy change.
Invalid pole visualization retains valid goal/pelvis repair handles and reports the issue.
Endpoint holds do not fit loop poses, Idle/Fall references, ancillary channels or arbitrary
motions automatically. Those broader services, body clearance and non-Brawl native adapters
remain modular roadmap work selected only through plans/current.md.

2026-10-17 H3 standing checkpoint after 04ef3d9: optional `HumanoidStance` in the existing
recipe pins separate native source/target phases and a constant common-space pelvis offset.
It is clip calibration, not a bind edit, body resize, Idle/Fall anchor or profile approval.
The adapter evaluates immutable native inputs; the core applies the saved translation
before explicit contact intervals. Old recipes omit it and retain their data. Bounds,
identity checks, transactions, undo and immutable assigned-recipe forks remain enforced.
Coupled pelvis Y intersects both inner and outer two-bone reach shells; conflicting goals
remain diagnostics. A bounded tridiagonal least-squares fit chooses native Hermite tangents
from one-third/two-third poses, preserving exact integer keys and duration. Fresh quarter
samples validate position/orientation/skinned geometry independently of those fitting times.
No native codec, renderer, dependency or tolerance changes are involved in this checkpoint.

Native model preview uses explicit constructor models or selected character-source roots,
not the global last-opened root. Multi-model Brawl archive/picker entrypoints catalogue
indexed choices without attaching every MDL0. Selecting a character detaches other models
from both native target and render lists; complete native roots remain available unchanged.
Motion-only attachments do not register their models as character choices. Bound project
comparisons keep their explicit single-model ownership. This does not remove shadow/article
resources from a package or claim that the entire native file is never read into memory.
Inherited collision/stage scene previews retain their multiple-model workflow; the
character/animation selection rule does not discard collision-to-model bindings.


Ordinary animation browsing amendment (2026-10-17, after bee76e5): ModelForm now hosts
PreviewAnimationBrowser above the inherited editor, independently of the collapsible
authoring pane/native left panel. Its splitter/toggle retain viewport/timeline space.
Opened native roots, selected character, project, shared catalogue and assignments are
views over the existing documents/project/store, not another project implementation.
Selection/search never assign a slot. Preview uses the native TargetAnimation/timeline;
project/shared sources and assignment auditions use the same explicit session, with
read-only document ownership. Shared pages remain metadata-only (32 results/cursor page).
Importing a selected shared revision reuses UseSharedClip; all assignment compatibility,
pending edit leases and validated transactions stay in the existing backend.

Plain preview startup restores native playback even when old settings hid the bottom.
Read-only native animation navigation remains enabled, with mutation/context actions
gated; key/curve/material editing stays disabled. The always-visible browser survives
compact layout. Advanced project selectors remain service-backed setup controls, not a
second catalogue. Source preview preserves the explicitly selected project target.

PreviewSourcePicker performs bounded audit/directory inspection before staging/decoding
one deliberate file/range. Canonical whole files retain their hierarchy; opaque motion
PACs require explicit BRRES selection. Existing narrow source-root and staging guards,
64 MiB/file, 16 files/128 MiB per viewer and 4096-entry/depth-16 browser limits remain.
It does not scan a dump, infer a character from names or silently bind companion files.
PreviewNativeExportForm exports the selected skeletal motion's native document separately,
uses existing Hermite encoding for edits, checks decoded keys/tangents/flags/timing, then
moves a validated unique temporary file to a new guarded destination. It does not combine
separate model/motion archives or claim opaque/package/other-channel fidelity.

Automatic model-only project registration, common dependency-aware character catalogue,
reviewed family reuse and fixed-duration semantic miscellaneous filling remain queued.
No Godot code/package is imported. The format-neutral solver/native adapters are retained;
H3 Mario-to-Snake contact rejection follows the navigation repair, without mirror priority.

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
is selected from a base BRRES (standalone or inside raw nested ARC/PAC); exact-name textures and one skeletal motion are selected
explicitly. Even one matching candidate requires a choice. Multiple matches remain
ambiguous until selected. The implemented gate is MDL0 v11, TEX0 v1 RGBA8 without palettes,
and CHR0 v4 with exact matching bone names; no cross-rig correction is performed.

Version 3 `.fpcompanion` XML (with versions 1/2 loading retained) records absolute source paths, complete-file SHA256, indexed
native resource paths, type/name identities, native versions and texture bindings. Loading
rejects relative/network source paths before file access and verifies hashes before
native parsing and again after staging; missing/changed sources
or identities stop restoration. XML is bounded to 1 MiB with DTD/entities disabled.
Native File > Open and command-line file opening route this extension to the workspace.
Save/Resume last uses `DataRoot/CompanionResolutions/last.fpcompanion`; the format stores
resolution choices, not dirty native edits or a game-neutral character project.

Assembly retains the entire native root and selected base BRRES resource tree, imports only selected textures and
one motion into that BRRES, and refuses existing-name replacement. Other native entries are listed as
retained base data or unimported companion data. A fresh native decode checks expected
entries and motion metadata before opening the new working BRRES. Native edits use the
existing editor/Save As path and are saved separately; original source files are never
rebuilt in place. This original fixture also verifies retention of a base SRT0 v4 entry's
name/timing; it does not prove arbitrary opaque-field fidelity.

Companion budgets: 16 source files, 128 MiB inventory, 64 MiB per file and conservative
selected-source assembly budget, 64 texture bindings, and 128 retained work sessions /
1 GiB under `DataRoot/CompanionWorkspaces`. Existing per-file staging quotas also apply.
Work sessions and source receipts remain until the user archives them. This path does
not discover installations, decompress inputs or act as an intermediate for other games.

`BrawlContainerAudit` provides a bounded read-only raw header inventory and companion
preservation check (64 MiB/file, 4,096 records, depth 16, 128 diagnostics). It records
native versions, indexed nested paths, ARC type/index/group/redirect metadata and names.
It reports unknown/compressed entries without invoking native decoders. Opt-in CLI
inventory is independent of companion format admission; finding MDL0 v9 does not enable it.
Before companion staging/native decode, the audit rejects opaque/compressed/empty/redirect
entries, noncanonical ARC name bytes, reserved fields, alignment or trailing bytes that
the inherited ARC writer cannot retain. The existing raw staging validator still runs; the audit also validates the exact
hash-pinned staged bytes before native decoding.

Companion inventory traverses ARC -> BRRES -> native groups, preserving file/mount and
full indexed container identity. Version 3 choices additionally pin enclosing BRRES paths
and native lineage. Same-name resources in different BRRES stay separate explicit choices.
Sources in the same PAC but different BRRES are imported when explicitly selected;
same-file membership alone never means a dependency is already in the selected base.
Assembly of an ARC source exports the enclosing native PAC, not a flattened BRRES.
Fresh output decode verifies the selected native subset. The raw audit compares all ARC
hierarchies/entry IDs and hashes unrelated payload ranges exactly, excluding only the
modified BRRES contents. The selected BRRES's container identity/version must still match.
This does not prove arbitrary opaque fields inside a modified BRRES; those remain a
separate fidelity obligation. Native editor Save/reopen is additionally checked by tests.

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

`Authoring/CharacterProject` now implements the first authoring domain in-process.
File > Character Library and native File > Open / command-line .fpcharacter opening
use CharacterLibraryForm alongside the inherited editors. This is a versioned actual
edit project, distinct from .fpcompanion. Registering a resolved companion selection
pins a stable SourceCharacterProfile ID/revision, exact source bytes/hashes, the full
resolution XML (including mount/logical/container identities), original native MDL0/CHR0
payload hashes, an assembled enclosing native archive and serialized native model hash.
Skeleton snapshots retain indexed parents, names, rest S/R/T, bind/inverse matrices,
bone and billboard flags. The deterministic fingerprint and fixed interpretation
record native XYZ/Euler degrees/native units with unknown physical scale. No semantic
roles or matching-name compatibility are approved.

AnimationAsset binds the selected native CHR0 to that profile/fingerprint, independent
of the viewport, display names or source order. Registration creates no MoveAsset.
Explicitly authored manual drafts persist notes/startup/active/recovery/damage and have
no parsed native attack commands. MoveInstance pins animation revision, draft ID,
target profile/fingerprint, slot, revision and its own native blob. Direct assignments require the same source profile. A separate reviewed FK path now
handles an explicitly supported different-rig subset, described below; direct mismatches
still reject.
Slot names are project assignments, not compiled native gameplay slots. Multiple independent
instances keep source/library revision 1 unchanged.

Open instance writes a separate copy and binds an edit lease to that project/instance,
baseline revision and exact native open path. The actual model wrapper/preview and CHR0
editor handle editing. After inherited queued viewer settings, the product selects the
bound motion and frames the model bounds. Capture validates a fresh native decode,
unchanged model/rig and every other selected-BRRES entry, plus enclosing ARC metadata
and unrelated payloads. Only that selected CHR0 may change. Stale/unbound capture and
unsupported model/resource changes reject; dirty native edits remain available separately.
Each successful capture renews its bound baseline so further native edits can be captured
as separate transactions without reopening or mutating a library source.
Undo/redo restores project snapshots and reopens the bound instance in the native editor.
Capture is required before project save or abandoning a bound edit. Other native editing
workflows remain available outside this first project capture subset.

Version 1 .fpcharacter is bounded XML with deduplicated SHA256-addressed base64 native
blobs. It retains full native originals, archive edits/curves and immutable source/library
data rather than reconstructing curves from a lossy schema. Detached public record snapshots
cannot mutate committed state. Commands validate a candidate metadata snapshot before commit;
undo/redo keeps at most 32 commands / 16 MiB metadata in memory. Saved state persists;
history deliberately clears on load. Save writes a guarded temporary file then atomically
replaces the project, retaining .previous. Fresh loads check blob hashes, original payloads,
resolution bindings, rig interpretation/fingerprints and native instance revisions. Missing
or changed external inputs leave the saved state recoverable and previewable, with visible
diagnostics; assignment/capture/export block until original identities are valid again.

Original embedded-v1 budgets (still loadable): 8 profiles with one selected CHR0 each, 64 drafts/instances, 512 blobs / 32 MiB
combined native/session-revision data, 1 MiB metadata and 48 MiB serialized document;
DTD/entities are prohibited. Native files retain existing admission/staging/audit gates.
Working copies are capped at 256 files / 128 MiB per project session, with 256 sessions /
1 GiB at session creation. Save/reload discards unused blob revisions. Native export
requires a new file and the original enclosing .brres/.pac extension, then checks output hash.
The shared service is local and adds no dependencies; only its Brawl adapter exists today.

Broader game-neutral editable curves/constraints, outfits/material/mesh edits, correction
layers and source-revision management remain queued. Current shared storage and explicit
FK characterization extend this original project implementation as follows.
Animation-only input never invents attack behavior.
The shared rigging core must separate fingerprint/template/heuristic recognition and
review from FK transfer, editor-only IK controls, interval contacts and measured target
baking. Aerial attacks receive no default ground locks. Native gameplay compilers and
real target templates remain separate from animation transfer and mesh rebinding.
Managed tool provisioning is a planned app-local service with reviewed pinned manifests,
trusted hashes/downloads, dependencies/compatibility probes, project locks, offline cache,
cancellation, rollback and manual paths. No arbitrary remote scripts/global installs,
silent EULAs, credentials or game data are authorized. No new dependency was added here.

Project state must preserve exact IDs/binds/winding/timing, opaque records, semantic maps,
correction layers, source and target branches, companion ambiguity and explicit losses.
Same-rig conversion, cross-rig retargeting, mesh rebinding and gameplay translation are distinct.
Local non-AI operation and no required DCC remain invariants. Legacy FPPROJ/FPWORK compatibility
is not a restart prerequisite; old implementation is only in recovery/history.


Shared animation storage and reviewed FK — 2026-10-17:

AnimationLibraryStore owns a stable library ID, immutable per-character/per-animation XML
records and SHA256-addressed native/original/edited payload files. The app-wide default is
UserData/AnimationLibrary (or explicit FIGHTERPORT_LIBRARY_ROOT). New GUI projects use it;
old embedded v1 projects require the deliberate Share / migrate action. Migration copies
native state and session-history payloads, preserves stable IDs/legacy drafts/actual edits,
and clears the save path so the original v1 file is not silently overwritten. Source and
clip revisions are immutable; this increment creates revision 1 and rejects conflicting
rewrites. Revision-management UI, portable bundles, tombstones and cleanup are queued.

Version 2 .fpcharacter pins library ID/root and exact record hashes, caching only its
selected graph (8 profiles / 128 clips / 64 instances, drafts and reviewed maps / 1 MiB
metadata). Native payloads stay on disk, including independent target edit revisions;
projects do not embed or enumerate the entire library. One registered source accepts
multiple loose CHR0 v4 imports, explicitly bound to its unchanged native model. Each clip
retains separate native archive/payload/export hashes, timing and original evidence.
Import changes only the selected motion, preserving native container/model/unrelated
resources. No non-Brawl intermediate is defined by this adapter.

Metadata browsing uses a resumable directory cursor: at most 256 visited metadata records
and 64 results per request, no native decode/blob reads; generation changes require refresh.
Animation metadata is at most 128 KiB; character records at most 1 MiB. Cancellation is
supported by indexing/import/bake APIs; the small synchronous metadata GUI pages do not
claim background whole-library search. The store reserves at most 4 GiB / 100,000 records,
with 32 MiB per native blob. A file lock serializes writers and metadata pages with a bounded
5-second busy failure. Reservation precedes writing; crashes can conservatively overcount
space. Atomic immutable files, read-only attributes and verified hashes protect revisions;
missing/wrong pinned libraries stop rather than create replacements. No automatic deletion.
Working-copy/session and undo budgets above still apply. Project undo restores references,
not destructive rewrites/deletion of library records. History clears on load.

RigTransfer is a format-neutral rigid pose/mapping service. ReviewedRigMap pins both exact
fingerprints and interpretations, explicit semantic role/index pairs, static ignored source
helpers/rest-only target helpers, native-unit ratio and root policy. A digest invalidates
accidental policy changes after review (not an authentication signature). The real review
form starts without name-based approvals and displays parents/rest transforms; every bone
needs an explicit policy. Maps currently live in the project and can be reused across its
source clips. Shared reviewed-map/template records across projects are still queued.

world-rest-delta-fk-v1 computes source world rest-relative rotation, applies it to the target
rest orientation and resolves target local rotations through animated parents. Target local
offsets/proportions remain intact; root is explicitly in-place or uses reviewed calibrated
native translation. No physical unit inference. Single parent-first roots, <=128 bones,
unit scales, no compensation/billboards, static ignored helpers and no animated non-root
translations are required. Euler singularities and ambiguous combined source Euler travel
>=179 degrees per baked frame reject. No IK, mesh rebinding, contacts or automatic semantic
recognition is claimed. More general winding/curve/constraint support remains queued.

The Brawl adapter samples inherited native evaluation, bakes CHR0 v4 into the existing target
archive and checks fresh native decoding before committing an instance. Source clip, target
native motion identity, map/review revision and measurement report are persisted separately.
Budgets: 2..2048 frames, <=1 million scalar keys, <=8 million decoded vertex samples.
Globally linear channels use exact sparse endpoints; inherited dense codecs can alter
fractional tangents, so other cases may reject. Quarter-frame decoded world bone position,
orientation and native weighted mesh errors must be <=.002 native units / .05 degrees /
.005 native units. This is a sampled bound, not continuous arbitrary-curve certification.
Native edits retain the original bake report explicitly as historical measurement.

The current UI hides legacy gameplay fields behind Legacy drafts. Assigning animations
requires no gameplay record. Legacy free-text slots have no verified runtime binding.
The v3 workspace below adds typed slots/anchors. The 2026-10-17 humanoid amendment removes
mirror as a prerequisite for final-target fitting, interval IK and template fill. Keep native originals, independent targets
and modular adapters; non-Brawl sources must not pass through MDL0/CHR0.


Model Previewer workspace and project v3 — 2026-10-17:

ModelForm still owns the inherited ModelEditControl and OpenGL viewport. Its additional
split pane hosts reusable Setup, Library, Animation Set, References, Rig/FK and Project/
Output surfaces. The native playback panel is reused; compact authoring hides wide native
edit detail panels to keep frame/playback controls inside small windows. Native details
remain accessible in a wide editor. Metadata-only edits retain native document/camera/
playhead; real native revision changes rebind the exact document without camera reset.
Layout visibility/width is a separately bounded app preference, never project state.

PreviewAuthoringSession wraps the existing CharacterProject. A weak registry returns the
same live service for the same open file, and the library and previewer subscribe to its
changes. Project commands publish detached snapshots; atomic batches expose their previous
committed snapshot until completion and record one undo entry. Native payloads remain in
the existing immutable shared store. Version 1/2 loading is retained; explicit slot/anchor
authoring upgrades to version 3 and preserves old custom assignments/legacy drafts. Shared
migration retains the original embedded file; new/migrated saves require a new path.

Each PreviewDocument carries session, target ID/fingerprint, instance/revision, native
resource identity, request ID, root/model/editor references, working path and edit lease.
Authoring files are independently owned native roots, not Program.RootNode. No authoring
command chooses ModelEditControl.Instances.Last. Exactly one authoring document can edit;
other project/source/reference views use disabled editing panels/hotkeys and guarded
mouse handlers while native camera/playback remain available. Explicit activation resolves
pending Apply/Discard/Cancel and changes edit ownership. Stale comparison writes reject.
The legacy resource document remains separately owned and must resolve dirty edits before
handoff. This is not certification of simultaneous editing across inherited legacy tools.

Apply reuses the native lease and current adapter validation. Native Export rebuilds and
clears dirty flags, so failed capture restores pending status; successful capture cannot
trigger document disposal during its own transaction. Unsupported model/material/channel
changes remain pending and can be saved separately, not misrepresented as project edits.
Save/switch/normal close preserves or explicitly resolves pending state. Closing a busy
view is blocked until cancellation/completion. Forced process termination is not recovery.
Project buttons/shortcuts act on structural/applied snapshots; native keys retain native
Undo, and text fields keep their own editing context. History remains session-only.

Slot records pin a pack ID/version/digest and target fingerprint. The Brawl semantic pack
v2 has 30 groups, with stable semantic definition IDs; target-scoped slot IDs, pack-scoped
variant IDs and independent part GUIDs survive rename/reorder. Phases and variant keys
belong to the versioned pack. Custom legacy entries live outside the immutable pack.
Authoring quotas: 8 sets, <=64 groups/pack, 512 parts/project and 1..64 parts/batch; these
are resource limits, not game behavior rules. Parts preserve separate clip/instance/map
revisions and are never flattened. Verified native bindings pin selected target model/
container/resource identities, native indexed path/hash and pack version. Duplicate exact
destinations reject; no current profile enables aliases. Unbound assigned parts persist.

Anchors pin target rig, native instance/clip revisions and hash, native timing/loop convention
and phase. Choosing a motion and setting its phase are separate commands; Idle/Fall can
be chosen as one atomic pair. Named pack/custom references use the same service. Current
native playhead phases are integer frames, stored zero-based (viewer displays one-based);
loop convention is FrameCount+1. Missing/stale dependencies diagnose and block reference
preview/phase selection until deliberate re-selection. Only native instances can be anchor
inputs, so recursive generated-anchor references are not representable. No fitting or
anchor metadata encoding in native files is claimed.

Assignments/retarget bake and diagnostics use a serialized cancellable session worker.
Other project controls freeze while work runs; rendering/native playback stay available
read-only. Worker nodes are separate CPU/native documents; control/GL updates return to
the UI thread. A result pins its original project/part/target and prior document request;
it cannot replace another selected view after handoff. Cancellation rolls back project
batches; unreferenced immutable blobs may remain within store quotas. This is not a general
thread-safe public CharacterProject API. Library indexing remains metadata-only and paged.

Additional fitting/IK/contact/template/channel/non-Brawl/Modkit/tool/mirror features must
extend this workspace and these explicit bindings. Native animation, mesh rebinding and
gameplay remain separate, with gameplay deferred in this phase. No dependency/runtime/
renderer/CPU migration or new third-party package was introduced.

Proportion/contact checkpoint H1/H2 — 2026-10-17, continued from 7458be7:

The existing Rig/FK tab now embeds HumanoidWorkspacePanel in the same session. Detection,
explicit review, chain/body landmark edits, contact intervals, FK/corrected comparisons,
Fit, independent Apply and native trial export all use explicit clip/target/destination
bindings. Virtual contact/pole/pelvis handles draw in ModelPanel and commit one recipe
transaction on release; they are never native bones. Native playback remains the clock.
Drag release rejects a changed document, selected clip/target/part or project snapshot;
handles are drawn only for the exact bound native destination.
Comparisons own separate decoded roots and remain read-only. Unreviewed proposals may
produce labeled diagnostic trials, but cannot become approved target assignments.

HumanoidRetargeting.cs contains common proper rigid transforms and the deterministic
stateless solver, without BrawlLib, native-format or UI dependencies. Adapter-side native
evaluation supplies local transforms after interpreting flags. World rest-rotation deltas
transfer through semantic chains; target locals reconstruct through the animated target
hierarchy. Spine/finger chain lengths can differ. This first route requires reviewed
three-joint limbs, unit rigid scales and InPlace actor motion. Pelvis displacement scales
by measured leg lengths, not assumed physical units; target bone lengths/mesh are retained.
Optional target channels retain normalized target-reference motion, with explicit loss
reporting for untransferred source channels. Old rigid FK and direct cross-rig guards remain.

Humanoid profiles bind exact rig/interpretation/calibration and reviewed digests. Body
profiles additionally bind model/bind/weight evidence and store sole/wrist landmarks,
poles, ground, pelvis limit, widths and torso/head measurements. Reviewed revisions publish
as immutable bounded shared records and can be reused across independent projects. Draft
chain/landmark edits are transactional; reviewed-revision editing/fork UI remains queued.
Bind/rest, calibration and Idle/Fall phases are distinct. Version 3 adds optional records;
old v1/v2/v3 files remain loadable. Limits: 8 rig profiles, 8 bodies, 64 recipes, 16 pins.

Explicit World/Head/Spine contact intervals drive analytic two-bone limbs, poles, end
orientation and bounded shared pelvis Y. Pelvis participation fades with interval margins;
there are no inferred floor locks on aerials. Unreachable reach/interval conflicts report
residuals without stretching. This is not a full anatomical/temporal/clearance/twist/prop
solver. Pure source-local basis re-expression, analytic planted soles, body-relative hand
and two-wrist goals provide independent expectations. Native serialization checks remain
separate from contact quality and from human/game acceptance.

BrawlHumanoidBake evaluates native curves and writes independent FK/corrected archives,
keeping target skeleton/binds/weights and unrelated native resources. I12 is an explicit
opt-in for baked Hermite values/tangents; inherited default encoding remains. The bfloat
storage fix keeps reversed IEEE bits in uint rather than a disguised float, avoiding x86
signaling-NaN quieting corruption. Four-byte layout is unchanged. Native orientation
measurement validates scale/shear before orthonormalizing float rotation frames; raw
position/mesh measurements remain unchanged. Limits are .02 position / .15 degrees / .03
mesh units at quarter frames, not an arbitrary continuous error certificate. Integer
bakes reject >120 degrees/frame travel/tangents, animated scale/compensation and unsupported
dynamic non-root translations. Source constant authored offsets are evaluated natively,
never copied as target limb lengths. Bake budgets: 2..1024 frames, one million scalar
keys, eight million decoded vertex samples. Working copies reserve 256 files / 128 MiB;
trial disposal releases only the exact newly created temporary comparisons.

Selected companion admission is separate from dump discovery. BrawlContainerSelection
reads bounded raw ARC directories, without decompression/magic scanning/opaque parsers,
and stages one explicitly chosen BRRES through existing strict validation. Empty/redirect
and unknown siblings stay outside decoding. The complete original hash/bytes/provenance
and original indexed path remain pinned; this is not a round trip of an opaque package.
Model bases still require the full supported archive. Companion v4 adds selected-range
and explicit palette identities while v1/v2/v3 load. MDL0 v9/v11, CHR0 v4 and the documented
normal-material TEX0/PLT0 subset are admitted; no repository-root source mount or limit
removal is involved. Target model PAC hierarchy is retained through native output.
