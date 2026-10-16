# Local reference inspection

The user supplied Brawl and Ultimate Mario folders on 2026-10-16. Both entire directories are ignored and forbidden by the staged-source guard. All 678 input files were SHA-256 hashed before inspection; the manifest and derived reports remain under ignored `.local/references`. Never commit bone dumps, native bytes, local paths/configuration or game textures. Existing source requirements remain in the master plan; the newer user instruction authorizes local reference reads.

Verified inspection scope: BrawlLib's actual resource tree decoded `FitMario00.pac`, finding a 63-bone main MDL0 v9 and an 18-bone shadow model. ssbh_data_py 0.9.1 read the supplied Ultimate body c00 SKEL 1.0 with 98 bones. Reports list names, native indices, parents and matrices. Native conventions are preserved, not guessed into canonical coordinates. No gameplay, mesh/animation conversion, native rewrite, character provenance/authenticity or in-game compatibility is implied. Costume variants and animation folders have not been semantically compared.

Commands, from the repository root (choose new output names):

```powershell
python -m pip install --target .deps/ssbh-python --only-binary=:all: ssbh_data_py==0.9.1 numpy==2.2.6
python scripts/inspect-skeleton.py brawl 'Brawl mario/FitMario00.pac' .local/references/brawl-mario00.json
python scripts/inspect-skeleton.py ultimate 'Smash Ultimate mario/model/body/c00/model.nusktb' .local/references/ultimate-mario-c00.json
python scripts/inspect-skeleton.py self-test unused artifacts/ssbh-test/result.json
```

The last command generates a genuine original SKEL 1.0 through the verified library, checks names/parents/local/world transforms and corrupt-input rejection. It requires no game files. This is a skeleton fixture, not a skinned Ultimate model or animation fixture. Optional Python dependencies are not needed by the desktop and are not packaged. Brawl worker inspection runs in a separate process with a 45-second timeout and traversal quotas; the standalone Python inspector has an input size limit but is not a hostile-parser sandbox. Malformed offset/fuzz hardening remains M2.C.

## Official Rivals II reference

The [official rig guide](https://rivals2.com/workshop/knowledge-base/character-creation/rig-your-character/) checked 2026-10-16 allows custom bone names/hierarchy with one root. It specifies at most eight influences per vertex and removes weights below 0.01 on editor import. Its 120–200 joint range describes existing characters, not a fixed limit. Authoring forward directions differ: Maya +Z, Blender -Y, Unreal +Y. Export joints at unit scale; nonuniform parent scaling needs care. A grab bone may have any name. These facts inform future target validation; our current FK subset does not implement Rivals retargeting or editor integration.

The [official texture guide](https://rivals2.com/workshop/knowledge-base/character-creation/texture-your-character/) confirms ORMS packing: outline red, roughness green, metalness blue, shading alpha; select Mask compression after import. A base-color texture requires UseBaseColorMap; ORMS alpha may then be omitted. Base-color sRGB/import behavior still needs verification against the installed Modkit before implementation. No material conversion was added here.
