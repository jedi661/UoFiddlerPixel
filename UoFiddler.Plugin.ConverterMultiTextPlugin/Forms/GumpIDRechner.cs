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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class GumpIDRechner : Form
    {
        public GumpIDRechner()
        {
            InitializeComponent();
        }

        #region [ BtMen ]
        private void BtMen_Click(object sender, EventArgs e)
        {
            string hexInput = tbInput.Text;

            if (hexInput.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hexInput = hexInput.Substring(2);
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(hexInput, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                int decimalValue = Convert.ToInt32(hexInput, 16);
                tbDecimal.Text = decimalValue.ToString();

                string hexValue = "0x" + decimalValue.ToString("X");
                tbHex.Text = hexValue;

                string decimalString = decimalValue.ToString();
                string lastThreeDigits = decimalString.Length >= 3 ? decimalString.Substring(decimalString.Length - 3) : decimalString;
                tbAminID.Text = lastThreeDigits;

                // Convert the last three digits to hexadecimal and display in tbAminHex
                int lastThreeDigitsDecimal = int.Parse(lastThreeDigits);
                string lastThreeDigitsHex = "0x" + lastThreeDigitsDecimal.ToString("X");
                tbAminHex.Text = lastThreeDigitsHex;
            }
            else
            {
                MessageBox.Show("Please enter a valid hexadecimal number.");
            }
        }
        #endregion

        #region [ BtWoman ]
        private void BtWoman_Click(object sender, EventArgs e)
        {
            string hexInput = tbInput.Text;

            if (hexInput.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hexInput = hexInput.Substring(2);
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(hexInput, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                int decimalValue = Convert.ToInt32(hexInput, 16) + 10000;
                tbDecimal.Text = decimalValue.ToString();

                string hexValue = "0x" + decimalValue.ToString("X");
                tbHex.Text = hexValue;

                string decimalString = decimalValue.ToString();
                string lastThreeDigits = decimalString.Length >= 3 ? decimalString.Substring(decimalString.Length - 3) : decimalString;
                tbAminID.Text = lastThreeDigits;

                // Convert the last three digits to hexadecimal and display in tbAminHex
                int lastThreeDigitsDecimal = int.Parse(lastThreeDigits);
                string lastThreeDigitsHex = "0x" + lastThreeDigitsDecimal.ToString("X");
                tbAminHex.Text = lastThreeDigitsHex;
            }
            else
            {
                MessageBox.Show("Bitte geben Sie eine gültige Hexadezimalzahl ein.");
            }
        }
        #endregion

        #region [ tbInput2 ]
        private void tbInput2_TextChanged(object sender, EventArgs e)
        {
            ConvertInput();
        }
        #endregion

        #region [ CheckBox_CheckedChanged ]
        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Ensure that only one checkbox is checked at a time
            CheckBox[] checkBoxes = { checkBoxDecimal, checkBoxHexAdress, checkBoxBinary, checkBoxOctal, checkBoxBaseN, checkBoxAscii, checkBoxCase, checkBoxAsciiCode, checkBoxAsciiToText };
            foreach (var checkBox in checkBoxes)
            {
                if (sender != checkBox && ((CheckBox)sender).Checked)
                {
                    checkBox.Checked = false;
                }
            }

            ConvertInput();
        }
        #endregion

        #region [ ConvertInput ]
        private void ConvertInput()
        {
            string input = tbInput2.Text;
            if (string.IsNullOrEmpty(input))
            {
                tbOutput.Text = "";
                return;
            }

            if (checkBoxDecimal.Checked)
            {
                if (int.TryParse(input, out int decimalValue))
                {
                    tbOutput.Text = decimalValue.ToString("X");
                }
                else
                {
                    MessageBox.Show("Please enter a valid decimal number.");
                }
            }
            else if (checkBoxHexAdress.Checked)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(input, @"\A\b[0-9a-fA-F]+\b\Z"))
                {
                    int decimalValue = Convert.ToInt32(input, 16);
                    tbOutput.Text = decimalValue.ToString();
                }
                else
                {
                    MessageBox.Show("Please enter a valid hexadecimal number.");
                }
            }
            else if (checkBoxBinary.Checked)
            {
                try
                {
                    int decimalValue = Convert.ToInt32(input, 2);
                    tbOutput.Text = decimalValue.ToString();
                }
                catch
                {
                    MessageBox.Show("Please enter a valid binary number.");
                }
            }
            else if (checkBoxOctal.Checked)
            {
                try
                {
                    int decimalValue = Convert.ToInt32(input, 8);
                    tbOutput.Text = decimalValue.ToString();
                }
                catch
                {
                    MessageBox.Show("Please enter a valid octal number.");
                }
            }
            else if (checkBoxBaseN.Checked)
            {
                // Here you need to define the base you want to convert to.
                int baseN = 10; // Change this to the base you want
                try
                {
                    int decimalValue = Convert.ToInt32(input, baseN);
                    tbOutput.Text = decimalValue.ToString();
                }
                catch
                {
                    MessageBox.Show($"Please provide a valid base-{baseN}-Number one.");
                }
            }
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
                    MessageBox.Show("Please enter a valid ASCII value.");
                }
            }
            else if (checkBoxCase.Checked)
            {
                tbOutput.Text = input.ToUpper();
            }
            else if (checkBoxAsciiCode.Checked)
            {
                StringBuilder asciiCodes = new StringBuilder();
                foreach (char c in input)
                {
                    asciiCodes.Append(((int)c).ToString() + " ");
                }
                tbOutput.Text = asciiCodes.ToString().TrimEnd();
            }
            else if (checkBoxAsciiToText.Checked)
            {
                StringBuilder text = new StringBuilder();
                string[] asciiCodes = input.Split(' ');
                foreach (string asciiCode in asciiCodes)
                {
                    if (int.TryParse(asciiCode, out int code))
                    {
                        text.Append((char)code);
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid ASCII codes.");
                        return;
                    }
                }
                tbOutput.Text = text.ToString();
            }
        }
        #endregion

        #region [ Clipboard ]
        private void tbOutput_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbOutput.Text))
            {
                Clipboard.SetText(tbOutput.Text);
            }
        }
        #endregion
    }
}
