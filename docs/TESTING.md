# Testing the restarted application

H3 standing continuation (2026-10-17): run `scripts/test.ps1 -Humanoid -Gui` and
`scripts/test.ps1 -AnimationBrowser -Gui`, plus all retained native/GUI suites. New original
checks use independent inner-reach geometry/opposing goals, actual calibration controls,
undo/redo/project reload and existing random-scrub/loop/full-turn/contact/native gates.
Browser tests cover both picker and ordinary ARC PreviewAll: zero attached models before
an explicit multiple-model choice, then exactly one native target/renderable, including
after switching. Complete source hierarchy is retained. Motion-only files are not model choices.

Opt-in local `--local-humanoid <private XML> <new private evidence>` still accepts the exact
configured pair. `calibrateStance="true" sourcePhase="10.25"` explicitly aligns that source
phase to target native phase 0; use separate direction configs, never infer reverse support.
It preserves duration, exports new PAC and loose CHR0, compares fresh standalone channels at
quarter times, reloads the exact saved recipe and requires deterministic regenerated native
hashes. Unreviewed passing diagnostics must still reject approved assignment. GUI mode
checks single-character scenes, source/FK/corrected playback, real calibration/export/save
controls and a newly reopened PAC through the main native browser and visible animation list.
Unexpected GUI exceptions fail the run; a watchdog bounds it. Commercial inputs, configs,
outputs and captures remain ignored. The old uncalibrated reverse failure is a historical
negative, not silently changed goals or relaxed tolerances. A first-frame calibration alone
also failed; the explicit diagnostic phase choice is recorded in the saved recipe.

The native baker fits tangents at thirds and validates freshly decoded quarters; identical
integer keys alone never establish motion fidelity. Existing original analytic sole/skin
expectations remain separate from native serialization comparisons. Normal executable
restart uses VerifyLibraryProduct and preserves exact project state; unreviewed diagnostic
recipes do not become approved instances on restart. Modkit/game checks remain unobserved.
VerifyLibraryProduct resolves PAC/BRRES from the selected target record, verifies that exact
working-file extension/hash and exports the same extension; it no longer assumes BRRES.


Ordinary animation regression (2026-10-17):

```powershell
./scripts/test.ps1 -AnimationBrowser -Gui
./tests/NativeSmoke/VerifyAnimationBrowserProduct.ps1 -Model <original-model-PAC> -Motion <original-motion-PAC>
```

The focused suite generates separate original model/motion PACs and two different rigs,
opens the main native browser/model wrapper, and selects the clip through real visible
list/search/Preview controls. It checks native bone transforms, >100 changed framebuffer
pixels, actual Play progression, read-only exploration, compact/old hidden layout recovery,
motion-first/previewer-first protected picker controls, cancellation/missing/changed inputs,
small windows with both panels, separate numeric edit/native export/decode, source overwrite
rejection and unchanged originals/shared revision/second project. Project/shared/assignment
scopes share the authoritative session; source A preserves an explicitly different target B.
No helper directly sets SelectedCHR0 for these new workflow acceptance checks.

The product helper launches the normal exe twice, initially with a clean isolated profile,
then persisted settings/guards. It drives File > Animation workspace via native menu/control
messages, opens separate files through the actual picker, sees OriginalSwing, invokes
Preview/Play/Stop, exports with decoded validation and requires normal exits/unchanged
input hashes. Its fixed native menu hit point is a 96-DPI automation constraint, not a
physical high-DPI claim. Product PrintWindow captures are separate from focused OpenGL
geometry captures. OS file pickers are bypassed; this is control automation, not manual use.
Optional `-SavedSettings <existing settings.xml>` adds a third launch, copying only that
configuration into the isolated profile before launch two. The donor settings are hashed
unchanged. Do not copy or publish user libraries/game data. The original empty SRT0 marker
tests channel routing/export rejection only; animated ancillary fidelity stays queued.

