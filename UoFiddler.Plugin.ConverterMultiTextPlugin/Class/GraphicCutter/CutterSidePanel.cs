using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter
{
    /// <summary>What the side panel needs from the Graphic Cutter window.</summary>
    internal interface ICutterHost
    {
        /// <summary>The current result (owned by the window – do not dispose).</summary>
        Bitmap CurrentResult { get; }

        TransparentFill SaveFill { get; }

        /// <summary>File extension of the quick save format, e.g. ".png".</summary>
        string SaveExtension { get; }

        /// <summary>Takes over the bitmap.</summary>
        void LoadIntoStage(Bitmap bitmap, string name);

        /// <summary>Takes over the bitmap.</summary>
        void LoadIntoResult(Bitmap bitmap, string name);

        void UseLandBackground(int landId);

        void ShowMessage(string text);
    }

    /// <summary>
    /// Right side of the Graphic Cutter: browser for art.mul (load art in, write results out)
    /// and the collection of cut results.
    /// </summary>
    internal sealed class CutterSidePanel : UserControl
    {
        private sealed class CollectedCut
        {
            public Bitmap Image;
            public string Label;
        }

        private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly ToolTip _toolTip = new ToolTip { AutoPopDelay = 15000, InitialDelay = 600 };

        // art.mul tab
        private readonly RadioButton _radioItems = new RadioButton { Text = "Items", Checked = true, AutoSize = true };
        private readonly RadioButton _radioLand = new RadioButton { Text = "Land tiles", AutoSize = true };
        private readonly ComboBox _filter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
        private readonly TextBox _idBox = new TextBox { Width = 90 };
        private readonly ArtTileGrid _artGrid = new ArtTileGrid { Dock = DockStyle.Fill, Text = "art.mul is not loaded.\nLoad the client files in UoFiddler." };
        private readonly Panel _artPreview = new Panel { Dock = DockStyle.Top, Height = 120 };
        private readonly Label _artInfo = new Label { Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleLeft };

        // collection tab
        private readonly ArtTileGrid _collectionGrid = new ArtTileGrid { Dock = DockStyle.Fill, Text = "The collection is empty.\nAdd results with Ctrl+Enter or the + button.\n\nDie Sammlung ist leer.\nErgebnisse mit Strg+Enter oder + hinzufügen." };
        private readonly Label _collectionInfo = new Label { Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleLeft };
        private readonly TextBox _collectionStartId = new TextBox { Width = 80 };
        private readonly CheckBox _collectionSkipUsed = new CheckBox { Text = "Skip used IDs", Checked = true, AutoSize = true };
        private readonly List<CollectedCut> _collection = new List<CollectedCut>();

        private bool[] _usedItems;
        private bool[] _usedLand;
        private bool _artLoaded;
        private bool _loadQueued;

        public CutterSidePanel()
        {
            Size = new Size(300, 600);
            _tabs.TabPages.Add(BuildArtPage());
            _tabs.TabPages.Add(BuildCollectionPage());
            Controls.Add(_tabs);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal ICutterHost Host { get; set; }

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private ArtKind Kind => _radioLand.Checked ? ArtKind.Land : ArtKind.Item;

        private bool[] Used => Kind == ArtKind.Land ? _usedLand : _usedItems;

        private static Button MakeButton(string text, EventHandler click)
        {
            var b = new Button { Text = text, Dock = DockStyle.Fill, Margin = new Padding(2), Height = 28, UseVisualStyleBackColor = true };
            b.Click += click;
            return b;
        }

        private static TableLayoutPanel MakeButtonTable(int rows)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Bottom, ColumnCount = 2, RowCount = rows, Height = rows * 32 + 4, Padding = new Padding(0, 2, 0, 2) };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < rows; i++)
            {
                t.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            }
            return t;
        }

        #region art.mul tab

        private TabPage BuildArtPage()
        {
            var page = new TabPage("art.mul") { Padding = new Padding(3) };

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, WrapContents = true };
            _filter.Items.AddRange(new object[] { "All", "Used", "Free" });
            _filter.SelectedIndex = 1;
            top.Controls.Add(_radioItems);
            top.Controls.Add(_radioLand);
            top.Controls.Add(_filter);
            top.Controls.Add(new Label { Text = "ID:", AutoSize = true, Margin = new Padding(3, 7, 0, 0) });
            top.Controls.Add(_idBox);
            var go = new Button { Text = "Go", Width = 40, Height = 24, UseVisualStyleBackColor = true };
            go.Click += (s, e) => GoToId();
            top.Controls.Add(go);
            var nextFree = new Button { Text = "Next free", Width = 72, Height = 24, UseVisualStyleBackColor = true };
            nextFree.Click += (s, e) => SelectNextFree();
            top.Controls.Add(nextFree);

            _radioItems.CheckedChanged += (s, e) => { if (_radioItems.Checked) RebuildArtList(); };
            _radioLand.CheckedChanged += (s, e) => { if (_radioLand.Checked) RebuildArtList(); };
            _filter.SelectedIndexChanged += (s, e) => RebuildArtList();
            _idBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    GoToId();
                }
            };

            _artGrid.ImageProvider = id => CutterArt.GetShared(Kind, id);
            _artGrid.LabelProvider = id => $"0x{id:X4}";
            _artGrid.IsMarked = id => Used != null && id < Used.Length && !Used[id];
            _artGrid.SelectionChanged += (s, e) => UpdateArtPreview();
            _artGrid.ItemActivated += (s, e) => ArtToStage();

            _artPreview.Paint += ArtPreview_Paint;
            _artPreview.BackColor = Color.FromArgb(45, 45, 48);

            var buttons = MakeButtonTable(5);
            Button toStage = MakeButton("→ Stage", (s, e) => ArtToStage());
            Button toResult = MakeButton("→ Result", (s, e) => ArtToResult());
            Button copy = MakeButton("Copy art", (s, e) => CopyArt());
            Button asBack = MakeButton("Use as background", (s, e) => UseAsBackground());
            Button write = MakeButton("Result → ID", (s, e) => WriteResult());
            Button paste = MakeButton("Clipboard → ID", (s, e) => WriteClipboard());
            Button reload = MakeButton("Reload list", (s, e) => ReloadArt());
            Button save = MakeButton("Save art.mul", (s, e) => SaveArt());
            Button remove = MakeButton("Remove ID", (s, e) => RemoveArt());
            buttons.Controls.Add(toStage, 0, 0);
            buttons.Controls.Add(toResult, 1, 0);
            buttons.Controls.Add(write, 0, 1);
            buttons.Controls.Add(paste, 1, 1);
            buttons.Controls.Add(copy, 0, 2);
            buttons.Controls.Add(asBack, 1, 2);
            buttons.Controls.Add(remove, 0, 3);
            buttons.Controls.Add(reload, 1, 3);
            buttons.Controls.Add(save, 0, 4);
            buttons.SetColumnSpan(save, 2);

            _toolTip.SetToolTip(_filter, Tip("Show all IDs, only used ones or only free ones (free = red).", "Alle IDs, nur belegte oder nur freie anzeigen (frei = rot)."));
            _toolTip.SetToolTip(_idBox, Tip("ID in hex (0x1A2B) or decimal, then Enter.", "ID hexadezimal (0x1A2B) oder dezimal, dann Enter."));
            _toolTip.SetToolTip(nextFree, Tip("Jump to the next free ID after the selected one.", "Zur nächsten freien ID nach der ausgewählten springen."));
            _toolTip.SetToolTip(_artGrid, Tip("Double click = load onto the stage.", "Doppelklick = auf die Arbeitsfläche laden."));
            _toolTip.SetToolTip(toStage, Tip("Load the selected art onto the stage to cut it.", "Ausgewählte Grafik zum Zuschneiden auf die Arbeitsfläche laden."));
            _toolTip.SetToolTip(toResult, Tip("Load the selected art directly into the result.", "Ausgewählte Grafik direkt ins Ergebnis laden."));
            _toolTip.SetToolTip(write, Tip("Write the current result into the selected ID (in memory until art.mul is saved).",
                "Aktuelles Ergebnis in die ausgewählte ID schreiben (im Speicher, bis art.mul gespeichert wird)."));
            _toolTip.SetToolTip(paste, Tip("Write the image from the clipboard into the selected ID.", "Bild aus der Zwischenablage in die ausgewählte ID schreiben."));
            _toolTip.SetToolTip(copy, Tip("Copy the selected art to the clipboard.", "Ausgewählte Grafik in die Zwischenablage kopieren."));
            _toolTip.SetToolTip(asBack, Tip("Use the selected land tile as ground on the stage.", "Ausgewählte Landkachel als Boden auf der Arbeitsfläche verwenden."));
            _toolTip.SetToolTip(remove, Tip("Remove the graphic of the selected ID.", "Grafik der ausgewählten ID entfernen."));
            _toolTip.SetToolTip(reload, Tip("Read the used / free IDs again.", "Belegte / freie IDs neu einlesen."));
            _toolTip.SetToolTip(save, Tip("Save artidx.mul and art.mul into the UoFiddler output folder.", "artidx.mul und art.mul in den UoFiddler-Ausgabeordner speichern."));

            page.Controls.Add(_artGrid);
            page.Controls.Add(_artInfo);
            page.Controls.Add(_artPreview);
            page.Controls.Add(top);
            page.Controls.Add(buttons);
            return page;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            QueueFirstLoad();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            QueueFirstLoad();
        }

        /// <summary>Reads art.mul the first time the panel is shown (not in the designer).</summary>
        private void QueueFirstLoad()
        {
            if (_loadQueued || _artLoaded || !Visible || !IsHandleCreated || DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }
            _loadQueued = true;
            BeginInvoke(new Action(() =>
            {
                _loadQueued = false;
                if (!_artLoaded)
                {
                    ReloadArt();
                }
            }));
        }

        /// <summary>Reads which ids are used. Call after art.mul was changed outside this window.</summary>
        public void ReloadArt()
        {
            _artLoaded = true;
            if (!CutterArt.IsAvailable)
            {
                _usedItems = _usedLand = null;
                _artGrid.Items = Array.Empty<int>();
                _artInfo.Text = "art.mul is not loaded.";
                return;
            }
            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _usedItems = CutterArt.ScanUsed(ArtKind.Item);
                _usedLand = CutterArt.ScanUsed(ArtKind.Land);
            }
            finally
            {
                Cursor.Current = old;
            }
            RebuildArtList();
        }

        private void RebuildArtList()
        {
            if (!_artLoaded)
            {
                return;
            }
            bool[] used = Used;
            if (used == null)
            {
                _artGrid.Items = Array.Empty<int>();
                return;
            }
            int keep = _artGrid.SelectedItem;
            var list = new List<int>(used.Length);
            for (int i = 0; i < used.Length; i++)
            {
                if (_filter.SelectedIndex == 0 || (_filter.SelectedIndex == 1 && used[i]) || (_filter.SelectedIndex == 2 && !used[i]))
                {
                    list.Add(i);
                }
            }
            _artGrid.Items = list;
            if (keep >= 0)
            {
                _artGrid.SelectItem(keep);
            }
            UpdateArtPreview();
        }

        private void GoToId()
        {
            if (!CutterImaging.TryParseId(_idBox.Text, out int id))
            {
                Host?.ShowMessage("Invalid ID. Use hex (0x1A2B) or decimal.");
                return;
            }
            // The id may be hidden by the filter: show all then.
            if (!_artGrid.Items.Contains(id) && _filter.SelectedIndex != 0)
            {
                _filter.SelectedIndex = 0;
            }
            _artGrid.SelectItem(id);
            _artGrid.Focus();
        }

        private void SelectNextFree()
        {
            int start = Math.Max(0, _artGrid.SelectedItem + 1);
            int id = CutterArt.FindFree(Kind, start);
            if (id < 0)
            {
                Host?.ShowMessage("No free ID found after the selected one.");
                return;
            }
            if (!_artGrid.Items.Contains(id))
            {
                _filter.SelectedIndex = 0;
            }
            _artGrid.SelectItem(id);
            _artGrid.Focus();
        }

        private void UpdateArtPreview()
        {
            int id = _artGrid.SelectedItem;
            if (id < 0)
            {
                _artInfo.Text = _artLoaded && Used != null ? $"{_artGrid.Items.Count} entries" : _artInfo.Text;
            }
            else
            {
                Bitmap bmp = CutterArt.GetShared(Kind, id);
                string kind = Kind == ArtKind.Land ? "Land" : "Item";
                _artInfo.Text = bmp == null ? $"{kind} {CutterImaging.IdText(id)} – free" : $"{kind} {CutterImaging.IdText(id)} – {bmp.Width}×{bmp.Height}";
                _idBox.Text = $"0x{id:X4}";
            }
            _artPreview.Invalidate();
        }

        private void ArtPreview_Paint(object sender, PaintEventArgs e)
        {
            int id = _artGrid.SelectedItem;
            if (id < 0)
            {
                return;
            }
            Bitmap bmp = CutterArt.GetShared(Kind, id);
            if (bmp == null)
            {
                TextRenderer.DrawText(e.Graphics, "free / frei", Font, _artPreview.ClientRectangle, Color.IndianRed,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            Rectangle box = Rectangle.Inflate(_artPreview.ClientRectangle, -4, -4);
            try
            {
                double scale = Math.Min(1.0, Math.Min(box.Width / (double)bmp.Width, box.Height / (double)bmp.Height));
                var size = new Size(Math.Max(1, (int)(bmp.Width * scale)), Math.Max(1, (int)(bmp.Height * scale)));
                var dest = new Rectangle(box.X + (box.Width - size.Width) / 2, box.Y + (box.Height - size.Height) / 2, size.Width, size.Height);
                CutterImaging.DrawChecker(e.Graphics, dest);
                CutterImaging.DrawFitted(e.Graphics, bmp, dest, true);
            }
            catch (Exception)
            {
                // Bitmap in use elsewhere – skip this paint.
            }
        }

        private bool CheckArt(bool needUsed)
        {
            if (!CutterArt.IsAvailable)
            {
                MessageBox.Show(this, "art.mul is not loaded. Load the client files in UoFiddler first.", "art.mul",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            int id = _artGrid.SelectedItem;
            if (id < 0)
            {
                MessageBox.Show(this, "Select an ID first.", "art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (needUsed && !CutterArt.IsValid(Kind, id))
            {
                MessageBox.Show(this, $"ID {CutterImaging.IdText(id)} is free – there is no graphic to use.", "art.mul",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private string ArtName(int id) => (Kind == ArtKind.Land ? "land " : "item ") + $"0x{id:X4}";

        private void ArtToStage()
        {
            if (!CheckArt(true))
            {
                return;
            }
            int id = _artGrid.SelectedItem;
            Bitmap bmp = CutterArt.GetCopy(Kind, id);
            if (bmp != null)
            {
                Host?.LoadIntoStage(bmp, ArtName(id));
            }
        }

        private void ArtToResult()
        {
            if (!CheckArt(true))
            {
                return;
            }
            int id = _artGrid.SelectedItem;
            Bitmap bmp = CutterArt.GetCopy(Kind, id);
            if (bmp != null)
            {
                Host?.LoadIntoResult(bmp, ArtName(id));
            }
        }

        private void CopyArt()
        {
            if (!CheckArt(true))
            {
                return;
            }
            int id = _artGrid.SelectedItem;
            using (Bitmap bmp = CutterArt.GetCopy(Kind, id))
            {
                if (bmp == null)
                {
                    return;
                }
                try
                {
                    CutterImaging.CopyToClipboard(bmp);
                    Host?.ShowMessage(ArtName(id) + " copied to the clipboard.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Copy failed:\n" + ex.Message, "Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UseAsBackground()
        {
            if (Kind != ArtKind.Land)
            {
                MessageBox.Show(this, "Choose \"Land tiles\" and select a land tile to use it as ground.", "Background",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!CheckArt(true))
            {
                return;
            }
            Host?.UseLandBackground(_artGrid.SelectedItem);
        }

        /// <summary>Asks before replacing a used id. Returns false if the user cancels.</summary>
        private bool ConfirmOverwrite(int id)
        {
            if (!CutterArt.IsValid(Kind, id))
            {
                return true;
            }
            return MessageBox.Show(this, $"{ArtName(id)} already contains a graphic.\nReplace it?", "Write to art.mul",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private void WriteBitmap(Bitmap bmp, string what)
        {
            int id = _artGrid.SelectedItem;
            if (Kind == ArtKind.Land && (bmp.Width != CutterArt.LandSize || bmp.Height != CutterArt.LandSize))
            {
                MessageBox.Show(this, $"Land tiles must be 44 × 44 pixels. The {what} is {bmp.Width} × {bmp.Height}.\nSet the cut area to 44 × 44 first.",
                    "Write to art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ConfirmOverwrite(id))
            {
                return;
            }
            try
            {
                CutterArt.Write(Kind, id, bmp, this);
                MarkUsed(Kind, id, true);
                Host?.ShowMessage($"{what} written to {ArtName(id)}. Save art.mul to keep it.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, "Write to art.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WriteResult()
        {
            if (!CheckArt(false))
            {
                return;
            }
            Bitmap result = Host?.CurrentResult;
            if (result == null)
            {
                MessageBox.Show(this, "There is no result yet.", "Write to art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            WriteBitmap(result, "Result");
        }

        private void WriteClipboard()
        {
            if (!CheckArt(false))
            {
                return;
            }
            Bitmap bmp;
            try
            {
                bmp = CutterImaging.GetClipboardImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The clipboard could not be read:\n" + ex.Message, "Paste", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (bmp == null)
            {
                Host?.ShowMessage("The clipboard contains no image.");
                return;
            }
            using (bmp)
            {
                WriteBitmap(bmp, "Clipboard image");
            }
        }

        private void RemoveArt()
        {
            if (!CheckArt(true))
            {
                return;
            }
            int id = _artGrid.SelectedItem;
            if (MessageBox.Show(this, $"Remove the graphic of {ArtName(id)}?", "Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                lock (AnimEdit.SdkSync.Lock)
                {
                    if (Kind == ArtKind.Item)
                    {
                        Ultima.Art.RemoveStatic(id);
                    }
                    else
                    {
                        Ultima.Art.RemoveLand(id);
                    }
                }
                if (Kind == ArtKind.Item)
                {
                    UoFiddler.Controls.Classes.ControlEvents.FireItemChangeEvent(this, id);
                }
                else
                {
                    UoFiddler.Controls.Classes.ControlEvents.FireLandTileChangeEvent(this, id);
                }
                UoFiddler.Controls.Classes.Options.ChangedUltimaClass["Art"] = true;
                MarkUsed(Kind, id, false);
                Host?.ShowMessage(ArtName(id) + " removed. Save art.mul to keep it.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Removing failed:\n" + ex.Message, "Remove", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Updates the used / free state after a write (also from the strips dialog).</summary>
        public void MarkUsed(ArtKind kind, int id, bool used)
        {
            bool[] list = kind == ArtKind.Land ? _usedLand : _usedItems;
            if (list != null && id >= 0 && id < list.Length)
            {
                list[id] = used;
            }
            _artGrid.Invalidate();
            UpdateArtPreview();
        }

        public void SelectArtId(ArtKind kind, int id)
        {
            if (kind == ArtKind.Land)
            {
                _radioLand.Checked = true;
            }
            else
            {
                _radioItems.Checked = true;
            }
            _tabs.SelectedIndex = 0;
            if (!_artGrid.Items.Contains(id))
            {
                _filter.SelectedIndex = 0;
            }
            _artGrid.SelectItem(id);
        }

        private void SaveArt()
        {
            if (!CutterArt.IsAvailable)
            {
                MessageBox.Show(this, "art.mul is not loaded.", "Save art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string folder = UoFiddler.Controls.Classes.Options.OutputPath;
            if (MessageBox.Show(this, $"Save artidx.mul and art.mul into:\n{folder}", "Save art.mul",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string path = CutterArt.Save();
                Host?.ShowMessage("art.mul saved in " + path);
                MessageBox.Show(this, "Saved in:\n" + path, "Save art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, "Save art.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region Collection tab

        private TabPage BuildCollectionPage()
        {
            var page = new TabPage("Collection") { Padding = new Padding(3) };

            _collectionGrid.ImageProvider = i => i >= 0 && i < _collection.Count ? _collection[i].Image : null;
            _collectionGrid.LabelProvider = i => i >= 0 && i < _collection.Count ? $"{i + 1}: {_collection[i].Image.Width}×{_collection[i].Image.Height}" : string.Empty;
            _collectionGrid.SelectionChanged += (s, e) => UpdateCollectionInfo();
            _collectionGrid.ItemActivated += (s, e) => CollectionToResult();

            var buttons = MakeButtonTable(5);
            Button toResult = MakeButton("→ Result", (s, e) => CollectionToResult());
            Button toStage = MakeButton("→ Stage", (s, e) => CollectionToStage());
            Button up = MakeButton("▲ Up", (s, e) => MoveCollectionItem(-1));
            Button down = MakeButton("▼ Down", (s, e) => MoveCollectionItem(1));
            Button remove = MakeButton("Remove", (s, e) => RemoveCollectionItem());
            Button clear = MakeButton("Clear all", (s, e) => ClearCollection());
            Button saveAll = MakeButton("Save all...", (s, e) => SaveCollection());
            Button writeAll = MakeButton("Write all to art", (s, e) => WriteCollection());
            Button freeBlock = MakeButton("Find free block", (s, e) => FindFreeBlockForCollection());
            buttons.Controls.Add(toResult, 0, 0);
            buttons.Controls.Add(toStage, 1, 0);
            buttons.Controls.Add(up, 0, 1);
            buttons.Controls.Add(down, 1, 1);
            buttons.Controls.Add(remove, 0, 2);
            buttons.Controls.Add(clear, 1, 2);
            buttons.Controls.Add(saveAll, 0, 3);
            buttons.Controls.Add(freeBlock, 1, 3);
            buttons.Controls.Add(writeAll, 0, 4);
            buttons.SetColumnSpan(writeAll, 2);

            var idRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 30 };
            idRow.Controls.Add(new Label { Text = "Item IDs from:", AutoSize = true, Margin = new Padding(3, 7, 0, 0) });
            idRow.Controls.Add(_collectionStartId);
            idRow.Controls.Add(_collectionSkipUsed);

            _toolTip.SetToolTip(_collectionGrid, Tip("Collected results. Double click = load into the result.", "Gesammelte Ergebnisse. Doppelklick = ins Ergebnis laden."));
            _toolTip.SetToolTip(saveAll, Tip("Save all pieces as files (format and fill from the main window).", "Alle Teile als Dateien speichern (Format und Füllung aus dem Hauptfenster)."));
            _toolTip.SetToolTip(writeAll, Tip("Write all pieces into art.mul as items, one ID after the other.", "Alle Teile als Items nacheinander in art.mul schreiben."));
            _toolTip.SetToolTip(freeBlock, Tip("Find enough free item IDs in a row for all pieces.", "Genug freie Item-IDs am Stück für alle Teile suchen."));
            _toolTip.SetToolTip(_collectionSkipUsed, Tip("Used IDs are skipped instead of being replaced.", "Belegte IDs werden übersprungen statt ersetzt."));

            page.Controls.Add(_collectionGrid);
            page.Controls.Add(_collectionInfo);
            page.Controls.Add(idRow);
            page.Controls.Add(buttons);
            return page;
        }

        public int CollectionCount => _collection.Count;

        /// <summary>Adds a copy of the bitmap to the collection.</summary>
        public void AddToCollection(Bitmap bitmap, string label)
        {
            _collection.Add(new CollectedCut { Image = CutterImaging.To32Bpp(bitmap), Label = label });
            RefreshCollection(_collection.Count - 1);
            _tabs.SelectedIndex = 1;
        }

        private void RefreshCollection(int select)
        {
            _collectionGrid.Items = Enumerable.Range(0, _collection.Count).ToList();
            _collectionGrid.SelectedIndex = select;
            UpdateCollectionInfo();
        }

        private void UpdateCollectionInfo()
        {
            int i = _collectionGrid.SelectedItem;
            _collectionInfo.Text = i >= 0
                ? $"{_collection.Count} pieces – #{i + 1}: {_collection[i].Image.Width}×{_collection[i].Image.Height} {_collection[i].Label}"
                : $"{_collection.Count} pieces";
        }

        private CollectedCut SelectedCut()
        {
            int i = _collectionGrid.SelectedItem;
            if (i >= 0 && i < _collection.Count)
            {
                return _collection[i];
            }
            MessageBox.Show(this, "Select a piece in the collection first.", "Collection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        private void CollectionToResult()
        {
            CollectedCut cut = SelectedCut();
            if (cut != null)
            {
                Host?.LoadIntoResult(CutterImaging.To32Bpp(cut.Image), "collection piece");
            }
        }

        private void CollectionToStage()
        {
            CollectedCut cut = SelectedCut();
            if (cut != null)
            {
                Host?.LoadIntoStage(CutterImaging.To32Bpp(cut.Image), "collection piece");
            }
        }

        private void MoveCollectionItem(int delta)
        {
            int i = _collectionGrid.SelectedItem;
            int j = i + delta;
            if (i < 0 || j < 0 || j >= _collection.Count)
            {
                return;
            }
            (_collection[i], _collection[j]) = (_collection[j], _collection[i]);
            RefreshCollection(j);
        }

        private void RemoveCollectionItem()
        {
            int i = _collectionGrid.SelectedItem;
            if (i < 0)
            {
                return;
            }
            _collection[i].Image.Dispose();
            _collection.RemoveAt(i);
            RefreshCollection(Math.Min(i, _collection.Count - 1));
        }

        private void ClearCollection()
        {
            if (_collection.Count == 0 ||
                MessageBox.Show(this, $"Remove all {_collection.Count} pieces from the collection?", "Collection",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            foreach (CollectedCut c in _collection)
            {
                c.Image.Dispose();
            }
            _collection.Clear();
            RefreshCollection(-1);
        }

        private bool CheckCollection()
        {
            if (_collection.Count > 0)
            {
                return true;
            }
            MessageBox.Show(this, "The collection is empty.", "Collection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void SaveCollection()
        {
            if (!CheckCollection())
            {
                return;
            }
            using (var dialog = new FolderBrowserDialog { Description = "Folder for the collected pieces", UseDescriptionForTitle = true })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                string ext = Host?.SaveExtension ?? ".png";
                TransparentFill fill = Host?.SaveFill ?? TransparentFill.Keep;
                try
                {
                    string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    for (int i = 0; i < _collection.Count; i++)
                    {
                        CutterImaging.SaveImage(_collection[i].Image, Path.Combine(dialog.SelectedPath, $"Cut_{stamp}_{i + 1:000}{ext}"), fill);
                    }
                    Host?.ShowMessage($"{_collection.Count} pieces saved in {dialog.SelectedPath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Saving failed:\n" + ex.Message, "Save all", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FindFreeBlockForCollection()
        {
            if (!CheckCollection() || !CutterArt.IsAvailable)
            {
                return;
            }
            CutterImaging.TryParseId(_collectionStartId.Text, out int start);
            int id = CutterArt.FindFreeBlock(ArtKind.Item, start, _collection.Count);
            if (id < 0)
            {
                Host?.ShowMessage($"No {_collection.Count} free item IDs in a row found.");
                return;
            }
            _collectionStartId.Text = $"0x{id:X4}";
            Host?.ShowMessage($"Free block: 0x{id:X4} – 0x{id + _collection.Count - 1:X4}");
        }

        private void WriteCollection()
        {
            if (!CheckCollection())
            {
                return;
            }
            if (!CutterArt.IsAvailable)
            {
                MessageBox.Show(this, "art.mul is not loaded.", "Write to art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!CutterImaging.TryParseId(_collectionStartId.Text, out int start))
            {
                MessageBox.Show(this, "Enter the first item ID (hex 0x1A2B or decimal) or use \"Find free block\".", "Write to art.mul",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int usedInRange = 0;
            if (!_collectionSkipUsed.Checked)
            {
                for (int i = 0; i < _collection.Count; i++)
                {
                    if (CutterArt.IsValid(ArtKind.Item, start + i))
                    {
                        usedInRange++;
                    }
                }
            }
            string question = $"Write {_collection.Count} pieces as items from 0x{start:X4} on?"
                              + (_collectionSkipUsed.Checked ? "\nUsed IDs are skipped." : usedInRange > 0 ? $"\n{usedInRange} used IDs will be REPLACED." : string.Empty);
            if (MessageBox.Show(this, question, "Write to art.mul", MessageBoxButtons.OKCancel,
                    usedInRange > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            try
            {
                List<int> ids = CutterArt.WriteMany(ArtKind.Item, start, _collection.Select(c => c.Image).ToList(), _collectionSkipUsed.Checked, this);
                foreach (int id in ids)
                {
                    MarkUsed(ArtKind.Item, id, true);
                }
                string range = ids.Count > 0 ? $"0x{ids[0]:X4} – 0x{ids[ids.Count - 1]:X4}" : "-";
                Host?.ShowMessage($"{ids.Count} pieces written ({range}). Save art.mul to keep them.");
                if (ids.Count < _collection.Count)
                {
                    MessageBox.Show(this, $"Only {ids.Count} of {_collection.Count} pieces fit before the last ID.", "Write to art.mul",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, "Write to art.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (CollectedCut c in _collection)
                {
                    c.Image.Dispose();
                }
                _collection.Clear();
                _toolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
