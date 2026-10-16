# Next executable acceptance criterion

**M2.C2 — Bounded malformed-native regression suite and worker failure handling.**

M1.D2 desktop manual mapping and M2.C1 encoded CHR0 validation before atomic new-file publication are complete for their declared subsets. Keep all master scope and existing evidence.

1. Read AGENTS, MASTER_PLAN, DEVELOPMENT_STATUS, NativeBackend, EncodedClipValidator and current native worker boundaries. Retain all 27 tests, optional SKEL self-test and GUI smoke.
2. Generate deterministic malformed size/offset/count/truncation cases from original MDL0/CHR0 fixtures. Execute each in a bounded subprocess, cap total workload, handle worker crash/timeout/cancellation, verify no published partial output and unchanged sources. Do not parse game inputs in CI.
3. Strengthen header/length/offset checks that the actual backends expose safely; don't invent complete format validators. Add response/output/log limits and explicit diagnostics. Process isolation alone is not a security sandbox. Test failed validation/cancellation at the full export publication boundary, including destination races.
4. Review decoded export validation beyond fixed quarter-frame samples: narrow Hermite extrema, loop-end tangents and world-space/end-effector/vertex errors remain unproven. Keep declared tolerances and reject failures without relaxing them.
5. Build/native tests/GUI smoke/publish and update status, capability evidence and master checkpoint. Inspect staged diff; recheck exact PRIVATE origin before commit/push. Use existing repository only; no force push or CI dispatch.

Then: native BrawlCrate GUI checking (shared backend, not independent correctness), M3.A explicit rest-axis/hierarchy correction with original mismatched rigs and confidence-based mapping; M3 channel fixtures; verified Brawl-to-Rivals interchange/Modkit; full Ultimate adapter. Preserve Melee/Dragon Ball/legacy/DCC requirements in the master plan.

Local references now available: `.local/references/brawl-mario00.md/.json` and `ultimate-mario-c00.md/.json`. Brawl main 63 bones plus shadow 18; Ultimate c00 98. All 678 supplied input hashes verified unchanged. Reference matrices are native conventions, not canonical mappings. Both user folders, manifests and reports stay ignored; never stage their contents. Optional Python inspector uses ssbh_data_py 0.9.1 / NumPy 2.2.6. Official Rivals rig/texture findings are in REFERENCE_INSPECTION.md; no Modkit install/cook or game run performed.

Source branch main, exact repository in config/repository.json. Verified paths and commands are in DEVELOPMENT_STATUS.md. No additional user input is required for M2.C2.
