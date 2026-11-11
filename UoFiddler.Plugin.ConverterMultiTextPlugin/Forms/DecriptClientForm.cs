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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class DecriptClientForm : Form
    {
        private byte[] bytes;
        private long FileSize;
        private string currentClientPath = string.Empty;

        // Patch-Ergebnisse speichern
        private bool patchLoginOk;
        private bool patchBlowfishOk;
        private bool patchTwofishOk;
        private bool patchMd5Ok;
        private bool patchMultiClientOk;

        public DecriptClientForm()
        {
            InitializeComponent();
            ResetPatchLabels();
            Log("Bereit. Bitte Client.exe auswählen oder per Drag & Drop hierher ziehen.");
        }

        #region UI Events

        private void btnSelectClient_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Ultima Online Client auswählen";
                ofd.Filter = "Executable (*.exe)|*.exe|Alle Dateien (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    SetClientPath(ofd.FileName);
                }
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentClientPath) || !File.Exists(currentClientPath))
            {
                MessageBox.Show("Bitte zuerst eine gültige Client.exe auswählen.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnDecrypt.Enabled = false;
            btnSelectClient.Enabled = false;
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Continuous;

            try
            {
                ReadAndProcessClient(currentClientPath);
            }
            catch (Exception ex)
            {
                Log("KRITISCHER FEHLER: " + ex.Message);
                SetStatus("Fehler aufgetreten!", Color.Red);
            }
            finally
            {
                btnDecrypt.Enabled = true;
                btnSelectClient.Enabled = true;
                progressBar.Value = 100;
            }
        }

        private void txtClientPath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void txtClientPath_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0 && files[0].EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                SetClientPath(files[0]);
            }
            else
            {
                Log("Nur .exe Dateien werden unterstützt.");
            }
        }

        #endregion

        #region Core Logic

        private void SetClientPath(string path)
        {
            currentClientPath = path;
            txtClientPath.Text = path;
            btnDecrypt.Enabled = true;
            Log("Client geladen: " + Path.GetFileName(path));
            SetStatus("Client bereit", Color.DarkGreen);
            ResetPatchLabels();
        }

        private void ReadAndProcessClient(string clientFilePath)
        {
            SetStatus("Lese Client...", Color.DarkOrange);
            Log("================================================");
            Log("Starte Verarbeitung: " + Path.GetFileName(clientFilePath));
            progressBar.Value = 10;

            using (FileStream fStream = File.OpenRead(clientFilePath))
            {
                FileSize = fStream.Length;
                bytes = new byte[FileSize];
                fStream.Read(bytes, 0, (int)FileSize);
            }

            Log($"Dateigröße: {FileSize:N0} Bytes");
            progressBar.Value = 20;

            // Version erkennen
            DetectClientVersion();
            progressBar.Value = 30;

            // Encryption entfernen (Login + Game)
            if (chkLogin.Checked || chkBlowfish.Checked || chkTwofish.Checked || chkMD5.Checked)
            {
                SetStatus("Entferne Encryption...", Color.DarkOrange);
                RemoveEncryption(bytes, FileSize);
            }
            else
            {
                Log("Encryption-Patches übersprungen (alle deaktiviert).");
            }
            progressBar.Value = 70;

            // Multi-Client Patch
            if (chkMultiClient.Checked)
            {
                SetStatus("Multi-Client Patch...", Color.DarkOrange);
                MultiClientPatch();
            }
            else
            {
                Log("Multi-Client Patch übersprungen.");
                UpdatePatchLabel(lblMultiClient, "Übersprungen", Color.Gray);
            }
            progressBar.Value = 85;

            // Datei speichern
            string outputPath = Path.Combine(
                Path.GetDirectoryName(clientFilePath),
                "Decrypted_client.exe");

            SetStatus("Schreibe Datei...", Color.DarkOrange);
            using (FileStream foStream = File.OpenWrite(outputPath))
            {
                foStream.Write(bytes, 0, bytes.Length);
            }

            Log("------------------------------------------------");
            Log("Fertig! Gespeichert als: " + outputPath);
            SetStatus("Decrypted_client.exe erstellt", Color.DarkGreen);
            progressBar.Value = 100;

            if (chkAutoOpenFolder.Checked)
            {
                try
                {
                    Process.Start("explorer.exe", "/select,\"" + outputPath + "\"");
                }
                catch { }
            }
        }

        private void DetectClientVersion()
        {
            byte[] MessageSig = { 0xA9, 0x20, 0x32, 0x30, 0x30, 0x36, 0x20, 0x45, 0x6C, 0x65,
                                  0x63, 0x74, 0x72, 0x6F, 0x6E, 0x69, 0x63, 0x20, 0x41, 0x72,
                                  0x74, 0x73, 0x20, 0x49, 0x6E, 0x63, 0x2E, 0x20, 0x20, 0x41,
                                  0x6C, 0x6C, 0x20, 0x52, 0x69, 0x67, 0x68, 0x74, 0x73, 0x20,
                                  0x52, 0x65, 0x73, 0x65, 0x72, 0x76, 0x65, 0x64, 0x2E };

            byte[] MessageSigOld = { 0xA9, 0x20, 0x32, 0x30, 0x30, 0x35, 0x20, 0x45, 0x6C, 0x65,
                                     0x63, 0x74, 0x72, 0x6F, 0x6E, 0x69, 0x63, 0x20, 0x41, 0x72,
                                     0x74, 0x73, 0x20, 0x49, 0x6E, 0x63, 0x2E, 0x20, 0x20, 0x41,
                                     0x6C, 0x6C, 0x20, 0x52, 0x69, 0x67, 0x68, 0x74, 0x73, 0x20,
                                     0x52, 0x65, 0x73, 0x65, 0x72, 0x76, 0x65, 0x64, 0x2E };

            byte[] MessageSig09 = { 0xA9, 0x20, 0x32, 0x30, 0x30, 0x39, 0x20, 0x45, 0x6C, 0x65,
                                    0x63, 0x74, 0x72, 0x6F, 0x6E, 0x69, 0x63, 0x20, 0x41, 0x72,
                                    0x74, 0x73, 0x20, 0x49, 0x6E, 0x63, 0x2E, 0x20, 0x20, 0x41,
                                    0x6C, 0x6C, 0x20, 0x52, 0x69, 0x67, 0x68, 0x74, 0x73, 0x20,
                                    0x52, 0x65, 0x73, 0x65, 0x72, 0x76, 0x65, 0x64, 0x2E };

            int pos2006 = ScanClient(0x100, MessageSig, bytes, FileSize, MessageSig.Length, 0);
            int pos2005 = ScanClient(0x100, MessageSigOld, bytes, FileSize, MessageSigOld.Length, 0);
            int pos2009 = ScanClient(0x100, MessageSig09, bytes, FileSize, MessageSig09.Length, 0);

            string version = "Unbekannt";
            if (pos2009 != -1) version = "2009+";
            else if (pos2006 != -1) version = "2006";
            else if (pos2005 != -1) version = "2005";

            Log($"Erkannte Client-Version: {version}");
            if (pos2005 != -1) Log($"  → 2005 Signatur @ 0x{pos2005:X}");
            if (pos2006 != -1) Log($"  → 2006 Signatur @ 0x{pos2006:X}");
            if (pos2009 != -1) Log($"  → 2009 Signatur @ 0x{pos2009:X}");
        }

        #endregion

        #region Encryption Removal (erweitert mit Reporting)

        private void RemoveEncryption(byte[] InClient, long InClientLength)
        {
            // ========== LOGIN ENCRYPTION ==========
            if (chkLogin.Checked)
            {
                byte[] CryptSig = { 0x81, 0xF9, 0x00, 0x00, 0x01, 0x00, 0x0F, 0x8F };
                byte[] CryptSigNew = { 0x00, 0x00, 0x00, 0x00, 0x75, 0x12, 0x8b, 0x54 };
                byte[] JNZSig = { 0x0F, 0x85 };
                byte[] JNESig = { 0x0F, 0x84 };

                int CryptPos = ScanClient(0x100, CryptSig, InClient, InClientLength, 8, 0);
                int CryptPosNew = -1;
                if (CryptPos == -1)
                    CryptPosNew = ScanClient(0x100, CryptSigNew, InClient, InClientLength, 8, 0);

                if (CryptPos != -1)
                {
                    int JNZPos = ScanClient(0x100, JNZSig, InClient, InClientLength, 2, CryptPos);
                    int JEPos = ScanClient(0x100, JNESig, InClient, InClientLength, 2, CryptPos);

                    if (JEPos > JNZPos && JNZPos != -1)
                    {
                        bytes[JNZPos + 1] = 0x84;
                        Log($"Login Encryption gepatcht (JE) @ 0x{JNZPos:X}");
                        patchLoginOk = true;
                    }
                    else if (JNZPos > JEPos && JEPos != -1)
                    {
                        bytes[JEPos + 1] = 0x85;
                        Log($"Login Encryption gepatcht (JNZ) @ 0x{JEPos:X}");
                        patchLoginOk = true;
                    }
                }
                else if (CryptPosNew != -1)
                {
                    bytes[CryptPosNew + 4] = 0xEB;
                    Log($"Login Encryption (neu) gepatcht @ 0x{(CryptPosNew + 4):X}");
                    patchLoginOk = true;
                }
                else
                {
                    Log("Login Encryption Signatur nicht gefunden.");
                    patchLoginOk = false;
                }

                UpdatePatchLabel(lblLogin, patchLoginOk ? "OK" : "Nicht gefunden", patchLoginOk ? Color.DarkGreen : Color.Red);
            }
            else
            {
                UpdatePatchLabel(lblLogin, "Übersprungen", Color.Gray);
            }

            // ========== BLOWFISH ==========
            if (chkBlowfish.Checked)
            {
                byte[] BFGamecryptSig = { 0x2C, 0x52, 0x00, 0x00, 0x76 };
                byte[] CmpSig = { 0x3B, 0xC3, 0x0F, 0x84 };

                int BFGamecryptPos = ScanClient(0x100, BFGamecryptSig, InClient, InClientLength, 5, 0);
                int CmpPos = -1;

                if (BFGamecryptPos != -1)
                    CmpPos = ScanClient(0x100, CmpSig, InClient, InClientLength, 4, BFGamecryptPos - 0x20);

                if (BFGamecryptPos != -1 && CmpPos != -1)
                {
                    bytes[CmpPos + 1] = 0xC0;
                    Log($"Blowfish gepatcht @ 0x{CmpPos:X}");
                    patchBlowfishOk = true;
                }
                else
                {
                    Log("Blowfish Signatur nicht gefunden.");
                    patchBlowfishOk = false;
                }

                UpdatePatchLabel(lblBlowfish, patchBlowfishOk ? "OK" : "Nicht gefunden", patchBlowfishOk ? Color.DarkGreen : Color.Red);
            }
            else
            {
                UpdatePatchLabel(lblBlowfish, "Übersprungen", Color.Gray);
            }

            // ========== TWOFISH ==========
            if (chkTwofish.Checked)
            {
                byte[] TFGamecryptSig = { 0x8B, 0x8B, 0xCC, 0xCC, 0xCC, 0xCC, 0x81, 0xF9, 0x00, 0x01, 0x00, 0x00, 0x74, 0x10 };
                byte[] TFGamecryptSigNew = { 0x74, 0x0f, 0x83, 0xb9, 0xb4, 0x00, 0x00, 0x00, 0x00 };
                byte[] JELong = { 0x0F, 0x84, 0xCC, 0x00, 0x00, 0x00, 0x55 };

                int TFGamecryptPos = ScanClient(0xCC, TFGamecryptSig, InClient, InClientLength, 14, 0);
                int GJEPos = -1;
                if (TFGamecryptPos != -1)
                    GJEPos = ScanClient(0xCC, JELong, InClient, InClientLength, 7, TFGamecryptPos - 0x20);

                int TFGamecryptPosNew = ScanClient(0xCC, TFGamecryptSigNew, InClient, InClientLength, 9, 0);

                if (TFGamecryptPos != -1 && GJEPos != -1)
                {
                    bytes[GJEPos + 1] = 0x85;
                    Log($"Twofish (alt) gepatcht @ 0x{(GJEPos + 1):X}");
                    patchTwofishOk = true;
                }
                else if (TFGamecryptPosNew != -1)
                {
                    bytes[TFGamecryptPosNew] = 0xEB;
                    Log($"Twofish (neu) gepatcht @ 0x{TFGamecryptPosNew:X}");
                    patchTwofishOk = true;
                }
                else
                {
                    Log("Twofish Signatur nicht gefunden.");
                    patchTwofishOk = false;
                }

                UpdatePatchLabel(lblTwofish, patchTwofishOk ? "OK" : "Nicht gefunden", patchTwofishOk ? Color.DarkGreen : Color.Red);
            }
            else
            {
                UpdatePatchLabel(lblTwofish, "Übersprungen", Color.Gray);
            }

            // ========== MD5 DECRYPT ==========
            if (chkMD5.Checked)
            {
                byte[] DecryptSig = { 0x4A, 0x83, 0xCA, 0xF0, 0x42, 0x8A, 0x94, 0x32 };
                byte[] DectestSig = { 0x85, 0xCC, 0x74, 0xCC, 0x33, 0xCC, 0x85, 0xCC, 0x7E, 0xCC };
                byte[] DecryptSigNew = { 0x00, 0x00, 0x74, 0x37, 0x83, 0xbe, 0xb4, 0x00, 0x00, 0x00, 0x00 };

                int DecryptPos = ScanClient(0x100, DecryptSig, InClient, InClientLength, 8, 0);
                int DecryptPosNew = ScanClient(0x100, DecryptSigNew, InClient, InClientLength, 11, 0);
                int DectestPos = -1;

                if (DecryptPos != -1)
                    DectestPos = ScanClient(0xCC, DectestSig, InClient, InClientLength, 10, DecryptPos - 0x100);

                if (DecryptPos != -1 && DectestPos != -1)
                {
                    bytes[DectestPos] = 0x3B;
                    Log($"MD5 Decrypt (alt) gepatcht @ 0x{DectestPos:X}");
                    patchMd5Ok = true;
                }
                else if (DecryptPosNew != -1)
                {
                    bytes[DecryptPosNew + 2] = 0xEB;
                    Log($"MD5 Decrypt (neu) gepatcht @ 0x{(DecryptPosNew + 2):X}");
                    patchMd5Ok = true;
                }
                else
                {
                    Log("MD5 Decrypt Signatur nicht gefunden.");
                    patchMd5Ok = false;
                }

                UpdatePatchLabel(lblMD5, patchMd5Ok ? "OK" : "Nicht gefunden", patchMd5Ok ? Color.DarkGreen : Color.Red);
            }
            else
            {
                UpdatePatchLabel(lblMD5, "Übersprungen", Color.Gray);
            }

            Log("Encryption-Phase abgeschlossen.");
        }

        #endregion

        #region Multi Client Patch

        private void MultiClientPatch()
        {
            bool triple = TripleCheckPatch(bytes);
            bool single = SingleCheckPatch(bytes);
            bool error = ErrorCheckPatch(bytes);

            patchMultiClientOk = triple && single && error;

            if (patchMultiClientOk)
            {
                Log("Multi-Client Patch erfolgreich (Triple + Single + Error Check).");
                UpdatePatchLabel(lblMultiClient, "OK", Color.DarkGreen);
            }
            else
            {
                Log("Multi-Client Patch unvollständig:");
                if (!triple) Log("  → TripleCheck fehlgeschlagen");
                if (!single) Log("  → SingleCheck fehlgeschlagen");
                if (!error) Log("  → ErrorCheck fehlgeschlagen");
                UpdatePatchLabel(lblMultiClient, "Teilweise / Fehlgeschlagen", Color.Red);
            }
        }

        private static bool FindSignatureOffset(byte[] signature, byte[] buffer, out int offset)
        {
            offset = 0;
            for (int x = 0; x < buffer.Length - signature.Length; x++)
            {
                bool found = true;
                for (int y = 0; y < signature.Length; y++)
                {
                    if (buffer[x + y] != signature[y])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    offset = x;
                    return true;
                }
            }
            return false;
        }

        private static bool ErrorCheckPatch(byte[] fileBuffer)
        {
            byte[] oldClientSig = { 0x85, 0xC0, 0x75, 0x2F, 0xBF };
            byte[] newClientSig = { 0x85, 0xC0, 0x5F, 0x5E, 0x75, 0x2F };
            int offset;

            if (FindSignatureOffset(oldClientSig, fileBuffer, out offset))
            {
                fileBuffer[offset] = 0x66;
                fileBuffer[offset + 1] = 0x33;
                fileBuffer[offset + 2] = 0xC0;
                fileBuffer[offset + 3] = 0x90;
                return true;
            }

            if (FindSignatureOffset(newClientSig, fileBuffer, out offset))
            {
                fileBuffer[offset + 4] = 0x90;
                fileBuffer[offset + 5] = 0x90;
                return true;
            }
            return false;
        }

        private static bool SingleCheckPatch(byte[] fileBuffer)
        {
            byte[] oldClientSig = { 0xC7, 0x44, 0x24, 0x10, 0x11, 0x01, 0x00, 0x00 };
            byte[] newClientSig = { 0x83, 0xC4, 0x04, 0x33, 0xDB, 0x53, 0x50 };
            int offset;

            if (FindSignatureOffset(oldClientSig, fileBuffer, out offset))
            {
                if (fileBuffer[offset + 0x17] == 0x74)
                {
                    fileBuffer[offset + 0x17] = 0xEB;
                    return true;
                }
                return false;
            }

            if (FindSignatureOffset(newClientSig, fileBuffer, out offset))
            {
                if (fileBuffer[offset + 0x0F] == 0x74)
                {
                    fileBuffer[offset + 0x0F] = 0xEB;
                    return true;
                }
                return false;
            }
            return false;
        }

        private static bool TripleCheckPatch(byte[] fileBuffer)
        {
            byte[] oldClientSig = { 0xFF, 0xD6, 0x6A, 0x01, 0xFF, 0xD7, 0x68 };
            byte[] newClientSig = { 0x3B, 0xC3, 0x89, 0x44, 0x24, 0x08 };
            int offset;

            if (FindSignatureOffset(oldClientSig, fileBuffer, out offset))
            {
                if (fileBuffer[offset - 0x2D] == 0x75 &&
                    fileBuffer[offset - 0x0E] == 0x75 &&
                    fileBuffer[offset + 0x18] == 0x74)
                {
                    fileBuffer[offset - 0x2D] = 0xEB;
                    fileBuffer[offset - 0x0E] = 0xEB;
                    fileBuffer[offset + 0x18] = 0xEB;
                    return true;
                }
                return false;
            }

            if (FindSignatureOffset(newClientSig, fileBuffer, out offset))
            {
                if (fileBuffer[offset + 0x06] == 0x75 &&
                    fileBuffer[offset + 0x2D] == 0x75 &&
                    fileBuffer[offset + 0x5F] == 0x74)
                {
                    fileBuffer[offset + 0x06] = 0xEB;
                    fileBuffer[offset + 0x2D] = 0xEB;
                    fileBuffer[offset + 0x5F] = 0xEB;
                    return true;
                }
                return false;
            }
            return false;
        }

        #endregion

        #region Helper Methods

        private int ScanClient(int FlexByte, byte[] signature, byte[] client, long client_length, int sig_length, int startat)
        {
            bool UseFlex = (FlexByte & 100) != 0;
            byte FByte = (byte)(FlexByte & 255);

            if (startat < 0) startat = 0;

            for (int i = startat; i < client_length - sig_length; i++)
            {
                for (int j = 0; j < sig_length; j++)
                {
                    if (UseFlex && signature[j] != FByte && signature[j] != client[i + j])
                        break;
                    else if (!UseFlex && signature[j] != client[i + j])
                        break;

                    if (j == sig_length - 1)
                        return i;
                }
            }
            return -1;
        }

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(message)));
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void SetStatus(string text, Color color)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => SetStatus(text, color)));
                return;
            }

            lblStatus.Text = "Status: " + text;
            lblStatus.ForeColor = color;
        }

        private void UpdatePatchLabel(Label lbl, string text, Color color)
        {
            if (lbl.InvokeRequired)
            {
                lbl.Invoke(new Action(() => UpdatePatchLabel(lbl, text, color)));
                return;
            }

            // Label-Text je nach Control anpassen
            if (lbl == lblLogin) lbl.Text = "Login Encryption: " + text;
            else if (lbl == lblBlowfish) lbl.Text = "Blowfish: " + text;
            else if (lbl == lblTwofish) lbl.Text = "Twofish: " + text;
            else if (lbl == lblMD5) lbl.Text = "MD5 Decrypt: " + text;
            else if (lbl == lblMultiClient) lbl.Text = "Multi-Client: " + text;

            lbl.ForeColor = color;
        }

        private void ResetPatchLabels()
        {
            UpdatePatchLabel(lblLogin, "-", Color.Gray);
            UpdatePatchLabel(lblBlowfish, "-", Color.Gray);
            UpdatePatchLabel(lblTwofish, "-", Color.Gray);
            UpdatePatchLabel(lblMD5, "-", Color.Gray);
            UpdatePatchLabel(lblMultiClient, "-", Color.Gray);

            patchLoginOk = patchBlowfishOk = patchTwofishOk = patchMd5Ok = patchMultiClientOk = false;
        }

        #endregion
    }
}