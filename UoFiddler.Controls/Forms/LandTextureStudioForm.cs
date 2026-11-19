/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Land tile / texture studio: one image -> a land tile (44x44 art) AND its texture (64/128) in the orientation the
 * client uses, check on a 3x3 map preview, then patch both into their slots and link them in the land tiledata
 * (texture id, name, flags) plus radar colour. See LandTextureMath for the orientation rules.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class LandTextureStudioForm : Form
    {
        private static LandTextureStudioForm _instance;

        private Bitmap _source;           // the loaded image (own copy)
        private ushort[] _landOriginal;   // land mode: the land tile as loaded (44x44, diamond)
        private ushort[] _baseTexture;    // texture before turning / mirroring
        private ushort[] _texture;        // current texture
        private ushort[] _land;           // current land tile
        private int _size = 64;
        private bool _turned;             // texture turned / mirrored by the user
        private bool _updatingIds;

        private Bitmap _sourceImage, _textureImage, _landImage, _mapImage;

        public LandTextureStudioForm()
        {
            InitializeComponent();
            _modeBox.SelectedIndex = 0;
            _fitBox.SelectedIndex = 0;
            _sizeBox.SelectedIndex = 0;
            _texIdBox.ReadOnly = _sameIdCheck.Checked;

            _tips.SetToolTip(_modeBox, "Texture: a square picture seen from above - the land tile is made by turning it 45 degrees like the client.\n" +
                                       "Land tile: a 44x44 diamond - the texture is made from it (inverse).");
            _tips.SetToolTip(_alignButton, "Tries the 8 turns / mirrors of the texture and takes the one whose land tile looks most like the given land tile.");
            _tips.SetToolTip(_sameIdCheck, "Classic convention: texture id = land tile id (e.g. grass 0x3 uses texture 0x3).");
            _tips.SetToolTip(_writeTexCheck, "Off: no new texture - the land tile is only linked to the existing texture id (e.g. one chosen with 'From texture').");
            _tips.SetToolTip(_mapBox, "The land tile as it lies on the map, 3 x 3 times. The edges should continue without seams.");
            _tips.SetToolTip(_patchButton, "Writes the land tile and the texture into the slots (in memory) and sets texture id, name, flags and radar colour " +
                                           "in the land tiledata. 'Save files' writes art.mul, tiledata.mul, texmaps.mul, radarcol.mul (+ extensions).");

            Shown += (s, e) => UpdateTargetInfo();
        }

        /// <summary>Opens the (single) studio; <paramref name="landId"/> &gt;= 0 loads that land tile and its texture.</summary>
        public static void ShowStudio(int landId = -1)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new LandTextureStudioForm();
                _instance.Show();
            }
            else
            {
                if (_instance.WindowState == FormWindowState.Minimized)
                {
                    _instance.WindowState = FormWindowState.Normal;
                }

                _instance.Activate();
            }

            if (landId >= 0)
            {
                _instance.LoadLand(landId, true);
            }
        }

        private void DisposeImages()
        {
            foreach (PictureBox box in new[] { _sourceBox, _textureBox, _landBox, _mapBox })
            {
                if (box != null)
                {
                    box.Image = null; // detach before dispose
                }
            }

            _sourceImage?.Dispose();
            _textureImage?.Dispose();
            _landImage?.Dispose();
            _mapImage?.Dispose();
            _source?.Dispose();
            _sourceImage = _textureImage = _landImage = _mapImage = null;
            _source = null;
        }

        #region Loading

        private void OnLoadFile(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Image for the land tile / texture",
                Filter = "Images (*.png;*.bmp;*.tif;*.tiff;*.jpg;*.jpeg;*.gif)|*.png;*.bmp;*.tif;*.tiff;*.jpg;*.jpeg;*.gif|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                using var img = Image.FromFile(dlg.FileName);
                SetSource(new Bitmap(img), Path.GetFileName(dlg.FileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"The image can not be loaded:{Environment.NewLine}{ex.Message}", "Load", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnPaste(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show(this, "There is no image in the clipboard.", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using Image img = Clipboard.GetImage();
            SetSource(new Bitmap(img), "clipboard");
        }

        private void OnLoadFromLand(object sender, EventArgs e)
        {
            if (TryId(_fromLandBox.Text, out int id))
            {
                LoadLand(id, false);
            }
        }

        private void OnLoadFromTexture(object sender, EventArgs e)
        {
            if (!TryId(_fromTexBox.Text, out int id) || !Textures.TestTexture(id))
            {
                MessageBox.Show(this, "This texture id has no texture.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Bitmap tex = Textures.GetTexture(id);
            SetSource(new Bitmap(tex), $"texture 0x{id:X} ({tex.Width}x{tex.Height})", false);
            _sizeBox.SelectedIndex = tex.Width >= 128 ? 1 : 0;
            if (!_writeTexCheck.Checked || _texIdBox.Text.Length == 0)
            {
                SetTexId(id);
            }
        }

        /// <summary>Loads a land tile as source; with <paramref name="asTarget"/> it also becomes the target and its texture is shown as link.</summary>
        private void LoadLand(int id, bool asTarget)
        {
            if (!Art.IsValidLand(id))
            {
                MessageBox.Show(this, $"Land tile 0x{id:X} has no graphic.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _fromLandBox.Text = $"0x{id:X}";
            _alignBox.Text = $"0x{id:X}";
            SetSource(new Bitmap(Art.GetLand(id)), $"land tile 0x{id:X}", true);
            if (asTarget)
            {
                SetLandId(id);
                int tex = id < TileData.LandTable?.Length ? TileData.LandTable[id].TextureId : 0;
                if (tex > 0)
                {
                    _sameIdCheck.Checked = tex == id;
                    SetTexId(tex);
                    _sizeBox.SelectedIndex = Textures.TestTexture(tex) && Textures.GetTexture(tex)?.Width >= 128 ? 1 : 0;
                }

                _nameBox.Text = TileData.LandTable != null && id < TileData.LandTable.Length ? TileData.LandTable[id].Name : string.Empty;
            }
        }

        private void SetSource(Bitmap bmp, string info, bool? isLand = null)
        {
            _source?.Dispose();
            _source = bmp;
            bool land = isLand ?? (bmp.Width == 44 && bmp.Height == 44);
            _modeBox.SelectedIndex = land ? 1 : 0; // calls Recompute when it changes
            _sourceInfo.Text = $"{info}: {bmp.Width}x{bmp.Height}";
            Recompute();
        }

        #endregion

        #region Computing

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            Recompute();
        }

        /// <summary>Source -> base texture (and land tile in land mode); turning / mirroring starts again.</summary>
        private void Recompute()
        {
            if (_source == null || _modeBox.SelectedIndex < 0 || _sizeBox.SelectedIndex < 0 || _fitBox.SelectedIndex < 0)
            {
                return;
            }

            _size = _sizeBox.SelectedIndex == 1 ? 128 : 64;
            bool landMode = _modeBox.SelectedIndex == 1;
            if (landMode)
            {
                using Bitmap scaled = Scale(_source, 44, false);
                _landOriginal = LandTextureMath.CutToDiamond(ArtLibraryCodec.ToPixels(scaled));
                _baseTexture = LandTextureMath.TextureFromLand(_landOriginal, _size);
            }
            else
            {
                _landOriginal = null;
                using Bitmap scaled = Scale(_source, _size, _fitBox.SelectedIndex == 0);
                ushort[] px = ArtLibraryCodec.ToPixels(scaled);
                for (int i = 0; i < px.Length; ++i)
                {
                    px[i] |= 0x8000; // textures are always opaque
                }

                _baseTexture = px;
            }

            _texture = (ushort[])_baseTexture.Clone();
            _turned = false;
            ShowSource();
            UpdateResult();
        }

        /// <summary>Square <paramref name="size"/> x <paramref name="size"/>: crop the middle square or stretch; 1:1 sizes stay pixel exact.</summary>
        private static Bitmap Scale(Bitmap src, int size, bool crop)
        {
            var result = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(result);
            g.Clear(Color.Black);
            bool exact = src.Width == size && src.Height == size;
            g.InterpolationMode = exact ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Rectangle from = new Rectangle(0, 0, src.Width, src.Height);
            if (crop && src.Width != src.Height)
            {
                int s = Math.Min(src.Width, src.Height);
                from = new Rectangle((src.Width - s) / 2, (src.Height - s) / 2, s, s);
            }

            using var attributes = new ImageAttributes();
            attributes.SetWrapMode(WrapMode.TileFlipXY); // no dark seam at the border when scaling
            g.DrawImage(src, new Rectangle(0, 0, size, size), from.X, from.Y, from.Width, from.Height, GraphicsUnit.Pixel, attributes);
            return result;
        }

        private void UpdateResult()
        {
            if (_texture == null)
            {
                return;
            }

            // land mode and not turned: keep the original land tile pixel exact
            _land = _landOriginal != null && !_turned ? _landOriginal : LandTextureMath.LandFromTexture(_texture, _size);

            Show(_textureBox, ref _textureImage, Enlarge(_texture, _size, 256 / _size));
            Show(_landBox, ref _landImage, Enlarge(_land, 44, 4));
            Show(_mapBox, ref _mapImage, MapPreview(_land));

            string text = $"Texture {_size}x{_size}" + (_turned ? " (turned / mirrored)" : string.Empty) + ".  ";
            if (_landOriginal != null)
            {
                text += $"Land tile -> texture -> land tile: {LandTextureMath.MatchPercent(LandTextureMath.LandFromTexture(_texture, _size), _landOriginal):0.0} % the same.";
            }

            if (TryId(_alignBox.Text, out int alignId) && Art.IsValidLand(alignId))
            {
                ushort[] reference = ArtLibraryCodec.ToPixels(Art.GetLand(alignId));
                text += $"  Compared with land tile 0x{alignId:X}: {LandTextureMath.MatchPercent(LandTextureMath.LandFromTexture(_texture, _size), reference):0.0} %.";
            }

            _matchLabel.Text = text;
            UpdateTargetInfo();
        }

        private void ShowSource()
        {
            if (_source == null)
            {
                return;
            }

            int max = 256;
            float scale = Math.Min(1f * max / _source.Width, 1f * max / _source.Height);
            if (scale >= 1)
            {
                scale = Math.Max(1, (int)scale); // whole numbers: pixel exact
            }

            int w = Math.Max(1, (int)(_source.Width * scale)), h = Math.Max(1, (int)(_source.Height * scale));
            var img = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.InterpolationMode = scale >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(_source, 0, 0, w, h);
            }

            Show(_sourceBox, ref _sourceImage, img);
        }

        private static void Show(PictureBox box, ref Bitmap current, Bitmap next)
        {
            Bitmap old = current;
            box.Image = next;
            current = next;
            old?.Dispose();
        }

        private static Bitmap Enlarge(ushort[] px, int size, int factor)
        {
            using Bitmap small = ArtLibraryCodec.ToBitmap(px, size, size);
            var big = new Bitmap(size * factor, size * factor);
            using Graphics g = Graphics.FromImage(big);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(small, 0, 0, size * factor, size * factor);
            return big;
        }

        /// <summary>3 x 3 land tiles in the map lattice (x down-right, y down-left), enlarged 2x.</summary>
        private static Bitmap MapPreview(ushort[] land)
        {
            const int w = 132, h = 132;
            using var canvas = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(canvas))
            using (Bitmap tile = ArtLibraryCodec.ToBitmap(land, 44, 44))
            {
                g.Clear(Color.FromArgb(40, 40, 40));
                for (int j = 0; j < 3; ++j)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        g.DrawImageUnscaled(tile, 44 + ((i - j) * 22), (i + j) * 22);
                    }
                }
            }

            var big = new Bitmap(w * 2, h * 2);
            using (Graphics g = Graphics.FromImage(big))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(canvas, 0, 0, w * 2, h * 2);
            }

            return big;
        }

        #endregion

        #region Orientation

        private void Turn(Func<ushort[], ushort[]> op)
        {
            if (_texture == null)
            {
                return;
            }

            _texture = op(_texture);
            _turned = true;
            UpdateResult();
        }

        private void OnRotateLeft(object sender, EventArgs e) => Turn(t => LandTextureMath.Rotate90(t, _size, false));

        private void OnRotateRight(object sender, EventArgs e) => Turn(t => LandTextureMath.Rotate90(t, _size, true));

        private void OnMirrorH(object sender, EventArgs e) => Turn(t => LandTextureMath.Mirror(t, _size, true));

        private void OnMirrorV(object sender, EventArgs e) => Turn(t => LandTextureMath.Mirror(t, _size, false));

        private void OnResetOrientation(object sender, EventArgs e)
        {
            if (_baseTexture == null)
            {
                return;
            }

            _texture = (ushort[])_baseTexture.Clone();
            _turned = false;
            UpdateResult();
        }

        private void OnAutoAlign(object sender, EventArgs e)
        {
            if (_baseTexture == null)
            {
                return;
            }

            if (!TryId(_alignBox.Text, out int id) || !Art.IsValidLand(id))
            {
                MessageBox.Show(this, "Enter the id of a land tile with a graphic to align the texture to (e.g. the land tile this texture belongs to).",
                    "Auto align", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ushort[] reference = ArtLibraryCodec.ToPixels(Art.GetLand(id));
            int best = LandTextureMath.BestOrientation(_baseTexture, _size, reference, out double match);
            _texture = LandTextureMath.Orient(_baseTexture, _size, best);
            _turned = best != 0;
            UpdateResult();
            string[] names = { "as it is", "turned right", "turned 180", "turned left", "mirrored", "mirrored + turned right", "mirrored + turned 180", "mirrored + turned left" };
            _statusLabel.ForeColor = match >= 80 ? Color.DarkGreen : Color.DarkOrange;
            _statusLabel.Text = $"Best orientation for land tile 0x{id:X}: {names[best]} ({match:0.0} % the same)" +
                                (match < 80 ? " - the pictures are quite different, check by eye." : ".");
        }

        #endregion

        #region Target slots

        private void SetLandId(int id)
        {
            _updatingIds = true;
            _landIdBox.Text = $"0x{id:X}";
            if (_sameIdCheck.Checked)
            {
                _texIdBox.Text = $"0x{id:X}";
            }

            _updatingIds = false;
            UpdateTargetInfo();
        }

        private void SetTexId(int id)
        {
            _updatingIds = true;
            _texIdBox.Text = $"0x{id:X}";
            _updatingIds = false;
            UpdateTargetInfo();
        }

        private void OnTargetChanged(object sender, EventArgs e)
        {
            if (_updatingIds)
            {
                return;
            }

            if (sender == _landIdBox && _sameIdCheck.Checked)
            {
                _updatingIds = true;
                _texIdBox.Text = _landIdBox.Text;
                _updatingIds = false;
            }

            UpdateTargetInfo();
        }

        private void OnSameIdChanged(object sender, EventArgs e)
        {
            _texIdBox.ReadOnly = _sameIdCheck.Checked;
            if (_sameIdCheck.Checked)
            {
                _texIdBox.Text = _landIdBox.Text;
            }
        }

        private static bool IsLegalTexture(int id) =>
            (id > 0 && id < Math.Min(Textures.GetIdxLength(), Textures.LegacyCount)) || Textures.Extension.Contains(id);

        private void UpdateTargetInfo()
        {
            if (_landIdInfo == null)
            {
                return;
            }

            _landIdInfo.Text = !TryId(_landIdBox.Text, out int land) ? "enter an id (0x... or decimal)"
                : !Art.IsLegalLandId(land) ? $"not a legal land id (max 0x{Art.GetMaxLandId():X})"
                : Art.IsValidLand(land) ? "USED - will be replaced" : "free";
            _landIdInfo.ForeColor = _landIdInfo.Text.StartsWith("USED") ? Color.DarkOrange : _landIdInfo.Text == "free" ? Color.DarkGreen : Color.DarkRed;

            _texIdInfo.Text = !TryId(_texIdBox.Text, out int tex) ? "enter an id"
                : !IsLegalTexture(tex) ? $"not a legal texture id (texidx.mul: 1 - 0x{Math.Min(Textures.GetIdxLength(), Textures.LegacyCount) - 1:X}" +
                                         (Textures.HasExtension ? $", extension 0x{Textures.Extension.BaseId:X} - 0x{Textures.Extension.MaxId:X})" : ")")
                : Textures.TestTexture(tex) ? (_writeTexCheck.Checked ? "USED - will be replaced" : "used - the land tile will be linked to it")
                : (_writeTexCheck.Checked ? "free" : "EMPTY - nothing to link to");
            _texIdInfo.ForeColor = _texIdInfo.Text.StartsWith("USED") || _texIdInfo.Text.StartsWith("EMPTY") ? Color.DarkOrange
                : _texIdInfo.Text == "free" || _texIdInfo.Text.StartsWith("used") ? Color.DarkGreen : Color.DarkRed;

            // the link that is in the tiledata now
            if (land >= 0 && TileData.LandTable != null && land < TileData.LandTable.Length)
            {
                int now = TileData.LandTable[land].TextureId;
                string match = string.Empty;
                if (now > 0 && Textures.TestTexture(now) && Art.IsValidLand(land))
                {
                    Bitmap t = Textures.GetTexture(now);
                    double m = LandTextureMath.MatchPercent(
                        LandTextureMath.LandFromTexture(ArtLibraryCodec.ToPixels(t), t.Width), ArtLibraryCodec.ToPixels(Art.GetLand(land)));
                    match = $", the texture fits the land tile to {m:0.0} %";
                }

                _linkLabel.Text = $"TileData now: land tile 0x{land:X} '{TileData.LandTable[land].Name}' -> texture id 0x{now:X}" +
                                  (now == 0 ? " (no texture)" : Textures.TestTexture(now) ? match : " (THIS TEXTURE DOES NOT EXIST)");
            }
            else
            {
                _linkLabel.Text = string.Empty;
            }
        }

        private void OnLandNextFree(object sender, EventArgs e)
        {
            int start = TryId(_landIdBox.Text, out int s) && s < ExtendedFormat.LandBaseId ? s + 1 : 1;
            for (int id = start; id < ExtendedFormat.LandBaseId; ++id)
            {
                if (!Art.IsValidLand(id))
                {
                    SetLandId(id);
                    return;
                }
            }

            MessageBox.Show(this, "No free classic land slot after this id - try 'Next free (0x4000+)'.", "Free slot", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnLandNextFreeExtended(object sender, EventArgs e)
        {
            if (!Art.HasLandExtension)
            {
                MessageBox.Show(this, "There is no land tile extension yet - create it in the Extended tab.", "Free slot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int start = TryId(_landIdBox.Text, out int s) && s >= ExtendedFormat.LandBaseId ? s + 1 : ExtendedFormat.LandBaseId;
            for (int id = start; id <= Art.GetMaxLandId(); ++id)
            {
                if (!Art.IsValidLand(id))
                {
                    SetLandId(id);
                    return;
                }
            }

            MessageBox.Show(this, "No free slot in the land tile extension.", "Free slot", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnTexNextFree(object sender, EventArgs e)
        {
            _sameIdCheck.Checked = false;
            int start = TryId(_texIdBox.Text, out int s) ? s + 1 : 1;
            int legacy = Math.Min(Textures.GetIdxLength(), Textures.LegacyCount);
            for (int id = Math.Max(1, start); id < legacy; ++id)
            {
                if (!Textures.TestTexture(id))
                {
                    SetTexId(id);
                    return;
                }
            }

            if (Textures.HasExtension)
            {
                for (int id = Math.Max(start, Textures.Extension.BaseId); id <= Textures.Extension.MaxId; ++id)
                {
                    if (!Textures.TestTexture(id))
                    {
                        SetTexId(id);
                        return;
                    }
                }
            }

            MessageBox.Show(this, "No free texture slot found.", "Free slot", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Patch / save / show

        private void OnPatch(object sender, EventArgs e)
        {
            if (_texture == null || _land == null)
            {
                MessageBox.Show(this, "Load an image first.", "Patch", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!TryId(_landIdBox.Text, out int landId) || !Art.IsLegalLandId(landId))
            {
                MessageBox.Show(this, "Enter a legal land tile id (or use 'Next free').", "Patch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryId(_texIdBox.Text, out int texId) || !IsLegalTexture(texId))
            {
                MessageBox.Show(this, "Enter a legal texture id (or use 'Next free').", "Patch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (TileData.LandTable == null || landId >= TileData.LandTable.Length)
            {
                MessageBox.Show(this, $"The land tiledata has no entry 0x{landId:X} (sizes differ - run the error search in the Extended tab).", "Patch",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_writeTexCheck.Checked && !Textures.TestTexture(texId))
            {
                MessageBox.Show(this, $"'Write the texture' is off, but texture 0x{texId:X} is empty - the land tile would point to nothing.", "Patch",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string replace = (_writeLandCheck.Checked && Art.IsValidLand(landId) ? $"land tile 0x{landId:X}" : string.Empty) +
                             (_writeTexCheck.Checked && Textures.TestTexture(texId) ? $"{(Art.IsValidLand(landId) && _writeLandCheck.Checked ? " and " : string.Empty)}texture 0x{texId:X}" : string.Empty);
            if (replace.Length > 0 && MessageBox.Show(this, $"Replace the existing {replace}?", "Patch", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            if (_writeLandCheck.Checked)
            {
                Art.ReplaceLand(landId, ArtLibraryCodec.ToBitmap(_land, 44, 44));
                ControlEvents.FireLandTileChangeEvent(this, landId);
                Options.ChangedUltimaClass["Art"] = true;
            }

            if (_writeTexCheck.Checked)
            {
                Textures.Replace(texId, ArtLibraryCodec.ToBitmap(_texture, _size, _size));
                ControlEvents.FireTextureChangeEvent(this, texId);
                Options.ChangedUltimaClass["Texture"] = true;
            }

            LandData data = TileData.LandTable[landId];
            data.TextureId = texId;
            if (_nameBox.Text.Trim().Length > 0)
            {
                data.Name = _nameBox.Text.Trim();
            }

            if (TryId(_flagsFromBox.Text, out int flagsFrom) && flagsFrom < TileData.LandTable.Length)
            {
                data.Flags = TileData.LandTable[flagsFrom].Flags;
            }

            TileData.LandTable[landId] = data;
            ControlEvents.FireTileDataChangeEvent(this, ControlEvents.TileDataLandEventId(landId));
            Options.ChangedUltimaClass["TileData"] = true;

            if (_radarCheck.Checked)
            {
                RadarCol.SetLandColor(landId, LandTextureMath.Average(_land));
                Options.ChangedUltimaClass["RadarColor"] = true;
            }

            UpdateTargetInfo();
            _statusLabel.ForeColor = Color.DarkGreen;
            _statusLabel.Text = $"Patched: land tile 0x{landId:X}{(_writeLandCheck.Checked ? string.Empty : " (only tiledata)")} -> texture 0x{texId:X}" +
                                $"{(_writeTexCheck.Checked ? $" ({_size}x{_size})" : " (existing)")}. Not saved yet - 'Save files' writes them.";
        }

        private void OnSaveFiles(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show(this, "The output folder is not set or does not exist (Options).", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(this, $"Write into {path}:{Environment.NewLine}art.mul / artidx.mul (+ land and item extensions){Environment.NewLine}" +
                                      $"tiledata.mul (+ extensions){Environment.NewLine}texidx.mul / texmaps.mul (+ extension){Environment.NewLine}radarcol.mul (+ extensions)" +
                                      $"{Environment.NewLine}{Environment.NewLine}This can take a while. Continue?",
                    "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Art.Save(path);
                TileData.SaveTileData(Path.Combine(path, "tiledata.mul"));
                Textures.Save(path);
                RadarCol.Save(Path.Combine(path, "radarcol.mul"));
                Options.ChangedUltimaClass["Art"] = false;
                Options.ChangedUltimaClass["TileData"] = false;
                Options.ChangedUltimaClass["Texture"] = false;
                Options.ChangedUltimaClass["RadarColor"] = false;
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = $"Saved to {path}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Saving failed: {ex.Message}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OnShowLand(object sender, EventArgs e)
        {
            if (TryId(_landIdBox.Text, out int id))
            {
                LandTilesControl.ActivateTab();
                if (!LandTilesControl.SearchGraphic(id))
                {
                    _statusLabel.Text = $"Land tile 0x{id:X} is not in the LandTiles view (empty slot? turn on 'free slots').";
                }
            }
        }

        private void OnShowTexture(object sender, EventArgs e)
        {
            if (TryId(_texIdBox.Text, out int id) && !TexturesControl.SelectInTab(id))
            {
                _statusLabel.Text = $"Texture 0x{id:X} was not found in the Textures tab.";
            }
        }

        private void OnShowTileData(object sender, EventArgs e)
        {
            if (TryId(_landIdBox.Text, out int id))
            {
                TileDataControl.ActivateTab();
                TileDataControl.Select(id, true);
            }
        }

        #endregion

        private static bool TryId(string text, out int id)
        {
            id = -1;
            text = (text ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                return false;
            }

            return Utils.ConvertStringToInt(text, out id) && id >= 0;
        }
    }
}
