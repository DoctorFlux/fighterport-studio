# Material pipeline

Current preview: neutral WPF diffuse material with original procedural checker drawing, per-mesh cloth/skin regions and colors; UV0 and source normals skin with geometry. The checker is generated locally and requires no external textures. MDL0 native shaders/extra material and texture data remain retained in original native bytes, not editable/evaluated. No destination style fidelity claim.

REQ-08 includes four non-destructive style preset families, regions, custom normals/textures/palettes/ramps/masks/vertex colors/transparency, packing/color spaces and supported bake routes. Rivals ORMS R-outline/G-roughness/B-metallic/A-shading must be checked against the official current guide before implementation; it is not yet a verified implemented capability.
