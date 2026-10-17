# Capability matrix — verified restart checkpoint

Inherited implementation is source-inspected, not automatically verified support.
No previous FighterPort counts or capabilities transfer to this codebase.

| Game/resource | Read/edit/write | Preview/GUI | Package/game |
|---|---|---|---|
| Original Brawl BRRES / MDL0 v11 / CHR0 v4 / TEX0 v1 RGBA8/material | Original fixture encode/decode, exact geometry/identity binds, key edit, Save-new/reopen and unchanged source hashes passed | Actual native tree, texture/material controls, textured OpenGL poses, playback and key edit observed | No separate native tool or game check |
| Original raw ARC/PAC 0x0101 containing the fixture BRRES | Source staging and embedded model/animation decode passed | BRRES workflow above; no separate ARC editing GUI claim | No game check |
| File > Open Source | Bounded loose-folder inventory, detected extension, verified working copy, guarded native outputs and persistent concurrent mounts tested | Actual list/button opens native editor; standalone product entrypoint also checked | No installation/image/decompression provider yet |
| Explicit Brawl companions: model BRRES / TEX0 v1 RGBA8 / CHR0 v4 | Missing/ambiguous choices, second-candidate selection, versioned source/resource identities, independent-process restore, guarded assembly, native edit/new-save/reopen and four unchanged sources passed | Actual selectors, native textured poses/playback/key edit and two standalone executable launches verified | Brawl-only loose resources; no palette, cross-rig, container, package or game claim |
| Configured multi-root Brawl loose dumps | Paged header-only discovery; complete relative-file/container-path overrides; equal-priority ambiguity; explicit lower-priority choice; roots/priorities and hash/native-identity pins restored across processes; six budgets/cancellation/missing/changed/reparse cases tested | Actual dump list/stage actions feed companion selectors and native textured playback/key edit/new-save/reopen; standalone product restart checked separately in status | BRRES/TEX0/CHR0 discovery only. No PAC/PCS nesting, installation/image access or fighter-package round trip |
| Base SRT0 v4 preservation | Original empty-entry fixture name, 17-frame timing and loop flag retained through assembly/native edit/save/reopen | No SRT0 authoring/preview claim added | Does not prove arbitrary channel/opaque-field preservation |
| Other inherited Brawl editors/resources | Source retained; operation-specific tests pending | Inherited controls retained, unverified | No universal fidelity claim |
| Smash 64, Melee, Wii U, 3DS, Ultimate | Planned per game/version | Planned | Planned native output/gameplay/game checks |
| Rivals II installed sources / Modkit projects | Planned separately from cooked extraction | Planned actual Modkit validation | Planned import/build/playtest |
| FighterZ, Sparking! ZERO, Xenoverse, Xenoverse 2 | Planned source and native-output adapters | Planned | Native/playable-mod goals retained |
| Additional games / interchange / DCC bridges | Planned modular adapters and explicit subsets | No mandatory DCC | No implied compatibility |

Retargeting, contacts, baking, complete character authoring and gameplay conversion
are future milestones. Same-format editing is not cross-rig retargeting.

Source admission recognizes raw BRRES/ARC and MDL0 v8–11, CHR0 v3–5, TEX0 v1–3,
PLT0 v1/3, SRT0 v4/5, PAT0 v3/4, CLR0/VIS0/SHP0 v3/4 and SCN0 v4/5. These are
header preflight rules, NOT tested round-trip claims for all admitted resources.
Compressed or opaque ARC children are refused. Existing native File > Open retains
broader inherited readers whose operation/version support remains unverified here.

Settings tests cover scalar, collection, XML/binary and complex viewer values, scoped
reset and cross-process persistence. Source guards are application-level checks, not
an OS sandbox. Many native exporter branches remain unexercised. The repeatable D:
8.3-alias probe was skipped; real hardlink and cross-process mount probes passed.
See DEVELOPMENT_STATUS.md and tests/NativeSmoke/README.md for exact evidence and limits.

Brawl character-readiness queue (inherited source retained; queued operations are unverified):

| Resource/version | Read | Preview/evaluate | Edit | Preservation | Native write / package |
|---|---|---|---|---|---|
| BRRES + MDL0 v11, rigid original fixture | Native tested | Textured poses tested | Declared key/material workflow only | Geometry/identity binds checked | Separate BRRES tested; no fighter package |
| MDL0 other versions / blended weights / changed rest hierarchy | Header admission only | Nontrivial fixtures queued | Queued per subset | Queued decoded binds/weights | Queued per version |
| CHR0 v4 original rigid motion | Native tested | Native playback tested | Numeric key tested | Declared key/timing/loop checked | Native BRRES save/reopen tested |
| CHR0 other versions / arbitrary rig semantics | Header admission only | Queued | Queued | Tangents/winding/scale/fractional cases queued | Queued, no retarget claim |
| TEX0 v1 RGBA8 without palette / one material | Native tested | Original texels/material preview tested | Inherited controls exercised; no broad edit claim | Original selected pixels checked | BRRES assembly/save/reopen tested |
| TEX0 other versions / PLT0 / palette alpha / mip chains / multiple materials | Header admission only | Queued original fixtures | Queued | Queued decoded pixels/palette/mips | Queued per version |
| SRT0 v4 empty base entry | Native metadata tested | Animated fidelity unverified | Unverified | Name/timing/loop only | Base BRRES metadata retained |
| Animated SRT0 / PAT0 / CLR0 / VIS0 / SHP0 / SCN0 and material/shader semantics | Header admission only | Nonempty synchronized fixtures queued | Queued | Opaque/curve fidelity unverified | Queued per version/channel |
| ARC/PAC 0x0101 original raw nested BRRES | Declared source staging/read tested | Embedded fixture only | No container GUI edit claim | General unknown-entry fidelity unverified | Container-preserving companion output queued |
| PCS/compressed/nested character packages | Loose provider refuses explicitly | Queued | Queued | Hierarchy/unknown entries must survive or fail precisely | Bounded decompression/recompression and package checks queued |

Configured real-input versions must be inventoried opt-in before widening these gates.
The local configuration currently names Brawl/Ultimate Mario folders, not a Goku source;
their historical metadata is not new verification. No commercial resource was decoded in
the multi-root fixture suite. Full Brawl/Project M/Project+ scope remains in the master plan.
