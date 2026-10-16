# Retargeting

`FkRetarget` implements a restricted, deterministic, non-AI FK subset in the core and CLI. Same-skeleton CHR0 import remains independent and binds exact unique bone names.

`map-template <source.fpproj> <target.fpproj> <profile.json>` creates an **unapproved** profile with exact-name suggestions only. Review source/target fingerprints and BoneIds (stable source ID -> target ID), correct mappings manually, choose RootTranslation (`Reject` default, or explicit `CopyDelta`), then set Approved=true. No anatomy confidence is implied by name matching.

`retarget <source> <clip-index> <target> <approved-profile.json> <new-output>` adds the transferred clip to a cloned target project. It retains target mesh/binds and existing clips, preserves source inputs, saves the approved profile and sets branch metadata to target/fk. A new output path is required. Full branch-management UI is still pending.

Required subset: mapped ancestors/hierarchy match, local rest Euler axes match, rest scales are one. Rotation values transfer relative to source/target local rest angles without reducing full turns. Hermite tangents and key timing/locks remain unchanged. Translation is permitted only for mapped root under explicit CopyDelta (centimeters, no automatic proportion scaling). Other translation, scale channels, incompatible axes/hierarchy, incomplete mappings or stale fingerprints fail without dropping channels. This is animation transfer only, not mesh rebinding.

Five M1.D tests and the packaged CLI verified these rules, renamed/proportioned rigs, input immutability and profile save/reopen. The alternate demo's rotated root intentionally fails the uncorrected matching-axis subset. GUI mapping table/3D selection, anatomical inference/confidence, axis/rest correction, twist distribution, contacts/IK, rebinding and facial/material mappings remain REQ-07 work.
