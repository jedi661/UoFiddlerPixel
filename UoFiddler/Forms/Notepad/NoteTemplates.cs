/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// Ready-made note templates.
    /// </summary>
    internal static class NoteTemplates
    {
        public static readonly string[] Names = { "Todo list", "Bug report", "Shard changelog", "Build session", "ServUO script" };

        private static readonly Color _headingColor = Color.FromArgb(31, 78, 121);

        private sealed class Builder : IDisposable
        {
            private readonly RichTextBox _box = new();
            private readonly string _font;
            private readonly float _size;

            public Builder(string font, float size)
            {
                _font = font;
                _size = size;
                _box.CreateControl();
                _ = _box.Handle;
                _box.Font = new Font(font, size);
            }

            private void Add(string text, float size, bool bold, Color? color, short numbering = 0, bool italic = false)
            {
                int start = _box.TextLength;
                _box.Select(start, 0);
                _box.SelectedText = text + "\n";
                _box.Select(start, text.Length + 1);
                RichEditNative.SetFontFace(_box, _font);
                RichEditNative.SetFontSize(_box, size);
                RichEditNative.SetEffect(_box, RichEditNative.CFM_BOLD, bold);
                RichEditNative.SetEffect(_box, RichEditNative.CFM_ITALIC, italic);
                RichEditNative.SetTextColor(_box, color);
                RichEditNative.SetNumbering(_box, numbering);
            }

            public Builder H1(string t) { Add(t, RtfConverter.H1Size, true, _headingColor); return this; }
            public Builder H2(string t) { Add(t, RtfConverter.H2Size, true, _headingColor); return this; }
            public Builder P(string t = "") { Add(t, _size, false, null); return this; }
            public Builder Hint(string t) { Add(t, _size - 1, false, Color.Gray, 0, true); return this; }
            public Builder Bullet(string t) { Add(t, _size, false, null, RichEditNative.PFN_BULLET); return this; }
            public Builder Number(string t) { Add(t, _size, false, null, RichEditNative.PFN_ARABIC); return this; }
            public Builder Check(string t) { Add(NoteRichTextBox.Unchecked + " " + t, _size, false, null); return this; }

            public (string Rtf, string Plain) Result => (_box.Rtf, _box.Text);

            public void Dispose() => _box.Dispose();
        }

        public static Note Create(string name, NotepadSettings settings)
        {
            string date = DateTime.Now.ToString("d");
            var note = new Note { Title = $"{name} {date}", Category = name };
            using var b = new Builder(settings.DefaultFont, settings.DefaultSize);

            switch (name)
            {
                case "Bug report":
                    note.Category = "Bugs";
                    note.Tags = new List<string> { "bug" };
                    b.H1("Bug report").Hint($"Date: {date}   ·   Client / shard version: ").P()
                        .H2("What happens?").P("Describe the problem …").P()
                        .H2("Steps to reproduce").Number("Go to …").Number("Use …").Number("See error").P()
                        .H2("Expected").P().H2("IDs involved").P("0x0000 – ").P()
                        .H2("Status").Check("Reported").Check("Fixed").Check("Tested on the shard");
                    break;
                case "Shard changelog":
                    note.Category = "Changelog";
                    b.H1("Changelog").H2($"Version 1.0 – {date}")
                        .Bullet("Added: ").Bullet("Changed: ").Bullet("Fixed: ").P()
                        .Hint("Tip: hover ids like 0x1A2B to see the item.");
                    break;
                case "Build session":
                    note.Category = "Building";
                    b.H1($"Build session {date}").H2("Area").P("Map / coordinates: ").P()
                        .H2("Statics used").Bullet("0x0000 – ").Bullet("0x0000 – ").P()
                        .H2("Next steps").Check("").Check("");
                    break;
                case "ServUO script":
                    note.Category = "Scripts";
                    note.CodeMode = true;
                    note.PlainText =
                        "using System;\nusing Server;\n\nnamespace Server.Items\n{\n    public class MyItem : Item\n    {\n" +
                        "        [Constructable]\n        public MyItem() : base(0x0EED)\n        {\n            Name = \"my item\";\n            Hue = 0;\n        }\n\n" +
                        "        public MyItem(Serial serial) : base(serial)\n        {\n        }\n\n" +
                        "        public override void Serialize(GenericWriter writer)\n        {\n            base.Serialize(writer);\n            writer.Write(0); // version\n        }\n\n" +
                        "        public override void Deserialize(GenericReader reader)\n        {\n            base.Deserialize(reader);\n            int version = reader.ReadInt();\n        }\n    }\n}\n";
                    note.Rtf = "";
                    return note;
                default:
                    note.Category = "Todo";
                    b.H1($"Todo {date}").Check("").Check("").Check("").P().Hint("Click a box to tick it off.");
                    break;
            }

            (note.Rtf, note.PlainText) = b.Result;
            return note;
        }
    }
}
