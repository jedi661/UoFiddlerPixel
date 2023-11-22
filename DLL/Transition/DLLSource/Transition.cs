using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ConstrainedExecution;
using System.Xml;
using Terrain;
using Ultima;
namespace Transition
{
	public class Transition
	{
		private string m_Description;
		private HashKeyCollection m_HashKey;
		private StaticTileCollection m_StaticTiles;
		private MapTileCollection m_MapTiles;
		private RandomStatics m_RandomTiles;
		private string m_File;
		public string File
		{
			get
			{
				return this.m_File;
			}
			set
			{
				this.m_File = value;
			}
		}
        #region Description
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
        #endregion
        #region HashKey
        public string HashKey
		{
			get
			{
				return this.m_HashKey.ToString();
			}
			set
			{
				byte b = 0;
				do
				{
					this.m_HashKey.Add(new HashKey(Strings.Mid(value, (int)checked(b * 2 + 1), 2)));
					b += 1;
				}
				while (b <= 8);
			}
		}
        #endregion
        #region MapTileCollection
        public MapTileCollection GetMapTiles
		{
			get
			{
				return this.m_MapTiles;
			}
			set
			{
				this.m_MapTiles = value;
			}
		}
        #endregion
        #region StaticTileCollection
        public StaticTileCollection GetStaticTiles
		{
			get
			{
				return this.m_StaticTiles;
			}
			set
			{
				this.m_StaticTiles = value;
			}
		}
        #endregion
        #region HashKeyCollection
        public HashKeyCollection GetHaskKeyTable
		{
			get
			{
				return this.m_HashKey;
			}
		}
        #endregion
        #region GetKey
        public byte GetKey(int Index)
		{		
				return this.m_HashKey[Index].Key;
		}
        #endregion
        #region virtual MapTile
        public virtual MapTile GetRandomMapTile()
		{
            MapTile randomTile = null;
			if (this.GetMapTiles.Count > 0)
			{
				randomTile = this.m_MapTiles.RandomTile;
			}
			return randomTile;
		}
        #endregion
        #region GetRandomStaticTiles
        public virtual void GetRandomStaticTiles(short X, short Y, short Z, Collection[,] StaticMap, bool iRandom)
		{
			if (this.m_StaticTiles.Count > 0)
			{
				StaticTile randomTile = this.m_StaticTiles.RandomTile;
				StaticMap[(int)((short)(X >> 3)), (int)((short)(Y >> 3))].Add(new StaticCell(randomTile.TileID, checked((byte)(X % 8)), checked((byte)(Y % 8)), (short)(Z + randomTile.AltIDMod)), null, null, null);
			}
			if (iRandom)
			{
				if (this.m_RandomTiles != null)
				{
					this.m_RandomTiles.GetRandomStatic(X, Y, Z, StaticMap);
				}
			}
		}
        #endregion
        #region Clone ClsTerrain
        public void Clone(ClsTerrain iGroupA, ClsTerrain iGroupB)
		{
			this.m_Description = this.m_Description.Replace(iGroupA.Name, iGroupB.Name);
			int num = 0;
			checked
			{
				do
				{
					if ((int)this.m_HashKey[num].Key == iGroupA.GroupID)
					{
						this.m_HashKey[num].Key = (byte)iGroupB.GroupID;
					}
					num++;
				}
				while (num <= 8);
			}
		}
        #endregion

        #region SetHashKey
        public void SetHashKey(int iKey, byte iKeyHash)
		{
			this.m_HashKey[iKey].Key = iKeyHash;
		}
        #endregion
        #region AddMapTile
        public void AddMapTile(short TileID, short AltIDMod)
		{
			this.m_MapTiles.Add(new MapTile(TileID, AltIDMod));
		}
        #endregion
        #region RemoveMapTile
        public void RemoveMapTile(MapTile iMapTile)
		{
			this.m_MapTiles.Remove(iMapTile);
		}
        #endregion
        #region AddStaticTile
        public void AddStaticTile(short TileID, short AltIDMod)
		{
			this.m_StaticTiles.Add(new StaticTile(TileID, AltIDMod));
		}
        #endregion
        #region RemoveStaticTile
        public void RemoveStaticTile(StaticTile iStaticTile)
		{
			this.m_StaticTiles.Remove(iStaticTile);
		}
        #endregion
        #region override string
        public override string ToString()
		{
			return string.Format("[{0}] {1}", this.m_HashKey.ToString(), this.m_Description);
		}
        #endregion