Opt-in local ordinary checks: `NativeSmoke.exe --local-browser <private localHumanoid XML>
<new private evidence directory>` reuses the configured explicit model path/motion BRRES/
clip selectors. It opens both real model PACs through the main native browser, attaches
separate motions with picker controls, selects the exact CHR0 among same-name channels,
checks matrices/rendered geometry/Play, exports separate motion BRRES and freshly decodes
it, then checks original hashes. It does not register/approve a commercial rig or create
a playable character. Keep all inputs, private configs, paths, derivatives and captures
ignored. Normal product project restart remains VerifyLibraryProduct -PreviewWorkspace;
run it for both independently saved shared projects. All retained suites remain required.
The opt-in browser has a 120-second watchdog and records/fails unexpected GUI thread
exceptions rather than waiting in or silently dismissing the interactive product report.
An observed GDI button-paint exception is retained in status; a resumed run is not a clean
pass, even if all later motion checks succeed. Repeated clean runs do not establish its cause.

Use Windows, installed Visual Studio 2022 MSBuild and the .NET Framework 4.7.2
targeting pack. The verified toolchain also has .NET SDK 9.0.300. Run from the
repository root:

```powershell
./scripts/doctor.ps1
./scripts/build.ps1
./scripts/test.ps1
./scripts/test.ps1 -Gui
./scripts/test.ps1 -Companions -Gui
./scripts/test.ps1 -DumpSources -Gui
./scripts/test.ps1 -Containers -Gui
./scripts/test.ps1 -Humanoid -Gui
./scripts/run.ps1
```

The app remains net472/x86/WinForms/OpenTK. `Directory.Build.props` and the build
script apply `build/Independent.targets`: no inherited updater invocation, recursive
output deletion, Cargo invocation or global tool installation. ColorSmash source is
present but its native helper has not been built. NuGet lock files are committed;
`build.ps1 -UpdateLocks` is an explicit dependency-maintenance operation.

The executable is `application/BrawlCrate/bin/x86/Release/net472/FighterPort.Studio.exe`.
`run.ps1` opens that application directly. `test.ps1` builds and runs the new
`tests/NativeSmoke` executable, using a fresh ignored `artifacts/restart/native-*`
directory. `-Gui` additionally opens real editor windows on the interactive desktop
and writes `gui-*` captures. Allow those windows to finish; avoid interacting with
them during the check. Watchdogs are 90 seconds normally, 180 for workspace and 300 for
humanoid modes. Logs and native
outputs stay local. No previous FighterPort tests or worker are used.

The original procedural fixture is a two-bone rigid paddle with four vertices, exact
identity binds, one material/sampler, a 32x32 RGBA8 quadrant texture and a looping
31-frame CHR0 v4 animation in BRRES/MDL0 v11. Native checks assert independent expected
positions, binds, texture pixels and the edited key after a new-file round trip.
The writer and reader share BrawlLib; this is not independent native-tool validation.

The GUI harness uses actual MainForm, SourceBrowserForm, wrappers, ModelEditControl,
CHR0 numeric control, Play button/timer and Program.Save/Open. It opens an editable
staged copy, previews/scrubs/plays, changes the 60-degree key to 35 degrees, refuses
a source overwrite while retaining dirty edits, saves a separate BRRES and reopens it.
It bypasses the operating-system folder/Save As pickers by supplying test paths.
This is automated handler/control verification, not manual clicking. Inspect both
window and OpenGL framebuffer captures before claiming visible geometry or textures.

Source tests cover signatures independent of filename, raw ARC/PAC with embedded
BRRES, malformed/truncated/compressed rejection, cancellation/outside-root rejection,
SHA256-verified staging, native output/stream protection, hard links, application/temp
directory rejection and persistence across processes. Distinct 8.3 aliases depend on
the volume; a skipped alias check is not a pass. Keep any focused junction/8.3 probes
separately identified from the repeatable harness. Commercial reference folders are
not loaded; their hashes can be compared with the verified recovery inventory.

Preferences, registrations and staging use executable-local `UserData`, or absolute
local `FIGHTERPORT_DATA_ROOT`. Harness runs use an isolated override. Settings tests
save/reload application and library scalars, collections, COLLADA options and complex
viewer settings in fresh instances and a separate process. Launch the actual product
separately to check its entry point and default portable profile. Preserve data folders
when updating; no automatic cleanup of edited copies or imported profile migration.

