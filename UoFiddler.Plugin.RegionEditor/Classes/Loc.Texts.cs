/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Texts of the region editor: key -> { German, English, French }
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    public static partial class Loc
    {
        private static readonly Dictionary<string, string[]> _texts = new Dictionary<string, string[]>
        {
            // ── general ─────────────────────────────────────────────
            ["tab.title"] = new[] { "Regionen", "Regions", "Régions" },
            ["plugin.description"] = new[]
            {
                "Regionen für ServUO/RunUO, Sphere und POL auf den Karten zeichnen, prüfen, importieren und exportieren.",
                "Draw, check, import and export regions for ServUO/RunUO, Sphere and POL on the maps.",
                "Dessiner, vérifier, importer et exporter des régions pour ServUO/RunUO, Sphere et POL sur les cartes."
            },
            ["project.unnamed"] = new[] { "Neues Projekt", "New project", "Nouveau projet" },
            ["new.region"] = new[] { "Neue Region", "New region", "Nouvelle région" },
            ["new.part"] = new[] { "Teil", "part", "partie" },
            ["html.title"] = new[] { "Regionen – Übersicht", "Regions – overview", "Régions – aperçu" },
            ["filter.all"] = new[] { "Alle Regionsdateien", "All region files", "Tous les fichiers de régions" },
            ["filter.any"] = new[] { "Alle Dateien (*.*)", "All files (*.*)", "Tous les fichiers (*.*)" },
            ["status.count"] = new[] { "{0} Regionen auf dieser Karte · {1} gesamt", "{0} regions on this map · {1} total", "{0} régions sur cette carte · {1} au total" },
            ["map.none"] = new[] { "Keine Karte geladen.", "No map loaded.", "Aucune carte chargée." },
            ["map.error"] = new[]
            {
                "Die Karte \"{0}\" kann nicht gelesen werden: {1}",
                "The map \"{0}\" can not be read: {1}",
                "La carte « {0} » ne peut pas être lue : {1}"
            },

            // ── menus ───────────────────────────────────────────────
            ["menu.file"] = new[] { "Datei", "File", "Fichier" },
            ["menu.new"] = new[] { "Neues Projekt", "New project", "Nouveau projet" },
            ["menu.open"] = new[] { "Öffnen…", "Open…", "Ouvrir…" },
            ["menu.recent"] = new[] { "Zuletzt geöffnet", "Recent files", "Fichiers récents" },
            ["menu.save"] = new[] { "Projekt speichern", "Save project", "Enregistrer le projet" },
            ["menu.saveas"] = new[] { "Projekt speichern unter…", "Save project as…", "Enregistrer le projet sous…" },
            ["menu.import"] = new[] { "Importieren", "Import", "Importer" },
            ["menu.import.polfolder"] = new[] { "POL-Ordner „regions“…", "POL regions folder…", "Dossier « regions » de POL…" },
            ["menu.import.project"] = new[] { "Regionsprojekt hinzufügen…", "Add region project…", "Ajouter un projet de régions…" },
            ["menu.export"] = new[] { "Exportieren", "Export", "Exporter" },
            ["menu.export.csharp"] = new[] { "C#-Klassen eigener Regionstypen…", "C# classes of own region types…", "Classes C# des types de région personnalisés…" },
            ["menu.export.all"] = new[] { "Alles in den Export-Ordner", "Everything into the export folder", "Tout dans le dossier d'export" },
            ["menu.export.open"] = new[] { "Export-Ordner öffnen", "Open export folder", "Ouvrir le dossier d'export" },
            ["menu.loadserver"] = new[] { "Vom Server laden", "Load from server", "Charger depuis le serveur" },
            ["menu.saveserver"] = new[] { "In den Server speichern", "Save into server", "Enregistrer dans le serveur" },
            ["menu.settings"] = new[] { "Einstellungen…", "Settings…", "Paramètres…" },
            ["menu.view"] = new[] { "Ansicht", "View", "Affichage" },
            ["server.servuo"] = new[] { "ServUO / RunUO (Data\\Regions.xml)", "ServUO / RunUO (Data\\Regions.xml)", "ServUO / RunUO (Data\\Regions.xml)" },
            ["server.sphere"] = new[] { "Sphere (Areas-Datei)", "Sphere (areas file)", "Sphere (fichier des zones)" },
            ["server.pol"] = new[] { "POL (Ordner regions)", "POL (regions folder)", "POL (dossier regions)" },
            ["fmt.servuo"] = new[] { "ServUO / RunUO Regions.xml…", "ServUO / RunUO Regions.xml…", "ServUO / RunUO Regions.xml…" },
            ["fmt.sphere"] = new[] { "Sphere Areas (.scp)…", "Sphere areas (.scp)…", "Zones Sphere (.scp)…" },
            ["fmt.pol"] = new[] { "POL-Regionsdatei (.cfg)…", "POL region file (.cfg)…", "Fichier de régions POL (.cfg)…" },
            ["fmt.polregions"] = new[] { "POL regions.cfg (alles)…", "POL regions.cfg (everything)…", "POL regions.cfg (tout)…" },
            ["fmt.poljustice"] = new[] { "POL justice.cfg (Wachen)…", "POL justice.cfg (guards)…", "POL justice.cfg (gardes)…" },
            ["fmt.polmusic"] = new[] { "POL music.cfg (Musik)…", "POL music.cfg (music)…", "POL music.cfg (musique)…" },
            ["fmt.csv"] = new[] { "CSV-Tabelle…", "CSV table…", "Tableau CSV…" },
            ["fmt.txt"] = new[] { "Textübersicht…", "Text overview…", "Aperçu texte…" },
            ["fmt.html"] = new[] { "HTML-Übersicht…", "HTML overview…", "Aperçu HTML…" },
            ["fmt.png"] = new[] { "Kartenbild mit Regionen (PNG)…", "Map image with regions (PNG)…", "Image de la carte avec régions (PNG)…" },

            // ── buttons / labels ────────────────────────────────────
            ["btn.undo"] = new[] { "↶ Rückgängig", "↶ Undo", "↶ Annuler" },
            ["btn.redo"] = new[] { "↷ Wiederholen", "↷ Redo", "↷ Rétablir" },
            ["tip.undo"] = new[] { "Rückgängig: {0} (Strg+Z)", "Undo: {0} (Ctrl+Z)", "Annuler : {0} (Ctrl+Z)" },
            ["tip.redo"] = new[] { "Wiederholen: {0} (Strg+Y)", "Redo: {0} (Ctrl+Y)", "Rétablir : {0} (Ctrl+Y)" },
            ["btn.check"] = new[] { "✔ Prüfen", "✔ Check", "✔ Vérifier" },
            ["tip.check"] = new[] { "Alle Regionen prüfen", "Check all regions", "Vérifier toutes les régions" },
            ["btn.settings"] = new[] { "⚙ Einstellungen", "⚙ Settings", "⚙ Paramètres" },
            ["tip.settings"] = new[] { "Server-Ordner, Export, Design, Sprache", "Server folders, export, design, language", "Dossiers du serveur, export, apparence, langue" },
            ["btn.help"] = new[] { "? Hilfe", "? Help", "? Aide" },
            ["btn.dark"] = new[] { "◐ Dunkel", "◐ Dark", "◐ Sombre" },
            ["btn.light"] = new[] { "◑ Hell", "◑ Light", "◑ Clair" },
            ["tip.theme"] = new[] { "Zwischen hellem und dunklem Design wechseln", "Switch between light and dark design", "Basculer entre thème clair et sombre" },
            ["btn.cancel"] = new[] { "Abbrechen", "Cancel", "Annuler" },
            ["btn.close"] = new[] { "Schließen", "Close", "Fermer" },
            ["lbl.map"] = new[] { "Karte:", "Map:", "Carte :" },
            ["lbl.language"] = new[] { "Sprache:", "Language:", "Langue :" },
            ["lbl.snap"] = new[] { "Raster:", "Snap:", "Grille :" },
            ["help.folder"] = new[] { "Hilfe-Ordner öffnen", "Open help folder", "Ouvrir le dossier d'aide" },

            // ── tools ───────────────────────────────────────────────
            ["tool.select"] = new[] { "➚ Auswahl", "➚ Select", "➚ Sélection" },
            ["tool.pan"] = new[] { "✋ Verschieben", "✋ Pan", "✋ Déplacer" },
            ["tool.rect"] = new[] { "▭ Rechteck", "▭ Rectangle", "▭ Rectangle" },
            ["tool.polygon"] = new[] { "▱ Polygon", "▱ Polygon", "▱ Polygone" },
            ["tool.circle"] = new[] { "◯ Kreis", "◯ Circle", "◯ Cercle" },
            ["tool.fill"] = new[] { "▦ Füllen", "▦ Fill", "▦ Remplir" },
            ["tool.go"] = new[] { "G Go", "G Go", "G Go" },
            ["tool.entrance"] = new[] { "E Eingang", "E Entrance", "E Entrée" },
            ["toolhelp.select"] = new[]
            {
                "Auswahl (V): Region anklicken, Rechteck ziehen = verschieben, Griffe = Größe ändern, Umschalt = ganze Region verschieben, Alt = nächste Region darunter.",
                "Select (V): click a region, drag a rectangle = move, handles = resize, Shift = move the whole region, Alt = next region below.",
                "Sélection (V) : cliquer une région, glisser un rectangle = déplacer, poignées = redimensionner, Maj = déplacer toute la région, Alt = région suivante en dessous."
            },
            ["toolhelp.pan"] = new[]
            {
                "Verschieben (H): Karte mit der Maus ziehen. Mausrad = Zoom, mittlere Maustaste verschiebt immer.",
                "Pan (H): drag the map. Mouse wheel = zoom, the middle mouse button always pans.",
                "Déplacer (H) : faire glisser la carte. Molette = zoom, le bouton du milieu déplace toujours."
            },
            ["toolhelp.rect"] = new[]
            {
                "Rechteck (R): aufziehen = neue Region. Umschalt = zur gewählten Region hinzufügen, Alt = aus der gewählten Region ausschneiden.",
                "Rectangle (R): drag = new region. Shift = add to the selected region, Alt = cut out of the selected region.",
                "Rectangle (R) : glisser = nouvelle région. Maj = ajouter à la région choisie, Alt = découper de la région choisie."
            },
            ["toolhelp.polygon"] = new[]
            {
                "Polygon (P): Ecken anklicken, Doppelklick/Enter/ersten Punkt anklicken = fertig, Rücktaste = letzten Punkt entfernen, Esc = abbrechen.",
                "Polygon (P): click the corners, double click / Enter / first point = finish, Backspace = remove the last point, Esc = cancel.",
                "Polygone (P) : cliquer les coins, double-clic / Entrée / premier point = terminer, Retour arrière = retirer le dernier point, Échap = annuler."
            },
            ["toolhelp.circle"] = new[]
            {
                "Kreis (C): vom Mittelpunkt aus ziehen. Wird beim Export als Rechtecke gespeichert.",
                "Circle (C): drag from the center. It is stored as rectangles.",
                "Cercle (C) : glisser depuis le centre. Il est enregistré sous forme de rectangles."
            },
            ["toolhelp.fill"] = new[]
            {
                "Füllen (F): klickt man auf Land, wird das zusammenhängende Gebiet gleicher Art im sichtbaren Bereich zur Region (z. B. Insel, Wald, See).",
                "Fill (F): click the land – the connected area of the same kind in the visible part becomes a region (e.g. island, forest, lake).",
                "Remplir (F) : cliquer sur le terrain – la zone contiguë du même type dans la partie visible devient une région (île, forêt, lac…)."
            },
            ["toolhelp.go"] = new[]
            {
                "Go (G): Klick setzt den Go-Punkt der gewählten Region ([go-Befehl, Runen).",
                "Go (G): a click sets the go location of the selected region ([go command, runes).",
                "Go (G) : un clic place le point « go » de la région choisie (commande [go, runes)."
            },
            ["toolhelp.entrance"] = new[]
            {
                "Eingang (E): Klick setzt den Eingang der gewählten Region (DungeonRegion, z. B. für Kick/Teleport).",
                "Entrance (E): a click sets the entrance of the selected region (DungeonRegion, e.g. for kick/teleport).",
                "Entrée (E) : un clic place l'entrée de la région choisie (DungeonRegion, p. ex. expulsion/téléportation)."
            },
            ["tip.zoomin"] = new[] { "Vergrößern (Mausrad)", "Zoom in (mouse wheel)", "Zoom avant (molette)" },
            ["tip.zoomout"] = new[] { "Verkleinern", "Zoom out", "Zoom arrière" },
            ["tip.zoomfit"] = new[] { "Gewählte Region zeigen", "Show the selected region", "Afficher la région choisie" },
            ["view.statics"] = new[] { "Statics", "Statics", "Statiques" },
            ["view.grid"] = new[] { "Raster", "Grid", "Grille" },
            ["view.labels"] = new[] { "Namen", "Names", "Noms" },
            ["view.minimap"] = new[] { "Minikarte", "Minimap", "Mini-carte" },
            ["view.client"] = new[] { "Client-Position", "Client position", "Position du client" },
            ["view.refresh"] = new[] { "Karte neu laden", "Reload map", "Recharger la carte" },
            ["btn.client"] = new[] { "⌖ Client", "⌖ Client", "⌖ Client" },
            ["tip.client"] = new[] { "Position des laufenden Clients zeigen", "Show the position of the running client", "Afficher la position du client lancé" },
            ["btn.follow"] = new[] { "⟳ Folgen", "⟳ Follow", "⟳ Suivre" },
            ["tip.follow"] = new[] { "Der Client-Position folgen", "Follow the client position", "Suivre la position du client" },

            // ── tree ────────────────────────────────────────────────
            ["btn.addregion"] = new[] { "＋ Region", "＋ Region", "＋ Région" },
            ["tip.addregion"] = new[] { "Neue Region (oberste Ebene) – Einfg", "New region (top level) – Insert", "Nouvelle région (premier niveau) – Inser" },
            ["btn.addchild"] = new[] { "＋ Unterregion", "＋ Child", "＋ Sous-région" },
            ["tip.addchild"] = new[] { "Neue Region in der gewählten Region – Umschalt+Einfg", "New region inside the selected region – Shift+Insert", "Nouvelle région dans la région choisie – Maj+Inser" },
            ["tip.duplicate"] = new[] { "Region duplizieren", "Duplicate region", "Dupliquer la région" },
            ["tip.delete"] = new[] { "Region löschen (Entf)", "Delete region (Del)", "Supprimer la région (Suppr)" },
            ["tip.moveup"] = new[] { "Nach oben", "Move up", "Monter" },
            ["tip.movedown"] = new[] { "Nach unten", "Move down", "Descendre" },
            ["tip.expandall"] = new[] { "Alle aufklappen", "Expand all", "Tout développer" },
            ["tip.collapseall"] = new[] { "Alle zuklappen", "Collapse all", "Tout réduire" },
            ["search.placeholder"] = new[] { "Region suchen… (Strg+F)", "Search region… (Ctrl+F)", "Rechercher une région… (Ctrl+F)" },
            ["tree.hidden"] = new[] { "(ausgeblendet)", "(hidden)", "(masquée)" },
            ["ctx.add"] = new[] { "Neue Region", "New region", "Nouvelle région" },
            ["ctx.addchild"] = new[] { "Neue Unterregion", "New child region", "Nouvelle sous-région" },
            ["ctx.duplicate"] = new[] { "Duplizieren", "Duplicate", "Dupliquer" },
            ["ctx.rename"] = new[] { "Umbenennen (F2)", "Rename (F2)", "Renommer (F2)" },
            ["ctx.delete"] = new[] { "Löschen", "Delete", "Supprimer" },
            ["ctx.zoomto"] = new[] { "Auf der Karte zeigen", "Show on map", "Afficher sur la carte" },
            ["ctx.sendgo"] = new[] { "Client zum Go-Punkt schicken", "Send client to the go location", "Envoyer le client au point « go »" },
            ["ctx.copyxml"] = new[] { "Als ServUO-XML kopieren", "Copy as ServUO XML", "Copier en XML ServUO" },
            ["ctx.optimize"] = new[] { "Rechtecke optimieren", "Optimize rectangles", "Optimiser les rectangles" },
            ["ctx.polytorects"] = new[] { "Polygone → Rechtecke", "Polygons → rectangles", "Polygones → rectangles" },
            ["ctx.totop"] = new[] { "Zur obersten Ebene machen", "Make top level region", "Passer au premier niveau" },
            ["ctx.hide"] = new[] { "Ausgeblendet", "Hidden", "Masquée" },
            ["ctx.lock"] = new[] { "Gesperrt", "Locked", "Verrouillée" },
            ["ctx.mapselect"] = new[] { "Region hier auswählen", "Select region here", "Choisir la région ici" },
            ["ctx.mapsetgo"] = new[] { "Go-Punkt hierher", "Set go location here", "Placer le point « go » ici" },
            ["ctx.mapsetentrance"] = new[] { "Eingang hierher", "Set entrance here", "Placer l'entrée ici" },
            ["ctx.mapdeleterect"] = new[] { "Fläche hier löschen", "Delete area here", "Supprimer la zone ici" },
            ["ctx.mapsendchar"] = new[] { "Client hierher schicken", "Send client here", "Envoyer le client ici" },
            ["ctx.copypos"] = new[] { "Position kopieren ({0}, {1})", "Copy position ({0}, {1})", "Copier la position ({0}, {1})" },

            // ── tabs / properties ───────────────────────────────────
            ["tab.props"] = new[] { "Eigenschaften", "Properties", "Propriétés" },
            ["tab.rects"] = new[] { "Flächen", "Areas", "Zones" },
            ["tab.check"] = new[] { "Prüfung", "Check", "Vérification" },
            ["tab.history"] = new[] { "Verlauf", "History", "Historique" },
            ["prop.name"] = new[] { "Name:", "Name:", "Nom :" },
            ["prop.type"] = new[] { "Typ:", "Type:", "Type :" },
            ["prop.priority"] = new[] { "Priorität", "Priority", "Priorité" },
            ["prop.music"] = new[] { "Musik:", "Music:", "Musique :" },
            ["prop.z"] = new[] { "Z-Bereich:", "Z range:", "Plage Z :" },
            ["prop.go"] = new[] { "Go:", "Go:", "Go :" },
            ["prop.entrance"] = new[] { "Eingang:", "Entrance:", "Entrée :" },
            ["prop.guards"] = new[] { "Wachen:", "Guards:", "Gardes :" },
            ["prop.guardsoff"] = new[] { "deaktiviert", "disabled", "désactivés" },
            ["prop.guardstype"] = new[] { "Wachen-Typ", "guard type", "type de garde" },
            ["prop.rune"] = new[] { "Rune:", "Rune:", "Rune :" },
            ["prop.options"] = new[] { "Optionen:", "Options:", "Options :" },
            ["prop.hidden"] = new[] { "ausgeblendet", "hidden", "masquée" },
            ["prop.locked"] = new[] { "gesperrt", "locked", "verrouillée" },
            ["prop.color"] = new[] { "Farbe:", "Colour:", "Couleur :" },
            ["prop.colorreset"] = new[] { "nach Typ", "by type", "selon le type" },
            ["prop.notes"] = new[] { "Notizen:", "Notes:", "Notes :" },
            ["prop.extra"] = new[] { "Server-Werte:", "Server keys:", "Clés serveur :" },
            ["col.key"] = new[] { "Schlüssel", "Key", "Clé" },
            ["col.value"] = new[] { "Wert", "Value", "Valeur" },
            ["col.width"] = new[] { "Breite", "Width", "Largeur" },
            ["col.height"] = new[] { "Höhe", "Height", "Hauteur" },
            ["col.region"] = new[] { "Region", "Region", "Région" },
            ["col.problem"] = new[] { "Problem", "Problem", "Problème" },
            ["tip.zrange"] = new[]
            {
                "Z-Bereich der ganzen Region (<zrange>). Ohne Haken gilt die volle Höhe.",
                "Z range of the whole region (<zrange>). Without the tick the full height is used.",
                "Plage Z de toute la région (<zrange>). Sans la coche, toute la hauteur est utilisée."
            },
            ["tip.smartnohousing"] = new[]
            {
                "ServUO <smartNoHousing>: grau = nicht schreiben",
                "ServUO <smartNoHousing>: grey = not written",
                "ServUO <smartNoHousing> : grisé = non écrit"
            },
            ["tip.logoutdelay"] = new[]
            {
                "ServUO <logoutDelay>: grau = nicht schreiben",
                "ServUO <logoutDelay>: grey = not written",
                "ServUO <logoutDelay> : grisé = non écrit"
            },
            ["tip.extra"] = new[]
            {
                "Weitere Werte für Sphere (EVENTS, FLAGS, GROUP, TAG.…) und POL (POL.EnterText, POL.NoCombat …). Neue Zeile = neuer Wert.",
                "Other values for Sphere (EVENTS, FLAGS, GROUP, TAG.…) and POL (POL.EnterText, POL.NoCombat …). New row = new value.",
                "Autres valeurs pour Sphere (EVENTS, FLAGS, GROUP, TAG.…) et POL (POL.EnterText, POL.NoCombat …). Nouvelle ligne = nouvelle valeur."
            },
            ["tip.priority"] = new[]
            {
                "Höhere Priorität gewinnt bei überlappenden Regionen (Standard 50). Unterregionen erben sie.",
                "The higher priority wins when regions overlap (default 50). Child regions inherit it.",
                "La priorité la plus haute l'emporte si des régions se chevauchent (50 par défaut). Les sous-régions en héritent."
            },
            ["info.noselection"] = new[] { "Keine Region gewählt.", "No region selected.", "Aucune région choisie." },
            ["info.area"] = new[] { "{0} Fläche(n), {1:N0} Felder", "{0} area(s), {1:N0} cells", "{0} zone(s), {1:N0} cases" },
            ["info.children"] = new[] { "{0} Unterregion(en)", "{0} child region(s)", "{0} sous-région(s)" },
            ["rect.polygon"] = new[] { "Polygon ({0} Punkte)", "polygon ({0} points)", "polygone ({0} points)" },
            ["btn.rectadd"] = new[] { "＋ Hinzufügen", "＋ Add", "＋ Ajouter" },
            ["tip.rectadd"] = new[] { "Die Werte unten als neues Rechteck hinzufügen", "Add the values below as new rectangle", "Ajouter les valeurs ci-dessous comme nouveau rectangle" },
            ["btn.rectapply"] = new[] { "✓ Übernehmen", "✓ Apply", "✓ Appliquer" },
            ["tip.rectapply"] = new[] { "Die Werte unten auf das gewählte Rechteck anwenden", "Apply the values below to the selected rectangle", "Appliquer les valeurs ci-dessous au rectangle choisi" },
            ["tip.rectdelete"] = new[] { "Gewähltes Rechteck / Polygon löschen", "Delete the selected rectangle / polygon", "Supprimer le rectangle / polygone choisi" },
            ["btn.rectoptimize"] = new[] { "⧈ Optimieren", "⧈ Optimize", "⧈ Optimiser" },
            ["tip.rectoptimize"] = new[] { "Rechtecke zu möglichst wenigen zusammenfassen", "Merge rectangles into as few as possible", "Fusionner les rectangles en un minimum" },
            ["tip.polytorects"] = new[] { "Polygone in Rechtecke umwandeln", "Convert polygons into rectangles", "Convertir les polygones en rectangles" },

            // ── check / history ─────────────────────────────────────
            ["btn.runcheck"] = new[] { "▶ Prüfen", "▶ Check", "▶ Vérifier" },
            ["btn.fix"] = new[] { "Beheben", "Fix", "Corriger" },
            ["tip.fix"] = new[] { "Gewählte Probleme automatisch beheben", "Fix the selected problems automatically", "Corriger automatiquement les problèmes choisis" },
            ["btn.fixall"] = new[] { "Alle beheben", "Fix all", "Tout corriger" },
            ["tip.fixall"] = new[] { "Alle automatisch behebbaren Probleme beheben (🔧)", "Fix all problems that can be fixed automatically (🔧)", "Corriger tous les problèmes corrigeables automatiquement (🔧)" },
            ["check.ok"] = new[] { "✔ Keine Probleme gefunden", "✔ No problems found", "✔ Aucun problème trouvé" },
            ["check.summary"] = new[] { "{0} Fehler, {1} Warnungen, {2} Hinweise", "{0} errors, {1} warnings, {2} notes", "{0} erreurs, {1} avertissements, {2} remarques" },
            ["btn.histjump"] = new[] { "Bis hierher zurück", "Back to here", "Revenir jusqu'ici" },
            ["tip.histjump"] = new[] { "Alle Schritte bis einschließlich des gewählten rückgängig machen", "Undo all steps up to and including the selected one", "Annuler toutes les étapes jusqu'à celle choisie incluse" },
            ["btn.histclear"] = new[] { "Leeren", "Clear", "Vider" },
            ["hist.current"] = new[] { "aktueller Stand", "current state", "état actuel" },

            // ── checks of the validator ─────────────────────────────
            ["chk.noname"] = new[] { "Region ohne Namen", "Region without a name", "Région sans nom" },
            ["chk.dupname"] = new[] { "Name „{0}“ gibt es mehrfach", "The name \"{0}\" is used more than once", "Le nom « {0} » est utilisé plusieurs fois" },
            ["chk.namechars"] = new[] { "Name enthält \" < oder & – manche Server mögen das nicht", "The name contains \" < or & – some servers do not like that", "Le nom contient \" < ou & – certains serveurs ne l'acceptent pas" },
            ["chk.noarea"] = new[] { "Region hat keine Fläche", "The region has no area", "La région n'a aucune zone" },
            ["chk.zerosize"] = new[] { "Rechteck ohne Größe: {0}", "Rectangle without size: {0}", "Rectangle sans taille : {0}" },
            ["chk.outside"] = new[] { "Rechteck {0} liegt außerhalb der Karte ({1}×{2})", "Rectangle {0} is outside the map ({1}×{2})", "Le rectangle {0} est hors de la carte ({1}×{2})" },
            ["chk.zorder"] = new[] { "Z-Min ist größer als Z-Max ({0})", "z min is larger than z max ({0})", "Z min est supérieur à Z max ({0})" },
            ["chk.polypoints"] = new[] { "Polygon mit weniger als 3 Punkten", "Polygon with less than 3 points", "Polygone de moins de 3 points" },
            ["chk.duprect"] = new[] { "Rechteck doppelt: {0}", "Duplicate rectangle: {0}", "Rectangle en double : {0}" },
            ["chk.optimize"] = new[] { "{0} Rechtecke lassen sich zu {1} zusammenfassen", "{0} rectangles can be merged into {1}", "{0} rectangles peuvent être fusionnés en {1}" },
            ["chk.childoutside"] = new[] { "Unterregion ragt aus „{0}“ heraus", "Child region reaches outside \"{0}\"", "La sous-région dépasse de « {0} »" },
            ["chk.priority"] = new[] { "Priorität {0} liegt nicht zwischen 0 und 255", "Priority {0} is not between 0 and 255", "La priorité {0} n'est pas entre 0 et 255" },
            ["chk.music"] = new[] { "Unbekannte Musik „{0}“ (ServUO MusicName)", "Unknown music \"{0}\" (ServUO MusicName)", "Musique inconnue « {0} » (ServUO MusicName)" },
            ["chk.type"] = new[] { "Eigener Regionstyp „{0}“ – die C#-Klasse muss es im Server geben (Export → C#-Klassen)", "Own region type \"{0}\" – the C# class must exist in the server (Export → C# classes)", "Type de région personnalisé « {0} » – la classe C# doit exister dans le serveur (Export → classes C#)" },
            ["chk.gooutside"] = new[] { "Go-Punkt {0} liegt nicht in der Region", "Go location {0} is not inside the region", "Le point « go » {0} n'est pas dans la région" },
            ["chk.gowater"] = new[] { "Go-Punkt {0} liegt im Wasser", "Go location {0} is in the water", "Le point « go » {0} est dans l'eau" },
            ["chk.entrancewater"] = new[] { "Eingang {0} liegt im Wasser", "Entrance {0} is in the water", "L'entrée {0} est dans l'eau" },
            ["chk.entrancetype"] = new[] { "Eingang wird nur von DungeonRegion benutzt", "The entrance is only used by DungeonRegion", "L'entrée n'est utilisée que par DungeonRegion" },
            ["chk.noentrance"] = new[] { "DungeonRegion ohne Eingang", "DungeonRegion without entrance", "DungeonRegion sans entrée" },
            ["chk.polyconvert"] = new[] { "{0} Polygon(e) werden beim Export in Rechtecke umgewandelt", "{0} polygon(s) are converted into rectangles on export", "{0} polygone(s) seront convertis en rectangles à l'export" },
            ["chk.overlap"] = new[] { "Überlappt „{0}“ ({1:N0} Felder) mit gleicher Priorität {2} – der Server wählt zufällig", "Overlaps \"{0}\" ({1:N0} cells) with the same priority {2} – the server picks one at random", "Chevauche « {0} » ({1:N0} cases) avec la même priorité {2} – le serveur choisit au hasard" },

            // ── history entries ─────────────────────────────────────
            ["hist.add"] = new[] { "Region „{0}“ hinzugefügt", "Added region \"{0}\"", "Région « {0} » ajoutée" },
            ["hist.delete"] = new[] { "Region „{0}“ gelöscht", "Deleted region \"{0}\"", "Région « {0} » supprimée" },
            ["hist.duplicate"] = new[] { "Region „{0}“ dupliziert", "Duplicated region \"{0}\"", "Région « {0} » dupliquée" },
            ["hist.rename"] = new[] { "„{0}“ umbenannt in „{1}“", "Renamed \"{0}\" to \"{1}\"", "« {0} » renommée en « {1} »" },
            ["hist.move"] = new[] { "„{0}“ verschoben", "Moved \"{0}\"", "« {0} » déplacée" },
            ["hist.props"] = new[] { "Eigenschaften von „{0}“", "Properties of \"{0}\"", "Propriétés de « {0} »" },
            ["hist.addarea"] = new[] { "Fläche zu „{0}“ hinzugefügt", "Added area to \"{0}\"", "Zone ajoutée à « {0} »" },
            ["hist.editarea"] = new[] { "Fläche von „{0}“ geändert", "Changed area of \"{0}\"", "Zone de « {0} » modifiée" },
            ["hist.deletearea"] = new[] { "Fläche von „{0}“ gelöscht", "Deleted area of \"{0}\"", "Zone de « {0} » supprimée" },
            ["hist.optimize"] = new[] { "Rechtecke von „{0}“ optimiert", "Optimized rectangles of \"{0}\"", "Rectangles de « {0} » optimisés" },
            ["hist.polytorects"] = new[] { "Polygone von „{0}“ umgewandelt", "Converted polygons of \"{0}\"", "Polygones de « {0} » convertis" },
            ["hist.cut"] = new[] { "Aus „{0}“ ausgeschnitten", "Cut out of \"{0}\"", "Découpé de « {0} »" },
            ["hist.go"] = new[] { "Go-Punkt von „{0}“", "Go location of \"{0}\"", "Point « go » de « {0} »" },
            ["hist.entrance"] = new[] { "Eingang von „{0}“", "Entrance of \"{0}\"", "Entrée de « {0} »" },
            ["hist.drag"] = new[] { "„{0}“ auf der Karte geändert", "Changed \"{0}\" on the map", "« {0} » modifiée sur la carte" },
            ["hist.import"] = new[] { "Import „{0}“", "Import \"{0}\"", "Import « {0} »" },
            ["hist.fix"] = new[] { "{0} Problem(e) behoben", "Fixed {0} problem(s)", "{0} problème(s) corrigé(s)" },

            // ── messages ────────────────────────────────────────────
            ["msg.undone"] = new[] { "Rückgängig: {0}", "Undone: {0}", "Annulé : {0}" },
            ["msg.redone"] = new[] { "Wiederholt: {0}", "Redone: {0}", "Rétabli : {0}" },
            ["msg.undosteps"] = new[] { "{0} Schritt(e) rückgängig gemacht", "Undid {0} step(s)", "{0} étape(s) annulée(s)" },
            ["msg.selectfirst"] = new[] { "Bitte zuerst eine Region wählen.", "Please select a region first.", "Veuillez d'abord choisir une région." },
            ["msg.drawarea"] = new[] { "Jetzt die Fläche der Region auf der Karte zeichnen.", "Now draw the area of the region on the map.", "Dessinez maintenant la zone de la région sur la carte." },
            ["msg.created"] = new[] { "Region „{0}“ erstellt – Namen eingeben.", "Region \"{0}\" created – enter a name.", "Région « {0} » créée – saisissez un nom." },
            ["msg.pickgo"] = new[] { "Auf die Karte klicken, um den Go-Punkt zu setzen.", "Click the map to set the go location.", "Cliquez sur la carte pour placer le point « go »." },
            ["msg.pickentrance"] = new[] { "Auf die Karte klicken, um den Eingang zu setzen.", "Click the map to set the entrance.", "Cliquez sur la carte pour placer l'entrée." },
            ["msg.copied"] = new[] { "In die Zwischenablage kopiert.", "Copied to the clipboard.", "Copié dans le presse-papiers." },
            ["msg.nooptimize"] = new[] { "Die Rechtecke sind bereits optimal.", "The rectangles are already optimal.", "Les rectangles sont déjà optimaux." },
            ["msg.optimized"] = new[] { "{0} Rechtecke → {1}", "{0} rectangles → {1}", "{0} rectangles → {1}" },
            ["msg.filled"] = new[] { "Gefüllt: {0:N0} Felder ({2}) in {1} Rechtecken", "Filled: {0:N0} cells ({2}) in {1} rectangles", "Rempli : {0:N0} cases ({2}) en {1} rectangles" },
            ["msg.fillnone"] = new[] { "Hier kann nicht gefüllt werden (Fläche zu groß oder außerhalb).", "Nothing to fill here (area too large or outside).", "Rien à remplir ici (zone trop grande ou hors carte)." },
            ["msg.mapmissing"] = new[] { "Die Dateien der Karte „{0}“ fehlen.", "The files of the map \"{0}\" are missing.", "Les fichiers de la carte « {0} » sont absents." },
            ["msg.mapunknown"] = new[] { "Karte {0} ist nicht in der Kartenliste.", "Map {0} is not in the map list.", "La carte {0} n'est pas dans la liste." },
            ["msg.noclient"] = new[] { "Kein laufender Client gefunden.", "No running client found.", "Aucun client lancé trouvé." },
            ["msg.sent"] = new[] { "Client geschickt nach {0}, {1}, {2}", "Sent the client to {0}, {1}, {2}", "Client envoyé à {0}, {1}, {2}" },
            ["msg.newproject"] = new[] { "Neues Projekt.", "New project.", "Nouveau projet." },
            ["msg.saved"] = new[] { "Gespeichert: {0}", "Saved: {0}", "Enregistré : {0}" },
            ["msg.loaded"] = new[] { "{0}: {1} Regionen auf {2} Karte(n) geladen", "{0}: loaded {1} regions on {2} map(s)", "{0} : {1} régions chargées sur {2} carte(s)" },
            ["msg.unknownfacets"] = new[]
            {
                "Diese Facets/Karten sind nicht in der Kartenliste von UoFiddler und können nicht angezeigt werden:\n{0}\n\nSie bleiben im Projekt erhalten und werden mit exportiert. Eigene Karten fügt man unter Karte → Karten verwalten hinzu.",
                "These facets/maps are not in the map list of UoFiddler and can not be shown:\n{0}\n\nThey stay in the project and are exported too. Own maps are added with Map → Manage maps.",
                "Ces facettes/cartes ne figurent pas dans la liste de UoFiddler et ne peuvent pas être affichées :\n{0}\n\nElles restent dans le projet et sont aussi exportées. Les cartes personnelles s'ajoutent via Carte → Gérer les cartes."
            },
            ["msg.written"] = new[] { "Geschrieben: {0}", "Written: {0}", "Écrit : {0}" },
            ["msg.writtenbackup"] = new[] { "Geschrieben: {0} (Sicherung: {1})", "Written: {0} (backup: {1})", "Écrit : {0} (sauvegarde : {1})" },
            ["msg.nocustomtypes"] = new[]
            {
                "Das Projekt benutzt nur Regionstypen, die ServUO schon kennt – es gibt keine Klassen zu erzeugen.",
                "The project only uses region types ServUO already knows – there are no classes to create.",
                "Le projet n'utilise que des types de région connus de ServUO – aucune classe à générer."
            },
            ["msg.csharp"] = new[] { "{0} C#-Datei(en) geschrieben nach {1}", "Wrote {0} C# file(s) into {1}", "{0} fichier(s) C# écrit(s) dans {1}" },
            ["msg.exportall"] = new[] { "{0} Dateien exportiert nach {1}", "Exported {0} files into {1}", "{0} fichiers exportés dans {1}" },
            ["msg.fixed"] = new[] { "{0} Problem(e) behoben.", "Fixed {0} problem(s).", "{0} problème(s) corrigé(s)." },
            ["msg.restart"] = new[]
            {
                "Gespeichert. Der Server liest die Regionen beim Start – bitte den Server neu starten.",
                "Saved. The server reads the regions when it starts – please restart the server.",
                "Enregistré. Le serveur lit les régions au démarrage – veuillez redémarrer le serveur."
            },

            // ── questions ───────────────────────────────────────────
            ["ask.savechanges"] = new[] { "Das Projekt wurde geändert. Jetzt speichern?", "The project was changed. Save it now?", "Le projet a été modifié. L'enregistrer maintenant ?" },
            ["ask.deletechildren"] = new[] { "„{0}“ mit {1} Unterregion(en) löschen?", "Delete \"{0}\" with {1} child region(s)?", "Supprimer « {0} » avec {1} sous-région(s) ?" },
            ["ask.importappend"] = new[]
            {
                "Die importierten Regionen zum aktuellen Projekt hinzufügen?\n\nJa = hinzufügen\nNein = neues Projekt",
                "Add the imported regions to the current project?\n\nYes = add\nNo = new project",
                "Ajouter les régions importées au projet actuel ?\n\nOui = ajouter\nNon = nouveau projet"
            },
            ["ask.exporterrors"] = new[]
            {
                "Die Prüfung hat {0} Fehler gefunden. Trotzdem schreiben?\n\nNein = Fehler anzeigen",
                "The check found {0} errors. Write anyway?\n\nNo = show the errors",
                "La vérification a trouvé {0} erreurs. Écrire quand même ?\n\nNon = afficher les erreurs"
            },
            ["ask.htmlimages"] = new[]
            {
                "Kartenbilder in die HTML-Übersicht einbetten? (dauert etwas, Datei wird größer)",
                "Embed map images in the HTML overview? (takes a moment, the file gets larger)",
                "Intégrer les images des cartes dans l'aperçu HTML ? (un peu plus long, fichier plus gros)"
            },
            ["ask.openexport"] = new[] { "{0} Dateien wurden nach\n{1}\nexportiert. Ordner öffnen?", "{0} files were exported into\n{1}\nOpen the folder?", "{0} fichiers ont été exportés dans\n{1}\nOuvrir le dossier ?" },
            ["ask.noserver"] = new[]
            {
                "Für {0} ist noch kein Ordner eingestellt. Jetzt die Einstellungen öffnen?",
                "No folder is set for {0} yet. Open the settings now?",
                "Aucun dossier n'est encore défini pour {0}. Ouvrir les paramètres ?"
            },
            ["ask.servermissing"] = new[] { "Nicht gefunden:\n{0}\n\nEinstellungen öffnen?", "Not found:\n{0}\n\nOpen the settings?", "Introuvable :\n{0}\n\nOuvrir les paramètres ?" },
            ["ask.saveservuo"] = new[]
            {
                "Regionen in\n{0}\nschreiben?\n\nVorhandene Datei: {1}\nVorher wird eine Sicherung (.bak) angelegt.",
                "Write the regions into\n{0}?\n\nExisting file: {1}\nA backup (.bak) is made first.",
                "Écrire les régions dans\n{0} ?\n\nFichier existant : {1}\nUne sauvegarde (.bak) est créée avant."
            },
            ["ask.saveserver"] = new[]
            {
                "Regionen in\n{0}\nschreiben? Vorher wird eine Sicherung (.bak) angelegt.",
                "Write the regions into\n{0}? A backup (.bak) is made first.",
                "Écrire les régions dans\n{0} ? Une sauvegarde (.bak) est créée avant."
            },
            ["ask.clearhistory"] = new[] { "Den Verlauf leeren? Danach ist kein Rückgängig mehr möglich.", "Clear the history? Undo is not possible afterwards.", "Vider l'historique ? Plus aucune annulation possible ensuite." },

            // ── server menu / loading ───────────────────────────────
            ["btn.server"] = new[] { "⇅ Server", "⇅ Server", "⇅ Serveur" },
            ["tip.server"] = new[]
            {
                "Regionen des eingestellten Servers laden oder in ihn speichern",
                "Load the regions of the configured server or save them into it",
                "Charger les régions du serveur configuré ou les y enregistrer"
            },
            ["server.load"] = new[] { "Von {0} laden", "Load from {0}", "Charger depuis {0}" },
            ["server.save"] = new[] { "In {0} speichern", "Save into {0}", "Enregistrer dans {0}" },
            ["server.folders"] = new[] { "Server-Ordner festlegen…", "Set server folders…", "Définir les dossiers du serveur…" },
            ["msg.serverloaded"] = new[]
            {
                "{0}: {1} Regionen auf {2} Karte(n) geladen aus {3}",
                "{0}: loaded {1} regions on {2} map(s) from {3}",
                "{0} : {1} régions chargées sur {2} carte(s) depuis {3}"
            },
            ["msg.serverfailed"] = new[] { "{0} konnte nicht geladen werden: {1}", "{0} could not be loaded: {1}", "{0} n'a pas pu être chargé : {1}" },
            ["msg.settingssaved"] = new[] { "Einstellungen gespeichert: {0}", "Settings saved: {0}", "Paramètres enregistrés : {0}" },
            ["settings.startup"] = new[] { "Beim Start laden:", "Load on start:", "Charger au démarrage :" },
            ["settings.loadnow"] = new[] { "Laden", "Load", "Charger" },
            ["settings.notset"] = new[] { "– nicht eingestellt", "– not set", "– non défini" },
            ["settings.notfound"] = new[] { "✖ nicht gefunden: {0}", "✖ not found: {0}", "✖ introuvable : {0}" },
            ["settings.found"] = new[] { "{0} Regionen auf {1} Karte(n)", "{0} regions on {1} map(s)", "{0} régions sur {1} carte(s)" },
            ["settings.unreadable"] = new[] { "nicht lesbar: {0}", "can not be read: {0}", "illisible : {0}" },
            ["settings.saveerror"] = new[]
            {
                "Die Einstellungen konnten nicht gespeichert werden:\n{0}\n\n{1}",
                "The settings could not be saved:\n{0}\n\n{1}",
                "Les paramètres n'ont pas pu être enregistrés :\n{0}\n\n{1}"
            },
            ["startup.LastProject"] = new[] { "zuletzt geöffnetes Projekt", "last opened project", "dernier projet ouvert" },
            ["startup.ServUo"] = new[] { "Regionen vom ServUO-Server", "regions of the ServUO server", "régions du serveur ServUO" },
            ["startup.Sphere"] = new[] { "Regionen vom Sphere-Server", "regions of the Sphere server", "régions du serveur Sphere" },
            ["startup.Pol"] = new[] { "Regionen vom POL-Server", "regions of the POL server", "régions du serveur POL" },
            ["startup.Nothing"] = new[] { "nichts (leeres Projekt)", "nothing (empty project)", "rien (projet vide)" },

            // ── settings ────────────────────────────────────────────
            ["settings.title"] = new[] { "Regionen – Einstellungen", "Regions – settings", "Régions – paramètres" },
            ["settings.server"] = new[] { "Server-Ordner", "Server folders", "Dossiers du serveur" },
            ["settings.servuo"] = new[] { "ServUO-/RunUO-Ordner:", "ServUO / RunUO folder:", "Dossier ServUO / RunUO :" },
            ["settings.servuo.hint"] = new[] { "z. B. C:\\ServUO  (Data\\Regions.xml)", "e.g. C:\\ServUO  (Data\\Regions.xml)", "p. ex. C:\\ServUO  (Data\\Regions.xml)" },
            ["settings.sphere"] = new[] { "Sphere-Areas-Datei:", "Sphere areas file:", "Fichier des zones Sphere :" },
            ["settings.sphere.hint"] = new[] { "z. B. C:\\Sphere\\scripts\\maps\\map0\\map0_areas.scp", "e.g. C:\\Sphere\\scripts\\maps\\map0\\map0_areas.scp", "p. ex. C:\\Sphere\\scripts\\maps\\map0\\map0_areas.scp" },
            ["settings.pol"] = new[] { "POL-Ordner regions:", "POL regions folder:", "Dossier regions de POL :" },
            ["settings.pol.hint"] = new[] { "z. B. C:\\pol\\regions", "e.g. C:\\pol\\regions", "p. ex. C:\\pol\\regions" },
            ["settings.export"] = new[] { "Export-Ordner:", "Export folder:", "Dossier d'export :" },
            ["settings.export.hint"] = new[] { "von hier zum Server kopieren", "copy the files from here to your server", "copiez les fichiers d'ici vers votre serveur" },
            ["settings.exportto"] = new[] { "Export nach: {0}", "Export into: {0}", "Export vers : {0}" },
            ["settings.exportoptions"] = new[] { "Export", "Export", "Export" },
            ["settings.merge"] = new[] { "Vorhandene Regions.xml:", "Existing Regions.xml:", "Regions.xml existant :" },
            ["settings.rectformat"] = new[] { "Rechtecke schreiben als:", "Write rectangles as:", "Écrire les rectangles en :" },
            ["settings.sphereevents"] = new[] { "Sphere EVENTS (neue Regionen):", "Sphere EVENTS (new regions):", "EVENTS Sphere (nouvelles régions) :" },
            ["settings.skiphidden"] = new[] { "Ausgeblendete Regionen nicht exportieren", "Do not export hidden regions", "Ne pas exporter les régions masquées" },
            ["settings.poltexts"] = new[] { "POL: Wachen-Texte (EnterText/LeaveText) schreiben", "POL: write guard texts (EnterText/LeaveText)", "POL : écrire les textes des gardes (EnterText/LeaveText)" },
            ["settings.editor"] = new[] { "Editor", "Editor", "Éditeur" },
            ["settings.theme"] = new[] { "Design:", "Design:", "Apparence :" },
            ["settings.priority"] = new[] { "Priorität neuer Regionen:", "Priority of new regions:", "Priorité des nouvelles régions :" },
            ["settings.alpha"] = new[] { "Deckkraft der Füllung:", "Fill opacity:", "Opacité du remplissage :" },
            ["theme.light"] = new[] { "Hell", "Light", "Clair" },
            ["theme.dark"] = new[] { "Dunkel", "Dark", "Sombre" },
            ["merge.ReplaceFile"] = new[] { "ganz ersetzen", "replace completely", "remplacer entièrement" },
            ["merge.ReplaceFacets"] = new[] { "nur die Facets des Projekts ersetzen", "replace only the facets of the project", "remplacer seulement les facettes du projet" },
            ["merge.MergeByName"] = new[] { "Regionen nach Namen ersetzen/ergänzen", "replace/add regions by name", "remplacer/ajouter les régions par nom" }
        };
    }
}
