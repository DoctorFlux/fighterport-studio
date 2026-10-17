// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using BrawlCrate.UI.Model_Previewer.ModelEditControl;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace BrawlCrate.UI
{
    public enum PendingEditDecision {Apply,Discard,Cancel}
    public sealed class PreviewDocument
    {
        public PreviewAuthoringSession Session {get;internal set;}
        public string TargetId {get;internal set;}
        public string TargetFingerprint {get;internal set;}
        public string NativeIdentity {get;internal set;}
        public int InstanceRevision {get;internal set;}
        public string RequestId {get;internal set;}=Guid.NewGuid().ToString("N");
        public string InstanceId {get;internal set;}
        public string AnimationId {get;internal set;}
        public string Mode {get;internal set;}
        public string Path {get;internal set;}
        public ResourceNode Root {get;internal set;}
        public MDL0Node Model {get;internal set;}
        public NativeEditLease Lease {get;internal set;}
        public ModelEditControl Editor {get;internal set;}
        internal ModelForm Owner;
        public bool Editable=>PreviewAuthoringSession.Active==this && Mode=="Target" && !Session.Busy;
    }
    // One live service per project; views share it rather than saving/reloading copies.
    public sealed class PreviewAuthoringSession
    {
        private static readonly ConditionalWeakTable<CharacterProject,PreviewAuthoringSession> Sessions=new ConditionalWeakTable<CharacterProject,PreviewAuthoringSession>();
        private static readonly List<WeakReference> Live=new List<WeakReference>();
        internal static PreviewDocument Active;
        private readonly List<PreviewDocument> _documents=new List<PreviewDocument>();
        private PreviewDocument _applying;
        private CancellationTokenSource _work;
        public bool Busy=>_work!=null;
        internal Control Dispatcher;
        public CharacterProject Project {get;}
        public event EventHandler Changed;
        public Func<PreviewDocument,PendingEditDecision> PendingDecision {get;set;}
        private PreviewAuthoringSession(CharacterProject project)
        {
            Project=project;Live.RemoveAll(w=>!w.IsAlive);Live.Add(new WeakReference(this));project.Changed+=(s,e)=>OnProjectChanged();
        }
        public static PreviewAuthoringSession For(CharacterProject project)=>Sessions.GetValue(project,p=>new PreviewAuthoringSession(p));
        public static PreviewAuthoringSession Open(string path)
        {
            string full=System.IO.Path.GetFullPath(path);var existing=Live.Select(w=>w.Target as PreviewAuthoringSession).FirstOrDefault(s=>s?.Project.FilePath!=null && string.Equals(s.Project.FilePath,full,StringComparison.OrdinalIgnoreCase));
            return existing??For(CharacterProject.Load(full));
        }
        private void OnProjectChanged()
        {
            var owner=(Control)_documents.FirstOrDefault(d=>!d.Owner.IsDisposed)?.Owner??Dispatcher;
            if(owner?.IsHandleCreated==true && owner.InvokeRequired){owner.Invoke(new Action(OnProjectChanged));return;}
            foreach(var doc in _documents.ToArray())
            {
                if(doc.Owner.IsDisposed)continue;
                if(doc!=_applying && doc.Lease!=null && !doc.Root.IsDirty)
                {
                    var instance=Project.State.Instances.SingleOrDefault(i=>i.Id==doc.InstanceId);
                    if(instance!=null && instance.WorkingHash==doc.Lease.BaselineHash){Project.RefreshNativeLease(doc.Lease);doc.InstanceRevision=instance.Revision;}
                    else if(instance!=null)doc.Owner.ReloadDocument();else doc.Owner.MarkDocumentStale();
                }
            }
            Changed?.Invoke(this,EventArgs.Empty);
        }
        public bool Execute(Action action)
        {
            if(Busy)throw new IOException("A bounded authoring operation is running; cancel or wait before changing this project.");
            if(Active?.Session==this && !ResolvePending(Active))return false;
            action();return true;
        }
        public void Apply(PreviewDocument doc)
        {
            if(doc==null || !doc.Editable || doc.Session!=this || doc.Editor.TargetModel!=doc.Model || doc.Root!=doc.Model.RootNode || Project.State.Characters.Single(n=>n.Id==doc.TargetId).RigFingerprint!=doc.TargetFingerprint)throw new IOException("No active explicit editable target/document binding. Activate the intended target first.");
            _applying=doc;
            try{Project.CaptureNativeEdits(doc.Lease,doc.Root,doc.Path);doc.Root.IsDirty=false;Project.RefreshNativeLease(doc.Lease);doc.InstanceRevision=Project.State.Instances.Single(n=>n.Id==doc.InstanceId).Revision;}
            finally{_applying=null;}
            Changed?.Invoke(this,EventArgs.Empty);
        }
        public void CancelWork()=>_work?.Cancel();
        public async Task<string> WorkAsync(Func<CancellationToken,string> operation)
        {
            if(Busy)throw new IOException("Another project operation is already running.");
            if(Active?.Session==this && !ResolvePending(Active))throw new OperationCanceledException("Pending edits retained.");
            var previous=Active?.Session==this?Active:null;_work=new CancellationTokenSource();previous?.Editor.SetAuthoringReadOnly(true);Changed?.Invoke(this,EventArgs.Empty);
            try{return await Task.Run(()=>operation(_work.Token));}
            finally{_work.Dispose();_work=null;if(previous?.Editable==true)previous.Editor.SetAuthoringReadOnly(false);Changed?.Invoke(this,EventArgs.Empty);}
        }
        public bool ResolvePending(PreviewDocument doc)
        {
            if(doc==null || !doc.Root.IsDirty)return true;
            if(!doc.Editable)throw new IOException("Comparison/source document changed unexpectedly; preserve it separately. It cannot be applied as target edits.");
            var choice=PendingDecision!=null?PendingDecision(doc):AskPending(doc.Owner);
            if(choice==PendingEditDecision.Cancel)return false;
            if(choice==PendingEditDecision.Apply){Apply(doc);return true;}
            // Discard replaces this view from the unchanged project revision, never the source.
            ShowInstance(doc.InstanceId,doc.Owner,"Target",0,true);return true;
        }
        private static PendingEditDecision AskPending(IWin32Window owner)
        {
            var result=MessageBox.Show(owner,"Pending native edits: Yes = Apply supported motion edits; No = Discard this working copy; Cancel = keep editing. Unsupported model/material/channel edits must be saved separately in the native editor.","Pending target edits",MessageBoxButtons.YesNoCancel);
            return result==DialogResult.Yes?PendingEditDecision.Apply:result==DialogResult.No?PendingEditDecision.Discard:PendingEditDecision.Cancel;
        }
        public ModelEditControl ShowInstance(string id)=>ShowInstance(id,null).Editor;
        internal PreviewDocument ShowInstance(string id,ModelForm owner,string mode="Target",double frame=0,bool discard=false)
        {
            if(Busy)throw new IOException("Cancel or finish the active project operation before switching its document.");
            PrepareSwitch(owner,mode,discard);
            var instance=Project.State.Instances.Single(i=>i.Id==id);var profile=Project.State.Characters.Single(c=>c.Id==instance.TargetCharacterId);
            var lease=Project.OpenInstance(id);return Show(owner,profile,Project.NativeAnimation(id).Id,id,lease.Path,lease,mode,frame,discard);
        }
        public ModelEditControl ShowSource(string clipId)=>ShowSource(clipId,null).Editor;
        internal PreviewDocument ShowSource(string clipId,ModelForm owner)
        {
            if(Busy)throw new IOException("Cancel or finish the active project operation before switching its document.");
            PrepareSwitch(owner,"Source",false);
            var clip=Project.State.Animations.Single(a=>a.Id==clipId);var profile=Project.State.Characters.Single(c=>c.Id==clip.SourceCharacterId);
            return Show(owner,profile,clipId,null,Project.PreviewSource(clipId),null,"Source (read-only)",0,false);
        }
        internal PreviewDocument PreviewAnchor(string targetId,string role,ModelForm owner)
        {
            var anchor=Project.State.Anchors.Single(a=>a.TargetId==targetId && a.Role==role);var problem=Project.AnchorProblem(anchor);if(problem!=null)throw new IOException(problem);
            return ShowInstance(anchor.InstanceId,owner,"Reference "+role+" (read-only)",anchor.Frame);
        }
        private PreviewDocument Show(ModelForm owner,SourceCharacterProfile profile,string animation,string instance,string path,NativeEditLease lease,string mode,double frame,bool discard)
        {
            try
            {
                if(mode=="Target")
                {
                    foreach(var editor in ModelEditControl.Instances.Where(e=>e!=owner?.Editor && e.AuthoringDocument==null))
                        if(editor._openedFiles.Any(r=>r?.IsDirty==true))throw new IOException("Resolve pending edits in the other native viewer before authoring handoff.");
                    if(Active!=null){var prior=Active;Active=null;prior.Mode="Target comparison (read-only)";prior.Editor.SetAuthoringReadOnly(true);prior.Owner.Workspace.RefreshViews();}
                    foreach(var editor in ModelEditControl.Instances.Where(e=>e!=owner?.Editor))editor.SetAuthoringReadOnly(true);
                }
                var root=NodeFactory.FromFile(null,path);PreviewDocument doc;
                try{doc=new PreviewDocument {Session=this,TargetId=profile.Id,TargetFingerprint=profile.RigFingerprint,NativeIdentity=Project.State.Animations.Single(n=>n.Id==animation).NativeIdentity,InstanceRevision=instance==null?0:Project.State.Instances.Single(n=>n.Id==instance).Revision,AnimationId=animation,InstanceId=instance,Path=path,Lease=lease,Root=root,Model=(MDL0Node)BrawlProjectAdapter.At(root,profile.ModelPath),Mode=mode};}
                catch{root.Dispose();throw;}
                owner=owner??new ModelForm();doc.Owner=owner;doc.Editor=owner.Editor;
                if(mode=="Target")Active=doc;
                owner.DisplayDocument(doc,frame);_documents.Add(doc);
                if(!owner.Visible)owner.Show(MainForm.Instance);Changed?.Invoke(this,EventArgs.Empty);return doc;
            }
            catch {Project.ReleaseNativeLease(lease);if(File.Exists(path))File.Delete(path);throw;}
        }
        internal static void MakeComparison(PreviewDocument doc)
        {
            if(doc==null)return;if(Active==doc)Active=null;doc.Mode=doc.InstanceId==null?"Source comparison (read-only)":"Target comparison (read-only)";doc.Editor.SetAuthoringReadOnly(true);doc.Owner.UpdateAuthoringIdentity();
        }
        internal bool ConfirmProjectSave(IWin32Window owner)
        {
            if(!Project.Dirty)return true;
            var choice=MessageBox.Show(owner,"Save project "+Project.State.Id.Substring(0,8)+" before leaving this view? No leaves disk unchanged; other open views keep their live state.","Character project",MessageBoxButtons.YesNoCancel);
            if(choice==DialogResult.Cancel)return false;if(choice==DialogResult.No)return true;
            string path=Project.FilePath;if(path==null)using(var dialog=new SaveFileDialog {Filter="Character project|*.fpcharacter",DefaultExt="fpcharacter",AddExtension=true}){if(dialog.ShowDialog(owner)!=DialogResult.OK)return false;path=dialog.FileName;}
            Project.Save(path);return true;
        }
        private void PrepareSwitch(ModelForm owner,string mode,bool discard)
        {
            if(owner!=null && owner.Document==null && owner.Editor._openedFiles.Any(r=>r?.IsDirty==true))throw new IOException("Save or discard the plain native document's pending edits before project handoff.");
            if(owner?.Document!=null && !discard && !owner.Document.Session.ResolvePending(owner.Document))throw new OperationCanceledException("Preview switch cancelled.");
            if(owner?.Document!=null && owner.Document.Session!=this && !owner.Document.Session.ConfirmProjectSave(owner))throw new OperationCanceledException("Project handoff cancelled.");
            if(mode=="Target" && Active!=null && Active.Owner!=owner && !Active.Session.ResolvePending(Active))throw new OperationCanceledException("Editable-document handoff cancelled.");
        }
        internal void Release(PreviewDocument doc)
        {
            _documents.Remove(doc);if(Active==doc)Active=null;
            Project.ReleaseNativeLease(doc.Lease);doc.Editor.AuthoringDocument=null;doc.Root.Dispose();if(File.Exists(doc.Path))File.Delete(doc.Path);
        }
    }
}