        #region Bitmap TransitionImage
        public Bitmap TransitionImage(ClsTerrainTable iTerrain)
		{
            // Create a new bitmap with specific dimensions and pixel format
            Bitmap bitmap = new Bitmap(400, 168, PixelFormat.Format16bppRgb555);
			Graphics graphics = Graphics.FromImage(bitmap); // Create a Graphics object from the bitmap
            Font font = new Font("Arial", 10f); // Create a new font
            Graphics graphics2 = graphics;
			graphics2.Clear(Color.White); // Clear the graphics to white
            // Draw the terrain groups on the graphics using the Art.GetLand method
            // The terrain groups are positioned at specific points on the graphics
            Graphics arg_5E_0 = graphics2;
			Image arg_5E_1 = Art.GetLand((int)iTerrain.TerrianGroup(0).TileID);
			Point point = new Point(61, 15);
			arg_5E_0.DrawImage(arg_5E_1, point);
			Graphics arg_85_0 = graphics2;
			Image arg_85_1 = Art.GetLand((int)iTerrain.TerrianGroup(1).TileID);
			point = new Point(84, 38);
			arg_85_0.DrawImage(arg_85_1, point);
			Graphics arg_AC_0 = graphics2;
            Image arg_AC_1 = Art.GetLand((int)iTerrain.TerrianGroup(2).TileID);
			point = new Point(107, 61);
			arg_AC_0.DrawImage(arg_AC_1, point);
			Graphics arg_D3_0 = graphics2;
            Image arg_D3_1 = Art.GetLand((int)iTerrain.TerrianGroup(3).TileID);
			point = new Point(38, 38);
			arg_D3_0.DrawImage(arg_D3_1, point);
			Graphics arg_FA_0 = graphics2;
            Image arg_FA_1 = Art.GetLand((int)iTerrain.TerrianGroup(4).TileID);
			point = new Point(61, 61);
			arg_FA_0.DrawImage(arg_FA_1, point);
			Graphics arg_121_0 = graphics2;
            Image arg_121_1 = Art.GetLand((int)iTerrain.TerrianGroup(5).TileID);
			point = new Point(84, 84);
			arg_121_0.DrawImage(arg_121_1, point);
			Graphics arg_148_0 = graphics2;
            Image arg_148_1 = Art.GetLand((int)iTerrain.TerrianGroup(6).TileID);
			point = new Point(15, 61);
			arg_148_0.DrawImage(arg_148_1, point);
			Graphics arg_16F_0 = graphics2;
            Image arg_16F_1 = Art.GetLand((int)iTerrain.TerrianGroup(7).TileID);
			point = new Point(38, 84);
			arg_16F_0.DrawImage(arg_16F_1, point);
			Graphics arg_196_0 = graphics2;
            Image arg_196_1 = Art.GetLand((int)iTerrain.TerrianGroup(8).TileID);
			point = new Point(61, 107);
			arg_196_0.DrawImage(arg_196_1, point);

            // Draw the string representation of the transition on the graphics
            graphics2.DrawString(this.ToString(), font, Brushes.Black, 151f, 2f);
			graphics.Dispose(); // Dispose the graphics object
            return bitmap; // Return the bitmap
        }
        #endregion

        #region Save XmlTextWriter
        public void Save(XmlTextWriter xmlInfo)
		{
			xmlInfo.WriteStartElement("TransInfo"); // Start a new element "TransInfo" in the XML
            xmlInfo.WriteAttributeString("Description", this.m_Description); // Write the description attribute to the XML
            xmlInfo.WriteAttributeString("HashKey", this.m_HashKey.ToString()); // Write the hash key attribute to the XML
            // If the file is not null, write the file attribute to the XML
            if (this.m_File != null)
			{
				xmlInfo.WriteAttributeString("File", this.m_File);
			}
			this.m_MapTiles.Save(xmlInfo); // Save the MapTiles to the XML
            this.m_StaticTiles.Save(xmlInfo); // Save the StaticTiles to the XML
            xmlInfo.WriteEndElement(); // End the "TransInfo" element in the XML
        }
        #endregion

