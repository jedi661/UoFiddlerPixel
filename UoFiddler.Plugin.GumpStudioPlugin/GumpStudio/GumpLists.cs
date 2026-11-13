using System;
using System.Collections.Generic;
using System.Linq;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>One column of a list: header, x offset in the row, width and the text of every row.</summary>
    public sealed class GumpListColumn
    {
        public string Header { get; set; } = string.Empty;

        public int Offset { get; set; }

        public int Width { get; set; } = 120;

        public int Hue { get; set; } = 1152;

        public List<string> Texts { get; set; } = new List<string>();
    }

    /// <summary>
    /// A list / table: row 1 is the template (the elements with ListRow 0), all other rows and the page buttons are made from it.
    /// In the script it becomes a for loop with string arrays.
    /// </summary>
    public sealed class GumpListSpec
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);

        public string Name { get; set; } = "List";

        public int X { get; set; } = 20;

        public int Y { get; set; } = 50;

        public int RowHeight { get; set; } = 25;

        public int Rows { get; set; } = 10;

        /// <summary>Rows per page; 0 = all rows on one page.</summary>
        public int RowsPerPage { get; set; } = 5;

        public int FirstPage { get; set; } = 1;

        public bool SelectButton { get; set; } = true;

        public int ButtonBase { get; set; } = 100;

        public int SwitchBase { get; set; } = 100;

        public int EntryBase { get; set; } = 100;

        public int NavY { get; set; } = 220;

        public int NavBackX { get; set; } = 20;

        public int NavNextX { get; set; } = 250;

        public List<GumpListColumn> Columns { get; set; } = new List<GumpListColumn>();

        public bool Paged => RowsPerPage > 0 && RowsPerPage < Rows;

        public int PerPage => Paged ? RowsPerPage : Math.Max(1, Rows);

        public int PageCount => Paged ? (Rows + RowsPerPage - 1) / RowsPerPage : 1;

        public string Text(int column, int row) =>
            column >= 0 && column < Columns.Count && row >= 0 && row < Columns[column].Texts.Count ? Columns[column].Texts[row] : $"Entry {row + 1}";

        public string Identifier => GumpCodeGenerator.Identifier(Name, "List");
    }

    public static class GumpLists
    {
        /// <summary>Elements of the template row (row 1).</summary>
        public static List<GumpElement> Template(GumpDesign d, GumpListSpec spec) =>
            d.Elements.Where(e => e.ListId == spec.Id && e.ListRow == 0).ToList();

        /// <summary>Creates the template row of a new list (select button + one label per column) and builds all rows.</summary>
        public static void Create(GumpDesign d, GumpListSpec spec)
        {
            int slot = 0;
            if (spec.SelectButton)
            {
                d.Elements.Add(new GumpElement
                {
                    Type = ElementType.Button, X = spec.X, Y = spec.Y, GumpId = 4005, GumpId2 = 4007, ButtonId = spec.ButtonBase,
                    Name = spec.Identifier + "Select", ListId = spec.Id, ListSlot = slot++, Page = spec.FirstPage
                });
            }
            int x = spec.X + (spec.SelectButton ? 35 : 0);
            for (int c = 0; c < spec.Columns.Count; c++)
            {
                GumpListColumn col = spec.Columns[c];
                if (col.Offset <= 0)
                {
                    col.Offset = x - spec.X;
                }
                d.Elements.Add(new GumpElement
                {
                    Type = ElementType.LabelCropped, X = spec.X + col.Offset, Y = spec.Y + 2, Width = col.Width, Height = 20, Hue = col.Hue,
                    Text = spec.Text(c, 0), ListId = spec.Id, ListSlot = slot++, ListColumn = c, Page = spec.FirstPage
                });
                if (!string.IsNullOrEmpty(col.Header))
                {
                    d.Elements.Add(new GumpElement
                    {
                        Type = ElementType.Label, X = spec.X + col.Offset, Y = spec.Y - 25, Hue = 0x35, Text = col.Header, Page = spec.Paged ? 0 : spec.FirstPage,
                        Name = spec.Identifier + "Header" + (c + 1)
                    });
                }
                x = spec.X + col.Offset + col.Width + 10;
            }
            d.Lists.Add(spec);
            Rebuild(d);
        }

        /// <summary>Makes rows 2 … n and the page buttons again from the template row (call after every change).</summary>
        public static void Rebuild(GumpDesign d)
        {
            if (d.Lists == null || d.Lists.Count == 0)
            {
                return;
            }
            // lists whose template was deleted disappear
            d.Lists.RemoveAll(l => !d.Elements.Any(e => e.ListId == l.Id && e.ListRow == 0));
            d.Elements.RemoveAll(e => !string.IsNullOrEmpty(e.ListId) && e.ListRow != 0 && !d.Lists.Any(l => l.Id == e.ListId));
            foreach (GumpListSpec spec in d.Lists)
            {
                List<GumpElement> template = Template(d, spec);
                d.Elements.RemoveAll(e => e.ListId == spec.Id && e.ListRow != 0);
                int insertAt = d.Elements.FindLastIndex(e => e.ListId == spec.Id && e.ListRow == 0) + 1;
                // the template sits on the first page, row 1
                int baseY = template.Count > 0 ? template.Min(e => e.Y) : spec.Y;
                spec.Y = baseY;
                spec.X = template.Count > 0 ? template.Min(e => e.X) : spec.X;
                foreach (GumpElement t in template)
                {
                    t.Page = spec.FirstPage;
                    if (t.ListColumn >= 0)
                    {
                        t.Text = spec.Text(t.ListColumn, 0);
                    }
                    ApplyIds(t, spec, 0);
                }
                var made = new List<GumpElement>();
                for (int row = 1; row < spec.Rows; row++)
                {
                    int page = spec.FirstPage + row / spec.PerPage;
                    int dy = row % spec.PerPage * spec.RowHeight;
                    foreach (GumpElement t in template)
                    {
                        GumpElement c = t.Clone();
                        c.ListRow = row;
                        c.Y = t.Y + dy;
                        c.Page = page;
                        c.Locked = true;
                        c.Name = string.Empty;
                        c.Source = string.Empty;
                        if (t.ListColumn >= 0)
                        {
                            c.Text = spec.Text(t.ListColumn, row);
                        }
                        ApplyIds(c, spec, row);
                        made.Add(c);
                    }
                }
                if (spec.Paged)
                {
                    for (int p = 0; p < spec.PageCount; p++)
                    {
                        int page = spec.FirstPage + p;
                        if (p > 0)
                        {
                            made.Add(Nav(spec, page, spec.NavBackX, 4014, 4016, page - 1, "Back"));
                        }
                        if (p < spec.PageCount - 1)
                        {
                            made.Add(Nav(spec, page, spec.NavNextX, 4005, 4007, page + 1, "Next"));
                        }
                    }
                }
                d.Elements.InsertRange(Math.Max(0, Math.Min(insertAt, d.Elements.Count)), made);
            }
        }

        private static GumpElement Nav(GumpListSpec spec, int page, int x, int up, int down, int target, string name) => new GumpElement
        {
            Type = ElementType.Button, X = x, Y = spec.NavY, GumpId = up, GumpId2 = down, ButtonType = ButtonKind.Page, ButtonId = 0, TargetPage = target,
            Page = page, ListId = spec.Id, ListRow = -1, Locked = true, Name = spec.Identifier + name
        };

        private static void ApplyIds(GumpElement e, GumpListSpec spec, int row)
        {
            switch (e.Type)
            {
                case ElementType.Button:
                case ElementType.ItemButton:
                    if (e.ButtonType == ButtonKind.Reply)
                    {
                        e.ButtonId = spec.ButtonBase + row;
                    }
                    break;
                case ElementType.Check:
                case ElementType.Radio:
                    e.SwitchId = spec.SwitchBase + row;
                    break;
                case ElementType.TextEntry:
                    e.EntryId = spec.EntryBase + row;
                    break;
            }
        }

        /// <summary>The template element a generated row element stands for (clicking a row selects its template).</summary>
        public static GumpElement TemplateOf(GumpDesign d, GumpElement e)
        {
            if (string.IsNullOrEmpty(e.ListId) || e.ListRow == 0)
            {
                return e;
            }
            if (e.ListRow < 0)
            {
                return d.Elements.FirstOrDefault(x => x.ListId == e.ListId && x.ListRow == 0) ?? e;
            }
            return d.Elements.FirstOrDefault(x => x.ListId == e.ListId && x.ListRow == 0 && x.ListSlot == e.ListSlot) ?? e;
        }

        public static GumpListSpec SpecOf(GumpDesign d, GumpElement e) =>
            string.IsNullOrEmpty(e?.ListId) ? null : d.Lists?.FirstOrDefault(l => l.Id == e.ListId);

        /// <summary>Removes a list: template, rows and page buttons.</summary>
        public static void Remove(GumpDesign d, GumpListSpec spec)
        {
            d.Elements.RemoveAll(e => e.ListId == spec.Id);
            d.Lists.Remove(spec);
        }
    }
}
