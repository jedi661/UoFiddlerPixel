using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>
    /// Gump Studio: design ServUO gumps visually with the graphics of the client, test them and get the script.
    /// Layout: toolbox left, work area (picture of the gump) with the live script below, properties / elements / gump browser right.
    /// </summary>
    public partial class GumpStudioControl : UserControl
    {
        private const string ClipboardFormat = "GumpStudioElements";
        private readonly Timer _codeTimer = new Timer { Interval = 300 };
        private readonly List<string> _undo = new List<string>();
        private readonly List<string> _redo = new List<string>();
        private string _committed;
        private string _file;
        private bool _dirty;
        private bool _updatingList;

        public GumpStudioControl()
        {
            InitializeComponent();
            if (DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            foreach (int g in new[] { 1, 2, 5, 10, 20 })
            {
                tsGridSize.Items.Add(g);
            }
            tsGridSize.SelectedItem = 10;
            foreach (string z in new[] { "100%", "200%", "300%", "400%" })
            {
                tsZoom.Items.Add(z);
            }
            tsZoom.SelectedIndex = 0;
            tsGrid.Checked = tsSnap.Checked = true;
            comboTarget.SelectedIndex = 0;
            comboNewPage.SelectedIndex = 0;
            listElements.Columns.Add("#", 32);
            listElements.Columns.Add("Type", 85);
            listElements.Columns.Add("Name", 80);
            listElements.Columns.Add("Page", 40);
            listElements.Columns.Add("X, Y", 70);
            listElements.Columns.Add("ID", 80);
            listElements.Columns.Add("Group / list", 90);
            tsRefOpacity.SelectedIndex = 1;
            tsLanguage.SelectedIndex = 0;
            BuildNewMenu();
            _codeTimer.Tick += (s, e) =>
            {
                _codeTimer.Stop();
                UpdateCode();
            };
            NewDesign(GumpTemplates.All[0].Create(), "Empty window", true);
            comboBrowser.SelectedIndex = 0;
        }

        #region Helpers

        private GumpDesign Design => gumpCanvas.Design;

        private void SetMessage(string text) => statusMessage.Text = text;

        private List<GumpElement> Selected => gumpCanvas.Selection.ToList();

        private string Json() => Design.ToJson();

        /// <summary>Takes the current state into the undo history (call after every change).</summary>
        private void Commit(string what = null)
        {
            // rows of lists follow their template row
            GumpLists.Rebuild(Design);
            if (Selected.Any(x => !Design.Elements.Contains(x)))
            {
                gumpCanvas.Select(Selected.Where(Design.Elements.Contains).ToList());
            }
            string now = Json();
            if (now == _committed)
            {
                return;
            }
            if (_committed != null)
            {
                _undo.Add(_committed);
                if (_undo.Count > 100)
                {
                    _undo.RemoveAt(0);
                }
                _redo.Clear();
            }
            _committed = now;
            _dirty = true;
            AfterChange();
            if (what != null)
            {
                SetMessage(what);
            }
        }

        private void AfterChange()
        {
            gumpCanvas.UpdateSize();
            gumpCanvas.Invalidate();
            FillElementList();
            UpdatePageLabel();
            if (radioGump.Checked)
            {
                propertyGrid.Refresh();
            }
            else
            {
                propertyGrid.SelectedObjects = Selected.Cast<object>().ToArray();
            }
            tsUndo.Enabled = _undo.Count > 0;
            tsRedo.Enabled = _redo.Count > 0;
            UpdateTitle();
            _codeTimer.Stop();
            _codeTimer.Start();
        }

        private void UpdateTitle()
        {
            Form f = FindForm();
            if (f != null && f.GetType().Name == "DesignStudioForm")
            {
                f.Text = "Gump Studio – " + (string.IsNullOrEmpty(_file) ? Design.ClassName : Path.GetFileName(_file)) + (_dirty ? " *" : string.Empty);
            }
        }

        private bool ConfirmDiscard()
        {
            return !_dirty || MessageBox.Show(this, "The gump has unsaved changes. Discard them?", "Gump Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private void NewDesign(GumpDesign d, string name, bool first = false)
        {
            if (!first && !ConfirmDiscard())
            {
                return;
            }
            GumpLists.Rebuild(d);
            gumpCanvas.Design = d;
            gumpCanvas.Page = 1;
            gumpCanvas.ReferenceDesign = null;
            gumpCanvas.ReferenceImage = null;
            _imported = null;
            listTestLog.Items.Clear();
            foreach (string t in d.TestLog ?? new List<string>())
            {
                listTestLog.Items.Add(t);
            }
            chkTestLogInScript.Checked = d.IncludeTestLog;
            tsWriteBack.Enabled = !string.IsNullOrEmpty(d.SourceFile) && File.Exists(d.SourceFile);
            _undo.Clear();
            _redo.Clear();
            _committed = Json();
            _file = null;
            _dirty = false;
            AfterChange();
            SetMessage($"New gump from \"{name}\". Click a part on the left, drag pictures from the right, the script is written below.");
        }

        private int NewElementPage => comboNewPage.SelectedIndex == 1 ? gumpCanvas.Page : 0;

        #endregion

        #region Toolbox

        private void ToolboxButton_Click(object sender, EventArgs e)
        {
            string tag = (sender as Control)?.Tag as string ?? string.Empty;
            Point at = FreeSpot();
            var added = new List<GumpElement>();
            if (tag.StartsWith("preset:", StringComparison.Ordinal))
            {
                added.AddRange(Preset(tag.Substring(7), at));
            }
            else if (Enum.TryParse(tag, out ElementType type))
            {
                GumpElement el = GumpDesign.Create(type, at.X, at.Y);
                if (type == ElementType.Button || type == ElementType.ItemButton)
                {
                    el.ButtonId = NextId(x => x.ButtonId, ElementType.Button, ElementType.ItemButton);
                }
                else if (type == ElementType.Check || type == ElementType.Radio)
                {
                    el.SwitchId = NextId(x => x.SwitchId, ElementType.Check, ElementType.Radio);
                }
                else if (type == ElementType.TextEntry)
                {
                    el.EntryId = NextId(x => x.EntryId, ElementType.TextEntry);
                }
                added.Add(el);
            }
            foreach (GumpElement el in added)
            {
                el.Page = NewElementPage;
                Design.Elements.Add(el);
            }
            gumpCanvas.Select(added);
            Commit($"{added.Count} element(s) added – drag them to their place, the properties are on the right.");
            gumpCanvas.Focus();
        }

        private int NextId(Func<GumpElement, int> id, params ElementType[] types) =>
            Design.Elements.Where(x => types.Contains(x.Type)).Select(id).DefaultIfEmpty(0).Max() + 1;

        /// <summary>A place inside the first background, a bit below the last added element.</summary>
        private Point FreeSpot()
        {
            GumpElement bg = GumpRenderer.Visible(Design, gumpCanvas.Page, true).FirstOrDefault(x => x.Type == ElementType.Background);
            int x = bg != null ? bg.X + 20 : 20, y = bg != null ? bg.Y + 20 : 20;
            GumpElement sel = Selected.LastOrDefault();
            if (sel != null)
            {
                Rectangle r = GumpRenderer.Measure(sel);
                x = sel.X;
                y = r.Bottom + 6;
            }
            return new Point(x, y);
        }

        private IEnumerable<GumpElement> Preset(string name, Point at)
        {
            int nextButton = NextId(x => x.ButtonId, ElementType.Button, ElementType.ItemButton);
            int page = gumpCanvas.Page;
            switch (name)
            {
                case "ok":
                    yield return new GumpElement { Type = ElementType.Button, X = at.X, Y = at.Y, GumpId = 247, GumpId2 = 248, ButtonId = nextButton, Name = "Okay" };
                    break;
                case "cancel":
                    yield return new GumpElement { Type = ElementType.Button, X = at.X, Y = at.Y, GumpId = 241, GumpId2 = 242, ButtonId = 0, Name = "Cancel" };
                    break;
                case "next":
                    yield return new GumpElement { Type = ElementType.Button, X = at.X, Y = at.Y, GumpId = 4005, GumpId2 = 4007, ButtonType = ButtonKind.Page, TargetPage = page + 1, Name = "Next" };
                    break;
                case "back":
                    yield return new GumpElement { Type = ElementType.Button, X = at.X, Y = at.Y, GumpId = 4014, GumpId2 = 4016, ButtonType = ButtonKind.Page, TargetPage = Math.Max(1, page - 1), Name = "Back" };
                    break;
                case "close":
                    yield return new GumpElement { Type = ElementType.Button, X = at.X, Y = at.Y, GumpId = 4017, GumpId2 = 4019, ButtonId = 0, Name = "Close" };
                    break;
                case "field":
                {
                    int entry = NextId(x => x.EntryId, ElementType.TextEntry);
                    yield return new GumpElement { Type = ElementType.Background, X = at.X, Y = at.Y, Width = 200, Height = 28, GumpId = 3000 };
                    yield return new GumpElement { Type = ElementType.TextEntry, X = at.X + 7, Y = at.Y + 5, Width = 186, Height = 20, EntryId = entry, Name = "Input" + entry };
                    break;
                }
                case "title":
                    yield return new GumpElement { Type = ElementType.ImageTiled, X = at.X, Y = at.Y, Width = 260, Height = 24, GumpId = 2624 };
                    yield return new GumpElement { Type = ElementType.AlphaRegion, X = at.X, Y = at.Y, Width = 260, Height = 24 };
                    yield return new GumpElement { Type = ElementType.Html, X = at.X, Y = at.Y + 3, Width = 260, Height = 20, Text = "<CENTER><BASEFONT COLOR=#FFFFFF>Title</BASEFONT></CENTER>", Name = "Title" };
                    break;
                case "row":
                    yield return new GumpElement { Type = ElementType.Button, X = at.X, Y = at.Y, GumpId = 4005, GumpId2 = 4007, ButtonId = nextButton, Name = "Row" + nextButton };
                    yield return new GumpElement { Type = ElementType.Label, X = at.X + 40, Y = at.Y + 2, Text = "Entry " + nextButton, Hue = 1152 };
                    break;
            }
        }

        #endregion

        #region Canvas events

        private void GumpCanvas_SelectionChanged(object sender, EventArgs e)
        {
            if (radioElement.Checked)
            {
                propertyGrid.SelectedObjects = Selected.Cast<object>().ToArray();
            }
            if (!_updatingList)
            {
                _updatingList = true;
                listElements.SelectedIndices.Clear();
                foreach (GumpElement el in Selected)
                {
                    int i = Design.Elements.IndexOf(el);
                    if (i >= 0 && i < listElements.Items.Count)
                    {
                        listElements.Items[i].Selected = true;
                        listElements.Items[i].EnsureVisible();
                    }
                }
                _updatingList = false;
            }
            statusPos.Text = Selected.Count == 1 ? Selected[0].ToString() : Selected.Count > 1 ? $"{Selected.Count} elements" : string.Empty;
        }

        private void GumpCanvas_BeforeChange(object sender, EventArgs e)
        {
            // the history is taken in Commit
        }

        private void GumpCanvas_DesignChanged(object sender, EventArgs e) => Commit();

        private void GumpCanvas_StatusText(object sender, string text) => statusPos.Text = text;

        private void GumpCanvas_PreviewReply(object sender, string text) => SetMessage("Test: " + text);

        private void GumpCanvas_PageChanged(object sender, EventArgs e) => UpdatePageLabel();

        private void UpdatePageLabel()
        {
            tsPageLabel.Text = $"Page {gumpCanvas.Page} / {Design.Pages}";
        }

        #endregion

        #region Toolbar

        private void TsOpen_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscard())
            {
                return;
            }
            using (var d = new OpenFileDialog { Filter = "Gump Studio project (*.gump.json)|*.gump.json;*.json|All files|*.*" })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    GumpDesign design = GumpDesign.FromJson(File.ReadAllText(d.FileName));
                    _dirty = false;
                    NewDesign(design, Path.GetFileName(d.FileName), true);
                    _file = d.FileName;
                    UpdateTitle();
                    SetMessage("Opened " + d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The project could not be read:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TsSave_Click(object sender, EventArgs e) => Save(false);

        private void TsSaveAs_Click(object sender, EventArgs e) => Save(true);

        private void Save(bool ask)
        {
            string file = _file;
            if (ask || string.IsNullOrEmpty(file))
            {
                using (var d = new SaveFileDialog { Filter = "Gump Studio project (*.gump.json)|*.gump.json", FileName = Design.ClassName + ".gump.json" })
                {
                    if (d.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    file = d.FileName;
                }
            }
            try
            {
                File.WriteAllText(file, Json(), new UTF8Encoding(false));
                _file = file;
                _dirty = false;
                UpdateTitle();
                SetMessage("Saved " + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TsImport_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscard())
            {
                return;
            }
            using (var d = new OpenFileDialog { Filter = "C# script (*.cs)|*.cs|All files|*.*", Title = "ServUO / RunUO gump script" })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                ImportScript(d.FileName);
            }
        }

        /// <summary>Reads a ServUO script: preview window (class, picture, hints), then the gump becomes the new design.</summary>
        private void ImportScript(string file)
        {
            try
            {
                string code = File.ReadAllText(file);
                using (var f = new GumpImportForm(file, code))
                {
                    if (f.ShowDialog(this) != DialogResult.OK || f.Result == null)
                    {
                        return;
                    }
                    _dirty = false;
                    f.Result.SourceFile = file;
                    f.Result.SourceHash = GumpScriptWriter.Hash(code);
                    f.Result.SourceResolve = f.Resolved;
                    NewDesign(f.Result, Path.GetFileName(file), true);
                    _imported = f.Result.Clone();
                    SetMessage($"Imported {f.Result.Elements.Count} elements of {f.Result.ClassName} from {Path.GetFileName(file)}"
                               + (f.Hints.Count > 0 ? $" – {f.Hints.Count} hint(s) below the script." : "."));
                    foreach (string w in f.Hints.Take(100))
                    {
                        listProblems.Items.Add("Import: " + w);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Import failed:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Templates

        /// <summary>New ▾: built-in templates, then your own templates per category, then the library.</summary>
        private void BuildNewMenu()
        {
            foreach (ToolStripItem old in tsNew.DropDownItems.Cast<ToolStripItem>().ToList())
            {
                old.Dispose();
            }
            tsNew.DropDownItems.Clear();
            foreach (GumpTemplateEntry entry in GumpTemplateLibrary.BuiltIn())
            {
                tsNew.DropDownItems.Add(TemplateItem(entry, entry.Name));
            }
            List<GumpTemplateEntry> user;
            try
            {
                user = GumpTemplateLibrary.LoadUser(null);
            }
            catch (Exception)
            {
                user = new List<GumpTemplateEntry>();
            }
            if (user.Count > 0)
            {
                tsNew.DropDownItems.Add(new ToolStripSeparator());
                foreach (IGrouping<string, GumpTemplateEntry> group in user.GroupBy(u => u.Category, StringComparer.OrdinalIgnoreCase))
                {
                    var sub = new ToolStripMenuItem(group.Key);
                    foreach (GumpTemplateEntry entry in group)
                    {
                        sub.DropDownItems.Add(TemplateItem(entry, entry.Name));
                    }
                    tsNew.DropDownItems.Add(sub);
                }
            }
            tsNew.DropDownItems.Add(new ToolStripSeparator());
            var library = new ToolStripMenuItem("Template library…") { ShortcutKeyDisplayString = "Ctrl+T" };
            library.Click += MiTemplateLibrary_Click;
            tsNew.DropDownItems.Add(library);
        }

        private ToolStripMenuItem TemplateItem(GumpTemplateEntry entry, string text)
        {
            var item = new ToolStripMenuItem(text) { Tag = entry, ToolTipText = entry.Description };
            item.Click += TemplateItem_Click;
            return item;
        }

        private void TemplateItem_Click(object sender, EventArgs e)
        {
            if (((ToolStripItem)sender).Tag is GumpTemplateEntry entry)
            {
                try
                {
                    NewDesign(entry.Load(), entry.Name);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The template could not be read:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TsNew_DropDownOpening(object sender, EventArgs e) => BuildNewMenu();

        private void TsTemplates_DropDownOpening(object sender, EventArgs e)
        {
            miTemplateSaveSelection.Enabled = Selected.Count > 0;
        }

        private void MiTemplateLibrary_Click(object sender, EventArgs e)
        {
            using (var f = new GumpTemplateForm(true))
            {
                if (f.ShowDialog(this) != DialogResult.OK || f.SelectedDesign == null)
                {
                    return;
                }
                if (f.Action == TemplateAction.Open)
                {
                    NewDesign(f.SelectedDesign, f.SelectedEntry?.Name ?? "template");
                }
                else if (f.Action == TemplateAction.Insert)
                {
                    InsertTemplate(f.SelectedDesign, f.SelectedEntry?.Name ?? "template");
                }
            }
        }

        /// <summary>Adds the elements of a template / building block to the gump (same position, IDs behind the used ones).</summary>
        private void InsertTemplate(GumpDesign template, string name)
        {
            if (template.Elements.Count == 0)
            {
                SetMessage($"\"{name}\" has no elements.");
                return;
            }
            int x = template.Elements.Min(el => el.X), y = template.Elements.Min(el => el.Y);
            List<GumpElement> list = GumpTemplateLibrary.ForInsert(template, Design, x, y, NewElementPage);
            Detach(list);
            Design.Elements.AddRange(list);
            gumpCanvas.Select(list);
            Commit($"\"{name}\" inserted: {list.Count} element(s) – drag them where you want them.");
        }

        private void MiTemplateSave_Click(object sender, EventArgs e) => SaveAsTemplate(false);

        private void MiTemplateSaveSelection_Click(object sender, EventArgs e) => SaveAsTemplate(true);

        private void SaveAsTemplate(bool selection)
        {
            GumpDesign block = null;
            if (Selected.Count > 0)
            {
                block = Design.Clone();
                block.Elements = GumpDesign.ElementsFromJson(GumpDesign.ElementsToJson(Selected));
            }
            string name = selection ? "Building block" : Design.ClassName;
            using (var f = new GumpTemplateSaveForm(Design, block, name, null, Design.Description))
            {
                if (selection && block != null)
                {
                    f.Shown += SaveForm_SelectBlock;
                }
                if (f.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    string file = GumpTemplateLibrary.Save(f.Result, f.TemplateName, f.Category, f.Description);
                    SetMessage($"Template \"{f.TemplateName}\" saved in {f.Category} ({file}).");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The template could not be saved:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void SaveForm_SelectBlock(object sender, EventArgs e)
        {
            var f = (GumpTemplateSaveForm)sender;
            f.SelectBuildingBlock();
        }

        #endregion

        private void TsExportPng_Click(object sender, EventArgs e)
        {
            using (Bitmap bmp = GumpRenderer.RenderImage(Design, gumpCanvas.Page))
            {
                if (bmp == null)
                {
                    SetMessage("The page is empty.");
                    return;
                }
                using (var d = new SaveFileDialog { Filter = "PNG|*.png", FileName = $"{Design.ClassName}_page{gumpCanvas.Page}.png" })
                {
                    if (d.ShowDialog(this) == DialogResult.OK)
                    {
                        bmp.Save(d.FileName, ImageFormat.Png);
                        SetMessage("Picture saved: " + d.FileName);
                    }
                }
            }
        }

        private void RestoreState(string json)
        {
            int[] sel = Selected.Select(x => Design.Elements.IndexOf(x)).ToArray();
            int page = gumpCanvas.Page;
            gumpCanvas.Design = GumpDesign.FromJson(json);
            gumpCanvas.Page = Math.Min(page, Design.Pages);
            gumpCanvas.Select(sel.Where(i => i >= 0 && i < Design.Elements.Count).Select(i => Design.Elements[i]));
            _committed = json;
            _dirty = true;
            if (radioGump.Checked)
            {
                propertyGrid.SelectedObject = Design;
            }
            AfterChange();
        }

        private void TsUndo_Click(object sender, EventArgs e)
        {
            if (_undo.Count == 0)
            {
                return;
            }
            _redo.Add(_committed);
            string json = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            RestoreState(json);
            SetMessage($"Undo ({_undo.Count} more).");
        }

        private void TsRedo_Click(object sender, EventArgs e)
        {
            if (_redo.Count == 0)
            {
                return;
            }
            _undo.Add(_committed);
            string json = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            RestoreState(json);
            SetMessage($"Redo ({_redo.Count} more).");
        }

        private void TsCopy_Click(object sender, EventArgs e)
        {
            if (Selected.Count == 0)
            {
                return;
            }
            try
            {
                var data = new DataObject();
                data.SetData(ClipboardFormat, GumpDesign.ElementsToJson(Selected));
                data.SetText(string.Join("\r\n", GumpCodeGenerator.AddLines(new GumpDesign { Elements = Selected.Select(x => x.Clone()).ToList() }, string.Empty)));
                Clipboard.SetDataObject(data, true);
                SetMessage($"{Selected.Count} element(s) copied (also as ServUO lines).");
            }
            catch (Exception ex)
            {
                SetMessage("Copy failed: " + ex.Message);
            }
        }

        private void TsPaste_Click(object sender, EventArgs e)
        {
            try
            {
                List<GumpElement> list = null;
                if (Clipboard.ContainsData(ClipboardFormat))
                {
                    list = GumpDesign.ElementsFromJson((string)Clipboard.GetData(ClipboardFormat));
                }
                else if (Clipboard.ContainsText())
                {
                    // ServUO lines from the clipboard
                    list = GumpImporter.Import(Clipboard.GetText(), null).Elements;
                }
                if (list == null || list.Count == 0)
                {
                    SetMessage("Nothing to paste.");
                    return;
                }
                Detach(list);
                foreach (GumpElement el in list)
                {
                    el.X += 10;
                    el.Y += 10;
                    if (el.Page > Design.Pages)
                    {
                        el.Page = NewElementPage;
                    }
                    Design.Elements.Add(el);
                }
                gumpCanvas.Select(list);
                Commit($"{list.Count} element(s) pasted.");
            }
            catch (Exception ex)
            {
                SetMessage("Paste failed: " + ex.Message);
            }
        }

        private void TsDuplicate_Click(object sender, EventArgs e)
        {
            if (Selected.Count == 0)
            {
                return;
            }
            var copies = Selected.Select(x =>
            {
                GumpElement c = x.Clone();
                c.X += 10;
                c.Y += 10;
                return c;
            }).ToList();
            Detach(copies);
            Design.Elements.AddRange(copies);
            gumpCanvas.Select(copies);
            Commit($"{copies.Count} element(s) duplicated (IDs stay – change them or use \"Renumber IDs\").");
        }

        private void TsDelete_Click(object sender, EventArgs e)
        {
            if (Selected.Count == 0)
            {
                return;
            }
            int n = Selected.Count;
            foreach (GumpElement el in Selected)
            {
                Design.Elements.Remove(el);
            }
            gumpCanvas.Select(Array.Empty<GumpElement>());
            Commit($"{n} element(s) deleted.");
        }

        private void Arrange_Click(object sender, EventArgs e)
        {
            List<GumpElement> sel = Selected.Where(x => !x.Locked).ToList();
            if (sel.Count == 0)
            {
                return;
            }
            var rects = sel.ToDictionary(x => x, GumpRenderer.Measure);
            Rectangle all = rects.Values.Aggregate(Rectangle.Union);
            GumpElement first = Selected[0];
            Rectangle anchor = GumpRenderer.Measure(first);
            string name = (sender as ToolStripItem)?.Name;
            switch (name)
            {
                case "miAlignLeft": sel.ForEach(x => x.X = anchor.Left); break;
                case "miAlignRight": sel.ForEach(x => x.X = anchor.Right - rects[x].Width); break;
                case "miAlignCenter": sel.ForEach(x => x.X = anchor.Left + (anchor.Width - rects[x].Width) / 2); break;
                case "miAlignTop": sel.ForEach(x => x.Y = anchor.Top); break;
                case "miAlignBottom": sel.ForEach(x => x.Y = anchor.Bottom - rects[x].Height); break;
                case "miAlignMiddle": sel.ForEach(x => x.Y = anchor.Top + (anchor.Height - rects[x].Height) / 2); break;
                case "miSameWidth": sel.Where(x => x.IsSized).ToList().ForEach(x => x.Width = first.Width); break;
                case "miSameHeight": sel.Where(x => x.IsSized).ToList().ForEach(x => x.Height = first.Height); break;
                case "miDistributeH":
                {
                    List<GumpElement> o = sel.OrderBy(x => x.X).ToList();
                    if (o.Count > 2)
                    {
                        int space = (all.Width - o.Sum(x => rects[x].Width)) / (o.Count - 1), pos = all.Left;
                        foreach (GumpElement x in o)
                        {
                            x.X = pos;
                            pos += rects[x].Width + space;
                        }
                    }
                    break;
                }
                case "miDistributeV":
                {
                    List<GumpElement> o = sel.OrderBy(x => x.Y).ToList();
                    if (o.Count > 2)
                    {
                        int space = (all.Height - o.Sum(x => rects[x].Height)) / (o.Count - 1), pos = all.Top;
                        foreach (GumpElement x in o)
                        {
                            x.Y = pos;
                            pos += rects[x].Height + space;
                        }
                    }
                    break;
                }
                case "miToFront":
                case "miToBack":
                case "miForward":
                case "miBackward":
                    Reorder(name);
                    break;
                case "miCenterInGump":
                {
                    GumpElement bg = Design.Elements.FirstOrDefault(x => x.Type == ElementType.Background && (x.Page == 0 || x.Page == gumpCanvas.Page));
                    Rectangle area = bg != null ? GumpRenderer.Measure(bg) : new Rectangle(0, 0, Design.CanvasWidth, Design.CanvasHeight);
                    int dx = area.Left + (area.Width - all.Width) / 2 - all.Left;
                    sel.Where(x => x != bg).ToList().ForEach(x => x.X += dx);
                    break;
                }
            }
            Commit("Arranged.");
        }

        private void Reorder(string how)
        {
            List<GumpElement> sel = Selected;
            List<GumpElement> els = Design.Elements;
            if (how == "miToFront" || how == "miToBack")
            {
                foreach (GumpElement el in sel)
                {
                    els.Remove(el);
                }
                if (how == "miToFront")
                {
                    els.AddRange(sel);
                }
                else
                {
                    els.InsertRange(0, sel);
                }
                return;
            }
            int dir = how == "miForward" ? 1 : -1;
            IEnumerable<GumpElement> order = dir > 0 ? sel.OrderByDescending(els.IndexOf) : sel.OrderBy(els.IndexOf);
            foreach (GumpElement el in order)
            {
                int i = els.IndexOf(el), j = i + dir;
                if (j >= 0 && j < els.Count && !sel.Contains(els[j]))
                {
                    els[i] = els[j];
                    els[j] = el;
                }
            }
        }

        private void TsGrid_Click(object sender, EventArgs e)
        {
            gumpCanvas.ShowGrid = tsGrid.Checked;
            gumpCanvas.Invalidate();
        }

        private void TsSnap_Click(object sender, EventArgs e) => gumpCanvas.Snap = tsSnap.Checked;

        private void TsGridSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tsGridSize.SelectedItem is int g)
            {
                gumpCanvas.GridSize = g;
                gumpCanvas.Invalidate();
            }
        }

        private void TsZoom_SelectedIndexChanged(object sender, EventArgs e) => gumpCanvas.Zoom = tsZoom.SelectedIndex + 1;

        private void TsPagePrev_Click(object sender, EventArgs e) => gumpCanvas.Page = Math.Max(1, gumpCanvas.Page - 1);

        private void TsPageNext_Click(object sender, EventArgs e) => gumpCanvas.Page = Math.Min(Math.Max(Design.Pages, gumpCanvas.Page + 1), gumpCanvas.Page + 1);

        private void TsPageAdd_Click(object sender, EventArgs e)
        {
            int page = Design.Pages + 1;
            gumpCanvas.Page = page;
            comboNewPage.SelectedIndex = 1;
            SetMessage($"Page {page}: new elements go to this page now. Page 0 elements (frame, title) show on every page. Add a \"Next page\" button to reach it.");
            UpdatePageLabel();
        }

        private void TsTest_Click(object sender, EventArgs e)
        {
            gumpCanvas.TestMode = tsTest.Checked;
            gumpCanvas.Page = 1;
            SetMessage(tsTest.Checked
                ? "Test mode: click buttons, tick boxes, type into text entries – what the server would get is shown here. Press ▶ Test again to edit."
                : "Edit mode.");
        }

        private void TsHelp_Click(object sender, EventArgs e) => GumpStudioHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");

        #endregion

        #region Script

        private void UpdateCode()
        {
            var target = (CodeTarget)Math.Max(0, comboTarget.SelectedIndex);
            string code = GumpCodeGenerator.Generate(Design, target);
            CodeHighlighter.Show(codeBox, code, target == CodeTarget.Sphere);
            listProblems.Items.Clear();
            foreach (string p in GumpCodeGenerator.Validate(Design))
            {
                listProblems.Items.Add(p);
            }
            listProblems.Visible = listProblems.Items.Count > 0;
        }

        private void ComboTarget_SelectedIndexChanged(object sender, EventArgs e) => UpdateCode();

        private void BtnCopyCode_Click(object sender, EventArgs e)
        {
            if (codeBox.TextLength == 0)
            {
                return;
            }
            Clipboard.SetText(codeBox.Text);
            SetMessage("Script copied.");
        }

        private void BtnSaveCode_Click(object sender, EventArgs e)
        {
            bool sphere = comboTarget.SelectedIndex == 2;
            using (var d = new SaveFileDialog { Filter = sphere ? "Sphere script (*.scp)|*.scp" : "C# script (*.cs)|*.cs", FileName = GumpCodeGenerator.Identifier(Design.ClassName, "MyGump") + (sphere ? ".scp" : ".cs") })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    File.WriteAllText(d.FileName, codeBox.Text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n"), new UTF8Encoding(false));
                    SetMessage("Script saved: " + d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Saving failed:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ChkWrapCode_CheckedChanged(object sender, EventArgs e) => codeBox.WordWrap = chkWrapCode.Checked;

        private void ListProblems_DoubleClick(object sender, EventArgs e)
        {
            if (!(listProblems.SelectedItem is string p))
            {
                return;
            }
            Match m = Regex.Match(p, @"at (-?\d+), (-?\d+)");
            if (m.Success)
            {
                int x = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture), y = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                GumpElement el = Design.Elements.FirstOrDefault(v => v.X == x && v.Y == y);
                if (el != null)
                {
                    if (el.Page > 0)
                    {
                        gumpCanvas.Page = el.Page;
                    }
                    gumpCanvas.Select(new[] { el });
                }
            }
        }

        #endregion

        #region Properties

        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (radioGump.Checked && (e.ChangedItem?.Label == "CanvasWidth" || e.ChangedItem?.Label == "CanvasHeight"))
            {
                gumpCanvas.UpdateSize();
            }
            Commit();
        }

        private void PropertyTarget_CheckedChanged(object sender, EventArgs e)
        {
            if (radioGump.Checked)
            {
                propertyGrid.SelectedObject = Design;
            }
            else
            {
                propertyGrid.SelectedObjects = Selected.Cast<object>().ToArray();
            }
        }

        #endregion

        #region Element list

        private void FillElementList()
        {
            _updatingList = true;
            listElements.BeginUpdate();
            listElements.Items.Clear();
            for (int i = 0; i < Design.Elements.Count; i++)
            {
                GumpElement el = Design.Elements[i];
                string id = el.Type switch
                {
                    ElementType.Button or ElementType.ItemButton => el.ButtonType == ButtonKind.Page ? $"→ page {el.TargetPage}" : $"reply {el.ButtonId}",
                    ElementType.Check or ElementType.Radio => $"switch {el.SwitchId}",
                    ElementType.TextEntry => $"entry {el.EntryId}",
                    ElementType.Item => $"0x{el.ItemId:X4}",
                    ElementType.Label or ElementType.LabelCropped => el.Text,
                    ElementType.AlphaRegion or ElementType.Html => string.Empty,
                    _ => el.GumpId.ToString(CultureInfo.InvariantCulture)
                };
                GumpListSpec spec = GumpLists.SpecOf(Design, el);
                string group = spec != null ? $"{spec.Name} row {(el.ListRow < 0 ? "–" : (el.ListRow + 1).ToString(CultureInfo.InvariantCulture))}" : el.DesignGroup;
                var item = new ListViewItem(new[] { i.ToString(CultureInfo.InvariantCulture), el.Type.ToString(), el.Name, el.Page.ToString(CultureInfo.InvariantCulture), $"{el.X}, {el.Y}", id, group }) { Tag = el };
                if (el.Hidden)
                {
                    item.ForeColor = Color.Gray;
                }
                if (el.Locked)
                {
                    item.Font = new Font(listElements.Font, FontStyle.Italic);
                }
                item.Selected = gumpCanvas.Selection.Contains(el);
                listElements.Items.Add(item);
            }
            listElements.EndUpdate();
            _updatingList = false;
        }

        private void ListElements_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingList)
            {
                return;
            }
            _updatingList = true;
            List<GumpElement> sel = listElements.SelectedItems.Cast<ListViewItem>().Select(i => (GumpElement)i.Tag).ToList();
            if (sel.Count > 0 && sel[0].Page > 0 && sel[0].Page != gumpCanvas.Page)
            {
                gumpCanvas.Page = sel[0].Page;
            }
            gumpCanvas.Select(sel);
            _updatingList = false;
        }

        private void ListElements_DoubleClick(object sender, EventArgs e)
        {
            tabRight.SelectedTab = tabProperties;
            radioElement.Checked = true;
        }

        private void BtnElementUp_Click(object sender, EventArgs e)
        {
            Reorder("miForward");
            Commit();
        }

        private void BtnElementDown_Click(object sender, EventArgs e)
        {
            Reorder("miBackward");
            Commit();
        }

        private void BtnElementHide_Click(object sender, EventArgs e)
        {
            foreach (GumpElement el in Selected)
            {
                el.Hidden = !el.Hidden;
            }
            Commit();
        }

        private void BtnElementLock_Click(object sender, EventArgs e)
        {
            foreach (GumpElement el in Selected)
            {
                el.Locked = !el.Locked;
            }
            Commit();
        }

        private void BtnRenumber_Click(object sender, EventArgs e)
        {
            int b = 1, s = 1, t = 1;
            foreach (GumpElement el in Design.Elements)
            {
                if ((el.Type == ElementType.Button || el.Type == ElementType.ItemButton) && el.ButtonType == ButtonKind.Reply && el.ButtonId != 0)
                {
                    el.ButtonId = b++;
                }
                else if (el.Type == ElementType.Check || el.Type == ElementType.Radio)
                {
                    el.SwitchId = s++;
                }
                else if (el.Type == ElementType.TextEntry)
                {
                    el.EntryId = t++;
                }
            }
            Commit("IDs renumbered (buttons with ID 0 = close stay 0).");
        }

        #endregion

        #region Gump browser

        private void ComboBrowser_SelectedIndexChanged(object sender, EventArgs e) => FillBrowser();

        private void TxtBrowserSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            e.SuppressKeyPress = true;
            string s = txtBrowserSearch.Text.Trim();
            bool hex = s.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            if (int.TryParse(hex ? s.Substring(2) : s, hex ? NumberStyles.HexNumber : NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            {
                if (!gumpBrowser.Items.Contains(id))
                {
                    gumpBrowser.Items = gumpBrowser.Items.Concat(new[] { id }).OrderBy(x => x).ToList();
                }
                gumpBrowser.SelectId(id);
            }
            else if (comboBrowser.SelectedIndex == 7)
            {
                gumpBrowser.Items = GumpBrowser.ValidItems(s);
            }
        }

        private void FillBrowser()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                int i = comboBrowser.SelectedIndex;
                gumpBrowser.Source = i == 7 ? BrowserSource.Items : BrowserSource.Gumps;
                gumpBrowser.DropType = i switch { 0 => ElementType.Background, 1 => ElementType.Button, 2 => ElementType.Check, 7 => ElementType.Item, _ => ElementType.Image };
                gumpBrowser.Items = i switch
                {
                    0 => GumpBrowser.KnownBackgrounds.Where(id => GumpRenderer.Gump(id) != null).ToList(),
                    1 => GumpBrowser.KnownButtons.Keys.Where(id => GumpRenderer.Gump(id) != null).ToList(),
                    2 => GumpBrowser.KnownSwitches.Keys.Where(id => GumpRenderer.Gump(id) != null).ToList(),
                    3 => GumpBrowser.ValidGumps(0, 10000),
                    4 => GumpBrowser.ValidGumps(10000, 30000),
                    5 => GumpBrowser.ValidGumps(30000, 50000),
                    6 => GumpBrowser.ValidGumps(50000, 70000),
                    _ => GumpBrowser.ValidItems(txtBrowserSearch.Text.Trim())
                };
                lblBrowserInfo.Text = $"{gumpBrowser.Items.Count} pictures. Drag onto the gump, or double click = use for the selected element.";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void GumpBrowser_SelectedChanged(object sender, int id)
        {
            statusPos.Text = gumpBrowser.Source == BrowserSource.Items ? $"Item 0x{id:X4}" : $"Gump {id} (0x{id:X4})";
        }

        /// <summary>Double click: puts the picture into the selected element (or adds a new one).</summary>
        private void GumpBrowser_Activated(object sender, int id)
        {
            GumpElement sel = Selected.Count == 1 ? Selected[0] : null;
            if (sel != null)
            {
                if (gumpBrowser.Source == BrowserSource.Items && (sel.Type == ElementType.Item || sel.Type == ElementType.ItemButton))
                {
                    sel.ItemId = id;
                    Commit($"Item 0x{id:X4} set.");
                    return;
                }
                if (gumpBrowser.Source == BrowserSource.Gumps && sel.Type != ElementType.Item && sel.Type != ElementType.Label && sel.Type != ElementType.TextEntry)
                {
                    sel.GumpId = id;
                    if (sel.Type == ElementType.Button || sel.Type == ElementType.ItemButton)
                    {
                        sel.GumpId2 = GumpBrowser.KnownButtons.TryGetValue(id, out int down) ? down : id + 1;
                    }
                    else if (sel.Type == ElementType.Check || sel.Type == ElementType.Radio)
                    {
                        sel.GumpId2 = GumpBrowser.KnownSwitches.TryGetValue(id, out int on) ? on : id + 1;
                    }
                    Commit($"Gump {id} set for the selected {sel.Type}.");
                    return;
                }
            }
            Point at = FreeSpot();
            GumpElement el = GumpDesign.Create(gumpBrowser.Source == BrowserSource.Items ? ElementType.Item : gumpBrowser.DropType, at.X, at.Y);
            if (el.Type == ElementType.Item)
            {
                el.ItemId = id;
            }
            else
            {
                el.GumpId = id;
                if (el.Type == ElementType.Button)
                {
                    el.GumpId2 = GumpBrowser.KnownButtons.TryGetValue(id, out int down) ? down : id + 1;
                    el.ButtonId = NextId(x => x.ButtonId, ElementType.Button, ElementType.ItemButton);
                }
                else if (el.Type == ElementType.Check)
                {
                    el.GumpId2 = GumpBrowser.KnownSwitches.TryGetValue(id, out int on) ? on : id + 1;
                    el.SwitchId = NextId(x => x.SwitchId, ElementType.Check, ElementType.Radio);
                }
            }
            el.Page = NewElementPage;
            Design.Elements.Add(el);
            gumpCanvas.Select(new[] { el });
            Commit($"{el.Type} added.");
        }

        #endregion

        #region Keyboard

        private bool TextInputFocused()
        {
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }
            return c is TextBoxBase || c is ComboBox || c is UpDownBase || c?.Parent is PropertyGrid || c?.GetType().Name.Contains("GridView") == true || c?.Parent is GumpCanvas;
        }

        #region Gallery, write back, lists, themes, graphics, compare, language, groups, test log

        private GumpDesign _imported;

        /// <summary>Copies / pasted elements are free elements: no script position, no list, own groups.</summary>
        private static void Detach(List<GumpElement> list)
        {
            var groups = new Dictionary<string, string>();
            foreach (GumpElement el in list)
            {
                el.Source = string.Empty;
                if (!string.IsNullOrEmpty(el.ListId))
                {
                    el.ListId = string.Empty;
                    el.ListRow = 0;
                    el.ListColumn = -1;
                    el.ListSlot = 0;
                    el.Locked = false;
                }
                if (!string.IsNullOrEmpty(el.DesignGroup))
                {
                    if (!groups.TryGetValue(el.DesignGroup, out string g))
                    {
                        groups[el.DesignGroup] = g = "Group" + Guid.NewGuid().ToString("N").Substring(0, 4);
                    }
                    el.DesignGroup = g;
                }
            }
        }

        private void TsGallery_Click(object sender, EventArgs e)
        {
            using (var f = new GumpGalleryForm())
            {
                if (f.ShowDialog(this) != DialogResult.OK || f.Chosen == null || !ConfirmDiscard())
                {
                    return;
                }
                GalleryEntry entry = f.Chosen;
                GumpDesign d = entry.Design;
                d.SourceFile = entry.File;
                d.SourceHash = GumpScriptWriter.Hash(entry.Code);
                d.SourceResolve = entry.Resolve;
                _dirty = false;
                NewDesign(d, entry.ClassName, true);
                _imported = d.Clone();
                SetMessage($"{entry.ClassName} from {entry.File} – change it and use \"Write back…\" to put the changes into the script.");
                foreach (string w in entry.Hints.Take(100))
                {
                    listProblems.Items.Add("Import: " + w);
                }
            }
        }

        private void TsWriteBack_Click(object sender, EventArgs e)
        {
            string file = Design.SourceFile;
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                MessageBox.Show(this, "This gump was not imported from a script (or the file is gone). Use \"Import ServUO…\" or the script gallery first.\n\n"
                                      + "DE: Dieses Gump wurde nicht aus einem Skript eingelesen.", "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string code = File.ReadAllText(file);
                ScriptWritePlan plan = GumpScriptWriter.Plan(Design, code, GumpImporter.FolderResolver(file));
                using (var f = new GumpWriteBackForm(plan, file))
                {
                    if (f.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    SetMessage($"{plan.Changes.Count} line(s) written into {Path.GetFileName(file)}" + (plan.Problems.Count > 0 ? $" – {plan.Problems.Count} thing(s) to change by hand." : "."));
                    if (f.Reimport)
                    {
                        string newCode = File.ReadAllText(file);
                        GumpDesign d = GumpImporter.Import(newCode, null, Design.ClassName, Design.SourceResolve ? GumpImporter.FolderResolver(file) : null);
                        d.SourceFile = file;
                        d.SourceHash = GumpScriptWriter.Hash(newCode);
                        d.SourceResolve = Design.SourceResolve;
                        _dirty = false;
                        NewDesign(d, Path.GetFileName(file), true);
                        _imported = d.Clone();
                    }
                    else
                    {
                        tsWriteBack.Enabled = false;
                        Design.SourceHash = "changed";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Write back failed:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TsList_Click(object sender, EventArgs e)
        {
            GumpListSpec edit = Selected.Select(x => GumpLists.SpecOf(Design, x)).FirstOrDefault(x => x != null);
            using (var f = new GumpListForm(Design, edit, Math.Max(1, gumpCanvas.Page)))
            {
                if (f.ShowDialog(this) != DialogResult.OK || f.Result == null)
                {
                    return;
                }
                GumpListSpec spec = f.Result;
                if (edit != null && edit.Columns.Count == spec.Columns.Count && edit.SelectButton == spec.SelectButton)
                {
                    // keep the template row (you may have moved / changed it), take the new settings and texts
                    int dx = spec.X - edit.X, dy = spec.Y - edit.Y;
                    foreach (GumpElement t in GumpLists.Template(Design, edit))
                    {
                        t.X += dx;
                        t.Y += dy;
                    }
                    edit.Name = spec.Name;
                    edit.Rows = spec.Rows;
                    edit.RowsPerPage = spec.RowsPerPage;
                    edit.RowHeight = spec.RowHeight;
                    edit.FirstPage = spec.FirstPage;
                    edit.ButtonBase = edit.SwitchBase = edit.EntryBase = spec.ButtonBase;
                    edit.NavY = spec.NavY;
                    edit.NavBackX = spec.NavBackX;
                    edit.NavNextX = spec.NavNextX;
                    for (int c = 0; c < spec.Columns.Count; c++)
                    {
                        edit.Columns[c].Texts = spec.Columns[c].Texts;
                        edit.Columns[c].Header = spec.Columns[c].Header;
                    }
                    Commit($"List \"{edit.Name}\" changed: {edit.Rows} rows.");
                    return;
                }
                if (edit != null)
                {
                    GumpLists.Remove(Design, edit);
                }
                GumpLists.Create(Design, spec);
                gumpCanvas.Page = Math.Max(1, spec.FirstPage);
                gumpCanvas.Select(GumpLists.Template(Design, spec));
                Commit($"List \"{spec.Name}\": {spec.Rows} rows, {spec.PageCount} page(s). Move / change row 1 – all rows follow; the script gets a for loop.");
            }
        }

        private void TsThemes_Click(object sender, EventArgs e)
        {
            using (var f = new GumpThemeForm(Design))
            {
                if (f.ShowDialog(this) != DialogResult.OK || f.Theme == null)
                {
                    return;
                }
                int n = GumpThemes.Apply(Design, f.Theme, f.Frames, f.Buttons, f.Switches, f.Texts);
                Commit($"Theme \"{f.Theme.Name}\": {n} element(s) changed.");
            }
        }

        private void TsArt_Click(object sender, EventArgs e)
        {
            using (var f = new GumpArtForm())
            {
                if (f.ShowDialog(this) != DialogResult.OK || f.InsertedId < 0)
                {
                    return;
                }
                GumpElement el = f.InsertedFrame
                    ? GumpDesign.Create(ElementType.Background, 20, 20)
                    : GumpDesign.Create(ElementType.Image, 20, 20);
                el.GumpId = f.InsertedId;
                el.Page = NewElementPage;
                Design.Elements.Add(el);
                gumpCanvas.Select(new[] { el });
                FillBrowser();
                Commit(f.InsertedFrame ? $"Own frame 0x{f.InsertedId:X} added as background." : $"Own picture 0x{f.InsertedId:X} added as image.");
            }
        }

        private void TsReference_DropDownOpening(object sender, EventArgs e)
        {
            miRefImported.Enabled = _imported != null;
            miRefClear.Enabled = gumpCanvas.HasReference;
            miRefTint.Checked = gumpCanvas.ReferenceTint;
        }

        private void MiRefCurrent_Click(object sender, EventArgs e)
        {
            gumpCanvas.ReferenceImage = null;
            gumpCanvas.ReferenceDesign = Design.Clone();
            SetMessage("The current state is shown as reference – change the gump and compare (Compare ▾ → Remove reference).");
        }

        private void MiRefImported_Click(object sender, EventArgs e)
        {
            if (_imported != null)
            {
                gumpCanvas.ReferenceImage = null;
                gumpCanvas.ReferenceDesign = _imported.Clone();
                SetMessage("The imported original is shown as reference.");
            }
        }

        private void MiRefPicture_Click(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog { Filter = "Pictures (*.png;*.bmp;*.jpg;*.gif)|*.png;*.bmp;*.jpg;*.jpeg;*.gif|All files|*.*" })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    LoadReferencePicture(d.FileName);
                }
            }
        }

        private void LoadReferencePicture(string file)
        {
            try
            {
                using (var b = new Bitmap(file))
                {
                    gumpCanvas.ReferenceDesign = null;
                    gumpCanvas.ReferenceImage = new Bitmap(b);
                }
                SetMessage("Picture shown as reference – rebuild the gump on top of it (e.g. from a screenshot).");
            }
            catch (Exception ex)
            {
                SetMessage("Picture not loaded: " + ex.Message);
            }
        }

        private void MiRefTint_Click(object sender, EventArgs e)
        {
            gumpCanvas.ReferenceTint = miRefTint.Checked;
            gumpCanvas.Invalidate();
        }

        private void MiRefClear_Click(object sender, EventArgs e)
        {
            gumpCanvas.ReferenceDesign = null;
            gumpCanvas.ReferenceImage = null;
        }

        private void TsRefOpacity_SelectedIndexChanged(object sender, EventArgs e)
        {
            gumpCanvas.ReferenceOpacity = new[] { 25, 50, 75, 100 }[Math.Max(0, tsRefOpacity.SelectedIndex)];
            gumpCanvas.Invalidate();
        }

        private void TsLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            GumpRenderer.SecondLanguage = tsLanguage.SelectedIndex == 1;
            gumpCanvas.Invalidate();
            if (GumpRenderer.SecondLanguage)
            {
                SetMessage($"Second language ({Design.SecondLanguage}): the work area shows and edits TextDe – empty TextDe shows Text.");
            }
        }

        private void TsGuides_Click(object sender, EventArgs e) => gumpCanvas.Guides = tsGuides.Checked;

        private void MiGroup_Click(object sender, EventArgs e)
        {
            if (Selected.Count < 2)
            {
                SetMessage("Select two or more elements to group them.");
                return;
            }
            int n = 1;
            while (Design.Elements.Any(x => x.DesignGroup == "Group" + n))
            {
                n++;
            }
            foreach (GumpElement el in Selected)
            {
                el.DesignGroup = "Group" + n;
            }
            Commit($"Group{n}: {Selected.Count} elements – a click selects all, Alt + click one.");
        }

        private void MiUngroup_Click(object sender, EventArgs e)
        {
            if (Selected.All(x => string.IsNullOrEmpty(x.DesignGroup)))
            {
                return;
            }
            foreach (GumpElement el in Selected)
            {
                el.DesignGroup = string.Empty;
            }
            Commit("Group removed.");
        }

        private void GumpCanvas_TestLogged(object sender, string line)
        {
            Design.TestLog ??= new List<string>();
            Design.TestLog.Add(line);
            if (Design.TestLog.Count > 200)
            {
                Design.TestLog.RemoveAt(0);
            }
            listTestLog.Items.Add(line);
            listTestLog.TopIndex = Math.Max(0, listTestLog.Items.Count - 1);
            if (Design.IncludeTestLog)
            {
                _codeTimer.Stop();
                _codeTimer.Start();
            }
        }

        private void BtnClearTestLog_Click(object sender, EventArgs e)
        {
            Design.TestLog?.Clear();
            listTestLog.Items.Clear();
            _codeTimer.Stop();
            _codeTimer.Start();
        }

        private void ChkTestLogInScript_CheckedChanged(object sender, EventArgs e)
        {
            if (Design.IncludeTestLog != chkTestLogInScript.Checked)
            {
                Design.IncludeTestLog = chkTestLogInScript.Checked;
                Commit();
            }
        }

        /// <summary>Files dropped on the work area: .cs = import, .gump.json = open, pictures = reference.</summary>
        private void GumpCanvas_FilesDropped(object sender, string[] files)
        {
            string file = files.FirstOrDefault();
            if (file == null)
            {
                return;
            }
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext == ".cs")
            {
                if (ConfirmDiscard())
                {
                    ImportScript(file);
                }
            }
            else if (ext == ".json")
            {
                if (!ConfirmDiscard())
                {
                    return;
                }
                try
                {
                    GumpDesign design = GumpDesign.FromJson(File.ReadAllText(file));
                    _dirty = false;
                    NewDesign(design, Path.GetFileName(file), true);
                    _file = file;
                    UpdateTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The project could not be read:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (ext == ".png" || ext == ".bmp" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif")
            {
                LoadReferencePicture(file);
            }
            else
            {
                SetMessage("Drop .cs scripts, .gump.json projects or pictures (as reference).");
            }
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F1: TsHelp_Click(this, EventArgs.Empty); return true;
                case Keys.F5: tsTest.Checked = !tsTest.Checked; TsTest_Click(this, EventArgs.Empty); return true;
                case Keys.Control | Keys.S: TsSave_Click(this, EventArgs.Empty); return true;
                case Keys.Control | Keys.O: TsOpen_Click(this, EventArgs.Empty); return true;
                case Keys.Control | Keys.T: MiTemplateLibrary_Click(this, EventArgs.Empty); return true;
            }
            if (!TextInputFocused())
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.G: MiGroup_Click(this, EventArgs.Empty); return true;
                    case Keys.Control | Keys.Shift | Keys.G: MiUngroup_Click(this, EventArgs.Empty); return true;
                }
            }
            if (!TextInputFocused())
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.Z: TsUndo_Click(this, EventArgs.Empty); return true;
                    case Keys.Control | Keys.Y: TsRedo_Click(this, EventArgs.Empty); return true;
                    case Keys.Control | Keys.C: TsCopy_Click(this, EventArgs.Empty); return true;
                    case Keys.Control | Keys.V: TsPaste_Click(this, EventArgs.Empty); return true;
                    case Keys.Control | Keys.D: TsDuplicate_Click(this, EventArgs.Empty); return true;
                    case Keys.Delete: TsDelete_Click(this, EventArgs.Empty); return true;
                    case Keys.Control | Keys.A: gumpCanvas.Select(GumpRenderer.Visible(Design, gumpCanvas.Page, true)); return true;
                    case Keys.Escape: gumpCanvas.Select(Array.Empty<GumpElement>()); return true;
                    case Keys.PageUp: TsPagePrev_Click(this, EventArgs.Empty); return true;
                    case Keys.PageDown: TsPageNext_Click(this, EventArgs.Empty); return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }

    /// <summary>Simple syntax colors for the script box (C# or Sphere).</summary>
    internal static class CodeHighlighter
    {
        private static readonly Regex Keywords = new Regex(@"\b(using|namespace|public|private|protected|static|readonly|override|class|new|return|if|else|switch|case|break|base|this|void|int|bool|string|true|false|null|const)\b", RegexOptions.Compiled);
        private static readonly Regex Strings = new Regex(@"""(?:[^""\\\r\n]|\\.)*""", RegexOptions.Compiled);
        private static readonly Regex Comments = new Regex(@"//[^\r\n]*", RegexOptions.Compiled);
        private static readonly Regex Numbers = new Regex(@"\b(0x[0-9A-Fa-f]+|\d+)\b", RegexOptions.Compiled);
        private static readonly Regex Adds = new Regex(@"\bAdd\w+\b", RegexOptions.Compiled);
        private static readonly Regex Sections = new Regex(@"^\[[^\]\r\n]+\]", RegexOptions.Compiled | RegexOptions.Multiline);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static void Show(RichTextBox box, string text, bool sphere)
        {
            int first = box.GetCharIndexFromPosition(new Point(1, 1));
            SendMessage(box.Handle, 0x0B, IntPtr.Zero, IntPtr.Zero);
            try
            {
                box.Text = text;
                string t = box.Text;
                using (var bold = new Font(box.Font, FontStyle.Bold))
                {
                    void Paint(Regex rx, Color c, bool b = false)
                    {
                        foreach (Match m in rx.Matches(t))
                        {
                            box.Select(m.Index, m.Length);
                            box.SelectionColor = c;
                            if (b)
                            {
                                box.SelectionFont = bold;
                            }
                        }
                    }
                    if (sphere)
                    {
                        Paint(Sections, Color.FromArgb(0, 90, 160), true);
                    }
                    else
                    {
                        Paint(Keywords, Color.Blue);
                        Paint(Adds, Color.FromArgb(120, 60, 0), true);
                    }
                    Paint(Numbers, Color.FromArgb(9, 134, 88));
                    Paint(Strings, Color.FromArgb(163, 21, 21));
                    Paint(Comments, Color.Green);
                }
                box.Select(Math.Min(first, box.TextLength), 0);
                box.ScrollToCaret();
            }
            finally
            {
                SendMessage(box.Handle, 0x0B, new IntPtr(1), IntPtr.Zero);
                box.Invalidate();
            }
        }
    }
}
