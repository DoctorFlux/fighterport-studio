// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public sealed class RigMapReviewForm : Form
    {
        public readonly DataGridView Pairs;
        public readonly CheckedListBox RestOnly;
        public readonly ComboBox RootPolicy;
        public readonly NumericUpDown UnitScale;
        public readonly TextBox ReviewNote;
        public string ReviewedMapId { get; private set; }
        public RigMapReviewForm(CharacterProject project,string sourceId,string targetId)
        {
            var source=project.State.Characters.Single(c=>c.Id==sourceId);var target=project.State.Characters.Single(c=>c.Id==targetId);
            Text="Review rig interpretation and FK mapping";ClientSize=new Size(1100,700);StartPosition=FormStartPosition.CenterParent;
            var layout=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(10),ColumnCount=1,RowCount=6};
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,70));layout.RowStyles.Add(new RowStyle(SizeType.Percent,30));
            layout.Controls.Add(new TextBox {Multiline=true,ReadOnly=true,Dock=DockStyle.Fill,Height=95,Text="Review source "+source.Name+" ["+source.RigFingerprint+"] -> target "+target.Name+" ["+target.RigFingerprint+"]\r\n"+source.Interpretation+"\r\nTarget local offsets/proportions are preserved. World rest-relative rotations transfer; non-unit scales/compensation/billboards and animated non-root translations reject. Every helper needs an explicit policy; this is not IK or mesh rebinding."});
            Pairs=new DataGridView {Name="rigPairs",Dock=DockStyle.Fill,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
            Pairs.Columns.Add(new DataGridViewTextBoxColumn {Name="Source",HeaderText="Source index / parent / rest S,R,T",ReadOnly=true,FillWeight=150});
            Pairs.Columns.Add(new DataGridViewTextBoxColumn {Name="Role",HeaderText="Reviewed semantic role",FillWeight=65});
            var choices=new[]{"Ignore (must remain static)"}.Concat(target.Bones.Select((b,i)=>i+": "+b.Name+" / parent "+b.Parent+" / "+b.Rest)).ToArray();
            var column=new DataGridViewComboBoxColumn {Name="Target",HeaderText="Explicit target bone",FillWeight=150};column.Items.AddRange(choices);Pairs.Columns.Add(column);
            for(int i=0;i<source.Bones.Count;i++) {var b=source.Bones[i];Pairs.Rows.Add(i+": "+b.Name+" / parent "+b.Parent+" / "+b.Rest,"",null);}
            layout.Controls.Add(Pairs);
            RestOnly=new CheckedListBox {Name="rigRestOnly",Dock=DockStyle.Fill,CheckOnClick=true};RestOnly.Items.AddRange(target.Bones.Select((b,i)=>(object)(i+": "+b.Name+" - preserve local rest, follow animated parent")).ToArray());layout.Controls.Add(RestOnly);
            var controls=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoSize=true};RootPolicy=new ComboBox {Name="rigRootPolicy",DropDownStyle=ComboBoxStyle.DropDownList,Width=190};RootPolicy.Items.AddRange(new object[]{"InPlace","ScaledNativeTranslation"});RootPolicy.SelectedIndex=0;
            UnitScale=new NumericUpDown {Name="rigUnitScale",Minimum=.001m,Maximum=100,DecimalPlaces=3,Value=1};controls.Controls.Add(new Label {Text="Root policy / reviewed native-unit ratio",AutoSize=true});controls.Controls.Add(RootPolicy);controls.Controls.Add(UnitScale);layout.Controls.Add(controls);
            ReviewNote=new TextBox {Name="rigReviewNote",Dock=DockStyle.Fill,Text="",MaxLength=2048};layout.Controls.Add(ReviewNote);
            var actions=new FlowLayoutPanel {AutoSize=true,Dock=DockStyle.Fill};var status=new Label {Name="rigReviewStatus",AutoSize=true};var approve=new Button {Name="rigApprove",Text="Approve these exact fingerprints and policies",AutoSize=true};
            approve.Click+=(s,e)=> {
                try
                {
                    Pairs.EndEdit();var map=new ReviewedRigMap {SourceCharacterId=source.Id,TargetCharacterId=target.Id,SourceFingerprint=source.RigFingerprint,TargetFingerprint=target.RigFingerprint,
                        SourceInterpretation=source.Interpretation,TargetInterpretation=target.Interpretation,RootPolicy=(string)RootPolicy.SelectedItem,NativeUnitScale=(double)UnitScale.Value,ReviewNote=ReviewNote.Text};
                    foreach(DataGridViewRow row in Pairs.Rows)
                    {
                        if(row.Cells[2].Value==null)throw new InvalidOperationException("Every source bone needs an explicit target or ignore choice.");
                        string choice=(string)row.Cells[2].Value;
                        if(choice==choices[0])map.IgnoredSource.Add(row.Index);
                        else map.Pairs.Add(new RigRolePair {Role=Convert.ToString(row.Cells[1].Value),Source=row.Index,Target=Array.IndexOf(choices,choice)-1});
                    }
                    map.TargetRestOnly.AddRange(RestOnly.CheckedIndices.Cast<int>());ReviewedMapId=project.ReviewRigMap(map);DialogResult=DialogResult.OK;Close();
                }
                catch(Exception error) {status.Text="Not approved: "+error.Message;}
            };
            actions.Controls.Add(approve);actions.Controls.Add(status);layout.Controls.Add(actions);Controls.Add(layout);
        }
    }
}
