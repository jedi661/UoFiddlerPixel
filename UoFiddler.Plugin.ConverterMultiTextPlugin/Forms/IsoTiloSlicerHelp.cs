// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class IsoTiloSlicerHelp : Form
    {
        public IsoTiloSlicerHelp()
        {
            InitializeComponent();
            rtbHelp.Rtf = BuildRtf();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string BuildRtf()
        {
            return @"{\rtf1\ansi\deff0
{\fonttbl{\f0\fswiss\fcharset0 Segoe UI;}{\f1\fmodern\fcharset0 Consolas;}}
{\colortbl;\red0\green0\blue0;\red0\green100\blue180;\red80\green80\blue80;\red200\green80\blue0;}
\f0\fs20\cf1

{\fs28\b\cf2 IsoTiloSlicer - Hilfe / Help}\par
\par

{\b\fs22 Was macht dieses Tool?}\par
\cf3 IsoTiloSlicer zerteilt ein grosses Bild in einzelne Kacheln (Tiles) und speichert sie als BMP-Dateien.\line
Zusaetzlich wird eine HTML-Datei erzeugt, die alle Kacheln wieder korrekt zusammensetzt.\par
\cf1\par

{\b\fs22 Zwei Modi}\par\par

{\b Straight view}  (ImageHandler1)\par
\cf3 Schneidet das Bild in ein einfaches Raster - Zeile fuer Zeile, Spalte fuer Spalte.\line
Gut geeignet fuer normale, nicht-isometrische Grafiken.\par\cf1\par

{\b Grid view}  (ImageHandler2)\par
\cf3 Schneidet das Bild isometrisch - jede zweite Zeile ist um eine halbe Tile-Breite versetzt.\line
Erzeugt zusaetzlich eine grid.png-Datei als Overlay-Referenz.\line
Gedacht fuer isometrische UO-Tiles im Rauten-Muster.\par\cf1\par

{\b\fs22 Einstellungen}\par\par
{\b Tile Width / Height}\tab Groesse jeder einzelnen Kachel in Pixel. Standard: 44 x 44\par
{\b Offset}\tab\tab\tab Versatz in Pixel beim isometrischen Schnitt. Standard: 1\par
{\b Start number}\tab\tab Erste Nummer der Ausgabedateien (z.B. 0 = 0.bmp, 1.bmp, ...)\par
{\b Filename format}\tab\tab .NET-Formatstring fuer den Dateinamen. \f1\cf4 {0}\f0\cf1  = laufende Nummer\par\par

{\b\fs22 Schritt-fuer-Schritt}\par\par
{\b 1.}\tab Bild laden: {\b Browse...} oder Rechtsklick \endash  {\b Load from file} oder {\b Import from Clipboard}\par
{\b 2.}\tab Einstellungen anpassen (Tile-Groesse, Offset, Startnummer)\par
{\b 3.}\tab Modus waehlen und auf {\b Straight view} oder {\b Grid view} klicken\par
{\b 4.}\tab Ergebnis ansehen: {\b Open output folder} oeffnet den Ordner tempGrafic\par
{\b 5.}\tab Temporaere Dateien loeschen: {\b Delete temp}\par\par

{\b\fs22 Rechtsklick auf das Vorschaubild}\par\par
{\b Load from file}\tab\tab Bild aus Datei laden\par
{\b Import from Clipboard}\tab Bild aus der Zwischenablage einfuegen\par
{\b Mirror horizontal}\tab\tab Bild horizontal spiegeln\par
{\b Run (Straight view)}\tab\tab Aktuelles Vorschaubild direkt verarbeiten\par\par

{\b\fs22 Ausgabe}\par
\cf3 Alle Dateien werden im Ordner \f1\cf4 [Programmverzeichnis]\tempGrafic\f0\cf3  gespeichert.\line
Pro Durchlauf:\line
\bullet  0.bmp, 1.bmp, 2.bmp, ...  - die einzelnen Tiles\line
\bullet  layout.html  - HTML-Vorschau die alle Tiles zusammensetzt\line
\bullet  grid.png  - Rasterreferenz (nur Grid view)\par\cf1\par

{\b\fs22 Tipps}\par
\cf3
\bullet  Vor jedem neuen Durchlauf {\b Delete temp} klicken, um alte Dateien zu entfernen.\line
\bullet  Das Bild kann gespiegelt werden bevor es verarbeitet wird.\line
\bullet  Tile-Groesse 44x44 ist der UO-Standard fuer isometrische Bodentiles.\line
\bullet  Die HTML-Datei im Browser oeffnen um das Ergebnis schnell zu pruefen.\par\cf1
}";
        }
    }
}