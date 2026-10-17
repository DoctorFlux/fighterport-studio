# Native restart smoke checks

The 2026-10-17 humanoid continuation uses `scripts/test.ps1 -Humanoid -Gui` for
independent continuous reach/pole tests, explicit held contact endpoints, original deep
crouch/native loop fixtures, real interval controls and native save/fresh decode. See
docs/TESTING.md for withheld fractions, budgets, the opt-in local standing diagnostics
and unsupported charts/clearance cases. The commercial transported-pole route is not
approved by passing the original fixtures. Existing native/editor/library suites remain
required; previous checkpoint counts are not new evidence.

The current authoring suite is `./scripts/test.ps1 -Library -Gui`: two original rigs,
stable source-bound library/project IDs, native motion capture, independent instances,
session undo/redo and genuine native output/reopen. Run
`VerifyLibraryProduct.ps1 -Project <absolute-generated-library-ui.fpcharacter>` for
two fresh executable launches, restored edits/assignments and native export controls.
See docs/TESTING.md for exact scope, budgets, manual workflow and limitations. The
library suite uses the product's own camera framing; older suites below set a fixture
camera explicitly. Full native/gameplay/retargeting fidelity is not implied.

`NativeSmoke` is a new .NET Framework 4.7.2/x86 harness for the imported application.
It has no legacy FighterPort Core, worker, project-format or renderer dependency.
Its two-bone paddle, four-color texture, UVs and rotation animation are original
procedural test artwork. The source and output permission notice is in
`OriginalFixture.cs`. No commercial game input is required or substituted.

Run `./scripts/test.ps1` for a locked Release/x86 build and native checks, or
`./scripts/test.ps1 -Gui` to include the interactive desktop checks. The wrapper
builds this project and its actual application/library references. For a direct
invocation, run `NativeSmoke.exe --native <new-evidence-directory>` for native
encode/edit/decode checks, or `--gui` for interactive-desktop checks. Use a new
directory each time; the harness refuses to overwrite earlier evidence. A 90-second
watchdog records a failure if a native modal dialog or editor operation hangs.

The GUI mode initializes the application's static startup before creating fixture
controls, enters a real `Application.Run(MainForm)` loop, shows the actual
Source Browser, selects the original BRRES and invokes its Open editable copy button.
That workflow stages a verified copy and invokes the native Open path. The harness
then selects native resource wrappers and opens the existing model
previewer through `MDL0Wrapper.Preview`. It expands the inherited editor panels
through their toggle buttons, selects the installed viewport and frames the
original fixture with the native camera API. It evaluates CHR0 poses while scrubbing,
invokes the real Play button, edits a numeric field through the CHR0 control's
change handler, supplies a separate destination to the native Save handler and
reopens it through the application's Open path. This is an automated control/API
smoke check in the real WinForms/OpenGL editor, not a claim of manual clicking.
The Save As operating-system dialog itself is bypassed. The model window is
temporarily topmost for the bounded visible-render check and is closed afterward.

Both modes enumerate a source folder, check signature availability, stage a
hash-verified independent copy, reject malformed/compressed/outside-root/cancelled
staging and attempt a native export into the protected source folder. A new isolated
source-root registry and staging directory under the evidence directory avoid
adding test mounts to the user's registry. This uses the guard's explicit test
initialization API; it does not disable protection.
A separate short-lived hidden process reloads that registry and verifies the source
write block survives process exit. A second concurrent process registers another
root; both processes must observe each other's registration, preserve both roots,
and reject native Export under the newly protected root. Writable native streams
and memory maps are checked against the actual protected file handle. Application,
data and OS temporary directory mounts are rejected. A real hardlink and 8.3 alias
are checked when available; unavailable aliases are explicitly logged as skipped.
Signature recognition is also checked on a valid
BRRES deliberately named `.txt`; the staged extension must match the native format.
An original uncompressed ARC/PAC containing that BRRES is staged and decoded too.

