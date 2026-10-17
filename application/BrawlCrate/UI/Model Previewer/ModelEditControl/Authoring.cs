// FighterPort addition, 2026-10-17. Repository LGPL-3.0 terms.
using System.Windows.Forms;
namespace BrawlCrate.UI.Model_Previewer.ModelEditControl
{
    public partial class ModelEditControl
    {
        public PreviewDocument AuthoringDocument {get;internal set;}
        public bool AuthoringReadOnly {get;private set;}
        private bool _authoringCompact;
        public void AuthoringLayout(bool compact)
        {
            _authoringCompact=compact;showLeft.Checked=showRight.Checked=showTop.Checked=!compact;showBottom.Checked=true;CheckDimensions();
        }
        public void SetAuthoringReadOnly(bool value)
        {
            AuthoringReadOnly=value;
            if(value){SelectedBone=null;ControlType=BrawlLib.Internal.Windows.Controls.ModelViewer.MainWindowBase.TransformType.None;}
            leftPanel.Enabled=!value;rightPanel.Enabled=!value;animCtrlPnl.Enabled=!value;toolStrip1.Enabled=!value;
            menuStrip1.Enabled=!value;
            if(AuthoringDocument!=null){fileToolStripMenuItem.Enabled=false;rightPanel.pnlOpenedFiles.Enabled=false;}
            AllowDrop=!value && AuthoringDocument==null;ModelPanel.AllowDrop=false;
        }
        protected override bool ProcessKeyPreview(ref Message m)
        {
            if(AuthoringReadOnly)return false; // Playback/frame controls still process their own keys.
            return base.ProcessKeyPreview(ref m);
        }
        protected override void modelPanel1_MouseDown(object sender,MouseEventArgs e){if(!AuthoringReadOnly)base.modelPanel1_MouseDown(sender,e);}
    }
}
