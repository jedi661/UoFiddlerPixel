/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>
    /// Asks where an image is saved: inside the app (image list of the MultiMap tab) or in a folder of choice.
    /// </summary>
    public class MapImageSaveDialog : Form
    {
        private readonly TextBox _name = new TextBox();
        private readonly RadioButton _internal = new RadioButton();
        private readonly RadioButton _external = new RadioButton();
        private readonly TextBox _folder = new TextBox();
        private readonly Button _browse = new Button();
        private readonly ComboBox _format = new ComboBox();
        private readonly CheckBox _addToList = new CheckBox();
        private readonly Label _preview = new Label();

        private static readonly string[] _formatNames = { "PNG", "JPEG", "BMP", "GIF", "TIFF", "ICO (256x256)" };
        private static readonly ImageFormat[] _formats =
        {
            ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.Bmp, ImageFormat.Gif, ImageFormat.Tiff, ImageFormat.Icon
        };

        private static bool _lastInternal = true;
        private static int _lastFormat;
        private static bool _lastAddToList = true;

        public MapImageSaveDialog(string name)
        {
            Text = "Save image";
            Icon = Options.GetFiddlerIcon();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(500, 270);

            var labelName = new Label { Text = "Name:", Left = 12, Top = 15, Width = 70 };
            _name.SetBounds(90, 12, 398, 23);
            _name.Text = MapImageLibrary.SafeFileName(name);

            _internal.Text = $"Inside the app (MultiMap image list, folder {MapImageLibrary.FolderName})";
            _internal.SetBounds(12, 46, 476, 22);
            _external.Text = "In a folder of my choice:";
            _external.SetBounds(12, 72, 476, 22);

            _folder.SetBounds(30, 98, 372, 23);
            _folder.Text = MapImageLibrary.LastExternalFolder ?? string.Empty;
            _browse.Text = "Browse...";
            _browse.SetBounds(408, 97, 80, 25);
            _browse.Click += OnBrowse;
            _addToList.Text = "Also show the file in the \"Load..\" list";
            _addToList.SetBounds(30, 128, 400, 22);
            _addToList.Checked = _lastAddToList;

            var labelFormat = new Label { Text = "Format:", Left = 12, Top = 165, Width = 70 };
            _format.SetBounds(90, 162, 150, 23);
            _format.DropDownStyle = ComboBoxStyle.DropDownList;
            _format.Items.AddRange(_formatNames);
            _format.SelectedIndex = Math.Max(0, Math.Min(_formatNames.Length - 1, _lastFormat));

            _preview.SetBounds(12, 196, 476, 32);
            _preview.ForeColor = SystemColors.GrayText;

            var ok = new Button { Text = "Save", DialogResult = DialogResult.None };
            ok.SetBounds(322, 234, 80, 27);
            ok.Click += OnOk;
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            cancel.SetBounds(408, 234, 80, 27);

            Controls.AddRange(new Control[] { labelName, _name, _internal, _external, _folder, _browse, _addToList, labelFormat, _format, _preview, ok, cancel });
            AcceptButton = ok;
            CancelButton = cancel;

            _internal.Checked = _lastInternal;
            _external.Checked = !_lastInternal;
            _internal.CheckedChanged += (s, e) => UpdateState();
            _external.CheckedChanged += (s, e) => UpdateState();
            _name.TextChanged += (s, e) => UpdateState();
            _folder.TextChanged += (s, e) => UpdateState();
            _format.SelectedIndexChanged += (s, e) => UpdateState();
            UpdateState();
        }

        /// <summary>True: saved into the app library</summary>
        public bool SaveInternal => _internal.Checked;

        /// <summary>Only for external: show the file in the list too</summary>
        public bool AddToList => _external.Checked && _addToList.Checked;

        public string ImageName => string.IsNullOrWhiteSpace(_name.Text) ? "MapImage" : _name.Text.Trim();

        public ImageFormat Format => _formats[Math.Max(0, _format.SelectedIndex)];

        /// <summary>Full file name for external saving</summary>
        public string ExternalFileName =>
            Path.Combine(_folder.Text.Trim(), $"{MapImageLibrary.SafeFileName(ImageName)}.{MapImageLibrary.ExtensionFor(Format)}");

        private void UpdateState()
        {
            _folder.Enabled = _browse.Enabled = _addToList.Enabled = _external.Checked;
            _preview.Text = _internal.Checked
                ? $"Stored in: {MapImageLibrary.Folder}"
                : $"File: {ExternalFileName}";
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Folder for the image", SelectedPath = _folder.Text, ShowNewFolderButton = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _folder.Text = dialog.SelectedPath;
                    _external.Checked = true;
                }
            }
        }

        private void OnOk(object sender, EventArgs e)
        {
            if (_external.Checked)
            {
                if (string.IsNullOrWhiteSpace(_folder.Text))
                {
                    MessageBox.Show(this, "Please choose a folder.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    Directory.CreateDirectory(_folder.Text.Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"The folder can not be used:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (File.Exists(ExternalFileName) &&
                    MessageBox.Show(this, $"{ExternalFileName}\nexists already. Overwrite?", Text, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                MapImageLibrary.LastExternalFolder = _folder.Text.Trim();
            }

            _lastInternal = _internal.Checked;
            _lastFormat = _format.SelectedIndex;
            _lastAddToList = _addToList.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Shows the dialog and saves the image. Returns the list entry (internal or added external) or null.
        /// <paramref name="savedFile"/> is the written file (null if cancelled).
        /// </summary>
        public static MapImageEntry SaveWithDialog(IWin32Window owner, Image image, string name, out string savedFile)
        {
            savedFile = null;
            using (var dialog = new MapImageSaveDialog(name))
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return null;
                }

                try
                {
                    if (dialog.SaveInternal)
                    {
                        MapImageEntry entry = MapImageLibrary.AddInternal(image, dialog.ImageName, dialog.Format);
                        savedFile = entry.FilePath;
                        return entry;
                    }

                    savedFile = dialog.ExternalFileName;
                    MapImageLibrary.SaveImage(image, savedFile, dialog.Format);
                    if (dialog.AddToList)
                    {
                        MapImageEntry existing = MapImageLibrary.FindByPath(savedFile);
                        if (existing != null)
                        {
                            MapImageLibrary.OnChanged();
                            return existing;
                        }

                        return MapImageLibrary.AddFile(savedFile, dialog.ImageName);
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    savedFile = null;
                    MessageBox.Show(owner, $"Saving failed:\n{ex.Message}", "Save image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }
    }
}
