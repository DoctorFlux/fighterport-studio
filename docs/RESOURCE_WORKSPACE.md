# Resource workspace — tested initial slice

Launch `artifacts/publish/desktop/FighterPort.Desktop.exe`. The existing WPF application now uses one Open command, lazy native trees, a large resizable viewport, context properties, menus and one playback timeline. Tools > Map / Retarget retains semantic/manual mapping and approved transfer modes. Demos contains the original rigs. Advanced bone/key editing retains scalar key edits and undo/redo.

## Supplied Brawl Mario: click-by-click

1. Click **Open…** (Ctrl+O), select the locally configured `Brawl mario/FitMario00.pac`.
2. Expand the PAC, **Model Data [0]**, **3DModels(NW4R)**. Double-click **FitMario00 [MDL0]**. Do not choose the separate shadow model under Model Data [10].
3. Native material references automatically resolve textures in the ARC Texture Data bank. Nine unique textures resolve; five sampler-0 images are used by this preview. Keep **Source textures (approx.)** checked. Other samplers and the native TEV shader are not reproduced.
4. Click **Open…** again and select `Brawl mario/FitMarioMotionEtc.pac`. The active model stays visible.
5. Expand **FitMarioMotion**, **Animation Data [0]**, **AnmChr(NW4R)**. Double-click **Wait2 [CHR0]**.
6. In the same animation BRRES, expand **AnmTexSrt(NW4R)** and double-click **Wait2 [SRT0]**. This is an explicit companion choice; the editor does not guess across files by name. Four eye-material sampler-0 UV tracks bind; all twelve native tracks remain retained. The other eight sampler tracks are unevaluated.
7. Click **Play / Pause**, or scrub. Skeletal motion and the implemented eye UV scale/translation use the same frame. Uncheck **SRT0** to restore rest UVs. Wait1's moving eye tracks use additional samplers, so Wait2 is the demonstrated visible subset. This is not full native eye/shader fidelity.
8. Click **Save workspace…** and save a `.fpwork` outside the input files. Reopen it through **Open…**. The app verifies every source hash and reloads textures; keep originals at their paths. Source relocation/relink UI is pending.
9. Single-click resources to inspect type/version/source. Expand Materials within the active MDL0 to inspect native sampler references. Right-click a TEX0 for preview, decoded PNG extraction, or an explicit persisted sampler override. **View > Problems / dependency limits** lists unresolved resources and approximation limits.

The main supplied motion BRRES has CHR0/CLR0/SRT0, not VIS0. CLR0 remains present in the tree but unevaluated. No commercial source, decoded content or screenshot is in Git.

## Original visibility and UV conformance workflow

Run `./scripts/test.ps1 -WorkspaceOnly`. Its reported timestamped test directory contains genuine `workspace-model.pac` and `workspace-motion.pac`, generated through BrawlLib. Open the model PAC, expand Model Data [0] > 3DModels(NW4R), and double-click AsterFixture. Open the motion PAC, activate Punch in AnmChr, OriginalAccessory in AnmVis and OriginalUvSlide in AnmTexSrt. One accessory disappears at frame 10 and returns at 20; UV motion and the skeleton share the timeline. VIS0 mute restores default visibility. PreservedUvGap is a genuine empty SRT0 resource, not an animated support claim. The model contains two same-name texture banks in different ARC groups; the native scope selects the correct quadrants.

For actual published apphost verification:

```powershell
./scripts/gui-smoke.ps1 -Published -WorkspaceModel '<test-directory>/workspace-model.pac' -WorkspaceMotion '<test-directory>/workspace-motion.pac'
./scripts/gui-smoke.ps1 -Published -WorkspaceModel 'Brawl mario/FitMario00.pac' -WorkspaceMotion 'Brawl mario/FitMarioMotionEtc.pac'
```

The smoke invokes the same open/activate/expand/save commands, asserts decoded original quadrant colors and accessory state, and creates a local capture. Original smoke also exercises explicit skeletal CHR0 bake/write/decode. Commercial captures are kept local and not sent to analysis services.

## Conversion and current boundaries

**Convert…** lists Brawl, Ultimate, Melee and Rivals 2. Complete character conversion is blocked for all four until their missing writers/packages are implemented. The explicitly accepted **skeletal-only** option uses existing measured native baking and genuine CHR0/NUANMB writers with decode validation. It does not create a target rig, transfer Mario across rigs, export a model, preserve ancillary groups in that output, or produce a playable fighter. Original workspace/source resources remain retained. Looping NUANMB output and unsupported bake cases still reject.

