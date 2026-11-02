/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UoFiddler.Plugin.Enhanced.EcUltima.Animations;
using UoFiddler.Plugin.Enhanced.EcUltima.Art;
using UoFiddler.Plugin.Enhanced.EcUltima.Hues;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;
using UoFiddler.Plugin.Enhanced.EcUltima.Multis;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Analysis
{
    public enum EcSearchKind
    {
        Item,
        Gump,
        Hue,
        Multi,
        Text
    }

    /// <summary>Where a search result can be shown.</summary>
    public enum EcTarget
    {
        None,
        Item,
        TileArt,
        Gump,
        Hue,
        Multi,
        Map,
        AnimDefinition,
        Animation
    }

    /// <summary>A place to jump to (tab and id or map position).</summary>
    public sealed class EcNavigation
    {
        public EcTarget Target { get; set; }
        public int Id { get; set; }
        public string File { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public sealed class EcSearchHit
    {
        public string Area { get; set; }
        public string Where { get; set; }
        public string Detail { get; set; }
        public EcNavigation Go { get; set; }
    }

    /// <summary>What the classic client knows (filled by the UI from UOFiddler).</summary>
    public sealed class EcClassicLookup
    {
        public Func<int, string> ItemInfo { get; set; }
        public Func<int, string> GumpInfo { get; set; }
        public Func<int, string> HueInfo { get; set; }
        public Func<string, IEnumerable<(int Id, string Name)>> ItemsByName { get; set; }
    }

    /// <summary>
    /// Searches an id (item, gump, hue, multi) or a text through the classic client and every part of the
    /// Enhanced Client: tileart, textures, multis, item animations, animation frames, hues and (optional) the maps.
    /// </summary>
    public sealed class EcSearch
    {
        public const int MaxMapHits = 300;

        private readonly EcClient _client;
        private readonly EcClassicLookup _classic;

        public EcSearch(EcClient client, EcClassicLookup classic)
        {
            _client = client;
            _classic = classic ?? new EcClassicLookup();
        }

        public List<EcSearchHit> Run(EcSearchKind kind, string text, bool maps, IProgress<int> progress = null, CancellationToken token = default)
        {
            var hits = new List<EcSearchHit>();
            text = (text ?? string.Empty).Trim();
            if (kind == EcSearchKind.Text)
            {
                SearchText(text, hits, token);
                return hits;
            }
            if (!TryParse(text, out int id))
            {
                throw new ArgumentException("Enter a number (decimal or 0x hex).");
            }
            switch (kind)
            {
                case EcSearchKind.Item:
                    SearchItem(id, maps, hits, progress, token);
                    break;
                case EcSearchKind.Gump:
                    SearchGump(id, hits);
                    break;
                case EcSearchKind.Hue:
                    SearchHue(id, maps, hits, progress, token);
                    break;
                case EcSearchKind.Multi:
                    SearchMulti(id, hits);
                    break;
            }
            progress?.Report(100);
            return hits;
        }

        public static bool TryParse(string text, out int id)
        {
            id = -1;
            text = text.Trim();
            bool ok = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id)
                : int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
            return ok && id >= 0;
        }

        private static EcSearchHit Hit(string area, string where, string detail, EcTarget target = EcTarget.None, int id = 0) =>
            new EcSearchHit { Area = area, Where = where, Detail = detail, Go = target == EcTarget.None ? null : new EcNavigation { Target = target, Id = id } };

        // ------------------------------------------------------------------ items

        private void SearchItem(int id, bool maps, List<EcSearchHit> hits, IProgress<int> progress, CancellationToken token)
        {
            string classic = _classic.ItemInfo?.Invoke(id);
            hits.Add(Hit("Classic", "tiledata / art", classic ?? "not in the classic client", classic != null ? EcTarget.Item : EcTarget.None, id));

            TileArtFile tileArt = _client.GetTileArt();
            TileArtEntry entry = tileArt?.Get((uint)id)?.Entry;
            var converter = new EcArtConverter(_client);
            EcItemState state = converter.GetState((uint)id);
            string name = entry != null && _client.HasStringDictionary ? _client.StringDictionary.Get(entry.NameIndex) : null;
            hits.Add(Hit("Enhanced", "tileart.uop", entry == null ? "no entry" : $"\"{name}\"  flags {entry.Flags}  – {EcArtConverter.StateText(state)}", entry != null ? EcTarget.TileArt : EcTarget.Item, id));
            if (entry != null && _client.HasStringDictionary)
            {
                foreach (TileArtTexture t in entry.Textures)
                {
                    string path = t.Images.Count > 0 ? _client.StringDictionary.Get(t.Images[0].NameIndex) : null;
                    string exists = string.Empty;
                    if (EcClient.TryParseTexturePath(path, out EcArtSource source, out int artId))
                    {
                        exists = _client.GetArtData(source, artId) != null ? "  (file found)" : "  (FILE MISSING)";
                    }
                    hits.Add(Hit("Enhanced", $"texture slot {t.Slot}", (path ?? "?") + exists, EcTarget.Item, id));
                }
            }
            progress?.Report(10);

            // multis
            EcUopPackage multis = _client.GetPackage("MultiCollection.uop");
            if (multis != null)
            {
                multis.ResolveNames(EcMulti.NamePattern, 0, 0x10000);
                foreach (EcUopEntry e in multis.Entries.Where(x => x.Name != null && x.Name.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) && !x.Name.EndsWith("housing.bin", StringComparison.OrdinalIgnoreCase)))
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        EcMulti multi = EcMulti.Read(multis.ReadData(e));
                        int count = multi.Parts.Count(p => p.ItemId == id);
                        if (count > 0 && EcClient.TryGetIdFromName(e.Name, out int multiId))
                        {
                            hits.Add(Hit("Multis", $"multi {multiId} (0x{multiId:X4})", $"{count} parts", EcTarget.Multi, multiId));
                        }
                    }
                    catch (Exception)
                    {
                        // unreadable multi
                    }
                }
            }
            progress?.Report(25);

            // item animations
            try
            {
                EcAnimDefinitionFile anims = _client.GetAnimDefinitions();
                if (anims != null)
                {
                    foreach (EcAnimDefinitionRecord r in anims.Records)
                    {
                        if (r.Id == id)
                        {
                            hits.Add(Hit("Item animations", "AnimationDefinition.uop", $"animation of this item, {r.Definition.Frames.Count} frames", EcTarget.AnimDefinition, id));
                        }
                        else if (r.Definition.Frames.Any(f => f.TileId == id))
                        {
                            hits.Add(Hit("Item animations", $"animation of {r.Id} (0x{r.Id:X4})", "uses this item as frame", EcTarget.AnimDefinition, (int)r.Id));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // no AnimationDefinition.uop
            }
            progress?.Report(30);

            if (maps)
            {
                SearchMaps(st => st.ItemId == id, $"item 0x{id:X4}", hits, progress, token);
            }
        }

        // ------------------------------------------------------------------ maps

        private void SearchMaps(Func<EcStatic, bool> match, string what, List<EcSearchHit> hits, IProgress<int> progress, CancellationToken token)
        {
            List<string> files = _client.GetUopFiles().Select(Path.GetFileName).Where(EcFacet.IsFacetFile).ToList();
            for (int f = 0; f < files.Count; f++)
            {
                string file = files[f];
                EcFacet facet;
                try
                {
                    facet = new EcFacet(_client.GetPackage(file), file);
                }
                catch (Exception)
                {
                    continue;
                }
                int total = 0, shown = 0;
                for (int sx = 0; sx < facet.SectorsX; sx++)
                {
                    token.ThrowIfCancellationRequested();
                    progress?.Report(30 + (int)(70.0 * (f + ((double)sx / facet.SectorsX)) / files.Count));
                    for (int sy = 0; sy < facet.SectorsY; sy++)
                    {
                        EcSector sector;
                        try
                        {
                            sector = facet.GetSector(sx, sy);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        if (sector == null)
                        {
                            continue;
                        }
                        for (int lx = 0; lx < EcSector.Size; lx++)
                        {
                            for (int ly = 0; ly < EcSector.Size; ly++)
                            {
                                foreach (EcStatic st in sector[lx, ly].Statics)
                                {
                                    if (!match(st))
                                    {
                                        continue;
                                    }
                                    total++;
                                    if (shown < MaxMapHits)
                                    {
                                        shown++;
                                        int x = (sx * EcSector.Size) + lx, y = (sy * EcSector.Size) + ly;
                                        hits.Add(new EcSearchHit
                                        {
                                            Area = "Map",
                                            Where = $"{file}  {x}, {y}, {st.Z}",
                                            Detail = st.ToString(),
                                            Go = new EcNavigation { Target = EcTarget.Map, File = file, X = x, Y = y }
                                        });
                                    }
                                }
                            }
                        }
                        facet.Forget(sx, sy);
                    }
                }
                hits.Add(Hit("Map", file, total == 0 ? $"{what}: not placed" : $"{what}: {total} times" + (total > shown ? $" (first {shown} listed)" : string.Empty)));
            }
        }

        // ------------------------------------------------------------------ gumps, hues, multis

        private void SearchGump(int id, List<EcSearchHit> hits)
        {
            string classic = _classic.GumpInfo?.Invoke(id);
            hits.Add(Hit("Classic", "gump", classic ?? "not in the classic client", classic != null ? EcTarget.Gump : EcTarget.None, id));
            var converter = new EcArtConverter(_client);
            GumpState state = converter.GetGumpState(id);
            hits.Add(Hit("Enhanced", EcArtConverter.GumpFile, state == GumpState.Missing ? "missing" : state == GumpState.Empty ? "empty entry" : "present", EcTarget.Gump, id));
            EcUopPackage ui = _client.GetPackage("Interface.uop");
            if (ui != null)
            {
                bool found = ui.GetEntry(string.Format(CultureInfo.InvariantCulture, "data/interface/default/textures/gumpart/{0:D8}.tga", id)) != null;
                hits.Add(Hit("Enhanced", "Interface.uop (EC user interface)", found ? "present" : "not present"));
            }
        }

        private void SearchHue(int id, bool maps, List<EcSearchHit> hits, IProgress<int> progress, CancellationToken token)
        {
            string classic = _classic.HueInfo?.Invoke(id);
            hits.Add(Hit("Classic", "hues.mul", classic ?? "not in the classic client"));
            EcUopPackage package = _client.GetPackage("Hues.uop");
            if (package != null)
            {
                var hues = new EcHues(package);
                EcHue hue = hues.Get(id);
                hits.Add(Hit("Enhanced", "Hues.uop", hue == null ? "no bitmap" : $"\"{hue.Name}\"  {HueDecodeResult.Text(hues.GetColors(hue).Quality)}", hue != null ? EcTarget.Hue : EcTarget.None, id));
            }
            if (maps)
            {
                SearchMaps(st => st.Hue == id, $"hue {id}", hits, progress, token);
            }
        }

        private void SearchMulti(int id, List<EcSearchHit> hits)
        {
            EcUopPackage multis = _client.GetPackage("MultiCollection.uop");
            byte[] data = multis?.ReadData(string.Format(CultureInfo.InvariantCulture, EcMulti.NamePattern, id));
            if (data == null)
            {
                hits.Add(Hit("Enhanced", "MultiCollection.uop", "not present"));
                return;
            }
            EcMulti multi = EcMulti.Read(data);
            var b = multi.Bounds();
            hits.Add(Hit("Enhanced", "MultiCollection.uop", $"{multi.Parts.Count} parts, {b.MaxX - b.MinX + 1} x {b.MaxY - b.MinY + 1}, z {b.MinZ} – {b.MaxZ}", EcTarget.Multi, id));
            TileArtFile tileArt = _client.GetTileArt();
            int unknown = multi.Parts.Count(p => tileArt != null && tileArt.Get(p.ItemId) == null);
            if (unknown > 0)
            {
                hits.Add(Hit("Enhanced", "parts", $"{unknown} parts use items without tileart entry"));
            }
        }

        // ------------------------------------------------------------------ text

        private void SearchText(string text, List<EcSearchHit> hits, CancellationToken token)
        {
            if (text.Length == 0)
            {
                return;
            }
            const int limit = 500;
            TileArtFile tileArt = _client.GetTileArt();
            if (tileArt != null && _client.HasStringDictionary)
            {
                int n = 0;
                foreach (TileArtRecord r in tileArt.Records)
                {
                    token.ThrowIfCancellationRequested();
                    string name = _client.StringDictionary.Get(r.Entry.NameIndex);
                    if (name != null && name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 && n++ < limit)
                    {
                        hits.Add(Hit("Enhanced", $"item {r.Id} (0x{r.Id:X4})", name, EcTarget.TileArt, (int)r.Id));
                    }
                }
            }
            if (_classic.ItemsByName != null)
            {
                foreach ((int id, string name) in _classic.ItemsByName(text).Take(limit))
                {
                    hits.Add(Hit("Classic", $"item {id} (0x{id:X4})", name, EcTarget.Item, id));
                }
            }
            EcUopPackage package = _client.GetPackage("Hues.uop");
            if (package != null)
            {
                foreach (EcHue hue in new EcHues(package).Hues.Where(h => h.Name != null && h.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).Take(limit))
                {
                    hits.Add(Hit("Enhanced", $"hue {hue.Number}", hue.Name, EcTarget.Hue, hue.Number));
                }
            }
        }
    }
}
