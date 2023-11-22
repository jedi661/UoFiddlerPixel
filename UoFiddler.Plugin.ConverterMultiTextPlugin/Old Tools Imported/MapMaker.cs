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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Runtime.InteropServices;
using System.Xml;
using System.Diagnostics;
using Elevation;
using Terrain;
using Transition;
using Logger;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;
using Serilog.Core;
using Microsoft.VisualBasic;


using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Helper;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class MapMaker : Form
    {
        private ClsTerrainTable iTerrain;
        private ClsElevationTable iAltitude;
        private LoggerForm iLogger;


        //Viewer
        private int i_Menu;
        //private Art i_UOArt;
        private ClsElevationTable i_Altitude;
        private ClsTerrainTable i_Terrain;

        private Bitmap i_TerrainStatic;
        private Bitmap i_AltitudeStaticStatic;
        private bool i_RandomStatic;


        public MapMaker()
        {
            InitializeComponent();
            MapMaker makeMapImage = this;
            base.Load += new EventHandler(makeMapImage.MakeMapImage_Load);
            this.iTerrain = new ClsTerrainTable();
            this.iAltitude = new ClsElevationTable();
            this.iLogger = new LoggerForm();

            // Create an instance of LoggerForm
            iLogger = new LoggerForm();

            //Viewer
            MapMaker viewer = this;
            base.Load += new EventHandler(viewer.Viewer_Load);
            this.i_Menu = 0;
            this.i_Altitude = new ClsElevationTable();
            this.i_Terrain = new ClsTerrainTable();

            //Altitude Map Set
            MapMaker altImagePrep = this;
            base.Load += new EventHandler(altImagePrep.MakeMapImage_Load);

            //Ultima Online Map Making Utility
            MapMaker uOMapMake = this;
            base.Load += new EventHandler(uOMapMake.Form1_Load);
            this.i_RandomStatic = true;
        }

        #region MakeAltMap
        public Bitmap MakeAltMap(int xSize, int ySize, byte DefaultAlt, bool Dungeon)
        {
            Bitmap bitmap = new Bitmap(xSize, ySize, PixelFormat.Format8bppIndexed)
            {
                Palette = this.iAltitude.GetAltPalette()
            };
            Rectangle rectangle = new Rectangle(0, 0, xSize, ySize);
            BitmapData bitmapDatum = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            IntPtr scan0 = bitmapDatum.Scan0;
            int width = checked(bitmapDatum.Width * bitmapDatum.Height);
            byte[] defaultAlt = new byte[checked(checked(width - 1) + 1)];
            Marshal.Copy(scan0, defaultAlt, 0, width);
            if (!Dungeon)
            {
                int num = checked(xSize - 1);
                for (int i = 0; i <= num; i++)
                {
                    int num1 = checked(ySize - 1);
                    for (int j = 0; j <= num1; j++)
                    {
                        defaultAlt[checked(checked(j * xSize) + i)] = DefaultAlt;
                    }
                }
            }
            else
            {
                int num2 = checked(xSize - 1);
                for (int k = 0; k <= num2; k++)
                {
                    int num3 = checked(ySize - 1);
                    for (int l = 0; l <= num3; l++)
                    {
                        if (k <= 5119)
                        {
                            defaultAlt[checked(checked(l * xSize) + k)] = DefaultAlt;
                        }
                        else
                        {
                            defaultAlt[checked(checked(l * xSize) + k)] = 72;
                        }
                    }
                }
            }
            Marshal.Copy(defaultAlt, 0, scan0, width);
            bitmap.UnlockBits(bitmapDatum);
            return bitmap;
        }
        #endregion

        #region MakeMapImage
        private void MakeMapImage_Load(object sender, EventArgs e)
        {
            IEnumerator enumerator = null;
            this.iLogger.Show();
            int x = checked(this.iLogger.Location.X + 100);
            Point location = this.iLogger.Location;
            Point point = new Point(x, checked(location.Y + 100));
            this.Location = point;
            this.iTerrain.Load();
            this.iAltitude.Load();
            string str = string.Format("{0}\\Data\\System\\{1}", Directory.GetCurrentDirectory(), "MapInfo.xml");
            this.tbProjectPath.Text = Directory.GetCurrentDirectory();
            this.iTerrain.Display(this.ComboBox1);
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(str);
                try
                {
                    this.ComboBox2.Items.Clear();
                    try
                    {
                        enumerator = xmlDocument.SelectNodes("//Maps/Map").GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            MapInfo mapInfo = new MapInfo((XmlElement)enumerator.Current);
                            this.ComboBox2.Items.Add(mapInfo);
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable)
                        {
                            ((IDisposable)enumerator).Dispose();
                        }
                    }
                }
                catch (Exception exception1)
                {
                    ProjectData.SetProjectError(exception1);
                    Exception exception = exception1;
                    this.iLogger.LogMessage(string.Format("XML Error:{0}", exception.Message));
                    ProjectData.ClearProjectError();
                }
            }
            catch (Exception exception2)
            {
                ProjectData.SetProjectError(exception2);
                this.iLogger.LogMessage(string.Format("Unable to find:{0}", str));
                ProjectData.ClearProjectError();
            }
        }
        #endregion

        #region MakeTerainMap
        public Bitmap MakeTerrainMap(int xSize, int ySize, byte DefaultTerrain, bool Dungeon)
        {
            Bitmap bitmap = new Bitmap(xSize, ySize, PixelFormat.Format8bppIndexed)
            {
                Palette = this.iTerrain.GetPalette()
            };
            Rectangle rectangle = new Rectangle(0, 0, xSize, ySize);
            BitmapData bitmapDatum = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            IntPtr scan0 = bitmapDatum.Scan0;
            int width = checked(bitmapDatum.Width * bitmapDatum.Height);
            byte[] defaultTerrain = new byte[checked(checked(width - 1) + 1)];
            Marshal.Copy(scan0, defaultTerrain, 0, width);
            if (!Dungeon)
            {
                int num = checked(xSize - 1);
                for (int i = 0; i <= num; i++)
                {
                    int num1 = checked(ySize - 1);
                    for (int j = 0; j <= num1; j++)
                    {
                        defaultTerrain[checked(checked(j * xSize) + i)] = DefaultTerrain;
                    }
                }
            }
            else
            {
                int num2 = checked(xSize - 1);
                for (int k = 0; k <= num2; k++)
                {
                    int num3 = checked(ySize - 1);
                    for (int l = 0; l <= num3; l++)
                    {
                        if (k <= 5119)
                        {
                            defaultTerrain[checked(checked(l * xSize) + k)] = DefaultTerrain;
                        }
                        else
                        {
                            defaultTerrain[checked(checked(l * xSize) + k)] = 19;
                        }
                    }
                }
            }
            Marshal.Copy(defaultTerrain, 0, scan0, width);
            bitmap.UnlockBits(bitmapDatum);
            return bitmap;
        }
        #endregion

        #region MenuPath
        private void MenuPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                SelectedPath = this.ProjectPath.Text
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.ProjectPath.Text = folderBrowserDialog.SelectedPath;
            }
        }
        #endregion

        #region MenuTerain
        private void MenuTerrain_Click(object sender, EventArgs e)
        {
            byte altID;
            byte groupID;
            MapInfo selectedItem = (MapInfo)this.ComboBox2.SelectedItem;
            if (selectedItem == null)
            {
                this.iLogger.LogMessage("Error: Select a Map Type.");
            }
            else if (StringType.StrCmp(this.ProjectName.Text, string.Empty, false) != 0)
            {
                string str = string.Format("{0}/{1}/Map{2}", this.ProjectPath.Text, this.ProjectName.Text, selectedItem.MapNumber);
                if (!Directory.Exists(str))
                {
                    Directory.CreateDirectory(str);
                }
                if (this.ComboBox1.SelectedItem != null)
                {
                    ClsTerrain clsTerrain = (ClsTerrain)this.ComboBox1.SelectedItem;
                    groupID = checked((byte)clsTerrain.GroupID);
                    altID = clsTerrain.AltID;
                }
                else
                {
                    groupID = 9;
                    altID = 66;
                }
                this.iLogger.LogMessage("Creating Terrain Image.");
                this.iLogger.StartTask();
                try
                {
                    string str1 = string.Format("{0}/{1}", str, this.TerrainFile.Text);
                    Bitmap palette = this.MakeTerrainMap(selectedItem.XSize, selectedItem.YSize, groupID, this.Dungeon.Checked);
                    palette.Palette = this.iTerrain.GetPalette();
                    palette.Save(str1, ImageFormat.Bmp);
                    palette.Dispose();
                }
                catch (Exception exception)
                {
                    ProjectData.SetProjectError(exception);
                    this.iLogger.LogMessage("Error: Problem creating Terrain Image.");
                    ProjectData.ClearProjectError();
                }
                //this.iLogger.EndTask();
                this.iLogger.LogTimeStamp();
                this.iLogger.LogMessage("Creating Altitude Image.");
                this.iLogger.StartTask();
                try
                {
                    string str2 = string.Format("{0}/{1}", str, this.AltitudeFile.Text);
                    Bitmap altPalette = this.MakeAltMap(selectedItem.XSize, selectedItem.YSize, altID, this.Dungeon.Checked);
                    altPalette.Palette = this.iAltitude.GetAltPalette();
                    altPalette.Save(str2, ImageFormat.Bmp);
                    altPalette.Dispose();
                }
                catch (Exception exception2)
                {
                    ProjectData.SetProjectError(exception2);
                    Exception exception1 = exception2;
                    this.iLogger.LogMessage("Error: Problem creating Altitude Image.");
                    this.iLogger.LogMessage(exception1.Message);
                    ProjectData.ClearProjectError();
                }
                //this.iLogger.EndTask();
                this.iLogger.LogTimeStamp();
                this.iLogger.LogMessage("Done.");
            }
            else
            {
                this.iLogger.LogMessage("Error: Enter a project Name.");
            }
        }
        #endregion

        #region Form Closed Logger
        void MapMaker_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Close the logger when MapMaker closes
            iLogger.Close();
        }
        #endregion

        #region Data Viewer

        #region ListBox
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ListBox1.SelectedItem != null)
            {
                switch (this.i_Menu)
                {
                    case 0:
                        {
                            ClsTerrain selectedItem = (ClsTerrain)this.ListBox1.SelectedItem;
                            this.PropertyGrid1.SelectedObject = selectedItem;
                            this.PictureBox1.Image = Art.GetLand(selectedItem.TileID);
                            break;
                        }
                    case 1:
                        {
                            ClsElevation clsAltitude = (ClsElevation)this.ListBox1.SelectedItem;
                            this.PropertyGrid1.SelectedObject = clsAltitude;
                            break;
                        }
                }
            }
        }
        #endregion

        #region LoadTool
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.i_Menu = 0;
            this.lbTerrainList.Text = "Terrain List";
            this.i_Terrain.Load();
            this.i_Terrain.Display(this.ListBox1);
            this.PictureBox1.Visible = true;
        }
        #endregion

        #region Save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.i_Terrain.Save();
        }

        private void terrainACTToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.i_Terrain.SaveACT();
        }

        private void terrainACOToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.i_Terrain.SaveACO();
        }

        private void AltitudeACTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.i_Altitude.SaveACT();
        }

        private void AltitudeACOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.i_Altitude.SaveACO();
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.i_Terrain.SaveACT();
        }
        #endregion

        #region Load Altitude
        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.i_Menu = 1;
            this.lbTerrainList.Text = "Altitude List";
            this.i_Altitude.Load();
            this.i_Altitude.Display(this.ListBox1);
            this.PictureBox1.Visible = false;
        }
        #endregion

        #region Viewer Load
        private void Viewer_Load(object sender, EventArgs e)
        {
            this.i_Menu = 0;
            this.lbTerrainList.Text = "Terrain List";
            this.i_Terrain.Load();
            this.PictureBox1.Visible = true;
        }
        #endregion
        #endregion

        #region Altitude Map Set        

        private void toolStripMenuItemAltitudeMapSet_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                SelectedPath = this.tbPathAltitude.Text
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.tbPathAltitude.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private async void toolStripMenuItemMakeAltitudeImage_Click(object sender, EventArgs e)
        {
            Progress<int> progress = new Progress<int>(i => { ProgressBar2.Value = i; });
            Progress<string> logger = new Progress<string>(i => { iLogger.LogMessage(i); });
            Task resetProgress = new Task(() => { Thread.Sleep(1000); ((IProgress<int>)progress).Report(0); });
            await Task.Run(() => CreateElevationBitmapHelper.MakeAltitudeImage(tbPathAltitude.Text, TerrainFile.Text, AltitudeFile.Text, iAltitude, iTerrain, progress, logger)).ContinueWith(c => resetProgress.Start());
        }

        private void AltImagePrep_Load(object sender, EventArgs e)
        {
            this.iLogger.Show();
            int x = checked(this.iLogger.Location.X + 100);
            Point location = this.iLogger.Location;
            Point point = new Point(x, checked(location.Y + 100));
            this.Location = point;
            this.ProjectPath.Text = Directory.GetCurrentDirectory();
            this.iTerrain.Load();
            this.iAltitude.Load();
        }
        #endregion

        #region Ultima Online Map Making Utility
        private void Make()
        {
            short altID = 0;
            string str;
            byte num = 0;
            int num1;
            int num2;
            int num3;
            int num4;
            IEnumerator enumerator = null;
            TransitionTable transitionTable = new TransitionTable();
            DateTime now = DateTime.Now;
            this.iLogger.StartTask();
            this.iLogger.LogMessage("Loading Terrain Image.");
            try
            {
                str = string.Format("{0}\\{1}", this.tbProjectPath.Text, this.TerrainFile.Text);
                this.iLogger.LogMessage(str);
                this.i_TerrainStatic = new Bitmap(str);
            }
            catch (Exception exception1)
            {
                ProjectData.SetProjectError(exception1);
                Exception exception = exception1;
                this.iLogger.LogMessage("Problem with Loading Terrain Image.");
                this.iLogger.LogMessage(exception.Message);
                ProjectData.ClearProjectError();
                return;
            }
            this.iLogger.LogMessage("Loading Altitude Image.");
            try
            {
                str = string.Format("{0}\\{1}", this.tbProjectPath.Text, this.AltitudeFile.Text);
                this.iLogger.LogMessage(str);
                this.i_AltitudeStaticStatic = new Bitmap(str);
            }
            catch (Exception exception3)
            {
                ProjectData.SetProjectError(exception3);
                Exception exception2 = exception3;
                this.iLogger.LogMessage("Problem with Loading Altitude Image.");
                this.iLogger.LogMessage(exception2.Message);
                ProjectData.ClearProjectError();
                return;
            }
            //this.iLogger.EndTask();
            this.iLogger.LogTimeStamp();
            this.iLogger.LogMessage("Preparing Image Files.");
            this.iLogger.StartTask();
            int width = this.i_TerrainStatic.Width;
            int height = this.i_TerrainStatic.Height;
            Rectangle rectangle = new Rectangle(0, 0, width, height);
            BitmapData bitmapDatum = this.i_TerrainStatic.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            IntPtr scan0 = bitmapDatum.Scan0;
            int width1 = checked(bitmapDatum.Width * bitmapDatum.Height);
            byte[] numArray = new byte[checked(checked(width1 - 1) + 1)];
            Marshal.Copy(scan0, numArray, 0, width1);
            BitmapData bitmapDatum1 = this.i_AltitudeStaticStatic.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            IntPtr intPtr = bitmapDatum1.Scan0;
            int width2 = checked(bitmapDatum1.Width * bitmapDatum1.Height);
            byte[] numArray1 = new byte[checked(checked(width2 - 1) + 1)];
            Marshal.Copy(intPtr, numArray1, 0, width2);
            //this.iLogger.EndTask();
            this.iLogger.LogTimeStamp();
            this.iLogger.LogMessage("Creating Master Terrian Table.");
            this.iLogger.StartTask();
            MapCell[,] mapCell = new MapCell[checked(width + 1), checked(height + 1)];
            ClsElevationTable clsAltitudeTable = new ClsElevationTable();
            clsAltitudeTable.Load();
            try
            {
                int num5 = checked(width - 1);
                for (int i = 0; i <= num5; i++)
                {
                    int num6 = checked(height - 1);
                    for (int j = 0; j <= num6; j++)
                    {
                        int num7 = checked(checked(j * width) + i);

                        //ClsAltitude getAltitude = clsAltitudeTable[numArray1[num7]];
                        ClsElevation getAltitude = clsAltitudeTable.GetAltitude(numArray1[num7]);

                        mapCell[i, j] = new MapCell(numArray[num7], getAltitude.GetAltitude);
                    }
                }
            }
            catch (Exception exception4)
            {
                ProjectData.SetProjectError(exception4);
                this.iLogger.LogMessage("Altitude image needs to be rebuilt");
                ProjectData.ClearProjectError();
                return;
            }
            this.i_TerrainStatic.Dispose();
            this.i_AltitudeStaticStatic.Dispose();
            //this.iLogger.EndTask();
            this.iLogger.LogTimeStamp();
            width--;
            height--;
            int num8 = checked((int)Math.Round((double)width / 8 - 1));
            int num9 = checked((int)Math.Round((double)height / 8 - 1));
            this.iLogger.LogMessage("Load Transition Tables.");
            this.iLogger.StartTask();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            baseDirectory = string.Concat(baseDirectory, "Data\\Transitions\\");
            if (Directory.Exists(baseDirectory))
            {
                transitionTable.MassLoad(baseDirectory);
                //this.iLogger.EndTask();
                this.iLogger.LogTimeStamp();
                this.iLogger.LogMessage("Preparing Static Tables");
                Collection[,] collections = new Collection[checked(num8 + 1), checked(num9 + 1)];
                int num10 = num8;
                for (int k = 0; k <= num10; k++)
                {
                    int num11 = num9;
                    for (int l = 0; l <= num11; l++)
                    {
                        collections[k, l] = new Collection();
                    }
                }
                this.iLogger.LogMessage("Applying Transition Tables.");
                this.iLogger.StartTask();
                this.progressBarUltimaOnlineMapMakingUtility.Maximum = width;
                ClsTerrainTable clsTerrainTable = new ClsTerrainTable();
                clsTerrainTable.Load();
                MapTile mapTile = new MapTile();

                //Transition transition = new Transition();
                Transition.Transition transition = new Transition.Transition();
                //Transition.Transition mytransition = new Transition.Transition();

                short[] numArray2 = new short[16];
                short num12 = checked((short)width);
                for (short m = 0; m <= num12; m = checked((short)(m + 1)))
                {
                    num1 = (m != 0 ? checked(m - 1) : width);
                    num2 = (m != width ? checked(m + 1) : 0);
                    short num13 = checked((short)height);
                    for (short n = 0; n <= num13; n = checked((short)(n + 1)))
                    {
                        num4 = (n != 0 ? checked(n - 1) : height);
                        num3 = (n != height ? checked(n + 1) : 0);
                        object[] groupID = new object[] { mapCell[num1, num4].GroupID, mapCell[m, num4].GroupID, mapCell[num2, num4].GroupID, mapCell[num1, n].GroupID, mapCell[m, n].GroupID, mapCell[num2, n].GroupID, mapCell[num1, num3].GroupID, mapCell[m, num3].GroupID, mapCell[num2, num3].GroupID };
                        string str1 = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}", groupID);
                        try
                        {
                            //transition = transitionTable[str1];

                            transition = (Transition.Transition)(transitionTable.GetTransitionTable[str1]);

                            //transitionTable.MassLoad(str1);
                            //transition = (transitionTable.MassLoad(str1));
                            //transition = transitionTable.MassLoad(str1);
                            if (transition == null)
                            {
                                //ClsTerrain terrianGroup = clsTerrainTable[mapCell[m, n].GroupID];
                                ClsTerrain terrianGroup = clsTerrainTable.TerrianGroup(mapCell[m, n].GroupID);

                                mapCell[m, n].TileID = terrianGroup.TileID;
                                mapCell[m, n].AltID = altID;
                                terrianGroup = null;
                            }
                            else
                            {
                                altID = mapCell[m, n].AltID;
                                mapTile = transition.GetRandomMapTile();
                                if (mapTile == null)
                                {
                                    //ClsTerrain clsTerrain = clsTerrainTable[mapCell[m, n].GroupID];
                                    ClsTerrain clsTerrain = clsTerrainTable.TerrianGroup(mapCell[m, n].GroupID);
                                    mapCell[m, n].TileID = clsTerrain.TileID;
                                    mapCell[m, n].ChangeAltID((short)clsTerrain.AltID);
                                    clsTerrain = null;
                                }
                                else
                                {
                                    MapTile mapTile1 = mapTile;
                                    mapCell[m, n].TileID = mapTile1.TileID;
                                    mapCell[m, n].ChangeAltID(mapTile1.AltIDMod);
                                    mapTile1 = null;
                                }
                                transition.GetRandomStaticTiles(m, n, altID, collections, this.i_RandomStatic);
                            }
                            if (mapCell[m, n].GroupID == 254)
                            {
                                mapCell[m, n].TileID = 1078;
                                mapCell[m, n].AltID = 0;
                            }
                        }
                        catch (Exception exception6)
                        {
                            ProjectData.SetProjectError(exception6);
                            Exception exception5 = exception6;
                            LoggerForm loggerForm = this.iLogger;
                            groupID = new object[] { m, n, altID, str1 };
                            loggerForm.LogMessage(string.Format("\r\nLocation: X:{0}, Y:{1}, Z:{2} Hkey:{3}", groupID));
                            this.iLogger.LogMessage(exception5.ToString());
                            ProjectData.ClearProjectError();
                            return;
                        }
                    }
                    this.progressBarUltimaOnlineMapMakingUtility.Value = m;
                }
                //this.iLogger.EndTask();
                this.iLogger.LogTimeStamp();
                this.iLogger.LogMessage("Second Pass.");
                this.iLogger.StartTask();
                short[] altID1 = new short[9];
                RoughEdge roughEdge = new RoughEdge();
                short num14 = checked((short)width);
                for (short o = 0; o <= num14; o = checked((short)(o + 1)))
                {
                    num1 = (o != 0 ? checked(o - 1) : width);
                    num2 = (o != width ? checked(o + 1) : 0);
                    short num15 = checked((short)height);
                    for (short p = 0; p <= num15; p = checked((short)(p + 1)))
                    {
                        num4 = (p != 0 ? checked(p - 1) : height);
                        num3 = (p != height ? checked(p + 1) : 0);
                        mapCell[o, p].ChangeAltID(roughEdge.CheckCorner(mapCell[num1, num4].TileID));
                        mapCell[o, p].ChangeAltID(roughEdge.CheckLeft(mapCell[num1, p].TileID));
                        mapCell[o, p].ChangeAltID(roughEdge.CheckTop(mapCell[o, num4].TileID));
                        if (mapCell[o, p].GroupID == 20)
                        {
                            altID1[0] = mapCell[num1, num4].AltID;
                            altID1[1] = mapCell[o, num4].AltID;
                            altID1[2] = mapCell[num2, num4].AltID;
                            altID1[3] = mapCell[num1, p].AltID;
                            altID1[4] = mapCell[o, p].AltID;
                            altID1[5] = mapCell[num2, p].AltID;
                            altID1[6] = mapCell[num1, num3].AltID;
                            altID1[7] = mapCell[o, num3].AltID;
                            altID1[8] = mapCell[num2, num3].AltID;
                            Array.Sort(altID1);
                            float single = 10f * VBMath.Rnd();
                            if (single == 0f)
                            {
                                mapCell[o, p].AltID = checked((short)(checked(altID1[8] - 4)));
                            }
                            else if (single >= 1f && single <= 2f)
                            {
                                mapCell[o, p].AltID = checked((short)(checked(altID1[8] - 2)));
                            }
                            else if (single >= 3f && single <= 7f)
                            {
                                mapCell[o, p].AltID = altID1[8];
                            }
                            else if (single >= 8f && single <= 9f)
                            {
                                mapCell[o, p].AltID = checked((short)(checked(altID1[8] + 2)));
                            }
                            else if (single == 10f)
                            {
                                mapCell[o, p].AltID = checked((short)(checked(altID1[8] + 4)));
                            }
                        }

                        //if (clsTerrainTable[mapCell[o, p].GroupID].RandAlt)
                        if (clsTerrainTable.TerrianGroup(mapCell[o, p].GroupID).RandAlt)
                        {
                            float single1 = 10f * VBMath.Rnd();
                            if (single1 == 0f)
                            {
                                mapCell[o, p].ChangeAltID(-4);
                            }
                            else if (single1 >= 1f && single1 <= 2f)
                            {
                                mapCell[o, p].ChangeAltID(-2);
                            }
                            else if (single1 >= 8f && single1 <= 9f)
                            {
                                mapCell[o, p].ChangeAltID(2);
                            }
                            else if (single1 == 10f)
                            {
                                mapCell[o, p].ChangeAltID(4);
                            }
                        }
                    }
                    this.progressBarUltimaOnlineMapMakingUtility.Value = o;
                }
                //this.iLogger.EndTask();
                this.iLogger.LogTimeStamp();
                int num16 = 1;
                int num17 = width;
                if (num17 == 6143)
                {
                    num = 0;
                }
                else if (num17 == 2303)
                {
                    num = 2;
                }
                else if (num17 == 2559)
                {
                    num = 3;
                }
                this.iLogger.LogMessage("\r\n");
                this.iLogger.LogMessage("Load . . . . . Import Tiles.");
                this.iLogger.StartTask();
                ImportTiles importTile = new ImportTiles(collections, this.tbProjectPath.Text);
                //this.iLogger.EndTask();
                this.iLogger.LogTimeStamp();
                this.iLogger.LogMessage("\r\n");
                this.iLogger.LogMessage("Write Mul Files.");
                this.iLogger.StartTask();
                str = string.Format("{0}/Map{1}.mul", this.tbProjectPath.Text, num);
                this.iLogger.LogMessage(str);
                FileStream fileStream = new FileStream(str, FileMode.Create);
                BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                int num18 = width;
                for (int q = 0; q <= num18; q = checked(q + 8))
                {
                    int num19 = height;
                    for (int r = 0; r <= num19; r = checked(r + 8))
                    {
                        binaryWriter.Write(num16);
                        int num20 = 0;
                        do
                        {
                            int num21 = 0;
                            do
                            {
                                mapCell[checked(q + num21), checked(r + num20)].WriteMapMul(binaryWriter);
                                num21++;
                            }
                            while (num21 <= 7);
                            num20++;
                        }
                        while (num20 <= 7);
                    }
                }
                binaryWriter.Close();
                fileStream.Close();
                str = string.Format("{0}/StaIdx{1}.mul", this.tbProjectPath.Text, num);
                FileStream fileStream1 = new FileStream(str, FileMode.Create);
                this.iLogger.LogMessage(str);
                str = string.Format("{0}/Statics{1}.mul", this.tbProjectPath.Text, num);
                FileStream fileStream2 = new FileStream(str, FileMode.Create);
                this.iLogger.LogMessage(str);
                BinaryWriter binaryWriter1 = new BinaryWriter(fileStream1);
                BinaryWriter binaryWriter2 = new BinaryWriter(fileStream2);
                int num22 = num8;
                for (int s = 0; s <= num22; s++)
                {
                    int num23 = num9;
                    for (int t = 0; t <= num23; t++)
                    {
                        int num24 = 0;
                        int position = checked((int)binaryWriter2.BaseStream.Position);
                        try
                        {
                            enumerator = collections[s, t].GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                ((StaticCell)enumerator.Current).Write(binaryWriter2);
                                num24 = checked(num24 + 7);
                            }
                        }
                        finally
                        {
                            if (enumerator is IDisposable)
                            {
                                ((IDisposable)enumerator).Dispose();
                            }
                        }
                        if (num24 == 0)
                        {
                            position = -1;
                        }
                        binaryWriter1.Write(position);
                        binaryWriter1.Write(num24);
                        binaryWriter1.Write(num16);
                    }
                }
                binaryWriter2.Close();
                binaryWriter1.Close();
                fileStream2.Close();
                fileStream1.Close();
                //this.iLogger.EndTask();
                this.iLogger.LogTimeStamp();
                this.iLogger.LogMessage("Done.");
            }
            else
            {
                this.iLogger.LogMessage("Unable to find Transition Data files in the following path: ");
                this.iLogger.LogMessage(baseDirectory);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.iLogger.Show();
            int x = checked(this.iLogger.Location.X + 100);
            Point location = this.iLogger.Location;
            Point point = new Point(x, checked(location.Y + 100));
            this.Location = point;
            this.tbProjectPath.Text = AppDomain.CurrentDomain.BaseDirectory;
        }
        #endregion


        private void toolStripMenuItemPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                SelectedPath = this.tbProjectPath.Text
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.tbProjectPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        #region randomStaticOn
        private void randomStaticOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.i_RandomStatic = true;
            this.randomStaticOnToolStripMenuItem.Checked = true; // Set the Checked property to true
            this.randomStaticOffToolStripMenuItem.Checked = false; // Set the Checked property to false
        }
        #endregion

        #region randomStaticOff
        private void randomStaticOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.i_RandomStatic = false;
            this.randomStaticOnToolStripMenuItem.Checked = false; // Set the Checked property to false
            this.randomStaticOffToolStripMenuItem.Checked = true; // Set the Checked property to true
        }
        #endregion


        #region MakeUOMap
        private void toolStripMenuItemMakeUOMap_Click(object sender, EventArgs e)
        {
            if (Interaction.MsgBox("You are about to create the Mul Files\r\nAre you sure ?", MsgBoxStyle.YesNo, "Make UO Map") == MsgBoxResult.Yes)
            {
                MapMaker uOMapMake = this;
                (new Thread(new ThreadStart(uOMapMake.Make))).Start();
            }
        }
        #endregion

        #region Create Data Index
        private CreateDataIndexForm createDataIndexForm = null; // Variable to store the instance of CreateDataIndexForm
        private void createDataIndexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (createDataIndexForm == null || createDataIndexForm.IsDisposed)
            {
                createDataIndexForm = new CreateDataIndexForm();
                createDataIndexForm.Show();
            }
            else
            {
                createDataIndexForm.BringToFront(); // Bring the form to the front if it's already open
            }
        }
        #endregion

        #region Create Statics XML
        private CreateStaticsForm createStaticsForm = null; // Variable to store the instance of CreateStaticsForm
        private void createStaticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (createStaticsForm == null || createStaticsForm.IsDisposed)
            {
                createStaticsForm = new CreateStaticsForm();
                createStaticsForm.Show();
            }
            else
            {
                createStaticsForm.BringToFront(); // Bring the form to the front if it's already open
            }
        }
        #endregion

        #region XMLHexSearch

        private XMLHEXSearchForm xmlHexSearchForm = null; // Variable to store the instance of XMLHEXSearchForm
        private void xMLHexSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (xmlHexSearchForm == null || xmlHexSearchForm.IsDisposed)
            {
                xmlHexSearchForm = new XMLHEXSearchForm();
                xmlHexSearchForm.Show();
            }
            else
            {
                xmlHexSearchForm.BringToFront(); // Bring the form to the front if it's already open
            }
        }
        #endregion
    }
}
