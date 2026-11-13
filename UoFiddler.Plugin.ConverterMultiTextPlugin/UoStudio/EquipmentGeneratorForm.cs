/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UoStudio
{
    /// <summary>
    /// Ausruestungs-Generator: erzeugt aus einem Item-Bild ein Paperdoll-Gump (50000 + Animation
    /// fuer maennlich, 60000 + Animation fuer weiblich), zeigt es auf der Paperdoll-Figur an und
    /// erstellt den passenden Tiledata-Eintrag (Wearable, Layer, Animation, Gewicht, Hue).
    /// </summary>
    public class EquipmentGeneratorForm : Form
    {
        private const int MaleGumpOffset = 50000;
        private const int FemaleGumpOffset = 60000;

        private static readonly string[] _layers =
        {
            "0 - Ungültig", "1 - Einhand-Waffe", "2 - Zweihand / Schild", "3 - Schuhe", "4 - Hose", "5 - Hemd",
            "6 - Helm / Hut", "7 - Handschuhe", "8 - Ring", "9 - Talisman", "10 - Halskette", "11 - Haare",
            "12 - Gürtel / Schürze", "13 - Innerer Torso (Brust)", "14 - Armband", "15 - Gesicht", "16 - Bart",
            "17 - Mittlerer Torso (Tunika)", "18 - Ohrringe", "19 - Arme", "20 - Umhang", "21 - Rucksack",
            "22 - Äußerer Torso (Robe)", "23 - Äußere Beine (Rock)", "24 - Innere Beine", "25 - Reittier"
        };

        private Bitmap _item;       // processed item image (32bpp)
        private Bitmap _gump;       // generated paperdoll gump
        private Bitmap _body;       // paperdoll body gump
        private Point _dragStart;
        private Point _dragOffset;
        private bool _dragging;

        private readonly PixelView _preview = new PixelView { Dock = DockStyle.Fill, Caption = "Paperdoll-Vorschau (Item mit der Maus verschieben)", Zoom = 2 };
        private readonly PixelView _itemView = new PixelView { Dock = DockStyle.Fill, Caption = "Item / Gump", Zoom = 2 };
        private readonly ToolStripStatusLabel _status = new ToolStripStatusLabel("Item-Bild laden (Datei oder aus Art).");

        private NumericUpDown _numOffX, _numOffY, _numScale, _numRotate, _numHue, _numBodyGump, _numZoom;
        private CheckBox _chkFlip, _chkGray, _chkFemale, _chkPartialHue, _chkShowBody;
        private ComboBox _cbGender, _cbLayer;
        private TextBox _txtItemId, _txtArtSource, _txtName;
        private NumericUpDown _numAnim, _numWeight, _numTileHue, _numHeight;
        private Label _lblGumpIds;

        public EquipmentGeneratorForm()
        {
            Text = "Ausrüstungs-Generator (Item → Paperdoll-Gump + Tiledata)";
            Size = new Size(1180, 800);
            MinimumSize = new Size(950, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BuildUi();
            LoadBody();
            UpdateGumpIds();
        }

        #region UI

        private void BuildUi()
        {
            var left = new Panel { Dock = DockStyle.Left, Width = 350, AutoScroll = true };
            int y = 6;

            var grpSrc = new GroupBox { Text = "Quelle", Left = 4, Top = y, Width = 326, Height = 84 };
            grpSrc.Controls.Add(StudioUi.Button("Bild laden...", 8, 20, 100, (s, e) => LoadItemFile()));
            grpSrc.Controls.Add(StudioUi.Label("Art-ID:", 116, 22));
            _txtArtSource = new TextBox { Left = 164, Top = 22, Width = 70, Text = "0x1415" };
            grpSrc.Controls.Add(_txtArtSource);
            grpSrc.Controls.Add(StudioUi.Button("aus Art", 240, 20, 76, (s, e) => LoadItemFromArt()));
            grpSrc.Controls.Add(StudioUi.Button("Vorhandenes Gump laden (Anim-ID)", 8, 50, 308, (s, e) => LoadExistingGump()));
            left.Controls.Add(grpSrc);
            y += 90;

            var grpFig = new GroupBox { Text = "Figur", Left = 4, Top = y, Width = 326, Height = 80 };
            _cbGender = StudioUi.Combo(8, 20, 120, "Männlich (0x0C)", "Weiblich (0x0D)", "Eigenes Gump");
            _cbGender.SelectedIndexChanged += (s, e) =>
            {
                if (_cbGender.SelectedIndex < 2)
                {
                    _numBodyGump.Value = _cbGender.SelectedIndex == 0 ? 0x0C : 0x0D;
                }
                LoadBody();
            };
            grpFig.Controls.Add(_cbGender);
            grpFig.Controls.Add(StudioUi.Label("Body-Gump:", 136, 20));
            _numBodyGump = StudioUi.Number(214, 20, 70, 0, 0xFFFF, 0x0C);
            _numBodyGump.ValueChanged += (s, e) => LoadBody();
            grpFig.Controls.Add(_numBodyGump);
            _chkShowBody = StudioUi.Check("Figur anzeigen", 8, 52, true);
            _chkShowBody.CheckedChanged += (s, e) => Compose();
            grpFig.Controls.Add(_chkShowBody);
            grpFig.Controls.Add(StudioUi.Label("Zoom:", 136, 52));
            _numZoom = StudioUi.Number(214, 50, 50, 1, 6, 2);
            _numZoom.ValueChanged += (s, e) => _preview.Zoom = (int)_numZoom.Value;
            grpFig.Controls.Add(_numZoom);
            left.Controls.Add(grpFig);
            y += 86;

            var grpPos = new GroupBox { Text = "Position & Form", Left = 4, Top = y, Width = 326, Height = 140 };
            grpPos.Controls.Add(StudioUi.Label("X:", 8, 20));
            _numOffX = StudioUi.Number(30, 20, 60, -500, 1000, 60);
            grpPos.Controls.Add(_numOffX);
            grpPos.Controls.Add(StudioUi.Label("Y:", 100, 20));
            _numOffY = StudioUi.Number(120, 20, 60, -500, 1000, 90);
            grpPos.Controls.Add(_numOffY);
            grpPos.Controls.Add(StudioUi.Label("Größe %:", 8, 50));
            _numScale = StudioUi.Number(70, 50, 60, 10, 800, 100);
            grpPos.Controls.Add(_numScale);
            grpPos.Controls.Add(StudioUi.Label("Drehen °:", 140, 50));
            _numRotate = StudioUi.Number(204, 50, 60, -180, 180, 0);
            grpPos.Controls.Add(_numRotate);
            _chkFlip = StudioUi.Check("Spiegeln", 8, 80, false);
            grpPos.Controls.Add(_chkFlip);
            _chkGray = StudioUi.Check("Graustufen (färbbar)", 100, 80, false);
            grpPos.Controls.Add(_chkGray);
            grpPos.Controls.Add(StudioUi.Label("Hue-Vorschau:", 8, 108));
            _numHue = StudioUi.Number(100, 108, 70, 0, 3000, 0);
            grpPos.Controls.Add(_numHue);
            left.Controls.Add(grpPos);
            y += 146;

            var grpTile = new GroupBox { Text = "Tiledata-Eintrag", Left = 4, Top = y, Width = 326, Height = 222 };
            grpTile.Controls.Add(StudioUi.Label("Item-ID:", 8, 20));
            _txtItemId = new TextBox { Left = 90, Top = 20, Width = 80, Text = "0x1415" };
            grpTile.Controls.Add(_txtItemId);
            grpTile.Controls.Add(StudioUi.Button("Lesen", 176, 18, 66, (s, e) => ReadTileData()));
            grpTile.Controls.Add(StudioUi.Label("Name:", 8, 48));
            _txtName = new TextBox { Left = 90, Top = 48, Width = 226, Text = "new armor" };
            grpTile.Controls.Add(_txtName);
            grpTile.Controls.Add(StudioUi.Label("Layer:", 8, 76));
            _cbLayer = StudioUi.Combo(90, 76, 226, (object[])_layers);
            _cbLayer.SelectedIndex = 13;
            grpTile.Controls.Add(_cbLayer);
            grpTile.Controls.Add(StudioUi.Label("Animation:", 8, 104));
            _numAnim = StudioUi.Number(90, 104, 70, 0, 9999, 1000);
            _numAnim.ValueChanged += (s, e) => UpdateGumpIds();
            grpTile.Controls.Add(_numAnim);
            grpTile.Controls.Add(StudioUi.Button("Freie suchen", 168, 102, 100, (s, e) => FindFreeAnim()));
            _lblGumpIds = StudioUi.Label("", 8, 130);
            _lblGumpIds.AutoSize = true;
            grpTile.Controls.Add(_lblGumpIds);
            grpTile.Controls.Add(StudioUi.Label("Gewicht:", 8, 158));
            _numWeight = StudioUi.Number(70, 158, 50, 0, 255, 5);
            grpTile.Controls.Add(_numWeight);
            grpTile.Controls.Add(StudioUi.Label("Hue:", 128, 158));
            _numTileHue = StudioUi.Number(160, 158, 50, 0, 255, 0);
            grpTile.Controls.Add(_numTileHue);
            grpTile.Controls.Add(StudioUi.Label("Höhe:", 216, 158));
            _numHeight = StudioUi.Number(256, 158, 50, 0, 255, 1);
            grpTile.Controls.Add(_numHeight);
            _chkPartialHue = StudioUi.Check("PartialHue", 8, 188, true);
            grpTile.Controls.Add(_chkPartialHue);
            _chkFemale = StudioUi.Check("auch weibliches Gump", 120, 188, true);
            grpTile.Controls.Add(_chkFemale);
            left.Controls.Add(grpTile);
            y += 228;

            var grpExp = new GroupBox { Text = "Export", Left = 4, Top = y, Width = 326, Height = 150 };
            grpExp.Controls.Add(StudioUi.Button("Alles setzen (Gump + Tiledata)", 8, 20, 308, (s, e) => ApplyAll()));
            grpExp.Controls.Add(StudioUi.Button("Nur Gump(s) setzen", 8, 50, 150, (s, e) => ApplyGumps(true)));
            grpExp.Controls.Add(StudioUi.Button("Nur Tiledata setzen", 166, 50, 150, (s, e) => ApplyTileData(true)));
            grpExp.Controls.Add(StudioUi.Button("Gump-MUL speichern", 8, 80, 150, (s, e) => SaveGumps()));
            grpExp.Controls.Add(StudioUi.Button("tiledata.mul speichern", 166, 80, 150, (s, e) => SaveTileData()));
            grpExp.Controls.Add(StudioUi.Button("Gump als PNG...", 8, 110, 150, (s, e) => SaveGumpPng()));
            grpExp.Controls.Add(StudioUi.Button("Script-Snippet kopieren", 166, 110, 150, (s, e) => CopyScript()));
            left.Controls.Add(grpExp);

            foreach (NumericUpDown n in new[] { _numOffX, _numOffY, _numScale, _numRotate, _numHue })
            {
                n.ValueChanged += (s, e) => Compose();
            }
            _chkFlip.CheckedChanged += (s, e) => Compose();
            _chkGray.CheckedChanged += (s, e) => Compose();

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 560 };
            split.Panel1.Controls.Add(_preview);
            split.Panel2.Controls.Add(_itemView);

            _preview.MouseDown += Preview_MouseDown;
            _preview.MouseMove += Preview_MouseMove;
            _preview.MouseUp += (s, e) => _dragging = false;

            var status = new StatusStrip();
            status.Items.Add(_status);

            Controls.Add(split);
            Controls.Add(left);
            Controls.Add(status);
        }

        private void UpdateGumpIds()
        {
            int anim = (int)_numAnim.Value;
            _lblGumpIds.Text = $"Gump männlich: {MaleGumpOffset + anim} (0x{MaleGumpOffset + anim:X})   weiblich: {FemaleGumpOffset + anim} (0x{FemaleGumpOffset + anim:X})";
        }

        #endregion

        #region Loading

        private void LoadBody()
        {
            _body?.Dispose();
            _body = null;
            try
            {
                int id = (int)_numBodyGump.Value;
                if (Gumps.IsValidIndex(id))
                {
                    using Bitmap g = Gumps.GetGump(id);
                    _body = StudioImaging.To32(g);
                }
            }
            catch
            {
                _body = null;
            }
            Compose();
        }

        private void LoadItemFile()
        {
            using var dlg = new OpenFileDialog { Filter = StudioUi.ImageFilter };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                using Bitmap b = StudioImaging.LoadImage(dlg.FileName);
                SetItem(StudioImaging.AutoCrop(b));
                _status.Text = "Bild geladen: " + dlg.FileName;
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void LoadItemFromArt()
        {
            if (!StudioUi.TryParseId(_txtArtSource.Text, out int id))
            {
                StudioUi.Error(this, "Ungültige Art-ID.");
                return;
            }
            try
            {
                if (!Art.IsValidStatic(id))
                {
                    StudioUi.Error(this, $"Art-Item 0x{id:X4} existiert nicht.");
                    return;
                }
                using Bitmap a = Art.GetStatic(id);
                using Bitmap b = StudioImaging.To32(a);
                SetItem(StudioImaging.AutoCrop(b));
                _txtItemId.Text = $"0x{id:X4}";
                ReadTileData();
                _numScale.Value = 150;
                _status.Text = $"Art 0x{id:X4} geladen. Tipp: Größe und Drehung an die Figur anpassen.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void LoadExistingGump()
        {
            int gid = ((_cbGender.SelectedIndex == 1) ? FemaleGumpOffset : MaleGumpOffset) + (int)_numAnim.Value;
            try
            {
                if (!Gumps.IsValidIndex(gid))
                {
                    StudioUi.Info(this, $"Gump {gid} existiert nicht.");
                    return;
                }
                using Bitmap g = Gumps.GetGump(gid);
                using Bitmap b = StudioImaging.To32(g);
                // gump is positioned at 0/0 relative to the body -> crop and remember offset
                Rectangle r = OpaqueBounds(b);
                SetItem(StudioImaging.Crop(b, r));
                _numScale.Value = 100;
                _numRotate.Value = 0;
                _chkFlip.Checked = false;
                _numOffX.Value = r.X;
                _numOffY.Value = r.Y;
                _status.Text = $"Gump {gid} geladen.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private static Rectangle OpaqueBounds(Bitmap b)
        {
            int[] p = StudioImaging.GetPixels(b);
            int minX = b.Width, minY = b.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    if (((p[(y * b.Width) + x] >> 24) & 0xFF) > 0)
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }
            return maxX < 0 ? new Rectangle(0, 0, b.Width, b.Height) : new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private void SetItem(Bitmap bmp)
        {
            _item?.Dispose();
            _item = bmp;
            Compose();
        }

        private void ReadTileData()
        {
            if (!StudioUi.TryParseId(_txtItemId.Text, out int id))
            {
                return;
            }
            try
            {
                if (TileData.ItemTable == null || id >= TileData.ItemTable.Length)
                {
                    return;
                }
                ItemData d = TileData.ItemTable[id];
                _txtName.Text = d.Name;
                _numWeight.Value = d.Weight;
                _numTileHue.Value = d.Hue;
                _numHeight.Value = d.Height;
                if (d.Quality < _layers.Length && d.Wearable)
                {
                    _cbLayer.SelectedIndex = d.Quality;
                }
                if (d.Animation > 0)
                {
                    _numAnim.Value = Math.Min(_numAnim.Maximum, d.Animation);
                }
                _chkPartialHue.Checked = (d.Flags & TileFlag.PartialHue) != 0;
            }
            catch
            {
                // tiledata not loaded
            }
        }

        #endregion

        #region Compose

        /// <summary>Builds the transformed item gump and the paperdoll preview.</summary>
        private void Compose()
        {
            _gump?.Dispose();
            _gump = null;

            int canvasW = _body?.Width ?? 260;
            int canvasH = _body?.Height ?? 237;

            if (_item != null)
            {
                Bitmap item = TransformItem();
                int ox = (int)_numOffX.Value, oy = (int)_numOffY.Value;

                // gump = item drawn at its offset relative to the body origin (0/0), trimmed right/bottom
                int gw = Math.Max(1, ox + item.Width), gh = Math.Max(1, oy + item.Height);
                var gump = new Bitmap(gw, gh, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(gump))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImageUnscaled(item, ox, oy);
                }
                item.Dispose();
                Bitmap uo = StudioImaging.ToUoColors(gump);
                gump.Dispose();
                _gump = uo;
                canvasW = Math.Max(canvasW, gw);
                canvasH = Math.Max(canvasH, gh);
            }

            var preview = new Bitmap(canvasW, canvasH, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(preview))
            {
                g.Clear(Color.Transparent);
                if (_body != null && _chkShowBody.Checked)
                {
                    g.DrawImageUnscaled(_body, 0, 0);
                }
                if (_gump != null)
                {
                    Hue hue = StudioUi.GetHue((int)_numHue.Value);
                    if (hue != null)
                    {
                        using Bitmap hued = StudioImaging.ApplyHue(_gump, hue, _chkPartialHue.Checked);
                        g.DrawImageUnscaled(hued, 0, 0);
                    }
                    else
                    {
                        g.DrawImageUnscaled(_gump, 0, 0);
                    }
                }
            }

            Image oldPreview = _preview.Image;
            _preview.Image = preview;
            oldPreview?.Dispose();

            Image oldItem = _itemView.Image;
            _itemView.Image = _gump != null ? new Bitmap(_gump) : null;
            oldItem?.Dispose();

            if (_gump != null)
            {
                _itemView.Caption = $"Gump {_gump.Width}x{_gump.Height}";
            }
        }

        private Bitmap TransformItem()
        {
            float scale = (float)_numScale.Value / 100f;
            int w = Math.Max(1, (int)Math.Round(_item.Width * scale));
            int h = Math.Max(1, (int)Math.Round(_item.Height * scale));
            Bitmap scaled = StudioImaging.Resize(_item, w, h, scale >= 1.5f ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic);

            float angle = (float)_numRotate.Value;
            if (Math.Abs(angle) > 0.01f)
            {
                double rad = angle * Math.PI / 180.0;
                int rw = (int)Math.Ceiling((Math.Abs(Math.Cos(rad)) * w) + (Math.Abs(Math.Sin(rad)) * h));
                int rh = (int)Math.Ceiling((Math.Abs(Math.Sin(rad)) * w) + (Math.Abs(Math.Cos(rad)) * h));
                var rot = new Bitmap(rw, rh, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(rot))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.TranslateTransform(rw / 2f, rh / 2f);
                    g.RotateTransform(angle);
                    g.DrawImage(scaled, -w / 2f, -h / 2f, w, h);
                }
                scaled.Dispose();
                scaled = rot;
            }

            if (_chkFlip.Checked)
            {
                scaled.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            Bitmap hard = StudioImaging.HardAlpha(scaled, 110);
            scaled.Dispose();

            if (_chkGray.Checked)
            {
                Bitmap gray = StudioImaging.ToGray(hard);
                hard.Dispose();
                hard = gray;
            }
            return hard;
        }

        private void Preview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _item == null)
            {
                return;
            }
            _dragging = true;
            _dragStart = e.Location;
            _dragOffset = new Point((int)_numOffX.Value, (int)_numOffY.Value);
        }

        private void Preview_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }
            int z = _preview.Zoom;
            int nx = _dragOffset.X + ((e.X - _dragStart.X) / z);
            int ny = _dragOffset.Y + ((e.Y - _dragStart.Y) / z);
            _numOffX.Value = Math.Max(_numOffX.Minimum, Math.Min(_numOffX.Maximum, nx));
            _numOffY.Value = Math.Max(_numOffY.Minimum, Math.Min(_numOffY.Maximum, ny));
        }

        #endregion

        #region Export

        private void FindFreeAnim()
        {
            try
            {
                int count = Gumps.GetCount();
                for (int a = 0x200; a < 5000; a++)
                {
                    int m = MaleGumpOffset + a, f = FemaleGumpOffset + a;
                    if (m >= count)
                    {
                        break;
                    }
                    if (!Gumps.IsValidIndex(m) && (f >= count || !Gumps.IsValidIndex(f)))
                    {
                        _numAnim.Value = a;
                        _status.Text = $"Freie Animation: {a} (Gump {m} / {f})";
                        return;
                    }
                }
                StudioUi.Info(this, "Keine freie Animations-ID gefunden.");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void ApplyAll()
        {
            if (ApplyGumps(false) && ApplyTileData(false))
            {
                _status.Text = "Gump(s) und Tiledata gesetzt. Jetzt Gump-MUL und tiledata.mul speichern.";
                StudioUi.Info(this, "Gump(s) und Tiledata wurden gesetzt.\nBitte \"Gump-MUL speichern\" und \"tiledata.mul speichern\" ausführen.");
            }
        }

        private bool ApplyGumps(bool message)
        {
            if (_gump == null)
            {
                StudioUi.Info(this, "Bitte zuerst ein Item-Bild laden.");
                return false;
            }
            int anim = (int)_numAnim.Value;
            int male = MaleGumpOffset + anim, female = FemaleGumpOffset + anim;
            try
            {
                int count = Gumps.GetCount();
                if (male >= count)
                {
                    StudioUi.Error(this, $"Gump {male} liegt außerhalb der Gump-Datei (max. {count - 1}).");
                    return false;
                }
                if (Gumps.IsValidIndex(male) && !StudioUi.Ask(this, $"Gump {male} existiert bereits. Überschreiben?"))
                {
                    return false;
                }
                Gumps.ReplaceGump(male, StudioImaging.To1555(_gump));
                string info = $"Gump {male}";
                if (_chkFemale.Checked && female < count)
                {
                    Gumps.ReplaceGump(female, StudioImaging.To1555(_gump));
                    info += $" und {female}";
                }
                StudioUi.MarkChanged("Gumps");
                _status.Text = info + " gesetzt.";
                if (message)
                {
                    StudioUi.Info(this, info + " gesetzt. Mit \"Gump-MUL speichern\" sichern.");
                }
                return true;
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
                return false;
            }
        }

        private bool ApplyTileData(bool message)
        {
            if (!StudioUi.TryParseId(_txtItemId.Text, out int id))
            {
                StudioUi.Error(this, "Ungültige Item-ID.");
                return false;
            }
            try
            {
                if (TileData.ItemTable == null || id < 0 || id >= TileData.ItemTable.Length)
                {
                    StudioUi.Error(this, "Item-ID liegt außerhalb der Tiledata.");
                    return false;
                }
                TileData.ItemTable[id].Name = _txtName.Text.Length > 20 ? _txtName.Text.Substring(0, 20) : _txtName.Text;
                TileData.ItemTable[id].Animation = (short)_numAnim.Value;
                TileData.ItemTable[id].Quality = (byte)_cbLayer.SelectedIndex;
                TileData.ItemTable[id].Weight = (byte)_numWeight.Value;
                TileData.ItemTable[id].Hue = (byte)_numTileHue.Value;
                TileData.ItemTable[id].Height = (byte)_numHeight.Value;
                TileFlag flags = TileData.ItemTable[id].Flags | TileFlag.Wearable;
                flags = _chkPartialHue.Checked ? flags | TileFlag.PartialHue : flags & ~TileFlag.PartialHue;
                if (_cbLayer.SelectedIndex is 1 or 2)
                {
                    flags |= TileFlag.Weapon;
                }
                TileData.ItemTable[id].Flags = flags;
                StudioUi.MarkChanged("TileData");
                _status.Text = $"Tiledata 0x{id:X4} gesetzt (Layer {_cbLayer.SelectedIndex}, Animation {(int)_numAnim.Value}).";
                if (message)
                {
                    StudioUi.Info(this, _status.Text + "\nMit \"tiledata.mul speichern\" sichern.");
                }
                return true;
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
                return false;
            }
        }

        private void SaveGumps()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Gumps.Save(StudioUi.OutputPath);
                StudioUi.ResetChanged("Gumps");
                StudioUi.Info(this, $"gumpart.mul / gumpidx.mul gespeichert in:\n{StudioUi.OutputPath}");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SaveTileData()
        {
            try
            {
                string file = System.IO.Path.Combine(StudioUi.OutputPath, "tiledata.mul");
                TileData.SaveTileData(file);
                StudioUi.ResetChanged("TileData");
                StudioUi.Info(this, "tiledata.mul gespeichert:\n" + file);
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void SaveGumpPng()
        {
            if (_gump == null)
            {
                return;
            }
            using var dlg = new SaveFileDialog { Filter = "PNG|*.png", FileName = $"gump_{MaleGumpOffset + (int)_numAnim.Value}.png", InitialDirectory = StudioUi.OutputPath };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _gump.Save(dlg.FileName, ImageFormat.Png);
            }
        }

        private void CopyScript()
        {
            StudioUi.TryParseId(_txtItemId.Text, out int id);
            int layer = _cbLayer.SelectedIndex;
            string layerName = layer < _layers.Length ? _layers[layer].Substring(_layers[layer].IndexOf('-') + 2) : layer.ToString();
            var sb = new StringBuilder();
            sb.AppendLine($"// Item 0x{id:X4} \"{_txtName.Text}\" – Layer {layer} ({layerName}), Animation {(int)_numAnim.Value}");
            sb.AppendLine($"// Paperdoll-Gump männlich {MaleGumpOffset + (int)_numAnim.Value}, weiblich {FemaleGumpOffset + (int)_numAnim.Value}");
            sb.AppendLine("// ServUO / RunUO:");
            string cls = MakeClassName(_txtName.Text);
            sb.AppendLine($"public class {cls} : BaseClothing");
            sb.AppendLine("{");
            sb.AppendLine("    [Constructable]");
            sb.AppendLine($"    public {cls}() : this(0) {{ }}");
            sb.AppendLine();
            sb.AppendLine("    [Constructable]");
            sb.AppendLine($"    public {cls}(int hue) : base(0x{id:X4}, Layer.{LayerEnumName(layer)}, hue)");
            sb.AppendLine("    {");
            sb.AppendLine($"        Name = \"{_txtName.Text}\";");
            sb.AppendLine($"        Weight = {(int)_numWeight.Value}.0;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public {cls}(Serial serial) : base(serial) {{ }}");
            sb.AppendLine();
            sb.AppendLine("    public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }");
            sb.AppendLine("    public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }");
            sb.AppendLine("}");
            Clipboard.SetText(sb.ToString());
            _status.Text = "Script-Snippet in die Zwischenablage kopiert.";
        }

        private static string MakeClassName(string name)
        {
            var sb = new StringBuilder();
            bool upper = true;
            foreach (char c in name ?? string.Empty)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(upper ? char.ToUpperInvariant(c) : c);
                    upper = false;
                }
                else
                {
                    upper = true;
                }
            }
            if (sb.Length == 0 || char.IsDigit(sb[0]))
            {
                sb.Insert(0, "Custom");
            }
            return sb.ToString();
        }

        private static string LayerEnumName(int layer)
        {
            string[] names =
            {
                "Invalid", "OneHanded", "TwoHanded", "Shoes", "Pants", "Shirt", "Helm", "Gloves", "Ring", "Talisman",
                "Neck", "Hair", "Waist", "InnerTorso", "Bracelet", "Face", "FacialHair", "MiddleTorso", "Earrings",
                "Arms", "Cloak", "Backpack", "OuterTorso", "OuterLegs", "InnerLegs", "Mount"
            };
            return layer >= 0 && layer < names.Length ? names[layer] : "InnerTorso";
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _item?.Dispose();
            _gump?.Dispose();
            _body?.Dispose();
            _preview.Image?.Dispose();
            _itemView.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
