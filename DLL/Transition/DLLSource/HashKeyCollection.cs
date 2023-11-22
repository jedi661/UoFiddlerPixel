using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Reflection;

namespace Transition
{
  public class HashKeyCollection : CollectionBase
  {
    public HashKey this[int index]
    {
      get
      {
        return (HashKey)this.List[index];
      }
      set
      {
        this.List[index] = value;
      }
    }

    public HashKeyCollection()
    {
    }

    public void Add(HashKey Value)
    {
      if (this.InnerList.Count <= 9)
      {
        this.InnerList.Add(Value);
      }
    }

    public void AddHashKey(string Value)
    {
      this.InnerList.Clear();
      byte num = 0;
      do
      {
        this.Add(new HashKey(Strings.Mid(Value, checked(checked(num * 2) + 1), 2)));
        num = checked((byte)(num + 1));
      }
      while (num <= 8);
    }

    public void Remove(HashKey Value)
    {
      this.InnerList.Remove(Value);
    }

    public override string ToString()
    {
      object[] key = new object[] { this[0].Key, this[1].Key, this[2].Key, this[3].Key, this[4].Key, this[5].Key, this[6].Key, this[7].Key, this[8].Key };
      return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}", key);
    }
  }
}