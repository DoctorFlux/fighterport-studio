# Native restart smoke checks

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
