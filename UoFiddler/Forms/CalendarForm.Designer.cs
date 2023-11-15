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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class CalendarForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalendarForm));
            monthCalendar1 = new System.Windows.Forms.MonthCalendar();
            lbCalendarWeek = new System.Windows.Forms.Label();
            lbDate = new System.Windows.Forms.Label();
            lbTime = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // monthCalendar1
            // 
            monthCalendar1.Location = new System.Drawing.Point(7, 8);
            monthCalendar1.Name = "monthCalendar1";
            monthCalendar1.TabIndex = 0;
            // 
            // lbCalendarWeek
            // 
            lbCalendarWeek.AutoSize = true;
            lbCalendarWeek.Location = new System.Drawing.Point(7, 177);
            lbCalendarWeek.Name = "lbCalendarWeek";
            lbCalendarWeek.Size = new System.Drawing.Size(82, 15);
            lbCalendarWeek.TabIndex = 1;
            lbCalendarWeek.Text = "calendar week";
            // 
            // lbDate
            // 
            lbDate.AutoSize = true;
            lbDate.Location = new System.Drawing.Point(7, 200);
            lbDate.Name = "lbDate";
            lbDate.Size = new System.Drawing.Size(31, 15);
            lbDate.TabIndex = 2;
            lbDate.Text = "Date";
            // 
            // lbTime
            // 
            lbTime.AutoSize = true;
            lbTime.Location = new System.Drawing.Point(7, 223);
            lbTime.Name = "lbTime";
            lbTime.Size = new System.Drawing.Size(33, 15);
            lbTime.TabIndex = 3;
            lbTime.Text = "Time";
            // 
            // CalendarForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(192, 247);
            Controls.Add(lbTime);
            Controls.Add(lbDate);
            Controls.Add(lbCalendarWeek);
            Controls.Add(monthCalendar1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CalendarForm";
            Text = "Calendar ";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MonthCalendar monthCalendar1;
        private System.Windows.Forms.Label lbCalendarWeek;
        private System.Windows.Forms.Label lbDate;
        private System.Windows.Forms.Label lbTime;
        private System.Windows.Forms.Timer timer1;
    }
}