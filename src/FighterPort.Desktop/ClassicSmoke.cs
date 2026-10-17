using FighterPort.Core;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FighterPort.Desktop;

public sealed partial class Workbench
{
    private MessageBoxResult? smokeDirtyChoice;
    private async Task ClassicSmoke(string output,string modelPath,string motionPath)
    {
        Directory.CreateDirectory(output);
        try
        {
            await OpenPath(modelPath);await OpenPath(motionPath);var models=await Inventory(workspace.Data.Documents[0]);var motions=await Inventory(workspace.Data.Documents[1]);
            await Activate(models.First(r=>r.Entry.Kind=="MDL0"));await Activate(motions.First(r=>r.Entry.Kind=="CHR0"));await Activate(motions.Single(r=>r.Entry.Name=="OriginalAccessory"));await Activate(motions.Single(r=>r.Entry.Name=="OriginalUvEditable"));
            var active=workspace.Data.ActiveModel;var clip=SelectedClip;string unchanged=StateFingerprint();
            ShowWorkspace(false);resourceSearch.Text="OriginalUvEditable";searchResults.SelectedIndex=0;while(operation!=null)await Task.Delay(25);
            if(workspace.Data.ActiveModel!=active||SelectedClip!=clip||StateFingerprint()!=unchanged)throw new InvalidDataException("Resource search/selection changed active session");resourceSearch.Clear();
            foreach(var size in new[]{(1280,800),(1920,1080)})
            {
                Width=size.Item1;Height=size.Item2;await Task.Delay(180);ShowWorkspace(false);await Reveal(workspace.Data.Uv!.Source);await Task.Delay(100);Capture(Path.Combine(output,$"browser-{size.Item1}.png"));
                ShowWorkspace(true);await Task.Delay(100);if(viewport.ActualWidth<350||viewport.ActualHeight<180)throw new InvalidDataException("Viewport collapsed during resize");Capture(Path.Combine(output,$"model-{size.Item1}.png"));
            }
            Width=1280;Height=800;await Task.Delay(100);
            timeline.Value=4;Keyboard.ClearFocus();
            RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(this),0,System.Windows.Input.Key.Right){RoutedEvent=Keyboard.PreviewKeyDownEvent});
            if(timeline.Value!=5)throw new InvalidDataException("Arrow transport keyboard route failed");
            frameEntry.Focus();frameEntry.SelectAll();frameEntry.SelectedText="7";
            RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,PresentationSource.FromVisual(this),0,System.Windows.Input.Key.Right){RoutedEvent=Keyboard.PreviewKeyDownEvent});
            if(timeline.Value!=5)throw new InvalidDataException("Transport intercepted text entry");Keyboard.ClearFocus();
            // Property-grid transactions, including a real DataGrid editing control and text undo.
            selectedBone=0;timeline.Value=10;ShowBoneFields();var field=boneFields.Fields.Single(f=>f.Name=="Rotate Y °");string before=field.Value;
            boneFields.Grid.SelectedItem=field;boneFields.Grid.ScrollIntoView(field,boneFields.Grid.Columns[1]);boneFields.Grid.CurrentCell=new(field,boneFields.Grid.Columns[1]);boneFields.Grid.BeginEdit();boneFields.Grid.UpdateLayout();
            var editor=FindVisual<TextBox>(boneFields.Grid)??throw new InvalidDataException("Property table did not create a text editor");editor.Focus();editor.SelectAll();editor.SelectedText="NaN";
            boneFields.Grid.CommitEdit(DataGridEditingUnit.Cell,true);if(field.Value!=before)throw new InvalidDataException("Invalid DataGrid text reached resource");boneFields.Grid.CancelEdit();
            if(field.TryCommit("NaN",out _ )||field.Value!=before)throw new InvalidDataException("Nonfinite property edit changed state");
            if(!field.TryCommit("12.5",out string error))throw new InvalidDataException(error);Keyboard.ClearFocus();History(false);if(field.Value!=before)throw new InvalidDataException("Property undo failed");History(true);if(field.Value!="12.5")throw new InvalidDataException("Property redo failed");
            smokeDirtyChoice=MessageBoxResult.Cancel;if(KeepUvEdits())throw new InvalidDataException("Dirty cancel accepted replacement");smokeDirtyChoice=null;
            EditUv();await Task.Delay(100);
            var uvGrid=(Grid)((ScrollViewer)contextEditor.Content).Content;
            IEnumerable<DependencyObject> Descendants(DependencyObject parent){for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var child=VisualTreeHelper.GetChild(parent,i);yield return child;foreach(var descendant in Descendants(child))yield return descendant;}}
            var controls=Descendants(uvGrid).ToArray();
            TextBox Text(string name)=>controls.OfType<TextBox>().Single(t=>t.Name==name);
            Text("UvFrame").Text="10";Text("UvValue").Text="0.375";Text("UvTangent").Text="0.03125";
            void Click(string prefix)=>controls.OfType<Button>().Single(b=>b.Content?.ToString()?.StartsWith(prefix)==true).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            Click("Set key");Click("Undo SRT0");Click("Redo SRT0");
            if(!workspace.Data.Uv!.Tracks[0].Curves.Single(c=>c.Channel==3).Keys.Any(k=>k.Frame==10&&k.Tangent==.03125f))throw new InvalidDataException("Embedded SRT0 editor did not commit/redo");
            await Task.Delay(150);Capture(Path.Combine(output,"srt-editor.png"));
            await workspace.ExportUv(Path.Combine(output,"edited.srt0"));
            workspace.Data.Project=P;string saved=Path.Combine(output,"classic.fpwork");workspace.Save(saved);MarkClean();await OpenPath(saved);
            timeline.Value=10;EditVisibility();await Task.Delay(100);var visGrid=(Grid)((ScrollViewer)contextEditor.Content).Content;var visButtons=Descendants(visGrid).OfType<Button>().ToArray();
            foreach(string prefix in new[]{"Set visibility","Undo visibility","Redo visibility"})visButtons.Single(b=>b.Content?.ToString()?.StartsWith(prefix)==true).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            if(!workspace.Data.Visibility!.Tracks[0].Values[10])throw new InvalidDataException("Embedded VIS0 edit/redo failed");await workspace.ExportVisibility(Path.Combine(output,"edited.vis0"));workspace.Data.Project=P;workspace.Save(saved);MarkClean();
            Capture(Path.Combine(output,"vis-editor.png"));
            // WPF 150% layout scaling stress, without changing the user's desktop DPI setting.
            ((FrameworkElement)Content).LayoutTransform=new ScaleTransform(1.5,1.5);Width=1920;Height=1080;await Task.Delay(200);if(viewport.ActualWidth<300)throw new InvalidDataException("150% layout scaling collapsed viewport");Capture(Path.Combine(output,"model-scale150.png"));((FrameworkElement)Content).LayoutTransform=Transform.Identity;
            foreach(var doc in workspace.Data.Documents)ResourceWorkspace.Verify(doc);
            string ci4=Path.Combine(Path.GetDirectoryName(modelPath)!,"ci4.pac");
            if(File.Exists(ci4))
            {
                await OpenPath(ci4);var source=workspace.Data.Documents.Last();var resources=await Inventory(source);var tex=resources.Single(r=>r.Entry.Kind=="TEX0"&&r.Entry.Name=="Indexed4Ambiguous");var palettes=resources.Where(r=>r.Entry.Kind=="PLT0"&&r.Entry.Name==tex.Entry.Name).ToArray();
                workspace.OverrideTexture(P.Materials[0].Id,0,tex.Handle,palettes[0].Handle.NodeId);await LoadTextures();await PreviewTexture(tex);
                var viewer=OwnedWindows.Cast<Window>().Last();var dock=(DockPanel)viewer.Content;var ci4Controls=(StackPanel)dock.Children.OfType<StackPanel>().Single();var combos=ci4Controls.Children.OfType<ComboBox>().ToArray();combos[0].SelectedIndex=1;combos[1].SelectedItem=combos[1].Items.Cast<string>().Single(s=>s.Contains("["+palettes[1].Handle.NodeId+"]"));
                ci4Controls.Children.OfType<Button>().First().RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));while(operation!=null)await Task.Delay(25);
                var image=(System.Windows.Media.Imaging.BitmapSource)dock.Children.OfType<Image>().Single().Source;if(image.PixelWidth!=16||image.PixelHeight!=8)throw new InvalidDataException("CI4 GUI mip dimensions");var converted=new System.Windows.Media.Imaging.FormatConvertedBitmap(image,PixelFormats.Bgra32,null,0);byte[] pixel=new byte[4];converted.CopyPixels(new Int32Rect(4,2,1,1),pixel,4,0);if(pixel[0]!=0||pixel[1]!=255||pixel[2]!=255||pixel[3]!=255)throw new InvalidDataException("CI4 GUI explicit alternate palette");converted.CopyPixels(new Int32Rect(12,6,1,1),pixel,4,0);if(pixel[3]!=109)throw new InvalidDataException("CI4 GUI partial alpha");
                ci4Controls.Children.OfType<Button>().Last().RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));while(operation!=null)await Task.Delay(25);if(P.Materials[0].Textures[0].PaletteId!=palettes[1].Handle.NodeId)throw new InvalidDataException("CI4 GUI palette override not applied");viewer.Close();workspace.Data.Project=P;workspace.Save(saved);MarkClean();
            }
            string nativeMaterial=Path.Combine(Path.GetDirectoryName(modelPath)!,"material.pac");if(File.Exists(nativeMaterial))await VerifyMaterialSurface(output,nativeMaterial);
            File.WriteAllText(Path.Combine(output,"classic-smoke.txt"),$"PASS original running WPF classic browser/model/context editors; 1280x800 and 1920x1080; actual DPI {VisualTreeHelper.GetDpi(this).PixelsPerInchX}; separate 150% WPF layout scaling stress (not OS-DPI change); shared search/session; routed keyboard transport/text focus; validated finite DataGrid transaction/undo/redo; dirty Cancel branch; embedded SRT0/VIS0 buttons, native export/decode and save/reopen; optional CI4 palette/mip/apply and native three-stage/two-sampler material surface checks when fixtures present. No BrawlCrate/native-game check.");MarkClean();Application.Current.Shutdown(0);
        }
        catch(Exception e){smokeDirtyChoice=MessageBoxResult.No;File.WriteAllText(Path.Combine(output,"classic-smoke.txt"),e.ToString());Application.Current.Shutdown(1);}
    }
    private static T? FindVisual<T>(DependencyObject parent)where T:DependencyObject
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++){var child=VisualTreeHelper.GetChild(parent,i);if(child is T match)return match;if(FindVisual<T>(child) is {} nested)return nested;}return null;
    }
}
