using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit
{
    /// <summary>
    /// NumericUpDown for a ToolStrip that the Visual Studio designer can create and edit
    /// (a plain ToolStripControlHost has no parameterless constructor).
    /// </summary>
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    [DefaultEvent(nameof(ValueChanged))]
    public class ToolStripNumericUpDown : ToolStripControlHost
    {
        public ToolStripNumericUpDown() : base(new NumericUpDown { Width = 48 })
        {
            Margin = new Padding(1, 1, 3, 1);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NumericUpDown NumericUpDown => (NumericUpDown)Control;

        [DefaultValue(typeof(decimal), "0")]
        public decimal Minimum
        {
            get => NumericUpDown.Minimum;
            set => NumericUpDown.Minimum = value;
        }

        [DefaultValue(typeof(decimal), "100")]
        public decimal Maximum
        {
            get => NumericUpDown.Maximum;
            set => NumericUpDown.Maximum = value;
        }

        [DefaultValue(typeof(decimal), "0")]
        public decimal Value
        {
            get => NumericUpDown.Value;
            set => NumericUpDown.Value = Math.Max(NumericUpDown.Minimum, Math.Min(NumericUpDown.Maximum, value));
        }

        public event EventHandler ValueChanged
        {
            add => NumericUpDown.ValueChanged += value;
            remove => NumericUpDown.ValueChanged -= value;
        }
    }
}
