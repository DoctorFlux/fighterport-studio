// FighterPort Studio modification, 2026-10-17: independent product identity and local-only safety.
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public partial class IssueDialog : Form
    {
        private readonly Exception _exception;

        public IssueDialog(Exception e, List<ResourceNode> edited)
        {
            _exception = e;

            InitializeComponent();
            label1.Text = "FighterPort Studio encountered an error. Diagnostics remain local. You can save a report and try to save changed resources.";
            button1.Text = "Save report";
            button1.Width = 90;
            button1.Left -= 15;
            button2.Text = "Close";
            button2.Width = 100;
            txtDescription.Text = "Describe what happened for your local report.";
            Text = "FighterPort Studio — " + e.GetType();
            txtStack.Text = e.Message + ' ' + (e.InnerException?.Message ?? "") + '\n' + e.StackTrace;

            lstChangedFiles.Visible =
                lblChangedFiles.Visible =
                    spltChangedFiles.Visible =
                        edited != null && edited.Count > 0;

            lstChangedFiles.Items.AddRange(edited.ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (chkForceClose.Checked)
            {
                try
                {
                    if (Program.CanRunDiscordRPC)
                    {
                        Discord.DiscordRpc.ClearPresence();
                        Discord.DiscordRpc.Shutdown();
                    }
                }
                catch
                {
                    // Discord RPC doesn't need to work always
                }

                Environment.Exit(0);
            }

            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Text diagnostic report|*.txt",
                FileName = "FighterPort-diagnostic.txt",
                OverwritePrompt = true
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    BrawlLib.Internal.IO.SourceWriteGuard.ThrowIfProtected(dialog.FileName);
                    File.WriteAllText(dialog.FileName, Program.AssemblyTitleFull + Environment.NewLine +
                        txtTitle.Text + Environment.NewLine + txtDescription.Text + Environment.NewLine +
                        txtStack.Text);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    MessageBox.Show(ex.Message, "Diagnostic report", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtTitle_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                txtTitle.ForeColor = Color.Gray;
                txtTitle.Text = "Write a quick one-sentence summary of the bug";
            }
        }

        private void txtTitle_Enter(object sender, EventArgs e)
        {
            if (txtTitle.ForeColor == Color.Gray)
            {
                txtTitle.ForeColor = Color.Black;
                txtTitle.Text = "";
            }
        }

        private void txtDescription_Enter(object sender, EventArgs e)
        {
        }

        private void txtDescription_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtDescription.Text))
            {
                txtDescription.ForeColor = Color.Gray;
                txtDescription.Text =
                    "Describe what happened for your local report. Nothing is submitted online. Review file paths and other personal information before sharing a report yourself.";
            }
        }

        private bool Save(ResourceNode r)
        {
            if (r._origPath == null)
            {
                return SaveAs(r);
            }

            r.Merge(ModifierKeys == (Keys.Control | Keys.Shift));
            r.Export(r._origPath);
            r.IsDirty = false;
            return true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResourceNode r = lstChangedFiles.SelectedItem as ResourceNode;
            if (r != null && Save(r))
            {
                MessageBox.Show("Successfully saved " + r.Name);
            }
            else
            {
                MessageBox.Show(r.Name + " was not saved successfully.");
            }
        }

        private bool SaveAs(ResourceNode r)
        {
            if (r != null)
            {
                using (SaveFileDialog d = new SaveFileDialog())
                {
                    d.InitialDirectory = r._origPath.Substring(0, r._origPath.LastIndexOf('\\'));
                    d.Filter = string.Format("(*{0})|*{0}", Path.GetExtension(r._origPath));
                    d.Title = "Please choose a location to save this file.";
                    if (d.ShowDialog(this) == DialogResult.OK)
                    {
                        r.Merge();
                        r.Export(d.FileName);
                        return true;
                    }
                }
            }

            return false;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResourceNode r = lstChangedFiles.SelectedItem as ResourceNode;
            if (r != null && SaveAs(r))
            {
                MessageBox.Show("Successfully saved " + r.Name);
            }
            else
            {
                MessageBox.Show(r.Name + " was not saved successfully.");
            }
        }

        private void ctxFile_Opening(object sender, CancelEventArgs e)
        {
            ResourceNode file = lstChangedFiles.SelectedItem as ResourceNode;
            if (file != null)
            {
                saveToolStripMenuItem.Enabled = file.IsDirty;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void txtDescription_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            // Report descriptions do not open inherited online destinations.
        }

        private void txtDescription_MouseUp(object sender, MouseEventArgs e)
        {
            if (txtDescription.ForeColor == Color.Gray)
            {
                txtDescription.ForeColor = Color.Black;
                txtDescription.Text = "";
            }
        }
    }
}