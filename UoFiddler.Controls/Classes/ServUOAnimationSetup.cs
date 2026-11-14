/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * ServUO: script templates for a (new) animation and a check of the client and
 * server files that need an entry so the animation is visible in the game.
 *
 * Client:  anim*.mul / AnimationFrame*.uop (data), Bodyconv.def (anim2..anim5),
 *          mobtypes.txt (animation type), body.def (redirects), tiledata (mount item /
 *          equipment: field "Animation"), gumps 50000+/60000+ (paperdoll).
 * ServUO:  the script (Body = id), Data/bodyTable.cfg (BodyType, only ids 0..4095 –
 *          Server/Body.cs: m_Types = new BodyType[0x1000]).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    public enum ServUOTemplate
    {
        Monster,
        MageMonster,
        Animal,
        Mount,
        SeaCreature,
        HumanNpc,
        EquipmentItem
    }

    public sealed class ServUOScriptSettings
    {
        public ServUOTemplate Template { get; set; } = ServUOTemplate.Monster;
        public string ClassName { get; set; } = "NewCreature";
        public string DisplayName { get; set; } = "a new creature";
        public int Body { get; set; }
        public int Hue { get; set; }
        public int SoundId { get; set; } = -1;

        public int StrMin { get; set; } = 100;
        public int StrMax { get; set; } = 150;
        public int DexMin { get; set; } = 50;
        public int DexMax { get; set; } = 70;
        public int IntMin { get; set; } = 30;
        public int IntMax { get; set; } = 50;
        public int HitsMin { get; set; } = 80;
        public int HitsMax { get; set; } = 120;
        public int DamageMin { get; set; } = 8;
        public int DamageMax { get; set; } = 14;
        public int Fame { get; set; } = 3000;
        public int Karma { get; set; } = -3000;
        public int VirtualArmor { get; set; } = 30;

        public bool Tamable { get; set; }
        public int ControlSlots { get; set; } = 1;
        public double MinTameSkill { get; set; } = 50.0;

        /// <summary>mount: item id of the mount item (tiledata "Animation" = mounted body)</summary>
        public int MountItemId { get; set; } = 0x3EA0;

        /// <summary>equipment: item id of the item that is worn</summary>
        public int ItemId { get; set; }

        /// <summary>equipment: name of the ServUO Layer enum value</summary>
        public string Layer { get; set; } = "OuterTorso";

        public string LootPack { get; set; } = "Average";
    }

    public enum SetupStatus
    {
        Ok,
        Info,
        Warning,
        Missing
    }

    /// <summary>Which file a suggested line belongs to</summary>
    public enum SetupFile
    {
        None,
        Bodyconv,
        Mobtypes,
        BodyTable
    }

    public sealed class SetupCheck
    {
        public string Group { get; set; }
        public string Title { get; set; }
        public SetupStatus Status { get; set; }
        public string Detail { get; set; }

        /// <summary>Line that can be appended to <see cref="File"/> (null = nothing to add)</summary>
        public string SuggestedLine { get; set; }

        public SetupFile File { get; set; }
    }

    public static class ServUOAnimationSetup
    {
        /// <summary>ServUO Layer enum (index = value)</summary>
        public static readonly string[] Layers =
        {
            "Invalid", "OneHanded", "TwoHanded", "Shoes", "Pants", "Shirt", "Helm", "Gloves", "Ring", "Talisman",
            "Neck", "Hair", "Waist", "InnerTorso", "Bracelet", "Unused_xF", "FacialHair", "MiddleTorso", "Earrings",
            "Arms", "Cloak", "Backpack", "OuterTorso", "OuterLegs", "InnerLegs", "Mount"
        };

        public static readonly string[] LootPacks =
        {
            "Poor", "Meager", "Average", "Rich", "FilthyRich", "UltraRich", "SuperBoss"
        };

        /// <summary>Highest body id ServUO accepts in Data/bodyTable.cfg (Server/Body.cs)</summary>
        public const int ServUOMaxBodyTableId = 0xFFF;

        #region Names

        /// <summary>Valid C# class name from a display name ("ice dragon (1234)" -> "IceDragon")</summary>
        public static string ToClassName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "NewCreature";
            }

            var sb = new StringBuilder();
            bool upper = true;
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) && c < 128)
                {
                    sb.Append(upper ? char.ToUpperInvariant(c) : c);
                    upper = false;
                }
                else
                {
                    upper = true;
                }
            }

            string result = sb.ToString();
            if (result.Length == 0)
            {
                return "NewCreature";
            }

            if (char.IsDigit(result[0]))
            {
                result = "Anim" + result;
            }

            return result;
        }

        /// <summary>Text for C# string literals</summary>
        private static string Esc(string text) => (text ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string D(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

        /// <summary>Animation list type (0..3, 4 = equipment) -> mobtypes.txt type</summary>
        public static string MobTypeName(int type) => type switch
        {
            1 => "SEA_MONSTER",
            2 => "ANIMAL",
            3 => "HUMAN",
            4 => "EQUIPMENT",
            _ => "MONSTER"
        };

        /// <summary>Animation list type -> ServUO BodyType</summary>
        public static string BodyTypeName(int type) => type switch
        {
            1 => "Sea",
            2 => "Animal",
            3 => "Human",
            4 => "Equipment",
            _ => "Monster"
        };

        public static ServUOTemplate SuggestTemplate(int type) => type switch
        {
            1 => ServUOTemplate.SeaCreature,
            2 => ServUOTemplate.Animal,
            3 => ServUOTemplate.HumanNpc,
            4 => ServUOTemplate.EquipmentItem,
            _ => ServUOTemplate.Monster
        };

        #endregion

        #region Script

        public static string BuildScript(ServUOScriptSettings s)
        {
            return s.Template == ServUOTemplate.EquipmentItem ? BuildItemScript(s) : BuildCreatureScript(s);
        }

        private static string BuildCreatureScript(ServUOScriptSettings s)
        {
            bool mount = s.Template == ServUOTemplate.Mount;
            bool animal = s.Template == ServUOTemplate.Animal || mount;
            bool mage = s.Template == ServUOTemplate.MageMonster;
            bool sea = s.Template == ServUOTemplate.SeaCreature;
            bool human = s.Template == ServUOTemplate.HumanNpc;
            bool tamable = s.Tamable || animal;

            string ai = mage ? "AIType.AI_Mage" : animal ? "AIType.AI_Animal" : "AIType.AI_Melee";
            string fight = animal || human ? "FightMode.Aggressor" : "FightMode.Closest";
            string name = Esc(s.DisplayName);
            string cls = s.ClassName;
            string baseClass = mount ? "BaseMount" : "BaseCreature";
            string corpse = name.StartsWith("a ", StringComparison.OrdinalIgnoreCase) || name.StartsWith("an ", StringComparison.OrdinalIgnoreCase)
                ? $"{name} corpse"
                : $"a {name} corpse";

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Server;");
            sb.AppendLine("using Server.Items;");
            sb.AppendLine();
            sb.AppendLine("namespace Server.Mobiles");
            sb.AppendLine("{");
            sb.AppendLine($"    // Generated by UoFiddler – body {s.Body} (0x{s.Body:X}), template {s.Template}");
            sb.AppendLine($"    // Client: the animation must exist for body {s.Body} (anim*.mul/uop, Bodyconv.def, mobtypes.txt)");
            sb.AppendLine($"    // Server: Data/bodyTable.cfg -> {s.Body}<TAB>{BodyTypeNameForTemplate(s.Template)}");
            sb.AppendLine($"    [CorpseName(\"{corpse}\")]");
            sb.AppendLine($"    public class {cls} : {baseClass}");
            sb.AppendLine("    {");
            sb.AppendLine("        [Constructable]");

            if (mount)
            {
                sb.AppendLine($"        public {cls}()");
                sb.AppendLine($"            : this(\"{name}\")");
                sb.AppendLine("        {");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        [Constructable]");
                sb.AppendLine($"        public {cls}(string name)");
                sb.AppendLine($"            : base(name, {s.Body}, 0x{s.MountItemId:X4}, {ai}, {fight}, 10, 1, 0.2, 0.4)");
                sb.AppendLine("        {");
            }
            else
            {
                sb.AppendLine($"        public {cls}()");
                sb.AppendLine($"            : base({ai}, {fight}, 10, 1, 0.2, 0.4)");
                sb.AppendLine("        {");
                sb.AppendLine($"            Name = \"{name}\";");
                sb.AppendLine($"            Body = {s.Body};");
            }

            if (s.Hue != 0)
            {
                sb.AppendLine($"            Hue = 0x{s.Hue:X};");
            }

            if (s.SoundId >= 0)
            {
                sb.AppendLine($"            BaseSoundID = 0x{s.SoundId:X};");
            }

            sb.AppendLine();
            sb.AppendLine($"            SetStr({s.StrMin}, {s.StrMax});");
            sb.AppendLine($"            SetDex({s.DexMin}, {s.DexMax});");
            sb.AppendLine($"            SetInt({s.IntMin}, {s.IntMax});");
            sb.AppendLine();
            sb.AppendLine($"            SetHits({s.HitsMin}, {s.HitsMax});");
            sb.AppendLine($"            SetDamage({s.DamageMin}, {s.DamageMax});");
            sb.AppendLine();
            sb.AppendLine("            SetDamageType(ResistanceType.Physical, 100);");
            sb.AppendLine();
            sb.AppendLine("            SetResistance(ResistanceType.Physical, 30, 40);");
            sb.AppendLine("            SetResistance(ResistanceType.Fire, 20, 30);");
            sb.AppendLine("            SetResistance(ResistanceType.Cold, 20, 30);");
            sb.AppendLine("            SetResistance(ResistanceType.Poison, 20, 30);");
            sb.AppendLine("            SetResistance(ResistanceType.Energy, 20, 30);");
            sb.AppendLine();
            sb.AppendLine("            SetSkill(SkillName.MagicResist, 40.0, 60.0);");
            sb.AppendLine("            SetSkill(SkillName.Tactics, 50.0, 70.0);");
            sb.AppendLine("            SetSkill(SkillName.Wrestling, 50.0, 70.0);");
            if (mage)
            {
                sb.AppendLine("            SetSkill(SkillName.EvalInt, 60.0, 80.0);");
                sb.AppendLine("            SetSkill(SkillName.Magery, 60.0, 80.0);");
            }

            sb.AppendLine();
            sb.AppendLine($"            Fame = {s.Fame};");
            sb.AppendLine($"            Karma = {s.Karma};");
            sb.AppendLine();
            sb.AppendLine($"            VirtualArmor = {s.VirtualArmor};");

            if (tamable)
            {
                sb.AppendLine();
                sb.AppendLine("            Tamable = true;");
                sb.AppendLine($"            ControlSlots = {s.ControlSlots};");
                sb.AppendLine($"            MinTameSkill = {D(s.MinTameSkill)};");
            }

            if (sea)
            {
                sb.AppendLine();
                sb.AppendLine("            CanSwim = true;");
                sb.AppendLine("            CantWalk = true;");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {cls}(Serial serial)");
            sb.AppendLine("            : base(serial)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();

            if (animal)
            {
                sb.AppendLine("        public override int Meat { get { return 3; } }");
                sb.AppendLine("        public override int Hides { get { return 10; } }");
                sb.AppendLine("        public override FoodType FavoriteFood { get { return FoodType.Meat | FoodType.FruitsAndVegies; } }");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("        public override void GenerateLoot()");
                sb.AppendLine("        {");
                sb.AppendLine($"            AddLoot(LootPack.{s.LootPack});");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            AppendSerialization(sb);
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string BuildItemScript(ServUOScriptSettings s)
        {
            string cls = s.ClassName;
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Server;");
            sb.AppendLine();
            sb.AppendLine("namespace Server.Items");
            sb.AppendLine("{");
            sb.AppendLine($"    // Generated by UoFiddler – equipment animation {s.Body} (0x{s.Body:X}), item 0x{s.ItemId:X4}");
            sb.AppendLine($"    // Client: tiledata item 0x{s.ItemId:X4} needs Animation = {s.Body}, flag Wearable and Quality = layer;");
            sb.AppendLine($"    //         paperdoll gump 0x{50000 + s.Body:X} (male) / 0x{60000 + s.Body:X} (female)");
            sb.AppendLine($"    public class {cls} : Item");
            sb.AppendLine("    {");
            sb.AppendLine("        [Constructable]");
            sb.AppendLine($"        public {cls}()");
            sb.AppendLine($"            : base(0x{s.ItemId:X4})");
            sb.AppendLine("        {");
            sb.AppendLine($"            Name = \"{Esc(s.DisplayName)}\";");
            sb.AppendLine($"            Layer = Layer.{s.Layer};");
            if (s.Hue != 0)
            {
                sb.AppendLine($"            Hue = 0x{s.Hue:X};");
            }

            sb.AppendLine("            Weight = 1.0;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {cls}(Serial serial)");
            sb.AppendLine("            : base(serial)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            AppendSerialization(sb);
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void AppendSerialization(StringBuilder sb)
        {
            sb.AppendLine("        public override void Serialize(GenericWriter writer)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Serialize(writer);");
            sb.AppendLine("            writer.Write(0); // version");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Deserialize(GenericReader reader)");
            sb.AppendLine("        {");
            sb.AppendLine("            base.Deserialize(reader);");
            sb.AppendLine("            int version = reader.ReadInt();");
            sb.AppendLine("        }");
        }

        private static string BodyTypeNameForTemplate(ServUOTemplate t) => t switch
        {
            ServUOTemplate.Animal or ServUOTemplate.Mount => "Animal",
            ServUOTemplate.SeaCreature => "Sea",
            ServUOTemplate.HumanNpc => "Human",
            ServUOTemplate.EquipmentItem => "Equipment",
            _ => "Monster"
        };

        #endregion

        #region Lines for the definition files

        /// <summary>Bodyconv.def: body anim2 anim3 anim4 anim5 (-1 = not in that file)</summary>
        public static string BodyconvLine(int body, int fileType, int index)
        {
            var cols = new[] { -1, -1, -1, -1 };
            if (fileType >= 2 && fileType <= 5)
            {
                cols[fileType - 2] = index;
            }

            return $"{body}\t{cols[0]}\t{cols[1]}\t{cols[2]}\t{cols[3]}";
        }

        public static string MobtypesLine(int body, int type) => $"{body}\t{MobTypeName(type)}\t0";

        public static string BodyTableLine(int body, int type) => $"{body}\t{BodyTypeName(type)}";

        /// <summary>First column (id) of all data lines of a definition file</summary>
        public static Dictionary<int, string> ReadIdLines(string path)
        {
            var result = new Dictionary<int, string>();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return result;
            }

            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//") || line.StartsWith("\""))
                {
                    continue;
                }

                string first = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (int.TryParse(first, out int id) && !result.ContainsKey(id))
                {
                    result[id] = line;
                }
            }

            return result;
        }

        /// <summary>
        /// Adds a line for an id (or replaces the existing line of that id). A backup *.bak is written first.
        /// </summary>
        public static void WriteIdLine(string path, int id, string line)
        {
            var lines = File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string>();
            int index = lines.FindIndex(l =>
            {
                string t = l.Trim();
                if (t.Length == 0 || t.StartsWith("#") || t.StartsWith("//"))
                {
                    return false;
                }

                string first = t.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                return int.TryParse(first, out int value) && value == id;
            });

            if (File.Exists(path))
            {
                File.Copy(path, path + ".bak", true);
            }

            if (index >= 0)
            {
                lines[index] = line;
            }
            else
            {
                lines.Add(line);
            }

            File.WriteAllLines(path, lines);
        }

        #endregion

        #region Check

        /// <param name="body">real body id (as used by the server)</param>
        /// <param name="type">animation list type 0..3, 4 = equipment</param>
        /// <param name="sourceFileType">file the animation was made in (1..5 = anim.mul..anim5.mul, 6 = uop) or 0 = unknown</param>
        /// <param name="sourceIndex">index inside that file</param>
        /// <param name="servUoDirectory">ServUO root (for Data/bodyTable.cfg) or null</param>
        public static List<SetupCheck> Check(int body, int type, int sourceFileType, int sourceIndex,
            ServUOScriptSettings settings, string servUoDirectory)
        {
            var list = new List<SetupCheck>();
            const string client = "Client";
            const string server = "ServUO";

            // ── animation data ─────────────────────────────────────────
            bool inFiles = AnimationListStore.IsInClientFiles(body);
            string fileName = SafeFileName(body);
            list.Add(new SetupCheck
            {
                Group = client,
                Title = "Animation data",
                Status = inFiles ? SetupStatus.Ok : SetupStatus.Missing,
                Detail = inFiles
                    ? $"Body {body} is found in the client files ({fileName})."
                    : $"Body {body} is not found in the loaded client files. Save the animation in the Animation editor " +
                      "(it is written to the output folder) and copy the files (anim*.mul/idx or AnimationFrame*.uop) " +
                      "into the client folder, then reload the files in UoFiddler."
            });

            // ── Bodyconv.def ───────────────────────────────────────────
            int converted = body;
            int fileType = BodyConverter.Convert(ref converted);
            if (sourceFileType >= 2 && sourceFileType <= 5)
            {
                bool fits = fileType == sourceFileType && converted == sourceIndex;
                list.Add(new SetupCheck
                {
                    Group = client,
                    Title = "Bodyconv.def",
                    Status = fits ? SetupStatus.Ok : SetupStatus.Missing,
                    Detail = fits
                        ? $"Body {body} -> anim{sourceFileType}.mul index {sourceIndex}."
                        : $"The animation is in anim{sourceFileType}.mul at index {sourceIndex}. Without a line in Bodyconv.def " +
                          $"the client does not find it for body {body}" +
                          (fileType > 1 ? $" (currently body {body} points to anim{fileType}.mul index {converted})." : "."),
                    SuggestedLine = fits ? null : BodyconvLine(body, sourceFileType, sourceIndex),
                    File = fits ? SetupFile.None : SetupFile.Bodyconv
                });
            }
            else if (fileType > 1)
            {
                list.Add(new SetupCheck
                {
                    Group = client,
                    Title = "Bodyconv.def",
                    Status = SetupStatus.Ok,
                    Detail = $"Body {body} -> anim{fileType}.mul index {converted}."
                });
            }
            else
            {
                list.Add(new SetupCheck
                {
                    Group = client,
                    Title = "Bodyconv.def",
                    Status = SetupStatus.Info,
                    Detail = sourceFileType == 6
                        ? "UOP animation: no Bodyconv.def entry needed."
                        : "No entry needed if the animation is in anim.mul (or in AnimationFrame*.uop)."
                });
            }

            // ── mobtypes.txt ───────────────────────────────────────────
            string mobtypesPath = Files.GetFilePath("mobtypes.txt");
            Dictionary<int, string> mobtypes = ReadIdLines(mobtypesPath);
            string expectedType = MobTypeName(type);
            if (mobtypes.TryGetValue(body, out string mobLine))
            {
                bool same = mobLine.IndexOf(expectedType, StringComparison.OrdinalIgnoreCase) >= 0;
                list.Add(new SetupCheck
                {
                    Group = client,
                    Title = "mobtypes.txt",
                    Status = same ? SetupStatus.Ok : SetupStatus.Warning,
                    Detail = same
                        ? $"Entry: {mobLine}"
                        : $"Entry \"{mobLine}\" does not match the chosen type {expectedType} (the client uses the wrong action table).",
                    SuggestedLine = same ? null : MobtypesLine(body, type),
                    File = same ? SetupFile.None : SetupFile.Mobtypes
                });
            }
            else
            {
                list.Add(new SetupCheck
                {
                    Group = client,
                    Title = "mobtypes.txt",
                    Status = SetupStatus.Missing,
                    Detail = mobtypesPath == null
                        ? "mobtypes.txt was not found in the client folder."
                        : $"No entry for body {body}. The client needs the type ({expectedType}) to play the right actions.",
                    SuggestedLine = MobtypesLine(body, type),
                    File = SetupFile.Mobtypes
                });
            }

            // ── body.def ───────────────────────────────────────────────
            if (BodyTable.Entries != null && BodyTable.Entries.TryGetValue(body, out BodyTableEntry redirect))
            {
                list.Add(new SetupCheck
                {
                    Group = client,
                    Title = "body.def",
                    Status = SetupStatus.Warning,
                    Detail = $"body.def redirects body {body} to {redirect.OldId}" + (redirect.NewHue != 0 ? $" (hue {redirect.NewHue})" : string.Empty) +
                             ". If your own animation is not shown, remove this line from body.def."
                });
            }

            // ── UOP client ─────────────────────────────────────────────
            if (Files.GetFilePath("AnimationFrame1.uop") != null)
            {
                list.Add(new SetupCheck
                {
                    Group = client,
                    Title = "AnimationFrame*.uop",
                    Status = SetupStatus.Info,
                    Detail = "The client uses UOP animations: for ids that exist in AnimationFrame*.uop the UOP data is used, " +
                             "mul animations only for the other ids. Save UOP edits in the Animation editor (UOP mode)."
                });
            }

            // ── mount item / equipment item (tiledata) ─────────────────
            if (settings != null && settings.Template == ServUOTemplate.Mount)
            {
                list.Add(CheckTileDataAnimation(settings.MountItemId, body, "Mount item", false));
            }

            if (type == 4 || (settings != null && settings.Template == ServUOTemplate.EquipmentItem))
            {
                int itemId = settings?.ItemId ?? 0;
                list.Add(CheckTileDataAnimation(itemId, body, "Equipment item", true));

                bool male = SafeGumpExists(50000 + body);
                bool female = SafeGumpExists(60000 + body);
                list.Add(new SetupCheck
                {
                    Group = client,
                    Title = "Paperdoll gump",
                    Status = male ? SetupStatus.Ok : SetupStatus.Info,
                    Detail = $"Gump {50000 + body} (male): {(male ? "present" : "missing")}, " +
                             $"gump {60000 + body} (female): {(female ? "present" : "missing")}. " +
                             "Without it the item is worn but not shown on the paperdoll."
                });
            }

            // ── ServUO ─────────────────────────────────────────────────
            list.Add(new SetupCheck
            {
                Group = server,
                Title = "Script",
                Status = SetupStatus.Info,
                Detail = "Save the generated script e.g. into Scripts/Custom/Animations/ and restart the server. " +
                         (settings?.Template == ServUOTemplate.EquipmentItem
                             ? $"In game: [add {settings.ClassName}"
                             : $"In game: [add {settings?.ClassName}")
            });

            if (type != 4)
            {
                string bodyTablePath = string.IsNullOrEmpty(servUoDirectory) ? null : Path.Combine(servUoDirectory, "Data", "bodyTable.cfg");
                Dictionary<int, string> bodyTable = ReadIdLines(bodyTablePath);
                string expected = BodyTypeName(type);
                if (body > ServUOMaxBodyTableId)
                {
                    list.Add(new SetupCheck
                    {
                        Group = server,
                        Title = "Data/bodyTable.cfg",
                        Status = SetupStatus.Warning,
                        Detail = $"Body {body} is above 4095. ServUO reads bodyTable.cfg only for ids 0..4095 " +
                                 "(Server/Body.cs: m_Types = new BodyType[0x1000]); the body gets type \"Empty\". " +
                                 "Enlarge the array in Server/Body.cs (e.g. new BodyType[0x10000]) or use a body id below 4096.",
                        SuggestedLine = BodyTableLine(body, type),
                        File = SetupFile.BodyTable
                    });
                }
                else if (bodyTablePath == null)
                {
                    list.Add(new SetupCheck
                    {
                        Group = server,
                        Title = "Data/bodyTable.cfg",
                        Status = SetupStatus.Info,
                        Detail = "Choose the ServUO folder to check the entry. Line (separated by a TAB): " + BodyTableLine(body, type),
                        SuggestedLine = BodyTableLine(body, type),
                        File = SetupFile.BodyTable
                    });
                }
                else if (bodyTable.TryGetValue(body, out string tableLine))
                {
                    bool same = tableLine.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
                    list.Add(new SetupCheck
                    {
                        Group = server,
                        Title = "Data/bodyTable.cfg",
                        Status = same ? SetupStatus.Ok : SetupStatus.Warning,
                        Detail = same ? $"Entry: {tableLine}" : $"Entry \"{tableLine}\" does not match the type {expected}.",
                        SuggestedLine = same ? null : BodyTableLine(body, type),
                        File = same ? SetupFile.None : SetupFile.BodyTable
                    });
                }
                else
                {
                    list.Add(new SetupCheck
                    {
                        Group = server,
                        Title = "Data/bodyTable.cfg",
                        Status = SetupStatus.Missing,
                        Detail = $"No entry for body {body}: ServUO treats it as \"Empty\" (no correct attack/hit/death animations).",
                        SuggestedLine = BodyTableLine(body, type),
                        File = SetupFile.BodyTable
                    });
                }
            }

            return list;
        }

        private static SetupCheck CheckTileDataAnimation(int itemId, int body, string title, bool wearable)
        {
            var check = new SetupCheck { Group = "Client", Title = $"tiledata: {title} 0x{itemId:X4}" };
            if (TileData.ItemTable == null || itemId <= 0 || itemId >= TileData.ItemTable.Length)
            {
                check.Status = SetupStatus.Missing;
                check.Detail = "Enter a valid item id.";
                return check;
            }

            ItemData data = TileData.ItemTable[itemId];
            var problems = new List<string>();
            if (data.Animation != body)
            {
                problems.Add($"field Animation is {data.Animation}, must be {body}");
            }

            if (wearable && !data.Wearable)
            {
                problems.Add("flag Wearable is missing");
            }

            if (wearable && data.Quality == 0)
            {
                problems.Add("Quality (= layer) is 0");
            }

            check.Status = problems.Count == 0 ? SetupStatus.Ok : SetupStatus.Missing;
            check.Detail = problems.Count == 0
                ? $"Item 0x{itemId:X4} \"{data.Name}\" uses animation {body}."
                : $"Item 0x{itemId:X4} \"{data.Name}\": {string.Join(", ", problems)}. Change it in the TileData tab and save tiledata.mul.";
            return check;
        }

        private static string SafeFileName(int body)
        {
            try
            {
                return Animations.GetFileName(body) ?? "?";
            }
            catch (Exception)
            {
                return "?";
            }
        }

        private static bool SafeGumpExists(int id)
        {
            try
            {
                return Gumps.IsValidIndex(id);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
