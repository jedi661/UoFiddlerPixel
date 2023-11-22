using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
namespace Transition
{
	public class StaticTileCollection : CollectionBase
	{
		public StaticTile this[int index]
		{
			get
			{
				return (StaticTile)this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}
		public StaticTile RandomTile
		{
			get
			{
				int num = checked((int)Math.Round((double)unchecked(VBMath.Rnd() * (float)checked(this.List.Count - 1))));
				return (StaticTile)this.List[num];
			}
		}
		public void Add(StaticTile Value)
		{
			this.InnerList.Add(Value);
		}
		public void Remove(StaticTile Value)
		{
			this.InnerList.Remove(Value);
		}
		public void Save(XmlTextWriter xmlInfo)
		{
			xmlInfo.WriteStartElement("StaticTiles");

            IEnumerator enumerator = this.InnerList.GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					StaticTile staticTile = (StaticTile)enumerator.Current;
					staticTile.Save(xmlInfo);
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
            IEnumerator enumerator = xmlInfo.SelectNodes("StaticTiles").GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					XmlElement xmlElement = (XmlElement)enumerator.Current;

                    IEnumerator enumerator2 = xmlElement.SelectNodes("StaticTile").GetEnumerator();

					try
					{
						while (enumerator2.MoveNext())
						{
							XmlElement xmlInfo2 = (XmlElement)enumerator2.Current;
							this.InnerList.Add(new StaticTile(xmlInfo2));
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
					StaticTile item = (StaticTile)enumerator.Current;
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
