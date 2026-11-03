/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.SendItem.Classes;

namespace UoFiddler.Plugin.SendItem.Forms
{
    /// <summary>
    /// Create or edit a tab button: a static to place or saved command lines.
    /// </summary>
    public class CommandButtonDialog : Form
    {
        private readonly RadioButton _kindStatic = new() { Text = "Static (place in game)", AutoSize = true };
        private readonly RadioButton _kindCommand = new() { Text = "Command(s)", AutoSize = true };
        private readonly TextBox _text = new();
        private readonly TextBox _commands = new();
        private readonly TextBox _staticId = new();
        private readonly NumericUpDown _hue = new();
        private readonly ComboBox _template = new();
        private readonly TextBox _tooltip = new();
        private readonly Button _color = new();
        private readonly PictureBox _preview = new();
        private readonly Label _staticName = new();
        private readonly Label _resultPreview = new();
        private readonly Panel _staticPanel = new();
        private readonly Panel _commandPanel = new();
        private readonly ErrorProvider _errors = new();
        private readonly string _globalTemplate;
        private int _colorArgb;

        public CommandButton Result { get; }

        public CommandButtonDialog(CommandButton button, string globalTemplate, bool isNew)
        {
            Result = button;
            _globalTemplate = globalTemplate;
            _colorArgb = button.Color;

            Text = isNew ? "New button" : "Edit button";
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(520, 470);
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            int y = 14;
            Label L(string text, int top) => new() { Text = text, AutoSize = true, Location = new Point(14, top + 3) };

            Controls.Add(L("Type", y));
            _kindStatic.Location = new Point(110, y);
            _kindCommand.Location = new Point(290, y);
            _kindStatic.CheckedChanged += (_, _) => UpdateKind();
            Controls.Add(_kindStatic);
            Controls.Add(_kindCommand);
            y += 32;

            Controls.Add(L("Button text", y));
            _text.SetBounds(110, y, 390, 23);
            _text.PlaceholderText = "Name shown on the button";
            Controls.Add(_text);
            y += 34;

            // --- static panel
            _staticPanel.SetBounds(0, y, 520, 200);
            _staticPanel.Controls.Add(L("Static ID", 2));
            _staticId.SetBounds(110, 0, 120, 23);
            _staticId.PlaceholderText = "0x1234 or 4660";
            _staticId.TextChanged += (_, _) => UpdatePreview();
            _staticPanel.Controls.Add(_staticId);

            _staticPanel.Controls.Add(L("Hue", 34));
            _hue.SetBounds(110, 32, 120, 23);
            _hue.Maximum = 3000;
            _hue.ValueChanged += (_, _) => UpdatePreview();
            _staticPanel.Controls.Add(_hue);

            _staticPanel.Controls.Add(L("Template", 66));
            _template.SetBounds(110, 64, 250, 23);
            _template.Items.Add("");
            _template.Items.AddRange(CommandText.PlaceTemplatePresets);
            _template.TextChanged += (_, _) => UpdatePreview();
            _staticPanel.Controls.Add(_template);

            _staticPanel.Controls.Add(new Label
            {
                Text = "Empty = global place template",
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(110, 90)
            });

            _staticName.SetBounds(110, 112, 250, 40);
            _staticName.ForeColor = Color.DimGray;
            _staticPanel.Controls.Add(_staticName);

            _preview.SetBounds(380, 0, 120, 120);
            _preview.BorderStyle = BorderStyle.FixedSingle;
            _preview.BackColor = Color.FromArgb(40, 42, 48);
            _preview.SizeMode = PictureBoxSizeMode.Zoom;
            _staticPanel.Controls.Add(_preview);

            _resultPreview.SetBounds(14, 160, 490, 36);
            _resultPreview.Font = new Font("Consolas", 9F);
            _staticPanel.Controls.Add(_resultPreview);

            // --- command panel
            _commandPanel.SetBounds(0, y, 520, 200);
            _commandPanel.Controls.Add(L("Commands", 2));
            _commands.SetBounds(110, 0, 390, 150);
            _commands.Multiline = true;
            _commands.AcceptsReturn = true;
            _commands.ScrollBars = ScrollBars.Vertical;
            _commands.Font = new Font("Consolas", 9.75F);
            _commandPanel.Controls.Add(_commands);
            _commandPanel.Controls.Add(new Label
            {
                Text = "One command per line, sent in order. \"#wait 500\" pauses 500 ms.\r\n" +
                       "Placeholders use the static selected in the browser: {hex} {id} {name} {hue} {sethue}",
                ForeColor = Color.DimGray,
                AutoSize = false,
                Bounds = new Rectangle(110, 154, 400, 40)
            });

            Controls.Add(_staticPanel);
            Controls.Add(_commandPanel);
            y += 206;

            Controls.Add(L("Tooltip", y));
            _tooltip.SetBounds(110, y, 390, 23);
            _tooltip.PlaceholderText = "Optional description";
            Controls.Add(_tooltip);
            y += 34;

            Controls.Add(L("Color", y));
            _color.SetBounds(110, y - 2, 120, 27);
            _color.Text = "Choose…";
            _color.Click += Color_Click;
            Controls.Add(_color);
            var resetColor = new Button { Text = "Default", Bounds = new Rectangle(236, y - 2, 80, 27) };
            resetColor.Click += (_, _) =>
            {
                _colorArgb = 0;
                UpdateColorButton();
            };
            Controls.Add(resetColor);

            var ok = new Button { Text = "Save", Bounds = new Rectangle(316, 428, 90, 30) };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(412, 428, 90, 30) };
            ok.Click += Ok_Click;
            Controls.Add(ok);
            Controls.Add(cancel);
            AcceptButton = null; // Enter is needed in the command box
            CancelButton = cancel;

