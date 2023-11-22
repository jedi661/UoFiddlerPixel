using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace Logger
{
    public partial class LoggerForm : Form
    {
        PrintDocument document = new PrintDocument();
        PrintDialog dialog = new PrintDialog();

        private string m_LogName;
        private DateTime m_Task_Start;
        private DateTime m_Task_End;

        public LoggerForm()
        {       
            LoggerForm loggerForm = this;
            base.Load += new EventHandler(loggerForm.LoggerForm_Load);
            document.PrintPage += new PrintPageEventHandler(document_PrintPage);
            InitializeComponent();
        }

        public void StartTask()
        {
            this.m_Task_Start = DateTime.UtcNow;
        }

        public void EndTask()
        {
            this.m_Task_End = DateTime.UtcNow;
        }

        private void LoggerForm_Load(object sender, EventArgs e)
        {
            TextBox textLog = this.TextLog;
            textLog.Text = string.Concat(textLog.Text, "Ultima Online™ Map Creator Log\r\n", DateTime.Now.ToString("dd/MM/yyyy, hh:mm:ss tt\r\n"));
            textLog = this.TextLog;
            textLog.Text = string.Concat(textLog.Text, "===========================================\r\n");         
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < (int)assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                if (assembly.EntryPoint != null)
                {
                    this.m_LogName = assembly.EntryPoint.DeclaringType.Name;
                    AssemblyName name = assembly.GetName();
                    textLog = this.TextLog;
                    textLog.Text = string.Concat(textLog.Text, string.Format("{0} version:{1}\r\n", name.Name, name.Version.ToString()));
                    name = null;
                    AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
                    for (int j = 0; j < (int)referencedAssemblies.Length; j++)
                    {
                        AssemblyName assemblyName = referencedAssemblies[j];
                        textLog = this.TextLog;
                        textLog.Text = string.Concat(textLog.Text, string.Format("{0} version:{1}\r\n", assemblyName.Name, assemblyName.Version.ToString()));
                        assemblyName = null;
                    }
                }
            }
            textLog = this.TextLog;
            textLog.Text = string.Concat(textLog.Text, "\r\n");
        }

        public void LogMessage(string Message)
        {
            TextBox textLog = this.TextLog;
            textLog.Text = string.Concat(textLog.Text, Message, "\r\n");
            this.Refresh();
        }

        public void LogTimeStamp()
        {
            TextBox textLog = this.TextLog;
            textLog.Text = string.Concat(textLog.Text, string.Format("  Task:{0:dd/MMM/yyyy hh:mm:ss}", this.m_Task_Start));
            textLog = this.TextLog;
            textLog.Text = string.Concat(textLog.Text, " === > ");
            textLog = this.TextLog;
            textLog.Text = string.Concat(textLog.Text, string.Format("{0:hh:mm:ss}", this.m_Task_End));
            textLog = this.TextLog;
            textLog.Text = string.Concat(textLog.Text, string.Format("  Total:{0} seconds\r\n", DateAndTime.DateDiff(DateInterval.Second, this.m_Task_Start, this.m_Task_End, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)));
            this.Refresh();
        }

        private void MenuItem2_Click(object sender, EventArgs e)
        {
            SaveFileDialog Log = new SaveFileDialog();
            Log.FileName = "DefaultLog.txt";
            Log.Filter = "ProgressLog | (*.txt)";

            if (Log.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(Log.FileName, true))
                {
                    sw.Write(this.TextLog.Text);
                    sw.Flush();
                    //sw.Close(); //Double closes file
                }
            }           
        }

        void document_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(TextLog.Text, new Font("Arial", 20, FontStyle.Regular), Brushes.Black, 20, 20);
        }

        private void MenuItem3_Click(object sender, EventArgs e)
        {
            dialog.Document = document;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                document.Print();
            }
        }
    }
}
