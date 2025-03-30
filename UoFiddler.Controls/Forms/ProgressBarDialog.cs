using System;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class ProgressBarDialog : Form
    {
        public event Action CancelClicked; // Abort event

        private bool _isCancelled = false;
        private bool _processFinished = false; // Flag for completed process
        private bool _isCancelMessageShown = false; // New flag for abort message

        public ProgressBarDialog()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        public ProgressBarDialog(int max, string desc, bool useFileSaveEvent = true)
        {
            InitializeComponent();
            Text = desc;
            progressBar.Maximum = max;
            progressBar.Minimum = 0;
            progressBar.Value = 0;
            progressBar.Step = 1;

            if (useFileSaveEvent)
            {
                Ultima.Files.FileSaveEvent += OnChangeEvent;
            }
            else
            {
                Classes.ControlEvents.ProgressChangeEvent += OnChangeEvent;
            }

            Show();
        }

        public void OnChangeEvent()
        {
            if (_isCancelled)
            {
                return; // No further progress on demolition
            }
            progressBar.PerformStep();
        }

        public void MarkProcessFinished()
        {
            _processFinished = true; // Marks the completion of the process
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you really want to cancel the process?", "Confirm cancellation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _isCancelled = true;
                if (!_isCancelMessageShown)
                {
                    _isCancelMessageShown = true;
                    MessageBox.Show("Export was aborted.", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                CancelClicked?.Invoke(); // Abort signal
                Close(); // Close dialog
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_isCancelled && !_processFinished) // Query only if the process is not completed
            {
                var result = MessageBox.Show("Do you really want to cancel the process?", "Confirm cancellation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true; // Prevent closing
                }
                else
                {
                    _isCancelled = true;
                    if (!_isCancelMessageShown)
                    {
                        _isCancelMessageShown = true;
                        MessageBox.Show("Export was aborted.", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    CancelClicked?.Invoke();
                }
            }

            base.OnFormClosing(e);
        }

        public bool IsCancelled => _isCancelled;
        public bool IsProcessFinished => _processFinished; // To query the status externally
    }
}
