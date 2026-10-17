// FighterPort Studio addition, 2026-10-17. Licensed under the repository LGPL-3.0 terms.
using BrawlCrate.Sources;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrawlCrate.UI
{
    public sealed class CompanionWorkspaceForm : Form
    {
        private CompanionWorkspace _workspace;
        private readonly ComboBox _models, _motions, _candidates;
        private readonly ListView _bindings;
        private readonly Label _status;
        private readonly Button _open;
        private readonly FlowLayoutPanel _actions;
        private bool _refreshing, _busy;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        public CompanionWorkspace Workspace => _workspace;
        public string LastOpenedPath { get; private set; }

        public static void ShowWorkspace(IWin32Window owner, string resolution = null)
        {
            using (var form = new CompanionWorkspaceForm())
            {
                if (resolution != null) form.Shown += async (sender, args) => await form.RunAsync(() => form.LoadResolutionAsync(resolution));
                form.ShowDialog(owner);
            }
        }

        public CompanionWorkspaceForm(CompanionWorkspace workspace = null)
        {
            _workspace = workspace ?? new CompanionWorkspace();
            Text = "FighterPort Studio — Brawl Companions";
            Name = "CompanionWorkspaceForm";
            ClientSize = new Size(1050, 650);
            MinimumSize = new Size(800, 580);
            StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 10 };
            for (int i = 0; i < 10; i++) layout.RowStyles.Add(new RowStyle(i == 5 ? SizeType.Percent : SizeType.AutoSize, i == 5 ? 100 : 0));
            layout.Controls.Add(new Label { AutoSize = true, Text = "Select source files, then choose a model, every texture binding, and one motion. Sources stay read-only." });
            _actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            AddAction("Dump folders…", "companionDumpFolders", () =>
            {
                using (var form = new BrawlDumpSourcesForm(_workspace)) form.ShowDialog(this);
                RefreshChoices(); return Task.CompletedTask;
            });
            AddAction("Add source files…", "companionAdd", async () =>
            {
                using (var dialog = new OpenFileDialog { Title = "Add Brawl companion sources", Multiselect = true, Filter = "Brawl resources|*.brres;*.tex0;*.chr0|All files|*.*" })
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        foreach (string path in dialog.FileNames) await AddSourceAsync(path);
            });
            AddAction("Load resolution…", "companionLoad", async () =>
            {
                using (var dialog = new OpenFileDialog { Filter = "FighterPort companion resolution|*.fpcompanion" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) await LoadResolutionAsync(dialog.FileName);
            });
            AddAction("Resume last resolution", "companionResume", () => LoadResolutionAsync(CompanionWorkspace.LastResolutionPath));
            AddAction("Save resolution…", "companionSave", () =>
            {
                using (var dialog = new SaveFileDialog { Filter = "FighterPort companion resolution|*.fpcompanion", DefaultExt = "fpcompanion", AddExtension = true, FileName = "character.fpcompanion" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) SaveResolution(dialog.FileName);
                return Task.CompletedTask;
            });
            layout.Controls.Add(_actions);
            layout.Controls.Add(new Label { AutoSize = true, Text = "Model from a BRRES (its remaining native resources are retained):" });
            _models = NewCombo("companionModel");
            _models.SelectedIndexChanged += (sender, args) =>
            {
                if (_refreshing || !(_models.SelectedItem is CompanionResource model)) return;
                TryAction(() => { _workspace.SelectModel(model); RefreshBindings(); });
            };
            layout.Controls.Add(_models);
            layout.Controls.Add(new Label { AutoSize = true, Text = "Texture bindings — select a row, then choose its exact resource below. No candidate is picked automatically." });
            _bindings = new ListView { Name = "companionBindings", View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false, Dock = DockStyle.Fill };
            _bindings.Columns.Add("Binding", 210); _bindings.Columns.Add("Resolution", 780);
            _bindings.SelectedIndexChanged += (sender, args) => RefreshCandidates();
            layout.Controls.Add(_bindings);
            _candidates = NewCombo("companionTexture");
            _candidates.SelectedIndexChanged += (sender, args) =>
            {
                if (_refreshing || _bindings.SelectedItems.Count != 1 || !(_candidates.SelectedItem is CompanionResource texture)) return;
                string name = _bindings.SelectedItems[0].Text;
                TryAction(() => { _workspace.SelectTexture(name, texture); RefreshBindings(name); });
            };
            layout.Controls.Add(_candidates);
            var motionRow = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 1 };
            motionRow.Controls.Add(new Label { AutoSize = true, Text = "Motion (exact bone names; this does not retarget different rigs):" });
            _motions = NewCombo("companionMotion");
            _motions.SelectedIndexChanged += (sender, args) =>
            {
                if (_refreshing || !(_motions.SelectedItem is CompanionResource motion)) return;
                TryAction(() => { _workspace.SelectMotion(motion); UpdateStatus(); });
            };
            motionRow.Controls.Add(_motions); layout.Controls.Add(motionRow);
            _status = new Label { Name = "companionStatus", AutoSize = true, MaximumSize = new Size(1000, 0), Padding = new Padding(0, 8, 0, 8) };
            layout.Controls.Add(_status);
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            var close = new Button { Text = "Close", AutoSize = true }; close.Click += (sender, args) => Close();
            _open = new Button { Name = "companionOpen", Text = "Open assembled copy", AutoSize = true };
            _open.Click += async (sender, args) => await RunAsync(async () =>
            {
                _workspace.Save(CompanionWorkspace.LastResolutionPath);
                string output = await Task.Run(() => _workspace.Assemble(_cancel.Token), _cancel.Token);
                if (IsDisposed) return;
                if (Program.Open(output)) { LastOpenedPath = output; Close(); }
                else _status.Text = "The native editor could not open the assembled copy. Sources and the saved resolution remain intact.";
            });
            bottom.Controls.Add(close); bottom.Controls.Add(_open); layout.Controls.Add(bottom);
            Controls.Add(layout);
            FormClosing += (sender, args) => _cancel.Cancel();
            RefreshChoices();
        }

        private static ComboBox NewCombo(string name) => new ComboBox { Name = name, DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, DropDownWidth = 1200 };
        private void AddAction(string text, string name, Func<Task> action)
        {
            var button = new Button { Text = text, Name = name, AutoSize = true };
            button.Click += async (sender, args) => await RunAsync(action);
            _actions.Controls.Add(button);
        }
        public async Task AddSourceAsync(string path)
        {
            await Task.Run(() => _workspace.AddSource(path, _cancel.Token), _cancel.Token);
            if (!IsDisposed) RefreshChoices();
        }
        public async Task LoadResolutionAsync(string path)
        {
            CompanionWorkspace loaded = await Task.Run(() => CompanionWorkspace.Load(path, _workspace.StorageRoot, _cancel.Token));
            if (IsDisposed) return;
            _workspace = loaded; RefreshChoices();
        }
        public void SaveResolution(string path)
        {
            _workspace.VerifySources(_cancel.Token);
            _workspace.Save(path);
            _workspace.Save(CompanionWorkspace.LastResolutionPath);
            _status.Text = "Resolution saved. Reopen it after restarting to verify these same source hashes and selections. " + CompanionWorkspace.ScopeNotice +
                (_workspace.AdditionalResourceNotice.Length == 0 ? "" : Environment.NewLine + _workspace.AdditionalResourceNotice);
        }
        private async Task RunAsync(Func<Task> action)
        {
            if (_busy) return;
            _busy = true; _actions.Enabled = _models.Enabled = _motions.Enabled = _bindings.Enabled = _candidates.Enabled = false; _open.Enabled = false;
            _status.Text = "Validating read-only sources and native dependencies…";
            try { await action(); }
            catch (Exception error) { if (!IsDisposed) _status.Text = "Not opened: " + error.Message; }
            finally
            {
                if (!IsDisposed)
                {
                    _busy = false; _actions.Enabled = _models.Enabled = _motions.Enabled = _bindings.Enabled = _candidates.Enabled = true;
                    TryAction(() => _open.Enabled = _workspace.Validate().Count == 0);
                }
            }
        }
        private void TryAction(Action action)
        {
            try { action(); }
            catch (Exception error) { _status.Text = error.Message; _open.Enabled = false; }
        }
        private void RefreshChoices()
        {
            _refreshing = true;
            _models.Items.Clear(); _motions.Items.Clear();
            _models.Items.AddRange(_workspace.Resources.Where(r => r.Kind == "MDL0").Cast<object>().ToArray());
            _motions.Items.AddRange(_workspace.Resources.Where(r => r.Kind == "CHR0").Cast<object>().ToArray());
            _models.SelectedItem = _workspace.Model; _motions.SelectedItem = _workspace.Motion;
            _refreshing = false;
            RefreshBindings();
        }
        private void RefreshBindings(string select = null)
        {
            _refreshing = true;
            _bindings.Items.Clear();
            foreach (string name in _workspace.RequiredTextures)
            {
                var item = new ListViewItem(name); item.SubItems.Add(_workspace.TextureStatus(name)); _bindings.Items.Add(item);
                if (name == select) item.Selected = true;
            }
            _refreshing = false;
            RefreshCandidates(); UpdateStatus();
        }
        private void RefreshCandidates()
        {
            if (_refreshing) return;
            _refreshing = true; _candidates.Items.Clear();
            if (_bindings.SelectedItems.Count == 1)
            {
                string name = _bindings.SelectedItems[0].Text;
                _candidates.Items.AddRange(_workspace.TextureCandidates(name).Cast<object>().ToArray());
                if (_workspace.TextureSelections.TryGetValue(name, out CompanionResource selected)) _candidates.SelectedItem = selected;
            }
            _refreshing = false;
        }
        private void UpdateStatus()
        {
            var errors = _workspace.Validate();
            _status.Text = (errors.Count == 0 ? "Dependencies explicitly resolved. " : string.Join("  ", errors)) + Environment.NewLine + CompanionWorkspace.ScopeNotice +
                (_workspace.AdditionalResourceNotice.Length == 0 ? "" : Environment.NewLine + _workspace.AdditionalResourceNotice);
            _open.Enabled = !_busy && errors.Count == 0;
        }
    }
}
