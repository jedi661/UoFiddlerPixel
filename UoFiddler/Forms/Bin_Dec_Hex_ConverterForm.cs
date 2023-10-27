using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Forms
{
    public partial class Bin_Dec_Hex_ConverterForm : Form
    {
        public Bin_Dec_Hex_ConverterForm()
        {
            InitializeComponent();

            if (Properties.Settings.Default.FormBinDecHexConverter != Point.Empty)
            {
                this.Location = Properties.Settings.Default.FormBinDecHexConverter;
            }
        }

        private void textBoxBinär_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int binValue = Convert.ToInt32(textBoxBinär.Text, 2);
                textBoxDecimal.Text = binValue.ToString();
                textBoxHexdezimal.Text = binValue.ToString("X");
                //UpdateListing();
            }
            catch
            {
                // Invalid Input
            }
        }

        private void textBoxDecimal_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int decValue = int.Parse(textBoxDecimal.Text);
                textBoxBinär.Text = Convert.ToString(decValue, 2);
                textBoxHexdezimal.Text = decValue.ToString("X");
                UpdateListing();
            }
            catch
            {
                // Invalid Input
            }
        }

        private void textBoxHexdezimal_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int hexValue = Convert.ToInt32(textBoxHexdezimal.Text, 16);
                textBoxBinär.Text = Convert.ToString(hexValue, 2);
                textBoxDecimal.Text = hexValue.ToString();
                //UpdateListing();
            }
            catch
            {
                // Invalid Input
            }
        }

        private void textBoxBinär_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '0' && e.KeyChar != '1')
            {
                e.Handled = true;
            }
        }

        private void textBoxDecimal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBoxHexdezimal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !Uri.IsHexDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void bt_0Binär_Click(object sender, EventArgs e)
        {
            textBoxBinär.AppendText("0");
            textBoxBinär_TextChanged(sender, e);
        }

        private void bt_1Binär_Click(object sender, EventArgs e)
        {
            textBoxBinär.AppendText("1");
            textBoxBinär_TextChanged(sender, e);
        }

        private void bt_Decimal01_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("1");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal02_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("2");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal03_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("3");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal04_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("4");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal05_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("5");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal06_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("6");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal07_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("7");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal08_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("8");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal09_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("9");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Decimal00_Click(object sender, EventArgs e)
        {
            textBoxDecimal.AppendText("0");
            textBoxDecimal_TextChanged(sender, e);
        }

        private void bt_Hex01_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("1");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex02_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("2");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex03_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("3");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex04_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("4");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex05_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("5");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex06_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("6");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex07_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("7");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex08_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("8");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex09_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("9");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex00_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("0");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex0A_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("A");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex0B_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("B");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex0C_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("C");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex0D_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("D");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex0E_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("E");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void bt_Hex0F_Click(object sender, EventArgs e)
        {
            textBoxHexdezimal.AppendText("F");
            textBoxHexdezimal_TextChanged(sender, e);
        }

        private void Bt_Delete_fields_Click(object sender, EventArgs e)
        {
            textBoxBinär.Clear();
            textBoxDecimal.Clear();
            textBoxHexdezimal.Clear();
        }

        private void UpdateListing()
        {
            tb_listing.AppendText($"Binary: {textBoxBinär.Text}\r\n");
            tb_listing.AppendText($"Decimal: {textBoxDecimal.Text}\r\n");
            tb_listing.AppendText($"Hexadecimal: {textBoxHexdezimal.Text}\r\n");
            tb_listing.AppendText("\r\n");
        }

        private void bt_Listing_Clear_Click(object sender, EventArgs e)
        {
            tb_listing.Clear();
        }

        // Save Position User.config
        private void Bin_Dec_Hex_ConverterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.FormBinDecHexConverter = this.Location;
            Properties.Settings.Default.Save();
        }

        //Load Postion
        private void Bin_Dec_Hex_ConverterForm_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.FormBinDecHexConverter != Point.Empty)
            {
                this.Location = Properties.Settings.Default.FormBinDecHexConverter;
            }
        }
    }
}
