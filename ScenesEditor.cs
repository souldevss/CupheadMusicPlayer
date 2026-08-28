using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CupheadMusicPlayer.Engine;

namespace CupheadMusicPlayer
{
    /// <summary>
    /// Editor for the per-scene music configuration. Every row corresponds to a
    /// SceneEntry: a friendly name (carrying its scene IDs), an optional file
    /// override, an optional volume override, and a preview button.
    /// </summary>
    public class ScenesEditor : Form
    {
        private readonly List<SceneEntry> tracks;
        private readonly DataGridView grid = new DataGridView();
        private readonly Engine.MusicPlayer previewPlayer = new Engine.MusicPlayer();

        private const int ColName = 0;
        private const int ColFile = 1;
        private const int ColFileBrowse = 2;
        private const int ColVolume = 3;
        private const int ColPreview = 4;

        // The edited, persisted entries (same list the caller passed in).
        public List<SceneEntry> Tracks => tracks;

        public ScenesEditor(List<SceneEntry> tracks)
        {
            this.tracks = tracks ?? new List<SceneEntry>();

            Text = "Edit Scenes";
            Width = 920;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;

            BuildUi();
            Theme.ApplyTo(this);

            ReloadGrid();
        }

        private void BuildUi()
        {
            var toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(6, 4, 6, 4),
                BackColor = Theme.Back
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var btnAdd = new Button { Text = "Add scene", AutoSize = false, Width = 90, Height = 26 };
            Theme.Style(btnAdd);
            btnAdd.Click += (s, e) => AddRow();

            var btnRemove = new Button { Text = "Remove", AutoSize = false, Width = 80, Height = 26 };
            Theme.Style(btnRemove);
            btnRemove.Click += (s, e) => RemoveSelected();

            var btnUp = new Button { Text = "▲", AutoSize = false, Width = 32, Height = 26 };
            Theme.Style(btnUp);
            btnUp.Click += (s, e) => MoveRow(-1);

            var btnDown = new Button { Text = "▼", AutoSize = false, Width = 32, Height = 26 };
            Theme.Style(btnDown);
            btnDown.Click += (s, e) => MoveRow(1);

            toolbar.Controls.Add(btnAdd, 0, 0);
            toolbar.Controls.Add(btnRemove, 1, 0);
            toolbar.Controls.Add(btnUp, 2, 0);
            toolbar.Controls.Add(btnDown, 3, 0);

            var hint = new Label
            {
                Text = "Pick a scene, a file, and set the volume. Use the arrows to reorder.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Theme.Style(hint);
            toolbar.Controls.Add(hint, 4, 0);

            // Grid
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EditMode = DataGridViewEditMode.EditOnEnter;
            Theme.Style(grid);

            // Name combo
            var colName = new DataGridViewComboBoxColumn
            {
                HeaderText = "Scene",
                DataSource = SceneCatalog.All,
                DisplayMember = "Name",
                ValueMember = "Name"
            };
            colName.FillWeight = 34;

            var colFile = new DataGridViewTextBoxColumn
            {
                HeaderText = "Music file",
                FillWeight = 44
            };

            var colFileBrowse = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Text = "...",
                UseColumnTextForButtonValue = true,
                Width = 32,
                FillWeight = 5
            };

            var colVolume = new DataGridViewTrackBarColumn
            {
                HeaderText = "Volume",
                FillWeight = 34
            };

            var colPreview = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Text = "Preview",
                UseColumnTextForButtonValue = true,
                Width = 70,
                FillWeight = 8
            };

            grid.Columns.Add(colName);
            grid.Columns.Add(colFile);
            grid.Columns.Add(colFileBrowse);
            grid.Columns.Add(colVolume);
            grid.Columns.Add(colPreview);

            grid.CellValueChanged += Grid_CellValueChanged;
            grid.CellContentClick += Grid_CellContentClick;
            grid.CellFormatting += Grid_CellFormatting;

            // Buttons row
            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = Theme.Back
            };
            var btnCancel = new Button { Text = "Cancel", AutoSize = false, Width = 90, Height = 28, DialogResult = DialogResult.Cancel };
            Theme.Style(btnCancel);
            var btnOk = new Button { Text = "OK", AutoSize = false, Width = 90, Height = 28, DialogResult = DialogResult.OK };
            Theme.Style(btnOk);
            btnRow.Controls.Add(btnCancel);
            btnRow.Controls.Add(btnOk);

            CancelButton = btnCancel;
            AcceptButton = btnOk;
            btnOk.Click += (s, e) => CommitGrid();

            Controls.Add(grid);
            Controls.Add(toolbar);
            Controls.Add(btnRow);

