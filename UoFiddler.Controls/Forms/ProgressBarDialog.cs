using System;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class ProgressBarDialog : Form
    {
        private bool _isCancelled = false;

        public event Action CancelClicked; // Event für Abbruch

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

        public bool IsCancelled => _isCancelled;

        public void OnChangeEvent()
        {
            if (!_isCancelled)
            {
                progressBar.PerformStep();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            _isCancelled = true;
            CancelClicked?.Invoke(); // Abbruch-Ereignis auslösen
            Close();
        }
    }
}