        #region Transition xmlInfo
        public Transition(XmlElement xmlInfo)
		{
			this.m_HashKey = new HashKeyCollection(); // Initialize the HashKeyCollection
            this.m_StaticTiles = new StaticTileCollection(); // Initialize the StaticTileCollection
            this.m_MapTiles = new MapTileCollection();  // Initialize the MapTileCollection
            this.m_RandomTiles = null; // Set the RandomTiles to null
            this.m_File = null; // Set the File to null
            // Set the description of the transition from the XmlElement attribute
            this.m_Description = xmlInfo.GetAttribute("Description");
            // Add the hash key from the XmlElement attribute to the HashKeyCollection
            this.m_HashKey.AddHashKey(xmlInfo.GetAttribute("HashKey"));
            // If the File attribute of the XmlElement is not empty
            if (StringType.StrCmp(xmlInfo.GetAttribute("File"), string.Empty, false) != 0)
			{
                // Initialize the RandomStatics with the File attribute
                this.m_RandomTiles = new RandomStatics(xmlInfo.GetAttribute("File"));
				this.m_File = xmlInfo.GetAttribute("File"); // Set the File to the File attribute of the XmlElement
            }
			this.m_MapTiles.Load(xmlInfo); // Load the MapTileCollection from the XmlElement
            this.m_StaticTiles.Load(xmlInfo); // Load the StaticTileCollection from the XmlElement
        }
        #endregion
        #region Transition
        public Transition()
		{
			this.m_HashKey = new HashKeyCollection(); // Initialize the HashKeyCollection
            this.m_StaticTiles = new StaticTileCollection(); // Initialize the StaticTileCollection
            this.m_MapTiles = new MapTileCollection(); // Initialize the MapTileCollection
            this.m_RandomTiles = null; // Set the RandomTiles to null
            this.m_File = null; // Set the File to null
            this.m_Description = "<New Transition>"; // Set the description of the transition to "<New Transition>"
            this.m_HashKey.Clear(); // Clear the HashKeyCollection
            byte b = 0; // Initialize a counter
            do // Loop to add new HashKeys to the HashKeyCollection
            {
				this.m_HashKey.Add(new HashKey()); // Add a new HashKey to the HashKeyCollection
                b += 1; // Increment the counter
            }
			while (b <= 8); // Continue the loop until the counter is greater than 8
        }
        #endregion

        #region Transition iDescription
        public Transition(string iDescription, string iHashKey, MapTileCollection iMapTiles, StaticTileCollection iStaticTiles)
		{
			this.m_HashKey = new HashKeyCollection(); // Initialize the HashKeyCollection
            this.m_StaticTiles = new StaticTileCollection();  // Initialize the StaticTileCollection
            this.m_MapTiles = new MapTileCollection(); // Initialize the MapTileCollection
            this.m_RandomTiles = null; // Set the RandomTiles to null
            this.m_File = null; // Set the File to null
            this.m_Description = iDescription;
			this.m_HashKey.AddHashKey(iHashKey);

            IEnumerator enumerator = iMapTiles.GetEnumerator();

			try
			{
				while (enumerator.MoveNext()) // Loop through the MapTileCollection
                {
					MapTile value = (MapTile)enumerator.Current; // Get the current MapTile
                    this.m_MapTiles.Add(value); // Add the MapTile to the MapTileCollection
                }
			}
			finally
			{
				if (enumerator is IDisposable) // Dispose the enumerator if it implements IDisposable
                {
					((IDisposable)enumerator).Dispose();
                }
			}

            // Get an enumerator for the StaticTileCollection
            IEnumerator enumerator2 = iStaticTiles.GetEnumerator();

			try
			{
				while (enumerator2.MoveNext()) // Loop through the StaticTileCollection
                {
					StaticTile value2 = (StaticTile)enumerator2.Current; // Get the current StaticTile
                    this.m_StaticTiles.Add(value2); // Add the StaticTile to the StaticTileCollection
                }
			}
			finally
			{
				if (enumerator2 is IDisposable) // Dispose the enumerator if it implements IDisposable
                {
					((IDisposable)enumerator2).Dispose();
				}
			}
		}
        #endregion
        #region Transition Constructor
        public Transition(string iDescription, ClsTerrain iGroupA, ClsTerrain iGroupB, string iHashKey)
		{
			this.m_HashKey = new HashKeyCollection(); // Initialize the HashKeyCollection
            this.m_StaticTiles = new StaticTileCollection(); // Initialize the StaticTileCollection
            this.m_MapTiles = new MapTileCollection(); // Initialize the MapTileCollection
            this.m_RandomTiles = null; // Set the RandomTiles to null
            this.m_File = null; // Set the File to null
            this.m_Description = iDescription; // Set the description of the transition
            byte b = 0; // Initialize a counter
            // Loop through the characters in the hash key
            do
            {
                // If the character is 'A', add the group ID of group A to the hash key collection
                string sLeft = Strings.Mid(iHashKey, (int)checked(b + 1), 1);
				if (StringType.StrCmp(sLeft, "A", false) == 0)
				{
					this.m_HashKey.Add(new HashKey(iGroupA.GroupID));
				}
				else
				{
                    // If the character is 'B', add the group ID of group B to the hash key collection
                    if (StringType.StrCmp(sLeft, "B", false) == 0)
					{
						this.m_HashKey.Add(new HashKey(iGroupB.GroupID));
					}
				}
				b += 1; // Increment the counter
            }
			while (b <= 8); // Continue the loop until the counter is greater than 8
        }
        #endregion

