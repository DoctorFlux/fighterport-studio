// Original procedural test artwork, authored for FighterPort Studio, 2026-10-17.
// Permission to use, modify and redistribute this generator and its original output
// for testing is granted with this notice retained. No game content is embedded.
using BrawlLib.Imaging;
using BrawlLib.Internal;
using BrawlLib.Modeling;
using BrawlLib.Modeling.Collada;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using BrawlLib.Wii.Graphics;
using BrawlLib.Wii.Textures;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

internal static class OriginalFixture
{
    public static string Create(string directory, bool blended = false, bool differentRig = false)
    {
        Directory.CreateDirectory(directory);
        string dae = Path.Combine(directory, "original-paddle.dae");
        // The inherited fast COLLADA reader expects double-quoted XML attributes.
        string scene = ColladaSource;
        if (blended)
        {
            scene = scene.Replace("<float_array id='weights-array' count='1'>1</float_array>", "<float_array id='weights-array' count='2'>0.25 0.75</float_array>")
                .Replace("source='#weights-array' count='1'", "source='#weights-array' count='2'")
                .Replace("<vcount>1 1 1 1</vcount><v>1 0 1 0 1 0 1 0</v>", "<vcount>2 2 2 2</vcount><v>0 1 1 0 0 1 1 0 0 0 1 1 0 0 1 1</v>")
                .Replace("<node id='Paddle' name='Paddle' sid='Paddle' type='JOINT'><matrix>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", "<node id='Paddle' name='Paddle' sid='Paddle' type='JOINT'><matrix>1 0 0 0 0 1 0 6 0 0 1 0 0 0 0 1")
                .Replace("count='32'>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", "count='32'>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 0 0 0 0 1 0 -6 0 0 1 0 0 0 0 1");
        }
        if (differentRig) scene = scene.Replace("-3 0 0 3 0 0 3 18 0 -3 18 0", "-5 0 0 5 0 0 5 24 0 -5 24 0");
        File.WriteAllText(dae, scene.Replace('\'', '"'));
        Collada._importOptions = new Collada.ImportOptions
        {
            ModelVersion = 11, SetOriginalPath = false, BlenderFBXBoneFix = false,
            ForceFloatVertices = true, ForceFloatNormals = true, ForceFloatUVs = true
        };
        using (var importer = new Collada())
        using (var archive = new BRRESNode { Name = "OriginalRestartFixture" })
        {
            var model = importer.ImportModel(dae, Collada.ImportType.MDL0) as MDL0Node;
            if (model == null) throw new InvalidDataException("Original fixture import failed: " + Collada.Error);
            model.Name = "OriginalPaddle";
            archive.GetOrCreateFolder<MDL0Node>().AddChild(model);
            model.Populate();
            if (differentRig)
            {
                // Rig B shares misleading Root/Paddle names with A, but has a translated
                // bind, different proportions, and an additional rotated descendant.
                var paddle = model.BoneCache.OfType<MDL0BoneNode>().Single(b => b.Name == "Paddle");
                var tip = new MDL0BoneNode { Name = "OriginalTipB", BindState = new FrameState(new Vector3(1), new Vector3(0,0,90), new Vector3(0,9,0)) };
                paddle.AddChild(tip); ((MDL0BoneNode)paddle.Parent).RecalcBindState(false,false);
                model._linker.RegenerateBoneCache(true);
            }
            Console.WriteLine("Fixture import groups: " + string.Join(", ", model.Children.Select(c => c.Name + "=" + c.Children.Count)));
            using (var bitmap = new Bitmap(32, 32))
            {
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                        bitmap.SetPixel(x, y, x < 16 ? (y < 16 ? Color.Red : Color.Blue) :
                            (y < 16 ? Color.Lime : Color.Yellow));
                var texture = archive.CreateResource<TEX0Node>("OriginalQuadrants");
                texture.ReplaceRaw(TextureConverter.RGBA8.EncodeTEX0Texture(bitmap, 1));
                texture.Name = "OriginalQuadrants";
            }
            foreach (MDL0MaterialNode material in model._matList)
            {
                var sampler = material.Children.OfType<MDL0MaterialRefNode>().FirstOrDefault();
                if (sampler == null) { sampler = new MDL0MaterialRefNode(); material.AddChild(sampler); }
                sampler.Texture = "OriginalQuadrants";
                sampler.Coordinates = TexSourceRow.TexCoord0;
                material.CullMode = CullMode.Cull_None;
            }
            var animation = archive.CreateResource<CHR0Node>("OriginalSwing");
            animation.Version = 4;
            animation.FrameCount = 31;
            animation.Loop = true;
            var joint = animation.CreateEntry("Paddle");
            joint.SetKeyframe(5, 0, 0);
            joint.SetKeyframe(5, 15, 60);
            joint.SetKeyframe(5, 30, 0);
            string path = Path.Combine(directory, "original-paddle.brres");
            archive.Export(path);
            return path;
        }
    }

