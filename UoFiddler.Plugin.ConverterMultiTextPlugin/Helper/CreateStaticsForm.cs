using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Transition;
using Terrain;
using Ultima;
using System.Collections;
using Microsoft.VisualBasic.CompilerServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Helper
{
    public partial class CreateStaticsForm : Form
    {
        private ClsTerrainTable iTerrain;
        private RandomStatics iRandomStatic;
        //private Art UOArt;
        //private TileData UOStatic;
        private Point[,] StaticGrid;

        // Erstellen Sie eine Klassenvariable, um den letzten Bewegungsbefehl zu speichern.
        private string lastMoveCommand = "";
        public CreateStaticsForm()
        {
            InitializeComponent();

            CreateStaticsForm sEdit = this;
            base.Load += new EventHandler(sEdit.SEdit_Load);
            this.StaticGrid = new Point[13, 13];
            this.iTerrain = new ClsTerrainTable();
            this.iRandomStatic = new RandomStatics();

            int num = 302;
            int num1 = 246;
            int num2 = 0;
            do
            {
                int num3 = 0;
                do
                {
                    Point[,] staticGrid = this.StaticGrid;
                    Point point = new Point(checked(num - checked(num3 * 22)), checked(num1 + checked(num3 * 22)));
                    staticGrid[num3, num2] = point;
                    num3++;
                }
                while (num3 <= 12);
                num = checked(num + 22);
                num1 = checked(num1 + 22);
                num2++;
            }
            while (num2 <= 12);

            // Make sure the first item is displayed in PictureBox1
            UpdateVScrollBar1();

        }

        #region
        private void GroupSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Panel3.Refresh();
        }
        #endregion

        #region ListBox1_SelectedIndexChanged
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.PictureBox1.Image = null;
            RandomStaticCollection selectedItem = (RandomStaticCollection)this.ListBox1.SelectedItem;
            if (selectedItem != null)
            {
                this.TextBox3.Text = selectedItem.Description;
                this.NumericUpDown4.Value = new decimal(selectedItem.Freq);
                selectedItem.Display(this.ListBox2);
                this.Panel3.Refresh();
            }
        }
        #endregion

        #region ListBox2_SelectedIndexChanged
        private void ListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RandomStatic selectedItem = (RandomStatic)this.ListBox2.SelectedItem;
            if (selectedItem != null)
            {
                RandomStatic randomStatic = selectedItem;
                this.VScrollBar1.Value = randomStatic.TileID;
                if (Art.GetStatic(randomStatic.TileID) != null)
                {
                    this.PictureBox1.Image = Art.GetStatic(randomStatic.TileID);
                    this.PropertyGrid1.SelectedObject = TileData.ItemTable[randomStatic.TileID];
                }
                this.TileID.Text = StringType.FromInteger(randomStatic.TileID);
                this.Xaxis.Value = new decimal(randomStatic.X);
                this.Yaxis.Value = new decimal(randomStatic.Y);
                this.Zaxis.Value = new decimal(randomStatic.Z);
                this.HueID.Text = StringType.FromInteger(randomStatic.Hue);
                randomStatic = null;
            }
        }
        #endregion

        #region MenuLoad
        private void MenuLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = string.Format("{0}Data/Statics", AppDomain.CurrentDomain.BaseDirectory),
                Filter = "xml files (*.xml)|*.xml",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                this.TextBox1.Text = fileInfo.Name;
                this.iRandomStatic = new RandomStatics(fileInfo.Name);
                this.iRandomStatic.Display(this.ListBox1);
                this.Panel3.Refresh();
            }
        }
        #endregion

        #region MenuNew
        private void MenuNew_Click(object sender, EventArgs e)
        {
            //this.iRandomStatic = new RandomStatics();
            new CreateStaticsForm().Show();
            this.Hide();
        }
        #endregion

        #region MenuSave
        private void MenuSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                InitialDirectory = string.Format("{0}Data/Statics", AppDomain.CurrentDomain.BaseDirectory),
                Filter = "xml files (*.xml)|*.xml",
                FileName = this.TextBox1.Text,
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.iRandomStatic.Save(saveFileDialog.FileName);
            }
        }
        #endregion

        #region NumericUpDown4_ValueChanged
        private void NumericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            RandomStaticCollection selectedItem = (RandomStaticCollection)this.ListBox1.SelectedItem;
            if (selectedItem != null)
            {
                selectedItem.Freq = Convert.ToInt32(this.NumericUpDown4.Value);
            }
        }
        #endregion

        #region NumericUpDown5_ValueChanged
        private void NumericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            this.iRandomStatic.Freq = Convert.ToInt32(this.NumericUpDown5.Value);
        }
        #endregion

        #region Panel2_Paint
        private void Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        #endregion

        #region Panel1_Paint
        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            IEnumerator enumerator = null;
            Graphics graphics = e.Graphics;
            Pen pen = new Pen(Color.Gray);
            ClsTerrain selectedItem = (ClsTerrain)this.GroupSelect.SelectedItem;
            int num = 0;
            do
            {
                int num1 = 0;
                do
                {
                    int num2 = num1;
                    int num3 = num;
                    if (selectedItem != null)
                    {
                        e.Graphics.DrawImage(Art.GetLand(selectedItem.TileID), checked(this.StaticGrid[num2, num3].X - 22), checked(this.StaticGrid[num2, num3].Y - 22));
                    }
                    e.Graphics.DrawLine(pen, checked(this.StaticGrid[num2, num3].X - 22), this.StaticGrid[num2, num3].Y, this.StaticGrid[num2, num3].X, checked(this.StaticGrid[num2, num3].Y + 22));
                    e.Graphics.DrawLine(pen, this.StaticGrid[num2, num3].X, checked(this.StaticGrid[num2, num3].Y + 22), checked(this.StaticGrid[num2, num3].X + 22), this.StaticGrid[num2, num3].Y);
                    e.Graphics.DrawLine(pen, checked(this.StaticGrid[num2, num3].X + 22), this.StaticGrid[num2, num3].Y, this.StaticGrid[num2, num3].X, checked(this.StaticGrid[num2, num3].Y - 22));
                    e.Graphics.DrawLine(pen, this.StaticGrid[num2, num3].X, checked(this.StaticGrid[num2, num3].Y - 22), checked(this.StaticGrid[num2, num3].X - 22), this.StaticGrid[num2, num3].Y);
                    num1++;
                }
                while (num1 <= 12);
                num++;
            }
            while (num <= 12);
            //pen = new Pen(Color.Red);
            pen = new Pen(selectedColor);
            int num4 = Convert.ToInt32(decimal.Add(new decimal(6L), this.Yaxis.Value));
            int num5 = Convert.ToInt32(decimal.Add(new decimal(6L), this.Xaxis.Value));
            e.Graphics.DrawLine(pen, checked(this.StaticGrid[num4, num5].X - 22), this.StaticGrid[num4, num5].Y, this.StaticGrid[num4, num5].X, checked(this.StaticGrid[num4, num5].Y + 22));
            e.Graphics.DrawLine(pen, this.StaticGrid[num4, num5].X, checked(this.StaticGrid[num4, num5].Y + 22), checked(this.StaticGrid[num4, num5].X + 22), this.StaticGrid[num4, num5].Y);
            e.Graphics.DrawLine(pen, checked(this.StaticGrid[num4, num5].X + 22), this.StaticGrid[num4, num5].Y, this.StaticGrid[num4, num5].X, checked(this.StaticGrid[num4, num5].Y - 22));
            e.Graphics.DrawLine(pen, this.StaticGrid[num4, num5].X, checked(this.StaticGrid[num4, num5].Y - 22), checked(this.StaticGrid[num4, num5].X - 22), this.StaticGrid[num4, num5].Y);
            try
            {
                enumerator = this.ListBox2.Items.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    RandomStatic current = (RandomStatic)enumerator.Current;
                    int y = checked(6 + current.Y);
                    int x = checked(6 + current.X);
                    Bitmap @static = Art.GetStatic(current.TileID);
                    Point point = new Point(checked((int)Math.Round((double)this.StaticGrid[y, x].X - (double)@static.Width / 2)), checked(checked(this.StaticGrid[y, x].Y - @static.Height) + 22));
                    e.Graphics.DrawImage(@static, point);
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    ((IDisposable)enumerator).Dispose();
                }
            }
            graphics = null;
        }
        #endregion

        #region SEdit_Load
        private void SEdit_Load(object sender, EventArgs e)
        {
            this.iTerrain.Load();
            this.iTerrain.Display(this.GroupSelect);
        }
        #endregion

        #region StaticZoom
        private RStaticZoom rStaticZoomWindow;
        private void StaticZoom_Click(object sender, EventArgs e)
        {
            if (rStaticZoomWindow == null || rStaticZoomWindow.IsDisposed)
            {
                rStaticZoomWindow = new RStaticZoom()
                {
                    Tag = this.VScrollBar1
                };
                rStaticZoomWindow.Show();
            }
            else
            {
                // The window is already open, so let's bring it to the foreground
                rStaticZoomWindow.BringToFront();
            }
        }
        #endregion

        #region ToolBar1_ButtonClick
        private void ToolBar1_ButtonClick(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            RandomStaticCollection selectedItem = (RandomStaticCollection)this.ListBox1.SelectedItem;
            if (selectedItem != null)
            {
                object tag = button.Tag;
                if (ObjectType.ObjTst(tag, "Add", false) == 0)
                {
                    selectedItem.Add(new RandomStatic(int.Parse(this.TileID.Text), Convert.ToInt16(this.Xaxis.Value), Convert.ToInt16(this.Yaxis.Value), Convert.ToInt16(this.Zaxis.Value), int.Parse(this.HueID.Text)));
                    selectedItem.Display(this.ListBox2);
                    this.Panel3.Refresh();
                }
                else if (ObjectType.ObjTst(tag, "Delete", false) == 0)
                {
                    selectedItem.Remove((RandomStatic)this.ListBox2.SelectedItem);
                    selectedItem.Display(this.ListBox2);
                    this.Panel3.Refresh();
                }
            }
        }


        #endregion

        #region ToolBar2_ButtonClick
        private void ToolBar2_ButtonClick(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, "Add", false) == 0)
            {
                if (StringType.StrCmp(this.TextBox3.Text, string.Empty, false) == 0)
                {
                    return;
                }
                this.iRandomStatic.Add(new RandomStaticCollection(this.TextBox3.Text, Convert.ToInt32(this.NumericUpDown4.Value)));
                this.iRandomStatic.Display(this.ListBox1);
                this.Panel3.Refresh();
            }

            else if (ObjectType.ObjTst(tag, "Delete", false) == 0)
            {
                this.iRandomStatic.Remove((RandomStaticCollection)this.ListBox1.SelectedItem);
                this.iRandomStatic.Display(this.ListBox1);
                this.Panel3.Refresh();
            }
        }
        #endregion

        #region ToolBar_NavClick
        /*private void ToolBar_NavClick(object sender, EventArgs e)
        {
            // The clicked control is transformed into a ToolStripButton object.
            ToolStripButton button = sender as ToolStripButton;
            // If the transformation fails (that is, the clicked control is not a ToolStripButton), the method exits.
            if (button == null)
            {
                return;
            }

            // The current values ​​of the Xaxis and Yaxis controls are stored in the num and y variables.
            short num = Convert.ToInt16(this.Xaxis.Value);
            short y = Convert.ToInt16(this.Yaxis.Value);

            // The currently selected element from ListBox2 is fetched.
            RandomStatic selectedItem = (RandomStatic)this.ListBox2.SelectedItem;

            // When an item is selected in ListBox2, the X and Y values ​​of that item are stored in the num and y variables.
            if (selectedItem != null)
            {
                num = selectedItem.X;
                y = selectedItem.Y;
            }

            // The Tag property of the clicked ToolStripButton is checked and the values ​​of num and y are changed accordingly.
            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, 1, false) == 0) // Northwest
            {
                if (y > -6 && num > -6)
                {
                    y = checked((short)(checked(y - 1)));
                    num = checked((short)(checked(num - 1)));
                }
            }
            else if (ObjectType.ObjTst(tag, 2, false) == 0) // north
            {
                if (y > -6)
                {
                    y = checked((short)(checked(y - 1)));
                }
            }
            else if (ObjectType.ObjTst(tag, 3, false) == 0) // Northeast
            {
                if (y > -6 && num < 6)
                {
                    y = checked((short)(checked(y - 1)));
                    num = checked((short)(checked(num + 1)));
                }
            }
            else if (ObjectType.ObjTst(tag, 4, false) == 0) //west
            {
                if (num > -6)
                {
                    num = checked((short)(checked(num - 1)));
                }
            }
            else if (ObjectType.ObjTst(tag, 5, false) != 0)
            {
                if (ObjectType.ObjTst(tag, 6, false) == 0) // east
                {
                    if (num < 6)
                    {
                        num = checked((short)(checked(num + 1)));
                    }
                }
                else if (ObjectType.ObjTst(tag, 7, false) == 0) // southwest
                {
                    if (y < 6 && num > -6)
                    {
                        y = checked((short)(checked(y + 1)));
                        num = checked((short)(checked(num - 1)));
                    }
                }
                else if (ObjectType.ObjTst(tag, 8, false) == 0) // south
                {
                    if (y < 6)
                    {
                        y = checked((short)(checked(y + 1)));
                    }
                }
                else if (ObjectType.ObjTst(tag, 9, false) == 0) // southeast
                {
                    if (y < 6 && num < 6)
                    {
                        y = checked((short)(checked(y + 1)));
                        num = checked((short)(checked(num + 1)));
                    }
                }
            }

            // The values ​​of the Xaxis and Yaxis controls are updated based on the new values ​​of num and y.
            this.Xaxis.Value = new decimal(num);
            this.Yaxis.Value = new decimal(y);

            // When an item in ListBox2 is selected, its X and Y values ​​are set to the new values ​​of num and y.
            if (selectedItem != null)
            {
                selectedItem.X = num;
                selectedItem.Y = y;
            }

            // Panel3 will be prompted to redraw itself to reflect the changes.
            this.Panel3.Refresh();
        }*/

        private void ToolBar_NavClick(object sender, EventArgs e)
        {
            // The clicked control is transformed into a ToolStripButton object.
            ToolStripButton button = sender as ToolStripButton;
            // If the transformation fails (that is, the clicked control is not a ToolStripButton), the method exits.
            if (button == null)
            {
                return;
            }

            // The current values ​​of the Xaxis and Yaxis controls are stored in the num and y variables.
            short num = Convert.ToInt16(this.Xaxis.Value);
            short y = Convert.ToInt16(this.Yaxis.Value);

            // The currently selected element from ListBox2 is fetched.
            RandomStatic selectedItem = (RandomStatic)this.ListBox2.SelectedItem;

            // When an item is selected in ListBox2, the X and Y values ​​of that item are stored in the num and y variables.
            if (selectedItem != null)
            {
                num = selectedItem.X;
                y = selectedItem.Y;
            }

            // The Tag property of the clicked ToolStripButton is checked and the values ​​of num and y are changed accordingly.
            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, 1, false) == 0) // Northwest
            {
                if (y > -6 && num > -6)
                {
                    y = checked((short)(checked(y - 1)));
                    num = checked((short)(checked(num - 1)));
                }
                lastMoveCommand = "Northwest";
            }
            else if (ObjectType.ObjTst(tag, 2, false) == 0) // north
            {
                if (y > -6)
                {
                    y = checked((short)(checked(y - 1)));
                }
                lastMoveCommand = "North";
            }
            else if (ObjectType.ObjTst(tag, 3, false) == 0) // Northeast
            {
                if (y > -6 && num < 6)
                {
                    y = checked((short)(checked(y - 1)));
                    num = checked((short)(checked(num + 1)));
                }
                lastMoveCommand = "Northeast";
            }
            else if (ObjectType.ObjTst(tag, 4, false) == 0) //west
            {
                if (num > -6)
                {
                    num = checked((short)(checked(num - 1)));
                }
                lastMoveCommand = "West";
            }
            else if (ObjectType.ObjTst(tag, 5, false) == 0) // center
            {
                // Code for the center button (tag 5)
                num = 0; // Set X-axis to center
                y = 0; // Set Y-axis to center

                // Update the selected item in ListBox2
                if (selectedItem != null)
                {
                    selectedItem.X = num;
                    selectedItem.Y = y;
                }

                // Invalidate the panels to reflect the changes
                // this.Panel3.Invalidate();
                // this.panel1.Invalidate();

                // Force an immediate update
                // this.Panel3.Update();
                // this.panel1.Update();
            }
            else if (ObjectType.ObjTst(tag, 6, false) == 0) // east
            {
                if (num < 6)
                {
                    num = checked((short)(checked(num + 1)));
                }
                lastMoveCommand = "East";
            }
            else if (ObjectType.ObjTst(tag, 7, false) == 0) // southwest
            {
                if (y < 6 && num > -6)
                {
                    y = checked((short)(checked(y + 1)));
                    num = checked((short)(checked(num - 1)));
                }
                lastMoveCommand = "Southwest";
            }
            else if (ObjectType.ObjTst(tag, 8, false) == 0) // south
            {
                if (y < 6)
                {
                    y = checked((short)(checked(y + 1)));
                }
                lastMoveCommand = "South";
            }
            else if (ObjectType.ObjTst(tag, 9, false) == 0) // southeast
            {
                if (y < 6 && num < 6)
                {
                    y = checked((short)(checked(y + 1)));
                    num = checked((short)(checked(num + 1)));
                }
                lastMoveCommand = "Southeast";
            }

            // The values ​​of the Xaxis and Yaxis controls are updated based on the new values ​​of num and y.
            this.Xaxis.Value = new decimal(num);
            this.Yaxis.Value = new decimal(y);

            // When an item in ListBox2 is selected, its X and Y values ​​are set to the new values ​​of num and y.
            if (selectedItem != null)
            {
                selectedItem.X = num;
                selectedItem.Y = y;
            }

            // Panel3 will be prompted to redraw itself to reflect the changes.
            this.Panel3.Refresh();

            // Update the direction label with the last move command
            this.directionLabel.Text = "Last movement command: " + lastMoveCommand;
        }

        #endregion

        #region VScrollBar1
        private void VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateVScrollBar1();
        }
        #endregion

        #region UpdateVScrollbar1
        private void UpdateVScrollBar1()
        {
            if (Art.GetStatic(this.VScrollBar1.Value) != null && this.VScrollBar1.Value < TileData.ItemTable.Length)
            {
                this.TileID.Text = this.VScrollBar1.Value.ToString();
                this.PictureBox1.Image = Art.GetStatic(this.VScrollBar1.Value);
                this.PropertyGrid1.SelectedObject = TileData.ItemTable[this.VScrollBar1.Value];
                this.TileDesc.Text = string.Format("{0} ({1})", TileData.ItemTable[this.VScrollBar1.Value].Name, this.VScrollBar1.Value);
            }
        }
        #endregion

        #region VScollBar1_ValueChanged
        private void VScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            UpdateVScrollBar1();
        }
        #endregion

        #region Xaxis_ValueChanged
        private void Xaxis_ValueChanged(object sender, EventArgs e)
        {
            this.Panel3.Refresh();
        }
        #endregion

        #region Yaxis_ValueChanged
        private void Yaxis_ValueChanged(object sender, EventArgs e)
        {
            this.Panel3.Refresh();
        }
        #endregion

        #region Color
        private Color selectedColor = Color.Red;
        private void colorButton_Click(object sender, EventArgs e)
        {
            // Erstellen Sie ein neues ColorDialog-Fenster.
            ColorDialog colorDialog = new ColorDialog();

            // Zeigen Sie das Dialogfenster an und speichern Sie das Ergebnis.
            DialogResult result = colorDialog.ShowDialog();

            // Wenn der Benutzer auf "OK" geklickt hat, aktualisieren Sie die ausgewählte Farbe.
            if (result == DialogResult.OK)
            {
                selectedColor = colorDialog.Color;
            }
        }
        #endregion
    }
}