        #region Transition Constructor
        public Transition(string iDescription, string iHashKey, ClsTerrain iGroupA, ClsTerrain iGroupB, MapTileCollection iMapTiles, StaticTileCollection iStaticTiles)
		{
			this.m_HashKey = new HashKeyCollection(); // Initialize the HashKeyCollection
            this.m_StaticTiles = new StaticTileCollection();  // Initialize the StaticTileCollection
            this.m_MapTiles = new MapTileCollection();  // Initialize the MapTileCollection
            this.m_RandomTiles = null; // Set the RandomTiles to null
            this.m_File = null; // Set the File to null
            this.m_Description = iDescription; // Set the description of the transition
            byte b = 0; // Initialize a counter
            do // Loop through the characters in the hash key
            {
                // Get the current character in the hash key
                string sLeft = Strings.Mid(iHashKey, (int)checked(b + 1), 1);
                // If the character is 'A', add the group ID of group A to the hash key collection
                if (StringType.StrCmp(sLeft, "A", false) == 0)
				{
					this.m_HashKey.Add(new HashKey(iGroupA.GroupID));
				}
                else // If the character is 'B', add the group ID of group B to the hash key collection
                {
					if (StringType.StrCmp(sLeft, "B", false) == 0)
					{
						this.m_HashKey.Add(new HashKey(iGroupB.GroupID));
					}
				}
				b += 1; // Increment the counter
            }
            // Continue the loop until the counter is greater than 8
            while (b <= 8);
            // Get an enumerator for the MapTileCollection
            if (iMapTiles != null)
			{
                IEnumerator enumerator = iMapTiles.GetEnumerator();
				try
				{
                    // Loop through the MapTileCollection
                    while (enumerator.MoveNext())
					{
						MapTile value = (MapTile)enumerator.Current; // Get the current MapTile
                        this.m_MapTiles.Add(value); // Add the MapTile to the MapTileCollection
                    }
				}
				finally
				{
                    // Dispose the enumerator if it implements IDisposable
                    if (enumerator is IDisposable)
					{
						((IDisposable)enumerator).Dispose();
					}
				}
			}
            // If the StaticTileCollection is not null
            if (iStaticTiles != null)
			{
                // Get an enumerator for the StaticTileCollection
                IEnumerator enumerator2 = iStaticTiles.GetEnumerator();

				try
				{
                    // Loop through the StaticTileCollection
                    while (enumerator2.MoveNext())
					{
						StaticTile value2 = (StaticTile)enumerator2.Current; // Get the current StaticTile
                        this.m_StaticTiles.Add(value2); // Add the StaticTile to the StaticTileCollection
                    }
				}
				finally
				{
                    // Dispose the enumerator if it implements IDisposable
                    if (enumerator2 is IDisposable)
					{
						((IDisposable)enumerator2).Dispose();
					}
				}
			}
		}
        #endregion

