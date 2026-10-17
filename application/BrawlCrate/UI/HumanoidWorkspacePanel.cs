// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using BrawlCrate.Authoring.Humanoid;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace BrawlCrate.UI
{
    public sealed class HumanoidWorkspaceContext
    {public string ClipId,TargetId,NativeId,SetId,PartId;}
    public sealed partial class HumanoidWorkspacePanel : UserControl
    {
        private readonly ModelForm _host;
        private readonly PreviewAuthoringSession _session;
        private readonly Func<HumanoidWorkspaceContext> _context;
        private readonly FlowLayoutPanel _flow;
        private readonly ComboBox _side,_limb,_anchor,_polePolicy,_reachPolicy;
        private readonly CheckBox _holdStart,_holdEnd;
        private readonly TextBox _note,_export;
        private readonly Label _status;
        private readonly DataGridView _chains,_landmarks;
        private readonly NumericUpDown _ground,_pelvis,_start,_end,_margin,_x,_y,_z,_stanceSourcePhase,_reachBlend;
        private readonly Label _stanceStatus;
        private readonly ListBox _contacts;
        private readonly Panel _advanced;
        private bool _refreshing;
        private string _lastClip;
        private string _selectedContactChain;
        private double _selectedContactStart;
        private bool _newContactRequested;
        private sealed class ContactChoice
        {
            public ContactInterval Contact;
            public override string ToString()=>Contact.Chain+" "+Contact.Start+".."+Contact.End+" / "+Contact.Anchor+" / "+(Contact.HoldStart?"hold":"fade")+" start, "+(Contact.HoldEnd?"hold":"fade")+" end / "+VectorText(Contact.Goal);
        }
        private HumanoidTrial _trial;
        public HumanoidTrial Trial=>_trial;
        internal HumanoidWorkspacePanel(ModelForm host,PreviewAuthoringSession session,Func<HumanoidWorkspaceContext> context)
        {
            _host=host;_session=session;_context=context;Name="humanoidWorkspace";Height=650;AutoScroll=true;
            _flow=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false};Controls.Add(_flow);
            Label("Humanoid / body fit — native source, FK-only and corrected comparisons. Proposals need your review; contact quality and native serialization are separate.");
            Add("Auto-detect source & target humanoids","humanoidDetect",async()=>{var ctx=Context();var source=Source(ctx);await Work(token=>{_session.Project.Atomic(()=>{_session.Project.DetectHumanoid(source);token.ThrowIfCancellationRequested();_session.Project.DetectHumanoid(ctx.TargetId);});return "Rig/body proposals stored. Review unresolved chains/landmarks; bind/rest, calibration and anchors remain distinct.";});});
            _side=Combo("humanoidProfileSide","Target","Source");_side.SelectedIndexChanged+=(s,e)=>RefreshFromProject();
            Add("Inspect selected rig (read-only source)","humanoidInspect",()=>{var ctx=Context();string character=SelectedCharacter(ctx);var clip=_session.Project.State.Animations.First(a=>a.SourceCharacterId==character);_session.ShowSource(clip.Id,_host);return Task.CompletedTask;});
            _note=new TextBox{Name="humanoidReviewNote",Width=280,MaxLength=2048};_flow.Controls.Add(_note);
            Add("I reviewed this exact rig and mesh body","humanoidReview",()=>{var ctx=Context();_session.Execute(()=>_session.Project.ReviewHumanoid(SelectedCharacter(ctx),_note.Text));return Task.CompletedTask;});
            var details=new Button{Name="humanoidAdvanced",Text="Advanced chains / body landmarks",AutoSize=true};details.Click+=(s,e)=>_advanced.Visible=!_advanced.Visible;_flow.Controls.Add(details);
            _advanced=new Panel{Name="humanoidAdvancedPanel",Width=290,Height=510,Visible=false,AutoScroll=true};var advancedFlow=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false};_advanced.Controls.Add(advancedFlow);_flow.Controls.Add(_advanced);
            _chains=new DataGridView{Name="humanoidChains",Width=280,Height=180,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};_chains.Columns.Add("Role","Chain");_chains.Columns[0].ReadOnly=true;_chains.Columns.Add("Bones","Bone indices");_chains.Columns.Add("Confidence","Proposal");_chains.Columns[2].ReadOnly=true;advancedFlow.Controls.Add(_chains);
            _chains.CellEndEdit+=(s,e)=>Try(()=>{if(_refreshing)return;string role=(string)_chains.Rows[e.RowIndex].Cells[0].Value;var indices=Convert.ToString(_chains.Rows[e.RowIndex].Cells[1].Value).Split(',').Select(n=>int.Parse(n,CultureInfo.InvariantCulture)).ToArray();_session.Execute(()=>_session.Project.UpdateHumanoidChain(SelectedCharacter(Context()),role,indices));RefreshFromProject();});
            _chains.SelectionChanged+=(s,e)=>{if(_refreshing || _chains.SelectedRows.Count!=1)return;Highlight(Convert.ToString(_chains.SelectedRows[0].Cells[1].Value).Split(',')[0]);};
            var roleInput=new TextBox{Name="humanoidChainRole",Width=270};var indexInput=new TextBox{Name="humanoidChainIndices",Width=270};advancedFlow.Controls.Add(new Label{AutoSize=true,Text="Add/resolve a role and its ordered bone indices"});advancedFlow.Controls.Add(roleInput);advancedFlow.Controls.Add(indexInput);var setChain=new Button{Name="humanoidSetChain",Text="Set explicit chain proposal",AutoSize=true};setChain.Click+=(s,e)=>Try(()=>{var indices=indexInput.Text.Split(',').Select(n=>int.Parse(n,CultureInfo.InvariantCulture));_session.Execute(()=>_session.Project.UpdateHumanoidChain(SelectedCharacter(Context()),roleInput.Text,indices));});advancedFlow.Controls.Add(setChain);
            _landmarks=new DataGridView{Name="humanoidLandmarks",Width=280,Height=140,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};_landmarks.Columns.Add("Role","Landmark");_landmarks.Columns[0].ReadOnly=true;_landmarks.Columns.Add("Offset","Local X,Y,Z");_landmarks.Columns.Add("Pole","Pole X,Y,Z");advancedFlow.Controls.Add(_landmarks);
            _landmarks.CellEndEdit+=(s,e)=>Try(()=>{if(_refreshing)return;var setup=_session.Project.HumanoidSetupFor(SelectedCharacter(Context()));var row=_landmarks.Rows[e.RowIndex];var point=setup.Body.Landmarks.Single(l=>l.Role==(string)row.Cells[0].Value);point.Local=ParseVector(Convert.ToString(row.Cells[1].Value));point.Pole=ParseVector(Convert.ToString(row.Cells[2].Value));_session.Execute(()=>_session.Project.UpdateBodyProfile(SelectedCharacter(Context()),setup.Body));});
            _ground=Number(advancedFlow,"humanoidGround",-100000,100000,0);_pelvis=Number(advancedFlow,"humanoidPelvisLimit",0,100000,0);
            foreach(var box in new[]{_ground,_pelvis})box.ValueChanged+=(s,e)=>Try(()=>{if(_refreshing)return;var c=SelectedCharacter(Context());var body=_session.Project.HumanoidSetupFor(c).Body;if(body==null)return;body.GroundY=(double)_ground.Value;body.PelvisLimit=(double)_pelvis.Value;_session.Execute(()=>_session.Project.UpdateBodyProfile(c,body));});
            advancedFlow.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(270,0),Text="Ground Y / maximum pelvis Y adjustment (native units). Geometry proxies are measurements, not exact collision or gameplay hitboxes. Reviewed shared revisions are immutable."});
            advancedFlow.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(270,0),Text="Contact pole policy: World preserves older authored poles. TransportedRest follows the reviewed rest limb as its reach direction changes. Dragging a pole explicitly saves a World override."});
            _polePolicy=new ComboBox{Name="humanoidPolePolicy",Width=270,DropDownStyle=ComboBoxStyle.DropDownList};_polePolicy.Items.AddRange(new object[]{"TransportedRest","World"});_polePolicy.SelectedIndex=0;advancedFlow.Controls.Add(_polePolicy);
            advancedFlow.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(270,0),Text="Reach policy: RestHemisphere keeps the reviewed rest-side branch through folded/straight reach. Nearest preserves older recipes and diagnoses branch ambiguity rather than certifying it."});
            _reachPolicy=new ComboBox{Name="humanoidReachPolicy",Width=270,DropDownStyle=ComboBoxStyle.DropDownList};_reachPolicy.Items.AddRange(new object[]{"RestHemisphere","Nearest"});_reachPolicy.SelectedIndex=0;advancedFlow.Controls.Add(_reachPolicy);
            _reachBlend=Number(advancedFlow,"humanoidReachBlend",0,.1m,0);_reachBlend.Increment=.005m;
            advancedFlow.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(270,0),Text="Reach blend is a fraction of target leg length (0..0.1). Zero preserves older hard reach limits. Full-clip standing enables 0.05 when no blend is saved; inspect contact residuals and regenerate Fit."});
            var saveReachBlend=new Button{Name="humanoidSaveReachBlend",Text="Save continuous reach blend",AutoSize=true};saveReachBlend.Click+=(s,e)=>Try(()=>{var r=Recipe(true,false);r.ReachBlend=(double)_reachBlend.Value;if(_session.Execute(()=>_session.Project.SaveHumanoidRecipe(r)))_status.Text="Saved reach blend "+r.ReachBlend.ToString("0.####",CultureInfo.InvariantCulture)+" of target leg length as one undoable recipe edit. Regenerate Fit and inspect native contact quality.";});advancedFlow.Controls.Add(saveReachBlend);
            var clearPole=new Button{Name="humanoidClearPoleOverride",Text="Use profile pole (clear override)",AutoSize=true};clearPole.Click+=(s,e)=>Try(()=>{var r=Recipe(true);var contact=r.Contacts.SingleOrDefault(c=>c.Chain==_selectedContactChain&&c.Start==_selectedContactStart);if(contact==null)throw new IOException("Select a saved contact first.");contact.UsePoleOverride=false;_session.Execute(()=>_session.Project.SaveHumanoidRecipe(r));});advancedFlow.Controls.Add(clearPole);
            Label("Explicit interval constraints (none by default, including aerials). Pin from a target FK/instance preview, then edit the saved goal. No prop/clearance solver is implied.");
            _limb=Combo("humanoidContactLimb","LeftLeg","RightLeg","LeftArm","RightArm");_anchor=Combo("humanoidContactAnchor","World","Head","Spine");
            _start=Number(_flow,"humanoidContactStart",0,100000,0);_end=Number(_flow,"humanoidContactEnd",0,100000,0);_margin=Number(_flow,"humanoidContactMargin",0,1000,0);
            _holdStart=new CheckBox{Name="humanoidContactHoldStart",Text="Already planted at clip start",AutoSize=true};_holdEnd=new CheckBox{Name="humanoidContactHoldEnd",Text="Still planted at clip end",AutoSize=true};_flow.Controls.Add(_holdStart);_flow.Controls.Add(_holdEnd);
            Label("Unchecked edges blend through the margin. Endpoint holds require the interval to reach the corresponding clip boundary. Hold both only for deliberate continuous contact; aerials have no automatic contacts. This does not fit loop poses or other channels.");
            _x=Number(_flow,"humanoidGoalX",-100000,100000,0);_y=Number(_flow,"humanoidGoalY",-100000,100000,0);_z=Number(_flow,"humanoidGoalZ",-100000,100000,0);
            Add("Read target contact at native playhead","humanoidReadContact",()=>{ReadContact();return Task.CompletedTask;});
            Add("Save contact interval / goal","humanoidSaveContact",()=>{SaveContact();return Task.CompletedTask;});
            Add("Use full clip / hold both contact edges","humanoidFullClipContact",()=>{var ctx=Context();if(!((string)_limb.SelectedItem).EndsWith("Leg"))throw new IOException("Full-clip standing contact applies only to an explicitly chosen leg. Author hand/prop intervals separately.");_start.Value=0;_end.Value=_session.Project.State.Animations.Single(a=>a.Id==ctx.ClipId).Frames-1;_holdStart.Checked=_holdEnd.Checked=true;SaveContact(true);return Task.CompletedTask;});
            Add("New interval (keep displayed values)","humanoidNewContact",()=>{_selectedContactChain=null;_newContactRequested=true;_contacts.ClearSelected();_status.Text="New interval. Choose its timing and goal, then Save; duplicate intervals reject and existing contacts are preserved.";return Task.CompletedTask;});
            _contacts=new ListBox{Name="humanoidContacts",Width=280,Height=90,HorizontalScrollbar=true};_flow.Controls.Add(_contacts);_contacts.SelectedIndexChanged+=(s,e)=>{if(!_refreshing && _contacts.SelectedItem is ContactChoice choice)LoadContact(choice.Contact);};
            _limb.SelectedIndexChanged+=(s,e)=>{if(!_refreshing){_selectedContactChain=null;_contacts.ClearSelected();}};InitializeConstraintHandles();
            Add("Clear recipe contacts (keep prior outputs)","humanoidClearContacts",()=>{var r=Recipe(true);r.Contacts.Clear();_session.Execute(()=>_session.Project.SaveHumanoidRecipe(r));return Task.CompletedTask;});
            _stanceSourcePhase=Number(_flow,"humanoidStanceSourcePhase",0,1023,0);
            Add("Align source phase to native target at playhead","humanoidCalibrateStance",async()=>{var ctx=Context();var doc=_host.Document;if(doc==null||doc.TargetId!=ctx.TargetId||doc.AnimationId!=ctx.NativeId||doc.Session!=_session)throw new IOException("Preview the intended target native reference first.");double targetTime=Math.Max(0,_host.Editor.CurrentFrame-1),sourceTime=(double)_stanceSourcePhase.Value;var r=Recipe(true);await Work(token=>{token.ThrowIfCancellationRequested();var calibrated=_session.Project.CalibrateHumanoidStance(r,sourceTime,targetTime);token.ThrowIfCancellationRequested();_session.Project.SaveHumanoidRecipe(calibrated);return "Saved clip-specific pelvis alignment from immutable native reference phases. Binds, Idle/Fall and review status unchanged; regenerate the trial.";});});
            Add("Clear clip stance calibration","humanoidClearStance",()=>{var r=Recipe(true);r.Stance=null;_session.Execute(()=>_session.Project.SaveHumanoidRecipe(r));return Task.CompletedTask;});
            _stanceStatus=new Label{Name="humanoidStanceStatus",AutoSize=true,MaximumSize=new Size(280,0)};_flow.Controls.Add(_stanceStatus);
            Add("Auto Retarget / Fit to Character (trial)","humanoidFit",async()=>{var ctx=Context();var recipe=Recipe(false);HumanoidTrial trial=null;try{await Work(token=>{trial=_session.Project.PreviewHumanoid(recipe,token);return trial.Report.ToString();});if(trial!=null && !IsDisposed && SameContext(ctx,Context())){_trial?.Dispose();_trial=trial;trial=null;_session.ShowHumanoidTrial(_trial,true,_host);_status.Text=(_trial.Report.ProfilesReviewed?"Reviewed profile trial. ":"UNREVIEWED diagnostic trial — not an approved assignment. ")+_trial.Report;}}finally{trial?.Dispose();}});
            Add("Compare FK-only native output","humanoidPreviewFk",()=>{CheckTrial();_session.ShowHumanoidTrial(_trial,false,_host);return Task.CompletedTask;});
            Add("Compare corrected native output","humanoidPreviewCorrected",()=>{CheckTrial();_session.ShowHumanoidTrial(_trial,true,_host);return Task.CompletedTask;});
            Add("Apply as independent target assignment","humanoidApply",async()=>{var ctx=Context();var recipe=Recipe(false);string instance=null;await Work(token=>instance=ctx.PartId==null?_session.Project.AssignHumanoid(recipe.Id,"Humanoid fit",token):_session.Project.AssignHumanoidPart(ctx.SetId,ctx.PartId,recipe.Id,token));if(instance!=null && !IsDisposed && SameContext(ctx,Context()))_session.ShowInstance(instance,_host);});
            _export=new TextBox{Name="humanoidExportPath",Width=280};_flow.Controls.Add(_export);Add("Export new corrected trial (review status retained)","humanoidExportTrial",()=>{CheckTrial();string path=_export.Text;if(string.IsNullOrWhiteSpace(path))using(var d=new SaveFileDialog{Filter="Target native archive|*.pac;*.brres"}){if(d.ShowDialog(this)!=DialogResult.OK)return Task.CompletedTask;path=d.FileName;}_session.Project.ExportHumanoidTrial(_trial,path);_status.Text="New native trial saved/decoded. Review and contact residuals remain separate from native validity; target game not tested.";return Task.CompletedTask;});
            _status=new Label{Name="humanoidStatus",AutoSize=true,MaximumSize=new Size(280,0)};_flow.Controls.Add(_status);_session.Changed+=ProjectChanged;SizeChanged+=(s,e)=>ResizeChildren();RefreshFromProject();
        }
        private void ResizeChildren(){int width=Math.Max(180,Width-24);foreach(Control c in _flow.Controls){if(c is TextBox||c is ComboBox||c is ListBox||c is Panel)c.Width=width;if(c is Label||c is Button)c.MaximumSize=new Size(width,0);}_status.MaximumSize=new Size(width,0);}
        private void Label(string text)=>_flow.Controls.Add(new Label{Text=text,AutoSize=true,MaximumSize=new Size(280,0)});
        private ComboBox Combo(string name,params string[] values){var c=new ComboBox{Name=name,Width=280,DropDownStyle=ComboBoxStyle.DropDownList};c.Items.AddRange(values);c.SelectedIndex=0;_flow.Controls.Add(c);return c;}
        private static NumericUpDown Number(Control parent,string name,decimal min,decimal max,decimal value){var n=new NumericUpDown{Name=name,AccessibleName=name,Minimum=min,Maximum=max,Value=value,DecimalPlaces=4,Width=150};parent.Controls.Add(new Label{AutoSize=true,Text=name.Replace("humanoid","")});parent.Controls.Add(n);return n;}
        private void Add(string text,string name,Func<Task> action){var b=new Button{Name=name,Text=text,AutoSize=true,MaximumSize=new Size(280,0)};b.Click+=async(s,e)=>{try{await action();}catch(Exception error){_status.Text="Stopped: "+error.Message;}};_flow.Controls.Add(b);}
        private void Try(Action a){try{a();}catch(Exception error){_status.Text="Stopped: "+error.Message;}}
        private async Task Work(Func<CancellationToken,string> operation){string result=await _session.WorkAsync(operation);if(!IsDisposed)_status.Text=result;}
        private HumanoidWorkspaceContext Context(){var c=_context();if(c.ClipId==null || c.TargetId==null || c.NativeId==null)throw new IOException("Select an explicit shared source clip, target and native destination in Library/Animation Set.");return c;}
        private string Source(HumanoidWorkspaceContext ctx)=>_session.Project.State.Animations.Single(a=>a.Id==ctx.ClipId).SourceCharacterId;
        private string SelectedCharacter(HumanoidWorkspaceContext ctx)=>_side.SelectedIndex==1?Source(ctx):ctx.TargetId;
        private HumanoidRecipe Recipe(bool fork,bool saveNew=true)
        {
            var ctx=Context();var state=_session.Project.State;var r=state.HumanoidRecipes.LastOrDefault(n=>n.AnimationId==ctx.ClipId && n.TargetAnimationId==ctx.NativeId && n.AssignmentPartId==ctx.PartId);
            if(r==null){r=_session.Project.CreateHumanoidRecipe(ctx.ClipId,ctx.NativeId);r.AssignmentPartId=ctx.PartId;if(saveNew)_session.Execute(()=>_session.Project.SaveHumanoidRecipe(r));}
            if(fork && state.Instances.Any(i=>i.HumanoidRecipeId==r.Id))r.Id=Guid.NewGuid().ToString("N");return r;
        }
        private void LoadContact(ContactInterval contact)
        {
            bool prior=_refreshing;_refreshing=true;
            try
            {
                _selectedContactChain=contact.Chain;_selectedContactStart=contact.Start;_newContactRequested=false;
                _limb.SelectedItem=contact.Chain;_anchor.SelectedItem=contact.Anchor;_start.Value=(decimal)contact.Start;_end.Value=(decimal)contact.End;_margin.Value=(decimal)contact.Margin;
                _holdStart.Checked=contact.HoldStart;_holdEnd.Checked=contact.HoldEnd;_polePolicy.SelectedItem=contact.PolePolicy;_reachPolicy.SelectedItem=contact.ReachPolicy;
                _x.Value=(decimal)contact.Goal.X;_y.Value=(decimal)contact.Goal.Y;_z.Value=(decimal)contact.Goal.Z;
            }
            finally{_refreshing=prior;}
        }
        private void SaveContact(bool continuousStanding=false)
        {
            var ctx=Context();int last=_session.Project.State.Animations.Single(a=>a.Id==ctx.ClipId).Frames-1;
            if(_holdStart.Checked && _start.Value!=0 || _holdEnd.Checked && _end.Value!=last)throw new IOException("Endpoint holds require interval start 0 / end at the last clip frame. Interior entry/exit phases must retain their contact margins.");
            var recipe=Recipe(true,false);string chain=(string)_limb.SelectedItem;double start=(double)_start.Value;
            if(continuousStanding && recipe.ReachBlend==0)recipe.ReachBlend=.05;
            if(_newContactRequested && recipe.Contacts.Any(c=>c.Chain==chain && c.Start==start))throw new IOException("A saved interval already uses this chain/start. Select it to edit, or choose a different interval; New never overwrites it.");
            // Update the selected interval, including when its start moves. Keep authored
            // pole/orientation/evidence fields; changing endpoint policy must not reset them.
            var contact=_selectedContactChain==null?recipe.Contacts.SingleOrDefault(c=>c.Chain==chain && c.Start==start):recipe.Contacts.SingleOrDefault(c=>c.Chain==_selectedContactChain && c.Start==_selectedContactStart);
            if(contact!=null && contact.UsePoleOverride && contact.PolePolicy!=(string)_polePolicy.SelectedItem)throw new IOException("This contact has an authored pole override. Use 'Use profile pole (clear override)' before changing its pole coordinate policy.");
            if(contact!=null)recipe.Contacts.Remove(contact);else contact=new ContactInterval{Evidence="Explicit user-authored target goal and interval; no inferred gameplay/contact approval."};
            contact.Chain=chain;contact.Anchor=(string)_anchor.SelectedItem;contact.Start=start;contact.End=(double)_end.Value;contact.Margin=(double)_margin.Value;contact.Goal=new HVec((double)_x.Value,(double)_y.Value,(double)_z.Value);contact.HoldStart=_holdStart.Checked;contact.HoldEnd=_holdEnd.Checked;contact.PolePolicy=(string)_polePolicy.SelectedItem;contact.ReachPolicy=(string)_reachPolicy.SelectedItem;recipe.Contacts.Add(contact);
            string priorChain=_selectedContactChain;double priorStart=_selectedContactStart;bool priorNew=_newContactRequested;_selectedContactChain=chain;_selectedContactStart=start;_newContactRequested=false;
            try{if(!_session.Execute(()=>_session.Project.SaveHumanoidRecipe(recipe))){_selectedContactChain=priorChain;_selectedContactStart=priorStart;_newContactRequested=priorNew;RefreshFromProject();return;}}
            catch{_selectedContactChain=priorChain;_selectedContactStart=priorStart;_newContactRequested=priorNew;throw;}
            _status.Text="Contact interval saved as one undoable change. Reach blend "+recipe.ReachBlend.ToString("0.####",CultureInfo.InvariantCulture)+" of target leg length"+(continuousStanding?" (explicit full-clip standing)":"")+". Prior outputs remain independent; regenerate Fit and inspect first/last native frames.";
        }
        private void ReadContact()
        {
            var ctx=Context();var doc=_host.Document;if(doc==null || doc.Session!=_session || doc.TargetId!=ctx.TargetId || doc.AnimationId!=ctx.NativeId || doc.Mode.StartsWith("Source"))throw new IOException("Preview this exact target native destination/instance/FK trial first.");
            var setup=_session.Project.HumanoidSetupFor(ctx.TargetId);var ids=setup.Profile.Chain((string)_limb.SelectedItem).Bones;int bone=ids.Last();var m=doc.Model.BoneCache[bone].Matrix;var local=setup.Body.Landmark((string)_limb.SelectedItem).Local;var point=new HVec(m[0]*local.X+m[4]*local.Y+m[8]*local.Z+m[12],m[1]*local.X+m[5]*local.Y+m[9]*local.Z+m[13],m[2]*local.X+m[6]*local.Y+m[10]*local.Z+m[14]);
            if((string)_anchor.SelectedItem=="World" && ((string)_limb.SelectedItem).EndsWith("Leg"))point.Y=setup.Body.GroundY;
            if((string)_anchor.SelectedItem!="World"){int a=setup.Profile.Chain((string)_anchor.SelectedItem).Bones.Last();var n=doc.Model.BoneCache[a].InverseMatrix;point=new HVec(n[0]*point.X+n[4]*point.Y+n[8]*point.Z+n[12],n[1]*point.X+n[5]*point.Y+n[9]*point.Z+n[13],n[2]*point.X+n[6]*point.Y+n[10]*point.Z+n[14]);}
            _x.Value=(decimal)point.X;_y.Value=(decimal)point.Y;_z.Value=(decimal)point.Z;_status.Text="Contact point read from explicit target at native frame "+(doc.Editor.CurrentFrame-1)+". Set the intended interval and save; no contact is inferred.";
        }
        private void CheckTrial(){var ctx=Context();if(_trial==null || _trial.AnimationId!=ctx.ClipId || _trial.TargetAnimationId!=ctx.NativeId)throw new IOException("Generate a trial for this selected source/target first.");}
        private static bool SameContext(HumanoidWorkspaceContext a,HumanoidWorkspaceContext b)=>a.ClipId==b.ClipId && a.TargetId==b.TargetId && a.NativeId==b.NativeId && a.PartId==b.PartId;
        private void Highlight(string value){if(int.TryParse(value,out int index) && _host.Document?.TargetId==SelectedCharacter(Context()) && index>=0 && index<_host.Document.Model.BoneCache.Length){_host.Editor.SelectedBone=_host.Document.Model.BoneCache[index];_host.Editor.ModelPanel.Invalidate();}}
        private static HVec ParseVector(string text){var a=text.Split(',').Select(n=>double.Parse(n,CultureInfo.InvariantCulture)).ToArray();if(a.Length!=3)throw new IOException("Enter X,Y,Z using invariant decimals.");return new HVec(a[0],a[1],a[2]);}
        private static string VectorText(HVec v)=>string.Join(",",new[]{v.X,v.Y,v.Z}.Select(n=>n.ToString("R",CultureInfo.InvariantCulture)));
        private void ProjectChanged(object sender,EventArgs args){if(!IsDisposed)RefreshFromProject();}
        public void RefreshFromProject()
        {
            if(_refreshing || IsDisposed)return;_refreshing=true;
            try
            {
                var ctx=_context();if(ctx.ClipId==null || ctx.TargetId==null)return;var c=_session.Project.State.Characters.Single(n=>n.Id==SelectedCharacter(ctx));var setup=_session.Project.HumanoidSetupFor(c.Id);_chains.Rows.Clear();_landmarks.Rows.Clear();
                if(setup.Profile!=null)foreach(var chain in setup.Profile.Chains){int row=_chains.Rows.Add(chain.Role,string.Join(",",chain.Bones),chain.Confidence.ToString("0.00",CultureInfo.InvariantCulture));_chains.Rows[row].Cells[1].ReadOnly=setup.Profile.Reviewed;_chains.Rows[row].Cells[0].ToolTipText=chain.Evidence;}
                if(setup.Body!=null){foreach(var point in setup.Body.Landmarks)_landmarks.Rows.Add(point.Role,VectorText(point.Local),VectorText(point.Pole));_landmarks.ReadOnly=setup.Body.Reviewed;_ground.Value=(decimal)setup.Body.GroundY;_pelvis.Value=(decimal)setup.Body.PelvisLimit;_ground.Enabled=_pelvis.Enabled=!setup.Body.Reviewed;}
                string clipKey=ctx.ClipId+":"+ctx.NativeId+":"+ctx.PartId;if(_lastClip!=clipKey){_lastClip=clipKey;_selectedContactChain=null;_newContactRequested=false;_start.Value=0;_end.Value=_session.Project.State.Animations.Single(a=>a.Id==ctx.ClipId).Frames-1;_holdStart.Checked=_holdEnd.Checked=false;_polePolicy.SelectedItem="TransportedRest";_reachPolicy.SelectedItem="RestHemisphere";}
                var recipe=_session.Project.State.HumanoidRecipes.LastOrDefault(r=>r.AnimationId==ctx.ClipId&&r.TargetAnimationId==ctx.NativeId&&r.AssignmentPartId==ctx.PartId);_contacts.Items.Clear();if(recipe!=null)foreach(var contact in recipe.Contacts){var choice=new ContactChoice{Contact=contact};_contacts.Items.Add(choice);if(contact.Chain==_selectedContactChain && contact.Start==_selectedContactStart){_contacts.SelectedItem=choice;LoadContact(contact);}}
                _reachBlend.Value=(decimal)(recipe?.ReachBlend??0);
                if(_stanceStatus!=null){_stanceStatus.Text=recipe?.Stance==null?"No clip-specific stance alignment. Native binds and Idle/Fall are separate.":"Saved source phase "+recipe.Stance.SourceTime+" -> immutable target phase "+recipe.Stance.TargetTime+"; pelvis world offset "+VectorText(recipe.Stance.PelvisOffset);if(recipe?.Stance!=null)_stanceSourcePhase.Value=(decimal)recipe.Stance.SourceTime;}
                _flow.Enabled=!_session.Busy;_status.Text=setup.Profile==null?"Characterize the exact source and target once. No guessed compatibility.":c.Name+": "+(setup.Profile.Reviewed&&setup.Body?.Reviewed==true?"reviewed shared rig/body revisions":"unreviewed proposals")+"; "+setup.Profile.Optional.Count+" optional/helper bones explicitly reported. "+(setup.Body==null?"Resolve chains, then re-run detection to measure the mesh.":"Leg length "+setup.Body.LegLength.ToString("0.000",CultureInfo.InvariantCulture)+"; physical units unknown. Clearance/props not solved.");
                if(_host.Document?.Session==_session)_host.Editor.SetAuthoringReadOnly(_editConstraints.Checked || !_host.Document.Editable);
            }
            catch(Exception error){if(_status!=null)_status.Text="Needs setup: "+error.Message;}
            finally{_refreshing=false;}
        }
        protected override void Dispose(bool disposing){if(disposing){_trial?.Dispose();RemoveConstraintHandles();_session.Changed-=ProjectChanged;}base.Dispose(disposing);}
    }
}