Original baseline build receipts: `D:/Users/ChatGPT/FighterPort-Upstream-d56cbac/`
contains `baseline-build.log` (Cargo pre-build failure), `baseline-repaired-build.log`
(managed application compiled using hook override) and `LocalBaseline.targets`.
See UPSTREAM.md for the exact boundary and DEVELOPMENT_STATUS.md for final evidence.

Review before each local commit with `scripts/review.ps1`, staged paths AND content
diff. `python scripts/verify-upstream.py` compares preserved baseline hashes and reports
intentional changes/missing files; new files are reviewed in Git. Source publication
requires exact origin/public-status verification and review of all outgoing files/commits.
The user's standing instruction on 2026-10-17 authorizes committing and pushing each
completed, passing source update; no repeat permission request is needed. Preserve history
with normal pushes to origin/main. CI dispatch, releases, tags, artifact uploads and binary
distribution remain unauthorized. The source/component audit remains required before
binary distribution.

Humanoid/contact H1/H2 — 2026-10-17:

`test.ps1 -Humanoid -Gui` generates original full humanoids (v9/v11, contrasting proportions,
local axes/parents/spines/fingers/helpers and blended weights), original CI8/palettes with
alpha/three mips, and raw ARC directories with opaque/empty siblings. It tests explicit
selected-BRRES intake, whole-original versus staged hashes, stale/missing/bounds/cancel/
reparse rejection, exact profile/body review, independent shared projects, native FK and
contact outputs, reverse transfer, body-relative/two-wrist contacts, aerial/no-lock, full
turn/finger loop, margins, basis re-expression and deterministic random scrubbing. Contact
goals include independent fixture-author floor/sole expectations and native decoded mesh
checks; solver self-comparison alone is insufficient. Native serialization limits are
.02 position/.15 degrees/.03 mesh units at quarter frames, separate from target contact
quality. Unreachable/unsupported/fast-winding cases reject rather than changing limits.

GUI mode exercises the actual previewer Fit/comparison/Apply/edit/save/undo surfaces and
virtual contact/pole/pelvis handles (handler automation, native GL/timeline). It verifies
rendered geometry, immutable source/other-project revisions and small-window/hide-reopen
behavior. Normal product restart is separate: run
`tests/NativeSmoke/VerifyLibraryProduct.ps1 -Project <original humanoid.fpcharacter> -PreviewWorkspace`
and repeat for the independently saved other project. The helper launches the real exe
twice, restores exact native hashes/project records, exports/saves from the previewer and
requires normal exit. No persistent undo-history, manual-click, physical high-DPI, external
native-tool, Modkit or game result follows from those tests.

Opt-in local checks (never CI/default fixtures): `NativeSmoke.exe --probe-brawl <selected
local file> <new private evidence directory>` lists bounded BRRES ranges/native metadata.
`--local-humanoid <private XML config> <new private evidence directory>` accepts exactly
two `<character>` elements under `<localHumanoid>` with explicit id/model/modelPath/motion/
motionContainer/clip/extraClip selections. `trial="true"`, `contacts="true"`, optional
`contactSource="A"`/`"B"` and `gui="true"` enable diagnostic trials and native GUI checks.
Store actual absolute inputs/config/receipts only in ignored local storage. No commercial
paths, hashes, captures or derivatives enter commits. Characterization remains unreviewed;
only explicitly authored original fixtures receive author review in the regression suite.
The current reverse local contact recipe is an intentionally recorded failing boundary,
not a default passing regression. Preserve that receipt and follow plans/current.md.

The bfloat fix has independent finite IEEE byte vectors whose reversed bits look like
signaling NaNs, including signed zero, subnormal/normal boundaries and large finite values.
Generated CHR0 bakes explicitly opt into existing I12 Hermite encoding. All eight retained
suite families must still pass in native and GUI modes after native codec changes; run
GUI tests sequentially and finish active harnesses before rebuilding their locked exe.

No broad fidelity, arbitrary skeleton retargeting, separate native tool, Modkit build,
console or game result follows from these checks. Keep each format/version/operation,
manual GUI observation and native/in-game validation separately declared.

