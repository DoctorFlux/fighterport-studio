// FighterPort Studio addition, 2026-10-17. Licensed under the repository LGPL-3.0 terms.
using BrawlCrate.Sources;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    /// <summary>First loose-folder provider, opening staged resources in the real native editor.</summary>
    public sealed class SourceBrowserForm : Form
    {
        private readonly SourceFolderSession _session;
        private readonly ListView _entries;
        private readonly Label _directory, _status;
        private readonly Button _open, _up, _cancel;
        private CancellationTokenSource _operation;
        private string _currentDirectory;

        public static void ShowSourceBrowser(IWin32Window owner)
        {
            using (var chooser = new FolderBrowserDialog
            {
                Description = "Open a source folder read-only. Supported raw Brawl resources open as independent editable copies.",
                ShowNewFolderButton = false
            })
            {
                if (chooser.ShowDialog(owner) != DialogResult.OK) return;
                try
                {
                    using (var browser = new SourceBrowserForm(new SourceFolderSession(chooser.SelectedPath)))
                        browser.ShowDialog(owner);
                }
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
                {
                    MessageBox.Show(owner, error.Message, "Open Source", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public SourceBrowserForm(SourceFolderSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            Text = "FighterPort Studio — Open Source";
            Name = "SourceBrowserForm";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(920, 530);
            MinimumSize = new Size(620, 400);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { AutoSize = true, Text = "Read-only source: " + session.RootPath, Margin = new Padding(0, 0, 0, 10) });
            var navigation = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = false };
            _up = new Button { Text = "Up", AutoSize = true, Name = "sourceUpButton" };
            _up.Click += (sender, args) => LoadDirectory(Path.GetDirectoryName(_currentDirectory));
            _directory = new Label { AutoSize = true, Margin = new Padding(10, 8, 0, 8) };
            navigation.Controls.Add(_up);
            navigation.Controls.Add(_directory);
            layout.Controls.Add(navigation);
            _entries = new ListView
            {
                Name = "sourceEntries", Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                HideSelection = false, MultiSelect = false, ShowItemToolTips = true
            };
            _entries.Columns.Add("Resource / folder", 280);
            _entries.Columns.Add("Detection / availability", 570);
            _entries.SelectedIndexChanged += (sender, args) => UpdateOpenButton();
            _entries.DoubleClick += (sender, args) => OpenSelection();
            layout.Controls.Add(_entries);
            _status = new Label
            {
                AutoSize = true, MaximumSize = new Size(860, 0), Padding = new Padding(0, 8, 0, 8),
                Text = "Raw Brawl resources only, up to 64 MiB. Companion files are not assembled; missing textures, palettes or animation targets require an enclosing BRRES/ARC or manual selection in the native editor."
            };
            layout.Controls.Add(_status);
            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var close = new Button { Text = "Close", AutoSize = true, DialogResult = DialogResult.Cancel };
            close.Click += (sender, args) => Close();
            _open = new Button { Text = "Open editable copy", AutoSize = true, Enabled = false, Name = "sourceOpenButton" };
            _open.Click += (sender, args) => OpenSelection();
            _cancel = new Button { Text = "Cancel operation", AutoSize = true, Enabled = false };
            _cancel.Click += (sender, args) => _operation?.Cancel();
            buttons.Controls.Add(close);
            buttons.Controls.Add(_open);
            buttons.Controls.Add(_cancel);
            layout.Controls.Add(buttons);
            Controls.Add(layout);
            CancelButton = close;
            Shown += (sender, args) => LoadDirectory(session.RootPath);
            FormClosing += (sender, args) => _operation?.Cancel();
        }

        private SourceEntry SelectedEntry
        {
            get { return _entries.SelectedItems.Count == 1 ? _entries.SelectedItems[0].Tag as SourceEntry : null; }
        }

        private void UpdateOpenButton()
        {
            SourceEntry selected = SelectedEntry;
            _open.Enabled = !_cancel.Enabled && selected != null && (selected.IsDirectory || selected.CanOpen);
            _open.Text = selected != null && selected.IsDirectory ? "Browse folder" : "Open editable copy";
        }

        private async void LoadDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory)) return;
            _operation?.Cancel();
            var operation = new CancellationTokenSource();
            _operation = operation;
            _cancel.Enabled = true;
            _open.Enabled = false;
            _up.Enabled = false;
            _entries.Items.Clear();
            _directory.Text = "Reading immediate folder entries…";
            try
            {
                var entries = await Task.Run(() => _session.EnumerateDirectory(directory, operation.Token), operation.Token);
                if (IsDisposed || operation.IsCancellationRequested) return;
                _currentDirectory = directory;
                _directory.Text = directory;
                foreach (SourceEntry entry in entries)
                {
                    var item = new ListViewItem(entry.Name) { Tag = entry, ToolTipText = entry.Description };
                    item.SubItems.Add(entry.Description);
                    if (!entry.IsDirectory && !entry.CanOpen) item.ForeColor = SystemColors.GrayText;
                    _entries.Items.Add(item);
                }
                _status.Text = "Showing immediate entries only (limit 512); folders load on demand. Companion files are not assembled: texture palettes, models and animation targets may need an enclosing supported BRRES/ARC or manual selection. Source mounts remain protected after this window closes.";
            }
            catch (OperationCanceledException) { if (!IsDisposed) _directory.Text = "Cancelled. Reopen Open Source to choose another folder."; }
            catch (Exception error) { if (!IsDisposed) _status.Text = error.Message; }
            finally
            {
                if (!IsDisposed && _operation == operation)
                {
                    _cancel.Enabled = false;
                    _up.Enabled = _currentDirectory != null && !string.Equals(_currentDirectory, _session.RootPath, StringComparison.OrdinalIgnoreCase);
                    UpdateOpenButton();
                }
                operation.Dispose();
                if (_operation == operation) _operation = null;
            }
        }

        private async void OpenSelection()
        {
            SourceEntry entry = SelectedEntry;
            if (entry == null || _cancel.Enabled) return;
            if (entry.IsDirectory) { LoadDirectory(entry.Path); return; }
            if (!entry.CanOpen) return;
            var operation = new CancellationTokenSource();
            _operation = operation;
            _cancel.Enabled = true;
            _open.Enabled = false;
            _up.Enabled = false;
            _status.Text = "Validating and hashing an independent editable copy…";
            try
            {
                StagedSource staged = await Task.Run(() => _session.Stage(entry.Path, operation.Token), operation.Token);
                if (IsDisposed || operation.IsCancellationRequested) return;
                if (Program.Open(staged.StagedPath)) Close();
                else _status.Text = "The native editor did not open this resource. The source is unchanged; its staged copy remains available.";
            }
            catch (OperationCanceledException) { if (!IsDisposed) _status.Text = "Staging cancelled. The source was not changed."; }
            catch (Exception error) { if (!IsDisposed) _status.Text = "Not opened: " + error.Message; }
            finally
            {
                if (!IsDisposed)
                {
                    _cancel.Enabled = false;
                    _up.Enabled = _currentDirectory != null && !string.Equals(_currentDirectory, _session.RootPath, StringComparison.OrdinalIgnoreCase);
                    UpdateOpenButton();
                }
                operation.Dispose();
                if (_operation == operation) _operation = null;
            }
        }
    }
}