The harness sets the supported `FIGHTERPORT_DATA_ROOT` override to the evidence
settings directory before application preferences are accessed. It tests the real
independent provider using fresh settings instances: audio volume, original inert
code-list text, COLLADA options, recent files and complex model-viewer settings.
It also checks explicit binary serialization, exact mixed newline preservation,
group-scoped reset, and a fresh process reloading application/library preferences.
The portable default root is a separate executable-startup check.

Results and app-only `PrintWindow`/native OpenGL framebuffer captures are written
locally. The capture code does not read an arbitrary screen region. Image existence is
not visual correctness; inspect the captures before claiming visible geometry,
texture fidelity or readable UI. Native decode assertions use independent expected
texture pixel values and motion key values, but encoding and decoding share
BrawlLib. No separate native tool, hardware console, Modkit or game check is implied.

The tested resource subset is one original BRRES containing MDL0 v11, a two-bone
rigid mesh, one material/sampler, 32×32 RGBA8 TEX0 and looping CHR0 v4. This does not
prove arbitrary models, blended skinning, other channels or universal native write
fidelity. Source hashes must match before/after. Generated binaries, native assets,
logs and captures stay ignored and out of commits.

The verified restart run on 2026-10-17 used Release/x86, a 1920×1080 desktop at
96 DPI, MainForm 1050×760 and the model window 1180×820. The completed wrapper log
is `artifacts/restart/test-wrapper-20261017-054521.log`, with native evidence in
`artifacts/restart/native-20261017-054523-481` and GUI evidence in
`artifacts/restart/gui-20261017-054526-294`. All seven source-folder file hashes
matched. The source BRRES SHA-256 was
`60d532743a03adda12f518127e08c1eba55ee0a9bf3ff1be5a4e819829377321`.

The final captures were inspected: `00-open-source.png` shows the source list,
`01-texture-editor.png` the original four-color texture, `02-material-editor.png`
the native material properties, `03-viewport-frame-1.png` the upright textured
mesh, `04-viewport-frame-16.png` its 60° pose, `05-edited-frame-16.png` the visible
35° mesh pose and numeric editor, and `06-reopened-native.png` the separate saved
BRRES's CHR0 in MainForm. Playback advanced to frame 16 during the bounded timer
check. The D: volume exposed no distinct 8.3 alias, which this run records as skipped;
its actual outside hardlink test passed. Earlier failed runs remain local diagnostic
receipts and are not verification evidence. In particular, blank viewport captures
were rejected until the actual viewport was selected and the camera framed.

Companion milestone: run `scripts/test.ps1 -Companions -Gui`. CompanionChecks.cs
splits the original model/material, texture and CHR0 into separate input folders,
adds a different magenta texture with the same name, and retains an original SRT0
v4 entry in the base BRRES. It requires explicit second-candidate selection and tests
missing/ambiguous resolution, hash mismatch, absent source, relative/network paths, wrong native identity,
manifest write protection and XML-entity rejection. Original inputs are not modified
to create negative cases; malformed resolution copies carry the deliberate errors.

The final companion native/GUI evidence is `artifacts/restart/companions-native-20261017-090347-261`
and `artifacts/restart/companions-gui-20261017-090351-036`. The GUI exercises real
selectors, Save resolution, Open assembled copy, texture/material editors, visible
OpenGL poses, native Play, the 35-degree numeric edit, protected Save and separate
native reopen. Four source hashes match before/after and in a separate fresh process.
SRT0 preservation checks only name/version/timing/loop metadata on this original
empty-entry fixture. No new general SRT0 editing or opaque fidelity claim follows.

Standalone product restart evidence is `artifacts/restart/companion-product-20261017-090452-628`:
two fresh FighterPort.Studio.exe processes reopen the resolution, enable the actual
workspace controls, open a new native BRRES and exit normally using an isolated
profile. Exact persisted choice XML is compared, since a ListView subitem is not
ordinary Win32 window text. All input roots and generated native data remain local.


