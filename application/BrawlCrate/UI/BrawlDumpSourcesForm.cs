// FighterPort Studio addition, 2026-10-17. Repository LGPL-3.0 terms.
using BrawlCrate.Sources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public sealed class BrawlDumpSourcesForm : Form
    {
        private readonly CompanionWorkspace _workspace;
        private BrawlDumpProfile _profile;
        private BrawlLooseDumpProvider _discovery;
        private readonly ListView _roots, _files;
        private readonly NumericUpDown _priority;
        private readonly Label _status;
        private readonly TextBox _issues;
        private readonly List<Button> _buttons = new List<Button>();
        private CancellationTokenSource _operation;
        private bool _busy, _refreshing;
        public BrawlDumpProfile Profile => _profile;
        public BrawlLooseDumpProvider Discovery => _discovery;
        public bool IsBusy => _busy;

        public BrawlDumpSourcesForm(CompanionWorkspace workspace)
        {
            _workspace = workspace;
            _profile = workspace.DumpProfile.Snapshot();
            string profileError = null;
            if (_profile.Mounts.Count == 0 && File.Exists(BrawlDumpProfile.DefaultPath))
                try { _profile = BrawlDumpProfile.Load(BrawlDumpProfile.DefaultPath); } catch (Exception error) { profileError = error.Message; }
            Text = "FighterPort Studio — Brawl dump folders"; Name = "BrawlDumpSourcesForm";
            ClientSize = new Size(1160, 720); MinimumSize = new Size(900, 650); StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 1, RowCount = 7 };
            for (int i = 0; i < 7; i++) layout.RowStyles.Add(new RowStyle(i == 3 ? SizeType.Percent : SizeType.AutoSize, i == 3 ? 100 : 0));
            layout.Controls.Add(new Label { AutoSize = true, MaximumSize = new Size(1120, 0), Text =
                "Configure matching dump layouts. Higher priority wins only for the same complete relative file path (including its BRRES/container). " +
                "Ties stay ambiguous. Discover headers in pages, stage chosen files, then choose native model/texture/motion resources in Brawl Companions. " +
                "Staging any row is an explicit manual choice; scans never change pinned choices or replace embedded base resources." });
            _roots = new ListView { Name = "dumpRoots", Dock = DockStyle.Top, Height = 125, View = View.Details, FullRowSelect = true, MultiSelect = false, CheckBoxes = true, HideSelection = false };
            _roots.Columns.Add("Enabled / root", 190); _roots.Columns.Add("Priority", 70); _roots.Columns.Add("Local path", 840);
            _roots.ItemChecked += (sender, args) =>
            {
                if (_refreshing || _busy || Disposing || IsDisposed || !(args.Item.Tag is SourceMount mount)) return;
                // Do not rebuild this ListView inside its own native checkbox notification.
                bool previous = mount.Enabled;
                try
                {
                    _profile.Configure(mount.Id, mount.Priority, args.Item.Checked);
                    _workspace.SetDumpProfile(_profile); ResetDiscovery();
                }
                catch (Exception error)
                {
                    _profile.Configure(mount.Id, mount.Priority, previous);
                    _refreshing = true; args.Item.Checked = previous; _refreshing = false;
                    _status.Text = "Root change not applied: " + error.Message;
                }
            };
            _roots.SelectedIndexChanged += (sender, args) => { if (_roots.SelectedItems.Count == 1) _priority.Value = ((SourceMount)_roots.SelectedItems[0].Tag).Priority; };
            layout.Controls.Add(_roots);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            ButtonAction(actions, "Add root…", "dumpAddRoot", async () =>
            {
                using (var dialog = new FolderBrowserDialog { Description = "Choose an opt-in Brawl loose dump or matching override root" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) AddRoot(dialog.SelectedPath, Path.GetFileName(dialog.SelectedPath), 0);
                await Task.CompletedTask;
            });
            _priority = new NumericUpDown { Name = "dumpPriority", Minimum = -999, Maximum = 999, Width = 65 }; actions.Controls.Add(_priority);
            ButtonAction(actions, "Set priority", "dumpSetPriority", () =>
            {
                if (_roots.SelectedItems.Count == 1) { var mount = (SourceMount)_roots.SelectedItems[0].Tag; ChangeRoot(mount.Id, (int)_priority.Value, mount.Enabled); }
                return Task.CompletedTask;
            });
            ButtonAction(actions, "Remove root", "dumpRemoveRoot", () =>
            {
                if (_roots.SelectedItems.Count == 1) { _profile.Remove(((SourceMount)_roots.SelectedItems[0].Tag).Id); ConfigurationChanged(); }
                return Task.CompletedTask;
            });
            ButtonAction(actions, "Save roots", "dumpSaveRoots", () => { SaveProfile(); _status.Text = "Roots saved. Pinned selections remain independent of root configuration."; return Task.CompletedTask; });
            ButtonAction(actions, "New discovery", "dumpNewScan", async () => { ResetDiscovery(); await DiscoverNextAsync(); });
            ButtonAction(actions, "Discover next 128", "dumpNextPage", () => DiscoverNextAsync());
            var cancel = new Button { Text = "Cancel", Name = "dumpCancel", AutoSize = true }; cancel.Click += (sender, args) => _operation?.Cancel(); actions.Controls.Add(cancel);
            layout.Controls.Add(actions);
            _files = new ListView { Name = "dumpCandidates", Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
            _files.Columns.Add("Logical file path", 310); _files.Columns.Add("Source root", 130); _files.Columns.Add("Priority", 65); _files.Columns.Add("Header / bytes", 115); _files.Columns.Add("Resolution / reason", 485);
            layout.Controls.Add(_files);
            _status = new Label { Name = "dumpStatus", AutoSize = true, MaximumSize = new Size(1120, 0), Text = profileError ?? "No scan performed. Discovery does not stage, hash or decode the dump." }; layout.Controls.Add(_status);
            _issues = new TextBox { Name = "dumpIssues", ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Top, Height = 100 }; layout.Controls.Add(_issues);
            var bottom = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            ButtonAction(bottom, "Stage selected file (explicit choice)", "dumpStageSelected", StageSelectedAsync);
            var close = new Button { Text = "Return to companions", AutoSize = true }; close.Click += (sender, args) => Close(); bottom.Controls.Add(close);
            layout.Controls.Add(bottom); Controls.Add(layout); RefreshRoots();
            FormClosing += (sender, args) => { _operation?.Cancel(); if (!_busy) _discovery?.Dispose(); };
        }
        private void ButtonAction(Control panel, string text, string name, Func<Task> action)
        {
            var button = new Button { Text = text, Name = name, AutoSize = true }; _buttons.Add(button);
            button.Click += async (sender, args) => await RunAsync(action); panel.Controls.Add(button);
        }
        private async Task RunAsync(Func<Task> action)
        {
            if (_busy) return;
            _busy = true; _operation = new CancellationTokenSource();
            foreach (var button in _buttons) button.Enabled = false;
            _roots.Enabled = _priority.Enabled = false;
            try { await action(); }
            catch (OperationCanceledException) { if (!IsDisposed) _status.Text = "Cancelled. Previously pinned choices and saved work remain intact."; }
            catch (Exception error) { if (!IsDisposed) _status.Text = "Not applied: " + error.Message; }
            finally
            {
                _operation.Dispose(); _operation = null; _busy = false;
                if (!IsDisposed) { foreach (var button in _buttons) button.Enabled = true; _roots.Enabled = _priority.Enabled = true; }
                else _discovery?.Dispose();
            }
        }
        public void AddRoot(string path, string label, int priority)
        {
            var mount = _profile.Add(path, label, priority);
            try { new SourceFolderSession(mount.Root, _workspace.StorageRoot); }
            catch { _profile.Remove(mount.Id); throw; }
            ConfigurationChanged();
        }
        public void ChangeRoot(string id, int priority, bool enabled)
        { _profile.Configure(id, priority, enabled); ConfigurationChanged(); }
        private void ConfigurationChanged() { ResetDiscovery(); _workspace.SetDumpProfile(_profile); RefreshRoots(); }
        public void SaveProfile() { _profile.Save(BrawlDumpProfile.DefaultPath); _workspace.SetDumpProfile(_profile); }
        private void ResetDiscovery() { _discovery?.Dispose(); _discovery = null; _files.Items.Clear(); }
        private void RefreshRoots()
        {
            _refreshing = true; _roots.Items.Clear();
            foreach (var mount in _profile.Mounts)
            {
                var item = new ListViewItem(mount.Label) { Tag = mount, Checked = mount.Enabled };
                item.SubItems.Add(mount.Priority.ToString()); item.SubItems.Add(mount.Root); _roots.Items.Add(item);
            }
            _refreshing = false;
        }
        public async Task DiscoverNextAsync(int entries = 128)
        {
            if (_discovery == null) _discovery = new BrawlLooseDumpProvider(_profile, _workspace.StorageRoot);
            var progress = new Progress<string>(text => { if (!IsDisposed) _status.Text = "Discovering: " + text; });
            CancellationToken token = _operation?.Token ?? CancellationToken.None;
            await Task.Run(() => _discovery.ReadNext(entries, token, text => ((IProgress<string>)progress).Report(text)));
            if (IsDisposed) return;
            string selected = _files.SelectedItems.Count == 1 ? ((DumpCandidate)_files.SelectedItems[0].Tag).Identity : null;
            _files.BeginUpdate(); _files.Items.Clear();
            foreach (var candidate in _discovery.Candidates)
            {
                var item = new ListViewItem(candidate.RelativePath) { Tag = candidate };
                item.SubItems.Add(candidate.RootLabel); item.SubItems.Add(candidate.Priority.ToString());
                item.SubItems.Add(candidate.Signature + " / " + candidate.Length); item.SubItems.Add(_discovery.Explain(candidate));
                _files.Items.Add(item); if (candidate.Identity == selected) item.Selected = true;
            }
            _files.EndUpdate(); _status.Text = _discovery.Status; _issues.Text = string.Join(Environment.NewLine, _discovery.Issues);
        }
        public async Task StageSelectedAsync()
        {
            if (_files.SelectedItems.Count != 1 || _discovery == null) throw new IOException("Select a discovered file explicitly.");
            var candidate = (DumpCandidate)_files.SelectedItems[0].Tag;
            string reason = _discovery.Explain(candidate);
            CancellationToken token = _operation?.Token ?? CancellationToken.None;
            await Task.Run(() => _workspace.AddDiscoveredSource(_profile, candidate, reason, token));
            if (!IsDisposed) _status.Text = "Pinned " + candidate.RootLabel + " / " + candidate.RelativePath + ". " + reason + ". Now choose its native resources in Brawl Companions.";
        }
    }
}
