/***************************************************************************
 *
 * MultiEditor – material / tile list with export (CSV, JSON, C# table)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    public sealed class MaterialListForm : Form
    {
        private sealed class Row
        {
            public int Id;
            public string Name;
            public int Count;
            public int Deco;
            public int Hued;
            public string Role;
            public decimal Price;
        }

        private readonly List<Row> _rows;
        private readonly ListView _list;
        private readonly Label _total;
        private readonly Action<int> _selectId;
        private readonly string _designName;

        private static readonly Dictionary<int, decimal> _prices = new Dictionary<int, decimal>();

        public MaterialListForm(IList<ImportTile> tiles, string designName, Action<int> selectId)
        {
            _selectId = selectId;
            _designName = designName;
            Text = L.T("Material- und Kachelliste – ", "Material and tile list – ") + designName;
            Size = new Size(820, 560);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            _rows = tiles.Where(t => t.Id != 0x1).GroupBy(t => (int)t.Id).Select(g => new Row
            {
                Id = g.Key,
                Name = SafeName(g.Key),
                Count = g.Count(),
                Deco = g.Count(t => t.Deco),
                Hued = g.Count(t => t.Hue > 0),
                Role = StyleKnowledge.Instance.GetRole(g.Key).ToString(),
                Price = _prices.TryGetValue(g.Key, out decimal p) ? p : 0
            }).OrderByDescending(r => r.Count).ToList();

            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, LabelEdit = false };
            _list.Columns.Add("ID", 70);
            _list.Columns.Add(L.T("Dez.", "Dec"), 55);
            _list.Columns.Add("Name", 180);
            _list.Columns.Add(L.T("Anzahl", "Count"), 60, HorizontalAlignment.Right);
            _list.Columns.Add(L.T("Einrichtung", "Furniture"), 70, HorizontalAlignment.Right);
            _list.Columns.Add(L.T("mit Hue", "hued"), 60, HorizontalAlignment.Right);
            _list.Columns.Add(L.T("Rolle", "Role"), 80);
            _list.Columns.Add(L.T("Preis/Stk", "Price/pc"), 70, HorizontalAlignment.Right);
            _list.Columns.Add(L.T("Summe", "Sum"), 80, HorizontalAlignment.Right);
            _list.ColumnClick += (_, e) => SortBy(e.Column);
            _list.DoubleClick += (_, _) =>
            {
                if (_list.SelectedItems.Count > 0)
                {
                    _selectId?.Invoke(((Row)_list.SelectedItems[0].Tag).Id);
                }
            };

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(4) };
            _total = new Label { AutoSize = true, Margin = new Padding(3, 8, 12, 3), Font = new Font(Font, FontStyle.Bold) };
            bottom.Controls.Add(_total);
            bottom.Controls.Add(Btn(L.T("Preis setzen…", "Set price…"), SetPrice));
            bottom.Controls.Add(Btn(L.T("Im Multi auswählen", "Select in multi"), () =>
            {
                if (_list.SelectedItems.Count > 0)
                {
                    _selectId?.Invoke(((Row)_list.SelectedItems[0].Tag).Id);
                }
            }));
            bottom.Controls.Add(Btn("CSV…", () => Export("csv")));
            bottom.Controls.Add(Btn("JSON…", () => Export("json")));
            bottom.Controls.Add(Btn(L.T("C#-Tabelle…", "C# table…"), () => Export("cs")));
            bottom.Controls.Add(Btn(L.T("Kopieren", "Copy"), () => Clipboard.SetText(BuildCsv())));

            Controls.Add(_list);
            Controls.Add(bottom);
            Fill();
        }

        private static string SafeName(int id)
        {
            try
            {
                return TileData.ItemTable[id].Name ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static Button Btn(string text, Action a)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += (_, _) => a();
            return b;
        }

        private void Fill()
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (Row r in _rows)
            {
                var item = new ListViewItem($"0x{r.Id:X4}") { Tag = r };
                item.SubItems.Add(r.Id.ToString(CultureInfo.InvariantCulture));
                item.SubItems.Add(r.Name);
                item.SubItems.Add(r.Count.ToString(CultureInfo.InvariantCulture));
                item.SubItems.Add(r.Deco > 0 ? r.Deco.ToString(CultureInfo.InvariantCulture) : string.Empty);
                item.SubItems.Add(r.Hued > 0 ? r.Hued.ToString(CultureInfo.InvariantCulture) : string.Empty);
                item.SubItems.Add(r.Role == "None" ? string.Empty : r.Role);
                item.SubItems.Add(r.Price != 0 ? r.Price.ToString("0.##", CultureInfo.CurrentCulture) : string.Empty);
                item.SubItems.Add(r.Price != 0 ? (r.Price * r.Count).ToString("0.##", CultureInfo.CurrentCulture) : string.Empty);
                _list.Items.Add(item);
            }

            _list.EndUpdate();
            decimal sum = _rows.Sum(r => r.Price * r.Count);
            _total.Text = L.T($"{_rows.Count} verschiedene IDs · {_rows.Sum(r => r.Count)} Kacheln", $"{_rows.Count} different ids · {_rows.Sum(r => r.Count)} tiles") +
                          (sum != 0 ? L.T($" · Kosten {sum:0.##}", $" · cost {sum:0.##}") : string.Empty);
        }

        private int _sortColumn = 3;
        private bool _sortDesc = true;

        private void SortBy(int column)
        {
            _sortDesc = column == _sortColumn ? !_sortDesc : column >= 3;
            _sortColumn = column;
            IEnumerable<Row> sorted = column switch
            {
                0 or 1 => _rows.OrderBy(r => r.Id),
                2 => _rows.OrderBy(r => r.Name),
                4 => _rows.OrderBy(r => r.Deco),
                5 => _rows.OrderBy(r => r.Hued),
                6 => _rows.OrderBy(r => r.Role),
                7 => _rows.OrderBy(r => r.Price),
                8 => _rows.OrderBy(r => r.Price * r.Count),
                _ => _rows.OrderBy(r => r.Count)
            };
            List<Row> list = (_sortDesc ? sorted.Reverse() : sorted).ToList();
            _rows.Clear();
            _rows.AddRange(list);
            Fill();
        }

        private void SetPrice()
        {
            if (_list.SelectedItems.Count == 0)
            {
                return;
            }

            var r = (Row)_list.SelectedItems[0].Tag;
            var d = new SimpleDialog(L.T("Preis", "Price"))
                .AddNumber("p", L.T($"Preis pro Stück für 0x{r.Id:X4}:", $"Price per piece for 0x{r.Id:X4}:"), r.Price, 0, 100000000, 2)
                .AddCheck("all", L.T("Für alle markierten / gleichen Rollen", "For all with the same role"), false);
            if (!d.ShowOk(this))
            {
                return;
            }

            decimal price = (decimal)d.GetFloat("p");
            foreach (Row row in d.GetCheck("all") ? _rows.Where(x => x.Role == r.Role).ToList() : new List<Row> { r })
            {
                row.Price = price;
                _prices[row.Id] = price;
            }

            Fill();
        }

        private string BuildCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id;Hex;Name;Count;Furniture;Hued;Role;Price;Sum");
            foreach (Row r in _rows)
            {
                sb.AppendLine(string.Join(";", r.Id, $"0x{r.Id:X4}", r.Name.Replace(";", ","), r.Count, r.Deco, r.Hued, r.Role,
                    r.Price.ToString(CultureInfo.InvariantCulture), (r.Price * r.Count).ToString(CultureInfo.InvariantCulture)));
            }

            return sb.ToString();
        }

        private void Export(string kind)
        {
            string cls = MultiFormats.SanitizeClassName(_designName);
            using var dlg = new SaveFileDialog
            {
                FileName = $"{cls}_materials.{kind}",
                Filter = kind switch { "csv" => "CSV|*.csv", "json" => "JSON|*.json", _ => "C#|*.cs" }
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string text;
            switch (kind)
            {
                case "csv":
                    text = BuildCsv();
                    break;
                case "json":
                    text = JsonSerializer.Serialize(_rows.Select(r => new { r.Id, Hex = $"0x{r.Id:X4}", r.Name, r.Count, Furniture = r.Deco, r.Hued, r.Role, r.Price }),
                        new JsonSerializerOptions { WriteIndented = true });
                    break;
                default:
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"// Material list of {_designName} - generated by UO Fiddler MultiEditor");
                        sb.AppendLine("// { item id, count }  (e.g. for crafting or cost scripts)");
                        sb.AppendLine($"public static readonly int[,] {cls}Materials = new int[,]");
                        sb.AppendLine("{");
                        for (int i = 0; i < _rows.Count; ++i)
                        {
                            Row r = _rows[i];
                            sb.AppendLine($"    {{ 0x{r.Id:X4}, {r.Count} }}{(i < _rows.Count - 1 ? "," : string.Empty)} // {r.Name}");
                        }

                        sb.AppendLine("};");
                        text = sb.ToString();
                        break;
                    }
            }

            File.WriteAllText(dlg.FileName, text, new UTF8Encoding(false));
        }
    }
}
