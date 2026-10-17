using FighterPort.Core;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FighterPort.Desktop;

public sealed partial class Workbench
{
    private async Task<BitmapSource> BuildMaterialSurface(PreviewMaterial source,int size=128)
    {
        var material=ProjectStore.Clone(source);MaterialSurface.Validate(material);var owner=workspace;var textures=new Dictionary<int,MaterialImage>();
        foreach(int slot in material.Program.Stages.Take(material.Program.ActiveStages).Where(s=>s.TextureEnabled&&(s.Color.Any(v=>v is 8 or 9)||s.Alpha.Contains(4))).Select(s=>s.TextureMap).Distinct())
        {
            var binding=material.Textures.Single(t=>t.Slot==slot);if(binding.Texture.NodeId=="")throw new InvalidDataException("Missing native material texture; resolve its dependency first");textures[slot]=await owner.TexturePixels(binding.Texture,binding.PaletteId,operation?.Token??default);
        }
        var rgba=await Task.Run(()=>MaterialSurface.Render(material,textures,size,size));operation?.Token.ThrowIfCancellationRequested();
        var bgra=new byte[rgba.Length];for(int i=0;i<rgba.Length;i+=4){bgra[i]=rgba[i+2];bgra[i+1]=rgba[i+1];bgra[i+2]=rgba[i];bgra[i+3]=rgba[i+3];}var image=BitmapSource.Create(size,size,96,96,PixelFormats.Bgra32,null,bgra,size*4);image.Freeze();return image;
    }
    private async Task PreviewMaterialSurface(PreviewMaterial material)
    {
        var owner=workspace;var image=await BuildMaterialSurface(material,256);if(workspace!=owner)return;
        ShowWorkspace(false);browserPreview.Content=null;browserPreview.Content=new Border{Background=CheckerBrush(),Child=new Image{Source=image,Width=256,Height=256,Stretch=Stretch.None}};
        previewCaption.Text=material.Name+" — static native color/alpha reference surface; model lighting, depth/cull, animated materials and GPU precision are not evaluated";
        ShowMaterialFields(material);SetStatus("Native color/alpha reference: each sampler filtered before TEV. The 3D viewport remains an isolated-sampler approximation.");
    }
    private async Task VerifyMaterialSurface(string output,string fixture)
    {
        await OpenPath(fixture);var doc=workspace.Data.Documents.Last();var resources=await Inventory(doc);await Activate(resources.Single(r=>r.Entry.Kind=="MDL0"));var material=P.Materials[0];
        var bitmap=await BuildMaterialSurface(material);var image=new Image{Width=128,Height=128,Source=bitmap,Stretch=Stretch.None};image.Measure(new(128,128));image.Arrange(new Rect(0,0,128,128));image.UpdateLayout();var rendered=new RenderTargetBitmap(128,128,96,96,PixelFormats.Pbgra32);rendered.Render(image);
        var points=new[]{(32,32,new byte[]{0,0,48,96}),(96,32,new byte[]{0,12,0,96}),(32,96,new byte[]{42,0,0,192}),(96,96,new byte[]{0,72,192,192})};
        foreach(var (x,y,expected) in points){byte[] actual=new byte[4];rendered.CopyPixels(new Int32Rect(x,y,1,1),actual,4,0);if(actual.Zip(expected).Any(p=>Math.Abs(p.First-p.Second)>1))throw new InvalidDataException("Actual WPF native material surface pixel mismatch");}
        await PreviewMaterialSurface(material);await Task.Delay(150);Capture(Path.Combine(output,"native-material-surface.png"));workspace.Data.Project=P;workspace.Save(Path.Combine(output,"material.fpwork"));MarkClean();
    }
}
