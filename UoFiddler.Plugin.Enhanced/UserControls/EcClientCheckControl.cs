/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Analysis;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>One check over the whole Enhanced Client with a report (tileart, textures, multis, item animations, hues, maps).</summary>
    public sealed partial class EcClientCheckControl : UserControl, IEcEditor
    {
        private EcClient _client;
        private CancellationTokenSource _cancel;

        public EcClientCheckControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ShowHelp();
        }

        public void SetClient(EcClient client) => _client = client;

        public void OnActivated()
        {
        }

        private async void Check()
        {
            if (_client == null)
            {
                EcDialogs.ShowError(this, "Choose the Enhanced Client folder on top first.");
                return;
            }
            var options = new EcCheckOptions
            {
                TileArt = toolStripButtonTileArt.Checked,
                Textures = toolStripButtonTextures.Checked,
                Multis = toolStripButtonMultis.Checked,
                ItemAnimations = toolStripButtonAnimations.Checked,
                Hues = toolStripButtonHues.Checked,
                Maps = toolStripButtonMaps.Checked
            };
            _cancel = new CancellationTokenSource();
            CancellationToken token = _cancel.Token;
            var progress = new Progress<int>(p => toolStripProgressBar.Value = Math.Max(0, Math.Min(100, p)));
            SetBusy(true);
            toolStripStatusLabel.Text = "Checking...";
            try
            {
                List<EcCheckSection> sections = await Task.Run(() => new EcClientCheck(_client).Run(options, progress, token));
                textBoxReport.Text = EcClientCheck.Report(sections, _client.Folder).Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
                int problems = sections.Sum(s => s.Problems);
                toolStripStatusLabel.Text = problems == 0 ? "No problems found." : $"{problems} problems in {sections.Count(s => s.Problems > 0)} areas.";
                tabControl.SelectedTab = tabPageReport;
            }
            catch (OperationCanceledException)
            {
                toolStripStatusLabel.Text = "Cancelled.";
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = "Error: " + ex.Message;
                EcDialogs.ShowError(this, "The check failed.", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            toolStripButtonCheck.Enabled = !busy;
            toolStripButtonCancel.Enabled = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            if (!busy)
            {
                toolStripProgressBar.Value = 0;
            }
        }

        private void SaveReport()
        {
            using (var dialog = new SaveFileDialog { Filter = "Text|*.txt", FileName = "EC_Check.txt" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, textBoxReport.Text);
                }
            }
        }

        private void ShowHelp() => richTextBoxHelp.Text = radioButtonHelpGerman.Checked ? ToolsHelp.CheckGerman : ToolsHelp.CheckEnglish;

        // ==================================================================== event handlers (designer)

        private void OnClickCheck(object sender, EventArgs e) => Check();

        private void OnClickCancel(object sender, EventArgs e) => _cancel?.Cancel();

        private void OnClickSave(object sender, EventArgs e) => SaveReport();

        private void OnHelpLanguageChanged(object sender, EventArgs e) => ShowHelp();
    }
}
