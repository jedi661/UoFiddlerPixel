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
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Ausruestungs-Generator: erzeugt aus einem Item-Bild ein Paperdoll-Gump (50000 + Animation
    /// fuer maennlich, 60000 + Animation fuer weiblich), zeigt es auf der Paperdoll-Figur an und
    /// erstellt den passenden Tiledata-Eintrag (Wearable, Layer, Animation, Gewicht, Hue).
    /// </summary>
    public partial class EquipmentGeneratorForm : Form
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
        private bool _initialized;

        public EquipmentGeneratorForm()
        {
            InitializeComponent();
            cbLayer.SelectedIndex = 13;
            cbGender.SelectedIndex = 0;
            _initialized = true;
            LoadBody();
            UpdateGumpIds();
        }

        #region Events

        private void Compose_Changed(object sender, EventArgs e)
        {
            if (_initialized)
            {
                Compose();
            }
        }

        private void CbGender_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbGender.SelectedIndex < 2)
            {
                numBodyGump.Value = cbGender.SelectedIndex == 0 ? 0x0C : 0x0D;
            }
            if (_initialized)
            {
                LoadBody();
            }
        }

        private void NumBodyGump_ValueChanged(object sender, EventArgs e)
        {
            if (_initialized)
            {
                LoadBody();
            }
        }

        private void NumZoom_ValueChanged(object sender, EventArgs e)
        {
            viewPaperdoll.Zoom = (int)numZoom.Value;
        }

        private void NumAnim_ValueChanged(object sender, EventArgs e)
        {
            UpdateGumpIds();
        }

        private void ViewPaperdoll_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void BtnLoadImage_Click(object sender, EventArgs e) => LoadItemFile();

        private void BtnLoadArt_Click(object sender, EventArgs e) => LoadItemFromArt();

        private void BtnLoadGump_Click(object sender, EventArgs e) => LoadExistingGump();

        private void BtnReadTileData_Click(object sender, EventArgs e) => ReadTileData();

        private void BtnFreeAnim_Click(object sender, EventArgs e) => FindFreeAnim();

        private void BtnApplyAll_Click(object sender, EventArgs e) => ApplyAll();

        private void BtnApplyGumps_Click(object sender, EventArgs e) => ApplyGumps(true);

        private void BtnApplyTileData_Click(object sender, EventArgs e) => ApplyTileData(true);

        private void BtnSaveGumps_Click(object sender, EventArgs e) => SaveGumps();

        private void BtnSaveTileData_Click(object sender, EventArgs e) => SaveTileData();

        private void BtnSaveGumpPng_Click(object sender, EventArgs e) => SaveGumpPng();

        private void BtnCopyScript_Click(object sender, EventArgs e) => CopyScript();

        private void UpdateGumpIds()
        {
            int anim = (int)numAnim.Value;
            lblGumpIds.Text = $"Gump männlich: {MaleGumpOffset + anim} (0x{MaleGumpOffset + anim:X})   weiblich: {FemaleGumpOffset + anim} (0x{FemaleGumpOffset + anim:X})";
        }

        #endregion

        #region Loading

        private void LoadBody()
        {
            _body?.Dispose();
            _body = null;
            try
            {
                int id = (int)numBodyGump.Value;
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
                lblStatus.Text = "Bild geladen: " + dlg.FileName;
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void LoadItemFromArt()
        {
            if (!StudioUi.TryParseId(txtArtSource.Text, out int id))
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
                txtItemId.Text = $"0x{id:X4}";
                ReadTileData();
                numScale.Value = 150;
                lblStatus.Text = $"Art 0x{id:X4} geladen. Tipp: Größe und Drehung an die Figur anpassen.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void LoadExistingGump()
        {
            int gid = ((cbGender.SelectedIndex == 1) ? FemaleGumpOffset : MaleGumpOffset) + (int)numAnim.Value;
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
                numScale.Value = 100;
                numRotate.Value = 0;
                chkFlip.Checked = false;
                numOffX.Value = r.X;
                numOffY.Value = r.Y;
                lblStatus.Text = $"Gump {gid} geladen.";
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
            if (!StudioUi.TryParseId(txtItemId.Text, out int id))
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
                txtName.Text = d.Name;
                numWeight.Value = d.Weight;
                numTileHue.Value = d.Hue;
                numHeight.Value = d.Height;
                if (d.Quality < _layers.Length && d.Wearable)
                {
                    cbLayer.SelectedIndex = d.Quality;
                }
                if (d.Animation > 0)
                {
                    numAnim.Value = Math.Min(numAnim.Maximum, d.Animation);
                }
                chkPartialHue.Checked = (d.Flags & TileFlag.PartialHue) != 0;
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
                int ox = (int)numOffX.Value, oy = (int)numOffY.Value;

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
                if (_body != null && chkShowBody.Checked)
                {
                    g.DrawImageUnscaled(_body, 0, 0);
                }
                if (_gump != null)
                {
                    Hue hue = StudioUi.GetHue((int)numHue.Value);
                    if (hue != null)
                    {
                        using Bitmap hued = StudioImaging.ApplyHue(_gump, hue, chkPartialHue.Checked);
                        g.DrawImageUnscaled(hued, 0, 0);
                    }
                    else
                    {
                        g.DrawImageUnscaled(_gump, 0, 0);
                    }
                }
            }

            Image oldPreview = viewPaperdoll.Image;
            viewPaperdoll.Image = preview;
            oldPreview?.Dispose();

            Image oldItem = viewGump.Image;
            viewGump.Image = _gump != null ? new Bitmap(_gump) : null;
            oldItem?.Dispose();

            if (_gump != null)
            {
                viewGump.Caption = $"Gump {_gump.Width}x{_gump.Height}";
            }
        }

        private Bitmap TransformItem()
        {
            float scale = (float)numScale.Value / 100f;
            int w = Math.Max(1, (int)Math.Round(_item.Width * scale));
            int h = Math.Max(1, (int)Math.Round(_item.Height * scale));
            Bitmap scaled = StudioImaging.Resize(_item, w, h, scale >= 1.5f ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic);

            float angle = (float)numRotate.Value;
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

            if (chkFlip.Checked)
            {
                scaled.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            Bitmap hard = StudioImaging.HardAlpha(scaled, 110);
            scaled.Dispose();

            if (chkGray.Checked)
            {
                Bitmap gray = StudioImaging.ToGray(hard);
                hard.Dispose();
                hard = gray;
            }
            return hard;
        }

        private void ViewPaperdoll_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _item == null)
            {
                return;
            }
            _dragging = true;
            _dragStart = e.Location;
            _dragOffset = new Point((int)numOffX.Value, (int)numOffY.Value);
        }

        private void ViewPaperdoll_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }
            int z = viewPaperdoll.Zoom;
            int nx = _dragOffset.X + ((e.X - _dragStart.X) / z);
            int ny = _dragOffset.Y + ((e.Y - _dragStart.Y) / z);
            numOffX.Value = Math.Max(numOffX.Minimum, Math.Min(numOffX.Maximum, nx));
            numOffY.Value = Math.Max(numOffY.Minimum, Math.Min(numOffY.Maximum, ny));
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
                        numAnim.Value = a;
                        lblStatus.Text = $"Freie Animation: {a} (Gump {m} / {f})";
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
                lblStatus.Text = "Gump(s) und Tiledata gesetzt. Jetzt Gump-MUL und tiledata.mul speichern.";
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
            int anim = (int)numAnim.Value;
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
                if (chkFemale.Checked && female < count)
                {
                    Gumps.ReplaceGump(female, StudioImaging.To1555(_gump));
                    info += $" und {female}";
                }
                StudioUi.MarkChanged("Gumps");
                lblStatus.Text = info + " gesetzt.";
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
            if (!StudioUi.TryParseId(txtItemId.Text, out int id))
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
                TileData.ItemTable[id].Name = txtName.Text.Length > 20 ? txtName.Text.Substring(0, 20) : txtName.Text;
                TileData.ItemTable[id].Animation = (short)numAnim.Value;
                TileData.ItemTable[id].Quality = (byte)cbLayer.SelectedIndex;
                TileData.ItemTable[id].Weight = (byte)numWeight.Value;
                TileData.ItemTable[id].Hue = (byte)numTileHue.Value;
                TileData.ItemTable[id].Height = (byte)numHeight.Value;
                TileFlag flags = TileData.ItemTable[id].Flags | TileFlag.Wearable;
                flags = chkPartialHue.Checked ? flags | TileFlag.PartialHue : flags & ~TileFlag.PartialHue;
                if (cbLayer.SelectedIndex is 1 or 2)
                {
                    flags |= TileFlag.Weapon;
                }
                TileData.ItemTable[id].Flags = flags;
                StudioUi.MarkChanged("TileData");
                lblStatus.Text = $"Tiledata 0x{id:X4} gesetzt (Layer {cbLayer.SelectedIndex}, Animation {(int)numAnim.Value}).";
                if (message)
                {
                    StudioUi.Info(this, lblStatus.Text + "\nMit \"tiledata.mul speichern\" sichern.");
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
            using var dlg = new SaveFileDialog { Filter = "PNG|*.png", FileName = $"gump_{MaleGumpOffset + (int)numAnim.Value}.png", InitialDirectory = StudioUi.OutputPath };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _gump.Save(dlg.FileName, ImageFormat.Png);
            }
        }

        private void CopyScript()
        {
            StudioUi.TryParseId(txtItemId.Text, out int id);
            int layer = cbLayer.SelectedIndex;
            string layerName = layer < _layers.Length ? _layers[layer].Substring(_layers[layer].IndexOf('-') + 2) : layer.ToString();
            var sb = new StringBuilder();
            sb.AppendLine($"// Item 0x{id:X4} \"{txtName.Text}\" – Layer {layer} ({layerName}), Animation {(int)numAnim.Value}");
            sb.AppendLine($"// Paperdoll-Gump männlich {MaleGumpOffset + (int)numAnim.Value}, weiblich {FemaleGumpOffset + (int)numAnim.Value}");
            sb.AppendLine("// ServUO / RunUO:");
            string cls = MakeClassName(txtName.Text);
            sb.AppendLine($"public class {cls} : BaseClothing");
            sb.AppendLine("{");
            sb.AppendLine("    [Constructable]");
            sb.AppendLine($"    public {cls}() : this(0) {{ }}");
            sb.AppendLine();
            sb.AppendLine("    [Constructable]");
            sb.AppendLine($"    public {cls}(int hue) : base(0x{id:X4}, Layer.{LayerEnumName(layer)}, hue)");
            sb.AppendLine("    {");
            sb.AppendLine($"        Name = \"{txtName.Text}\";");
            sb.AppendLine($"        Weight = {(int)numWeight.Value}.0;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public {cls}(Serial serial) : base(serial) {{ }}");
            sb.AppendLine();
            sb.AppendLine("    public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }");
            sb.AppendLine("    public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }");
            sb.AppendLine("}");
            Clipboard.SetText(sb.ToString());
            lblStatus.Text = "Script-Snippet in die Zwischenablage kopiert.";
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
            viewPaperdoll.Image?.Dispose();
            viewGump.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
