using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.Models.Uop;

namespace UoFiddler.Controls.Forms
{
    public partial class VdExportRemapperForm : Form
    {
        private TableLayoutPanel _mainLayout;
        private Button _saveButton;
        private Button _cancelButton;

        private Dictionary<string, ComboBox> _actionComboBoxes;

        public VdExportRemapperForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // VdExportRemapperForm
            // 
            ClientSize = new Size(400, 500);
            Name = "VdExportRemapperForm";
            Text = "VD Export Remapper";
            ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            _actionComboBoxes = new Dictionary<string, ComboBox>();

            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10),
                AutoScroll = true
            };
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            
            var bottomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(10)
            };
            _saveButton = new Button { Text = "Save" };
            _cancelButton = new Button { Text = "Cancel" };
            _saveButton.Click += (sender, e) => { DialogResult = DialogResult.OK; };
            _cancelButton.Click += (sender, e) => { DialogResult = DialogResult.Cancel; };
            bottomPanel.Controls.Add(_cancelButton);
            bottomPanel.Controls.Add(_saveButton);

            var scrollablePanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            scrollablePanel.Controls.Add(_mainLayout);

            Controls.Add(scrollablePanel);
            Controls.Add(bottomPanel);
        }

        public void PopulateForm(List<string> actions, List<UopIndexOption> uopGroupOptions, Dictionary<int, int> initialMapping)
        {
            _mainLayout.SuspendLayout();
            _mainLayout.Controls.Clear();
            _actionComboBoxes.Clear();
            _mainLayout.RowCount = actions.Count;
            _mainLayout.RowStyles.Clear();

            for (int i = 0; i < actions.Count; i++)
            {
                var actionName = actions[i];
                var actionLabel = new Label
                {
                    Text = actionName,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var actionComboBox = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Dock = DockStyle.Fill,
                    DisplayMember = "DisplayName",
                    ValueMember = "Id"
                };

                // Créer une nouvelle liste pour CHAQUE ComboBox
                var comboDataSource = new List<UopIndexOption>(uopGroupOptions);
                
                // IMPORTANT: Ajouter le ComboBox au layout AVANT de définir DataSource
                _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                _mainLayout.Controls.Add(actionLabel, 0, i);
                _mainLayout.Controls.Add(actionComboBox, 1, i);
                
                // Maintenant définir le DataSource (le contrôle est déjà ajouté)
                actionComboBox.DataSource = comboDataSource;
                
                // Ajouter au dictionnaire
                _actionComboBoxes[actionName] = actionComboBox;

                // Tentative de sélection - le ComboBox devrait maintenant avoir ses items
                if (initialMapping != null && initialMapping.TryGetValue(i, out int mappedValue))
                {
                    // Chercher l'index de l'élément qui a cet ID
                    int foundIndex = -1;
                    for (int j = 0; j < comboDataSource.Count; j++)
                    {
                        if (comboDataSource[j].Id == mappedValue)
                        {
                            foundIndex = j;
                            break;
                        }
                    }
                    
                    if (foundIndex >= 0 && foundIndex < actionComboBox.Items.Count)
                    {
                        actionComboBox.SelectedIndex = foundIndex;
                    }
                    else
                    {
                        // L'ID n'existe pas dans la liste, sélectionner "None"
                        actionComboBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    // Pas de mapping, sélectionner "None" par défaut
                    if (actionComboBox.Items.Count > 0)
                    {
                        actionComboBox.SelectedIndex = 0;
                    }
                }
            }

            _mainLayout.ResumeLayout();
        }

        public Dictionary<string, int> GetRemapping()
        {
            var mapping = new Dictionary<string, int>();
            foreach (var pair in _actionComboBoxes)
            {
                if (pair.Value.SelectedValue != null)
                {
                    mapping[pair.Key] = (int)pair.Value.SelectedValue;
                }
            }
            return mapping;
        }
    }
}
