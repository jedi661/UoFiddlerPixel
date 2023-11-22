using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.IO;
using System.Xml;
namespace Transition
{
	public class ImportTiles
	{
		public ImportTiles(Collection[,] StaticMap, string iPath)
		{
			iPath += "\\Import Files\\";
			if (!Directory.Exists(iPath))
			{
				Interaction.MsgBox("No Import Tiles directory was found.", MsgBoxStyle.OkOnly, null);
			}
			else
			{
				this.ProcessDirectory(StaticMap, iPath);
			}
		}
		public void Load(Collection[,] StaticMap, string iFilename)
		{
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(iFilename);
				XmlElement xmlElement = (XmlElement)xmlDocument.SelectSingleNode("//Static_Tiles");

                IEnumerator enumerator = xmlElement.SelectNodes("Tile").GetEnumerator();

				try
				{
					while (enumerator.MoveNext())
					{
						XmlElement xmlElement2 = (XmlElement)enumerator.Current;
						short iTileID = XmlConvert.ToInt16(xmlElement2.GetAttribute("TileID"));
						short num = XmlConvert.ToInt16(xmlElement2.GetAttribute("X"));
						short num2 = XmlConvert.ToInt16(xmlElement2.GetAttribute("Y"));
						short iZ = XmlConvert.ToInt16(xmlElement2.GetAttribute("Z"));
						short iHue = XmlConvert.ToInt16(xmlElement2.GetAttribute("Hue"));
						StaticCell item = checked(new StaticCell(iTileID, (byte)(num % 8), (byte)(num2 % 8), iZ, iHue));
						StaticMap[(int)((short)(num >> 3)), (int)((short)(num2 >> 3))].Add(item, null, null, null);
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
			catch (Exception expr_FB)
			{
				ProjectData.SetProjectError(expr_FB);
				Interaction.MsgBox("Can not find:" + iFilename, MsgBoxStyle.OkOnly, null);
				ProjectData.ClearProjectError();
			}
		}
		public void ProcessDirectory(Collection[,] StaticMap, string targetDirectory)
		{
			string[] files = Directory.GetFiles(targetDirectory, "*.xml");
			string[] array = files;
			checked
			{
				for (int i = 0; i < array.Length; i++)
				{
					string iFilename = array[i];
					this.Load(StaticMap, iFilename);
				}
				string[] directories = Directory.GetDirectories(targetDirectory);
				string[] array2 = directories;
				for (int j = 0; j < array2.Length; j++)
				{
					string targetDirectory2 = array2[j];
					this.ProcessDirectory(StaticMap, targetDirectory2);
				}
			}
		}
	}
}
