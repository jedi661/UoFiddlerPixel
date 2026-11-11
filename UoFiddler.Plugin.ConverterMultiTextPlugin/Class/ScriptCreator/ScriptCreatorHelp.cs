using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator
{
    /// <summary>Help window of the script creator (English / German).</summary>
    internal static class ScriptCreatorHelp
    {
        public const string English =
@"SCRIPT CREATOR – HELP

PURPOSE
Creates server scripts (ServUO / RunUO C#, SphereServer .scp) from templates. Every template is a text file with
fields and {{placeholders}} – you can change the built-in ones, write new ones, or turn your existing scripts into templates.

TAB GENERATOR
1. Left: choose a template (search box on top). ★ = own template.
2. Middle: fill the fields. Lists (attributes, skills, loot …) are tables: pick the name, type the value, the last empty row adds one.
   Item ID, hue and body show a picture from the loaded client files. Every field has a tooltip with its {{Key}}.
3. Right: the script, live and colored. Red lines above it = problems (invalid class name, number out of range,
   damage types not 100 % …).
4. ""Save"" writes it into the scripts folder (e.g. ServUO\Scripts\Custom) with the file name of the template; ""Save as…"" asks.
• ""…"" next to item ID, hue, body, sound, gump and cliloc fields opens the UO browser – choose with pictures, playing
  animations and sounds. Book covers have a picture list again.
• ""Reset values"" = defaults · ""Edit template"" = open it in the Templates tab · ""Values as defaults"" = own copy with your values.

UO BROWSER / OVERVIEW (toolbar)
Items (art with name, flags, layer, AnimID; filter wearable, weapon, container, light, door …), animations (names from
Animationlist.xml, animation plays, action and direction, which anim file), sounds (play; creature sound set +0 … +4),
hues, gumps (interface / containers / paperdoll), cliloc texts (search by text or number) and an overview tab
""where do I find which ID"". Double click or ""Use"" takes the value; Ctrl+C copies the ID.
• The values of every template are kept while the window is open.

BUILT-IN TEMPLATES (all old tabs are in them)
ServUO: new ranged / melee weapon TYPE (own class with ammo, effect, abilities, AOS / ML / old speed and damage, sounds –
old tabs Crossbow and Sword), ranged / melee weapon VARIANT (magic or artifact version of an existing weapon – old tabs Bow,
Staff, Axe), armor / shield, clothing, creature / mount,
vendor NPC, chair / furniture, container (gift box, barrel …), commodity deed box, bag with items, decoration item,
damageable wall, door, book (text is wrapped at whole words, empty line = new page).
Sphere: ITEMDEF and CHARDEF.

USE YOUR OWN SCRIPTS AS GENERATOR (""Import script…"" or ""Import from clipboard"")
1. Choose an existing .cs (or Sphere .scp) script.
2. The tool finds the class name, the base class and all values: assignments (Name = ""…"", Hue = 0x481, LootType.Blessed),
   method calls (SetStr(100, 150), SetSkill(SkillName.Magery, 90, 100) …), base(…) arguments, [Flipable(…)], [CorpseName(…)],
   overridden properties (LabelNumber, DefaultGumpID …).
3. Tick the values that should become fields, rename the keys, choose the type (int, itemid, hue, bool …).
   The class name always becomes {{ClassName}} – everywhere in the file.
4. ""Save as template"" – it appears at once under ""My templates"". Now you make 20 variants of your script in seconds.
Tip: afterwards open it in the Templates tab, e.g. to turn several attribute lines into one list field with {{#each}}.

TAB TEMPLATES (write and change templates)
Header lines, then @template and the script text:
  @name Name            @group Group in the list      @output cs (or scp, txt …)
  @file {{ClassName}}.cs                               @description Text
  @field Key | type | Label | default | tooltip
  @check sum:Phys,Fire,Cold=100     (the values / list values must add up to 100)
Types: class (C# name), string, text (several lines), int:min:max, double, range (min,max), bool, itemid, itemid:0x1,0x2 (with a
  picture list), hue, body, sound, gump, cliloc (all with the browser button),
  choice:a,b,c (fixed list) or choice:@ListName, combo:… (list + free text), list:@ListName (table name = value),
  pages:lines:chars (book text).
Lists: AosAttribute, AosWeaponAttribute, AosArmorAttribute, SkillName, LootPack, AIType, FightMode, LootType, ItemLevel, HideType,
  FoodType, Layer, LightType, Reagent, ItemType, SBInfo, RangedBase, MeleeBase, ArmorBase, ClothingBase, ContainerBase …
  Own lists: ScriptTemplates\Lists\Name.txt (one entry per line) – adds a list or replaces a built-in one.
In the text:
  {{Key}}  value       {{Key|cs}} for C# strings (quotes are escaped)     |lower |upper |hex |dec |scp (Sphere hex) |ident
  {{#if Key}} … {{else}} … {{/if}}      (true = not empty, not 0, not false)      {{#if !Key}}
  {{#each List}} … {{key}} {{value}} {{value1}} {{value2}} {{number}} {{sep}} … {{/each}}
  {{lb}} writes ""{{"". A line that only holds a block tag disappears completely.
""Test"" (F5) builds the script with the defaults, ""Save"" (Ctrl+S) stores it in ScriptTemplates. Saving a built-in template creates
an own copy with the same name that replaces it; delete the copy to get the built-in one back.

TAB CLIENT TEXT FILES
All text files of the client folder (walls.txt, doors.txt, floors.txt, roof.txt, stairs.txt, misc.txt – house designer –,
bodyconv.def, mobtypes.txt, corpse.def …). Find (no special characters needed), replace, replace all, save.
The first save makes a .bak copy. Bytes stay as they are (Latin-1).

KEYBOARD
Ctrl+S save (script / template / file, depending on the tab) · F5 test template · Ctrl+F find (client files) · Enter / Shift+Enter next / previous · F1 help";

        public const string German =
@"SCRIPT CREATOR – HILFE

ZWECK
Erzeugt Server-Skripte (ServUO / RunUO C#, SphereServer .scp) aus Vorlagen. Jede Vorlage ist eine Textdatei mit Feldern und
{{Platzhaltern}} – die eingebauten lassen sich ändern, neue schreiben, und vorhandene Skripte werden per Import zu Vorlagen.

REITER GENERATOR
1. Links: Vorlage wählen (Suchfeld oben). ★ = eigene Vorlage.
2. Mitte: Felder ausfüllen. Listen (Attribute, Skills, Loot …) sind Tabellen: Namen wählen, Wert eintippen, die letzte leere Zeile fügt eine hinzu.
   Item-ID, Hue und Body zeigen ein Bild aus den geladenen Client-Dateien. Jedes Feld hat einen Tooltip mit seinem {{Key}}.
3. Rechts: das Skript, live und farbig. Rote Zeilen darüber = Probleme (ungültiger Klassenname, Zahl außerhalb des Bereichs,
   Schadensarten nicht 100 % …).
4. „Save“ schreibt es mit dem Dateinamen der Vorlage in den Scripts-Ordner (z. B. ServUO\Scripts\Custom); „Save as…“ fragt nach.
• „…“ neben Item-ID-, Hue-, Body-, Sound-, Gump- und Cliloc-Feldern öffnet den UO-Browser – auswählen mit Bildern, laufenden
  Animationen und Tönen. Buch-Cover haben wieder eine Bilderliste.
• „Reset values“ = Vorgaben · „Edit template“ = Vorlage im Reiter Templates öffnen · „Values as defaults“ = eigene Kopie mit deinen Werten.

UO-BROWSER / ÜBERSICHT (Leiste)
Items (Art mit Name, Flags, Layer, AnimID; Filter tragbar, Waffe, Behälter, Licht, Tür …), Animationen (Namen aus
Animationlist.xml, Animation läuft, Aktion und Richtung, welche anim-Datei), Sounds (abspielen; Kreatur-Sounds +0 … +4),
Hues, Gumps (Oberfläche / Behälter / Paperdoll), Cliloc-Texte (Suche nach Text oder Nummer) und ein Reiter
„wo finde ich welche ID“. Doppelklick oder „Use“ übernimmt den Wert; Strg+C kopiert die ID.
• Die Werte jeder Vorlage bleiben erhalten, solange das Fenster offen ist.

EINGEBAUTE VORLAGEN (alle alten Reiter stecken darin)
ServUO: neue Fern- / Nahkampf-WAFFENART (eigene Klasse mit Munition, Effekt, Fähigkeiten, AOS / ML / alter Geschwindigkeit
und Schaden, Sounds – alte Reiter Crossbow und Sword), Fern- / Nahkampf-VARIANTE (magische oder Artefakt-Version einer vorhandenen
Waffe – alte Reiter Bow, Staff, Axe), Rüstung / Schild, Kleidung, Kreatur / Reittier,
Händler-NPC, Stuhl / Möbel, Behälter (Geschenkbox, Fass …), Commodity-Deed-Box, Beutel mit Items, Deko-Item,
zerstörbare Wand, Tür, Buch (Text wird an ganzen Wörtern umbrochen, Leerzeile = neue Seite).
Sphere: ITEMDEF und CHARDEF.

EIGENE SKRIPTE ALS GENERATOR NUTZEN („Import script…“ oder „Import from clipboard“)
1. Ein vorhandenes .cs- (oder Sphere-.scp-) Skript wählen.
2. Das Tool findet Klassenname, Basisklasse und alle Werte: Zuweisungen (Name = „…“, Hue = 0x481, LootType.Blessed),
   Methodenaufrufe (SetStr(100, 150), SetSkill(SkillName.Magery, 90, 100) …), base(…)-Argumente, [Flipable(…)], [CorpseName(…)],
   überschriebene Eigenschaften (LabelNumber, DefaultGumpID …).
3. Die Werte ankreuzen, die Felder werden sollen, Schlüssel umbenennen, Typ wählen (int, itemid, hue, bool …).
   Der Klassenname wird immer {{ClassName}} – überall in der Datei.
4. „Save as template“ – sie erscheint sofort unter „My templates“. Jetzt machst du 20 Varianten deines Skripts in Sekunden.
Tipp: danach im Reiter Templates öffnen, z. B. um mehrere Attributzeilen zu einem Listenfeld mit {{#each}} zusammenzufassen.

REITER TEMPLATES (Vorlagen schreiben und ändern)
Kopfzeilen, dann @template und der Skripttext:
  @name Name            @group Gruppe in der Liste    @output cs (oder scp, txt …)
  @file {{ClassName}}.cs                              @description Text
  @field Key | Typ | Beschriftung | Vorgabe | Tooltip
  @check sum:Phys,Fire,Cold=100     (die Werte / Listenwerte müssen 100 ergeben)
Typen: class (C#-Name), string, text (mehrzeilig), int:min:max, double, range (min,max), bool, itemid, itemid:0x1,0x2 (mit
  Bilderliste), hue, body, sound, gump, cliloc (alle mit Browser-Knopf),
  choice:a,b,c (feste Liste) oder choice:@Listenname, combo:… (Liste + freier Text), list:@Listenname (Tabelle Name = Wert),
  pages:Zeilen:Zeichen (Buchtext).
Listen: AosAttribute, AosWeaponAttribute, AosArmorAttribute, SkillName, LootPack, AIType, FightMode, LootType, ItemLevel, HideType,
  FoodType, Layer, LightType, Reagent, ItemType, SBInfo, RangedBase, MeleeBase, ArmorBase, ClothingBase, ContainerBase …
  Eigene Listen: ScriptTemplates\Lists\Name.txt (ein Eintrag pro Zeile) – fügt eine Liste hinzu oder ersetzt eine eingebaute.
Im Text:
  {{Key}}  Wert        {{Key|cs}} für C#-Texte (Anführungszeichen werden maskiert)  |lower |upper |hex |dec |scp (Sphere-Hex) |ident
  {{#if Key}} … {{else}} … {{/if}}      (wahr = nicht leer, nicht 0, nicht false)    {{#if !Key}}
  {{#each Liste}} … {{key}} {{value}} {{value1}} {{value2}} {{number}} {{sep}} … {{/each}}
  {{lb}} schreibt „{{“. Eine Zeile, die nur einen Block-Tag enthält, verschwindet ganz.
„Test“ (F5) baut das Skript mit den Vorgaben, „Save“ (Strg+S) speichert in ScriptTemplates. Speichern einer eingebauten Vorlage legt
eine eigene Kopie mit gleichem Namen an, die sie ersetzt; Kopie löschen = eingebaute Vorlage ist wieder da.

REITER CLIENT TEXT FILES
Alle Textdateien des Client-Ordners (walls.txt, doors.txt, floors.txt, roof.txt, stairs.txt, misc.txt – Hausbau –,
bodyconv.def, mobtypes.txt, corpse.def …). Suchen (Sonderzeichen kein Problem), ersetzen, alle ersetzen, speichern.
Beim ersten Speichern entsteht eine .bak-Kopie. Die Bytes bleiben unverändert (Latin-1).

TASTEN
Strg+S speichern (Skript / Vorlage / Datei je nach Reiter) · F5 Vorlage testen · Strg+F suchen (Client-Dateien) · Enter / Umschalt+Enter weiter / zurück · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Script Creator",
                Size = new Size(960, 780),
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