Multi-root loose dumps: `scripts/test.ps1 -DumpSources -Gui` uses original matching
base/override/conflict layouts, unrelated same-name resources, lightweight paged
header discovery, explicit scoped choices, bounded/cancelled/unavailable discovery
and two process reloads. It exercises the actual dump list and Stage action before
native companion preview/Play/key edit/separate Save/reopen. Ten original source
hashes must stay unchanged. A real Windows junction is created only inside the new
ignored evidence directory and must be rejected; recovery/game inputs are untouched.
The source choices and saved work must survive cancelled rediscovery and changed
priority recommendations. Existing default and companion suites remain separate;
companion checks include old version 1 resolution loading.

`VerifyDumpProduct.ps1 -Resolution <absolute-original-fixture-dump-ui.fpcompanion>`
launches FighterPort.Studio.exe twice, checks actual restored root/companion windows,
compares persisted mount/hash/native identities, opens the new native output and
observes normal exits. Captures/output remain ignored. OS folder/Save As pickers
are bypassed throughout; no manual-click, native-tool or game claim follows.


`-Containers -Gui` selects the original nested PAC fixture, not a commercial fighter.
It proves explicit full container identities, native enclosing PAC save/reopen, exact
unrelated payload preservation, nonidentity binds and two-bone blended-skin positions
against analytic expected results. It tests header-loss rejection, cancellation,
bounds/depth, stale identities, source protection and unchanged original hashes.
Use VerifyDumpProduct.ps1 -NativeExtension pac for the normal product entrypoint check.
The --inspect-brawl mode is explicit read-only local metadata inspection only; its TSV
and hashes stay ignored. It is not retail native editing or game validation.


Reviewed FK and shared storage: run `scripts/test.ps1 -Retarget -Gui` and
`-SharedLibrary -Gui` separately. RetargetChecks uses original 2/4-bone changed-axis,
changed-parent/proportion blended fixtures, reviewed roles/helpers and independent forward/
reverse analytic/native validation, quarter-frame decode and full-turn/native-loop-boundary
checks. SharedLibraryChecks uses immutable disk-backed source/multi-clip records, legacy
v1 native-edit migration, separate project files, fresh-process edits/undo, cancellation,
concurrent writers, cursor/record invalidation and unchanged source hashes. The actual
shared catalogue and mapping grid/native preview/Play are exercised. See TESTING.md for
numeric limits, budgets and unsupported cases; no mirror/fitting/Modkit/game claim.
`VerifyLibraryProduct.ps1 -InstanceIndex 1` selects the shared project's second (retargeted)
instance; default 0 still verifies older projects. It requires no gameplay draft.


`-Workspace -Gui` proves the actual Model Previewer session/typed slots/reference workflow.
It includes same-object library synchronization, native Apply/Undo, bound/unbound parts,
repeat counts, stable IDs through rename/reorder, other named references, cancellable worker,
explicit two-view handoff and unsupported/pending edit rejection. See TESTING.md and
UI_ACCEPTANCE.md for detailed GUI steps, native pose tolerances and honest DPI limits.
Use VerifyLibraryProduct.ps1 -PreviewWorkspace on each independently saved fixture project
for normal application restarts, restored anchor phases, previewer export/save and exits.
No source assets, generated binaries, settings or captures belong in Git.

Proportion/contact continuation (2026-10-17): run `scripts/test.ps1 -Humanoid -Gui`
for original full humanoids, palette/mip/selected-container intake, exact shared rig/body
review, native FK/corrected contact output and actual previewer controls/handles/undo.
See docs/TESTING.md for the independent analytic expectations, numeric limits, opt-in
private local pair configuration and separately launched normal-product restart checks.
This is a bounded rigid/two-bone route, not a complete arbitrary humanoid/game solver.
The local reverse standing recipe remains a recorded rejected boundary. Mirroring is
not a prerequisite. Game inputs, derivatives, captures and configs remain ignored.
