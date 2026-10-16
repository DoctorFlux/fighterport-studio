# Development status — 2026-10-16

First runnable M1/M2 subset built and tested. 17 automated cases passed, zero failed, plus actual Windows GUI smoke and screenshot inspection. Native evidence: original humanoid -> BrawlLib MDL0 v11/CHR0 v4 -> same importer used by UI/CLI -> weighted motion -> edit -> saved project -> native CHR0 -> quarter-frame decode comparison. Error 0.000012993813; native model 126148 bytes, edited animation 240 bytes. No commercial assets used.

Desktop: New/Load/Open Demo, alternate/tail original rigs, depth-buffered playback with checker materials, skeleton inspector/markers, play/pause/scrub, camera orbit/pan/zoom, scalar key editing and undo/redo, saved project with original native resources. UI is usable but early. M1/M2 remain partial overall: see roadmap.

Limitations: generic rigs only; MDL0 v11/CHR0 v4 declared subsets; opaque material preservation, neutral preview; no general MDL0 writing, archives, native material/ancillary playback, auto mapping, retargeting, rebinding, optimization, other games or DCC/Modkit integration yet. Original Hold is evaluated locally but native export rejects it. Missing/unsupported inputs fail explicitly. Upstream unsafe parser is process-isolated but not a sandbox. No complete hostile-file fuzzing.

SDK 10.0.402 local; .NET 10 desktop/framework-dependent executable and net472 x86 worker built. BrawlLib endian writer fix is documented. Core/native tests and GUI launched; native-tool GUI/retail/in-game/remote CI unrun. Private remote created and verified, first source push pending. Next: M1.D restricted fingerprinted FK profile and transfer tests, after first safe commit/push.
