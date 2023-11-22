using Microsoft.VisualBasic.CompilerServices;
using System;
namespace Transition
{
	public class HashKey
	{
		private byte m_Key;
		public byte Key
		{
			get
			{
				return this.m_Key;
			}
			set
			{
				this.m_Key = value;
			}
		}
		public HashKey()
		{
			this.m_Key = 0;
		}
		public HashKey(int Key)
		{
			this.m_Key = Convert.ToByte(Key);
		}
		public HashKey(byte Key)
		{
			this.m_Key = Key;
		}
		public HashKey(string Key)
		{
			this.m_Key = ByteType.FromString("&H" + Key);
		}
		public override string ToString()
		{
			return string.Format("{0:X2}", this.m_Key);
		}
	}
}
