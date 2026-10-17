// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public sealed class SharedAnimationLibraryForm : Form
    {
        private readonly CharacterProject _project;
        private readonly AnimationLibraryStore _store;
        private readonly ListView _clips;
        private readonly TextBox _search;
        private readonly Label _status;
        private AnimationLibraryStore.LibraryCursor _cursor;
        public SharedAnimationLibraryForm(CharacterProject project)
        {
            _project=project;_store=project.SharedLibrary??new AnimationLibraryStore();Text="Shared animation library";ClientSize=new Size(950,560);StartPosition=FormStartPosition.CenterParent;
            var layout=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(10),ColumnCount=1,RowCount=4};layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            var actions=new FlowLayoutPanel {AutoSize=true,Dock=DockStyle.Fill};_search=new TextBox {Name="sharedSearch",Width=220};actions.Controls.Add(_search);
            var refresh=new Button {Name="sharedRefresh",Text="Search / refresh",AutoSize=true};refresh.Click+=(s,e)=>{_cursor?.Dispose();_cursor=null;Page();};actions.Controls.Add(refresh);
            var next=new Button {Name="sharedNext",Text="Next page",AutoSize=true};next.Click+=(s,e)=>Page();actions.Controls.Add(next);layout.Controls.Add(actions);
            _clips=new ListView {Name="sharedClips",Dock=DockStyle.Fill,View=View.Details,FullRowSelect=true,MultiSelect=false,HideSelection=false};_clips.Columns.Add("Clip",260);_clips.Columns.Add("Source profile",270);_clips.Columns.Add("Revision",65);_clips.Columns.Add("Native frames / channels",190);layout.Controls.Add(_clips);
            var use=new Button {Name="sharedUse",Text="Use selected immutable revision in this project",AutoSize=true};use.Click+=(s,e)=> {
                try {if(_clips.SelectedItems.Count!=1)throw new InvalidOperationException("Select a clip revision.");var a=(AnimationAsset)_clips.SelectedItems[0].Tag;_project.UseSharedClip(a.Id,a.Revision,_store.Root);DialogResult=DialogResult.OK;Close();}
                catch(Exception error){_status.Text=error.Message;}
            };layout.Controls.Add(use);_status=new Label {Name="sharedStatus",AutoSize=true,MaximumSize=new Size(920,0)};layout.Controls.Add(_status);Controls.Add(layout);FormClosed+=(s,e)=>_cursor?.Dispose();Page();
        }
        private void Page()
        {
            try
            {
                if(_cursor==null)_cursor=_store.OpenCursor(_search.Text);var page=_cursor.Next(32);_clips.Items.Clear();
                foreach(var a in page.Clips)_clips.Items.Add(new ListViewItem(new[]{a.Name,a.SourceCharacterId,a.Revision.ToString(),a.Frames+" / skeletal CHR0"}){Tag=a});
                _status.Text="Library "+_store.Id+" / generation "+page.Generation+". Metadata page only; native data is loaded on selection. "+(page.HasMore?"Continue to check the next page.":"End of catalogue.");
            }
            catch(Exception error){_status.Text="Stopped: "+error.Message;}
        }
    }
}
