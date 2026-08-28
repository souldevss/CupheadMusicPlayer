using System.Drawing;
using System.Windows.Forms;

namespace CupheadMusicPlayer
{
    public enum ThemeMode { Dark, Light }

    /// <summary>
    /// Shared theme palette and styling helpers. Palette colors are dynamic so
    /// both forms can be re-styled when the theme is toggled at runtime.
    /// </summary>
    public static class Theme
    {
        public static ThemeMode Mode { get; private set; } = ThemeMode.Dark;

        public static Color Back { get; private set; }
        public static Color PanelBack { get; private set; }
        public static Color InputBack { get; private set; }
        public static Color ButtonBack { get; private set; }
        public static Color Fore { get; private set; }
        public static Color ForeDim { get; private set; }
        public static Color Accent { get; private set; }
        public static Color GridHeaderBack { get; private set; }
        public static Color GridBack { get; private set; }
        public static Color GridAltBack { get; private set; }
        public static Color GridLine { get; private set; }
        public static Color Border { get; private set; }

        static Theme() { Apply(Mode); }

        public static void Toggle(ThemeMode? mode = null)
        {
            Mode = mode ?? (Mode == ThemeMode.Dark ? ThemeMode.Light : ThemeMode.Dark);
            Apply(Mode);
        }

        private static void Apply(ThemeMode mode)
        {
            if (mode == ThemeMode.Dark)
            {
                Back = Color.FromArgb(30, 30, 34);
                PanelBack = Color.FromArgb(37, 37, 42);
                InputBack = Color.FromArgb(22, 22, 26);
                ButtonBack = Color.FromArgb(44, 48, 58);          // blue-ish dark gray
                Fore = Color.FromArgb(230, 230, 235);
                ForeDim = Color.FromArgb(160, 160, 168);
                Accent = Color.FromArgb(120, 180, 255);
                GridHeaderBack = Color.FromArgb(44, 44, 50);
                GridBack = Color.FromArgb(30, 30, 34);
                GridAltBack = Color.FromArgb(35, 35, 40);
                GridLine = Color.FromArgb(48, 48, 55);
                Border = Color.FromArgb(70, 70, 78);
            }
            else
            {
                Back = Color.FromArgb(243, 243, 246);
                PanelBack = Color.FromArgb(255, 255, 255);
                InputBack = Color.White;
                ButtonBack = Color.FromArgb(228, 232, 240);       // soft blue-gray
                Fore = Color.FromArgb(30, 30, 34);
                ForeDim = Color.FromArgb(110, 110, 120);
                Accent = Color.FromArgb(0, 100, 200);
                GridHeaderBack = Color.FromArgb(235, 238, 244);
                GridBack = Color.White;
                GridAltBack = Color.FromArgb(244, 246, 249);
                GridLine = Color.FromArgb(220, 224, 230);
                Border = Color.FromArgb(190, 195, 205);
            }
        }

        /// <summary>Applies the current theme to a form and every child control.</summary>
        public static void ApplyTo(Form form)
        {
            form.BackColor = Back;
            form.ForeColor = Fore;
            ApplyTree(form);
        }

        private static void ApplyTree(Control c)
        {
            switch (c)
            {
                case DataGridView g:
                    Style(g);
                    break;
                case Button b:
                    Style(b);
                    break;
                case TextBox t:
                    Style(t);
                    break;
                case NumericUpDown n:
                    Style(n);
                    break;
                case TrackBar tb:
                    Style(tb);
                    break;
                case Label l:
                    Style(l);
                    break;
                case ComboBox cb:
                    cb.BackColor = InputBack;
                    cb.ForeColor = Fore;
                    cb.FlatStyle = FlatStyle.Flat;
                    break;
                case FlowLayoutPanel _:
                case TableLayoutPanel _:
                case GroupBox _:
                case Panel _:
                    c.BackColor = Back;
                    c.ForeColor = Fore;
                    break;
            }

            // DataGridView manages its own cell styling; don't recurse into it.
            if (c is DataGridView)
                return;

            foreach (Control child in c.Controls)
                ApplyTree(child);
        }

        public static void Style(Button b)
        {
            b.BackColor = ButtonBack;
            b.ForeColor = Fore;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Border;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Lighten(ButtonBack, 0.06f);
            b.FlatAppearance.MouseDownBackColor = Darken(ButtonBack, 0.08f);
        }

        public static void Style(TextBox t)
        {
            t.BackColor = InputBack;
            t.ForeColor = Fore;
            t.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void Style(NumericUpDown n)
        {
            n.BackColor = InputBack;
            n.ForeColor = Fore;
            n.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void Style(TrackBar t)
        {
            t.BackColor = Back;
            t.ForeColor = Accent;
        }

        public static void Style(Label l)
        {
            l.ForeColor = Fore;
            l.BackColor = Back;
        }

        public static void Style(DataGridView grid)
        {
            grid.BackgroundColor = GridBack;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = GridLine;

            grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBack;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Fore;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeaderBack;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Fore;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            grid.DefaultCellStyle.BackColor = GridBack;
            grid.DefaultCellStyle.ForeColor = Fore;
            grid.DefaultCellStyle.SelectionBackColor = Lighten(GridAltBack, 0.08f);
            grid.DefaultCellStyle.SelectionForeColor = Fore;
            grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltBack;

            grid.RowHeadersVisible = false;
            grid.EnableHeadersVisualStyles = false;

            // Style every column's cell template so combo/button cells are not white.
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col is DataGridViewButtonColumn btnCol)
                {
                    btnCol.FlatStyle = FlatStyle.Flat;
                    btnCol.DefaultCellStyle.BackColor = PanelBack;
                    btnCol.DefaultCellStyle.ForeColor = Fore;
                    btnCol.DefaultCellStyle.SelectionBackColor = ButtonBack;
                    btnCol.DefaultCellStyle.SelectionForeColor = Fore;
                }
                else if (col is DataGridViewComboBoxColumn comboCol)
                {
                    comboCol.FlatStyle = FlatStyle.Flat;
                    comboCol.DefaultCellStyle.BackColor = GridBack;
                    comboCol.DefaultCellStyle.ForeColor = Fore;
                    comboCol.DefaultCellStyle.SelectionBackColor = ButtonBack;
                    comboCol.DefaultCellStyle.SelectionForeColor = Fore;
                }
                else
                {
                    col.DefaultCellStyle.BackColor = GridBack;
                    col.DefaultCellStyle.ForeColor = Fore;
                    col.DefaultCellStyle.SelectionBackColor = Lighten(GridAltBack, 0.08f);
                    col.DefaultCellStyle.SelectionForeColor = Fore;
                }
            }
        }

        private static Color Lighten(Color c, float f) => Mix(c, Color.White, f);
        private static Color Darken(Color c, float f) => Mix(c, Color.Black, f);
        private static Color Mix(Color a, Color b, float t) => Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }
}
