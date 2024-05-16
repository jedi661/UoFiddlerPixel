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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Windows.Controls;
using Microsoft.Web.WebView2.WinForms;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace UoFiddler.Forms
{
    public partial class HelpDokuForm : Form
    { 
        public HelpDokuForm()
        {
            InitializeComponent();

            InitializeAsync();
        }

        public string FileName { get; set; }

        private async void InitializeAsync()
        {
            // Get the path to the %LOCALAPPDATA% directory
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // Create the path to the UoFiddler.exe.WebView2 folder in the %LOCALAPPDATA% directory
            string userDataFolder = Path.Combine(localAppData, "UoFiddler.exe.WebView2");
            // Create a CoreWebView2Environment instance with the specified userDataFolder
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            // Ensure that the CoreWebView2 runtime is initialized and use the specified CoreWebView2Environment instance
            await webView2.EnsureCoreWebView2Async(env);
            // Navigate to the specified file in the UOFiddlerHelp folder in the current directory
            webView2.CoreWebView2.Navigate($"file:///{Path.Combine(Environment.CurrentDirectory, "UOFiddlerHelp", FileName)}");
        }

        #region Search Button
        private async void toolStripButtonSuche_Click(object sender, EventArgs e)
        {
            string searchText = toolStripTextBoxSearch.Text;
            if (!string.IsNullOrEmpty(searchText))
            {
                await webView2.CoreWebView2.ExecuteScriptAsync($"window.find('{searchText}')");
            }
        }
        #endregion
    }
}