    // Two joints, four positions, a two-triangle rigid paddle, exact identity bind
    // matrices and original UVs. This is a tiny editor conformance fixture, not a fighter.
    private const string ColladaSource = @"<?xml version='1.0' encoding='utf-8'?>
<COLLADA xmlns='http://www.collada.org/2005/11/COLLADASchema' version='1.4.1'>
 <asset><unit name='centimeter' meter='0.01'/><up_axis>Y_UP</up_axis></asset>
 <library_effects><effect id='material-fx'><profile_COMMON><technique sid='common'><lambert><diffuse><color>1 1 1 1</color></diffuse></lambert></technique></profile_COMMON></effect></library_effects>
 <library_materials><material id='material'><instance_effect url='#material-fx'/></material></library_materials>
 <library_geometries><geometry id='paddle' name='OriginalPaddleMesh'><mesh>
  <source id='positions'><float_array id='positions-array' count='12'>-3 0 0 3 0 0 3 18 0 -3 18 0</float_array><technique_common><accessor source='#positions-array' count='4' stride='3'><param name='X' type='float'/><param name='Y' type='float'/><param name='Z' type='float'/></accessor></technique_common></source>
  <source id='normals'><float_array id='normals-array' count='12'>0 0 1 0 0 1 0 0 1 0 0 1</float_array><technique_common><accessor source='#normals-array' count='4' stride='3'><param name='X' type='float'/><param name='Y' type='float'/><param name='Z' type='float'/></accessor></technique_common></source>
  <source id='uv'><float_array id='uv-array' count='8'>0 0 1 0 1 1 0 1</float_array><technique_common><accessor source='#uv-array' count='4' stride='2'><param name='S' type='float'/><param name='T' type='float'/></accessor></technique_common></source>
  <vertices id='vertices'><input semantic='POSITION' source='#positions'/></vertices>
  <triangles count='2' material='material'><input semantic='VERTEX' source='#vertices' offset='0'/><input semantic='NORMAL' source='#normals' offset='1'/><input semantic='TEXCOORD' source='#uv' offset='2' set='0'/><p>0 0 0 1 1 1 2 2 2 0 0 0 2 2 2 3 3 3</p></triangles>
 </mesh></geometry></library_geometries>
 <library_controllers><controller id='skin'><skin source='#paddle'>
  <bind_shape_matrix>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</bind_shape_matrix>
  <source id='joints'><Name_array id='joints-array' count='2'>Root Paddle</Name_array><technique_common><accessor source='#joints-array' count='2' stride='1'><param name='JOINT' type='Name'/></accessor></technique_common></source>
  <source id='binds'><float_array id='binds-array' count='32'>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</float_array><technique_common><accessor source='#binds-array' count='2' stride='16'><param name='TRANSFORM' type='float4x4'/></accessor></technique_common></source>
  <source id='weights'><float_array id='weights-array' count='1'>1</float_array><technique_common><accessor source='#weights-array' count='1' stride='1'><param name='WEIGHT' type='float'/></accessor></technique_common></source>
  <joints><input semantic='JOINT' source='#joints'/><input semantic='INV_BIND_MATRIX' source='#binds'/></joints>
  <vertex_weights count='4'><input semantic='JOINT' source='#joints' offset='0'/><input semantic='WEIGHT' source='#weights' offset='1'/><vcount>1 1 1 1</vcount><v>1 0 1 0 1 0 1 0</v></vertex_weights>
 </skin></controller></library_controllers>
 <library_visual_scenes><visual_scene id='Scene'>
  <node id='Root' name='Root' sid='Root' type='JOINT'><matrix>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</matrix><node id='Paddle' name='Paddle' sid='Paddle' type='JOINT'><matrix>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</matrix></node></node>
  <node id='paddle-node'><instance_controller url='#skin'><skeleton>#Root</skeleton><bind_material><technique_common><instance_material symbol='material' target='#material'/></technique_common></bind_material></instance_controller></node>
 </visual_scene></library_visual_scenes><scene><instance_visual_scene url='#Scene'/></scene>
</COLLADA>";
}
