// /***************************************************************************
//  *
//  * Dragon Map Studio – text editor for the Dragon scripts with "what does this line mean"
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    /// <summary>
    /// Edits the text files of a Dragon script folder. For the line under the cursor it shows what it addresses
    /// (colour, group, tiles / statics as pictures, transition pattern as 3×3 picture) and a help with an example per file type.
    /// </summary>
    public partial class ScriptEditorForm : Form
    {
        private enum Kind
        {
            Generic,
            Groups,
            MapTrans,
            MapList,
            StaticList,
            ItemList,
            Uneven,
            MapPattern,
            StaticPattern,
            ItemFile
        }

        private static readonly Encoding FileEncoding = Encoding.Latin1;
        private readonly string _folder;
        private readonly DragonRuleSet _rules;
        private string _file;
        private Kind _kind;
        private bool _dirty;
        private bool _loading;
        private int _lastLine = -1;

        public ScriptEditorForm()
            : this(string.Empty, new DragonRuleSet())
        {
        }

        public ScriptEditorForm(string scriptFolder, DragonRuleSet rules)
        {
            InitializeComponent();
            UiTranslator.Apply(this);
            _folder = scriptFolder ?? string.Empty;
            _rules = rules;
            FillTree();
            txtHelp.Text = L.T("Links eine Datei wählen. Rechts steht, was die Zeile unter dem Cursor anspricht. Nach dem Speichern die Regeln neu laden.");
            KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.S)
                {
                    Save();
                    e.SuppressKeyPress = true;
                }
            };
        }

        /// <summary>At least one file was saved (the rules should be reloaded).</summary>
        public bool Saved { get; private set; }

        /// <summary>Opens this file at start (relative to the script folder).</summary>
        public void OpenFile(string relative)
        {
            TreeNode node = FindNode(treeFiles.Nodes, Path.Combine(_folder, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (node != null)
            {
                treeFiles.SelectedNode = node;
            }
        }

        private static TreeNode FindNode(TreeNodeCollection nodes, string path)
        {
            foreach (TreeNode n in nodes)
            {
                if (string.Equals(n.Tag as string, path, StringComparison.OrdinalIgnoreCase))
                {
                    return n;
                }
                TreeNode sub = FindNode(n.Nodes, path);
                if (sub != null)
                {
                    return sub;
                }
            }
            return null;
        }

        private void FillTree()
        {
            treeFiles.BeginUpdate();
            treeFiles.Nodes.Clear();
            if (Directory.Exists(_folder))
            {
                var root = new TreeNode(Path.GetFileName(_folder.TrimEnd('\\', '/'))) { Tag = null };
                foreach (string f in Directory.GetFiles(_folder, "*.txt").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    root.Nodes.Add(new TreeNode(Path.GetFileName(f)) { Tag = f });
                }
                foreach (string dir in Directory.GetDirectories(_folder).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
                {
                    var dn = new TreeNode(Path.GetFileName(dir) + "/");
                    foreach (string f in Directory.GetFiles(dir, "*.txt").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        dn.Nodes.Add(new TreeNode(Path.GetFileName(f)) { Tag = f });
                    }
                    if (dn.Nodes.Count > 0)
                    {
                        root.Nodes.Add(dn);
                    }
                }
                treeFiles.Nodes.Add(root);
                root.Expand();
            }
            treeFiles.EndUpdate();
        }

        private bool ConfirmDiscard()
        {
            if (!_dirty)
            {
                return true;
            }
            DialogResult r = MessageBox.Show(this, L.Pick($"{Path.GetFileName(_file)} wurde geändert. Speichern?", $"{Path.GetFileName(_file)} was changed. Save?", $"{Path.GetFileName(_file)} a été modifié. Enregistrer ?"),
                Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
            {
                return false;
            }
            return r == DialogResult.No || Save();
        }

        private void TreeFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is not string file || string.Equals(file, _file, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (!ConfirmDiscard())
            {
                return;
            }
            LoadFile(file);
        }

        private void LoadFile(string file)
        {
            _loading = true;
            _file = file;
            _kind = KindOf(file);
            txtEditor.Text = File.ReadAllText(file, FileEncoding).Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            txtEditor.SelectionStart = 0;
            _dirty = false;
            _lastLine = -1;
            _loading = false;
            tslFile.Text = Path.GetRelativePath(_folder, file).Replace('\\', '/');
            txtHelp.Text = HelpOf(_kind).Replace("\n", Environment.NewLine);
            ShowLine();
        }

        private Kind KindOf(string file)
        {
            string name = Path.GetFileName(file).ToLowerInvariant();
            string dir = Path.GetFileName(Path.GetDirectoryName(file) ?? string.Empty).ToLowerInvariant();
            bool inRoot = string.Equals(Path.GetFullPath(Path.GetDirectoryName(file) ?? string.Empty).TrimEnd('\\', '/'), Path.GetFullPath(_folder).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);
            if (inRoot)
            {
                if (name == "groups.txt")
                {
                    return Kind.Groups;
                }
                if (name.StartsWith("maptrans"))
                {
                    return Kind.MapTrans;
                }
                if (name.StartsWith("statbetweentrans"))
                {
                    return Kind.StaticList;
                }
                if (name.StartsWith("betweentrans"))
                {
                    return Kind.MapList;
                }
                if (name == "items.txt")
                {
                    return Kind.ItemList;
                }
                if (name.StartsWith("unevenly"))
                {
                    return Kind.Uneven;
                }
                return Kind.Generic;
            }
            if (dir.StartsWith("map"))
            {
                return Kind.MapPattern;
            }
            if (dir.StartsWith("statics"))
            {
                return Kind.StaticPattern;
            }
            return dir.StartsWith("items") ? Kind.ItemFile : Kind.Generic;
        }

        private static string HelpOf(Kind k)
        {
            switch (k)
            {
                case Kind.Groups:
                    return L.Pick("groups.txt – welche Farbe zu welcher Gruppe gehört.\nZeile:  ADRESSE GRUPPE   (hex)\nBeispiel:  20 02   = Farbe 20 (Water -5) gehört zur Gruppe 02 (Water)\nKommentar  // 02 = Water  gibt der Gruppe ihren Namen. Spätere Zeilen überschreiben frühere.",
                        "groups.txt – which colour belongs to which group.\nLine:  ADDRESS GROUP   (hex)\nExample:  20 02   = colour 20 (Water -5) belongs to group 02 (Water)\nComment  // 02 = Water  names the group. Later lines override earlier ones.",
                        "groups.txt – quelle couleur appartient à quel groupe.\nLigne :  ADRESSE GROUPE   (hex)\nExemple :  20 02   = couleur 20 (Water -5) dans le groupe 02 (Water)\nCommentaire  // 02 = Water  nomme le groupe.");
                case Kind.MapTrans:
                    return L.Pick("maptrans.txt – was eine Farbe auf der Karte wird.\nZeile:  ADRESSE HÖHE TILE TILE …   (Adresse hex, Höhe dezimal, Tiles hex)\nBeispiel:  00 000 0003 0004 0005 0006   = Farbe 00: Gras auf Höhe 0, eines der 4 Tiles zufällig\n16 Bit: Adressen 4-stellig, z. B.  0123 010 03e9 03ea",
                        "maptrans.txt – what a colour becomes on the map.\nLine:  ADDRESS HEIGHT TILE TILE …   (address hex, height decimal, tiles hex)\nExample:  00 000 0003 0004 0005 0006   = colour 00: grass at height 0, one of the 4 tiles at random\n16 bit: 4-digit addresses, e.g.  0123 010 03e9 03ea",
                        "maptrans.txt – ce que devient une couleur sur la carte.\nLigne :  ADRESSE HAUTEUR TUILE TUILE …\nExemple :  00 000 0003 0004 0005 0006   = herbe à hauteur 0, une des 4 tuiles au hasard");
                case Kind.MapList:
                case Kind.StaticList:
                    return L.Pick("betweentrans.txt (Boden) / statbetweentrans.txt (Statics) – welche Übergangsdatei zwischen zwei Gruppen gilt.\nSatz 1:  A B datei.txt   = Zelle der Gruppe A neben Gruppe B\nSatz 2 (…2.txt):  A datei.txt   = A neben jeder anderen Gruppe;  Satz 3 (…3.txt):  B datei.txt\nBeispiel:  00 05 grass2sand.txt   (Ordner map/ bzw. statics/)\n// am Zeilenanfang schaltet eine Regel aus.",
                        "betweentrans.txt (ground) / statbetweentrans.txt (statics) – which transition file applies between two groups.\nSet 1:  A B file.txt   = cell of group A next to group B\nSet 2 (…2.txt):  A file.txt   = A next to any other group;  set 3 (…3.txt):  B file.txt\nExample:  00 05 grass2sand.txt   (folder map/ or statics/)\n// at the start of a line switches a rule off.",
                        "betweentrans.txt (sol) / statbetweentrans.txt (statiques) – quel fichier de transition s'applique entre deux groupes.\nJeu 1 :  A B fichier.txt\nExemple :  00 05 grass2sand.txt");
                case Kind.ItemList:
                    return L.Pick("items.txt – Items (Bäume, Steine …) je Gruppe.\nZeile:  GRUPPE CHANCE LINKS OBEN RECHTS UNTEN datei.txt\nBeispiel:  01 0050 0000 0000 0000 0000 forest.txt   = Gruppe 01, Chance 50 (‰), ganze Karte, Items aus items/forest.txt",
                        "items.txt – items (trees, stones …) per group.\nLine:  GROUP CHANCE LEFT TOP RIGHT BOTTOM file.txt\nExample:  01 0050 0000 0000 0000 0000 forest.txt   = group 01, chance 50 (‰), whole map, items from items/forest.txt",
                        "items.txt – objets par groupe.\nLigne :  GROUPE CHANCE GAUCHE HAUT DROITE BAS fichier.txt");
                case Kind.Uneven:
                    return L.Pick("unevenly.txt – zufällige Unebenheit je Gruppe.\nZeile:  GRUPPE HÄUFIGKEIT LINKS OBEN RECHTS UNTEN MENGE\nBeispiel:  00 0100 0000 0000 0000 0000 003   = Gras, 10 % der Tiles, Höhe +0..3",
                        "unevenly.txt – random unevenness per group.\nLine:  GROUP FREQUENCY LEFT TOP RIGHT BOTTOM AMOUNT\nExample:  00 0100 0000 0000 0000 0000 003   = grass, 10 % of the tiles, height +0..3",
                        "unevenly.txt – irrégularité aléatoire par groupe.");
                case Kind.MapPattern:
                case Kind.StaticPattern:
                    return L.Pick("Übergangsdatei – Muster und was gesetzt wird.\nZeile:  MUSTER TILE TILE … Z1 Z2\nMUSTER = 8 Zeichen A/B für die Nachbarn NW N NO O SO S SW W (A = eigene Gruppe, B = Nachbargruppe)\nBeispiel:  BBBAAAAA 0035 0036 000 000   = Norden ist B (Kante oben) → Tile 0035 oder 0036, Höhe +0\nmap/: Boden-Tiles;  statics/: Statics (z. B. Ufer-Wellen). Z = alte Höhe + Zufall Z1..Z2.",
                        "Transition file – patterns and what is placed.\nLine:  PATTERN TILE TILE … Z1 Z2\nPATTERN = 8 characters A/B for the neighbours NW N NE E SE S SW W (A = own group, B = neighbour group)\nExample:  BBBAAAAA 0035 0036 000 000   = north is B (top edge) → tile 0035 or 0036, height +0\nmap/: ground tiles;  statics/: statics (e.g. shore waves). Z = old height + random Z1..Z2.",
                        "Fichier de transition – motifs et ce qui est posé.\nLigne :  MOTIF TUILE TUILE … Z1 Z2\nMOTIF = 8 caractères A/B pour NO N NE E SE S SO O\nExemple :  BBBAAAAA 0035 0036 000 000");
                case Kind.ItemFile:
                    return L.Pick("Item-Datei – Gruppen von Statics.\nZeile:  HÄUFIGKEIT  ID Z HUE DX DY  [ID Z HUE DX DY …]\nBeispiel:  10 0cd0 0 0 0 0 0cd1 0 0 0 0   = Baum aus 2 Teilen, Häufigkeit 10",
                        "Item file – groups of statics.\nLine:  FREQUENCY  ID Z HUE DX DY  [ID Z HUE DX DY …]\nExample:  10 0cd0 0 0 0 0 0cd1 0 0 0 0   = tree of 2 parts, frequency 10",
                        "Fichier d'objets – groupes de statiques.\nLigne :  FRÉQUENCE  ID Z TEINTE DX DY  [ID Z TEINTE DX DY …]");
                default:
                    return L.Pick("Text-Datei. // am Zeilenanfang = Kommentar.", "Text file. // at the start of a line = comment.", "Fichier texte. // en début de ligne = commentaire.");
            }
        }

        private void TxtEditor_TextChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }
            if (!_dirty)
            {
                _dirty = true;
                tslFile.Text = "* " + tslFile.Text;
            }
            _lastLine = -1;
            ShowLine();
        }

        private void TxtEditor_CaretMoved(object sender, EventArgs e) => ShowLine();

        private void ShowLine()
        {
            if (_file == null)
            {
                return;
            }
            int line = txtEditor.GetLineFromCharIndex(txtEditor.SelectionStart);
            if (line == _lastLine)
            {
                return;
            }
            _lastLine = line;
            string text = line >= 0 && line < txtEditor.Lines.Length ? txtEditor.Lines[line].Trim() : string.Empty;
            foreach (Control c in flowLineTiles.Controls.Cast<Control>().ToList())
            {
                c.Dispose();
            }
            picPattern.Image?.Dispose();
            picPattern.Image = null;
            lblLineInfo.Text = $"{L.T("Zeile")} {line + 1}" + Environment.NewLine + Describe(text);
        }

        private string Describe(string line)
        {
            if (line.Length == 0)
            {
                return L.T("(leer)");
            }
            if (line.StartsWith("//"))
            {
                return L.T("Kommentar") + (line.TrimStart('/').Trim().Length > 0 ? ": " + line.TrimStart('/').Trim() : string.Empty);
            }
            string[] t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            switch (_kind)
            {
                case Kind.Groups:
                    if (t.Length >= 2 && HexList.TryParseHexAddress(t[0], out int idx) && HexList.TryParseHexAddress(t[1], out int grp))
                    {
                        sb.AppendLine(ColourText(idx));
                        sb.AppendLine($"→ {L.T("Gruppe")} {_rules.HexGroup(grp)} {_rules.GroupName(grp)}");
                        ShowColourTiles(idx);
                    }
                    break;
                case Kind.MapTrans:
                    if (t.Length >= 3 && HexList.TryParseHexAddress(t[0], out int idx2))
                    {
                        sb.AppendLine(ColourText(idx2));
                        sb.AppendLine($"{L.T("Höhe")} {HexList.ParseInt(t[1])}");
                        ushort[] tiles = t.Skip(2).Select(HexList.ParseHex).ToArray();
                        sb.AppendLine($"{tiles.Length} {L.T("Tiles (zufällig)")}");
                        AddPictures(tiles, true);
                    }
                    break;
                case Kind.MapList:
                case Kind.StaticList:
                    {
                        string file = t.LastOrDefault(x => x.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));
                        var groups = t.TakeWhile(x => !x.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)).ToList();
                        foreach (string g in groups)
                        {
                            if (HexList.TryParseHexAddress(g, out int gi))
                            {
                                sb.AppendLine($"{L.T("Gruppe")} {g} = {_rules.GroupName(gi)}");
                            }
                        }
                        if (file != null)
                        {
                            string folder = TransitionRule.GetFolder(_kind == Kind.MapList ? TransitionLayer.Map : TransitionLayer.Static, SetOfList());
                            string full = Path.Combine(_folder, folder, file);
                            TransitionFile tf = _rules.FindTransitionFile(folder + "/" + file);
                            sb.AppendLine($"{L.T("Datei")}: {folder}/{file}");
                            sb.AppendLine(File.Exists(full) ? $"✓ {L.T("vorhanden")}" + (tf != null ? $", {tf.Patterns.Count} {L.T("Muster")}" : string.Empty) : $"✗ {L.T("Datei fehlt!")}");
                        }
                        break;
                    }
                case Kind.ItemList:
                case Kind.Uneven:
                    if (t.Length >= 1 && HexList.TryParseHexAddress(t[0], out int g2))
                    {
                        sb.AppendLine($"{L.T("Gruppe")} {t[0]} = {_rules.GroupName(g2)}");
                        if (_kind == Kind.ItemList && t.Length >= 7)
                        {
                            sb.AppendLine($"{L.T("Chance")} {HexList.ParseInt(t[1])} ‰, {L.T("Datei")} items/{t[6]}");
                            AddPictures(RuleSetTools.ItemIdsOfGroup(_rules, g2, 16).ToArray(), false);
                        }
                        else if (t.Length >= 7)
                        {
                            sb.AppendLine($"{L.T("Häufigkeit")} {HexList.ParseInt(t[1])}, {L.T("Höhe")} +0..{HexList.ParseInt(t[6])}");
                        }
                    }
                    break;
                case Kind.MapPattern:
                case Kind.StaticPattern:
                    if (t.Length >= 1 && t[0].Length == 8 && t[0].All(c => c == 'A' || c == 'B' || c == 'a' || c == 'b'))
                    {
                        string p = t[0].ToUpperInvariant();
                        sb.AppendLine($"{L.T("Muster")} {p}");
                        string[] dirs = { "NW", "N", "NO", "O", "SO", "S", "SW", "W" };
                        sb.AppendLine("B: " + string.Join(" ", Enumerable.Range(0, 8).Where(i => p[i] == 'B').Select(i => dirs[i])));
                        var ids = t.Skip(1).Where(x => x.Length == 4).Select(HexList.ParseHex).ToArray();
                        var nums = t.Skip(1).Where(x => x.Length != 4).ToArray();
                        if (nums.Length >= 2)
                        {
                            sb.AppendLine($"Z1 {nums[0]}  Z2 {nums[1]}");
                        }
                        picPattern.Image = DrawPattern(p);
                        AddPictures(ids, _kind == Kind.MapPattern);
                    }
                    break;
                case Kind.ItemFile:
                    if (t.Length >= 6)
                    {
                        sb.AppendLine($"{L.T("Häufigkeit")} {t[0]}, {(t.Length - 1) / 5} {L.T("Teile")}");
                        AddPictures(Enumerable.Range(0, (t.Length - 1) / 5).Select(i => HexList.ParseHex(t[1 + i * 5])).ToArray(), false);
                    }
                    break;
            }
            return sb.Length == 0 ? L.T("(keine Daten erkannt)") : sb.ToString();
        }

        private int SetOfList()
        {
            string name = Path.GetFileNameWithoutExtension(_file);
            char last = name.Length > 0 ? name[name.Length - 1] : '1';
            return last == '2' ? 2 : last == '3' ? 3 : 1;
        }

        private string ColourText(int index)
        {
            if (index < 0 || index >= _rules.Palette.Count)
            {
                return $"{L.T("Farbe")} {index:x} – {L.T("außerhalb der Palette")}";
            }
            DragonPaletteEntry p = _rules.Palette[index];
            var swatch = new Panel { Size = new Size(36, 36), BackColor = p.Color, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(2) };
            flowLineTiles.Controls.Add(swatch);
            return $"{L.T("Farbe")} {_rules.HexIndex(index)}: {p.Name} ({p.HtmlColor}, Z {p.Altitude})";
        }

        private void ShowColourTiles(int index)
        {
            if (index >= 0 && index < _rules.Palette.Count)
            {
                AddPictures(_rules.Palette[index].GetTileIds(), true);
            }
        }

        private void AddPictures(IEnumerable<ushort> ids, bool land)
        {
            foreach (ushort id in ids.Distinct().Take(30))
            {
                Image img = null;
                try
                {
                    img = land ? (Image)UoArtCache.Land(id) ?? UoArtCache.Texture(id) : UoArtCache.Static(id, 0);
                }
                catch (Exception)
                {
                    img = null;
                }
                var panel = new Panel { Size = new Size(48, 62), Margin = new Padding(2) };
                panel.Controls.Add(new PictureBox { Image = img, SizeMode = PictureBoxSizeMode.Zoom, Bounds = new Rectangle(0, 0, 48, 46), BackColor = Color.FromArgb(48, 48, 54) });
                panel.Controls.Add(new Label { Text = id.ToString("x4"), ForeColor = Color.Gainsboro, Bounds = new Rectangle(0, 46, 48, 16), TextAlign = ContentAlignment.MiddleCenter });
                flowLineTiles.Controls.Add(panel);
            }
        }

        private static Bitmap DrawPattern(string pattern)
        {
            const int cell = 30;
            (int dx, int dy)[] pos = { (0, 0), (1, 0), (2, 0), (2, 1), (2, 2), (1, 2), (0, 2), (0, 1) };
            var bmp = new Bitmap(cell * 3, cell * 3);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
            for (int k = 0; k < 8; k++)
            {
                bool b = pattern[k] == 'B';
                using var brush = new SolidBrush(b ? Color.FromArgb(70, 110, 190) : Color.FromArgb(110, 170, 80));
                var r = new Rectangle(pos[k].dx * cell, pos[k].dy * cell, cell - 1, cell - 1);
                g.FillRectangle(brush, r);
                TextRenderer.DrawText(g, b ? "B" : "A", font, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            g.FillRectangle(Brushes.Gold, cell, cell, cell - 1, cell - 1);
            return bmp;
        }

        private bool Save()
        {
            if (_file == null)
            {
                return false;
            }
            try
            {
                if (File.Exists(_file))
                {
                    File.Copy(_file, _file + ".bak", true);
                }
                File.WriteAllText(_file, txtEditor.Text.Replace("\r\n", "\n").Replace("\n", "\r\n"), FileEncoding);
                _dirty = false;
                Saved = true;
                tslFile.Text = Path.GetRelativePath(_folder, _file).Replace('\\', '/') + "  ✓";
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void TsbSave_Click(object sender, EventArgs e) => Save();

        private void TsbReloadFile_Click(object sender, EventArgs e)
        {
            if (_file != null && ConfirmDiscard())
            {
                LoadFile(_file);
            }
        }

        private void TsbOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = _folder, UseShellExecute = true });
            }
            catch (Exception)
            {
                // no explorer
            }
        }

        private void TstSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FindNext();
                e.SuppressKeyPress = true;
            }
        }

        private void TsbFindNext_Click(object sender, EventArgs e) => FindNext();

        private void FindNext()
        {
            string what = tstSearch.Text;
            if (what.Length == 0)
            {
                return;
            }
            int start = txtEditor.SelectionStart + txtEditor.SelectionLength;
            int i = txtEditor.Text.IndexOf(what, Math.Min(start, txtEditor.TextLength), StringComparison.OrdinalIgnoreCase);
            if (i < 0)
            {
                i = txtEditor.Text.IndexOf(what, StringComparison.OrdinalIgnoreCase);
            }
            if (i >= 0)
            {
                txtEditor.Select(i, what.Length);
                txtEditor.ScrollToCaret();
                txtEditor.Focus();
                ShowLine();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!ConfirmDiscard())
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}
