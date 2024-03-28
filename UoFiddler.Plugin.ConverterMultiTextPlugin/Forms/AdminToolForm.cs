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
        private static AdminToolForm instance; // Static variable to store the instance
        private AdminToolForm adminToolForm;


        public AdminToolForm()
        {
            // Check whether an instance of the shape is already open
            if (instance != null && !instance.IsDisposed)
            {
                // An instance is already open, so show the existing instance and close the new instance
                instance.Focus();
                Close();
                return;
            }

            // No other instance was found, so save this instance
            instance = this;

            InitializeComponent();

            label1.Text = "";
        }

        #region ÖffneAdminToolForm
        public void ÖffneAdminToolForm()
        {
            // Check if the AdminToolForm has already been discarded or is null
            if (adminToolForm == null || adminToolForm.IsDisposed)
            {
                // Create a new instance of the AdminToolForm
                adminToolForm = new AdminToolForm();
                // Show the form
                adminToolForm.Show();
            }
            else
            {
                // Show the already existing form
                adminToolForm.Focus();
            }
        }
        #endregion

        #region AdminToolForm
        // Method to get the already opened instance
        public static AdminToolForm GetInstance()
        {
            if (instance == null || instance.IsDisposed)
            {
                // If no instance exists or the instance has been discarded, create a new instance
                instance = new AdminToolForm();
            }

            return instance;
        }
        #endregion

        #region btnPing
        private void btnPing_Click(object sender, System.EventArgs e)
        {
            string address = textBoxAdress.Text;
            // Verify that the address is a valid IP address or domain
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
                // If the address is invalid, a message will be displayed
                MessageBox.Show("The entered address is invalid. Please enter a valid IP address or domain.");
            }

            label1.Text = address;
        }
        #endregion

        #region IsValidIPAddress
        // Checks whether the specified string is a valid IP address
        private bool IsValidIPAddress(string address)
        {
            // Check the IPv4 address
            string patternIPv4 = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            if (Regex.IsMatch(address, patternIPv4))
            {
                return true;
            }

            // Check the IPv6 address
            string patternIPv6 = @"^(?:[A-F0-9]{1,4}:){7}[A-F0-9]{1,4}$";
            if (Regex.IsMatch(address, patternIPv6))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region IsValidDomainName
        // Checks whether the specified string is a valid domain
        private bool IsValidDomainName(string address)
        {
            string pattern = @"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$";
            return Regex.IsMatch(address, pattern);
        }
        #endregion

        #region textBoxAdress_KeyDown
        private void textBoxAdress_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed
            if (e.KeyCode == Keys.Enter)
            {
                // Start ping
                btnPing_Click(this, EventArgs.Empty);
            }
        }
        #endregion

        #region btnTracert
        private async void btnTracert_Click(object sender, EventArgs e)
        {
            string address = textBoxAdress.Text;
            // Verify that the address is a valid IP address or domain
            if (IsValidIPAddressTracert(address) || IsValidDomainNameTracert(address))
            {
                // Empty the textBoxPingOutput
                textBoxPingAusgabe.Clear();
                // Maximum TTL value
                int maxHops = 30;
                // Current TTL value
                int currentHop = 1;
                // Goal achieved?
                bool targetReached = false;
                // Create ping object
                Ping pingSender = new Ping();
                // Create ping options
                PingOptions pingOptions = new PingOptions(currentHop, true);
                // Create buffer
                byte[] buffer = new byte[32];
                // Set timeout
                int timeout = 5000;
                try
                {
                    // Create IPHostEntry object for the destination address
                    IPHostEntry hostEntry = Dns.GetHostEntry(address);
                    // Set target IP address
                    IPAddress targetAddress = hostEntry.AddressList[0];
                    while (!targetReached && currentHop <= maxHops)
                    {
                        // Send ping
                        PingReply reply = await pingSender.SendPingAsync(targetAddress, timeout, buffer, pingOptions);
                        // show result
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
                        // Increase TTL value
                        currentHop++;
                        pingOptions.Ttl = currentHop;
                    }
                }
                catch (SocketException)
                {
                    // If a SocketException occurs, a message is displayed
                    MessageBox.Show("The entered address is invalid. Please enter a valid IP address or domain.");
                }
            }
            else
            {
                // If the address is invalid, a message will be displayed
                MessageBox.Show("The entered address is invalid. Please enter a valid IP address or domain.");
            }
        }
        #endregion

        #region IsValidIPAddressTracert
        // Checks whether the specified string is a valid IP address
        private bool IsValidIPAddressTracert(string address)
        {
            IPAddress ipAddress;
            return IPAddress.TryParse(address, out ipAddress);
        }
        #endregion

        #region IsValidDomainNameTracert
        // Checks whether the specified string is a valid domain
        private bool IsValidDomainNameTracert(string address)
        {
            return Uri.CheckHostName(address) != UriHostNameType.Unknown;
        }
        #endregion

        #region btnClose
        // Method of closing the form
        private void btnClose_Click(object sender, EventArgs e)
        {
            // Set the instance variable to null to allow the form to be reopened
            instance = null;
            Close();
        }
        #endregion
    }
}
