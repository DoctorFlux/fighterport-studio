// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Authoring;
using System.Drawing;
using System.Windows.Forms;
namespace BrawlCrate.UI
{
    public sealed class RigMapReviewForm : Form
    {
        private readonly RigMapReviewPanel _panel;
        public DataGridView Pairs=>_panel.Pairs;
        public CheckedListBox RestOnly=>_panel.RestOnly;
        public TextBox ReviewNote=>_panel.ReviewNote;
        public string ReviewedMapId=>_panel.ReviewedMapId;
        public RigMapReviewForm(CharacterProject project,string sourceId,string targetId)
        {
            Text="Review rig interpretation and FK mapping";ClientSize=new Size(1100,700);StartPosition=FormStartPosition.CenterParent;
            _panel=new RigMapReviewPanel(project,sourceId,targetId){Dock=DockStyle.Fill};
            _panel.Approved+=(s,e)=>{DialogResult=DialogResult.OK;Close();};Controls.Add(_panel);
        }
    }
}
