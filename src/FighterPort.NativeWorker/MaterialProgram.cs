using System.Linq;
using BrawlLib.Imaging;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Graphics;
using FighterPort.Core;

internal static partial class ResourceOperations
{
    private static int[] Rgba(GXColorS10 c)=>new[]{(int)c.R,c.G,c.B,c.A};
    private static NativeMaterialProgram ReadMaterialProgram(MDL0MaterialNode material)
    {
        var shader=material.ShaderNode;
        var p=new NativeMaterialProgram{ActiveStages=material.ActiveShaderStages,Colors=new[]{new int[4],Rgba(material.Color0),Rgba(material.Color1),Rgba(material.Color2)},Constants=new[]{Rgba(material.ConstantColor0),Rgba(material.ConstantColor1),Rgba(material.ConstantColor2),Rgba(material.ConstantColor3)},Compare0=(int)material.Comp0,Compare1=(int)material.Comp1,Reference0=material.Ref0,Reference1=material.Ref1,AlphaLogic=(int)material.Logic,Blend=material.EnableBlend,SourceFactor=(int)material.SrcFactor,DestinationFactor=(int)material.DstFactor,BlendLogic=material.EnableBlendLogic,SubtractBlend=material.Subtract,Cull=material.CullMode.ToString(),DepthTest=material.EnableDepthTest,DepthWrite=material.EnableDepthUpdate,DepthFunction=material.DepthFunction.ToString()};
        if(shader==null)return p;
        p.IdentitySwap0=shader.Swap0Red==ColorChannel.Red&&shader.Swap0Green==ColorChannel.Green&&shader.Swap0Blue==ColorChannel.Blue&&shader.Swap0Alpha==ColorChannel.Alpha;
        foreach(MDL0TEVStageNode s in shader.Children)p.Stages.Add(new NativeTevStage{TextureEnabled=s.TextureEnabled,TextureMap=(int)s.TextureMapID,TextureCoord=(int)s.TextureCoordID,Color=new[]{(int)s.ColorSelectionA,(int)s.ColorSelectionB,(int)s.ColorSelectionC,(int)s.ColorSelectionD},Alpha=new[]{(int)s.AlphaSelectionA,(int)s.AlphaSelectionB,(int)s.AlphaSelectionC,(int)s.AlphaSelectionD},ColorOperation=(int)s.ColorOperation,AlphaOperation=(int)s.AlphaOperation,ColorBias=(int)s.ColorBias,AlphaBias=(int)s.AlphaBias,ColorScale=(int)s.ColorScale,AlphaScale=(int)s.AlphaScale,ColorRegister=(int)s.ColorRegister,AlphaRegister=(int)s.AlphaRegister,ColorClamp=s.ColorClamp,AlphaClamp=s.AlphaClamp,ConstantColor=(int)s.ConstantColorSelection,ConstantAlpha=(int)s.ConstantAlphaSelection,TextureSwap=(int)s.TextureSwap,RasterSwap=(int)s.RasterSwap,RasterSource=(int)s._colorChan,NativeColor=s._colorEnv,NativeAlpha=s._alphaEnv,NativeIndirect=s._cmd});
        return p;
    }
}