Companion verification uses four original input files: a model/material BRRES with no
texture or CHR0, two texture BRRES files offering the same texture name with different
pixels, and a loose CHR0 in another folder. The explicit second texture must win; no
first-match fallback is allowed. A base SRT0 v4 entry must retain its name/timing/loop
metadata. Missing/ambiguous choices, source-hash mismatch, absent source, wrong resource
identity, guarded manifest output and prohibited XML entities are checked separately.

`-Companions` runs companion native tests instead of the single-file suite; add `-Gui`
to exercise real model/texture/motion selectors, persistent resolution, Open assembled
copy, native texture/material controls, OpenGL poses/playback, numeric key edit and
separate Save/reopen. A separate hidden process restores the resolution and checks all
four source hashes again. No folder or Save As picker is driven by these tests.

The standalone executable also accepts a `.fpcompanion` argument through its normal
file-open path. The recorded product check launches it twice with an isolated data
root, restores the saved choices, invokes Open assembled copy, compares the exact
resolution identities persisted by the product and observes normal exits. Application
window captures and native framebuffer captures are visually inspected separately.
The resolution stores pinned source choices; save native edits to a separate BRRES.
See DEVELOPMENT_STATUS.md for exact latest paths, hashes and tested scope.

Multi-root original-fixture verification (`-DumpSources`, add `-Gui` for real controls):

- Three matching dump roots contain a model-only BRRES, separate same-name texture
  alternatives and a loose CHR0; an unrelated character has the same texture name.
  Discovery pages visit lightweight headers only. No staged directory may exist after
  discovery. It checks ordered winners, equal-priority ambiguity and explicit selection
  of a lower-priority file whose decoded pixels differ from the recommended source.
- A later priority change recommends a different file but must leave the explicit
  selected resource/hash unchanged. Root IDs/priorities and selection-time lineage
  survive two independent verification processes. A cancelled rescan leaves saved
  resolution bytes and pinned choices unchanged. Separate negative copies cover stale
  metadata, changed full hashes and missing inputs; originals are not mutated.
- Lower entry, depth, directory, candidate, header-byte and metadata budgets must
  report incomplete/truncated discovery. Missing roots, overlap, relative/network
  roots, unsupported compression/containers and a real Windows junction are checked.
- GUI checks operate the actual dump list/page/stage buttons, existing companion
  selectors, native OpenGL preview/Play/numeric editor and separate Save/reopen.
  All ten original inputs retain SHA256. Folder and Save As pickers are bypassed.

To verify the standalone entrypoint rather than the harness host, use a generated
original-fixture `dump-ui.fpcompanion` from the latest dump GUI evidence directory:

```powershell
./tests/NativeSmoke/VerifyDumpProduct.ps1 -Resolution <absolute-original-fixture-resolution-path>
```

This launches the actual FighterPort.Studio.exe twice with an isolated profile,
opens Dump folders to capture restored roots/priorities, returns to the companion
workspace, opens the assembled native copy, compares exact root/source/hash/resource
records persisted by the product, and observes normal exits. The second launch uses
the product's last resolution. It stores app-only captures and JSON under ignored
artifacts/restart/dump-product-*. Inspect the captures; this is Win32 control automation,
not manual clicking or independent native-tool/game validation. This script is for
original fixture resolutions; commercial derivatives/captures must remain local.

Run the existing default and `-Companions` suites separately; the new selection does
not replace them. Companion checks verify version 1 and 2 resolution compatibility without version 3 container attributes.


Container acceptance (`-Containers`, add `-Gui`): original raw ARC/PAC with two nested
branches, a selected model BRRES, texture/motion BRRES companions and an unrelated
same-name texture. Explicit indexed choices must create enclosing PAC output. Checks
compare ARC entry type/index/group/redirect/name/hierarchy, exact unrelated payload
hashes, original geometry, two blended influences, translated child bind/inverse bind,
material binding, every original RGBA8 texel, motion timing/loop and edited key.

The original mesh blends Root/Paddle by 75/25 at its bottom and 25/75 at its top, with
child bind pivot y=6. Native decoded weighted positions at the selected key are compared
with independent weighted rotation about that pivot: 0.002 native position units, 0.0001
for bind/weight checks and 0.001 degrees for the key. This is declared sampled/key-frame
LBS verification, not arbitrary skeleton support or a continuous-error bound. The real
GUI uses the inherited model preview, Play, CHR0 numeric edit, Program.Save and reopen.
The saved output is edited-gui.pac. Nine original source hashes must remain unchanged.

