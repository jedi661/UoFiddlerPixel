using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Forms;


using Terrain;
using Transition;
using Ultima;

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Helper
{
    public partial class CreateTransitions : Form
    {
        private VScrollBar _LandItems;
        private bool ImageTest;
        private ClsTerrainTable iGroups;
        private ClsTerrain iSelectedGroup;
        private ClsTerrain m_SelectedGroupA;
        private ClsTerrain m_SelectedGroupB;
        private ClsTerrain m_SelectedGroupC;
        private bool iSelected;
        private Transition.Transition iTransition;
        private TransitionTable iTransitionTable;

        public ClsTerrain Selected_Terrain_A
        {
            get
            {
                return this.m_SelectedGroupA;
            }
            set
            {
                this.m_SelectedGroupA = value;
            }
        }

        public ClsTerrain Selected_Terrain_B
        {
            get
            {
                return this.m_SelectedGroupB;
            }
            set
            {
                this.m_SelectedGroupB = value;
            }
        }

        public ClsTerrain Selected_Terrain_C
        {
            get
            {
                return this.m_SelectedGroupC;
            }
            set
            {
                this.m_SelectedGroupC = value;
            }
        }

        public CreateTransitions()
        {
            InitializeComponent();

            this.Load += new EventHandler(this.TEdit_Load);
            this.ImageTest = false;
            this.iGroups = new ClsTerrainTable();
            this.iSelected = false;
            this.iTransition = new Transition.Transition();
            this.iTransitionTable = new TransitionTable();
        }

        private void TEdit_Load(object sender, EventArgs e)
        {
            this.iGroups.Load();
            this.iGroups.Display(this.GroupSelect);
        }

        private void GroupSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.iSelectedGroup = (ClsTerrain)this.GroupSelect.SelectedItem;
            this.PictureBox3.Image = (Image)Art.GetLand((int)this.iSelectedGroup.TileID);
            this.Box_TileID.Text = StringType.FromInteger((int)this.iSelectedGroup.TileID);
            this.Box_TileID_Hex.Text = string.Format("{0:X4}", (object)this.iSelectedGroup.TileID);
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics1 = e.Graphics;
            this.LandImage.Image = (Image)null;
            this.StaticImage.Image = (Image)null;
            this.Box_Description.Text = this.iTransition.Description;
            this.Lbl_HashKey.Text = this.iTransition.HashKey;
            this.BoxFileName.Text = this.iTransition.File;
            this.iTransition.GetStaticTiles.Display(this.StaticTileList);
            this.iTransition.GetMapTiles.Display(this.MapTileList);
            graphics1.Clear(Color.LightGray);
            Graphics graphics2 = graphics1;
            Bitmap land1 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(0)).TileID);
            Point point1 = new Point(61, 15);
            Point point2 = point1;
            graphics2.DrawImage((Image)land1, point2);
            Graphics graphics3 = graphics1;
            Bitmap land2 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(1)).TileID);
            point1 = new Point(84, 38);
            Point point3 = point1;
            graphics3.DrawImage((Image)land2, point3);
            Graphics graphics4 = graphics1;
            Bitmap land3 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(2)).TileID);
            point1 = new Point(107, 61);
            Point point4 = point1;
            graphics4.DrawImage((Image)land3, point4);
            Graphics graphics5 = graphics1;
            Bitmap land4 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(3)).TileID);
            point1 = new Point(38, 38);
            Point point5 = point1;
            graphics5.DrawImage((Image)land4, point5);
            if (this.ImageTest)
            {
                Graphics graphics6 = graphics1;
                Bitmap land5 = Art.GetLand(IntegerType.FromString(this.Map_TileID.Text));
                point1 = new Point(61, 61);
                Point point6 = point1;
                graphics6.DrawImage((Image)land5, point6);
            }
            else
            {
                Graphics graphics6 = graphics1;
                Bitmap land5 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(4)).TileID);
                point1 = new Point(61, 61);
                Point point6 = point1;
                graphics6.DrawImage((Image)land5, point6);
            }
            Graphics graphics7 = graphics1;
            Bitmap land6 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(5)).TileID);
            point1 = new Point(84, 84);
            Point point7 = point1;
            graphics7.DrawImage((Image)land6, point7);
            Graphics graphics8 = graphics1;
            Bitmap land7 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(6)).TileID);
            point1 = new Point(15, 61);
            Point point8 = point1;
            graphics8.DrawImage((Image)land7, point8);
            Graphics graphics9 = graphics1;
            Bitmap land8 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(7)).TileID);
            point1 = new Point(38, 84);
            Point point9 = point1;
            graphics9.DrawImage((Image)land8, point9);
            Graphics graphics10 = graphics1;
            Bitmap land9 = Art.GetLand((int)this.iGroups.TerrianGroup((int)this.iTransition.GetKey(8)).TileID);
            point1 = new Point(61, 107);
            Point point10 = point1;
            graphics10.DrawImage((Image)land9, point10);
            Graphics graphics11 = graphics1;
            Pen pen1 = new Pen(Color.Red, 1f);
            point1 = new Point(82, 60);
            Point pt1_1 = point1;
            Point point11 = new Point(60, 82);
            Point pt2_1 = point11;
            graphics11.DrawLine(pen1, pt1_1, pt2_1);
            Graphics graphics12 = graphics1;
            Pen pen2 = new Pen(Color.Red, 1f);
            point11 = new Point(60, 83);
            Point pt1_2 = point11;
            point1 = new Point(82, 105);
            Point pt2_2 = point1;
            graphics12.DrawLine(pen2, pt1_2, pt2_2);
            Graphics graphics13 = graphics1;
            Pen pen3 = new Pen(Color.Red, 1f);
            point11 = new Point(83, 105);
            Point pt1_3 = point11;
            point1 = new Point(105, 83);
            Point pt2_3 = point1;
            graphics13.DrawLine(pen3, pt1_3, pt2_3);
            Graphics graphics14 = graphics1;
            Pen pen4 = new Pen(Color.Red, 1f);
            point11 = new Point(105, 82);
            Point pt1_4 = point11;
            point1 = new Point(83, 60);
            Point pt2_4 = point1;
            graphics14.DrawLine(pen4, pt1_4, pt2_4);
        }

        #region private void ToolBar1_ButtonClick(object sender, EventArgs e)

        private void ToolBarButton1_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }

        private void ToolBarButton2_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }

        private void ToolBarButton3_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }

        private void ToolBarButton4_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }

        private void ToolBarButton5_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }

        private void ToolBarButton6_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }

        private void ToolBarButton7_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }

        private void ToolBarButton8_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            ClsTerrain clsTerrain = (ClsTerrain)this.GroupSelect.SelectedItem;
            if (clsTerrain == null)
                return;
            this.iTransition.SetHashKey(IntegerType.FromObject(button.Tag), checked((byte)clsTerrain.GroupID));
            this.PictureBox1.Refresh();
        }
        #endregion

        private void Btn_Land_Click(object sender, EventArgs e)
        {
            try
            {
                int index = IntegerType.FromString(this.Map_TileID.Text);
                this.LandItems.Value = index;
                if (Art.GetLand(index) == null)
                    this.LandImage.Image = (Image)null;
                else
                    this.LandImage.Image = (Image)Art.GetLand(index);
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                int num = (int)Interaction.MsgBox((object)this.ErrorMessage(this.Map_TileID.Text, "1", "16535"), MsgBoxStyle.OkOnly, (object)null);
                ProjectData.ClearProjectError();
            }
        }

        private void Btn_Land_Hex_Click(object sender, EventArgs e)
        {
            try
            {
                int index = IntegerType.FromString(this.Map_TileID_Hex.Text);
                this.LandItems.Value = index;
                this.Map_TileID.Text = index.ToString();
                if (Art.GetLand(index) == null)
                    this.LandImage.Image = (Image)null;
                else
                    this.LandImage.Image = (Image)Art.GetLand(index);
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                int num = (int)Interaction.MsgBox((object)this.ErrorMessage(this.Map_TileID_Hex.Text, "&H0000", "&H3FFF"), MsgBoxStyle.OkOnly, (object)null);
                ProjectData.ClearProjectError();
            }
        }

        private void Btn_Static_Click(object sender, EventArgs e)
        {
            try
            {
                int index = IntegerType.FromString(this.Static_TileID.Text);
                this.StaticItems.Value = index;
                if (Art.GetStatic(index) == null)
                    this.StaticImage.Image = (Image)null;
                else
                    this.StaticImage.Image = (Image)Art.GetStatic(index);
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                int num = (int)Interaction.MsgBox((object)this.ErrorMessage(this.Static_TileID.Text, "1", "16535"), MsgBoxStyle.OkOnly, (object)null);
                ProjectData.ClearProjectError();
            }
        }

        private void Btn_Static_Hex_Click(object sender, EventArgs e)
        {
            try
            {
                int index = IntegerType.FromString(this.Static_TileID_Hex.Text);
                this.StaticItems.Value = index;
                this.Static_TileID.Text = index.ToString();
                if (Art.GetStatic(index) == null)
                    this.StaticImage.Image = (Image)null;
                else
                    this.StaticImage.Image = (Image)Art.GetStatic(index);
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                int num = (int)Interaction.MsgBox((object)this.ErrorMessage(this.Static_TileID_Hex.Text, "&H0000", "&H3FFF"), MsgBoxStyle.OkOnly, (object)null);
                ProjectData.ClearProjectError();
            }
        }

        #region ErrorMessage
        private string ErrorMessage(string iValue, string iMin, string iMax)
        {
            return string.Format("{0} is outside the range\r\nPlease enter a value between {1} and {2}", (object)iValue, (object)iMin, (object)iMax);
        }
        #endregion

        private void LandItems_Scroll(object sender, ScrollEventArgs e)
        {
            this.Map_TileID.Text = this.LandItems.Value.ToString();
            this.Map_TileID_Hex.Text = "&H" + Conversion.Hex(this.LandItems.Value);
            if (Art.GetLand(this.LandItems.Value) == null)
                this.LandImage.Image = (Image)null;
            else
                this.LandImage.Image = (Image)Art.GetLand(this.LandItems.Value);
        }

        #region LandItems_ValueChanged
        private void LandItems_ValueChanged(object sender, EventArgs e)
        {
            this.Map_TileID.Text = this.LandItems.Value.ToString();
            this.Map_TileID_Hex.Text = "&H" + Conversion.Hex(this.LandItems.Value);
            if (Art.GetLand(this.LandItems.Value) == null)
                this.LandImage.Image = (Image)null;
            else
                this.LandImage.Image = (Image)Art.GetLand(this.LandItems.Value);
        }
        #endregion

        #region StaticItems_Scroll
        private void StaticItems_Scroll(object sender, ScrollEventArgs e)
        {
            this.Static_TileID.Text = this.StaticItems.Value.ToString();
            this.Static_TileID_Hex.Text = "&H" + Conversion.Hex(this.StaticItems.Value);
            if (Art.GetStatic(this.StaticItems.Value) == null)
                this.StaticImage.Image = (Image)null;
            else
                this.StaticImage.Image = (Image)Art.GetStatic(this.StaticItems.Value);
        }
        #endregion

        #region StaticItems_ValueChanged
        private void StaticItems_ValueChanged(object sender, EventArgs e)
        {
            this.Static_TileID.Text = this.StaticItems.Value.ToString();
            this.Static_TileID_Hex.Text = "&H" + Conversion.Hex(this.StaticItems.Value);
            if (Art.GetStatic(this.StaticItems.Value) == null)
                this.StaticImage.Image = (Image)null;
            else
                this.StaticImage.Image = (Image)Art.GetStatic(this.StaticItems.Value);
        }
        #endregion

        #region ToolBarButton10 
        private void ToolBarButton10_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, (object)"Add", false) == 0)
            {
                if (StringType.StrCmp(this.Map_TileID.Text, string.Empty, false) == 0)
                    return;
                this.iTransition.AddMapTile(ShortType.FromString(this.Map_TileID.Text), Convert.ToInt16(this.Map_AltIDMod.Value));
                this.iTransition.GetMapTiles.Display(this.MapTileList);
            }
        }
        #endregion

        #region ToolBarButton11
        private void ToolBarButton11_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, (object)"Delete", false) == 0)
            {
                MapTile iMapTile = (MapTile)this.MapTileList.SelectedItem;
                if (iMapTile == null)
                    return;
                this.LandImage.Image = (Image)null;
                this.iTransition.RemoveMapTile(iMapTile);
                this.iTransition.GetMapTiles.Display(this.MapTileList);
            }
        }
        #endregion

        #region ToolBarButton12
        private void ToolBarButton12_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, (object)"Test", false) == 0)
            {
                if (StringType.StrCmp(this.Map_TileID.Text, string.Empty, false) == 0)
                    return;
                this.ImageTest = !this.ImageTest;
                this.PictureBox1.Refresh();
                this.LandImage.Image = Art.GetLand(IntegerType.FromString(this.Map_TileID.Text)) != null ? (Image)Art.GetLand(IntegerType.FromString(this.Map_TileID.Text)) : (Image)null;
            }
        }
        #endregion

        #region mapZoom
        private MapZoom mapZoom;
        private void ToolBarButton13_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, (object)"Select", false) == 0)
            {
                if (mapZoom == null || mapZoom.IsDisposed)
                {
                    mapZoom = new MapZoom();
                    mapZoom.Tag = (object)this.LandItems;
                    mapZoom.Show();
                }
                else
                {
                    // Das Formular ist bereits geöffnet. Sie können es in den Vordergrund bringen, wenn Sie möchten.
                    mapZoom.BringToFront();
                }
            }
        }
        #endregion

        #region ToolBarButton14
        private void ToolBarButton14_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, (object)"Add", false) == 0)
            {
                if (StringType.StrCmp(this.Static_TileID.Text, string.Empty, false) == 0)
                    return;
                this.iTransition.AddStaticTile(ShortType.FromString(this.Static_TileID.Text), Convert.ToInt16(this.Static_AltIDMod.Value));
                this.iTransition.GetStaticTiles.Display(this.StaticTileList);
            }
        }
        #endregion

        #region ToolBarButton15
        private void ToolBarButton15_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, (object)"Delete", false) == 0)
            {
                Transition.StaticTile iStaticTile = (Transition.StaticTile)this.StaticTileList.SelectedItem;
                if (iStaticTile == null)
                    return;
                this.StaticImage.Image = (Image)null;
                this.iTransition.RemoveStaticTile(iStaticTile);
                this.iTransition.GetStaticTiles.Display(this.StaticTileList);
            }
        }
        #endregion

        #region ToolBarButton16
        private StaticZoom staticZoom;
        private void ToolBarButton16_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            if (button == null)
            {
                return;
            }

            object tag = button.Tag;
            if (ObjectType.ObjTst(tag, (object)"Select", false) == 0)
            {
                if (staticZoom == null || staticZoom.IsDisposed)
                {
                    staticZoom = new StaticZoom();
                    staticZoom.Tag = (object)this.StaticItems;
                    staticZoom.Show();
                }
                else
                {
                    // Das Formular ist bereits geöffnet. Sie können es in den Vordergrund bringen, wenn Sie möchten.
                    staticZoom.BringToFront();
                }
            }
        }
        #endregion

        #region MenuTerrainA
        private GroupSelect groupSelectA;
        private void MenuTerrainA_Click(object sender, EventArgs e)
        {
            if (groupSelectA == null || groupSelectA.IsDisposed)
            {
                groupSelectA = new GroupSelect();
                Label selectGroupNameLabel = groupSelectA.SelectGroupName as Label;
                if (selectGroupNameLabel != null)
                {
                    selectGroupNameLabel.Text = "Select Group A";
                }
                groupSelectA.Tag = (object)this;
                groupSelectA.Show();
            }
            else
            {
                groupSelectA.BringToFront();
            }
        }
        #endregion

        #region MenuTerrainB
        private GroupSelect groupSelectB;
        private void MenuTerrainB_Click(object sender, EventArgs e)
        {
            if (groupSelectB == null || groupSelectB.IsDisposed)
            {
                groupSelectB = new GroupSelect();
                Label selectGroupNameLabel = groupSelectB.SelectGroupName as Label;
                if (selectGroupNameLabel != null)
                {
                    selectGroupNameLabel.Text = "Select Group B";
                }
                groupSelectB.Tag = (object)this;
                groupSelectB.Show();
            }
            else
            {
                // Das Formular ist bereits geöffnet. Sie können es in den Vordergrund bringen, wenn Sie möchten.
                groupSelectB.BringToFront();
            }
        }
        #endregion

        #region MenuTerrainC
        private GroupSelect groupSelectC;
        private void MenuTerrainC_Click(object sender, EventArgs e)
        {
            if (groupSelectC == null || groupSelectC.IsDisposed)
            {
                groupSelectC = new GroupSelect();
                Label selectGroupNameLabel = groupSelectC.SelectGroupName as Label;
                if (selectGroupNameLabel != null)
                {
                    selectGroupNameLabel.Text = "Select Group C";
                }
                groupSelectC.Tag = (object)this;
                groupSelectC.Show();
            }
            else
            {
                // Das Formular ist bereits geöffnet. Sie können es in den Vordergrund bringen, wenn Sie möchten.
                groupSelectC.BringToFront();
            }
        }
        #endregion

        #region ListBox1_SelectedIndexChanged
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Transition.Transition transition = (Transition.Transition)this.ListBox1.SelectedItem;
            if (transition == null)
                return;
            this.iTransition = transition;
            this.PictureBox1.Refresh();
        }
        #endregion

        #region MenuNew
        private void MenuNew_Click(object sender, EventArgs e)
        {
            this.iTransitionTable.Clear();
            this.iTransitionTable.Display(this.ListBox1);
            this.iTransition = new Transition.Transition();
            this.PictureBox1.Refresh();
        }
        #endregion

        #region MenuLoad
        private void MenuLoad_Click(object sender, EventArgs e)
        {
            this.iTransitionTable.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            this.iTransitionTable.Load(openFileDialog.FileName);
            this.iTransitionTable.Display(this.ListBox1);
        }
        #endregion

        #region MenuSave
        private void MenuSave_Click(object sender, EventArgs e)
        {
            this.iTransitionTable.Save(this.Box_Description.Text);
        }
        #endregion

        #region MapTileList
        private void MapTileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapTile mapTile = (MapTile)this.MapTileList.SelectedItem;
            if (mapTile == null)
                return;
            this.Map_TileID.Text = mapTile.TileID.ToString();
            this.Map_TileID_Hex.Text = "&H" + Conversion.Hex(mapTile.TileID);
            this.LandItems.Value = (int)mapTile.TileID;
            this.LandImage.Image = Art.GetLand((int)mapTile.TileID) != null ? (Image)Art.GetLand((int)mapTile.TileID) : (Image)null;
        }
        #endregion

        #region StaticTileList_SelectedIndexChanged
        private void StaticTileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Transition.StaticTile staticTile = (Transition.StaticTile)this.StaticTileList.SelectedItem;
            if (staticTile == null)
                return;
            this.Static_TileID.Text = staticTile.TileID.ToString();
            this.Static_TileID_Hex.Text = "&H" + Conversion.Hex(staticTile.TileID);
            this.StaticItems.Value = (int)staticTile.TileID;
            this.StaticImage.Image = Art.GetStatic((int)staticTile.TileID) != null ? (Image)Art.GetStatic((int)staticTile.TileID) : (Image)null;
        }
        #endregion

        #region MenuAddKey
        private void MenuAddKey_Click(object sender, EventArgs e)
        {
            this.iTransitionTable.Add(this.iTransition);
            this.iTransition = new Transition.Transition();
            this.iTransitionTable.Display(this.ListBox1);
            this.PictureBox1.Refresh();
        }
        #endregion

        #region MenuDelKey
        private void MenuDelKey_Click(object sender, EventArgs e)
        {
            Transition.Transition iValue = (Transition.Transition)this.ListBox1.SelectedItem;
            if (iValue == null)
                return;
            this.iTransitionTable.Remove(iValue);
            this.iTransitionTable.Display(this.ListBox1);
        }
        #endregion

        #region MenuCopyKey
        private void MenuCopyKey_Click(object sender, EventArgs e)
        {
            this.iTransitionTable.Add(this.iTransition);
            this.iTransitionTable.Display(this.ListBox1);
            this.PictureBox1.Refresh();
        }
        #endregion

        #region MenuItem1
        private void MenuItem1_Click(object sender, EventArgs e)
        {
            this.iTransition = new Transition.Transition();
            this.PictureBox1.Refresh();
        }
        #endregion

        #region BoxFileName_TextChanged
        private void BoxFileName_TextChanged(object sender, EventArgs e)
        {
            this.iTransition.File = this.BoxFileName.Text;
            this.iTransitionTable.Display(this.ListBox1);
        }
        #endregion

        #region Btn_StaticFile
        private void Btn_StaticFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Data\\Statics";
            openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            this.BoxFileName.Text = new FileInfo(openFileDialog.FileName).Name;
        }
        #endregion

        #region Box_Description_Leave
        private void Box_Description_Leave(object sender, EventArgs e)
        {
            this.iTransition.Description = this.Box_Description.Text;
            this.iTransitionTable.Display(this.ListBox1);
        }
        #endregion

        #region Menu2Way
        private void Menu2Way_Click(object sender, EventArgs e)
        {
            if (this.m_SelectedGroupA == null || this.m_SelectedGroupB == null || StringType.StrCmp(this.m_SelectedGroupA.Name, this.m_SelectedGroupB.Name, false) == 0)
                return;
            string iDescription = string.Format("{0} To {1}", (object)this.m_SelectedGroupA.Name, (object)this.m_SelectedGroupB.Name);
            string filename = string.Format("{0}Data\\System\\2Way_Template.xml", (object)AppDomain.CurrentDomain.BaseDirectory);
            XmlDocument xmlDocument = new XmlDocument();
            this.iTransitionTable.Clear();
            try
            {
                xmlDocument.Load(filename);
                try
                {
                    foreach (XmlElement xmlElement in xmlDocument.SelectNodes("//Wizard/Tile"))
                    {
                        string attribute = xmlElement.GetAttribute("Pattern");
                        this.iTransitionTable.Add(new Transition.Transition(iDescription, this.m_SelectedGroupA, this.m_SelectedGroupB, attribute));
                    }
                }
                finally
                {
                    IEnumerator enumerator = null;
                    if (enumerator is IDisposable)
                        ((IDisposable)enumerator).Dispose();
                }
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                int num = (int)Interaction.MsgBox((object)string.Format("XMLFile:{0}", (object)filename), MsgBoxStyle.OkOnly, (object)null);
                ProjectData.ClearProjectError();
            }
            this.iTransitionTable.Display(this.ListBox1);
        }
        #endregion

        #region Menu3Way
        private void Menu3Way_Click(object sender, EventArgs e)
        {
            if (this.m_SelectedGroupA == null || this.m_SelectedGroupB == null || (this.m_SelectedGroupC == null || StringType.StrCmp(this.m_SelectedGroupA.Name, this.m_SelectedGroupB.Name, false) == 0))
                return;
            string iDescription = string.Format("{0}-{1}-{2}", (object)this.m_SelectedGroupA.Name, (object)this.m_SelectedGroupB.Name, (object)this.m_SelectedGroupC.Name);
            string filename = string.Format("{0}Data\\System\\3Way_Template.xml", (object)AppDomain.CurrentDomain.BaseDirectory);
            XmlDocument xmlDocument = new XmlDocument();
            this.iTransitionTable.Clear();
            try
            {
                xmlDocument.Load(filename);
                try
                {
                    foreach (XmlElement xmlElement in xmlDocument.SelectNodes("//Wizard/Tile"))
                    {
                        string attribute = xmlElement.GetAttribute("Pattern");
                        this.iTransitionTable.Add(new Transition.Transition(iDescription, this.m_SelectedGroupA, this.m_SelectedGroupB, this.m_SelectedGroupC, attribute));
                    }
                }
                finally
                {
                    IEnumerator enumerator = null;
                    if (enumerator is IDisposable)
                        ((IDisposable)enumerator).Dispose();
                }
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                int num = (int)Interaction.MsgBox((object)string.Format("XMLFile:{0}", (object)filename), MsgBoxStyle.OkOnly, (object)null);
                ProjectData.ClearProjectError();
            }
            this.iTransitionTable.Display(this.ListBox1);
        }
        #endregion

        #region MenuItem3
        private void MenuItem3_Click(object sender, EventArgs e)
        {
            PrintTransition printTransition = new PrintTransition();
        }
        #endregion

        #region Menu_CloneGroupA
        private void Menu_CloneGroupA_Click(object sender, EventArgs e)
        {
            GroupSelect groupSelect = new GroupSelect();
            Label selectGroupNameLabel = groupSelect.SelectGroupName as Label;
            if (selectGroupNameLabel != null)
            {
                selectGroupNameLabel.Text = "Clone Group A";
            }
            groupSelect.Tag = (object)this;
            groupSelect.Show();
        }

        #endregion

        #region Menu_CloneGroupB
        private void Menu_CloneGroupB_Click(object sender, EventArgs e)
        {
            GroupSelect groupSelect = new GroupSelect();
            Label selectGroupNameLabel = groupSelect.SelectGroupName as Label;
            if (selectGroupNameLabel != null)
            {
                selectGroupNameLabel.Text = "Clone Group B";
            }
            groupSelect.Tag = (object)this;
            groupSelect.Show();
        }
        #endregion

        #region MenuItem10
        private void MenuItem10_Click(object sender, EventArgs e)
        {
            if (this.m_SelectedGroupA == null || this.m_SelectedGroupB == null)
                return;
            this.iTransitionTable.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                this.iTransitionTable.Load(openFileDialog.FileName);
            try
            {
                foreach (Transition.Transition transition in (IEnumerable)this.iTransitionTable.GetTransitionTable.Values)
                    transition.Clone(this.m_SelectedGroupA, this.m_SelectedGroupB);
            }
            finally
            {
                IEnumerator enumerator = null;
                if (enumerator is IDisposable)
                    ((IDisposable)enumerator).Dispose();
            }
            this.iTransitionTable.Save(openFileDialog.FileName.Replace(this.m_SelectedGroupA.Name, this.m_SelectedGroupB.Name));
        }
        #endregion

        #region
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Helper.TransitionWizard transitionWizardForm = null;
        private void launchTransitionWizardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.transitionWizardForm == null || this.transitionWizardForm.IsDisposed)
            {
                this.transitionWizardForm = new UoFiddler.Plugin.ConverterMultiTextPlugin.Helper.TransitionWizard();
                this.transitionWizardForm.Show();
            }
            else
            {
                // Das Formular ist bereits geöffnet, also bringen wir es in den Vordergrund
                this.transitionWizardForm.BringToFront();
            }
        }
        #endregion

    }
}
