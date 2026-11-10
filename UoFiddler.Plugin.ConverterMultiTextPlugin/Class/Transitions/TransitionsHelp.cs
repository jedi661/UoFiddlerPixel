using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.Transitions
{
    /// <summary>Help window of the transition tool (English / German).</summary>
    internal static class TransitionsHelp
    {
        public const string English =
@"TEXTURE TRANSITIONS – HELP

PURPOSE
Creates the 12 transition pieces between two textures (e.g. meadow → road, grass → cobblestone) for CentrED+.
Every piece is texture A and texture B mixed with a mask (white = A, black = B).
Masks can be generated, loaded, learned from existing UO transitions and kept in a pattern library.

THE 12 PIECES (CentrED+ TilesBrush types)
• A_ pieces belong to brush A, texture B comes in:  A_UU (B at the top / north), A_LL (B at the left / west),
  A_UL, A_UR, A_DL, A_DR (B in that corner – outer corners).
• B_ pieces belong to brush B, texture A comes in: B_UU, B_LL (A at the top / left = B at the bottom / right),
  B_UL … B_DR (A in that corner – the inner corners of the B area).
• Directions are texture directions (top = north, left = west). The top left corner of a texture is the top
  corner of the land tile diamond.

LAYOUT
• Left: tabs Textures, Masks, Learn (UO), Library, Output.
• Top: the 12 pieces (show textures, land tiles or masks; arrow keys select) and the selected piece large.
• Bottom: ""Assembled"": the pieces put together automatically on a small map (lake, straight road, road with a
  curve, islands, half / half) exactly like CentrED+ chooses them – 3D like the client or 2D. Every seam shows here.

TAB TEXTURES
• Texture A and B: from a file, the clipboard, a texmaps ID or a land tile ID (its texture from tiledata, or the
  land tile turned into a texture). Name and brush ID are used in the XML, ""Land IDs"" are the pure land tiles of the brush.
• Texture size 64 or 128 (the size of the pieces). ""Swap A / B"".

TAB MASKS
• Generator: softness (soft border), rough (irregular natural border – the pattern repeats with the tile, so the
  pieces fit together), pattern number, border (more A / more B), corner shape (cut off / round / square).
  All borders meet the tile edges in the middle, so the 12 pieces always fit together without seams. ""Live"" generates all 12 at once.
• Mixing: blur and contrast for all pieces (the old ""Transition blur"" / ""Contrast"", now working).
• Selected mask: load, paste, copy, invert, smooth, mirror, rotate. Loaded or learned masks switch ""Live"" off.
• Mask files: load a folder with A_DR … B_UL (like the old tool) or save all masks.

TAB LEARN (UO) – look at existing transitions and learn from them
• All land tiles of art.mul with their tiledata name and texture; filter by name (grass, sand, dirt, cobble …) or ID.
  Land tile and texture are shown side by side.
• ""→ A"", ""→ B"": take the pure textures. ""→ T"": the existing transition you want to learn from
  (also by texmaps ID, file or clipboard).
• ""Extract"": calculates how much A and B is in every pixel of T – that is the mask Origin used.
  ""Average colors"" is robust, ""Pixel exact"" is for transitions made exactly from A and B. ""Detail"" = calmer / finer.
  ""→ best fitting piece"" finds out which of the 12 pieces T is.
• Learn the whole set (12 transitions of a pair), save it in the library, then use it with your own textures.

TAB LIBRARY (patterns)
• A pattern = 12 masks + generator values + preview, sorted in categories (roads, meadow, cobblestone, coast …).
• Save the current masks as pattern, load (double click), delete, open the folder (TransitionLibrary next to UoFiddler).

TAB OUTPUT
• Files: textures, land tiles (44 × 44, black outside), masks and transition.xml into a folder (empty = new folder in tempGrafic).
• texmaps.mul from ID and / or art.mul land tiles from ID (""Free"" finds 12 free IDs in a row, used IDs can be skipped).
• Both together: the new land tiles can be linked to their textures in tiledata (name + texture ID).
  Everything is in memory – save texmaps.mul / art.mul here or in UoFiddler, tiledata.mul in UoFiddler.
• XML: TilesBrush.xml for CentrED+ (root TilesBrush, or ""transition"" like the old tool). ""Show / edit XML"":
  change, save, copy, open another XML.

KEYBOARD
Arrow keys select pieces (when the piece view has the focus) · F1 help";

        public const string German =
@"TEXTUR-ÜBERGÄNGE – HILFE

ZWECK
Erzeugt die 12 Übergangsteile zwischen zwei Texturen (z. B. Wiese → Straße, Gras → Pflaster) für CentrED+.
Jedes Teil ist Textur A und Textur B, gemischt mit einer Maske (weiß = A, schwarz = B).
Masken lassen sich erzeugen, laden, aus vorhandenen UO-Übergängen lernen und in einer Musterbibliothek aufbewahren.

DIE 12 TEILE (CentrED+ TilesBrush-Typen)
• A_-Teile gehören zu Brush A, Textur B kommt herein: A_UU (B oben / Norden), A_LL (B links / Westen),
  A_UL, A_UR, A_DL, A_DR (B in dieser Ecke – Außenecken).
• B_-Teile gehören zu Brush B, Textur A kommt herein: B_UU, B_LL (A oben / links = B unten / rechts),
  B_UL … B_DR (A in dieser Ecke – die Innenecken der B-Fläche).
• Richtungen sind Texturrichtungen (oben = Norden, links = Westen). Die linke obere Ecke einer Textur ist die obere
  Spitze der Landkachel-Raute.

AUFBAU
• Links: Reiter Textures, Masks, Learn (UO), Library, Output.
• Oben: die 12 Teile (Texturen, Landkacheln oder Masken anzeigen; Pfeiltasten wählen) und das gewählte Teil groß.
• Unten: „Assembled“: die Teile automatisch auf einer kleinen Karte zusammengesetzt (See, gerade Straße, Straße mit
  Kurve, Inseln, halb / halb) genau so, wie CentrED+ sie wählt – 3D wie im Client oder 2D. Hier sieht man jede Naht.

REITER TEXTURES
• Textur A und B: aus Datei, Zwischenablage, texmaps-ID oder Landkachel-ID (ihre Textur aus tiledata, oder die
  Landkachel in eine Textur umgerechnet). Name und Brush-ID kommen in die XML, „Land IDs“ sind die reinen Landkacheln des Brushes.
• Texturgröße 64 oder 128 (Größe der Teile). „Swap A / B“ tauscht.

REITER MASKS
• Generator: Softness (weiche Kante), Rough (unregelmäßige natürliche Kante – das Muster wiederholt sich mit der Kachel,
  die Teile passen zusammen), Musternummer, Border (mehr A / mehr B), Eckenform (abgeschnitten / rund / eckig).
  Alle Kanten treffen die Kachelränder in der Mitte, die 12 Teile passen daher immer nahtlos zusammen. „Live“ erzeugt alle 12 sofort.
• Mixing: Weichzeichnen und Kontrast für alle Teile (das alte „Transition blur“ / „Contrast“, jetzt funktionierend).
• Gewählte Maske: laden, einfügen, kopieren, invertieren, glätten, spiegeln, drehen. Geladene oder gelernte Masken schalten „Live“ aus.
• Maskendateien: einen Ordner mit A_DR … B_UL laden (wie im alten Tool) oder alle Masken speichern.

REITER LEARN (UO) – vorhandene Übergänge ansehen und daraus lernen
• Alle Landkacheln der art.mul mit tiledata-Namen und Textur; Filter nach Name (grass, sand, dirt, cobble …) oder ID.
  Landkachel und Textur werden nebeneinander gezeigt.
• „→ A“, „→ B“: die reinen Texturen übernehmen. „→ T“: der vorhandene Übergang, von dem du lernen willst
  (auch per texmaps-ID, Datei oder Zwischenablage).
• „Extract“: berechnet, wie viel A und B in jedem Pixel von T steckt – das ist die Maske, die Origin benutzt hat.
  „Average colors“ ist robust, „Pixel exact“ für Übergänge, die genau aus A und B gemacht wurden. „Detail“ = ruhiger / feiner.
  „→ best fitting piece“ findet heraus, welches der 12 Teile T ist.
• Den ganzen Satz lernen (12 Übergänge eines Paares), in der Bibliothek speichern und mit eigenen Texturen benutzen.

REITER LIBRARY (Muster)
• Ein Muster = 12 Masken + Generator-Werte + Vorschau, sortiert in Kategorien (Straßen, Wiese, Pflaster, Küste …).
• Aktuelle Masken als Muster speichern, laden (Doppelklick), löschen, Ordner öffnen (TransitionLibrary neben UoFiddler).

REITER OUTPUT
• Dateien: Texturen, Landkacheln (44 × 44, außen schwarz), Masken und transition.xml in einen Ordner
  (leer = neuer Ordner in tempGrafic).
• texmaps.mul ab ID und / oder art.mul-Landkacheln ab ID („Free“ sucht 12 freie IDs am Stück, belegte IDs lassen sich überspringen).
• Beides zusammen: die neuen Landkacheln lassen sich in tiledata mit ihren Texturen verknüpfen (Name + Textur-ID).
  Alles ist im Speicher – texmaps.mul / art.mul hier oder in UoFiddler speichern, tiledata.mul in UoFiddler.
• XML: TilesBrush.xml für CentrED+ (Wurzel TilesBrush, oder „transition“ wie im alten Tool). „Show / edit XML“:
  ändern, speichern, kopieren, eine andere XML öffnen.

TASTEN
Pfeiltasten wählen Teile (wenn die Teile-Ansicht den Fokus hat) · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Texture Transitions",
                Size = new Size(880, 760),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                ShowIcon = false,
                TopMost = owner is Form f && f.TopMost
            };
            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(MakePage("English", English));
            tabs.TabPages.Add(MakePage("Deutsch", German));
            tabs.SelectedIndex = german ? 1 : 0;
            form.Controls.Add(tabs);
            form.Show(owner);
        }

        private static TabPage MakePage(string title, string text)
        {
            var page = new TabPage(title);
            page.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.75f),
                BackColor = SystemColors.Window,
                Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n")
            });
            return page;
        }
    }
}
