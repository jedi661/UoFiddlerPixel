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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class CalendarForm : Form
    {
        public CalendarForm()
        {
            InitializeComponent();

            // Displays the current calendar week when the application starts
            lbCalendarWeek.Text = "Calendar Week: " + GetCalendarWeek(DateTime.Now).ToString();

            // Adds an event handler that is called when the selected date changes
            monthCalendar1.DateChanged += new DateRangeEventHandler(this.monthCalendar_DateChanged);

            // Displays the current date when the application starts
            lbDate.Text = "Current Date: " + DateTime.Now.ToShortDateString();

            timer1 = new Timer();
            timer1.Interval = 1000; // Sets the interval to 1 second
            timer1.Tick += new EventHandler(this.timer1_Tick); // Adds an event handler
            timer1.Start(); // Starts the timer
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
        }
        #endregion

        #region Timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Updates the lbTime label with the current time every second
            lbTime.Text = "Time: " + DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion
    }
}
