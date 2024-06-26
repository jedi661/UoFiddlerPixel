﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin
{
    public partial class AltitudeToolForm : Form
    {
        private string selectedDirectory; // Deklaration von selectedDirectory
        private Process externalProcess;

        public AltitudeToolForm()
        {
            InitializeComponent();
        }

        #region FormClosin
        private void AltitudeToolForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Beende den externen Prozess, wenn das Formular geschlossen wird
            if (externalProcess != null && !externalProcess.HasExited)
            {
                externalProcess.Kill();
            }
        }
        #endregion

        #region StartExternalProcess
        public void StartExternalProcess(string fileName)
        {
            externalProcess = new Process();
            externalProcess.StartInfo.FileName = fileName;
            externalProcess.Start();
        }
        #endregion


        #region Dir
        private void BtSelectDirectory_Click(object sender, EventArgs e)
        {
            using (folderBrowserDialog1 = new FolderBrowserDialog())
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    selectedDirectory = folderBrowserDialog1.SelectedPath;

                    // Setzen Sie den Text von textBoxOutput und labelDir auf den ausgewählten Pfad
                    textBoxOutput.Text = selectedDirectory;
                    labelDir.Text = selectedDirectory;
                }
            }
        }
        #endregion
        #region Button Start
        private async void BtStartCommand_Click(object sender, EventArgs e)
        {
            string command;

            // Überprüfen, ob der Benutzer einen manuellen Befehl eingegeben hat
            if (!string.IsNullOrWhiteSpace(textBoxManuel.Text))
            {
                // Wenn ja, verwenden Sie den manuellen Befehl
                command = textBoxManuel.Text;
            }
            else if (comboBoxCommand.SelectedItem != null)
            {
                // Andernfalls, wenn ein Element in der ComboBox ausgewählt wurde, verwenden Sie es
                command = comboBoxCommand.SelectedItem.ToString();
            }
            else
            {
                // Wenn weder ein manueller Befehl noch ein ComboBox-Element ausgewählt wurde, zeigen Sie eine Meldung an und beenden Sie die Methode
                MessageBox.Show("Bitte wählen Sie ein Element aus der ComboBox aus oder geben Sie einen manuellen Befehl ein.");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = selectedDirectory + "\\AltitudeTool.exe",
                Arguments = command, // Verwenden Sie den ausgewählten oder manuell eingegebenen Befehl
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = selectedDirectory
            };

            var process = Process.Start(psi);

            if (process != null)
            {
                string result = await process.StandardOutput.ReadToEndAsync();
                textBoxOutput.Text = result;
            }
        }
        #endregion
        #region Button Copy
        private void btCopyMul_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Alle Dateien|*.*"; // Filter für alle Dateitypen
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string sourceFilePath = openFileDialog.FileName; // Der ausgewählte Dateipfad

                    // Das Verzeichnis, in dem sich Ihre ausführbare Datei (AltitudeTool.exe) befindet
                    string executableDirectory = Path.GetDirectoryName(Application.ExecutablePath);

                    // Das Zielverzeichnis, in das die Datei kopiert werden soll
                    string destinationDirectory = Path.Combine(executableDirectory, "Tools", "AltitudeTool");

                    // Erstellen Sie das Zielverzeichnis, wenn es nicht existiert
                    Directory.CreateDirectory(destinationDirectory);

                    // Der Ziel-Dateipfad: Kombinieren Sie das Zielverzeichnis mit dem Dateinamen der Quelldatei
                    string destinationFilePath = Path.Combine(destinationDirectory, Path.GetFileName(sourceFilePath));

                    // Wenn die Quelldatei existiert, kopieren Sie sie ins Zielverzeichnis
                    if (File.Exists(sourceFilePath))
                    {
                        try
                        {
                            File.Copy(sourceFilePath, destinationFilePath, true);
                            MessageBox.Show("Datei erfolgreich kopiert.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Fehler beim Kopieren der Datei: {ex.Message}");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Die ausgewählte Quelldatei existiert nicht.");
                    }
                }
            }
        }
        #endregion
        #region CheckBox
        private void checkBoxCopyText_CheckedChanged(object sender, EventArgs e)
        {
            // Überprüfen, ob die CheckBox aktiviert ist
            if (checkBoxCopyText.Checked && comboBoxCommand.SelectedItem != null)
            {
                // Wenn ja, kopieren Sie den ausgewählten Text in die textBoxManuel
                textBoxManuel.Text = comboBoxCommand.SelectedItem.ToString();
            }
            else
            {
                // Wenn die CheckBox nicht aktiviert ist oder kein Element ausgewählt ist, leeren Sie die textBoxManuel
                textBoxManuel.Clear();
            }
        }
        #endregion
        #region Help
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            // Text für die verschiedenen Befehle und ihre Verwendung
            string helpText = "Befehle und ihre Verwendung:" + Environment.NewLine +
                              "  extract <mapX.mul> <mapheight> <heightmap.bmp>" + Environment.NewLine +
                              "      generiert <heightmap.bmp> aus <mapX.mul>, welches" + Environment.NewLine +
                              "      <mapheight> Blöcke hoch ist." + Environment.NewLine +
                              Environment.NewLine +
                              "  apply <heightmap.bmp> <mapX.mul> <mapheight> [<staticsX.mul> <staidxX.mul>]" + Environment.NewLine +
                              "      wendet die Höhe von <heightmap.bmp> auf <mapX.mul> an, welches" + Environment.NewLine +
                              "      <mapheight> Blöcke hoch ist, und passt optional auch die Statics an." + Environment.NewLine +
                              Environment.NewLine +
                              "  modify <mapX.mul> <mapheight> <offset> [<staticsX.mul> <staidxX.mul>]" + Environment.NewLine +
                              "      erhöht/verringert die Höhe jeder Kachel in <mapX.mul> mit" + Environment.NewLine +
                              "      <mapheight> Höhe um <offset>. Optional wird der Offset auch auf die Statics angewendet.";

            // Den Text in die textBoxOutput einblenden
            textBoxOutput.Text = helpText;
        }
        #endregion
    }
}