Preview: direct Maya ST and XSI/3dsMax SRT0 sampler 0/UV0, VIS0 draw visibility, decoded TEX0 color/alpha and approximate material appearance. Maya rotation, additional UVs/samplers, TEV, alpha-test, material/color animation, PAT0 switches, morphs and constraints remain pending. Original CI8/RGB5A3 alpha and two-mip fixtures pass; full format/wrap coverage remains pending. Indexed palette ambiguity rejects rather than selecting the first palette. Raw extraction is an **unrelocated byte slice**, not a portable native exporter.

FPWORK v1 stores bounded verified external document links, canonical model/clip state and typed VIS0/SRT0 groups. No texture cache bytes are embedded. Unknown payloads survive via unchanged source documents. Moved or modified sources reject reopen. Undo covers scalar keys and a separate 16-step visibility editor history, not all workspace commands. Search/recent-source UI, viewport bone picking, dirty-state prompts, richer channel editing/mute/solo/rate controls, companion auto-assembly, full dependency graphs, SRT0/other ancillary editing/export and native package export remain open acceptance criteria. Folder Open indexes at most 256 immediate entries; no recursive disk scan. Ultimate still uses the existing skeletal backend; a full logical companion tree/material preview is pending.


## Palette, rotating UV and visibility editing increment

1. Run `./scripts/test.ps1 -WorkspaceOnly`; use its newly reported test directory. Open `indexed.pac`, expand the BRRES and Textures group, then double-click **IndexedAlpha [TEX0]**. In the texture window select **Mip 1**, **AlternatePalette**, then **Decode selected mip / palette**. It displays a 16×16 image with yellow/green/blue/transparent quadrants. Mip 0 is 32×32. The palette-use button applies the selected palette to listed active-model uses of that exact texture; Save workspace persists it. The viewport still uses mip 0.
2. Open the generated model and motion PACs as above. Activate **OriginalUvMatrixXSI** or **OriginalUvMatrix3dsMax** under AnmTexSrt; scrub to 20.25. These genuine SRT0 v4 fixtures animate rotation, scale and translation. **OriginalUvSlide** retains the earlier Maya ST example.
3. Bind **OriginalAccessory [VIS0]**, scrub to frame **10**, then right-click it and choose **Edit active visibility / export VIS0…**. Select the LeftHand track, enter group frame **10**, check Visible, then **Set visibility at group frame**. The accessory becomes visible at that frame. Use the dialog's **Undo visibility edit** and **Redo visibility edit** buttons.
4. To correct a binding, choose a bone and click **Bind selected track to selected bone**. This explicitly changes both the preview bone and exported native target name; duplicate bindings reject. Undo restores the previous mapping. The dialog edits the active group identified in its title, not an arbitrary selected tree node.
5. Choose **Export active VIS0 v3 / decode-check…** and a new `.vis0` path. The genuine native writer is followed by decode checks of name/version/original-path metadata/frame count/loop, track names, constant representation and every Boolean value. Existing outputs and original sources cannot be overwritten. Close the dialog and Save workspace to retain working-copy edits. Reopen the exported VIS0 through Open and bind it to verify playback.

This is a standalone VIS0 output, not PAC/BRRES repacking or full character conversion. Unsupported channels remain in their source documents. Rate/offset/mute overrides block export rather than being dropped. Native user-data/opaque metadata round trips, SRT0 editing/export and native-tool/in-game validation are not claimed.

## SRT0 curve editor and corrected sampling

Use the current published executable. Open FitMario00.pac, expand Model Data [0] → 3DModels(NW4R), and double-click FitMario00. Open FitMarioMotionEtc.pac, expand FitMarioMotion → Animation Data [0]. Activate Wait2 under AnmChr, then Wait2 under AnmTexSrt. Scrub and toggle SRT0 to compare with static material defaults. Original texture data and UVs remain unchanged; bitmap-row/addressing correction is at draw submission. Eye layering remains approximate.

For independently testable editing, open the latest generated workspace-model.pac and workspace-motion.pac, activate model, Punch CHR0, then OriginalUvEditable SRT0. Right-click an SRT0 → **Edit active SRT0 curves / bindings / export…**. Select a track and channel, enter integer frame 10, value 0.375 and tangent 0.03125, then **Set key / explicit tangent**. Select a key and enter another frame to move it. Undo/redo is available in the editor and through Ctrl+Z/Y; closing/reopening retains the session. Preview material and native exported binding are separately editable. Additional/indirect tracks remain visible even when unevaluated.

Choose **Save workspace…** to persist edits, or **Export SRT0 v4 / decode validate…** to a new path. The writer preserves all source tracks and metadata and checks the decoded result; it does not repack a PAC. Original/v5/unknown data are never silently replaced. SRT0 source changes and closing prompt for unsaved edits. Other workspace commands still need broader undo/dirty coverage. See TEXTURE_DIAGNOSIS.md for full limitations and verified scope.
