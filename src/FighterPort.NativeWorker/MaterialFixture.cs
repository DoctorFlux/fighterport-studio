using System.Drawing;
using System.Linq;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Graphics;
using BrawlLib.Wii.Textures;

internal static partial class ResourceOperations
{
    private static void CreateMaterialFixture(MDL0Node model,BRRESNode bank)
    {
        using(var image=new Bitmap(32,32))
        {
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)image.SetPixel(x,y,Color.FromArgb(y<16?128:255,x<16?128:255,y<16?64:192,255-x*4));
            var texture=bank.CreateResource<TEX0Node>("OriginalModulator");texture.ReplaceRaw(TextureConverter.RGBA8.EncodeTEX0Texture(image,1));texture.Name="OriginalModulator";
        }
        foreach(MDL0MaterialNode material in model._matList)
        {
            var sampler=material.Children.OfType<MDL0MaterialRefNode>().ElementAt(1);sampler.Texture="OriginalModulator";sampler.Coordinates=TexSourceRow.TexCoord0;sampler.UWrapMode=MatWrapMode.Repeat;sampler.VWrapMode=MatWrapMode.Repeat;
            var shader=material.ShaderNode;foreach(var old in shader.Children.ToArray())shader.RemoveChild(old);shader.TextureRef0=true;shader.TextureRef1=true;
            var first=new MDL0TEVStageNode();shader.AddChild(first);first.TextureEnabled=true;first.TextureMapID=TexMapID.TexMap0;first.TextureCoordID=TexCoordID.TexCoord0;first.ColorSelectionD=ColorArg.TextureColor;first.AlphaSelectionD=AlphaArg.TextureAlpha;
            var second=new MDL0TEVStageNode();shader.AddChild(second);second.TextureEnabled=true;second.TextureMapID=TexMapID.TexMap1;second.TextureCoordID=TexCoordID.TexCoord1;second.ColorSelectionB=ColorArg.OutputColor;second.ColorSelectionC=ColorArg.TextureColor;second.AlphaSelectionB=AlphaArg.OutputAlpha;second.AlphaSelectionC=AlphaArg.TextureAlpha;
            var tint=new MDL0TEVStageNode();shader.AddChild(tint);tint.ColorSelectionB=ColorArg.OutputColor;tint.ColorSelectionC=ColorArg.ConstantColorSelection;tint.ConstantColorSelection=TevKColorSel.ConstantColor0_RGB;tint.AlphaSelectionB=AlphaArg.OutputAlpha;tint.AlphaSelectionC=AlphaArg.ConstantAlphaSelection;tint.ConstantAlphaSelection=TevKAlphaSel.ConstantColor0_Alpha;
            material.Color0=material.Color1=material.Color2=new BrawlLib.Imaging.GXColorS10(255,255,255,255);material.ConstantColor0=new BrawlLib.Imaging.GXColorS10(192,255,128,64);material.ConstantColor1=material.ConstantColor2=material.ConstantColor3=new BrawlLib.Imaging.GXColorS10(255,255,255,255);
            material.ActiveShaderStages=3;material.EnableBlend=true;material.EnableBlendLogic=false;material.Subtract=false;material.SrcFactor=BlendFactor.SourceAlpha;material.DstFactor=BlendFactor.InverseSourceAlpha;material.Comp0=AlphaCompare.Always;material.Comp1=AlphaCompare.Always;material.Logic=AlphaOp.And;
        }
    }
}
