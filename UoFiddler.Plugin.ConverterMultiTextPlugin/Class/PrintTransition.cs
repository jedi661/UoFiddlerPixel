using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    internal class PrintTransition
    {
        [AccessedThroughProperty("PrDoc")]
        private PrintDocument _PrDoc;
        private XmlDocument iXml;
        private Font mFont;
        private XmlElement iElement;
        private string iKey;
        private string iMapTile;
        private string iStaticTile;
        private float xPos;
        private float yPos;

        public virtual PrintDocument PrDoc
        {
            get { return this._PrDoc; }
            set
            {
                if (this._PrDoc != null)
                {
                    PrintTransition printTransition = this;
                    this._PrDoc.PrintPage -= new PrintPageEventHandler(printTransition.PrDoc_PrintPage);
                }
                this._PrDoc = value;
                if (this._PrDoc != null)
                {
                    PrintTransition printTransition1 = this;
                    this._PrDoc.PrintPage += new PrintPageEventHandler(printTransition1.PrDoc_PrintPage);
                }
            }
        }

        public PrintTransition()
        {
            this.iXml = new XmlDocument();
            this.PrDoc = new PrintDocument();
            this.mFont = new Font("Arial", 10f);
            string str = string.Format("{0}Data\\System\\2Way_Template.xml", AppDomain.CurrentDomain.BaseDirectory);
            this.iXml.Load(str);
            PrintDialog printDialog = new PrintDialog() { Document = this.PrDoc };
            this.iElement = (XmlElement)this.iXml.SelectSingleNode("//Wizard/Tile");
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                this.PrDoc.Print();
            }
        }

        private void PrDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Rectangle rectangle = new Rectangle();
            Point point = new Point();
            Point point1;
            float single;
            Rectangle marginBounds = e.MarginBounds;
            this.yPos = (float)marginBounds.Top;
            marginBounds = e.MarginBounds;
            this.xPos = (float)marginBounds.Left;
            Bitmap land = Art.GetLand(3);
            Bitmap bitmap = Art.GetLand(22);
            do
            {
                this.iKey = this.iElement.GetAttribute("Pattern");
                this.iMapTile = this.iElement.GetAttribute("MapTile");
                this.iStaticTile = this.iElement.GetAttribute("StaticTile");
                Graphics graphics = e.Graphics;
                Bitmap bitmap1 = new Bitmap(28, 28);
                Graphics graphic = Graphics.FromImage(bitmap1);
                byte num = 0;
                do
                {
                    switch (num)
                    {
                        case 0:
                            {
                                marginBounds = new Rectangle(0, 0, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                        case 1:
                            {
                                marginBounds = new Rectangle(9, 0, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                        case 2:
                            {
                                marginBounds = new Rectangle(18, 0, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                        case 3:
                            {
                                marginBounds = new Rectangle(0, 9, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                        case 4:
                            {
                                marginBounds = new Rectangle(9, 9, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                        case 5:
                            {
                                marginBounds = new Rectangle(18, 9, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                        case 6:
                            {
                                marginBounds = new Rectangle(0, 18, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                        case 7:
                            {
                                marginBounds = new Rectangle(9, 18, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                        case 8:
                            {
                                marginBounds = new Rectangle(18, 18, 7, 7);
                                rectangle = marginBounds;
                                break;
                            }
                    }
                    string str = Strings.Mid(this.iKey, checked(num + 1), 1);
                    if (StringType.StrCmp(str, "A", false) == 0)
                    {
                        graphic.FillRectangle(Brushes.LightGray, rectangle);
                    }
                    else if (StringType.StrCmp(str, "B", false) == 0)
                    {
                        graphic.FillRectangle(Brushes.DarkGray, rectangle);
                    }
                    num = checked((byte)(num + 1));
                }
                while (num <= 8);
                graphic = null;
                Bitmap bitmap2 = new Bitmap(134, 134);
                Graphics graphic1 = Graphics.FromImage(bitmap2);
                byte num1 = 0;
                do
                {
                    switch (num1)
                    {
                        case 0:
                            {
                                point1 = new Point(44, 0);
                                point = point1;
                                break;
                            }
                        case 1:
                            {
                                point1 = new Point(66, 22);
                                point = point1;
                                break;
                            }
                        case 2:
                            {
                                point1 = new Point(88, 44);
                                point = point1;
                                break;
                            }
                        case 3:
                            {
                                point1 = new Point(22, 22);
                                point = point1;
                                break;
                            }
                        case 4:
                            {
                                point1 = new Point(44, 44);
                                point = point1;
                                break;
                            }
                        case 5:
                            {
                                point1 = new Point(66, 66);
                                point = point1;
                                break;
                            }
                        case 6:
                            {
                                point1 = new Point(0, 44);
                                point = point1;
                                break;
                            }
                        case 7:
                            {
                                point1 = new Point(22, 66);
                                point = point1;
                                break;
                            }
                        case 8:
                            {
                                point1 = new Point(44, 88);
                                point = point1;
                                break;
                            }
                    }
                    string str1 = Strings.Mid(this.iKey, checked(num1 + 1), 1);
                    if (StringType.StrCmp(str1, "A", false) == 0)
                    {
                        graphic1.DrawImage(land, point);
                    }
                    else if (StringType.StrCmp(str1, "B", false) == 0)
                    {
                        graphic1.DrawImage(bitmap, point);
                    }
                    num1 = checked((byte)(num1 + 1));
                }
                while (num1 <= 8);
                graphic1 = null;
                graphics.DrawLine(new Pen(Color.Black), this.xPos, this.yPos, this.xPos + 600f, this.yPos);
                point1 = new Point(checked((int)Math.Round((double)this.xPos)), checked((int)Math.Round((double)((float)(this.yPos + 2f)))));
                graphics.DrawImage(bitmap2, point1);
                point1 = new Point(checked((int)Math.Round((double)((float)(this.xPos + 150f)))), checked((int)Math.Round((double)((float)(this.yPos + 2f)))));
                graphics.DrawImage(bitmap1, point1);
                graphics.DrawString(this.iKey, this.mFont, Brushes.Black, this.xPos + 190f, this.yPos);
                graphics.DrawString(string.Format("Map Tile:{0}", this.iMapTile), this.mFont, Brushes.Black, this.xPos + 340f, this.yPos);
                graphics.DrawString(string.Format("Static Tile:{0}", this.iStaticTile), this.mFont, Brushes.Black, this.xPos + 340f, this.yPos + 20f);
                graphics = null;
                this.yPos = this.yPos + 140f;
                if (this.iElement.NextSibling != null)
                {
                    this.iElement = (XmlElement)this.iElement.NextSibling;
                }
                single = this.yPos;
                marginBounds = e.MarginBounds;
            }
            while (!(single > (float)(checked(marginBounds.Bottom - 140)) | this.iElement.NextSibling == null));
            if (this.iElement.NextSibling != null)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
            }
        }
    }
}
