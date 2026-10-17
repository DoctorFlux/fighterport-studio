// FighterPort Studio modification, 2026-10-17: independent product identity and local-only safety.
using BrawlLib.Internal.Audio;
using System;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public partial class AboutForm : Form
    {
        private static AboutForm _instance;
        public static AboutForm Instance => _instance ?? (_instance = new AboutForm());

        public AboutForm()
        {
            InitializeComponent();
            lblName.Text = Program.AssemblyTitleFull;
            Text = "About FighterPort Studio";
            ClientSize = new System.Drawing.Size(620, 420);
            lblName.SetBounds(12, 12, 596, 30);
            txtDescription.SetBounds(12, 48, 596, 220);
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Text = "Independent application derived from BrawlCrate v0.42 Hotfix 1, " +
                "commit d56cbac89dc2cd0e5c3f923e1d03041b16902add.\r\n\r\n" +
                "Lineage: BrawlCrate / BrawlBox / BrawlLib. Upstream authors include soopercool101, " +
                "NuclearNecrosis, Simon381, Onium, Bryan Moulton, BlackJax96, libertyernie and Sammi Husky.\r\n\r\n" +
                "FighterPort modifications dated 17 October 2026. No upstream endorsement is implied. " +
                "Inherited updating, online issue submission and Discord presence are disabled.\r\n\r\n" +
                "See LICENSE, licenses/GPL-3.0.txt, THIRD_PARTY_NOTICES.md and UPSTREAM.md in the source distribution. " +
                "Component licenses and original notices remain applicable.\r\n\r\n" + Program.AssemblyDescription;
            lblCopyright.SetBounds(12, 276, 596, 62);
            lblCopyright.Text = Program.AssemblyCopyright;
            lblBrawlLib.SetBounds(12, 342, 596, 18);
            lblBrawlLib.Text = "Using " + Program.BrawlLibTitle;
            lblAudioBackend.SetBounds(12, 364, 596, 18);
            btnClose.SetBounds(272, 389, 75, 23);

            AudioProvider provider = AudioProvider.Create(null);
            lblAudioBackend.Text = "Audio backend: " + (provider?.ToString() ?? "none");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}