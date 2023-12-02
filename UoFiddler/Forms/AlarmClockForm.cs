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
using System.Media;

namespace UoFiddler.Forms
{
    public partial class AlarmClockForm : Form
    {
        private Timer timer; // no value assigned yet
        private Timer realTimeTimer;
        private DateTime alarmTime;
        private SoundPlayer alarmSound = new SoundPlayer(@"C:\Windows\Media\Alarm01.wav"); // Path to alarm sound file Default alarm sound file

        #region AlarmClockForm
        public AlarmClockForm()
        {
            InitializeComponent();
            timer1 = new Timer();
            timer1.Interval = 1000; // Set timer to 1 second
            timer1.Tick += Timer_Tick;

            realTimeTimer = new Timer();
            realTimeTimer.Interval = 1000; // Set timer to 1 second
            realTimeTimer.Tick += RealTimeTimer_Tick;
            realTimeTimer.Start();

            timer = new Timer();
            timer.Interval = 1000; // Set timer to 1 second
            timer.Tick += SnoozeTimer_Tick;

            this.FormClosing += AlarmClockForm_FormClosing;
            this.Shown += AlarmClockForm_Shown;            

        }
        #endregion

        #region startButton
        private void startButton_Click(object sender, EventArgs e)
        {
            // Set the alarm time to the selected time in dateTimePicker1
            alarmTime = dateTimePicker1.Value;

            timer1.Start();
        }
        #endregion

        #region stopButton
        private void stopButton_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            alarmSound.Stop();
        }
        #endregion

        #region Timer_Tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan remainingTime = alarmTime - DateTime.Now;

            if (remainingTime.TotalSeconds <= 0)
            {
                timer1.Stop();
                timeLabel.Text = "00:00:00";

                // Play the alarm sound
                alarmSound.PlayLooping();
            }
            else
            {
                timeLabel.Text = remainingTime.ToString(@"hh\:mm\:ss");
            }
        }
        #endregion

        #region RealTimeTimer
        private void RealTimeTimer_Tick(object sender, EventArgs e)
        {
            LabelTimeReal.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion

        #region AlarmClockForm_FormClosing
        private void AlarmClockForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            alarmSound.Stop();
            Properties.Settings.Default.FormLocationAlarm = this.Location;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region btLoadWave
        private void btLoadWave_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialogWave = new OpenFileDialog();
            openFileDialogWave.Filter = "WAV Files|*.wav";

            if (openFileDialogWave.ShowDialog() == DialogResult.OK)
            {
                alarmSound.SoundLocation = openFileDialogWave.FileName;
                alarmSound.LoadAsync();
            }
        }
        #endregion

        #region AlarmClockForm_Shown
        private void AlarmClockForm_Shown(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.FormLocationAlarm != Point.Empty)
            {
                this.Location = Properties.Settings.Default.FormLocationAlarm;
            }
        }
        #endregion

        #region SnoozeTimer
        private void SnoozeTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan remainingTime = alarmTime - DateTime.Now; // Calculate the remaining snooze time

            if (remainingTime.TotalSeconds <= 0)
            {
                timer.Stop();
                timeLabel.Text = "00:00:00";
                alarmSound.PlayLooping();
            }
            else
            {
                timeLabel.Text = remainingTime.ToString(@"hh\:mm\:ss");
            }
        }
        #endregion

        #region snoozeButton
        private void snoozeButton_Click(object sender, EventArgs e)
        {
            timer1.Stop(); // Stop the original alarm timer
            alarmSound.Stop(); // Stop the alarm sound
            alarmTime = DateTime.Now.AddMinutes(5); // Set the alarm time to 5 minutes from now
            timer.Start(); // Start the snooze timer
            timeLabel.Text = "05:00:00"; // Display the initial snooze time
        }
        #endregion
    }
}
