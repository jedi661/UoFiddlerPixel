/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 * 
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class MapMarkerForm : Form
    {
        private readonly Action<int, int, int, Color, string> _addOverlayAction;
        private static Color _lastColor = Color.FromArgb(180, Color.Yellow);

        private MapControl _mapControl;

        public MapMarkerForm(Action<int, int, int, Color, string> addOverlayAction, int x, int y, int map)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _addOverlayAction = addOverlayAction;

            numericUpDown1.Value = x;
            numericUpDown2.Value = y;

            comboBox1.Items.AddRange(Options.MapNames);
            
            if (map < Options.MapNames.Length)
            {
                comboBox1.SelectedIndex = map;
            }
            else
            {
                // Handle the error or set a default value
            }

            pictureBox1.BackColor = _lastColor;

            LoadMapOverlays();
        }

        public void SetMapControl(MapControl mapControl)
        {
            _mapControl = mapControl;
        }

        private void OnClickColor(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
            {
                _lastColor = pictureBox1.BackColor = colorDialog1.Color;
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            _addOverlayAction(
                (int)numericUpDown1.Value,
                (int)numericUpDown2.Value,
                comboBox1.SelectedIndex,
                pictureBox1.BackColor,
                textBox1.Text);

            Close();
        }

        private void LoadMapOverlays()
        {
            // Load the XML document
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Options.AppDataPath, "MapOverlays.xml"));

            // Clear the ListBox
            listBoxMapOverlaysList.Items.Clear();

            // Add each marker to the ListBox
            foreach (XmlElement markerElement in doc.DocumentElement.SelectNodes("Marker"))
            {
                string text = markerElement.GetAttribute("text");
                listBoxMapOverlaysList.Items.Add(text);
            }
        }

        private void deleteMapOverlaysList_Click(object sender, EventArgs e)
        {
            // Get the selected item
            string selectedText = (string)listBoxMapOverlaysList.SelectedItem;

            // Load the XML document
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Options.AppDataPath, "MapOverlays.xml"));

            // Find the marker with the selected text and remove it
            XmlElement markerToRemove = null;
            foreach (XmlElement markerElement in doc.DocumentElement.SelectNodes("Marker"))
            {
                if (markerElement.GetAttribute("text") == selectedText)
                {
                    markerToRemove = markerElement;
                    break;
                }
            }

            // If a marker was found, remove it from the XML document and the ListBox
            if (markerToRemove != null)
            {
                doc.DocumentElement.RemoveChild(markerToRemove);
                listBoxMapOverlaysList.Items.Remove(selectedText);

                // Save the changes to the XML document
                doc.Save(Path.Combine(Options.AppDataPath, "MapOverlays.xml"));

                // Call LoadMapOverlays after saving the changes
                LoadMapOverlays();

                // Call LoadMapOverlays on the MapControl instance
                _mapControl.LoadMapOverlays();
            }
        }
    }
}
