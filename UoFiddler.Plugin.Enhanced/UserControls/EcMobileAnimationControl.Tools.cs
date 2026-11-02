/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Animations;
using UoFiddler.Plugin.Enhanced.EcUltima.Art;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>Sprite sheets and classic → EC conversion of the mobile animations.</summary>
    public sealed partial class EcMobileAnimationControl
    {
        // ==================================================================== sprite sheets

        private void ExportSheet()
        {
            if (_animation == null || _current == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "PNG|*.png", FileName = $"{_current.Body:D6}_{_current.Action:D2}_sheet.png" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                byte[] sheet = EcAnimationTools.ToSheet(_animation, out int w, out int h, out SpriteSheetInfo info);
                using (Bitmap bitmap = EcImageHelper.ToBitmap(sheet, w, h))
                {
                    bitmap.Save(dialog.FileName, ImageFormat.Png);
                }
                File.WriteAllText(Path.ChangeExtension(dialog.FileName, ".txt"), info.ToText());
                toolStripStatusLabel.Text = $"Sheet {w} x {h} ({info.Columns} frames x 5 directions) and {Path.GetFileName(Path.ChangeExtension(dialog.FileName, ".txt"))} written.";
            }
        }

        private void ImportSheet()
        {
            if (_current == null)
            {
                return;
            }
            using (var dialog = new OpenFileDialog { Filter = "PNG|*.png|All images|*.png;*.bmp;*.gif;*.tga;*.dds" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    SpriteSheetInfo info;
                    string txt = Path.ChangeExtension(dialog.FileName, ".txt");
                    if (File.Exists(txt))
                    {
                        info = SpriteSheetInfo.FromText(File.ReadAllText(txt));
                    }
                    else
                    {
                        // no description: ask for the layout (5 rows = directions)
                        string[] v = EcDialogs.Prompt(this, "Sprite sheet layout", new[] { "Frames per direction (columns):", "Cell width:", "Cell height:", "Foot point x in the cell:", "Foot point y in the cell:" },
                            new[] { "10", "100", "120", "50", "110" });
                        if (v == null)
                        {
                            return;
                        }
                        info = new SpriteSheetInfo
                        {
                            Columns = int.Parse(v[0], CultureInfo.InvariantCulture),
                            CellWidth = int.Parse(v[1], CultureInfo.InvariantCulture),
                            CellHeight = int.Parse(v[2], CultureInfo.InvariantCulture),
                            AnchorX = int.Parse(v[3], CultureInfo.InvariantCulture),
                            AnchorY = int.Parse(v[4], CultureInfo.InvariantCulture)
                        };
                    }
                    byte[] px;
                    int w, h;
                    using (Bitmap image = EcImageHelper.LoadImageFile(dialog.FileName))
                    {
                        px = EcImageHelper.ToBgra(image, out w, out h);
                    }
                    EcAnimation animation = EcAnimationTools.FromSheet(px, w, h, info, (uint)_current.Body, (ushort)_current.Action);
                    if (!EcDialogs.Confirm(this, $"Replace body {_current.Body} action {_current.Action} with {animation.Frames.Count} frames from the sheet?"))
                    {
                        return;
                    }
                    _current.Package.SetData(_current.Entry, animation.Write());
                    toolStripButtonSave.Enabled = true;
                    ShowAction();
                    toolStripStatusLabel.Text = $"{animation.Frames.Count} frames imported – Save to keep them.";
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The sheet can't be imported.", ex);
                }
            }
        }

        // ==================================================================== classic → EC

        private static List<int> ParseActions(string text)
        {
            var actions = new List<int>();
            text = text.Trim();
            if (text.Equals("all", StringComparison.OrdinalIgnoreCase) || text == "*")
            {
                return Enumerable.Range(0, 35).ToList();
            }
            foreach (string part in text.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] range = part.Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out int a) && int.TryParse(range[1], out int b))
                {
                    for (int i = Math.Min(a, b); i <= Math.Max(a, b); i++)
                    {
                        actions.Add(i);
                    }
                }
                else if (int.TryParse(part, out int single))
                {
                    actions.Add(single);
                }
            }
            return actions.Distinct().OrderBy(x => x).ToList();
        }

        /// <summary>The 5 stored directions of a classic action (copies of the images; UOFiddler keeps its own).</summary>
        private static List<List<ClassicAnimFrame>> ClassicAction(int body, int action)
        {
            var directions = new List<List<ClassicAnimFrame>>();
            int found = 0;
            for (int dir = 0; dir < EcAnimationTools.Directions; dir++)
            {
                var list = new List<ClassicAnimFrame>();
                int hue = 0;
                Ultima.AnimationFrame[] frames = Ultima.Animations.GetAnimation(body, action, dir, ref hue, true, false);
                foreach (Ultima.AnimationFrame f in frames ?? Array.Empty<Ultima.AnimationFrame>())
                {
                    if (f?.Bitmap == null)
                    {
                        list.Add(null);
                        continue;
                    }
                    using (var copy = new Bitmap(f.Bitmap))
                    {
                        byte[] px = EcImageHelper.ToBgra(copy, out int w, out int h);
                        list.Add(new ClassicAnimFrame { Bgra = px, Width = w, Height = h, CenterX = f.Center.X, CenterY = f.Center.Y });
                        found++;
                    }
                }
                directions.Add(list);
            }
            return found == 0 ? null : directions;
        }

        private void ImportClassic()
        {
            if (_client == null)
            {
                return;
            }
            string[] v = EcDialogs.Prompt(this, "Animation from the classic client",
                new[] { "Classic body id:", "Actions (e.g. 0-4, 6 or all):", "EC body id:", "File (AnimationFrame1-6.uop):", "Enlarge to EC size 1.5 x (yes / no):" },
                new[] { _current?.Body.ToString(CultureInfo.InvariantCulture) ?? "0", "all", _current?.Body.ToString(CultureInfo.InvariantCulture) ?? "0", _current?.File ?? "AnimationFrame1.uop", "yes" });
            if (v == null || !int.TryParse(v[0], out int classicBody) || !int.TryParse(v[2], out int ecBody))
            {
                return;
            }
            List<int> actions = ParseActions(v[1]);
            EcUopPackage package = _client.GetPackage(v[3].Trim());
            if (package == null)
            {
                EcDialogs.ShowError(this, $"{v[3]} was not found.");
                return;
            }
            ArtScaleMode? scale = v[4].Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase) || v[4].Trim().StartsWith("j", StringComparison.OrdinalIgnoreCase) ? ArtScaleMode.Smooth : (ArtScaleMode?)null;

            var log = new StringBuilder();
            int written = 0;
            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (int action in actions)
                {
                    List<List<ClassicAnimFrame>> frames;
                    try
                    {
                        frames = ClassicAction(classicBody, action);
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine($"action {action}: {ex.Message}");
                        continue;
                    }
                    if (frames == null)
                    {
                        continue;
                    }
                    EcAnimation animation = EcAnimationTools.FromClassic(frames, (uint)ecBody, (ushort)action, scale);
                    string name = string.Format(CultureInfo.InvariantCulture, EcClient.AnimationFramePattern, ecBody, action);
                    package.AddOrReplace(name, animation.Write(), EcUopCompression.Zlib);
                    log.AppendLine($"action {action}: {animation.Frames.Count} frames");
                    written++;
                }
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "The classic animation can't be read (is a classic client loaded in UOFiddler?).", ex);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            if (written == 0)
            {
                EcDialogs.ShowError(this, $"Body {classicBody} has no classic frames for the actions {v[1]}.");
                return;
            }
            toolStripButtonSave.Enabled = true;
            _loaded = false;
            OnActivated();
            toolStripTextBoxSearch.Text = ecBody.ToString(CultureInfo.InvariantCulture);
            ApplyFilter();
            MessageBox.Show(this, $"Classic body {classicBody} → EC body {ecBody} in {v[3]}:\r\n{log}\r\nSave to keep them.", "Classic animation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ==================================================================== event handlers (designer)

        private void OnClickExportSheet(object sender, EventArgs e) => ExportSheet();

        private void OnClickImportSheet(object sender, EventArgs e) => ImportSheet();

        private void OnClickImportClassic(object sender, EventArgs e) => ImportClassic();
    }
}
