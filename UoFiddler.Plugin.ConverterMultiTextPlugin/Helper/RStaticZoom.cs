﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using Microsoft.VisualBasic.CompilerServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Helper
{
    public partial class RStaticZoom : Form
    {
        //private Art UOArt;
        private int iSelected;

        public RStaticZoom()
        {
            InitializeComponent();
            this.iSelected = 0;
        }

        #region Panel2 Mousedown
        private void Panel2_MouseDown(object sender, MouseEventArgs e)
        {
            int num = 0; // Variable for the horizontal position of the selected tile
            int num1 = 0; // Variable for the vertical position of the selected tile

            // Check whether the left mouse button was clicked
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // The horizontal position of the mouse click
                int x = e.X;

                // Check in which area the horizontal mouse click lies and set the variable 'num' accordingly
                // You will need to adjust these conditions as you increase the number of horizontal tiles
                if (x >= 0 && x <= 49) { num = 0; }
                else if (x >= 50 && x <= 99) { num = 1; }
                else if (x >= 100 && x <= 149) { num = 2; }
                else if (x >= 150 && x <= 199) { num = 3; }
                else if (x >= 200 && x <= 249) { num = 4; }
                else if (x >= 250 && x <= 299) { num = 5; }
                else if (x >= 300 && x <= 349) { num = 6; }
                else if (x >= 350 && x <= 399) { num = 7; }
                else if (x >= 400 && x <= 449) { num = 8; }
                else if (x >= 450 && x <= 499) { num = 9; }
                else if (x >= 500 && x <= 549) { num = 10; }
                else if (x >= 550 && x <= 599) { num = 11; }
                else if (x >= 600 && x <= 649) { num = 12; }
                else if (x >= 650 && x <= 699) { num = 13; }
                else if (x >= 700 && x <= 749) { num = 14; }

                // The vertical position of the mouse click
                int y = e.Y;

                // Check in which area the vertical mouse click lies and set the variable 'num1' accordingly
                // You will need to adjust these conditions as you increase the number of vertical tiles
                if (y >= 0 && y <= 59) { num1 = 0; }
                else if (y >= 60 && y <= 119) { num1 = 1; }
                else if (y >= 120 && y <= 179) { num1 = 2; }
                else if (y >= 180 && y <= 239) { num1 = 3; }
                else if (y >= 240 && y <= 299) { num1 = 4; }
                else if (y >= 300 && y <= 359) { num1 = 5; }
                else if (y >= 360 && y <= 419) { num1 = 6; }
                else if (y >= 420 && y <= 479) { num1 = 7; }
                else if (y >= 480 && y <= 539) { num1 = 8; }
                else if (y >= 540 && y <= 599) { num1 = 9; }

                // Calculating the selected index based on the values ​​of 'num' and 'num1'
                this.iSelected = checked(checked(this.VScrollBar1.Value + checked(num1 * 15)) + num);

                // Setting the selected index as the value of the 'Tag' attribute
                object tag = this.Tag;
                object[] objArray = new object[] { this.iSelected };
                LateBinding.LateSetComplex(tag, null, "Value", objArray, null, false, true);
            }
        }
        #endregion

        #region Paint Arts
        private void Panel2_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Font font = new System.Drawing.Font("Arial", 8f);
            SolidBrush solidBrush = new SolidBrush(Color.Black);
            Pen pen = new Pen(Color.Black);
            Graphics graphics = e.Graphics;
            graphics.Clear(Color.LightGray);
            int value = this.VScrollBar1.Value;
            int num = 0;
            do
            {
                int num1 = 0;
                do
                {
                    graphics.DrawRectangle(pen, checked(num1 * 50), checked(num * 60), 48, 58);
                    if (Art.GetStatic(value) != null)
                    {
                        graphics.DrawString(value.ToString(), font, solidBrush, (float)(checked(checked(num1 * 50) + 1)), (float)(checked(checked(num * 60) + 1)));
                        Rectangle rectangle = new Rectangle(checked(checked(num1 * 50) + 2), checked(checked(num * 60) + 12), 44, 44);
                        Rectangle rectangle1 = new Rectangle(1, 1, 44, 44);
                        graphics.DrawImage(Art.GetStatic(value), rectangle, rectangle1, GraphicsUnit.Pixel);
                        value++;
                    }
                    else
                    {
                        value++;
                    }
                    num1++;
                } while (num1 <= 14); // Increase this value to 9 for 10 tiles next to each other
                num++;
            } while (num <= 9); // Increase this value to 9 for 10 tiles one below the other
            graphics = null;
        }
        #endregion

        private void VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            this.Refresh();
        }
    }
}
