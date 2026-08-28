using System;
using System.Drawing;
using System.Windows.Forms;

namespace CupheadMusicPlayer
{
    /// <summary>
    /// Grid column that hosts a TrackBar + "Global" checkbox editing control for
    /// per-scene volume. Value is an int: -1 means "use global volume", otherwise 0-100.
    /// </summary>
    public class DataGridViewTrackBarColumn : DataGridViewColumn
    {
        public DataGridViewTrackBarColumn()
        {
            ValueType = typeof(int);
            CellTemplate = new DataGridViewTrackBarCell();
        }

        public override DataGridViewCell CellTemplate
        {
            get => base.CellTemplate;
            set
            {
                if (value != null &&
                    !value.GetType().IsAssignableFrom(typeof(DataGridViewTrackBarCell)))
                    throw new InvalidCastException("Cell template must be a DataGridViewTrackBarCell.");
                base.CellTemplate = value;
            }
        }
    }

    public class DataGridViewTrackBarCell : DataGridViewTextBoxCell
    {
        public override Type EditType => typeof(TrackBarEditingControl);

        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue,
            DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            if (DataGridView?.EditingControl is TrackBarEditingControl ctl)
                ctl.EditingControlValue = Value;
        }
    }

    public class TrackBarEditingControl : Control, IDataGridViewEditingControl
    {
        private readonly TrackBar trackBar = new TrackBar { Minimum = 0, Maximum = 100, TickStyle = TickStyle.None };
        private readonly CheckBox globalCheck = new CheckBox { Text = "Global", TextAlign = ContentAlignment.MiddleLeft };

        private DataGridView dataGridView;
        private bool valueChanged;
        private int rowIndex;
        private event EventHandler editingControlValueChanged;

        public TrackBarEditingControl()
        {
            BackColor = Theme.Back;
            ForeColor = Theme.Fore;

            globalCheck.AutoSize = false;
            globalCheck.BackColor = Theme.Back;
            globalCheck.ForeColor = Theme.Fore;
            globalCheck.CheckedChanged += (s, e) => NotifyChanged();

            trackBar.BackColor = Theme.Back;
            trackBar.ForeColor = Theme.Accent;
            trackBar.ValueChanged += (s, e) => NotifyChanged();

            Controls.Add(globalCheck);
            Controls.Add(trackBar);
            LayoutControls();
        }

        private void LayoutControls()
        {
            globalCheck.Left = 2;
            globalCheck.Width = 58;
            globalCheck.Top = Math.Max(0, (Height - 20) / 2);
            globalCheck.Height = 20;

            trackBar.Left = globalCheck.Right + 2;
            trackBar.Width = Math.Max(20, Width - trackBar.Left - 4);
            trackBar.Height = 24;
            trackBar.Top = Math.Max(0, (Height - trackBar.Height) / 2);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutControls();
        }

        private void NotifyChanged()
        {
            valueChanged = true;
            dataGridView?.NotifyCurrentCellDirty(true);
            editingControlValueChanged?.Invoke(this, EventArgs.Empty);
        }

        // ---- IDataGridViewEditingControl ----
        public DataGridView EditingControlDataGridView { get => dataGridView; set => dataGridView = value; }
        public int EditingControlRowIndex { get => rowIndex; set => rowIndex = value; }
        public Cursor EditingPanelCursor => Cursor;
        public bool RepositionEditingControlOnValueChange => true;
        public event EventHandler EditingControlValueChanged
        {
            add { editingControlValueChanged += value; }
            remove { editingControlValueChanged -= value; }
        }
        bool IDataGridViewEditingControl.EditingControlValueChanged
        {
            get => valueChanged;
            set => valueChanged = value;
        }

        public object EditingControlValue
        {
            get => globalCheck.Checked ? -1 : trackBar.Value;
            set
            {
                if (value is int v)
                {
                    globalCheck.Checked = v < 0;
                    trackBar.Value = Math.Max(0, Math.Min(100, v));
                }
            }
        }

        public object EditingControlFormattedValue
        {
            get => ((int)EditingControlValue).ToString();
            set { }
        }

        public void PrepareEditingControlForEdit(bool selectAll) { }
        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey) => false;
        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
            => ((int)EditingControlValue).ToString();
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) { }
    }
}