            _text.Text = button.Text;
            _commands.Text = (button.Commands ?? "").Replace("\r\n", "\n").Replace("\n", "\r\n");
            _staticId.Text = button.Kind == ButtonKind.Static || button.StaticId > 0 ? "0x" + button.StaticId.ToString("X4") : "";
            _hue.Value = Math.Clamp(button.Hue, 0, 3000);
            _template.Text = button.Template ?? "";
            _tooltip.Text = button.Tooltip;
            _kindStatic.Checked = button.Kind == ButtonKind.Static;
            _kindCommand.Checked = button.Kind == ButtonKind.Command;
            UpdateKind();
            UpdateColorButton();
            UpdatePreview();
        }

        private void UpdateKind()
        {
            _staticPanel.Visible = _kindStatic.Checked;
            _commandPanel.Visible = !_kindStatic.Checked;
        }

        private void UpdateColorButton()
        {
            _color.BackColor = _colorArgb == 0 ? SystemColors.Control : Color.FromArgb(_colorArgb);
            _color.UseVisualStyleBackColor = _colorArgb == 0;
        }

        private void Color_Click(object sender, EventArgs e)
        {
            using var dialog = new ColorDialog { FullOpen = true };
            if (_colorArgb != 0)
            {
                dialog.Color = Color.FromArgb(_colorArgb);
            }

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _colorArgb = dialog.Color.ToArgb();
                UpdateColorButton();
            }
        }

        private void UpdatePreview()
        {
            Image old = _preview.Image;
            _preview.Image = null;
            old?.Dispose();

            if (!CommandText.TryParseId(_staticId.Text, out int id))
            {
                _staticName.Text = "";
                _resultPreview.Text = "";
                return;
            }

            string name = StaticImages.GetName(id);
            _staticName.Text = $"0x{id:X4} ({id})" + (name.Length > 0 ? Environment.NewLine + name : "");
            _preview.Image = StaticImages.GetHued(id, (int)_hue.Value);
            string template = string.IsNullOrWhiteSpace(_template.Text) ? _globalTemplate : _template.Text;
            _resultPreview.Text = "Sends: " + CommandText.Expand(template, id, name, (int)_hue.Value);
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            _errors.Clear();
            bool isStatic = _kindStatic.Checked;
            int id = 0;

            if (isStatic && !CommandText.TryParseId(_staticId.Text, out id))
            {
                _errors.SetError(_staticId, "Enter a static id, e.g. 0x1234");
                return;
            }

            if (!isStatic && string.IsNullOrWhiteSpace(_commands.Text))
            {
                _errors.SetError(_commands, "Enter at least one command.");
                return;
            }

            string text = _text.Text.Trim();
            if (text.Length == 0)
            {
                text = isStatic
                    ? (StaticImages.GetName(id) is { Length: > 0 } n ? n : "0x" + id.ToString("X4"))
                    : _commands.Lines.Length > 0 ? _commands.Lines[0].Trim() : "Command";
            }

            Result.Text = text;
            Result.Kind = isStatic ? ButtonKind.Static : ButtonKind.Command;
            Result.StaticId = isStatic ? id : 0;
            Result.Hue = isStatic ? (int)_hue.Value : 0;
            Result.Template = isStatic ? _template.Text.Trim() : "";
            Result.Commands = isStatic ? "" : _commands.Text.Trim();
            Result.Tooltip = _tooltip.Text.Trim();
            Result.Color = _colorArgb;

            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _preview.Image?.Dispose();
                _errors.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
