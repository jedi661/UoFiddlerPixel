// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>Item for terrain combo / list boxes.</summary>
    internal sealed class TerrainChoice
    {
        public int Id { get; }
        public string Name { get; }
        public Color Color { get; }

        public TerrainChoice(int id, string name, Color color)
        {
            Id = id;
            Name = name;
            Color = color;
        }

        public override string ToString() => Name;

        public static List<TerrainChoice> FromPalette(TerrainPalette palette, bool includeAll = false)
        {
            List<TerrainChoice> list = new List<TerrainChoice>();
            if (includeAll)
            {
                list.Add(new TerrainChoice(-1, "(all terrains)", Color.Transparent));
            }

            list.AddRange(palette.DefinedIds.Select(id => new TerrainChoice(id, palette.DisplayName(id), palette.Colors[id])).OrderBy(c => c.Id));
            return list;
        }

        /// <summary>Owner draw handler for ComboBox / ListBox items with a colour swatch.</summary>
        public static void DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0)
            {
                return;
            }

            object item = sender is ComboBox combo ? combo.Items[e.Index] : ((ListBox)sender).Items[e.Index];
            Rectangle swatch = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 18, e.Bounds.Height - 4);
            if (item is TerrainChoice choice && choice.Id >= 0)
            {
                using (SolidBrush brush = new SolidBrush(choice.Color))
                {
                    e.Graphics.FillRectangle(brush, swatch);
                }

                e.Graphics.DrawRectangle(Pens.Gray, swatch);
            }

            TextRenderer.DrawText(e.Graphics, item?.ToString() ?? string.Empty, e.Font, new Point(swatch.Right + 4, e.Bounds.Y + 1), e.ForeColor);
            e.DrawFocusRectangle();
        }

        public static void Select(ComboBox combo, int id)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is TerrainChoice c && c.Id == id)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        public static int SelectedId(ComboBox combo, int fallback)
        {
            return combo.SelectedItem is TerrainChoice c ? c.Id : fallback;
        }
    }

    /// <summary>Shows terrain IDs as "[0B] Forest" drop down in the PropertyGrid.</summary>
    internal sealed class TerrainIdConverter : Int32Converter
    {
        /// <summary>Set by the Map Maker after the palette was loaded.</summary>
        public static TerrainPalette Palette { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => Palette != null;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => false;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Palette == null ? new List<int>() : Palette.DefinedIds.ToList());
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is int id && Palette != null && id >= 0 && id < 256)
            {
                return Palette.DisplayName(id);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string text)
            {
                text = text.Trim();
                // "[0B] Forest" -> 0x0B
                if (text.StartsWith("[") && text.IndexOf(']') == 3 && int.TryParse(text.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hex))
                {
                    return hex;
                }

                if (Palette != null)
                {
                    foreach (int id in Palette.DefinedIds)
                    {
                        if (string.Equals(Palette.Names[id], text, StringComparison.OrdinalIgnoreCase))
                        {
                            return id;
                        }
                    }
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <summary>
    /// Undo / redo for the editor. The map is split into 64x64 chunks; before a chunk is changed the first time
    /// in a stroke its content is saved. Memory stays small even on 7168x4096 maps.
    /// </summary>
    internal sealed class UndoBuffer
    {
        private const int ChunkSize = 64;

        private sealed class Snapshot
        {
            public readonly Dictionary<int, (byte[] Terrain, byte[] Altitude)> Chunks = new Dictionary<int, (byte[], byte[])>();
        }

        private readonly MapDocument _doc;
        private readonly int _chunksX;
        private readonly LinkedList<Snapshot> _undo = new LinkedList<Snapshot>();
        private readonly Stack<Snapshot> _redo = new Stack<Snapshot>();
        private Snapshot _current;

        public int Limit { get; set; } = 40;

        public UndoBuffer(MapDocument doc)
        {
            _doc = doc;
            _chunksX = (doc.Width + ChunkSize - 1) / ChunkSize;
        }

        public bool CanUndo => _undo.Count > 0;

        public bool CanRedo => _redo.Count > 0;

        public void Begin()
        {
            _current = new Snapshot();
        }

        /// <summary>Call before tile (x, y) is changed.</summary>
        public void Touch(int x, int y)
        {
            if (_current == null)
            {
                Begin();
            }

            int key = ((y / ChunkSize) * _chunksX) + (x / ChunkSize);
            if (!_current.Chunks.ContainsKey(key))
            {
                _current.Chunks[key] = Copy(key);
            }
        }

        /// <summary>Saves the whole map (for operations on all tiles).</summary>
        public void TouchAll()
        {
            for (int y = 0; y < _doc.Height; y += ChunkSize)
            {
                for (int x = 0; x < _doc.Width; x += ChunkSize)
                {
                    Touch(x, y);
                }
            }
        }

        public void End()
        {
            if (_current != null && _current.Chunks.Count > 0)
            {
                _undo.AddLast(_current);
                while (_undo.Count > Limit)
                {
                    _undo.RemoveFirst();
                }

                _redo.Clear();
            }

            _current = null;
        }

        public void Undo()
        {
            if (_undo.Count == 0)
            {
                return;
            }

            Snapshot snapshot = _undo.Last.Value;
            _undo.RemoveLast();
            _redo.Push(Swap(snapshot));
        }

        public void Redo()
        {
            if (_redo.Count == 0)
            {
                return;
            }

            _undo.AddLast(Swap(_redo.Pop()));
        }

        /// <summary>Restores the snapshot and returns the replaced content (for the other stack).</summary>
        private Snapshot Swap(Snapshot snapshot)
        {
            Snapshot replaced = new Snapshot();
            foreach (KeyValuePair<int, (byte[] Terrain, byte[] Altitude)> chunk in snapshot.Chunks)
            {
                replaced.Chunks[chunk.Key] = Copy(chunk.Key);
                Restore(chunk.Key, chunk.Value);
            }

            return replaced;
        }

        private void Bounds(int key, out int x0, out int y0, out int w, out int h)
        {
            x0 = (key % _chunksX) * ChunkSize;
            y0 = (key / _chunksX) * ChunkSize;
            w = Math.Min(ChunkSize, _doc.Width - x0);
            h = Math.Min(ChunkSize, _doc.Height - y0);
        }

        private (byte[], byte[]) Copy(int key)
        {
            Bounds(key, out int x0, out int y0, out int w, out int h);
            byte[] t = new byte[w * h];
            byte[] a = new byte[w * h];
            for (int y = 0; y < h; y++)
            {
                int src = ((y0 + y) * _doc.Width) + x0;
                Buffer.BlockCopy(_doc.Terrain, src, t, y * w, w);
                Buffer.BlockCopy(_doc.Altitude, src, a, y * w, w);
            }

            return (t, a);
        }

        private void Restore(int key, (byte[] Terrain, byte[] Altitude) data)
        {
            Bounds(key, out int x0, out int y0, out int w, out int h);
            for (int y = 0; y < h; y++)
            {
                int dst = ((y0 + y) * _doc.Width) + x0;
                Buffer.BlockCopy(data.Terrain, y * w, _doc.Terrain, dst, w);
                Buffer.BlockCopy(data.Altitude, y * w, _doc.Altitude, dst, w);
            }
        }
    }
}
