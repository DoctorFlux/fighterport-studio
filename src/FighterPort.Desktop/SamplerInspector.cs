using FighterPort.Core;
using System.Windows;
using System.Windows.Controls;

namespace FighterPort.Desktop;

public sealed partial class Workbench
{
    private void InspectMaterial(ResourceSelection selection,bool smoke=false)
    {
        if(selection.Handle.DocumentId!=workspace.Data.ActiveModel?.DocumentId)throw new InvalidOperationException("Display this document's model first");
        var material=P.Materials.SingleOrDefault(m=>m.Id==selection.Entry.Id)??throw new InvalidOperationException("Choose a material of the displayed model");
        var panel=new StackPanel{Margin=new(12)};var window=new Window{Owner=this,Title="Material / isolated sampler — "+material.Name,Width=650,Height=500,Content=new ScrollViewer{Content=panel}};
        panel.Children.Add(new TextBlock{Text="Individual sampler inspection changes only this material's preview selection. It does not compose textures or rewrite native material state. The selection is saved with the workspace. Native TEV, alpha-test/cull/depth, nearest/mip filtering and indirect/projected texgen are unsupported.",TextWrapping=TextWrapping.Wrap,Margin=new(5)});
        var slots=new ComboBox{ItemsSource=material.Textures.Select(t=>$"Sampler {t.Slot} / UV{t.UVSet}: {t.Name}").ToArray(),SelectedIndex=material.Textures.FindIndex(t=>t.Slot==material.PreviewSlot),Margin=new(5)};
        var details=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new(5)};panel.Children.Add(slots);panel.Children.Add(details);
        void Describe(){if(slots.SelectedIndex<0)return;var t=material.Textures[slots.SelectedIndex];details.Text=$"Native binding: {t.Texture.DocumentId} / {t.Texture.NodeId}\nPalette: {t.PaletteName} / {t.PaletteId}\nUV{t.UVSet}; texgen {t.Texgen}; projection {t.Projection}\nU {t.WrapU}, V {t.WrapV}; min {t.MinFilter}; mag {t.Filter}; LOD bias {t.LodBias}\n{t.MatrixMode}: {string.Join(", ",t.RestSrt)}\n{t.Status}\nPreview: {(UvEvaluator.DirectCoordinates(t)?"Direct UV coordinates; per-mesh availability checked at draw":"Unsupported texgen; neutral fallback")}\n"+string.Join("\n",material.Limitations);}
        slots.SelectionChanged+=(_,_)=>Describe();Describe();
        var apply=new Button{Content="Inspect selected sampler in viewport",Margin=new(5),Padding=new(5)};panel.Children.Add(apply);
        apply.Click+=(_,_)=>Native(async()=>{if(slots.SelectedIndex<0)return;await SelectSampler(material,material.Textures[slots.SelectedIndex].Slot);ShowProperties(selection);Describe();});
        var reference=new Button{Content="Preview static native color / alpha surface",Margin=new(5)};panel.Children.Add(reference);try{MaterialSurface.Validate(material);}catch(Exception e){reference.IsEnabled=false;reference.ToolTip=e.Message;}reference.Click+=(_,_)=>Native(()=>PreviewMaterialSurface(material));
        Exception? failure=null;
        if(smoke)window.ContentRendered+=async(_,_)=>
        {
            try
            {
                slots.SelectedIndex=1;apply.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                while(operation!=null)await Task.Delay(50);
                if(material.PreviewSlot!=1||!textureCache.ContainsKey(TextureKey(material.Textures[1])))throw new InvalidOperationException("Sampler inspector did not apply/decode selection");
                Capture(System.IO.Path.Combine(root,"artifacts/gui-workspace-workspace-model/uv1-inspection.png"));
            }
            catch(Exception e){failure=e;}finally{window.Close();}
        };
        window.ShowDialog();if(failure!=null)throw failure;
    }
    private async Task SelectSampler(PreviewMaterial material,int slot)
    {
        if(!material.Textures.Any(t=>t.Slot==slot))throw new InvalidOperationException("Missing native sampler");
        material.PreviewSlot=slot;await LoadTextures();Render();SetStatus($"Inspecting {material.Name}, sampler {slot}; native material composition remains unsupported. Save workspace retains this preview selection.");
    }
}
