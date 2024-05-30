/***************************************************************************
 *
 * $Author: Nikodemus
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
using System.Linq;
using System.Drawing;
using System.Net.Http;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class AdminToolForm : Form
    {
        private static AdminToolForm _instance; // Static variable to store the instance
        private AdminToolForm _adminToolForm;


        public AdminToolForm()
        {
            // Check whether an instance of the shape is already open
            if (_instance != null && !_instance.IsDisposed)
            {
                // An instance is already open, so show the existing instance and close the new instance
                _instance.Focus();
                Close();
                return;
            }

            // No other instance was found, so save this instance
            _instance = this;

            InitializeComponent();

            labelIP.Text = "";
            this.Load += AdminToolForm_Load;
        }

        #region AdminToolForm_Load
        private async void AdminToolForm_Load(object sender, EventArgs e)
        {
            await CheckInternetConnectionAsync();
        }
        #endregion

        #region ÖffneAdminToolForm
        public void ÖffneAdminToolForm()
        {
            // Check if the AdminToolForm has already been discarded or is null
            if (_adminToolForm == null || _adminToolForm.IsDisposed)
            {
                // Create a new instance of the AdminToolForm
                _adminToolForm = new AdminToolForm();
                // Show the form
                _adminToolForm.Show();
            }
            else
            {
                // Show the already existing form
                _adminToolForm.Focus();
            }
        }
        #endregion

        #region AdminToolForm
        // Method to get the already opened instance
        public static AdminToolForm GetInstance()
        {
            if (_instance == null || _instance.IsDisposed)
            {
                // If no instance exists or the instance has been discarded, create a new instance
                _instance = new AdminToolForm();
            }

            return _instance;
        }
        #endregion

        #region btnPing
        private void BtnPing_Click(object sender, System.EventArgs e)
        {
            string address = textBoxAdress.Text;

            // Verify that the address is a valid IP address or domain
            if (!IsValidIPAddress(address) && !IsValidDomainName(address))
            {
                // If the address is invalid, display a message and exit the method
                MessageBox.Show("The entered address is invalid. Please enter a valid IP address or domain.");
                return;
            }

            // Create a new Ping object
            Ping pingSender = new();

            // Attempt to send three pings to the address
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    PingReply reply = pingSender.Send(address);

                    if (reply.Status == IPStatus.Success)
                    {
                        // If the ping is successful, display the reply information
                        textBoxPingAusgabe.AppendText($"Answer from {reply.Address}: Time={reply.RoundtripTime}ms\n");
                    }
                    else
                    {
                        // If the ping fails, display the error status
                        textBoxPingAusgabe.AppendText($"Error: {reply.Status}\n");
                    }
                }
                catch (PingException ex)
                {
                    // If a PingException is thrown, display the exception message
                    textBoxPingAusgabe.AppendText($"PingException: {ex.Message}\n");
                }
            }

            // Display the address in the label
            labelIP.Text = address;
        }

        #endregion

        #region IsValidIPAddress
        // Checks whether the specified string is a valid IP address
        private static bool IsValidIPAddress(string address)
        {
            // Check the IPv4 address
            string patternIPv4 = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            if (Regex.IsMatch(address, patternIPv4))
            {
                return true;
            }

            // Check the IPv6 address
            string patternIPv6 = @"^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$";
            if (Regex.IsMatch(address, patternIPv6))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region IsValidDomainName
        // Checks whether the specified string is a valid domain
        private static bool IsValidDomainName(string address)
        {
            string pattern = @"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$";
            return Regex.IsMatch(address, pattern);
        }
        #endregion

        #region textBoxAdress_KeyDown
        private void TextBoxAdress_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed
            if (e.KeyCode == Keys.Enter)
            {
                // Start ping
                BtnPing_Click(this, EventArgs.Empty);
            }
        }
        #endregion

        #region btnTracert
        private async void BtnTracert_Click(object sender, EventArgs e)
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
                Ping pingSender = new();
                // Create ping options
                PingOptions pingOptions = new(currentHop, true);
                // Create buffer
                byte[] buffer = new byte[32];
                // Set timeout
                int timeout = 5000;
                try
                {
                    // Create IPHostEntry object for the destination address
                    IPHostEntry hostEntry = Dns.GetHostEntry(address);
                    // Set target IP address
                    IPAddress targetAddress = hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetworkV6) ?? hostEntry.AddressList[0];
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
        private static bool IsValidIPAddressTracert(string address)
        {
            return IPAddress.TryParse(address, out _);
        }
        #endregion

        #region IsValidDomainNameTracert
        // Checks whether the specified string is a valid domain
        private static bool IsValidDomainNameTracert(string address)
        {
            return Uri.CheckHostName(address) != UriHostNameType.Unknown;
        }
        #endregion

        #region btnClose
        // Method of closing the form
        private void BtnClose_Click(object sender, EventArgs e)
        {
            // Set the instance variable to null to allow the form to be reopened
            _instance = null;
            Close();
        }
        #endregion

        #region BtnCopyIP
        private void BtnCopyIP_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(labelIP.Text);
        }
        #endregion

        #region CheckInternetConnectionAsync
        private async Task CheckInternetConnectionAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("http://google.com/generate_204");
                    if (response.IsSuccessStatusCode)
                    {
                        LabelInternetStatus.Text = "Internet connected";
                        LabelInternetStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        LabelInternetStatus.Text = "Internet not connected";
                        LabelInternetStatus.ForeColor = Color.Red;
                    }
                }
            }
            catch
            {
                LabelInternetStatus.Text = "Internet not connected";
                LabelInternetStatus.ForeColor = Color.Red;
            }
        }
        #endregion
    }
}
