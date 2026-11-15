/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Texts of the client check in German, English and French.
 * The chosen language is stored in <AppData>\ClientCheck.lang.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.ClientCheck.Classes
{
    public static class Loc
    {
        public static readonly string[] Languages = { "de", "en", "fr" };
        public static readonly string[] LanguageNames = { "Deutsch", "English", "Français" };

        private static string _language;

        /// <summary>Raised after the language was changed</summary>
        public static event EventHandler LanguageChanged;

        public static string Language
        {
            get
            {
                if (_language == null)
                {
                    _language = LoadLanguage();
                }

                return _language;
            }
            set
            {
                if (Array.IndexOf(Languages, value) < 0 || value == _language)
                {
                    return;
                }

                _language = value;
                SaveLanguage(value);
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static string SettingsFile => Path.Combine(Options.AppDataPath ?? string.Empty, "ClientCheck.lang");

        private static string LoadLanguage()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string lang = File.ReadAllText(SettingsFile).Trim();
                    if (Array.IndexOf(Languages, lang) >= 0)
                    {
                        return lang;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            // first start: language of Windows (German, French), otherwise English
            string ui = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return ui == "de" || ui == "fr" ? ui : "en";
        }

        private static void SaveLanguage(string language)
        {
            try
            {
                File.WriteAllText(SettingsFile, language);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>Text of a key in the current language</summary>
        public static string T(string key)
        {
            if (!_texts.TryGetValue(key, out string[] values))
            {
                return key;
            }

            int i = Array.IndexOf(Languages, Language);
            return values[i < 0 ? 1 : i];
        }

        /// <summary>Formatted text of a key</summary>
        public static string F(string key, params object[] args)
        {
            string format = T(key);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, args ?? Array.Empty<object>());
            }
            catch (FormatException)
            {
                return format;
            }
        }

        // key -> { German, English, French }
        private static readonly Dictionary<string, string[]> _texts = new Dictionary<string, string[]>
        {
            // ── UI ───────────────────────────────────────────────────
            ["tab.title"] = new[] { "Client-Prüfung", "Client Check", "Vérification du client" },
            ["plugin.description"] = new[]
            {
                "Prüft alle Client-Dateien gegeneinander (Art, tiledata, Animationen, Gumps, radarcol, Multis, Karten, Cliloc).",
                "Checks all client files against each other (art, tiledata, animations, gumps, radarcol, multis, maps, cliloc).",
                "Vérifie la cohérence de tous les fichiers du client (art, tiledata, animations, gumps, radarcol, multis, cartes, cliloc)."
            },
            ["btn.start"] = new[] { "▶ Prüfung starten", "▶ Start check", "▶ Lancer la vérification" },
            ["btn.cancel"] = new[] { "■ Abbrechen", "■ Cancel", "■ Annuler" },
            ["btn.checks"] = new[] { "Prüfungen", "Checks", "Contrôles" },
            ["btn.checks.all"] = new[] { "Alle auswählen", "Select all", "Tout sélectionner" },
            ["btn.checks.none"] = new[] { "Keine auswählen", "Select none", "Ne rien sélectionner" },
            ["filter.show"] = new[] { "Anzeigen:", "Show:", "Afficher :" },
            ["filter.allareas"] = new[] { "Alle Bereiche", "All areas", "Tous les domaines" },
            ["filter.search"] = new[] { "Suche:", "Search:", "Recherche :" },
            ["filter.hidefixed"] = new[] { "Behobene ausblenden", "Hide fixed", "Masquer les corrigés" },
            ["sev.all"] = new[] { "Alle Stufen", "All levels", "Tous les niveaux" },
            ["sev.Error"] = new[] { "Fehler", "Error", "Erreur" },
            ["sev.Warning"] = new[] { "Warnung", "Warning", "Avertissement" },
            ["sev.Info"] = new[] { "Hinweis", "Info", "Info" },
            ["btn.fix"] = new[] { "Beheben", "Fix", "Corriger" },
            ["btn.fixall"] = new[] { "Alle behebbaren beheben…", "Fix all fixable…", "Tout corriger…" },
            ["btn.export"] = new[] { "Bericht exportieren", "Export report", "Exporter le rapport" },
            ["btn.export.html"] = new[] { "HTML (Webseite)…", "HTML (web page)…", "HTML (page web)…" },
            ["btn.export.csv"] = new[] { "CSV (Tabelle)…", "CSV (spreadsheet)…", "CSV (tableur)…" },
            ["btn.export.txt"] = new[] { "Text…", "Text…", "Texte…" },
            ["btn.saveradar"] = new[] { "radarcol.mul speichern", "Save radarcol.mul", "Enregistrer radarcol.mul" },
            ["btn.help"] = new[] { "? Hilfe", "? Help", "? Aide" },
            ["btn.copyid"] = new[] { "ID kopieren", "Copy ID", "Copier l'ID" },
            ["btn.copyline"] = new[] { "Zeile kopieren", "Copy line", "Copier la ligne" },
            ["lang.label"] = new[] { "Sprache:", "Language:", "Langue :" },
            ["col.severity"] = new[] { "Stufe", "Level", "Niveau" },
            ["col.category"] = new[] { "Bereich", "Area", "Domaine" },
            ["col.id"] = new[] { "ID", "ID", "ID" },
            ["col.message"] = new[] { "Beschreibung", "Description", "Description" },
            ["detail.hint"] = new[] { "So beheben:", "How to fix:", "Comment corriger :" },
            ["detail.autofix"] = new[] { "Automatisch behebbar:", "Can be fixed automatically:", "Correction automatique possible :" },
            ["detail.fixed"] = new[] { "✓ Behoben", "✓ Fixed", "✓ Corrigé" },
            ["detail.nopreview"] = new[] { "keine Vorschau", "no preview", "pas d'aperçu" },
            ["detail.select"] = new[] { "Einen Eintrag auswählen.", "Select an entry.", "Sélectionnez une entrée." },

            ["status.ready"] = new[]
            {
                "Bereit. Prüfungen wählen und „Prüfung starten“ klicken.",
                "Ready. Choose the checks and click \"Start check\".",
                "Prêt. Choisissez les contrôles et cliquez sur « Lancer la vérification »."
            },
            ["status.running"] = new[] { "Prüfe: {0} …", "Checking: {0} …", "Vérification : {0} …" },
            ["status.done"] = new[]
            {
                "Fertig in {0:0.0} s – {1} Fehler, {2} Warnungen, {3} Hinweise.",
                "Done in {0:0.0} s – {1} errors, {2} warnings, {3} infos.",
                "Terminé en {0:0.0} s – {1} erreurs, {2} avertissements, {3} infos."
            },
            ["status.cancelled"] = new[] { "Abgebrochen.", "Cancelled.", "Annulé." },
            ["status.failed"] = new[] { "Prüfung fehlgeschlagen: {0}", "Check failed: {0}", "Échec de la vérification : {0}" },
            ["status.fixed"] = new[] { "{0} Einträge behoben.", "{0} entries fixed.", "{0} entrées corrigées." },
            ["status.shown"] = new[] { "{0} von {1} Einträgen angezeigt", "{0} of {1} entries shown", "{0} sur {1} entrées affichées" },
            ["summary"] = new[] { "{0} Fehler · {1} Warnungen · {2} Hinweise", "{0} errors · {1} warnings · {2} infos", "{0} erreurs · {1} avertissements · {2} infos" },

            ["msg.nofix"] = new[]
            {
                "Für diesen Eintrag gibt es keine automatische Behebung – siehe „So beheben“.",
                "There is no automatic fix for this entry – see \"How to fix\".",
                "Pas de correction automatique pour cette entrée – voir « Comment corriger »."
            },
            ["msg.nooutput"] = new[]
            {
                "Der Output-Ordner ist nicht gesetzt oder existiert nicht (Optionen).",
                "The output folder is not set or does not exist (options).",
                "Le dossier de sortie n'est pas défini ou n'existe pas (options)."
            },
            ["msg.fixconfirm"] = new[]
            {
                "{0} Einträge automatisch beheben?\n\n{1}",
                "Fix {0} entries automatically?\n\n{1}",
                "Corriger automatiquement {0} entrées ?\n\n{1}"
            },
            ["msg.radarsaved"] = new[] { "radarcol.mul gespeichert:\n{0}", "radarcol.mul saved:\n{0}", "radarcol.mul enregistré :\n{0}" },
            ["msg.exported"] = new[] { "Bericht gespeichert:\n{0}", "Report saved:\n{0}", "Rapport enregistré :\n{0}" },
            ["msg.radarunsaved"] = new[]
            {
                "Es gibt geänderte Radarfarben, die noch nicht gespeichert sind. Jetzt speichern?",
                "There are changed radar colours that are not saved yet. Save now?",
                "Des couleurs radar modifiées ne sont pas encore enregistrées. Enregistrer maintenant ?"
            },
            ["fix.radar"] = new[]
            {
                "Radarfarbe = Durchschnittsfarbe der Grafik (im Speicher; danach „radarcol.mul speichern“).",
                "Radar colour = average colour of the graphic (in memory; then \"Save radarcol.mul\").",
                "Couleur radar = couleur moyenne du graphisme (en mémoire ; puis « Enregistrer radarcol.mul »)."
            },
            ["fix.mobtypes"] = new[]
            {
                "Zeile in mobtypes.txt eintragen (Kopie im Output-Ordner, Backup .bak).",
                "Add the line to mobtypes.txt (copy in the output folder, backup .bak).",
                "Ajouter la ligne à mobtypes.txt (copie dans le dossier de sortie, sauvegarde .bak)."
            },
            ["report.title"] = new[] { "UoFiddler – Client-Prüfung", "UoFiddler – Client check", "UoFiddler – Vérification du client" },
            ["report.date"] = new[] { "Erstellt", "Created", "Créé le" },
            ["report.client"] = new[] { "Client-Ordner", "Client folder", "Dossier du client" },

            // ── categories ───────────────────────────────────────────
            ["cat.ArtTileData"] = new[] { "Art ↔ tiledata", "Art ↔ tiledata", "Art ↔ tiledata" },
            ["cat.LandTiles"] = new[] { "Land-Tiles", "Land tiles", "Tuiles de terrain" },
            ["cat.Equipment"] = new[] { "Ausrüstung", "Equipment", "Équipement" },
            ["cat.Bodies"] = new[] { "Bodies / Animationen", "Bodies / animations", "Corps / animations" },
            ["cat.RadarCol"] = new[] { "radarcol (Kartenfarben)", "radarcol (map colours)", "radarcol (couleurs de carte)" },
            ["cat.Multis"] = new[] { "Multis", "Multis", "Multis" },
            ["cat.Statics"] = new[] { "Statics auf Karten", "Statics on maps", "Statiques sur les cartes" },
            ["cat.Cliloc"] = new[] { "Cliloc (Namen)", "Cliloc (names)", "Cliloc (noms)" },

            ["step.prepare"] = new[] { "Vorbereitung", "Preparing", "Préparation" },

            // ── issues: message / hint ───────────────────────────────
            ["art.noname"] = new[]
            {
                "Item-Grafik 0x{0:X4} hat keinen Namen in der tiledata.",
                "Item graphic 0x{0:X4} has no name in tiledata.",
                "Le graphisme d'objet 0x{0:X4} n'a pas de nom dans tiledata."
            },
            ["art.noname.hint"] = new[]
            {
                "Im Tab „TileData“ Namen und Flags setzen und tiledata.mul speichern. Ohne Namen zeigt der Client nichts an, wenn kein Cliloc existiert.",
                "Set name and flags in the \"TileData\" tab and save tiledata.mul. Without a name the client shows nothing if there is no cliloc.",
                "Définir le nom et les flags dans l'onglet « TileData » puis enregistrer tiledata.mul. Sans nom, le client n'affiche rien s'il n'y a pas de cliloc."
            },
            ["art.tiledatanoart"] = new[]
            {
                "tiledata-Eintrag 0x{0:X4} „{1}“ hat keine Item-Grafik.",
                "tiledata entry 0x{0:X4} \"{1}\" has no item graphic.",
                "L'entrée tiledata 0x{0:X4} « {1} » n'a pas de graphisme d'objet."
            },
            ["art.tiledatanoart.hint"] = new[]
            {
                "Grafik im Tab „Items“ einfügen – oder den tiledata-Eintrag leeren, falls er nicht gebraucht wird.",
                "Add the graphic in the \"Items\" tab – or clear the tiledata entry if it is not needed.",
                "Ajouter le graphisme dans l'onglet « Items » – ou vider l'entrée tiledata si elle est inutile."
            },
            ["land.noname"] = new[]
            {
                "Land-Tile 0x{0:X4} hat keinen Namen in der tiledata.",
                "Land tile 0x{0:X4} has no name in tiledata.",
                "La tuile de terrain 0x{0:X4} n'a pas de nom dans tiledata."
            },
            ["land.noname.hint"] = new[]
            {
                "Im Tab „TileData“ (Land) einen Namen setzen und tiledata.mul speichern.",
                "Set a name in the \"TileData\" tab (land) and save tiledata.mul.",
                "Définir un nom dans l'onglet « TileData » (terrain) puis enregistrer tiledata.mul."
            },
            ["land.notexture"] = new[]
            {
                "Land-Tile 0x{0:X4} „{1}“ verweist auf Textur {2} (0x{2:X}), die fehlt.",
                "Land tile 0x{0:X4} \"{1}\" points to texture {2} (0x{2:X}) which is missing.",
                "La tuile de terrain 0x{0:X4} « {1} » utilise la texture {2} (0x{2:X}) qui manque."
            },
            ["land.notexture.hint"] = new[]
            {
                "Textur im Tab „Texturen“ einfügen oder in der tiledata TextureId auf 0 setzen (dann nur flaches Land-Art). Schräges Gelände sieht ohne Textur falsch aus.",
                "Add the texture in the \"Textures\" tab or set TextureId to 0 in tiledata (flat land art only). Sloped ground looks wrong without a texture.",
                "Ajouter la texture dans l'onglet « Textures » ou mettre TextureId à 0 dans tiledata (art de terrain plat uniquement). Les pentes s'affichent mal sans texture."
            },
            ["equip.noanim"] = new[]
            {
                "Ausrüstung 0x{0:X4} „{1}“: tiledata-Animation {2} existiert nicht.",
                "Equipment 0x{0:X4} \"{1}\": tiledata animation {2} does not exist.",
                "Équipement 0x{0:X4} « {1} » : l'animation tiledata {2} n'existe pas."
            },
            ["equip.noanim.hint"] = new[]
            {
                "Animation {2} im Animation-Editor anlegen oder das Feld „Animation“ in der tiledata korrigieren. Sonst ist das Item am Körper unsichtbar.",
                "Create animation {2} in the animation editor or correct the field \"Animation\" in tiledata. Otherwise the item is invisible when worn.",
                "Créer l'animation {2} dans l'éditeur d'animations ou corriger le champ « Animation » dans tiledata. Sinon l'objet est invisible une fois porté."
            },
            ["equip.nolayer"] = new[]
            {
                "Ausrüstung 0x{0:X4} „{1}“ ist „Wearable“, hat aber keinen Layer (Quality = 0).",
                "Equipment 0x{0:X4} \"{1}\" is \"Wearable\" but has no layer (Quality = 0).",
                "L'équipement 0x{0:X4} « {1} » est « Wearable » mais n'a pas de couche (Quality = 0)."
            },
            ["equip.nolayer.hint"] = new[]
            {
                "In der tiledata „Quality“ auf den Layer setzen (z. B. 1 = Einhand, 5 = Hemd, 6 = Helm, 22 = Robe/Überkleid).",
                "Set \"Quality\" in tiledata to the layer (e.g. 1 = one-handed, 5 = shirt, 6 = helm, 22 = robe/outer torso).",
                "Mettre « Quality » à la couche dans tiledata (ex. 1 = une main, 5 = chemise, 6 = casque, 22 = robe/torse extérieur)."
            },
            ["equip.nogump"] = new[]
            {
                "Ausrüstung 0x{0:X4} „{1}“: Paperdoll-Gump {2} (männlich) fehlt.",
                "Equipment 0x{0:X4} \"{1}\": paperdoll gump {2} (male) is missing.",
                "Équipement 0x{0:X4} « {1} » : le gump de paperdoll {2} (homme) manque."
            },
            ["equip.nogump.hint"] = new[]
            {
                "Gump {2} (= 50000 + Animation) im Tab „Gumps“ einfügen oder einen Ersatz in gump.def eintragen. Sonst fehlt das Item auf dem Paperdoll.",
                "Add gump {2} (= 50000 + animation) in the \"Gumps\" tab or add a replacement in gump.def. Otherwise the item is missing on the paperdoll.",
                "Ajouter le gump {2} (= 50000 + animation) dans l'onglet « Gumps » ou un remplacement dans gump.def. Sinon l'objet n'apparaît pas sur le paperdoll."
            },
            ["equip.nogumpfemale"] = new[]
            {
                "Ausrüstung 0x{0:X4} „{1}“: weiblicher Paperdoll-Gump {2} fehlt (der Client nimmt dann den männlichen).",
                "Equipment 0x{0:X4} \"{1}\": female paperdoll gump {2} is missing (the client uses the male one).",
                "Équipement 0x{0:X4} « {1} » : le gump féminin {2} manque (le client utilise celui de l'homme)."
            },
            ["equip.nogumpfemale.hint"] = new[]
            {
                "Optional: Gump {2} (= 60000 + Animation) einfügen, wenn die Frauen-Version anders aussehen soll.",
                "Optional: add gump {2} (= 60000 + animation) if the female version should look different.",
                "Optionnel : ajouter le gump {2} (= 60000 + animation) si la version féminine doit être différente."
            },
            ["body.nomobtypes"] = new[]
            {
                "Body {0}: Animation vorhanden, aber kein Eintrag in mobtypes.txt.",
                "Body {0}: animation exists, but there is no entry in mobtypes.txt.",
                "Corps {0} : l'animation existe mais il n'y a pas d'entrée dans mobtypes.txt."
            },
            ["body.nomobtypes.hint"] = new[]
            {
                "Zeile „{0}\t{1}\t0“ in mobtypes.txt eintragen (vorgeschlagener Typ {1}). Ohne Eintrag spielt der Client falsche Aktionen ab.",
                "Add the line \"{0}\t{1}\t0\" to mobtypes.txt (suggested type {1}). Without it the client plays wrong actions.",
                "Ajouter la ligne « {0}\t{1}\t0 » à mobtypes.txt (type proposé {1}). Sans elle, le client joue de mauvaises actions."
            },
            ["body.mobtypesnoanim"] = new[]
            {
                "mobtypes.txt enthält Body {0}, aber es gibt keine Animation dafür.",
                "mobtypes.txt contains body {0}, but there is no animation for it.",
                "mobtypes.txt contient le corps {0}, mais il n'a pas d'animation."
            },
            ["body.mobtypesnoanim.hint"] = new[]
            {
                "Animation anlegen oder die Zeile aus mobtypes.txt entfernen (unschädlich, nur Altlast).",
                "Create the animation or remove the line from mobtypes.txt (harmless, just leftover).",
                "Créer l'animation ou supprimer la ligne de mobtypes.txt (sans danger, simple reste)."
            },
            ["body.bodyconvmissing"] = new[]
            {
                "Bodyconv.def: Body {0} → anim{1}.mul Index {2}, dort sind keine Daten.",
                "Bodyconv.def: body {0} → anim{1}.mul index {2}, there is no data.",
                "Bodyconv.def : corps {0} → anim{1}.mul index {2}, aucune donnée."
            },
            ["body.bodyconvmissing.hint"] = new[]
            {
                "Animation in anim{1}.mul an Index {2} speichern oder die Zeile in Bodyconv.def korrigieren. Sonst ist das Monster unsichtbar.",
                "Save the animation into anim{1}.mul at index {2} or correct the line in Bodyconv.def. Otherwise the creature is invisible.",
                "Enregistrer l'animation dans anim{1}.mul à l'index {2} ou corriger la ligne de Bodyconv.def. Sinon la créature est invisible."
            },
            ["body.bodydefmissing"] = new[]
            {
                "body.def: Body {0} wird auf Body {1} umgeleitet, aber {1} hat keine Animation.",
                "body.def: body {0} is redirected to body {1}, but {1} has no animation.",
                "body.def : le corps {0} est redirigé vers {1}, mais {1} n'a pas d'animation."
            },
            ["body.bodydefmissing.hint"] = new[]
            {
                "Zeile in body.def auf einen vorhandenen Body ändern oder die Animation {1} anlegen.",
                "Change the line in body.def to an existing body or create animation {1}.",
                "Modifier la ligne de body.def vers un corps existant ou créer l'animation {1}."
            },
            ["radar.item"] = new[]
            {
                "Item 0x{0:X4} „{1}“ hat keine Radarfarbe – auf Karte und Radar schwarz.",
                "Item 0x{0:X4} \"{1}\" has no radar colour – black on the map and radar.",
                "L'objet 0x{0:X4} « {1} » n'a pas de couleur radar – noir sur la carte et le radar."
            },
            ["radar.item.hint"] = new[]
            {
                "„Beheben“ setzt die Durchschnittsfarbe der Grafik. Danach „radarcol.mul speichern“ und die Datei in den Client kopieren. Von Hand: Tab „RadarColor“.",
                "\"Fix\" sets the average colour of the graphic. Then \"Save radarcol.mul\" and copy the file into the client. Manually: tab \"RadarColor\".",
                "« Corriger » attribue la couleur moyenne du graphisme. Puis « Enregistrer radarcol.mul » et copier le fichier dans le client. À la main : onglet « RadarColor »."
            },
            ["radar.land"] = new[]
            {
                "Land-Tile 0x{0:X4} „{1}“ hat keine Radarfarbe – auf Karte und Radar schwarz.",
                "Land tile 0x{0:X4} \"{1}\" has no radar colour – black on the map and radar.",
                "La tuile de terrain 0x{0:X4} « {1} » n'a pas de couleur radar – noire sur la carte et le radar."
            },
            ["radar.land.hint"] = new[]
            {
                "„Beheben“ setzt die Durchschnittsfarbe des Land-Tiles. Danach „radarcol.mul speichern“ und die Datei in den Client kopieren.",
                "\"Fix\" sets the average colour of the land tile. Then \"Save radarcol.mul\" and copy the file into the client.",
                "« Corriger » attribue la couleur moyenne de la tuile. Puis « Enregistrer radarcol.mul » et copier le fichier dans le client."
            },
            ["radar.toosmall"] = new[]
            {
                "radarcol.mul ist zu kurz für Item 0x{0:X4} (Datei hat Platz bis 0x{1:X4}).",
                "radarcol.mul is too short for item 0x{0:X4} (the file has room up to 0x{1:X4}).",
                "radarcol.mul est trop court pour l'objet 0x{0:X4} (le fichier va jusqu'à 0x{1:X4})."
            },
            ["radar.toosmall.hint"] = new[]
            {
                "radarcol.mul muss für die erweiterten Item-IDs vergrößert werden (z. B. mit dem Tab „RadarColor“ / CSV-Import einer größeren Datei).",
                "radarcol.mul has to be enlarged for the extended item ids (e.g. tab \"RadarColor\" / CSV import of a bigger file).",
                "radarcol.mul doit être agrandi pour les identifiants d'objets étendus (ex. onglet « RadarColor » / import CSV d'un fichier plus grand)."
            },
            ["multi.invalid"] = new[]
            {
                "Multi {0} (0x{0:X}): {1} Bauteil(e) mit fehlender Item-Grafik, z. B. 0x{2:X4}.",
                "Multi {0} (0x{0:X}): {1} component(s) with a missing item graphic, e.g. 0x{2:X4}.",
                "Multi {0} (0x{0:X}) : {1} élément(s) sans graphisme d'objet, ex. 0x{2:X4}."
            },
            ["multi.invalid.hint"] = new[]
            {
                "Die fehlenden Grafiken im Tab „Items“ ergänzen oder die Bauteile im Multi-Editor ersetzen. Sonst fehlen Teile des Gebäudes/Schiffs.",
                "Add the missing graphics in the \"Items\" tab or replace the components in the multi editor. Otherwise parts of the house/ship are missing.",
                "Ajouter les graphismes manquants dans l'onglet « Items » ou remplacer les éléments dans l'éditeur de multis. Sinon des parties de la maison/du bateau manquent."
            },
            ["statics.invalidid"] = new[]
            {
                "Karte: Item 0x{0:X4} existiert nicht, aber ist {1}× platziert (z. B. {2}).",
                "Map: item 0x{0:X4} does not exist, but is placed {1}× (e.g. {2}).",
                "Carte : l'objet 0x{0:X4} n'existe pas mais est placé {1}× (ex. {2})."
            },
            ["statics.invalidid.hint"] = new[]
            {
                "Grafik 0x{0:X4} im Tab „Items“ ergänzen oder die Statics im Kartentab (Region-Viewer / Map Editor) löschen bzw. ersetzen.",
                "Add graphic 0x{0:X4} in the \"Items\" tab or delete/replace the statics in the map tab (region viewer / map editor).",
                "Ajouter le graphisme 0x{0:X4} dans l'onglet « Items » ou supprimer/remplacer les statiques dans l'onglet carte (visionneuse de région / éditeur de carte)."
            },
            ["statics.invalidhue"] = new[]
            {
                "Karte: {0} Static(s) mit Hue {1} – es gibt nur {2} Hues (z. B. Item 0x{3:X4} bei {4}).",
                "Map: {0} static(s) with hue {1} – there are only {2} hues (e.g. item 0x{3:X4} at {4}).",
                "Carte : {0} statique(s) avec la teinte {1} – il n'y a que {2} teintes (ex. objet 0x{3:X4} à {4})."
            },
            ["statics.invalidhue.hint"] = new[]
            {
                "Hue der Statics im Region-Viewer korrigieren. Ungültige Hues zeigt der Client falsch oder gar nicht an.",
                "Correct the hue of the statics in the region viewer. The client shows invalid hues wrongly or not at all.",
                "Corriger la teinte des statiques dans la visionneuse de région. Le client affiche mal les teintes invalides."
            },
            ["statics.readerror"] = new[]
            {
                "Karte {0}: Statics konnten nicht gelesen werden ({1}).",
                "Map {0}: statics could not be read ({1}).",
                "Carte {0} : les statiques n'ont pas pu être lues ({1})."
            },
            ["statics.readerror.hint"] = new[]
            {
                "Prüfen, ob staidx/statics zusammenpassen (gleicher Stand, nicht beschädigt).",
                "Check that staidx/statics belong together (same version, not damaged).",
                "Vérifier que staidx/statics vont ensemble (même version, non endommagés)."
            },
            ["cliloc.item"] = new[]
            {
                "Item 0x{0:X4} „{1}“ hat keinen Cliloc-Namen ({2}).",
                "Item 0x{0:X4} \"{1}\" has no cliloc name ({2}).",
                "L'objet 0x{0:X4} « {1} » n'a pas de nom cliloc ({2})."
            },
            ["cliloc.item.hint"] = new[]
            {
                "Eintrag {2} im Tab „Cliloc“ anlegen (Text z. B. „{1}“). Der Client zeigt Item-Namen über Cliloc (1020000 + ID, ab 0x4000: 1078872 + ID).",
                "Add entry {2} in the \"Cliloc\" tab (text e.g. \"{1}\"). The client shows item names through cliloc (1020000 + id, from 0x4000: 1078872 + id).",
                "Ajouter l'entrée {2} dans l'onglet « Cliloc » (texte ex. « {1} »). Le client affiche les noms via cliloc (1020000 + ID, à partir de 0x4000 : 1078872 + ID)."
            },
            ["cliloc.notloaded"] = new[]
            {
                "Cliloc konnte nicht geladen werden: {0}",
                "Cliloc could not be loaded: {0}",
                "Le cliloc n'a pas pu être chargé : {0}"
            },
            ["cliloc.notloaded.hint"] = new[]
            {
                "Prüfen, ob Cliloc.enu im Client-Ordner liegt und das Format (Optionen: neues Cliloc-Format) stimmt.",
                "Check that Cliloc.enu is in the client folder and that the format (options: new cliloc format) is right.",
                "Vérifier que Cliloc.enu est dans le dossier du client et que le format (options : nouveau format cliloc) est correct."
            }
        };
    }
}
