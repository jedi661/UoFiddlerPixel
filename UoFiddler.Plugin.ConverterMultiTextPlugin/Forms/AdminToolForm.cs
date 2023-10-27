/***************************************************************************
 *
 * $Author: Turley
 * Coder: Nikodemus
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class AdminToolForm : Form
    {
        private static AdminToolForm instance; // Statische Variable zur Speicherung der Instanz
        private AdminToolForm adminToolForm;


        public AdminToolForm()
        {
            // Überprüfen, ob bereits eine Instanz der Form geöffnet ist
            if (instance != null && !instance.IsDisposed)
            {
                // Eine Instanz ist bereits geöffnet, also die vorhandene Instanz anzeigen und die neue Instanz schließen
                instance.Focus();
                Close();
                return;
            }

            // Es wurde keine andere Instanz gefunden, also diese Instanz speichern
            instance = this;

            InitializeComponent();

            label1.Text = "";
        }

        public void ÖffneAdminToolForm()
        {
            // Überprüfen, ob das AdminToolForm bereits verworfen wurde oder null ist
            if (adminToolForm == null || adminToolForm.IsDisposed)
            {
                // Eine neue Instanz des AdminToolForm erstellen
                adminToolForm = new AdminToolForm();
                // Das Formular anzeigen
                adminToolForm.Show();
            }
            else
            {
                // Das bereits vorhandene Formular anzeigen
                adminToolForm.Focus();
            }
        }

        // Methode zum Abrufen der bereits geöffneten Instanz
        public static AdminToolForm GetInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                // Wenn keine Instanz vorhanden ist oder die Instanz verworfen wurde, erstelle eine neue Instanz
                instance = new AdminToolForm();
            }

            return instance;
        }

        private void btnPing_Click(object sender, System.EventArgs e)
        {
            string address = textBoxAdress.Text;
            // Überprüfen, ob die Adresse eine gültige IP-Adresse oder Domain ist
            if (IsValidIPAddress(address) || IsValidDomainName(address))
            {
                Ping pingSender = new Ping();
                for (int i = 0; i < 3; i++)
                {
                    PingReply reply = pingSender.Send(address);
                    if (reply.Status == IPStatus.Success)
                    {
                        textBoxPingAusgabe.AppendText("Antwort von " + reply.Address.ToString() + ": Zeit=" + reply.RoundtripTime.ToString() + "ms\n");
                    }
                    else
                    {
                        textBoxPingAusgabe.AppendText("Fehler: " + reply.Status.ToString() + "\n");
                    }
                }
            }
            else
            {
                // Wenn die Adresse ungültig ist, wird eine Nachricht angezeigt
                MessageBox.Show("The entered address is invalid. Please enter a valid IP address or domain.");
            }

            label1.Text = address;
        }

        // Überprüft, ob die angegebene Zeichenfolge eine gültige IP-Adresse ist
        private bool IsValidIPAddress(string address)
        {
            // Überprüfen Sie die IPv4-Adresse
            string patternIPv4 = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            if (Regex.IsMatch(address, patternIPv4))
            {
                return true;
            }

            // Überprüfen Sie die IPv6-Adresse
            string patternIPv6 = @"^(?:[A-F0-9]{1,4}:){7}[A-F0-9]{1,4}$";
            if (Regex.IsMatch(address, patternIPv6))
            {
                return true;
            }

            return false;
        }

        // Überprüft, ob die angegebene Zeichenfolge eine gültige Domain ist
        private bool IsValidDomainName(string address)
        {
            string pattern = @"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$";
            return Regex.IsMatch(address, pattern);
        }

        private void textBoxAdress_KeyDown(object sender, KeyEventArgs e)
        {
            // Überprüfe, ob die Eingabetaste gedrückt wurde
            if (e.KeyCode == Keys.Enter)
            {
                // Starte den Ping
                btnPing_Click(this, EventArgs.Empty);
            }
        }

        private async void btnTracert_Click(object sender, EventArgs e)
        {
            string address = textBoxAdress.Text;
            // Überprüfen, ob die Adresse eine gültige IP-Adresse oder Domain ist
            if (IsValidIPAddressTracert(address) || IsValidDomainNameTracert(address))
            {
                // Leere die textBoxPingAusgabe
                textBoxPingAusgabe.Clear();
                // Maximaler TTL-Wert
                int maxHops = 30;
                // Aktueller TTL-Wert
                int currentHop = 1;
                // Ziel erreicht?
                bool targetReached = false;
                // Ping-Objekt erstellen
                Ping pingSender = new Ping();
                // Ping-Optionen erstellen
                PingOptions pingOptions = new PingOptions(currentHop, true);
                // Puffer erstellen
                byte[] buffer = new byte[32];
                // Timeout festlegen
                int timeout = 5000;
                try
                {
                    // IPHostEntry-Objekt für die Zieladresse erstellen
                    IPHostEntry hostEntry = Dns.GetHostEntry(address);
                    // Ziel-IP-Adresse festlegen
                    IPAddress targetAddress = hostEntry.AddressList[0];
                    while (!targetReached && currentHop <= maxHops)
                    {
                        // Ping senden
                        PingReply reply = await pingSender.SendPingAsync(targetAddress, timeout, buffer, pingOptions);
                        // Ergebnis anzeigen
                        if (reply.Status == IPStatus.Success)
                        {
                            textBoxPingAusgabe.AppendText(currentHop + "\t" + reply.Address.ToString() + "\r\n");
                            targetReached = true;
                        }
                        else if (reply.Status == IPStatus.TtlExpired)
                        {
                            textBoxPingAusgabe.AppendText(currentHop + "\t" + reply.Address.ToString() + "\r\n");
                        }
                        else
                        {
                            textBoxPingAusgabe.AppendText(currentHop + "\t*\r\n");
                        }
                        // TTL-Wert erhöhen
                        currentHop++;
                        pingOptions.Ttl = currentHop;
                    }
                }
                catch (SocketException)
                {
                    // Wenn eine SocketException auftritt, wird eine Nachricht angezeigt
                    MessageBox.Show("The entered address is invalid. Please enter a valid IP address or domain.");
                }
            }
            else
            {
                // Wenn die Adresse ungültig ist, wird eine Nachricht angezeigt
                MessageBox.Show("The entered address is invalid. Please enter a valid IP address or domain.");
            }
        }

        // Überprüft, ob die angegebene Zeichenfolge eine gültige IP-Adresse ist
        private bool IsValidIPAddressTracert(string address)
        {
            IPAddress ipAddress;
            return IPAddress.TryParse(address, out ipAddress);
        }

        // Überprüft, ob die angegebene Zeichenfolge eine gültige Domain ist
        private bool IsValidDomainNameTracert(string address)
        {
            return Uri.CheckHostName(address) != UriHostNameType.Unknown;
        }

        // Methode zum Schließen der Form
        private void btnClose_Click(object sender, EventArgs e)
        {
            // Setze die Instanzvariable auf null, um das erneute Öffnen der Form zu ermöglichen
            instance = null;
            Close();
        }
    }
}
