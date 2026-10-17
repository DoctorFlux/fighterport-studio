// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Sources;
using BrawlLib.Internal.IO;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    // The user chooses one file/range, not a dump mount. Discovery and native staging stay separate.
    public sealed class PreviewSourcePicker : Form
    {
        private sealed class Choice
        {
            public string Container;
            public override string ToString()=>Container==null?"Complete supported native file (retains its hierarchy)":"Selected BRRES ["+Container+"] only; opaque siblings are not decoded or rebuilt";
        }
        private readonly ModelForm _host;
        private readonly TextBox _path;
        private readonly ListBox _choices;
        private readonly CheckBox _models;
        private readonly Label _status;
        private readonly FlowLayoutPanel _body;
        private CancellationTokenSource _work;
        private string _inspected;
        private long _inspectedLength,_inspectedTicks;
        internal PreviewSourcePicker(ModelForm host,bool openCharacter=false,string storageRoot=null)
        {
            _host=host;Name="PreviewSourcePicker";Text="Add native source / Open Character";ClientSize=new Size(650,350);MinimumSize=new Size(560,350);
            _body=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(8)};
            _path=new TextBox{Name="previewSourcePath",Width=600};_body.Controls.Add(_path);
            Add("Browse…","previewSourceBrowse",()=>{using(var d=new OpenFileDialog{Filter="Brawl native model/motion|*.pac;*.arc;*.brres;*.chr0"})if(d.ShowDialog(this)==DialogResult.OK)_path.Text=d.FileName;return Task.CompletedTask;});
            _choices=new ListBox{Name="previewSourceChoices",Width=600,Height=90};
            Add("Inspect selected file","previewSourceInspect",async()=>{
                string path=_path.Text;var choices=await Task.Run(()=>Inspect(path,_work.Token));_choices.Items.Clear();_choices.Items.AddRange(choices);_inspected=path;
                if(choices.Length==0)throw new IOException("No supported native file or raw BRRES candidate. Compression/images/other-game readers require their own providers.");
                var info=new FileInfo(path);_inspectedLength=info.Length;_inspectedTicks=info.LastWriteTimeUtc.Ticks;
                if(choices.Length==1)_choices.SelectedIndex=0;
                _status.Text="Select the intended file/container. No native payload decoded until Open. Original paths remain protected.";
            });
            _body.Controls.Add(_choices);
            _models=new CheckBox{Name="previewSourceModels",Text="Open model(s) as well as motions",Checked=openCharacter || host.Editor.TargetModel==null,AutoSize=true};_body.Controls.Add(_models);
            Add("Open selected source","previewSourceOpen",async()=>{
                if(_inspected!=_path.Text || !(_choices.SelectedItem is Choice choice))throw new IOException("Inspect this file and select its native container first.");
                if(_host.Document!=null && _models.Checked)throw new IOException("This viewer has an explicit project target. Open a separate native viewer for another model, or clear Open models to attach motions.");
                string path=_inspected;long size=new FileInfo(path).Length;
                if(size!=_inspectedLength || new FileInfo(path).LastWriteTimeUtc.Ticks!=_inspectedTicks)throw new IOException("Selected file changed after inspection; inspect and deliberately select it again.");
                _host.CheckSourceBudget(size);
                var staged=await Task.Run(()=>{
                    var source=new SourceFolderSession(Path.GetDirectoryName(Path.GetFullPath(path)),storageRoot??host.Workspace.Session?.Project.StorageRoot);
                    return choice.Container==null?source.Stage(path,_work.Token):source.StageContainer(path,choice.Container,_work.Token);
                });
                _work.Token.ThrowIfCancellationRequested();_host.Editor.OpenFile(staged.StagedPath,false,true,false);
                var addedRoot=_host.Editor._openedFiles.Last();
                var addedModels=_models.Checked?PreviewAnimationBrowser.NativeModels(addedRoot).ToArray():new BrawlLib.Modeling.IModel[0];
                if(_models.Checked)_host.RecordCharacterSource(addedRoot);
                if(_models.Checked && addedModels.Length>0)_host.SelectNativeCharacter(addedModels.Length==1?addedModels[0]:null);
                if(_host.Editor.TargetModel!=null)_host.Editor.EditingAll=false;
                _host.Editor.SetAuthoringReadOnly(_host.Editor.AuthoringReadOnly);
                _host.RecordSource(staged);_host.Animations.RefreshClips();_host.FrameNativeModel();
                _status.Text="Source staged separately. "+(addedModels.Length>1?"Choose the intended model in the visible model selector; none was inferred. ":"")+"Use the animation browser, then native Play. Container selection is not a whole-package round trip.";
            });
            _status=new Label{Name="previewSourceStatus",AutoSize=true,MaximumSize=new Size(600,0)};_body.Controls.Add(_status);Controls.Add(_body);
            var cancel=new Button{Name="previewSourceCancel",Text="Cancel operation",Dock=DockStyle.Bottom};cancel.Click+=(s,e)=>_work?.Cancel();Controls.Add(cancel);
            FormClosing+=(s,e)=>{if(_work!=null){_work.Cancel();e.Cancel=true;_status.Text="Cancelling; wait before closing.";}};
        }
        private static object[] Inspect(string path,CancellationToken token)
        {
            // The audit is bounded and does not invoke inherited opaque/redirect parsers.
            try{BrawlContainerAudit.Inspect(path,token).RequirePreservable();return new object[]{new Choice()};}
            catch(IOException)
            {return BrawlContainerSelection.List(path,token).Select(c=>(object)new Choice{Container=c.Path}).ToArray();}
        }
        private void Add(string text,string name,Func<Task> operation)
        {
            var button=new Button{Text=text,Name=name,AutoSize=true};_body.Controls.Add(button);
            button.Click+=async(s,e)=>{if(_work!=null)return;_work=new CancellationTokenSource();_body.Enabled=false;
                try{await Task.Yield();_work.Token.ThrowIfCancellationRequested();await operation();}catch(Exception error){_status.Text="Stopped: "+error.Message;}
                finally{_work.Dispose();_work=null;_body.Enabled=true;}};
        }
    }
}
