// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Xml;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class CalendarForm : Form
    {
        private ToolTip toolTip;

        public CalendarForm()
        {
            InitializeComponent();

            // Update the lbDaysTo and lbWeekendDays and lbWorkingDays labels when the application starts
            UpdateDaysTo();
            UpdateWeekendDays();
            UpdateWorkingDays();

            // Displays the current calendar week when the application starts
            lbCalendarWeek.Text = "Calendar Week: " + GetCalendarWeek(DateTime.Now).ToString();

            // Displays the current date when the application starts
            lbDate.Text = "Current Date: " + DateTime.Now.ToShortDateString();

            timer1 = new Timer();
            timer1.Interval = 1000; // Sets the interval to 1 second
            timer1.Tick += new EventHandler(this.timer1_Tick); // Adds an event handler
            timer1.Start(); // Starts the timer

            // Sets the highlighted data
            monthCalendar1.BoldedDates = GetNotedDates().ToArray();

            // Creates a new ToolTip control
            toolTip = new ToolTip();
            toolTip.OwnerDraw = true;
            toolTip.Draw += new DrawToolTipEventHandler(toolTip_Draw);
            toolTip.Popup += new PopupEventHandler(toolTip_Popup);

            // Erstellt einen neuen Timer
            Timer timer = new Timer();
            timer.Interval = 1000; // Sets the interval to 1 second
            timer.Tick += (sender, e) =>
            {
                // Gets the selected date
                string date = monthCalendar1.SelectionStart.ToShortDateString();

                // Gets the note for the selected date
                string note = GetNoteForDate(date);

                // Sets the tooltip text
                toolTip.SetToolTip(monthCalendar1, note);

                // Stops the timer
                timer.Stop();
            };

            // Fügt den MouseHover- und MouseLeave-Ereignishandler hinzu
            monthCalendar1.MouseHover += (sender, e) => timer.Start();
            monthCalendar1.MouseLeave += (sender, e) => timer.Stop();
        }

        #region calendar week
        // Function for calculating the calendar week
        public static int GetCalendarWeek(DateTime date)
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            Calendar calendar = currentCulture.Calendar;
            int calendarWeek = calendar.GetWeekOfYear(date, currentCulture.DateTimeFormat.CalendarWeekRule, currentCulture.DateTimeFormat.FirstDayOfWeek);
            return calendarWeek;
        }
        #endregion

        #region monthCalendar_DateChange
        private void monthCalendar_DateChanged(object sender, DateRangeEventArgs e)
        {
            // Displays the selected date in the label
            lbDate.Text = "Selected Date: " + monthCalendar1.SelectionStart.ToShortDateString();

            // Calculates the calendar week of the selected date and displays it in the label
            int calendarWeek = GetCalendarWeek(monthCalendar1.SelectionStart);
            lbCalendarWeek.Text = "Calendar Week: " + calendarWeek.ToString();

            // Update the lbDaysTo label
            UpdateDaysTo();

            // Update the lbWeekendDays label
            UpdateWeekendDays();

            // Update the lbWorkingDays label
            UpdateWorkingDays();
        }
        #endregion

        #region Timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Updates the lbTime label with the current time every second
            lbTime.Text = "Time: " + DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion

        #region monthCalendarForm_DateSelected
        private void monthCalendarForm_DateSelected(object sender, DateRangeEventArgs e)
        {
            // Creates a new shape
            Form noteForm = new Form();
            noteForm.Text = "Note for " + monthCalendar1.SelectionStart.ToShortDateString();

            // Always puts the form in the foreground
            noteForm.TopMost = true;

            // Creates a new RichTextBox
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            noteForm.Controls.Add(richTextBox);

            // Loads the text from the XML file if it exists or creates a new one if it does not exist
            string date = monthCalendar1.SelectionStart.ToShortDateString();
            XDocument doc;
            if (File.Exists("CalendarDateNotes.xml"))
            {
                doc = XDocument.Load("CalendarDateNotes.xml");
            }
            else
            {
                doc = new XDocument(new XElement("notes"));
            }
            XElement note = doc.Root.Descendants("note").FirstOrDefault(n => n.Element("date").Value == date);
            if (note != null)
            {
                richTextBox.Text = note.Element("text").Value;
            }

            // Creates a new save button
            Button saveButton = new Button();
            saveButton.Text = "Save";
            saveButton.Dock = DockStyle.Bottom;
            saveButton.Click += (sender, e) =>
            {
                // Saves the text from the RichTextBox to an XML file
                if (note == null)
                {
                    doc.Root.Add(new XElement("note",
                        new XElement("date", date),
                        new XElement("text", richTextBox.Text)
                    ));
                }
                else
                {
                    note.Element("text").Value = richTextBox.Text;
                }
                doc.Save("CalendarDateNotes.xml");

                // Updates the highlighted dates in the MonthCalendar control
                monthCalendar1.BoldedDates = GetNotedDates().ToArray();

                // Closes the form
                noteForm.Close();
            };
            noteForm.Controls.Add(saveButton);

            // Creates a new delete button
            Button deleteButton = new Button();
            deleteButton.Text = "Delete";
            deleteButton.Dock = DockStyle.Bottom;
            deleteButton.Click += (sender, e) =>
            {
                // Deletes the entry from the XML file if it exists
                if (note != null)
                {
                    note.Remove();
                    doc.Save("CalendarDateNotes.xml");
                }

                // Closes the form
                noteForm.Close();
            };
            noteForm.Controls.Add(deleteButton);

            // Displays the shape
            noteForm.Show();
        }
        #endregion

        #region Hervorgehobene daten
        private List<DateTime> GetNotedDates()
        {
            List<DateTime> notedDates = new List<DateTime>();

            XDocument doc;
            if (File.Exists("CalendarDateNotes.xml"))
            {
                doc = XDocument.Load("CalendarDateNotes.xml");
                foreach (XElement note in doc.Root.Descendants("note"))
                {
                    DateTime date = DateTime.Parse(note.Element("date").Value);
                    notedDates.Add(date);
                }
            }

            return notedDates;
        }
        #endregion

        #region monthCalendar1_MouseHover
        private void monthCalendar1_MouseHover(object sender, EventArgs e)
        {
            // Gets the selected date
            string date = monthCalendar1.SelectionStart.ToShortDateString();

            // Gets the note for the selected date
            string note = GetNoteForDate(date);

            // Sets the tooltip text
            toolTip.SetToolTip(monthCalendar1, note);
        }
        #endregion

        #region Tooltip
        private string GetNoteForDate(string date)
        {
            XDocument doc;
            if (File.Exists("CalendarDateNotes.xml"))
            {
                doc = XDocument.Load("CalendarDateNotes.xml");
                XElement note = doc.Root.Descendants("note").FirstOrDefault(n => n.Element("date").Value == date);
                if (note != null)
                {
                    return note.Element("text").Value;
                }
            }

            return null;
        }
        private void toolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }

        private void toolTip_Popup(object sender, PopupEventArgs e)
        {
            string toolTipText = toolTip.GetToolTip(e.AssociatedControl);
            e.ToolTipSize = TextRenderer.MeasureText(toolTipText, new Font("Arial", 16));
        }
        #endregion

        #region lbDaysTo
        private void UpdateDaysTo()
        {
            // Get the selected date
            DateTime selectedDate = monthCalendar1.SelectionStart;

            // Calculate the difference between the current date and the selected date
            TimeSpan difference = selectedDate - DateTime.Now;

            // Update the lbDaysTo label
            lbDaysTo.Text = "Days to Selected Date: " + difference.Days.ToString();
        }
        #endregion

        #region lbWeekendDays
        private void UpdateWeekendDays()
        {
            // Get the current date
            DateTime currentDate = DateTime.Now;

            // Get the end of the year
            DateTime endOfYear = new DateTime(currentDate.Year, 12, 31);

            // Initialize a counter for the weekend days
            int weekendDays = 0;

            // Iterate through all days until the end of the year
            for (DateTime date = currentDate; date <= endOfYear; date = date.AddDays(1))
            {
                // Check if the day is a weekend day
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekendDays++;
                }
            }

            // Update the lbWeekendDays label
            lbWeekendDays.Text = "Remaining Weekend Days: " + weekendDays.ToString();
        }
        #endregion

        #region lbWorkingDays
        private void UpdateWorkingDays()
        {
            // Get the current date
            DateTime currentDate = DateTime.Now;

            // Get the end of the year
            DateTime endOfYear = new DateTime(currentDate.Year, 12, 31);

            // Initialize a counter for the working days
            int workingDays = 0;

            // Iterate through all days until the end of the year
            for (DateTime date = currentDate; date <= endOfYear; date = date.AddDays(1))
            {
                // Check if the day is a working day
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }

            // Update the lbWorkingDays label
            lbWorkingDays.Text = "Remaining Working Days: " + workingDays.ToString();
        }
        #endregion

    }
}