Negative cases cover cancelled inventory, entry bounds/depth, stale source/container
identity, changed unrelated payloads, reserved header bytes the native writer would
discard, compressed PCS and implicit embedded-resource replacement. Malformed inputs
are separate disposable copies. The first negative-fixture generation attempt correctly
hit the source write guard; generation was moved outside the mounted negative folder.
A source-label encoding issue found in captures was corrected before accepting the final GUI run.

Standalone product check (two launches, exact version 3 choices, normal native PAC open):

```powershell
./tests/NativeSmoke/VerifyDumpProduct.ps1 -NativeExtension pac -Resolution <absolute-original-container-ui.fpcompanion>
```

For explicit opt-in local native-header inventory only, after building the harness:

```powershell
./tests/NativeSmoke/bin/x86/Release/net472/NativeSmoke.exe --inspect-brawl <absolute-configured-PAC-path> <new-local-evidence-directory>
```

This registers read-only protection, reads bounded raw header metadata without a native
model/moveset decoder, records versions/unsupported entries locally and checks the one
input hash. It does not scan a drive, decode commercial geometry or demonstrate gameplay.
Commercial metadata files/captures/derivatives remain ignored. Header inventory and
companion admission are separate: unsupported MDL0 versions/opaque contents remain unsupported.

Library/project acceptance:

```powershell
./scripts/test.ps1 -Library -Gui
./tests/NativeSmoke/VerifyLibraryProduct.ps1 -Project <absolute-original-library-ui.fpcharacter>
```

The first command generates two original native rigs with duplicate display/model/bone
names: A has two bones/identity binds/rigid weights; B has three bones, a translated
Paddle bind, rotated descendant, different proportions and blended weights. Both are
MDL0 v11 / CHR0 v4 / RGBA8 native sources. Their fingerprints and associations must differ.
It registers explicit companion resolutions, creates a manual draft (7/3/18 frames,
9.5 damage, no native attack evidence), assigns independent instances and edits A's
native key from 60 to 35 degrees. Slot/key undo/redo, atomic save/load, reordered profiles,
duplicate names, detached records, missing/changed sources, stale leases, unreviewed rig
edits, blob tampering and prohibited entities are checked. Rejection must retain state.

GUI mode uses the actual library tree, native preview button on B, slot assignment/
undo/redo, A's native model viewer/Play/numeric editor, capture and project undo/redo.
It changes the inherited key to 25 degrees, saves .fpcharacter, invokes the native-export
button and reopens the separate BRRES in MainForm. The product frames its model after
queued viewer initialization; this suite does not supply a camera correction. Inspect
the native OpenGL framebuffer captures as well as the library/window captures. A blank
framebuffer fails. Win32 PrintWindow alone may omit OpenGL and is not geometry evidence.
Both a fresh service load and an independent harness process must restore actual native
edits, stable source/move/target associations and the second target's unchanged 60-degree
motion. The immutable library clip remains at 60. Decoded expected weighted positions
are compared to analytic rotations about each rig's bind pivot (0.002 native units),
with 0.001-degree key checks. Four original source/DAE hashes must remain unchanged.
These are sampled native poses, not cross-rig transfer or full curve-fidelity evidence.

The standalone script copies the original fixture project into ignored evidence, launches
the actual executable twice with fresh isolated profiles, restores visible source/draft
data, opens the persisted instance in the inherited model editor, exports through the
real output path/button, compares native output hashes with the previously decoded
25-degree revision, saves and compares all project records and observes normal exits.
No test-specific product startup flag or external editor is involved. Inputs and captures
stay local. Review all original hashes again after standalone launches.

Manual workflow: File > Brawl Companions, resolve and save .fpcompanion; File > Character
Library, register that resolution with a display name, select its clip, optionally create
an explicitly manual draft, select the same source profile as target and assign a slot.
Open the instance, use native animation controls, return to Capture native edits, then
Save project. Undo/redo restores the native instance. Enter a new absolute output path
beside Export native copy, or leave it empty to browse. Reopen .fpcharacter through File
> Open or command-line file opening; export and reopen a separate native file.

