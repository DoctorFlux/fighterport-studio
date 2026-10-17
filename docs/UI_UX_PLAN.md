# Classic workspace acceptance plan

Authority: CODEX_CLASSIC_BRAWLCRATE_STYLE_WORKSPACE.md, read completely. Preserve WPF, native workers, requirement IDs and full multi-game scope. This is the focused UI plan linked by MASTER_PLAN/current.md.

Reference inspected: BrawlCrate pin d56cbac89dc2cd0e5c3f923e1d03041b16902add. MainForm.cs selection routing assigns the native node to PropertyGrid and chooses contextual previews. BaseWrapper/GenericWrapper and MDL0/SRT0 wrappers expose resource-specific commands and availability. LeftPanel.cs contains animation filtering plus object/texture lists; ModelEditControl/Designer.cs places left/right panels around the viewport and typed editors/playback at the bottom. Animation.cs/Rendering.cs dispatch animation/viewport state. BrawlCrate GUI is not installed in the inspected checkout/PATH/registered paths; source inspection is not a performed GUI comparison. No upstream UI code, icons or application shell are copied. Existing licensed BrawlLib integration remains unchanged.

| Reference workflow | FighterPort acceptance |
|---|---|
| MainForm native tree / PropertyGrid / selected preview | Default Resource Browser: compact tree/search, categorized validated name/value fields above contextual preview; selection leaves active session intact |
| Model Previewer left/right/bottom panels | Reusable Model Editor tab: animation/object/material/texture lists, viewport, bones, embedded existing VIS/SRT services and playback |
| Wrapper-specific context menu | Implemented native operations enabled; unsupported native model/container/replace/rename/delete unavailable with reasons; raw slices explicitly distinguished |
| Compact system styling | Shared WPF light/system resources, conventional controls, thin splitters; no dashboard cards or format-button row |

Acceptance: both tabs share one EditSession, ResourceWorkspace and viewport; search/recent sources are bounded; typed edits use existing validated transactions rather than mutable backend properties. Ctrl+O/S/Shift+S and contextual undo work without stealing text-entry undo. Space/arrows transport only outside text fields. Dirty cancel prevents discarding skeletal/VIS/SRT edits. Splitters persist in ignored versioned local preferences; reset changes layout only, unknown preferences are preserved. Model/resource selection, embedded edit/undo/save/export/decode, missing/unsupported cases and actual rendered preview must be exercised.

Verify running original fixtures at 1280×800 and 1920×1080 at 100%, plus a recorded 150% DPI/scaling check. Capture browser, model and contextual animation editor; inspect the actual images. Real OS-DPI changes require a separate environment when unavailable; an explicit WPF scaling stress test must be labelled as such. Native-tool/game/CI checks remain separate.

Known differences to retain explicitly: no complete native archive writer, unrestricted property editing, broad docking framework or separate model window; full shader/texgen/LOD and Maya rotation unresolved. Melee/Ultimate/Rivals/Dragon Ball/DCC capabilities remain per-operation, not implied by the classic shell.


## Classic implementation evidence — 2026-10-17

Published original-fixture smoke passed: `artifacts/classic-smoke-20261017-015053`. Actual Resource Browser, Model Editor and embedded SRT/VIS captures at 1280×800 and 1920×1080 were inspected. The actual monitor reports 96 DPI; 150% was exercised as WPF layout scaling stress, not an OS setting change. Visual review found and fixed a stale dark menu and aspect-ratio model clipping. Property-grid nonfinite input rejection, undo/redo, shared search/session, dirty Cancel branch, embedded SRT/VIS buttons, native export/decode and save/reopen passed. Routed keyboard transport/text-focus checks also passed. The dirty decision branch is injected for automation; clicking a live MessageBox is not claimed.

The existing app locks default publish DLLs, so the current build is delivered alongside it at `artifacts/publish-classic/desktop/FighterPort.Desktop.exe` through `scripts/publish.ps1 -Destination artifacts/publish-classic`. Original session is left open. Layout/recent sources use ignored `.local/classic-workspace.json`; old projects and source inputs are not migrated/destructively rewritten. Nonfinite/unsupported property values reject. Native names/hierarchy remain read-only; supported skeletal key/preview fields are editable. Search indexes at most 5,000 archive/resource entries with 100 matches and reveals ancestors; it is not a recursive disk scanner.

CI4/palette conformance is now exercised in the published texture viewer. A native material context action and sampler-inspector button show a checker-backed static color/alpha reference surface when its strict subset validates. Disabled actions explain unsupported programs. This contextual surface is distinct from the still approximate 3D model viewport. Original running-app material captures and four premultiplied pixel assertions supplement independent core expectations.

Final packaged classic/CI4/material smoke: `artifacts/classic-smoke-20261017-020959`, passed. Actual browser and material-reference captures inspected; the captured plane is explicitly labelled, not passed off as native model rendering.
