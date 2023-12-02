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

namespace UoFiddler.Forms
{
    partial class AlarmClockForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            timeLabel = new System.Windows.Forms.Label();
            startButton = new System.Windows.Forms.Button();
            stopButton = new System.Windows.Forms.Button();
            LabelTimeReal = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            btLoadWave = new System.Windows.Forms.Button();
            openFileDialogWave = new System.Windows.Forms.OpenFileDialog();
            snoozeButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            dateTimePicker1.Location = new System.Drawing.Point(28, 12);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new System.Drawing.Size(115, 23);
            dateTimePicker1.TabIndex = 0;
            // 
            // timeLabel
            // 
            timeLabel.AutoSize = true;
            timeLabel.Location = new System.Drawing.Point(56, 64);
            timeLabel.Name = "timeLabel";
            timeLabel.Size = new System.Drawing.Size(66, 15);
            timeLabel.TabIndex = 1;
            timeLabel.Text = "CountTime";
            // 
            // startButton
            // 
            startButton.Location = new System.Drawing.Point(28, 86);
            startButton.Name = "startButton";
            startButton.Size = new System.Drawing.Size(47, 23);
            startButton.TabIndex = 2;
            startButton.Text = "Start";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            // 
            // stopButton
            // 
            stopButton.Location = new System.Drawing.Point(148, 86);
            stopButton.Name = "stopButton";
            stopButton.Size = new System.Drawing.Size(44, 23);
            stopButton.TabIndex = 3;
            stopButton.Text = "Stop";
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Click += stopButton_Click;
            // 
            // LabelTimeReal
            // 
            LabelTimeReal.AutoSize = true;
            LabelTimeReal.Location = new System.Drawing.Point(56, 40);
            LabelTimeReal.Name = "LabelTimeReal";
            LabelTimeReal.Size = new System.Drawing.Size(55, 15);
            LabelTimeReal.TabIndex = 4;
            LabelTimeReal.Text = "RealTime";
            // 
            // btLoadWave
            // 
            btLoadWave.Location = new System.Drawing.Point(149, 12);
            btLoadWave.Name = "btLoadWave";
            btLoadWave.Size = new System.Drawing.Size(43, 23);
            btLoadWave.TabIndex = 5;
            btLoadWave.Text = "Load";
            btLoadWave.UseVisualStyleBackColor = true;
            btLoadWave.Click += btLoadWave_Click;
            // 
            // openFileDialogWave
            // 
            openFileDialogWave.FileName = "openFileDialogWave";
            // 
            // snoozeButton
            // 
            snoozeButton.Location = new System.Drawing.Point(81, 86);
            snoozeButton.Name = "snoozeButton";
            snoozeButton.Size = new System.Drawing.Size(57, 23);
            snoozeButton.TabIndex = 6;
            snoozeButton.Text = "5 min";
            snoozeButton.UseVisualStyleBackColor = true;
            snoozeButton.Click += snoozeButton_Click;
            // 
            // AlarmClockForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(218, 118);
            Controls.Add(snoozeButton);
            Controls.Add(btLoadWave);
            Controls.Add(LabelTimeReal);
            Controls.Add(stopButton);
            Controls.Add(startButton);
            Controls.Add(timeLabel);
            Controls.Add(dateTimePicker1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "AlarmClockForm";
            Text = "Alarm Clock";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label timeLabel;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Label LabelTimeReal;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btLoadWave;
        private System.Windows.Forms.OpenFileDialog openFileDialogWave;
        private System.Windows.Forms.Button snoozeButton;
    }
}