# Original fixture notice

All procedural character designs, rig names, mesh/UV/checker data and motion values in Demo.cs and ColladaFixture.cs were created for this project. No game assets or third-party character references were copied. Permission is granted to use, modify and redistribute the generated original fixtures and their generators for testing, with this notice retained. Other project source has no general public license grant at this checkpoint; third-party dependencies retain their own licenses.

Fixtures are generated deterministically; native binary outputs are ignored and never pushed. BrawlLib is the genuine native encoding backend. Core generation has no runtime inference, network or DCC requirement. Float exporter precision, zero-weight removal and backend endian fix are documented in ADR 0002.

Continuation fixtures: scripts/generate-ultimate-fixture.py constructs an original two-bone rig, a blended triangle and a rigid accessory triangle, UVs/normals, a neutral placeholder material and quaternion motion entirely in source. ssbh_data_py writes genuine NUMDLB/MESH/SKEL/MATL/ANIM resources. The test material identifier is deliberately not a game shader. UltimateTests exercises independent fractional skin expectations and both-direction same-rig native animation bakes/write/decode. Generated files stay ignored; no commercial geometry, textures or animation samples are embedded in this fixture source.

WorkerFailureTests uses a clearly separate test executable fault mode to exercise process supervision; it is never substituted for a game parser/exporter. Malformed native mutations derive only from the original BrawlLib-generated fixture. Supplied Mario tests are explicit opt-in in LocalReferenceTests and excluded from portable CI.
