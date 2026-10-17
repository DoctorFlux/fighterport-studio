// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
// Virtual editor handles modify recipes only. They never enter a native bone tree.
using BrawlCrate.Authoring.Humanoid;
using BrawlCrate.Authoring;
using BrawlLib.Internal;
using BrawlLib.Internal.Windows.Controls.Model_Panel;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace BrawlCrate.UI
{
    public sealed partial class HumanoidWorkspacePanel
    {
        private CheckBox _editConstraints;
        private string _dragKind,_dragDocument;
        private HumanoidWorkspaceContext _dragContext;
        private string _dragStateHash;
        private HVec _dragPoint,_dragStart,_poleOrigin;
        private float _dragDepth;
        private bool _handleToggle;
        private sealed class HandlePoint{public string Kind;public HVec World;public float R,G,B;}
        private void InitializeConstraintHandles()
        {
            _editConstraints=new CheckBox{Name="humanoidEditConstraints",Text="Edit contact / pole / pelvis handles",AutoSize=true};_flow.Controls.Add(_editConstraints);
            Label("Select a limb. Green = contact, cyan = pole, yellow = pelvis. Drag in the camera plane; pelvis uses Y only. Release commits one recipe edit; regenerate Fit to bake. No control bones are exported.");
            _editConstraints.CheckedChanged+=(s,e)=>{if(_handleToggle)return;Try(()=>{if(!_session.Execute(()=>_host.Editor.SetAuthoringReadOnly(_editConstraints.Checked || _host.Document?.Editable!=true))){_handleToggle=true;_editConstraints.Checked=false;_handleToggle=false;}_host.Editor.ModelPanel.Invalidate();});};
            _host.Editor.ModelPanel.PostRender+=DrawConstraintHandles;_host.Editor.ModelPanel.MouseDown+=HandleDown;_host.Editor.ModelPanel.MouseMove+=HandleMove;_host.Editor.ModelPanel.MouseUp+=HandleUp;_host.Editor.ModelPanel.MouseCaptureChanged+=HandleCaptureChanged;
        }
        private void RemoveConstraintHandles()
        {
            _dragKind=null;((Control)_host.Editor.ModelPanel).Capture=false;
            if(_host.Document?.Session==_session)_host.Editor.SetAuthoringReadOnly(!_host.Document.Editable);
            _host.Editor.ModelPanel.PostRender-=DrawConstraintHandles;_host.Editor.ModelPanel.MouseDown-=HandleDown;_host.Editor.ModelPanel.MouseMove-=HandleMove;_host.Editor.ModelPanel.MouseUp-=HandleUp;_host.Editor.ModelPanel.MouseCaptureChanged-=HandleCaptureChanged;
        }
        private HumanoidRecipe CurrentRecipe()
        {var c=Context();return _session.Project.State.HumanoidRecipes.LastOrDefault(r=>r.AnimationId==c.ClipId&&r.TargetAnimationId==c.NativeId&&r.AssignmentPartId==c.PartId);}
        private HVec NativePoint(int bone,HVec local)
        {var m=_host.Document.Model.BoneCache[bone].Matrix;return new HVec(m[0]*local.X+m[4]*local.Y+m[8]*local.Z+m[12],m[1]*local.X+m[5]*local.Y+m[9]*local.Z+m[13],m[2]*local.X+m[6]*local.Y+m[10]*local.Z+m[14]);}
        private List<HandlePoint> Handles()
        {
            var c=Context();if(!_editConstraints.Checked || _session.Busy || _host.Document?.Session!=_session || _host.Document.TargetId!=c.TargetId || _host.Document.AnimationId!=c.NativeId || _host.Document.Mode.StartsWith("Source"))return new List<HandlePoint>();
            var setup=_session.Project.HumanoidSetupFor(c.TargetId);var recipe=CurrentRecipe();if(setup.Profile==null || setup.Body==null || recipe==null)return new List<HandlePoint>();
            string limb=(string)_limb.SelectedItem;var contact=recipe.Contacts.FirstOrDefault(n=>n.Chain==limb && n.Start==(double)_start.Value)??recipe.Contacts.FirstOrDefault(n=>n.Chain==limb);var result=new List<HandlePoint>();
            int pelvis=setup.Profile.Chain("Pelvis").Bones[0];result.Add(new HandlePoint{Kind="pelvis",World=NativePoint(pelvis,new HVec()),R=1,G=1,B=0});
            if(contact!=null)
            {
                var goal=contact.Anchor=="World"?contact.Goal:NativePoint(setup.Profile.Chain(contact.Anchor).Bones.Last(),contact.Goal);result.Add(new HandlePoint{Kind="goal",World=goal,R=0,G=1,B=0});
                _poleOrigin=NativePoint(setup.Profile.Chain(limb).Bones[0],new HVec());var pole=contact.UsePoleOverride?contact.Pole:setup.Body.Landmark(limb).Pole;
                result.Add(new HandlePoint{Kind="pole",World=_poleOrigin+pole.Unit()*(setup.Body.LegLength*.35),R=0,G=1,B=1});
            }
            foreach(var p in result)if(p.Kind==_dragKind)p.World=_dragPoint;return result;
        }
        private void DrawConstraintHandles(ModelPanelViewport viewport)
        {
            if(IsDisposed || !_editConstraints.Checked)return;
            try
            {
                var handles=Handles();if(handles.Count==0)return;double size=_session.Project.HumanoidSetupFor(Context().TargetId).Body.LegLength*.025;
                GL.PushAttrib(AttribMask.AllAttribBits);try{GL.Disable(EnableCap.DepthTest);GL.Disable(EnableCap.Lighting);GL.Disable(EnableCap.Texture2D);GL.LineWidth(3);GL.Begin(BeginMode.Lines);
                    foreach(var h in handles){GL.Color3(h.R,h.G,h.B);foreach(var axis in new[]{new HVec(size,0,0),new HVec(0,size,0),new HVec(0,0,size)}){Vertex(h.World-axis);Vertex(h.World+axis);}if(h.Kind=="pole"){Vertex(_poleOrigin);Vertex(h.World);}}
                    GL.End();
                }finally{GL.PopAttrib();}
            }catch(Exception){/* A changed/missing binding draws no stale handle; commands report diagnostics. */}
        }
        private static void Vertex(HVec p)=>GL.Vertex3(p.X,p.Y,p.Z);
        private void HandleDown(object sender,MouseEventArgs e)
        {
            if(e.Button!=MouseButtons.Left || !_editConstraints.Checked)return;
            Try(()=>{var vp=_host.Editor.ModelPanel.CurrentViewport;if(vp==null)return;var picked=Handles().Select(h=>new{Handle=h,Screen=vp.Project(new Vector3((float)h.World.X,(float)h.World.Y,(float)h.World.Z))}).Select(h=>new{h.Handle,h.Screen,Distance=Math.Pow(h.Screen._x+vp.Region.X-e.X,2)+Math.Pow(vp.LocalToWorldYf(h.Screen._y)-e.Y,2)}).Where(h=>h.Distance<144).OrderBy(h=>h.Distance).FirstOrDefault();if(picked==null)return;
                _dragKind=picked.Handle.Kind;_dragPoint=_dragStart=picked.Handle.World;_dragDepth=picked.Screen._z;_dragDocument=_host.Document.RequestId;_dragContext=Context();_dragStateHash=CharacterProject.Hash(CharacterProject.Serialize(_session.Project.State));((Control)_host.Editor.ModelPanel).Capture=true;
            });
        }
        private void HandleMove(object sender,MouseEventArgs e)
        {
            if(_dragKind==null)return;var viewport=_host.Editor.ModelPanel.CurrentViewport;var p=viewport.UnProject(e.X,e.Y,_dragDepth);_dragPoint=new HVec(p._x,p._y,p._z);_host.Editor.ModelPanel.Invalidate();
        }
        private void HandleUp(object sender,MouseEventArgs e)
        {
            if(e.Button!=MouseButtons.Left || _dragKind==null)return;string kind=_dragKind;_dragKind=null;((Control)_host.Editor.ModelPanel).Capture=false;
            Try(()=>{if(_host.Document?.RequestId!=_dragDocument || !SameContext(_dragContext,Context()) || _dragStateHash!=CharacterProject.Hash(CharacterProject.Serialize(_session.Project.State)))throw new InvalidOperationException("Native document, selected clip/target/part or project changed during handle drag; recipe was not changed.");var r=Recipe(true);var setup=_session.Project.HumanoidSetupFor(Context().TargetId);
                if(kind=="pelvis")r.PelvisBias=Math.Max(-setup.Body.PelvisLimit,Math.Min(setup.Body.PelvisLimit,r.PelvisBias+_dragPoint.Y-_dragStart.Y));
                else
                {
                    string limb=(string)_limb.SelectedItem;var contact=r.Contacts.FirstOrDefault(c=>c.Chain==limb&&c.Start==(double)_start.Value)??r.Contacts.First(c=>c.Chain==limb);
                    if(kind=="pole"){contact.Pole=(_dragPoint-_poleOrigin).Unit();contact.UsePoleOverride=true;}
                    else if(contact.Anchor=="World")contact.Goal=_dragPoint;
                    else{int bone=setup.Profile.Chain(contact.Anchor).Bones.Last();var inv=_host.Document.Model.BoneCache[bone].InverseMatrix;var p=inv*new Vector3((float)_dragPoint.X,(float)_dragPoint.Y,(float)_dragPoint.Z);contact.Goal=new HVec(p._x,p._y,p._z);}
                }
                _session.Execute(()=>_session.Project.SaveHumanoidRecipe(r));_status.Text="Constraint recipe saved as one undoable change. Regenerate Fit to bake it; the current native comparison is historical.";_host.Editor.ModelPanel.Invalidate();
            });
        }
        private void HandleCaptureChanged(object sender,EventArgs e){if(!((Control)_host.Editor.ModelPanel).Capture && _dragKind!=null){_dragKind=null;_host.Editor.ModelPanel.Invalidate();}}
    }
}