Version 1 supports one selected motion per profile (8 profiles), 64 drafts/instances,
32 MiB deduplicated native/session blobs, 1 MiB metadata and 48 MiB document. Working
copies: 256 files/128 MiB per session; 256 sessions/1 GiB at creation. Save/reload discards
unused revisions. Undo is 32 commands/16 MiB metadata, session-only; saved actual state
persists, history clears on load. Missing/changed originals leave recovery preview
available but block assignments/capture/export. Capture accepts selected CHR0 edits only.
Different source/rig assignments reject. No inferred attacks, compiled gameplay slots,
retargeting, material/mesh/outfit authoring or game result follows from this increment.


Shared library and reviewed FK acceptance (current extension):

```powershell
./scripts/doctor.ps1
./scripts/build.ps1
./scripts/test.ps1 -Retarget -Gui
./scripts/test.ps1 -SharedLibrary -Gui
./tests/NativeSmoke/VerifyLibraryProduct.ps1 -Project <absolute-project-one.fpcharacter> -InstanceIndex 1
./tests/NativeSmoke/VerifyLibraryProduct.ps1 -Project <absolute-project-two.fpcharacter>
```

Run the retained Library, Containers, DumpSources, Companions and default -Gui suites
separately as above. No old WPF counts are evidence. Each run creates new ignored evidence.

RetargetChecks creates original A (2 bones) and B (4 bones) with changed parents, a 90-degree
rest-axis bridge, different proportions, root offset and blended skinning. Explicit
test-authored semantic root/effector pairs are reviewed against exact fingerprints; no
matching-name approval or commercial rig certification. A 31-frame full-turn linear motion
is transferred in both directions independently. Native exports/reopened projects are
measured at quarter frames (129 times including the native loop boundary), and a separate analytic expectation checks world
orientation and weighted positions at fractional times. Limits: .002 native bone-position
units, .05 orientation degrees and .005 native mesh units. Original source hashes, loop
flag/duplicate endpoint and 360-degree native key survive; another instance is unchanged.
Check unreviewed/stale/tampered maps, missing helper policies, animated ignored helpers,
non-unit scales, non-root translation, calibrated/in-place root policy, cancelled bake and
ambiguous fast winding rejection with no partial assignment. Root translation's numeric
service check is distinct from the in-place native fixture. General curves, arbitrary Euler
winding/loop conventions and continuous error bounds remain unverified.

GUI mode fills the real mapping grid and helper policies, rejects an empty approval,
approves the explicit original mapping, opens the retargeted instance through inherited
model editors and runs native Play with a bounded stop timer. Inspect fk-reviewed-map.png
and fk-native-preview.png: the latter is the OpenGL framebuffer, not PrintWindow. No
native-codec changes or test-side camera correction are supplied.

SharedLibraryChecks reproduces the original b45fcc3 XML schema with an actual 77-degree
native edit and a legacy manual draft, deliberately migrates it without rewriting that
file, and imports two loose original CHR0 clips onto one registered source. A bounded cursor
reads only metadata; cancel/page budgets, generation changes, simultaneous writers,
immutable-record conflicts, altered pinned metadata and missing pinned-library rejection
are exercised. Separate projects pin the identical source clip revision; native edit 25
in project one leaves project two at 180 and shared metadata/native hashes unchanged.
Project XML has no embedded blobs. Save/reload and a fresh child process decode both outputs;
the child edits/undoes again to prove independent revisions after restart. GUI mode exercises
the actual shared catalogue and Use button. All four original source hashes remain unchanged.
No mirror/fitting claims follow from independent native edits.

VerifyLibraryProduct accepts InstanceIndex (default 0), copies only the selected fixture
project into evidence and launches the normal executable twice with isolated profiles.
For shared project one choose index 1 (index 0 retains the migrated legacy edit); project
two uses index 0. It restores exact working native hashes, opens the real editor, exports
through the real path/button, saves/compares project records and observes normal exits.
PrintWindow product captures prove UI restoration only; use the focused framebuffer capture
for geometry. Compare original and library/other-project hashes after these product launches.
OS file pickers are bypassed; no manual-click, external native-tool, Modkit or game check.

