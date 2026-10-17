// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlLib.Modeling;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    internal partial class ModelForm
    {
        public ModelEditControl Editor=>modelEditControl1;
        public PreviewDocument Document {get;private set;}
        public PreviewWorkspacePane Workspace {get;private set;}
        private SplitContainer _workspaceSplit;
        private ToolStripLabel _identity;
        private void InitializeAuthoringWorkspace()
        {
            AutoScaleMode=AutoScaleMode.Dpi;AutoScaleDimensions=new SizeF(96,96);
            Controls.Remove(Editor);
            _workspaceSplit=new SplitContainer {Width=1200,Dock=DockStyle.Fill,Name="previewWorkspaceSplit",FixedPanel=FixedPanel.Panel1,SplitterDistance=340,Panel1MinSize=240,Panel2MinSize=240,Panel1Collapsed=true};
            Workspace=new PreviewWorkspacePane(this){Dock=DockStyle.Fill};_workspaceSplit.Panel1.Controls.Add(Workspace);_workspaceSplit.Panel2.Controls.Add(Editor);
            var bar=new ToolStrip {Name="previewWorkspaceBar",Dock=DockStyle.Top,GripStyle=ToolStripGripStyle.Hidden};
            var toggle=new ToolStripButton("FighterPort workspace") {Name="previewWorkspaceToggle"};toggle.Click+=(s,e)=>ToggleWorkspace();bar.Items.Add(toggle);
            var reset=new ToolStripButton("Reset layout");reset.Click+=(s,e)=>{_workspaceSplit.Panel1Collapsed=false;_workspaceSplit.SplitterDistance=Math.Min(340,Math.Max(240,Width-340));};bar.Items.Add(reset);
            var compact=new ToolStripButton("Authoring layout");compact.Click+=(s,e)=>Editor.AuthoringLayout(true);bar.Items.Add(compact);
            var native=new ToolStripButton("Native details");native.Click+=(s,e)=>{if(Editor.Width<650){Workspace.Report("Enlarge the window or hide the workspace for native details; the native timeline stays visible.");return;}Editor.AuthoringLayout(false);};bar.Items.Add(native);
            _identity=new ToolStripLabel("Native model — not associated with a character project");bar.Items.Add(_identity);
            Controls.Add(_workspaceSplit);Controls.Add(bar);MinimumSize=new Size(720,520);
            SizeChanged+=(s,e)=>{if(!_workspaceSplit.Panel1Collapsed && Editor.Width<640)Editor.AuthoringLayout(true);};
        }
        public void ToggleWorkspace()=>_workspaceSplit.Panel1Collapsed=!_workspaceSplit.Panel1Collapsed;
        internal void DisplayDocument(PreviewDocument doc,double frame)
        {
            bool first=Document==null || Document.TargetId!=doc.TargetId;DetachDocument();Document=doc;Editor.AuthoringDocument=doc;_models=new System.Collections.Generic.List<IModel>{doc.Model};
            Workspace.Attach(doc.Session,doc.Mode=="Target"||doc.Mode.StartsWith("Reference")?doc.TargetId:null);
            if(!Visible){Size=new Size(1280,850);try{var layout=PreviewLayoutSettings.Read();_workspaceSplit.SplitterDistance=Math.Min(layout.Width,ClientSize.Width-300);_workspaceSplit.Panel1Collapsed=!layout.Visible;}catch(Exception error){Workspace.Report(error.Message);_workspaceSplit.Panel1Collapsed=false;}}
            else {if(first)Editor.AuthoringLayout(true);InitializeBoundDocument(first);}
            _initialFrame=(int)frame+1;
            if(Visible)Editor.SetFrame(_initialFrame);
            UpdateAuthoringIdentity();
        }
        private int _initialFrame=1;
        private bool _initializedAuthoring;
        private void InitializeBoundDocument(bool frameCamera)
        {
            if(Document==null){if(PreviewAuthoringSession.Active!=null)Editor.SetAuthoringReadOnly(true);return;}
            if(!_initializedAuthoring){Editor.AuthoringLayout(true);_initializedAuthoring=true;}
            var doc=Document;Editor.StopAnim();Editor._resetCamera=false;Editor.TargetModel=null;Editor.SelectedCHR0=null;Editor.ModelPanel.ClearAll();Editor._targetModels.Clear();Editor._openedFiles.Clear();
            Editor._openedFiles.Add(doc.Root);Editor.AppendTarget(doc.Model);Editor._resetCamera=false;Editor.TargetModel=doc.Model;Editor.EditingAll=false;
            var a=doc.Session.Project.State.Animations.Single(n=>n.Id==doc.AnimationId);
            Editor.SelectedCHR0=(CHR0Node)BrawlProjectAdapter.At(doc.Root,a.NativePath);Editor.SetFrame(_initialFrame);
            Editor._undoSaves.Clear();Editor._redoSaves.Clear();Editor._currentUndo=null;Editor._saveIndex=0;Editor.UpdateUndoButtons();
            Editor.SetAuthoringReadOnly(!doc.Editable);
            if(frameCamera)
            {
                Editor.ModelPanel.CurrentViewport=(BrawlLib.Internal.Windows.Controls.Model_Panel.ModelPanelViewport)Editor.ModelPanel.First();
                var box=doc.Model.GetBox();var span=box.Max-box.Min;float distance=Math.Max(1,Math.Max(span._x,Math.Max(span._y,span._z))*2);
                if(!float.IsNaN(distance)&&!float.IsInfinity(distance))Editor.ModelPanel.Camera.ZoomExtents((box.Min+box.Max)/2,distance);
            }
            Editor.ModelPanel.Invalidate();
        }
        internal void ReloadDocument()
        {
            var doc=Document;if(doc!=null)doc.Session.ShowInstance(doc.InstanceId,this,doc.Mode,Math.Max(0,Editor.CurrentFrame-1),true);
        }
        internal void MarkDocumentStale(){Editor.SetAuthoringReadOnly(true);_identity.Text="Stale target instance — select and reopen its current revision";}
        internal void UpdateAuthoringIdentity()
        {
            _identity.Text=Document==null?(Editor.AuthoringReadOnly?"Native comparison (read-only) — not associated with a character project":"Native model — not associated with a character project"):Document.Mode+" | project "+Document.Session.Project.State.Id.Substring(0,8)+" | target "+Document.TargetId.Substring(0,8)+" / rig "+Document.TargetFingerprint.Substring(0,8)+" | "+(Document.InstanceId??(Document.Mode.StartsWith("Source")?"immutable source":"unassigned native comparison"))+" r"+Document.InstanceRevision;
        }
        private void DetachDocument()
        {
            if(Document==null)return;
            Editor.StopAnim();Editor.SelectedCHR0=null;Editor._resetCamera=false;Editor.TargetModel=null;Editor.ModelPanel.ClearAll();Editor._targetModels.Clear();Editor._openedFiles.Remove(Document.Root);
            var previous=Document;Document=null;previous.Session.Release(previous);
        }
        private bool CloseAuthoringDocument()
        {
            try
            {
                if(Workspace.Session?.Busy==true || Document?.Session.Busy==true){Workspace.Report("Cancel the active operation and wait before closing this view.");return false;}
                if(Document!=null && !Document.Session.ResolvePending(Document))return false;
                foreach(var session in new[]{Document?.Session,Workspace.Session}.Where(s=>s!=null).Distinct())if(!session.ConfirmProjectSave(this))return false;
                if(Workspace.Session!=null)try{new PreviewLayoutSettings {Visible=!_workspaceSplit.Panel1Collapsed,Width=Math.Max(240,Math.Min(800,_workspaceSplit.SplitterDistance))}.Save();}catch(Exception error){Workspace.Report("Layout preference not saved: "+error.Message);}
                DetachDocument();Workspace.Detach();return true;
            }
            catch(Exception error){Workspace.Report(error.Message);return false;}
        }
        protected override bool ProcessCmdKey(ref Message msg,Keys keyData)
        {
            if(Workspace.ContainsFocus && (keyData==(Keys.Control|Keys.Z)||keyData==(Keys.Control|Keys.Y)))
            {
                // Let the focused native text control process its key. Calling the form's
                // base implementation here can invoke an inherited editor menu shortcut.
                if(Workspace.TextEntryFocused())return false;
                Workspace.ProjectUndo(keyData==(Keys.Control|Keys.Y));return true;
            }
            return base.ProcessCmdKey(ref msg,keyData);
        }
    }
}
