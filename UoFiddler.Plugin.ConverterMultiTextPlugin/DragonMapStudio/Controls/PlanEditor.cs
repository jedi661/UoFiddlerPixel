/***************************************************************************
 *
 *  * Dragon Map Studio – 2D plan of a dungeon (dungeon workshop): see and draw the floor plan by hand
 *  *
 *  *  left mouse = floor (room), right mouse = rock / void, Umschalt + left = inner wall line, Strg + left = corridor;
 *  *  brush 1 / 2 / 3 cells; the plan is then built in the style
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    [ToolboxItem(false)]
    public sealed class PlanEditor : Control
    {
        private DungeonPlan _plan;
        private bool _painting;
        private byte _paintValue;

        public PlanEditor()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(18, 18, 22);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DungeonPlan Plan
        {
            get => _plan;
            set
            {
                _plan = value;
                Invalidate();
            }
        }

        /// <summary>Brush size in cells (1–3).</summary>
        public int BrushSize { get; set; } = 1;

        /// <summary>The plan was changed by hand.</summary>
        public event EventHandler PlanEdited;

        private float CellSize => _plan == null ? 1 : Math.Max(1f, Math.Min((Width - 8) / (float)_plan.W, (Height - 8) / (float)_plan.H));

        private PointF Offset
        {
            get
            {
                float c = CellSize;
                return new PointF((Width - c * _plan.W) / 2, (Height - c * _plan.H) / 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_plan == null)
            {
                TextRenderer.DrawText(g, L.T("Noch kein Plan – 'Generieren' oder 'Leerer Plan'."), Font, new Point(10, 10), Color.Gainsboro);
                return;
            }
            float c = CellSize;
            PointF o = Offset;
            using var room = new SolidBrush(Color.FromArgb(176, 168, 150));
            using var corr = new SolidBrush(Color.FromArgb(214, 205, 180));
            using var inner = new SolidBrush(Color.FromArgb(120, 72, 40));
            using var wall = new SolidBrush(Color.FromArgb(70, 64, 60));
            for (int y = 0; y < _plan.H; y++)
            {
                for (int x = 0; x < _plan.W; x++)
                {
                    byte v = _plan.Cells[y * _plan.W + x];
                    Brush b = v == DungeonPlan.Room ? room : v == DungeonPlan.Corridor ? corr : v == DungeonPlan.Inner ? inner : _plan.Wall(x, y) ? wall : null;
                    if (b != null)
                    {
                        g.FillRectangle(b, o.X + x * c, o.Y + y * c, Math.Max(1, c - (c > 6 ? 1 : 0)), Math.Max(1, c - (c > 6 ? 1 : 0)));
                    }
                }
            }
            using var frame = new Pen(Color.FromArgb(120, 255, 255, 255));
            g.DrawRectangle(frame, o.X - 1, o.Y - 1, c * _plan.W + 1, c * _plan.H + 1);
            string help = L.T("Links = Boden · Rechts = Fels / Leere · Umschalt+Links = Innenwand · Strg+Links = Gang · Norden ist oben links (wie die Karte)");
            TextRenderer.DrawText(g, help, Font, new Point(6, Height - 20), Color.Silver);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_plan == null)
            {
                return;
            }
            _painting = true;
            _paintValue = e.Button == MouseButtons.Right ? DungeonPlan.Solid
                : (ModifierKeys & Keys.Shift) != 0 ? DungeonPlan.Inner
                : (ModifierKeys & Keys.Control) != 0 ? DungeonPlan.Corridor : DungeonPlan.Room;
            PaintAt(e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_painting)
            {
                PaintAt(e.Location);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_painting)
            {
                _painting = false;
                PlanEdited?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PaintAt(Point p)
        {
            float c = CellSize;
            PointF o = Offset;
            int cx = (int)Math.Floor((p.X - o.X) / c), cy = (int)Math.Floor((p.Y - o.Y) / c);
            int r = Math.Max(1, BrushSize);
            for (int y = cy - (r - 1) / 2; y <= cy + r / 2; y++)
            {
                for (int x = cx - (r - 1) / 2; x <= cx + r / 2; x++)
                {
                    if (x >= 1 && y >= 1 && x < _plan.W - 1 && y < _plan.H - 1)
                    {
                        _plan.Cells[y * _plan.W + x] = _paintValue;
                    }
                }
            }
            Invalidate();
        }
    }
}
