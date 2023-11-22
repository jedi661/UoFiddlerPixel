using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
namespace Transition
{
	public class MapTileCollection : CollectionBase
	{
		public MapTile this[int index]
		{
			get
			{
				return (MapTile)this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}
		public MapTile RandomTile
		{
			get
			{
				int num = checked((int)Math.Round((double)unchecked(VBMath.Rnd() * (float)checked(this.List.Count - 1))));
				return (MapTile)this.List[num];
			}
		}
		public void Add(MapTile Value)
		{
			this.InnerList.Add(Value);
		}
		public void Remove(MapTile Value)
		{
			this.InnerList.Remove(Value);
		}
		public void Save(XmlTextWriter xmlInfo)
		{
			xmlInfo.WriteStartElement("MapTiles");

            IEnumerator enumerator = this.InnerList.GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					MapTile mapTile = (MapTile)enumerator.Current;
					mapTile.Save(xmlInfo);
				}
			}
			finally
			{
				if (enumerator is IDisposable)
				{
					((IDisposable)enumerator).Dispose();
				}
			}
			xmlInfo.WriteEndElement();
		}
		public void Load(XmlElement xmlInfo)
		{

            IEnumerator enumerator = xmlInfo.SelectNodes("MapTiles").GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					XmlElement xmlElement = (XmlElement)enumerator.Current;

                    IEnumerator enumerator2 = xmlElement.SelectNodes("MapTile").GetEnumerator();

					try
					{				
						while (enumerator2.MoveNext())
						{
							XmlElement xmlInfo2 = (XmlElement)enumerator2.Current;
							this.InnerList.Add(new MapTile(xmlInfo2));
						}
					}
					finally
					{
						if (enumerator2 is IDisposable)
						{
							((IDisposable)enumerator2).Dispose();
						}
					}
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
		public void Display(ListBox iList)
		{
			iList.Items.Clear();

            IEnumerator enumerator = this.InnerList.GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					MapTile item = (MapTile)enumerator.Current;
					iList.Items.Add(item);
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
	}
}