        #region Transition Constructor
        public Transition(string iDescription, ClsTerrain iGroupA, ClsTerrain iGroupB, ClsTerrain iGroupC, string iHashKey)
		{
			this.m_HashKey = new HashKeyCollection(); // Initialize the HashKeyCollection
            this.m_StaticTiles = new StaticTileCollection(); // Initialize the StaticTileCollection
            this.m_MapTiles = new MapTileCollection(); // Initialize the MapTileCollection
            this.m_RandomTiles = null;// Set the RandomTiles to null
			this.m_File = null; // Set the File to null
            this.m_Description = iDescription; // Set the description of the transition
            byte b = 0; // Initialize a counter
            // Loop through the characters in the hash key
            do
            {
                // Get the current character in the hash key
                string sLeft = Strings.Mid(iHashKey, (int)checked(b + 1), 1);
                // If the character is 'A', add the group ID of group A to the hash key collection
                if (StringType.StrCmp(sLeft, "A", false) == 0)
				{
					this.m_HashKey.Add(new HashKey(iGroupA.GroupID));
				}
				else
				{
                    // If the character is 'B', add the group ID of group B to the hash key collection
                    if (StringType.StrCmp(sLeft, "B", false) == 0)
					{
						this.m_HashKey.Add(new HashKey(iGroupB.GroupID));
					}
					else
					{
                        // If the character is 'C', add the group ID of group C to the hash key collection
                        if (StringType.StrCmp(sLeft, "C", false) == 0)
						{
							this.m_HashKey.Add(new HashKey(iGroupC.GroupID));
						}
					}
				}
				b += 1; // Increment the counter
            }
            // Continue the loop until the counter is greater than 8
            while (b <= 8);
		}
        #endregion

        #region Transition Constructor
        public Transition(string iDescription, string iHashKey)
		{
			this.m_HashKey = new HashKeyCollection(); // Initialize the HashKeyCollection
            this.m_StaticTiles = new StaticTileCollection(); // Initialize the StaticTileCollection
            this.m_MapTiles = new MapTileCollection(); // Initialize the MapTileCollection
            this.m_RandomTiles = null; // Set the RandomTiles to null
            this.m_File = null; // Set the File to null
            // Set the description of the transition
            this.m_Description = iDescription;
            // Initialize a counter
            byte b = 0;
            // Loop through the characters in the hash key
            do
            {
                // Add a new HashKey to the HashKeyCollection for each character in the hash key
                this.m_HashKey.Add(new HashKey(Strings.Mid(iHashKey, (int)checked(b * 2 + 1), 2)));
                // Increment the counter
                b += 1;
			}
            // Continue the loop until the counter is greater than 8
            while (b <= 8);
		}
        #endregion

        #region Transition Constructor
        public Transition(string iDescription, ClsTerrain iGroupA, ClsTerrain iGroupB, string iHashKey, MapTile iMapTile)
		{
            // Initialize collections and variables
            this.m_HashKey = new HashKeyCollection();
			this.m_StaticTiles = new StaticTileCollection();
			this.m_MapTiles = new MapTileCollection();
			this.m_RandomTiles = null;
			this.m_File = null;
            // Set the description of the transition
            this.m_Description = iDescription;
            // Loop through the characters in the hash key
            byte b = 0;
			do
			{
                // Get the current character in the hash key
                string sLeft = Strings.Mid(iHashKey, (int)checked(b + 1), 1);
                // If the character is 'A', add the group ID of group A to the hash key collection
                if (StringType.StrCmp(sLeft, "A", false) == 0)
				{
					this.m_HashKey.Add(new HashKey(iGroupA.GroupID));
				}
                else // If the character is 'B', add the group ID of group B to the hash key collection
                {
					if (StringType.StrCmp(sLeft, "B", false) == 0)
					{
						this.m_HashKey.Add(new HashKey(iGroupB.GroupID));
					}
				}
				b += 1;
			}
			while (b <= 8);
            // Add the map tile to the map tiles collection
            this.m_MapTiles.Add(iMapTile);
		}
        #endregion
    }
}
