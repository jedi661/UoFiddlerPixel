// /***************************************************************************
//  *
//  * $Author: Turley
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    public class EditMultiForm : Form
    {
        private TextBox idTextBox;
        private TextBox nameTextBox;
        private ListBox typeListBox;
        private Button okButton;
        private Button cancelButton;

        private int selectedMultiType;

        public int SelectedMultiType
        {
            get { return selectedMultiType; }
        }

        public string MultiName { get; private set; }
        public int MultiID { get; private set; }

        public EditMultiForm(string multiName, int multiID, int multiType)
        {
            InitializeComponent();

            MultiName = multiName;
            MultiID = multiID;
            selectedMultiType = multiType;

            // Setze die Werte der Steuerelemente
            nameTextBox.Text = multiName;
            idTextBox.Text = multiID.ToString();
            typeListBox.SelectedIndex = multiType;

            // Füge die Event Handler hinzu
            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;
        }


        private void OkButton_Click(object sender, EventArgs e)
        {
            // Validiere die Eingabe
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a valid name.");
                return;
            }

            // Setze das Ergebnis und schließe das Formular
            MultiName = nameTextBox.Text;
            selectedMultiType = typeListBox.SelectedIndex;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditMultiForm));
            idTextBox = new TextBox();
            nameTextBox = new TextBox();
            typeListBox = new ListBox();
            okButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // idTextBox
            // 
            idTextBox.Location = new Point(10, 40);
            idTextBox.Name = "idTextBox";
            idTextBox.ReadOnly = true;
            idTextBox.Size = new Size(59, 23);
            idTextBox.TabIndex = 0;
            // 
            // nameTextBox
            // 
            nameTextBox.Location = new Point(10, 10);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(250, 23);
            nameTextBox.TabIndex = 1;
            // 
            // typeListBox
            // 
            typeListBox.ItemHeight = 15;
            typeListBox.Items.AddRange(new object[] { "0: Boat", "1: House", "2: Decoration", "3: Other" });
            typeListBox.Location = new Point(138, 40);
            typeListBox.Name = "typeListBox";
            typeListBox.Size = new Size(120, 64);
            typeListBox.TabIndex = 2;
            // 
            // okButton
            // 
            okButton.Location = new Point(12, 110);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 3;
            okButton.Text = "OK";
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(183, 110);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 4;
            cancelButton.Text = "Cancel";
            // 
            // EditMultiForm
            // 
            ClientSize = new Size(269, 144);
            Controls.Add(idTextBox);
            Controls.Add(nameTextBox);
            Controls.Add(typeListBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "EditMultiForm";
            Text = "XML Editor";
            ResumeLayout(false);
            PerformLayout();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Schließe das Formular ohne Änderungen
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
