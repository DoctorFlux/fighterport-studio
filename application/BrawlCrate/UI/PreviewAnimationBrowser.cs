// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using BrawlLib.Internal.IO;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NativeAnimationKind=BrawlLib.Internal.Windows.Controls.ModelViewer.MainWindowBase.NW4RAnimType;

namespace BrawlCrate.UI
{
    // A view over the host's native documents and authoritative project, never a second project.
    public sealed class PreviewAnimationBrowser : UserControl
    {
        private sealed class Row
        {
            public string Key, Label, Clip, Instance;
            public NW4RAnimationNode Native;
            public AnimationAsset Shared;
            public AnimationAsset Asset;
            public override string ToString()=>Label;
        }
        private readonly ModelForm _host;
        private readonly ComboBox _scope;
        private readonly ComboBox _models;
        private readonly TextBox _search;
        private readonly ListBox _clips;
        private readonly Label _status;
        private readonly Label _details;
        private readonly Button _next;
        private AnimationLibraryStore.LibraryCursor _cursor;
        private PreviewAuthoringSession _session;
        private bool _refreshing;
        internal PreviewAnimationBrowser(ModelForm host)
        {
            _host=host;Name="previewAnimationBrowser";Height=174;MinimumSize=new Size(220,120);
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var bar=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,WrapContents=true};
            _scope=new ComboBox{Name="animationScope",DropDownStyle=ComboBoxStyle.DropDownList,Width=150};
            _scope.Items.AddRange(new object[]{"Opened sources","Selected character","Project","Shared library","Assignments"});_scope.SelectedIndex=0;
            _models=new ComboBox{Name="animationModel",DropDownStyle=ComboBoxStyle.DropDownList,Width=150,DisplayMember="Label",AccessibleName="Choose explicit native model"};
            _models.SelectedIndexChanged+=(s,e)=>{if(_refreshing)return;Try(()=>{var choice=_models.SelectedItem as ModelChoice;if(choice==null)return;_host.SelectNativeCharacter(choice.Model);ShowDetails();});};
            _search=new TextBox{Name="animationSearch",Width=140,AccessibleName="Search animations"};
            bar.Controls.Add(new Label{Text="Animations",AutoSize=true});bar.Controls.Add(_scope);bar.Controls.Add(_search);
            bar.Controls.Add(_models);
            Add(bar,"Preview","animationPreview",Preview);
            Add(bar,"Add Source…","animationAddSource",()=>{using(var picker=new PreviewSourcePicker(_host))picker.ShowDialog(_host);});
            Add(bar,"Open Character…","animationOpenCharacter",()=>{using(var picker=new PreviewSourcePicker(_host,true))picker.ShowDialog(_host);});
            Add(bar,"Apply Animation","animationApply",()=>{
                var row=_clips.SelectedItem as Row??throw new IOException("Select a source-bound project/library clip first.");
                if(row.Native!=null || row.Instance!=null)throw new IOException("Opened native clips are auditions. Register this source for project assignment; no rig compatibility is inferred.");
                var session=_host.Workspace.Session??throw new IOException("Attach the intended target project first.");
                if(row.Shared!=null && !session.Project.State.Animations.Any(a=>a.Id==row.Clip && a.Revision==row.Shared.Revision))if(!session.Execute(()=>session.Project.UseSharedClip(row.Shared.Id,row.Shared.Revision)))return;
                _host.Workspace.SelectClip(row.Clip);_host.Workspace.ApplySelectedAnimation();
            });
            Add(bar,"Save/Export…","animationSaveExport",()=>{if(_host.Workspace.Session!=null)_host.ShowProjectOutput();else using(var dialog=new PreviewNativeExportForm(_host.Editor))dialog.ShowDialog(_host);});
            _next=Add(bar,"Next page","animationNext",()=>RefreshClips(false));_next.Enabled=false;
            _clips=new ListBox{Name="animationClips",Dock=DockStyle.Fill,IntegralHeight=false,HorizontalScrollbar=true};
            _clips.DoubleClick+=(s,e)=>Try(Preview);
            _clips.SelectedIndexChanged+=(s,e)=>ShowDetails();
            _details=new Label{Name="animationDetails",Dock=DockStyle.Fill,AutoSize=true,MaximumSize=new Size(1600,0)};
            _status=new Label{Name="animationBrowserStatus",Dock=DockStyle.Fill,AutoSize=true,MaximumSize=new Size(1600,0),Text="Add a model and its separate motion file. Select a clip, then Preview. Assignments change only through Apply Animation."};
            layout.Controls.Add(bar);layout.Controls.Add(_clips);layout.Controls.Add(_details);layout.Controls.Add(_status);Controls.Add(layout);
            _scope.SelectedIndexChanged+=(s,e)=>RefreshClips();_search.TextChanged+=(s,e)=>RefreshClips();
            SizeChanged+=(s,e)=>_status.MaximumSize=_details.MaximumSize=new Size(Math.Max(200,Width-8),0);
            host.Editor._openedFiles.ListChanged+=(s,e)=>RefreshClips();
            host.Editor.TargetModelChanged+=(s,e)=>RefreshClips();
        }
        private sealed class ModelChoice {public BrawlLib.Modeling.IModel Model;public string Label {get;set;}}
        private Button Add(Control bar,string text,string name,Action action)
        {var button=new Button{Text=text,Name=name,AutoSize=true};button.Click+=(s,e)=>Try(action);bar.Controls.Add(button);return button;}
        private void Try(Action action){try{action();}catch(Exception error){_status.Text="Stopped: "+error.Message;}}
        internal void BindSession()
        {
            var session=_host.Workspace.Session;if(_session!=session){if(_session!=null)_session.Changed-=Changed;_session=session;if(_session!=null)_session.Changed+=Changed;}
            RefreshClips();
        }
        private void Changed(object sender,EventArgs e)=>RefreshClips();
        private void ShowDetails()
        {
            var row=_clips.SelectedItem as Row;if(_details==null)return;
            var document=_host.Document;string target=_host.Workspace.TargetId;
            _details.Text="Target: "+(target==null?(_host.Editor.TargetModel?.ToString()??"none selected"):target.Substring(0,8))+" | "+(document?.Mode??"native audition")+
                (row?.Native==null?"": " | "+row.Native.Name+" / "+row.Native.FrameCount+" frames / loop "+row.Native.Loop)+
                (row?.Asset==null?"": " | CHR0 v"+row.Asset.Version+" / "+row.Asset.Frames+" frames / loop "+row.Asset.Loop+" | source rig "+row.Asset.SourceRigFingerprint.Substring(0,8)+" ["+row.Asset.NativePath+"]");
        }
        public void RefreshClips(bool reset=true)
        {
            if(_refreshing || IsDisposed)return;_refreshing=true;
            try
            {
                string key=(_clips.SelectedItem as Row)?.Key,search=_search.Text;_clips.BeginUpdate();_clips.Items.Clear();_next.Enabled=false;
                _models.Items.Clear();foreach(var model in _host.CharacterChoices()){var native=model as ResourceNode;_models.Items.Add(new ModelChoice{Model=model,Label=native==null?model.ToString():native.Name+" ["+BrawlProjectAdapter.PathOf(native.RootNode,native)+"]"});}
                _models.SelectedItem=_models.Items.Cast<ModelChoice>().FirstOrDefault(m=>ReferenceEquals(m.Model,_host.Editor.TargetModel));_models.Visible=_models.Items.Count>1;_models.Enabled=_host.Document==null;
                var project=_host.Workspace.Session?.Project;string scope=(string)_scope.SelectedItem;
                if(scope=="Shared library")
                {
                    var store=project?.SharedLibrary??new AnimationLibraryStore();
                    if(reset){_cursor?.Dispose();_cursor=store.OpenCursor(search);}if(_cursor==null)_cursor=store.OpenCursor(search);
                    var page=_cursor.Next(32);foreach(var clip in page.Clips)_clips.Items.Add(new Row{Key=clip.Id+":"+clip.Revision,Clip=clip.Id,Shared=clip,Asset=clip,Label=clip.Name+" | shared r"+clip.Revision+" | rig "+clip.SourceRigFingerprint.Substring(0,8)});
                    _next.Enabled=page.HasMore;_status.Text=page.HasMore?"Bounded metadata page; Next page continues the search.":"End of shared catalogue. No native decoding while browsing.";
                }
                else if(scope=="Opened sources")
                {
                    int visited=0;foreach(var root in _host.Editor._openedFiles.Where(n=>n!=null).Distinct().ToArray())
                        foreach(var node in Animations(root,0,()=>++visited))
                            AddRow(new Row{Key="native:"+root.GetHashCode()+":"+BrawlProjectAdapter.PathOf(root,node),Native=node,Label=node.Name+" | "+node.ResourceFileType+" | "+_host.NativeSourceLabel(root)+" / "+root.Name+" ["+BrawlProjectAdapter.PathOf(root,node)+"]"},search);
                    _status.Text=_clips.Items.Count==0?"No matching clips in opened files. Add the separate motion PAC/BRRES; it is not discovered from names.":"Opened native clips. Preview does not assign or certify rig compatibility.";
                }
                else if(project!=null)
                {
                    var state=project.State;
                    if(scope=="Assignments")foreach(var instance in state.Instances.Where(i=>i.TargetCharacterId==_host.Workspace.TargetId))
                        AddRow(new Row{Key=instance.Id,Instance=instance.Id,Asset=project.NativeAnimation(instance.Id),Label=instance.Slot+" | target instance r"+instance.Revision},search);
                    else foreach(var clip in state.Animations.Where(a=>scope!="Selected character" || a.SourceCharacterId==_host.Workspace.TargetId))
                        AddRow(new Row{Key=clip.Id,Clip=clip.Id,Asset=clip,Label=clip.Name+" | source "+clip.SourceCharacterId.Substring(0,8)+" r"+clip.Revision},search);
                    _status.Text="Select Preview for source/target playback. Apply Animation is a separate project command.";
                }
                else _status.Text="Open a character project to browse its registered clips and assignments. Opened sources need no project.";
                _clips.SelectedItem=_clips.Items.Cast<Row>().FirstOrDefault(r=>r.Key==key);
                if(_clips.SelectedIndex<0 && _clips.Items.Count>0)_clips.SelectedIndex=0;
            }
            catch(Exception error){_status.Text="Stopped: "+error.Message;_cursor?.Dispose();_cursor=null;}
            finally{_clips.EndUpdate();_refreshing=false;ShowDetails();}
        }
        private void AddRow(Row row,string search){if(row.Label.IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0)_clips.Items.Add(row);}
        internal static System.Collections.Generic.IEnumerable<BrawlLib.Modeling.IModel> NativeModels(ResourceNode root)
        {int visited=0;return ModelNodes(root,0,()=>++visited).ToArray();}
        private static System.Collections.Generic.IEnumerable<BrawlLib.Modeling.IModel> ModelNodes(ResourceNode node,int depth,Func<int> visit)
        {
            if(depth>16 || visit()>4096)throw new IOException("Model catalogue exceeds 4096 entries / 16 levels; select a smaller container.");
            if(node is BrawlLib.Modeling.IModel model){yield return model;yield break;}
            if(node is ARCNode || node is BRRESNode || node is BRESGroupNode)
                foreach(var child in node.Children)foreach(var item in ModelNodes(child,depth+1,visit))yield return item;
        }
        private static System.Collections.Generic.IEnumerable<NW4RAnimationNode> Animations(ResourceNode root,int depth,Func<int> visit)
        {
            if(depth>16 || visit()>4096)throw new IOException("Animation browser exceeds 4096 entries / 16 levels; select a smaller native container.");
            if(root is NW4RAnimationNode animation){yield return animation;yield break;}
            // Never descend into fighter scripts/opaque entries or model geometry just to list motion.
            if(root is ARCNode || root is BRRESNode || root is BRESGroupNode)
                foreach(var child in root.Children)foreach(var item in Animations(child,depth+1,visit))yield return item;
        }
        private void Preview()
        {
            var row=_clips.SelectedItem as Row??throw new IOException("Select an animation first.");
            var session=_host.Workspace.Session;
            if(row.Native!=null)
            {
                if(_host.Editor.TargetModel==null)throw new IOException("Open a model before previewing this motion; the motion file remains available.");
                if(_host.Document!=null && _host.Editor.TargetAnimation!=row.Native)
                {
                    if(!_host.Document.Session.ResolvePending(_host.Document))return;
                    // ResolvePending may replace/dispose the old native root. Refresh rather than use a stale row.
                    if(!_host.Editor._openedFiles.Contains(row.Native.RootNode)){RefreshClips();throw new IOException("Native document changed; select its current clip again.");}
                    _host.Document.RequestId=Guid.NewGuid().ToString("N");
                    PreviewAuthoringSession.MakeComparison(_host.Document);
                }
                _host.Editor.StopAnim();_host.Editor.TargetAnimType=NativeType(row.Native);_host.Editor.TargetAnimation=row.Native;_host.Editor.SetFrame(1);
                _status.Text="Native audition: "+row.Native.Name+" (rig compatibility unverified). Target identity unchanged; use native Play/timeline.";
            }
            else
            {
                if(session==null)throw new IOException("Attach the intended character project before previewing a shared revision.");
                if(row.Shared!=null && !session.Project.State.Animations.Any(a=>a.Id==row.Clip && a.Revision==row.Shared.Revision))
                    if(!session.Execute(()=>session.Project.UseSharedClip(row.Shared.Id,row.Shared.Revision)))return;
                if(row.Instance!=null)session.ShowInstance(row.Instance,_host,"Target comparison (read-only)");
                else {_host.Workspace.SelectClip(row.Clip);session.ShowSource(row.Clip,_host);}
            }
        }
        private static NativeAnimationKind NativeType(NW4RAnimationNode node)
        {
            if(node is CHR0Node)return NativeAnimationKind.CHR;
            if(node is SRT0Node)return NativeAnimationKind.SRT;
            if(node is PAT0Node)return NativeAnimationKind.PAT;
            if(node is VIS0Node)return NativeAnimationKind.VIS;
            if(node is SHP0Node)return NativeAnimationKind.SHP;
            if(node is CLR0Node)return NativeAnimationKind.CLR;
            if(node is SCN0Node)return NativeAnimationKind.SCN;
            throw new IOException("This native animation type has no inherited preview binding.");
        }
        protected override void Dispose(bool disposing)
        {if(disposing){if(_session!=null)_session.Changed-=Changed;_cursor?.Dispose();}base.Dispose(disposing);}
    }
}
