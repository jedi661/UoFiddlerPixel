using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Transition
{
	public class RandomStaticCollection : CollectionBase
	{
		private string m_Description;
		private int m_Freq;
		public Collection iCollection
		{
			get
			{
				return (Collection)this.List;
			}
		}
		public string Description
		{
			get
			{
				return this.m_Description;
			}
			set
			{
				this.m_Description = value;
			}
		}
		public int Freq
		{
			get
			{
				return this.m_Freq;
			}
			set
			{
				this.m_Freq = value;
			}
		}
		public RandomStatic this[int index]
		{
			get
			{
				return (RandomStatic)this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}
		public void Add(RandomStatic Value)
		{
			this.InnerList.Add(Value);
		}
		public void Remove(RandomStatic Value)
		{
			this.InnerList.Remove(Value);
		}
		public void Save(XmlTextWriter xmlInfo)
		{
			xmlInfo.WriteStartElement("Statics");
			xmlInfo.WriteAttributeString("Description", this.m_Description);
			xmlInfo.WriteAttributeString("Freq", StringType.FromInteger(this.m_Freq));

            IEnumerator enumerator = this.InnerList.GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					RandomStatic randomStatic = (RandomStatic)enumerator.Current;
					randomStatic.Save(xmlInfo);
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
		public void Display(ListBox iList)
		{
			iList.Items.Clear();
			try
			{
                foreach (RandomStatic item in InnerList)
				{
					iList.Items.Add(item);
				}
			}
            catch
            {

            }
		}
        public void RandomStatic(int X, int Y, int Z, Collection[,] StaticMap)  // Ändern Sie den Datentyp von short zu int für X, Y und Z
        {
            IEnumerator enumerator = this.InnerList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    RandomStatic randomStatic = (RandomStatic)enumerator.Current;
                    int x = X + randomStatic.X;
                    int y = Y + randomStatic.Y;
                    // statics that would land outside of the map are skipped (used to throw and abort the map build)
                    if (x < 0 || y < 0 || (x >> 3) >= StaticMap.GetLength(0) || (y >> 3) >= StaticMap.GetLength(1))
                    {
                        continue;
                    }
                    StaticCell item = new StaticCell(randomStatic.TileID, (byte)(x % 8), (byte)(y % 8), Z + randomStatic.Z);
                    StaticMap[x >> 3, y >> 3].Add(item, null, null, null);
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

        public override string ToString()
		{
			return string.Format("{0} Freq:{1}", this.m_Description, this.m_Freq);
		}
		public RandomStaticCollection()
		{
		}
		public RandomStaticCollection(string iDescription, int iFreq)
		{
			this.m_Description = iDescription;
			this.m_Freq = iFreq;
		}
		public RandomStaticCollection(XmlElement xmlInfo)
		{
			this.m_Description = xmlInfo.GetAttribute("Description");
			this.m_Freq = (int)XmlConvert.ToInt16(xmlInfo.GetAttribute("Freq"));

            IEnumerator enumerator = xmlInfo.SelectNodes("Static").GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					XmlElement xmlInfo2 = (XmlElement)enumerator.Current;
					this.InnerList.Add(new RandomStatic(xmlInfo2));
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
