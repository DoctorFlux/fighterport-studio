// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using BrawlCrate.Sources;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public sealed class PreviewNativeExportForm : Form
    {
        internal PreviewNativeExportForm(ModelEditControl editor)
        {
            Text="Save separate native motion";Name="PreviewNativeExportForm";ClientSize=new Size(630,180);
            var body=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(8)};
            body.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(600,0),Text="Exports the selected skeletal motion's native document with a fresh skeletal decode check. Other channels/opaque data are unvalidated. A separate motion PAC does not become a character package or playable fighter."});
            var path=new TextBox{Name="previewNativeOutput",Width=600};body.Controls.Add(path);
            var browse=new Button{Text="Choose new output…",AutoSize=true};browse.Click+=(s,e)=>{using(var dialog=new SaveFileDialog{Filter="Native document|*.pac;*.brres;*.chr0"})if(dialog.ShowDialog(this)==DialogResult.OK)path.Text=dialog.FileName;};body.Controls.Add(browse);
            var status=new Label{Name="previewNativeOutputStatus",AutoSize=true,MaximumSize=new Size(600,0)};
            var save=new Button{Text="Export and validate",Name="previewNativeExport",AutoSize=true};
            save.Click+=(s,e)=>{try{Export(editor,path.Text);status.Text="Saved new native document; decoded skeletal keys/tangents/timing/flags verified. Original unchanged.";}catch(Exception error){status.Text="Stopped: "+error.Message;}};
            body.Controls.Add(save);body.Controls.Add(status);Controls.Add(body);
        }
        public static void Export(ModelEditControl editor,string path)
        {
            if(editor.AuthoringDocument!=null)throw new IOException("Use the bound project's native output command for this document.");
            var clip=editor.TargetAnimation as CHR0Node??throw new IOException("Select a skeletal animation first. Other channel export validation is pending; the previously loaded CHR0 is not the selected output.");
            var root=clip.RootNode;path=Path.GetFullPath(path);SourceWriteGuard.ThrowIfProtected(path);SourceWriteGuard.EnsureNoReparsePoints(path);
            string extension=root is ARCNode?".pac":root is BRRESNode?".brres":root is CHR0Node?".chr0":null;
            if(extension==null || !string.Equals(Path.GetExtension(path),extension,StringComparison.OrdinalIgnoreCase))throw new IOException("Use a new "+extension+" output matching the native document; no flattening or renamed output.");
            if(File.Exists(path) || editor._openedFiles.Any(r=>string.Equals(r?._origPath,path,StringComparison.OrdinalIgnoreCase)))throw new IOException("Choose a new file outside all original/opened inputs.");
            string nativePath=BrawlProjectAdapter.PathOf(root,clip);bool dirty=root.IsDirty;
            var interpolation=clip.Children.OfType<CHR0EntryNode>().ToDictionary(n=>n,n=>n.PreserveInterpolation);
            string temporary=Path.Combine(Path.GetDirectoryName(path),".preview-"+Guid.NewGuid().ToString("N")+extension);
            try
            {
                // Reuse the tested Hermite encoding for edited curves. The ordinary size-driven
                // codec may quantize tangents; this command promises exact decoded keys instead.
                if(dirty)foreach(var track in interpolation.Keys)track.PreserveInterpolation=true;
                if(root.CalculateSize(true)>SourceFolderSession.MaxFileBytes)throw new IOException("Native output exceeds the 64 MiB document budget.");
                SourceWriteGuard.ThrowIfProtected(temporary);root.Export(temporary);
                using(var reopened=NodeFactory.FromFile(null,temporary))CompanionWorkspace.RequireSameMotion(clip,(CHR0Node)BrawlProjectAdapter.At(reopened,nativePath));
                SourceWriteGuard.ThrowIfProtected(path);SourceWriteGuard.EnsureNoReparsePoints(path);File.Move(temporary,path);
            }
            finally{if(File.Exists(temporary))File.Delete(temporary);foreach(var track in interpolation)track.Key.PreserveInterpolation=track.Value;root.IsDirty=dirty;}
        }
    }
}
