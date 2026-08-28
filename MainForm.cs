using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace CupheadMusicPlayer
{
    public class MainForm : Form
    {
        private readonly Engine.SceneDetection sceneDetection = new Engine.SceneDetection();
        private readonly Engine.MusicPlayer musicPlayer = new Engine.MusicPlayer();

        private readonly TrackBar volumeSlider = new TrackBar();        private readonly Label lblVolumeValue = new Label();
        private readonly Label lblPolling = new Label();
        private readonly NumericUpDown nudPolling = new NumericUpDown();
        private readonly Label lblStatus = new Label();
        private readonly Label lblScene = new Label();
        private readonly Button btnStartStop = new Button();
        private readonly Button btnEditScenes = new Button();
        private readonly Button btnTheme = new Button();
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private readonly NotifyIcon trayIcon = new NotifyIcon();
        private readonly ContextMenuStrip trayMenu = new ContextMenuStrip();
        private readonly List<Control> centeredRows = new List<Control>();

        private bool running;
        private string lastScene;
        private DateTime startedAt = DateTime.MinValue;
        private DateTime lastIdleWarning = DateTime.MinValue;
        private Settings settings = new Settings();

        private class Settings
        {
            public int Volume { get; set; } = 100;
            public int PollingHz { get; set; } = 4;
            public string ThemeMode { get; set; } = "Dark";
            public List<SceneEntry> SceneEntries { get; set; } = new List<SceneEntry>();
        }

        public MainForm()
        {
            Text = "Cuphead Music Player";
            Width = 460;
            Height = 340;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new System.Drawing.Size(420, 300);

            BuildUi();
            LoadSettings();
            ApplySavedTheme();
            ApplyPollingRate(); // also fixes timer.Interval from the loaded Hz
            SetupTray();

            timer.Tick += Timer_Tick;
        }

        private void SetupTray()
        {
            trayMenu.Items.Add("Restore", null, (s, e) => RestoreFromTray());
            trayMenu.Items.Add("Exit", null, (s, e) => Close());

            trayIcon.Icon = System.Drawing.SystemIcons.Application;
            trayIcon.Text = "Cuphead Music Player";
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void BuildUi()
        {
            // Volume row
            var lblVol = new Label { Text = "Volume:", AutoSize = true, Margin = new Padding(0, 13, 8, 0) };
            Theme.Style(lblVol);
            volumeSlider.Width = 250;
            volumeSlider.Minimum = 0;
            volumeSlider.Maximum = 100;
            volumeSlider.TickStyle = TickStyle.None;
            volumeSlider.Value = 100;
            Theme.Style(volumeSlider);
            volumeSlider.Scroll += (s, e) => UpdateVolume();
            lblVolumeValue.AutoSize = true;
            lblVolumeValue.Text = "100%";
            lblVolumeValue.Margin = new Padding(8, 13, 0, 0);
            Theme.Style(lblVolumeValue);

            var rowVolume = MakeRow();
            rowVolume.Controls.Add(lblVol);
            rowVolume.Controls.Add(volumeSlider);
            rowVolume.Controls.Add(lblVolumeValue);

            // Buttons row
            btnStartStop.Width = 120;
            btnStartStop.Text = "Start";
            btnStartStop.Margin = new Padding(0);
            Theme.Style(btnStartStop);
            btnStartStop.Click += (s, e) => ToggleRunning();

            btnEditScenes.Width = 122;
            btnEditScenes.Text = "Edit Scenes...";
            btnEditScenes.Margin = new Padding(10, 0, 0, 0);
            Theme.Style(btnEditScenes);
            btnEditScenes.Click += (s, e) => OpenSceneEditor();

            var rowButtons = MakeRow();
            rowButtons.Controls.Add(btnStartStop);
            rowButtons.Controls.Add(btnEditScenes);

            // Polling rate row
            lblPolling.Text = "Poll rate (Hz):";
            lblPolling.AutoSize = true;
            lblPolling.Margin = new Padding(0, 5, 8, 0);
            Theme.Style(lblPolling);
            nudPolling.Width = 60;
            nudPolling.Minimum = 1;
            nudPolling.Maximum = 60;
            nudPolling.Increment = 1;
            nudPolling.Value = 4;
            nudPolling.Margin = new Padding(0, 3, 0, 0);
            Theme.Style(nudPolling);
            nudPolling.ValueChanged += (s, e) => UpdatePollingRate();

            var rowPoll = MakeRow();
            rowPoll.Controls.Add(lblPolling);
            rowPoll.Controls.Add(nudPolling);

            // Status + scene (centered in the flow so it's easy to see)
            lblStatus.AutoSize = true;
            lblStatus.Text = "Stopped.";
            Theme.Style(lblStatus);
            lblStatus.ForeColor = Theme.Accent;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblScene.AutoSize = true;
            lblScene.Text = "Waiting for Cuphead...";
            Theme.Style(lblScene);
            lblScene.ForeColor = Theme.Fore;

            centeredRows.Add(rowVolume);
            centeredRows.Add(rowButtons);
            centeredRows.Add(rowPoll);
            centeredRows.Add(lblStatus);
            centeredRows.Add(lblScene);

            // Theme toggle (top-right corner)
            btnTheme.Size = new System.Drawing.Size(28, 24);
            btnTheme.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnTheme.Click += (s, e) => ToggleTheme();
            Theme.Style(btnTheme);

            foreach (var row in centeredRows)
                Controls.Add(row);
            Controls.Add(btnTheme);

            CenterLayout();
        }

        private FlowLayoutPanel MakeRow()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0),
                BackColor = Theme.Back
            };
        }

        private void CenterLayout()
        {
            PerformLayout();

            int cx = ClientSize.Width / 2;
            int y = 46;
            foreach (var row in centeredRows)
            {
                row.Location = new System.Drawing.Point(cx - row.Width / 2, y);
                y += row.Height + 20;
            }

            btnTheme.Location = new System.Drawing.Point(ClientSize.Width - btnTheme.Width - 10, 10);
        }

        private string SettingsPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CupheadMusicPlayer", "settings.json");

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
            }
            catch { settings = new Settings(); }

            volumeSlider.Value = Math.Max(0, Math.Min(100, settings.Volume));
            musicPlayer.Volume = volumeSlider.Value / 100f;
            lblVolumeValue.Text = volumeSlider.Value + "%";
            nudPolling.Value = Math.Max(1, Math.Min(60, settings.PollingHz));
        }

        private void ApplyPollingRate()
        {
            int hz = (int)nudPolling.Value;
            timer.Interval = Math.Max(1, 1000 / hz);
        }

        private void UpdatePollingRate()
        {
            ApplyPollingRate();
            SaveSettings();
        }

        private void ApplySavedTheme()
        {
            Theme.Toggle(settings.ThemeMode == "Light" ? ThemeMode.Light : ThemeMode.Dark);
            Theme.ApplyTo(this);
            ApplyStatusLook();
            ApplyThemeIcon();
        }

        private void ToggleTheme()
        {
            Theme.Toggle();
            Theme.ApplyTo(this);
            ApplyStatusLook();
            ApplyThemeIcon();
            SaveSettings();
        }

        private void ApplyStatusLook()
        {
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblStatus.ForeColor = Theme.Accent;
            lblScene.ForeColor = Theme.Fore;
        }

        private void ApplyThemeIcon()
        {
            btnTheme.Text = Theme.Mode == ThemeMode.Dark ? "☼" : "☾";
        }

        private void SaveSettings()
        {
            settings.Volume = volumeSlider.Value;
            settings.PollingHz = (int)nudPolling.Value;
            settings.ThemeMode = Theme.Mode == ThemeMode.Dark ? "Dark" : "Light";

            try
            {
                var dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings));
            }
            catch { }
        }

        private void UpdateVolume()
        {
            float v = volumeSlider.Value / 100f;
            musicPlayer.Volume = v;
            lblVolumeValue.Text = volumeSlider.Value + "%";
            SaveSettings();
        }

        private void ToggleRunning()
        {
            if (running) Stop();
            else Start();
        }

        private void Start()
        {
            if (running) return;

            running = true;
            startedAt = DateTime.Now;
            lastIdleWarning = DateTime.MinValue;
            btnStartStop.Text = "Stop";
            lblStatus.Text = "Running. Waiting for Cuphead...";
            SaveSettings();
            timer.Start();
        }

        private void Stop()
        {
            running = false;
            startedAt = DateTime.MinValue;
            lastIdleWarning = DateTime.MinValue;
            timer.Stop();
            btnStartStop.Text = "Start";
            lblStatus.Text = "Stopped.";
            lblScene.Text = "Current scene: —";
            musicPlayer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!running) return;

            sceneDetection.Update();

            string currentScene = sceneDetection.CurrentScene;

            if (string.IsNullOrWhiteSpace(currentScene))
            {
                // No active scene (Cuphead not running / loading).
                musicPlayer.Stop();
                MaybeWarnIdle();
                if (sceneDetection.IsHooked)
                {
                    lblStatus.Text = "Running. Cuphead hooked but scene not detected...";
                    lblScene.Text = "Current scene: —  " + sceneDetection.Diagnose();
                }
                else
                {
                    lblStatus.Text = "Running. Waiting for Cuphead...";
                    lblScene.Text = "Current scene: —";
                }
                return;
            }

            lblStatus.Text = "Running.";
            lblScene.Text = $"Current scene: {currentScene}";

            // A scene is active, so reset the idle-reminder timer.
            lastIdleWarning = DateTime.Now;

            // Resolve the track for this scene through the user's custom entries.
            SceneEntry entry = FindEntryForScene(currentScene);
            if (entry == null)
            {
                lastScene = currentScene;
                musicPlayer.Stop();
                return;
            }

            // File: each entry must hold a full path to the track file.
            string musicFile = entry.File;
            if (string.IsNullOrWhiteSpace(musicFile) || !File.Exists(musicFile))
            {
                lastScene = currentScene;
                musicPlayer.Stop();
                return;
            }

            // Per-scene volume override, else the global volume.
            musicPlayer.Volume = entry.Volume >= 0
                ? entry.Volume / 100f
                : volumeSlider.Value / 100f;

            // Retrying the same level: lastScene unchanged (scene went away and
            // came back) -> near-instant restart with a short fade and no delay.
            bool retrying = !string.IsNullOrEmpty(lastScene) &&
                string.Equals(lastScene, currentScene, StringComparison.OrdinalIgnoreCase);

            // Otherwise: if the song we are leaving is a platformer level, hold it
            // (keep playing) a little longer before fading out.
            int startDelay = retrying ? 0 : 1000;
            int holdMs = retrying ? 0 : (IsPlatformerScene(lastScene) ? 3000 : 0);
            int fadeMs = retrying ? 1000 : 1500;

            try { musicPlayer.PlayLooping(musicFile, startDelay, holdMs, fadeMs); }
            catch { musicPlayer.Stop(); }

            lastScene = currentScene;
        }

        // Warn the user when the player has been running for a while without
        // detecting Cuphead, so it doesn't sit forgotten in the background.
        private void MaybeWarnIdle()
        {
            if (startedAt == DateTime.MinValue) return;

            // Only nag once the player has been up for at least 10 minutes.
            if (DateTime.Now - startedAt < TimeSpan.FromMinutes(10)) return;

            // Then remind again every 10 minutes until the user stops it.
            if (DateTime.Now - lastIdleWarning < TimeSpan.FromMinutes(10)) return;

            lastIdleWarning = DateTime.Now;
            ShowIdleReminder();
        }

        private void ShowIdleReminder()
        {
            var elapsed = (int)(DateTime.Now - startedAt).TotalMinutes;
            string message =
                $"Cuphead Music Player has been running for about {elapsed} minutes " +
                "but hasn't detected Cuphead yet.\n\n" +
                "It's still running in the background. Would you like to keep it going?";

            ShowTrayBalloon("Still running...", message);

            var result = MessageBox.Show(this, message,
                "Cuphead Music Player",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                Stop();
        }

        private void ShowTrayBalloon(string title, string text)
        {
            try
            {
                trayIcon.ShowBalloonTip(5000, title, text, ToolTipIcon.Info);
            }
            catch { }
        }

        private SceneEntry FindEntryForScene(string scene)
        {
            if (string.IsNullOrEmpty(scene)) return null;
            foreach (var entry in settings.SceneEntries)
            {
                if (entry.SceneIds == null) continue;
                foreach (var id in entry.SceneIds)
                {
                    if (string.Equals(id, scene, StringComparison.OrdinalIgnoreCase))
                        return entry;
                }
            }
            return null;
        }

        private void OpenSceneEditor()
        {
            using (var editor = new ScenesEditor(settings.SceneEntries))
            {
                if (editor.ShowDialog(this) == DialogResult.OK)
                {
                    settings.SceneEntries = editor.Tracks;
                    SaveSettings();
                }
            }
        }

        private static bool IsPlatformerScene(string scene)
        {
            if (string.IsNullOrEmpty(scene))
                return false;

            return
                string.Equals(scene, "scene_level_platforming_1_1F", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scene, "scene_level_platforming_1_2F", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scene, "scene_level_platforming_2_1F", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scene, "scene_level_platforming_2_2F", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scene, "scene_level_platforming_3_1F", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scene, "scene_level_platforming_3_2F", StringComparison.OrdinalIgnoreCase);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Minimizing hides the window to the tray so the player keeps running
            // in the background without cluttering the taskbar.
            if (WindowState == FormWindowState.Minimized)
                Hide();
            else
                CenterLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer.Stop();
            SaveSettings();
            musicPlayer.Dispose();
            sceneDetection.Dispose();
            trayIcon.Visible = false;
            trayIcon.Dispose();
            base.OnFormClosing(e);
        }
    }
}