            // Grid sits between toolbar (top) and button row (bottom).
            grid.BringToFront();
        }

        private void ReloadGrid()
        {
            grid.Rows.Clear();
            foreach (var entry in tracks)
                AddRow(entry);
        }

        private void AddRow(SceneEntry entry = null)
        {
            var e = entry ?? new SceneEntry { Name = "Inkwell Isle 1" };
            if (e.SceneIds == null || e.SceneIds.Count == 0)
            {
                var cat = SceneCatalog.All.FirstOrDefault(x => string.Equals(x.Name, e.Name, StringComparison.OrdinalIgnoreCase));
                e.SceneIds = cat != null ? new List<string>(cat.SceneIds) : new List<string>();
            }

            // Keep the backing list in sync with the grid rows so entries persist.
            if (!tracks.Contains(e))
                tracks.Add(e);

            int i = grid.Rows.Add();
            grid.Rows[i].Tag = e;
            grid.Rows[i].Cells[ColName].Value = e.Name;
            grid.Rows[i].Cells[ColFile].Value = e.File ?? string.Empty;
            grid.Rows[i].Cells[ColVolume].Value = e.Volume < 0 ? -1 : e.Volume;
        }

        private void RemoveSelected()
        {
            if (grid.SelectedRows.Count == 0) return;
            var row = grid.SelectedRows[0];
            if (row.Tag is SceneEntry e)
            {
                tracks.Remove(e);
                if (string.Equals(previewPlayer.CurrentFile,
                        ResolvePreviewPath(e), StringComparison.OrdinalIgnoreCase))
                    previewPlayer.Stop();
            }
            grid.Rows.Remove(row);
        }

        private void MoveRow(int delta)
        {
            if (grid.SelectedRows.Count == 0) return;
            int i = grid.SelectedRows[0].Index;
            int j = i + delta;
            if (j < 0 || j >= grid.Rows.Count) return;

            var r1 = grid.Rows[i];
            var r2 = grid.Rows[j];

            // Swap the underlying entries (tags) first, then the cell values,
            // so CellValueChanged writes into the correct entry objects.
            object tag1 = r1.Tag;
            r1.Tag = r2.Tag;
            r2.Tag = tag1;

            object name1 = r1.Cells[ColName].Value;
            object file1 = r1.Cells[ColFile].Value;
            object vol1 = r1.Cells[ColVolume].Value;

            r1.Cells[ColName].Value = r2.Cells[ColName].Value;
            r1.Cells[ColFile].Value = r2.Cells[ColFile].Value;
            r1.Cells[ColVolume].Value = r2.Cells[ColVolume].Value;

            r2.Cells[ColName].Value = name1;
            r2.Cells[ColFile].Value = file1;
            r2.Cells[ColVolume].Value = vol1;

            // Keep the backing list in sync with the new order.
            if (r1.Tag is SceneEntry a && r2.Tag is SceneEntry b)
            {
                int ia = tracks.IndexOf(a);
                int ib = tracks.IndexOf(b);
                if (ia >= 0 && ib >= 0)
                {
                    tracks[ia] = b;
                    tracks[ib] = a;
                }
            }

            grid.ClearSelection();
            grid.Rows[j].Selected = true;
            grid.CurrentCell = grid.Rows[j].Cells[ColName];
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var row = grid.Rows[e.RowIndex];
            if (!(row.Tag is SceneEntry entry)) return;

            if (e.ColumnIndex == ColName)
            {
                var val = row.Cells[ColName].Value as string;
                entry.Name = val ?? string.Empty;
                var cat = SceneCatalog.All.FirstOrDefault(x => string.Equals(x.Name, entry.Name, StringComparison.OrdinalIgnoreCase));
                entry.SceneIds = cat != null ? new List<string>(cat.SceneIds) : new List<string>();
            }
            else if (e.ColumnIndex == ColFile)
            {
                entry.File = row.Cells[ColFile].Value as string ?? string.Empty;
            }
            else if (e.ColumnIndex == ColVolume)
            {
                entry.Volume = row.Cells[ColVolume].Value is int v ? v : -1;
            }
        }

        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            var row = grid.Rows[e.RowIndex];
            if (!(row.Tag is SceneEntry entry)) return;

            if (e.ColumnIndex == ColFileBrowse)
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Audio files (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|All files (*.*)|*.*";
                    if (!string.IsNullOrWhiteSpace(entry.File) && File.Exists(entry.File))
                        dlg.InitialDirectory = Path.GetDirectoryName(entry.File);
                    else
                        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        // Keep the full path (folder + file) so it stays accurate.
                        entry.File = dlg.FileName;
                        row.Cells[ColFile].Value = entry.File;
                    }
                }
            }
            else if (e.ColumnIndex == ColPreview)
            {
                TogglePreview(row);
            }
        }

        private void TogglePreview(DataGridViewRow row)
        {
            if (!(row.Tag is SceneEntry entry)) return;

            CommitGrid();

            string path = ResolvePreviewPath(entry);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show(this, "No valid music file to preview for this scene.",
                    "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Clicking the same preview row again stops it.
            if (previewPlayer.IsPlaying &&
                string.Equals(previewPlayer.CurrentFile, path, StringComparison.OrdinalIgnoreCase))
            {
                previewPlayer.Stop();
                return;
            }

            previewPlayer.Volume = entry.Volume >= 0
                ? entry.Volume / 100f
                : 0.75f;

            try { previewPlayer.PlayLooping(path, 0); }
            catch { previewPlayer.Stop(); }
        }

        private string ResolvePreviewPath(SceneEntry entry)
        {
            return string.IsNullOrWhiteSpace(entry.File) ? string.Empty : entry.File;
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == ColVolume && e.RowIndex >= 0)
            {
                object raw = e.Value;
                int v = raw is int i ? i : -1;
                if (v < 0)
                {
                    e.Value = "Global";
                    e.FormattingApplied = true;
                }
            }
        }

        private void CommitGrid()
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow || !(row.Tag is SceneEntry entry)) continue;
                entry.Name = row.Cells[ColName].Value as string ?? string.Empty;
                var cat = SceneCatalog.All.FirstOrDefault(x => string.Equals(x.Name, entry.Name, StringComparison.OrdinalIgnoreCase));
                entry.SceneIds = cat != null ? new List<string>(cat.SceneIds) : new List<string>();
                entry.File = row.Cells[ColFile].Value as string ?? string.Empty;
                entry.Volume = row.Cells[ColVolume].Value is int v ? v : -1;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            previewPlayer.Dispose();
            base.OnFormClosing(e);
        }
    }
}
