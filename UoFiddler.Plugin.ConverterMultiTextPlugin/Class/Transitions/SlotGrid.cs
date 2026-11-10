using System;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.Transitions
{
    /// <summary>Shows the 12 transition pieces in two rows (A_ pieces above, B_ pieces below).</summary>
    internal sealed class SlotGrid : Control
    {
        private int _selected;

        public SlotGrid()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.Gainsboro;
        }

        /// <summary>Image of a slot (not disposed by the grid).</summary>
        public Func<int, Image> ImageProvider { get; set; }

        /// <summary>Draw the images checkered (for diamonds with transparent corners).</summary>
        public bool Checker { get; set; }

        public event EventHandler SelectedChanged;

        public int Selected
        {
            get => _selected;
            set
            {
                int v = Math.Max(0, Math.Min(TransitionMasks.Slots.Length - 1, value));
                if (v == _selected)
                {
                    return;
                }
                _selected = v;
                Invalidate();
                SelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private Rectangle Cell(int index, out Rectangle imageBox)
        {
            int cols = 6;
            int cw = Math.Max(40, (ClientSize.Width - 8) / cols);
            int ch = Math.Max(56, (ClientSize.Height - 8) / 2);
            int size = Math.Min(cw - 8, ch - 26);
            var cell = new Rectangle(4 + (index % cols) * cw, 4 + (index / cols) * ch, cw, ch);
            imageBox = new Rectangle(cell.X + (cw - size) / 2, cell.Y + 4, size, size);
            return cell;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            for (int i = 0; i < TransitionMasks.Slots.Length; i++)
            {
                Rectangle cell = Cell(i, out Rectangle box);
                if (i == _selected)
                {
                    using (var b = new SolidBrush(Color.FromArgb(0, 122, 204)))
                    {
                        g.FillRectangle(b, Rectangle.Inflate(cell, -2, -2));
                    }
                }
                if (Checker)
                {
                    CutterImaging.DrawChecker(g, box);
                }
                else
                {
                    g.FillRectangle(Brushes.Black, box);
                }
                Image img = null;
                try
                {
                    img = ImageProvider?.Invoke(i);
                    if (img != null)
                    {
                        CutterImaging.DrawFitted(g, img, box, true);
                    }
                }
                catch (Exception)
                {
                    // image in use – skip
                }
                if (img == null)
                {
                    TextRenderer.DrawText(g, "–", Font, box, Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                TransitionSlot slot = TransitionMasks.Slots[i];
                TextRenderer.DrawText(g, slot.Name, Font, new Rectangle(cell.X, box.Bottom + 2, cell.Width, 18), slot.IsA ? Color.LightGreen : Color.SandyBrown,
                    TextFormatFlags.HorizontalCenter);
            }
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < TransitionMasks.Slots.Length; i++)
            {
                if (Cell(i, out _).Contains(p))
                {
                    return i;
                }
            }
            return -1;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int i = HitTest(e.Location);
            if (i >= 0)
            {
                Selected = i;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Left: Selected = (_selected + 11) % 12; break;
                case Keys.Right: Selected = (_selected + 1) % 12; break;
                case Keys.Up:
                case Keys.Down: Selected = (_selected + 6) % 12; break;
            }
        }
    }
}
