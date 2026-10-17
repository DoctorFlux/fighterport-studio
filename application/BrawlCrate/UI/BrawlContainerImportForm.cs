// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Sources;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace BrawlCrate.UI
{
    // A selected-source command surface, not a second authoring project/session.
    public sealed class BrawlContainerImportForm : Form
    {
        private readonly CompanionWorkspace _workspace;
        private readonly PreviewAuthoringSession _session;
        private readonly string _character;
        private readonly TextBox _path;
        private readonly ListBox _containers,_motions;
        private readonly Label _status;
        private readonly FlowLayoutPanel _body;
        private readonly Button _cancel;
        private CancellationTokenSource _work;
        private bool _returnAfterWork;
        public BrawlContainerImportForm(CompanionWorkspace workspace):this(workspace,null,null){}
        public BrawlContainerImportForm(PreviewAuthoringSession session,string character):this(new CompanionWorkspace(session.Project.StorageRoot),session,character){}
        private BrawlContainerImportForm(CompanionWorkspace workspace,PreviewAuthoringSession session,string character)
        {
            _workspace=workspace;_session=session;_character=character;Text="Select native companion container";Name="BrawlContainerImportForm";ClientSize=new Size(680,600);MinimumSize=new Size(540,480);
            _body=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(10)};
            _body.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(640,0),Text="Choose a narrow asset file, list its raw ARC directory, then select one BRRES explicitly. Unknown/redirect siblings are not parsed or rebuilt. Full original source hashes/provenance remain pinned."});
            _path=new TextBox{Name="containerSourcePath",Width=620};_body.Controls.Add(_path);
            Add("Browse source…","containerBrowse",()=>{using(var d=new OpenFileDialog{Filter="Raw Brawl archives|*.pac;*.arc;*.brres"})if(d.ShowDialog(this)==DialogResult.OK)_path.Text=d.FileName;return Task.CompletedTask;});
            _containers=new ListBox{Name="containerChoices",Width=620,Height=130};_body.Controls.Add(_containers);
            Add("List raw BRRES candidates","containerList",async()=>{string path=_path.Text;var choices=await Task.Run(()=>BrawlContainerSelection.List(path,_work.Token));_containers.Items.Clear();_containers.Items.AddRange(choices.Cast<object>().ToArray());_status.Text="Directory only; select one candidate. No native payload was decoded.";});
            Add("Stage selected BRRES","containerStage",async()=>{var choice=_containers.SelectedItem as BrawlContainerChoice??throw new IOException("Choose one explicit BRRES.");string path=_path.Text;
                await Task.Run(()=>_workspace.AddSelectedContainer(path,choice.Path,_work.Token));_motions.Items.Clear();_motions.Items.AddRange(_workspace.Resources.Where(r=>r.Kind=="CHR0").Cast<object>().ToArray());
                _status.Text="Selected range staged/validated. Original file remains read-only; unrelated payloads were not passed to a native decoder.";
                if(_session==null)_returnAfterWork=true;
            });
            _motions=new ListBox{Name="containerMotionChoices",Width=620,Height=130};if(session!=null)_body.Controls.Add(_motions);
            if(session!=null)Add("Import selected source-bound clip","containerImportClip",async()=>{var clip=_motions.SelectedItem as CompanionResource??throw new IOException("Choose a motion.");await _session.WorkAsync(token=>_session.Project.AddAnimationFromContainer(_character,clip.SourcePath,clip.SourceContainer,clip.TreePath,token));_status.Text="Immutable clip revision added to this registered source. No target assignment changed.";});
            _status=new Label{Name="containerImportStatus",AutoSize=true,MaximumSize=new Size(640,0)};_body.Controls.Add(_status);
            _cancel=new Button{Name="containerImportCancel",Text="Cancel operation",Dock=DockStyle.Bottom};_cancel.Click+=(s,e)=>{_work?.Cancel();_session?.CancelWork();};Controls.Add(_body);Controls.Add(_cancel);
            FormClosing+=(s,e)=>{if(_work!=null){_work.Cancel();_session?.CancelWork();e.Cancel=true;_status.Text="Cancelling; wait before closing.";}};
            if(_session!=null){var previous=_session.Dispatcher;_session.Dispatcher=this;FormClosed+=(s,e)=>{if(_session.Dispatcher==this)_session.Dispatcher=previous;};}
        }
        private void Add(string label,string name,Func<Task> action)
        {var b=new Button{Name=name,Text=label,AutoSize=true};b.Click+=async(s,e)=>{if(_work!=null)return;_work=new CancellationTokenSource();_body.Enabled=false;try{await action();}catch(Exception error){_status.Text="Stopped: "+error.Message;}finally{_work.Dispose();_work=null;if(!IsDisposed){_body.Enabled=true;if(_returnAfterWork){DialogResult=DialogResult.OK;Close();}}}};_body.Controls.Add(b);}
    }
}
