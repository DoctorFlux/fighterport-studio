// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public sealed class PreviewWorkspacePane : UserControl
    {
        private readonly ModelForm _host;
        public PreviewAuthoringSession Session {get;private set;}
        private readonly TabControl _tabs;
        private readonly Label _status,_context,_anchorStatus;
        private readonly Button _cancel;
        private readonly ComboBox _targets,_clips,_instances,_destinations,_groups,_phase,_variant,_idle,_fall,_namedInstance;
        private readonly TreeView _parts;
        private readonly ListBox _catalogue;
        private readonly TextBox _projectPath,_resolution,_output,_search,_savePath,_referenceRole,_partLabel;
        private readonly NumericUpDown _count;
        private readonly Panel _rigHost;
        private readonly Panel _humanoidHost;
        private HumanoidWorkspacePanel _humanoid;
        private AnimationLibraryStore.LibraryCursor _cursor;
        private bool _updating;
        public string TargetId=>(_targets.SelectedItem as Choice)?.Id;
        public string SelectedClipId=>(_clips.SelectedItem as Choice)?.Id;
        public string SelectedPartId=>_parts.SelectedNode?.Tag as string;
        private CharacterAnimationSet Set=>Session?.Project.State.AnimationSets.SingleOrDefault(n=>n.TargetId==TargetId);
        internal PreviewWorkspacePane(ModelForm host)
        {
            _host=host;Name="previewWorkspace";AutoScaleMode=AutoScaleMode.Dpi;
            var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _context=new Label {Name="workspaceContext",AutoSize=true,MaximumSize=new Size(1000,0),Text="Not associated with a character project. Use Setup to attach explicitly."};var header=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false};header.Controls.Add(_context);
            _cancel=new Button {Name="workspaceCancel",Text="Cancel operation",AutoSize=true,Visible=false};_cancel.Click+=(s,e)=>Session?.CancelWork();header.Controls.Add(_cancel);layout.Controls.Add(header);
            _tabs=new TabControl {Name="workspaceTabs",Dock=DockStyle.Fill,Multiline=true};layout.Controls.Add(_tabs);
            var setup=Page("Setup");_projectPath=Input(setup,"workspaceProjectPath","Project file (.fpcharacter)");
            Button(setup,"Attach project","workspaceAttach",()=>{string path=_projectPath.Text;if(string.IsNullOrWhiteSpace(path))using(var d=new OpenFileDialog{Filter="Character project|*.fpcharacter"}){if(d.ShowDialog(this)!=DialogResult.OK)return;path=d.FileName;}if(_host.Document!=null && !_host.Document.Session.ResolvePending(_host.Document))return;Attach(PreviewAuthoringSession.Open(path));});
            _resolution=Input(setup,"workspaceResolution","Saved source resolution (.fpcompanion)");
            Button(setup,"Create shared project from resolution","workspaceCreate",()=>{string path=_resolution.Text;if(string.IsNullOrWhiteSpace(path))using(var d=new OpenFileDialog{Filter="Source resolution|*.fpcompanion"}){if(d.ShowDialog(this)!=DialogResult.OK)return;path=d.FileName;}if(_host.Document!=null && !_host.Document.Session.ResolvePending(_host.Document))return;var p=new CharacterProject();p.MigrateToSharedLibrary();p.RegisterSource(path,"Source character");Attach(PreviewAuthoringSession.For(p));});
            var sourceName=Input(setup,"workspaceSourceName","Character display name for registration (identity is native/hash-bound)");
            Button(setup,"Add character from resolution to this project","workspaceRegisterCharacter",async()=>{try{Require();var session=Session;string path=_resolution.Text,name=sourceName.Text;await session.WorkAsync(token=>session.Project.RegisterSource(path,name,token));}catch(Exception error){Report("Stopped: "+error.Message);}});
            Button(setup,"Share / migrate legacy project","workspaceMigrate",()=>Command(()=>Session.Project.MigrateToSharedLibrary()));
            Label(setup,"Select the intended target explicitly; attaching a project does not infer an association for a plain model.");_targets=Combo(setup,"workspaceTarget");_targets.SelectedIndexChanged+=(s,e)=>{if(!_updating)RefreshViews();};
            Label(setup,"Current asset adapter: Brawl native v1. Semantic pack labels do not certify runtime/game bindings; other adapters remain queued.");
            _instances=Combo(setup,"workspaceInstances");Button(setup,"Inspect / activate target instance","workspaceActivate",()=>{Require();string id=ChoiceId(_instances);Session.ShowInstance(id,_host);});
            Button(setup,"Inspect selected target (new working instance)","workspaceInspect",()=>Command(()=>{var a=Session.Project.State.Animations.First(n=>n.SourceCharacterId==TargetId);string id=Session.Project.Assign(a.Id,TargetId,"Custom preview instance");Session.ShowInstance(id,_host);}));
            Button(setup,"Source paths / companions...","workspaceSources",()=>new CompanionWorkspaceForm(new BrawlCrate.Sources.CompanionWorkspace(Session?.Project.StorageRoot)).Show(_host));
            var library=Page("Library");_search=Input(library,"workspaceSearch","Shared library search");
            _catalogue=new ListBox {Name="workspaceCatalogue",Width=290,Height=130,DisplayMember="Label"};library.Controls.Add(_catalogue);
            Button(library,"Search / refresh shared clips","workspaceLibrarySearch",()=>Browse(true));Button(library,"Next metadata page","workspaceLibraryNext",()=>Browse(false));
            Button(library,"Use selected immutable revision","workspaceLibraryUse",()=>Command(()=>{var a=(AnimationAsset)((Choice)_catalogue.SelectedItem).Value;Session.Project.UseSharedClip(a.Id,a.Revision);}));
            Label(library,"Project clips (source-bound):");_clips=Combo(library,"workspaceClip");_clips.SelectedIndexChanged+=(s,e)=>{if(!_updating)UpdateRigPanel();};
            Button(library,"Preview source (read-only)","workspaceSourcePreview",()=>{Require();Session.ShowSource(SelectedClipId,_host);});
            Button(library,"Add native clips to selected source...","workspaceAddClips",()=>Command(()=>{var a=Session.Project.State.Animations.Single(n=>n.Id==SelectedClipId);using(var d=new OpenFileDialog {Filter="Skeletal native clips|*.chr0",Multiselect=true})if(d.ShowDialog(this)==DialogResult.OK)foreach(string file in d.FileNames)Session.Project.AddAnimation(a.SourceCharacterId,file);}));
            Button(library,"Add clips from a selected native container…","workspaceAddContainerClips",()=>{Require();string character=Session.Project.State.Animations.Single(n=>n.Id==SelectedClipId).SourceCharacterId;using(var dialog=new BrawlContainerImportForm(Session,character))dialog.ShowDialog(_host);});
            var set=Page("Animation Set");Button(set,"Install Brawl semantic pack (unbound)","workspaceInstallPack",()=>Command(()=>Session.Project.InstallAnimationPack(TargetId,AnimationPack.BrawlAuthoring())));
            _parts=new TreeView {Name="workspaceParts",Width=290,Height=180,HideSelection=false};set.Controls.Add(_parts);
            _parts.AfterSelect+=(s,e)=>{if(!_updating){var part=Set?.Parts.SingleOrDefault(n=>n.Id==SelectedPartId);if(part?.InstanceId!=null)Try(()=>Session.ShowInstance(part.InstanceId,_host));ShowPartStatus();}};
            _groups=Combo(set,"workspaceGroup");_groups.SelectedIndexChanged+=(s,e)=>{if(!_updating)FillPhases();};_phase=Combo(set,"workspacePhase");_variant=Combo(set,"workspaceVariant");
            _count=new NumericUpDown {Name="workspacePartCount",Minimum=1,Maximum=64,Value=1,Width=100};set.Controls.Add(_count);
            Button(set,"Add parts / variants (one undo)","workspaceAddParts",()=>Command(()=>Session.Project.AddAnimationParts(Set.Id,ChoiceId(_groups),(int)_count.Value,(string)_phase.SelectedItem,(string)_variant.SelectedItem)));
            _partLabel=Input(set,"workspacePartLabel","Selected part label");_partLabel.MaxLength=120;Button(set,"Rename part","workspaceRenamePart",()=>Command(()=>Session.Project.RenameAnimationPart(Set.Id,SelectedPartId,_partLabel.Text)));
            Button(set,"Move earlier in variant","workspacePartEarlier",()=>Command(()=>Session.Project.MoveAnimationPart(Set.Id,SelectedPartId,-1)));Button(set,"Move later in variant","workspacePartLater",()=>Command(()=>Session.Project.MoveAnimationPart(Set.Id,SelectedPartId,1)));
            Label(set,"Actual native asset destination (not a runtime action):");_destinations=Combo(set,"workspaceDestination");
            _destinations.SelectedIndexChanged+=(s,e)=>{if(!_updating)_humanoid?.RefreshFromProject();};
            Button(set,"Verify / bind selected destination","workspaceBind",()=>Command(()=>Session.Project.BindAnimationPart(Set.Id,SelectedPartId,ChoiceId(_destinations))));
            Button(set,"Assign selected library clip","workspaceAssign",AssignSelected);
            Button(set,"Clear assignment (keep edited instance)","workspaceClearPart",()=>Command(()=>Session.Project.ClearAnimationPart(Set.Id,SelectedPartId,false)));
            Button(set,"Leave destination unbound","workspaceUnbindPart",()=>Command(()=>Session.Project.ClearAnimationPart(Set.Id,SelectedPartId,true)));
            var refs=Page("References");Label(refs,"Choose reference animations separately from their phases. Anchors are project metadata; no fitting is applied.");
            Label(refs,"Idle target instance");_idle=Combo(refs,"workspaceIdleInstance");Label(refs,"Fall target instance");_fall=Combo(refs,"workspaceFallInstance");
            Button(refs,"Choose Idle and Fall motions (one undo)","workspaceChooseAnchors",()=>Command(()=>Session.Project.Atomic(()=>{Session.Project.ChooseAnchor(TargetId,"Idle",ChoiceId(_idle));Session.Project.ChooseAnchor(TargetId,"Fall",ChoiceId(_fall));})));
            foreach(string role in new[]{"Idle","Fall"})
            {
                string r=role;Button(refs,"Preview "+r+" on target","workspacePreview"+r,()=>{Require();Session.PreviewAnchor(TargetId,r,_host);});
                Button(refs,"Set "+r+" phase from native playhead","workspacePhase"+r,()=>Command(()=>{
                    var doc=_host.Document;if(doc?.InstanceId==null || doc.TargetId!=TargetId || doc.Editor.SelectedCHR0!=BrawlProjectAdapter.At(doc.Root,Session.Project.NativeAnimation(doc.InstanceId).NativePath))throw new IOException("Preview this target reference motion before selecting its phase.");
                    Session.Project.SetAnchorPhase(TargetId,r,doc.InstanceId,doc.Editor.CurrentFrame-1);
                }));
            }
            _anchorStatus=new Label {Name="workspaceAnchorStatus",AutoSize=true,MaximumSize=new Size(900,0)};refs.Controls.Add(_anchorStatus);
            _referenceRole=Input(refs,"workspaceReferenceRole","Other reference role (pack-specific or custom)");_referenceRole.MaxLength=64;_namedInstance=Combo(refs,"workspaceNamedReferenceInstance");
            Button(refs,"Choose named reference motion","workspaceChooseNamedReference",()=>Command(()=>Session.Project.ChooseAnchor(TargetId,_referenceRole.Text,ChoiceId(_namedInstance))));
            Button(refs,"Preview named reference on target","workspacePreviewNamedReference",()=>{Require();Session.PreviewAnchor(TargetId,_referenceRole.Text,_host);});
            Button(refs,"Set named phase from native playhead","workspacePhaseNamedReference",()=>Command(()=>{var doc=_host.Document;if(doc?.InstanceId==null || doc.TargetId!=TargetId || doc.Editor.SelectedCHR0!=BrawlProjectAdapter.At(doc.Root,Session.Project.NativeAnimation(doc.InstanceId).NativePath))throw new IOException("Preview this target reference first.");Session.Project.SetAnchorPhase(TargetId,_referenceRole.Text,doc.InstanceId,doc.Editor.CurrentFrame-1);}));
            var rig=Page("Rig / FK");Label(rig,"Choose the source clip in Library and intended target in Setup. Explicit policies are required.");
            _humanoidHost=new Panel{Name="workspaceHumanoidHost",Width=310,Height=650};rig.Controls.Add(_humanoidHost);
            _rigHost=new Panel {Name="workspaceRigHost",Width=310,Height=610};rig.Controls.Add(_rigHost);
            var output=Page("Project / Output");Button(output,"Apply native edits","workspaceApply",()=>{Require();Session.Apply(_host.Document);});
            _savePath=Input(output,"workspaceSavePath","New project path (new or migrated projects)");
            _output=Input(output,"workspaceOutputPath","New native output path");Button(output,"Export native copy","workspaceExport",()=>Command(()=>{var doc=_host.Document;if(doc?.InstanceId==null || doc.Session!=Session || doc.TargetId!=TargetId)throw new IOException("Preview the intended target instance before exporting.");string path=_output.Text;if(string.IsNullOrWhiteSpace(path))using(var d=new SaveFileDialog{Filter="Native container|*.brres;*.pac"}){if(d.ShowDialog(this)!=DialogResult.OK)return;path=d.FileName;}Session.Project.ExportInstance(doc.InstanceId,path);Report("Native output reopened/validated by the adapter. Slot/anchor metadata remains in the project; runtime behavior unverified.");}));
            Button(output,"Save unsupported native edits separately...","workspaceNativeDraft",()=>{var doc=_host.Document;if(doc==null || !doc.Editable)throw new IOException("Activate a target document first.");using(var d=new SaveFileDialog {Filter="Native archive|*.brres;*.pac"})if(d.ShowDialog(this)==DialogResult.OK){if(File.Exists(d.FileName))throw new IOException("Choose a new file.");bool pending=doc.Root.IsDirty;try{doc.Root.Export(d.FileName);}finally{doc.Root.IsDirty=pending;}Report("Separate native draft saved; unsupported edits were not captured into the project.");}});
            Button(output,"Save project","workspaceSave",()=>SaveProject());Button(output,"Undo project structure","workspaceUndo",()=>ProjectUndo(false));Button(output,"Redo project structure","workspaceRedo",()=>ProjectUndo(true));
            Button(output,"Check source / reference diagnostics","workspaceDiagnostics",CheckDiagnostics);
            Label(output,"Native key/curve Undo stays in the native editor. Project Undo restores structural transactions and applied revisions. Text fields keep their own Undo.");
            _status=new Label {Name="workspaceStatus",AutoSize=true,MaximumSize=new Size(1100,0)};layout.Controls.Add(_status);Controls.Add(layout);
            SizeChanged+=(s,e)=>{ResizeAuthoringControls();if(IsHandleCreated)BeginInvoke(new Action(()=>{if(!IsDisposed)ResizeAuthoringControls();}));};
        }
        private void ResizeAuthoringControls()
        {
            int width=Math.Max(180,Width-SystemInformation.VerticalScrollBarWidth-24);_context.MaximumSize=_status.MaximumSize=_anchorStatus.MaximumSize=new Size(width,0);
            foreach(TabPage tab in _tabs.TabPages)foreach(var flow in tab.Controls.OfType<FlowLayoutPanel>())
            {
                flow.Width=width+8;
                foreach(Control control in flow.Controls)
                {
                    if(control is TextBox || control is ComboBox || control is ListBox || control is TreeView || control==_rigHost || control==_humanoidHost)control.Width=width;
                    if(control is Label || control is Button)control.MaximumSize=new Size(width,0);
                }
            }
        }
        private FlowLayoutPanel Page(string name)
        {
            var tab=new TabPage(name){AutoScroll=true};var panel=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(4),Width=310};tab.Controls.Add(panel);_tabs.TabPages.Add(tab);return panel;
        }
        private static void Label(Control parent,string text)=>parent.Controls.Add(new Label {Text=text,AutoSize=true,MaximumSize=new Size(290,0)});
        private static TextBox Input(Control parent,string name,string hint){Label(parent,hint);var box=new TextBox{Name=name,AccessibleName=name,Width=290};parent.Controls.Add(box);return box;}
        private static ComboBox Combo(Control parent,string name){var box=new ComboBox {Name=name,AccessibleName=name,Width=290,DropDownStyle=ComboBoxStyle.DropDownList,DisplayMember="Label"};parent.Controls.Add(box);return box;}
        private void Button(Control parent,string text,string name,Action action){var b=new Button {Text=text,Name=name,AutoSize=true,MaximumSize=new Size(310,0)};b.Click+=(s,e)=>Try(action);parent.Controls.Add(b);}
        private void Try(Action action){try{action();}catch(Exception error){Report("Stopped: "+error.Message);}}
        public void Report(string message)=>_status.Text=message;
        private void Require(){if(Session==null || TargetId==null)throw new IOException("Attach a project and explicitly select its target in Setup.");}
        private void Command(Action action){Require();Session.Execute(action);}
        private async void AssignSelected()
        {
            try
            {
                Require();var session=Session;string target=TargetId,set=Set.Id,part=SelectedPartId,clip=SelectedClipId;var previous=_host.Document;
                var source=session.Project.State.Animations.Single(n=>n.Id==clip);var map=session.Project.State.RigMaps.LastOrDefault(n=>n.SourceCharacterId==source.SourceCharacterId && n.TargetCharacterId==target);
                string id=await session.WorkAsync(token=>session.Project.AssignAnimationPart(set,part,clip,map?.Id,token));
                if(!IsDisposed && Session==session && TargetId==target && SelectedPartId==part && _host.Document==previous && (PreviewAuthoringSession.Active==previous || PreviewAuthoringSession.Active==null))session.ShowInstance(id,_host);
                else if(!IsDisposed)Report("Assignment completed in its original project/part; select it to preview. The current view was not replaced.");
            }
            catch(OperationCanceledException){if(!IsDisposed)Report("Cancelled; no partial assignment was committed.");}
            catch(Exception error){if(!IsDisposed)Report("Stopped: "+error.Message);}
        }
        private async void CheckDiagnostics()
        {
            try
            {
                Require();var session=Session;string result=await session.WorkAsync(token=>{
                    var state=session.Project.State;token.ThrowIfCancellationRequested();var issues=session.Project.SourceProblems().ToList();
                    if(session.Project.SharedLibrary!=null)foreach(var pin in state.LibraryPins){token.ThrowIfCancellationRequested();try{session.Project.SharedLibrary.Read(pin);}catch(IOException error){issues.Add(error.Message);}}
                    foreach(var anchor in state.Anchors){string issue=session.Project.AnchorProblem(anchor);if(issue!=null)issues.Add(anchor.Role+": "+issue);}
                    foreach(var map in state.RigMaps){token.ThrowIfCancellationRequested();try{RigTransfer.Validate(map,state.Characters.Single(n=>n.Id==map.SourceCharacterId),state.Characters.Single(n=>n.Id==map.TargetCharacterId));}catch(IOException error){issues.Add(error.Message);}}
                    return issues.Count==0?"Source hashes, selected library pins and reference/map dependencies verified. "+state.AnimationSets.SelectMany(n=>n.Parts).Count(p=>p.InstanceId!=null && p.Binding==null)+" assigned parts remain unbound; runtime behavior unverified.":string.Join("\r\n",issues);
                });if(!IsDisposed && Session==session)Report(result);
            }catch(OperationCanceledException){if(!IsDisposed)Report("Diagnostics cancelled.");}catch(Exception error){if(!IsDisposed)Report("Stopped: "+error.Message);}
        }
        private static string ChoiceId(ComboBox combo)=>(combo.SelectedItem as Choice)?.Id??throw new IOException("Choose an explicit record.");
        private sealed class Choice{public string Id,Label;public object Value;public override string ToString()=>Label;}
        private static void Fill(ComboBox combo,System.Collections.Generic.IEnumerable<Choice> values,string preferred=null)
        {string id=preferred??(combo.SelectedItem as Choice)?.Id;combo.Items.Clear();combo.Items.AddRange(values.Cast<object>().ToArray());combo.SelectedItem=combo.Items.Cast<Choice>().FirstOrDefault(n=>n.Id==id);if(combo.SelectedIndex<0 && combo.Items.Count>0)combo.SelectedIndex=0;}
        internal void Attach(PreviewAuthoringSession session,string target=null)
        {
            if(Session!=session){if(_host.Document!=null && _host.Document.Session!=session)PreviewAuthoringSession.MakeComparison(_host.Document);Detach();Session=session;session.Changed+=SessionChanged;}session.Dispatcher=_host;_preferredTarget=target??TargetId;RefreshViews();
        }
        private string _preferredTarget;
        public void Detach(){if(Session!=null)Session.Changed-=SessionChanged;_cursor?.Dispose();_cursor=null;Session=null;}
        private void SessionChanged(object sender,EventArgs e){if(!IsDisposed)RefreshViews();}
        public void RefreshViews()
        {
            if(Session==null || _updating)return;_updating=true;
            try
            {
                var state=Session.Project.State;Fill(_targets,state.Characters.Select(n=>new Choice {Id=n.Id,Label=n.Name+" ["+n.Id.Substring(0,8)+"]"}),_preferredTarget);_preferredTarget=null;
                _tabs.Enabled=!Session.Busy;_cancel.Visible=Session.Busy;
                Fill(_clips,state.Animations.Select(n=>new Choice {Id=n.Id,Label=n.Name+" ["+n.SourceCharacterId.Substring(0,8)+"] r"+n.Revision}));
                var instances=state.Instances.Where(n=>n.TargetCharacterId==TargetId).Select(n=>new Choice {Id=n.Id,Label=n.Slot+" ["+n.Id.Substring(0,8)+"]"}).ToArray();Fill(_instances,instances);Fill(_idle,instances);Fill(_fall,instances);
                Fill(_namedInstance,instances);
                Fill(_destinations,state.Animations.Where(n=>n.SourceCharacterId==TargetId).Select(n=>new Choice {Id=n.Id,Label=n.Name+" / "+n.NativeIdentity}));
                string part=SelectedPartId;_parts.Nodes.Clear();var set=Set;
                if(set!=null)foreach(var group in set.Groups){var node=_parts.Nodes.Add(group.Label);foreach(var p in set.Parts.Where(n=>n.GroupId==group.Id)){var child=node.Nodes.Add(p.Label+" / "+p.Variant+" / "+p.Phase+" / "+(p.InstanceId==null?"empty":p.Binding==null?"assigned; unbound":"assigned; verified asset"));child.Tag=p.Id;if(p.Id==part)_parts.SelectedNode=child;}node.Expand();}
                Fill(_groups,set?.Groups.Select(n=>new Choice {Id=n.Id,Label=n.Label})??Enumerable.Empty<Choice>());FillPhases();
                _anchorStatus.Text=string.Join("\r\n",state.Anchors.Where(n=>n.TargetId==TargetId).Select(n=>n.Role+": "+(Session.Project.AnchorProblem(n)??("instance r"+n.InstanceRevision+"; native frame "+n.Frame+" / "+n.Frames+"; loop="+n.Loop))));
                _context.Text="Project "+state.Id.Substring(0,8)+(Session.Project.Dirty?" *":"")+" | Target "+(TargetId??"choose explicitly")+"\r\n"+(_host.Document?.Mode??"Native model not associated")+" | part "+(part??"none");
                _host.UpdateAuthoringIdentity();
            }finally{_updating=false;}
            UpdateRigPanel();ShowPartStatus();
        }
        private void FillPhases()
        {
            var g=Set?.Groups.SingleOrDefault(n=>n.Id==(_groups.SelectedItem as Choice)?.Id);string phase=_phase.SelectedItem as string,variant=_variant.SelectedItem as string;
            _phase.Items.Clear();_variant.Items.Clear();if(g!=null){_phase.Items.AddRange(g.Phases.Cast<object>().ToArray());_variant.Items.AddRange(g.Variants.Cast<object>().ToArray());_phase.SelectedItem=phase;_variant.SelectedItem=variant;if(_phase.SelectedIndex<0)_phase.SelectedIndex=0;if(_variant.SelectedIndex<0)_variant.SelectedIndex=0;}
        }
        private string _rigKey;
        private void UpdateRigPanel()
        {
            if(Session==null || SelectedClipId==null || TargetId==null)return;var source=Session.Project.State.Animations.Single(n=>n.Id==SelectedClipId).SourceCharacterId;string key=Session.Project.State.Id+source+TargetId;
            if(_rigKey==key){_humanoid?.RefreshFromProject();return;}foreach(Control c in _rigHost.Controls)c.Dispose();_rigHost.Controls.Clear();_rigKey=key;
            _humanoid?.Dispose();_humanoidHost.Controls.Clear();_humanoid=new HumanoidWorkspacePanel(_host,Session,()=>new HumanoidWorkspaceContext{ClipId=SelectedClipId,TargetId=TargetId,NativeId=(_destinations.SelectedItem as Choice)?.Id,SetId=Set?.Id,PartId=SelectedPartId}){Dock=DockStyle.Fill};_humanoidHost.Controls.Add(_humanoid);
            var panel=new RigMapReviewPanel(Session.Project,source,TargetId){Dock=DockStyle.Fill,Execute=action=>Command(action)};_rigHost.Controls.Add(panel);
        }
        private void ShowPartStatus()
        {
            var p=Set?.Parts.SingleOrDefault(n=>n.Id==SelectedPartId);if(p!=null){_context.Text="Project "+Session.Project.State.Id.Substring(0,8)+" | Target "+TargetId.Substring(0,8)+"\r\n"+(_host.Document?.Mode??"Native model not associated")+" | "+p.Label+" / "+p.Variant+" / "+p.Phase+" ["+p.Id.Substring(0,8)+"]";_status.Text=p.Binding==null?"Unbound native destination; animation remains saved. No new runtime behavior.":"Verified native asset: "+p.Binding.NativeIdentity+" / "+p.Binding.NativePath+". Runtime binding unverified.";}
        }
        private void Browse(bool reset)
        {
            Require();if(Session.Project.SharedLibrary==null)throw new IOException("Migrate the legacy project to the shared library first.");
            if(reset){_cursor?.Dispose();_cursor=Session.Project.SharedLibrary.OpenCursor(_search.Text);}_cursor=_cursor??Session.Project.SharedLibrary.OpenCursor(_search.Text);
            var page=_cursor.Next(32);_catalogue.Items.Clear();foreach(var a in page.Clips)_catalogue.Items.Add(new Choice {Id=a.Id,Label=a.Name+" ["+a.SourceCharacterId.Substring(0,8)+"] r"+a.Revision,Value=a});Report(page.HasMore?"Bounded metadata page; continue for more.":"End of shared catalogue. No native decoding while browsing.");
        }
        public bool SaveProject()
        {
            if(Session==null)throw new IOException("Attach a project first.");string path=Session.Project.FilePath;
            if(string.IsNullOrEmpty(path)){path=_savePath.Text;if(string.IsNullOrWhiteSpace(path))using(var d=new SaveFileDialog{Filter="Character project|*.fpcharacter",DefaultExt="fpcharacter",AddExtension=true}){if(d.ShowDialog(this)!=DialogResult.OK)return false;path=d.FileName;}if(File.Exists(path))throw new IOException("Save new/migrated projects to a new file; the original is retained.");}
            return Session.Execute(()=>Session.Project.Save(path));
        }
        public void ProjectUndo(bool redo)=>Try(()=>Command(()=>{if(redo)Session.Project.Redo();else Session.Project.Undo();}));
        public bool TextEntryFocused()=>FindText(this);
        private static bool FindText(Control c)=>c is TextBoxBase && c.Focused || c.Controls.Cast<Control>().Any(FindText);
        protected override void Dispose(bool disposing){if(disposing)Detach();base.Dispose(disposing);}
    }
}
