# Capability matrix — verified restart checkpoint

Inherited implementation is source-inspected, not automatically verified support.
No previous FighterPort counts or capabilities transfer to this codebase.

| Game/resource | Read/edit/write | Preview/GUI | Package/game |
|---|---|---|---|
| Original Brawl BRRES / MDL0 v11 / CHR0 v4 / TEX0 v1 RGBA8/material | Original fixture encode/decode, exact geometry/identity binds, key edit, Save-new/reopen and unchanged source hashes passed | Actual native tree, texture/material controls, textured OpenGL poses, playback and key edit observed | No separate native tool or game check |
| Original raw ARC/PAC 0x0101 containing the fixture BRRES | Source staging and embedded model/animation decode passed | BRRES workflow above; no separate ARC editing GUI claim | No game check |
| File > Open Source | Bounded loose-folder inventory, detected extension, verified working copy, guarded native outputs and persistent concurrent mounts tested | Actual list/button opens native editor; standalone product entrypoint also checked | No installation/image/decompression/companion provider yet |
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
