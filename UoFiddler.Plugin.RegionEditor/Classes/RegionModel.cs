/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Data model of the region editor.
 *
 * Project -> facets (one per map) -> regions (nested) -> rectangles / polygons.
 * ServUO knows only rectangles; polygons exist in the editor and are converted
 * into rectangles when a server format is written.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    /// <summary>Rectangle of a region in map coordinates (X/Y = upper left cell, Width/Height in cells)</summary>
    public sealed class RegionRect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>lowest z (null = from the region / whole height)</summary>
        public int? ZMin { get; set; }

        /// <summary>highest z (null = from the region / whole height)</summary>
        public int? ZMax { get; set; }

        /// <summary>ServUO: rect only used from this expansion on (e.g. "ML")</summary>
        public string Expansion { get; set; }

        [JsonIgnore]
        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public bool Contains(int x, int y) => x >= X && y >= Y && x < X + Width && y < Y + Height;

        public RegionRect Clone() => (RegionRect)MemberwiseClone();

        public static RegionRect From(Rectangle r) => new RegionRect { X = r.X, Y = r.Y, Width = r.Width, Height = r.Height };

        /// <summary>positive width/height (after drawing from right to left)</summary>
        public void Normalize()
        {
            if (Width < 0)
            {
                X += Width;
                Width = -Width;
            }

            if (Height < 0)
            {
                Y += Height;
                Height = -Height;
            }
        }

        public override string ToString() => $"{X},{Y}  {Width}×{Height}" + (ZMin.HasValue || ZMax.HasValue ? $"  z {ZMin?.ToString() ?? "…"}..{ZMax?.ToString() ?? "…"}" : string.Empty);
    }

    /// <summary>Point of a region (go location, entrance)</summary>
    public sealed class RegionPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int? Z { get; set; }

        /// <summary>entrance: facet name of the destination (ServUO "map" attribute), null = same map</summary>
        public string Map { get; set; }

        public RegionPoint Clone() => (RegionPoint)MemberwiseClone();

        public override string ToString() => $"{X},{Y}" + (Z.HasValue ? $",{Z}" : string.Empty) + (string.IsNullOrEmpty(Map) ? string.Empty : $" ({Map})");
    }

    /// <summary>A shape drawn as polygon (editor only)</summary>
    public sealed class RegionPolygon
    {
        public List<Point> Points { get; set; } = new List<Point>();
        public int? ZMin { get; set; }
        public int? ZMax { get; set; }

        public RegionPolygon Clone() => new RegionPolygon { Points = new List<Point>(Points), ZMin = ZMin, ZMax = ZMax };

        [JsonIgnore]
        public Rectangle Bounds
        {
            get
            {
                if (Points.Count == 0)
                {
                    return Rectangle.Empty;
                }

                int x1 = Points.Min(p => p.X), y1 = Points.Min(p => p.Y), x2 = Points.Max(p => p.X), y2 = Points.Max(p => p.Y);
                return Rectangle.FromLTRB(x1, y1, x2, y2);
            }
        }
    }

    public sealed class RegionNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;

        /// <summary>class name of the server (TownRegion, DungeonRegion …); empty = base Region</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>ServUO: only top level regions have an own priority, children inherit it</summary>
        public int Priority { get; set; } = 50;

        public string Music { get; set; }

        /// <summary>z range of the whole region (ServUO &lt;zrange&gt;)</summary>
        public int? ZMin { get; set; }
        public int? ZMax { get; set; }

        public RegionPoint Go { get; set; }
        public RegionPoint Entrance { get; set; }

        public List<RegionRect> Rects { get; set; } = new List<RegionRect>();
        public List<RegionPolygon> Polygons { get; set; } = new List<RegionPolygon>();
        public List<RegionNode> Children { get; set; } = new List<RegionNode>();

        // ── type specific values (ServUO) ──
        public bool GuardsDisabled { get; set; }
        public string GuardsType { get; set; }
        public bool? SmartNoHousing { get; set; }
        public bool? LogoutDelayActive { get; set; }
        public string RuneName { get; set; }
        public string Expansion { get; set; }

        /// <summary>key/values of other servers (Sphere: EVENTS, FLAGS …; POL: EnterText …)</summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>XML of unknown child elements of an imported region (e.g. &lt;spawning&gt;), written back unchanged</summary>
        public List<string> ExtraXml { get; set; } = new List<string>();

        /// <summary>unknown attributes of the &lt;region&gt; element</summary>
        public Dictionary<string, string> ExtraAttributes { get; set; } = new Dictionary<string, string>();

        /// <summary>XML comment in front of the region (kept for the export)</summary>
        public string LeadingComment { get; set; }

        // ── editor only ──
        public string Notes { get; set; }
        public int? Color { get; set; }
        public bool Hidden { get; set; }
        public bool Locked { get; set; }

        [JsonIgnore]
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "(no name)" : Name;

        [JsonIgnore]
        public string DisplayType => string.IsNullOrWhiteSpace(Type) ? "Region" : Type;

        /// <summary>bounds of all rectangles and polygons</summary>
        [JsonIgnore]
        public Rectangle Bounds
        {
            get
            {
                Rectangle r = Rectangle.Empty;
                foreach (Rectangle b in Rects.Select(x => x.Bounds).Concat(Polygons.Select(p => p.Bounds)))
                {
                    r = r.IsEmpty ? b : Rectangle.Union(r, b);
                }

                return r;
            }
        }

        public bool Contains(int x, int y)
        {
            return Rects.Any(r => r.Contains(x, y)) || Polygons.Any(p => Geometry.PointInPolygon(p.Points, x + 0.5, y + 0.5));
        }

        public IEnumerable<RegionNode> SelfAndDescendants()
        {
            yield return this;
            foreach (RegionNode c in Children)
            {
                foreach (RegionNode n in c.SelfAndDescendants())
                {
                    yield return n;
                }
            }
        }

        /// <summary>number of cells (approximation for polygons)</summary>
        [JsonIgnore]
        public long Area => Rects.Sum(r => (long)r.Width * r.Height) + Polygons.Sum(p => (long)Geometry.PolygonArea(p.Points));
    }

    public sealed class FacetRegions
    {
        /// <summary>facet name as used by the server (Felucca, Trammel …)</summary>
        public string Name { get; set; }

        /// <summary>id of the map in UoFiddler (MapRegistry)</summary>
        public int MapId { get; set; }

        public List<RegionNode> Regions { get; set; } = new List<RegionNode>();

        public IEnumerable<RegionNode> AllRegions() => Regions.SelectMany(r => r.SelfAndDescendants());
    }

    public sealed class RegionProject
    {
        public int Version { get; set; } = 1;
        public List<FacetRegions> Facets { get; set; } = new List<FacetRegions>();

        /// <summary>file the regions were imported from (for "save back")</summary>
        public string SourceFile { get; set; }
        public string SourceFormat { get; set; }

        public FacetRegions GetFacet(int mapId, string name, bool create)
        {
            FacetRegions f = Facets.FirstOrDefault(x => x.MapId == mapId);
            if (f == null && create)
            {
                f = new FacetRegions { MapId = mapId, Name = name };
                Facets.Add(f);
            }

            return f;
        }

        public IEnumerable<RegionNode> AllRegions() => Facets.SelectMany(f => f.AllRegions());

        public RegionNode FindById(string id, out FacetRegions facet, out RegionNode parent)
        {
            foreach (FacetRegions f in Facets)
            {
                RegionNode found = Find(f.Regions, id, null, out parent);
                if (found != null)
                {
                    facet = f;
                    return found;
                }
            }

            facet = null;
            parent = null;
            return null;
        }

        private static RegionNode Find(List<RegionNode> list, string id, RegionNode owner, out RegionNode parent)
        {
            foreach (RegionNode r in list)
            {
                if (r.Id == id)
                {
                    parent = owner;
                    return r;
                }

                RegionNode child = Find(r.Children, id, r, out parent);
                if (child != null)
                {
                    return child;
                }
            }

            parent = null;
            return null;
        }

        /// <summary>list that contains the region (top level list of the facet or children of the parent)</summary>
        public List<RegionNode> OwnerList(RegionNode region)
        {
            RegionNode found = FindById(region.Id, out FacetRegions facet, out RegionNode parent);
            if (found == null)
            {
                return null;
            }

            return parent == null ? facet.Regions : parent.Children;
        }
    }

    /// <summary>Known ServUO values (region types, music names, facets)</summary>
    public static class RegionCatalog
    {
        public static readonly string[] Types =
        {
            "TownRegion", "DungeonRegion", "GuardedRegion", "NoHousingRegion", "Jail", "BaseRegion", "Region",
            "HouseRegion", "GreenAcres", "MondainRegion", "TeleportRegion", "DamagingRegion", "SeaMarketRegion",
            "TokunoDocksRegion", "TwistedWealdDesert", "NewMaginciaRegion"
        };

        public static readonly string[] Facets = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur" };

        /// <summary>ServUO MusicName (Server/Region.cs), value = client music id</summary>
        public static readonly string[] Music =
        {
            "OldUlt01", "Create1", "DragFlit", "OldUlt02", "OldUlt03", "OldUlt04", "OldUlt05", "OldUlt06", "Stones2", "Britain1", "Britain2",
            "Bucsden", "Jhelom", "LBCastle", "Linelle", "Magincia", "Minoc", "Ocllo", "Samlethe", "Serpents", "Skarabra", "Trinsic", "Vesper",
            "Wind", "Yew", "Cave01", "Dungeon9", "Forest_a", "InTown01", "Jungle_a", "Mountn_a", "Plains_a", "Sailing", "Swamp_a", "Tavern01",
            "Tavern02", "Tavern03", "Tavern04", "Combat1", "Combat2", "Combat3", "Approach", "Death", "Victory", "BTCastle", "Nujelm", "Dungeon2",
            "Cove", "Moonglow", "Zento", "TokunoDungeon", "Taiko", "DreadHornArea", "ElfCity", "GrizzleDungeon", "MelisandesLair",
            "ParoxysmusLair", "GwennoConversation", "GoodEndGame", "GoodVsEvil", "GreatEarthSerpents", "Humanoids_U9", "MinocNegative", "Paws",
            "SelimsBar", "SerpentIsleCombat_U7", "ValoriaShips", "TheWanderer", "Castle", "Festival", "Honor", "Medieval", "BattleOnStones",
            "Docktown", "GargoyleQueen", "GenericCombat", "Holycity", "HumanLevel", "LoginLoop", "NorthernForestBattleonStones", "PrimevalLich",
            "QueenPalace", "RoyalCity", "SlasherVeil", "StygianAbyss", "StygianDragon", "Void", "CodexShrine", "AnvilStrikeInMinoc",
            "ASkaranLullaby", "BlackthornsMarch", "DupresNightInTrinsic", "FayaxionAndTheSix", "FlightOfTheNexus", "GalehavenJaunt",
            "JhelomToArms", "MidnightInYew", "MoonglowSonata", "NewMaginciaMarch", "NujelmWaltz", "SherrysSong", "StarlightInBritain",
            "TheVesperMist"
        };

        public static int MusicId(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            for (int i = 0; i < Music.Length; i++)
            {
                if (string.Equals(Music[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public static string MusicName(int id) => id >= 0 && id < Music.Length ? Music[id] : null;

        public static bool IsKnownMusic(string name) => MusicId(name) >= 0;

        /// <summary>facet name for a built-in map id, otherwise the registry name</summary>
        public static string FacetName(int mapId, string registryName)
        {
            return mapId >= 0 && mapId < Facets.Length ? Facets[mapId] : (registryName ?? $"Map{mapId}").Replace(" ", string.Empty);
        }

        /// <summary>map id of a facet name (built-in names, then the maps of the MapRegistry), -1 = unknown</summary>
        public static int FacetMapId(string name)
        {
            for (int i = 0; i < Facets.Length; i++)
            {
                if (string.Equals(Facets[i], name?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            string wanted = (name ?? string.Empty).Replace(" ", string.Empty);
            try
            {
                foreach (Ultima.MapDefinition def in Ultima.MapRegistry.Definitions)
                {
                    if (def.Id >= Facets.Length && string.Equals((def.Name ?? string.Empty).Replace(" ", string.Empty), wanted, StringComparison.OrdinalIgnoreCase))
                    {
                        return def.Id;
                    }
                }
            }
            catch (Exception)
            {
                // registry not loaded
            }

            return -1;
        }

        /// <summary>default colour of a region type (ARGB)</summary>
        public static Color TypeColor(string type)
        {
            switch ((type ?? string.Empty).ToLowerInvariant())
            {
                case "townregion": return Color.FromArgb(46, 160, 67);
                case "guardedregion": return Color.FromArgb(38, 132, 255);
                case "dungeonregion": return Color.FromArgb(214, 64, 69);
                case "nohousingregion": return Color.FromArgb(230, 160, 30);
                case "jail": return Color.FromArgb(120, 120, 120);
                case "houseregion": return Color.FromArgb(160, 100, 220);
                case "": case "region": case "baseregion": return Color.FromArgb(0, 180, 200);
                default:
                    int h = Math.Abs((type ?? string.Empty).GetHashCode());
                    return Color.FromArgb(80 + (h % 150), 80 + ((h / 150) % 150), 80 + ((h / 22500) % 150));
            }
        }
    }
}
