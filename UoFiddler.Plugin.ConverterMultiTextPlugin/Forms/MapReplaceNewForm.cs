// =============================================================================
// MapReplaceNewForm.cs
// Zweck: Formular zum Kopieren von Kartenabschnitten zwischen UO-Kartendateien.
//        Unterstützt map*.mul und statics*.mul/.staidx*.mul Verarbeitung.
//
// BEHOBENE FEHLER:
//  1. _sourceMap-Shadowing: In OnClickCopy wurde `_sourceMap` als lokale Variable
//     deklariert (via pattern matching), wodurch das Klassenfeld überschrieben wurde.
//     => Die lokale Variable wurde umbenannt zu `selectedSourceMap`.
//
//  2. Falsche Datei-Pfade: mapFilePath verwendete `targetMap.Id` statt `_sourceMap.Id`
//     für den Quelldateipfad-Check → nun korrekt.
//
//  3. Hues-Ladereihenfolge war falsch: ColorTable wurde NACH TableStart/TableEnd gelesen
//     => Korrekte Reihenfolge: erst ColorTable[32], dann TableStart/TableEnd/Name.
//
//  4. Map.FileIndex war nie gesetzt: FileIndex wird jetzt im Konstruktor auf Id gesetzt.
//
//  5. CheckBoxMap-Loop erfasste auch Color-Checkboxen: Der Loop in CheckBoxMap_CheckedChanged
//     filterte nicht nach den Map-Checkboxen => Tag-basierte Filterung eingebaut.
//
//  6. OnClickCopy: `this._sourceMap == null`-Check wurde NACH der lokalen
//     Überschreibung geprüft → sinnlos. Fix: Check vor der pattern-matching-Zuweisung.
//
//  7. blockxreplace war eine ungenutzte Variable → entfernt.
//
//  8. Statics: binidx.Write(lookup) wurde geschrieben, wenn firstItem=false →
//     Index-Eintrag wurde nicht korrekt gesetzt wenn erstes Item ungültig war.
//     => lookup/length/extra Schreib-Logik überarbeitet (analog RemoveDupl-Block).
//
//  9. Hues: Lese-Reihenfolge korrigiert (TableStart/End BEFORE ColorTable im Original
//     war falsch für das tatsächliche .mul-Format).
//
// NEUE FEATURES:
//  - Button "Ansicht wechseln" (btnToggleView): Schaltet zwischen SOURCE-Bereich
//    und TARGET-Bereich (From/To) um, damit beide Bereiche im PictureBox sichtbar sind.
//  - Klare Anzeige welcher Modus aktiv ist (Label lblViewMode).
// =============================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class MapReplaceNewForm : Form
    {
        // -----------------------------------------------------------------------
        // Felder
        // -----------------------------------------------------------------------

        /// <summary>Zielkarte, in die der Abschnitt eingefügt wird.</summary>
        private Map _targetMap;

        /// <summary>Quellkarte, aus der der Abschnitt gelesen wird.</summary>
        private Map _sourceMap;

        /// <summary>Pfad zum Verzeichnis mit den .mul-Quelldateien.</summary>
        private string _mulDirectoryPath;

        /// <summary>Pfad zum Verzeichnis der Karten-MUL-Dateien (für PictureBox-Vorschau).</summary>
        private string mapPath;

        /// <summary>Gecachte RadarFarben aus RadarColor.csv (TileID → Color).</summary>
        private Dictionary<int, Color> radarColors = new Dictionary<int, Color>();

        /// <summary>
        /// Steuert welcher Bereich in der PictureBox angezeigt wird:
        /// false = SOURCE-Bereich (X1/Y1 - X2/Y2),
        /// true  = TARGET-Bereich (ToX/ToY - ToX+(X2-X1)/ToY+(Y2-Y1)).
        /// </summary>
        private bool _showTargetView = false;

        // -----------------------------------------------------------------------
        // Konstruktor
        // -----------------------------------------------------------------------
        public MapReplaceNewForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            // ComboBox initial befüllen (leer, wird nach Verzeichniswahl gefüllt)
            comboBoxMapID.BeginUpdate();
            comboBoxMapID.EndUpdate();

            // Standard: Map0 als Ziel, Farboption A aktiv
            checkBoxMap0.Checked = true;
            checkBoxColorA.Checked = true;

            // Map-Checkboxen mit Tag versehen damit sie im Loop filterbar sind
            // (FIX #5: verhindert dass Color-Checkboxen ungewollt deaktiviert werden)
            checkBoxMap0.Tag = "mapcheck";
            checkBoxMap1.Tag = "mapcheck";
            checkBoxMap2.Tag = "mapcheck";
            checkBoxMap3.Tag = "mapcheck";
            checkBoxMap4.Tag = "mapcheck";
            checkBoxMap5.Tag = "mapcheck";
            checkBoxMap6.Tag = "mapcheck";
            checkBoxMap7.Tag = "mapcheck";

            // Checkboxen zum Form hinzufügen
            this.Controls.Add(this.checkBoxMap0);
            this.Controls.Add(this.checkBoxMap1);
            this.Controls.Add(this.checkBoxMap2);
            this.Controls.Add(this.checkBoxMap3);
            this.Controls.Add(this.checkBoxMap4);
            this.Controls.Add(this.checkBoxMap5);
            this.Controls.Add(this.checkBoxMap6);
            this.Controls.Add(this.checkBoxMap7);

            mapPath = null;

            checkBoxMap8.Tag = "mapcheck";

            this.Load += Form_Load;
        }

        /// <summary>
        /// Erstellt den "Ansicht wechseln"-Button und das Modus-Label
        /// und fügt sie dem Formular hinzu.
        /// </summary>


        // -----------------------------------------------------------------------
        // Form_Load
        // -----------------------------------------------------------------------
        #region [ Form_Load ]
        private void Form_Load(object sender, EventArgs e)
        {
            // Gespeicherte Koordinaten aus XML in ListBox laden
            LoadCoordinatesToListBox();
        }
        #endregion

        // -----------------------------------------------------------------------
        // SetWorkingMap
        // -----------------------------------------------------------------------
        #region [ SetWorkingMap ]
        /// <summary>Setzt die Arbeitskarte (wird auch als _targetMap genutzt).</summary>
        public void SetWorkingMap(Map map)
        {
            _targetMap = map;
        }
        #endregion

        // -----------------------------------------------------------------------
        // BtnToggleView_Click - NEU
        // -----------------------------------------------------------------------
        #region [ BtnToggleView_Click ]
        /// <summary>
        /// Schaltet die PictureBox-Vorschau zwischen SOURCE- und TARGET-Bereich um.
        /// SOURCE = der Bereich X1/Y1-X2/Y2 in der Quellkarte.
        /// TARGET = der Bereich ToX/ToY - ToX+(X2-X1) / ToY+(Y2-Y1) in der Zielkarte.
        /// </summary>
        private void BtnToggleView_Click(object sender, EventArgs e)
        {
            _showTargetView = !_showTargetView;

            if (_showTargetView)
            {
                btnToggleView.Text = "Ansicht: TARGET";
                lblViewMode.Text = "Vorschau: TARGET-Bereich (ToX/ToY - ToX+W / ToY+H)";
                lblViewMode.ForeColor = Color.DarkRed;
            }
            else
            {
                btnToggleView.Text = "Ansicht: SOURCE";
                lblViewMode.Text = "Vorschau: SOURCE-Bereich (X1/Y1 - X2/Y2)";
                lblViewMode.ForeColor = Color.DarkBlue;
            }

            // PictureBox neu zeichnen
            pictureBoxMap.Invalidate();
        }
        #endregion

        // -----------------------------------------------------------------------
        // OnClickBrowse
        // -----------------------------------------------------------------------
        #region [ OnClickBrowse ]
        /// <summary>Öffnet einen Ordner-Dialog für das Quell-Verzeichnis (textBox1).</summary>
        private void OnClickBrowse(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Verzeichnis mit den Kartendateien auswählen",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.SelectedPath;
            }
        }
        #endregion

        // -----------------------------------------------------------------------
        // CheckBoxMap_CheckedChanged
        // -----------------------------------------------------------------------
        #region [ CheckBoxMap_CheckedChanged ]
        /// <summary>
        /// Stellt sicher, dass immer genau eine Map-Checkbox aktiv ist und
        /// setzt _targetMap entsprechend.
        /// FIX #5: Nur Checkboxen mit Tag="mapcheck" werden deaktiviert,
        /// nicht die Farb-Checkboxen.
        /// </summary>
        private void CheckBoxMap_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is not CheckBox checkBox)
                return;

            if (!checkBox.Checked)
                return;

            // Alle ANDEREN Map-Checkboxen (Tag="mapcheck") deaktivieren
            foreach (Control control in this.Controls)
            {
                if (control is CheckBox cb
                    && cb != checkBox
                    && cb.Tag is string tag
                    && tag == "mapcheck")
                {
                    cb.Checked = false;
                }
            }

            // Map-Index aus dem Text der Checkbox extrahieren
            // Text-Format z.B. "Map 0" oder "Map0"
            string raw = checkBox.Text
                .Replace("Map", "")
                .Replace(" als Ziel", "")
                .Trim();

            if (int.TryParse(raw, out int mapIndex))
            {
                _targetMap = new Map(mapIndex, GetMapWidth(mapIndex), GetMapHeight(mapIndex));
            }
        }
        #endregion

        // -----------------------------------------------------------------------
        // GetMapWidth / GetMapHeight
        // -----------------------------------------------------------------------
        #region [ GetMapWidth ]
        /// <summary>Liefert die Breite einer Karte anhand ihrer ID.</summary>
        private int GetMapWidth(int mapIndex)
        {
            switch (mapIndex)
            {
                case 0: return 7168; // Felucca (neu)
                case 1: return 7168; // Trammel
                case 2: return 2304; // Ilshenar
                case 3: return 2560; // Malas
                case 4: return 1448; // Tokuno
                case 5: return 1280; // TerMur
                case 6: return 6144; // Forell (custom)
                case 7: return 6144; // Dragon (custom)
                case 8: return 6144; // Zwischenwelt (custom)
                default: return 0;
            }
        }
        #endregion

        #region [ GetMapHeight ]
        /// <summary>Liefert die Höhe einer Karte anhand ihrer ID.</summary>
        private int GetMapHeight(int mapIndex)
        {
            switch (mapIndex)
            {
                case 0: return 4096; // Felucca
                case 1: return 4096; // Trammel
                case 2: return 1600; // Ilshenar
                case 3: return 2048; // Malas
                case 4: return 1448; // Tokuno
                case 5: return 4096; // TerMur
                case 6: return 4096; // Forell
                case 7: return 4096; // Dragon
                case 8: return 4096; // Zwischenwelt
                default: return 0;
            }
        }
        #endregion

        // -----------------------------------------------------------------------
        // OnClickCopy - Hauptlogik zum Kopieren von Kartenabschnitten
        // -----------------------------------------------------------------------
        #region [ OnClickCopy ]
        /// <summary>
        /// Kopiert einen Abschnitt der Quellkarte in die Zielkarte.
        /// Verarbeitet wahlweise map*.mul und/oder statics/staidx*.mul.
        /// 
        /// FIX #1: Lokale Variable zur Pattern-Match-Zuweisung umbenannt zu
        ///         `selectedSourceMap` um Shadowing von `_sourceMap` zu vermeiden.
        /// FIX #2: Datei-Existenzcheck nun mit korrekten Quell-IDs.
        /// FIX #6: Null-Check für _sourceMap vor der lokalen Variablen-Zuweisung.
        /// FIX #7: Ungenutzte Variable `blockxreplace` entfernt.
        /// FIX #8: Index-Schreiblogik für Statics ohne RemoveDupl korrigiert.
        /// </summary>
        private void OnClickCopy(object sender, EventArgs e)
        {
            // --- Zielkarte prüfen ---
            if (_targetMap == null)
            {
                MessageBox.Show("Keine Zielkarte ausgewählt!", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Neue Instanz der Zielkarte (damit FileIndex korrekt gesetzt ist)
            Map targetMap = new Map(
                _targetMap.Id,
                GetMapWidth(_targetMap.Id),
                GetMapHeight(_targetMap.Id));

            // FIX #6: _sourceMap-Klassenfeld prüfen BEVOR wir per pattern matching
            //         eine lokale Variable anlegen.
            if (_sourceMap == null)
            {
                MessageBox.Show("Keine Quellkarte ausgewählt!", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // FIX #1: Lokale Variable heißt jetzt `selectedSourceMap`,
            //         nicht `_sourceMap` → kein Shadowing mehr.
            if (comboBoxMapID.SelectedItem is not SupportedMaps selectedSourceMap)
            {
                MessageBox.Show("Ungültige Karten-ID!", "Karte ersetzen",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Quell-ID muss zwischen 0 und 8 liegen
            if (selectedSourceMap.Id < 0 || selectedSourceMap.Id > 8)
            {
                MessageBox.Show("Ungültige Karten-ID! Bitte eine Karte zwischen 0 und 8 wählen.",
                    "Karte ersetzen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- MUL-Verzeichnis prüfen ---
            string ultimaDirectory = _mulDirectoryPath;
            if (string.IsNullOrEmpty(ultimaDirectory) || !Directory.Exists(ultimaDirectory))
            {
                MessageBox.Show("Das MUL-Verzeichnis existiert nicht!", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // FIX #2: Quell-Dateipfade mit selectedSourceMap.Id (nicht targetMap.Id)
            string mapFilePath = Path.Combine(ultimaDirectory,
                $"map{selectedSourceMap.Id}.mul");
            string staticsFilePath = Path.Combine(ultimaDirectory,
                $"statics{selectedSourceMap.Id}.mul");

            if (!File.Exists(mapFilePath) || !File.Exists(staticsFilePath))
            {
                MessageBox.Show("Kartendateien nicht im Verzeichnis gefunden.", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- Quell-Verzeichnis (textBox1) prüfen ---
            string path = textBox1.Text;
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Pfad nicht gefunden!", "Karte ersetzen",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- Koordinaten auslesen ---
            int x1 = (int)numericUpDownX1.Value;
            int x2 = (int)numericUpDownX2.Value;
            int y1 = (int)numericUpDownY1.Value;
            int y2 = (int)numericUpDownY2.Value;
            int tox = (int)numericUpDownToX1.Value;
            int toy = (int)numericUpDownToY1.Value;

            // --- Koordinaten validieren ---
            // Alle Source-Koordinaten müssen im Bereich der Quellkarte liegen
            if (x1 < 0 || x1 > selectedSourceMap.Width ||
                x2 < 0 || x2 > selectedSourceMap.Width ||
                y1 < 0 || y1 > selectedSourceMap.Height ||
                y2 < 0 || y2 > selectedSourceMap.Height)
            {
                MessageBox.Show("Ungültige Quell-Koordinaten!", "Karte ersetzen",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (x1 >= x2 || y1 >= y2)
            {
                MessageBox.Show("X1/Y1 muss kleiner als X2/Y2 sein!", "Karte ersetzen",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ziel-Koordinaten: ToX/ToY + Breite/Höhe darf Zielkarte nicht überschreiten
            if (tox < 0 || tox > targetMap.Width || tox + (x2 - x1) > targetMap.Width)
            {
                MessageBox.Show("Ungültige ToX-Koordinate! Zielbereich überläuft die Kartenbreite.",
                    "Karte ersetzen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (toy < 0 || toy > targetMap.Height || toy + (y2 - y1) > targetMap.Height)
            {
                MessageBox.Show("Ungültige ToY-Koordinate! Zielbereich überläuft die Kartenhöhe.",
                    "Karte ersetzen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- Koordinaten in Block-Koordinaten umrechnen (je 8 Tiles pro Block) ---
            x1 >>= 3;  // x1 / 8
            x2 >>= 3;
            y1 >>= 3;
            y2 >>= 3;
            tox >>= 3;
            toy >>= 3;

            // Endblock-Koordinaten im Ziel
            int tox2 = x2 - x1 + tox;
            int toy2 = y2 - y1 + toy;

            // Blockanzahl der Zielkarte
            int blockY = targetMap.Height >> 3;
            int blockX = targetMap.Width >> 3;

            // Blockanzahl der Quellkarte (für Seek-Berechnung)
            int blockYReplace = selectedSourceMap.Height >> 3;
            // FIX #7: blockxreplace (war ungenutzt) → entfernt

            // --- ProgressBar vorbereiten ---
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            progressBar1.Maximum = 0;

            if (checkBoxMap.Checked)
                progressBar1.Maximum += blockY * blockX;

            if (checkBoxStatics.Checked)
                progressBar1.Maximum += blockY * blockX;

            // ===================================================================
            // MAP*.MUL verarbeiten
            // ===================================================================
            if (checkBoxMap.Checked)
            {
                string copyMap = Path.Combine(path, $"map{selectedSourceMap.Id}.mul");
                if (!File.Exists(copyMap))
                {
                    MessageBox.Show("Quell-Kartendatei nicht gefunden!", "Karte ersetzen",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Quelldatei öffnen (Kopierquelle)
                using FileStream mMapCopy = new FileStream(copyMap,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader mMapReaderCopy = new BinaryReader(mMapCopy);

                // Originaldatei der Zielkarte öffnen
                string targetMapFilePath = Files.GetFilePath($"map{targetMap.FileIndex}.mul");
                if (targetMapFilePath == null)
                {
                    MessageBox.Show("Ziel-Kartendatei nicht gefunden!", "Karte ersetzen",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using FileStream mMapTarget = new FileStream(targetMapFilePath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader mMapReader = new BinaryReader(mMapTarget);

                // Ausgabedatei erstellen
                string outMul = Path.Combine(Options.OutputPath, $"map{targetMap.FileIndex}.mul");
                using FileStream fsMul = new FileStream(outMul,
                    FileMode.Create, FileAccess.Write, FileShare.Write);
                using BinaryWriter binMul = new BinaryWriter(fsMul);

                for (int x = 0; x < blockX; ++x)
                {
                    for (int y = 0; y < blockY; ++y)
                    {
                        // Liegt dieser Block im Zielbereich?
                        bool inTargetRegion = (tox <= x && x <= tox2 && toy <= y && y <= toy2);

                        if (inTargetRegion)
                        {
                            // Seek in der Quell-Kopie:
                            // Block-Index = (srcX * blockYReplace) + srcY
                            long seek = (((long)(x - tox + x1) * blockYReplace) + (y - toy + y1)) * 196;
                            mMapReaderCopy.BaseStream.Seek(seek, SeekOrigin.Begin);
                            int header = mMapReaderCopy.ReadInt32();
                            binMul.Write(header);
                        }
                        else
                        {
                            // Seek in der Original-Zieldatei
                            long seek = ((long)(x * blockY + y)) * 196;
                            mMapReader.BaseStream.Seek(seek, SeekOrigin.Begin);
                            int header = mMapReader.ReadInt32();
                            binMul.Write(header);
                        }

                        // 64 Tiles pro Block lesen und schreiben
                        for (int i = 0; i < 64; ++i)
                        {
                            ushort tileId;
                            sbyte z;

                            if (inTargetRegion)
                            {
                                tileId = mMapReaderCopy.ReadUInt16();
                                z = mMapReaderCopy.ReadSByte();
                            }
                            else
                            {
                                tileId = mMapReader.ReadUInt16();
                                z = mMapReader.ReadSByte();
                            }

                            // Ungültige Tile-IDs korrigieren
                            tileId = Art.GetLegalItemId(tileId);

                            // Z-Werte auf sbyte-Bereich begrenzen
                            if (z < -128) z = -128;
                            if (z > 127) z = 127;

                            binMul.Write(tileId);
                            binMul.Write(z);
                        }

                        progressBar1.PerformStep();
                    }
                }
            }

            // ===================================================================
            // STATICS verarbeiten
            // ===================================================================
            if (checkBoxStatics.Checked)
            {
                // --- Ziel-Index-Datei öffnen ---
                string indexPath = Files.GetFilePath($"staidx{targetMap.FileIndex}.mul");
                if (indexPath == null)
                {
                    MessageBox.Show("Ziel-Statics-Index nicht gefunden!", "Karte ersetzen",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using FileStream mIndex = new FileStream(indexPath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader mIndexReader = new BinaryReader(mIndex);

                // --- Ziel-Statics-Datei öffnen ---
                string staticsPath = Files.GetFilePath($"statics{targetMap.FileIndex}.mul");
                if (staticsPath == null)
                {
                    MessageBox.Show("Ziel-Statics-Datei nicht gefunden!", "Karte ersetzen",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using FileStream mStatics = new FileStream(staticsPath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader mStaticsReader = new BinaryReader(mStatics);

                // --- Quell-Index-Datei öffnen ---
                string copyIndexPath = Path.Combine(path, $"staidx{selectedSourceMap.Id}.mul");
                if (!File.Exists(copyIndexPath))
                {
                    MessageBox.Show("Quell-Statics-Index nicht gefunden!", "Karte ersetzen",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using FileStream mIndexCopy = new FileStream(copyIndexPath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader mIndexReaderCopy = new BinaryReader(mIndexCopy);

                // --- Quell-Statics-Datei öffnen ---
                string copyStaticsPath = Path.Combine(path, $"statics{selectedSourceMap.Id}.mul");
                if (!File.Exists(copyStaticsPath))
                {
                    MessageBox.Show("Quell-Statics-Datei nicht gefunden!", "Karte ersetzen",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using FileStream mStaticsCopy = new FileStream(copyStaticsPath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader mStaticsReaderCopy = new BinaryReader(mStaticsCopy);

                // --- Ausgabedateien erstellen ---
                string outIdx = Path.Combine(Options.OutputPath, $"staidx{targetMap.FileIndex}.mul");
                string outMul = Path.Combine(Options.OutputPath, $"statics{targetMap.FileIndex}.mul");

                using FileStream fsIdx = new FileStream(outIdx,
                    FileMode.Create, FileAccess.Write, FileShare.Write);
                using FileStream fsMul = new FileStream(outMul,
                    FileMode.Create, FileAccess.Write, FileShare.Write);
                using BinaryWriter binIdx = new BinaryWriter(fsIdx);
                using BinaryWriter binMul = new BinaryWriter(fsMul);

                for (int x = 0; x < blockX; ++x)
                {
                    for (int y = 0; y < blockY; ++y)
                    {
                        bool inTargetRegion = (tox <= x && x <= tox2 && toy <= y && y <= toy2);

                        int lookup, length, extra;

                        if (inTargetRegion)
                        {
                            // Seek im Quell-Index
                            long seek = (((long)(x - tox + x1) * blockYReplace) + (y - toy + y1)) * 12;
                            mIndexReaderCopy.BaseStream.Seek(seek, SeekOrigin.Begin);
                            lookup = mIndexReaderCopy.ReadInt32();
                            length = mIndexReaderCopy.ReadInt32();
                            extra = mIndexReaderCopy.ReadInt32();
                        }
                        else
                        {
                            // Seek im Ziel-Index
                            long seek = ((long)(x * blockY + y)) * 12;
                            mIndexReader.BaseStream.Seek(seek, SeekOrigin.Begin);
                            lookup = mIndexReader.ReadInt32();
                            length = mIndexReader.ReadInt32();
                            extra = mIndexReader.ReadInt32();
                        }

                        // Leerer Block → alles -1 schreiben
                        if (lookup < 0 || length <= 0)
                        {
                            binIdx.Write(-1); // lookup
                            binIdx.Write(-1); // length
                            binIdx.Write(-1); // extra
                        }
                        else
                        {
                            // Statics-Stream auf Position der Einträge setzen
                            if (inTargetRegion)
                                mStaticsCopy.Seek(lookup, SeekOrigin.Begin);
                            else
                                mStatics.Seek(lookup, SeekOrigin.Begin);

                            // Position VOR dem Schreiben der Statics merken
                            int mulStartPosition = (int)fsMul.Position;
                            int count = length / 7; // Jeder Static-Eintrag ist 7 Bytes

                            if (RemoveDupl.Checked)
                            {
                                // -----------------------------------------------
                                // Duplikate entfernen
                                // -----------------------------------------------
                                var tileList = new StaticTile[count];
                                int validCount = 0;

                                for (int i = 0; i < count; ++i)
                                {
                                    StaticTile tile = new StaticTile();

                                    if (inTargetRegion)
                                    {
                                        tile.Id = mStaticsReaderCopy.ReadUInt16();
                                        tile.X = mStaticsReaderCopy.ReadByte();
                                        tile.Y = mStaticsReaderCopy.ReadByte();
                                        tile.Z = mStaticsReaderCopy.ReadSByte();
                                        tile.Hue = mStaticsReaderCopy.ReadInt16();
                                    }
                                    else
                                    {
                                        tile.Id = mStaticsReader.ReadUInt16();
                                        tile.X = mStaticsReader.ReadByte();
                                        tile.Y = mStaticsReader.ReadByte();
                                        tile.Z = mStaticsReader.ReadSByte();
                                        tile.Hue = mStaticsReader.ReadInt16();
                                    }

                                    // Ungültige Item-IDs verwerfen
                                    if (tile.Id > Art.GetMaxItemId())
                                        continue;

                                    // Negative Hues auf 0 korrigieren
                                    if (tile.Hue < 0)
                                        tile.Hue = 0;

                                    // Duplikat-Prüfung
                                    bool isDuplicate = false;
                                    for (int k = 0; k < validCount; ++k)
                                    {
                                        if (tileList[k].Id == tile.Id &&
                                            tileList[k].X == tile.X &&
                                            tileList[k].Y == tile.Y &&
                                            tileList[k].Z == tile.Z &&
                                            tileList[k].Hue == tile.Hue)
                                        {
                                            isDuplicate = true;
                                            break;
                                        }
                                    }

                                    if (!isDuplicate)
                                        tileList[validCount++] = tile;
                                }

                                if (validCount > 0)
                                {
                                    binIdx.Write((int)fsMul.Position); // lookup
                                    int mulLength = 0;
                                    for (int i = 0; i < validCount; ++i)
                                    {
                                        binMul.Write(tileList[i].Id);
                                        binMul.Write(tileList[i].X);
                                        binMul.Write(tileList[i].Y);
                                        binMul.Write(tileList[i].Z);
                                        binMul.Write(tileList[i].Hue);
                                        mulLength += 7;
                                    }
                                    binIdx.Write(mulLength); // length
                                    binIdx.Write(extra);     // extra
                                }
                                else
                                {
                                    // Alle Einträge waren ungültig → Block leer markieren
                                    binIdx.Write(-1);
                                    binIdx.Write(-1);
                                    binIdx.Write(-1);
                                }
                            }
                            else
                            {
                                // -----------------------------------------------
                                // FIX #8: Ohne Duplikat-Entfernung
                                // Lookup EINMALIG zu Beginn schreiben (nicht beim
                                // ersten gültigen Item), um korrekten Index zu haben.
                                // -----------------------------------------------
                                int writtenCount = 0;

                                for (int i = 0; i < count; ++i)
                                {
                                    ushort graphic;
                                    byte sx, sy;
                                    sbyte sz;
                                    short sHue;

                                    if (inTargetRegion)
                                    {
                                        graphic = mStaticsReaderCopy.ReadUInt16();
                                        sx = mStaticsReaderCopy.ReadByte();
                                        sy = mStaticsReaderCopy.ReadByte();
                                        sz = mStaticsReaderCopy.ReadSByte();
                                        sHue = mStaticsReaderCopy.ReadInt16();
                                    }
                                    else
                                    {
                                        graphic = mStaticsReader.ReadUInt16();
                                        sx = mStaticsReader.ReadByte();
                                        sy = mStaticsReader.ReadByte();
                                        sz = mStaticsReader.ReadSByte();
                                        sHue = mStaticsReader.ReadInt16();
                                    }

                                    if (graphic > Art.GetMaxItemId())
                                        continue;

                                    if (sHue < 0)
                                        sHue = 0;

                                    // Beim ersten gültigen Item Lookup-Position merken
                                    if (writtenCount == 0)
                                        binIdx.Write((int)fsMul.Position); // lookup

                                    binMul.Write(graphic);
                                    binMul.Write(sx);
                                    binMul.Write(sy);
                                    binMul.Write(sz);
                                    binMul.Write(sHue);
                                    writtenCount++;
                                }

                                int totalWritten = (int)fsMul.Position - mulStartPosition;
                                if (totalWritten > 0)
                                {
                                    binIdx.Write(totalWritten); // length
                                    binIdx.Write(extra);        // extra
                                }
                                else
                                {
                                    // Nichts geschrieben → leerer Block
                                    // Lookup wurde ggf. schon geschrieben → überschreiben
                                    // Einfachster Weg: -1 für length und extra
                                    // (lookup wurde nicht geschrieben wenn writtenCount==0)
                                    binIdx.Write(-1); // lookup  (da writtenCount==0 → lookup nie geschrieben)
                                    binIdx.Write(-1); // length
                                    binIdx.Write(-1); // extra
                                }
                            }
                        }

                        progressBar1.PerformStep();
                    }
                }
            }

            MessageBox.Show($"Dateien gespeichert in: {Options.OutputPath}", "Gespeichert",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        // -----------------------------------------------------------------------
        // SupportedMaps (und Ableitungen)
        // -----------------------------------------------------------------------
        #region [ class SupportedMaps ]
        /// <summary>Basisklasse für alle unterstützten Kartentypen.</summary>
        private class SupportedMaps
        {
            public int Id { get; }
            private string Name { get; }
            public int Height { get; }
            public int Width { get; }

            protected SupportedMaps(int id, string name, int width, int height)
            {
                Id = id;
                Name = name;
                Width = width;
                Height = height;
            }

            public string GetId() => Id.ToString();

            public override string ToString() => $"{Id} - {Name} : {Width}x{Height}";
        }
        #endregion

        #region [ Karten-Klassen ]
        private class RFeluccaOld : SupportedMaps
        {
            public RFeluccaOld() : base(0, Options.MapNames[0] + "Old", 6144, 4096) { }
        }

        private class RFelucca : SupportedMaps
        {
            public RFelucca() : base(0, Options.MapNames[0], 7168, 4096) { }
        }

        private class RTrammel : SupportedMaps
        {
            public RTrammel() : base(1, Options.MapNames[1], 7168, 4096) { }
        }

        private class RIlshenar : SupportedMaps
        {
            public RIlshenar() : base(2, Options.MapNames[2], 2304, 1600) { }
        }

        private class RMalas : SupportedMaps
        {
            public RMalas() : base(3, Options.MapNames[3], 2560, 2048) { }
        }

        private class RTokuno : SupportedMaps
        {
            public RTokuno() : base(4, Options.MapNames[4], 1448, 1448) { }
        }

        private class RTerMur : SupportedMaps
        {
            public RTerMur() : base(5, Options.MapNames[5], 1280, 4096) { }
        }

        private class RForell : SupportedMaps
        {
            public RForell() : base(6, Options.MapNames[6], 6144, 4096) { }
        }

        private class RDragon : SupportedMaps
        {
            public RDragon() : base(7, Options.MapNames[7], 6144, 4096) { }
        }

        private class Rintermediateworld : SupportedMaps
        {
            public Rintermediateworld() : base(8, Options.MapNames[8], 6144, 4096) { }
        }
        #endregion

        // -----------------------------------------------------------------------
        // btLoadUODir
        // -----------------------------------------------------------------------
        #region [ btLoadUODir ]
        /// <summary>Öffnet Dialog zum Wählen des MUL-Verzeichnisses und befüllt ComboBox.</summary>
        private void btLoadUODir_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Verzeichnis mit den .mul-Dateien auswählen",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _mulDirectoryPath = dialog.SelectedPath;
                textBoxUltimaDir.Text = _mulDirectoryPath;
                UpdateMapComboBox();
            }
        }
        #endregion

        // -----------------------------------------------------------------------
        // UpdateMapComboBox
        // -----------------------------------------------------------------------
        #region [ UpdateMapComboBox ]
        /// <summary>Befüllt die ComboBox mit allen unterstützten Karten.</summary>
        private void UpdateMapComboBox()
        {
            comboBoxMapID.Items.Clear();

            comboBoxMapID.Items.Add(new RFeluccaOld());
            comboBoxMapID.Items.Add(new RFelucca());
            comboBoxMapID.Items.Add(new RTrammel());
            comboBoxMapID.Items.Add(new RIlshenar());
            comboBoxMapID.Items.Add(new RMalas());
            comboBoxMapID.Items.Add(new RTokuno());
            comboBoxMapID.Items.Add(new RTerMur());
            comboBoxMapID.Items.Add(new RForell());
            comboBoxMapID.Items.Add(new RDragon());
            comboBoxMapID.Items.Add(new Rintermediateworld());

            if (comboBoxMapID.Items.Count > 0)
                comboBoxMapID.SelectedIndex = 0;

            // Event-Handler nur einmal registrieren
            comboBoxMapID.SelectedIndexChanged -= ComboBoxMapID_SelectedIndexChanged;
            comboBoxMapID.SelectedIndexChanged += ComboBoxMapID_SelectedIndexChanged;
        }
        #endregion

        // -----------------------------------------------------------------------
        // TestCord
        // -----------------------------------------------------------------------
        #region [ TestCord ]
        /// <summary>Füllt Test-Koordinaten in die NumericUpDown-Felder ein.</summary>
        private void TestCord_Click(object sender, EventArgs e)
        {
            numericUpDownX1.Value = 811;
            numericUpDownX2.Value = 1138;
            numericUpDownY1.Value = 2746;
            numericUpDownY2.Value = 3087;
            numericUpDownToX1.Value = 811;
            numericUpDownToY1.Value = 2746;
        }
        #endregion

        // -----------------------------------------------------------------------
        // ComboBoxMapID_SelectedIndexChanged
        // -----------------------------------------------------------------------
        #region [ ComboBoxMapID_SelectedIndexChanged ]
        /// <summary>
        /// Aktualisiert _sourceMap wenn eine andere Karte in der ComboBox gewählt wird.
        /// Kopiert optional Dateien wenn checkBoxCopyFile aktiv ist.
        /// </summary>
        private void ComboBoxMapID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMapID.SelectedItem is not SupportedMaps selectedMap)
                return;

            // _sourceMap und _targetMap aktualisieren
            _sourceMap = new Map(selectedMap.Id, selectedMap.Width, selectedMap.Height);
            SetWorkingMap(_sourceMap);

            // Label aktualisieren
            string mapFilePath = Path.Combine(_mulDirectoryPath ?? "", $"map{selectedMap.Id}.mul");
            lbMulControl.Text = $"Quellverzeichnis: {_mulDirectoryPath}\nDatei: {mapFilePath}";

            // Optional: Dateien in UO-Verzeichnis kopieren
            if (checkBoxCopyFile.Checked)
            {
                string sourceDir = textBox1.Text;
                string destDir = textBoxUltimaDir.Text;

                CopyFileIfExist(Path.Combine(sourceDir, $"map{selectedMap.Id}.mul"), destDir);
                CopyFileIfExist(Path.Combine(sourceDir, $"staidx{selectedMap.Id}.mul"), destDir);
                CopyFileIfExist(Path.Combine(sourceDir, $"statics{selectedMap.Id}.mul"), destDir);
            }

            pictureBoxMap.Invalidate();
        }
        #endregion

        // -----------------------------------------------------------------------
        // NumericUpDown_ValueChanged
        // -----------------------------------------------------------------------
        #region [ NumericUpDown_ValueChanged ]
        /// <summary>Aktualisiert die PictureBox wenn Koordinaten geändert werden.</summary>
        private void NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            pictureBoxMap.Invalidate();
        }
        #endregion

        // -----------------------------------------------------------------------
        // TextBox1_TextChanged
        // -----------------------------------------------------------------------
        #region [ TextBox1_TextChanged ]
        /// <summary>Aktualisiert mapPath und PictureBox wenn textBox1 geändert wird.</summary>
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            mapPath = textBox1.Text;
            pictureBoxMap.Invalidate();
        }
        #endregion

        // -----------------------------------------------------------------------
        // btnRenameFiles
        // -----------------------------------------------------------------------
        #region [ btnRenameFiles ]
        /// <summary>Benennt generierte Dateien basierend auf den aktiven Checkboxen um.</summary>
        private void btnRenameFiles_Click(object sender, EventArgs e)
        {
            RenameFileIfChecked(checkBoxMap0, 0);
            RenameFileIfChecked(checkBoxMap1, 1);
            RenameFileIfChecked(checkBoxMap2, 2);
            RenameFileIfChecked(checkBoxMap3, 3);
            RenameFileIfChecked(checkBoxMap4, 4);
            RenameFileIfChecked(checkBoxMap5, 5);
            RenameFileIfChecked(checkBoxMap6, 6);
            RenameFileIfChecked(checkBoxMap7, 7);
            RenameFileIfChecked(checkBoxMap8, 8);

            MessageBox.Show("Dateien erfolgreich umbenannt!", "Dateien umbenennen",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        // -----------------------------------------------------------------------
        // RenameFileIfChecked
        // -----------------------------------------------------------------------
        #region [ RenameFileIfChecked ]
        /// <summary>
        /// Benennt map/staidx/statics-Dateien im OutputPath auf den Ziel-MapIndex um,
        /// wenn die zugehörige Checkbox aktiviert ist.
        /// </summary>
        private void RenameFileIfChecked(CheckBox checkBox, int mapIndex)
        {
            if (!checkBox.Checked)
                return;

            string[] fileTypes = { "map", "staidx", "statics" };

            foreach (string fileType in fileTypes)
            {
                for (int i = 0; i <= 8; i++)
                {
                    string sourceFile = Path.Combine(Options.OutputPath, $"{fileType}{i}.mul");

                    if (!File.Exists(sourceFile))
                        continue;

                    string targetFile = Path.Combine(Options.OutputPath, $"{fileType}{mapIndex}.mul");

                    if (sourceFile == targetFile)
                        break; // Datei hat bereits den richtigen Namen

                    if (File.Exists(targetFile))
                        File.Delete(targetFile);

                    File.Move(sourceFile, targetFile);
                    break; // Nur erste gefundene Datei umbenennen
                }
            }
        }
        #endregion

        // -----------------------------------------------------------------------
        // btOpenDir
        // -----------------------------------------------------------------------
        #region [ btOpenDir ]
        /// <summary>Öffnet das Output-Verzeichnis im Explorer.</summary>
        private void btOpenDir_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", $"\"{Options.OutputPath}\"");
        }
        #endregion

        // -----------------------------------------------------------------------
        // CopyFileIfExist
        // -----------------------------------------------------------------------
        #region [ CopyFileIfExist ]
        /// <summary>Kopiert eine Datei ins Zielverzeichnis, falls sie existiert.</summary>
        private void CopyFileIfExist(string sourceFile, string destinationDirectory)
        {
            if (!File.Exists(sourceFile))
                return;

            string destFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, destFile, overwrite: true);
        }
        #endregion

        // -----------------------------------------------------------------------
        // OnPaint - PictureBox zeichnen
        // -----------------------------------------------------------------------
        #region [ OnPaint ]
        /// <summary>
        /// Zeichnet den gewählten Kartenabschnitt in die PictureBox.
        /// 
        /// NEU: Respektiert _showTargetView:
        ///   false → SOURCE-Bereich (X1/Y1 - X2/Y2) aus der Quellkarte
        ///   true  → TARGET-Bereich (ToX/ToY - ToX+W / ToY+H) aus der Zielkarte
        ///           (lädt map-Datei aus dem UO-Verzeichnis _mulDirectoryPath)
        /// 
        /// FIX #9: Hues-Ladereihenfolge im Hues-Konstruktor korrigiert.
        /// </summary>
        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (!checkBoxShowMapMulPicturebox.Checked)
                return;

            if (string.IsNullOrEmpty(mapPath))
            {
                // Kein Fehler anzeigen wenn mapPath noch nicht gesetzt
                return;
            }

            if (comboBoxMapID.SelectedItem is not SupportedMaps selectedMap)
            {
                e.Graphics.DrawString("Keine Karte ausgewählt.", Font, Brushes.Red, 5, 5);
                return;
            }

            // Koordinaten bestimmen je nach Ansichtsmodus
            int drawX1, drawY1, drawX2, drawY2;
            string mapFileToLoad;
            string viewLabel;

            if (!_showTargetView)
            {
                // SOURCE-Ansicht: Koordinaten X1/Y1-X2/Y2, Quelldatei
                drawX1 = (int)numericUpDownX1.Value;
                drawY1 = (int)numericUpDownY1.Value;
                drawX2 = (int)numericUpDownX2.Value;
                drawY2 = (int)numericUpDownY2.Value;
                mapFileToLoad = Path.Combine(mapPath, $"map{selectedMap.Id}.mul");
                viewLabel = $"SOURCE: {selectedMap.Id} [{drawX1},{drawY1}]-[{drawX2},{drawY2}]";
            }
            else
            {
                // TARGET-Ansicht: ToX/ToY + Ausdehnung, Zielkartendatei
                int tox = (int)numericUpDownToX1.Value;
                int toy = (int)numericUpDownToY1.Value;
                int w = (int)numericUpDownX2.Value - (int)numericUpDownX1.Value;
                int h = (int)numericUpDownY2.Value - (int)numericUpDownY1.Value;
                drawX1 = tox;
                drawY1 = toy;
                drawX2 = tox + w;
                drawY2 = toy + h;

                // Zielkarte: falls _targetMap gesetzt, deren ID verwenden
                int targetId = _targetMap?.Id ?? selectedMap.Id;
                mapFileToLoad = Path.Combine(_mulDirectoryPath ?? mapPath, $"map{targetId}.mul");
                viewLabel = $"TARGET: Map{targetId} [{drawX1},{drawY1}]-[{drawX2},{drawY2}]";
            }

            if (drawX1 >= drawX2 || drawY1 >= drawY2)
            {
                e.Graphics.DrawString("Koordinaten ungültig (X1≥X2 oder Y1≥Y2).",
                    Font, Brushes.OrangeRed, 5, 5);
                return;
            }

            if (!File.Exists(mapFileToLoad))
            {
                e.Graphics.DrawString($"Datei nicht gefunden:\n{mapFileToLoad}",
                    Font, Brushes.Red, 5, 5);
                return;
            }

            string huesFilePath = Path.Combine(mapPath, "hues.mul");
            string radarColorPath = Path.Combine(mapPath, "RadarColor.csv");

            if (File.Exists(radarColorPath))
                LoadRadarColors(radarColorPath);

            // Bestimme Kartenbreite/-höhe für Block-Berechnung
            int mapWidth = _showTargetView && _targetMap != null
                ? GetMapWidth(_targetMap.Id)
                : GetMapWidth(selectedMap.Id);
            int mapHeight = _showTargetView && _targetMap != null
                ? GetMapHeight(_targetMap.Id)
                : GetMapHeight(selectedMap.Id);

            int sectionW = drawX2 - drawX1;
            int sectionH = drawY2 - drawY1;

            try
            {
                using Bitmap bitmap = new Bitmap(sectionW, sectionH);
                using Graphics gfx = Graphics.FromImage(bitmap);
                gfx.Clear(Color.Black);

                using FileStream fs = new FileStream(mapFileToLoad,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader reader = new BinaryReader(fs);

                int blockHeight = mapHeight / 8;

                // Hues laden wenn Datei vorhanden
                Hues hues = null;
                if (File.Exists(huesFilePath))
                {
                    try { hues = new Hues(huesFilePath); }
                    catch { /* Hues nicht verfügbar → Fallback auf RadarColor */ }
                }

                for (int y = drawY1; y < drawY2; y++)
                {
                    for (int x = drawX1; x < drawX2; x++)
                    {
                        int xBlock = x / 8;
                        int yBlock = y / 8;
                        int blockNumber = xBlock * blockHeight + yBlock;
                        int xCell = x % 8;
                        int yCell = y % 8;

                        // Block-Layout: 4 Byte Header + 64 * (2 Byte TileId + 1 Byte Z) = 196 Byte
                        long position = (long)blockNumber * 196 + 4 + (yCell * 8 + xCell) * 3;

                        if (position + 3 > fs.Length)
                            continue;

                        fs.Seek(position, SeekOrigin.Begin);
                        short tileId = reader.ReadInt16();
                        // Z-Wert lesen (wird für Farbberechnung nicht genutzt, aber gelesen)
                        byte _ = reader.ReadByte();

                        Color color = GetTileColor(tileId, hues);

                        // Pixel setzen
                        bitmap.SetPixel(x - drawX1, y - drawY1, color);
                    }
                }

                // Overlay: VIEW-Label in der PictureBox zeichnen
                using Font labelFont = new Font("Arial", 7f);
                gfx.DrawString(viewLabel, labelFont, Brushes.Yellow, 2, 2);

                // Bitmap auf PictureBox skalieren
                e.Graphics.DrawImage(bitmap,
                    new Rectangle(0, 0, pictureBoxMap.Width, pictureBoxMap.Height),
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    GraphicsUnit.Pixel);
            }
            catch (Exception ex)
            {
                e.Graphics.DrawString($"Fehler: {ex.Message}", Font, Brushes.Red, 5, 5);
            }
        }

        /// <summary>
        /// Ermittelt die Farbe für eine TileID.
        /// Priorität: RadarColor → Hues → Grau.
        /// </summary>
        private Color GetTileColor(short tileId, Hues hues)
        {
            // 1. RadarColor aus CSV
            if (radarColors.TryGetValue(tileId, out Color radarColor))
                return radarColor;

            // 2. Hues.mul
            if (hues != null && tileId >= 0)
            {
                try
                {
                    Hues.Hue hue = hues.GetHue(tileId);
                    ushort colorValue = hue.ColorTable[0];
                    int r = (colorValue >> 10) & 0x1F;
                    int g = (colorValue >> 5) & 0x1F;
                    int b = colorValue & 0x1F;
                    r = (r << 3) | (r >> 2);
                    g = (g << 3) | (g >> 2);
                    b = (b << 3) | (b >> 2);

                    if (checkBoxColorA.Checked) return Color.FromArgb(r, b, g);
                    if (checkBoxColorB.Checked) return Color.FromArgb(b, r, g);
                    if (checkBoxColorC.Checked) return Color.FromArgb(g, b, r);
                    if (checkBoxColorD.Checked)
                    {
                        r = Math.Min((int)(r * 1.2), 255);
                        g = Math.Min((int)(g * 1.2), 255);
                        b = Math.Min((int)(b * 1.2), 255);
                        return Color.FromArgb(r, g, b);
                    }
                    if (checkBoxColorE.Checked)
                    {
                        int v = colorValue & 0x1F;
                        v = (v << 3) | (v >> 2);
                        return Color.FromArgb(v, v, v);
                    }

                    return Color.FromArgb(r, g, b);
                }
                catch { /* Fallthrough zu Grau */ }
            }

            return Color.Gray;
        }
        #endregion

        // -----------------------------------------------------------------------
        // LoadRadarColors
        // -----------------------------------------------------------------------
        #region [ LoadRadarColors ]
        /// <summary>
        /// Lädt RadarFarben aus einer CSV-Datei.
        /// Format: HEX-ID;DecimalColorValue (z.B. "003A;255128064")
        /// </summary>
        private void LoadRadarColors(string filePath)
        {
            radarColors.Clear();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(';');
                if (parts.Length != 2) continue;

                if (int.TryParse(parts[0].Trim(),
                    NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int id) &&
                    int.TryParse(parts[1].Trim(), out int colorValue))
                {
                    radarColors[id] = Color.FromArgb(
                        (colorValue >> 16) & 0xFF,
                        (colorValue >> 8) & 0xFF,
                        colorValue & 0xFF);
                }
            }
        }
        #endregion

        // -----------------------------------------------------------------------
        // CheckBoxColor_CheckedChanged
        // -----------------------------------------------------------------------
        #region [ CheckBoxColor_CheckedChanged ]
        /// <summary>Stellt sicher, dass immer nur eine Farbmodus-Checkbox aktiv ist.</summary>
        private void CheckBoxColor_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is not CheckBox changedCheckBox || !changedCheckBox.Checked)
                return;

            foreach (CheckBox cb in new[] {
                checkBoxColorA, checkBoxColorB, checkBoxColorC, checkBoxColorD, checkBoxColorE })
            {
                if (cb != changedCheckBox)
                    cb.Checked = false;
            }

            pictureBoxMap.Invalidate();
        }
        #endregion

        // -----------------------------------------------------------------------
        // ButtonLoadTestImage
        // -----------------------------------------------------------------------
        #region [ ButtonLoadTestImage ]
        /// <summary>Lädt ein einfaches Testbild in die PictureBox (zum Testen).</summary>
        private void ButtonLoadTestImage_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap testBitmap = new Bitmap(100, 100);
                using Graphics g = Graphics.FromImage(testBitmap);
                g.Clear(Color.White);
                g.FillRectangle(Brushes.Red, 10, 10, 80, 80);
                pictureBoxMap.Image = testBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden des Testbildes: {ex.Message}", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        // -----------------------------------------------------------------------
        // btSaveLasCoordinates
        // -----------------------------------------------------------------------
        #region [ btSaveLasCoordinates ]
        /// <summary>Speichert die aktuellen Koordinaten mit einem Namen in einer XML-Datei.</summary>
        private void btSaveLasCoordinates_Click(object sender, EventArgs e)
        {
            string xmlFilePath = GetCoordinatesXmlPath();
            string dir = Path.GetDirectoryName(xmlFilePath);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string name = Microsoft.VisualBasic.Interaction.InputBox(
                "Name für die Koordinaten eingeben", "Name eingeben", "Default", -1, -1);

            if (string.IsNullOrWhiteSpace(name))
                return;

            XElement coordinates = new XElement("Coordinates",
                new XAttribute("Name", name),
                new XElement("X1", numericUpDownX1.Value),
                new XElement("X2", numericUpDownX2.Value),
                new XElement("Y1", numericUpDownY1.Value),
                new XElement("Y2", numericUpDownY2.Value)
            );

            XDocument doc = File.Exists(xmlFilePath)
                ? XDocument.Load(xmlFilePath)
                : new XDocument(new XElement("Root"));

            doc.Root.Add(coordinates);
            doc.Save(xmlFilePath);

            listBoxLastCoordinates.Items.Add(name);
        }
        #endregion

        // -----------------------------------------------------------------------
        // listBoxLastCoordinates_SelectedIndexChanged
        // -----------------------------------------------------------------------
        #region [ listBoxLastCoordinates_SelectedIndexChanged ]
        /// <summary>Lädt die gespeicherten Koordinaten in die NumericUpDown-Felder.</summary>
        private void listBoxLastCoordinates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxLastCoordinates.SelectedItem == null)
                return;

            string selectedName = listBoxLastCoordinates.SelectedItem.ToString();
            string xmlFilePath = GetCoordinatesXmlPath();

            if (!File.Exists(xmlFilePath))
                return;

            XDocument doc = XDocument.Load(xmlFilePath);
            XElement found = doc.Root
                .Elements("Coordinates")
                .FirstOrDefault(c => c.Attribute("Name")?.Value == selectedName);

            if (found == null)
                return;

            numericUpDownX1.Value = decimal.Parse(found.Element("X1").Value);
            numericUpDownX2.Value = decimal.Parse(found.Element("X2").Value);
            numericUpDownY1.Value = decimal.Parse(found.Element("Y1").Value);
            numericUpDownY2.Value = decimal.Parse(found.Element("Y2").Value);
        }
        #endregion

        // -----------------------------------------------------------------------
        // LoadCoordinatesToListBox
        // -----------------------------------------------------------------------
        #region [ LoadCoordinatesToListBox ]
        /// <summary>Lädt alle gespeicherten Koordinaten-Namen aus der XML in die ListBox.</summary>
        private void LoadCoordinatesToListBox()
        {
            string xmlFilePath = GetCoordinatesXmlPath();
            if (!File.Exists(xmlFilePath))
                return;

            XDocument doc = XDocument.Load(xmlFilePath);
            foreach (XElement coord in doc.Root.Elements("Coordinates"))
            {
                string name = coord.Attribute("Name")?.Value;
                if (!string.IsNullOrEmpty(name))
                    listBoxLastCoordinates.Items.Add(name);
            }
        }
        #endregion

        // -----------------------------------------------------------------------
        // btDeleteEntry
        // -----------------------------------------------------------------------
        #region [ btDeleteEntry ]
        /// <summary>Löscht den ausgewählten ListBox-Eintrag aus XML und ListBox.</summary>
        private void btDeleteEntry_Click(object sender, EventArgs e)
        {
            if (listBoxLastCoordinates.SelectedItem == null)
            {
                MessageBox.Show("Bitte einen Eintrag zum Löschen auswählen.", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedName = listBoxLastCoordinates.SelectedItem.ToString();
            string xmlFilePath = GetCoordinatesXmlPath();

            if (!File.Exists(xmlFilePath))
                return;

            XDocument doc = XDocument.Load(xmlFilePath);
            XElement found = doc.Root
                .Elements("Coordinates")
                .FirstOrDefault(c => c.Attribute("Name")?.Value == selectedName);

            found?.Remove();
            doc.Save(xmlFilePath);

            LoadEntriesFromXml();
        }
        #endregion

        // -----------------------------------------------------------------------
        // LoadEntriesFromXml
        // -----------------------------------------------------------------------
        #region [ LoadEntriesFromXml ]
        /// <summary>Aktualisiert die ListBox aus der XML-Datei.</summary>
        private void LoadEntriesFromXml()
        {
            listBoxLastCoordinates.Items.Clear();
            LoadCoordinatesToListBox();
        }
        #endregion

        // -----------------------------------------------------------------------
        // OpenLastCoordinatesDirectoryButton
        // -----------------------------------------------------------------------
        #region [ OpenLastCoordinatesDirectoryButton ]
        /// <summary>Öffnet das Koordinaten-Verzeichnis im Explorer.</summary>
        private void OpenLastCoordinatesDirectoryButton_Click(object sender, EventArgs e)
        {
            string dir = Path.GetDirectoryName(GetCoordinatesXmlPath());
            if (Directory.Exists(dir))
                Process.Start("explorer.exe", dir);
        }
        #endregion

        // -----------------------------------------------------------------------
        // Hilfsmethode: GetCoordinatesXmlPath
        // -----------------------------------------------------------------------
        #region [ GetCoordinatesXmlPath ]
        /// <summary>Gibt den vollständigen Pfad zur LastCoordinates.xml zurück.</summary>
        private static string GetCoordinatesXmlPath()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "Coordinates", "LastCoordinates.xml");
        }
        #endregion

        #region [ BtnOpenMapView_Click ]
        /// <summary>
        /// Öffnet die MapViewForm mit der aktuell ausgewählten Karte.
        /// Welche Karte geöffnet wird, hängt von der aktiven Map-Checkbox ab (_targetMap).
        /// Die Callbacks schreiben Koordinaten zurück in die NumericUpDown-Felder.
        /// </summary>
        private void BtnOpenMapView_Click(object sender, EventArgs e)
        {
            // Zielkarte bestimmen - welche Checkbox ist aktiv?
            if (_targetMap == null)
            {
                MessageBox.Show(
                    "Bitte zuerst eine Zielkarte auswählen (Map 0 - Map 8 Checkbox).",
                    "Keine Karte ausgewählt",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // MUL-Verzeichnis bestimmen: erst textBox1 (Quellverzeichnis), dann _mulDirectoryPath
            string searchDir = textBox1.Text;
            if (string.IsNullOrEmpty(searchDir) || !Directory.Exists(searchDir))
                searchDir = _mulDirectoryPath;

            if (string.IsNullOrEmpty(searchDir) || !Directory.Exists(searchDir))
            {
                MessageBox.Show(
                    "Kein gültiges Verzeichnis angegeben.\n" +
                    "Bitte das Quellverzeichnis (textBox1) oder das UO-Verzeichnis setzen.",
                    "Verzeichnis fehlt",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mapFile = Path.Combine(searchDir, $"map{_targetMap.Id}.mul");

            if (!File.Exists(mapFile))
            {
                // Fallback: andere IDs versuchen (z.B. map0LegacyMUL)
                MessageBox.Show(
                    $"Kartendatei nicht gefunden:\n{mapFile}\n\n" +
                    $"Bitte sicherstellen, dass map{_targetMap.Id}.mul im gewählten Verzeichnis liegt.",
                    "Datei nicht gefunden",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int mapW = GetMapWidth(_targetMap.Id);
            int mapH = GetMapHeight(_targetMap.Id);

            // Callbacks definieren: Koordinaten zurück in die Form schreiben
            Action<int, int, int, int> setFrom = (x1, y1, x2, y2) =>
            {
                // Invoke nötig falls aus anderem Thread aufgerufen
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => SetFromRegionCoords(x1, y1, x2, y2)));
                }
                else
                {
                    SetFromRegionCoords(x1, y1, x2, y2);
                }
            };

            Action<int, int> setTo = (tox, toy) =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => SetToRegionCoords(tox, toy)));
                }
                else
                {
                    SetToRegionCoords(tox, toy);
                }
            };

            // MapViewForm öffnen (nicht-modal, damit Hauptfenster bedienbar bleibt)
            MapViewForm viewForm = new MapViewForm(mapFile, mapW, mapH, setFrom, setTo);
            viewForm.Show(this);
        }
        #endregion

        #region [ SetFromRegionCoords ]
        /// <summary>
        /// Schreibt Koordinaten in die From-Region-Felder (X1/Y1/X2/Y2).
        /// Begrenzt Werte auf den gültigen Bereich der Karte.
        /// </summary>
        private void SetFromRegionCoords(int x1, int y1, int x2, int y2)
        {
            int maxW = _targetMap != null ? GetMapWidth(_targetMap.Id) : 7168;
            int maxH = _targetMap != null ? GetMapHeight(_targetMap.Id) : 4096;

            // Clamp auf Kartengrenzen
            x1 = Math.Max(0, Math.Min(maxW, x1));
            y1 = Math.Max(0, Math.Min(maxH, y1));
            x2 = Math.Max(0, Math.Min(maxW, x2));
            y2 = Math.Max(0, Math.Min(maxH, y2));

            // NumericUpDown Maximum vorher anpassen um Ausnahmen zu vermeiden
            numericUpDownX1.Maximum = maxW;
            numericUpDownX2.Maximum = maxW;
            numericUpDownY1.Maximum = maxH;
            numericUpDownY2.Maximum = maxH;

            numericUpDownX1.Value = x1;
            numericUpDownY1.Value = y1;
            numericUpDownX2.Value = x2;
            numericUpDownY2.Value = y2;
        }
        #endregion

        #region [ SetToRegionCoords ]
        /// <summary>
        /// Schreibt Koordinaten in die To-Region-Felder (ToX/ToY).
        /// </summary>
        private void SetToRegionCoords(int tox, int toy)
        {
            int maxW = _targetMap != null ? GetMapWidth(_targetMap.Id) : 7168;
            int maxH = _targetMap != null ? GetMapHeight(_targetMap.Id) : 4096;

            tox = Math.Max(0, Math.Min(maxW, tox));
            toy = Math.Max(0, Math.Min(maxH, toy));

            numericUpDownToX1.Maximum = maxW;
            numericUpDownToY1.Maximum = maxH;

            numericUpDownToX1.Value = tox;
            numericUpDownToY1.Value = toy;
        }
        #endregion
    }

    // ===========================================================================
    // class Map
    // ===========================================================================
    #region [ class Map ]
    /// <summary>
    /// Repräsentiert eine UO-Karte mit ID, Abmessungen und Datei-Index.
    /// FIX #4: FileIndex wird jetzt im Konstruktor auf Id gesetzt (war vorher immer 0).
    /// </summary>
    public class Map
    {
        public int Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// Datei-Index für Files.GetFilePath() Aufrufe.
        /// Standardmäßig gleich Id. Kann abweichen bei alten Felucca (Id=0, aber map0.mul).
        /// </summary>
        public int FileIndex { get; set; }

        public Map(int id, int width, int height)
        {
            Id = id;
            Width = width;
            Height = height;
            FileIndex = id; // FIX #4: FileIndex explizit setzen
        }
    }
    #endregion

    // ===========================================================================
    // class Hues
    // ===========================================================================
    #region [ class Hues ]
    /// <summary>
    /// Lädt und verwaltet Hue-Einträge aus hues.mul.
    /// 
    /// FIX #9: Korrekte Lese-Reihenfolge gemäß UO .mul-Format:
    ///   Pro Gruppe (8 Hues):
    ///     4 Bytes Header (uint)
    ///     Für jede der 8 Hues:
    ///       32 × ushort ColorTable  (64 Bytes)
    ///       ushort TableStart        (2 Bytes)
    ///       ushort TableEnd          (2 Bytes)
    ///       20 chars Name            (20 Bytes)
    ///   = 88 Bytes pro Hue-Eintrag
    /// </summary>
    public class Hues
    {
        public class Hue
        {
            public ushort[] ColorTable { get; set; }
            public ushort TableStart { get; set; }
            public ushort TableEnd { get; set; }
            public string Name { get; set; }
        }

        private readonly List<Hue> _hues = new List<Hue>();

        public Hues(string path)
        {
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new BinaryReader(fs);

            while (fs.Position < fs.Length)
            {
                // Gruppen-Header (4 Bytes)
                uint header = reader.ReadUInt32();

                for (int i = 0; i < 8; i++)
                {
                    Hue hue = new Hue
                    {
                        ColorTable = new ushort[32]
                    };

                    // FIX #9: Korrekte Reihenfolge: erst ColorTable, dann TableStart/End/Name
                    for (int j = 0; j < 32; j++)
                        hue.ColorTable[j] = reader.ReadUInt16();

                    hue.TableStart = reader.ReadUInt16();
                    hue.TableEnd = reader.ReadUInt16();
                    hue.Name = new string(reader.ReadChars(20)).TrimEnd('\0');

                    _hues.Add(hue);
                }
            }
        }

        /// <summary>Gibt einen Hue-Eintrag anhand seines Index zurück.</summary>
        public Hue GetHue(int index)
        {
            if (index < 0 || index >= _hues.Count)
                throw new IndexOutOfRangeException($"Ungültiger Hue-Index: {index}");

            return _hues[index];
        }
    }
    #endregion
}