Everyday current path: File > Character Library creates a shared project; register a saved
companion resolution once, or Shared library > Use an existing immutable clip. Add clips to
source imports loose CHR0 v4 without registering another model. Select source/target, review
exact roles/helpers/root policy, Transfer reviewed FK, then open the independent native
instance, edit/capture, save project and export a new enclosing native archive. V1 opening
stays embedded until Share / migrate; save the migrated project to a new file. Keep the
library directory with the projects; moving it currently requires restoring its pinned
absolute location. The Model Previewer workspace below supplies typed slots/anchors;
drag/drop, mirror/fitting and IK remain future work. Legacy drafts are available via
their explicit toggle.


Model Previewer workspace acceptance:

```powershell
./scripts/test.ps1 -Workspace -Gui
./tests/NativeSmoke/VerifyLibraryProduct.ps1 -Project <absolute-workspace-one.fpcharacter> -PreviewWorkspace
./tests/NativeSmoke/VerifyLibraryProduct.ps1 -Project <absolute-workspace-two.fpcharacter> -PreviewWorkspace
```

WorkspaceChecks starts with the real resource browser/model wrapper on an original native
model. From the previewer it attaches the shared live project, browses immutable library
metadata, installs the semantic pack, adds five jab parts and ground-start/air-loop special
parts, binds a selected native asset, keeps an assigned extra unbound, renames/reorders
without changing stable IDs and preserves a legacy custom assignment. It edits an actual
native key to 25 degrees, applies it, tests native versus project undo independently, chooses
Idle/Fall as one atomic pair, sets distinct native phases and a named Landing reference,
and previews all through the inherited timeline. An embedded reviewed rig grid transfers
original A -> different B and verifies its native output against independent analytic poses.
Focused text Undo retains the project/native state; the host shortcut handler leaves it
to the native text control. With the workspace tree focused, Ctrl-Z/Y is consumed once
by project history and restores exact state. This is handler automation, not physical-key
injection. The native numeric key editor's own Undo/Redo is tested separately.

Failure checks include invalid/cancelled packs, duplicate destinations, target/phase errors,
missing/stale anchors, cancelled background assignment, busy-view command rejection,
source overwrite, pending switch cancellation, unsupported bone edits retained after failed
Apply, and explicit two-viewer handoff/stale comparison capture rejection. No operation
may silently mutate another project or make a shared source writable. Version 1 -> 3 authoring
and undo plus existing exact v1 shared migration are separate tests. Library and previewer
use the same object/session; no synchronization through saving/reloading separate copies.

Inspect workspace-idle-native.png, workspace-fall-native.png, workspace-source-native.png,
workspace-fk-native.png and workspace-target-native.png: these are actual GL framebuffer
captures with nonblank geometry, not just form screenshots. Main target native output is
fresh-decoded at frames 0/15/30 with intended 0/25/360-degree rigid poses and weighted vertex
errors <=.002 native units. FK keeps its separate quarter-frame/.002 position/.05 degree/
.005 mesh limits. Slot/anchor metadata is verified in XML and fresh-process load; it is not
claimed to exist in CHR0/PAC. No new runtime animation behavior is implied.

The GUI checks 850x620 layout, native playback inside the visible client area, panel hide/
reopen and simulated 150% scaling with bounded dropdowns. Simulated scaling is not a real
OS-DPI/monitor test; record the actual desktop DPI separately. Layout preferences are saved
in preview-layout.xml under app data and tested separately from character project records.
Native details need a sufficiently wide editor; Authoring layout preserves the native
frame/playback controls in smaller windows.

VerifyLibraryProduct -PreviewWorkspace launches the normal executable twice using the
same isolated profile. It selects actual workspace tabs through native tab focus messages,
previews persisted Idle/Fall phases, reactivates the intended target, exports and saves
through previewer controls, compares exact saved records/native output hashes and observes
normal exits. Inputs/pickers are supplied through real path controls; no test startup mode
or external editor is added. Product window captures may omit GL; use the harness GL images
for geometry. Run both independent shared projects, then rehash originals, shared metadata/
payloads and the untouched second project. Retain every prior focused/default suite.

See UI_ACCEPTANCE.md for the usable control path and currently unsupported operations.
