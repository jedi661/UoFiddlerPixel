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
        private bool _placing;

        /// <summary>Position / form of the item for one figure (male or female).</summary>
        private struct Placement
        {
            public int OffX, OffY, Scale, Rotate;
            public bool Flip;
        }

        private Placement _male = new Placement { OffX = 60, OffY = 90, Scale = 100 };
        private Placement _female = new Placement { OffX = 60, OffY = 90, Scale = 100 };
        private bool _editingFemale;
        private readonly System.Collections.Generic.Dictionary<int, Bitmap> _overlayCache = new System.Collections.Generic.Dictionary<int, Bitmap>();

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
            if (_initialized && !_placing)
            {
                Compose();
            }
        }

        private void CbGender_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveCurrentPlacement();
            _editingFemale = cbGender.SelectedIndex == 1;
            if (chkFemaleSeparate.Checked)
            {
                ApplyPlacement(_editingFemale ? _female : _male);
            }
            if (cbGender.SelectedIndex < 2)
            {
                numBodyGump.Value = cbGender.SelectedIndex == 0 ? 0x0C : 0x0D;
            }
            if (_initialized)
            {
                LoadBody();
            }
        }

        private void ChkFemaleSeparate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFemaleSeparate.Checked)
            {
                // start the female variant with the current placement
                Placement p = CurrentPlacement();
                _male = p;
                _female = p;
                lblStatus.Text = "Weibliche Variante separat: Figur oben auf \"Weiblich\" stellen und dort platzieren.";
            }
            else
            {
                SaveCurrentPlacement();
            }
        }

        private void BtnApplyArt_Click(object sender, EventArgs e) => ApplyArt(true);

        private void BtnSaveArt_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Art.Save(StudioUi.OutputPath);
                StudioUi.ResetChanged("Art");
                StudioUi.Info(this, $"art.mul / artidx.mul gespeichert in:\n{StudioUi.OutputPath}");
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

        private void BtnAnimStudio_Click(object sender, EventArgs e)
        {
            // equipment animations live in anim.mul under the body = tiledata "Animation" value
            var form = new AnimationStudioForm();
            form.SetTarget(1, (int)numAnim.Value, 0, 4);
            form.Show();
            lblStatus.Text = $"Animations-Studio für Body {(int)numAnim.Value} (Typ EQUIPMENT) geöffnet.";
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

        private void BtnAutoPlace_Click(object sender, EventArgs e)
        {
            if (_item == null)
            {
                StudioUi.Info(this, "Bitte zuerst ein Item-Bild laden.");
                return;
            }
            AutoPlace();
        }

        private void CbLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initialized && chkAutoPlace.Checked && _item != null)
            {
                AutoPlace();
            }
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
                    _body = UltimaImages.CopyGump(id);
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
                AutoPlace();
                lblStatus.Text = "Bild geladen und nach Layer platziert: " + dlg.FileName;
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
                using Bitmap b = UltimaImages.CopyStatic(id);
                SetItem(StudioImaging.AutoCrop(b));
                txtItemId.Text = $"0x{id:X4}";
                ReadTileData();
                AutoPlace();
                lblStatus.Text = $"Art 0x{id:X4} geladen und nach Layer \"{cbLayer.Text}\" platziert. Mit Maus / X / Y / Größe feinjustieren.";
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
                using Bitmap b = UltimaImages.CopyGump(gid);
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

        #region Auto placement

        /// <summary>
        /// Target area of each layer on the paperdoll body, as fractions of the visible body
        /// (left, top, right, bottom). 0/0 = top left of the figure, 1/1 = bottom right.
        /// </summary>
        private static RectangleF LayerArea(int layer)
        {
            switch (layer)
            {
                case 1: return RectangleF.FromLTRB(-0.05f, 0.25f, 0.38f, 0.70f);   // one handed weapon (right hand = left side)
                case 2: return RectangleF.FromLTRB(0.60f, 0.25f, 1.05f, 0.65f);    // two handed / shield (left hand)
                case 3: return RectangleF.FromLTRB(0.28f, 0.86f, 0.72f, 1.00f);    // shoes
                case 4:
                case 24: return RectangleF.FromLTRB(0.28f, 0.44f, 0.72f, 0.90f);   // pants / inner legs
                case 23: return RectangleF.FromLTRB(0.24f, 0.42f, 0.76f, 0.88f);   // outer legs (skirt)
                case 5:
                case 13:
                case 17: return RectangleF.FromLTRB(0.22f, 0.13f, 0.78f, 0.47f);   // shirt / chest / tunic
                case 22: return RectangleF.FromLTRB(0.15f, 0.12f, 0.85f, 0.95f);   // robe
                case 20: return RectangleF.FromLTRB(0.12f, 0.10f, 0.88f, 0.90f);   // cloak
                case 19: return RectangleF.FromLTRB(0.10f, 0.14f, 0.90f, 0.42f);   // arms
                case 7: return RectangleF.FromLTRB(0.02f, 0.38f, 0.98f, 0.52f);    // gloves
                case 6:
                case 11: return RectangleF.FromLTRB(0.30f, -0.02f, 0.70f, 0.12f);  // helm / hair
                case 16: return RectangleF.FromLTRB(0.38f, 0.07f, 0.62f, 0.15f);   // beard
                case 15:
                case 18: return RectangleF.FromLTRB(0.40f, 0.04f, 0.60f, 0.10f);   // face / earrings
                case 10: return RectangleF.FromLTRB(0.38f, 0.10f, 0.62f, 0.18f);   // necklace
                case 12: return RectangleF.FromLTRB(0.30f, 0.40f, 0.70f, 0.50f);   // belt / apron
                case 8:
                case 14: return RectangleF.FromLTRB(0.04f, 0.42f, 0.20f, 0.50f);   // ring / bracelet
                case 9: return RectangleF.FromLTRB(0.65f, 0.30f, 0.85f, 0.45f);    // talisman
                case 21: return RectangleF.FromLTRB(0.55f, 0.15f, 0.95f, 0.45f);   // backpack
                default: return RectangleF.FromLTRB(0.22f, 0.13f, 0.78f, 0.47f);
            }
        }

        /// <summary>
        /// Scales and moves the item into the area of the selected layer on the body
        /// (rotation and mirroring stay as they are).
        /// </summary>
        private void AutoPlace()
        {
            if (_item == null)
            {
                return;
            }

            // visible bounds of the body gump (fallback: typical paperdoll size)
            Rectangle body = _body != null ? OpaqueBounds(_body) : new Rectangle(20, 10, 180, 230);
            RectangleF f = LayerArea(cbLayer.SelectedIndex);
            var area = new RectangleF(body.X + (f.X * body.Width), body.Y + (f.Y * body.Height), f.Width * body.Width, f.Height * body.Height);

            float scale = Math.Min(area.Width / _item.Width, area.Height / _item.Height);
            scale = Math.Max(0.1f, Math.Min(8f, scale));

            _placing = true;
            try
            {
                numScale.Value = Math.Max(numScale.Minimum, Math.Min(numScale.Maximum, (decimal)Math.Round(scale * 100)));
                // size after scaling (rotation is applied around the centre)
                using (Bitmap t = TransformItem())
                {
                    int ox = (int)Math.Round(area.X + ((area.Width - t.Width) / 2f));
                    int oy = (int)Math.Round(area.Y + ((area.Height - t.Height) / 2f));
                    numOffX.Value = Math.Max(numOffX.Minimum, Math.Min(numOffX.Maximum, ox));
                    numOffY.Value = Math.Max(numOffY.Minimum, Math.Min(numOffY.Maximum, oy));
                }
            }
            finally
            {
                _placing = false;
            }
            Compose();
        }

        #endregion

        #region Placement (male / female)

        private Placement CurrentPlacement() => new Placement
        {
            OffX = (int)numOffX.Value,
            OffY = (int)numOffY.Value,
            Scale = (int)numScale.Value,
            Rotate = (int)numRotate.Value,
            Flip = chkFlip.Checked
        };

        private void ApplyPlacement(Placement p)
        {
            _placing = true;
            try
            {
                numOffX.Value = Math.Max(numOffX.Minimum, Math.Min(numOffX.Maximum, p.OffX));
                numOffY.Value = Math.Max(numOffY.Minimum, Math.Min(numOffY.Maximum, p.OffY));
                numScale.Value = Math.Max(numScale.Minimum, Math.Min(numScale.Maximum, p.Scale));
                numRotate.Value = Math.Max(numRotate.Minimum, Math.Min(numRotate.Maximum, p.Rotate));
                chkFlip.Checked = p.Flip;
            }
            finally
            {
                _placing = false;
            }
        }

        /// <summary>Stores the values of the controls in the set that is currently edited.</summary>
        private void SaveCurrentPlacement()
        {
            if (!_initialized)
            {
                return;
            }
            Placement p = CurrentPlacement();
            if (!chkFemaleSeparate.Checked)
            {
                _male = p;
                _female = p;
            }
            else if (_editingFemale)
            {
                _female = p;
            }
            else
            {
                _male = p;
            }
        }

        /// <summary>Additional gumps from the text box (dec or 0x hex, separated by comma / space).</summary>
        private System.Collections.Generic.List<Bitmap> OverlayGumps()
        {
            var list = new System.Collections.Generic.List<Bitmap>();
            foreach (string part in txtOverlayGumps.Text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!StudioUi.TryParseId(part, out int id))
                {
                    continue;
                }
                if (!_overlayCache.TryGetValue(id, out Bitmap bmp))
                {
                    try
                    {
                        bmp = UltimaImages.CopyGump(id);
                    }
                    catch
                    {
                        bmp = null;
                    }
                    _overlayCache[id] = bmp;
                }
                if (bmp != null)
                {
                    list.Add(bmp);
                }
            }
            return list;
        }

        #endregion

        #region Compose

        /// <summary>Builds the transformed item gump and the paperdoll preview.</summary>
        private void Compose()
        {
            _gump?.Dispose();
            _gump = null;
            SaveCurrentPlacement();

            int canvasW = _body?.Width ?? 260;
            int canvasH = _body?.Height ?? 237;

            if (_item != null)
            {
                _gump = BuildGump(CurrentPlacement());
                canvasW = Math.Max(canvasW, _gump.Width);
                canvasH = Math.Max(canvasH, _gump.Height);
            }

            System.Collections.Generic.List<Bitmap> overlays = OverlayGumps();
            foreach (Bitmap o in overlays)
            {
                canvasW = Math.Max(canvasW, o.Width);
                canvasH = Math.Max(canvasH, o.Height);
            }

            var preview = new Bitmap(canvasW, canvasH, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(preview))
            {
                g.Clear(Color.Transparent);
                if (_body != null && chkShowBody.Checked)
                {
                    g.DrawImageUnscaled(_body, 0, 0);
                }
                // existing equipment (all paperdoll gumps share the body origin 0/0)
                foreach (Bitmap o in overlays)
                {
                    g.DrawImageUnscaled(o, 0, 0);
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

        /// <summary>Gump = item drawn at its offset relative to the body origin (0/0), trimmed right/bottom.</summary>
        private Bitmap BuildGump(Placement p)
        {
            using Bitmap item = TransformItem(p);
            int gw = Math.Max(1, p.OffX + item.Width), gh = Math.Max(1, p.OffY + item.Height);
            using var gump = new Bitmap(gw, gh, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(gump))
            {
                g.Clear(Color.Transparent);
                g.DrawImageUnscaled(item, p.OffX, p.OffY);
            }
            return StudioImaging.ToUoColors(gump);
        }

        private Bitmap TransformItem() => TransformItem(CurrentPlacement());

        private Bitmap TransformItem(Placement p)
        {
            float scale = p.Scale / 100f;
            int w = Math.Max(1, (int)Math.Round(_item.Width * scale));
            int h = Math.Max(1, (int)Math.Round(_item.Height * scale));
            Bitmap scaled = StudioImaging.Resize(_item, w, h, scale >= 1.5f ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic);

            float angle = p.Rotate;
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

            if (p.Flip)
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
            if (!ApplyGumps(false) || !ApplyTileData(false))
            {
                return;
            }
            bool art = chkWriteArt.Checked && ApplyArt(false);
            string files = art ? "Gump-MUL, tiledata.mul und Art-MUL" : "Gump-MUL und tiledata.mul";
            lblStatus.Text = $"Gump(s), Tiledata{(art ? " und Art-Item" : string.Empty)} gesetzt. Jetzt {files} speichern.";
            StudioUi.Info(this, lblStatus.Text);
        }

        /// <summary>Writes the (untransformed) item picture as art static at the item ID.</summary>
        private bool ApplyArt(bool message)
        {
            if (_item == null)
            {
                StudioUi.Info(this, "Bitte zuerst ein Item-Bild laden.");
                return false;
            }
            if (!StudioUi.TryParseId(txtItemId.Text, out int id) || id < 0)
            {
                StudioUi.Error(this, "Ungültige Item-ID.");
                return false;
            }
            try
            {
                if (id > Art.GetMaxItemId())
                {
                    StudioUi.Error(this, $"Item-ID 0x{id:X4} liegt über der maximalen Art-ID 0x{Art.GetMaxItemId():X4}.");
                    return false;
                }
                if (Art.IsValidStatic(id) && !StudioUi.Ask(this, $"Art-Item 0x{id:X4} existiert bereits. Mit dem Item-Bild überschreiben?"))
                {
                    return false;
                }
                int max = (int)numArtMax.Value;
                using Bitmap fit = StudioImaging.ResizeFit(_item, max, max, InterpolationMode.HighQualityBicubic);
                using Bitmap hard = StudioImaging.HardAlpha(fit, 110);
                using Bitmap col = chkGray.Checked ? StudioImaging.ToGray(hard) : new Bitmap(hard);
                using Bitmap uo = StudioImaging.ToUoColors(col);
                Art.ReplaceStatic(id, StudioImaging.To1555(uo));
                StudioUi.MarkChanged("Art");
                lblStatus.Text = $"Art-Item 0x{id:X4} gesetzt ({uo.Width}x{uo.Height}).";
                if (message)
                {
                    StudioUi.Info(this, lblStatus.Text + "\nMit \"Art-MUL speichern\" sichern.");
                }
                return true;
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
                return false;
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
                SaveCurrentPlacement();
                using (Bitmap maleGump = BuildGump(_male))
                {
                    Gumps.ReplaceGump(male, StudioImaging.To1555(maleGump));
                }
                string info = $"Gump {male}";
                if (chkFemale.Checked && female < count)
                {
                    using Bitmap femaleGump = BuildGump(chkFemaleSeparate.Checked ? _female : _male);
                    Gumps.ReplaceGump(female, StudioImaging.To1555(femaleGump));
                    info += $" und {female}" + (chkFemaleSeparate.Checked ? " (separat platziert)" : string.Empty);
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
            foreach (Bitmap b in _overlayCache.Values)
            {
                b?.Dispose();
            }
            viewPaperdoll.Image?.Dispose();
            viewGump.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
