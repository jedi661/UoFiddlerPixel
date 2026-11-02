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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UoFiddler.Plugin.Enhanced.EcUltima.Animations;
using UoFiddler.Plugin.Enhanced.EcUltima.Hues;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;
using UoFiddler.Plugin.Enhanced.EcUltima.Multis;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Analysis
{
    public sealed class EcCheckOptions
    {
        public bool TileArt { get; set; } = true;
        public bool Textures { get; set; } = true;
        public bool Multis { get; set; } = true;
        public bool ItemAnimations { get; set; } = true;
        public bool Hues { get; set; } = true;
        public bool Maps { get; set; }
    }

    /// <summary>Result of one area of the check.</summary>
    public sealed class EcCheckSection
    {
        public string Title { get; set; }
        public int Problems { get; set; }

        public StringBuilder Text { get; } = new StringBuilder();

        public void Problem(string text, IEnumerable<int> ids = null)
        {
            Problems++;
            Text.AppendLine("  ! " + text);
            if (ids != null)
            {
                Text.AppendLine("      " + EcClientCheck.Ranges(ids));
            }
        }

        public void Info(string text) => Text.AppendLine("    " + text);
    }

    /// <summary>
    /// Checks the whole Enhanced Client: tileart (entries without image, broken name references, wrong ids),
    /// texture files that tileart points to, multis and item animations with unknown items, Hues.uop and the maps.
    /// </summary>
    public sealed class EcClientCheck
    {
        private readonly EcClient _client;

        public EcClientCheck(EcClient client)
        {
            _client = client;
        }

        public List<EcCheckSection> Run(EcCheckOptions options, IProgress<int> progress = null, CancellationToken token = default)
        {
            var sections = new List<EcCheckSection>();
            int steps = new[] { options.TileArt || options.Textures, options.Multis, options.ItemAnimations, options.Hues, options.Maps }.Count(b => b);
            int step = 0;
            void Next() => progress?.Report(++step * 100 / Math.Max(1, steps));

            TileArtFile tileArt = _client.GetTileArt();
            if (options.TileArt || options.Textures)
            {
                sections.AddRange(CheckTileArt(tileArt, options, token));
                Next();
            }
            if (options.Multis)
            {
                sections.Add(CheckMultis(tileArt, token));
                Next();
            }
            if (options.ItemAnimations)
            {
                sections.Add(CheckAnimations(tileArt));
                Next();
            }
            if (options.Hues)
            {
                sections.Add(CheckHues(token));
                Next();
            }
            if (options.Maps)
            {
                sections.AddRange(CheckMaps(token));
                Next();
            }
            return sections;
        }

        public static string Report(List<EcCheckSection> sections, string folder)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ENHANCED CLIENT CHECK");
            sb.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm}   {folder}");
            sb.AppendLine();
            sb.AppendLine("SUMMARY");
            foreach (EcCheckSection s in sections)
            {
                sb.AppendLine($"  {(s.Problems == 0 ? "OK  " : "!!  ")} {s.Title}" + (s.Problems > 0 ? $"  – {s.Problems} problems" : string.Empty));
            }
            foreach (EcCheckSection s in sections)
            {
                sb.AppendLine();
                sb.AppendLine("== " + s.Title);
                sb.Append(s.Text.Length == 0 ? "    no problems" + Environment.NewLine : s.Text.ToString());
            }
            return sb.ToString();
        }

        /// <summary>Ids as compact ranges: "0x0100-0x01FF (256), 0x0400".</summary>
        public static string Ranges(IEnumerable<int> ids, int maxParts = 40)
        {
            List<int> list = ids.Distinct().OrderBy(i => i).ToList();
            var parts = new List<string>();
            for (int i = 0; i < list.Count;)
            {
                int start = list[i], end = start;
                while (i + 1 < list.Count && list[i + 1] == end + 1)
                {
                    end = list[++i];
                }
                i++;
                parts.Add(start == end ? $"0x{start:X4}" : $"0x{start:X4}-0x{end:X4} ({end - start + 1})");
            }
            return string.Join(", ", parts.Take(maxParts)) + (parts.Count > maxParts ? $", ... ({list.Count} ids)" : string.Empty);
        }

        // ------------------------------------------------------------------ tileart

        private IEnumerable<EcCheckSection> CheckTileArt(TileArtFile tileArt, EcCheckOptions options, CancellationToken token)
        {
            var main = new EcCheckSection { Title = "tileart.uop" };
            var textures = new EcCheckSection { Title = "Textures (Texture.uop / LegacyTexture.uop)" };
            if (tileArt == null)
            {
                main.Problem("tileart.uop was not found.");
                yield return main;
                yield break;
            }
            int dictCount = _client.HasStringDictionary ? _client.StringDictionary.Count : 0;
            var badName = new List<int>();
            var badTexture = new List<int>();
            var mismatch = new List<int>();
            var unreadable = new List<int>();
            var noTexture = new List<int>();
            var missingWorld = new List<int>();
            var missingLegacy = new List<int>();
            var cache = new Dictionary<(string, int), bool>();

            foreach (TileArtRecord r in tileArt.Records)
            {
                token.ThrowIfCancellationRequested();
                TileArtEntry e = r.Entry;
                int id = (int)Math.Min(int.MaxValue, r.Id);
                if (r.IdMismatch)
                {
                    mismatch.Add(id);
                }
                if (dictCount > 0 && e.NameIndex > dictCount)
                {
                    badName.Add(id);
                }
                if (!e.TexturesComplete)
                {
                    unreadable.Add(id);
                    continue;
                }
                if (!e.Textures.Any(t => t.Slot <= 1))
                {
                    noTexture.Add(id);
                }
                foreach (TileArtTexture t in e.Textures)
                {
                    foreach (TileArtTextureImage img in t.Images)
                    {
                        if (dictCount > 0 && img.NameIndex > dictCount)
                        {
                            badTexture.Add(id);
                        }
                    }
                    if (!options.Textures || dictCount == 0 || t.Slot > 1 || t.Images.Count == 0)
                    {
                        continue;
                    }
                    string path = _client.StringDictionary.Get(t.Images[0].NameIndex);
                    if (!EcClient.TryParseTexturePath(path, out EcArtSource source, out int artId))
                    {
                        continue;
                    }
                    if (!cache.TryGetValue((source.Title, artId), out bool exists))
                    {
                        exists = source.FileNames.Any(f => _client.GetPackage(f)?.GetEntry(source.GetName(artId)) != null);
                        cache[(source.Title, artId)] = exists;
                    }
                    if (!exists)
                    {
                        (t.Slot == 0 ? missingWorld : missingLegacy).Add(id);
                    }
                }
            }

            main.Info($"{tileArt.Count} entries, {tileArt.FailedEntries} could not be read.");
            if (mismatch.Count > 0)
            {
                main.Problem($"{mismatch.Count} entries have another id inside than in the file name:", mismatch);
            }
            if (badName.Count > 0)
            {
                main.Problem($"{badName.Count} names point behind the end of string_dictionary.uop:", badName);
            }
            if (badTexture.Count > 0)
            {
                main.Problem($"{badTexture.Count} entries have texture names behind the end of string_dictionary.uop:", badTexture);
            }
            if (unreadable.Count > 0)
            {
                main.Info($"{unreadable.Count} entries have a texture layout this plugin can't read (not checked): " + Ranges(unreadable));
            }
            if (noTexture.Count > 0)
            {
                main.Info($"{noTexture.Count} entries have no EC and no legacy texture (invisible in the EC): " + Ranges(noTexture));
            }
            yield return main;

            if (options.Textures)
            {
                textures.Info($"{cache.Count} different textures checked.");
                if (missingWorld.Count > 0)
                {
                    textures.Problem($"{missingWorld.Count} items point to an EC image (slot 0) that is not in Texture.uop:", missingWorld);
                }
                if (missingLegacy.Count > 0)
                {
                    textures.Problem($"{missingLegacy.Count} items point to a legacy texture (slot 1) that is not in LegacyTexture.uop:", missingLegacy);
                }
                yield return textures;
            }
        }

        // ------------------------------------------------------------------ multis, animations

        private EcCheckSection CheckMultis(TileArtFile tileArt, CancellationToken token)
        {
            var s = new EcCheckSection { Title = "MultiCollection.uop" };
            EcUopPackage package = _client.GetPackage("MultiCollection.uop");
            if (package == null)
            {
                s.Info("not found");
                return s;
            }
            package.ResolveNames(EcMulti.NamePattern, 0, 0x10000);
            int count = 0, unreadable = 0;
            var unknownItems = new HashSet<int>();
            var multisWithUnknown = new List<int>();
            foreach (EcUopEntry e in package.Entries.Where(x => x.Name != null && !x.Name.EndsWith("housing.bin", StringComparison.OrdinalIgnoreCase)))
            {
                token.ThrowIfCancellationRequested();
                if (!EcClient.TryGetIdFromName(e.Name, out int id))
                {
                    continue;
                }
                count++;
                try
                {
                    EcMulti multi = EcMulti.Read(package.ReadData(e));
                    List<int> unknown = multi.Parts.Where(p => tileArt != null && tileArt.Get(p.ItemId) == null).Select(p => (int)p.ItemId).ToList();
                    if (unknown.Count > 0)
                    {
                        multisWithUnknown.Add(id);
                        unknownItems.UnionWith(unknown);
                    }
                }
                catch (Exception)
                {
                    unreadable++;
                }
            }
            s.Info($"{count} multis.");
            if (unreadable > 0)
            {
                s.Problem($"{unreadable} multis can't be read.");
            }
            if (multisWithUnknown.Count > 0)
            {
                s.Problem($"{multisWithUnknown.Count} multis use items without tileart entry (invisible parts):", multisWithUnknown);
                s.Info("those items: " + Ranges(unknownItems));
            }
            return s;
        }

        private EcCheckSection CheckAnimations(TileArtFile tileArt)
        {
            var s = new EcCheckSection { Title = "AnimationDefinition.uop (item animations)" };
            EcAnimDefinitionFile anims;
            try
            {
                anims = _client.GetAnimDefinitions();
            }
            catch (Exception ex)
            {
                s.Problem("can't be read: " + ex.Message);
                return s;
            }
            if (anims == null)
            {
                s.Info("not found");
                return s;
            }
            var bad = new List<int>();
            var unknown = new HashSet<int>();
            foreach (EcAnimDefinitionRecord r in anims.Records)
            {
                List<int> missing = r.Definition.Frames.Where(f => tileArt != null && tileArt.Get(f.TileId) == null).Select(f => (int)f.TileId).ToList();
                if (missing.Count > 0)
                {
                    bad.Add((int)r.Id);
                    unknown.UnionWith(missing);
                }
            }
            s.Info($"{anims.Records.Count} item animations.");
            if (bad.Count > 0)
            {
                s.Problem($"{bad.Count} animations have frames without tileart entry:", bad);
                s.Info("those frames: " + Ranges(unknown));
            }
            return s;
        }

        // ------------------------------------------------------------------ hues, maps

        private EcCheckSection CheckHues(CancellationToken token)
        {
            var s = new EcCheckSection { Title = "Hues.uop" };
            EcUopPackage package = _client.GetPackage("Hues.uop");
            if (package == null)
            {
                s.Info("not found");
                return s;
            }
            HueReport report = EcHuesConverter.Analyze(new EcHues(package), null, token);
            foreach (string line in report.ToString().Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Length > 0))
            {
                if (line.StartsWith("! ", StringComparison.Ordinal))
                {
                    s.Problem(line.Substring(2));
                }
                else
                {
                    s.Info(line);
                }
            }
            return s;
        }

        private IEnumerable<EcCheckSection> CheckMaps(CancellationToken token)
        {
            foreach (string file in _client.GetUopFiles().Select(Path.GetFileName).Where(EcFacet.IsFacetFile))
            {
                var s = new EcCheckSection { Title = file };
                try
                {
                    var facet = new EcFacet(_client.GetPackage(file), file);
                    MapReport report = EcMapConverter.AnalyzeEc(facet, null, null, token);
                    foreach (string line in report.ToString().Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Length > 0))
                    {
                        s.Text.AppendLine("  " + line);
                    }
                    s.Problems = report.Problems;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    s.Problem("can't be read: " + ex.Message);
                }
                yield return s;
            }
        }
    }
}
