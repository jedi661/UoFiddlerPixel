// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class GumpIDRechner : Form
    {
        // -----------------------------------------------------------------------
        // Konstanten fuer UO Gump Adress-Berechnung
        // Die Woman-Variante liegt im Speicher immer +10000 hoeher als Men
        // Dies entspricht dem Offset in der UO Gump Address Table
        // -----------------------------------------------------------------------
        private const int WOMAN_OFFSET = 10000;

        // -----------------------------------------------------------------------
        // Gueltige UO Gump ID Range (0x0000 bis 0xFFFF = 0 bis 65535)
        // IDs ausserhalb dieses Bereichs sind in UO nicht gueltig
        // -----------------------------------------------------------------------
        private const int UO_GUMP_MIN = 0;
        private const int UO_GUMP_MAX = 65535;

        public GumpIDRechner()
        {
            InitializeComponent();
        }

        // -----------------------------------------------------------------------
        // Hilfsmethode: Bereinigt Hex-Input und entfernt "0x"-Prefix falls vorhanden
        // Gibt den bereinigten String zurueck
        // -----------------------------------------------------------------------
        private string StripHexPrefix(string input)
        {
            input = input.Trim();
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                input = input.Substring(2);
            return input;
        }

        // -----------------------------------------------------------------------
        // Hilfsmethode: Prueft ob ein String ein gueltiger Hex-Wert ist
        // -----------------------------------------------------------------------
        private bool IsValidHex(string input)
        {
            return Regex.IsMatch(input, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        // -----------------------------------------------------------------------
        // Hilfsmethode: Prueft ob eine Decimal-ID im gueltigen UO Gump Bereich liegt
        // Gibt eine Warnung als String zurueck, oder leer wenn alles OK
        // -----------------------------------------------------------------------
        private string CheckGumpRange(int decimalValue)
        {
            if (decimalValue < UO_GUMP_MIN || decimalValue > UO_GUMP_MAX)
                return $"⚠ Warnung: ID {decimalValue} liegt ausserhalb des gueltigen UO Gump Bereichs (0 – 65535)!";
            return string.Empty;
        }

        // -----------------------------------------------------------------------
        // Kernmethode fuer die Gump ID Berechnung
        // Wird von BtMen_Click und BtWoman_Click verwendet
        // isWoman = true  → +10000 auf den Hex-Eingabewert (Woman Adress-Offset)
        // isWoman = false → kein Offset (Men Adress)
        // -----------------------------------------------------------------------
        private void CalculateGumpID(bool isWoman)
        {
            string hexInput = StripHexPrefix(tbInput.Text);

            // Pruefen ob ein gueltiger Hex-Wert eingegeben wurde
            if (!IsValidHex(hexInput))
            {
                MessageBox.Show(
                    "Bitte eine gueltige Hexadezimalzahl eingeben.\nBeispiel: 0x1234 oder 1234",
                    "Ungueltige Eingabe",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Hex in Decimal umrechnen, bei Woman +10000 addieren (Gump Address Offset)
            int decimalValue = Convert.ToInt32(hexInput, 16);
            if (isWoman)
                decimalValue += WOMAN_OFFSET;

            // --- Decimal Ausgabe ---
            tbDecimal.Text = decimalValue.ToString();

            // --- Hex Ausgabe mit 0x-Prefix und 4-stelligem Padding (UO-Standard) ---
            tbHex.Text = "0x" + decimalValue.ToString("X4");

            // --- AMin ID Berechnung ---
            // Die AMin ID ist der Modulo-1000-Wert der Decimal-ID
            // Dies gibt die letzten 3 Stellen als Animations-Index wieder
            // WICHTIG: Modulo 1000 ist korrekt - nicht einfach die letzten 3 Zeichen des Strings!
            int aminID = decimalValue % 1000;
            tbAminID.Text = aminID.ToString();

            // --- AMin Hex Ausgabe ---
            // Die AMin Hex ist der AMin ID Wert als Hexadezimal mit 0x-Prefix
            tbAminHex.Text = "0x" + aminID.ToString("X");

            // --- UO Gump Range Validierung ---
            // Warnt den Benutzer wenn die berechnete ID ausserhalb des gueltigen Bereichs liegt
            string rangeWarning = CheckGumpRange(decimalValue);
            lbHexAdressInput.Text = string.IsNullOrEmpty(rangeWarning)
                ? $"✓ Gueltige UO Gump ID | {(isWoman ? "Woman (+10000)" : "Men")}"
                : rangeWarning;

            // --- Hex Eingabe Label aktualisieren ---
            // Zeigt dem Benutzer die aktuelle Berechnungsart (Men oder Woman)
            lbHexAdressInput.ForeColor = string.IsNullOrEmpty(rangeWarning)
                ? System.Drawing.Color.Green
                : System.Drawing.Color.Red;
        }

        #region [ BtMen ]
        // -----------------------------------------------------------------------
        // Men Button: Berechnet die Gump-Adresse ohne Offset
        // Eingabe: Hex-Adresse aus tbInput
        // Ausgabe: Decimal, Hex (0x), AMin ID, AMin Hex in den Result-Feldern
        // -----------------------------------------------------------------------
        private void BtMen_Click(object sender, EventArgs e)
        {
            CalculateGumpID(false);
        }
        #endregion

        #region [ BtWoman ]
        // -----------------------------------------------------------------------
        // Woman Button: Berechnet die Gump-Adresse mit +10000 Offset
        // In Ultima Online liegen die Woman-Gumps immer +10000 hoeher in der Adresstabelle
        // Eingabe: Hex-Adresse aus tbInput
        // Ausgabe: Decimal (+10000), Hex, AMin ID, AMin Hex in den Result-Feldern
        // -----------------------------------------------------------------------
        private void BtWoman_Click(object sender, EventArgs e)
        {
            CalculateGumpID(true);
        }
        #endregion

        #region [ BtDecimalInput ]
        // -----------------------------------------------------------------------
        // Decimal Input Button: Erlaubt die Eingabe als Dezimalzahl statt Hex
        // Rechnet den Decimal-Wert in Hex um und befuellt tbInput automatisch
        // Danach kann Men oder Woman normal berechnet werden
        // -----------------------------------------------------------------------
        private void BtDecimalInput_Click(object sender, EventArgs e)
        {
            // Pruefen ob eine gueltige Dezimalzahl im Decimal-Input-Feld steht
            if (!int.TryParse(tbDecimalInput.Text.Trim(), out int decimalInput))
            {
                MessageBox.Show(
                    "Bitte eine gueltige Dezimalzahl eingeben.",
                    "Ungueltige Eingabe",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Decimal in Hex umrechnen und in tbInput eintragen
            // Damit kann der Benutzer danach Men/Woman normal berechnen
            tbInput.Text = "0x" + decimalInput.ToString("X4");
            lbHexAdressInput.Text = $"Decimal {decimalInput} → Hex {tbInput.Text} eingetragen";
            lbHexAdressInput.ForeColor = System.Drawing.Color.Blue;
        }
        #endregion

        #region [ BtCopyResults ]
        // -----------------------------------------------------------------------
        // Copy Button: Kopiert alle Ergebnisse als formatierten Text in die Zwischenablage
        // Nützlich um die berechneten IDs direkt in Scripts/Editoren einzufuegen
        // -----------------------------------------------------------------------
        private void BtCopyResults_Click(object sender, EventArgs e)
        {
            // Alle Ergebnisfelder pruefen ob Inhalt vorhanden
            if (string.IsNullOrEmpty(tbDecimal.Text))
            {
                MessageBox.Show(
                    "Keine Ergebnisse zum Kopieren vorhanden.\nBitte zuerst Men oder Woman berechnen.",
                    "Nichts zu kopieren",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Formatierter Text fuer die Zwischenablage
            string clipboardText =
                $"Decimal : {tbDecimal.Text}\r\n" +
                $"Hex     : {tbHex.Text}\r\n" +
                $"AMin ID : {tbAminID.Text}\r\n" +
                $"AMin Hex: {tbAminHex.Text}";

            Clipboard.SetText(clipboardText);
            MessageBox.Show(
                "Ergebnisse in die Zwischenablage kopiert!",
                "Kopiert",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        #endregion

        #region [ tbInput2 ]
        // -----------------------------------------------------------------------
        // TextChanged Event fuer den Converter-Tab
        // Wird bei jeder Eingabe ausgeloest und ruft ConvertInput() auf
        // -----------------------------------------------------------------------
        private void tbInput2_TextChanged(object sender, EventArgs e)
        {
            ConvertInput();
        }
        #endregion

        #region [ CheckBox_CheckedChanged ]
        // -----------------------------------------------------------------------
        // CheckBox Event: Stellt sicher dass immer nur eine CheckBox aktiv ist
        // Wechselt automatisch zur neuen Auswahl und berechnet direkt neu
        // -----------------------------------------------------------------------
        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Alle Checkboxen ausser der gerade angeklickten deaktivieren
            // So ist immer nur eine Konvertierungsart aktiv
            CheckBox[] checkBoxes =
            {
                checkBoxDecimal,
                checkBoxHexAdress,
                checkBoxBinary,
                checkBoxOctal,
                checkBoxBaseN,
                checkBoxAscii,
                checkBoxCase,
                checkBoxAsciiCode,
                checkBoxAsciiToText,
                checkBoxAllConversions  // NEU: Alle auf einmal anzeigen
            };

            foreach (var checkBox in checkBoxes)
            {
                if (sender != checkBox && ((CheckBox)sender).Checked)
                    checkBox.Checked = false;
            }

            ConvertInput();
        }
        #endregion

        #region [ ConvertInput ]
        // -----------------------------------------------------------------------
        // Hauptmethode fuer alle Konvertierungen im Converter-Tab
        // Liest den Input aus tbInput2 und schreibt das Ergebnis in tbOutput
        // Jede CheckBox entspricht einer anderen Konvertierungsart
        // -----------------------------------------------------------------------
        private void ConvertInput()
        {
            string input = tbInput2.Text;

            // Leeres Input-Feld → Output leeren
            if (string.IsNullOrEmpty(input))
            {
                tbOutput.Text = string.Empty;
                return;
            }

            // --- Decimal → Hex ---
            // Eingabe: Dezimalzahl | Ausgabe: Hexadezimal (ohne 0x-Prefix)
            if (checkBoxDecimal.Checked)
            {
                if (int.TryParse(input, out int decimalValue))
                    tbOutput.Text = decimalValue.ToString("X");
                else
                    ShowConvertError("Bitte eine gueltige Dezimalzahl eingeben.");
            }

            // --- Hex → Decimal ---
            // Eingabe: Hexadezimalzahl (mit oder ohne 0x) | Ausgabe: Dezimalzahl
            else if (checkBoxHexAdress.Checked)
            {
                string cleanHex = StripHexPrefix(input);
                if (IsValidHex(cleanHex))
                    tbOutput.Text = Convert.ToInt32(cleanHex, 16).ToString();
                else
                    ShowConvertError("Bitte eine gueltige Hexadezimalzahl eingeben.");
            }

            // --- Binaer → Decimal ---
            // Eingabe: Binaerzahl (z.B. 11010110) | Ausgabe: Dezimalzahl
            else if (checkBoxBinary.Checked)
            {
                try
                {
                    tbOutput.Text = Convert.ToInt32(input, 2).ToString();
                }
                catch
                {
                    ShowConvertError("Bitte eine gueltige Binaerzahl eingeben (nur 0 und 1).");
                }
            }

            // --- Oktal → Decimal ---
            // Eingabe: Oktalzahl (z.B. 377) | Ausgabe: Dezimalzahl
            else if (checkBoxOctal.Checked)
            {
                try
                {
                    tbOutput.Text = Convert.ToInt32(input, 8).ToString();
                }
                catch
                {
                    ShowConvertError("Bitte eine gueltige Oktalzahl eingeben (0-7).");
                }
            }

            // --- Base-N Konvertierung ---
            // Eingabe: Dezimalzahl und gewuenschte Zielbasis aus tbBaseN
            // Ausgabe: Zahl in der angegebenen Basis
            else if (checkBoxBaseN.Checked)
            {
                // Zielbasis aus dem Base-N Eingabefeld lesen (z.B. 16 fuer Hex)
                if (!int.TryParse(tbBaseN.Text, out int baseN) || baseN < 2 || baseN > 36)
                {
                    ShowConvertError("Bitte eine gueltige Basis zwischen 2 und 36 eingeben.");
                    return;
                }

                // Eingabe zuerst als Dezimal parsen
                if (!int.TryParse(input, out int baseDecimal))
                {
                    ShowConvertError($"Bitte eine gueltige Dezimalzahl als Eingabe fuer Base-{baseN} Konvertierung eingeben.");
                    return;
                }

                // Dezimal → Zielbasis umrechnen
                tbOutput.Text = ConvertToBaseN(baseDecimal, baseN);
            }

            // --- ASCII Code → Zeichen ---
            // Eingabe: ASCII-Zahlenwert (z.B. 65) | Ausgabe: entsprechendes Zeichen (A)
            else if (checkBoxAscii.Checked)
            {
                try
                {
                    int asciiValue = Convert.ToInt32(input);
                    char character = (char)asciiValue;
                    tbOutput.Text = character.ToString();
                }
                catch
                {
                    ShowConvertError("Bitte einen gueltigen ASCII-Wert (0-127) eingeben.");
                }
            }

            // --- Upper / Lower Case Toggle ---
            // Eingabe: beliebiger Text | Ausgabe: GROSSBUCHSTABEN oder kleinbuchstaben
            // Beim ersten Aufruf → Upper, beim zweiten → Lower (Toggle)
            else if (checkBoxCase.Checked)
            {
                // Toggle: Wenn der Input Grossbuchstaben hat → Lower, sonst → Upper
                if (input == input.ToUpper())
                    tbOutput.Text = input.ToLower();
                else
                    tbOutput.Text = input.ToUpper();
            }

            // --- Text → ASCII Codes ---
            // Eingabe: Text | Ausgabe: Leerzeichen-getrennte ASCII-Zahlen
            else if (checkBoxAsciiCode.Checked)
            {
                var asciiCodes = new StringBuilder();
                foreach (char c in input)
                    asciiCodes.Append(((int)c).ToString() + " ");
                tbOutput.Text = asciiCodes.ToString().TrimEnd();
            }

            // --- ASCII Codes → Text ---
            // Eingabe: Leerzeichen-getrennte ASCII-Zahlen | Ausgabe: Text
            else if (checkBoxAsciiToText.Checked)
            {
                var text = new StringBuilder();
                string[] asciiCodes = input.Split(' ');
                foreach (string asciiCode in asciiCodes)
                {
                    if (int.TryParse(asciiCode, out int code))
                        text.Append((char)code);
                    else
                    {
                        ShowConvertError("Bitte gueltige ASCII-Codes eingeben (leerzeichen-getrennte Zahlen).");
                        return;
                    }
                }
                tbOutput.Text = text.ToString();
            }

            // --- Alle Konvertierungen auf einmal ---
            // Zeigt alle moeglichen Umrechnungen gleichzeitig im Output-Feld an
            // Nützlich fuer einen schnellen Ueberblick aller Werte
            else if (checkBoxAllConversions.Checked)
            {
                ShowAllConversions(input);
            }
        }
        #endregion

        #region [ ShowAllConversions ]
        // -----------------------------------------------------------------------
        // Zeigt alle Konvertierungen gleichzeitig im Output-Feld an
        // Versucht den Input als Dezimal zu interpretieren und rechnet alles um
        // Besonders nuetzlich um schnell alle Werte eines UO Gump-Wertes zu sehen
        // -----------------------------------------------------------------------
        private void ShowAllConversions(string input)
        {
            // Versuchen den Input als Dezimalzahl zu parsen
            if (!int.TryParse(input, out int decimalValue))
            {
                // Wenn kein Decimal → versuche als Hex
                string cleanHex = StripHexPrefix(input);
                if (IsValidHex(cleanHex))
                    decimalValue = Convert.ToInt32(cleanHex, 16);
                else
                {
                    tbOutput.Text = "Eingabe muss eine Dezimal- oder Hexadezimalzahl sein fuer 'Alle anzeigen'.";
                    return;
                }
            }

            // Alle Werte berechnen
            int aminID = decimalValue % 1000;
            int womanValue = decimalValue + WOMAN_OFFSET;

            // Formatierter Multi-Line Output mit allen Konvertierungen
            var sb = new StringBuilder();
            sb.AppendLine($"Decimal      : {decimalValue}");
            sb.AppendLine($"Hex          : 0x{decimalValue:X4}");
            sb.AppendLine($"Binary       : {Convert.ToString(decimalValue, 2)}");
            sb.AppendLine($"Octal        : {Convert.ToString(decimalValue, 8)}");
            sb.AppendLine($"AMin ID      : {aminID}");
            sb.AppendLine($"AMin Hex     : 0x{aminID:X}");
            sb.AppendLine("---");
            sb.AppendLine($"Gump Men     : 0x{decimalValue:X4}  ({decimalValue})");
            sb.AppendLine($"Gump Woman   : 0x{womanValue:X4}  ({womanValue})  [+{WOMAN_OFFSET}]");

            // UO Range Check mit ausgeben
            string rangeStatus = (decimalValue >= UO_GUMP_MIN && decimalValue <= UO_GUMP_MAX)
                ? "✓ Gueltige UO Gump ID"
                : $"⚠ Ausserhalb UO Gump Bereich (0–{UO_GUMP_MAX})";
            sb.AppendLine($"Range Check  : {rangeStatus}");

            tbOutput.Text = sb.ToString().TrimEnd();
        }
        #endregion

        #region [ ConvertToBaseN ]
        // -----------------------------------------------------------------------
        // Hilfsmethode: Rechnet eine Dezimalzahl in eine beliebige Basis um (2–36)
        // Verwendet Ziffern 0-9 und Buchstaben A-Z fuer Basis > 10
        // -----------------------------------------------------------------------
        private string ConvertToBaseN(int value, int baseN)
        {
            if (value == 0) return "0";

            const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var result = new StringBuilder();
            bool isNegative = value < 0;
            value = Math.Abs(value);

            while (value > 0)
            {
                result.Insert(0, digits[value % baseN]);
                value /= baseN;
            }

            if (isNegative)
                result.Insert(0, '-');

            return result.ToString();
        }
        #endregion

        #region [ ShowConvertError ]
        // -----------------------------------------------------------------------
        // Hilfsmethode: Zeigt eine Fehlermeldung im Output-Feld statt eines MessageBox-Popups
        // Weniger störend beim Tippen als ein MessageBox.Show()
        // -----------------------------------------------------------------------
        private void ShowConvertError(string message)
        {
            tbOutput.Text = $"⚠ {message}";
        }
        #endregion

        #region [ Clipboard ]
        // -----------------------------------------------------------------------
        // Doppelklick auf Output-Feld kopiert den Inhalt in die Zwischenablage
        // Schnelle Moeglichkeit um Ergebnisse zu kopieren ohne extra Button
        // -----------------------------------------------------------------------
        private void tbOutput_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbOutput.Text))
            {
                Clipboard.SetText(tbOutput.Text);
                MessageBox.Show(
                    "Output in Zwischenablage kopiert!",
                    "Kopiert",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        #endregion
    }
}