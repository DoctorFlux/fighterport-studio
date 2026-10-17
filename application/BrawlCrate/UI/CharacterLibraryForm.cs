// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using BrawlCrate.NodeWrappers;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Internal.IO;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public sealed class CharacterLibraryForm : Form
    {
        private CharacterProject _project;
        private readonly TreeView _library;
        private readonly ListView _instances;
        private readonly ComboBox _targets;
        private readonly TextBox _name, _slot, _notes, _outputPath, _details;
        private readonly NumericUpDown _startup, _active, _recovery, _damage;
        private readonly Label _status;
        private readonly Button _undo, _redo;
        private NativeEditLease _lease;
        private string _nativeInstanceId;
        public CharacterProject Project => _project;
        public string SelectedAnimationId => _library.SelectedNode?.Tag as string;
        public string SelectedInstanceId => _instances.SelectedItems.Count == 1 ? (string)_instances.SelectedItems[0].Tag : null;

        public static void ShowProject(IWin32Window owner, string path = null)
        {
            try { var form = new CharacterLibraryForm(path == null ? null : CharacterProject.Load(path)); form.Show(owner); }
            catch (Exception e) { MessageBox.Show(owner, e.Message, "Character project", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        public CharacterLibraryForm(CharacterProject project = null)
        {
            _project = project ?? new CharacterProject();
            if(project==null)_project.MigrateToSharedLibrary(); // New authoring uses the app-wide store; v1 opening stays deliberate.
            Text = "FighterPort Studio - Character Library"; Name = "CharacterLibraryForm";
            ClientSize = new Size(1160, 740); MinimumSize = new Size(970, 680); StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 1, RowCount = 8 };
            for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(i == 2 || i == 5 ? SizeType.Percent : SizeType.AutoSize, i == 2 || i == 5 ? 50 : 0));
            layout.Controls.Add(new Label { AutoSize = true, Text = "Shared source-bound animations. Review exact rig roles before FK transfer. Animation slots organize clips; gameplay is deferred." });
            var files = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _name = new TextBox { Name = "libraryName", Width = 150, Text = "Source character" }; files.Controls.Add(_name);
            Button(files,"Register resolution...","libraryRegister",() => {
                using (var dialog = new OpenFileDialog { Filter = "Resolved Brawl sources|*.fpcompanion" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) RegisterSource(dialog.FileName,_name.Text);
            });
            Button(files,"Load project...","libraryLoad",() => {
                if (!DiscardCurrent()) return;
                using (var dialog = new OpenFileDialog { Filter = "Character project|*.fpcharacter" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) { _project = CharacterProject.Load(dialog.FileName); _lease = null; _nativeInstanceId = null; }
            });
            Button(files,"Save project","librarySave",() => {
                if (_project.FilePath != null) SaveProject(_project.FilePath);
                else using (var dialog = new SaveFileDialog { Filter = "Character project|*.fpcharacter", DefaultExt = "fpcharacter", AddExtension = true, FileName = "character.fpcharacter" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) SaveProject(dialog.FileName);
            });
            _undo = Button(files,"Undo project","libraryUndo",() => { EnsureCaptured(); _project.Undo(); RestoreNativeView(); });
            _redo = Button(files,"Redo project","libraryRedo",() => { EnsureCaptured(); _project.Redo(); RestoreNativeView(); });
            layout.Controls.Add(files);
            Button(files,"Shared library...","librarySharedBrowse",()=> {EnsureCaptured();using(var browser=new SharedAnimationLibraryForm(_project))browser.ShowDialog(this);RestoreNativeView();});
            Button(files,"Share / migrate project","libraryMigrate",()=> {EnsureCaptured();_project.MigrateToSharedLibrary();RestoreNativeView();});
            Button(files,"Add clips to source...","libraryAddClips",()=> {
                EnsureCaptured();var source=_project.State.Animations.Single(a=>a.Id==RequireAnimation()).SourceCharacterId;
                using(var dialog=new OpenFileDialog {Filter="Native skeletal clips|*.chr0",Multiselect=true})
                    if(dialog.ShowDialog(this)==DialogResult.OK)foreach(string file in dialog.FileNames)_project.AddAnimation(source,file);
                RestoreNativeView();
            });
            var split = new SplitContainer { Size = new Size(1140,260), Dock = DockStyle.Fill, SplitterDistance = 450 };
            _library = new TreeView { Name = "characterLibrary", Dock = DockStyle.Fill, HideSelection = false };
            _details = new TextBox { Name = "libraryDetails", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = SystemColors.Control, BorderStyle = BorderStyle.None };
            split.Panel1.Controls.Add(_library); split.Panel2.Controls.Add(_details); layout.Controls.Add(split);
            _library.AfterSelect += (s,e) => ShowDetails();
            var assign = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            Button(assign,"Preview library clip","libraryPreview",() => { var id = RequireAnimation(); OpenNative(_project.PreviewSource(id), id); _lease = null; _nativeInstanceId = null; });
            assign.Controls.Add(new Label { Text = "Target", AutoSize = true, Padding = new Padding(0,7,0,0) });
            _targets = new ComboBox { Name = "libraryTarget", DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 }; assign.Controls.Add(_targets);
            _slot = new TextBox { Name = "librarySlot", Width = 130, Text = "DraftSlot" }; assign.Controls.Add(_slot);
            Button(assign,"Assign clip/draft","libraryAssign",() => {
                EnsureCaptured();
                var animation = RequireAnimation(); var draft = _project.State.Moves.LastOrDefault(m => m.AnimationId == animation);
                _project.Assign(animation, ((TargetItem)_targets.SelectedItem)?.Id ?? throw new IOException("Select a target."), _slot.Text, draft?.Id);
                RestoreNativeView();
            });
            Button(assign,"Update slot","librarySetSlot",() => { EnsureCaptured(); _project.SetSlot(RequireInstance(),_slot.Text); RestoreNativeView(); });
            layout.Controls.Add(assign);
            var draftPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            draftPanel.Controls.Add(new Label { Text = "Manual draft: notes / startup / active / recovery / damage", AutoSize = true, Padding = new Padding(0,6,0,0) });
            _notes = new TextBox { Name = "libraryDraftNotes", Width = 165, Text = "Manually authored draft" }; draftPanel.Controls.Add(_notes);
            _startup = Number(draftPanel,"libraryStartup",100000); _active = Number(draftPanel,"libraryActive",100000); _recovery = Number(draftPanel,"libraryRecovery",100000); _damage = Number(draftPanel,"libraryDamage",10000); _damage.DecimalPlaces = 2;
            Button(draftPanel,"Create draft","libraryDraft",() => { EnsureCaptured(); _project.CreateDraft(RequireAnimation(),_notes.Text,(int)_startup.Value,(int)_active.Value,(int)_recovery.Value,(float)_damage.Value); RestoreNativeView(); });
            // Legacy draft fields remain available for compatibility, outside the animation path.
            var legacy=new GroupBox {Text="Legacy gameplay drafts (deferred)",AutoSize=true,Dock=DockStyle.Fill,Visible=false};legacy.Controls.Add(draftPanel);
            layout.Controls.Add(legacy);
            var legacyToggle=Button(files,"Legacy drafts","libraryLegacyDrafts",()=>legacy.Visible=!legacy.Visible);
            Button(assign,"Review rig map...","libraryReviewRig",()=> {
                EnsureCaptured();var source=_project.State.Animations.Single(a=>a.Id==RequireAnimation()).SourceCharacterId;
                using(var review=new RigMapReviewForm(_project,source,((TargetItem)_targets.SelectedItem).Id))review.ShowDialog(this);
                RestoreNativeView();
            });
            Button(assign,"Transfer reviewed FK","libraryTransfer",()=> {
                EnsureCaptured();var animation=_project.State.Animations.Single(a=>a.Id==RequireAnimation());string target=((TargetItem)_targets.SelectedItem).Id;
                var map=_project.State.RigMaps.LastOrDefault(m=>m.SourceCharacterId==animation.SourceCharacterId && m.TargetCharacterId==target)??throw new IOException("Review the exact source/target rig map first.");
                var targetClip=_project.State.Animations.First(a=>a.SourceCharacterId==target);
                _project.AssignRetargeted(animation.Id,targetClip.Id,map.Id,_slot.Text);RestoreNativeView();
            });
            _instances = new ListView { Name = "libraryInstances", Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
            _instances.Columns.Add("Target / stable ID",250); _instances.Columns.Add("Slot",150); _instances.Columns.Add("Source-bound clip / status",340); _instances.Columns.Add("Native revision",160); layout.Controls.Add(_instances);
            _instances.SelectedIndexChanged+=(s,e)=>ShowDetails();
            var edit = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            Button(edit,"Open instance in native editor","libraryEdit",() => { var lease = _project.OpenInstance(RequireInstance()); OpenNative(lease.Path,_project.NativeAnimation(lease.InstanceId).Id); _lease = lease; _nativeInstanceId = lease.InstanceId; });
            Button(edit,"Capture native edits","libraryCapture",() => {
                _project.CaptureNativeEdits(_lease, Program.RootNode, Program.RootPath); Program.RootNode.IsDirty = false;
            });
            _outputPath = new TextBox { Name = "libraryOutputPath", Width = 280, AccessibleName = "New native output path (or leave empty to browse)" }; edit.Controls.Add(_outputPath);
            Button(edit,"Export native copy...","libraryExport",() => {
                EnsureCaptured();
                var instance = _project.State.Instances.Single(i => i.Id == RequireInstance()); var source = _project.State.Characters.Single(c => c.Id == instance.TargetCharacterId);
                if (!string.IsNullOrWhiteSpace(_outputPath.Text)) { _project.ExportInstance(instance.Id,_outputPath.Text); return; }
                using (var dialog = new SaveFileDialog { Filter = "Native container|*" + source.Extension, DefaultExt = source.Extension.Substring(1), AddExtension = true, FileName = "instance" + source.Extension })
                    if (dialog.ShowDialog(this) == DialogResult.OK) _project.ExportInstance(instance.Id,dialog.FileName);
            });
            layout.Controls.Add(edit);
            _status = new Label { Name = "libraryStatus", AutoSize = true, MaximumSize = new Size(1120,0) }; layout.Controls.Add(_status);
            Controls.Add(layout); RefreshProject();
            FormClosing += (s,e) => { if (!DiscardCurrent()) e.Cancel = true; };
        }
        public void RegisterSource(string resolution, string name) { EnsureCaptured(); _project.RegisterSource(resolution,name); RestoreNativeView(); RefreshProject(); }
        public void SaveProject(string path) { EnsureCaptured(); _project.Save(path); RefreshProject(); }
        private void EnsureCaptured() { if (_lease != null && string.Equals(Program.RootPath,_lease.Path,StringComparison.OrdinalIgnoreCase) && Program.RootNode?.IsDirty == true) throw new IOException("Capture the bound native edits before this project command, or save/discard them in the native editor."); }
        private void RestoreNativeView()
        {
            _lease = null;
            if (_nativeInstanceId == null || !_project.State.Instances.Any(i => i.Id == _nativeInstanceId)) return;
            var lease = _project.OpenInstance(_nativeInstanceId); OpenNative(lease.Path,_project.NativeAnimation(lease.InstanceId).Id); _lease = lease;
        }
        private void OpenNative(string path, string animationId)
        {
            if (!Program.Open(path)) throw new IOException("Native editor did not open this working copy.");
            var animation = _project.State.Animations.Single(a=>a.Id==animationId);
            var profile = _project.State.Characters.Single(c=>c.Id==animation.SourceCharacterId);
            var model = (MDL0Node)BrawlProjectAdapter.At(Program.RootNode,profile.ModelPath);
            MainForm.Instance.TargetResource(model);
            (MainForm.Instance.resourceTree.SelectedNode as MDL0Wrapper)?.Preview();
            var editor = ModelEditControl.Instances.Last();
            var nativeRoot = Program.RootNode;
            // ModelForm.OnShown distributes inherited settings and resets the camera.
            // Frame the explicitly bound model after that queued initialization completes.
            editor.BeginInvoke(new Action(() => {
                if (editor.IsDisposed || Program.RootNode != nativeRoot) return;
                editor.SelectedCHR0 = (CHR0Node)BrawlProjectAdapter.At(nativeRoot,animation.NativePath);
                editor.SetFrame(1);
                editor.ModelPanel.CurrentViewport = (BrawlLib.Internal.Windows.Controls.Model_Panel.ModelPanelViewport)editor.ModelPanel.First();
                var box = model.GetBox(); var span = box.Max - box.Min;
                float distance = Math.Max(1,Math.Max(span._x,Math.Max(span._y,span._z))*2);
                if (!float.IsNaN(distance) && !float.IsInfinity(distance)) editor.ModelPanel.Camera.ZoomExtents((box.Min+box.Max)/2,distance);
                editor.ModelPanel.Invalidate();
            }));
        }
        private bool DiscardCurrent()
        {
            try { EnsureCaptured(); }
            catch (IOException error) { _status.Text = error.Message; return false; }
            return !_project.Dirty || MessageBox.Show(this,"Unsaved project commands will be lost. Continue? Native edits must be captured before project save.","Character project",MessageBoxButtons.OKCancel) == DialogResult.OK;
        }
        private string RequireAnimation() => SelectedAnimationId ?? throw new IOException("Select a library animation beneath its source character.");
        private string RequireInstance() => SelectedInstanceId ?? throw new IOException("Select a target instance.");
        private Button Button(Control parent, string text, string name, Action action)
        {
            var b = new Button { Text = text, Name = name, AutoSize = true };
            b.Click += (s,e) => { try { action(); RefreshProject(); } catch (Exception error) { _status.Text = "Stopped: " + error.Message; } }; parent.Controls.Add(b); return b;
        }
        private static NumericUpDown Number(Control parent,string name,int max) { var n = new NumericUpDown { Name = name, Maximum = max, Width = 60 }; parent.Controls.Add(n); return n; }
        private sealed class TargetItem
        {
            public string Id, Name;
            public override string ToString() => Name + " [" + Id.Substring(0,8) + "]";
        }
        public void RefreshProject()
        {
            string clip = SelectedAnimationId, selected = SelectedInstanceId, target = (_targets.SelectedItem as TargetItem)?.Id;
            var state = _project.State; _library.BeginUpdate(); _library.Nodes.Clear(); _targets.Items.Clear();
            foreach (var source in state.Characters)
            {
                var group = _library.Nodes.Add(source.Name + " [" + source.Id.Substring(0,8) + "] - " + source.Bones.Count + " bones");
                foreach (var a in state.Animations.Where(a => a.SourceCharacterId == source.Id))
                {
                    var node = group.Nodes.Add(a.Name+" ["+a.Id.Substring(0,8)+"] r"+a.Revision + (state.Moves.Any(m => m.AnimationId == a.Id) ? " - legacy draft retained" : " - animation only")); node.Tag = a.Id;
                    if (clip == a.Id) _library.SelectedNode = node;
                }
                var item = new TargetItem { Id = source.Id, Name = source.Name }; _targets.Items.Add(item); if (target == item.Id) _targets.SelectedItem = item;
                group.Expand();
            }
            _library.EndUpdate(); if (_targets.SelectedIndex < 0 && _targets.Items.Count > 0) _targets.SelectedIndex = 0;
            _instances.Items.Clear();
            foreach (var i in state.Instances)
            {
                var a = state.Animations.Single(n => n.Id == i.AnimationId); var c = state.Characters.Single(n => n.Id == i.TargetCharacterId);
                var row = new ListViewItem(new[] { c.Name + " [" + c.Id.Substring(0,8) + "]", i.Slot, a.Name + (i.MoveId == null ? " / animation only" : " / manual draft (no native attack)"), i.Revision + " / " + i.WorkingHash.Substring(0,12) }) { Tag = i.Id };
                _instances.Items.Add(row); if (i.Id == selected) row.Selected = true;
            }
            _undo.Enabled = _project.CanUndo; _redo.Enabled = _project.CanRedo;
            var problems = _project.SourceProblems();
            _status.Text = (problems.Count == 0 ? "Sources verified. " : string.Join("; ",problems) + " Preview retained; assignments/capture/export blocked. ") +
                (_project.Dirty ? "Unsaved project. " : "Saved state. ") + "Capture native edits before saving. Native output: enter a new absolute path or leave empty to browse. Undo is session-only (32 commands); load clears history. FK requires review; IK, mirror and fitting remain queued.";
            ShowDetails();
        }
        private void ShowDetails()
        {
            var state = _project.State; var a = state.Animations.SingleOrDefault(n => n.Id == SelectedAnimationId);
            if (a == null) { _details.Text = "Choose a source-bound clip. Register an explicit .fpcompanion resolution to add one native character and its selected motion."; return; }
            var c = state.Characters.Single(n => n.Id == a.SourceCharacterId); var draft = state.Moves.LastOrDefault(m => m.AnimationId == a.Id);
            _details.Text = c.Name + "\r\nSource profile " + c.Id + " / revision " + c.Revision + "\r\nModel " + c.ModelIdentity + "\r\nContainer " + c.ContainerIdentity +
                "\r\nRig " + c.RigFingerprint + "\r\n" + c.Interpretation + "\r\n\r\n" + a.Name + " / CHR0 v" + a.Version + " / " + a.Frames + " frames / loop=" + a.Loop +
                "\r\nAnimation " + a.Id + " / revision " + a.Revision + "\r\n" + (draft == null ? "Animation-only input: no imported attack behavior." : "Manual draft: " + draft.Notes + "\r\nStartup/active/recovery: " + draft.Startup + "/" + draft.Active + "/" + draft.Recovery + "; damage " + draft.Damage + ". No native attack evidence or output.");
            var instance=state.Instances.SingleOrDefault(i=>i.Id==SelectedInstanceId);
            if(instance!=null)_details.Text+="\r\n\r\nSelected target instance: "+instance.Id+"\r\nNative target clip: "+(instance.NativeAnimationId??instance.AnimationId)+"\r\n"+(instance.BakeEvidence??"Same-source assignment; no cross-rig transfer.");
        }
    }
}
