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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class DecriptClientForm : Form
    {
        private static string m_ClientFileLocation = AppDomain.CurrentDomain.BaseDirectory; // App's working directory
        string CLIENT = "client.exe";  // Name of un-decrypted client file
        private static byte[] bytes;    // Byte array client's read into
        public static long FileSize;    // Length of the client read into the byte arra

        public DecriptClientForm()
        {
            InitializeComponent();
        }

        // Read in the client file
        private void ReadClientFile(string clientFilePath)
        {
            LAB_StatusIS.Text = "Reading Client...";

            try
            {
                // Die Verwendung von clientFilePath anstelle von "client.exe"
                using (FileStream fStream = File.OpenRead(clientFilePath))
                {
                    FileSize = fStream.Length;
                    bytes = new byte[FileSize];

                    fStream.Read(bytes, 0, bytes.Length);
                    fStream.Close();

                    LAB_StatusIS.Text = "Removing Encryption...";

                    RemoveEncryption(bytes, FileSize);

                    LAB_StatusIS.Text = "Patching Multi Client Stuff...";
                    MultiClientPatch();

                    // Verwendung von Path.GetDirectoryName(clientFilePath)
                    string decryptedClientFilePath = Path.Combine(Path.GetDirectoryName(clientFilePath), "Decrypted_client.exe");

                    LAB_StatusIS.Text = "Writing new client file...";
                    using (FileStream foStream = File.OpenWrite(decryptedClientFilePath))
                    {
                        foStream.Write(bytes, 0, bytes.Length);
                    }

                    LAB_StatusIS.Text = "Decrypted Client.exe Created.";
                }
            }
            catch (IOException)
            {
                LAB_StatusIS.Text = "File I/O ERROR !!!";
            }
        }

        // Crypt Stuff starts from here...
        #region Multi Client Patch

        private static bool FindSignatureOffset(byte[] signature, byte[] buffer, out int offset)
        {
            bool found = false;
            offset = 0;
            for (int x = 0; x < buffer.Length - signature.Length; x++)
            {
                for (int y = 0; y < signature.Length; y++)
                {
                    if (buffer[x + y] == signature[y])
                        found = true;
                    else
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    offset = x;
                    break;
                }
            }
            return found;
        }

        private static bool ErrorCheckPatch(byte[] fileBuffer)
        {
            /* Patches the following check:
	     * GetLastError returns non-zero */

            byte[] oldClientSig = new byte[] { 0x85, 0xC0, 0x75, 0x2F, 0xBF };
            byte[] newClientSig = new byte[] { 0x85, 0xC0, 0x5F, 0x5E, 0x75, 0x2F };
            int offset;

            if (FindSignatureOffset(oldClientSig, fileBuffer, out offset)) //signature = target bytes, so no check necessary
            {
                //XOR AX, AX
                fileBuffer[offset] = 0x66;
                fileBuffer[offset + 1] = 0x33;
                fileBuffer[offset + 2] = 0xC0;
                fileBuffer[offset + 3] = 0x90;
                return true;
            }

            if (FindSignatureOffset(newClientSig, fileBuffer, out offset)) //signature = target bytes, so no check necessary
            {
                fileBuffer[offset + 4] = 0x90;
                fileBuffer[offset + 5] = 0x90;
                return true;
            }

            return false;
        }

        private static bool SingleCheckPatch(byte[] fileBuffer)
        {
            /* Patches the following check:
	     * "Another copy of UO is already running!" */

            byte[] oldClientSig = new byte[] { 0xC7, 0x44, 0x24, 0x10, 0x11, 0x01, 0x00, 0x00 };
            byte[] newClientSig = new byte[] { 0x83, 0xC4, 0x04, 0x33, 0xDB, 0x53, 0x50 };
            int offset;

            if (FindSignatureOffset(oldClientSig, fileBuffer, out offset))
            {
                if (fileBuffer[offset + 0x17] == 0x74)
                {
                    fileBuffer[offset + 0x17] = 0xEB;
                    return true;
                }
                else
                {
                    // Single check signature found, but actual byte differs from expected.  Aborting.
                    return false;
                }
            }

            if (FindSignatureOffset(newClientSig, fileBuffer, out offset))
            {
                if (fileBuffer[offset + 0x0F] == 0x74)
                {
                    fileBuffer[offset + 0x0F] = 0xEB;
                    return true;
                }
                else
                {
                    // Single check signature found, but actual byte differs from expected.  Aborting.
                    return false;
                }
            }

            return false;
        }

        private static bool TripleCheckPatch(byte[] fileBuffer)
        {
            /* Patches following checks:
	     * "Another instance of UO may already be running."
	     * "Another instance of UO is already running."
	     * "An instance of UO Patch is already running." */

            byte[] oldClientSig = new byte[] { 0xFF, 0xD6, 0x6A, 0x01, 0xFF, 0xD7, 0x68 };
            byte[] newClientSig = new byte[] { 0x3B, 0xC3, 0x89, 0x44, 0x24, 0x08 };
            int offset;

            if (FindSignatureOffset(oldClientSig, fileBuffer, out offset))
            {
                if (fileBuffer[offset - 0x2D] == 0x75 && fileBuffer[offset - 0x0E] == 0x75 && fileBuffer[offset + 0x18] == 0x74)
                {
                    fileBuffer[offset - 0x2D] = 0xEB;
                    fileBuffer[offset - 0x0E] = 0xEB;
                    fileBuffer[offset + 0x18] = 0xEB;
                    return true;
                }
                else
                {
                    // Triple check signature found, but actual bytes differ from expected.  Aborting.
                    return false;
                }
            }

            if (FindSignatureOffset(newClientSig, fileBuffer, out offset))
            {
                if (fileBuffer[offset + 0x06] == 0x75 && fileBuffer[offset + 0x2D] == 0x75 && fileBuffer[offset + 0x5F] == 0x74)
                {
                    fileBuffer[offset + 0x06] = 0xEB;
                    fileBuffer[offset + 0x2D] = 0xEB;
                    fileBuffer[offset + 0x5F] = 0xEB;
                    return true;
                }
                else
                {
                    // Triple check signature found, but actual bytes differ from expected.  Aborting.
                    return false;
                }
            }

            return false;
        }

        private void MultiClientPatch()
        {
            if (!TripleCheckPatch(bytes))
            {
                // Triple check signature not found. Aborting.
                return;
            }
            if (!SingleCheckPatch(bytes))
            {
                // Single check signature not found. Aborting.
                return;
            }
            if (!ErrorCheckPatch(bytes))
            {
                // Error check signature not found. Aborting.
                return;
            }

            this.LAB_StatusIS.Text = "Multi Client Patching...Done";
        }

        #endregion

        private void RemoveEncryption(byte[] InClient, long InClientLength)
        {
            // These are the Signatures to search for LOGIN ENCRYPTIONS
            byte[] CryptSig = { 0x81, 0xF9, 0x00, 0x00, 0x01, 0x00, 0x0F, 0x8F };
            byte[] CryptSigNew = { 0x00, 0x00, 0x00, 0x00, 0x75, 0x12, 0x8b, 0x54 };
            byte[] JNZSig = { 0x0F, 0x85 };
            byte[] JNESig = { 0x0F, 0x84 };

            int CryptPos = -1, CryptPosNew = -1, JNZPos = -1, JEPos = -1, NewClient = -1;

            /* for game encryption */
            byte[] BFGamecryptSig = { 0x2C, 0x52, 0x00, 0x00, 0x76 }; /* CMP XXX, 522c - JBE */
            byte[] CmpSig = { 0x3B, 0xC3, 0x0F, 0x84 }; /* CMP EAX,EBX - JE */
            int BFGamecryptPos = -1, CmpPos = -1;

            /* RTD: make sure the JE 0x10 and JE 0x9X000000 stays like that, if not... I have to find a new way */
            byte[] TFGamecryptSig = { 0x8B, 0x8B, 0xCC, 0xCC, 0xCC, 0xCC, 0x81, 0xF9, 0x00, 0x01, 0x00, 0x00, 0x74, 0x10 }; /* MOV EBX, XXX - CMP ECX, 0x100 - JE 0x10 */
            byte[] TFGamecryptSigNew = { 0x74, 0x0f, 0x83, 0xb9, 0xb4, 0x00, 0x00, 0x00, 0x00 };
            byte[] JELong = { 0x0F, 0x84, 0xCC, 0x00, 0x00, 0x00, 0x55 }; /* JE XX000000 -  */
            int TFGamecryptPos = -1, TFGamecryptPosNew = -1, GJEPos = -1;

            /* for game decryption */
            byte[] DecryptSig = { 0x4A, 0x83, 0xCA, 0xF0, 0x42, 0x8A, 0x94, 0x32 };
            byte[] DectestSig = { 0x85, 0xCC, 0x74, 0xCC, 0x33, 0xCC, 0x85, 0xCC, 0x7E, 0xCC };
            byte[] DecryptSigNew = { 0x00, 0x00, 0x74, 0x37, 0x83, 0xbe, 0xb4, 0x00, 0x00, 0x00, 0x00 };
            int DecryptPos = -1, DectestPos = -1, DecryptPosNew = -1;

            byte[] MessageSig = { 0xA9, 0x20, 0x32, 0x30, 0x30, 0x36, 0x20, 0x45, 0x6C, 0x65,
                  0x63, 0x74, 0x72, 0x6F, 0x6E, 0x69, 0x63, 0x20, 0x41, 0x72,
                  0x74, 0x73, 0x20, 0x49, 0x6E, 0x63, 0x2E, 0x20, 0x20, 0x41,
                  0x6C, 0x6C, 0x20, 0x52, 0x69, 0x67, 0x68, 0x74, 0x73, 0x20,
                  0x52, 0x65, 0x73, 0x65, 0x72, 0x76, 0x65, 0x64, 0x2E};

            byte[] MessageSigOld = { 0xA9, 0x20, 0x32, 0x30, 0x30, 0x35, 0x20, 0x45, 0x6C, 0x65,
                     0x63, 0x74, 0x72, 0x6F, 0x6E, 0x69, 0x63, 0x20, 0x41, 0x72,
                     0x74, 0x73, 0x20, 0x49, 0x6E, 0x63, 0x2E, 0x20, 0x20, 0x41,
                     0x6C, 0x6C, 0x20, 0x52, 0x69, 0x67, 0x68, 0x74, 0x73, 0x20,
                     0x52, 0x65, 0x73, 0x65, 0x72, 0x76, 0x65, 0x64, 0x2E};

            byte[] MessageSig09 = { 0xA9, 0x20, 0x32, 0x30, 0x30, 0x39, 0x20, 0x45, 0x6C, 0x65,
                    0x63, 0x74, 0x72, 0x6F, 0x6E, 0x69, 0x63, 0x20, 0x41, 0x72,
                    0x74, 0x73, 0x20, 0x49, 0x6E, 0x63, 0x2E, 0x20, 0x20, 0x41,
                    0x6C, 0x6C, 0x20, 0x52, 0x69, 0x67, 0x68, 0x74, 0x73, 0x20,
                    0x52, 0x65, 0x73, 0x65, 0x72, 0x76, 0x65, 0x64, 0x2E};

            int MessagePos = -1, MessagePosOld = -1, MessagePos09 = -1;

            // ***** Start to search for LOGIN ENCRYPTION... *****
            // magic x90 encryption signature: 81 f9 00 00 01 00 0f 8f
            // patching : find the first 0f 84 bellow and change it to 0f 85
            // or first 0f 85 and change it to 0f 84
            CryptPos = ScanClient(0x100, CryptSig, InClient, InClientLength, 8, 0);

            // If was not found, try looking for the new signature
            if (CryptPos == -1)
            {
                CryptPosNew = ScanClient(0x100, CryptSigNew, InClient, InClientLength, 8, 0);
            }

            if (CryptPos != -1 && CryptPosNew != -1)
            {
                this.LAB_StatusIS.Text = "Can't find a login signature in this file ???";
            }
            else
            {
                if (CryptPos != -1)
                {
                    JNZPos = ScanClient(0x100, JNZSig, InClient, InClientLength, 2, CryptPos);
                    JEPos = ScanClient(0x100, JNESig, InClient, InClientLength, 2, CryptPos);

                    if (JEPos > JNZPos)
                    {
                        bytes[JNZPos + 1] = 0x84;
                        this.LAB_StatusIS.Text = string.Format("Patching with JE (0x0F 0x84) - (15 132) @{0:X} - ({1})", JNZPos, JNZPos.ToString());
                    }
                    else if (JNZPos > JEPos)
                    {
                        bytes[JNZPos + 1] = 0x85;
                        this.LAB_StatusIS.Text = string.Format("Patching with JNZ (0x0F 0x85) - (15 133) @{0:X} - ({1})", JEPos, JEPos.ToString());
                    }
                }
                else if (CryptPosNew != -1)
                {
                    bytes[CryptPosNew + 4] = 0xEB;
                    this.LAB_StatusIS.Text = string.Format("Patching with (0xEB) - (235) @{0:X} - ({1})", (CryptPosNew + 4), (CryptPosNew + 4).ToString());
                    NewClient = 1;
                }
            }

            /*
	     * BLOWFISH
	     * this is simply "lost" inside the "send" function, just above it
	     * it looks like:
	     * if(GameMode != LOGIN_SOCKET) ;game socket
	     * {
	     *    BlowfishEncrypt() ;starts with a (while > 0x522c)
	     *    ;a whole bunch of other stuff
	     *    TwofishEncrypt() ;if present
	     *    send()
	     * }
	     * else ;login socket
	     *    send() ;yay, a send that bypasses all the encryption crap
	     *
	     * find the beginning of the loop while(Obj->stream_pos + len > CRYPT_GAMETABLE_TRIGGER)
	     * CRYPT_GAMETABLE_TRIGGER is 0x522c
	     * find the first CMP EAX,EBX-JE above and patch it to CMP EAX,EAX
	     */

            BFGamecryptPos = ScanClient(0x100, BFGamecryptSig, InClient, InClientLength, 5, 0);
            // FIX ? PROBABLY NEED TO REMOVE
            if (BFGamecryptPos != -1)
            {
                CmpPos = ScanClient(0x100, CmpSig, InClient, InClientLength, 4, BFGamecryptPos - 0x20);
            }

            if (BFGamecryptPos == -1 || CmpPos == -1)
            {
                this.LAB_StatusIS.Text = "Can't find the blowfish signature";
            }
            else
            {
                bytes[CmpPos + 1] = 0xC0;
                this.LAB_StatusIS.Text = string.Format("Patching with CMP (0xC0) - (192) @{0:X} - ({1})", CmpPos, CmpPos.ToString());
            }

            /*
	     * TWOFISH
	     * patch the encryption function to skip encryption
	     * the function is always called before send
	     *
	     * find the beginning of the loop and the first JE above it
	     * patch the JE (0x84) to JNE (0x85)
	     */

            TFGamecryptPos = ScanClient(0xCC, TFGamecryptSig, InClient, InClientLength, 14, 0);
            if (TFGamecryptPos != -1)
            {
                GJEPos = ScanClient(0xCC, JELong, InClient, InClientLength, 7, TFGamecryptPos - 0x20);
            }

            TFGamecryptPosNew = ScanClient(0xCC, TFGamecryptSigNew, InClient, InClientLength, 9, 0);

            if (TFGamecryptPos == -1 && GJEPos == -1 && TFGamecryptPosNew == -1)
            {
                this.LAB_StatusIS.Text = "Can't find old OR new Twofish signatures";
            }
            else
            {
                if (TFGamecryptPos != -1 && GJEPos != -1)
                {
                    bytes[GJEPos + 1] = 0x85;
                    this.LAB_StatusIS.Text = string.Format("Patching (old TF) with JNZ (0x85) - (133) @{0:X} - ({1})", (GJEPos + 1), (GJEPos + 1).ToString());
                }
                else if (TFGamecryptPosNew != -1)
                {
                    bytes[TFGamecryptPosNew] = 0xEB;
                    this.LAB_StatusIS.Text = string.Format("Patching (new TF) with (0xEB) - (235) @{0:X} - ({1})", TFGamecryptPosNew, TFGamecryptPosNew.ToString());
                }
            }

            /* GAMEENCRYPTION END */

            /* GAMEDECRYPTION START (now for the easy part) */

            /*
	     * search for 4A 83 CA F0 42 8A 94 32
	     * and above it, 85 xx 74 xx 33 xx 85 xx 7E xx
	     * the first TEST (85 xx) must be cracked to CMP EAX, EAX (3B C0)
	     * if I want to do it like LB does in UORice, I'd crack
	     * the first CMP xx JMP xx (85 xx 74 xx) to CMP EAX, EAX (3B C0)
	     * which is bellow the one I crack
	     */

            /* find 4A 83 CA F0 42 8A 94 32 */
            /* find the TEST above it (not the one right above that is) */

            DecryptPos = ScanClient(0x100, DecryptSig, InClient, InClientLength, 8, 0);
            DecryptPosNew = ScanClient(0x100, DecryptSigNew, InClient, InClientLength, 11, 0);
            if (DecryptPos != -1)
            {
                DectestPos = ScanClient(0xCC, DectestSig, InClient, InClientLength, 10, DecryptPos - 0x100);
            }

            if (DecryptPos == -1 && DectestPos == -1 && DecryptPosNew == -1)
            {
                this.LAB_StatusIS.Text = "Can't find any MD5 Decrypt signatures ???";
            }
            else
            {
                if (NewClient == -1)
                {
                    bytes[DectestPos] = 0x3B;
                    this.LAB_StatusIS.Text = string.Format("Patching old MD5 with CMP (0x3B) - (59) @{0:X} - ({1})", DectestPos, DectestPos.ToString());
                }
                else if (NewClient == 1)
                {
                    bytes[DecryptPosNew + 2] = 0xEB;
                    this.LAB_StatusIS.Text = string.Format("Patching (new MD5 (D2+2)) with (0xEB) - (235) @{0:X} - ({1})", DecryptPosNew, DecryptPosNew.ToString());
                }
            }

            this.LAB_StatusIS.Text = "Client Decryption Done...";
        }
        private int ScanClient(int FlexByte, byte[] signature, byte[] client, long client_length, int sig_length, int startat)
        {
            int Count = 0, i = 0, j = 0;

            // This was causing a pain for some reason
            // Easier to do these 2 items in there own sections
            bool UseFlex = GetMyBool(FlexByte, 100);
            byte FByte = GetMyByte(FlexByte);

            if (startat == -1)
            {
                startat = 0;
            }

            for (i = startat; i < client_length - sig_length; i++)
            {
                /* compare src_size from src against src_size bytes from buf */
                // Check signature bytes from i's position
                for (j = 0; j < sig_length; j++)
                {
                    /* if there's a difference and this isn't the flex_byte, move on */
                    if ((UseFlex && signature[j] != FByte) && signature[j] != client[i + j])
                        break;
                    else if (!UseFlex && signature[j] != client[i + j])
                        break;

                    /* if its the last byte for comparison, everything matched */
                    if (j == (sig_length - 1))
                    {
                        Count++;
                        if (Count >= 1)
                            return i;
                    }
                }
            }

            return -1;
        }

        private bool GetMyBool(int a, int b)
        {
            int TheBool = a & b;
            bool MyBool = Convert.ToBoolean(TheBool);
            return MyBool;
        }

        private byte GetMyByte(int flex)
        {
            int TheByte = flex & 255;
            byte MyByte = Convert.ToByte(TheByte);
            return MyByte;
        }

        private void btDecriptClientForm_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string clientFilePath = openFileDialog1.FileName;
                LAB_StatusIS.Text = "Found...";
                ReadClientFile(clientFilePath); // This is where the ReadClientFile method is called
            }
        }
    }
}
